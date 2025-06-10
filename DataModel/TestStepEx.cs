using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the test step artifact
    /// </summary>
    public partial class TestStep : Artifact
    {
        public const string ARTIFACT_PREFIX = "TS";

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
                return ArtifactTypeEnum.TestStep;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.TestStepId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.TestStepId;
            }
        }

        public int? ProjectId
        {
            get
            {
                if (this.TestCase != null)
                {
                    return this.TestCase.ProjectId;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the parent test case's name if available (used by history)
        /// </summary>
        public string Name
        {
            get
            {
                if (this.TestCase != null)
                {
                    return this.TestCase.Name;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the name of the execution status
        /// </summary>
        public string ExecutionStatusName
        {
            get
            {
                if (this.ExecutionStatus != null)
                {
                    return this.ExecutionStatus.Name;
                }
                return null;
            }
        }
    }
}
