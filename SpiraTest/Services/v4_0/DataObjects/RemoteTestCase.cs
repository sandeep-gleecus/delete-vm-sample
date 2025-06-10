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

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single Test Case artifact in the system
    /// </summary>
    public class RemoteTestCase : RemoteArtifact
    {
        /// <summary>
        /// The id of the test case
        /// </summary>
        public Nullable<int> TestCaseId;

        /// <summary>
        /// The indentation level of the artifact
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        public string IndentLevel;

        /// <summary>
        /// The execution status id of the test case
        /// </summary>
        public Nullable<int> ExecutionStatusId;

        /// <summary>
        /// The id of the user that wrote the test case
        /// </summary>
        /// <remarks>
        /// The authenticated user is used if no value is provided
        /// </remarks>
        public Nullable<int> AuthorId;

        /// <summary>
        /// The id of the user that the test case is assigned-to
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the priority of the test case
        /// </summary>
        public Nullable<int> TestCasePriorityId;

        /// <summary>
        /// The id of the automation engine the associated test script uses (null if manual only)
        /// </summary>
        public Nullable<int> AutomationEngineId;

        /// <summary>
        /// The id of the attachment that is being used to store the test script (file or url)
        /// </summary>
        /// <remarks>
        /// Null if manual only
        /// </remarks>
        public Nullable<int> AutomationAttachmentId;

        /// <summary>
        /// The name of the test case
        /// </summary>
        public String Name;

        /// <summary>
        /// The description of the test case
        /// </summary>
        public String Description;

        /// <summary>
        /// The date the test case was created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date the test case was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date the test case was last executed
        /// </summary>
        public Nullable<DateTime> ExecutionDate;

        /// <summary>
        /// The estimated time to execute the test case
        /// </summary>
        public Nullable<int> EstimatedDuration;

        /// <summary>
        /// Whether this is actually a test case folder or not
        /// </summary>
        public bool Folder;

        /// <summary>
        /// Whether this test case is marked as active or not
        /// </summary>
        public bool Active;

        /// <summary>
        /// The display name of the user that wrote the test case
        /// </summary>
        public String AuthorName;

        /// <summary>
        /// The display name of the user that the test case is assigned-to
        /// </summary>
        public String OwnerName;

        /// <summary>
        /// The display name of the project that the test case belongs to
        /// </summary>
        public String ProjectName;

        /// <summary>
        /// The display name of the priority of the test case
        /// </summary>
        public String TestCasePriorityName;

        /// <summary>
        /// The list of test steps that comprise the test case
        /// </summary>
        public List<RemoteTestStep> TestSteps;
    }
}
