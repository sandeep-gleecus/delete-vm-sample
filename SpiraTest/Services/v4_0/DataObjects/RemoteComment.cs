using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    public class RemoteComment
    {
		/// <summary>The unique ID for this comment / artifact type.</summary>
        public Nullable<int> CommentId;

		/// <summary>The artifact ID that this comment belongs to.</summary>
		public int ArtifactId;

		/// <summary>
        /// The userID of the author.
        /// </summary>
        /// <remarks>The authenticated user is used if no value provided</remarks>
        public Nullable<int> UserId;

		/// <summary>The full name of the author.</summary>
		public string UserName;

		/// <summary>The text of the comment.</summary>
		public string Text;

		/// <summary>The date and time the comment was made.</summary>
		public Nullable<DateTime> CreationDate;

		/// <summary>Whether the comment was marked for deletion or hidden.</summary>
		/// <remarks>Not currently used, should remain false.</remarks>
		public bool IsDeleted;
    }
}
