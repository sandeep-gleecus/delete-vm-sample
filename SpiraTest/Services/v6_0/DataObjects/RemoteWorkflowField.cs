using System;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents an artifact field that can be controlled by the workflow
    /// </summary>
    public class RemoteWorkflowField
    {
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
