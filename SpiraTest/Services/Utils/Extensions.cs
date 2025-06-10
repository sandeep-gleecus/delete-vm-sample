using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace Inflectra.SpiraTest.Web.Services.Utils
{
    /// <summary>
    /// Utility functions used by the web services
    /// </summary>
    public static class Extensions
    {
        public static string GetHeaderType(this MessageHeaderDescription header)
        {
            return (string)ReflectionUtils.GetValue(header, "BaseType");
        }

        /// <summary>
        /// Helper method for web services to quickly check permissions (used to simpify migration from DataSets > EF)
        /// </summary>
        /// <param name="projectRole">The current list of project roles</param>
        /// <param name="projectRoleId">The id of the role</param>
        /// <param name="artifactTypeId">The type of artfiact</param>
        /// <param name="permissionId">The permission we're checking</param>
        /// <returns>The matching ProjectRolePermission or null</returns>
        public static DataModel.ProjectRolePermission FindByProjectRoleIdArtifactTypeIdPermissionId(this DataModel.ProjectRole projectRole, int projectRoleId, int artifactTypeId, int permissionId)
        {
            if (projectRole.RolePermissions == null)
            {
                return null;
            }
            return projectRole.RolePermissions.FirstOrDefault(p => p.ProjectRoleId == projectRoleId && p.ArtifactTypeId == artifactTypeId && p.PermissionId == permissionId);
        }
    }
}
