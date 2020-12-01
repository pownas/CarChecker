using CarChecker.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarChecker.Client.Auth
{
    /// <summary>
    /// Account management using navigation for web.
    /// </summary>
    public class WebAccountManager : IAccountManager
    {
        private readonly NavigationManager navigationManager;
        private readonly SignOutSessionStateManager signOutManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAccountManager"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to use.</param>
        /// <param name="signOutManager">The signout session state manager to use.</param>
        public WebAccountManager(NavigationManager navigationManager, SignOutSessionStateManager signOutManager)
        {
            if (navigationManager == null)
                throw new ArgumentNullException(nameof(navigationManager));

            this.navigationManager = navigationManager;
            this.signOutManager = signOutManager ?? throw new ArgumentNullException(nameof(signOutManager));
        }

        /// <inheritdoc/>
        public Task Profile()
        {
            this.navigationManager.NavigateTo("authentication/profile");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SignIn()
        {
            this.navigationManager.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(this.navigationManager.Uri)}");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task SignOut()
        {
            await signOutManager.SetSignOutState();
            this.navigationManager.NavigateTo("authentication/logout");
        }
    }
}
