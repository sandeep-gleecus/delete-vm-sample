using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays a list of shared filters for the current project
    /// </summary>
    public partial class SharedSearches : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.SharedSearches::";

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
                //Add event handlers
                this.grdSavedSearches.RowDataBound += new GridViewRowEventHandler(grdSavedSearches_RowDataBound);
                this.grdSavedSearches.RowCommand += new GridViewCommandEventHandler(grdSavedSearches_RowCommand);

                //Now load the content
                if (!IsPostBack)
                {
                    if (WebPartVisible)
                    {
                        LoadAndBindData();
                    }
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
        /// Adds the rewriter URLs for launching a saved search or subscribing as an RSS feed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdSavedSearches_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Make sure we have a data row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get the data row
                DataModel.SavedFilter savedFilter = (DataModel.SavedFilter)e.Row.DataItem;

                //Make sure that the user has view permissions for this artifact type, otherwise hide the row
                if (SpiraContext.Current.ProjectRoleId.HasValue)
                {
                    Project.AuthorizationState authorizationState = new ProjectManager().IsAuthorized(SpiraContext.Current.ProjectRoleId.Value, (Artifact.ArtifactTypeEnum)savedFilter.ArtifactTypeId, DataModel.Project.PermissionEnum.View);
                    if (authorizationState != DataModel.Project.AuthorizationState.Authorized)
                    {
                        e.Row.Visible = false;
                    }
                }

                //Hide the delete button if we didn't create this shared search
                //If we are project admin/owner, then allow the delete
                LinkButtonEx btnDelete = (LinkButtonEx)e.Row.FindControl("btnDelete");
                if (btnDelete != null)
                {
                    btnDelete.Visible = (savedFilter.UserId == UserId || UserIsProjectAdmin);
                }

                //Locate the hyperlinks
                HyperLinkEx lnkApplyFilter = (HyperLinkEx)e.Row.FindControl("lnkApplyFilter");
                if (lnkApplyFilter != null)
                {
                    lnkApplyFilter.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SavedSearch, savedFilter.ProjectId, savedFilter.SavedFilterId);
                }
                HyperLinkEx lnkRss = (HyperLinkEx)e.Row.FindControl("lnkRss");
                if (String.IsNullOrEmpty(UserRssToken))
                {
                    lnkRss.Visible = false;
                }
                else
                {
                    if (lnkRss != null)
                    {
                        lnkRss.NavigateUrl = String.Format("~/Feeds/{0}/{1}/SavedSearch/{2}.aspx?", UserId, UserRssToken, savedFilter.SavedFilterId);
                    }
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
        /// Loads and binds the data
        /// </summary>
        public void LoadAndBindData()
        {
            //Now get the list of shared filters that exist for the current project
            SavedFilterManager savedFilterManager = new SavedFilterManager();
            List<DataModel.SavedFilter> savedFilters = savedFilterManager.Retrieve(UserId, ProjectId, null, true);
            this.grdSavedSearches.DataSource = savedFilters;
            this.grdSavedSearches.DataBind();
        }

        /// <summary>
        /// Called when link-buttons in the saved searches grid are clicked
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void grdSavedSearches_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //First see which command was called
            if (e.CommandName == "DeleteSearch")
            {
                SavedFilterManager savedFilterManager = new SavedFilterManager();
                int savedFilterId = Int32.Parse((string)e.CommandArgument);
                //Delete the saved search
                savedFilterManager.Delete(savedFilterId);

                //Now refresh the list
                LoadAndBindData();
            }
        }
    }
}