using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Release entity
    /// </summary>
    public partial class ReleaseView : Artifact
    {
        public const string ARTIFACT_PREFIX = "RL";

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
                return ArtifactTypeEnum.Release;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.ReleaseId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.ReleaseId;
            }
        }

        /// <summary>
        /// Is the release in an active status?
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress || ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed);
            }
        }

        /// <summary>
        /// Is the release an iteration or phase
        /// </summary>
        public bool IsIterationOrPhase
        {
            get
            {
                return (ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase);
            }
        }

        /// <summary>
        /// Is the release an iteration
        /// </summary>
        public bool IsIteration
        {
            get
            {
                return (ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration);
            }
        }

        /// <summary>
        /// Returns the combined % of the tasks that are are on track / completed and running late (but started)
        /// </summary>
        public int TaskPercentComplete
        {
            get
            {
                return TaskPercentOnTime + TaskPercentLateFinish;
            }
        }

        /// <summary>
        /// The remaining available points left in the release
        /// </summary>
        public decimal? AvailablePoints
        {
            get
            {
                if (PlannedPoints.HasValue && RequirementPoints.HasValue)
                {
                    return PlannedPoints - RequirementPoints;
                }
                return null;
            }
        }
    }
}
