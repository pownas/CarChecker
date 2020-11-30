using Microsoft.MobileBlazorBindings.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CarChecker.Auth
{
    /// <summary>
    /// Account manager class that uses <see cref="IAuthenticationService"/> to sign in and sign out.
    /// </summary>
    internal class AppAccountManager : IAccountManager
    {
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAccountManager"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service to use.</param>
        public AppAccountManager(IAuthenticationService authenticationService)
        {
            if (authenticationService == null)
                throw new ArgumentNullException(nameof(authenticationService));

            this.authenticationService = authenticationService;
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
