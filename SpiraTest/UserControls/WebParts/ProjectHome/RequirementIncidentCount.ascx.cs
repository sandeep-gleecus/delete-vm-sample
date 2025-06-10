using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of requirements together with their open incident count
	/// </summary>
	public partial class RequirementIncidentCount : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementIncidentCount::";

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is unlimited
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
		]
		public Nullable<int> RowsToDisplay
		{
			get
			{
				return this.rowsToDisplay;
			}
			set
			{
				this.rowsToDisplay = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected Nullable<int> rowsToDisplay = null;

		/// <summary>
		/// Should we show all requirements with incidents or only those with open incidents
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("RequirementIncidentCount_OnlyIncludeOpenSetting"),
        LocalizedWebDescription("RequirementIncidentCount_OnlyIncludeOpenSettingTooltip"),
		DefaultValue(true)
		]
		public bool OnlyIncludeWithOpenIncidents
		{
			get
			{
				return this.onlyIncludeWithOpenIncidents;
			}
			set
			{
				this.onlyIncludeWithOpenIncidents = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected bool onlyIncludeWithOpenIncidents = true;

		#endregion

		/// <summary>
		/// Loads the control data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }
            }
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the release id from settings
			int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

			Business.RequirementManager requirement = new Business.RequirementManager();
            List<DataModel.RequirementIncidentCount> requirementIncidentCounts;
            int RowsToDisplayMax = 50;
            int RowsToDisplayActual = RowsToDisplayMax;// This is the max that we allow for performance reasons - it could probably be quite a bit more but this is on the safe side
            if (RowsToDisplay.HasValue && (int)RowsToDisplay < RowsToDisplayMax)
            {
                RowsToDisplayActual = (int)RowsToDisplay;
            }
            requirementIncidentCounts = requirement.RetrieveIncidentCount(ProjectId, ((releaseId == -1) ? null : (int?)releaseId), RowsToDisplayActual, OnlyIncludeWithOpenIncidents);

            //Set the navigate url for the name field
            ((NameDescriptionFieldEx)this.grdRequirementIncidentCount.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -3, GlobalFunctions.PARAMETER_TAB_ASSOCIATION);

            this.grdRequirementIncidentCount.DataSource = requirementIncidentCounts;
			this.grdRequirementIncidentCount.DataBind();
		}
	}
}