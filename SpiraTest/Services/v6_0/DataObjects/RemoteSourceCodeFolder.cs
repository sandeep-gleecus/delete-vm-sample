using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a source code folder
    /// </summary>
    public class RemoteSourceCodeFolder
    {
        /// <summary>
        /// The id of the folder
        /// </summary>
        public string Id;

        /// <summary>The full nema of the folder.</summary>
        public string Name;

        /// <summary>The parent folder, if it has one.</summary>
        public RemoteSourceCodeFolder ParentFolder;

        /// <summary>A list of Files that are contained within this folder.</summary>
        public List<RemoteSourceCodeFile> Files;

        /// <summary>A list of child Folders that are contained within this folder.</summary>
        public List<RemoteSourceCodeFolder> Folders;

        /// <summary>Flag indicating whether the current folder is the root folder or not.</summary>
        public bool IsRoot;
    }
}