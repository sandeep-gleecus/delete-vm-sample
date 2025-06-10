using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a source code branch
    /// </summary>
    public class RemoteSourceCodeBranch
    {
        /// <summary>
        /// The ID of the branch
        /// </summary>
        public string Id;

        /// <summary>
        /// The display name of the branch
        /// </summary>
        public string Name;

        /// <summary>
        /// Is this the default branch
        /// </summary>
        public bool IsDefault;
    }
}