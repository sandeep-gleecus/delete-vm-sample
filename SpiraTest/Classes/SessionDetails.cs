using System;
using System.Collections.Generic;
using System.Web;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Stores the information about a user's session.
    /// Used to track the concurrent licensing and users
    /// </summary>
    public class SessionDetails
    {
        /// <summary>
        /// The id of the current user
        /// </summary>
        public int UserId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the external plugin using the web service API. Set to NULL if the session is from the
        /// SpiraTeam web application itself
        /// </summary>
        public string PlugInName
        {
            get;
            set;
        }

        /// <summary>
        /// Denotes if a user session counts against the # concurrent user license available
        /// </summary>
        public bool LicenseUsed
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SessionDetails()
        {
            this.UserId = 0;
            this.LicenseUsed = true;
            this.PlugInName = "";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="licenseUsed">Are we using up a concurrent license</param>
        /// <param name="plugInName">What is the name of the plug-in</param>
        /// <param name="userId">The id of the current user</param>
        public SessionDetails(int userId, bool licenseUsed, string plugInName)
        {
            this.UserId = userId;
            this.LicenseUsed = licenseUsed;
            this.PlugInName = plugInName;
        }
    }
}
