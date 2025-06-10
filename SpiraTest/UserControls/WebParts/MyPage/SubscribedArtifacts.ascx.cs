using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    /// <summary>
    /// Displays a list of a user's subscribed artifacts
    /// </summary>
    public partial class SubscribedArtifacts : WebPartBase, IWebPartReloadable
    {
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.SubscribedArtifacts::";

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

				//Add event handlers
                this.grdSubscribedArtifacts.RowDataBound += new GridViewRowEventHandler(grdSubscribedArtifacts_RowDataBound);
				this.grdSubscribedArtifacts.RowCommand += new GridViewCommandEventHandler(grdSubscribedArtifacts_RowCommand);

                //Set the RSS Feed link if it's enabled
                if (!String.IsNullOrEmpty(UserRssToken))
                {
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/SubscribedArtifacts.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }

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
        void grdSubscribedArtifacts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Make sure we have a data row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get the data row
                NotificationUserSubscriptionView subscriptionRow = (NotificationUserSubscriptionView)e.Row.DataItem;

                //Locate the hyperlinks
                HyperLinkEx lnkViewArtifact = (HyperLinkEx)e.Row.FindControl("lnkViewArtifact");
                if (lnkViewArtifact != null)
                {
                    lnkViewArtifact.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)subscriptionRow.ArtifactTypeId, subscriptionRow.ProjectId, subscriptionRow.ArtifactId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
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
			//Get the current project filter (if any)
			int? filterProjectId = null;
			if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
			{
				filterProjectId = ProjectId;
			}

			//Now get the list of subscribed artifacts that belong to the current user
            NotificationManager notificationManager = new NotificationManager();
			List<NotificationUserSubscriptionView> subscriptions = notificationManager.RetrieveSubscriptionsForUser(UserId, filterProjectId);
            this.grdSubscribedArtifacts.DataSource = subscriptions;
			this.grdSubscribedArtifacts.DataBind();
		}

		/// <summary>
		/// Called when link-buttons in the saved searches grid are clicked
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		void grdSubscribedArtifacts_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			//First see which command was called
            if (e.CommandName == "Unsubscribe")
			{
				//Unsubscribe from the artifact

                //The command argument is the artifact type id and artifact id separated by a colon
                string args = (string)e.CommandArgument;
                string[] arg = args.Split(':');
                if (arg.Length == 2)
                {
                    int artifactTypeId = Int32.Parse(arg[0]);
                    int artifactId = Int32.Parse(arg[1]);

                    NotificationManager notification = new NotificationManager();
                    notification.RemoveUserSubscription(UserId, artifactTypeId, artifactId);

                    //Now refresh the list
                    LoadAndBindData();
                }
			}
		}
    }
}
