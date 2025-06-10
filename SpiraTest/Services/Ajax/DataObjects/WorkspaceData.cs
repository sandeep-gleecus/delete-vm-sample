using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents the overall set of workspace data for a particular workspace dashboard (project, program, portfolio, etc.)
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class WorkspaceData
    {
        /// <summary>
        /// The primary workspace item
        /// </summary>
        [DataMember(Name = "workspace", EmitDefaultValue = false)]
        public WorkspaceItem Workspace
        {
            get;
            set;
        }

        /// <summary>
        /// The portfolio workspace items
        /// </summary>
        [DataMember(Name = "portfolios", EmitDefaultValue = false)]
        public List<WorkspaceItem> Portfolios
        {
            get;
            set;
        }

        /// <summary>
        /// The program workspace items
        /// </summary>
        [DataMember(Name = "programs", EmitDefaultValue = false)]
        public List<WorkspaceItem> Programs
        {
            get;
            set;
        }

        /// <summary>
        /// The product workspace items
        /// </summary>
        [DataMember(Name = "products", EmitDefaultValue = false)]
        public List<WorkspaceItem> Products
        {
            get;
            set;
        }

        /// <summary>
        /// The release workspace items
        /// </summary>
        [DataMember(Name = "releases", EmitDefaultValue = false)]
        public List<WorkspaceItem> Releases
        {
            get;
            set;
        }

        /// <summary>
        /// The sprint workspace items
        /// </summary>
        [DataMember(Name = "sprints", EmitDefaultValue = false)]
        public List<WorkspaceItem> Sprints
        {
            get;
            set;
        }

        /// <summary>
        /// The task workspace items that are nested under releases
        /// </summary>
        [DataMember(Name = "releaseTasks", EmitDefaultValue = false)]
        public List<WorkspaceItem> ReleaseTasks
        {
            get;
            set;
        }

        /// <summary>
        /// The task workspace items that are nested under sprints
        /// </summary>
        [DataMember(Name = "sprintTasks", EmitDefaultValue = false)]
        public List<WorkspaceItem> SprintTasks
        {
            get;
            set;
        }

        /// <summary>
        /// Used by the product dashboard to know if you should display sprints or just releases.
        /// Not used by the other workspace dashboards
        /// </summary>
        [DataMember(Name = "displaySprints", EmitDefaultValue = false)]
        public bool DisplaySprints
        {
            get;
            set;
        }
    }
}