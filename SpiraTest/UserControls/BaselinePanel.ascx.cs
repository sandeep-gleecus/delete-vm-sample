namespace Inflectra.SpiraTest.Web.UserControls
{
	using Inflectra.SpiraTest.Business;
	using Inflectra.SpiraTest.Common;
	using Inflectra.SpiraTest.DataModel;
	using System.Collections.Generic;

	/// <summary>
	/// This user control displays the Baseline list used (currently) only in the
	/// Release Details page. But may, in the future, be used elsewhere.
	/// </summary>
	public partial class BaselinePanel : ArtifactUserControlBase, IArtifactUserControl
	{
		private const string CLASS_NAME = "Web.UserControls.HistoryPanel::";

		#region Event Handlers
		/// <summary>This sets up the user control upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//See if user is project owner
			bool isProjectAdmin = UserIsAdmin;

			//Populate the user and project id in the grid control
			grdBaselineList.ProjectId = ProjectId;
			grdBaselineList.Authorized_ArtifactType = ArtifactTypeEnum;

			//Specify if we need to auto-load the data (used if tab is initially visible)
			grdBaselineList.AutoLoad = AutoLoad;

			//Specify the artifact as two standard filters
			Dictionary<string, object> standardFilters = new Dictionary<string, object>
			{
				{ "ArtifactId", ArtifactId },
				{ "ArtifactType", (int)ArtifactTypeEnum },
				{ "IsProjectAdmin", isProjectAdmin }
			};
			grdBaselineList.SetFilters(standardFilters);

			//See if we have any history items so that we know to display the 'has-data' flag
			int count = new BaselineManager().Baseline_Count(ProjectId, ArtifactId);
			HasData = (count > 0);

			//Set the URL..
			grdBaselineList.BaseUrl = "~/" + ProjectId + "/Administration/BaselineDetails/{art}.aspx";

			Logger.LogExitingEvent(METHOD_NAME);
		}
		#endregion

		/// <summary>Required for the Interface. Not used.</summary>
		public void LoadAndBindData(bool dataBind)
		{ }
	}
}
