using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Notification_ViewNotificationTemplates", "Template-Notifications/#notification-templates", "Admin_Notification_ViewNotificationTemplates")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin)]
	public partial class NotificationTemplates : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.ProjectTemplate.NotificationTemplates::";

		#region Event Handlers

		///<summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			((MasterPages.Administration)Master).PageTitle = Resources.Main.Admin_Notification_ViewEditNotificationEvent;

			//Reset the error message
			lblMessage.Text = "";

			lblProjectTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            //Set event handler
            grdArtifactTemplates.RowCommand += new System.Web.UI.WebControls.GridViewCommandEventHandler(grdArtifactTemplates_RowCommand);

			//Only load the data once
			if (!IsPostBack)
			{
				List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(true, false, false);

				grdArtifactTemplates.DataSource = artifactTypes;
				grdArtifactTemplates.DataBind();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Hit when the user wants to perform an action on a listed Template.</summary>
		void grdArtifactTemplates_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
		{
			//We need to get a project associated with the current template
			ProjectManager projectManager = new ProjectManager();
			List<DataModel.Project> projects = projectManager.RetrieveForTemplate(ProjectTemplateId);

			//They wanted to do something to .. something.
			switch (e.CommandName)
			{
				case "SendTestEmail":
					{
						//Try the first project that has an artifact of the type
						foreach (DataModel.Project project in projects)
						{
							int projectId = project.ProjectId;
							try
							{
								//Get the first item for the artifact type specified.
								int artID = -1;
								Artifact.ArtifactTypeEnum artType = Artifact.ArtifactTypeEnum.None;
								int artifactTypeId;
								if (int.TryParse((string)e.CommandArgument, out artifactTypeId))
								{
									artType = (Artifact.ArtifactTypeEnum)artifactTypeId;
									switch (artType)
									{
										case Artifact.ArtifactTypeEnum.Requirement:
											{
												// Grab a requirement.
												artType = Artifact.ArtifactTypeEnum.Requirement;
												List<RequirementView> reqData = new RequirementManager().Retrieve(UserManager.UserInternal, projectId, 1, Int32.MaxValue, null, 0);
												if (reqData.Count > 0)
													artID = reqData[0].RequirementId;
												break;
											}

										case Artifact.ArtifactTypeEnum.TestCase:
											{
												//Grab a test case.
												artType = Artifact.ArtifactTypeEnum.TestCase;
												List<TestCaseView> testCases = new TestCaseManager().Retrieve(projectId, "Name", true, 1, 1, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
												if (testCases.Count > 0)
													artID = testCases[0].TestCaseId;
												break;
											}

										case Artifact.ArtifactTypeEnum.Incident:
											{
												//Grab an incident.
												artType = Artifact.ArtifactTypeEnum.Incident;
												List<IncidentView> incidents = new IncidentManager().Retrieve(projectId, "", true, 1, 1, new System.Collections.Hashtable(), 0);
												if (incidents.Count > 0)
													artID = incidents[0].IncidentId;
												break;
											}

										case Artifact.ArtifactTypeEnum.Task:
											{
												//Grab a task.
												artType = Artifact.ArtifactTypeEnum.Task;
												List<TaskView> tasks = new TaskManager().Retrieve(projectId, "", true, 1, 1, new System.Collections.Hashtable(), 0);
												if (tasks.Count > 0)
													artID = tasks[0].TaskId;
												break;
											}

										case Artifact.ArtifactTypeEnum.TestSet:
											{
												//Grab a test set.
												artType = Artifact.ArtifactTypeEnum.TestSet;
												List<TestSetView> testSets = new TestSetManager().Retrieve(projectId, "Name", true, 1, 1, null, 0, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
												if (testSets.Count > 0)
													artID = testSets[0].TestSetId;
												break;
											}

										case Artifact.ArtifactTypeEnum.Document:
											{
												//Grab a document
												artType = Artifact.ArtifactTypeEnum.Document;
												List<ProjectAttachmentView> documents = new AttachmentManager().RetrieveForProject(projectId, null, "Filename", true, 1, 1, null, 0);
												if (documents.Count > 0)
													artID = documents[0].AttachmentId;
												break;
											}

										case Artifact.ArtifactTypeEnum.Risk:
											{
												//Grab a risk!
												artType = Artifact.ArtifactTypeEnum.Risk;
												List<RiskView> risks = new RiskManager().Risk_Retrieve(projectId, "RiskId", true, 1, 1, new System.Collections.Hashtable(), 0, false);
												if (risks.Count > 0)
													artID = risks[0].RiskId;
												break;
											}

										case Artifact.ArtifactTypeEnum.Release:
											{
												//Grab a release!
												artType = Artifact.ArtifactTypeEnum.Release;
												List<ReleaseView> releaseData = new ReleaseManager().RetrieveByProjectId(UserManager.UserInternal, projectId, 1, Int32.MaxValue, null, 0);
												if (releaseData.Count > 0)
													artID = releaseData[0].ReleaseId;
												break;
											}
									}
								}

								if (artType != Artifact.ArtifactTypeEnum.None && artID > 0)
								{
									//Send the e-mail to yourself
									new NotificationManager().SendNotification(projectId, artID, artType, UserId, UserId, "Template Test");
									lblMessage.Text = Resources.Messages.Admin_Notification_TestTemplateSent;
									lblMessage.Type = MessageBox.MessageType.Information;

									//End the project loop
									break;
								}
								else
								{
									lblMessage.Text = Resources.Messages.Admin_Notification_NoArtifactFound;
									lblMessage.Type = MessageBox.MessageType.Error;
								}
							}
							catch (ArtifactNotExistsException)
							{
								lblMessage.Text = Resources.Messages.Admin_Notification_NoArtifactFound;
								lblMessage.Type = MessageBox.MessageType.Error;
							}
							catch (Exception)
							{
								lblMessage.Text = Resources.Messages.Admin_Notification_ErrorSendingTestEmail;
								lblMessage.Type = MessageBox.MessageType.Error;
							}
						}

						break;
					}

				default:
					break;
			}
		}

		#endregion
	}
}
