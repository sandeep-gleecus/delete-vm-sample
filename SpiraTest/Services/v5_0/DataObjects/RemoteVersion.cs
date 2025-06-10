using System;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Contains application version information
    /// </summary>
    public class RemoteVersion
    {
        /// <summary>
        /// The version number of the installation
        /// </summary>
		public string Version;

        /// <summary>
        /// The patch number (if any)
        /// </summary>
        /// <remarks>
        /// (Alpha) = -2
        /// (Beta) = -1
        /// </remarks>
		public Nullable<int> Patch;
    }
}
