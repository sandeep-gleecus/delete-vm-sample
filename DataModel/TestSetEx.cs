using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the TestSet entity
    /// </summary>
    public partial class TestSet : Artifact
    {
        public const string ARTIFACT_PREFIX = "TX";

        #region Enumerations

        /// <summary>
        /// Test Set Status IDs
        /// </summary>
        public enum TestSetStatusEnum
        {
            NotStarted = 1,
            InProgress = 2,
            Completed = 3,
            Blocked = 4,
            Deferred = 5
        }

        /// <summary>
        /// Test Set Recurrence IDs
        /// </summary>
        public enum RecurrenceEnum
        {
            Hourly = 1,
            Daily = 2,
            Weekly = 3,
            Monthly = 4,
            Quarterly = 5,
            Yearly = 6
        }

        #endregion

        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.TestSet;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.TestSetId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TestSetId;
            }
        }

        /// <summary>
        /// The total count of test cases (regardless of status)
        /// </summary>
        public int TestCaseCount
        {
            get
            {
                return CountBlocked + CountCaution + CountFailed + CountNotApplicable + CountNotRun + CountPassed;
            }
        }
    }
}
