using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// The base data object for all artifact data objects
    /// </summary>
    public class RemoteArtifact
    {
        /// <summary>
        /// The id of the project that the artifact belongs to
        /// </summary>
        /// <remarks>
        /// The current project is always used for Insert operations for security reasons
        /// </remarks>
        public int? ProjectId;

        /// <summary>
        /// The type of artifact that we have
        /// </summary>
        [ReadOnly]
        public int ArtifactTypeId;

        /// <summary>
        /// The datetime used to track optimistic concurrency to prevent edit conflicts
        /// </summary>
        public DateTime ConcurrencyDate;

        /// <summary>
        /// The list of associated custom properties/fields for this artifact
        /// </summary>
        public List<RemoteArtifactCustomProperty> CustomProperties;

        /// <summary>
        /// Does this artifact have any attachments?
        /// </summary>
        [ReadOnly]
        public bool IsAttachments;
    }
}
