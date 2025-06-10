using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a source code file
    /// </summary>
    public class RemoteSourceCodeFile
    {
        /// <summary>The file's unique key.</summary>
        public string Id;

        /// <summary>The full filename</summary>
        public string Name;

        /// <summary>The parent folder, if it has one.</summary>
        public RemoteSourceCodeFolder ParentFolder;

        /// <summary>The size of the file, in bytes.</summary>
        public int Size;

        /// <summary>The name of the file's author</summary>
        public string AuthorName;

        /// <summary>The most recent revision of the file</summary>
        public RemoteSourceCodeRevision LastRevision;

        /// <summary>The last update date/time</summary>
        public DateTime LastUpdateDate;

        /// <summary>The name of the last action performed on the file</summary>
        public string Action;

        /// <summary>The file's path (usually the connection string + any sub directories.</summary>
        public string Path;

        /// <summary>
        /// Any artifacts this file is linked to
        /// </summary>
        public List<RemoteLinkedArtifact> LinkedArtifacts;
    }
}