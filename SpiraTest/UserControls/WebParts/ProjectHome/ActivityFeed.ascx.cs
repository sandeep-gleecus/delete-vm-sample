using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Data;
using System.Collections;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays the most recent activity in the project
    /// </summary>
    public partial class ActivityFeed : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ActivityFeed::";

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
                this.rowsToDisplay = value;
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
		public void LoadAndBindData()
        {
            //First check admin case
            Hashtable filters = new Hashtable();
            if (!SpiraContext.Current.IsProjectAdmin)
            {
                //Get the list of artifact types that the current user is allowed to view
                Business.ProjectManager projectManager = new Business.ProjectManager();
                ProjectRole rolePermissions = projectManager.RetrieveRolePermissions(this.ProjectRoleId);

                //If limited view user don't show anything
                if (rolePermissions == null || rolePermissions.IsLimitedView)
                {
                    return;
                }

                //Apply as a filter
                MultiValueFilter mvf = new MultiValueFilter();
                foreach (ProjectRolePermission dataRow in rolePermissions.RolePermissions)
                {
                    if (dataRow.PermissionId == (int)Project.PermissionEnum.View)
                    {
                        mvf.Values.Add(dataRow.ArtifactTypeId);
                    }
                }
                filters.Add("ArtifactTypeId", mvf);
            }

			//Get the list of history items for the current project
			Business.HistoryManager historyManager = new HistoryManager();
            List<HistoryChangeSetView> historyChangeSets = historyManager.RetrieveSetsByProjectId(ProjectId, GlobalFunctions.GetCurrentTimezoneUtcOffset(), "ChangeDate", false, filters, 1, this.rowsToDisplay);
            this.rptActivity.DataSource = historyChangeSets;
            this.rptActivity.DataBind();
        }

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
                //Add the event handlers
                this.rptActivity.ItemDataBound += new RepeaterItemEventHandler(rptActivity_ItemDataBound);

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
        /// Adds URLs and other dynamic data to the repeater items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rptActivity_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                HistoryChangeSetView historyChangeSet = (HistoryChangeSetView)e.Item.DataItem;

                HyperLinkEx lnkArtifact = (HyperLinkEx)e.Item.FindControl("lnkArtifact");
                if (lnkArtifact != null)
                {
                    lnkArtifact.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)historyChangeSet.ArtifactTypeId, ProjectId, historyChangeSet.ArtifactId);
                }

                //If we have SpiraTest, hide the user link since the resource page is not available
                HyperLinkEx lnkUser = (HyperLinkEx)e.Item.FindControl("lnkUser");
                Literal ltrUser = (Literal)e.Item.FindControl("ltrUser");
                if (Common.License.LicenseProductName == Common.LicenseProductNameEnum.SpiraTest)
                {
                    if (lnkUser != null)
                    {
                        lnkUser.Visible = false;
                    }
                    if (ltrUser != null)
                    {
                        ltrUser.Visible = true;
                    }
                }
                else
                {
                    if (lnkUser != null)
                    {
                        lnkUser.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, ProjectId, historyChangeSet.UserId);
                    }
                }
            }
        }
    }
}
