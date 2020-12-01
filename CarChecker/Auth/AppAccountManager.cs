using Microsoft.Extensions.Options;
using Microsoft.MobileBlazorBindings.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CarChecker.Auth
{
    /// <summary>
    /// Account manager class that uses <see cref="IAuthenticationService"/> to sign in and sign out.
    /// </summary>
    internal class AppAccountManager : IAccountManager
    {
        private readonly IAuthenticationService authenticationService;
        private readonly RemoteAuthenticationOptions<ApiAuthorizationProviderOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAccountManager"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service to use.</param>
        public AppAccountManager(IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> options, IAuthenticationService authenticationService)
        {
            if (authenticationService == null)
                throw new ArgumentNullException(nameof(authenticationService));

            this.authenticationService = authenticationService;
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Profile()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.WPF:
                    // Opens request in the browser.
                    var ps = new ProcessStartInfo(this.options.AuthenticationPaths.RemoteProfilePath)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(ps);
                    break;
                case Device.iOS:
                case Device.macOS:
                    await Browser.OpenAsync(this.options.AuthenticationPaths.RemoteProfilePath, BrowserLaunchMode.External);
                    break;
                default:
                    await Browser.OpenAsync(this.options.AuthenticationPaths.RemoteProfilePath, BrowserLaunchMode.SystemPreferred);
                    break;
            }
        }

        /// <inheritdoc />
        public async Task SignIn()
        {
            await this.authenticationService.SignIn();
        }

        /// <inheritdoc/>
        public async Task SignOut()
        {
            await this.authenticationService.SignOut();
        }
    }
}
