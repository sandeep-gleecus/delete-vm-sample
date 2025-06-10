using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extensions to the TestCaseFolder artifact
    /// </summary>
    public partial class TestCaseFolder : Artifact
    {
        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.TestCase;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.TestCaseFolderId.ToString();
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TestCaseFolderId;
            }
        }

        /// <summary>
        /// Dummy field used to allow folders to display execution status in list view
        /// </summary>
        public int ExecutionStatusId
        {
            get;
            set;
        }

        /// <summary>
        /// The total count of test cases
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
