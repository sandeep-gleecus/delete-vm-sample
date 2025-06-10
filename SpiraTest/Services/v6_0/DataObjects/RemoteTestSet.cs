using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single Test Set artifact in the system
    /// </summary>
    public class RemoteTestSet : RemoteArtifact
    {
        /// <summary>
        /// The id of the test set
        /// </summary>
        public Nullable<int> TestSetId;

        /// <summary>
        /// (Not used in this version of the API)
        /// </summary>
        public string IndentLevel;
        
        /// <summary>
        /// The id of the test set's status
        /// </summary>
        public int TestSetStatusId;

        /// <summary>
        /// The id of the user who created the test set
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used
        /// </remarks>
        public Nullable<int> CreatorId;

        /// <summary>
        /// The id of the user who the test set is assigned-to
        /// </summary>
        public Nullable<int> OwnerId;

        /// <summary>
        /// The id of the release that the test set is assigned-to
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The id of the automation host the test set is assigned-to
        /// </summary>
        public Nullable<int> AutomationHostId;

        /// <summary>
        /// The id of the type of test set (1 = Manual, 2 = Automated)
        /// </summary>
        public Nullable<int> TestRunTypeId;

        /// <summary>
        /// The id of the recurrence pattern the test set is scheduled for
        /// </summary>
        public Nullable<int> RecurrenceId;

        /// <summary>
        /// The name of the test set
        /// </summary>
        public String Name;

        /// <summary>
        /// The detailed description of the test set
        /// </summary>
        public String Description;

        /// <summary>
        /// The date the test set was originally created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date the test set was last modified
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date that the test set needs is planned to be executed on
        /// </summary>
        public Nullable<DateTime> PlannedDate;

        /// <summary>
        /// The date that the test set was last executed by a tester
        /// </summary>
        public Nullable<DateTime> ExecutionDate;

        /// <summary>
        /// How many passed test cases are in the set
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountPassed;

        /// <summary>
        /// How many failed test cases are in the set
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountFailed;

        /// <summary>
        /// How many cautioned test cases are in the set
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountCaution;

        /// <summary>
        /// How many blocked test cases are in the set
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountBlocked;

        /// <summary>
        /// How many test cases in the set have not been run
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountNotRun;

        /// <summary>
        /// How many test cases in the set are not applicable
        /// </summary>
        [ReadOnly]
        public Nullable<int> CountNotApplicable;

        /// <summary>
        /// The display name of the user that created the test set
        /// </summary>
        [ReadOnly]
        public String CreatorName;

        /// <summary>
        /// The display name of the user that the test set is assigned-to
        /// </summary>
        [ReadOnly]
        public String OwnerName;

        /// <summary>
        /// The display name of the project that the test set belongs to
        /// </summary>
        [ReadOnly]
        public String ProjectName;

        /// <summary>
        /// The display name of the status of the test set
        /// </summary>
        [ReadOnly]
        public String TestSetStatusName;

        /// <summary>
        /// The version number of the release the test set is scheduled for
        /// </summary>
        [ReadOnly]
        public String ReleaseVersionNumber;

        /// <summary>
        /// The display name of the recurrence pattern
        /// </summary>
        [ReadOnly]
        public String RecurrenceName;

        /// <summary>
        /// The ID of the test set folder this test set belongs to (NULL = root)
        /// </summary>
        public int? TestSetFolderId;

        /// <summary>
        /// The total estimated duration for all the test cases in this set
        /// </summary>
        [ReadOnly]
        public int? EstimatedDuration;

        /// <summary>
        /// The total actual duration for all the test cases in this set
        /// </summary>
        [ReadOnly]
        public int? ActualDuration;
        
        /// <summary>
        /// Is this test set auto-scheduled when a build associated with the release runs
        /// </summary>
        public bool IsAutoScheduled;

        /// <summary>
        /// Is this a dynamic test set
        /// </summary>
        public bool IsDynamic;

        /// <summary>
        /// The underlying query if this is a dynamic test set
        /// </summary>
        public string DynamicQuery;

        /// <summary>
        /// The id of any test configuration set to be used with this test set
        /// </summary>
        public int? TestConfigurationSetId;

        /// <summary>
        /// The interval between a build finishing and the test being execution (if auto-scheduled)
        /// </summary>
        public int? BuildExecuteTimeInterval;
    }
}
