using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents an artifact that something has linked to
    /// </summary>
    public class RemoteLinkedArtifact
    {
        /// <summary>
        /// The ID of the artifact
        /// </summary>
        public int ArtifactId;

        /// <summary>
        /// The ID of the type of the artifact
        /// </summary>
        /// <remarks>
        /// None = 0,
        /// Requirement = 1,
        /// TestCase = 2,
        /// Incident = 3,
        /// Release = 4,
        /// TestRun = 5,
        /// Task = 6,
        /// TestStep = 7,
        /// TestSet = 8,
        /// AutomationHost = 9,
        /// AutomationEngine = 10,
        /// Placeholder = 11,
        /// RequirementStep = 12,
        /// Document = 13
        /// </remarks>
        public int ArtifactTypeId;

        /// <summary>
        /// The name of the artifact
        /// </summary>
        [ReadOnly]
        public string Name;

        /// <summary>
        /// The artifact's status name/description
        /// </summary>
        [ReadOnly]
        public string Status;
    }
}