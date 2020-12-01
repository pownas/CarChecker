using CarChecker.Data;
using Microsoft.MobileBlazorBindings.Authentication;
using Microsoft.MobileBlazorBindings.Authentication.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarChecker.Data
{
    public class AppOfflineAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        private readonly IServiceProvider services;

        public AppOfflineAccountClaimsPrincipalFactory(IServiceProvider services, IAccessTokenProviderAccessor accessor) : base(accessor)
        {
            this.services = services;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var localVehiclesStore = services.GetRequiredService<ILocalVehiclesStore>();

            var result = await base.CreateUserAsync(account, options);
            if (result.Identity.IsAuthenticated)
            {
                await localVehiclesStore.SaveUserAccountAsync(result);
            }
            else
            {
                result = await localVehiclesStore.LoadUserAccountAsync();
            }

            return result;
        }
    }
}
