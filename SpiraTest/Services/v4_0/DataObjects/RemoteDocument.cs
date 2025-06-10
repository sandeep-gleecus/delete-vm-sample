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

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>Represents a document (attachment/url) in the system</summary>
    public class RemoteDocument
    {
        /// <summary>
        /// The id of the attachment
        /// </summary>
        public Nullable<int> AttachmentId;

        /// <summary>
        /// The id of the attachment type
        /// </summary>
        /// <remarks>
        /// File = 1,
        /// URL = 2
        /// </remarks>
        public int AttachmentTypeId;

        /// <summary>
        /// The id of the attachment type relative to the current project
        /// </summary>
        public Nullable<int> ProjectAttachmentTypeId;

        /// <summary>
        /// The id of the attachment folder id for the current project
        /// </summary>
        public Nullable<int> ProjectAttachmentFolderId;

        /// <summary>
        /// The id of the type of artifact it's attached to
        /// </summary>
        /// <remarks>
        /// Requirement = 1,
        ///	TestCase = 2,
        /// Incident = 3,
        ///	Release = 4,
        ///	TestRun = 5,
        ///	Task = 6,
        /// TestStep = 7,
        /// TestSet = 8
        /// </remarks>
        public Nullable<int> ArtifactTypeId;

        /// <summary>
        /// The id of the artifact it's being attached to
        /// </summary>
        public Nullable<int> ArtifactId;

        /// <summary>
        /// The id of the user that uploaded the attachment
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used
        /// </remarks>
        public Nullable<int> AuthorId;

        /// <summary>
        /// The id of the user that edited the document
        /// </summary>
        public Nullable<int> EditorId;

        /// <summary>
        /// The filename of the file (if a file attachment) or the full URL if a URL attachment
        /// </summary>
        public string FilenameOrUrl;

        /// <summary>
        /// The description of the attachment
        /// </summary>
        public string Description;

        /// <summary>
        /// The date/time the attachment was uploaded
        /// </summary>
        public DateTime UploadDate;

        /// <summary>
        /// The date/time the attachment was last edited
        /// </summary>
        public DateTime EditedDate;

        /// <summary>
        /// The size of the attachment in bytes
        /// </summary>
        /// <remarks>
        /// Pass 0 for a URL attachment
        /// </remarks>
        public int Size;

        /// <summary>
        /// The version name of the current attachment
        /// </summary>
        public string CurrentVersion;

        /// <summary>
        /// The list of meta-tags that should be associated with the attachment
        /// </summary>
        public string Tags;

        /// <summary>
        /// The list of document versions
        /// </summary>
        public List<RemoteDocumentVersion> Versions;

        /// <summary>
        /// The display name of the attachment type relative to the current project
        /// </summary>
        /// <remarks>
        /// This is not whether it's a file or url, but the project-specific classification
        /// </remarks>
        public string ProjectAttachmentTypeName;

        /// <summary>
        /// The display name of the attachment type (i.e. whether it's a file or url)
        /// </summary>
        public string AttachmentTypeName;
        
        /// <summary>
        /// The display name of the user that uploaded the attachment
        /// </summary>
        public string AuthorName;

        /// <summary>
        /// The display name of the user that edited the document
        /// </summary>
        public string EditorName;
    }
}
