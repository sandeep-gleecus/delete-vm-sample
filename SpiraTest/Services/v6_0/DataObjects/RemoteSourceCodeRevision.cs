using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single source code revision
    /// </summary>
    public class RemoteSourceCodeRevision
    {
        /// <summary>The ID of the revision</summary>
        public string Id;

        /// <summary>The display name of the revision.</summary>
        public string Name;

        /// <summary>The name of the author who submitted the update.</summary>
        public string AuthorName;

        /// <summary>The commit message submnitted with the changes.</summary>
        public string Message;

        /// <summary>The date of the revision.</summary>
        public DateTime UpdateDate;

        /// <summary>Flag specifying wether the Content has been changed.</summary>
        public bool ContentChanged;

        /// <summary>Flag indicating whether the Properties have been changed.</summary>
        public bool PropertiesChanged;

        /// <summary>A list of files that were included in this revision.</summary>
        public List<RemoteSourceCodeFile> Files;
    }
}