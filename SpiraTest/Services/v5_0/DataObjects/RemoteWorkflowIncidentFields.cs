using System;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents an incident field that can be controlled by the workflow
    /// </summary>
    public class RemoteWorkflowIncidentFields
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteWorkflowIncidentFields()
        {
        }

        /// <summary>
        /// Constructor that creates data object based on corresponding entity
        /// </summary>
        /// <param name="workflowField">The corresponding workflow field entity</param>
        internal RemoteWorkflowIncidentFields(WorkflowField workflowField)
        {
            this.FieldCaption = workflowField.Field.Caption;
            this.FieldId = workflowField.ArtifactFieldId;
            this.FieldName = workflowField.Field.Name;
            this.FieldStateId = workflowField.WorkflowFieldStateId;
        }

        /// <summary>
        /// What is the caption for the field
        /// </summary>
		public string FieldCaption;

        /// <summary>
        /// What is the system name of the field
        /// </summary>
		public string FieldName;

        /// <summary>
        /// What is the id of the field
        /// </summary>
		public int FieldId;

        /// <summary>
        /// What is the state of the field that this represents
        /// </summary>
        /// <remarks>
        /// Inactive = 1,
	    /// Required = 2,
        /// Hidden = 3
        /// </remarks>
		public int FieldStateId;
	}
}
