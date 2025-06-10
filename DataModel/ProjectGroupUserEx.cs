using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions ot the project group user entity
    /// </summary>
    public partial class ProjectGroupUser : Entity
    {
        /// <summary>
        /// Returns the name of the role
        /// </summary>
        public string ProjectGroupRoleName
        {
            get
            {
                if (this.ProjectGroupRole != null)
                {
                    return this.ProjectGroupRole.Name;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Returns the full name of the user
        /// </summary>
        public string FullName
        {
            get
            {
                if (this.User != null)
                {
                    return this.User.FullName;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Returns the login name of the user
        /// </summary>
        public string UserName
        {
            get
            {
                if (this.User != null)
                {
                    return this.User.UserName;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
