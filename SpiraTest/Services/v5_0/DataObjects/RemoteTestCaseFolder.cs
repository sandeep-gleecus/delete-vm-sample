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
    /// Represents a single Test Case Folder in the system
    /// </summary>
    public class RemoteTestCaseFolder
    {
        /// <summary>
        /// The id of the test case folder
        /// </summary>
        public Nullable<int> TestCaseFolderId;

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
        /// The name of the test case folder
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the test case folder
        /// </summary>
        public String Description;

        /// <summary>
        /// The date the test case folder was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date the test cases in the folder were last executed
        /// </summary>
        [ReadOnly]
        public Nullable<DateTime> ExecutionDate;

        /// <summary>
        /// The estimated time to execute the test case
        /// </summary>
        [ReadOnly]
        public Nullable<int> EstimatedDuration;

        /// <summary>
        /// The display name of the project that the test case folder belongs to
        /// </summary>
        public String ProjectName;

        /// <summary>
        /// The ID of the parent folder of this folder (if any)
        /// </summary>
        public int? ParentTestCaseFolderId;

        /// <summary>
        /// The actual duration of all the tests in the folder
        /// </summary>
        public int? ActualDuration;

        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountBlocked;
        
        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountCaution;
        
        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountFailed;
        
        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountNotApplicable;
        
        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountNotRun;

        /// <summary>
        /// The count of blocked test cases in the folder
        /// </summary>
        public int CountPassed;
    }
}
