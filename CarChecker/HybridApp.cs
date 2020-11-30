using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.MobileBlazorBindings;
using Xamarin.Forms;
using System.Net.Http;
using Microsoft.MobileBlazorBindings.Authentication;

namespace CarChecker
{
    public class HybridApp : Application
    {
        private const string BaseAddress = "https://localhost:5001/";

        public HybridApp(IFileProvider fileProvider = null)
        {
            var hostBuilder = MobileBlazorBindingsHost.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Adds web-specific services such as NavigationManager
                    services.AddBlazorHybrid();

                    // Register app-specific services
                    // Configure HttpClient for use when talking to server backend
                    services.AddHttpClient("CarChecker.ServerAPI",
                        client => client.BaseAddress = new Uri(BaseAddress))
                        .AddHttpMessageHandler(() => new ApiAuthorizationMessageHandler(BaseAddress));

                    // Add protected storage for storage of refresh tokens
                    services.AddProtectedStorage();

                    // Other DI services
                    //                    services.AddScoped<ILocalVehiclesStore, LocalVehiclesStore>();

                    // Add the http client as the default to inject.
                    services.AddScoped<HttpClient>(sp =>
                    {
                        var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
                        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                        ApiAuthorizationMessageHandler.RegisterTokenProvider(BaseAddress, accessTokenProvider);
                        return httpClientFactory.CreateClient("CarChecker.ServerAPI");
                    });

                    services.AddApiAuthorization(BaseAddress);
//                    services.AddScoped<AccountClaimsPrincipalFactory<RemoteUserAccount>, OfflineAccountClaimsPrincipalFactory>();
                    services.AddLocalization(options => options.ResourcesPath = "Resources");

                })
                .UseWebRoot("wwwroot");

            if (fileProvider != null)
            {
                hostBuilder.UseStaticFiles(fileProvider);
            }
            else
            {
                hostBuilder.UseStaticFiles();
            }
            var host = hostBuilder.Build();

            MainPage = new ContentPage { Title = "CarChecker" };
            host.AddComponent<Main>(parent: MainPage);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
