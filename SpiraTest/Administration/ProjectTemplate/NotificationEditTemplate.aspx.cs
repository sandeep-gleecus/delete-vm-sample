using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Notification_ViewNotificationTemplates", "Template-Notifications/#notification-templates", "Admin_Notification_ViewNotificationTemplates")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin)]
	public partial class NotificationEditTemplate : Inflectra.SpiraTest.Web.Administration.AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationEditTemplate::";

		#region Event Handlers

		///<summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Set the return url
            this.lnkBackToNotificationList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationTemplates");

			//Grab data..
			if (Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID] == null)
            {
				Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationTemplates"), true);
            }
            int artifactTypeId;
            if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID], out artifactTypeId))
            {
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationTemplates"), true);
            }
            ArtifactType artifactType = ArtifactManager.ArtifactTypes.FirstOrDefault(a => a.ArtifactTypeId == artifactTypeId);
            if (artifactType == null)
				Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationTemplates"), true);

			//Set page display..
			this.artNum.Value = artifactTypeId.ToString();
			((MasterPageBase)this.Master).PageTitle = Resources.Main.Admin_Notification_ViewEditNotificationEvent;
			this.lblMessage.Text = "";
            this.localSubMessage.Text = string.Format(Resources.Messages.Admin_Notification_EditArtifactTemplate, ((artifactType.ArtifactTypeId == 3) ? "or workflow" : ""), artifactType.Name);
            this.litTokenHeader.Text = string.Format(Resources.Messages.Admin_Notification_EditTemplateTokens, artifactType.Name);
            this.lblArtifactTypeTitle.Text = artifactType.Name;

			//Set event binding..
			this.grdFields.ItemDataBound += new System.Web.UI.WebControls.DataListItemEventHandler(grdFields_ItemDataBound);
			this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
			this.btnCancel.Click+=new EventHandler(btnCancel_Click);

			//Load available fields for specified template.
            List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveAll(artifactType.ArtifactTypeId, true, true);

            //--Strip ID from the end of any that have the id.
            for (int i = 0; i < artifactFields.Count; i++)
			{
                if (artifactFields[i].Name.EndsWith("Id"))
                    artifactFields[i].Name = artifactFields[i].Name.Replace("Id", "");
			}
			//--Add standard fields.
            artifactFields.Add(new ArtifactField(0, 1, artifactType.ArtifactTypeId, "URL", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_URL));
            artifactFields.Add(new ArtifactField(0, 1, artifactType.ArtifactTypeId, "NotifyTo", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_NotifyTo));
            artifactFields.Add(new ArtifactField(0, 1, artifactType.ArtifactTypeId, "Product", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_Product));
            //artifactFields.Add(new ArtifactField(0, 1, artRow.ArtifactTypeId, "EventName", "", false, true, false, false, 0, false, false, true, "The name of the event that fired the notification.")); //Temporarily commented out.
            artifactFields.Add(new ArtifactField(0, 1, artifactType.ArtifactTypeId, "ProjectName", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_Project));
            artifactFields.Add(new ArtifactField(0, 1, artifactType.ArtifactTypeId, "ID#", "", false, true, false, false, 0, false, false, true, Resources.Main.Notification_ID));
			//Display it.
            this.grdFields.DataSource = artifactFields;
			this.grdFields.DataBind();

			//Only load the data once
			if (!IsPostBack)
			{
				//Load the template text..
                this.txtTemplate.Text = new NotificationManager().RetrieveTemplateTextById(ProjectTemplateId, artifactType.ArtifactTypeId);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		void btnUpdate_Click(object sender, EventArgs e)
		{
			//Save the changed template.
            int artifactTypeId = int.Parse(this.artNum.Value);
            NotificationManager notificationManager = new NotificationManager();
            NotificationArtifactTemplate template = notificationManager.RetrieveTemplateById(ProjectTemplateId, artifactTypeId);
            template.StartTracking();
            template.TemplateText = this.txtTemplate.Text;
            notificationManager.UpdateTemplate(template);
			this.lblMessage.Text = Resources.Messages.Admin_Notification_TemplateSaved;
			this.lblMessage.Type = MessageBox.MessageType.Information;
		}

		void grdFields_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
		{
			//Add the onClick function to the item.
			HtmlAnchor aTokenClick = (HtmlAnchor)e.Item.FindControl("aTokenClick");
			if (aTokenClick != null)
			{
				aTokenClick.Attributes.Add("onclick", "javascript:insert_token('${" + ((ArtifactField)e.Item.DataItem).Name + "}');");
			}

		}

		/// <summary>Hit when the user wants to cancel and go back to the template list.</summary>
		/// <param name="sender">ImageButtonEx</param>
		/// <param name="e">EventArgs</param>
		void btnCancel_Click(object sender, EventArgs e)
		{
			Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationTemplates"), true);
		}

		#endregion
	}
}
