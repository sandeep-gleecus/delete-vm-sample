using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the ProjectAttachmentView entity
    /// </summary>
    public partial class ProjectAttachmentView : Artifact
    {
        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return Attachment.ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.Document;
            }
        }

        /// <summary>The Artifact's token.</summary>
        public override string ArtifactToken
        {
            get
            {
                return this.ArtifactPrefix + ":" + this.AttachmentId;
            }
        }

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.AttachmentId;
            }
        }

        /// <summary>The Document versions field - used for workflow control. Added here to make sure populate row sends the data to the page, without this field needing to be returned from the database </summary>
        public bool DocumentVersions
        {
            get
            {
                return true;
            }
        }

    }
}
