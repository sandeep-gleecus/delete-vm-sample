using System;
using System.Collections.Generic;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Stores a single project user membership entry
    /// </summary>
    public class RemoteProjectUser
    {
        /// <summary>
        /// The ID of the project
        /// </summary>
        public int ProjectId
        {
            get;
            set;
        }

        /// <summary>
        /// The ID of the user
        /// </summary>
        public int UserId
        {
            get;
            set;
        }

        /// <summary>
        /// The ID of the role the user has on the project
        /// </summary>
        public int ProjectRoleId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the role the user has on the project
        /// </summary>
        /// <remarks>Read-only</remarks>
        public string ProjectRoleName
        {
            get;
            set;
        }
    }
}
