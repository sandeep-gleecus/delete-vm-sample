using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents the name/status of a workspace object (project, program, etc.)
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class WorkspaceItem
    {
        /// <summary>
        /// The id of the workspace
        /// </summary>
        [DataMember(Name = "workspaceId", EmitDefaultValue = true)]
        public int WorkspaceId
        {
            get;
            set;
        }

        /// <summary>
        /// The parent id of the workspace
        /// </summary>
        [DataMember(Name = "parentId", EmitDefaultValue = false)]
        public int? ParentId
        {
            get;
            set;
        }

        /// <summary>
        /// Sometimes (eg releases) the parent can be the same workspace type as the child. This bool flags this as such
        /// </summary>
        [DataMember(Name = "parentIsSameType", EmitDefaultValue = false)]
        public bool? ParentIsSameType
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the workspace
        /// </summary>
        [DataMember(Name = "workspaceName", EmitDefaultValue = true)]
        public string WorkspaceName
        {
            get;
            set;
        }

        /// <summary>
        /// The number of requirements in the workspace
        /// </summary>
        [DataMember(Name = "requirementsAll", EmitDefaultValue = true)]
        public int RequirementsAll
        {
            get;
            set;
        }

        /// <summary>
        /// The completion percentage
        /// </summary>
        [DataMember(Name = "percentComplete", EmitDefaultValue = true)]
        public int PercentComplete
        {
            get;
            set;
        }

        /// <summary>
        /// The last update date of the workspace
        /// </summary>
        [DataMember(Name = "lastUpdatedDate", EmitDefaultValue = false)]
        public DateTime? LastUpdatedDate
        {
            get;
            set;
        }

        /// <summary>
        /// The start date of the workspace
        /// </summary>
        [DataMember(Name = "startDate", EmitDefaultValue = false)]
        public DateTime? StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// The end date of the workspace
        /// </summary>
        [DataMember(Name = "endDate", EmitDefaultValue = false)]
        public DateTime? EndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Used if the workspace has child workspaces
        /// </summary>
        [DataMember(Name = "children", EmitDefaultValue = false)]
        public List<WorkspaceItem> Children
        {
            get;
            set;
        }

        /// <summary>
        /// Used if the workspace has child data items (e.g. builds)
        /// </summary>
        [DataMember(Name = "dataItems", EmitDefaultValue = false)]
        public List<DataItem> DataItems
        {
            get;
            set;
        }
    }
}