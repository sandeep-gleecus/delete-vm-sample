using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Notification_ViewEditNotification", "Template-Notifications/#notification-events", "Admin_Notification_ViewEditNotification")]
	public partial class NotificationEvents : AdministrationBase
	{
		private const string CLASS_NAME = "Administration.ProjectTemplate.NotificationEvents::";

		#region Event Handlers

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (ProjectTemplateId < 1)
			{
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);
			}

			((MasterPageBase)Master).PageTitle = Resources.Main.Admin_Notification_ViewEditNotification;
			lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            //Add URLs to incident workflows and add new event
            lnkIncidentWorkflows.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflows");

			//Load events.
			lblMessage.Text = "";
			grdNotificationEvents.RowCommand += new GridViewCommandEventHandler(grdNotificationEvents_RowCommand);
			grdNotificationEvents.RowDataBound += GrdNotificationEvents_RowDataBound;
			LoadEventData();
			btnEventAdd.Click += new DropMenuEventHandler(btnEventAdd_Click);

			Logger.LogExitingEvent( METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Adds selective formatting</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GrdNotificationEvents_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				NotificationEvent notificationEvent = (NotificationEvent)e.Row.DataItem;
				if (notificationEvent.IsActive)
				{
					e.Row.Cells[3].CssClass = "priority1 bg-success";
				}
				else
				{
					e.Row.Cells[3].CssClass = "priority1 bg-warning";
				}
			}
		}

		/// <summary>Hit when the user wants to add a new template.</summary>
		void btnEventAdd_Click(object sender, EventArgs e)
		{
			//Redirect to the details page..
			Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationEventDetails"), true);
		}

		/// <summary>Hit when the user clicks a button on the grid.</summary>
		void grdNotificationEvents_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "DeleteRow":
					int evtNum = int.Parse((string)e.CommandArgument);
					new NotificationManager().DeleteEvent(evtNum);
					lblMessage.Text = Resources.Messages.Notification_EventDeleted;
					lblMessage.Type = MessageBox.MessageType.Information;
					LoadEventData();
					break;
			}
		}
		#endregion

		#region Data Functions
		/// <summary>Loads events & data onto the grid.</summary>
		private void LoadEventData()
		{
			List<NotificationEvent> notifyEvents = new NotificationManager().RetrieveEvents(ProjectTemplateId);
			grdNotificationEvents.DataSource = notifyEvents;
			grdNotificationEvents.DataBind();
		}
		#endregion
	}
}
