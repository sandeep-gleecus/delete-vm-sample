using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v1_2_0
{
    /// <summary>
    /// Represents the project data that can be returned by the web service (used to be a dataset in older versions of the system)
    /// </summary>
    public class ProjectData
    {
        public ProjectData()
        {
            Project = new List<ProjectRow>();
        }

        public List<ProjectRow> Project;
    }

    /// <summary>
    /// A project entry
    /// </summary>
    public class ProjectRow
    {
        /// <summary>
        /// The ID of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The name of the project
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the project
        /// </summary>
        public string Description;

        /// <summary>
        /// The URL of the project
        /// </summary>
        public string Website;

        /// <summary>
        /// The id of the project group it belongs to
        /// </summary>
        public int ProjectGroupId;

        /// <summary>
        /// The name of the project group it belongs to
        /// </summary>
        public string ProjectGroupName;

        /// <summary>
        /// Whether the project is active or not (Y/N)
        /// </summary>
        public string ActiveYn;

        /// <summary>
        /// The creation date of the project
        /// </summary>
        public DateTime CreationDate;
    }
}