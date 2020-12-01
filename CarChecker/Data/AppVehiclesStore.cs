using CarChecker.Shared;
using LiteDB;
using Microsoft.MobileBlazorBindings.ProtectedStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarChecker.Data
{
    /// <summary>
    /// App implementation of a vehicle store.
    /// </summary>
    internal sealed class AppVehiclesStore : ILocalVehiclesStore, IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly IProtectedStorage protectedStorage;
        private LiteDatabase liteDatabase;
        private Task initTask;

        private ILiteCollection<Vehicle> vehicles;
        private ILiteCollection<Vehicle> localEdits;

        public AppVehiclesStore(HttpClient httpClient, IProtectedStorage protectedStorage)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.protectedStorage = protectedStorage ?? throw new ArgumentNullException(nameof(protectedStorage));
        }

        public async ValueTask<string[]> Autocomplete(string prefix)
        {
            await EnsureLiteDb();

            return await Task.Run(() => this.vehicles
                .Query()
                .Where(x => x.LicenseNumber.StartsWith(prefix))
                .OrderBy(x => x.LicenseNumber)
                .Select(x => x.LicenseNumber)
                .Limit(5)
                .ToArray());
        }

        public async ValueTask<DateTime?> GetLastUpdateDateAsync()
        {
            return await this.protectedStorage.GetAsync<DateTime?>("last_update_date");
        }

        public async ValueTask<Vehicle[]> GetOutstandingLocalEditsAsync()
        {
            await EnsureLiteDb();
            return await Task.Run(() => this.localEdits.Query().ToArray());
        }

        public async Task<Vehicle> GetVehicle(string licenseNumber)
        {
            await EnsureLiteDb();

            return await Task.Run(() =>
            {
                var result = this.localEdits.Query().Where(x => x.LicenseNumber.Equals(licenseNumber, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                
                if (result != null)
                {
                    return result;
                }

                return this.vehicles.Query().Where(x => x.LicenseNumber.Equals(licenseNumber, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            });
        }

        public async Task<ClaimsPrincipal> LoadUserAccountAsync()
        {
            var bytes = await this.protectedStorage.GetAsync<byte[]>("claims_principal");

            if (bytes != null)
            {
                try
                {
                    using var stream = new MemoryStream(bytes);
                    using var reader = new BinaryReader(stream);
                    return new ClaimsPrincipal(reader);
                }
                catch
                { 
                    // we don't want to fail on trying to restore a claimsprincipal.
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        public async ValueTask SaveUserAccountAsync(ClaimsPrincipal user)
        {
            if (user == null)
            {
                await this.protectedStorage.DeleteAsync("claims_principal");
            } else
            {
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                user.WriteTo(writer);
                await this.protectedStorage.SetAsync("claims_principal", stream.ToArray());
            }
        }

        public async ValueTask SaveVehicleAsync(Vehicle vehicle)
        {
            await EnsureLiteDb();
            await Task.Run(() => this.localEdits.Upsert(vehicle.LicenseNumber, vehicle));
        }

        public async Task SynchronizeAsync()
        {
            await EnsureLiteDb();
            await Task.Run<Task>(async () =>
            {
                // If there are local edits, always send them first
                foreach (var editedVehicle in this.localEdits.Query().ToArray())
                {
                    (await httpClient.PutAsJsonAsync("api/vehicle/details", editedVehicle)).EnsureSuccessStatusCode();
                    this.localEdits.Delete(editedVehicle.LicenseNumber);
                }
            }).Unwrap();

            await FetchChangesAsync();
        }

        private async Task FetchChangesAsync()
        {
            await EnsureLiteDb();
            var syncDate = DateTime.Now;
            var mostRecentlyUpdated = this.vehicles.Query().OrderByDescending(x => x.LastUpdated).FirstOrDefault();
            var since = mostRecentlyUpdated?.LastUpdated ?? DateTime.MinValue;

            // trick to leave timezone info behind.
            since = new DateTime(since.Ticks, DateTimeKind.Unspecified);
            var vehicles = await httpClient.GetFromJsonAsync<Vehicle[]>($"api/vehicle/changedvehicles?since={since:o}");
            foreach (var vehicle in vehicles)
            {
                this.vehicles.Upsert(vehicle.LicenseNumber, vehicle);
            }
            await this.protectedStorage.SetAsync("last_update_date", syncDate);
        }

        private async Task EnsureLiteDb()
        {
            if (liteDatabase != null)
            {
                return;
            }

            void InitTask()
            {
                var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
                
                if (!Directory.Exists(dbFolder))
                {
                    Directory.CreateDirectory(dbFolder);
                }

                var dbLocation = Path.Combine(dbFolder, "lite.db");
                liteDatabase = new LiteDatabase(dbLocation);

                vehicles = liteDatabase.GetCollection<Vehicle>("vehicles");

                vehicles.EnsureIndex(x => x.LicenseNumber);
                vehicles.EnsureIndex(x => x.LastUpdated);

                localEdits = liteDatabase.GetCollection<Vehicle>("localEdits");

                localEdits.EnsureIndex(x => x.LicenseNumber);
                localEdits.EnsureIndex(x => x.LastUpdated);
            }

            Task task = null;

            if ((task = Interlocked.CompareExchange(ref initTask, new Task(InitTask), null)) == null)
            {
                task = initTask;
                task.Start(TaskScheduler.Default);
            }

            await task;
        }

        public void Dispose()
        {
            if (liteDatabase != null)
            {
                liteDatabase.Dispose();
                liteDatabase = null;
            }
        }
    }
}
