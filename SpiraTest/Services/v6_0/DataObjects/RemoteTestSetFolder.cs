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

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single Test Set Folder in the system
    /// </summary>
    public class RemoteTestSetFolder
    {
        /// <summary>
        /// The id of the test set folder
        /// </summary>
        public Nullable<int> TestSetFolderId;

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
        /// The name of the test set folder
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the test set folder
        /// </summary>
        public String Description;

        /// <summary>
        /// The date the test set folder was created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date the test set folder was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date the test sets in the folder were last executed
        /// </summary>
        [ReadOnly]
        public Nullable<DateTime> ExecutionDate;

        /// <summary>
        /// The estimated time to execute the test set
        /// </summary>
        [ReadOnly]
        public Nullable<int> EstimatedDuration;

        /// <summary>
        /// The display name of the project that the test set folder belongs to
        /// </summary>
        public String ProjectName;

        /// <summary>
        /// The ID of the parent folder of this folder (if any)
        /// </summary>
        public int? ParentTestSetFolderId;

        /// <summary>
        /// The actual duration of all the tests in the folder
        /// </summary>
        public int? ActualDuration;

        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountBlocked;
        
        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountCaution;
        
        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountFailed;
        
        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountNotApplicable;
        
        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountNotRun;

        /// <summary>
        /// The count of blocked test sets in the folder
        /// </summary>
        public int CountPassed;
    }
}
