using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents an incident custom property that can be controlled by the workflow
    /// </summary>
    public class RemoteWorkflowIncidentCustomProperties
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteWorkflowIncidentCustomProperties()
        {
        }

        /// <summary>
        /// Constructor that creates an object based on its equivalent custom property entity
        /// </summary>
        /// <param name="workflowCustomProperty">The corresponding entity</param>
        internal RemoteWorkflowIncidentCustomProperties(WorkflowCustomProperty workflowCustomProperty)
        {
            this.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            this.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            this.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            this.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

        /// <summary>
        /// What is the caption for the custom property
        /// </summary>
        public string FieldCaption;

        /// <summary>
        /// What is the system name of the custom property
        /// </summary>
        /// <remarks>Uses the format Custom_01, Custom_02, Custom_03, etc.</remarks>
        public string FieldName;

        /// <summary>
        /// What is the id of the custom property
        /// </summary>
        public int CustomPropertyId;

        /// <summary>
        /// What is the state of the custom property that this represents
        /// </summary>
        /// <remarks>
        /// Active = 1,
        /// Required = 2
        /// </remarks>
        public int FieldStateId;
    }
}