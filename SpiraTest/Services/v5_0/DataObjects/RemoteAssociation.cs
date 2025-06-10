using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents an association/link between artifacts in the system
    /// </summary>
    public class RemoteAssociation
    {
        /// <summary>
        /// The id of the association/link
        /// </summary>
        public Nullable<int> ArtifactLinkId;
        
        /// <summary>
        /// The artifact id being linked FROM
        /// </summary>
        public int SourceArtifactId;

        /// <summary>
        /// The type of artifact being linked FROM
        /// </summary>
        public int SourceArtifactTypeId;

        /// <summary>
        /// The artifact id being linked TO
        /// </summary>
        public int DestArtifactId;

        /// <summary>
        /// The type of artifact being linked TO
        /// </summary>
        public int DestArtifactTypeId;

        /// <summary>
        /// The type of artifact link (related to, depends-upon, etc.)
        /// </summary>
        public int ArtifactLinkTypeId;

        /// <summary>
        /// The user creating the association/link
        /// </summary>
        /// <remarks>
        /// If not specified, the authenticated user is used
        /// </remarks>
        public Nullable<int> CreatorId;

        /// <summary>
        /// A comment that describes the association in more detail
        /// </summary>
        public string Comment;

        /// <summary>
        /// The date/time the association was created
        /// </summary>
        public Nullable<DateTime> CreationDate;

        /// <summary>
        /// The display name of the artifact being linked to
        /// </summary>
        [ReadOnly]
        public string DestArtifactName;

        /// <summary>
        /// The display name of the type of artifact being linked to
        /// </summary>
        [ReadOnly]
        public string DestArtifactTypeName;

        /// <summary>
        /// The display name of the user that created the association/link
        /// </summary>
        [ReadOnly]
        public string CreatorName;

        /// <summary>
        /// The display name of the type of association/link
        /// </summary>
        [ReadOnly]
        public string ArtifactLinkTypeName;
    }
}