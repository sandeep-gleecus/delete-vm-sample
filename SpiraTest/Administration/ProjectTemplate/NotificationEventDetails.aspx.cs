using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.DataModel;
using System.Web.UI.HtmlControls;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Project Details Page and handling all raised events
	/// </summary>
	[
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Notification_ViewEditNotificationEvent", "Template-Notifications/#notification-events", "Admin_Notification_ViewEditNotificationEvent"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class NotificationEventDetails : Inflectra.SpiraTest.Web.Administration.AdministrationBase
	{
		#region Static values..
		private static int USER_OPENER = 1;
		private static int USER_OWNER = 2;
		#endregion
		protected SortedList<string, string> flagList;

        private const int DEFAULT_ARTIFACT_TYPE_ID = (int)Artifact.ArtifactTypeEnum.Incident;
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationEventDetails::";
		private NotificationEvent notifyEvent = null;

		#region Event Handlers

		///<summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we don't have a project template selected, redirect back to the admin home with a message
			if (ProjectTemplateId < 1)
			{
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);
			}

			((MasterPageBase)this.Master).PageTitle = Resources.Main.Admin_Notification_ViewEditNotificationEvent;

			//Reset the error message
			this.lblMessage.Text = "";

            //Set the return link
            this.lnkBackToNotificationList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationEvents");

            //Add the event handlers to the page
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.btnInsert.Click += new EventHandler(btnInsert_Click);
			this.ddlArtifactType.SelectedIndexChanged += new EventHandler(ddlArtifactType_SelectedIndexChanged);
			this.grdAvailFields.RowDataBound += new System.Web.UI.WebControls.GridViewRowEventHandler(grdAvailFields_RowDataBound);
			this.grdAvailRoles.RowDataBound += new System.Web.UI.WebControls.GridViewRowEventHandler(grdAvailRoles_RowDataBound);
            this.grdFields.ItemDataBound += new System.Web.UI.WebControls.DataListItemEventHandler(grdFields_ItemDataBound);

			//Only load the data once
			if (!IsPostBack)
			{
				int eventId = -1;
				this.notifyEvent = null;

				if (Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID] != null && Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID] != "")
				{
					eventId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID]);
					try
					{
						this.notifyEvent = new NotificationManager().RetrieveEventById(eventId);
					}
					catch (ArtifactNotExistsException)
					{
						this.lblMessage.Text = Resources.Messages.Admin_Notification_ErrorEventIdNotFound;
						eventId = -1;
					}
				}

                //Load available fields for specified template for use on modal popup.
                int artifactTypeId;
                if (eventId == -1 || notifyEvent == null)
                {
                    artifactTypeId = DEFAULT_ARTIFACT_TYPE_ID;
                }
                else
                {
                    artifactTypeId = notifyEvent.ArtifactTypeId;
                }
                ArtifactType artifactType = ArtifactManager.ArtifactTypes.FirstOrDefault(a => a.ArtifactTypeId == artifactTypeId);
                List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveAll(artifactTypeId, true, true);
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

                this.litTokenHeader.Text = string.Format(Resources.Messages.Admin_Notification_EditTemplateTokens, artifactType.Name);


				//Create Y/N lookups.
                this.flagList = new SortedList<string, string>() { { "Y", Resources.Main.Global_Yes }, { "N", Resources.Main.Global_No } };
				this.ddlArtifactType.DataSource = new ArtifactManager().ArtifactType_RetrieveAll(excludeNotifyDisabled: true);
				this.grdAvailRoles.DataSource = new Business.ProjectManager().RetrieveProjectRoles(true);

				this.DataBind();

				if (eventId == -1)
				{
					//Make the insert button visible
					this.ddlArtifactType.Enabled = true;
					this.btnInsert.Visible = true;
					this.btnUpdate.Visible = false;
					this.lblProjectName.Text = this.txtName.Text = Resources.Main.Admin_Notification_NewEvent;

					this.chkActiveYn.Checked = true;
					this.chkOnCreation.Checked = true;
                    this.ddlArtifactType.SelectedValue = DEFAULT_ARTIFACT_TYPE_ID.ToString(); //Incident artifact type id #.
					this.ddlArtifactType_SelectedIndexChanged(this.ddlArtifactType, new EventArgs());
				}
				else
				{
					//Make the insert button visible
					this.ddlArtifactType.Enabled = false;
					this.btnInsert.Visible = false;
					this.btnUpdate.Visible = true;

					//Populate the form with the existing values (update case)
					this.lblProjectName.Text = this.txtName.Text = GlobalFunctions.HtmlRenderAsPlainText(notifyEvent.Name);
					this.txtSubject.Text = notifyEvent.EmailSubject;
					this.chkActiveYn.Checked = notifyEvent.IsActive;
					this.chkOnCreation.Checked = notifyEvent.IsArtifactCreation;
					this.ddlArtifactType.SelectedValue = notifyEvent.ArtifactTypeId.ToString();
					this.ddlArtifactType_SelectedIndexChanged(this.ddlArtifactType, new EventArgs());
					// - See if the users exist.
					this.chkOpener.Checked = this.notifyEvent.NotificationArtifactUserTypes.Any(n => n.ProjectArtifactNotifyTypeId == USER_OPENER);
					this.chkOwner.Checked = this.notifyEvent.NotificationArtifactUserTypes.Any(n => n.ProjectArtifactNotifyTypeId == USER_OWNER);
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		void grdAvailRoles_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
		{
			if ((e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow) && (this.notifyEvent != null))
			{
				//See if the specified field is in the dataset.
				int projectRoleId = ((ProjectRole)e.Row.DataItem).ProjectRoleId;
                bool isMatch = this.notifyEvent.ProjectRoles.Any(p => p.ProjectRoleId == projectRoleId);
				CheckBoxEx chkSelected = (CheckBoxEx)e.Row.Cells[1].FindControl("chkNotifyRole");

                if (chkSelected != null)
                {
                    chkSelected.Checked = isMatch;
                }
			}
		}

		void grdAvailFields_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
		{
			if ((e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow) && (this.notifyEvent != null))
			{
				//See if the specified field is in the dataset.
				int artFieldId = ((ArtifactField)e.Row.DataItem).ArtifactFieldId;
                bool isMatch = this.notifyEvent.ArtifactFields.Any(f => f.ArtifactFieldId == artFieldId);

				CheckBoxEx chkSelected = (CheckBoxEx)e.Row.Cells[1].FindControl("chkSelected");
                if (chkSelected != null) chkSelected.Checked = isMatch;
			}
		}

		void ddlArtifactType_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Reload the datasets
            int artifactTypeId = Int32.Parse(this.ddlArtifactType.SelectedValue);

            ArtifactManager artifactManager = new ArtifactManager();
            ArtifactType artifactType = artifactManager.ArtifactType_RetrieveById(artifactTypeId);

            List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, true, true);
			this.grdAvailFields.DataSource = artifactFields;
			this.grdAvailFields.DataBind();

            this.grdFields.DataSource = artifactFields;
            this.grdFields.DataBind();

            this.litTokenHeader.Text = string.Format(Resources.Messages.Admin_Notification_EditTemplateTokens, artifactType.Name);

		}

        /// <summary>
        /// Inserts selected token from modal popup at cursor point of subject line
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        void grdFields_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
        {
            //The item is bound, get and populate the desc label.
            //Add the onClick function to the item.
            HtmlAnchor aTokenClick = (HtmlAnchor)e.Item.FindControl("aTokenClick");
            if (aTokenClick != null)
            {
                aTokenClick.Attributes.Add("onclick", "javascript:insert_token('${" + (string)((ArtifactField)e.Item.DataItem).Name + "}');");
            }
        }

		///<summary>Validates the form, and inserts the new notification event</summary>
		///<param name="sender">The sending object</param>
		///<param name="e">The event arguments</param>
		private void btnInsert_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnInsert_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
				return;

			this.SaveEventDetails();

			//Save it.
			new NotificationManager().SaveEvent(this.notifyEvent);

			//Go back to the list page.
			Response.Redirect("NotificationEvents.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		///<summary>Validates the form, and updates the project record</summary>
		///<param name="sender">The sending object</param>
		///<param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid) return;

			//If for some reason we don't have an existing dataset, we'll create one instead of updating on save
            NotificationManager notificationManager = new NotificationManager();
			if (Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID] != null && Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID] != "")
			{
				int eventId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_EVENT_ID]);
				try
				{
                    this.notifyEvent = notificationManager.RetrieveEventById(eventId);
				}
				catch (ArtifactNotExistsException)
				{
                    //Just leave as null
                    this.notifyEvent = null;
				}
			}

			this.SaveEventDetails();

			//Save it.
            notificationManager.SaveEvent(this.notifyEvent);

			//Go back to the list page.
			Response.Redirect("NotificationEvents.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		///<summary>Redirects the user back to the administration home page when cancel clicked</summary>
		///<param name="sender">The sending object</param>
		///<param name="e">The event arguments</param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Response.Redirect("NotificationEvents.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

        /// <summary>
        /// Saves the new or modified event
        /// </summary>
        private void SaveEventDetails()
		{
			//If it's empty, create a new row
            if (this.notifyEvent == null)
            {
                this.notifyEvent = new NotificationEvent();
                this.notifyEvent.ProjectTemplateId = ProjectTemplateId;
            }
            else
            {
                //Update existing fields.
                this.notifyEvent.StartTracking();
            }
            this.notifyEvent.Name = GlobalFunctions.HtmlScrubInput(this.txtName.Text);
            this.notifyEvent.ArtifactTypeId = int.Parse(this.ddlArtifactType.SelectedValue);
            this.notifyEvent.IsActive = this.chkActiveYn.Checked;
            this.notifyEvent.IsArtifactCreation = this.chkOnCreation.Checked;
            this.notifyEvent.EmailSubject = this.txtSubject.Text.Trim();

			//Update selected fields.
			for (int i = 0; i < this.grdAvailFields.Rows.Count; i++)
			{
				//Through each data row.
				if (this.grdAvailFields.Rows[i].RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
				{
					//Get the checkbox for the row.
					CheckBoxEx isSelected = (CheckBoxEx)this.grdAvailFields.Rows[i].Cells[1].FindControl("chkSelected");
					if (isSelected != null)
					{
                        int artifactFieldId = int.Parse(isSelected.MetaData);
                        ArtifactField artifactField = this.notifyEvent.ArtifactFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId);
						//If it's checked, add it. If it's not, remove it.
                        if (isSelected.Checked)
                        {
                            //It is. If we don't already have a row, add it.
                            if (artifactField == null)
                            {
                                artifactField = new ArtifactField();
                                artifactField.ArtifactFieldId = artifactFieldId;
                                this.notifyEvent.ArtifactFields.Add(artifactField);
                            }
                        }
                        else
                        {
                            if (artifactField != null)
                            {
                                this.notifyEvent.ArtifactFields.Remove(artifactField);
                            }
                        }
					}
				}
			}

			//Get is the opener is selected.
            NotificationArtifactUserType notificationArtifactUserType = this.notifyEvent.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == USER_OPENER);
            if (this.chkOpener.Checked)
			{
				//It is, add it if needed.
                notificationArtifactUserType = new NotificationArtifactUserType();
                notificationArtifactUserType.ProjectArtifactNotifyTypeId = USER_OPENER;
                this.notifyEvent.NotificationArtifactUserTypes.Add(notificationArtifactUserType);
			}
			else
			{
				//Delete it if needed.
                this.notifyEvent.NotificationArtifactUserTypes.Remove(notificationArtifactUserType);
			}

			//Get if the item Owner is selected.
            notificationArtifactUserType = this.notifyEvent.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == USER_OWNER);
            if (this.chkOwner.Checked)
            {
                //It is, add it if needed.
                notificationArtifactUserType = new NotificationArtifactUserType();
                notificationArtifactUserType.ProjectArtifactNotifyTypeId = USER_OWNER;
                this.notifyEvent.NotificationArtifactUserTypes.Add(notificationArtifactUserType);
            }
            else
            {
                //Delete it if needed.
                this.notifyEvent.NotificationArtifactUserTypes.Remove(notificationArtifactUserType);
            }

			//Now get Project Roles.
			for (int i = 0; i < this.grdAvailRoles.Rows.Count; i++)
			{
				if (this.grdAvailRoles.Rows[i].RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
				{
					CheckBoxEx isSelected = (CheckBoxEx)this.grdAvailRoles.Rows[i].Cells[1].FindControl("chkNotifyRole");
                    int projectRoleId = int.Parse(isSelected.MetaData);
                    ProjectRole projectRole = this.notifyEvent.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId);

					if (isSelected.Checked)
					{
						//It is, add it if necessary.
                        if (projectRole == null)
                        {
                            projectRole = new ProjectRole();
                            projectRole.ProjectRoleId = projectRoleId;
                            this.notifyEvent.ProjectRoles.Add(projectRole);
                        }
					}
					else
					{
						//Is isn't, delete it if necessary.
                        if (projectRole != null)
                            this.notifyEvent.ProjectRoles.Remove(projectRole);
					}
				}
			}
		}
	}
}
