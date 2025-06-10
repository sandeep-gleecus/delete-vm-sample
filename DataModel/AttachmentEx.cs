using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the Attachment entity
    /// </summary>
    public partial class Attachment : Artifact
    {
        public const string ARTIFACT_PREFIX = "DC";

        #region Enumerations

        public enum AttachmentTypeEnum
        {
            File = 1,
            URL = 2,
            HTML = 3,
            Markdown = 4,
            SourceCode = -2
        }

        #endregion

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
        /// Used to store the project id for use by the HistoryManager and Notification Manager
        /// </summary>
        public int ProjectId
        {
            get;
            set;
        }

        /// <summary>
        /// Used to store the tags that used to be physically stored on the attachment, but now use the general ArtifactTags entity instead
        /// </summary>
        public string Tags
        {
            get;
            set;
        }
    }
}
