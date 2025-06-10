using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a document/attachment folder in the system
    /// </summary>
    public class RemoteDocumentFolder
    {
        /// <summary>
        /// The id of the document folder
        /// </summary>
        public int? ProjectAttachmentFolderId;

        /// <summary>
        /// The id of the project that the folder belongs to
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The id of the parent folder of this folder
        /// </summary>
        /// <remarks>
        /// Null for root folders
        /// </remarks>
        public int? ParentProjectAttachmentFolderId;

        /// <summary>
        /// The name of the folder.
        /// </summary>
        public string Name;


        /// <summary>
        /// The indentation level of the artifact
        /// </summary>
        /// <remarks>
        /// The system uses a set of three-letter segments to denote indent (e.g. AAA followed by AAB, etc.)
        /// </remarks>
        public string IndentLevel;
    }
}