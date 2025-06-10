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

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single Test Case Folder. This is added to the v4.x API
    /// so that clients that need folders when using Spira v5.0 can get all of the folders easily.
    /// This fixes a serious issue that we were having with Rapise.
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
        /// The parent test case folder
        /// </summary>
        public int? ParentTestCaseFolderId;
    }
}
