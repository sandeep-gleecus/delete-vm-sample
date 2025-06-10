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
    /// Represents a single Test Case artifact in the system
    /// </summary>
    public class RemoteTestCase : RemoteArtifact
    {
        /// <summary>
        /// The id of the test case
        /// </summary>
        public Nullable<int> TestCaseId;

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
        /// The type of test case, pass null to use the default value
        /// </summary>
        public int? TestCaseTypeId;

        /// <summary>
        /// The status of the test case, pass 0 to use the default value
        /// </summary>
        public int TestCaseStatusId;

        /// <summary>
        /// The id of the folder the test case belongs to. Null = root folder
        /// </summary>
        public int? TestCaseFolderId;

        /// <summary>
        /// The list of components that this test case belongs to
        /// </summary>
        public List<int> ComponentIds;

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
        /// The display name of the user that wrote the test case
        /// </summary>
        [ReadOnly]
        public String AuthorName;

        /// <summary>
        /// The display name of the user that the test case is assigned-to
        /// </summary>
        [ReadOnly]
        public String OwnerName;

        /// <summary>
        /// The display name of the project that the test case belongs to
        /// </summary>
        [ReadOnly]
        public String ProjectName;

        /// <summary>
        /// The display name of the priority of the test case
        /// </summary>
        [ReadOnly]
        public String TestCasePriorityName;

        /// <summary>
        /// The display name of the status of the test case
        /// </summary>
        [ReadOnly]
        public String TestCaseStatusName;

        /// <summary>
        /// The display name of the type of the test case
        /// </summary>
        [ReadOnly]
        public String TestCaseTypeName;

        /// <summary>
        /// The display name of the execution status
        /// </summary>
        [ReadOnly]
        public String ExecutionStatusName;

        /// <summary>
        /// The list of test steps that comprise the test case
        /// </summary>
        public List<RemoteTestStep> TestSteps;

        /// <summary>
        /// The actual result from the most recent test run of the this test case
        /// </summary>
        public int? ActualDuration;
        
        /// <summary>
        /// Have any of the requirements associated with this test case changed
        /// </summary>
        [ReadOnly]
        public bool IsSuspect;

        /// <summary>
        /// Does this test case have steps
        /// </summary>
        [ReadOnly]
        public bool IsTestSteps;
    }
}
