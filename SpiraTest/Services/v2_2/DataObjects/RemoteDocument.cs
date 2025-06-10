using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a document (attachment/url) in the system
    /// </summary>
    public class RemoteDocument
    {
        public Nullable<int> AttachmentId;
        public int AttachmentTypeId;
        public Nullable<int> ProjectAttachmentTypeId;
        public Nullable<int> ProjectAttachmentFolderId;
        public Nullable<int> ArtifactTypeId;
        public Nullable<int> ArtifactId;
        public int AuthorId;
        public int EditorId;
        public string FilenameOrUrl;
        public string Description;
        public DateTime UploadDate;
        public DateTime EditedDate;
        public int Size;
        public string CurrentVersion;
        public string Tags;
    }
}
