using System;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Contains a workflow transition.
    /// </summary>
    public class RemoteWorkflowIncidentTransition
    {
		public bool ExecuteByDetector;
		public bool ExecuteByOwner;
		public int IncidentStatusID_Input;
		public string IncidentStatusName_Input;
		public int IncidentStatusID_Output;
		public string IncidentStatusName_Output;
		public string Name;
		public int WorkflowID;
		public int TransitionID;
	}
}
