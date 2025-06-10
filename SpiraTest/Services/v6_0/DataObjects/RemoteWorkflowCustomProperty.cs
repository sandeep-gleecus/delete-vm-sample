using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents an artifact custom property that can be controlled by the workflow
    /// </summary>
    public class RemoteWorkflowCustomProperty
    {
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