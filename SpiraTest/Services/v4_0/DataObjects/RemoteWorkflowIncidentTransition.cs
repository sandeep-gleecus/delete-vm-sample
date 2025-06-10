using System;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Contains a workflow transition.
    /// </summary>
    public class RemoteWorkflowIncidentTransition
    {
        /// <summary>
        /// Can this transition be executed by the detector of the incident
        /// </summary>
		public bool ExecuteByDetector;

        /// <summary>
        /// Can this transition be executed by the owner of the incident
        /// </summary>
		public bool ExecuteByOwner;

        /// <summary>
        /// What is the id of the input incident status
        /// </summary>
		public int IncidentStatusId_Input;
        
        /// <summary>
        /// What is the display name of the input incident status
        /// </summary>
        public string IncidentStatusName_Input;

        /// <summary>
        /// What is the id of the output incident status
        /// </summary>
        public int IncidentStatusId_Output;
        
        /// <summary>
        /// What is the display name of the output incident status
        /// </summary>
        public string IncidentStatusName_Output;

        /// <summary>
        /// What is the name of the transition
        /// </summary>
		public string Name;

        /// <summary>
        /// What workflow does this transition belong to
        /// </summary>
		public int WorkflowId;

        /// <summary>
        /// What is the id of this transition
        /// </summary>
		public int TransitionId;
	}
}
