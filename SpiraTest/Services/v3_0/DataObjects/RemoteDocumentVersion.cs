using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a document version in the system
    /// </summary>
    public class RemoteDocumentVersion
    {
        /// <summary>
        /// The id of the document version
        /// </summary>
        public int? AttachmentVersionId;

        /// <summary>
        /// The id of the document
        /// </summary>
        public int AttachmentId;

        /// <summary>
        /// The id of the user that uploaded the version
        /// </summary>
        /// <remarks>
        /// If no value is provided, the authenticated user is used
        /// </remarks>
        public Nullable<int> AuthorId;

        /// <summary>
        /// The filename of the file (if a file attachment) or the full URL if a URL attachment
        /// </summary>
        public string FilenameOrUrl;

        /// <summary>
        /// The description of the attachment version
        /// </summary>
        public string Description;

        /// <summary>
        /// The date/time the attachment version was uploaded
        /// </summary>
        public DateTime UploadDate;

        /// <summary>
        /// The size of the attachment version in bytes
        /// </summary>
        /// <remarks>
        /// Pass 0 for a URL attachment
        /// </remarks>
        public int Size;

        /// <summary>
        /// The version number
        /// </summary>
        public string VersionNumber;
        
        /// <summary>
        /// The display name of the user that uploaded the attachment version
        /// </summary>
        public string AuthorName;
    }
}
