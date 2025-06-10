using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Stores a single project user membership entry
    /// </summary>
    public class RemoteProjectUser : RemoteUser
    {
        /// <summary>
        /// The ID of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The ID of the role the user has on the project
        /// </summary>
        public int ProjectRoleId;

        /// <summary>
        /// The name of the role the user has on the project
        /// </summary>
        /// <remarks>Read-only</remarks>
        public string ProjectRoleName;
    }
}
