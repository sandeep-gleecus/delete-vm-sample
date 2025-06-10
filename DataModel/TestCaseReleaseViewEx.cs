using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the TestCaseReleaseView entity
    /// </summary>
    public partial class TestCaseReleaseView : Artifact
    {
        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return TestCase.ARTIFACT_PREFIX;
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
                return this.ArtifactPrefix + ":" + this.TestCaseId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TestCaseId;
            }
        }

        /// <summary>
        /// Is the test case in an active status?
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Draft || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Approved || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForReview || TestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForTest);
            }
        }
    }
}
