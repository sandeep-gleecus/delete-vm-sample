using System;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Contains a workflow transition.
    /// </summary>
    public class RemoteWorkflowIncidentFields
    {
		public string FieldCaption;
		public string FieldName;
		public int FieldID;

        /// <summary>
        /// What is the state of the field that this represents
        /// </summary>
        /// <remarks>
        /// Active = 1,
        /// Required = 2
        /// </remarks>
		public int FieldStatus;

        public static RemoteWorkflowIncidentFields ConvertFromWorkflowField(WorkflowField workflowField)
		{
			RemoteWorkflowIncidentFields retField = new RemoteWorkflowIncidentFields();
            retField.FieldCaption = workflowField.Field.Caption;
            retField.FieldID = workflowField.ArtifactFieldId;
            retField.FieldName = workflowField.Field.Name;
            retField.FieldStatus = workflowField.WorkflowFieldStateId;

			return retField;
		}
	}
}
