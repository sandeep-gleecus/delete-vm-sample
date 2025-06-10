using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single role's permission (e.g. create + requirement)
    /// </summary>
    public class RemoteRolePermission
    {
        /// <summary>
        /// The artifact type
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The permission id:
        /// None = -1,
        /// ProjectAdmin = -2,
        /// SystemAdmin = -3,
        /// ProjectGroupAdmin = -4,
        /// Create = 1,
        /// Modify = 2,
        /// Delete = 3,
        /// View = 4,
        /// LimitedModify = 5
        /// </summary>
        public int PermissionId;

        /// <summary>
        /// The role
        /// </summary>
        public int ProjectRoleId;
    }
}