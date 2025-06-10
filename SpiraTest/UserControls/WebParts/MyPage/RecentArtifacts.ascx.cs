using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    /// <summary>
    /// Displays a list of the most recently accessed artifacts
    /// </summary>
    public partial class RecentArtifacts : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.RecentArtifacts::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 5
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(5)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
				int rowsToDisplayMax = 50;
				this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
				//Force the data to reload
				LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 5;

        #endregion

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
                //Add any event handlers
                this.grdRecentArtifacts.RowDataBound += new GridViewRowEventHandler(grdRecentArtifacts_RowDataBound);

                //Now load the content
                if (!IsPostBack && WebPartVisible)
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
        /// Adds the rewriter URLs for viewing an artifact
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdRecentArtifacts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
			//Make sure we have a data row
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//Get the data row
				ArtifactInfoEx artifactInfoEx = (ArtifactInfoEx)e.Row.DataItem;

				//Locate the hyperlinks
				HyperLinkEx lnkViewArtifact = (HyperLinkEx)e.Row.FindControl("lnkViewArtifact");
				if (lnkViewArtifact != null)
				{
					lnkViewArtifact.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)artifactInfoEx.ArtifactTypeId, artifactInfoEx.ProjectId.Value, artifactInfoEx.ArtifactId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
				}
			}
		}

        /// <summary>
        /// Loads and binds the data
        /// </summary>
        public void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            try
            {
                //See if we're displaying for all projects or just current project
                int? filterProjectId = null;
                if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
                {
                    filterProjectId = ProjectId;
                }

				//Get the list of projects, the user has access to
				ProjectManager projectManager = new ProjectManager();
				List<ProjectForUserView> projects = projectManager.RetrieveForUser(UserId);

				//Get the list of recent artifacts for the user
				UserManager userManager = new UserManager();
				List<UserRecentArtifact> recentArtifacts = userManager.RetrieveRecentArtifactsForUser(UserId, filterProjectId, RowsToDisplay);

				//Loop through and get the artifact info
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactInfoEx> artifacts = new List<ArtifactInfoEx>();
				foreach (UserRecentArtifact recentArtifact in recentArtifacts)
				{
					ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)recentArtifact.ArtifactTypeId, recentArtifact.ArtifactId, recentArtifact.ProjectId);
					ProjectForUserView project = projects.FirstOrDefault(p => p.ProjectId == recentArtifact.ProjectId);
					if (artifactInfo != null && project != null && (!filterProjectId.HasValue || filterProjectId == recentArtifact.ProjectId))
					{
						ArtifactInfoEx artifactInfoEx = new ArtifactInfoEx();
						artifactInfoEx.ArtifactId = artifactInfo.ArtifactId;
						artifactInfoEx.Name = String.IsNullOrWhiteSpace(artifactInfo.Name) ? Resources.ClientScript.Global_None2 : artifactInfo.Name;
						artifactInfoEx.Description = artifactInfo.Description;
						artifactInfoEx.ArtifactToken = artifactInfo.ArtifactToken;
						artifactInfoEx.ArtifactTypeId = recentArtifact.ArtifactTypeId;
						artifactInfoEx.ProjectId = recentArtifact.ProjectId;
						artifactInfoEx.ProjectName = recentArtifact.Project.Name;
						artifactInfoEx.LastAccessed = GlobalFunctions.LocalizeDate(recentArtifact.LastAccessDate);
						artifacts.Add(artifactInfoEx);
					}
				}

                //Databind
                this.grdRecentArtifacts.DataSource = artifacts;
                this.grdRecentArtifacts.DataBind();
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
		/// Extends the main artifact info class to store the artifact type and date it was last accessed
		/// </summary>
		public class ArtifactInfoEx : ArtifactInfo
		{
			public string ProjectName { get; set; }
			public int ArtifactTypeId { get; set; }
			public DateTime LastAccessed { get; set; }
		}
	}
}
