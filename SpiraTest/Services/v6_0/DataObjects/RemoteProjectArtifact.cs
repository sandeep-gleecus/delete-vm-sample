using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single project and a list of associated artifacts
    /// </summary>
    public class RemoteProjectArtifact
    {
        /// <summary>
        /// The id of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The list of artifacts in this project
        /// </summary>
        public List<int> ArtifactIds;
    }
}