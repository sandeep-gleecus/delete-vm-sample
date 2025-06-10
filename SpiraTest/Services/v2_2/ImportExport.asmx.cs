using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web;
using Inflectra.SpiraTest.Web.Services.v2_2.DataObjects;
using Inflectra.SpiraTest.DataModel;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v2_2
{
	/// <summary>
	/// Enables the import and export of data to/from the system
	/// </summary>
	[
	WebService(Namespace = "http://www.inflectra.com/SpiraTest/Services/v2.2/", Description = "Enables the import and export of data to/from the system"),
	WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1),
	ToolboxItem(false)
	]
	public class ImportExport : WebServiceBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v2_2.ImportExport::";

		#region Project Methods

		/// <summary>
		/// Creates a new project in the system and makes the authenticated user owner of it
		/// </summary>
		/// <param name="remoteProject">The new project object (primary key will be empty)</param>
		/// <param name="existingProjectId">The id of an existing project to use as a template, or null to use the default template</param>
		/// <returns>The populated project object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new project in the system and makes the authenticated user owner of it",
			EnableSession = true
			)
		]
		public RemoteProject Project_Create(RemoteProject remoteProject, Nullable<int> existingProjectId)
		{
			const string METHOD_NAME = "Project_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a project id specified
			if (remoteProject.ProjectId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Project object needs to have ProjectId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we have permissions to create projects (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Projects - Need to be System Administrator",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			string description = "";
			string url = "";
			if (!String.IsNullOrEmpty(remoteProject.Description))
			{
				description = remoteProject.Description;
			}
			if (!String.IsNullOrEmpty(remoteProject.Website))
			{
				url = remoteProject.Website;
			}

			//Now create the project - either using default template or existing template
			ProjectManager projectManager = new ProjectManager();
			if (existingProjectId.HasValue)
			{
				int projectId = projectManager.CreateFromExisting(
					remoteProject.Name,
					description,
					url,
					existingProjectId.Value);

				//Now populate the project id onto the object
				remoteProject.ProjectId = projectId;
			}
			else
			{
				int projectId = projectManager.Insert(
					remoteProject.Name,
					null,
					description,
					url,
					remoteProject.Active,
					null,
					userId);

				//Now populate the project id onto the object
				remoteProject.ProjectId = projectId;
			}

			//Now connect to the project
			Connection_ConnectToProject(remoteProject.ProjectId.Value);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteProject;
		}

		/// <summary>
		/// Deletes an existing project in the system
		/// </summary>
		/// <param name="projectId">The project being deleted</param>
		[
		WebMethod
			(
			Description = "Deletes an existing project in the system",
			EnableSession = true
			)
		]
		public void Project_Delete(int projectId)
		{
			const string METHOD_NAME = "Project_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we have permissions to delete projects (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Delete Projects - Need to be System Administrator",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Now delete the project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			projectManager.Delete(userId, projectId);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Retrieves a list of projects that the authenticated user has access to
		/// </summary>
		/// <returns>The list of active projects</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of projects that the passed in user has access to",
			EnableSession = true
			)
		]
		public List<RemoteProject> Project_Retrieve()
		{
			const string METHOD_NAME = "Project_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				//Get the list of projects
				int userId = AuthenticatedUserId;
				ProjectManager projectManager = new ProjectManager();
				List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId);

				//Populate the API data object and return
				List<RemoteProject> remoteProjects = new List<RemoteProject>();
				foreach (ProjectForUserView project in projects)
				{
					//Create and populate the row
					RemoteProject remoteProject = new RemoteProject();
					PopulateProject(remoteProject, project.ConvertTo<ProjectForUserView, Project>());
					remoteProjects.Add(remoteProject);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteProjects;
			}
		}

		/// <summary>
		/// Retrieves a list of project roles in the system
		/// </summary>
		/// <returns>The list of project roles</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of project roles in the system",
			EnableSession = true
			)
		]
		public List<RemoteProjectRole> ProjectRole_Retrieve()
		{
			const string METHOD_NAME = "ProjectRole_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				//Get the list of project roles
				int userId = AuthenticatedUserId;
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(false, true);

				//Populate the API data object and return
				List<RemoteProjectRole> remoteProjectRoles = new List<RemoteProjectRole>();
				foreach (ProjectRole projectRoleRow in projectRoles)
				{
					//Create and populate the row
					RemoteProjectRole remoteProjectRole = new RemoteProjectRole();
					PopulateProjectRole(remoteProjectRole, projectRoleRow);
					remoteProjectRoles.Add(remoteProjectRole);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteProjectRoles;
			}
		}

		/// <summary>
		/// Retrieves a single project in the system
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The project object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single project in the system",
			EnableSession = true
			)
		]
		public RemoteProject Project_RetrieveById(int projectId)
		{
			const string METHOD_NAME = "Project_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				//Get the list of projects
				int userId = AuthenticatedUserId;
				Business.ProjectManager projectManager = new Business.ProjectManager();

				//If the project was not found, just return null
				try
				{
					Project project = projectManager.RetrieveById(projectId);

					//Populate the API data object and return
					RemoteProject remoteProject = new RemoteProject();
					PopulateProject(remoteProject, project);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return remoteProject;
				}
				catch (ArtifactNotExistsException)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested project");
					Logger.Flush();
					return null;
				}
			}
		}

		/// <summary>
		/// Populates a project API object from the internal datarow
		/// </summary>
		/// <param name="remoteProject">The API data object</param>
		/// <param name="project">The internal project entity</param>
		protected void PopulateProject(RemoteProject remoteProject, Project project)
		{
			remoteProject.ProjectId = project.ProjectId;
			remoteProject.Name = project.Name;
			remoteProject.Description = project.Description;
			remoteProject.Website = project.Website;
			remoteProject.CreationDate = GlobalFunctions.LocalizeDate(project.CreationDate);
			remoteProject.Active = project.IsActive;
			remoteProject.WorkingHours = project.WorkingHours;
			remoteProject.WorkingDays = project.WorkingDays;
			remoteProject.NonWorkingHours = project.NonWorkingHours;
		}

		/// <summary>
		/// Populates a project API object from the internal datarow
		/// </summary>
		/// <param name="remoteProjectRole">The API data object</param>
		/// <param name="projectRoleRow">The internal project role datarow</param>
		protected void PopulateProjectRole(RemoteProjectRole remoteProjectRole, ProjectRole projectRoleRow)
		{
			remoteProjectRole.ProjectRoleId = projectRoleRow.ProjectRoleId;
			remoteProjectRole.Name = projectRoleRow.Name;
			remoteProjectRole.Description = projectRoleRow.Description;
			remoteProjectRole.Active = (projectRoleRow.IsActive);
			remoteProjectRole.Admin = (projectRoleRow.IsAdmin);
			remoteProjectRole.DocumentsAdd = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Create && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
			remoteProjectRole.DocumentsDelete = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Modify && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
			remoteProjectRole.DocumentsEdit = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Delete && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
			remoteProjectRole.DiscussionsAdd = (projectRoleRow.IsDiscussionsAdd);
			remoteProjectRole.SourceCodeView = (projectRoleRow.IsSourceCodeView);
		}


		/// <summary>
		/// Adds a new custom list into the project
		/// </summary>
		/// <param name="remoteCustomList">The new custom list object</param>
		/// <returns>The custom list object with the primary key set</returns>
		[
		WebMethod
			(
			Description = "Adds a new custom list into the project",
			EnableSession = true
			)
		]
		public RemoteCustomList Project_AddCustomList(RemoteCustomList remoteCustomList)
		{
			const string METHOD_NAME = "Project_AddCustomList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomList.CustomPropertyListId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Custom List object needs to have CustomPropertyListId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create custom lists (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Custom Lists - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the new custom list
				remoteCustomList.CustomPropertyListId = customPropertyManager.CustomPropertyList_Add(projectTemplateId, remoteCustomList.Name, remoteCustomList.Active).CustomPropertyListId;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteCustomList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new custom property list value into the system
		/// </summary>
		/// <param name="remoteCustomListValue">The new custom list value object being added</param>
		/// <returns>custom list value object with its primary key set</returns>
		[
		WebMethod
			(
			Description = "Adds a new custom property list value into the system",
			EnableSession = true
			)
		]
		public RemoteCustomListValue Project_AddCustomListValue(RemoteCustomListValue remoteCustomListValue)
		{
			const string METHOD_NAME = "Project_AddCustomListValue";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomListValue.CustomPropertyValueId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Custom List Value object needs to have CustomPropertyValueId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create custom list values (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Custom List Values - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Now insert the new custom property list value
				remoteCustomListValue.CustomPropertyValueId = customPropertyManager.CustomPropertyList_AddValue(remoteCustomListValue.CustomPropertyListId, remoteCustomListValue.Name, remoteCustomListValue.Active).CustomPropertyValueId;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteCustomListValue;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the custom property configuration information associated with a particular project
		/// </summary>
		/// <param name="artifactTypeId">The type of the artifact</param>
		/// <returns>A list of project custom property objects</returns>
		[
		WebMethod
			(
			Description = "Retrieves the custom property configuration information associated with a particular project",
			EnableSession = true
			)
		]
		public List<RemoteProjectCustomProperty> Project_RetrieveCustomProperties(int artifactTypeId)
		{
			const string METHOD_NAME = "Project_RetrieveCustomProperties";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to retrieve custom properties (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now retrieve the project's custom properties, we only return the legacy text and list ones for this older API
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, false, false);

				//Convert into a list of project custom list data objects
				List<RemoteProjectCustomProperty> remoteProjectCustomProperties = new List<RemoteProjectCustomProperty>();
				foreach (CustomProperty customProperty in customProperties)
				{
					if ((customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List) && !String.IsNullOrEmpty(customProperty.LegacyName))
					{
						RemoteProjectCustomProperty remoteProjectCustomProperty = new RemoteProjectCustomProperty();
						remoteProjectCustomProperties.Add(remoteProjectCustomProperty);
						//Set the values on the data object
						remoteProjectCustomProperty.CustomPropertyId = customProperty.CustomPropertyId;
						remoteProjectCustomProperty.ProjectId = projectId;
						remoteProjectCustomProperty.ArtifactTypeId = customProperty.ArtifactTypeId;
						remoteProjectCustomProperty.Alias = customProperty.Name;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteProjectCustomProperties;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Saves the updated custom property configuration for a project
		/// </summary>
		/// <param name="remoteProjectCustomProperties">The list of custom properties to be persisted</param>
		/// <param name="artifactTypeId">The type of artifact the custom properties belong to</param>
		/// <remarks>This method performs the necessary inserts, updates and deletes</remarks>
		[
		WebMethod
			(
			Description = "Saves the updated custom property configuration for a project",
			EnableSession = true
			)
		]
		public void Project_SaveCustomProperties(int artifactTypeId, List<RemoteProjectCustomProperty> remoteProjectCustomProperties)
		{
			const string METHOD_NAME = "Project_SaveCustomProperties";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update custom properties (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Update Project Custom Properties - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Need to first get the existing list and then populate as appropriate
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, false, false);

				//Find the highest used custom property number
				int maxUsedCustomPropertyNumber = 0;
				foreach (CustomProperty customProperty in customProperties)
				{
					if (customProperty.PropertyNumber > maxUsedCustomPropertyNumber)
					{
						maxUsedCustomPropertyNumber = customProperty.PropertyNumber;
					}
				}

				//Now populate
				foreach (RemoteProjectCustomProperty remoteProjectCustomProperty in remoteProjectCustomProperties)
				{
					//See if we have a matching row in the list
					CustomProperty customProperty = customProperties.FirstOrDefault(cp => cp.CustomPropertyId == remoteProjectCustomProperty.CustomPropertyId && cp.ProjectTemplateId == projectTemplateId && cp.ArtifactTypeId == artifactTypeId);
					if (customProperty == null)
					{
						//We need to add a new row, the custom property type has to be deduced implicitly from the "old" custom property id value
						//1-10 = Text
						//11-20 = List
						CustomProperty.CustomPropertyTypeEnum customPropertyType = CustomProperty.CustomPropertyTypeEnum.Text;
						if (remoteProjectCustomProperty.CustomPropertyId > 10 && remoteProjectCustomProperty.CustomPropertyId <= 20)
						{
							customPropertyType = CustomProperty.CustomPropertyTypeEnum.List;
						}

						//Make sure we have an available number of custom properties for this artifact
						int propertyNumber = maxUsedCustomPropertyNumber + 1;
						if (propertyNumber <= CustomProperty.MAX_NUMBER_ARTIFACT_PROPERTIES)
						{
							customPropertyManager.CustomPropertyDefinition_AddToArtifact(
								projectTemplateId, 
								(Artifact.ArtifactTypeEnum)artifactTypeId, 
								(int)customPropertyType, propertyNumber, 
								remoteProjectCustomProperty.Alias,
								null,
								null,
								remoteProjectCustomProperty.CustomPropertyListId);
							maxUsedCustomPropertyNumber++;
						}
						else
						{
							throw new SoapException(Resources.Messages.Services_NoAvailableCustomProperties,
								SoapException.ClientFaultCode,
								Context.Request.Url.AbsoluteUri);
						}
					}
					else
					{
						//Update the existing row
						customProperty.StartTracking();
						customProperty.Name = remoteProjectCustomProperty.Alias;

						//If list property, set the list id
						if (remoteProjectCustomProperty.CustomPropertyListId.HasValue)
						{
							customProperty.CustomPropertyListId = remoteProjectCustomProperty.CustomPropertyListId.Value;
						}

						//Save any changes
						customPropertyManager.CustomPropertyDefinition_Update(customProperty);
					}
				}

				//Finally we need to see if any rows have been removed
				//Only the correct project is allowed
				foreach (CustomProperty customProperty in customProperties)
				{
					//See if we have an entry that's not in the API data object
					bool matchFound = false;
					foreach (RemoteProjectCustomProperty remoteProjectCustomProperty in remoteProjectCustomProperties)
					{
						if (remoteProjectCustomProperty.CustomPropertyId == customProperty.CustomPropertyId
							&& remoteProjectCustomProperty.ArtifactTypeId == customProperty.ArtifactTypeId
							&& remoteProjectCustomProperty.ProjectId == projectId)
						{
							matchFound = true;
						}
					}
					if (!matchFound)
					{
						//Delete the entry
						customPropertyManager.CustomPropertyDefinition_RemoveById(customProperty.CustomPropertyId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Document Methods

		/// <summary>
		/// Adds a new document (file) into the system and associates it with the provided artifact (optional)
		/// and project folder/type (optional)
		/// </summary>
		/// <param name="remoteDocument">The new document object (primary key will be empty)</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <returns>
		/// The populated document object - including the primary key and default values for project attachment type
		/// and project folder if they were not specified
		/// </returns>
		[
		WebMethod
			(
			Description = "Adds a new document (file) into the system and associates it with the provided artifact (optional) and project folder/type (optional)",
			EnableSession = true
			)
		]
		public RemoteDocument Document_AddFile(RemoteDocument remoteDocument, byte[] binaryData)
		{
			const string METHOD_NAME = "Document_AddFile";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a Attachment ID specified
			if (remoteDocument.AttachmentId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Document object needs to have AttachmentId set to null for adding",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotAuthorizedAddDocuments,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Default to the authenticated user if no author provided
				if (remoteDocument.AuthorId == -1 || remoteDocument.AuthorId == 0)
				{
					remoteDocument.AuthorId = userId;
				}

				//Handle any nullable values safely
				string description = null;
				string version = null;
				string tags = null;
				if (!String.IsNullOrEmpty(remoteDocument.Description))
				{
					description = remoteDocument.Description;
				}
				if (!String.IsNullOrEmpty(remoteDocument.CurrentVersion))
				{
					version = remoteDocument.CurrentVersion;
				}
				if (!String.IsNullOrEmpty(remoteDocument.Tags))
				{
					tags = remoteDocument.Tags;
				}

				DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
				if (remoteDocument.ArtifactTypeId.HasValue)
				{
					artifactType = (DataModel.Artifact.ArtifactTypeEnum)remoteDocument.ArtifactTypeId.Value;
				}

				//Now insert the attachment
				Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
				remoteDocument.AttachmentId = attachmentManager.Insert(
					projectId,
					remoteDocument.FilenameOrUrl,
					description,
					remoteDocument.AuthorId,
					binaryData,
					remoteDocument.ArtifactId,
					artifactType,
					version,
					tags,
					remoteDocument.ProjectAttachmentTypeId,
					remoteDocument.ProjectAttachmentFolderId,
					null
					);

				//Send a notification
				attachmentManager.SendCreationNotification(projectId, remoteDocument.AttachmentId.Value, null, null);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDocument;
		}

		/// <summary>
		/// Adds a new document (url) into the system and associates it with the provided artifact (optional)
		/// and project folder/type (optional)
		/// </summary>
		/// <param name="remoteDocument">The new document object (primary key will be empty). Populate the FilenameOrUrl field with the URL</param>
		/// <returns>
		/// The populated document object - including the primary key and default values for project attachment type
		/// and project folder if they were not specified
		/// </returns>
		[
		WebMethod
			(
			Description = "Adds a new document (url) into the system and associates it with the provided artifact (optional) and project folder/type (optional)",
			EnableSession = true
			)
		]
		public RemoteDocument Document_AddUrl(RemoteDocument remoteDocument)
		{
			const string METHOD_NAME = "Document_AddUrl";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a Attachment ID specified
			if (remoteDocument.AttachmentId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Document object needs to have AttachmentId set to null for adding",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotAuthorizedAddDocuments,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Default to the authenticated user if no author provided
				if (remoteDocument.AuthorId == -1 || remoteDocument.AuthorId == 0)
				{
					remoteDocument.AuthorId = userId;
				}

				//Handle any nullable values safely
				string description = null;
				string version = null;
				string tags = null;
				if (!String.IsNullOrEmpty(remoteDocument.Description))
				{
					description = remoteDocument.Description;
				}
				if (!String.IsNullOrEmpty(remoteDocument.CurrentVersion))
				{
					version = remoteDocument.CurrentVersion;
				}
				if (!String.IsNullOrEmpty(remoteDocument.Tags))
				{
					tags = remoteDocument.Tags;
				}

				DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
				if (remoteDocument.ArtifactTypeId.HasValue)
				{
					artifactType = (DataModel.Artifact.ArtifactTypeEnum)remoteDocument.ArtifactTypeId.Value;
				}

				//Now insert the attachment
				Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
				remoteDocument.AttachmentId = attachmentManager.Insert(
					projectId,
					remoteDocument.FilenameOrUrl,
					description,
					remoteDocument.AuthorId,
					remoteDocument.ArtifactId,
					artifactType,
					version,
					tags,
					remoteDocument.ProjectAttachmentTypeId,
					remoteDocument.ProjectAttachmentFolderId,
					null
					);

				//Send a notification
				attachmentManager.SendCreationNotification(projectId, remoteDocument.AttachmentId.Value, null, null);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDocument;
		}

		#endregion

		#region User Methods

		/// <summary>
		/// Creates a new user in the system and adds them to the current project as the specified role
		/// </summary>
		/// <param name="remoteUser">The new user object (primary key will be empty)</param>
		/// <param name="projectRoleId">The project role for the user</param>
		/// <returns>The populated user object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new user in the system and adds them to the current project as the specified role",
			EnableSession = true
			)
		]
		public RemoteUser User_Create(RemoteUser remoteUser, int projectRoleId)
		{
			const string METHOD_NAME = "User_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a user id specified
			if (remoteUser.UserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("User object needs to have UserId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int authenticatedUserId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create users (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Users - Need to be System Administrator",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have a populated first and last name so that we don't have the user created without a valid profile
			if (String.IsNullOrEmpty(remoteUser.FirstName) || String.IsNullOrEmpty(remoteUser.LastName))
			{
				throw new SoapException("DataValidationError: " + Resources.Messages.Services_FirstOrLastNameNotProvided, SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//If a user already exists with the ID, simply return that back instead
				Business.ProjectManager projectManager = new Business.ProjectManager();
				MembershipUser membershipUser = Membership.GetUser(remoteUser.UserName);
				if (membershipUser != null)
				{
					int existingUserId = (int)membershipUser.ProviderUserKey;
					remoteUser.UserId = existingUserId;

					//Now add the user to the current project as the specified role
					//Ignore any duplicate key errors
					try
					{
						projectManager.InsertUserMembership(existingUserId, projectId, projectRoleId);
					}
					catch (ProjectDuplicateMembershipRecordException)
					{
						//Ignore this error
					}
					catch (EntityConstraintViolationException)
					{
						//Ignore error due to duplicate row
					}

					return remoteUser;
				}

				//Next create the user, if it exists already ignore the exception
				string passwordQuestion = "What was the email address originally associated with the account?";
				string passwordAnswer = remoteUser.EmailAddress;
				MembershipCreateStatus status;
				SpiraMembershipProvider membershipProvider = (SpiraMembershipProvider)Membership.Provider;
				membershipUser = membershipProvider.CreateUser(remoteUser.UserName, remoteUser.Password, remoteUser.EmailAddress, passwordQuestion, passwordAnswer, remoteUser.Active, remoteUser.LdapDn, null, out status);
				if (status == MembershipCreateStatus.Success)
				{
					if (membershipUser == null)
					{
						string message = "Unable to create user - " + status.ToString();
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, message);
						Logger.Flush();
						throw new SoapException(message,
							SoapException.ClientFaultCode,
							Context.Request.Url.AbsoluteUri);
					}

					//Now we need to add the profile information
					ProfileEx profile = new ProfileEx(remoteUser.UserName);
					profile.FirstName = remoteUser.FirstName;
					profile.LastName = remoteUser.LastName;
					profile.MiddleInitial = remoteUser.MiddleInitial;
					profile.LastOpenedProjectId = projectId;
					profile.IsEmailEnabled = true;
					profile.IsAdmin = remoteUser.Admin;
					profile.Save();
				}
				else if (status == MembershipCreateStatus.DuplicateUserName)
				{
					//If we get this error we need to instead return the user record already in the system
					membershipUser = Membership.GetUser(remoteUser.UserName);
				}
				else
				{
					string message = "Unable to create user - " + status.ToString();
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, message);
					Logger.Flush();
					throw new SoapException(message,
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}

				//Now populate the user id onto the object
				int userId = (int)membershipUser.ProviderUserKey;
				remoteUser.UserId = userId;

				//Now add the user to the current project as the specified role
				//Ignore any duplicate key errors
				try
				{
					projectManager.InsertUserMembership(userId, projectId, projectRoleId);
				}
				catch (ProjectDuplicateMembershipRecordException)
				{
					//Ignore this error
				}
				catch (EntityConstraintViolationException)
				{
					//Ignore error due to duplicate row
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteUser;
		}

		/// <summary>
		/// Retrieves the list of users that are members of the current project
		/// </summary>
		/// <returns>List of ProjectUser objects</returns>
		[
		WebMethod
			(
			Description = "Retrieves the list of users that are members of the current project",
			EnableSession = true
			)
		]
		public List<RemoteProjectUser> Project_RetrieveUserMembership()
		{
			const string METHOD_NAME = "Project_RetrieveUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			try
			{
				//Retrieve the list of users that are members of the current project
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(projectId);

				//Populate the API data object and return
				List<RemoteProjectUser> remoteProjectUsers = new List<RemoteProjectUser>();
				foreach (ProjectUser projectUserRow in projectUsers)
				{
					RemoteProjectUser remoteProjectUser = new RemoteProjectUser();
					PopulateProjectUser(remoteProjectUser, projectUserRow);
					remoteProjectUsers.Add(remoteProjectUser);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteProjectUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves a single user in the system
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>The user object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single user in the system",
			EnableSession = true
			)
		]
		public RemoteUser User_RetrieveById(int userId)
		{
			const string METHOD_NAME = "User_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				//Retrieve the user dataset
				Business.UserManager userManager = new Business.UserManager();

				//If the user was not found, just return null
				try
				{
					User user = userManager.GetUserById(userId);

					//Populate the API data object and return
					RemoteUser remoteUser = new RemoteUser();
					PopulateUser(remoteUser, user);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return remoteUser;
				}
				catch (ArtifactNotExistsException)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested user");
					Logger.Flush();
					return null;
				}
			}
		}

		/// <summary>
		/// Retrieves a single user in the system by user-name
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>The user object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single user in the system",
			EnableSession = true
			)
		]
		public RemoteUser User_RetrieveByUserName(string userName)
		{
			const string METHOD_NAME = "User_RetrieveByUserName";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				//Retrieve the user dataset
				Business.UserManager userManager = new Business.UserManager();

				//If the user was not found, just return null
				try
				{
					User user = userManager.GetUserByLogin(userName);

					//Populate the API data object and return
					RemoteUser remoteUser = new RemoteUser();
					PopulateUser(remoteUser, user);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return remoteUser;
				}
				catch (ArtifactNotExistsException)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested user");
					Logger.Flush();
					return null;
				}
			}
		}

		/// <summary>
		/// Populates a user API object from the internal datarow
		/// </summary>
		/// <param name="remoteUser">The API data object</param>
		/// <param name="user">The internal user entity object</param>
		protected void PopulateUser(RemoteUser remoteUser, User user)
		{
			remoteUser.UserId = user.UserId;
			remoteUser.FirstName = user.Profile.FirstName;
			remoteUser.LastName = user.Profile.LastName;
			remoteUser.MiddleInitial = user.Profile.MiddleInitial;
			remoteUser.UserName = user.UserName;
			remoteUser.Password = "";   //Not returned for security reasons
			remoteUser.LdapDn = user.LdapDn;
			remoteUser.EmailAddress = user.EmailAddress;
			remoteUser.Active = (user.IsActive);
			remoteUser.Admin = (user.Profile.IsAdmin);
		}

		/// <summary>
		/// Populates a project user API object from the internal datarow
		/// </summary>
		/// <param name="remoteProjectUser">The API data object</param>
		/// <param name="projectUserRow">The internal datarow</param>
		protected void PopulateProjectUser(RemoteProjectUser remoteProjectUser, ProjectUser projectUserRow)
		{
			remoteProjectUser.UserId = projectUserRow.UserId;
			remoteProjectUser.ProjectId = projectUserRow.ProjectId;
			remoteProjectUser.ProjectRoleId = projectUserRow.ProjectRoleId;
			remoteProjectUser.ProjectRoleName = projectUserRow.ProjectRoleName;
		}

		#endregion

		#region Incident Methods

		/// <summary>
		/// Retrieves a list of incidents in the system that match the provided filter/sort
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of incidents</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of incidents in the system that match the provided filter/sort",
			EnableSession = true
			)
		]
		public List<RemoteIncident> Incident_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "Incident_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("You need to provide a populated RemoteSort object as parameter",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulateIncident(remoteIncident, incident);
				PopulateCustomProperties(remoteIncident, incident, customProperties);
				remoteIncidents.Add(remoteIncident);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves the 1000 most recent incidents added in the system since the date specified
		/// </summary>
		/// <param name="creationDate">The date after which the incident needs to have been created</param>
		/// <returns>List of incidents</returns>
		[
		WebMethod
			(
			Description = "Retrieves all new incidents added in the system since the date specified",
			EnableSession = true
			)
		]
		public List<RemoteIncident> Incident_RetrieveNew(DateTime creationDate)
		{
			const string METHOD_NAME = "Incident_RetrieveNew";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			Hashtable filters = new Hashtable();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = GlobalFunctions.UniversalizeDate(creationDate);
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, "CreationDate", false, 1, 1000, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulateIncident(remoteIncident, incident);
				PopulateCustomProperties(remoteIncident, incident, customProperties);
				remoteIncidents.Add(remoteIncident);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves all open incidents owned by the currently authenticated user
		/// </summary>
		/// <returns>List of incidents</returns>
		[
		WebMethod
			(
			Description = "Retrieves all open incidents owned by the currently authenticated user",
			EnableSession = true
			)
		]
		public List<RemoteIncident> Incident_RetrieveForOwner()
		{
			const string METHOD_NAME = "Incident_RetrieveForOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.RetrieveOpenByOwnerId(userId, null, null);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Incident);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulateIncident(remoteIncident, incident);
				PopulateCustomProperties(remoteIncident, incident, customProperties);
				remoteIncidents.Add(remoteIncident);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves the incident resolutions for an incident
		/// </summary>
		/// <param name="incidentId">The id of the incident</param>
		/// <returns>List of incident resolutions</returns>
		[
		WebMethod
			(
			Description = "Retrieves the incident resolutions for an incident",
			EnableSession = true
			)
		]
		public List<RemoteIncidentResolution> Incident_RetrieveResolutions(int incidentId)
		{
			const string METHOD_NAME = "Incident_RetrieveResolutions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the incident resolutions
			IncidentManager incidentManager = new IncidentManager();
			try
			{
				Incident incident = incidentManager.RetrieveById(incidentId, true);

				//Populate the API data object and return
				List<RemoteIncidentResolution> remoteIncidentResolutions = new List<RemoteIncidentResolution>();

				//Sort the resolutions by oldest first
				List<IncidentResolution> resolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();

				foreach (IncidentResolution resolution in resolutions)
				{
					//Create and populate the row
					RemoteIncidentResolution remoteIncidentResolution = new RemoteIncidentResolution();
					remoteIncidentResolution.IncidentResolutionId = resolution.IncidentResolutionId;
					remoteIncidentResolution.IncidentId = resolution.IncidentId;
					remoteIncidentResolution.CreatorId = resolution.CreatorId;
					remoteIncidentResolution.Resolution = resolution.Resolution;
					remoteIncidentResolution.CreationDate = GlobalFunctions.LocalizeDate(resolution.CreationDate);
					remoteIncidentResolution.CreatorName = resolution.Creator.FullName;
					remoteIncidentResolutions.Add(remoteIncidentResolution);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentResolutions;
			}
			catch (ArtifactNotExistsException)
			{
				//Can't locate the incident so return back no resolutions
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Adds new incident resolutions to incidents in the system
		/// </summary>
		/// <param name="remoteIncidentResolutions">List of new resolutions to add</param>
		/// <returns>The list of resolutions with the IncidentResolutionId populated</returns>
		[
		WebMethod
			(
			Description = "Adds new incident resolutions to incidents in the system",
			EnableSession = true
			)
		]
		public List<RemoteIncidentResolution> Incident_AddResolutions(List<RemoteIncidentResolution> remoteIncidentResolutions)
		{
			const string METHOD_NAME = "Incident_AddResolutions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to modify incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Iterate through the provided resolutions, inserting them as needed
			IncidentManager incidentManager = new IncidentManager();
			foreach (RemoteIncidentResolution remoteIncidentResolution in remoteIncidentResolutions)
			{
				remoteIncidentResolution.IncidentResolutionId = incidentManager.InsertResolution(
					remoteIncidentResolution.IncidentId,
					remoteIncidentResolution.Resolution,
					GlobalFunctions.UniversalizeDate(remoteIncidentResolution.CreationDate),
					remoteIncidentResolution.CreatorId,
					true
					);
			}

			//Finally return the populated incident resolution list
			return remoteIncidentResolutions;
		}

		/// <summary>
		/// Retrieves a single incident in the system
		/// </summary>
		/// <param name="incidentId">The id of the incident</param>
		/// <returns>Incident object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single incident in the system",
			EnableSession = true
			)
		]
		public RemoteIncident Incident_RetrieveById(int incidentId)
		{
			const string METHOD_NAME = "Incident_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the incident was not found, just return null
			try
			{
				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);
				IncidentView incident = incidentManager.RetrieveById2(incidentId);

				//Also get the first linked test run step
				int? testRunStepId = null;
				List<TestRunStepIncidentView> testRunStepIncidents = incidentManager.Incident_RetrieveTestRunSteps(incidentId);
				if (testRunStepIncidents != null && testRunStepIncidents.Count > 0)
				{
					testRunStepId = testRunStepIncidents.FirstOrDefault().TestRunStepId;
				}

				//Populate the API data object and return
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulateIncident(remoteIncident, incident, testRunStepId);
				PopulateCustomProperties(remoteIncident, incident, customProperties);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncident;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested incident");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates an incident in the system
		/// </summary>
		/// <param name="remoteIncident">The updated incident object</param>
		[
		WebMethod
			(
			Description = "Updates an incident in the system",
			EnableSession = true
			)
		]
		public void Incident_Update(RemoteIncident remoteIncident)
		{
			const string METHOD_NAME = "Incident_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an incident id specified
			if (!remoteIncident.IncidentId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Incident object needs to have IncidentId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				IncidentManager incidentManager = new IncidentManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				Incident incident = incidentManager.RetrieveById(remoteIncident.IncidentId.Value, false, false, true);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteIncident.IncidentId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, true);

				//Make sure that the project ids match
				if (incident.ProjectId != projectId)
				{
					throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateIncidentData(incident, remoteIncident);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value);

				//Get copies of everything..
				Artifact notificationArt = incident.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Call the business object to actually update the incident dataset and the custom properties
				incidentManager.Update(incident, userId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//If the test run step is not linked already, add
				if (remoteIncident.TestRunStepId.HasValue && !incident.TestRunSteps.Any(t => t.TestRunStepId == remoteIncident.TestRunStepId.Value))
				{
					incidentManager.Incident_AssociateToTestRunStep(projectId, remoteIncident.TestRunStepId.Value, new List<int>() { remoteIncident.IncidentId.Value }, userId);
				}

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident #" + incident.IncidentId + ".");
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested incident");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Creates a new incident in the system
		/// </summary>
		/// <param name="remoteIncident">The new incident object (primary key will be empty)</param>
		/// <returns>The populated incident object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new incident in the system",
			EnableSession = true
			)
		]
		public RemoteIncident Incident_Create(RemoteIncident remoteIncident)
		{
			const string METHOD_NAME = "Incident_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an incident id specified
			if (remoteIncident.IncidentId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Incident object needs to have IncidentId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Incidents",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no opener provided
			if (remoteIncident.OpenerId == -1 || remoteIncident.OpenerId == 0)
			{
				remoteIncident.OpenerId = userId;
			}
			//Default to the current project if not set
			if (remoteIncident.ProjectId == 0 || remoteIncident.ProjectId == -1)
			{
				remoteIncident.ProjectId = projectId;
			}

			//If we don't have a valid creation date, set it to the current date
			if (remoteIncident.CreationDate == DateTime.MinValue)
			{
				remoteIncident.CreationDate = DateTime.Now; //It will be UTCed later on
			}

			//Convert %complete into remaining effort value
			Nullable<int> remainingEffort = null;
			if (remoteIncident.EstimatedEffort.HasValue)
			{
				remainingEffort = remoteIncident.EstimatedEffort.Value * (100 - remoteIncident.CompletionPercent);
				remainingEffort /= 100;
			}

			//The API used -1 for default status and type, need to convert to null
			Nullable<int> incidentStatusId = null;
			Nullable<int> incidentTypeId = null;
			if (remoteIncident.IncidentStatusId != -1)
			{
				incidentStatusId = remoteIncident.IncidentStatusId;
			}
			if (remoteIncident.IncidentTypeId != -1)
			{
				incidentTypeId = remoteIncident.IncidentTypeId;
			}

			//First insert the new incident record itself, capturing and populating the id
			IncidentManager incidentManager = new IncidentManager();
			remoteIncident.IncidentId = incidentManager.Insert(
				projectId,
				remoteIncident.PriorityId,
				remoteIncident.SeverityId,
				remoteIncident.OpenerId,
				remoteIncident.OwnerId,
				(remoteIncident.TestRunStepId.HasValue) ? new List<int>() { remoteIncident.TestRunStepId.Value } : null,
				remoteIncident.Name,
				remoteIncident.Description,
				remoteIncident.DetectedReleaseId,
				remoteIncident.ResolvedReleaseId,
				remoteIncident.VerifiedReleaseId,
				incidentTypeId,
				incidentStatusId,
				GlobalFunctions.UniversalizeDate(remoteIncident.CreationDate),
				GlobalFunctions.UniversalizeDate(remoteIncident.StartDate),
				GlobalFunctions.UniversalizeDate(remoteIncident.ClosedDate),
				remoteIncident.EstimatedEffort,
				remoteIncident.ActualEffort,
				remainingEffort,
				null,
				null,
				userId
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			incidentManager.SendCreationNotification(remoteIncident.IncidentId.Value, artifactCustomProperty, null);

			//Finally return the populated incident object
			return remoteIncident;
		}

		/// <summary>
		/// Adds a new incident severity to the current project
		/// </summary>
		/// <param name="remoteIncidentSeverity">The new severity record</param>
		/// <returns>The severity object with the primary key populated</returns>
		/// <remarks>The color should be provide in RRGGBB hex format</remarks>
		[
		WebMethod
			(
			Description = "Adds a new incident severity to the current project",
			EnableSession = true
			)
		]
		public RemoteIncidentSeverity Incident_AddSeverity(RemoteIncidentSeverity remoteIncidentSeverity)
		{
			const string METHOD_NAME = "Incident_AddSeverity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident severities (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Incident Severities - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident severity
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentSeverity.SeverityId = incidentManager.InsertIncidentSeverity(projectTemplateId, remoteIncidentSeverity.Name, remoteIncidentSeverity.Color, remoteIncidentSeverity.Active);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidentSeverity;
		}

		/// <summary>
		/// Adds a new incident type to the current project
		/// </summary>
		/// <param name="remoteIncidentType">The new incident type object</param>
		/// <returns>The incident type object with its primary key populated</returns>
		[
		WebMethod
			(
			Description = "Adds a new incident type to the current project",
			EnableSession = true
			)
		]
		public RemoteIncidentType Incident_AddType(RemoteIncidentType remoteIncidentType)
		{
			const string METHOD_NAME = "Incident_AddType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident types (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Incident Types - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident type
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentType.IncidentTypeId = incidentManager.InsertIncidentType(projectTemplateId, remoteIncidentType.Name, null, remoteIncidentType.Issue, remoteIncidentType.Risk, false, remoteIncidentType.Active);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidentType;
		}

		/// <summary>
		/// Adds a new incident status to the current project
		/// </summary>
		/// <param name="remoteIncidentStatus">The incident status object</param>
		/// <returns>The incident status object with the primary key populated</returns>
		[
		WebMethod
			(
			Description = "Adds a new incident status to the current project",
			EnableSession = true
			)
		]
		public RemoteIncidentStatus Incident_AddStatus(RemoteIncidentStatus remoteIncidentStatus)
		{
			const string METHOD_NAME = "Incident_AddStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident statuses (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Incident Statuses - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident status
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentStatus.IncidentStatusId = incidentManager.IncidentStatus_Insert(projectTemplateId, remoteIncidentStatus.Name, remoteIncidentStatus.Open, false, remoteIncidentStatus.Active);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidentStatus;
		}

		/// <summary>
		/// Adds a new incident priority to the current project
		/// </summary>
		/// <param name="remoteIncidentPriority">The new priority record</param>
		/// <returns>The priority object with the primary key populated</returns>
		/// <remarks>The color should be provide in RRGGBB hex format</remarks>
		[
		WebMethod
			(
			Description = "Adds a new incident priority to the current project",
			EnableSession = true
			)
		]
		public RemoteIncidentPriority Incident_AddPriority(RemoteIncidentPriority remoteIncidentPriority)
		{
			const string METHOD_NAME = "Incident_AddPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident priorities (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Add Incident Priorities - need to be Project Owner",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident priority
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentPriority.PriorityId = incidentManager.InsertIncidentPriority(projectTemplateId, remoteIncidentPriority.Name, remoteIncidentPriority.Color, remoteIncidentPriority.Active);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidentPriority;
		}

		/// <summary>
		/// Retrieves a list of the active incident priorities for the current project
		/// </summary>
		/// <returns>The list of active incident priorities for the current project</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of the active incident priorities for the current project",
			EnableSession = true
			)
		]
		public List<RemoteIncidentPriority> Incident_RetrievePriorities()
		{
			const string METHOD_NAME = "Incident_RetrievePriorities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//All project members can see the list of priorities, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true);

				//Now populate the list of API data objects
				List<RemoteIncidentPriority> remoteIncidentPriorities = new List<RemoteIncidentPriority>();
				foreach (IncidentPriority priority in incidentPriorities)
				{
					RemoteIncidentPriority remoteIncidentPriority = new RemoteIncidentPriority();
					remoteIncidentPriorities.Add(remoteIncidentPriority);
					//Populate fields
					remoteIncidentPriority.PriorityId = priority.PriorityId;
					remoteIncidentPriority.Name = priority.Name;
					remoteIncidentPriority.Color = priority.Color;
					remoteIncidentPriority.Active = priority.IsActive;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentPriorities;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident severities for the current project
		/// </summary>
		/// <returns>The list of active incident severities for the current project</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of the active incident severities for the current project",
			EnableSession = true
			)
		]
		public List<RemoteIncidentSeverity> Incident_RetrieveSeverities()
		{
			const string METHOD_NAME = "Incident_RetrieveSeverities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//All project members can see the list of severities, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, true);

				//Now populate the list of API data objects
				List<RemoteIncidentSeverity> remoteIncidentSeverities = new List<RemoteIncidentSeverity>();
				foreach (IncidentSeverity severity in incidentSeverities)
				{
					RemoteIncidentSeverity remoteIncidentSeverity = new RemoteIncidentSeverity();
					remoteIncidentSeverities.Add(remoteIncidentSeverity);
					//Populate fields
					remoteIncidentSeverity.SeverityId = severity.SeverityId;
					remoteIncidentSeverity.Name = severity.Name;
					remoteIncidentSeverity.Color = severity.Color;
					remoteIncidentSeverity.Active = severity.IsActive;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentSeverities;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident types for the current project
		/// </summary>
		/// <returns>The list of active incident types for the current project</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of the active incident types for the current project",
			EnableSession = true
			)
		]
		public List<RemoteIncidentType> Incident_RetrieveTypes()
		{
			const string METHOD_NAME = "Incident_RetrieveTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//All project members can see the list of types, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(projectTemplateId, true);

				//Now populate the list of API data objects
				List<RemoteIncidentType> remoteIncidentTypes = new List<RemoteIncidentType>();
				foreach (IncidentType type in incidentTypes)
				{
					RemoteIncidentType remoteIncidentType = new RemoteIncidentType();
					remoteIncidentTypes.Add(remoteIncidentType);
					//Populate fields
					remoteIncidentType.IncidentTypeId = type.IncidentTypeId;
					remoteIncidentType.Name = type.Name;
					remoteIncidentType.Risk = type.IsRisk;
					remoteIncidentType.Issue = type.IsIssue;
					remoteIncidentType.Active = type.IsActive;
					remoteIncidentType.WorkflowID = type.WorkflowId;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentTypes;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident statuses for the current project
		/// </summary>
		/// <returns>The list of active incident statuses for the current project</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of the active incident statuses for the current project",
			EnableSession = true
			)
		]
		public List<RemoteIncidentStatus> Incident_RetrieveStatuses()
		{
			const string METHOD_NAME = "Incident_RetrieveStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//All project members can see the list of statuses, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);

				//Now populate the list of API data objects
				List<RemoteIncidentStatus> remoteIncidentStatuses = new List<RemoteIncidentStatus>();
				foreach (IncidentStatus status in incidentStati)
				{
					RemoteIncidentStatus remoteIncidentStatus = new RemoteIncidentStatus();
					remoteIncidentStatuses.Add(remoteIncidentStatus);
					//Populate fields
					remoteIncidentStatus.IncidentStatusId = status.IncidentStatusId;
					remoteIncidentStatus.Name = status.Name;
					remoteIncidentStatus.Open = status.IsOpenStatus;
					remoteIncidentStatus.Active = status.IsActive;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentStatuses;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteIncident">The API data object</param>
		/// <param name="incident">The internal entity</param>
		protected void PopulateIncident(RemoteIncident remoteIncident, IncidentView incident, int? testRunStepId = null)
		{
			remoteIncident.IncidentId = incident.IncidentId;
			remoteIncident.ProjectId = incident.ProjectId;
			remoteIncident.PriorityId = incident.PriorityId;
			remoteIncident.SeverityId = incident.SeverityId;
			remoteIncident.IncidentStatusId = incident.IncidentStatusId;
			remoteIncident.IncidentTypeId = incident.IncidentTypeId;
			remoteIncident.OpenerId = incident.OpenerId;
			remoteIncident.OwnerId = incident.OwnerId;
			remoteIncident.TestRunStepId = testRunStepId;
			remoteIncident.DetectedReleaseId = incident.DetectedReleaseId;
			remoteIncident.ResolvedReleaseId = incident.ResolvedReleaseId;
			remoteIncident.VerifiedReleaseId = incident.VerifiedReleaseId;
			remoteIncident.Name = incident.Name;
			remoteIncident.Description = incident.Description;
			remoteIncident.CreationDate = GlobalFunctions.LocalizeDate(incident.CreationDate);
			remoteIncident.StartDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)incident.StartDate);
			remoteIncident.ClosedDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)incident.ClosedDate);
			remoteIncident.CompletionPercent = incident.CompletionPercent;
			remoteIncident.EstimatedEffort = incident.EstimatedEffort;
			remoteIncident.ActualEffort = incident.ActualEffort;
			remoteIncident.LastUpdateDate = GlobalFunctions.LocalizeDate(incident.LastUpdateDate);
			remoteIncident.PriorityName = incident.PriorityName;
			remoteIncident.SeverityName = incident.SeverityName;
			remoteIncident.IncidentStatusName = incident.IncidentStatusName;
			remoteIncident.IncidentTypeName = incident.IncidentTypeName;
			remoteIncident.OpenerName = incident.OpenerName;
			remoteIncident.OwnerName = incident.OwnerName;
			remoteIncident.ProjectName = incident.ProjectName;
			remoteIncident.DetectedReleaseVersionNumber = incident.DetectedReleaseVersionNumber;
			remoteIncident.ResolvedReleaseVersionNumber = incident.ResolvedReleaseVersionNumber;
			remoteIncident.VerifiedReleaseVersionNumber = incident.VerifiedReleaseVersionNumber;
			remoteIncident.IncidentStatusOpenStatus = incident.IncidentStatusIsOpenStatus;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteIncident.LastUpdateDate = GlobalFunctions.LocalizeDate(incident.ConcurrencyDate);
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteIncident">The API data object</param>
		/// <param name="incident">The internal entity</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateIncidentData(Incident incident, RemoteIncident remoteIncident)
		{
			//We first need to update the concurrency information before we start tracking
			//The remote object uses LastUpdateDate for concurrency
			incident.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteIncident.LastUpdateDate);

			incident.StartTracking();
			incident.PriorityId = remoteIncident.PriorityId;
			incident.SeverityId = remoteIncident.SeverityId;
			incident.IncidentStatusId = remoteIncident.IncidentStatusId;
			incident.IncidentTypeId = remoteIncident.IncidentTypeId;
			incident.OpenerId = remoteIncident.OpenerId;
			incident.OwnerId = remoteIncident.OwnerId;
			incident.DetectedReleaseId = remoteIncident.DetectedReleaseId;
			incident.ResolvedReleaseId = remoteIncident.ResolvedReleaseId;
			incident.VerifiedReleaseId = remoteIncident.VerifiedReleaseId;
			incident.Name = remoteIncident.Name;
			incident.Description = remoteIncident.Description;
			incident.CreationDate = GlobalFunctions.UniversalizeDate(remoteIncident.CreationDate);
			incident.StartDate = GlobalFunctions.UniversalizeDate(remoteIncident.StartDate);
			incident.ClosedDate = GlobalFunctions.UniversalizeDate(remoteIncident.ClosedDate);
			incident.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteIncident.LastUpdateDate);
			incident.EstimatedEffort = remoteIncident.EstimatedEffort;
			incident.ActualEffort = remoteIncident.ActualEffort;

			//Convert %complete into remaining effort value
			if (remoteIncident.EstimatedEffort.HasValue)
			{
				int remainingEffort = remoteIncident.EstimatedEffort.Value * (100 - remoteIncident.CompletionPercent);
				remainingEffort /= 100;
				incident.RemainingEffort = remainingEffort;
			}
			else
			{
				incident.RemainingEffort = null;
			}
		}

		/// <summary>
		/// Will retrieve available transitions for the specied status ID for the currently logged-on user.
		/// </summary>
		/// <param name="currentTypeID">The current incident type</param>
		/// <param name="currentStatusID">The current incident status</param>
		/// <param name="isDetector">Is the user the detector of the incident</param>
		/// <param name="isOwner">Is the user the owner of the incident</param>
		/// <returns>The list of workflow transitions</returns>
		[WebMethod(
			Description = "Will retrieve available transitions for the specied status ID for the currently logged-on user.",
			EnableSession = true
		)]
		public List<RemoteWorkflowIncidentTransition> Incident_RetrieveWorkflowTransitions(int currentTypeID, int currentStatusID, bool isDetector, bool isOwner)
		{
			List<RemoteWorkflowIncidentTransition> retList = new List<RemoteWorkflowIncidentTransition>();

			//Get the use's role in the project.
			int roleId = this.ProjectRoleId;

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(AuthorizedProject).ProjectTemplateId;

			//Get the workflow ID for the specified status.
			int workflowId = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == currentTypeID)
				{
					workflowId = incidentTypes[i].WorkflowId;
				}
			}

			WorkflowManager workflowManager = new Business.WorkflowManager();
			List<WorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, currentStatusID, roleId, isDetector, isOwner);

			foreach (WorkflowTransition transition in workflowTransitions)
			{
				RemoteWorkflowIncidentTransition wrkTransition = new RemoteWorkflowIncidentTransition();
				wrkTransition.ExecuteByDetector = transition.IsExecuteByDetector;
				wrkTransition.ExecuteByOwner = transition.IsExecuteByOwner;
				wrkTransition.IncidentStatusID_Input = transition.InputIncidentStatusId;
				wrkTransition.IncidentStatusName_Input = transition.InputStatus.Name;
				wrkTransition.IncidentStatusID_Output = transition.OutputIncidentStatusId;
				wrkTransition.IncidentStatusName_Output = transition.OutputStatus.Name;
				wrkTransition.Name = transition.Name;
				wrkTransition.TransitionID = transition.WorkflowTransitionId;
				wrkTransition.WorkflowID = transition.WorkflowId;

				retList.Add(wrkTransition);
			}

			return retList;
		}

		/// <summary>
		/// Retrieves the list of incident fields and their workflow status for a given type and status/step.
		/// </summary>
		/// <param name="currentTypeID">The current incident type</param>
		/// <param name="currentStatusID">The current incident status</param>
		/// <returns>The list of incident fields</returns>
		[WebMethod(
			Description = "Retrieves the list of incident fields and their workflow status for a given type and status/step.",
			EnableSession = true
		)]
		public List<RemoteWorkflowIncidentFields> Incident_RetrieveWorkflowFields(int currentTypeID, int currentStatusID)
		{
			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(AuthorizedProject).ProjectTemplateId;

			List<RemoteWorkflowIncidentFields> retList = new List<RemoteWorkflowIncidentFields>();

			//Get the workflow ID for the specified status.
			int workflowID = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowID < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == currentTypeID)
				{
					workflowID = incidentTypes[i].WorkflowId;
				}
			}

			//We need to translate the field states to the data expected by v2.2 API clients
			//They expect to get 1=Active instead of 1=Inactive. They also do not know how to handle 3=Hidden
			//So we need to get all the fields first, remove the inactive and then only send back the active ones

			//Retrieve all fields
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Incident);

			//Retrieve field states
			List<WorkflowField> workflowFields = new WorkflowManager().Workflow_RetrieveFieldStates(workflowID, currentStatusID);

			//First we need to add the Required fields, since that hasn't changed
			foreach (WorkflowField workflowField in workflowFields)
			{
				if (workflowField.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required)
				{
					retList.Add(RemoteWorkflowIncidentFields.ConvertFromWorkflowField(workflowField));
				}
			}

			//Next we need to add the active fields
			foreach (ArtifactField artifactField in artifactFields)
			{
				//Make sure it's not inactive or hidden
				bool fieldIsInactiveOrHidden = workflowFields.Any(w => (w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive || w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) && w.ArtifactFieldId == artifactField.ArtifactFieldId);
				if (!fieldIsInactiveOrHidden)
				{
					//We have an active field to add
					RemoteWorkflowIncidentFields remoteWorkflowIncidentField = new RemoteWorkflowIncidentFields();
					remoteWorkflowIncidentField.FieldCaption = artifactField.Caption;
					remoteWorkflowIncidentField.FieldID = artifactField.ArtifactFieldId;
					remoteWorkflowIncidentField.FieldName = artifactField.Name;
					remoteWorkflowIncidentField.FieldStatus = 1;   //v2.2 API value for Active
					retList.Add(remoteWorkflowIncidentField);
				}
			}

			return retList;
		}
		#endregion

		#region Release Methods

		/// <summary>
		/// Retrieves all the releases and iterations belonging to the current project
		/// </summary>
		/// <param name="activeOnly">Do we want just active releases?</param>
		/// <returns>List of releases</returns>
		[
		WebMethod
			(
			Description = "Retrieves all the releases and iterations belonging to the current project",
			EnableSession = true
			)
		]
		public List<RemoteRelease> Release_Retrieve(bool activeOnly)
		{
			const string METHOD_NAME = "Release_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Releases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the release dataset
			ReleaseManager releaseManager = new ReleaseManager();
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, activeOnly);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

			//Populate the API data object and return
			List<RemoteRelease> remoteReleases = new List<RemoteRelease>();
			foreach (ReleaseView release in releases)
			{
				//Create and populate the row
				RemoteRelease remoteRelease = new RemoteRelease();
				PopulateRelease(remoteRelease, release);
				PopulateCustomProperties(remoteRelease, release, customProperties);
				remoteReleases.Add(remoteRelease);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteReleases;
		}

		/// <summary>
		/// Retrieves a list of releases in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of releases</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of releases in the system that match the provided filter",
			EnableSession = true
			)
		]
		public List<RemoteRelease> Release_Retrieve2(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "Release_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Releases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the release dataset
			ReleaseManager releaseManager = new ReleaseManager();
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(Business.UserManager.UserInternal, projectId, startingRow, numberOfRows, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

			//Populate the API data object and return
			List<RemoteRelease> remoteReleases = new List<RemoteRelease>();
			foreach (ReleaseView release in releases)
			{
				//Create and populate the row
				RemoteRelease remoteRelease = new RemoteRelease();
				PopulateRelease(remoteRelease, release);
				PopulateCustomProperties(remoteRelease, release, customProperties);
				remoteReleases.Add(remoteRelease);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteReleases;
		}

		/// <summary>
		/// Retrieves a single release in the system
		/// </summary>
		/// <param name="releaseId">The id of the release</param>
		/// <returns>Release object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single release in the system",
			EnableSession = true
			)
		]
		public RemoteRelease Release_RetrieveById(int releaseId)
		{
			const string METHOD_NAME = "Release_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Releases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the release dataset
			ReleaseManager releaseManager = new ReleaseManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the release was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, true);

				//Populate the API data object and return
				RemoteRelease remoteRelease = new RemoteRelease();
				PopulateRelease(remoteRelease, release);
				if (artifactCustomProperty != null)
				{
					PopulateCustomProperties(remoteRelease, artifactCustomProperty);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteRelease;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested release");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates a release in the system
		/// </summary>
		/// <param name="remoteRelease">The updated release object</param>
		[
		WebMethod
			(
			Description = "Updates a release in the system",
			EnableSession = true
			)
		]
		public void Release_Update(RemoteRelease remoteRelease)
		{
			const string METHOD_NAME = "Release_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an release id specified
			if (!remoteRelease.ReleaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Release object needs to have ReleaseId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Releases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				ReleaseManager releaseManager = new ReleaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				Release release = releaseManager.RetrieveById3(projectId, remoteRelease.ReleaseId.Value);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteRelease.ReleaseId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, true);

				//Make sure that the project ids match
				if (release.ProjectId != projectId)
				{
					throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}

				//Get copies of everything..
				Artifact notificationArt = release.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateReleaseData(release, remoteRelease);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value);

				//Call the business object to actually update the release dataset and the custom properties
				releaseManager.Update(new List<Release>() { release }, userId, projectId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for ReleaseId #" + release.ReleaseId + ".");
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested release");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Maps a release to a test case, so that the test case is needs to be tested for that release
		/// </summary>
		/// <param name="remoteReleaseTestCaseMapping">The release and test case mapping entry</param>
		/// <remarks>If the mapping record already exists no error is raised</remarks>
		[
		WebMethod
			(
			Description = "Maps a requirement to a test case, so that the test case 'covers' the requirement",
			EnableSession = true
			)
		]
		public void Release_AddTestMapping(RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping)
		{
			const string METHOD_NAME = "Release_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Add the test case we want to use for mapping
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReleaseTestCaseMapping.TestCaseId);
				testCaseManager.AddToRelease(projectId, remoteReleaseTestCaseMapping.ReleaseId, testCaseIds, userId);
			}
			catch (EntityConstraintViolationException)
			{
				//Ignore error due to duplicate row
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Removes a mapping entry for a specific release and test case
		/// </summary>
		/// <param name="remoteReleaseTestCaseMapping">The release and test case mapping entry</param>
		[
	   WebMethod
		   (
		   Description = "Removes a mapping entry for a specific release and test case",
		   EnableSession = true
		   )
	   ]
		public void Release_RemoveTestMapping(RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping)
		{
			const string METHOD_NAME = "Release_RemoveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Restore the test case we want to remove coverage for
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReleaseTestCaseMapping.TestCaseId);
				testCaseManager.RemoveFromRelease(projectId, remoteReleaseTestCaseMapping.ReleaseId, testCaseIds, userId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Creates a new release in the system</summary>
		/// <param name="remoteRelease">The new release object (primary key will be empty)</param>
		/// <param name="parentReleaseId">Do we want to insert the release under a parent release</param>
		/// <returns>The populated release object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new release in the system",
			EnableSession = true
			)
		]
		public RemoteRelease Release_Create(RemoteRelease remoteRelease, Nullable<int> parentReleaseId)
		{
			const string METHOD_NAME = "Release_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an release id specified
			if (remoteRelease.ReleaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Release object needs to have ReleaseId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Releases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the current project if not set
			if (remoteRelease.ProjectId == 0 || remoteRelease.ProjectId == -1)
			{
				remoteRelease.ProjectId = projectId;
			}
			//Set a default creator if one not specified
			if (remoteRelease.CreatorId == 0 || remoteRelease.CreatorId == -1)
			{
				remoteRelease.CreatorId = userId;
			}

			//If we have a passed in parent release, then we need to insert the release as a child item
			ReleaseManager releaseManager = new ReleaseManager();
			if (parentReleaseId.HasValue)
			{
				//We use user internal, since this web service shouldn't have to worry about what's collapsed
				ReleaseView release = releaseManager.RetrieveById2(projectId, parentReleaseId.Value);
				if (release == null)
				{
					throw new SoapException("Cannot find the supplied release id in the system",
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}
				List<ReleaseView> childReleases = releaseManager.RetrieveChildren(Business.UserManager.UserInternal, projectId, release.IndentLevel, false);

				//See if we have any existing child releases
				if (childReleases.Count > 0)
				{
					//Get the indent level of the last existing child
					string indentLevel = childReleases.Last().IndentLevel;

					//Now get the next indent level and use for that for the new item
					indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);

					//Now insert the release at the specified position
					remoteRelease.ReleaseId = releaseManager.Insert(
						userId,
						projectId,
						remoteRelease.CreatorId,
						remoteRelease.Name,
						remoteRelease.Description,
						remoteRelease.VersionNumber,
						indentLevel,
						(remoteRelease.Active) ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
						(remoteRelease.Iteration) ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
						GlobalFunctions.UniversalizeDate(remoteRelease.StartDate),
						GlobalFunctions.UniversalizeDate(remoteRelease.EndDate),
						remoteRelease.ResourceCount,
						remoteRelease.DaysNonWorking,
						null
						);
				}
				else
				{
					//We have no children so get the indent level of the parent and increment that
					//i.e. insert after the parent, then we can do an indent
					string indentLevel = HierarchicalList.IncrementIndentLevel(release.IndentLevel);

					//Now insert the release at the specified position
					remoteRelease.ReleaseId = releaseManager.Insert(
						userId,
						projectId,
						remoteRelease.CreatorId,
						remoteRelease.Name,
						remoteRelease.Description,
						remoteRelease.VersionNumber,
						indentLevel,
						(remoteRelease.Active) ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
						(remoteRelease.Iteration) ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
						GlobalFunctions.UniversalizeDate(remoteRelease.StartDate),
						GlobalFunctions.UniversalizeDate(remoteRelease.EndDate),
						remoteRelease.ResourceCount,
						remoteRelease.DaysNonWorking,
						null
						);

					//Finally perform an indent
					releaseManager.Indent(userId, projectId, remoteRelease.ReleaseId.Value);
				}
			}
			else
			{
				//Now insert the release at the end of the list
				remoteRelease.ReleaseId = releaseManager.Insert(
					userId,
					projectId,
					remoteRelease.CreatorId,
					remoteRelease.Name,
					remoteRelease.Description,
					remoteRelease.VersionNumber,
					(int?)null,
					(remoteRelease.Active) ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
					(remoteRelease.Iteration) ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
					GlobalFunctions.UniversalizeDate(remoteRelease.StartDate),
					GlobalFunctions.UniversalizeDate(remoteRelease.EndDate),
					remoteRelease.ResourceCount,
					remoteRelease.DaysNonWorking,
					null,
					true);
			}

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			releaseManager.SendCreationNotification(remoteRelease.ReleaseId.Value, artifactCustomProperty, null);

			//Finally return the populated release object
			return remoteRelease;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteRelease">The API data object</param>
		/// <param name="release">The internal datarow</param>
		protected void PopulateRelease(RemoteRelease remoteRelease, ReleaseView release)
		{
			remoteRelease.ReleaseId = release.ReleaseId;
			remoteRelease.ProjectId = release.ProjectId;
			remoteRelease.CreatorId = release.CreatorId;
			remoteRelease.Name = release.Name;
			remoteRelease.Description = release.Description;
			remoteRelease.VersionNumber = release.VersionNumber;
			remoteRelease.CreationDate = GlobalFunctions.LocalizeDate(release.CreationDate);
			remoteRelease.LastUpdateDate = GlobalFunctions.LocalizeDate(release.LastUpdateDate);
			remoteRelease.Summary = (release.IsSummary);
			remoteRelease.Active = (release.IsActive);
			remoteRelease.Iteration = (release.IsIteration);
			remoteRelease.StartDate = GlobalFunctions.LocalizeDate(release.StartDate);
			remoteRelease.EndDate = GlobalFunctions.LocalizeDate(release.EndDate);
			remoteRelease.ResourceCount = (int)release.ResourceCount;
			remoteRelease.DaysNonWorking = (int)release.DaysNonWorking;
			remoteRelease.PlannedEffort = release.PlannedEffort;
			remoteRelease.AvailableEffort = release.AvailableEffort;
			remoteRelease.TaskEstimatedEffort = release.TaskEstimatedEffort;
			remoteRelease.TaskActualEffort = release.TaskActualEffort;
			remoteRelease.TaskCount = release.TaskCount;
			remoteRelease.CreatorName = release.CreatorName;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteRelease.LastUpdateDate = GlobalFunctions.LocalizeDate(release.ConcurrencyDate);
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteRelease">The API data object</param>
		/// <param name="release">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateReleaseData(Release release, RemoteRelease remoteRelease)
		{
			release.CreatorId = remoteRelease.CreatorId;
			release.Name = remoteRelease.Name;
			release.Description = remoteRelease.Description;
			release.VersionNumber = remoteRelease.VersionNumber;
			release.CreationDate = GlobalFunctions.UniversalizeDate(remoteRelease.CreationDate);
			release.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteRelease.LastUpdateDate);
			release.IsSummary = remoteRelease.Summary;
			release.ReleaseStatusId = remoteRelease.Active ? (int)Release.ReleaseStatusEnum.InProgress : (int)Release.ReleaseStatusEnum.Completed;
			release.ReleaseTypeId = remoteRelease.Iteration ? (int)Release.ReleaseTypeEnum.Iteration : (int)Release.ReleaseTypeEnum.MajorRelease;
			release.StartDate = GlobalFunctions.UniversalizeDate(remoteRelease.StartDate);
			release.EndDate = GlobalFunctions.UniversalizeDate(remoteRelease.EndDate);
			release.ResourceCount = remoteRelease.ResourceCount;
			release.DaysNonWorking = remoteRelease.DaysNonWorking;
			release.PlannedEffort = remoteRelease.PlannedEffort;
			release.AvailableEffort = remoteRelease.AvailableEffort;
			release.TaskEstimatedEffort = remoteRelease.TaskEstimatedEffort;
			release.TaskActualEffort = remoteRelease.TaskActualEffort;
			release.TaskCount = remoteRelease.TaskCount;

			//Concurrency Management
			release.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteRelease.LastUpdateDate);
		}

		#endregion

		#region Requirement Methods

		/// <summary>
		/// Creates a new requirement record in the current project using the position offset method
		/// </summary>
		/// <param name="remoteRequirement">The new requirement object (primary key will be empty)</param>
		/// <param name="indentPosition">The number of columns to indent the requirement by (positive for indent, negative for outdent)</param>
		/// <returns>The populated requirement object - including the primary key</returns>
		/// <remarks>This version is use when you want to specify the relative indentation level</remarks>
		[
		WebMethod
			(
			Description = "Creates a new requirement record in the current project using the position offset method",
			EnableSession = true
			)
		]
		public RemoteRequirement Requirement_Create1(RemoteRequirement remoteRequirement, int indentPosition)
		{
			const string METHOD_NAME = "Requirement_Create1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a requirement id specified
			if (remoteRequirement.RequirementId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Requirement object needs to have RequirementId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Requirements",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no author provided
			if (remoteRequirement.AuthorId == 0 || remoteRequirement.AuthorId == -1)
			{
				remoteRequirement.AuthorId = userId;
			}
			//Default to the current project if not set
			if (remoteRequirement.ProjectId == 0 || remoteRequirement.ProjectId == -1)
			{
				remoteRequirement.ProjectId = projectId;
			}

			//If we have a passed in parent requirement, then we need to insert the requirement as a child item
			Business.RequirementManager requirementManager = new Business.RequirementManager();

			//Convert the old planned effort into points
			decimal? estimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteRequirement.ImportanceId >= 1 && remoteRequirement.ImportanceId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
			}

			//Now insert the requirement at the end of the list
			remoteRequirement.RequirementId = requirementManager.Insert(
				userId,
				projectId,
				remoteRequirement.ReleaseId,
				null,
				(int?)null,
				(Requirement.RequirementStatusEnum)remoteRequirement.StatusId,
				null,
				remoteRequirement.AuthorId,
				remoteRequirement.OwnerId,
				remoteRequirement.ImportanceId,
				remoteRequirement.Name,
				remoteRequirement.Description,
				estimatePoints,
				userId,
				false
				);

			//Now we need to indent it or outdent it the correct number of times
			if (indentPosition > 0)
			{
				for (int i = 0; i < indentPosition; i++)
				{
					requirementManager.Indent(userId, projectId, remoteRequirement.RequirementId.Value);
				}
			}
			if (indentPosition < 0)
			{
				for (int i = 0; i > indentPosition; i--)
				{
					requirementManager.Outdent(userId, projectId, remoteRequirement.RequirementId.Value);
				}
			}

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			requirementManager.SendCreationNotification(remoteRequirement.RequirementId.Value, artifactCustomProperty, null);

			//Finally return the populated requirement object
			return remoteRequirement;
		}

		/// <summary>
		/// Creates a new requirement in the system
		/// </summary>
		/// <param name="remoteRequirement">The new requirement object (primary key will be empty)</param>
		/// <param name="parentRequirementId">Do we want to insert the requirement under a parent requirement</param>
		/// <returns>The populated requirement object - including the primary key</returns>
		/// <remarks>This version is use when you want to specify the location by parent requirement</remarks>
		[
		WebMethod
			(
			Description = "Creates a new requirement in the system",
			EnableSession = true
			)
		]
		public RemoteRequirement Requirement_Create2(RemoteRequirement remoteRequirement, Nullable<int> parentRequirementId)
		{
			const string METHOD_NAME = "Requirement_Create2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a requirement id specified
			if (remoteRequirement.RequirementId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Requirement object needs to have RequirementId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Requirements",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no author provided
			if (remoteRequirement.AuthorId == 0 || remoteRequirement.AuthorId == -1)
			{
				remoteRequirement.AuthorId = userId;
			}
			//Default to the current project if not set
			if (remoteRequirement.ProjectId == 0 || remoteRequirement.ProjectId == -1)
			{
				remoteRequirement.ProjectId = projectId;
			}

			//Convert the old planned effort into points
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			decimal? estimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteRequirement.ImportanceId >= 1 && remoteRequirement.ImportanceId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
			}

			//If we have a passed in parent requirement, then we need to insert the requirement as a child item
			if (parentRequirementId.HasValue)
			{
				//Now insert the requirement under the current item.
				remoteRequirement.RequirementId = requirementManager.InsertChild(
					userId,
					projectId,
					remoteRequirement.ReleaseId,
					null,
					parentRequirementId.Value,
					(Requirement.RequirementStatusEnum)remoteRequirement.StatusId,
					null,
					remoteRequirement.AuthorId,
					remoteRequirement.OwnerId,
					remoteRequirement.ImportanceId,
					remoteRequirement.Name,
					remoteRequirement.Description,
					estimatePoints,
					userId
					);
			}
			else
			{
				//Now insert the requirement at the end of the list
				remoteRequirement.RequirementId = requirementManager.Insert(
					userId,
					projectId,
					remoteRequirement.ReleaseId,
					null,
					(int?)null,
					(Requirement.RequirementStatusEnum)remoteRequirement.StatusId,
					null,
					remoteRequirement.AuthorId,
					remoteRequirement.OwnerId,
					remoteRequirement.ImportanceId,
					remoteRequirement.Name,
					remoteRequirement.Description,
					estimatePoints,
					userId
					);
			}

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			requirementManager.SendCreationNotification(remoteRequirement.RequirementId.Value, artifactCustomProperty, null);

			//Finally return the populated requirement object
			return remoteRequirement;
		}

		/// <summary>
		/// Maps a requirement to a test case, so that the test case 'covers' the requirement
		/// </summary>
		/// <param name="remoteReqTestCaseMapping">The requirement and test case mapping entry</param>
		/// <remarks>If the coverage record already exists no error is raised</remarks>
		[
		WebMethod
			(
			Description = "Maps a requirement to a test case, so that the test case 'covers' the requirement",
			EnableSession = true
			)
		]
		public void Requirement_AddTestCoverage(RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			const string METHOD_NAME = "Requirement_AddTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Add the test case we want to use for coverage
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReqTestCaseMapping.TestCaseId);
				testCaseManager.AddToRequirement(projectId, remoteReqTestCaseMapping.RequirementId, testCaseIds, userId);
			}
			catch (EntityConstraintViolationException)
			{
				//Ignore error due to duplicate row
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Removes a coverage mapping entry for a specific requirement and test case
		/// </summary>
		/// <param name="remoteReqTestCaseMapping">The requirement and test case mapping entry</param>
		[
	   WebMethod
		   (
		   Description = "Removes a coverage mapping entry for a specific requirement and test case",
		   EnableSession = true
		   )
	   ]
		public void Requirement_RemoveTestCoverage(RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			const string METHOD_NAME = "Requirement_RemoveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Restore the test case we want to remove coverage for
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReqTestCaseMapping.TestCaseId);
				testCaseManager.RemoveFromRequirement(projectId, remoteReqTestCaseMapping.RequirementId, testCaseIds, userId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Retrieves all requirements owned by the currently authenticated user
		/// </summary>
		/// <returns>List of requirements</returns>
		[
		WebMethod
			(
			Description = "Retrieves all requirements owned by the currently authenticated user",
			EnableSession = true
			)
		]
		public List<RemoteRequirement> Requirement_RetrieveForOwner()
		{
			const string METHOD_NAME = "Requirement_RetrieveForOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the requirement dataset
			RequirementManager requirementManager = new RequirementManager();
			List<RequirementView> requirements = requirementManager.RetrieveByOwnerId(userId, null, null, false);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Requirement);

			//Populate the API data object and return
			List<RemoteRequirement> remoteRequirements = new List<RemoteRequirement>();
			foreach (RequirementView requirement in requirements)
			{
				//Create and populate the row
				RemoteRequirement remoteRequirement = new RemoteRequirement();
				PopulateRequirement(remoteRequirement, requirement);
				PopulateCustomProperties(remoteRequirement, requirement, customProperties);
				remoteRequirements.Add(remoteRequirement);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteRequirements;
		}

		/// <summary>
		/// Retrieves a single requirement in the system
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>Requirement object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single requirement in the system",
			EnableSession = true
			)
		]
		public RemoteRequirement Requirement_RetrieveById(int requirementId)
		{
			const string METHOD_NAME = "Requirement_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Requirements",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the requirement dataset
			RequirementManager requirementManager = new RequirementManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the requirement was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);

				//Populate the API data object and return
				RemoteRequirement remoteRequirement = new RemoteRequirement();
				PopulateRequirement(remoteRequirement, requirement);
				if (artifactCustomProperty != null)
				{
					PopulateCustomProperties(remoteRequirement, artifactCustomProperty);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteRequirement;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested requirement");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Retrieves a list of requirements in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of requirements</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of requirements in the system that match the provided filter",
			EnableSession = true
			)
		]
		public List<RemoteRequirement> Requirement_Retrieve(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "Requirement_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Requirements",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the requirement dataset
			RequirementManager requirementManager = new RequirementManager();
			List<RequirementView> requirements = requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, startingRow, numberOfRows, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);

			//Populate the API data object and return
			List<RemoteRequirement> remoteRequirements = new List<RemoteRequirement>();
			foreach (RequirementView requirement in requirements)
			{
				//Create and populate the row
				RemoteRequirement remoteRequirement = new RemoteRequirement();
				PopulateRequirement(remoteRequirement, requirement);
				PopulateCustomProperties(remoteRequirement, requirement, customProperties);
				remoteRequirements.Add(remoteRequirement);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteRequirements;
		}

		/// <summary>
		/// Updates a requirement in the system
		/// </summary>
		/// <param name="remoteRequirement">The updated requirement object</param>
		[
		WebMethod
			(
			Description = "Updates a requirement in the system",
			EnableSession = true
			)
		]
		public void Requirement_Update(RemoteRequirement remoteRequirement)
		{
			const string METHOD_NAME = "Requirement_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a requirement id specified
			if (!remoteRequirement.RequirementId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Requirement object needs to have RequirementId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Requirements",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				Requirement requirement = requirementManager.RetrieveById3(projectId, remoteRequirement.RequirementId.Value);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteRequirement.RequirementId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);

				//Make sure that the project ids match
				if (requirement.ProjectId != projectId)
				{
					throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateRequirementData(projectTemplateId, requirement, remoteRequirement);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);

				//Convert from estimated effort to story points
				requirement.EstimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

				//Get copies of everything for notification.
				Artifact notificationArt = requirement.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Call the business object to actually update the requirement dataset and the custom properties
				requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Requirement #" + requirement.RequirementId + ".");
				}

			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested requirement");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteRequirement">The API data object</param>
		/// <param name="requirement">The internal entity</param>
		protected void PopulateRequirement(RemoteRequirement remoteRequirement, RequirementView requirement)
		{
			remoteRequirement.RequirementId = requirement.RequirementId;
			remoteRequirement.StatusId = requirement.RequirementStatusId;
			remoteRequirement.ProjectId = requirement.ProjectId;
			remoteRequirement.AuthorId = requirement.AuthorId;
			remoteRequirement.OwnerId = requirement.OwnerId;
			remoteRequirement.ImportanceId = requirement.ImportanceId;
			remoteRequirement.ReleaseId = requirement.ReleaseId;
			remoteRequirement.Name = requirement.Name;
			remoteRequirement.Description = requirement.Description;
			remoteRequirement.CreationDate = GlobalFunctions.LocalizeDate(requirement.CreationDate);
			remoteRequirement.LastUpdateDate = GlobalFunctions.LocalizeDate(requirement.LastUpdateDate);
			remoteRequirement.Summary = requirement.IsSummary;
			remoteRequirement.CoverageCountTotal = requirement.CoverageCountTotal;
			remoteRequirement.CoverageCountPassed = requirement.CoverageCountPassed;
			remoteRequirement.CoverageCountFailed = requirement.CoverageCountFailed;
			remoteRequirement.CoverageCountCaution = requirement.CoverageCountCaution;
			remoteRequirement.CoverageCountBlocked = requirement.CoverageCountBlocked;
			remoteRequirement.PlannedEffort = requirement.EstimatedEffort;
			remoteRequirement.TaskEstimatedEffort = requirement.TaskEstimatedEffort;
			remoteRequirement.TaskActualEffort = requirement.TaskActualEffort;
			remoteRequirement.TaskCount = requirement.TaskCount;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteRequirement.LastUpdateDate = GlobalFunctions.LocalizeDate(requirement.ConcurrencyDate);
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template</param>
		/// <param name="remoteRequirement">The API data object</param>
		/// <param name="requirement">The internal entity</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateRequirementData(int projectTemplateId, Requirement requirement, RemoteRequirement remoteRequirement)
		{
			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteRequirement.ImportanceId >= 1 && remoteRequirement.ImportanceId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
			}

			//We first need to update the concurrency information before we start tracking
			//The remote object uses LastUpdateDate for concurrency
			requirement.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteRequirement.LastUpdateDate);

			requirement.StartTracking();
			requirement.RequirementStatusId = remoteRequirement.StatusId;
			requirement.AuthorId = remoteRequirement.AuthorId;
			requirement.OwnerId = remoteRequirement.OwnerId;
			requirement.ImportanceId = remoteRequirement.ImportanceId;
			requirement.ReleaseId = remoteRequirement.ReleaseId;
			requirement.Name = remoteRequirement.Name;
			requirement.Description = remoteRequirement.Description;
			requirement.CreationDate = GlobalFunctions.UniversalizeDate(remoteRequirement.CreationDate);
			requirement.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteRequirement.LastUpdateDate);
			requirement.IsSummary = remoteRequirement.Summary;
		}

		#endregion

		#region Test Case Methods

		/// <summary>
		/// Creates a new test case folder in the system
		/// </summary>
		/// <param name="remoteTestCase">The new test case object (primary key will be empty)</param>
		/// <param name="parentTestFolderId">Do we want to insert the test case under a parent folder</param>
		/// <returns>The populated test case object - including the primary key</returns>
		/// <remarks>(folder ids are negative to avoid collisions with test case ids)</remarks>
		[
		WebMethod
			(
			Description = "Creates a new test case folder in the system",
			EnableSession = true
			)
		]
		public RemoteTestCase TestCase_CreateFolder(RemoteTestCase remoteTestCase, Nullable<int> parentTestFolderId)
		{
			const string METHOD_NAME = "TestCase_CreateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test case id specified
			if (remoteTestCase.TestCaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Case object needs to have TestCaseId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Folders",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no author provided
			if (remoteTestCase.AuthorId == -1 || remoteTestCase.AuthorId == 0)
			{
				remoteTestCase.AuthorId = userId;
			}
			//Default to the current project if not set
			if (remoteTestCase.ProjectId == 0 || remoteTestCase.ProjectId == -1)
			{
				remoteTestCase.ProjectId = projectId;
			}

			//Instantiate the test case business class
			TestCaseManager testCaseManager = new TestCaseManager();

			//See if we have a passed in parent folder or not
			if (parentTestFolderId.HasValue)
			{
				//If the folder is negative, inverse
				if (parentTestFolderId.Value < 0)
				{
					parentTestFolderId = -parentTestFolderId.Value;
				}
			}

			//Insert the folder inside the parent folder
			remoteTestCase.TestCaseId = -testCaseManager.TestCaseFolder_Create(
				remoteTestCase.Name,
				projectId,
				remoteTestCase.Description,
				parentTestFolderId
				).TestCaseFolderId;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCase;
		}

		/// <summary>
		/// Creates a new test case in the system
		/// </summary>
		/// <param name="remoteTestCase">The new test case object (primary key will be empty)</param>
		/// <param name="parentTestFolderId">Do we want to insert the test case under a parent folder</param>
		/// <returns>The populated test case object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new test case in the system",
			EnableSession = true
			)
		]
		public RemoteTestCase TestCase_Create(RemoteTestCase remoteTestCase, Nullable<int> parentTestFolderId)
		{
			const string METHOD_NAME = "TestCase_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test case id specified
			if (remoteTestCase.TestCaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Case object needs to have TestCaseId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no author provided
			if (remoteTestCase.AuthorId == -1 || remoteTestCase.AuthorId == 0)
			{
				remoteTestCase.AuthorId = userId;
			}
			//Default to the current project if not set
			if (remoteTestCase.ProjectId == 0 || remoteTestCase.ProjectId == -1)
			{
				remoteTestCase.ProjectId = projectId;
			}

			//See if we have a passed in parent folder or not
			if (parentTestFolderId.HasValue)
			{
				//If the folder is negative, inverse
				if (parentTestFolderId.Value < 0)
				{
					parentTestFolderId = -parentTestFolderId.Value;
				}
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTestCase.TestCasePriorityId >= 1 && remoteTestCase.TestCasePriorityId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteTestCase);
			}

			//Instantiate the test case business class
			TestCaseManager testCaseManager = new TestCaseManager();
			//See if we have a passed in parent folder or not

			//Now insert the test case
			remoteTestCase.TestCaseId = testCaseManager.Insert(
				userId,
				projectId,
				remoteTestCase.AuthorId,
				remoteTestCase.OwnerId,
				remoteTestCase.Name,
				remoteTestCase.Description,
				null,
				(remoteTestCase.Active) ? TestCase.TestCaseStatusEnum.ReadyForTest : TestCase.TestCaseStatusEnum.Obsolete,
				remoteTestCase.TestCasePriorityId,
				parentTestFolderId,
				remoteTestCase.EstimatedDuration,
				null,
				null
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			testCaseManager.SendCreationNotification(remoteTestCase.TestCaseId.Value, artifactCustomProperty, null);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCase;
		}

		/// <summary>
		/// Retrieves a list of testCases in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of testCases and/or folders</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of testCases in the system that match the provided filter",
			EnableSession = true
			)
		]
		public List<RemoteTestCase> TestCase_Retrieve(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "TestCase_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testCases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View TestCases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, startingRow, numberOfRows, filters, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

			//Get the complete folder list for the project
			List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);

			//All Items / Filtered List - we need to group under the appropriate folder
			List<IGrouping<int?, TestCaseView>> groupedTestCasesByFolder = testCases.GroupBy(t => t.TestCaseFolderId).ToList();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

			//Populate the API data object and return
			//string runningIndentLevel = "AAA";  //Not used by this version of the API
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();

			//Add the root-folder testCases (if any)
			IGrouping<int?, TestCaseView> groupedTestCases = groupedTestCasesByFolder.FirstOrDefault(f => f.Key == null);
			if (groupedTestCases != null)
			{
				List<TestCaseView> groupedTestCaseList = groupedTestCases.ToList();
				if (groupedTestCaseList != null && groupedTestCaseList.Count > 0)
				{
					foreach (TestCaseView testCase in groupedTestCaseList)
					{
						//Create and populate the row
						RemoteTestCase remoteTestCase = new RemoteTestCase();
						PopulateTestCase(remoteTestCase, testCase);
						PopulateCustomProperties(remoteTestCase, testCase, customProperties);
						remoteTestCases.Add(remoteTestCase);

						//Increment the indent level -- Not used by this version of the API
						//runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
				}
				else
				{
					//No testCases at root, so folder is first child entry
					//runningIndentLevel = runningIndentLevel + "AAA";
				}
			}

			//Loop through the folders
			//int lastFolderLevel = 0;
			foreach (TestCaseFolderHierarchyView testCaseFolder in testCaseFolders)
			{
				//See if this folder is a peer, child or above the last one
				if (testCaseFolder.HierarchyLevel.HasValue)
				{
					/* -- not used by the v2.2 API
					if (testCaseFolder.HierarchyLevel.Value > lastFolderLevel)
					{
						runningIndentLevel = runningIndentLevel + "AAA";
					}
					else if (testCaseFolder.HierarchyLevel.Value == lastFolderLevel)
					{
						//Increment the indent level
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
					else
					{
						int numberOfLevels = lastFolderLevel - testCaseFolder.HierarchyLevel.Value;
						runningIndentLevel = runningIndentLevel.SafeSubstring(0, runningIndentLevel.Length - (numberOfLevels * 3));
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
					lastFolderLevel = testCaseFolder.HierarchyLevel.Value;*/
				}

				//Add the folder item
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulateTestCase(projectId, remoteTestCase, testCaseFolder);
				remoteTestCases.Add(remoteTestCase);

				//Add the testCases (if any)
				groupedTestCases = groupedTestCasesByFolder.FirstOrDefault(f => f.Key == testCaseFolder.TestCaseFolderId);
				if (groupedTestCases != null)
				{
					List<TestCaseView> groupedTestCaseList = groupedTestCases.ToList();
					if (groupedTestCaseList != null && groupedTestCaseList.Count > 0)
					{
						foreach (TestCaseView testCase in groupedTestCaseList)
						{
							//Create and populate the row
							remoteTestCase = new RemoteTestCase();
							PopulateTestCase(remoteTestCase, testCase);
							PopulateCustomProperties(remoteTestCase, testCase, customProperties);
							remoteTestCases.Add(remoteTestCase);

							//Increment the indent level -- Not used by this version of the API
							//runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
						}
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCases;
		}

		/// <summary>
		/// Retrieves a list of testCases in a particular release that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <param name="releaseId">The release we're interested in</param>
		/// <returns>List of testCases (no folders)</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of testCases in a particular release that match the provided filter",
			EnableSession = true
			)
		]
		public List<RemoteTestCase> TestCase_RetrieveByReleaseId(int releaseId, List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "TestCase_RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testCases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View TestCases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(projectId, releaseId, "Name", true, startingRow, numberOfRows, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

			//Populate the API data object and return
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
			foreach (TestCaseReleaseView testCase in testCases)
			{
				//Create and populate the row
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulateTestCase(remoteTestCase, testCase);
				PopulateCustomProperties(remoteTestCase, testCase, customProperties);
				remoteTestCases.Add(remoteTestCase);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCases;
		}

		/// <summary>
		/// Retrieves all testCases owned by the currently authenticated user
		/// </summary>
		/// <returns>List of testCases</returns>
		[
		WebMethod
			(
			Description = "Retrieves all testCases owned by the currently authenticated user",
			EnableSession = true
			)
		]
		public List<RemoteTestCase> TestCase_RetrieveForOwner()
		{
			const string METHOD_NAME = "TestCase_RetrieveForOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(userId, null);

			//Get the custom property definitions for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.TestCase);

			//Populate the API data object and return
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
			foreach (TestCaseView testCase in testCases)
			{
				//Create and populate the row
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulateTestCase(remoteTestCase, testCase);
				PopulateCustomProperties(remoteTestCase, testCase, customProperties);
				remoteTestCases.Add(remoteTestCase);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCases;
		}

		/// <summary>
		/// Retrieves a single test case/folder in the system
		/// </summary>
		/// <param name="testCaseId">The id of the test case/folder</param>
		/// <returns>Test Case object</returns>
		/// <remarks>positive = test case, negative = test folder</remarks>
		[
		WebMethod
			(
			Description = "Retrieves a single test case/folder in the system",
			EnableSession = true
			)
		]
		public RemoteTestCase TestCase_RetrieveById(int testCaseId)
		{
			const string METHOD_NAME = "TestCase_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the test case dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test case was not found, just return null
			try
			{
				//See if we have folder or test case
				RemoteTestCase remoteTestCase = null;
				if (testCaseId > 0)
				{
					TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Get the custom property definitions
					List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

					//Populate the API data object and return
					remoteTestCase = new RemoteTestCase();
					PopulateTestCase(remoteTestCase, testCase);
					PopulateCustomProperties(remoteTestCase, testCase, customProperties);
				}
				else
				{
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(-testCaseId);

					//Populate the API data object and return
					remoteTestCase = new RemoteTestCase();
					PopulateTestCase(remoteTestCase, testCaseFolder);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestCase;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Retrieves all the test cases that are part of a test set
		/// </summary>
		/// <param name="testSetId">The id of the test set</param>
		/// <returns>List of Test Cases (not folders)</returns>
		[
		WebMethod
			(
			Description = "Retrieves all the test cases that are part of a test set",
			EnableSession = true
			)
		]
		public List<RemoteTestCase> TestCase_RetrieveByTestSetId(int testSetId)
		{
			const string METHOD_NAME = "TestCase_RetrieveByTestSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the test set test case dataset
			TestSetManager testSetManager = new TestSetManager();

			//If the test case was not found, just return null
			try
			{
				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

				//Populate the API data object and return
				List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
				foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
				{
					RemoteTestCase remoteTestCase = new RemoteTestCase();
					remoteTestCases.Add(remoteTestCase);
					PopulateTestCase(remoteTestCase, testSetTestCase);
					PopulateCustomProperties(remoteTestCase, testSetTestCase, customProperties);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestCases;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates a test case (or test folder) in the system
		/// </summary>
		/// <param name="remoteTestCase">The updated test case object</param>
		/// <param name="remoteTestSteps">Any updated test step objects</param>
		/// <remarks>Does not currently update test step custom properties</remarks>
		[
		WebMethod
			(
			Description = "Updates a test case in the system",
			EnableSession = true
			)
		]
		public void TestCase_Update(RemoteTestCase remoteTestCase, List<RemoteTestStep> remoteTestSteps)
		{
			const string METHOD_NAME = "TestCase_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a testCase id specified
			if (!remoteTestCase.TestCaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("TestCase object needs to have TestCaseId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//See if we have a test case or test folder
				if (remoteTestCase.TestCaseId < 0)
				{
					//Test Folder
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(-remoteTestCase.TestCaseId.Value);

					//Make sure that the project ids match
					if (testCaseFolder.ProjectId != projectId)
					{
						throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
							SoapException.ClientFaultCode,
							Context.Request.Url.AbsoluteUri);
					}

					//Update
					testCaseFolder.StartTracking();
					testCaseFolder.Name = remoteTestCase.Name;
					testCaseFolder.Description = remoteTestCase.Description;
					testCaseFolder.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestCase.LastUpdateDate);
					testCaseManager.TestCaseFolder_Update(testCaseFolder);
				}
				else
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Test Case
					TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, remoteTestCase.TestCaseId.Value);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestCase.TestCaseId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);

					//Make sure that the project ids match
					if (testCase.ProjectId != projectId)
					{
						throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
							SoapException.ClientFaultCode,
							Context.Request.Url.AbsoluteUri);
					}

					//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
					//First the test case report
					UpdateTestCaseData(projectTemplateId, testCase, remoteTestCase);
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value);

					//Now the test steps records
					if (remoteTestSteps != null)
					{
						foreach (RemoteTestStep remoteTestStep in remoteTestSteps)
						{
							//Locate the matching row
							if (remoteTestStep.TestStepId.HasValue)
							{
								TestStep testStep = testCase.TestSteps.FirstOrDefault(s => s.TestStepId == remoteTestStep.TestStepId.Value);
								if (testStep != null)
								{
									UpdateTestStepData(testStep, remoteTestStep);
								}
							}
						}
					}

					//Extract changes for use in notifications
					Dictionary<string, object> changes = testCase.ExtractChanges();
					ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

					//Call the business object to actually update the testCase dataset and the custom properties
					testCaseManager.Update(testCase, userId);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Call notifications..
					try
					{
						testCase.ApplyChanges(changes);
						new NotificationManager().SendNotificationForArtifact(testCase, notificationCust, null);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + testCase.ArtifactToken);
					}
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Returns the full token of a test case parameter from its name
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <returns>The tokenized representation of the parameter used for search/replace</returns>
		/// <remarks>We use the same parameter format as Ant/NAnt</remarks>
		[
		WebMethod
			(
			Description = "Returns the full token of a test caseparameter from its name",
			EnableSession = true
			)
		]
		public string TestCase_CreateParameterToken(string parameterName)
		{
			return TestCaseManager.CreateParameterToken(parameterName);
		}

		/// <summary>
		/// Adds a new parameter for a test case
		/// </summary>
		/// <param name="remoteTestCaseParameter">The new test case parameter to add</param>
		/// <returns>The test case parameter object with the primary key populated</returns>
		/// <remarks>The parameter name is always made lower case</remarks>
		[
		WebMethod
			(
			Description = "Adds a new parameter for a test case",
			EnableSession = true
			)
		]
		public RemoteTestCaseParameter TestCase_AddParameter(RemoteTestCaseParameter remoteTestCaseParameter)
		{
			const string METHOD_NAME = "TestCase_AddParameter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test case parameter id already specified
			if (remoteTestCaseParameter.TestCaseParameterId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Case Parameter object needs to have TestCaseParameterId set to null for adding",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}


			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test case business class
				TestCaseManager testCaseManager = new TestCaseManager();

				//Now insert the test case parameter
				remoteTestCaseParameter.TestCaseParameterId = testCaseManager.InsertParameter(
					projectId,
					remoteTestCaseParameter.TestCaseId,
					remoteTestCaseParameter.Name,
					remoteTestCaseParameter.DefaultValue
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestCaseParameter;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCaseView">The internal datarow</param>
		protected void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseView testCaseView)
		{
			remoteTestCase.TestCaseId = testCaseView.TestCaseId;
			remoteTestCase.ProjectId = testCaseView.ProjectId;
			remoteTestCase.ExecutionStatusId = testCaseView.ExecutionStatusId;
			remoteTestCase.AuthorId = testCaseView.AuthorId;
			remoteTestCase.OwnerId = testCaseView.OwnerId;
			remoteTestCase.TestCasePriorityId = testCaseView.TestCasePriorityId;
			remoteTestCase.Name = testCaseView.Name;
			remoteTestCase.Description = testCaseView.Description;
			remoteTestCase.Folder = false; //Separate case now
			remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testCaseView.CreationDate);
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.LastUpdateDate);
			remoteTestCase.ExecutionDate = testCaseView.ExecutionDate;
			remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
			remoteTestCase.Active = testCaseView.IsActive;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.ConcurrencyDate);
		}

		/// <summary>
		/// Populates a data-sync system API object from a test folder (makes the id negative)
		/// </summary>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCaseFolder">The internal datarow</param>
		protected void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseFolder testCaseFolder)
		{
			remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
			remoteTestCase.ProjectId = testCaseFolder.ProjectId;
			remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
			remoteTestCase.Name = testCaseFolder.Name;
			remoteTestCase.Description = testCaseFolder.Description;
			remoteTestCase.Folder = true;
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseFolder.LastUpdateDate);
			remoteTestCase.ExecutionDate = testCaseFolder.ExecutionDate;
			remoteTestCase.EstimatedDuration = testCaseFolder.EstimatedDuration;
			remoteTestCase.Active = true;
		}

		/// <summary>
		/// Populates a data-sync system API object from a test folder (makes the id negative)
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCaseFolder">The internal datarow</param>
		protected void PopulateTestCase(int projectId, RemoteTestCase remoteTestCase, TestCaseFolderHierarchyView testCaseFolder)
		{
			remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
			remoteTestCase.ProjectId = projectId;
			remoteTestCase.Name = testCaseFolder.Name;
			remoteTestCase.Folder = true;
			remoteTestCase.Active = true;
			remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCaseView">The internal datarow</param>
		protected void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseReleaseView testCaseView)
		{
			remoteTestCase.TestCaseId = testCaseView.TestCaseId;
			remoteTestCase.ProjectId = testCaseView.ProjectId;
			remoteTestCase.ExecutionStatusId = testCaseView.ExecutionStatusId;
			remoteTestCase.AuthorId = testCaseView.AuthorId;
			remoteTestCase.OwnerId = testCaseView.OwnerId;
			remoteTestCase.TestCasePriorityId = testCaseView.TestCasePriorityId;
			remoteTestCase.Name = testCaseView.Name;
			remoteTestCase.Description = testCaseView.Description;
			remoteTestCase.Folder = false; //Separate case now
			remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testCaseView.CreationDate);
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.LastUpdateDate);
			remoteTestCase.ExecutionDate = testCaseView.ExecutionDate;
			remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
			remoteTestCase.Active = testCaseView.IsActive;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.ConcurrencyDate);
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testSetTestCaseView">The internal datarow</param>
		/// <remarks>Used to translate the newer test set test case list to the older API object</remarks>
		protected void PopulateTestCase(RemoteTestCase remoteTestCase, TestSetTestCaseView testSetTestCaseView)
		{
			remoteTestCase.TestCaseId = testSetTestCaseView.TestCaseId;
			remoteTestCase.ProjectId = testSetTestCaseView.ProjectId;
			if (testSetTestCaseView.ExecutionStatusId.HasValue)
			{
				remoteTestCase.ExecutionStatusId = testSetTestCaseView.ExecutionStatusId.Value;
			}
			remoteTestCase.AuthorId = testSetTestCaseView.AuthorId;
			remoteTestCase.OwnerId = testSetTestCaseView.OwnerId;
			remoteTestCase.TestCasePriorityId = testSetTestCaseView.TestCasePriorityId;
			remoteTestCase.Name = testSetTestCaseView.Name;
			remoteTestCase.Description = testSetTestCaseView.Description;
			remoteTestCase.Folder = false;  //There are no folders in a test set
			remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.CreationDate);
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.LastUpdateDate);
			remoteTestCase.ExecutionDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)testSetTestCaseView.ExecutionDate);
			remoteTestCase.EstimatedDuration = testSetTestCaseView.EstimatedDuration;
			remoteTestCase.Active = true;   //Always active if in test set

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.ConcurrencyDate);
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template</param>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCase">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateTestCaseData(int projectTemplateId, TestCase testCase, RemoteTestCase remoteTestCase)
		{
			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTestCase.TestCasePriorityId >= 1 && remoteTestCase.TestCasePriorityId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteTestCase);
			}

			//The remote object uses LastUpdateDate for concurrency
			testCase.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteTestCase.LastUpdateDate);
			testCase.StartTracking();

			testCase.AuthorId = remoteTestCase.AuthorId;
			testCase.OwnerId = remoteTestCase.OwnerId;
			testCase.TestCasePriorityId = remoteTestCase.TestCasePriorityId;
			testCase.Name = remoteTestCase.Name;
			testCase.Description = remoteTestCase.Description;
			testCase.CreationDate = GlobalFunctions.UniversalizeDate(remoteTestCase.CreationDate);
			testCase.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestCase.LastUpdateDate);
			testCase.EstimatedDuration = remoteTestCase.EstimatedDuration;
		}

		#endregion

		#region Test Step Methods

		/// <summary>
		/// Creates a new test step in the system
		/// </summary>
		/// <param name="remoteTestStep">The new test step object (primary key will be empty)</param>
		/// <param name="testCaseId">The test case to add it to</param>
		/// <returns>The populated test step object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new test step in the system",
			EnableSession = true
			)
		]
		public RemoteTestStep TestStep_Create(RemoteTestStep remoteTestStep, int testCaseId)
		{
			const string METHOD_NAME = "TestStep_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test step id specified
			if (remoteTestStep.TestStepId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Step object needs to have TestStepId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Steps",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test case business class
				TestCaseManager testCaseManager = new TestCaseManager();

				//Now insert the test step
				remoteTestStep.TestStepId = testCaseManager.InsertStep(
					userId,
					testCaseId,
					remoteTestStep.Position,
					remoteTestStep.Description,
					remoteTestStep.ExpectedResult,
					remoteTestStep.SampleData
					);

				//Now we need to populate any custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = null;
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestStep, projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, remoteTestStep.TestStepId.Value);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestStep;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new test step that is actually a link to a test case
		/// </summary>
		/// <param name="testCaseId">The test case to add it to</param>
		/// <param name="linkedTestCaseId">The test case being linked to</param>
		/// <param name="parameters">Any parameters to passed to the linked test case</param>
		/// <param name="position">The position in the test step list</param>
		/// <returns>The id of the new test step</returns>
		[
		WebMethod
			(
			Description = "Adds a new test step that is actually a link to a test case",
			EnableSession = true
			)
		]
		public int TestStep_CreateLink(int testCaseId, int position, int linkedTestCaseId, List<RemoteTestStepParameter> parameters)
		{
			const string METHOD_NAME = "TestStep_CreateLink";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Steps",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test case business class
				TestCaseManager testCaseManager = new TestCaseManager();

				//Convert the Soap-safe collection into a normal dictionary, handling null case
				Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();
				if (parameters != null)
				{
					foreach (RemoteTestStepParameter parameter in parameters)
					{
						parametersDictionary.Add(parameter.Name, parameter.Value);
					}
				}

				//Now insert the link test step
				int testStepId = testCaseManager.InsertLink(userId, testCaseId, position, linkedTestCaseId, parametersDictionary);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return testStepId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the test steps in a particular test case
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <returns>Collection of Test Step objects</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single test case/folder in the system",
			EnableSession = true
			)
		]
		public List<RemoteTestStep> TestStep_RetrieveByTestCaseId(int testCaseId)
		{
			const string METHOD_NAME = "TestStep_RetrieveByTestCaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Steps",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the test case dataset
			TestCaseManager testCaseManager = new TestCaseManager();

			//If the test case was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);

				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);

				//Populate the test step API data objects and return
				List<RemoteTestStep> remoteTestSteps = new List<RemoteTestStep>();
				foreach (TestStepView testStep in testSteps)
				{
					RemoteTestStep remoteTestStep = new RemoteTestStep();
					PopulateTestStep(remoteTestStep, testStep);
					PopulateCustomProperties(remoteTestStep, testStep, customProperties);
					remoteTestSteps.Add(remoteTestStep);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestSteps;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestStep">The API data object</param>
		/// <param name="testStepView">The internal datarow</param>
		protected void PopulateTestStep(RemoteTestStep remoteTestStep, TestStepView testStepView)
		{
			remoteTestStep.TestStepId = testStepView.TestStepId;
			remoteTestStep.TestCaseId = testStepView.TestCaseId;
			remoteTestStep.ExecutionStatusId = testStepView.ExecutionStatusId;
			remoteTestStep.Position = testStepView.Position;
			remoteTestStep.Description = testStepView.Description;
			remoteTestStep.ExpectedResult = testStepView.ExpectedResult;
			remoteTestStep.SampleData = testStepView.SampleData;
			remoteTestStep.LinkedTestCaseId = testStepView.LinkedTestCaseId;
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestStep">The API data object</param>
		/// <param name="testStep">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateTestStepData(TestStep testStep, RemoteTestStep remoteTestStep)
		{
			//No concurrency tracking supported in the v2.2 API for test steps
			testStep.StartTracking();

			testStep.TestCaseId = remoteTestStep.TestCaseId;
			testStep.ExecutionStatusId = remoteTestStep.ExecutionStatusId;
			testStep.Position = remoteTestStep.Position;
			testStep.Description = remoteTestStep.Description;
			testStep.ExpectedResult = remoteTestStep.ExpectedResult;
			testStep.SampleData = remoteTestStep.SampleData;
			testStep.LinkedTestCaseId = remoteTestStep.LinkedTestCaseId;
		}

		#endregion

		#region Test Run Methods

		/// <summary>
		/// Creates a new test run shell from the provided test case(s)
		/// </summary>
		/// <param name="testCaseIds">The test cases to create the run for</param>
		/// <param name="releaseId">A release to associate the test run with (optional)</param>
		/// <returns>the new test case run data object</returns>
		[
		WebMethod
			(
			Description = "Creates a new test run shell from the provided test case(s)",
			EnableSession = true
			)
		]
		public RemoteTestRun TestRun_CreateFromTestCases(List<int> testCaseIds, Nullable<int> releaseId)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestCases";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(userId, projectId, releaseId, testCaseIds, false);

				if (testRunsPending.TestRuns.Count == 0)
				{
					//No test runs
					return null;
				}

				//Populate the API data object and return
				//We don't have any custom properties to populate at this point
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulateTestRun(remoteTestRun, testRunsPending.TestRuns[0]);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new test run shell from the provided test set
		/// </summary>
		/// <param name="testSetId">The test set to create the run for</param>
		/// <returns>the new test case run data object</returns>
		[
		WebMethod
			(
			Description = "Creates a new test run shell from the provided test set",
			EnableSession = true
			)
		]
		public RemoteTestRun TestRun_CreateFromTestSet(int testSetId)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(userId, projectId, testSetId, false);

				if (testRunsPending.TestRuns.Count == 0)
				{
					//No test runs
					return null;
				}

				//Populate the API data object and return
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulateTestRun(remoteTestRun, testRunsPending.TestRuns[0]);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//If the test set had some list custom properties set then we should look for corresponding
				//lists in the test run and if there are matching lists we should set the values to be the same
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty testSetCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

				//Make sure we have some custom properties on the test set
				if (testSetCustomProperties != null)
				{
					//Get the custom property definitions for test runs
					List<CustomProperty> testRunCustomPropertyDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

					//Create a new custom property entity for the test run
					ArtifactCustomProperty testRunArtifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, -1, testRunCustomPropertyDefinitions);
					testRunArtifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, testRunArtifactCustomProperty);
					foreach (CustomProperty testSetCustomPropertyDefinition in testSetCustomProperties.CustomPropertyDefinitions)
					{
						//See if we have a matching property in the test run custom properties (only works for lists)
						if (testSetCustomPropertyDefinition.CustomPropertyListId.HasValue)
						{
							foreach (CustomProperty testRunCustomPropertyDefinition in testRunCustomPropertyDefinitions)
							{
								if (testRunCustomPropertyDefinition.CustomPropertyListId.HasValue && testRunCustomPropertyDefinition.CustomPropertyListId.Value == testSetCustomPropertyDefinition.CustomPropertyListId.Value)
								{
									//We have a matching custom list between the test set and test run
									//So set the value on the matching test run property
									object customPropertyValue = testSetCustomProperties.CustomProperty(testSetCustomPropertyDefinition.PropertyNumber);
									testRunArtifactCustomProperty.SetCustomProperty(testRunCustomPropertyDefinition.PropertyNumber, customPropertyValue);
								}
							}
						}
					}
					//Populate the API data object from the internal entity
					PopulateCustomProperties(remoteTestRun, testRunArtifactCustomProperty);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of test runs in the system that match the provided filter/sort
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		[
		WebMethod
			(
			Description = "Retrieves a list of test runs in the system that match the provided filter/sort",
			EnableSession = true
			)
		]
		public List<RemoteTestRun> TestRun_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "TestRun_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("You need to provide a populated RemoteSort object as parameter",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testRuns
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the testRun dataset
			//Doesn't include the test run steps
			TestRunManager testRunManager = new TestRunManager();
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

			//Populate the API data object and return
			List<RemoteTestRun> remoteTestRuns = new List<RemoteTestRun>();
			foreach (TestRunView testRun in testRuns)
			{
				//Create and populate the row
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulateTestRun(remoteTestRun, testRun);
				PopulateCustomProperties(remoteTestRun, testRun, customProperties);
				remoteTestRuns.Add(remoteTestRun);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestRuns;
		}

		/// <summary>
		/// Retrieves a single test run in the system including any associated steps
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>Test Run object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single test run in the system including any associated steps",
			EnableSession = true
			)
		]
		public RemoteTestRun TestRun_RetrieveById(int testRunId)
		{
			const string METHOD_NAME = "TestRun_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();

			//If the test run was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulateTestRun(remoteTestRun, testRun);
				PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test run");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Records the results of executing an automated test
		/// </summary>
		/// <param name="testSetId">The test set to create the run for</param>
		/// <returns>the test run data object with its primary key populated</returns>
		[
		WebMethod
			(
			Description = "Records the results of executing an automated test",
			EnableSession = true
			)
		]
		public RemoteTestRun TestRun_RecordAutomated1(RemoteTestRun remoteTestRun)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a runner name was provided (needed for automated tests)
			if (String.IsNullOrEmpty(remoteTestRun.RunnerName))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Run object needs to have RunnerName set to a value for recording an automated test result",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Default to the authenticated user if we have no tester provided
				if (remoteTestRun.TesterId == -1 || remoteTestRun.TesterId == 0)
				{
					remoteTestRun.TesterId = userId;
				}

				//Handle nullable properties
				DateTime endDate = DateTime.UtcNow;
				string runnerTestName = "Unknown?";
				string runnerMessage = "Nothing Reported";
				string runnerStackTrace = "Nothing Reported";
				int runnerAssertCount = 0;
				if (remoteTestRun.EndDate.HasValue)
				{
					endDate = GlobalFunctions.UniversalizeDate(remoteTestRun.EndDate.Value);
				}
				if (!String.IsNullOrEmpty(remoteTestRun.RunnerTestName))
				{
					runnerTestName = remoteTestRun.RunnerTestName;
				}
				if (!String.IsNullOrEmpty(remoteTestRun.RunnerMessage))
				{
					runnerMessage = remoteTestRun.RunnerMessage;
				}
				if (!String.IsNullOrEmpty(remoteTestRun.RunnerStackTrace))
				{
					runnerStackTrace = remoteTestRun.RunnerStackTrace;
				}
				if (remoteTestRun.RunnerAssertCount.HasValue)
				{
					runnerAssertCount = remoteTestRun.RunnerAssertCount.Value;
				}

				//Actually create the new test run
				int testRunId = testRunManager.Record(
					projectId,
					remoteTestRun.TesterId,
					remoteTestRun.TestCaseId,
					remoteTestRun.ReleaseId,
					remoteTestRun.TestSetId,
					null,
					GlobalFunctions.UniversalizeDate(remoteTestRun.StartDate),
					endDate,
					remoteTestRun.ExecutionStatusId,
					remoteTestRun.RunnerName,
					runnerTestName,
					runnerAssertCount,
					runnerMessage,
					runnerStackTrace,
					null,
					null,
					null,
					TestRun.TestRunFormatEnum.PlainText,
					null
					);

				//Need to now save the custom properties if necessary
				ArtifactCustomProperty artifactCustomProperty = null;
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Populate the API data object with the primary key and return
				remoteTestRun.TestRunId = testRunId;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Records the results of executing an automated test
		/// </summary>
		/// <param name="userName">The username of the user</param>
		/// <param name="password">The unhashed password of the user</param>
		/// <param name="projectId">The project to connect to</param>
		/// <param name="testerUserId">The user id of the person who's running the test (-1 for logged in user)</param>
		/// <param name="testCaseId">The test case being executed</param>
		/// <param name="releaseId">The release being executed against (optional)</param>
		/// <param name="testSetId">The test set being executed against (optional)</param>
		/// <param name="executionStatusId">The status of the test run (pass/fail/not run)</param>
		/// <param name="runnerName">The name of the automated testing tool</param>
		/// <param name="runnerAssertCount">The number of assertions</param>
		/// <param name="runnerMessage">The failure message (if appropriate)</param>
		/// <param name="runnerStackTrace">The error stack trace (if any)s</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <returns>The newly created test run id</returns>
		/// <remarks>
		/// Use this version of the method for clients that cannot handle session cookies or complex data objects.
		/// Unlike the TestRun_RecordAutomated1 it cannot handle custom properties
		/// </remarks>
		[
		WebMethod
			(
			Description = "Records the results of executing an automated test, use this version when client cannot handle session cookies",
			EnableSession = true
			)
		]
		public int TestRun_RecordAutomated2(string userName, string password, int projectId, int testerUserId, int testCaseId, Nullable<int> releaseId, Nullable<int> testSetId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a runner name was provided (needed for automated tests)
			if (String.IsNullOrEmpty(runnerName))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Run object needs to have RunnerName set to a value for recording an automated test result",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!this.Connection_Authenticate(userName, password))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Session Not Authenticated - check user name and password",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!this.Connection_ConnectToProject(projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Cannot connect to project, check that user is a member of the project and that it exists and is active.",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Default to the authenticated user if no tester provided
				//Really should have made field Nullable<> but don't wnat to break interface compatiblity
				if (testerUserId == -1 || testerUserId == 0)
				{
					testerUserId = userId;
				}

				//Actually create the new test run
				int testRunId = testRunManager.Record(
					projectId,
					testerUserId,
					testCaseId,
					releaseId,
					testSetId,
					null,
					GlobalFunctions.UniversalizeDate(startDate),
					GlobalFunctions.UniversalizeDate(endDate),
					executionStatusId,
					runnerName,
					runnerTestName,
					runnerAssertCount,
					runnerMessage,
					runnerStackTrace,
					null,
					null,
					null,
					TestRun.TestRunFormatEnum.PlainText,
					null
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return testRunId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
		/// <param name="dataSet">The internal dataset containing a test run and its test run steps</param>
		protected void PopulateTestRun(RemoteTestRun remoteTestRun, TestRun testRun)
		{
			//First populate the test run itself
			remoteTestRun.TestRunId = testRun.TestRunId;
			remoteTestRun.Name = testRun.Name;
			remoteTestRun.TestCaseId = testRun.TestCaseId;
			remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
			remoteTestRun.TesterId = testRun.TesterId;
			remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
			remoteTestRun.ReleaseId = testRun.ReleaseId;
			remoteTestRun.TestSetId = testRun.TestSetId;
			remoteTestRun.StartDate = GlobalFunctions.LocalizeDate(testRun.StartDate);
			remoteTestRun.EndDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)testRun.EndDate);
			remoteTestRun.RunnerName = testRun.RunnerName;
			remoteTestRun.RunnerTestName = testRun.RunnerTestName;
			remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
			remoteTestRun.RunnerMessage = testRun.RunnerMessage;
			remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;

			//Next any test run steps
			if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
			{
				remoteTestRun.TestRunSteps = new List<RemoteTestRunStep>();
				foreach (TestRunStep testRunStep in testRun.TestRunSteps)
				{
					RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
					remoteTestRun.TestRunSteps.Add(remoteTestRunStep);
					//Populate the item
					remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
					remoteTestRunStep.TestRunId = testRunStep.TestRunId;
					remoteTestRunStep.TestStepId = testRunStep.TestStepId;
					remoteTestRunStep.TestCaseId = testRunStep.TestCaseId;
					remoteTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
					remoteTestRunStep.Position = testRunStep.Position;
					remoteTestRunStep.Description = testRunStep.Description;
					remoteTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
					remoteTestRunStep.SampleData = testRunStep.SampleData;
					remoteTestRunStep.ActualResult = testRunStep.ActualResult;
				}
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
		/// <param name="dataSet">The internal dataset containing a test run and its test run steps</param>
		/// <remarks>This version does not handle the steps</remarks>
		protected void PopulateTestRun(RemoteTestRun remoteTestRun, TestRunView testRun)
		{
			//First populate the test run itself
			remoteTestRun.TestRunId = testRun.TestRunId;
			remoteTestRun.Name = testRun.Name;
			remoteTestRun.TestCaseId = testRun.TestCaseId;
			remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
			remoteTestRun.TesterId = testRun.TesterId;
			remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
			remoteTestRun.ReleaseId = testRun.ReleaseId;
			remoteTestRun.TestSetId = testRun.TestSetId;
			remoteTestRun.StartDate = GlobalFunctions.LocalizeDate(testRun.StartDate);
			remoteTestRun.EndDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)testRun.EndDate);
			remoteTestRun.RunnerName = testRun.RunnerName;
			remoteTestRun.RunnerTestName = testRun.RunnerTestName;
			remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
			remoteTestRun.RunnerMessage = testRun.RunnerMessage;
			remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
		/// <param name="testRunsPending">The internal dataset</param>
		/// <remarks>Updates both the test run and the test run steps</remarks>
		protected void UpdateTestRunData(TestRunsPending testRunsPending, RemoteTestRun remoteTestRun, int projectId)
		{
			testRunsPending.StartTracking();

			//First populate the pending run
			testRunsPending.TesterId = remoteTestRun.TesterId;
			testRunsPending.Name = remoteTestRun.Name;
			testRunsPending.CreationDate = DateTime.UtcNow;
			testRunsPending.LastUpdateDate = DateTime.UtcNow;
			testRunsPending.ProjectId = projectId;
			testRunsPending.TestSetId = remoteTestRun.TestSetId;
			testRunsPending.CountNotRun = 1;    //Single test run object

			//Next populate the test run
			TestRun testRun = new TestRun();
			testRunsPending.TestRuns.Add(testRun);
			testRun.Name = remoteTestRun.Name;
			testRun.TestCaseId = remoteTestRun.TestCaseId;
			testRun.TestRunTypeId = remoteTestRun.TestRunTypeId;
			testRun.TesterId = remoteTestRun.TesterId;
			testRun.ExecutionStatusId = remoteTestRun.ExecutionStatusId;
			testRun.ReleaseId = remoteTestRun.ReleaseId;
			testRun.TestSetId = remoteTestRun.TestSetId;
			testRun.StartDate = GlobalFunctions.UniversalizeDate(remoteTestRun.StartDate);
			testRun.EndDate = GlobalFunctions.UniversalizeDate(remoteTestRun.EndDate);
			testRun.RunnerName = remoteTestRun.RunnerName;
			testRun.RunnerTestName = remoteTestRun.RunnerTestName;
			testRun.RunnerAssertCount = remoteTestRun.RunnerAssertCount;
			testRun.RunnerMessage = remoteTestRun.RunnerMessage;
			testRun.RunnerStackTrace = remoteTestRun.RunnerStackTrace;
			testRun.IsAttachments = false;

			//Concurrency Management
			testRun.ConcurrencyDate = DateTime.UtcNow;

			//Next the test run steps
			if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
			{
				foreach (RemoteTestRunStep remoteTestRunStep in remoteTestRun.TestRunSteps)
				{
					//Add a new test run step row
					TestRunStep testRunStep = new TestRunStep();
					testRunStep.ExecutionStatusId = remoteTestRunStep.ExecutionStatusId;
					testRunStep.Position = remoteTestRunStep.Position;
					testRunStep.Description = remoteTestRunStep.Description;
					testRunStep.TestCaseId = remoteTestRunStep.TestCaseId;
					testRunStep.TestStepId = remoteTestRunStep.TestStepId;
					testRunStep.ExpectedResult = remoteTestRunStep.ExpectedResult;
					testRunStep.SampleData = remoteTestRunStep.SampleData;
					testRunStep.ActualResult = remoteTestRunStep.ActualResult;
					testRun.TestRunSteps.Add(testRunStep);
				}
			}
		}

		/// <summary>
		/// Saves a single test run containing test run steps
		/// </summary>
		/// <param name="remoteTestRun">The test run object to persist</param>
		/// <param name="endDate">The effective end-date of the test run</param>
		/// <returns>The saved copy of the test run object (contains generated IDs)</returns>
		[
		WebMethod
			(
			Description = "Saves a single test run containing test run steps",
			EnableSession = true
			)
		]
		public RemoteTestRun TestRun_Save(RemoteTestRun remoteTestRun, DateTime endDate)
		{
			const string METHOD_NAME = "TestRun_Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Runs",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Instantiate the test run business class and dataset
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TestRunsPending testRunsPending = new TestRunsPending();

				//Populate the dataset from the test run object
				UpdateTestRunData(testRunsPending, remoteTestRun, projectId);

				//Update the status of the test run
				//We're only passed a single test run, so use index 0
				testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, 0, GlobalFunctions.UniversalizeDate(endDate), false);

				//Actually save the test run and get the primary keys
				testRunManager.Save(testRunsPending, projectId, false);
				int testRunsPendingId = testRunsPending.TestRunsPendingId;
				remoteTestRun.TestRunId = testRunsPending.TestRuns[0].TestRunId;

				//Need to now save the custom properties if necessary
				ArtifactCustomProperty artifactCustomProperty = null;
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, remoteTestRun.TestRunId.Value);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Finally we need to return the fully populated test run (after saving), it's easiest just to
				//regenerate from the test run dataset
				RemoteTestRun updatedRemoteTestRun = new RemoteTestRun();
				PopulateTestRun(updatedRemoteTestRun, testRunsPending.TestRuns[0]);

				//Also need to complete the pending run so that it is no longer displayed
				testRunManager.CompletePending(testRunsPendingId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return updatedRemoteTestRun;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Set Methods

		/// <summary>
		/// Creates a new test set folder in the system
		/// </summary>
		/// <param name="remoteTestSet">The new test set object (primary key will be empty)</param>
		/// <param name="parentTestSetFolderId">Do we want to insert the test set under a parent folder</param>
		/// <returns>The populated test set object - including the primary key</returns>
		/// <remarks>Test set folder ids are negative</remarks>
		[
		WebMethod
			(
			Description = "Creates a new test set folder in the system",
			EnableSession = true
			)
		]
		public RemoteTestSet TestSet_CreateFolder(RemoteTestSet remoteTestSet, Nullable<int> parentTestSetFolderId)
		{
			const string METHOD_NAME = "TestSet_CreateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test set id specified
			if (remoteTestSet.TestSetId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Set object needs to have TestSetId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Folders",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no creator provided
			if (remoteTestSet.CreatorId == -1 || remoteTestSet.CreatorId == 0)
			{
				remoteTestSet.CreatorId = userId;
			}
			//Default to the current project if not set
			if (remoteTestSet.ProjectId == 0 || remoteTestSet.ProjectId == -1)
			{
				remoteTestSet.ProjectId = projectId;
			}

			//Instantiate the test set business class
			TestSetManager testSetManager = new TestSetManager();

			//See if we have a passed in parent folder or not
			if (parentTestSetFolderId.HasValue)
			{
				//If the folder is negative, inverse
				if (parentTestSetFolderId.Value < 0)
				{
					parentTestSetFolderId = -parentTestSetFolderId.Value;
				}
			}

			//Now insert the test folder under the specified parent folder
			remoteTestSet.TestSetId = -testSetManager.TestSetFolder_Create(
				remoteTestSet.Name,
				projectId,
				remoteTestSet.Description,
				parentTestSetFolderId
				).TestSetFolderId;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSet;
		}

		/// <summary>
		/// Creates a new test set in the system
		/// </summary>
		/// <param name="remoteTestSet">The new test set object (primary key will be empty)</param>
		/// <param name="parentTestSetFolderId">Do we want to insert the test set under a parent folder</param>
		/// <returns>The populated test set object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new test set in the system",
			EnableSession = true
			)
		]
		public RemoteTestSet TestSet_Create(RemoteTestSet remoteTestSet, Nullable<int> parentTestSetFolderId)
		{
			const string METHOD_NAME = "TestSet_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test set id specified
			if (remoteTestSet.TestSetId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Test Set object needs to have TestSetId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Test Sets",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the authenticated user if we have no creator provided
			if (remoteTestSet.CreatorId == -1 || remoteTestSet.CreatorId == 0)
			{
				remoteTestSet.CreatorId = userId;
			}
			//Default to the current project if not set
			if (remoteTestSet.ProjectId == 0 || remoteTestSet.ProjectId == -1)
			{
				remoteTestSet.ProjectId = projectId;
			}

			//Instantiate the test set business class
			TestSetManager testSetManager = new TestSetManager();

			//See if we have a passed in parent folder or not
			if (parentTestSetFolderId.HasValue)
			{
				//If the folder is negative, inverse
				if (parentTestSetFolderId.Value < 0)
				{
					parentTestSetFolderId = -parentTestSetFolderId.Value;
				}
			}

			//Now insert the test set under the specified folder
			remoteTestSet.TestSetId = testSetManager.Insert(
				userId,
				projectId,
				parentTestSetFolderId,
				remoteTestSet.ReleaseId,
				remoteTestSet.CreatorId,
				remoteTestSet.OwnerId,
				(TestSet.TestSetStatusEnum)remoteTestSet.TestSetStatusId,
				remoteTestSet.Name,
				remoteTestSet.Description,
				GlobalFunctions.UniversalizeDate(remoteTestSet.PlannedDate),
				TestRun.TestRunTypeEnum.Manual,
				null,
				null
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			testSetManager.SendCreationNotification(remoteTestSet.TestSetId.Value, artifactCustomProperty, null);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSet;
		}

		/// <summary>
		/// Retrieves a list of test sets and folders in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of test sets and test set folders</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of testSets in the system that match the provided filter",
			EnableSession = true
			)
		]
		public List<RemoteTestSet> TestSet_Retrieve(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "TestSet_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testSets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View TestSets",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testSet dataset
			TestSetManager testSetManager = new TestSetManager();
			List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, startingRow, numberOfRows, filters, 0, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);

			//Get the complete folder list for the project
			List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);

			//All Items / Filtered List - we need to group under the appropriate folder
			List<IGrouping<int?, TestSetView>> groupedTestSetsByFolder = testSets.GroupBy(t => t.TestSetFolderId).ToList();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);

			//Populate the API data object and return
			//string runningIndentLevel = "AAA";  //Not used by this version of the API
			List<RemoteTestSet> remoteTestSets = new List<RemoteTestSet>();

			//Add the root-folder testSets (if any)
			IGrouping<int?, TestSetView> groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == null);
			if (groupedTestSets != null)
			{
				List<TestSetView> groupedTestSetList = groupedTestSets.ToList();
				if (groupedTestSetList != null && groupedTestSetList.Count > 0)
				{
					foreach (TestSetView testSet in groupedTestSetList)
					{
						//Create and populate the row
						RemoteTestSet remoteTestSet = new RemoteTestSet();
						PopulateTestSet(remoteTestSet, testSet);
						PopulateCustomProperties(remoteTestSet, testSet, customProperties);
						remoteTestSets.Add(remoteTestSet);

						//Increment the indent level -- Not used by this version of the API
						//runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
				}
				else
				{
					//No testSets at root, so folder is first child entry
					//runningIndentLevel = runningIndentLevel + "AAA";
				}
			}

			//Loop through the folders
			//int lastFolderLevel = 0;
			foreach (TestSetFolderHierarchyView testSetFolder in testSetFolders)
			{
				//See if this folder is a peer, child or above the last one
				if (testSetFolder.HierarchyLevel.HasValue)
				{
					/* -- not used by the v2.2 API
					if (testSetFolder.HierarchyLevel.Value > lastFolderLevel)
					{
						runningIndentLevel = runningIndentLevel + "AAA";
					}
					else if (testSetFolder.HierarchyLevel.Value == lastFolderLevel)
					{
						//Increment the indent level
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
					else
					{
						int numberOfLevels = lastFolderLevel - testSetFolder.HierarchyLevel.Value;
						runningIndentLevel = runningIndentLevel.SafeSubstring(0, runningIndentLevel.Length - (numberOfLevels * 3));
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
					lastFolderLevel = testSetFolder.HierarchyLevel.Value;*/
				}

				//Add the folder item
				RemoteTestSet remoteTestSet = new RemoteTestSet();
				PopulateTestSet(projectId, remoteTestSet, testSetFolder);
				remoteTestSets.Add(remoteTestSet);

				//Add the testSets (if any)
				groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == testSetFolder.TestSetFolderId);
				if (groupedTestSets != null)
				{
					List<TestSetView> groupedTestSetList = groupedTestSets.ToList();
					if (groupedTestSetList != null && groupedTestSetList.Count > 0)
					{
						foreach (TestSetView testSet in groupedTestSetList)
						{
							//Create and populate the row
							remoteTestSet = new RemoteTestSet();
							PopulateTestSet(remoteTestSet, testSet);
							PopulateCustomProperties(remoteTestSet, testSet, customProperties);
							remoteTestSets.Add(remoteTestSet);

							//Increment the indent level -- Not used by this version of the API
							//runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
						}
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSets;
		}

		/// <summary>
		/// Retrieves all test sets owned by the currently authenticated user
		/// </summary>
		/// <returns>List of testSets</returns>
		[
		WebMethod
			(
			Description = "Retrieves all test sets owned by the currently authenticated user",
			EnableSession = true
			)
		]
		public List<RemoteTestSet> TestSet_RetrieveForOwner()
		{
			const string METHOD_NAME = "TestSet_RetrieveForOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the testSet dataset
			TestSetManager testSetManager = new TestSetManager();
			List<TestSetView> testSets = testSetManager.RetrieveByOwnerId(userId, null);

			//Get the custom property definitions for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.TestSet);

			//Populate the API data object and return
			List<RemoteTestSet> remoteTestSets = new List<RemoteTestSet>();
			foreach (TestSetView testSet in testSets)
			{
				//Create and populate the row
				RemoteTestSet remoteTestSet = new RemoteTestSet();
				PopulateTestSet(remoteTestSet, testSet);
				PopulateCustomProperties(remoteTestSet, testSet, customProperties);
				remoteTestSets.Add(remoteTestSet);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSets;
		}

		/// <summary>
		/// Retrieves a single test set/folder in the system
		/// </summary>
		/// <param name="testSetId">The id of the test set/folder</param>
		/// <returns>Test Set object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single test set/folder in the system",
			EnableSession = true
			)
		]
		public RemoteTestSet TestSet_RetrieveById(int testSetId)
		{
			const string METHOD_NAME = "TestSet_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Test Sets",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the test set dataset
			TestSetManager testSetManager = new TestSetManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test set was not found, just return null
			try
			{
				//See if we have folder or test set
				RemoteTestSet remoteTestSet = null;
				if (testSetId > 0)
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

					//Populate the API data object and return
					remoteTestSet = new RemoteTestSet();
					PopulateTestSet(remoteTestSet, testSet);
					if (artifactCustomProperty != null)
					{
						PopulateCustomProperties(remoteTestSet, artifactCustomProperty);
					}
				}
				else
				{
					TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(-testSetId);

					//Populate the API data object and return
					remoteTestSet = new RemoteTestSet();
					PopulateTestSet(remoteTestSet, testSetFolder);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestSet;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test set");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates a test set in the system
		/// </summary>
		/// <param name="remoteTestSet">The updated test set object</param>
		[
		WebMethod
			(
			Description = "Updates a test set in the system",
			EnableSession = true
			)
		]
		public void TestSet_Update(RemoteTestSet remoteTestSet)
		{
			const string METHOD_NAME = "TestSet_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a testSet id specified
			if (!remoteTestSet.TestSetId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("TestSet object needs to have TestSetId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Sets",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				TestSetManager testSetManager = new TestSetManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//See if we have a test set or test set folder
				if (remoteTestSet.TestSetId < 0)
				{
					//Test Folder
					TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(-remoteTestSet.TestSetId.Value);

					//Make sure that the project ids match
					if (testSetFolder.ProjectId != projectId)
					{
						throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
							SoapException.ClientFaultCode,
							Context.Request.Url.AbsoluteUri);
					}
					//Update

					testSetFolder.StartTracking();
					testSetFolder.Name = remoteTestSet.Name;
					testSetFolder.Description = remoteTestSet.Description;
					testSetFolder.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestSet.LastUpdateDate);
					testSetManager.TestSetFolder_Update(testSetFolder);
				}
				else
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					TestSet testSet = testSetManager.RetrieveById2(projectId, remoteTestSet.TestSetId.Value);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestSet.TestSetId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

					//Make sure that the project ids match
					if (testSet.ProjectId != projectId)
					{
						throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
							SoapException.ClientFaultCode,
							Context.Request.Url.AbsoluteUri);
					}

					//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
					UpdateTestSetData(testSet, remoteTestSet);
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value);

					//Get copies of everything..
					Artifact notificationArt = testSet.Clone();
					ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

					//Call the business object to actually update the testSet dataset and the custom properties
					testSetManager.Update(testSet, userId);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Call notifications..
					try
					{
						new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + notificationArt.ArtifactToken);
					}
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test set");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Maps a test set to a test case, so that the test case is part of the test set
		/// </summary>
		/// <param name="remoteTestSetTestCaseMapping">The test set and test case mapping entry</param>
		/// <remarks>
		/// If the we are passed in the id of a test folder, we add all the child test cases,
		/// ignoring any duplicates.
		/// </remarks>
		[
		WebMethod
			(
			Description = "Maps a test set to a test case, so that the test case is part of the test set",
			EnableSession = true
			)
		]
		public void TestSet_AddTestMapping(RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping)
		{
			const string METHOD_NAME = "TestSet_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Now add the test case to the test set
				TestSetManager testSetManager = new TestSetManager();
				testSetManager.AddTestCase(projectId, remoteTestSetTestCaseMapping.TestSetId, remoteTestSetTestCaseMapping.TestCaseId, null, null, null);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Removes a test case from a test set
		/// </summary>
		/// <param name="remoteTestSetTestCaseMapping">The test set and test case mapping entry</param>
		[
		WebMethod
			(
			Description = "Removes a test case from a test set",
			EnableSession = true
			)
		]
		public void TestSet_RemoveTestMapping(RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping)
		{
			const string METHOD_NAME = "TestSet_RemoveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Test Cases",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			try
			{
				//Now remove the test case from the test set
				TestSetManager testSetManager = new TestSetManager();

				//The v2.2 api doesn't provide the unique TestSetTestCaseId so we simply remove all matching test cases
				//(previously test sets only allowed one instance of a test case per set)
				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(remoteTestSetTestCaseMapping.TestSetId);
				foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
				{
					if (testSetTestCase.TestCaseId == remoteTestSetTestCaseMapping.TestCaseId)
					{
						testSetManager.RemoveTestCase(projectId, remoteTestSetTestCaseMapping.TestSetId, testSetTestCase.TestSetTestCaseId);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestSet">The API data object</param>
		/// <param name="testSetView">The internal datarow</param>
		protected void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetView testSetView)
		{
			remoteTestSet.TestSetId = testSetView.TestSetId;
			remoteTestSet.ProjectId = testSetView.ProjectId;
			remoteTestSet.TestSetStatusId = testSetView.TestSetStatusId;
			remoteTestSet.CreatorId = testSetView.CreatorId;
			remoteTestSet.OwnerId = testSetView.OwnerId;
			remoteTestSet.ReleaseId = testSetView.ReleaseId;
			remoteTestSet.Name = testSetView.Name;
			remoteTestSet.Description = testSetView.Description;
			remoteTestSet.Folder = false;   //Separate object for folders now
			remoteTestSet.CreationDate = GlobalFunctions.LocalizeDate(testSetView.CreationDate);
			remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetView.LastUpdateDate);
			remoteTestSet.ExecutionDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)testSetView.ExecutionDate);
			remoteTestSet.PlannedDate = GlobalFunctions.LocalizeDate((Nullable<DateTime>)testSetView.PlannedDate);
			remoteTestSet.CountPassed = testSetView.CountPassed;
			remoteTestSet.CountFailed = testSetView.CountFailed;
			remoteTestSet.CountCaution = testSetView.CountCaution;
			remoteTestSet.CountBlocked = testSetView.CountBlocked;
			remoteTestSet.CountNotRun = testSetView.CountNotRun;
			remoteTestSet.CountNotApplicable = testSetView.CountNotApplicable;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetView.ConcurrencyDate);
		}

		/// <summary>
		/// Populates a data-sync system API object from a test folder (makes the id negative)
		/// </summary>
		/// <param name="remoteTestSet">The API data object</param>
		/// <param name="testSetFolder">The internal datarow</param>
		protected void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetFolder testSetFolder)
		{
			remoteTestSet.TestSetId = -testSetFolder.TestSetFolderId;
			remoteTestSet.ProjectId = testSetFolder.ProjectId;
			remoteTestSet.Name = testSetFolder.Name;
			remoteTestSet.Description = testSetFolder.Description;
			remoteTestSet.Folder = true;
			remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetFolder.LastUpdateDate);
			remoteTestSet.ExecutionDate = testSetFolder.ExecutionDate;
		}

		/// <summary>
		/// Populates a data-sync system API object from a test folder (makes the id negative)
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="remoteTestSet">The API data object</param>
		/// <param name="testSetFolder">The internal datarow</param>
		protected void PopulateTestSet(int projectId, RemoteTestSet remoteTestSet, TestSetFolderHierarchyView testSetFolder)
		{
			remoteTestSet.TestSetId = -testSetFolder.TestSetFolderId;
			remoteTestSet.ProjectId = projectId;
			remoteTestSet.Name = testSetFolder.Name;
			remoteTestSet.Folder = true;
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestSet">The API data object</param>
		/// <param name="testSet">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateTestSetData(TestSet testSet, RemoteTestSet remoteTestSet)
		{
			//The remote object uses LastUpdateDate for concurrency
			testSet.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteTestSet.LastUpdateDate);
			testSet.StartTracking();

			testSet.CreatorId = remoteTestSet.CreatorId;
			testSet.OwnerId = remoteTestSet.OwnerId;
			testSet.ReleaseId = remoteTestSet.ReleaseId;
			testSet.Name = remoteTestSet.Name;
			testSet.TestSetStatusId = remoteTestSet.TestSetStatusId;
			testSet.Description = remoteTestSet.Description;
			testSet.CreationDate = GlobalFunctions.UniversalizeDate(remoteTestSet.CreationDate);
			testSet.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestSet.LastUpdateDate);
			testSet.PlannedDate = GlobalFunctions.UniversalizeDate(remoteTestSet.PlannedDate);
		}

		#endregion

		#region Task Methods

		/// <summary>
		/// Retrieves all new tasks added in the system since the date specified
		/// </summary>
		/// <param name="creationDate">The date after which the task needs to have been created</param>
		/// <returns>List of tasks</returns>
		[
		WebMethod
			(
			Description = "Retrieves all new tasks added in the system since the date specified",
			EnableSession = true
			)
		]
		public List<RemoteTask> Task_RetrieveNew(DateTime creationDate)
		{
			const string METHOD_NAME = "Task_RetrieveNew";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Tasks",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			Hashtable filters = new Hashtable();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = GlobalFunctions.UniversalizeDate(creationDate);
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			List<TaskView> tasks = taskManager.Retrieve(projectId, "CreationDate", true, 1, Int32.MaxValue, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulateTask(remoteTask, task);
				PopulateCustomProperties(remoteTask, task, customProperties);
				remoteTasks.Add(remoteTask);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTasks;
		}

		/// <summary>
		/// Retrieves a list of tasks in the system that match the provided filter/sort
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of tasks</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of tasks in the system that match the provided filter/sort",
			EnableSession = true
			)
		]
		public List<RemoteTask> Task_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "Task_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("You need to provide a populated RemoteSort object as parameter",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Tasks",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulateTask(remoteTask, task);
				PopulateCustomProperties(remoteTask, task, customProperties);
				remoteTasks.Add(remoteTask);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTasks;
		}

		/// <summary>
		/// Retrieves all tasks owned by the currently authenticated user
		/// </summary>
		/// <returns>List of tasks</returns>
		[
		WebMethod
			(
			Description = "Retrieves all tasks owned by the currently authenticated user",
			EnableSession = true
			)
		]
		public List<RemoteTask> Task_RetrieveForOwner()
		{
			const string METHOD_NAME = "Task_RetrieveForOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.RetrieveByOwnerId(userId, null, null, false);

			//Get the custom property definitions for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Task);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulateTask(remoteTask, task);
				PopulateCustomProperties(remoteTask, task, customProperties);
				remoteTasks.Add(remoteTask);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTasks;
		}

		/// <summary>
		/// Retrieves a single task in the system
		/// </summary>
		/// <param name="taskId">The id of the task</param>
		/// <returns>Task object</returns>
		[
		WebMethod
			(
			Description = "Retrieves a single task in the system",
			EnableSession = true
			)
		]
		public RemoteTask Task_RetrieveById(int taskId)
		{
			const string METHOD_NAME = "Task_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to View Tasks",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();

			//If the task was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TaskView task = taskManager.TaskView_RetrieveById(taskId);
				ArtifactCustomProperty artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, true);

				//Populate the API data object and return
				RemoteTask remoteTask = new RemoteTask();
				PopulateTask(remoteTask, task);
				PopulateCustomProperties(remoteTask, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTask;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested task");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates an task in the system
		/// </summary>
		/// <param name="remoteTask">The updated task object</param>
		[
		WebMethod
			(
			Description = "Retrieves a single task in the system",
			EnableSession = true
			)
		]
		public void Task_Update(RemoteTask remoteTask)
		{
			const string METHOD_NAME = "Task_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an task id specified
			if (!remoteTask.TaskId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Task object needs to have TaskId populated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Modify Tasks",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				Task task = taskManager.RetrieveById(remoteTask.TaskId.Value);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTask.TaskId.Value, DataModel.Artifact.ArtifactTypeEnum.Task, true);

				//Make sure that the project ids match
				if (task.ProjectId != projectId)
				{
					throw new SoapException(Resources.Messages.Services_ItemNotBelongToProject,
						SoapException.ClientFaultCode,
						Context.Request.Url.AbsoluteUri);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateTaskData(projectTemplateId, task, remoteTask);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value);

				//Get copies of everything..
				Artifact notificationArt = task.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Call the business object to actually update the task dataset and the custom properties
				taskManager.Update(task, userId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Task #" + task.TaskId + ".");
				}
			}
			catch (DataValidationException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested task");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Creates a new task in the system
		/// </summary>
		/// <param name="remoteTask">The new task object (primary key will be empty)</param>
		/// <returns>The populated task object - including the primary key</returns>
		[
		WebMethod
			(
			Description = "Creates a new task in the system",
			EnableSession = true
			)
		]
		public RemoteTask Task_Create(RemoteTask remoteTask)
		{
			const string METHOD_NAME = "Task_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an task id specified
			if (remoteTask.TaskId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Task object needs to have TaskId set to null for creation",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Authorized to Create Tasks",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Default to the current project if not set
			if (remoteTask.ProjectId == 0 || remoteTask.ProjectId == -1)
			{
				remoteTask.ProjectId = projectId;
			}

			//Convert %complete into remaining effort value
			Nullable<int> remainingEffort = null;
			if (remoteTask.EstimatedEffort.HasValue)
			{
				remainingEffort = remoteTask.EstimatedEffort.Value * (100 - remoteTask.CompletionPercent);
				remainingEffort /= 100;
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTask.TaskPriorityId >= 1 && remoteTask.TaskPriorityId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteTask);
			}

			//First insert the new task record itself, capturing and populating the id
			TaskManager taskManager = new TaskManager();
			remoteTask.TaskId = taskManager.Insert(
				projectId,
				userId,
				(Task.TaskStatusEnum)remoteTask.TaskStatusId,
				null,
				null,
				remoteTask.RequirementId,
				remoteTask.ReleaseId,
				remoteTask.OwnerId,
				remoteTask.TaskPriorityId,
				remoteTask.Name,
				remoteTask.Description,
				GlobalFunctions.UniversalizeDate(remoteTask.StartDate),
				GlobalFunctions.UniversalizeDate(remoteTask.EndDate),
				remoteTask.EstimatedEffort,
				remoteTask.ActualEffort,
				remainingEffort,
				userId
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			taskManager.SendCreationNotification(remoteTask.TaskId.Value, artifactCustomProperty, null);

			//Finally return the populated task object
			return remoteTask;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTask">The API data object</param>
		/// <param name="task">The internal entity</param>
		protected void PopulateTask(RemoteTask remoteTask, TaskView task)
		{
			remoteTask.TaskId = task.TaskId;
			remoteTask.ProjectId = task.ProjectId;
			remoteTask.TaskStatusId = task.TaskStatusId;
			remoteTask.RequirementId = task.RequirementId;
			remoteTask.ReleaseId = task.ReleaseId;
			remoteTask.OwnerId = task.OwnerId;
			remoteTask.TaskPriorityId = task.TaskPriorityId;
			remoteTask.Name = task.Name;
			remoteTask.Description = task.Description;
			remoteTask.CreationDate = GlobalFunctions.LocalizeDate(task.CreationDate);
			remoteTask.LastUpdateDate = GlobalFunctions.LocalizeDate(task.LastUpdateDate);
			remoteTask.StartDate = GlobalFunctions.LocalizeDate(task.StartDate);
			remoteTask.EndDate = GlobalFunctions.LocalizeDate(task.EndDate);
			remoteTask.CompletionPercent = task.CompletionPercent;
			remoteTask.EstimatedEffort = task.EstimatedEffort;
			remoteTask.ActualEffort = task.ActualEffort;
			remoteTask.TaskStatusName = task.TaskStatusName;
			remoteTask.OwnerName = task.OwnerName;
			remoteTask.TaskPriorityName = task.TaskPriorityName;
			remoteTask.ProjectName = task.ProjectName;
			remoteTask.ReleaseVersionNumber = task.ReleaseVersionNumber;

			//The remote object uses LastUpdateDate for concurrency, so need to override the value
			remoteTask.LastUpdateDate = GlobalFunctions.LocalizeDate(task.ConcurrencyDate);
		}

		/// <summary>
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template</param>
		/// <param name="remoteTask">The API data object</param>
		/// <param name="task">The internal entity</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateTaskData(int projectTemplateId, Task task, RemoteTask remoteTask)
		{
			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTask.TaskPriorityId >= 1 && remoteTask.TaskPriorityId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteTask);
			}

			//We first need to update the concurrency information before we start tracking
			//The remote object uses LastUpdateDate for concurrency
			task.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteTask.LastUpdateDate);

			task.StartTracking();
			task.TaskStatusId = remoteTask.TaskStatusId;
			task.RequirementId = remoteTask.RequirementId;
			task.ReleaseId = remoteTask.ReleaseId;
			task.OwnerId = remoteTask.OwnerId;
			task.TaskPriorityId = remoteTask.TaskPriorityId;
			task.Name = remoteTask.Name;
			task.Description = remoteTask.Description;
			task.CreationDate = GlobalFunctions.UniversalizeDate(remoteTask.CreationDate);
			task.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTask.LastUpdateDate);
			task.StartDate = GlobalFunctions.UniversalizeDate(remoteTask.StartDate);
			task.EndDate = GlobalFunctions.UniversalizeDate(remoteTask.EndDate);
			task.EstimatedEffort = remoteTask.EstimatedEffort;
			task.ActualEffort = remoteTask.ActualEffort;

			//Convert %complete into remaining effort value
			if (remoteTask.EstimatedEffort.HasValue)
			{
				int remainingEffort = remoteTask.EstimatedEffort.Value * (100 - remoteTask.CompletionPercent);
				remainingEffort /= 100;
				task.RemainingEffort = remainingEffort;
			}
			else
			{
				task.RemainingEffort = null;
			}
		}

		#endregion

		#region Artifact Generic Methods

		/// <summary>
		/// Populates the custom property entity from the API data object
		/// </summary>
		/// <param name="remoteIncident">The API data object</param>
		/// <param name="artifactCustomProperty">The custom property entity (reference)</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="projectId">The current project</param>
		protected void UpdateCustomPropertyData(ref ArtifactCustomProperty artifactCustomProperty, RemoteArtifact remoteArtifact, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
		{
			//First see if we have any custom properties set
			bool hasValues = false;
			if (!String.IsNullOrEmpty(remoteArtifact.Text01))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text02))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text03))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text04))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text05))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text06))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text07))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text08))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text09))
			{
				hasValues = true;
			}
			if (!String.IsNullOrEmpty(remoteArtifact.Text10))
			{
				hasValues = true;
			}
			if (remoteArtifact.List01.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List02.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List03.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List04.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List05.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List06.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List07.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List08.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List09.HasValue)
			{
				hasValues = true;
			}
			if (remoteArtifact.List10.HasValue)
			{
				hasValues = true;
			}

			//Create an entity if we need to
			if (artifactCustomProperty == null)
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false, false);
				artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, artifactType, artifactId, customProperties);
				artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
			}
			else
			{
				artifactCustomProperty.StartTracking();
			}

			//Handle the cases where we have values or not separately
			if (hasValues)
			{
				//Now populate the data
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_01", remoteArtifact.Text01);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_02", remoteArtifact.Text02);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_03", remoteArtifact.Text03);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_04", remoteArtifact.Text04);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_05", remoteArtifact.Text05);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_06", remoteArtifact.Text06);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_07", remoteArtifact.Text07);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_08", remoteArtifact.Text08);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_09", remoteArtifact.Text09);
				artifactCustomProperty.SetLegacyCustomProperty("TEXT_10", remoteArtifact.Text10);

				artifactCustomProperty.SetLegacyCustomProperty("LIST_01", remoteArtifact.List01);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_02", remoteArtifact.List02);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_03", remoteArtifact.List03);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_04", remoteArtifact.List04);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_05", remoteArtifact.List05);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_06", remoteArtifact.List06);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_07", remoteArtifact.List07);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_08", remoteArtifact.List08);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_09", remoteArtifact.List09);
				artifactCustomProperty.SetLegacyCustomProperty("LIST_10", remoteArtifact.List10);
			}
		}

		/// <summary>
		/// Converts the API filter object into the kinds that can be consumed internally
		/// </summary>
		/// <param name="remoteFilters">The list of API filters</param>
		/// <returns>The populated Hashtable of internal filters</returns>
		protected Hashtable PopulateFilters(List<RemoteFilter> remoteFilters)
		{
			//Handle the null case
			if (remoteFilters == null)
			{
				return null;
			}
			Hashtable filters = new Hashtable();
			foreach (RemoteFilter remoteFilter in remoteFilters)
			{
				if (!String.IsNullOrEmpty(remoteFilter.PropertyName))
				{
					//See what type we have and populate accordingly
					if (remoteFilter.IntValue.HasValue)
					{
						filters.Add(remoteFilter.PropertyName, remoteFilter.IntValue.Value);
					}
					else if (!String.IsNullOrEmpty(remoteFilter.StringValue))
					{
						filters.Add(remoteFilter.PropertyName, remoteFilter.StringValue);
					}
					else if (remoteFilter.MultiValue != null)
					{
						filters.Add(remoteFilter.PropertyName, remoteFilter.MultiValue);
					}
					else if (remoteFilter.DateRangeValue != null)
					{
						remoteFilter.DateRangeValue.StartDate = GlobalFunctions.UniversalizeDate(remoteFilter.DateRangeValue.StartDate);
						remoteFilter.DateRangeValue.EndDate = GlobalFunctions.UniversalizeDate(remoteFilter.DateRangeValue.EndDate);
						filters.Add(remoteFilter.PropertyName, remoteFilter.DateRangeValue);
					}
				}
			}
			return filters;
		}

		/// <summary>
		/// Populates a data-sync system API object's custom properties from the internal datarow
		/// </summary>
		/// <param name="remoteArtifact">The API data object</param>
		/// <param name="artifactCustomProperty">The internal custom property entity</param>
		/// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
		protected void PopulateCustomProperties(RemoteArtifact remoteArtifact, ArtifactCustomProperty artifactCustomProperty)
		{
			if (artifactCustomProperty != null)
			{
				remoteArtifact.Text01 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_01");
				remoteArtifact.Text02 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_02");
				remoteArtifact.Text03 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_03");
				remoteArtifact.Text04 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_04");
				remoteArtifact.Text05 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_05");
				remoteArtifact.Text06 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_06");
				remoteArtifact.Text07 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_07");
				remoteArtifact.Text08 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_08");
				remoteArtifact.Text09 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_09");
				remoteArtifact.Text10 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_10");

				remoteArtifact.List01 = artifactCustomProperty.LegacyCustomListProperty("LIST_01");
				remoteArtifact.List02 = artifactCustomProperty.LegacyCustomListProperty("LIST_02");
				remoteArtifact.List03 = artifactCustomProperty.LegacyCustomListProperty("LIST_03");
				remoteArtifact.List04 = artifactCustomProperty.LegacyCustomListProperty("LIST_04");
				remoteArtifact.List05 = artifactCustomProperty.LegacyCustomListProperty("LIST_05");
				remoteArtifact.List06 = artifactCustomProperty.LegacyCustomListProperty("LIST_06");
				remoteArtifact.List07 = artifactCustomProperty.LegacyCustomListProperty("LIST_07");
				remoteArtifact.List08 = artifactCustomProperty.LegacyCustomListProperty("LIST_08");
				remoteArtifact.List09 = artifactCustomProperty.LegacyCustomListProperty("LIST_09");
				remoteArtifact.List10 = artifactCustomProperty.LegacyCustomListProperty("LIST_10");
			}
		}

		/// <summary>
		/// Gets the value for a specific text custom field
		/// </summary>
		/// <param name="dataRow">The artifact datarow</param>
		/// <param name="legacyName">The legacy custom property field name ("TEXT_01")</param>
		/// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
		/// <returns>The string value or null</returns>
		protected string GetCustomTextPropertyValueFromDataRow(DataRow dataRow, string legacyName, List<CustomProperty> customPropertyDefinitions)
		{
			//Find the custom property that uses this legacy name
			CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text);
			if (customProperty != null)
			{
				if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
				{
					return null;
				}
				else
				{
					return (string)dataRow[customProperty.CustomPropertyFieldName];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the value for a specific text custom field
		/// </summary>
		/// <param name="artifact">The artifact enity</param>
		/// <param name="legacyName">The legacy custom property field name ("TEXT_01")</param>
		/// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
		/// <returns>The string value or null</returns>
		protected string GetCustomTextPropertyValueFromArtifact(Artifact artifact, string legacyName, List<CustomProperty> customPropertyDefinitions)
		{
			//Find the custom property that uses this legacy name
			CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text);
			if (customProperty != null)
			{
				if (!artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
				{
					return null;
				}
				else
				{
					return (string)artifact[customProperty.CustomPropertyFieldName];
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the value for a specific list custom field
		/// </summary>
		/// <param name="dataRow">The artifact datarow</param>
		/// <param name="legacyName">The legacy custom property field name ("LIST_01")</param>
		/// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
		/// <returns>The string value or null</returns>
		protected int? GetCustomListPropertyValueFromDataRow(DataRow dataRow, string legacyName, List<CustomProperty> customPropertyDefinitions)
		{
			//Find the custom property that uses this legacy name
			CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List);
			if (customProperty != null)
			{
				if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
				{
					return null;
				}
				else
				{
					string serializedValue = (string)dataRow[customProperty.CustomPropertyFieldName];
					int? value = serializedValue.FromDatabaseSerialization_Int32();
					return value;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the value for a specific list custom field
		/// </summary>
		/// <param name="artifact">The artifact enity</param>
		/// <param name="legacyName">The legacy custom property field name ("LIST_01")</param>
		/// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
		/// <returns>The string value or null</returns>
		protected int? GetCustomListPropertyValueFromArtifact(Artifact artifact, string legacyName, List<CustomProperty> customPropertyDefinitions)
		{
			//Find the custom property that uses this legacy name
			CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List);
			if (customProperty != null)
			{
				if (!artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
				{
					return null;
				}
				else
				{
					string serializedValue = (string)artifact[customProperty.CustomPropertyFieldName];
					int? value = serializedValue.FromDatabaseSerialization_Int32();
					return value;
				}
			}
			return null;
		}

		/// <summary>
		/// Populates a data-sync system API object's custom properties from the internal datarow
		/// </summary>
		/// <param name="remoteArtifact">The API data object</param>
		/// <param name="dataRow">The internal artifact data row (that also contains associated custom properties)</param>
		/// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
		/// <param name="customPropertyDefinitions">The list of custom property definitions for this artifact type</param>
		protected void PopulateCustomProperties(RemoteArtifact remoteArtifact, DataRow dataRow, List<CustomProperty> customPropertyDefinitions)
		{
			remoteArtifact.Text01 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_01", customPropertyDefinitions);
			remoteArtifact.Text02 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_02", customPropertyDefinitions);
			remoteArtifact.Text03 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_03", customPropertyDefinitions);
			remoteArtifact.Text04 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_04", customPropertyDefinitions);
			remoteArtifact.Text05 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_05", customPropertyDefinitions);
			remoteArtifact.Text06 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_06", customPropertyDefinitions);
			remoteArtifact.Text07 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_07", customPropertyDefinitions);
			remoteArtifact.Text08 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_08", customPropertyDefinitions);
			remoteArtifact.Text09 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_09", customPropertyDefinitions);
			remoteArtifact.Text10 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_10", customPropertyDefinitions);

			remoteArtifact.List01 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_01", customPropertyDefinitions);
			remoteArtifact.List02 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_02", customPropertyDefinitions);
			remoteArtifact.List03 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_03", customPropertyDefinitions);
			remoteArtifact.List04 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_04", customPropertyDefinitions);
			remoteArtifact.List05 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_05", customPropertyDefinitions);
			remoteArtifact.List06 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_06", customPropertyDefinitions);
			remoteArtifact.List07 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_07", customPropertyDefinitions);
			remoteArtifact.List08 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_08", customPropertyDefinitions);
			remoteArtifact.List09 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_09", customPropertyDefinitions);
			remoteArtifact.List10 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_10", customPropertyDefinitions);
		}

		/// <summary>
		/// Populates a data-sync system API object's custom properties from an artifact entity
		/// </summary>
		/// <param name="remoteArtifact">The API data object</param>
		/// <param name="artifact">The internal artifact entity (that also contains associated custom properties)</param>
		/// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
		/// <param name="customPropertyDefinitions">The list of custom property definitions for this artifact type</param>
		protected void PopulateCustomProperties(RemoteArtifact remoteArtifact, Artifact artifact, List<CustomProperty> customPropertyDefinitions)
		{
			remoteArtifact.Text01 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_01", customPropertyDefinitions);
			remoteArtifact.Text02 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_02", customPropertyDefinitions);
			remoteArtifact.Text03 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_03", customPropertyDefinitions);
			remoteArtifact.Text04 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_04", customPropertyDefinitions);
			remoteArtifact.Text05 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_05", customPropertyDefinitions);
			remoteArtifact.Text06 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_06", customPropertyDefinitions);
			remoteArtifact.Text07 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_07", customPropertyDefinitions);
			remoteArtifact.Text08 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_08", customPropertyDefinitions);
			remoteArtifact.Text09 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_09", customPropertyDefinitions);
			remoteArtifact.Text10 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_10", customPropertyDefinitions);

			remoteArtifact.List01 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_01", customPropertyDefinitions);
			remoteArtifact.List02 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_02", customPropertyDefinitions);
			remoteArtifact.List03 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_03", customPropertyDefinitions);
			remoteArtifact.List04 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_04", customPropertyDefinitions);
			remoteArtifact.List05 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_05", customPropertyDefinitions);
			remoteArtifact.List06 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_06", customPropertyDefinitions);
			remoteArtifact.List07 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_07", customPropertyDefinitions);
			remoteArtifact.List08 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_08", customPropertyDefinitions);
			remoteArtifact.List09 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_09", customPropertyDefinitions);
			remoteArtifact.List10 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_10", customPropertyDefinitions);
		}

		#endregion

		#region Data Mapping Methods

		/// <summary>
		/// Retrieves a list of data mappings for artifact field values
		/// </summary>
		/// <param name="artifactFieldId">The field we're interested in</param>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of data mappings for artifact field values",
			EnableSession = true
			)
		]
		public List<RemoteDataMapping> DataMapping_RetrieveFieldValueMappings(int dataSyncSystemId, int artifactFieldId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the list of artifact field value mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncFieldValueMappingView> dataMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, artifactFieldId, false);

			//Populate the API data object and return
			List<RemoteDataMapping> remoteDataMappings = new List<RemoteDataMapping>();
			foreach (DataSyncFieldValueMappingView fieldMapping in dataMappings)
			{
				//Create and populate the row
				RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
				remoteDataMapping.InternalId = fieldMapping.ArtifactFieldValue.Value;
				remoteDataMapping.ProjectId = fieldMapping.ProjectId;
				remoteDataMapping.ExternalKey = fieldMapping.ExternalKey;
				remoteDataMapping.Primary = (fieldMapping.PrimaryYn == "Y");
				remoteDataMappings.Add(remoteDataMapping);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMappings;
		}

		/// <summary>
		/// Retrieves the data mapping for a custom property
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <param name="artifactTypeId">The id of the type of artifact</param>
		/// <param name="customPropertyId">The id of the custom property</param>
		/// <returns>Data mapping object</returns>
		[
		WebMethod
			(
			Description = "Retrieves the data mapping for a custom property",
			EnableSession = true
			)
		]
		public RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(int dataSyncSystemId, int artifactTypeId, int customPropertyId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveCustomPropertyMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the custom property mapping
			DataMappingManager dataMappingManager = new DataMappingManager();
			DataSyncCustomPropertyMapping dataMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, customPropertyId);

			//Populate the API data object and return
			if (dataMapping == null)
			{
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return null;
			}
			RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
			remoteDataMapping.InternalId = dataMapping.CustomPropertyId;
			remoteDataMapping.ProjectId = dataMapping.ProjectId;
			remoteDataMapping.ExternalKey = dataMapping.ExternalKey;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMapping;
		}

		/// <summary>
		/// Retrieves a list of data mappings for custom property values
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <param name="artifactTypeId">The id of the type of artifact</param>
		/// <param name="customPropertyId">The id of the custom property that the values are for</param>
		/// <returns>The list of data mappings</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of data mappings for custom property values",
			EnableSession = true
			)
		]
		public List<RemoteDataMapping> DataMapping_RetrieveCustomPropertyValueMappings(int dataSyncSystemId, int artifactTypeId, int customPropertyId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the list of custom property value mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncCustomPropertyValueMappingView> dataMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, customPropertyId, false);

			//Populate the API data object and return
			List<RemoteDataMapping> remoteDataMappings = new List<RemoteDataMapping>();
			foreach (DataSyncCustomPropertyValueMappingView dataRow in dataMappings)
			{
				//Create and populate the row
				RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
				remoteDataMapping.InternalId = dataRow.CustomPropertyValueId;
				remoteDataMapping.ProjectId = dataRow.ProjectId;
				remoteDataMapping.ExternalKey = dataRow.ExternalKey;
				remoteDataMappings.Add(remoteDataMapping);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMappings;
		}

		/// <summary>
		/// Retrieves a list of data mappings for projecs in the system
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of data mappings for projects in the system",
			EnableSession = true
			)
		]
		public List<RemoteDataMapping> DataMapping_RetrieveProjectMappings(int dataSyncSystemId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveProjectMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Get the list of project mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncProject> dataMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId);

			//Populate the API data object and return
			List<RemoteDataMapping> remoteDataMappings = new List<RemoteDataMapping>();
			foreach (DataSyncProject dataRow in dataMappings)
			{
				//Create and populate the row
				RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
				remoteDataMapping.InternalId = dataRow.ProjectId;
				remoteDataMapping.ProjectId = dataRow.ProjectId;
				remoteDataMapping.ExternalKey = dataRow.ExternalKey;
				remoteDataMappings.Add(remoteDataMapping);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMappings;
		}

		/// <summary>
		/// Retrieves a list of data mappings for users in the system
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of data mappings for users in the system",
			EnableSession = true
			)
		]
		public List<RemoteDataMapping> DataMapping_RetrieveUserMappings(int dataSyncSystemId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Get the list of user mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncUserMapping> dataMappings = dataMappingManager.RetrieveDataSyncUserMappingsForSystem(dataSyncSystemId);

			//Populate the API data object and return
			List<RemoteDataMapping> remoteDataMappings = new List<RemoteDataMapping>();
			foreach (DataSyncUserMapping dataRow in dataMappings)
			{
				//Create and populate the row
				RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
				remoteDataMapping.InternalId = dataRow.UserId;
				remoteDataMapping.ExternalKey = dataRow.ExternalKey;
				remoteDataMappings.Add(remoteDataMapping);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMappings;
		}

		/// <summary>
		/// Adds new user data mapping entries
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="remoteDataMappings">The list of mapping entries to add</param>
		[
		WebMethod
			(
			Description = "Adds new user data mapping entries",
			EnableSession = true
			)
		]
		public void DataMapping_AddUserMappings(int dataSyncSystemId, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_AddUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Get the list of existing user mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncUserMapping> dataMappings = dataMappingManager.RetrieveDataSyncUserMappingsForSystem(dataSyncSystemId);

			//Now iterate through the provided list of mappings and add any new ones (ignore duplicates)
			foreach (RemoteDataMapping remoteDataMapping in remoteDataMappings)
			{
				//See if we already have the mapping
				DataSyncUserMapping dataRow = dataMappings.FirstOrDefault(d => d.UserId == remoteDataMapping.InternalId);
				if (dataRow == null)
				{
					DataSyncUserMapping newMapping = new DataSyncUserMapping();
					newMapping.MarkAsAdded();
					newMapping.DataSyncSystemId = dataSyncSystemId;
					newMapping.UserId = remoteDataMapping.InternalId;
					newMapping.ExternalKey = remoteDataMapping.ExternalKey;
					dataMappings.Add(newMapping);
				}
			}

			//Save the changes
			dataMappingManager.SaveDataSyncUserMappings(dataMappings);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}


		/// <summary>
		/// Retrieves a list of data mappings for artifact ids in the system
		/// </summary>
		/// <param name="artifactTypeId">The type of artifact we're interested in</param>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of data mappings for artifact ids in the system",
			EnableSession = true
			)
		]
		public List<RemoteDataMapping> DataMapping_RetrieveArtifactMappings(int dataSyncSystemId, int artifactTypeId)
		{
			const string METHOD_NAME = "DataMapping_RetrieveArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the list of artifact id mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncArtifactMapping> dataMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(dataSyncSystemId, projectId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

			//Populate the API data object and return
			List<RemoteDataMapping> remoteDataMappings = new List<RemoteDataMapping>();
			foreach (DataSyncArtifactMapping dataRow in dataMappings)
			{
				//Create and populate the row
				RemoteDataMapping remoteDataMapping = new RemoteDataMapping();
				remoteDataMapping.InternalId = dataRow.ArtifactId;
				remoteDataMapping.ProjectId = dataRow.ProjectId;
				remoteDataMapping.ExternalKey = dataRow.ExternalKey;
				remoteDataMappings.Add(remoteDataMapping);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataMappings;
		}

		/// <summary>
		/// Adds new artifact data mapping entries
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="artifactTypeId">The type of artifact the mappings are for</param>
		/// <param name="remoteDataMappings">The list of mapping entries to add</param>
		[
		WebMethod
			(
			Description = "Adds new artifact data mapping entries",
			EnableSession = true
			)
		]
		public void DataMapping_AddArtifactMappings(int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_AddArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the list of existing artifact id mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncArtifactMapping> dataMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(dataSyncSystemId, projectId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

			//Now iterate through the provided list of mappings and add any new ones (ignore duplicates)
			foreach (RemoteDataMapping remoteDataMapping in remoteDataMappings)
			{
				//See if we already have the mapping
				DataSyncArtifactMapping dataRow = dataMappings.FirstOrDefault(d => d.ArtifactId == remoteDataMapping.InternalId);
				if (dataRow == null)
				{
					DataSyncArtifactMapping newMapping = new DataSyncArtifactMapping();
					newMapping.MarkAsAdded();
					newMapping.DataSyncSystemId = dataSyncSystemId;
					newMapping.ProjectId = projectId;
					newMapping.ArtifactTypeId = artifactTypeId;
					newMapping.ArtifactId = remoteDataMapping.InternalId;
					newMapping.ExternalKey = remoteDataMapping.ExternalKey;
					dataMappings.Add(newMapping);
				}
			}

			//Save the changes
			dataMappingManager.SaveDataSyncArtifactMappings(dataMappings);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Removes existing artifact data mapping entries
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="artifactTypeId">The type of artifact the mappings are for</param>
		/// <param name="remoteDataMappings">The list of mapping entries to remove</param>
		[
		WebMethod
			(
			Description = "Removes existing artifact data mapping entries",
			EnableSession = true
			)
		]
		public void DataMapping_RemoveArtifactMappings(int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_RemoveArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the list of existing artifact id mappings
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncArtifactMapping> dataMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(dataSyncSystemId, projectId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

			//Now iterate through the provided list of mappings and remove any old ones (ignore it if they've been already deleted)
			foreach (RemoteDataMapping remoteDataMapping in remoteDataMappings)
			{
				//See if we already have the mapping
				DataSyncArtifactMapping dataRow = dataMappings.FirstOrDefault(d => d.ArtifactId == remoteDataMapping.InternalId);
				if (dataRow != null)
				{
					//To remove the mapping, just set the external key to null
					dataRow.StartTracking();
					dataRow.ExternalKey = null;
				}
			}

			//Save the changes
			dataMappingManager.SaveDataSyncArtifactMappings(dataMappings);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		#region Custom Property Methods

		/// <summary>
		/// Retrieves the list of custom properties configure for the current project
		/// </summary>
		/// <param name="artifactTypeId">The id of the type of artifact</param>
		/// <returns>Data mapping object</returns>
		[
		WebMethod
			(
			Description = "Retrieves the list of custom properties configure for the current project",
			EnableSession = true
			)
		]
		public List<RemoteCustomProperty> CustomProperty_RetrieveProjectProperties(int artifactTypeId)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveProjectProperties";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = AuthorizedProject;

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property list for the project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, false, false);

			//Populate the API data objects and return
			List<RemoteCustomProperty> remoteCustomProperties = new List<RemoteCustomProperty>();
			foreach (CustomProperty customProperty in customProperties)
			{
				//We only return the pre-v4.0 legacy properies
				if (!String.IsNullOrEmpty(customProperty.LegacyName) && (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List))
				{
					//Create and populate the row
					RemoteCustomProperty remoteCustomProperty = new RemoteCustomProperty();
					remoteCustomProperty.CustomPropertyId = customProperty.CustomPropertyId;
					remoteCustomProperty.ProjectId = projectId;
					remoteCustomProperty.ArtifactTypeId = customProperty.ArtifactTypeId;
					remoteCustomProperty.Alias = customProperty.Name;
					remoteCustomProperty.CustomPropertyListId = customProperty.CustomPropertyListId;
					remoteCustomProperty.CustomPropertyName = customProperty.LegacyName;
					//Need to convert to 1=Text,2=List
					remoteCustomProperty.CustomPropertyTypeId = (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text) ? 1 : 2;
					remoteCustomProperty.CustomPropertyTypeName = customProperty.CustomPropertyTypeName;
					remoteCustomProperties.Add(remoteCustomProperty);
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteCustomProperties;
		}

		#endregion

		#region System methods

		/// <summary>
		/// Retrieves the version number of the current installation.
		/// </summary>
		/// <returns>A RemoteVersion data object.</returns>
		[
			WebMethod
			(
			Description = "Retrieves the version number of the current installation.",
			EnableSession = true
			)
		]
		public RemoteVersion System_GetProductVersion()
		{
			const string METHOD_NAME = "System_GetProductVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			RemoteVersion returnValues = new RemoteVersion();

			returnValues.Version = GlobalFunctions.DISPLAY_SOFTWARE_VERSION + "." + GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD;
			returnValues.Patch = GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return returnValues;
		}

		/// <summary>
		/// Returns the current configuration settings for the installation.
		/// </summary>
		/// <returns>List of remote settings objects (name, value)</returns>
		/// <remarks>
		/// To maintain backwards compatibility with Spira v3.1 and earlier, we also return
		/// the old setting names/values for the Rich-Text enabled settings
		/// </remarks>
		[
		WebMethod
			(
			Description = "Returns the current configuration settings for the installation.",
			EnableSession = true
			)
		]
		public List<RemoteSetting> System_GetSettings()
		{
			const string METHOD_NAME = "System_GetSettings";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}

			List<RemoteSetting> returnList = new List<RemoteSetting>();

			//Get the list of settings from the provider
			System.Configuration.SettingsPropertyValueCollection settingsPropValueCol = Common.ConfigurationSettings.Default.PropertyValues;

			//Loop through the settings and remove any password ones that may contain sensitive information
			foreach (System.Configuration.SettingsPropertyValue settingsValue in settingsPropValueCol)
			{
				string propName = settingsValue.Name;
				if (!propName.ToLowerInvariant().Contains("password") && !propName.ToLowerInvariant().Contains("passwd"))
				{
					RemoteSetting remoteSetting = new RemoteSetting();
					remoteSetting.Name = propName;
					remoteSetting.Value = (string)settingsValue.PropertyValue.ToString();
					returnList.Add(remoteSetting);
				}
			}

			//Also add the two legacy Spira v3.1 (and earlier) property values for rich text configuration
			//as some existing applications will be expecting them
			string richTextArtifactDesc = "Y";
			RemoteSetting remoteSetting1 = new RemoteSetting();
			remoteSetting1.Name = "SpiraTest.RichTextArtifactDesc";
			remoteSetting1.Value = richTextArtifactDesc;
			returnList.Add(remoteSetting1);

			string richTextTestSteps = (ConfigurationSettings.Default.General_RichTextTestSteps) ? "Y" : "N";
			RemoteSetting remoteSetting2 = new RemoteSetting();
			remoteSetting2.Name = "SpiraTest.RichTextTestSteps";
			remoteSetting2.Value = richTextTestSteps;
			returnList.Add(remoteSetting2);

			return returnList;
		}

		#endregion

		#region Workflow Methods
		#endregion

		#region Data Translation Methods

		/// <summary>
		/// Converts a requirement static ID to be template-based
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="remoteRequirement">The requirement object</param>
		public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteRequirement remoteRequirement)
		{
			//First importance
			RequirementManager requirementManager = new RequirementManager();
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			Importance importance = importances.FirstOrDefault(p => p.Score == remoteRequirement.ImportanceId || p.ImportanceId == remoteRequirement.ImportanceId);
			if (importance == null)
			{
				//No match, so leave blank
				remoteRequirement.ImportanceId = null;
			}
			else
			{
				remoteRequirement.ImportanceId = importance.ImportanceId;
			}
		}

		/// <summary>
		/// Converts a task static ID to be template-based
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="remoteTask">The task object</param>
		public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteTask remoteTask)
		{
			//First priority
			TaskManager taskManager = new TaskManager();
			List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			TaskPriority priority = priorities.FirstOrDefault(p => p.Score == remoteTask.TaskPriorityId || p.TaskPriorityId == remoteTask.TaskPriorityId);
			if (priority == null)
			{
				//No match, so leave blank
				remoteTask.TaskPriorityId = null;
			}
			else
			{
				remoteTask.TaskPriorityId = priority.TaskPriorityId;
			}
		}

		/// <summary>
		/// Converts a task static ID to be template-based
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="remoteTestCase">The task object</param>
		public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteTestCase remoteTestCase)
		{
			//First priority
			TestCaseManager taskManager = new TestCaseManager();
			List<TestCasePriority> priorities = taskManager.TestCasePriority_Retrieve(projectTemplateId);
			TestCasePriority priority = priorities.FirstOrDefault(p => p.Score == remoteTestCase.TestCasePriorityId || p.TestCasePriorityId == remoteTestCase.TestCasePriorityId);
			if (priority == null)
			{
				//No match, so leave blank
				remoteTestCase.TestCasePriorityId = null;
			}
			else
			{
				remoteTestCase.TestCasePriorityId = priority.TestCasePriorityId;
			}
		}

		#endregion
	}
}
