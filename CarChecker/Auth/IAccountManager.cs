using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CarChecker.Auth
{
    /// <summary>
    /// Account manager interface.
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Signs the user in.
        /// </summary>
        Task SignIn();

        /// <summary>
        /// Signs the user out.
        /// </summary>
        Task SignOut();
    }
}
