using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the test run entity
    /// </summary>
    public partial class TestRun : Artifact
    {
        public const string ARTIFACT_PREFIX = "TR";

        #region Enumerations

        /// <summary>
        /// Test Run Types
        /// </summary>
        public enum TestRunTypeEnum
        {
            Manual = 1,
            Automated = 2
        }

        /// <summary>
        /// Test Run Formats (automated test runs only)
        /// </summary>
        public enum TestRunFormatEnum
        {
            PlainText = 1,
            HTML = 2
        }

        #endregion

        #region Overrides

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
                return ArtifactTypeEnum.TestRun;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.TestRunId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TestRunId;
            }
        }

        #endregion

        /// <summary>
        /// Used to store the project ID, needed by the history manager
        /// </summary>
        /// <see cref="HistoryManager"/>
        public int ProjectId
        {
            get;
            set;
        }
    }
}
