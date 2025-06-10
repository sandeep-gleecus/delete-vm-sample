using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a saved filter for a user, includes any sorts
    /// </summary>
    public class RemoteSavedFilter
    {
        /// <summary>
        /// The id of the saved filter
        /// </summary>
        public int SavedFilterId;

        /// <summary>
        /// The id of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The id of the type of artifact this is for
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The name given to the collection of filters
        /// </summary>
        public string Name;

        /// <summary>
        /// Is this a shared filter
        /// </summary>
        public bool IsShared;

        /// <summary>
        /// The sort associated with the filter (not currently used)
        /// </summary>
        public RemoteSort Sort;

        /// <summary>
        /// The individual filters that make up this saved filter
        /// </summary>
        public List<RemoteFilter> Filters;

        /// <summary>
        /// The display name of the artifact type
        /// </summary>
        [ReadOnly]
        public string ArtifactTypeName;

        /// <summary>
        /// The display name of the project
        /// </summary>
        [ReadOnly]
        public string ProjectName;
    }
}