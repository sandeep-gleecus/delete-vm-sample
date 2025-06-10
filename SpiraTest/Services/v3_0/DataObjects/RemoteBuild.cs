using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a single build in SpiraTeam
    /// </summary>
    public class RemoteBuild
    {
        /// <summary>
        /// The id of the build
        /// </summary>
        public int? BuildId;

        /// <summary>
        /// The id of the status of the build (1=Failed, 2=Passed)
        /// </summary>
        public int BuildStatusId;

        /// <summary>
        /// The id of the project the build belongs to
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The id of the release or iteration the build belongs to
        /// </summary>
        public int ReleaseId;

        /// <summary>
        /// The name of the build
        /// </summary>
        public String Name;

        /// <summary>
        /// The detailed description of the host
        /// </summary>
        /// <remarks>
        /// Optional
        /// </remarks>
        public String Description;

        /// <summary>
        /// The date/time that the build was last modified
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date the build was created
        /// </summary>
        /// <remarks>
        /// Pass null to use the current server date/time
        /// </remarks>
        public Nullable<DateTime> CreationDate;

        /// <summary>
        /// The display name of the status of the build
        /// </summary>
        /// <remarks>
        /// Read-only
        /// </remarks>
        public String BuildStatusName;

        /// <summary>
        /// The list of source code revisions associated with the build
        /// </summary>
        public List<RemoteBuildSourceCode> Revisions;
    }
}