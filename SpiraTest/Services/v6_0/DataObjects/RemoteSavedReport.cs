using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single saved report in the system
    /// </summary>
    public class RemoteSavedReport
    {
        /// <summary>
        /// The id of the saved report
        /// </summary>
        public int SavedReportId;

        /// <summary>
        /// The id of the project
        /// </summary>
        public int? ProjectId;

        /// <summary>
        /// The name of the saved report
        /// </summary>
        public string Name;

        /// <summary>
        /// Is this a shared saved report
        /// </summary>
        public bool IsShared;

        /// <summary>
        /// The id of the format of the report
        /// </summary>
        public int ReportFormatId;
    }
}