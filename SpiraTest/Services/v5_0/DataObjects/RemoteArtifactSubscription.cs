using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents an artifact subscription in the system
    /// </summary>
    public class RemoteArtifactSubscription
    {
        /// <summary>
        /// The id of the artifact
        /// </summary>
        public int ArtifactId;

        /// <summary>
        /// The id of the artifact type
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The name of the artifact
        /// </summary>
        [ReadOnly]
        public string ArtifactName;

        /// <summary>
        /// The long description of the artifact
        /// </summary>
        [ReadOnly]
        public string ArtifactDescription;

        /// <summary>
        /// The display name of the type of artifact
        /// </summary>
        [ReadOnly]
        public string ArtifactTypeName;

        /// <summary>
        /// The id of the project
        /// </summary>
        public int ProjectId;
        
        /// <summary>
        /// The name of the project
        /// </summary>
        [ReadOnly]
        public string ProjectName;

        /// <summary>
        /// The id of the user
        /// </summary>
        public int UserId;
        
        /// <summary>
        /// The date/time the artifact was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The full name of the user
        /// </summary>
        [ReadOnly]
        public string UserFullName;
    }
}