namespace Inflectra.SpiraTest.Web.UserControls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	using Inflectra.SpiraTest.Business;
	
	using Inflectra.SpiraTest.Common;
	using System.Collections.Generic;
	using System.Collections;
	using Inflectra.SpiraTest.Web.Classes;

	/// <summary>
	///		This user control displays the artifact history list used by the various artifact
	///		details pages. It is typically enclosed in a panel
	/// </summary>
	public partial class HistoryPanel : ArtifactUserControlBase, IArtifactUserControl
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.HistoryPanel::";

        #region Properties

        public string AdminViewUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/{0}/Administration/HistoryList/{1}/{2}.aspx");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>This sets up the user control upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Only load the data once
			if (!IsPostBack)
			{
				//Do Nothing since the LoadAndBindData will be called by the enclosing page
			}

			//See if user is project owner
            bool isProjectAdmin = UserIsAdmin;

			//Populate the user and project id in the grid control
			this.grdHistoryList.ProjectId = this.ProjectId;
			this.grdHistoryList.Authorized_ArtifactType = this.ArtifactTypeEnum;

			//Specify if we need to auto-load the data (used if tab is initially visible)
			this.grdHistoryList.AutoLoad = this.AutoLoad;

			//Specify the artifact as two standard filters
			Dictionary<string, object> standardFilters = new Dictionary<string, object>();
			standardFilters.Add("ArtifactId", this.ArtifactId);
			standardFilters.Add("ArtifactType", (int)this.ArtifactTypeEnum);
			standardFilters.Add("IsProjectAdmin", isProjectAdmin);
            if (this.ShowTestStepData)
            {
                standardFilters.Add("IncludeSteps", this.ShowTestStepData);
            }
			this.grdHistoryList.SetFilters(standardFilters);

			//See if we have any history items so that we know to display the 'has-data' flag
			HistoryManager history = new HistoryManager();
            int count = history.Count(ProjectId, this.ArtifactId, this.ArtifactTypeEnum, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
			this.HasData = (count > 0);

            //Custom CSS for test steps
            Dictionary<string, string> historyCssClasses = new Dictionary<string, string>();
            historyCssClasses.Add("ChangeDate", "priority2");
            historyCssClasses.Add("FieldName", "priority1");
            historyCssClasses.Add("OldValue", "priority2");
            historyCssClasses.Add("NewValue", "priority2");
            historyCssClasses.Add("ChangerId", "priority4");
            historyCssClasses.Add("ChangeSetTypeName", "priority3");
            this.grdHistoryList.SetCustomCssClasses(historyCssClasses);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		#region Other Methods

		/// <summary>
		/// This method populates the history datagrid and databinds the control
		/// </summary>
		/// <param name="dataBind">Whether to databind or not</param>
		public void LoadAndBindData(bool dataBind)
		{
		}

		#endregion

		/// <summary>Whether or not to include test step data with test cases.</summary>
		public bool ShowTestStepData
		{
			get
			{
				if (ViewState["ShowStepData"] == null)
				{
					return false;
				}
				else
				{
					return (bool)ViewState["ShowStepData"];
				}
			}
			set
			{
				ViewState["ShowStepData"] = value;
			}
		}
	}
}
