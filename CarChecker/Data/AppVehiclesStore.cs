using CarChecker.Shared;
using Microsoft.MobileBlazorBindings.ProtectedStorage;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CarChecker.Data
{
    /// <summary>
    /// App implementation of a vehicle store.
    /// </summary>
    internal class AppVehiclesStore : ILocalVehiclesStore
    {
        private readonly HttpClient httpClient;
        private readonly IProtectedStorage protectedStorage;

        public AppVehiclesStore(HttpClient httpClient, IProtectedStorage protectedStorage)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.protectedStorage = protectedStorage ?? throw new ArgumentNullException(nameof(protectedStorage));
        }

        public ValueTask<string[]> Autocomplete(string prefix)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<DateTime?> GetLastUpdateDateAsync()
        {
            return await this.protectedStorage.GetAsync<DateTime?>("last_update_date");
        }

        public async ValueTask<Vehicle[]> GetOutstandingLocalEditsAsync()
        {
            return await this.protectedStorage.GetAsync<Vehicle[]>("local_edits") ?? Array.Empty<Vehicle>();
        }

        public Task<Vehicle> GetVehicle(string licenseNumber)
        {
            throw new NotImplementedException();
        }

        public Task<ClaimsPrincipal> LoadUserAccountAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask SaveUserAccountAsync(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public ValueTask SaveVehicleAsync(Vehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public Task SynchronizeAsync()
        {
            throw new NotImplementedException();
        }


        class ClaimData
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
