using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    /// <summary>
    /// Displays a place where users can quickly launch needed tasks
    /// </summary>
    public partial class QuickLaunch : WebPartBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.QuickLaunch::";

        #region Properties

        /// <summary>
        /// Returns the base URL for creating a new incident
        /// </summary>
        protected string CreateIncidentBaseUrl
        {
            get
            {
                //Get a base url with the project id as a token (-2) and the artifact id set to create new items (-1)
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, -2, -1) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST_DETECTED + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

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
                //We have to set the message box programmatically for items that start out in the catalog
                this.MessageBoxId = "lblMessage";

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
		/// Loads and binds the data
		/// </summary>
        public void LoadAndBindData()
        {
            //Get the list of projects the current user is a member of
            ProjectManager projectManager = new ProjectManager();
            List<ProjectForUserView> projects = projectManager.RetrieveForUser(UserId);
            this.ddlChooseProject.DataSource = projects;
            this.ddlChooseProject.DataBind();

            //Register any client-side handlers
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "ddlChooseProject_loaded");
            this.ddlChooseProject.SetClientEventHandlers(handlers);

            //Get the current project and select that if available
            if (ProjectId > 0)
            {
                this.ddlChooseProject.SelectedValue = ProjectId.ToString();
            }
        }
    }
}