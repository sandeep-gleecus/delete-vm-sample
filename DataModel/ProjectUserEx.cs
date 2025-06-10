using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the projectuser entity
    /// </summary>
    public partial class ProjectUser : Entity
    {
        /// <summary>
        /// Returns the role name
        /// </summary>
        public string ProjectRoleName
        {
            get
            {
                if (Role != null)
                {
                    return Role.Name;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the project name
        /// </summary>
        public string ProjectName
        {
            get
            {
                if (Project != null)
                {
                    return Project.Name;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the user's login name
        /// </summary>
        public string UserName
        {
            get
            {
                if (User != null)
                {
                    return User.UserName;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the user's full name (from their profile)
        /// </summary>
        public string FullName
        {
            get
            {
                if (User != null && User.Profile != null)
                {
                    return User.Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
