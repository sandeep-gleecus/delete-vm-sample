using System;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Contains a workflow transition.
    /// </summary>
    public class RemoteWorkflowTransition
    {
        /// <summary>
        /// Can this transition be executed by the creator of the artifact
        /// </summary>
		public bool ExecuteByCreator;

        /// <summary>
        /// Can this transition be executed by the owner of the artifact
        /// </summary>
		public bool ExecuteByOwner;

        /// <summary>
        /// What is the id of the input artifact status
        /// </summary>
		public int StatusId_Input;
        
        /// <summary>
        /// What is the display name of the input artifact status
        /// </summary>
        public string StatusName_Input;

        /// <summary>
        /// What is the id of the output artifact status
        /// </summary>
        public int StatusId_Output;
        
        /// <summary>
        /// What is the display name of the output artifact status
        /// </summary>
        public string StatusName_Output;

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

        /// <summary>
        /// Does it require an electronic signature
        /// </summary>
        public bool RequireSignature;
	}
}
