using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents the credentials used to connect to the SOAP API
    /// </summary>
    /// <remarks>
    /// The login is always populated. Either the password or API is populated
    /// </remarks>
    public class RemoteCredentials
    {
        /// <summary>
        /// The ID of the user. This is read-only and not needed for authentication, it is just returned by the API
        /// when you use the connection option as a reference
        /// </summary>
        [ReadOnly]
        public int UserId;

        /// <summary>
        /// A valid login for connecting to the system
        /// </summary>
        public string UserName;

        /// <summary>
        /// The password for the user name
        /// </summary>
        /// <remarks>Either pass this or the API Key</remarks>
        public string Password;

        /// <summary>
        /// The API Key (RSS Token) for the user nsme
        /// </summary>
        /// <remarks>Either pass this or the Password</remarks>
        public string ApiKey;

        /// <summary>
        /// The name of the client/plugin calling the API Key (optional)
        /// </summary>
        [Optional]
        public string PlugInName;

        /// <summary>
        /// Is the user a system admin. This is read-only and returned by the API.
        /// </summary>
        [ReadOnly]
        public bool IsSystemAdmin;
    }
}