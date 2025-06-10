using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the ProjectAttachment entity
    /// </summary>
    public partial class ProjectAttachment : Artifact
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

        /// <summary>
        /// The name of the attachment, points to the Filename field (if available otherwise the token is used)
        /// </summary>
        public string Filename
        {
            get
            {
                if (this.Attachment != null)
                {
                    return this.Attachment.Filename;
                }
                return this.ArtifactToken;
            }
        }
    }
}
