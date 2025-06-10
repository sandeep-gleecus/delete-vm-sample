using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extension methods/properties for the project resource view entity
    /// </summary>
    public partial class ProjectResourceView : Artifact
    {
        public const string ARTIFACT_PREFIX = "US";

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.UserId;
            }
        }

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
                return ArtifactTypeEnum.User;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.UserId;
            }
        }

        /// <summary>
        /// The amount of effort available to the resource
        /// </summary>
        public int? ResourceEffort
        {
            get
            {
                return this.resourceEffort;
            }
        }
        protected internal int? resourceEffort = null;

        /// <summary>
        /// The amount of effort remaining (i.e. available for new tasks/reqs/incidents)
        /// </summary>
        public int? RemainingEffort
        {
            get
            {
                return this.remainingEffort;
            }
        }
        protected internal int? remainingEffort = null;

        /// <summary>
        /// The total amount of effort allocated to this resource
        /// </summary>
        public int TotalEffort
        {
            get
            {
                return this.totalEffort;
            }
        }
        protected internal int totalEffort = 0;

        /// <summary>
        /// The total amount of effort allocated to this resource that is 'open' (in-progress, not yet done)
        /// </summary>
        public int TotalOpenEffort
        {
            get
            {
                return this.totalOpenEffort;
            }
        }
        protected internal int totalOpenEffort = 0;

        /// <summary>
        /// Is this resource over-allocated
        /// </summary>
        public bool IsOverAllocated
        {
            get
            {
                return this.isOverAllocated;
            }
        }
        protected internal bool isOverAllocated = false;

        /// <summary>
        /// Dummy fields for the allocation equalizer bar graph
        /// </summary>
        public object AllocationIndicator
        {
            get;
            set;
        }
    }
}
