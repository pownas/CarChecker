using CarChecker.Shared;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarChecker.Data
{
    public interface ILocalVehiclesStore
    {
        ValueTask<string[]> Autocomplete(string prefix);
        ValueTask<DateTime?> GetLastUpdateDateAsync();
        ValueTask<Vehicle[]> GetOutstandingLocalEditsAsync();
        Task<Vehicle> GetVehicle(string licenseNumber);
        Task<ClaimsPrincipal> LoadUserAccountAsync();
        ValueTask SaveUserAccountAsync(ClaimsPrincipal user);
        ValueTask SaveVehicleAsync(Vehicle vehicle);
        Task SynchronizeAsync();
    }
}