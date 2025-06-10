using System;
using System.Collections.Generic;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a single Project Role in the system
    /// </summary>
    public class RemoteProjectRole
    {
        public Nullable<int> ProjectRoleId;
        public string Name;
        public String Description;
        public bool Active;
        public bool Admin;
        public bool DocumentsAdd;
        public bool DocumentsEdit;
        public bool DocumentsDelete;
        public bool DiscussionsAdd;
        public bool SourceCodeView;
    }
}
