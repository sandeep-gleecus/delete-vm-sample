using System;
using System.Collections.Generic;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single Project Role in the system
    /// </summary>
    public class RemoteProjectRole
    {
        /// <summary>
        /// The id of the project role
        /// </summary>
        public Nullable<int> ProjectRoleId;

        /// <summary>
        /// The name of the project role
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the project role
        /// </summary>
        public String Description;

        /// <summary>
        /// Whether the role is active or not
        /// </summary>
        public bool Active;

        /// <summary>
        /// Whether the role is a project owner or not
        /// </summary>
        public bool Admin;

        /// <summary>
        /// Can this role add documents to the project
        /// </summary>
        public bool DocumentsAdd;

        /// <summary>
        /// Can this role edit documents in the project
        /// </summary>
        public bool DocumentsEdit;

        /// <summary>
        /// Can this role delete documents in the project
        /// </summary>
        public bool DocumentsDelete;

        /// <summary>
        /// Can this role add discussions/comments to the project
        /// </summary>
        public bool DiscussionsAdd;

        /// <summary>
        /// Can this role view the source code repository of the project
        /// </summary>
        public bool SourceCodeView;
    }
}
