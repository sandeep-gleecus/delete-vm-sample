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

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>Represents a document (attachment/url) in the system</summary>
    public class RemoteDocument : RemoteArtifact
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
        /// The id of the document type relative to the current project template
        /// </summary>
        public Nullable<int> DocumentTypeId;

        /// <summary>
        /// The id of the document status relative to the current project template
        /// </summary>
        public Nullable<int> DocumentStatusId;

        /// <summary>
        /// The id of the attachment folder id for the current project
        /// </summary>
        public Nullable<int> ProjectAttachmentFolderId;

        /// <summary>
        /// The list of artifacts the document is attached to
        /// </summary>
        public List<RemoteLinkedArtifact> AttachedArtifacts;

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
        /// The list of document versions
        /// </summary>
        public List<RemoteDocumentVersion> Versions;

        /// <summary>
        /// The display name of the attachment type relative to the current project template
        /// </summary>
        /// <remarks>
        /// This is not whether it's a file or url, but the project-template specific classification
        /// </remarks>
        public string DocumentTypeName;

        /// <summary>
        /// The display name of the document status relative to the current project template
        /// </summary>
        public string DocumentStatusName;

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

    /// <summary>
    /// Extends class to allow you to specify the file binary data
    /// </summary>
    public class RemoteDocumentFile : RemoteDocument
    {
        /// <summary>
        /// The file data base64 encoded if using the REST service
        /// </summary>
        public byte[] BinaryData;
    } 
}
