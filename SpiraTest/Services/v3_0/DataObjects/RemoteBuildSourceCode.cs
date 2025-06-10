using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a single source code revision association with a SpiraTeam build
    /// </summary>
    public class RemoteBuildSourceCode
    {
        /// <summary>
        /// The id of the build this revision is associated with
        /// </summary>
        public int BuildId;

        /// <summary>
        /// The key that uniquely identifies the revision
        /// </summary>
        public string RevisionKey;

        /// <summary>
        /// The date/time that the revision was associated with the build
        /// </summary>
        /// <remarks>
        /// Pass null to use the current date/time on the server
        /// </remarks>
        public Nullable<DateTime> CreationDate;
    }
}