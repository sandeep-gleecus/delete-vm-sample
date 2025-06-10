using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
	/// <summary>
	/// Special HTTP Post Handler API. Used to receive new incident POSTs from an external system (e.g. KronoDesk).
	/// Right now it only supports inserting new incidents
	/// </summary>
	/// <remarks>
	/// Used when data needs to be posted from another website directly from the browser (cross-domain posting). Cannot use JSONP
	/// because that only supports GET operations. Note that the user does not have to be authenticated with the Spira website
	/// to use this handler so we need to check the provide ApiKey to make sure it's valid
	/// </remarks>
	public class PostHandler : LocalizedHttpHandler
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v4_0.PostHandler::";

		//Constants used to access FORM POST data..
		private const string FORM_USERNAME = "UserName";
		private const string FORM_APIKEY = "ApiKey";
		private const string FORM_SESSION = "SessionId";
		private const string FORM_RETURN = "ReturnUrl";
		private const string FORM_OPERATION = "Operation";
		private const string FORM_PROJECT = "ddlProject";
		private const string FORM_INCNAME = "txtIncidentName";
		private const string FORM_INCDESC = "txtIncidentDesc";
		private const string FORM_INCTYPE = "ddlIncidentType";
		private const string FORM_INCPRI = "ddlPriority";
		private const string FORM_INCOWNER = "ddlOwner";
		private const string FORM_INCREL = "ddlRelease";
		private const string FORM_TKTURL = "tktURL";
		private const string FORM_TKTDESC = "tktDesc";

		/// <summary>Processes the inbound request from KronoDesk (or other application)</summary>
		/// <param name="context"></param>
		public override void ProcessRequest(HttpContext context)
		{
			const string METHOD_NAME = "ProcessRequest()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the required hidden variables
				string userName = context.Request.Form[FORM_USERNAME];
				string apiKey = context.Request.Form[FORM_APIKEY];
				string sessionId = context.Request.Form[FORM_SESSION];
				string returnUrl = context.Request.Form[FORM_RETURN];
				string operation = context.Request.Form[FORM_OPERATION];

				try
				{
					string errorMessage = "";
					string successInfo = "";
					//Validate the user based in username and RSS token combination. Locks the user if retries exceeded
					//which prevents a brute force attack
					SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
					bool authenticated = spiraProvider.ValidateUserByRssToken(userName, apiKey);
					if (!authenticated)
					{
						errorMessage = String.Format(Resources.Messages.Services_UnableToConnectWithApiKey, Common.ConfigurationSettings.Default.License_ProductType);
					}
					if (String.IsNullOrEmpty(errorMessage) && authenticated)
					{
						User user = spiraProvider.GetProviderUser(userName);
						int userId = user.UserId;

						//See which operation we have
						if (operation == "Incident_Add")
						{
							//Lets get the various incident fields
							string incidentProject = context.Request.Form[FORM_PROJECT];
							string incidentName = context.Request.Form[FORM_INCNAME];
							string incidentDesc = context.Request.Form[FORM_INCDESC];
							string incidentType = context.Request.Form[FORM_INCTYPE];
							string incidentPriority = context.Request.Form[FORM_INCPRI];
							string incidentOwner = context.Request.Form[FORM_INCOWNER];
							string incidentRelease = context.Request.Form[FORM_INCREL];
							string ticketURL = context.Request.Form[FORM_TKTURL]; //Added in Krono vX.X.X.X
							string ticketDesc = context.Request.Form[FORM_TKTDESC]; //Added in Krono vX.X.X.X

							int? incidentTypeId = null;
							if (incidentType.IsInteger())
							{
								incidentTypeId = Int32.Parse(incidentType);
							}
							int? incidentPriorityId = null;
							if (incidentPriority.IsInteger())
							{
								incidentPriorityId = Int32.Parse(incidentPriority);
							}
							int? detectedReleaseId = null;
							if (incidentRelease.IsInteger())
							{
								detectedReleaseId = Int32.Parse(incidentRelease);
							}
							int? ownerId = null;
							if (incidentOwner.IsInteger())
							{
								ownerId = Int32.Parse(incidentOwner);
							}

							//Custom properties

							//Get the project id as an integer
							int projectId;
							if (Int32.TryParse(incidentProject, out projectId))
							{
								//Make sure we're authorized
								Business.ProjectManager projectManager = new Business.ProjectManager();
								List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId);
								if (!projects.Any(p => p.ProjectId == projectId))
								{
									//Throw back an exception
									Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
									Logger.Flush();

									errorMessage = String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId);
								}

								//Make sure we have permissions to create incidents
								if (!IsAuthorized(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.Create))
								{
									//Throw back an exception
									Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
									Logger.Flush();

									errorMessage = Resources.Messages.Services_NotAuthorizedCreateIncidents;
								}

                                //Get the template associated with the project
                                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                                //First insert the new incident record itself, capturing and populating the id
                                IncidentManager incidentManager = new IncidentManager();
								int incidentId = incidentManager.Insert(
								   projectId,
								   incidentPriorityId,
								   null,
								   userId,
								   ownerId,
								   null,
								   incidentName,
								   incidentDesc,
								   detectedReleaseId,
								   null,
								   null,
								   incidentTypeId,
								   null,
								   DateTime.UtcNow,
								   null,
								   null,
								   null,
								   null,
								   null,
								   null,
								   null,
								   userId
								   );

								//Now we need to populate any custom properties
								CustomPropertyManager customPropertyManager = new CustomPropertyManager();
								ArtifactCustomProperty artifactCustomProperty = null;
								UpdateCustomPropertyData(ref artifactCustomProperty, context.Request.Form, projectId, projectTemplateId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId);
								customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

								//Add a URL attachment if we were given the ticket URL.
								if (!string.IsNullOrWhiteSpace(ticketURL))
								{
									//Try to parse a URL..
									Uri ticket = null;
									if (Uri.TryCreate(ticketURL, UriKind.Absolute, out ticket))
									{
										//We have a valid URL. Insert it as an attachment to the incident.
										new AttachmentManager().Insert(
											projectId,
											ticket.ToString(),
											(string.IsNullOrWhiteSpace(ticketDesc) ? "" : ticketDesc),
											userId,
											incidentId,
											Artifact.ArtifactTypeEnum.Incident,
											"",
											"",
											null,
                                            null,
											null);
									}
								}

								//Send a notification
								incidentManager.SendCreationNotification(incidentId, artifactCustomProperty, null);

								successInfo = projectId + "-" + incidentId;
							}
						}
					}

					//Redirect back to the originating URL
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					if (String.IsNullOrEmpty(errorMessage))
					{
						context.Response.Redirect(returnUrl + "&sessionId=" + sessionId + "&success=" + successInfo, false);
					}
					else
					{
						context.Response.Redirect(returnUrl + "&sessionId=" + sessionId + "&message=" + errorMessage, false);
					}
				}
				catch (Exception exception)
				{
					//Redirect back to the originating URL with the message
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
					Logger.Flush();
					context.Response.Redirect(returnUrl + "&sessionId=" + sessionId + "&message=" + exception.Message, false);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				//End the response since we have no return URL
				context.Response.End();
			}
		}

		/// <summary>Checks to see if the current user is authorized to perform a specific action on an artifact type</summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactType">The artifact type</param>
		/// <param name="permission">The operation being performed</param>
		/// <returns>True if authorized</returns>
		protected bool IsAuthorized(int userId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, Project.PermissionEnum permission)
		{
			bool isAuthorized = false;
			//See if we're authorized for this project
			//We only check to see that they have at least one role as the specific methods
			//need to check that the user can perform that specific function
			//we put the project role in session
			ProjectManager projectManager = new ProjectManager();
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, userId);
            if (projectUser != null)
			{
				//If we're the project owner, then we're authorized
                if (projectUser.IsAdmin)
				{
					isAuthorized = true;
				}
				else
				{
					//Now get the list of permissions for this role
                    int projectRoleId = projectUser.ProjectRoleId;
					ProjectRole projectRole = projectManager.RetrieveRolePermissions(projectRoleId);
                    if (projectRole.RolePermissions.Any(p => p.ProjectRoleId == projectRoleId && p.ArtifactTypeId == (int)artifactType && p.PermissionId == (int)permission))
                    {
						isAuthorized = true;
					}
				}
			}

			return isAuthorized;
		}

		/// <summary>Populates the custom property dataset from the API data object</summary>
		/// <param name="remoteArtifact">The API data object</param>
		/// <param name="artifactCustomProperty">The custom property entity</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">The type of artifact</param>
        /// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The current project</param>
		protected void UpdateCustomPropertyData(ref ArtifactCustomProperty artifactCustomProperty, NameValueCollection values, int projectId, int projectTemplateId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
		{
			//Create an entity if we need to
			if (artifactCustomProperty == null)
			{
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false, false);
				artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, artifactType, artifactId, customProperties);
			}
			else
			{
				artifactCustomProperty.StartTracking();
			}

			//We need to have definitions loaded
			if (artifactCustomProperty.CustomPropertyDefinitions == null)
			{
				throw new ApplicationException(Resources.Messages.Services_CustomPropertyDefinitionsNotLoaded);
			}

			//Loop through all the active custom properties
			foreach (CustomProperty customProperty in artifactCustomProperty.CustomPropertyDefinitions)
			{
				//The client needs to use the custom property field name (e.g. Custom_01)
				string fieldName = customProperty.CustomPropertyFieldName;
				if (String.IsNullOrEmpty(values[fieldName]))
				{
					//Set the property to NULL
					artifactCustomProperty.SetCustomProperty(customProperty.PropertyNumber, null);
				}
				else
				{
					//We set the property to the serialized value, checking the serialization and type match
					//The calling client needs to use the same serialization method
					//as SpiraTeam, currently we don't expose this through the API
					//So it needs to be in the documentation explicitly
					string clientSerializedValue = values[fieldName];
					object newValue = null;
					switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
					{
						case CustomProperty.CustomPropertyTypeEnum.Boolean:
							newValue = clientSerializedValue.FromDatabaseSerialization_Boolean();
							break;

						case CustomProperty.CustomPropertyTypeEnum.Date:
							newValue = clientSerializedValue.FromDatabaseSerialization_DateTime();
							break;

						case CustomProperty.CustomPropertyTypeEnum.Decimal:
							newValue = clientSerializedValue.FromDatabaseSerialization_Decimal();
							break;

						case CustomProperty.CustomPropertyTypeEnum.Integer:
							newValue = clientSerializedValue.FromDatabaseSerialization_Int32();
							break;

						case CustomProperty.CustomPropertyTypeEnum.List:
							newValue = clientSerializedValue.FromDatabaseSerialization_Int32();
							break;

						case CustomProperty.CustomPropertyTypeEnum.MultiList:
							newValue = clientSerializedValue.FromDatabaseSerialization_List_Int32();
							break;

						case CustomProperty.CustomPropertyTypeEnum.Text:
							newValue = clientSerializedValue.FromDatabaseSerialization_String();
							break;

						case CustomProperty.CustomPropertyTypeEnum.User:
							newValue = clientSerializedValue.FromDatabaseSerialization_Int32();
							break;

					}
					artifactCustomProperty.SetCustomProperty(customProperty.PropertyNumber, newValue);
				}
			}
		}

		public override bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}