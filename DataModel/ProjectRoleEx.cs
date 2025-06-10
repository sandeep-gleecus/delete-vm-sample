using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the project role
    /// </summary>
    [Serializable]
    public partial class ProjectRole : Entity, ISerializable 
    {
        /// <summary>
        /// Normal Constructor
        /// </summary>
        public ProjectRole()
        {
        }

        /// <summary>
        /// Serializes the data
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("ProjectRoleId", this.ProjectRoleId, typeof(int));
            info.AddValue("Name", this.Name, typeof(string));
            info.AddValue("Description", this.Description, typeof(string));
            info.AddValue("IsActive", this.IsActive, typeof(bool));
            info.AddValue("IsAdmin", this.IsAdmin, typeof(bool));
            info.AddValue("IsDiscussionsAdd", this.IsDiscussionsAdd, typeof(bool));
            info.AddValue("IsLimitedView", this.IsLimitedView, typeof(bool));
            info.AddValue("IsSourceCodeEdit", this.IsSourceCodeEdit, typeof(bool));
            info.AddValue("IsSourceCodeView", this.IsSourceCodeView, typeof(bool));

            //Also serialize the permissions
            if (this.RolePermissions != null && this.RolePermissions.Count > 0)
            {
                info.AddValue("RolePermissions_Count", this.RolePermissions.Count, typeof(int));
                for (int i = 0; i < this.RolePermissions.Count; i++)
                {
                    info.AddValue("RolePermissions_" + i + "_ProjectRoleId", this.RolePermissions[i].ProjectRoleId, typeof(int));
                    info.AddValue("RolePermissions_" + i + "_PermissionId", this.RolePermissions[i].PermissionId, typeof(int));
                    info.AddValue("RolePermissions_" + i + "_ArtifactTypeId", this.RolePermissions[i].ArtifactTypeId, typeof(int));
                }
            }
            else
            {
                info.AddValue("RolePermissions_Count", 0, typeof(int));
            }
        }

        /// <summary>
        /// Deserialize the data
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ProjectRole(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            this.ProjectRoleId = (int)info.GetValue("ProjectRoleId", typeof(int));
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Description = (string)info.GetValue("Description", typeof(string));
            this.IsActive = (bool)info.GetValue("IsActive", typeof(bool));
            this.IsAdmin = (bool)info.GetValue("IsAdmin", typeof(bool));
            this.IsDiscussionsAdd = (bool)info.GetValue("IsDiscussionsAdd", typeof(bool));
            this.IsLimitedView = (bool)info.GetValue("IsLimitedView", typeof(bool));
            this.IsSourceCodeEdit = (bool)info.GetValue("IsSourceCodeEdit", typeof(bool));
            this.IsSourceCodeView = (bool)info.GetValue("IsSourceCodeView", typeof(bool));

            //Also deserialize the permissions
            int permissionsCount = (int)info.GetValue("RolePermissions_Count", typeof(int));
            if (permissionsCount > 0)
            {
                for (int i = 0; i < permissionsCount; i++)
                {
                    ProjectRolePermission projectRolePermission = new DataModel.ProjectRolePermission();
                    projectRolePermission.ProjectRoleId = (int)info.GetValue("RolePermissions_" + i + "_ProjectRoleId", typeof(int));
                    projectRolePermission.PermissionId = (int)info.GetValue("RolePermissions_" + i + "_PermissionId", typeof(int));
                    projectRolePermission.ArtifactTypeId = (int)info.GetValue("RolePermissions_" + i + "_ArtifactTypeId", typeof(int));
                    this.RolePermissions.Add(projectRolePermission);
                }
            }
        }
    }
}
