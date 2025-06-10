using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single Task Folder in the system
    /// </summary>
    public class RemoteTaskFolder
    {
        /// <summary>
        /// The id of the task folder
        /// </summary>
        public Nullable<int> TaskFolderId;

        /// <summary>
        /// The id of the project this folder belongs to
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The indentation level of the folder
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        [ReadOnly]
        public string IndentLevel;

        /// <summary>
        /// The name of the task folder
        /// </summary>
        public String Name;

        /// <summary>
        /// The ID of the parent folder of this folder (if any)
        /// </summary>
        public int? ParentTaskFolderId;
    }
}
