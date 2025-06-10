using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Web.Security;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.v3_0.DataObjects;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v3_0
{
	/// <summary>
	/// This web service enables the import and export of data to/from the system. Each function is prefixed by the
	/// area of the system that it relates to. For example, the Requirement_Retrieve function relates to the
	/// Requirements module.
	/// </summary>
	/// <remarks>
	/// This API is available for all installations v3.0.0 or greater. There are older APIs available
	/// for v2.2.0+ and v1.2.0+
	/// </remarks>
	/// <version>v3.0</version>
	public class ImportExport : ServiceBase, IImportExport
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v3_0.ImportExport::";

		#region Project Methods

		/// <summary>
		/// Creates a new project in the system and makes the authenticated user owner of it
		/// </summary>
		/// <param name="remoteProject">The new project object (primary key will be empty)</param>
		/// <param name="existingProjectId">The id of an existing project to use as a template, or null to use the default template</param>
		/// <returns>The populated project object - including the primary key</returns>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_ProjectIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we have permissions to create projects (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateProjects);
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
			Business.ProjectManager projectManager = new Business.ProjectManager();
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we have permissions to delete projects (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteProjects);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			else
			{
				//Get the list of projects
				int userId = AuthenticatedUserId;
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId);

				//Populate the API data object and return
				List<RemoteProject> remoteProjects = new List<RemoteProject>();
				foreach (ProjectForUserView project in projects)
				{
					//Create and populate the row
					RemoteProject remoteProject = new RemoteProject();
					PopulationFunctions.PopulateProject(remoteProject, project.ConvertTo<ProjectForUserView, Project>());
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			else
			{
				//Get the list of project roles
				int userId = AuthenticatedUserId;
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(false, true);

				//Populate the API data object and return
				List<RemoteProjectRole> remoteProjectRoles = new List<RemoteProjectRole>();
				foreach (ProjectRole projectRole in projectRoles)
				{
					//Create and populate the row
					RemoteProjectRole remoteProjectRole = new RemoteProjectRole();
					PopulationFunctions.PopulateProjectRole(remoteProjectRole, projectRole);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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
					PopulationFunctions.PopulateProject(remoteProject, project);

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
		/// Retrieves a custom list by its ID, including any custom list values
		/// </summary>
		/// <param name="customListId">The id of the custom list we want to retrieve</param>
		/// <returns>The custom list object (including any custom list values)</returns>
		/// <example>
		/// remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId1);
		/// </example>
		public RemoteCustomList CustomProperty_RetrieveCustomListById(int customListId)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveCustomListById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Get the custom list for the project
			try
			{
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				CustomPropertyList customPropertyList = customPropertyManager.CustomPropertyList_RetrieveById(customListId, true);

				//Make sure we have a list
				if (customPropertyList == null)
				{
					throw new Exception(Resources.Messages.Services_CustomListDoesNotExist);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Make sure that this list belongs to the current project (for security reasons)
				if (customPropertyList.ProjectTemplateId != projectTemplateId)
				{
					throw new Exception(Resources.Messages.Services_CustomListNotBelongToProject);
				}

				//Populate the API data object and return
				RemoteCustomList remoteCustomList = new RemoteCustomList(projectId, customPropertyList);

				//Now populate the associated custom list values
				remoteCustomList.Values = new List<RemoteCustomListValue>();
				foreach (CustomPropertyValue customPropertyValue in customPropertyList.Values)
				{
					//Create and populate the row
					RemoteCustomListValue remoteCustomListValue = new RemoteCustomListValue(customPropertyValue);
					remoteCustomList.Values.Add(remoteCustomListValue);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteCustomList;
			}
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves all the custom lists in the current project
		/// </summary>
		/// <returns>A collection of custom list data objects</returns>
		/// <remarks>
		/// Does not return the actual custom list values
		/// </remarks>
		/// <example>
		/// RemoteCustomList[] remoteCustomLists = spiraImportExport.CustomProperty_RetrieveCustomLists();
		/// </example>
		public List<RemoteCustomList> CustomProperty_RetrieveCustomLists()
		{
			const string METHOD_NAME = "CustomProperty_RetrieveCustomLists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Get the custom lists for the project
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomPropertyList> customPropertyLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId);

				//Populate the API data objects and return
				List<RemoteCustomList> remoteCustomLists = new List<RemoteCustomList>();
				foreach (CustomPropertyList customPropertyList in customPropertyLists)
				{
					//Create and populate the row
					RemoteCustomList remoteCustomList = new RemoteCustomList(projectId, customPropertyList);
					remoteCustomLists.Add(remoteCustomList);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteCustomLists;
			}
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Updates a custom list and any associated values in the system
		/// </summary>
		/// <param name="remoteCustomList">The custom list to update</param>
		/// <remarks>This will not add any new custom values, for that you need to use the AddCustomListValue() function</remarks>
		/// <example>
		/// remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId2);
		/// remoteCustomList.Name = "Component Names";
		/// remoteCustomList.Values[0].Name = "Component One";
		/// spiraImportExport.CustomProperty_UpdateCustomList(remoteCustomList);
		/// </example>
		public void CustomProperty_UpdateCustomList(RemoteCustomList remoteCustomList)
		{
			const string METHOD_NAME = "CustomProperty_UpdateCustomList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update custom properties (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedUpdateCustomProperties);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//First we need to get the current definition of the custom list and associated values
				CustomPropertyList customPropertyList = customPropertyManager.CustomPropertyList_RetrieveById(remoteCustomList.CustomPropertyListId.Value, true);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Make sure that this list belongs to the current project template (for security reasons)
				if (customPropertyList.ProjectTemplateId != projectTemplateId)
				{
					throw new Exception(Resources.Messages.Services_CustomListNotBelongToProject);
				}

				//Next we need to update the custom list itself
				customPropertyList.StartTracking();
				customPropertyList.Name = remoteCustomList.Name;
				customPropertyList.IsActive = remoteCustomList.Active;

				//Next we need to update any custom values (if there are any)
				if (remoteCustomList.Values != null)
				{
					//Now populate
					foreach (RemoteCustomListValue remoteCustomListValue in remoteCustomList.Values)
					{
						//Ignore any remoteCustomListValue objects that have no primary key value
						//Since they should be added using the AddCustomListValue() function instead
						if (remoteCustomListValue.CustomPropertyValueId.HasValue)
						{
							//See if we have a matching row in the entity collection
							CustomPropertyValue customPropertyValue = customPropertyList.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == remoteCustomListValue.CustomPropertyValueId);
							if (customPropertyValue != null)
							{
								//Update the existing row
								customPropertyValue.Name = remoteCustomListValue.Name;
								customPropertyValue.IsActive = remoteCustomListValue.Active;
							}
						}
					}
				}

				//Now update the custom list and associated values
				customPropertyManager.CustomPropertyList_Update(customPropertyList);

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

		/// <summary>
		/// Adds a new custom list into the project
		/// </summary>
		/// <param name="remoteCustomList">The new custom list object</param>
		/// <returns>The custom list object with the primary key set</returns>
		/// <remarks>Also adds any custom list values if they are provided</remarks>
		/// <example>
		/// RemoteCustomListValue remoteCustomListValue = new RemoteCustomListValue();
		/// remoteCustomListValue.Name = "Feature";
		/// remoteCustomListValue.Active = true;
		/// RemoteCustomList remoteCustomList = new RemoteCustomList();
		/// remoteCustomList.Name = "Req Types";
		/// remoteCustomList.Active = true;
		/// remoteCustomList.Values = new RemoteCustomListValue[] { remoteCustomListValue };
		/// customListId1 = spiraImportExport.CustomProperty_AddCustomList(remoteCustomList).CustomPropertyListId.Value;
		/// </example>
		public RemoteCustomList CustomProperty_AddCustomList(RemoteCustomList remoteCustomList)
		{
			const string METHOD_NAME = "CustomProperty_AddCustomList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomList.CustomPropertyListId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_CustomListIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create custom lists (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddCustomLists);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the new custom list
				remoteCustomList.CustomPropertyListId = customPropertyManager.CustomPropertyList_Add(projectTemplateId, remoteCustomList.Name, remoteCustomList.Active).CustomPropertyListId;

				//Now add the values if any are provided
				if (remoteCustomList.Values != null)
				{
					foreach (RemoteCustomListValue remoteCustomListValue in remoteCustomList.Values)
					{
						remoteCustomListValue.CustomPropertyValueId = customPropertyManager.CustomPropertyList_AddValue(remoteCustomList.CustomPropertyListId.Value, remoteCustomListValue.Name, remoteCustomListValue.Active).CustomPropertyValueId;
					}
				}

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
		/// <example>
		/// remoteCustomListValue = new RemoteCustomListValue();
		/// remoteCustomListValue.CustomPropertyListId = customListId1;
		/// remoteCustomListValue.Name = "Technical Quality";
		/// remoteCustomListValue.Active = true;
		/// customValueId2 = spiraImportExport.CustomProperty_AddCustomListValue(remoteCustomListValue).CustomPropertyValueId.Value;
		/// </example>
		public RemoteCustomListValue CustomProperty_AddCustomListValue(RemoteCustomListValue remoteCustomListValue)
		{
			const string METHOD_NAME = "CustomProperty_AddCustomListValue";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomListValue.CustomPropertyValueId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_CustomPropertyValueIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create custom list values (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddCustomListValues);
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
		/// Saves the updated custom property configuration for a project
		/// </summary>
		/// <param name="remoteCustomProperties">The list of custom properties to be persisted</param>
		/// <param name="artifactTypeId">The type of artifact the custom properties belong to</param>
		/// <remarks>
		/// This method performs the necessary inserts, updates and deletes on the custom properties.
		/// However it does not update the custom lists or custom list values themselves. For that you 
		/// need to use the UpdateCustomList() function instead
		/// </remarks>
		/// <example>
		/// remoteCustomProperty = new RemoteCustomProperty();
		/// remoteCustomProperty.ArtifactTypeId = (int)CustomProperty.ArtifactType.Requirement;
		/// remoteCustomProperty.CustomPropertyId = 11;
		/// remoteCustomProperty.ProjectId = projectId1;
		/// remoteCustomProperty.Alias = "Req Type";
		/// remoteCustomProperty.CustomList = remoteCustomList;
		/// spiraImportExport.CustomProperty_UpdateCustomProperties((int)CustomProperty.ArtifactType.Requirement, RemoteCustomProperty[] { remoteCustomProperty });
		/// </example>
		public void CustomProperty_UpdateCustomProperties(int artifactTypeId, List<RemoteCustomProperty> remoteCustomProperties)
		{
			const string METHOD_NAME = "CustomProperty_UpdateCustomProperties";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update custom properties (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedUpdateCustomProperties);
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
				foreach (RemoteCustomProperty remoteCustomProperty in remoteCustomProperties)
				{
					//See if we have a matching row in the list
					CustomProperty customProperty = customProperties.FirstOrDefault(cp => cp.CustomPropertyId == remoteCustomProperty.CustomPropertyId && cp.ProjectTemplateId == projectTemplateId && cp.ArtifactTypeId == artifactTypeId);
					if (customProperty == null)
					{
						//We need to add a new row, the custom property type has to be deduced implicitly from the "old" custom property id value
						//1-10 = Text
						//11-20 = List
						CustomProperty.CustomPropertyTypeEnum customPropertyType = CustomProperty.CustomPropertyTypeEnum.Text;
						if (remoteCustomProperty.CustomPropertyId > 10 && remoteCustomProperty.CustomPropertyId <= 20)
						{
							customPropertyType = CustomProperty.CustomPropertyTypeEnum.List;
						}

						//Make sure we have an available number of custom properties for this artifact
						int propertyNumber = maxUsedCustomPropertyNumber + 1;
						if (propertyNumber <= CustomProperty.MAX_NUMBER_ARTIFACT_PROPERTIES)
						{
							if (remoteCustomProperty.CustomList == null)
							{
								customPropertyManager.CustomPropertyDefinition_AddToArtifact(
									projectTemplateId, 
									(Artifact.ArtifactTypeEnum)artifactTypeId,
									(int)customPropertyType,
									propertyNumber,
									remoteCustomProperty.Alias,
									null,
									null,
									null);
							}
							else
							{
								customPropertyManager.CustomPropertyDefinition_AddToArtifact(
									projectTemplateId, 
									(Artifact.ArtifactTypeEnum)artifactTypeId,
									(int)customPropertyType, 
									propertyNumber,
									remoteCustomProperty.Alias,
									null,
									null,
									remoteCustomProperty.CustomList.CustomPropertyListId);
							}
							maxUsedCustomPropertyNumber++;
						}
						else
						{
							throw CreateFault("NoAvailableCustomProperties", Resources.Messages.Services_NoAvailableCustomProperties);
						}
					}
					else
					{
						//Update the existing row
						customProperty.StartTracking();
						customProperty.Name = remoteCustomProperty.Alias;

						//If list property, set the list id
						if (remoteCustomProperty.CustomList != null && remoteCustomProperty.CustomList.CustomPropertyListId.HasValue)
						{
							customProperty.CustomPropertyListId = remoteCustomProperty.CustomList.CustomPropertyListId.Value;
						}

						//Save any changes
						customPropertyManager.CustomPropertyDefinition_Update(customProperty);
					}
				}

				//Finally we need to see if any rows have been removed
				foreach (CustomProperty customProperty in customProperties)
				{
					//See if we have an entry that's not in the API data object
					bool matchFound = false;
					foreach (RemoteCustomProperty remoteCustomProperty in remoteCustomProperties)
					{
						if (remoteCustomProperty.CustomPropertyId == customProperty.CustomPropertyId
							&& remoteCustomProperty.ArtifactTypeId == customProperty.ArtifactTypeId
							&& remoteCustomProperty.ProjectId == projectId)
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

		#region Association Methods

		/// <summary>
		/// Adds a new association in the system
		/// </summary>
		/// <param name="remoteAssociation">The association to add</param>
		/// <returns>The association with its primary key populated</returns>
		/// <example>
		/// RemoteAssociation remoteAssociation = new RemoteAssociation();
		/// remoteAssociation.SourceArtifactId = requirementId1;
		/// remoteAssociation.SourceArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
		/// remoteAssociation.DestArtifactId = incidentId1;
		/// remoteAssociation.DestArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
		/// remoteAssociation.Comment = "They are related";
		/// spiraImportExport.Association_Create(remoteAssociation);
		/// </example>
		public RemoteAssociation Association_Create(RemoteAssociation remoteAssociation)
		{
			const string METHOD_NAME = "Association_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a ArtifactLinkID specified
			if (remoteAssociation.ArtifactLinkId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_ArtifactLinkIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add associations to the Source artifact (considered a modify operation)
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, remoteAssociation.SourceArtifactTypeId, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddAssociations);
			}

			try
			{
				//Default to the authenticated user if no creator provided
				int creatorId = userId;
				if (remoteAssociation.CreatorId.HasValue)
				{
					creatorId = remoteAssociation.CreatorId.Value;
				}

				//Check the comment length
				string comment = remoteAssociation.Comment.MaxLength(255);

				//If the creation date is not specified, use the current one
				if (!remoteAssociation.CreationDate.HasValue)
				{
					//We will convert to UTC later
					remoteAssociation.CreationDate = DateTime.Now;
				}

				//Now insert the association
				Business.ArtifactLinkManager artifactLinkManager = new Business.ArtifactLinkManager();
				remoteAssociation.ArtifactLinkId = artifactLinkManager.Insert(
				   projectId,
				   (DataModel.Artifact.ArtifactTypeEnum)remoteAssociation.SourceArtifactTypeId,
				   remoteAssociation.SourceArtifactId,
				   (DataModel.Artifact.ArtifactTypeEnum)remoteAssociation.DestArtifactTypeId,
				   remoteAssociation.DestArtifactId,
				   creatorId,
				   comment,
				   GlobalFunctions.UniversalizeDate(remoteAssociation.CreationDate.Value),
				   ArtifactLink.ArtifactLinkTypeEnum.RelatedTo
				   );
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw ConvertExceptions(exception);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteAssociation;
		}

		/// <summary>
		/// Updates the specified Association's information
		/// </summary>
		/// <param name="remoteAssociation">The updated association information</param>
		/// <example>
		/// remoteAssociation.Comment = "They are the same bugs";
		/// spiraImportExport.Association_Update(remoteAssociation);
		/// </example>
		/// <remarks>
		/// Currently only the comment field is updated
		/// </remarks>
		public void Association_Update(RemoteAssociation remoteAssociation)
		{
			const string METHOD_NAME = "Incident_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an artifact link id specified
			if (!remoteAssociation.ArtifactLinkId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_ArtifactLinkIdMissing);
			}

			//Make sure that we're not trying update a system-generated association (they have negative primary keys)
			if (remoteAssociation.ArtifactLinkId.Value < 0)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("InvalidArgument", Resources.Messages.Services_ArtifactLinkReadOnly);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update the source artifact id
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, remoteAssociation.SourceArtifactTypeId, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddAssociations);
			}

			//First retrieve the existing datarow
			try
			{
				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				ArtifactLink artifactLink = artifactLinkManager.RetrieveById(remoteAssociation.ArtifactLinkId.Value);

				//Need to extract the data from the API data object and add to the internal dataset
				UpdateAssociationData(artifactLink, remoteAssociation);

				//Call the business object to actually update the association
				artifactLinkManager.Update(artifactLink, userId);
			}
			catch (Exception exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Updates the pending test run from the passing in test run API objects
		/// </summary>
		/// <param name="testRunsPending">The pending test run set</param>
		/// <param name="remoteTestRuns">The API objects</param>
		/// <param name="projectId">The id of the project</param>
		protected void UpdatePendingTestRun(TestRunsPending testRunsPending, List<RemoteManualTestRun> remoteTestRuns, int projectId, int userId)
		{
			if (remoteTestRuns.Count > 0)
			{
				testRunsPending.TesterId = userId;
				testRunsPending.Name = remoteTestRuns[0].Name;  //Will be deleted when pending run completed, so name doesn't really matter
				testRunsPending.CreationDate = DateTime.UtcNow;
				testRunsPending.LastUpdateDate = DateTime.UtcNow;
				testRunsPending.ProjectId = projectId;
				testRunsPending.TestSetId = remoteTestRuns[0].TestSetId;
				testRunsPending.CountNotRun = remoteTestRuns.Count;
			}
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteAssociation">The API data object</param>
		/// <param name="artifactLink">The internal datarow</param>
		/// <remarks>Only the comment and creator fields are updated</remarks>
		protected void UpdateAssociationData(ArtifactLink artifactLink, RemoteAssociation remoteAssociation)
		{
			artifactLink.StartTracking();
			if (remoteAssociation.CreatorId.HasValue)
			{
				artifactLink.CreatorId = remoteAssociation.CreatorId.Value;
			}
			artifactLink.Comment = remoteAssociation.Comment.MaxLength(255);
		}

		/// <summary>
		/// Retrieves a set of associations to the specified artifact
		/// </summary>
		/// <param name="artifactTypeId">The id of the artifact type</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <returns>An array of association records</returns>
		/// <example>
		/// RemoteSort remoteSort = new RemoteSort();
		/// remoteSort.PropertyName = "Comment";
		/// remoteSort.SortAscending = true;
		/// RemoteFilter remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "ArtifactTypeId";
		/// remoteFilter.IntValue = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
		/// remoteAssociations = spiraImportExport.Association_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, new RemoteFilter[] { remoteFilter }, remoteSort);
		/// </example>
		/// <remarks>
		/// The source artifact type and id will be the same as the ones passed in
		/// </remarks>
		public List<RemoteAssociation> Association_RetrieveForArtifact(int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort)
		{
			const string METHOD_NAME = "Association_RetrieveForArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}


			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view associations for the Source artifact (considered a view operation)
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, artifactTypeId, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewAssociations);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Call the business object to actually retrieve the association record
				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, remoteSort.PropertyName, remoteSort.SortAscending, filters);

				//Populate the API data object and return
				//Note that the view doesn't contain the source artifact info, we have to get that
				//from the passed in values to the API.
				List<RemoteAssociation> remoteAssociations = new List<RemoteAssociation>();
				foreach (ArtifactLinkView artifactLink in artifactLinks)
				{
					RemoteAssociation remoteAssociation = new RemoteAssociation();
					remoteAssociation.ArtifactLinkId = artifactLink.ArtifactLinkId;
					remoteAssociation.SourceArtifactId = artifactId;
					remoteAssociation.SourceArtifactTypeId = artifactTypeId;
					remoteAssociation.DestArtifactId = artifactLink.ArtifactId;
					remoteAssociation.DestArtifactTypeId = artifactLink.ArtifactTypeId;
					remoteAssociation.CreatorId = artifactLink.CreatorId;
					remoteAssociation.Comment = artifactLink.Comment;
					remoteAssociation.CreationDate = GlobalFunctions.LocalizeDate(artifactLink.CreationDate);
					remoteAssociation.DestArtifactName = artifactLink.ArtifactName;
					remoteAssociation.DestArtifactTypeName = artifactLink.ArtifactTypeName;
					remoteAssociation.CreatorName = artifactLink.CreatorName;

					//Add to the list
					remoteAssociations.Add(remoteAssociation);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteAssociations;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		#endregion

		#region Document Methods

		/// <summary>
		/// Retrieves a filtered list of documents/attachments in a project for the specified folder
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <param name="folderId">The id of the project attachment folder</param>
		/// <returns>List of documents</returns>
		/// <example>
		/// RemoteFilter remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "Filename";
		/// remoteFilter.StringValue = "test_data";
		/// remoteSort.PropertyName = "Filename";
		/// remoteSort.SortAscending = true;
		/// remoteDocuments = spiraImportExport.Document_RetrieveForFolder(projectAttachmentFolderId, new RemoteFilter[] { remoteFilter }, remoteSort, 1, 999);
		/// </example>
		public List<RemoteDocument> Document_RetrieveForFolder(int folderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberRows)
		{
			const string METHOD_NAME = "Document_RetrieveForFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}


			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we can view documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, folderId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberRows, filters, 0);

				//Populate the API data object and return
				List<RemoteDocument> remoteDocuments = new List<RemoteDocument>();
				foreach (ProjectAttachmentView projectAttachment in projectAttachments)
				{
					//Create and populate the row
					RemoteDocument remoteDocument = new RemoteDocument();
					PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);
					remoteDocuments.Add(remoteDocument);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocuments;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Deletes an attachment from an artifact. The attachment will still remain in the project
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to delete</param>
		/// <param name="artifactId">The ID of the artifact to delete</param>
		/// <param name="artifactTypeId">The ID of the type of artifact being deleted</param>
		/// <remarks>
		/// spiraImportExport.Document_DeleteFromArtifact(attachmentId2, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1);
		/// </remarks>
		public void Document_DeleteFromArtifact(int attachmentId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "Document_DeleteFromArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteDocuments);
			}

			try
			{
				//Convert the artifact type id into the enumeration
				DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;

				//Now delete the attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Delete(projectId, attachmentId, artifactId, artifactType);
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
		/// Deletes an attachment from the project completely
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to delete</param>
		public void Document_Delete(int attachmentId)
		{
			const string METHOD_NAME = "Document_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteDocuments);
			}

			try
			{
				//Now delete the attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Delete(projectId, attachmentId);
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
		/// Returns the actual binary content of a file attachment in the system
		/// </summary>
		/// <param name="attachmentId">The id of the file attachment to be retrieved</param>
		/// <returns>An array of bytes representing the attachment content</returns>
		/// <example>
		/// byte[] attachmentData = spiraImportExport.Document_OpenFile(attachmentId1);
		/// </example>
		public byte[] Document_OpenFile(int attachmentId)
		{
			const string METHOD_NAME = "Document_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we can view documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}

			try
			{
				//Call the business object to actually retrieve the attachment data
				AttachmentManager attachmentManager = new AttachmentManager();
				FileStream stream = attachmentManager.OpenById(attachmentId);

				//Extract the data from the stream in byte form
				byte[] attachmentBytes = new byte[stream.Length];
				stream.Read(attachmentBytes, 0, (int)stream.Length);

				//Return the array of bytes
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return attachmentBytes;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_DocumentNotFound);
				Logger.Flush();
				return null;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves a list of all the document types in the current project
		/// </summary>
		/// <param name="activeOnly">Do we only want the active types</param>
		/// <returns>List of document types</returns>
		public List<RemoteDocumentType> Document_RetrieveTypes(bool activeOnly)
		{
			const string METHOD_NAME = "Document_RetrieveTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//There are no specific permissions required to view document types as long as you are a member of the project

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<DocumentType> attachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, true);

				//Populate the API data object and return
				List<RemoteDocumentType> remoteDocumentTypes = new List<RemoteDocumentType>();
				foreach (DocumentType attachmentType in attachmentTypes)
				{
					//Create and populate the row
					RemoteDocumentType remoteDocumentType = new RemoteDocumentType();
					PopulationFunctions.PopulateDocumentType(projectId, remoteDocumentType, attachmentType);
					remoteDocumentTypes.Add(remoteDocumentType);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocumentTypes;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Adds a new document folder into the current project
		/// </summary>
		/// <param name="remoteDocumentFolder">The new document folder</param>
		/// <returns>The document folder with the primary key populated</returns>
		public RemoteDocumentFolder Document_AddFolder(RemoteDocumentFolder remoteDocumentFolder)
		{
			const string METHOD_NAME = "Document_AddFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add document folders
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestFolders);
			}

			try
			{
				//Make sure that the specified parent folder exists in the project
				AttachmentManager attachmentManager = new AttachmentManager();
				if (remoteDocumentFolder.ParentProjectAttachmentFolderId.HasValue)
				{
					try
					{
						ProjectAttachmentFolder projectAttachmentFolder = attachmentManager.RetrieveFolderById(remoteDocumentFolder.ParentProjectAttachmentFolderId.Value);
						if (projectAttachmentFolder.ProjectId != projectId)
						{
							//Set the folder to the project default
							attachmentManager.GetDefaultProjectFolder(projectId);
							remoteDocumentFolder.ParentProjectAttachmentFolderId = attachmentManager.GetDefaultProjectFolder(projectId);
						}
					}
					catch (ArtifactNotExistsException)
					{
						//Set the folder to the project default
						attachmentManager.GetDefaultProjectFolder(projectId);
						remoteDocumentFolder.ParentProjectAttachmentFolderId = attachmentManager.GetDefaultProjectFolder(projectId);
					}
				}

				//Now insert the document folder
				remoteDocumentFolder.ProjectId = projectId;
				remoteDocumentFolder.ProjectAttachmentFolderId = attachmentManager.InsertProjectAttachmentFolder(
					remoteDocumentFolder.ProjectId,
					remoteDocumentFolder.Name,
					remoteDocumentFolder.ParentProjectAttachmentFolderId
					);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDocumentFolder;
		}

		/// <summary>
		/// Updates the name and position of an existing folder in the project
		/// </summary>
		/// <param name="remoteDocumentFolder">The updated folder information</param>
		public void Document_UpdateFolder(RemoteDocumentFolder remoteDocumentFolder)
		{
			const string METHOD_NAME = "Document_UpdateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an folder id specified
			if (!remoteDocumentFolder.ProjectAttachmentFolderId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_ProjectAttachmentFolderIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to edit document folders
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestFolders);
			}

			try
			{
				//Make sure that the specified parent folder exists in the project
				AttachmentManager attachmentManager = new AttachmentManager();
				if (remoteDocumentFolder.ParentProjectAttachmentFolderId.HasValue)
				{
					try
					{
						ProjectAttachmentFolder attachmentFolder = attachmentManager.RetrieveFolderById(remoteDocumentFolder.ParentProjectAttachmentFolderId.Value);
						if (attachmentFolder.ProjectId != projectId)
						{
							//Set the folder to the project default
							attachmentManager.GetDefaultProjectFolder(projectId);
							remoteDocumentFolder.ParentProjectAttachmentFolderId = attachmentManager.GetDefaultProjectFolder(projectId);
						}
					}
					catch (ArtifactNotExistsException)
					{
						//Set the folder to the project default
						attachmentManager.GetDefaultProjectFolder(projectId);
						remoteDocumentFolder.ParentProjectAttachmentFolderId = attachmentManager.GetDefaultProjectFolder(projectId);
					}
				}

				//Now update the document folder
				ProjectAttachmentFolder projectAttachmentFolder = attachmentManager.RetrieveFolderById(remoteDocumentFolder.ProjectAttachmentFolderId.Value);
				projectAttachmentFolder.StartTracking();
				projectAttachmentFolder.Name = remoteDocumentFolder.Name;
				projectAttachmentFolder.ParentProjectAttachmentFolderId = remoteDocumentFolder.ParentProjectAttachmentFolderId;
				attachmentManager.UpdateFolder(projectAttachmentFolder);
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
		/// Deletes an existing document folder from the current project
		/// </summary>
		/// <param name="projectAttachmentFolderId">The id of the folder to delete</param>
		/// <remarks>This will delete all child folders and documents/attachements</remarks>
		public void Document_DeleteFolder(int projectAttachmentFolderId)
		{
			const string METHOD_NAME = "Document_DeleteFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete document folders
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestFolders);
			}

			try
			{
				//Now delete the document folder after making sure the folder is actually in the specified project
				AttachmentManager attachmentManager = new AttachmentManager();
				ProjectAttachmentFolder attachmentFolder = attachmentManager.RetrieveFolderById(projectAttachmentFolderId);
				if (attachmentFolder.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}
				attachmentManager.DeleteFolder(projectId, projectAttachmentFolderId);
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
		/// Retrieves a list of all the document folders in the current project
		/// </summary>
		/// <returns>List of document types</returns>
		public List<RemoteDocumentFolder> Document_RetrieveFolders()
		{
			const string METHOD_NAME = "Document_RetrieveFolders";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//There are no specific permissions required to view document folders as long as you are a member of the project
			try
			{
				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentFolderHierarchy> attachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);

				//Populate the API data object and return
				//We have to access the sorted dataview to get them in the right order
				List<RemoteDocumentFolder> remoteDocumentFolders = new List<RemoteDocumentFolder>();
				foreach (ProjectAttachmentFolderHierarchy attachmentFolder in attachmentFolders)
				{
					//Create and populate the row
					RemoteDocumentFolder remoteDocumentFolder = new RemoteDocumentFolder();
					PopulationFunctions.PopulateDocumentFolder(remoteDocumentFolder, attachmentFolder);
					remoteDocumentFolders.Add(remoteDocumentFolder);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocumentFolders;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>Retrieves the folder by the specified ID.</summary>
		/// <param name="folderId">The id of the folder being retrieved</param>
		/// <returns>RemoteDocumentFolder</returns>
		public RemoteDocumentFolder Document_RetrieveFolderById(int folderId)
		{
			const string METHOD_NAME = "Document_RetrieveFolderById";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (this.IsAuthenticated && this.IsAuthorized)
			{
				int userId = AuthenticatedUserId;
				int projectId = AuthorizedProject;

				try
				{
					//Call the business object to actually retrieve the project attachment dataset
					ProjectAttachmentFolder folder = new AttachmentManager().RetrieveFolderById(folderId);

					//Copy over the remote document.
					RemoteDocumentFolder remoteDocumentFolder = new RemoteDocumentFolder();
					PopulationFunctions.PopulateDocumentFolder(remoteDocumentFolder, folder);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return remoteDocumentFolder;
				}
				catch (Exception ex)
				{
					//Log and then rethrow the converted exception
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
					throw ConvertExceptions(ex);
				}

			}
			else
			{
				if (!IsAuthenticated)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
				}
				else if (!IsAuthorized)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
				}
			}

			return null;
		}

		/// <summary>
		/// Retrieves a single project document by its id
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to retrieve</param>
		/// <returns>The document data object</returns>
		/// <example>
		/// remoteDocument = spiraImportExport.Document_RetrieveById(attachmentId3);
		/// </example>
		/// <remarks>
		/// 1) For files it does not include the raw file data, you need to use Document_OpenById
		/// 2) It also retrieves the list of document versions
		/// </remarks>
		public RemoteDocument Document_RetrieveById(int attachmentId)
		{
			const string METHOD_NAME = "Document_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we can view documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}

			try
			{
				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);

				//Populate the API data object
				RemoteDocument remoteDocument = new RemoteDocument();
				PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);

				//Now get the list of versions and populate
				List<AttachmentVersionView> attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);
				remoteDocument.Versions = new List<RemoteDocumentVersion>();
				foreach (AttachmentVersionView attachmentVersion in attachmentVersions)
				{
					RemoteDocumentVersion remoteDocumentVersion = new RemoteDocumentVersion();
					PopulationFunctions.PopulateDocumentVersion(remoteDocumentVersion, attachmentVersion);
					remoteDocument.Versions.Add(remoteDocumentVersion);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocument;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_DocumentNotFound);
				Logger.Flush();
				return null;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves a filtered list of documents/attachments in a project attached to a specific artifact
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="artifactId">The id of the artifact we want the attachments for</param>
		/// <param name="artifactTypeId">
		/// The id of the type of artifact we want to retrieve the documents for:
		/// (Requirement = 1,
		///	TestCase = 2,
		/// Incident = 3,
		///	Release = 4,
		///	TestRun = 5,
		///	Task = 6,
		/// TestStep = 7,
		/// TestSet = 8)
		/// </param>
		/// <returns>List of documents</returns>
		/// <example>
		/// remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "Filename";
		/// remoteFilter.StringValue = "test_data2";
		/// remoteSort.PropertyName = "AttachmentId";
		/// remoteSort.SortAscending = true;
		/// remoteDocuments = spiraImportExport.Document_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, new RemoteFilter[] { remoteFilter }, remoteSort);
		/// </example>
		public List<RemoteDocument> Document_RetrieveForArtifact(int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort)
		{
			const string METHOD_NAME = "Document_RetrieveForArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}


			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view the artifact type in question
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, artifactTypeId, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;

			//Make sure we can view documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, artifactId, artifactType, remoteSort.PropertyName, remoteSort.SortAscending, 1, Int32.MaxValue, filters, 0);

				//Populate the API data object and return
				List<RemoteDocument> remoteDocuments = new List<RemoteDocument>();
				foreach (ProjectAttachmentView projectAttachment in attachments)
				{
					//Create and populate the row
					RemoteDocument remoteDocument = new RemoteDocument();
					PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);
					remoteDocuments.Add(remoteDocument);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocuments;
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>Adds an existing attachment to the specified artifact.</summary>
		/// <param name="artifactTypeId">The id of the type of artifact we want to retrieve the documents for:
		/// (Requirement = 1,
		///	TestCase = 2,
		/// Incident = 3,
		///	Release = 4,
		///	TestRun = 5,
		///	Task = 6,
		/// TestStep = 7,
		/// TestSet = 8)</param>
		/// <param name="artifactId">The id of the artifact we want the attachments for</param>
		/// <param name="attachmentId"></param>
		public void Document_AddToArtifactId(int artifactTypeId, int artifactId, int attachmentId)
		{
			const string METHOD_NAME = CLASS_NAME + "Document_AddToArtifactId";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view the artifact type in question
			if (this.ProjectRolePermissions == null || (userId != 1 && this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, artifactTypeId, (int)Project.PermissionEnum.Modify) == null))
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyArtifact);
			}
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;

			//Make sure we can view documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewArtifact);
			}

			try
			{
				new AttachmentManager().InsertArtifactAssociation(projectId, attachmentId, artifactId, artifactType);
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

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
		/// <example>
		/// byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
		/// RemoteDocument remoteDocument = new RemoteDocument();
		/// remoteDocument.FilenameOrUrl = "test_data.xls";
		/// remoteDocument.Description = "Sample Test Case Attachment";
		/// remoteDocument.AuthorId = userId2;
		/// remoteDocument.ArtifactId = testCaseId1;
		/// remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
		/// attachmentId1 = spiraImportExport.Document_AddFile(remoteDocument, attachmentData).AttachmentId.Value;
		/// </example>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_AttachmentIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddDocuments);
			}

			try
			{
				//Default to the authenticated user if no author provided
				int authorId = userId;
				if (remoteDocument.AuthorId.HasValue)
				{
					authorId = remoteDocument.AuthorId.Value;
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
				AttachmentManager attachmentManager = new AttachmentManager();
				remoteDocument.AttachmentId = attachmentManager.Insert(
				   projectId,
				   remoteDocument.FilenameOrUrl,
				   description,
				   authorId,
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
		/// <param name="remoteDocument">The new document object (primary key will be empty). PopulationFunctions.Populate the FilenameOrUrl field with the URL</param>
		/// <returns>
		/// The populated document object - including the primary key and default values for project attachment type
		/// and project folder if they were not specified
		/// </returns>
		/// <example>
		/// remoteDocument = new RemoteDocument();
		/// remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
		/// remoteDocument.Description = "Sample Test Case URL";
		/// remoteDocument.AuthorId = userId2;
		/// remoteDocument.ArtifactId = testCaseId2;
		/// remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
		/// attachmentId3 = spiraImportExport.Document_AddUrl(remoteDocument).AttachmentId.Value;
		/// </example>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_AttachmentIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddDocuments);
			}

			try
			{
				//Default to the authenticated user if no author provided
				int authorId = userId;
				if (remoteDocument.AuthorId.HasValue)
				{
					authorId = remoteDocument.AuthorId.Value;
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
				AttachmentManager attachmentManager = new AttachmentManager();
				remoteDocument.AttachmentId = attachmentManager.Insert(
				   projectId,
				   remoteDocument.FilenameOrUrl,
				   description,
				   authorId,
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
		/// Adds a new version to a file attachment in the system
		/// </summary>
		/// <param name="remoteDocumentVersion">The version data object</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="makeCurrent">Should we make this the current version</param>
		/// <returns>The version data object with the primary key populated</returns>
		public RemoteDocumentVersion Document_AddFileVersion(RemoteDocumentVersion remoteDocumentVersion, byte[] binaryData, bool makeCurrent)
		{
			const string METHOD_NAME = "Document_AddFileVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a Attachment Version ID specified
			if (remoteDocumentVersion.AttachmentVersionId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_AttachmentVersionIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddDocuments);
			}

			try
			{
				//Retrieve the attachment and make sure that they are of the same type
				AttachmentManager attachmentManager = new AttachmentManager();

				//Default to the authenticated user if no author provided
				int authorId = userId;
				if (remoteDocumentVersion.AuthorId.HasValue)
				{
					authorId = remoteDocumentVersion.AuthorId.Value;
				}

				//Handle any nullable values safely
				string description = null;
				string version = null;
				if (!String.IsNullOrEmpty(remoteDocumentVersion.Description))
				{
					description = remoteDocumentVersion.Description;
				}
				if (!String.IsNullOrEmpty(remoteDocumentVersion.VersionNumber))
				{
					version = remoteDocumentVersion.VersionNumber;
				}

				//Now insert the attachment version
				remoteDocumentVersion.AttachmentVersionId = attachmentManager.InsertVersion(
				   projectId,
				   remoteDocumentVersion.AttachmentId,
				   remoteDocumentVersion.FilenameOrUrl,
				   description,
				   authorId,
				   binaryData,
				   version,
				   makeCurrent
				   );
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDocumentVersion;
		}

		/// <summary>
		/// Adds a new version to a URL attachment in the system
		/// </summary>
		/// <param name="remoteDocumentVersion">The version data object</param>
		/// <param name="makeCurrent">Should we make this the current version</param>
		/// <returns>The version data object with the primary key populated</returns>
		public RemoteDocumentVersion Document_AddUrlVersion(RemoteDocumentVersion remoteDocumentVersion, bool makeCurrent)
		{
			const string METHOD_NAME = "Document_AddUrlVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a Attachment Version ID specified
			if (remoteDocumentVersion.AttachmentVersionId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_AttachmentVersionIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to add documents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddDocuments);
			}

			try
			{
				//Retrieve the attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				Attachment attachment = attachmentManager.RetrieveById(remoteDocumentVersion.AttachmentId);

				//Default to the authenticated user if no author provided
				int authorId = userId;
				if (remoteDocumentVersion.AuthorId.HasValue)
				{
					authorId = remoteDocumentVersion.AuthorId.Value;
				}

				//Handle any nullable values safely
				string description = null;
				string version = null;
				if (!String.IsNullOrEmpty(remoteDocumentVersion.Description))
				{
					description = remoteDocumentVersion.Description;
				}
				if (!String.IsNullOrEmpty(remoteDocumentVersion.VersionNumber))
				{
					version = remoteDocumentVersion.VersionNumber;
				}

				//Now insert the attachment version
				remoteDocumentVersion.AttachmentVersionId = attachmentManager.InsertVersion(
				   projectId,
				   remoteDocumentVersion.AttachmentId,
				   remoteDocumentVersion.FilenameOrUrl,
				   description,
				   authorId,
				   version,
				   makeCurrent
				   );
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDocumentVersion;
		}

		#endregion

		#region User Methods

		/// <summary>
		/// Creates a new user in the system and adds them to the current project as the specified role
		/// </summary>
		/// <param name="remoteUser">The new user object (primary key will be empty)</param>
		/// <param name="projectRoleId">The project role for the user</param>
		/// <returns>The populated user object - including the primary key</returns>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_UserIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int authenticatedUserId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create users (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateUsers);
			}

			//Make sure we have a populated first and last name so that we don't have the user created without a valid profile
			if (String.IsNullOrEmpty(remoteUser.FirstName) || String.IsNullOrEmpty(remoteUser.LastName))
			{
				throw CreateFault("DataValidationError", Resources.Messages.Services_FirstOrLastNameNotProvided);
			}

			try
			{
				Business.ProjectManager projectManager = new Business.ProjectManager();

				//If a user already exists with the ID, simply return that back instead
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

				//Next create the user
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
						throw CreateFault(status.ToString(), message);
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
					throw CreateFault(status.ToString(), message);
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
		/// Refreshes the task progress and test execution status for a project.
		/// Typically this needs to be called after TestRun_RecordAutomated3(...) API calls
		/// to ensure the data in the system is consistent
		/// </summary>
		/// <param name="releaseId">The release we want to refresh, or pass NULL for all releases in the project</param>
		/// <param name="runInBackground">
		/// Do we want to run this in the background. If it runs in the background it will return immediately and continue processing on the server/
		/// Otherwise the caller will have to wait until it finishes. Choosing True is better if you have a large project that will take longer to
		/// refresh that the web service timeout, but False is better if your subsequent code relies on the data being refreshed
		/// </param>
		public void Project_RefreshProgressExecutionStatusCaches(int? releaseId, bool runInBackground)
		{
			const string METHOD_NAME = "Project_RefreshProgressExecutionStatusCaches";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we are a project owner to execute this
			if (!IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyProject);
			}

			try
			{
				//We need to refresh the test status for the whole project
				Business.ProjectManager project = new Business.ProjectManager();
				project.RefreshTestStatusAndTaskProgressCache(projectId, runInBackground, releaseId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of users that are members of the current project
		/// </summary>
		/// <returns>List of ProjectUser objects</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			try
			{
				//Retrieve the list of users that are members of the current project
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(projectId);

				//Populate the API data object and return
				List<RemoteProjectUser> remoteProjectUsers = new List<RemoteProjectUser>();
				foreach (ProjectUser projectUser in projectUsers)
				{
					RemoteProjectUser remoteProjectUser = new RemoteProjectUser();
					PopulationFunctions.PopulateProjectUser(remoteProjectUser, projectUser);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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
					PopulationFunctions.PopulateUser(remoteUser, user);

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
		/// <param name="userName">The login of the user</param>
		/// <returns>The user object</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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
					PopulationFunctions.PopulateUser(remoteUser, user);

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

		#endregion

		#region Incident Methods

		/// <summary>Returns the number of incidents that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Incident_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Incident_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = (long)new IncidentManager().Count(projectId, filters, 0, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Retrieves a list of incidents in the system that match the provided filter/sort
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of incidents</returns>
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulationFunctions.PopulateIncident(remoteIncident, incident);
				PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
				remoteIncidents.Add(remoteIncident);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves a list of incidents in the system that are linked to a specific test run step
		/// </summary>
		/// <param name="testRunStepId">The id of the test run step</param>
		/// <returns>List of incidents</returns>
		public List<RemoteIncident> Incident_RetrieveByTestRunStep(int testRunStepId)
		{
			const string METHOD_NAME = "Incident_RetrieveByTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row, making sure that the incidents belong to the appropriate project (for security reasons)
				if (incident.ProjectId == projectId)
				{
					RemoteIncident remoteIncident = new RemoteIncident();
					PopulationFunctions.PopulateIncident(remoteIncident, incident, testRunStepId);
					PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
					remoteIncidents.Add(remoteIncident);
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves a list of incidents in the system that are linked to a specific test step
		/// (either directly or indirectly through test runs)
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <returns>List of incidents</returns>
		public List<RemoteIncident> Incident_RetrieveByTestStep(int testStepId)
		{
			const string METHOD_NAME = "Incident_RetrieveByTestStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.RetrieveByTestStepId(testStepId);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row, making sure that the incidents belong to the appropriate project (for security reasons)
				if (incident.ProjectId == projectId)
				{
					RemoteIncident remoteIncident = new RemoteIncident();
					PopulationFunctions.PopulateIncident(remoteIncident, incident);
					PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
					remoteIncidents.Add(remoteIncident);
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteIncidents;
		}

		/// <summary>
		/// Retrieves a list of incidents in the system that are linked to a specific test case
		/// (either through test runs or test steps)
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="openOnly">Do we only want incidents that are in one of the 'open' statuses</param>
		/// <returns>List of incidents</returns>
		public List<RemoteIncident> Incident_RetrieveByTestCase(int testCaseId, bool openOnly)
		{
			const string METHOD_NAME = "Incident_RetrieveByTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.RetrieveByTestCaseId(testCaseId, openOnly);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row, making sure that the incidents belong to the appropriate project (for security reasons)
				if (incident.ProjectId == projectId)
				{
					RemoteIncident remoteIncident = new RemoteIncident();
					PopulationFunctions.PopulateIncident(remoteIncident, incident);
					PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
					remoteIncidents.Add(remoteIncident);
				}
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			Hashtable filters = new Hashtable();
			Common.DateRange dateRange = new Common.DateRange();
			dateRange.StartDate = GlobalFunctions.UniversalizeDate(creationDate);
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, "CreationDate", false, 1, 1000, filters, 0);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulationFunctions.PopulateIncident(remoteIncident, incident);
				PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentView> incidents = incidentManager.RetrieveOpenByOwnerId(userId, null, null);

			//Get the custom property definitions - for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Incident);

			//Populate the API data object and return
			List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
			foreach (IncidentView incident in incidents)
			{
				//Create and populate the row
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulationFunctions.PopulateIncident(remoteIncident, incident);
				PopulationFunctions.PopulateCustomProperties(remoteIncident, incident, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to modify incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyIncidents);
			}

			//Iterate through the provided resolutions, inserting them as needed
			IncidentManager incidentManager = new IncidentManager();
			foreach (RemoteIncidentResolution remoteIncidentResolution in remoteIncidentResolutions)
			{
				//If the creator is not specified, use the current user
				int creatorId = userId;
				if (remoteIncidentResolution.CreatorId.HasValue)
				{
					creatorId = remoteIncidentResolution.CreatorId.Value;
				}

				remoteIncidentResolution.IncidentResolutionId = incidentManager.InsertResolution(
				   remoteIncidentResolution.IncidentId,
				   remoteIncidentResolution.Resolution,
				   GlobalFunctions.UniversalizeDate(remoteIncidentResolution.CreationDate),
				   creatorId,
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();

			//If the incident was not found, just return null
			try
			{
				IncidentView incident = incidentManager.RetrieveById2(incidentId);

				//Make sure that the project ids match
				if (incident.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, true);

				//Also get the first linked test run step
				int? testRunStepId = null;
				List<TestRunStepIncidentView> testRunStepIncidents = incidentManager.Incident_RetrieveTestRunSteps(incidentId);
				if (testRunStepIncidents != null && testRunStepIncidents.Count > 0)
				{
					testRunStepId = testRunStepIncidents.FirstOrDefault().TestRunStepId;
				}

				//Populate the API data object and return
				RemoteIncident remoteIncident = new RemoteIncident();
				PopulationFunctions.PopulateIncident(remoteIncident, incident, testRunStepId);
				PopulationFunctions.PopulateCustomProperties(remoteIncident, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncident;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Updates an incident in the system
		/// </summary>
		/// <param name="remoteIncident">The updated incident object</param>
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_IncidentIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyIncidents);
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
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateIncidentData(incident, remoteIncident);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value);

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
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident " + incident.ArtifactToken + ".");
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
		/// Deletes a incident in the system
		/// </summary>
		/// <param name="incidentId">The id of the incident</param>
		public void Incident_Delete(int incidentId)
		{
			const string METHOD_NAME = "Incident_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				IncidentManager incidentManager = new IncidentManager();
				Incident incident = incidentManager.RetrieveById(incidentId, false);

				//Make sure that the project ids match
				if (incident.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				incidentManager.MarkAsDeleted(projectId, incidentId, userId);
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_IncidentIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateIncidents);
			}

			//Default to the authenticated user if we have no opener provided
			int openerId = userId;
			if (remoteIncident.OpenerId.HasValue)
			{
				openerId = remoteIncident.OpenerId.Value;
			}

			//Always use the current project
			remoteIncident.ProjectId = projectId;

			//If we don't have a valid creation date, set it to the current date
			DateTime creationDate = DateTime.Now;
			if (remoteIncident.CreationDate.HasValue)
			{
				//We will convert to UTC later
				creationDate = remoteIncident.CreationDate.Value;
			}

			//First insert the new incident record itself, capturing and populating the id
			IncidentManager incidentManager = new IncidentManager();
			remoteIncident.IncidentId = incidentManager.Insert(
			   projectId,
			   remoteIncident.PriorityId,
			   remoteIncident.SeverityId,
			   openerId,
			   remoteIncident.OwnerId,
			   (remoteIncident.TestRunStepId.HasValue) ? new List<int>() { remoteIncident.TestRunStepId.Value } : null,
			   remoteIncident.Name,
			   remoteIncident.Description,
			   remoteIncident.DetectedReleaseId,
			   remoteIncident.ResolvedReleaseId,
			   remoteIncident.VerifiedReleaseId,
			   remoteIncident.IncidentTypeId,
			   remoteIncident.IncidentStatusId,
			   GlobalFunctions.UniversalizeDate(creationDate),
			   GlobalFunctions.UniversalizeDate(remoteIncident.StartDate),
			   GlobalFunctions.UniversalizeDate(remoteIncident.ClosedDate),
			   remoteIncident.EstimatedEffort,
			   remoteIncident.ActualEffort,
			   remoteIncident.RemainingEffort,
			   remoteIncident.FixedBuildId,
			   null,
			   userId
			   );

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident severities (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddIncidentSeverities);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident types (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddIncidentTypes);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident statuses (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddIncidentStatuses);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create incident priorities (project owner)
			if (!this.IsProjectOwner)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedAddIncidentPriorities);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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
					remoteIncidentType.WorkflowId = type.WorkflowId;
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//All project members can see the list of statuses, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentStatus> incidentStatuses = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);

				//Now populate the list of API data objects
				List<RemoteIncidentStatus> remoteIncidentStatuses = new List<RemoteIncidentStatus>();
				foreach (IncidentStatus status in incidentStatuses)
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
		/// PopulationFunctions.Populates the internal datarow from the API data object
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
			if (remoteIncident.IncidentStatusId.HasValue)
			{
				incident.IncidentStatusId = remoteIncident.IncidentStatusId.Value;
			}
			if (remoteIncident.IncidentTypeId.HasValue)
			{
				incident.IncidentTypeId = remoteIncident.IncidentTypeId.Value;
			}
			if (remoteIncident.OpenerId.HasValue)
			{
				incident.OpenerId = remoteIncident.OpenerId.Value;
			}
			incident.OwnerId = remoteIncident.OwnerId;
			incident.DetectedReleaseId = remoteIncident.DetectedReleaseId;
			incident.ResolvedReleaseId = remoteIncident.ResolvedReleaseId;
			incident.VerifiedReleaseId = remoteIncident.VerifiedReleaseId;
			incident.Name = remoteIncident.Name;
			incident.Description = remoteIncident.Description;
			if (remoteIncident.CreationDate.HasValue)
			{
				incident.CreationDate = GlobalFunctions.UniversalizeDate(remoteIncident.CreationDate.Value);
			}
			incident.StartDate = GlobalFunctions.UniversalizeDate(remoteIncident.StartDate);
			incident.ClosedDate = GlobalFunctions.UniversalizeDate(remoteIncident.ClosedDate);
			incident.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteIncident.LastUpdateDate);
			incident.EstimatedEffort = remoteIncident.EstimatedEffort;
			incident.ActualEffort = remoteIncident.ActualEffort;
			incident.RemainingEffort = remoteIncident.RemainingEffort;
			incident.BuildId = remoteIncident.FixedBuildId;
		}

		/// <summary>
		/// Will retrieve available transitions for the specied status ID for the currently logged-on user.
		/// </summary>
		/// <param name="currentTypeId">The current incident type</param>
		/// <param name="currentStatusId">The current incident status</param>
		/// <param name="isDetector">Is the user the detector of the incident</param>
		/// <param name="isOwner">Is the user the owner of the incident</param>
		/// <returns>The list of workflow transitions</returns>
		public List<RemoteWorkflowIncidentTransition> Incident_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isDetector, bool isOwner)
		{
			List<RemoteWorkflowIncidentTransition> retList = new List<RemoteWorkflowIncidentTransition>();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(AuthorizedProject).ProjectTemplateId;

			//Get the use's role in the project.
			int roleId = this.ProjectRoleId;

			//Get the workflow ID for the specified type.
			int workflowId = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == currentTypeId)
				{
					workflowId = incidentTypes[i].WorkflowId;
				}
			}

			WorkflowManager workflowManager = new Business.WorkflowManager();
			List<WorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, currentStatusId, roleId, isDetector, isOwner);

			foreach (WorkflowTransition transition in workflowTransitions)
			{
				RemoteWorkflowIncidentTransition wrkTransition = new RemoteWorkflowIncidentTransition();
				wrkTransition.ExecuteByDetector = transition.IsExecuteByDetector;
				wrkTransition.ExecuteByOwner = transition.IsExecuteByOwner;
				wrkTransition.IncidentStatusId_Input = transition.InputIncidentStatusId;
				wrkTransition.IncidentStatusName_Input = transition.InputStatus.Name;
				wrkTransition.IncidentStatusId_Output = transition.OutputIncidentStatusId;
				wrkTransition.IncidentStatusName_Output = transition.OutputStatus.Name;
				wrkTransition.Name = transition.Name;
				wrkTransition.TransitionId = transition.WorkflowTransitionId;
				wrkTransition.WorkflowId = transition.WorkflowId;

				retList.Add(wrkTransition);
			}

			return retList;
		}

		/// <summary>
		/// Retrieves the list of incident fields and their workflow status for a given type and status/step.
		/// </summary>
		/// <param name="currentTypeId">The current incident type</param>
		/// <param name="currentStatusId">The current incident status</param>
		/// <returns>The list of incident fields</returns>
		public List<RemoteWorkflowIncidentFields> Incident_RetrieveWorkflowFields(int currentTypeId, int currentStatusId)
		{
			List<RemoteWorkflowIncidentFields> retList = new List<RemoteWorkflowIncidentFields>();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(AuthorizedProject).ProjectTemplateId;

			//Get the workflow ID for the specified status.
			int workflowId = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == currentTypeId)
				{
					workflowId = incidentTypes[i].WorkflowId;
				}
			}

			//We need to translate the field states to the data expected by v3.0 API clients
			//They expect to get 1=Active instead of 1=Inactive. They also do not know how to handle 3=Hidden
			//So we need to get all the fields first, remove the inactive and then only send back the active ones

			//Retrieve all fields
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Incident);

			//Retrieve field states
			List<WorkflowField> workflowFields = new WorkflowManager().Workflow_RetrieveFieldStates(workflowId, currentStatusId);

			//First we need to add the Required fields, since that hasn't changed
			foreach (WorkflowField workflowField in workflowFields)
			{
				if (workflowField.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required)
				{
					retList.Add(new RemoteWorkflowIncidentFields(workflowField));
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
					remoteWorkflowIncidentField.FieldId = artifactField.ArtifactFieldId;
					remoteWorkflowIncidentField.FieldName = artifactField.Name;
					remoteWorkflowIncidentField.FieldStateId = 1;   //v3.0 API value for Active
					retList.Add(remoteWorkflowIncidentField);
				}
			}

			return retList;
		}

		/// <summary>
		/// Retrieves the list of incident custom properties and their workflow state for a given type and status/step.
		/// </summary>
		/// <param name="currentTypeId">The current incident type</param>
		/// <param name="currentStatusId">The current incident status</param>
		/// <returns>The list of incident custom property states</returns>
		public List<RemoteWorkflowIncidentCustomProperties> Incident_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId)
		{
			List<RemoteWorkflowIncidentCustomProperties> retList = new List<RemoteWorkflowIncidentCustomProperties>();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(AuthorizedProject).ProjectTemplateId;

			//Get the workflow ID for the specified status.
			int workflowId = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == currentTypeId)
				{
					workflowId = incidentTypes[i].WorkflowId;
				}
			}

			//We need to translate the field states to the data expected by v3.0 API clients
			//They expect to get 1=Active instead of 1=Inactive. They also do not know how to handle 3=Hidden
			//So we need to get all the fields first, remove the inactive and then only send back the active ones
			//Also only the text and list custom properties that have a LegacyName should be sent

			//Pull all incident custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

			//Pull workflow custom properties
			List<WorkflowCustomProperty> workflowCustomProperties = new WorkflowManager().Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

			//First we need to add the Required fields, since that hasn't changed
			foreach (WorkflowCustomProperty workflowCustomProperty in workflowCustomProperties)
			{
				if (workflowCustomProperty.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required)
				{
					retList.Add(new RemoteWorkflowIncidentCustomProperties(workflowCustomProperty));
				}
			}

			//Next we need to add the active legacy compatible custom properties
			foreach (CustomProperty customProperty in customProperties)
			{
				//Make sure it's not inactive or hidden
				bool fieldIsInactive = workflowCustomProperties.Any(w => (w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive || w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) && w.CustomPropertyId == customProperty.CustomPropertyId && !String.IsNullOrEmpty(customProperty.LegacyName));
				if (!fieldIsInactive)
				{
					//We have an active field to add
					RemoteWorkflowIncidentCustomProperties remoteWorkflowIncidentCustomProperty = new RemoteWorkflowIncidentCustomProperties();
					remoteWorkflowIncidentCustomProperty.CustomPropertyId = customProperty.CustomPropertyId;
					remoteWorkflowIncidentCustomProperty.FieldName = customProperty.LegacyName;
					remoteWorkflowIncidentCustomProperty.FieldStateId = 1;   //v3.0 API value for Active
					retList.Add(remoteWorkflowIncidentCustomProperty);
				}
			}

			return retList;
		}

		#endregion

		#region Release Methods
		/// <summary>Returns the number of releases that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Release_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Release_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new ReleaseManager().Count(-1, projectId, filters, 0, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Retrieves all the releases and iterations belonging to the current project
		/// </summary>
		/// <param name="activeOnly">Do we want just active releases?</param>
		/// <returns>List of releases</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewReleases);
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
				PopulationFunctions.PopulateRelease(remoteRelease, release);
				PopulationFunctions.PopulateCustomProperties(remoteRelease, release, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewReleases);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the release dataset
			ReleaseManager releaseManager = new ReleaseManager();
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(Business.UserManager.UserInternal, projectId, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

			//Populate the API data object and return
			List<RemoteRelease> remoteReleases = new List<RemoteRelease>();
			foreach (ReleaseView release in releases)
			{
				//Create and populate the row
				RemoteRelease remoteRelease = new RemoteRelease();
				PopulationFunctions.PopulateRelease(remoteRelease, release);
				PopulationFunctions.PopulateCustomProperties(remoteRelease, release, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewReleases);
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

				//Make sure that the project ids match
				if (release.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Populate the API data object and return
				RemoteRelease remoteRelease = new RemoteRelease();
				PopulationFunctions.PopulateRelease(remoteRelease, release);
				if (artifactCustomProperty != null)
				{
					PopulationFunctions.PopulateCustomProperties(remoteRelease, artifactCustomProperty);
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_ReleaseIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyReleases);
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
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Get copies of everything..
				Artifact notificationArt = release.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateReleaseData(release, remoteRelease);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value);

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
		/// Moves a release to another location in the hierarchy
		/// </summary>
		/// <param name="releaseId">The id of the release we want to move</param>
		/// <param name="destinationRelease">The id of the release it's to be inserted before in the list (or null to be at the end)</param>
		public void Release_Move(int releaseId, int? destinationReleaseId)
		{
			const string METHOD_NAME = "Release_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyReleases);
			}

			//First retrieve the releases to make sure they exists and are in the authorized project
			try
			{
				ReleaseManager releaseManager = new ReleaseManager();
				ReleaseView sourceRelease = releaseManager.RetrieveById2(projectId, releaseId);

				//Make sure that the project ids match
				if (sourceRelease.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}
				if (destinationReleaseId.HasValue)
				{
					ReleaseView destRelease = releaseManager.RetrieveById2(projectId, destinationReleaseId.Value);
					if (destRelease.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}
				}

				//Call the business object to actually perform the move
				releaseManager.Move(userId, projectId, releaseId, destinationReleaseId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested release");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Deletes a release in the system
		/// </summary>
		/// <param name="releaseId">The id of the release</param>
		public void Release_Delete(int releaseId)
		{
			const string METHOD_NAME = "Release_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				ReleaseManager releaseManager = new ReleaseManager();
				ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);

				//Make sure that the project ids match
				if (release.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				releaseManager.MarkAsDeleted(userId, projectId, releaseId);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
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
		/// Maps a release to a test case, so that the test case is needs to be tested for that release
		/// </summary>
		/// <param name="remoteReleaseTestCaseMappings">The release and test case mapping entries</param>
		/// <remarks>
		/// 1) If the mapping record already exists no error is raised
		/// 2) This version of the function supports an array of mappings and is faster for multiple adds
		/// </remarks>
		public void Release_AddTestMapping2(RemoteReleaseTestCaseMapping[] remoteReleaseTestCaseMappings)
		{
			const string METHOD_NAME = "Release_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			try
			{
				//Add the test case we want to use for mapping
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> releases = new List<int>();

				//First see how many releases we have
				foreach (RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping in remoteReleaseTestCaseMappings)
				{
					if (!releases.Contains(remoteReleaseTestCaseMapping.ReleaseId))
					{
						releases.Add(remoteReleaseTestCaseMapping.ReleaseId);
					}
				}

				//Iterate for each release and add the mappings
				foreach (int releaseId in releases)
				{
					List<int> testCaseIds = new List<int>();
					foreach (RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping in remoteReleaseTestCaseMappings)
					{
						if (remoteReleaseTestCaseMapping.ReleaseId == releaseId)
						{
							testCaseIds.Add(remoteReleaseTestCaseMapping.TestCaseId);
						}
					}
					testCaseManager.AddToRelease(projectId, releaseId, testCaseIds, userId);
				}
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
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

		/// <summary>
		/// Creates a new release in the system
		/// </summary>
		/// <param name="remoteRelease">The new release object (primary key will be empty)</param>
		/// <param name="parentReleaseId">Do we want to insert the release under a parent release</param>
		/// <returns>The populated release object - including the primary key</returns>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_ReleaseIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create releases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateReleases);
			}

			//Always use the current project
			remoteRelease.ProjectId = projectId;

			//Set a default creator if one not specified
			int creatorId = userId;
			if (remoteRelease.CreatorId.HasValue)
			{
				creatorId = remoteRelease.CreatorId.Value;
			}

			//If we have a passed in parent release, then we need to insert the release as a child item
			ReleaseManager releaseManager = new ReleaseManager();
			if (parentReleaseId.HasValue)
			{
				//We use user internal, since this web service shouldn't have to worry about what's collapsed
				ReleaseView release = releaseManager.RetrieveById2(projectId, parentReleaseId.Value);
				if (release == null)
				{
					throw CreateFault("ArtifactNotFound", Resources.Messages.Services_ReleaseNotFound);
				}
				List<ReleaseView> childReleases = releaseManager.RetrieveChildren(Business.UserManager.UserInternal, projectId, release.IndentLevel, false);

				//See if we have any existing child releases
				if (childReleases.Count > 0)
				{
					//Get the indent level of the last existing child
					string indentLevel = childReleases[childReleases.Count - 1].IndentLevel;

					//Now get the next indent level and use for that for the new item
					indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);

					//Now insert the release at the specified position
					remoteRelease.ReleaseId = releaseManager.Insert(
						userId,
						projectId,
						creatorId,
						remoteRelease.Name,
						remoteRelease.Description,
						remoteRelease.VersionNumber,
						indentLevel,
						remoteRelease.Active ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
						remoteRelease.Iteration ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
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
						creatorId,
						remoteRelease.Name,
						remoteRelease.Description,
						remoteRelease.VersionNumber,
						indentLevel,
						remoteRelease.Active ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
						remoteRelease.Iteration ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
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
				   creatorId,
				   remoteRelease.Name,
				   remoteRelease.Description,
				   remoteRelease.VersionNumber,
				   (int?)null,
				   remoteRelease.Active ? Release.ReleaseStatusEnum.InProgress : Release.ReleaseStatusEnum.Completed,
				   remoteRelease.Iteration ? Release.ReleaseTypeEnum.Iteration : Release.ReleaseTypeEnum.MajorRelease,
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
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			releaseManager.SendCreationNotification(remoteRelease.ReleaseId.Value, artifactCustomProperty, null);

			//Finally return the populated release object
			return remoteRelease;
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteRelease">The API data object</param>
		/// <param name="release">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateReleaseData(Release release, RemoteRelease remoteRelease)
		{
			if (remoteRelease.CreatorId.HasValue)
			{
				release.CreatorId = remoteRelease.CreatorId.Value;
			}
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
			release.TaskEstimatedEffort = remoteRelease.TaskEstimatedEffort;
			release.TaskActualEffort = remoteRelease.TaskActualEffort;

			//Concurrency Management
			release.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteRelease.LastUpdateDate);
		}

		/// <summary>Retrieves comments for a specified release.</summary>
		/// <param name="ReleaseId">The ID of the Release/Iteratyion to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified release.</returns>
		public List<RemoteComment> Release_RetrieveComments(int ReleaseId)
		{
			List<RemoteComment> retList = new List<RemoteComment>();

			if (ReleaseId > 0)
			{
				retList = this.commentRetrieve(ReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release);
			}

			return retList;
		}

		/// <summary>
		/// Retrieves the mapped test cases for a specific release
		/// </summary>
		/// <param name="releaseId">The id of the release</param>
		/// <returns>The list of mapped test cases</returns>
		public List<RemoteReleaseTestCaseMapping> Release_RetrieveTestMapping(int releaseId)
		{
			const string METHOD_NAME = "Release_RetrieveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to retrieve test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			try
			{
				//Retrieve the list of mapped test cases and convert to API object
				TestCaseManager testCaseManager = new TestCaseManager();
				List<RemoteReleaseTestCaseMapping> remoteReleaseTestCaseMappings = new List<RemoteReleaseTestCaseMapping>();
				List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
				foreach (TestCase testCase in mappedTestCases)
				{
					RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
					remoteReleaseTestCaseMapping.ReleaseId = releaseId;
					remoteReleaseTestCaseMapping.TestCaseId = testCase.TestCaseId;
					remoteReleaseTestCaseMappings.Add(remoteReleaseTestCaseMapping);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteReleaseTestCaseMappings;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates a new comment for a release.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Release_CreateComment(RemoteComment remoteComment)
		{
			RemoteComment retComment = this.createComment(remoteComment, DataModel.Artifact.ArtifactTypeEnum.Release);

			//Send Notification..
			//Pull the release.
			int releaseId = -1;
			try
			{
				ReleaseView release = new ReleaseManager().RetrieveById2(null, remoteComment.ArtifactId, true);
				releaseId = release.ReleaseId;
				if (release != null)
					new NotificationManager().SendNotificationForArtifact(release, null, remoteComment.Text);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + "Release_CreateComment()", ex, "Sending message for Release #" + releaseId + ".");
			}

			return retComment;
		}

		#endregion

		#region Requirement Methods
		/// <summary>Returns the number of requirements that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Requirement_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Requirement_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new RequirementManager().Count(-1, projectId, filters, 0, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Creates a new requirement record in the current project using the position offset method
		/// </summary>
		/// <param name="remoteRequirement">The new requirement object (primary key will be empty)</param>
		/// <param name="indentPosition">The number of columns to indent the requirement by (positive for indent, negative for outdent)</param>
		/// <returns>The populated requirement object - including the primary key</returns>
		/// <remarks>This version is use when you want to specify the relative indentation level</remarks>
		/// <example>
		/// 	spiraImportExport.Connection_Authenticate("aant", "aant");
		///		spiraImportExport.Connection_ConnectToProject(projectId1);
		///		//Lets add a nested tree of requirements
		///		//First the summary item
		///		RemoteRequirement remoteRequirement = new RemoteRequirement();
		///		remoteRequirement.StatusId = 1;
		///		remoteRequirement.Name = "Functionality Area";
		///		remoteRequirement.Description = String.Empty;
		///		remoteRequirement.AuthorId = userId1;
		///		remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
		///		requirementId1 = remoteRequirement.RequirementId.Value;
		///		//Detail Item 1
		///		remoteRequirement = new RemoteRequirement();
		///		remoteRequirement.StatusId = 2;
		///		remoteRequirement.ImportanceId = 1;
		///		remoteRequirement.ReleaseId = releaseId1;
		///		remoteRequirement.Name = "Requirement 1";
		///		remoteRequirement.Description = "Requirement Description 1";
		///		remoteRequirement.AuthorId = userId1;
		///		remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 1);
		///		requirementId2 = remoteRequirement.RequirementId.Value;
		/// </example>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_RequirementIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateRequirements);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Default to the authenticated user if we have no author provided
			int authorId = userId;
			if (remoteRequirement.AuthorId.HasValue)
			{
				authorId = remoteRequirement.AuthorId.Value;
			}

			//Default to the requested status if not value is provided
			Requirement.RequirementStatusEnum requirementStatus = Requirement.RequirementStatusEnum.Requested;
			if (remoteRequirement.StatusId.HasValue)
			{
				requirementStatus = (Requirement.RequirementStatusEnum)(remoteRequirement.StatusId.Value);
			}

			//The current project is always used
			remoteRequirement.ProjectId = projectId;

			//Convert the old planned effort into points
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			decimal? estimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

			//If we have a passed in parent requirement, then we need to insert the requirement as a child item

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
			   requirementStatus,
			   null,
			   authorId,
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
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);
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
		/// <example>
		/// remoteRequirement = new RemoteRequirement();
		///	remoteRequirement.ReleaseId = releaseId1;
		///	remoteRequirement.StatusId = 3;
		///	remoteRequirement.ImportanceId = 1;
		///	remoteRequirement.Name = "Test Child 1";
		///	remoteRequirement.Description = String.Empty;
		///	remoteRequirement.AuthorId = userId1;
		///	remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, requirementId5);
		///	int requirementId6 = remoteRequirement.RequirementId.Value;
		/// </example>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_RequirementIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateRequirements);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Default to the authenticated user if we have no author provided
			int authorId = userId;
			if (remoteRequirement.AuthorId.HasValue)
			{
				authorId = remoteRequirement.AuthorId.Value;
			}

			//Default to the requested status if not value is provided
			Requirement.RequirementStatusEnum requirementStatus = Requirement.RequirementStatusEnum.Requested;
			if (remoteRequirement.StatusId.HasValue)
			{
				requirementStatus = (Requirement.RequirementStatusEnum)(remoteRequirement.StatusId.Value);
			}

			//The current project is always used
			remoteRequirement.ProjectId = projectId;

			//Convert the old planned effort into points
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			decimal? estimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

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
					requirementStatus,
					null,
					authorId,
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
					requirementStatus,
					null,
					authorId,
					remoteRequirement.OwnerId,
					remoteRequirement.ImportanceId,
					remoteRequirement.Name,
					remoteRequirement.Description,
					estimatePoints,
					userId,
					false
				   );
			}

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			requirementManager.SendCreationNotification(remoteRequirement.RequirementId.Value, artifactCustomProperty, null);

			//Finally return the populated requirement object
			return remoteRequirement;
		}

		/// <summary>
		/// Moves a requirement to another location in the hierarchy
		/// </summary>
		/// <param name="requirementId">The id of the requirement we want to move</param>
		/// <param name="destinationRequirement">The id of the requirement it's to be inserted before in the list (or null to be at the end)</param>
		public void Requirement_Move(int requirementId, int? destinationRequirementId)
		{
			const string METHOD_NAME = "Requirement_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyRequirements);
			}

			//First retrieve the requirements to make sure they exists and are in the authorized project
			try
			{
				RequirementManager requirementManager = new RequirementManager();
				RequirementView sourceRequirement = requirementManager.RetrieveById2(projectId, requirementId);

				//Make sure that the project ids match
				if (sourceRequirement.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}
				if (destinationRequirementId.HasValue)
				{
					RequirementView destRequirement = requirementManager.RetrieveById2(projectId, destinationRequirementId.Value);
					if (destRequirement.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}
				}

				//Call the business object to actually perform the move
				requirementManager.Move(userId, projectId, requirementId, destinationRequirementId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested requirement");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Maps a requirement to a test case, so that the test case 'covers' the requirement
		/// </summary>
		/// <param name="remoteReqTestCaseMapping">The requirement and test case mapping entry</param>
		/// <remarks>If the coverage record already exists no error is raised</remarks>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
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
		/// Retrieves the test coverage for a specific requirement
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>The list of mapped test cases</returns>
		public List<RemoteRequirementTestCaseMapping> Requirement_RetrieveTestCoverage(int requirementId)
		{
			const string METHOD_NAME = "Requirement_RetrieveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to retrieve test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			try
			{
				//Retrieve the list of mapped test cases and convert to API object
				TestCaseManager testCaseManager = new TestCaseManager();
				List<RemoteRequirementTestCaseMapping> remoteRequirementTestCaseMappings = new List<RemoteRequirementTestCaseMapping>();
				List<TestCase> mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId);
				foreach (TestCase testCase in mappedTestCases)
				{
					RemoteRequirementTestCaseMapping remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
					remoteRequirementTestCaseMapping.RequirementId = requirementId;
					remoteRequirementTestCaseMapping.TestCaseId = testCase.TestCaseId;
					remoteRequirementTestCaseMappings.Add(remoteRequirementTestCaseMapping);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteRequirementTestCaseMappings;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all requirements owned by the currently authenticated user
		/// </summary>
		/// <returns>List of requirements</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the requirement dataset
			RequirementManager requirementManager = new RequirementManager();
			List<RequirementView> requirements = requirementManager.RetrieveByOwnerId(userId, null, null, false);

			//Get the custom property definitions - for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Requirement);

			//Populate the API data object and return
			List<RemoteRequirement> remoteRequirements = new List<RemoteRequirement>();
			foreach (RequirementView requirement in requirements)
			{
				//Create and populate the row
				RemoteRequirement remoteRequirement = new RemoteRequirement();
				PopulationFunctions.PopulateRequirement(remoteRequirement, requirement);
				PopulationFunctions.PopulateCustomProperties(remoteRequirement, requirement, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewRequirements);
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

				//Make sure that the project ids match
				if (requirement.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Populate the API data object and return
				RemoteRequirement remoteRequirement = new RemoteRequirement();
				PopulationFunctions.PopulateRequirement(remoteRequirement, requirement);
				if (artifactCustomProperty != null)
				{
					PopulationFunctions.PopulateCustomProperties(remoteRequirement, artifactCustomProperty);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewRequirements);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

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
					PopulationFunctions.PopulateRequirement(remoteRequirement, requirement);
					PopulationFunctions.PopulateCustomProperties(remoteRequirement, requirement, customProperties);
					remoteRequirements.Add(remoteRequirement);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteRequirements;
			}
			catch (Exception exception)
			{
				//Convert into a SOAP exception and throw
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Deletes a requirement in the system
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		public void Requirement_Delete(int requirementId)
		{
			const string METHOD_NAME = "Requirement_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				RequirementManager requirementManager = new RequirementManager();
				RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);

				//Make sure that the project ids match
				if (requirement.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				requirementManager.MarkAsDeleted(userId, projectId, requirementId);
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
		/// Updates a requirement in the system
		/// </summary>
		/// <param name="remoteRequirement">The updated requirement object</param>
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_RequirementIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update requirements
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyRequirements);
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
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateRequirementData(projectTemplateId, requirement, remoteRequirement);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value);

				//Convert from estimated effort to story points
				requirement.EstimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

				//Get copies of everything..
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
		/// PopulationFunctions.Populates the internal datarow from the API data object
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
			if (remoteRequirement.StatusId.HasValue)
			{
				requirement.RequirementStatusId = remoteRequirement.StatusId.Value;
			}
			if (remoteRequirement.AuthorId.HasValue)
			{
				requirement.AuthorId = remoteRequirement.AuthorId.Value;
			}
			requirement.OwnerId = remoteRequirement.OwnerId;
			requirement.ImportanceId = remoteRequirement.ImportanceId;
			requirement.ReleaseId = remoteRequirement.ReleaseId;
			requirement.Name = remoteRequirement.Name;
			requirement.Description = remoteRequirement.Description;
			requirement.CreationDate = GlobalFunctions.UniversalizeDate(remoteRequirement.CreationDate);
			requirement.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteRequirement.LastUpdateDate);
			requirement.IsSummary = remoteRequirement.Summary;
		}

		/// <summary>Retrieves comments for a specified requirement.</summary>
		/// <param name="RequirementId">The ID of the Requirement to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified requirement.</returns>
		public List<RemoteComment> Requirement_RetrieveComments(int RequirementId)
		{
			List<RemoteComment> retList = new List<RemoteComment>();

			if (RequirementId > 0)
			{
				retList = this.commentRetrieve(RequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a requirement.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Requirement_CreateComment(RemoteComment remoteComment)
		{
			RemoteComment retComment = this.createComment(remoteComment, DataModel.Artifact.ArtifactTypeEnum.Requirement);

			return retComment;
		}

		#endregion

		#region Test Case Methods
		/// <summary>Returns the number of test cases that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long TestCase_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestCase_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the count
			long retNum = new TestCaseManager().Count(projectId, filters, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Creates a new test case folder in the system
		/// </summary>
		/// <param name="remoteTestCase">The new test case object (primary key will be empty)</param>
		/// <param name="parentTestFolderId">Do we want to insert the test case under a parent folder</param>
		/// <returns>The populated test case object - including the primary key</returns>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestCaseIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestFolders);
			}

			//Default to the authenticated user if we have no author provided
			int authorId = userId;
			if (remoteTestCase.AuthorId.HasValue)
			{
				authorId = remoteTestCase.AuthorId.Value;
			}
			//Always use the current project
			remoteTestCase.ProjectId = projectId;

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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestCaseIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestCases);
			}

			//Default to the authenticated user if we have no author provided
			int authorId = userId;
			if (remoteTestCase.AuthorId.HasValue)
			{
				authorId = remoteTestCase.AuthorId.Value;
			}
			//Always use the current project
			remoteTestCase.ProjectId = projectId;

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTestCase.TestCasePriorityId >= 1 && remoteTestCase.TestCasePriorityId <= 4)
			{
				ConvertLegacyStaticIds(projectTemplateId, remoteTestCase);
			}

			//Now insert the test case
			remoteTestCase.TestCaseId = testCaseManager.Insert(
				userId,
				projectId,
				authorId,
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
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value);
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
		/// <returns>List of testCases</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testCases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

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
			string runningIndentLevel;
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();

			//Loop through the folders
			IGrouping<int?, TestCaseView> groupedTestCases;
			foreach (TestCaseFolderHierarchyView testCaseFolder in testCaseFolders)
			{
				//Add the folder item
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulationFunctions.PopulateTestCase(projectId, remoteTestCase, testCaseFolder);
				remoteTestCases.Add(remoteTestCase);

				//Add the testCases (if any)
				groupedTestCases = groupedTestCasesByFolder.FirstOrDefault(f => f.Key == testCaseFolder.TestCaseFolderId);
				if (groupedTestCases != null)
				{
					List<TestCaseView> groupedTestCaseList = groupedTestCases.ToList();
					if (groupedTestCaseList != null && groupedTestCaseList.Count > 0)
					{
						runningIndentLevel = testCaseFolder.IndentLevel + "AAA";
						foreach (TestCaseView testCase in groupedTestCaseList)
						{
							//Create and populate the row
							remoteTestCase = new RemoteTestCase();
							PopulationFunctions.PopulateTestCase(remoteTestCase, testCase, runningIndentLevel);
							PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);
							remoteTestCases.Add(remoteTestCase);

							//Increment the indent level -- Not used by this version of the API
							runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
						}
					}
				}
			}

			//Find the last used root indent level
			TestCaseFolderHierarchyView lastRootTestCaseFolder = testCaseFolders.Where(f => f.IndentLevel.Length == 3).OrderByDescending(f => f.IndentLevel).FirstOrDefault();
			if (lastRootTestCaseFolder == null)
			{
				runningIndentLevel = "AAA";
			}
			else
			{
				runningIndentLevel = HierarchicalList.IncrementIndentLevel(lastRootTestCaseFolder.IndentLevel);
			}

			//Add the root-folder testCases (if any)
			groupedTestCases = groupedTestCasesByFolder.FirstOrDefault(f => f.Key == null);
			if (groupedTestCases != null)
			{
				List<TestCaseView> groupedTestCaseList = groupedTestCases.ToList();
				if (groupedTestCaseList != null && groupedTestCaseList.Count > 0)
				{
					foreach (TestCaseView testCase in groupedTestCaseList)
					{
						//Create and populate the row
						RemoteTestCase remoteTestCase = new RemoteTestCase();
						PopulationFunctions.PopulateTestCase(remoteTestCase, testCase, runningIndentLevel);
						PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);
						remoteTestCases.Add(remoteTestCase);

						//Increment the indent level -- Not used by this version of the API
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testCases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

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
				PopulationFunctions.PopulateTestCase(remoteTestCase, testCase);
				PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(userId, null);

			//Get the custom property definitions - all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.TestCase);

			//Populate the API data object and return
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
			foreach (TestCaseView testCase in testCases)
			{
				//Create and populate the row
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulationFunctions.PopulateTestCase(remoteTestCase, testCase);
				PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);
				remoteTestCases.Add(remoteTestCase);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCases;
		}

		/// <summary>
		/// Retrieves all test cases that are stored in a particular test folder
		/// </summary>
		/// <param name="testCaseFolderId">The id of the test case folder</param>
		/// <returns>List of testCases</returns>
		public List<RemoteTestCase> TestCase_RetrieveByFolder(int testCaseFolderId)
		{
			const string METHOD_NAME = "TestCase_RetrieveByFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testCases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			//Call the business object to actually retrieve the test case dataset
			//If the test case was not found, just return null
			TestCaseManager testCaseManager = new TestCaseManager();
			//If the folder is negative, inverse
			if (testCaseFolderId < 0)
			{
				testCaseFolderId = -testCaseFolderId;
			}

			//First get the folder's indent
			TestCaseFolderHierarchyView testCaseFolder = testCaseManager.TestCaseFolder_GetList(projectId).FirstOrDefault(f => f.TestCaseFolderId == testCaseFolderId);
			if (testCaseFolder == null)
			{
				//Folder does not exist
				return new List<RemoteTestCase>();
			}

			//First get the test cases in the folder
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, testCaseFolderId);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

			//Populate the API data object and return
			List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
			string runningIndentLevel = testCaseFolder.IndentLevel + "AAA";
			foreach (TestCaseView testCase in testCases)
			{
				//Create and populate the row
				RemoteTestCase remoteTestCase = new RemoteTestCase();
				PopulationFunctions.PopulateTestCase(remoteTestCase, testCase, runningIndentLevel);
				PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);
				remoteTestCases.Add(remoteTestCase);

				//Increment indent levels
				runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
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
		/// <remarks>
		/// This also populates the test steps collection if you have the appropriate permissions
		/// positive = test case, negative = test folder
		/// </remarks>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
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

					//Make sure that the project ids match
					if (testCase.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Get the test case custom properties
					List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

					//Populate the API data object and return
					remoteTestCase = new RemoteTestCase();
					PopulationFunctions.PopulateTestCase(remoteTestCase, testCase);
					PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);

					//See if this user has the permissions to view test steps
					//Make sure we have permissions to view test steps
					if (this.ProjectRolePermissions != null && this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.View) != null)
					{
						//Populate the test step API data objects and return
						List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);

						//Get the test step custom property definitions
						customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);

						remoteTestCase.TestSteps = new List<RemoteTestStep>();
						foreach (TestStepView testStep in testSteps)
						{
							RemoteTestStep remoteTestStep = new RemoteTestStep();
							PopulationFunctions.PopulateTestStep(remoteTestStep, testStep);
							PopulationFunctions.PopulateCustomProperties(remoteTestStep, testStep, customProperties);
							remoteTestCase.TestSteps.Add(remoteTestStep);
						}
					}
				}
				else
				{
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(-testCaseId);

					//Populate the API data object and return
					remoteTestCase = new RemoteTestCase();
					PopulationFunctions.PopulateTestCase(remoteTestCase, testCaseFolder);
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
		/// <returns>List of Test Case objects</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			//Call the business object to actually retrieve the test set test case dataset
			TestSetManager testSetManager = new TestSetManager();

			//If the test case was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);

				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

				//Populate the API data object and return
				List<RemoteTestCase> remoteTestCases = new List<RemoteTestCase>();
				foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
				{
					RemoteTestCase remoteTestCase = new RemoteTestCase();
					remoteTestCases.Add(remoteTestCase);
					PopulationFunctions.PopulateTestCase(remoteTestCase, testSetTestCase);
					PopulationFunctions.PopulateCustomProperties(remoteTestCase, testSetTestCase, customProperties);
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
		/// Updates a test case / folder in the system together with its test steps (if populated)
		/// </summary>
		/// <param name="remoteTestCase">The updated test case object</param>
		/// <remarks>Does not currently update test step custom properties</remarks>
		public void TestCase_Update(RemoteTestCase remoteTestCase)
		{
			const string METHOD_NAME = "TestCase_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a testCase id specified
			if (!remoteTestCase.TestCaseId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_TestCaseIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
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
					TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, remoteTestCase.TestCaseId.Value);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestCase.TestCaseId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);

					//Make sure that the project ids match
					if (testCase.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
					//First the test case report
					UpdateTestCaseData(projectTemplateId, testCase, remoteTestCase);
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Now the test steps records
					if (remoteTestCase.TestSteps != null)
					{
						foreach (RemoteTestStep remoteTestStep in remoteTestCase.TestSteps)
						{
							//Locate the matching row
							if (remoteTestStep.TestStepId.HasValue)
							{
								TestStep testStep = testCase.TestSteps.FirstOrDefault(s => s.TestStepId == remoteTestStep.TestStepId.Value);
								if (testStep != null)
								{
									//Get the corresponding custom properties
									ArtifactCustomProperty testStepCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestStep.TestStepId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, true);
									UpdateTestStepData(testStep, remoteTestStep);
									UpdateCustomPropertyData(ref testStepCustomProperty, remoteTestStep, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, remoteTestStep.TestStepId.Value);

									//Save the custom properties
									customPropertyManager.ArtifactCustomProperty_Save(testStepCustomProperty, userId);
								}
							}
						}
					}

					//Extract changes for use in notifications
					Dictionary<string, object> changes = testCase.ExtractChanges();
					ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

					//Call the business object to actually update the testCase dataset
					testCaseManager.Update(testCase, userId);

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
		/// Moves a test step to a different position in the test case
		/// </summary>
		/// <param name="testCaseId">The id of the test case we're interested in</param>
		/// <param name="sourceTestStepId">The id of the test step we want to move</param>
		/// <param name="destinationTestStepId">The id of the test step we want to move it in front of (passing Null means end of the list)</param>
		public void TestCase_MoveStep(int testCaseId, int sourceTestStepId, int? destinationTestStepId)
		{
			const string METHOD_NAME = "TestCase_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			//First retrieve the test case to make sure it exists and are in the authorized project
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase sourceTestCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);

				//Make sure that the test case is in the authorized project
				if (sourceTestCase.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Make sure that both test step ids are in the test case
				bool sourceTestStepFound = false;
				bool destTestStepFound = false;
				foreach (TestStep testStep in sourceTestCase.TestSteps)
				{
					if (testStep.TestStepId == sourceTestStepId)
					{
						sourceTestStepFound = true;
					}
					if (!destinationTestStepId.HasValue || testStep.TestStepId == destinationTestStepId.Value)
					{
						destTestStepFound = true;
					}
				}
				if (!sourceTestStepFound)
				{
					throw CreateFault("TestStepNotInTestCase", "The source test step id cannot be found in the test case");
				}
				if (!destTestStepFound)
				{
					throw CreateFault("TestStepNotInTestCase", "The destination test step id cannot be found in the test case");
				}

				//Call the business object to actually perform the move
				testCaseManager.MoveStep(testCaseId, sourceTestStepId, destinationTestStepId, userId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Moves a test case to another folder in the hierarchy
		/// </summary>
		/// <param name="testCaseId">The id of the test case we want to move</param>
		/// <param name="destinationTestFolderId">The id of the folder case it's to be inserted before in the list (or null to be at the root)</param>
		public void TestCase_Move(int testCaseId, int? destinationTestFolderId)
		{
			const string METHOD_NAME = "TestCase_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			//First retrieve the test cases to make sure they exists and are in the authorized project
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseView sourceTestCase = testCaseManager.RetrieveById(projectId, testCaseId);

				//Make sure that the project ids match
				if (sourceTestCase.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}
				if (destinationTestFolderId.HasValue)
				{
					//Make sure the folder is positive
					if (destinationTestFolderId.Value < 0)
					{
						destinationTestFolderId = -destinationTestFolderId;
					}

					TestCaseFolder destTestCaseFolder = testCaseManager.TestCaseFolder_GetById(destinationTestFolderId.Value);
					if (destTestCaseFolder == null)
					{
						throw new ArtifactNotExistsException();
					}
					if (destTestCaseFolder.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}
				}

				//Call the business object to actually perform the move
				testCaseManager.TestCase_UpdateFolder(testCaseId, destinationTestFolderId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Deletes a test case in the system
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <remarks>
		/// Also deletes any child test steps
		/// </remarks>
		public void TestCase_Delete(int testCaseId)
		{
			const string METHOD_NAME = "TestCase_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();

				//See if we have a test case or folder
				if (testCaseId < 0)
				{
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(-testCaseId);
					if (testCaseFolder == null)
					{
						throw new ArtifactNotExistsException(Resources.Messages.TestCaseDetails_ArtifactNotExists);
					}

					//Make sure that the project ids match
					if (testCaseFolder.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Call the business object to actually mark the item as deleted
					testCaseManager.TestCaseFolder_Delete(projectId, -testCaseId, userId);
				}
				else
				{
					TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);

					//Make sure that the project ids match
					if (testCase.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Call the business object to actually mark the item as deleted
					testCaseManager.MarkAsDeleted(userId, projectId, testCaseId);
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
		/// Deletes a test step in the system
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="testStepId">The id of the test step</param>
		/// <remarks>Doesn't throw an exception if the step no longer exists</remarks>
		public void TestCase_DeleteStep(int testCaseId, int testStepId)
		{
			const string METHOD_NAME = "TestCase_DeleteStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
				if (testCase.TestSteps.Count == 0 || !testCase.TestSteps.Any(s => s.TestStepId == testStepId))
				{
					//Log a warning and just ignore if the step does not exist
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test step");
					return;
				}

				//Make sure that the project ids match
				if (testCase.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				testCaseManager.MarkStepAsDeleted(userId, testCaseId, testStepId);
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

		/// <summary>Adds or updates the automation test script associated with a test case</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="automationEngineId">The id of the automation engine</param>
		/// <param name="urlOrFilename">The url or filename for the test script</param>
		/// <param name="description">The description of the automation script</param>
		/// <param name="binaryData">The binary data that comprises the script (only for file attachments)</param>
		/// <param name="version">The version of the test script</param>
		/// <param name="projectAttachmentTypeId">The attachment type to store the script under (optional)</param>
		/// <param name="projectAttachmentFolderId">The attachment folder to store the script under (optional)</param>
		public void TestCase_AddUpdateAutomationScript(int testCaseId, Nullable<int> automationEngineId, string urlOrFilename, string description, byte[] binaryData, string version, Nullable<int> projectAttachmentTypeId, Nullable<int> projectAttachmentFolderId)
		{
			const string METHOD_NAME = "TestCase_AddUpdateAutomationScript";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			try
			{
				//Call the test case function to update the automation script
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.AddUpdateAutomationScript(userId, projectId, testCaseId, automationEngineId, urlOrFilename, description, binaryData, version, projectAttachmentTypeId, projectAttachmentFolderId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Returns the full token of a test case parameter from its name
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <returns>The tokenized representation of the parameter used for search/replace</returns>
		/// <remarks>We use the same parameter format as Ant/NAnt</remarks>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestCaseParameterIdNotNull);
			}


			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
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
		/// PopulationFunctions.Populates the internal datarow from the API data object
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

			if (remoteTestCase.AuthorId.HasValue)
			{
				testCase.AuthorId = remoteTestCase.AuthorId.Value;
			}
			testCase.OwnerId = remoteTestCase.OwnerId;
			testCase.TestCasePriorityId = remoteTestCase.TestCasePriorityId;
			testCase.Name = remoteTestCase.Name;
			testCase.Description = remoteTestCase.Description;
			testCase.CreationDate = GlobalFunctions.UniversalizeDate(remoteTestCase.CreationDate);
			testCase.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestCase.LastUpdateDate);
			testCase.EstimatedDuration = remoteTestCase.EstimatedDuration;
		}

		/// <summary>
		/// Adds a new test step to the specified test case
		/// </summary>
		/// <param name="remoteTestStep">The new test step object (primary key will be empty)</param>
		/// <param name="testCaseId">The test case to add it to</param>
		/// <returns>The populated test step object - including the primary key</returns>
		public RemoteTestStep TestCase_AddStep(RemoteTestStep remoteTestStep, int testCaseId)
		{
			const string METHOD_NAME = "TestCase_AddStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a test step id specified
			if (remoteTestStep.TestStepId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestStepIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestSteps);
			}

			try
			{
				//Instantiate the test case business class
				TestCaseManager testCaseManager = new TestCaseManager();

				//Now insert the test step
				remoteTestStep.TestStepId = testCaseManager.InsertStep(
				   userId,
				   testCaseId,
				   (remoteTestStep.Position < 1) ? null : (int?)remoteTestStep.Position,
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
		public int TestCase_AddLink(int testCaseId, int position, int linkedTestCaseId, List<RemoteTestStepParameter> parameters)
		{
			const string METHOD_NAME = "TestCase_AddLink";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test steps
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestSteps);
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
		/// Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestStep">The API data object</param>
		/// <param name="testStep">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateTestStepData(TestStep testStep, RemoteTestStep remoteTestStep)
		{
			testStep.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteTestStep.LastUpdateDate);
			testStep.StartTracking();

			testStep.TestCaseId = remoteTestStep.TestCaseId;
			if (remoteTestStep.ExecutionStatusId.HasValue)
			{
				testStep.ExecutionStatusId = remoteTestStep.ExecutionStatusId.Value;
			}
			testStep.Position = remoteTestStep.Position;
			testStep.Description = remoteTestStep.Description;
			testStep.ExpectedResult = remoteTestStep.ExpectedResult;
			testStep.SampleData = remoteTestStep.SampleData;
			testStep.LinkedTestCaseId = remoteTestStep.LinkedTestCaseId;
		}

		/// <summary>Retrieves comments for a specified test case.</summary>
		/// <param name="TestCaseId">The ID of the test case to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified test case.</returns>
		public List<RemoteComment> TestCase_RetrieveComments(int TestCaseId)
		{
			List<RemoteComment> retList = new List<RemoteComment>();

			if (TestCaseId > 0)
			{
				retList = this.commentRetrieve(TestCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a test case.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment TestCase_CreateComment(RemoteComment remoteComment)
		{
			RemoteComment retComment = this.createComment(remoteComment, DataModel.Artifact.ArtifactTypeEnum.TestCase);

			return retComment;
		}

		#endregion

		#region Test Run Methods

		/// <summary>Returns the number of test runs that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long TestRun_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestRunManager().Count(projectId, filters, 0);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Creates a new test run shell from the provided test case(s)
		/// </summary>
		/// <param name="testCaseIds">The list of test cases to create the run for</param>
		/// <param name="releaseId">A release to associate the test run with (optional)</param>
		/// <returns>The list of new test case run data objects</returns>
		/// <example>
		/// RemoteManualTestRun[] remoteTestRuns = spiraImportExport.TestRun_CreateFromTestCases(new int[] { testCaseId3, testCaseId4 }, iterationId1);
		/// </example>
		public List<RemoteManualTestRun> TestRun_CreateFromTestCases(List<int> testCaseIds, Nullable<int> releaseId)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestCases";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(userId, projectId, releaseId, testCaseIds, false);

				//Populate the API data object and return
				//We don't have any custom properties to populate at this point
				List<RemoteManualTestRun> remoteTestRuns = new List<RemoteManualTestRun>();
				foreach (TestRun testRun in testRunsPending.TestRuns)
				{
					RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun);
					remoteTestRuns.Add(remoteTestRun);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
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
		/// <returns>The list of new test case run data objects</returns>
		/// <example>
		/// RemoteManualTestRun[] remoteTestRuns = spiraImportExport.TestRun_CreateFromTestSet(testSetId1);
		/// </example>
		public List<RemoteManualTestRun> TestRun_CreateFromTestSet(int testSetId)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(userId, projectId, testSetId, false);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Populate the API data object and return
				//We don't have any custom properties to populate at this point
				List<RemoteManualTestRun> remoteTestRuns = new List<RemoteManualTestRun>();
				foreach (TestRun testRun in testRunsPending.TestRuns)
				{
					RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun);
					remoteTestRuns.Add(remoteTestRun);
				}

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

					//Populate the data objects if we have a row
					if (testRunArtifactCustomProperty != null)
					{
						foreach (RemoteTestRun remoteTestRun in remoteTestRuns)
						{
							PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRunArtifactCustomProperty);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds both test set and test case parameters to the API test run object
		/// </summary>
		/// <param name="remoteTestRun">The Test Run API object</param>
		/// <param name="testSetParameterValues">The list of test set parameter values</param>
		/// <param name="testSetTestCaseParameterValues">The list of test set parameter values</param>
		protected void AddParameterValues(RemoteAutomatedTestRun remoteTestRun, List<TestSetParameter> testSetParameterValues, List<TestSetTestCaseParameter> testSetTestCaseParameterValues)
		{
			if (testSetParameterValues.Count > 0 || testSetTestCaseParameterValues.Count > 0)
			{
				remoteTestRun.Parameters = new List<RemoteTestSetTestCaseParameter>();
				//Test Set/Case Parameters
				foreach (TestSetTestCaseParameter parameterValue in testSetTestCaseParameterValues)
				{
					RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter = new RemoteTestSetTestCaseParameter();
					remoteTestSetTestCaseParameter.Name = parameterValue.Name;
					remoteTestSetTestCaseParameter.Value = parameterValue.Value;
					remoteTestRun.Parameters.Add(remoteTestSetTestCaseParameter);
				}
				//Test Set Parameters
				foreach (TestSetParameter parameterValue in testSetParameterValues)
				{
					if (!remoteTestRun.Parameters.Any(p => p.Name == parameterValue.Name))
					{
						RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter = new RemoteTestSetTestCaseParameter();
						remoteTestSetTestCaseParameter.Name = parameterValue.Name;
						remoteTestSetTestCaseParameter.Value = parameterValue.Value;
						remoteTestRun.Parameters.Add(remoteTestSetTestCaseParameter);
					}
				}
			}

		}

		/// <summary>
		/// Creates a shell set of test runs for an external automated test runner based on the provided test set id
		/// </summary>
		/// <param name="testSetId">The automated test set we want to execute</param>
		/// <param name="automationHostToken">The unique token that identifies this host</param>
		/// <returns>The list of test run objects</returns>
		/// <example>
		/// spiraImportExport.Connection_Authenticate2(userName, password, engineName);
		/// spiraImportExport.Connection_ConnectToProject(projectId);
		/// RemoteAutomatedTestRun[] remoteTestRuns = spiraImportExport.TestRun_CreateForAutomatedTestSet(testSetId, token);
		/// </example>
		/// <remarks>For this method the test set doesn't need an automated host to be set</remarks>
		public List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(int testSetId, string automationHostToken)
		{
			const string METHOD_NAME = "TestRun_CreateForAutomatedTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Retrieve the automation host by its token
				AutomationManager automationManager = new AutomationManager();
				AutomationHostView automationHost = automationManager.RetrieveHostByToken(projectId, automationHostToken);
				int automationHostId = automationHost.AutomationHostId;

				//Retrieve the provided test set
				TestSetManager testSetManager = new TestSetManager();
				TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);

				//Make sure that this is an automated test set
				if (testSet.TestRunTypeId != (int)TestRun.TestRunTypeEnum.Automated)
				{
					throw new ApplicationException(Resources.Messages.Services_TestSetNotAutomated);
				}

				//Now we need to retieve any test set parameter values (used later)
				List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);

				//Create the list of automated test runs
				List<RemoteAutomatedTestRun> remoteTestRuns = new List<RemoteAutomatedTestRun>();

				TestCaseManager testCaseManager = new TestCaseManager();
				//Iterate through the test cases in the test set
				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
				foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
				{
					//Get the actual test case record for this item and make it has an automation engine and script
					TestCaseView testCase = testCaseManager.RetrieveById(projectId, testSetTestCase.TestCaseId);
					if (testCase.AutomationEngineId.HasValue && testCase.AutomationAttachmentId.HasValue)
					{
						//Get the automation engine token
						AutomationEngine automationEngine = automationManager.RetrieveEngineById(testCase.AutomationEngineId.Value);

						//Create the new automated test run shell
						RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
						remoteTestRun.ProjectId = projectId;
						remoteTestRun.AutomationHostId = automationHostId;
						remoteTestRun.AutomationEngineId = testCase.AutomationEngineId;
						remoteTestRun.AutomationEngineToken = automationEngine.Token;
						remoteTestRun.AutomationAttachmentId = testCase.AutomationAttachmentId;
						remoteTestRun.TestCaseId = testCase.TestCaseId;
						remoteTestRun.TestSetId = testSet.TestSetId;
						remoteTestRun.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
						remoteTestRun.ReleaseId = testSet.ReleaseId;
						remoteTestRun.Name = testCase.Name;
						remoteTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						remoteTestRun.RunnerName = testCase.AutomationEngineName;
						remoteTestRun.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
						remoteTestRun.TesterId = AuthenticatedUserId;
						remoteTestRuns.Add(remoteTestRun);

						//Now we need to add any test set and/or test case parameter values
						List<TestSetTestCaseParameter> testSetTestCaseParameterValues = testSetManager.RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);

						//Add parameters/values, test case ones override test set ones
						AddParameterValues(remoteTestRun, testSetParameterValues, testSetTestCaseParameterValues);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Don't log this because we get too many of them when called by RemoteLaunch
				throw ConvertExceptions(exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Creates a shell set of test runs for an external automated test runner based on the provided automation host token
		/// and the specified date range
		/// </summary>
		/// <param name="automationHostToken">The unique token that identifies this host</param>
		/// <param name="dateRange">The range of planned dates that we want to include test sets for</param>
		/// <returns>The list of test run objects</returns>
		/// <example>
		/// spiraImportExport.Connection_Authenticate2(userName, password, engineName);
		/// spiraImportExport.Connection_ConnectToProject(projectId);
		/// DateRange dateRange = new DateRange();
		/// dateRange.StartDate = DateTime.Now.AddHours(-1);
		/// dateRange.EndDate = DateTime.Now.AddHours(1);
		/// RemoteAutomatedTestRun[] remoteTestRuns = spiraImportExport.TestRun_CreateForAutomationHost(automationHostToken, dateRange);
		/// </example>
		public List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(string automationHostToken, DataObjects.DateRange dateRange)
		{
			const string METHOD_NAME = "TestRun_CreateForAutomationHost";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the business classes
				AutomationManager automationManager = new AutomationManager();
				AutomationHostView automationHost = automationManager.RetrieveHostByToken(projectId, automationHostToken);
				int automationHostId = automationHost.AutomationHostId;

				//See if we have any assigned test sets for this automation host
				TestSetManager testSetManager = new TestSetManager();
				List<TestSetView> testSets = testSetManager.RetrieveByAutomationHostId(automationHostId, GlobalFunctions.UniversalizeDate(dateRange.StartDate), GlobalFunctions.UniversalizeDate(dateRange.EndDate));

				//If we have no test sets, just return null quickly to avoid load on server
				if (testSets.Count < 1)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return null;
				}

				//Create the list of automated test runs
				List<RemoteAutomatedTestRun> remoteTestRuns = new List<RemoteAutomatedTestRun>();

				//Iterate through the list of test sets
				TestCaseManager testCaseManager = new TestCaseManager();
				foreach (TestSetView testSetRow in testSets)
				{
					int testSetId = testSetRow.TestSetId;

					//Now we need to retieve any test set parameter values (used later)
					List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);

					//Iterate through the test cases in the test set
					List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
					foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
					{
						//Get the actual test case record for this item and make it has an automation engine and script
						TestCaseView testCase = testCaseManager.RetrieveById(projectId, testSetTestCase.TestCaseId);
						if (testCase.AutomationEngineId.HasValue && testCase.AutomationAttachmentId.HasValue)
						{
							//Get the automation engine token
							AutomationEngine automationEngine = automationManager.RetrieveEngineById(testCase.AutomationEngineId.Value);

							//Create the new automated test run shell
							RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
							remoteTestRun.ProjectId = projectId;
							remoteTestRun.AutomationHostId = automationHostId;
							remoteTestRun.AutomationEngineId = testCase.AutomationEngineId;
							remoteTestRun.AutomationEngineToken = automationEngine.Token;
							remoteTestRun.AutomationAttachmentId = testCase.AutomationAttachmentId;
							remoteTestRun.TestCaseId = testCase.TestCaseId;
							remoteTestRun.TestSetId = testSetRow.TestSetId;
							remoteTestRun.ScheduledDate = GlobalFunctions.LocalizeDate(testSetRow.PlannedDate);
							remoteTestRun.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
							remoteTestRun.ReleaseId = testSetRow.ReleaseId;
							remoteTestRun.Name = testCase.Name;
							remoteTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
							remoteTestRun.RunnerName = testCase.AutomationEngineName;
							remoteTestRun.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
							remoteTestRun.TesterId = AuthenticatedUserId;
							remoteTestRuns.Add(remoteTestRun);

							//Now we need to add any test set and/or test case parameter values
							List<TestSetTestCaseParameter> testSetTestCaseParameterValues = testSetManager.RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);

							//Add parameters/values, test case ones override test set ones
							AddParameterValues(remoteTestRun, testSetParameterValues, testSetTestCaseParameterValues);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Don't log this because we get too many of them when called by RemoteLaunch
				throw ConvertExceptions(exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Creates a shell set of test runs for an external automated test runner based on the provided automation host token.
		/// This version only includes test sets that have not been started
		/// </summary>
		/// <param name="automationHostToken">The unique token that identifies this host</param>
		/// <param name="dateRange">The range of planned dates that we want to include test sets for</param>
		/// <param name="includeOverdueItems">Include overdue items</param>
		/// <returns>The list of test run objects</returns>
		/// <example>
		/// spiraImportExport.Connection_Authenticate2(userName, password, engineName);
		/// spiraImportExport.Connection_ConnectToProject(projectId);
		/// RemoteAutomatedTestRun[] remoteTestRuns = spiraImportExport.TestRun_CreateForAutomationHost2(automationHostToken);
		/// </example>
		public List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost2(string automationHostToken)
		{
			const string METHOD_NAME = "TestRun_CreateForAutomationHost2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the business classes
				AutomationManager automationManager = new AutomationManager();
				AutomationHostView automationHost = automationManager.RetrieveHostByToken(projectId, automationHostToken);
				int automationHostId = automationHost.AutomationHostId;

				//See if we have any assigned test sets for this automation host
				TestSetManager testSetManager = new TestSetManager();
				List<TestSetView> testSets = testSetManager.RetrieveByAutomationHostId(automationHostId);

				//If we have no test sets, just return null quickly to avoid load on server
				if (testSets.Count < 1)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return null;
				}

				//Create the list of automated test runs
				List<RemoteAutomatedTestRun> remoteTestRuns = new List<RemoteAutomatedTestRun>();

				//Iterate through the list of test sets
				TestCaseManager testCaseManager = new TestCaseManager();
				foreach (TestSetView testSet in testSets)
				{
					int testSetId = testSet.TestSetId;

					//Now we need to retieve any test set parameter values (used later)
					List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);

					//Iterate through the test cases in the test set
					List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
					foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
					{
						//Get the actual test case record for this item and make it has an automation engine and script
						TestCaseView testCase = testCaseManager.RetrieveById(projectId, testSetTestCase.TestCaseId);
						if (testCase.AutomationEngineId.HasValue && testCase.AutomationAttachmentId.HasValue)
						{
							//Get the automation engine token
							AutomationEngine automationEngine = automationManager.RetrieveEngineById(testCase.AutomationEngineId.Value);

							//Create the new automated test run shell
							RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
							remoteTestRun.ProjectId = projectId;
							remoteTestRun.AutomationHostId = automationHostId;
							remoteTestRun.AutomationEngineId = testCase.AutomationEngineId;
							remoteTestRun.AutomationEngineToken = automationEngine.Token;
							remoteTestRun.AutomationAttachmentId = testCase.AutomationAttachmentId;
							remoteTestRun.TestCaseId = testCase.TestCaseId;
							remoteTestRun.TestSetId = testSet.TestSetId;
							remoteTestRun.ScheduledDate = GlobalFunctions.LocalizeDate(testSet.PlannedDate);
							remoteTestRun.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
							remoteTestRun.ReleaseId = testSet.ReleaseId;
							remoteTestRun.Name = testCase.Name;
							remoteTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
							remoteTestRun.RunnerName = testCase.AutomationEngineName;
							remoteTestRun.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
							remoteTestRun.TesterId = AuthenticatedUserId;
							remoteTestRuns.Add(remoteTestRun);

							//Now we need to add any test set and/or test case parameter values
							List<TestSetTestCaseParameter> testSetTestCaseParameterValues = testSetManager.RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);

							//Add parameters/values, test case ones override test set ones
							AddParameterValues(remoteTestRun, testSetParameterValues, testSetTestCaseParameterValues);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Don't log this because we get too many of them when called by RemoteLaunch
				throw ConvertExceptions(exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw ConvertExceptions(exception);
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testRuns
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

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
				PopulationFunctions.PopulateTestRun(remoteTestRun, testRun);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRun, customProperties);
				remoteTestRuns.Add(remoteTestRun);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestRuns;
		}

		/// <summary>Retrieves a list of automated test runs in the system that match the provided filter/sort</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		public List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_RetrieveAutomated";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testRuns
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object and add the automated filter.
			if (remoteFilters == null) remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { IntValue = (int)TestRun.TestRunTypeEnum.Automated, PropertyName = "TestRunTypeId" });
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testRun dataset
			List<TestRunView> testRuns = new TestRunManager().Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

			//Populate the API data object and return
			List<RemoteAutomatedTestRun> remoteTestRuns = new List<RemoteAutomatedTestRun>();
			foreach (TestRunView testRun in testRuns)
			{
				//Create and populate the row
				RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
				PopulationFunctions.PopulateAutomatedTestRun(remoteTestRun, testRun);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRun, customProperties);
				remoteTestRuns.Add(remoteTestRun);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return remoteTestRuns;
		}

		/// <summary>Retrieves a list of manual test runs in the system that match the provided filter/sort</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="remoteSort">The sort to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Does include the test run steps</remarks>
		public List<RemoteManualTestRun> TestRun_RetrieveManual(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_RetrieveManual";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testRuns
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object and add the manual filter.
			if (remoteFilters == null) remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { IntValue = (int)TestRun.TestRunTypeEnum.Manual, PropertyName = "TestRunTypeId" });
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Create the manager..
			TestRunManager testRunManager = new TestRunManager();
			//Call the business object to actually retrieve the testRun dataset
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

			//Populate the API data object and return
			List<RemoteManualTestRun> remoteTestRuns = new List<RemoteManualTestRun>();
			foreach (TestRunView testRunView in testRuns)
			{
				//Get the manual test with test steps..
				TestRun testRunWithSteps = testRunManager.RetrieveByIdWithSteps(testRunView.TestRunId);

				if (testRunWithSteps != null)
				{
					//Create and populate the row
					RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRunWithSteps);
					PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRunWithSteps, customProperties);
					remoteTestRuns.Add(remoteTestRun);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return remoteTestRuns;
		}

		/// <summary>
		/// Retrieves a single test run in the system. Only returns the generic information
		/// that is applicable for both automated and manual tests. Consider using
		/// TestRun_RetrieveAutomatedById or TestRun_RetrieveManualById if you need
		/// the automation/manual specific data for the test run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>Test Run object</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
			}

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test run was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulationFunctions.PopulateTestRun(remoteTestRun, testRun);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

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
		/// Retrieves a single automated test run in the system including the automation-specific information
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>Test Run object</returns>
		public RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(int testRunId)
		{
			const string METHOD_NAME = "TestRun_RetrieveAutomatedById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
			}

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test run was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
				PopulationFunctions.PopulateAutomatedTestRun(remoteTestRun, testRun);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

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
		/// Retrieves a single manual test run in the system including any associated steps
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>Test Run object</returns>
		public RemoteManualTestRun TestRun_RetrieveManualById(int testRunId)
		{
			const string METHOD_NAME = "TestRun_RetrieveManualById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestRuns);
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
				RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
				PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

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
		/// <param name="remoteTestRun">The automated test run information</param>
		/// <returns>the test run data object with its primary key populated</returns>
		/// <remarks>
		/// You need to use this overload when you want to be able to set Test Run custom properties
		/// </remarks>
		/// <example>
		/// remoteTestRun = new RemoteAutomatedTestRun();
		///	remoteTestRun.TestCaseId = testCaseId1;
		///	remoteTestRun.ReleaseId = iterationId2;
		///	remoteTestRun.StartDate = DateTime.Now;
		///	remoteTestRun.EndDate = DateTime.Now.AddMinutes(2);
		/// remoteTestRun.ExecutionStatusId = Business.TestCase.ExecutionStatusPassed;
		/// remoteTestRun.RunnerName = "TestSuite";
		/// remoteTestRun.RunnerTestName = "02_Test_Method";
		/// testRunId3 = spiraImportExport.TestRun_RecordAutomated1(remoteTestRun).TestRunId.Value;
		/// </example>
		public RemoteAutomatedTestRun TestRun_RecordAutomated1(RemoteAutomatedTestRun remoteTestRun)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a runner name was provided (needed for automated tests)
			if (String.IsNullOrEmpty(remoteTestRun.RunnerName))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_RunnerNameMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Default to the authenticated user if we have no tester provided
				int testerId = userId;
				if (remoteTestRun.TesterId.HasValue)
				{
					testerId = remoteTestRun.TesterId.Value;
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
				   testerId,
				   remoteTestRun.TestCaseId,
				   remoteTestRun.ReleaseId,
				   remoteTestRun.TestSetId,
				   remoteTestRun.TestSetTestCaseId,
				   GlobalFunctions.UniversalizeDate(remoteTestRun.StartDate),
				   endDate, //Already in UTC
				   remoteTestRun.ExecutionStatusId,
				   remoteTestRun.RunnerName,
				   runnerTestName,
				   runnerAssertCount,
				   runnerMessage,
				   runnerStackTrace,
				   remoteTestRun.AutomationHostId,
				   remoteTestRun.AutomationEngineId,
				   remoteTestRun.BuildId,
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
		/// <param name="runnerTestName">The name of the test case in the external tool</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <param name="testSetTestCaseId">The id of the unique test case entry in the test set (if none provided, the first matching test case is used)</param>
		/// <returns>The newly created test run id</returns>
		/// <remarks>
		/// Use this version of the method for clients that cannot handle session cookies or complex data objects.
		/// Unlike the TestRun_RecordAutomated1 it cannot handle custom properties
		/// </remarks>
		/// <example>
		/// int testRunId5 = spiraImportExport.TestRun_RecordAutomated2("aant", "aant", projectId1, userId2, testCaseId1, iterationId2, null, null, DateTime.Now, DateTime.Now.AddSeconds(20), Business.TestCase.ExecutionStatusFailed, "TestSuite", "02_Test_Method", 5, "Expected 1, Found 0", "Error Stack Trace........");
		/// </example>
		public int TestRun_RecordAutomated2(string userName, string password, int projectId, int testerUserId, int testCaseId, Nullable<int> releaseId, Nullable<int> testSetId, Nullable<int> testSetTestCaseId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a runner name was provided (needed for automated tests)
			if (String.IsNullOrEmpty(runnerName))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_RunnerNameMissing);
			}

			//Make sure we have an authenticated user
			if (!this.Connection_Authenticate(userName, password))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!this.Connection_ConnectToProject(projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationFailure", Resources.Messages.Services_CannotConnectToProject);
			}

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
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
				   testSetTestCaseId,
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
		/// Records the results of executing multiple automated tests
		/// </summary>
		/// <returns>the test run data object with its primary key populated</returns>
		/// <remarks>
		/// You need to use this overload when you want to be able to execute a large batch of test runs.
		/// It's faster than TestRun_RecordAutomated1 for large numbers of test runs.
		/// *However* it does not refresh any of the other items in the project (test cases, requirements, test sets)
		/// that also have summarized forms of this data. So once you're done loading data, you *must*
		/// call the Project_RefreshProgressExecutionStatusCaches() command once.
		/// </remarks>
		/// <param name="remoteTestRuns">The list of populated automated test runs</param>
		/// <returns>The list of test runs with the TestRunId populated</returns>
		/// <seealso cref="TestRun_RecordAutomated1"/>
		public List<RemoteAutomatedTestRun> TestRun_RecordAutomated3(List<RemoteAutomatedTestRun> remoteTestRuns)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated3";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a runner name was provided (needed for automated tests)
			foreach (RemoteAutomatedTestRun remoteTestRun in remoteTestRuns)
			{
				if (String.IsNullOrEmpty(remoteTestRun.RunnerName))
				{
					//Throw back an exception
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();

					throw CreateFault("ArgumentMissing", Resources.Messages.Services_RunnerNameMissing);
				}
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Loop through the test runs
				foreach (RemoteAutomatedTestRun remoteTestRun in remoteTestRuns)
				{
					//Default to the authenticated user if we have no tester provided
					int testerId = userId;
					if (remoteTestRun.TesterId.HasValue)
					{
						testerId = remoteTestRun.TesterId.Value;
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
					//We use the option to NOT update associated test statuses for the test run
					//because we will do that at the end for ALL the test runs
					int testRunId = testRunManager.Record(
					   projectId,
					   testerId,
					   remoteTestRun.TestCaseId,
					   remoteTestRun.ReleaseId,
					   remoteTestRun.TestSetId,
					   remoteTestRun.TestSetTestCaseId,
					   GlobalFunctions.UniversalizeDate(remoteTestRun.StartDate),
					   endDate, //Already in UTC
					   remoteTestRun.ExecutionStatusId,
					   remoteTestRun.RunnerName,
					   runnerTestName,
					   runnerAssertCount,
					   runnerMessage,
					   runnerStackTrace,
					   remoteTestRun.AutomationHostId,
					   remoteTestRun.AutomationEngineId,
					   remoteTestRun.BuildId,
					   TestRun.TestRunFormatEnum.PlainText,
					   null,
					   false
					   );

					//Need to now save the custom properties if necessary
					ArtifactCustomProperty artifactCustomProperty = null;
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Populate the API data object with the primary key and return
					remoteTestRun.TestRunId = testRunId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRuns;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
		/// <param name="testRunsPending">The internal dataset</param>
		/// <remarks>Updates both the test run and the test run steps</remarks>
		protected void UpdateManualTestRunData(TestRunsPending testRunsPending, RemoteManualTestRun remoteTestRun)
		{
			testRunsPending.StartTracking();

			//First populate the test run itself
			TestRun testRun = new TestRun();
			testRunsPending.TestRuns.Add(testRun);
			testRun.Name = remoteTestRun.Name;
			testRun.TestCaseId = remoteTestRun.TestCaseId;
			testRun.TestRunTypeId = remoteTestRun.TestRunTypeId;
			if (remoteTestRun.TesterId.HasValue)
			{
				testRun.TesterId = remoteTestRun.TesterId.Value;
			}
			testRun.ExecutionStatusId = remoteTestRun.ExecutionStatusId;
			testRun.ReleaseId = remoteTestRun.ReleaseId;
			testRun.TestSetId = remoteTestRun.TestSetId;
			testRun.StartDate = GlobalFunctions.UniversalizeDate(remoteTestRun.StartDate);
			testRun.EndDate = GlobalFunctions.UniversalizeDate(remoteTestRun.EndDate);
			testRun.IsAttachments = false;

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

			//Concurrency management
			testRun.ConcurrencyDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Saves set of test runs, each containing test run steps
		/// </summary>
		/// <param name="remoteTestRuns">The test run objects to persist</param>
		/// <param name="endDate">The effective end-date of the test run</param>
		/// <returns>The saved copy of the test run objects (contains generated IDs)</returns>
		/// <example>
		/// RemoteManualTestRun[] remoteTestRuns = spiraImportExport.TestRun_Save(remoteTestRuns, DateTime.Now);
		/// </example>
		public List<RemoteManualTestRun> TestRun_Save(List<RemoteManualTestRun> remoteTestRuns, DateTime endDate)
		{
			const string METHOD_NAME = "TestRun_Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test runs
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestRuns);
			}

			try
			{
				//Instantiate the test run business class and dataset
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TestRunsPending testRunsPending = new TestRunsPending();
				List<RemoteManualTestRun> updatedRemoteTestRuns = new List<RemoteManualTestRun>();

				//First populate the pending run
				UpdatePendingTestRun(testRunsPending, remoteTestRuns, projectId, userId);

				//Loop through the test runs
				for (int i = 0; i < remoteTestRuns.Count; i++)
				{
					RemoteManualTestRun remoteTestRun = remoteTestRuns[i];
					//Make sure the test run has a tester set, if not, use the authenticated user
					if (!remoteTestRun.TesterId.HasValue)
					{
						remoteTestRun.TesterId = userId;
					}

					//Populate the dataset from the test run object
					UpdateManualTestRunData(testRunsPending, remoteTestRun);

					//Update the status of the test run
					testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, i, GlobalFunctions.UniversalizeDate(endDate), false);
				}

				//Actually save the test run and get the primary keys
				testRunManager.Save(testRunsPending, projectId, false);
				int testRunsPendingId = testRunsPending.TestRunsPendingId;
				for (int i = 0; i < remoteTestRuns.Count; i++)
				{
					RemoteManualTestRun remoteTestRun = remoteTestRuns[i];
					remoteTestRun.TestRunId = testRunsPending.TestRuns[i].TestRunId;

					//Need to now save the custom properties if necessary
					ArtifactCustomProperty artifactCustomProperty = null;
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, remoteTestRun.TestRunId.Value);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Finally we need to return the fully populated test run (after saving), it's easiest just to
					//regenerate from the test run dataset
					RemoteManualTestRun updatedRemoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(updatedRemoteTestRun, testRunsPending.TestRuns[i]);
					updatedRemoteTestRuns.Add(updatedRemoteTestRun);
				}

				//Also need to complete the pending run so that it is no longer displayed
				testRunManager.CompletePending(testRunsPendingId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return updatedRemoteTestRuns;
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
		/// <summary>Returns the number of test sets that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long TestSet_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestSet_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestSetManager().Count(projectId, filters, 0, null, false, true);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Creates a new test set folder in the system
		/// </summary>
		/// <param name="remoteTestSet">The new test set object (primary key will be empty)</param>
		/// <param name="parentTestSetFolderId">Do we want to insert the test set under a parent folder</param>
		/// <returns>The populated test set object - including the primary key</returns>
		/// <remarks>Test set folder ids are negative</remarks>
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestSetIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestFolders);
			}

			//Default to the authenticated user if we have no creator provided
			int creatorId = userId;
			if (remoteTestSet.CreatorId.HasValue)
			{
				creatorId = remoteTestSet.CreatorId.Value;
			}
			//Always use the current project
			remoteTestSet.ProjectId = projectId;

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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TestSetIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTestSets);
			}

			//Default to the authenticated user if we have no creator provided
			int creatorId = userId;
			if (remoteTestSet.CreatorId.HasValue)
			{
				creatorId = remoteTestSet.CreatorId.Value;
			}
			//Always use the current project
			remoteTestSet.ProjectId = projectId;

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
				creatorId,
				remoteTestSet.OwnerId,
				(TestSet.TestSetStatusEnum)remoteTestSet.TestSetStatusId,
				remoteTestSet.Name,
				remoteTestSet.Description,
				GlobalFunctions.UniversalizeDate(remoteTestSet.PlannedDate),
				TestRun.TestRunTypeEnum.Manual,
				remoteTestSet.AutomationHostId,
				(remoteTestSet.RecurrenceId.HasValue) ? (TestSet.RecurrenceEnum?)remoteTestSet.RecurrenceId : null
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			testSetManager.SendCreationNotification(remoteTestSet.TestSetId.Value, artifactCustomProperty, null);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSet;
		}

		/// <summary>
		/// Retrieves all the test cases that are part of a test set
		/// </summary>
		/// <param name="testSetId">The id of the test set</param>
		/// <returns>List of Test Set, Test Case mapping objects</returns>
		public List<RemoteTestSetTestCaseMapping> TestSet_RetrieveTestCaseMapping(int testSetId)
		{
			const string METHOD_NAME = "TestSet_RetrieveTestCaseMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
			}

			//Call the business object to actually retrieve the test set test case dataset
			TestSetManager testSetManager = new TestSetManager();

			//If the test case was not found, just return null
			try
			{
				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);

				//Populate the API data object and return
				List<RemoteTestSetTestCaseMapping> remoteTestSetTestCaseMappings = new List<RemoteTestSetTestCaseMapping>();
				foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
				{
					RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
					remoteTestSetTestCaseMappings.Add(remoteTestSetTestCaseMapping);

					//Populate fields
					remoteTestSetTestCaseMapping.TestSetTestCaseId = testSetTestCase.TestSetTestCaseId;
					remoteTestSetTestCaseMapping.TestSetId = testSetTestCase.TestSetId;
					remoteTestSetTestCaseMapping.TestCaseId = testSetTestCase.TestCaseId;
					remoteTestSetTestCaseMapping.OwnerId = testSetTestCase.OwnerId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestSetTestCaseMappings;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test case");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Retrieves a list of testSets in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of testSets</returns>
		/// <example>
		/// spiraImportExport.Connection_Authenticate("fredbloggs", "fredbloggs");
		/// spiraImportExport.Connection_ConnectToProject(1);
		/// List&lt;RemoteFilter&gt; remoteFilters = new List&lt;RemoteFilter&gt;();
		/// RemoteFilter remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "OwnerId";
		/// remoteFilter.MultiValue = new MultiValueFilter();
		/// List&lt;int&gt; multiValues = remoteFilter.MultiValue.Values;
		/// multiValues.Add(2);
		/// multiValues.Add(3);
		/// remoteFilters.Add(remoteFilter);
		/// remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "TestSetStatusId";
		/// remoteFilter.MultiValue = new MultiValueFilter();
		/// multiValues = remoteFilter.MultiValue.Values;
		/// multiValues.Add((int)Task.TaskStatus.NotStarted);
		/// multiValues.Add((int)Task.TaskStatus.InProgress);
		/// remoteFilters.Add(remoteFilter);
		/// remoteFilter = new RemoteFilter();
		/// remoteFilter.PropertyName = "PlannedDate";
		/// remoteFilter.DateRangeValue = new DateRange();
		/// remoteFilter.DateRangeValue.StartDate = DateTime.Parse("2/1/2007");
		/// remoteFilter.DateRangeValue.EndDate = DateTime.Parse("2/28/2007");
		/// remoteFilters.Add(remoteFilter);
		/// RemoteTestSet[] remoteTestSets = spiraImportExport.TestSet_Retrieve(remoteFilters.ToArray(), 1, 999999);
		/// </example>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view testSets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestSets);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testSet dataset
			TestSetManager testSetManager = new TestSetManager();
			List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, startingRow, numberOfRows, filters, 0, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);

			//Get the complete folder list for the project
			List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);

			//All Items / Filtered List - we need to group under the appropriate folder
			List<IGrouping<int?, TestSetView>> groupedTestSetsByFolder = testSets.GroupBy(t => t.TestSetFolderId).ToList();

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);

			//Populate the API data object and return
			string runningIndentLevel;
			List<RemoteTestSet> remoteTestSets = new List<RemoteTestSet>();

			//Loop through the folders
			IGrouping<int?, TestSetView> groupedTestSets;
			foreach (TestSetFolderHierarchyView testSetFolder in testSetFolders)
			{
				//Add the folder item
				RemoteTestSet remoteTestSet = new RemoteTestSet();
				PopulationFunctions.PopulateTestSet(projectId, remoteTestSet, testSetFolder);
				remoteTestSets.Add(remoteTestSet);

				//Add the testSets (if any)
				groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == testSetFolder.TestSetFolderId);
				if (groupedTestSets != null)
				{
					List<TestSetView> groupedTestSetList = groupedTestSets.ToList();
					if (groupedTestSetList != null && groupedTestSetList.Count > 0)
					{
						runningIndentLevel = testSetFolder.IndentLevel + "AAA";
						foreach (TestSetView testSet in groupedTestSetList)
						{
							//Create and populate the row
							remoteTestSet = new RemoteTestSet();
							PopulationFunctions.PopulateTestSet(remoteTestSet, testSet, runningIndentLevel);
							PopulationFunctions.PopulateCustomProperties(remoteTestSet, testSet, customProperties);
							remoteTestSets.Add(remoteTestSet);

							//Increment the indent level
							runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
						}
					}
				}
			}

			//Find the last used root indent level
			TestSetFolderHierarchyView lastRootTestSetFolder = testSetFolders.Where(f => f.IndentLevel.Length == 3).OrderByDescending(f => f.IndentLevel).FirstOrDefault();
			if (lastRootTestSetFolder == null)
			{
				runningIndentLevel = "AAA";
			}
			else
			{
				runningIndentLevel = HierarchicalList.IncrementIndentLevel(lastRootTestSetFolder.IndentLevel);
			}

			//Add the root-folder testSets (if any)
			groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == null);
			if (groupedTestSets != null)
			{
				List<TestSetView> groupedTestSetList = groupedTestSets.ToList();
				if (groupedTestSetList != null && groupedTestSetList.Count > 0)
				{
					foreach (TestSetView testSet in groupedTestSetList)
					{
						//Create and populate the row
						RemoteTestSet remoteTestSet = new RemoteTestSet();
						PopulationFunctions.PopulateTestSet(remoteTestSet, testSet, runningIndentLevel);
						PopulationFunctions.PopulateCustomProperties(remoteTestSet, testSet, customProperties);
						remoteTestSets.Add(remoteTestSet);

						//Increment the indent level
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSets;
		}

		/// <summary>
		/// Retrieves all testSets owned by the currently authenticated user
		/// </summary>
		/// <returns>List of testSets</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the testSet dataset
			TestSetManager testSetManager = new TestSetManager();
			List<TestSetView> testSets = testSetManager.RetrieveByOwnerId(userId, null);

			//Get the custom property definitions - for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.TestSet);

			//Populate the API data object and return
			List<RemoteTestSet> remoteTestSets = new List<RemoteTestSet>();
			foreach (TestSetView testSet in testSets)
			{
				//Create and populate the row
				RemoteTestSet remoteTestSet = new RemoteTestSet();
				PopulationFunctions.PopulateTestSet(remoteTestSet, testSet);
				PopulationFunctions.PopulateCustomProperties(remoteTestSet, testSet, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestSets);
			}

			//Call the business object to actually retrieve the test set dataset
			TestSetManager testSetManager = new TestSetManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test set was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId, false);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

				//Make sure that the project ids match
				if (testSet.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Populate the API data object and return
				RemoteTestSet remoteTestSet = new RemoteTestSet();
				PopulationFunctions.PopulateTestSet(remoteTestSet, testSet);
				if (artifactCustomProperty != null)
				{
					PopulationFunctions.PopulateCustomProperties(remoteTestSet, artifactCustomProperty);
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_TestSetIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestSets);
			}

			//First retrieve the existing datarow
			try
			{
				TestSetManager testSetManager = new TestSetManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we have a test set or test set folder
				if (remoteTestSet.TestSetId < 0)
				{
					//Test Set Folder
					TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(-remoteTestSet.TestSetId.Value);

					//Make sure that the project ids match
					if (testSetFolder.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
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
					TestSet testSet = testSetManager.RetrieveById2(projectId, remoteTestSet.TestSetId.Value);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestSet.TestSetId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

					//Make sure that the project ids match
					if (testSet.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
					UpdateTestSetData(testSet, remoteTestSet);
					UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value);

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
		/// Moves a test set to another location in the hierarchy
		/// </summary>
		/// <param name="testSetId">The id of the test set we want to move</param>
		/// <param name="destinationTestSetFolderId">The id of the folder it's to be inserted inserted inside (or null to be at the root)</param>
		public void TestSet_Move(int testSetId, int? destinationTestSetFolderId)
		{
			const string METHOD_NAME = "TestSet_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestSets);
			}

			//First retrieve the test sets to make sure they exists and are in the authorized project
			try
			{
				TestSetManager testSetManager = new TestSetManager();
				TestSetView sourceTestSet = testSetManager.RetrieveById(projectId, testSetId);

				//Make sure that the project ids match
				if (sourceTestSet.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}
				if (destinationTestSetFolderId.HasValue)
				{
					//Make sure the folder is positive
					if (destinationTestSetFolderId.Value < 0)
					{
						destinationTestSetFolderId = -destinationTestSetFolderId;
					}

					TestSetFolder destTestSetFolder = testSetManager.TestSetFolder_GetById(destinationTestSetFolderId.Value);
					if (destTestSetFolder == null)
					{
						throw new ArtifactNotExistsException();
					}
					if (destTestSetFolder.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}
				}

				//Call the business object to actually perform the move
				testSetManager.TestSet_UpdateFolder(testSetId, destinationTestSetFolderId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test set");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Deletes a test set in the system
		/// </summary>
		/// <param name="testSetId">The id of the test set</param>
		public void TestSet_Delete(int testSetId)
		{
			const string METHOD_NAME = "TestSet_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete test sets
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				TestSetManager testSetManager = new TestSetManager();

				//See if we have a test set or folder
				if (testSetId < 0)
				{
					TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(-testSetId);
					if (testSetFolder == null)
					{
						throw new ArtifactNotExistsException(Resources.Messages.TestSetDetails_ArtifactNotExists);
					}

					//Make sure that the project ids match
					if (testSetFolder.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Call the business object to actually mark the item as deleted
					testSetManager.TestSetFolder_Delete(projectId, -testSetId, userId);
				}
				else
				{
					TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);

					//Make sure that the project ids match
					if (testSet.ProjectId != projectId)
					{
						throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
					}

					//Call the business object to actually mark the item as deleted
					testSetManager.MarkAsDeleted(userId, projectId, testSetId);
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
		/// <param name="existingTestSetTestCaseId">The id of the existing entry we want to insert it before (if not set, will be simply added to the end of the list)</param>
		/// <param name="remoteTestSetTestCaseMapping">The test set and test case mapping entry</param>
		/// <param name="parameters">Any parameter values to be passed from the test set to the test case</param>
		/// <remarks>
		/// You can only pass in a test case id not a test case folder id
		/// </remarks>
		public void TestSet_AddTestMapping(RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping, Nullable<int> existingTestSetTestCaseId, List<RemoteTestSetTestCaseParameter> parameters)
		{
			const string METHOD_NAME = "TestSet_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			try
			{
				//Now add the test case to the test set, seeing if we need to set any parameter values or not
				TestSetManager testSetManager = new TestSetManager();
				if (parameters != null && parameters.Count > 0)
				{
					Dictionary<string, string> parameterValues = new Dictionary<string, string>();
					foreach (RemoteTestSetTestCaseParameter parameter in parameters)
					{
						parameterValues.Add(parameter.Name, parameter.Value);
					}
					testSetManager.AddTestCase(projectId, remoteTestSetTestCaseMapping.TestSetId, remoteTestSetTestCaseMapping.TestCaseId, remoteTestSetTestCaseMapping.OwnerId, existingTestSetTestCaseId, parameterValues);
				}
				else
				{
					testSetManager.AddTestCase(projectId, remoteTestSetTestCaseMapping.TestSetId, remoteTestSetTestCaseMapping.TestCaseId, remoteTestSetTestCaseMapping.OwnerId, existingTestSetTestCaseId);
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
		/// Removes a test case from a test set
		/// </summary>
		/// <param name="remoteTestSetTestCaseMapping">The test set and test case mapping entry</param>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update test cases
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTestCases);
			}

			try
			{
				//Now remove the test case from the test set
				TestSetManager testSetManager = new TestSetManager();
				testSetManager.RemoveTestCase(projectId, remoteTestSetTestCaseMapping.TestSetId, remoteTestSetTestCaseMapping.TestSetTestCaseId);
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
			if (remoteTestSet.CreatorId.HasValue)
			{
				testSet.CreatorId = remoteTestSet.CreatorId.Value;
			}
			testSet.OwnerId = remoteTestSet.OwnerId;
			testSet.ReleaseId = remoteTestSet.ReleaseId;
			testSet.AutomationHostId = remoteTestSet.AutomationHostId;
			testSet.TestRunTypeId = remoteTestSet.TestRunTypeId;
			testSet.Name = remoteTestSet.Name;
			testSet.TestSetStatusId = remoteTestSet.TestSetStatusId;
			testSet.Description = remoteTestSet.Description;
			testSet.CreationDate = GlobalFunctions.UniversalizeDate(remoteTestSet.CreationDate);
			testSet.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteTestSet.LastUpdateDate);
			testSet.PlannedDate = GlobalFunctions.UniversalizeDate(remoteTestSet.PlannedDate);
			testSet.RecurrenceId = remoteTestSet.RecurrenceId;
		}

		/// <summary>Retrieves comments for a specified test set.</summary>
		/// <param name="TestSetId">The ID of the test set to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified test set.</returns>
		public List<RemoteComment> TestSet_RetrieveComments(int TestSetId)
		{
			List<RemoteComment> retList = new List<RemoteComment>();

			if (TestSetId > 0)
			{
				retList = this.commentRetrieve(TestSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a test set.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment TestSet_CreateComment(RemoteComment remoteComment)
		{
			RemoteComment retComment = this.createComment(remoteComment, DataModel.Artifact.ArtifactTypeEnum.TestSet);

			return retComment;
		}

		#endregion

		#region Task Methods
		/// <summary>Returns the number of tasks that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Task_Count(List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Task_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view incidents
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewIncidents);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TaskManager().Count(projectId, filters, 0, null, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Retrieves all new tasks added in the system since the date specified
		/// </summary>
		/// <param name="creationDate">The date after which the task needs to have been created</param>
		/// <returns>List of tasks</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTasks);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			Hashtable filters = new Hashtable();
			Common.DateRange dateRange = new Common.DateRange();
			dateRange.StartDate = GlobalFunctions.UniversalizeDate(creationDate);
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			List<TaskView> tasks = taskManager.Retrieve(projectId, "CreationDate", true, 1, Int32.MaxValue, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulationFunctions.PopulateTask(remoteTask, task);
				PopulationFunctions.PopulateCustomProperties(remoteTask, task, customProperties);
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTasks);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.Retrieve(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulationFunctions.PopulateTask(remoteTask, task);
				PopulationFunctions.PopulateCustomProperties(remoteTask, task, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.RetrieveByOwnerId(userId, null, null, false);

			//Get the custom property definitions - for all projects
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.Task);

			//Populate the API data object and return
			List<RemoteTask> remoteTasks = new List<RemoteTask>();
			foreach (TaskView task in tasks)
			{
				//Create and populate the row
				RemoteTask remoteTask = new RemoteTask();
				PopulationFunctions.PopulateTask(remoteTask, task);
				PopulationFunctions.PopulateCustomProperties(remoteTask, task, customProperties);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTasks);
			}

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();

			//If the task was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				TaskView task = taskManager.TaskView_RetrieveById(taskId);

				//Make sure that the project ids match
				if (task.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				ArtifactCustomProperty artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, true);

				//Populate the API data object and return
				RemoteTask remoteTask = new RemoteTask();
				PopulationFunctions.PopulateTask(remoteTask, task);
				PopulationFunctions.PopulateCustomProperties(remoteTask, artifactCustomProperty);

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
		/// Updates a task in the system
		/// </summary>
		/// <param name="remoteTask">The updated task object</param>
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

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_TaskIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyTasks);
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
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateTaskData(projectTemplateId, task, remoteTask);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value);

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
		/// Deletes a task in the system
		/// </summary>
		/// <param name="taskId">The id of the task</param>
		public void Task_Delete(int taskId)
		{
			const string METHOD_NAME = "Task_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				TaskManager taskManager = new TaskManager();
				Task task = taskManager.RetrieveById(taskId);

				//Make sure that the project ids match
				if (task.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				taskManager.MarkAsDeleted(projectId, taskId, userId);
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

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_TaskIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create tasks
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateTasks);
			}

			//Always use the current project
			remoteTask.ProjectId = projectId;

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Default to the authenticated user if we have no creator provided
			int creatorId = userId;
			if (remoteTask.CreatorId.HasValue)
			{
				creatorId = remoteTask.CreatorId.Value;
			}

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
			   creatorId,
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
			   remoteTask.RemainingEffort,
			   userId);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			taskManager.SendCreationNotification(remoteTask.TaskId.Value, artifactCustomProperty, null);

			//Finally return the populated task object
			return remoteTask;
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
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
			if (remoteTask.CreatorId.HasValue)
			{
				task.CreatorId = remoteTask.CreatorId.Value;
			}
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
			task.RemainingEffort = remoteTask.RemainingEffort;
		}

		/// <summary>Retrieves comments for a specified task.</summary>
		/// <param name="TaskId">The ID of the task to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified task.</returns>
		public List<RemoteComment> Task_RetrieveComments(int TaskId)
		{
			List<RemoteComment> retList = new List<RemoteComment>();

			if (TaskId > 0)
			{
				retList = this.commentRetrieve(TaskId, DataModel.Artifact.ArtifactTypeEnum.Task);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a task.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Task_CreateComment(RemoteComment remoteComment)
		{
			RemoteComment retComment = this.createComment(remoteComment, DataModel.Artifact.ArtifactTypeEnum.Task);

			return retComment;
		}

		#endregion

		#region Artifact Generic Methods

		/// <summary>
		/// Populates the custom property dataset from the API data object
		/// </summary>
		/// <param name="remoteArtifact">The API data object</param>
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

		#endregion

		#region Data Mapping Methods

		/// <summary>
		/// Retrieves a list of data mappings for artifact field values
		/// </summary>
		/// <param name="artifactFieldId">The field we're interested in</param>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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
		/// Retrieves a list of data mappings for projects in the system
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
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
		/// Retrieves the list of custom properties configured for the current project and the specified artifact type
		/// </summary>
		/// <param name="artifactTypeId">The id of the type of artifact</param>
		/// <returns>The list of custom properties</returns>
		/// <remarks>
		/// 1) Includes the custom list and custom list value child objects
		/// 2) The custom list values objects will include both active and inactive values, so need to check the flag before displaying
		/// </remarks>
		/// <example>
		/// RemoteCustomProperty[] remoteCustomProperties = spiraImportExport.CustomProperty_RetrieveForArtifactType((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
		/// </example>
		public List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(int artifactTypeId)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveForArtifactType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the custom properties for the current project and specified artifact
			//We don't get the corresponding custom lists right now, we'll do that in a separate step
			//since we also want the list values as well
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
					if (customProperty.CustomPropertyListId.HasValue)
					{
						//We need to get the custom list and matching values for this custom property
						remoteCustomProperty.CustomList = CustomProperty_RetrieveCustomListById(customProperty.CustomPropertyListId.Value);
					}
					else
					{
						remoteCustomProperty.CustomList = null;
					}
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

		#region Comment Methods (Internal)

		private List<RemoteComment> commentRetrieve(int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			List<RemoteComment> retComments = new List<RemoteComment>();

			//Get our comments, then convert them over.
			IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(artifactId, artifactType, false);

			if (comments.Count() > 0)
			{
				foreach (IDiscussion commentRow in comments)
				{
					RemoteComment newComment = new RemoteComment();
					newComment.ArtifactId = commentRow.ArtifactId;
					newComment.CommentId = commentRow.DiscussionId;
					newComment.IsDeleted = commentRow.IsDeleted;
					newComment.Text = commentRow.Text;
					newComment.CreationDate = GlobalFunctions.LocalizeDate(commentRow.CreationDate);
					newComment.UserId = commentRow.CreatorId;
					newComment.UserName = commentRow.CreatorName;

					retComments.Add(newComment);
				}
			}

			return retComments;
		}

		/// <summary>
		/// Creates a new comment from the provided API object
		/// </summary>
		/// <param name="remoteComment">The API data object</param>
		/// <param name="artifactType">The  type of artifact</param>
		/// <returns>The updated API object</returns>
		private RemoteComment createComment(RemoteComment remoteComment, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			if (remoteComment.ArtifactId > 0)
			{
				DiscussionManager discussionManager = new DiscussionManager();
				if (remoteComment.CreationDate.HasValue)
				{
					remoteComment.CommentId = discussionManager.Insert(
						(remoteComment.UserId.HasValue) ? remoteComment.UserId.Value : this.AuthenticatedUserId,
						remoteComment.ArtifactId,
						artifactType,
						remoteComment.Text,
						GlobalFunctions.UniversalizeDate(remoteComment.CreationDate.Value),
						this.AuthorizedProject,
						false,
						true
						);
				}
				else
				{
					remoteComment.CommentId = discussionManager.Insert(
						(remoteComment.UserId.HasValue) ? remoteComment.UserId.Value : this.AuthenticatedUserId,
						remoteComment.ArtifactId,
						artifactType,
						remoteComment.Text,
						this.AuthorizedProject,
						false,
						true
						);
				}
			}
			else
			{
				throw new Exception("ArtifactID must be entered.");
			}

			return remoteComment;
		}

		#endregion

		#region System methods

		/// <summary>
		/// Returns the application-relative URL to a particular artifact in the system
		/// </summary>
		/// <param name="navigationLinkId">
		/// The id of the type of page to return. Possible values are:
		/// None = -1,
		/// Login = -2,
		/// MyPage = -3,
		/// ProjectHome = -4,
		/// Requirements = 1,
		/// TestCases = 2,
		/// Incidents = 3,
		/// Releases = 4,
		/// Reports = -5,
		/// TestSets = 8,
		/// Administration = -6,
		/// ErrorPage = -7,
		/// MyProfile = -8,
		/// Tasks = 6,
		/// Iterations = -9,
		/// Documents = -10,
		/// Resources = -11,
		/// ProjectGroupHome = -12,
		/// SourceCode = -13,
		/// TestRuns = 5,
		/// TestSteps = 7,
		/// Attachment = -14,
		/// TestStepRuns = -15,
		/// TestExecute = -16
		/// </param>
		/// <param name="projectId">The project ID of the artifact. Ignored if not needed. Specifying -1 will not include ProjectPath, specifying -2 will insert the token {proj} for the ProjectID.</param>
		/// <param name="artifactId">The ID of the artifact. Ignored if not needed, specifying -2 will insert the token {art} for the ArtifactID</param>
		/// <param name="tabName">The name of the tab or extra item in the URL. Null if not specified.</param>
		/// <returns>String of the new URL. It includes the ~ character to represent the application root</returns>
		public string System_GetArtifactUrl(int navigationLinkId, int projectId, int artifactId, string tabName)
		{
			const string METHOD_NAME = "System_GetArtifactUrl";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the appropriate rewriter Url
			string serverUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)navigationLinkId, projectId, artifactId, tabName);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return serverUrl;

		}

		/// <summary>
		/// Retrieves the version number of the current installation.
		/// </summary>
		/// <returns>A RemoteVersion data object.</returns>
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
		public List<RemoteSetting> System_GetSettings()
		{
			const string METHOD_NAME = "System_GetSettings";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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

		#region AutomationHost methods

		/// <summary>
		/// Retrieves the list of automation hosts in the current project
		/// </summary>
		/// <param name="remoteFilters">The array of filters</param>
		/// <param name="remoteSort">The sort to be applied</param>
		/// <param name="startingRow">The starting row (1-based)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <returns>List of automation host objects</returns>
		public List<RemoteAutomationHost> AutomationHost_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "AutomationHost_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure a sort object was provided (filters are optional)
			if (remoteSort == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_SortMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view automation hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewAutomationHosts);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();
			List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, remoteSort.PropertyName, remoteSort.SortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, false);

			//Populate the API data object and return
			List<RemoteAutomationHost> remoteAutomationHosts = new List<RemoteAutomationHost>();
			foreach (AutomationHostView automationHost in automationHosts)
			{
				//Create and populate the row
				RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
				PopulationFunctions.PopulateAutomationHost(remoteAutomationHost, automationHost);
				PopulationFunctions.PopulateCustomProperties(remoteAutomationHost, automationHost, customProperties);
				remoteAutomationHosts.Add(remoteAutomationHost);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteAutomationHosts;
		}

		/// <summary>
		/// Retrieves a single automation host by its id
		/// </summary>
		/// <param name="automationHostId">The id of the automation host</param>
		/// <returns>The automation host object</returns>
		public RemoteAutomationHost AutomationHost_RetrieveById(int automationHostId)
		{
			const string METHOD_NAME = "AutomationHost_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view automation hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewAutomationHosts);
			}

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();

			//If the host was not found, just return null
			try
			{
				AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId);

				//Make sure that the project ids match
				if (automationHost.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				ArtifactCustomProperty artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, automationHostId, Artifact.ArtifactTypeEnum.AutomationHost, true);

				//Populate the API data object and return
				RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
				PopulationFunctions.PopulateAutomationHost(remoteAutomationHost, automationHost);
				PopulationFunctions.PopulateCustomProperties(remoteAutomationHost, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteAutomationHost;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation host");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Retrieves a single automation host by its token name
		/// </summary>
		/// <param name="token">The automation host's token name</param>
		/// <returns>The automation host object</returns>
		/// <remarks>Token names are only unique within a project</remarks>
		public RemoteAutomationHost AutomationHost_RetrieveByToken(string token)
		{
			const string METHOD_NAME = "AutomationHost_RetrieveByToken";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view automation hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewAutomationHosts);
			}

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();

			//If the host was not found, just return null
			try
			{
				AutomationHostView automationHost = automationManager.RetrieveHostByToken(projectId, token);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, false);

				//Populate the API data object and return
				RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
				PopulationFunctions.PopulateAutomationHost(remoteAutomationHost, automationHost);
				PopulationFunctions.PopulateCustomProperties(remoteAutomationHost, automationHost, customProperties);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteAutomationHost;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation host");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Creates a new automation host in the system
		/// </summary>
		/// <param name="remoteAutomationHost">The new automation host object (primary key will be empty)</param>
		/// <returns>The populated automation host object - including the primary key</returns>
		public RemoteAutomationHost AutomationHost_Create(RemoteAutomationHost remoteAutomationHost)
		{
			const string METHOD_NAME = "AutomationHost_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an primary key specified
			if (remoteAutomationHost.AutomationHostId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_AutomationHostIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create automation hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateAutomationHosts);
			}

			//Always use the current project
			remoteAutomationHost.ProjectId = projectId;

			//First insert the new automation host record itself, capturing and populating the id
			AutomationManager automationManager = new AutomationManager();
			remoteAutomationHost.AutomationHostId = automationManager.InsertHost(
			   projectId,
			   remoteAutomationHost.Name,
			   remoteAutomationHost.Token,
			   remoteAutomationHost.Description,
			   remoteAutomationHost.Active,
			   userId);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			UpdateCustomPropertyData(ref artifactCustomProperty, remoteAutomationHost, remoteAutomationHost.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, remoteAutomationHost.AutomationHostId.Value);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Finally return the populated daTa object
			return remoteAutomationHost;
		}

		/// <summary>
		/// Updates an automation host in the system
		/// </summary>
		/// <param name="remoteAutomationHost">The updated task object</param>
		public void AutomationHost_Update(RemoteAutomationHost remoteAutomationHost)
		{
			const string METHOD_NAME = "AutomationHost_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a primary key id specified
			if (!remoteAutomationHost.AutomationHostId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("ArgumentMissing", Resources.Messages.Services_AutomationHostIdMissing);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to update hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.Modify) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedModifyAutomationHosts);
			}

			//First retrieve the existing datarow
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				AutomationManager automationManager = new AutomationManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				AutomationHost automationHost = automationManager.RetrieveHostById2(remoteAutomationHost.AutomationHostId.Value);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteAutomationHost.AutomationHostId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, true);

				//Make sure that the project ids match
				if (automationHost.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateAutomationHostData(automationHost, remoteAutomationHost);
				UpdateCustomPropertyData(ref artifactCustomProperty, remoteAutomationHost, remoteAutomationHost.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, remoteAutomationHost.AutomationHostId.Value);

				//Call the business object to actually update the dataset and the custom properties
				automationManager.UpdateHost(automationHost, userId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation host");
				Logger.Flush();
			}
		}

		/// <summary>
		/// Deletes a automation host in the system
		/// </summary>
		/// <param name="automationHostId">The id of the automation host</param>
		public void AutomationHost_Delete(int automationHostId)
		{
			const string METHOD_NAME = "AutomationHost_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to delete automation hosts
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.AutomationHost, (int)Project.PermissionEnum.Delete) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedDeleteArtifactType);
			}

			//First retrieve the existing datarow
			try
			{
				AutomationManager automationManager = new AutomationManager();
				AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId);

				//Make sure that the project ids match
				if (automationHost.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually mark the item as deleted
				automationManager.MarkHostAsDeleted(projectId, automationHostId, userId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation host");
				Logger.Flush();
			}
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteAutomationHost">The API data object</param>
		/// <param name="automationHost">The internal datarow</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		protected void UpdateAutomationHostData(AutomationHost automationHost, RemoteAutomationHost remoteAutomationHost)
		{
			//We first need to update the concurrency information before we start tracking
			//The remote object uses LastUpdateDate for concurrency
			automationHost.ConcurrencyDate = GlobalFunctions.UniversalizeDate(remoteAutomationHost.LastUpdateDate);

			automationHost.StartTracking();
			automationHost.Name = remoteAutomationHost.Name;
			automationHost.Token = remoteAutomationHost.Token;
			automationHost.Description = remoteAutomationHost.Description;
			automationHost.IsActive = remoteAutomationHost.Active;
			automationHost.LastUpdateDate = GlobalFunctions.UniversalizeDate(remoteAutomationHost.LastUpdateDate);
		}

		#endregion

		#region AutomationEngine functions

		/// <summary>
		/// Retrieves a single automation engine record by its Token
		/// </summary>
		/// <param name="token">The token of the engine</param>
		/// <returns>The automation engine data object</returns>
		public RemoteAutomationEngine AutomationEngine_RetrieveByToken(string token)
		{
			const string METHOD_NAME = "AutomationEngine_RetrieveByToken";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Any authenticated user can see the list of automation engines

			//Call the business object to actually retrieve the automation engine dataset
			AutomationManager automationManager = new AutomationManager();

			//If the engine was not found, just return null
			try
			{
				AutomationEngine automationEngine = automationManager.RetrieveEngineByToken(token);

				//Populate the API data object and return
				RemoteAutomationEngine remoteAutomationEngine = new RemoteAutomationEngine();
				PopulationFunctions.PopulateAutomationEngine(remoteAutomationEngine, automationEngine);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteAutomationEngine;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation engine");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>Creates a new Automation Engine in the system.</summary>
		/// <param name="activeOnly">Do we only want the active ones</param>
		/// <returns>The newly-created Automation Engine.</returns>
		public RemoteAutomationEngine AutomationEngine_Create(RemoteAutomationEngine remoteEngine)
		{
			const string METHOD_NAME = CLASS_NAME + "AutomationEngine_Create";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(METHOD_NAME);
				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we have permissions to create automation engines (i.e. is a system admin)
			if (!IsSystemAdmin)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateAutomationEngines);
			}

			//Create the engine..
			new AutomationManager().InsertEngine(remoteEngine.Name, remoteEngine.Token, remoteEngine.Description, remoteEngine.Active, userId);

			//Call the business object to actually retrieve the automation engine dataset
			AutomationManager automationManager = new AutomationManager();
			List<AutomationEngine> automationEngines = automationManager.RetrieveEngines(true);

			//Populate the API data object and return
			List<RemoteAutomationEngine> remoteAutomationEngines = new List<RemoteAutomationEngine>();
			foreach (AutomationEngine automationEngine in automationEngines.Where(ae => ae.Token == remoteEngine.Token))
			{
				//Create and populate the row
				RemoteAutomationEngine remoteAutomationEngine = new RemoteAutomationEngine();
				PopulationFunctions.PopulateAutomationEngine(remoteAutomationEngine, automationEngine);
				remoteAutomationEngines.Add(remoteAutomationEngine);
			}

			//Withdraw the one we need..
			RemoteAutomationEngine retEngine = null;
			if (remoteAutomationEngines.Where(ae => ae.Token == remoteEngine.Token).Count() == 1)
			{
				retEngine = remoteAutomationEngines.Where(ae => ae.Token == remoteEngine.Token).Single();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retEngine;
		}


		/// <summary>
		/// Retrieves the list of automation engines in the system
		/// </summary>
		/// <param name="activeOnly">Do we only want  the active ones</param>
		/// <returns>List of automation engine data objects</returns>
		public List<RemoteAutomationEngine> AutomationEngine_Retrieve(bool activeOnly)
		{
			const string METHOD_NAME = "AutomationEngine_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Any authenticated user can see the list of automation engines

			//Call the business object to actually retrieve the automation engine dataset
			AutomationManager automationManager = new AutomationManager();
			List<AutomationEngine> automationEngines = automationManager.RetrieveEngines(activeOnly);

			//Populate the API data object and return
			List<RemoteAutomationEngine> remoteAutomationEngines = new List<RemoteAutomationEngine>();
			foreach (AutomationEngine automationEngine in automationEngines)
			{
				//Create and populate the row
				RemoteAutomationEngine remoteAutomationEngine = new RemoteAutomationEngine();
				PopulationFunctions.PopulateAutomationEngine(remoteAutomationEngine, automationEngine);
				remoteAutomationEngines.Add(remoteAutomationEngine);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteAutomationEngines;
		}

		/// <summary>
		/// Retrieves a single automation engine record by its ID
		/// </summary>
		/// <param name="automationEngineId">The id of the engine</param>
		/// <returns>The automation engine data object</returns>
		public RemoteAutomationEngine AutomationEngine_RetrieveById(int automationEngineId)
		{
			const string METHOD_NAME = "AutomationEngine_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Any authenticated user can see the list of automation engines

			//Call the business object to actually retrieve the automation engine dataset
			AutomationManager automationManager = new AutomationManager();

			//If the engine was not found, just return null
			try
			{
				AutomationEngine automationEngine = automationManager.RetrieveEngineById(automationEngineId);

				//Populate the API data object and return
				RemoteAutomationEngine remoteAutomationEngine = new RemoteAutomationEngine();
				PopulationFunctions.PopulateAutomationEngine(remoteAutomationEngine, automationEngine);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteAutomationEngine;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested automation engine");
				Logger.Flush();
				return null;
			}
		}

		#endregion

		#region Build functions

		/// <summary>
		/// Retrieves the list of builds that are associated with a specific Release
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <param name="releaseId">The release we're interested in</param>
		/// <param name="remoteSort">The sort to apply (pass null for default)</param>
		/// <returns>List of builds</returns>
		public List<RemoteBuild> Build_RetrieveByReleaseId(int releaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows)
		{
			const string METHOD_NAME = "Build_RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view Releases (since builds don't have their own permission)
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewReleases);
			}

			//Extract the filters and sortsfrom the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);
			string sortExpression = "";
			if (remoteSort != null)
			{
				sortExpression = PopulationFunctions.PopulateSort(remoteSort);
			}

			//Call the business object to actually retrieve the list of builds
			BuildManager buildManager = new BuildManager();
			int artifactCount; //Not used
			List<BuildView> builds = buildManager.RetrieveForRelease(projectId, releaseId, sortExpression, startingRow - 1, numberOfRows, filters, 0, out artifactCount);

			//Populate the API data object and return
			List<RemoteBuild> remoteBuilds = new List<RemoteBuild>();
			foreach (BuildView buildView in builds)
			{
				//Need to retrieve the build with its long description
				Build build = buildManager.RetrieveById(buildView.BuildId);

				//Create and populate the row
				RemoteBuild remoteBuild = new RemoteBuild();
				PopulationFunctions.PopulateBuild(remoteBuild, build);
				remoteBuilds.Add(remoteBuild);

				//Now get any revisions and populate
				List<BuildSourceCode> revisions = buildManager.RetrieveRevisionsForBuild(projectId, build.BuildId);
				if (revisions.Count > 0)
				{
					remoteBuild.Revisions = new List<RemoteBuildSourceCode>();
					foreach (BuildSourceCode revision in revisions)
					{
						//Create and populate the row
						RemoteBuildSourceCode remoteBuildSourceCode = new RemoteBuildSourceCode();
						PopulationFunctions.PopulateBuildSourceCode(remoteBuildSourceCode, revision);
						remoteBuild.Revisions.Add(remoteBuildSourceCode);
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteBuilds;
		}

		/// <summary>
		/// Retrieves the a single build (and associated source code revisions) by its id
		/// </summary>
		/// <param name="releaseId">The release we're interested in</param>
		/// <param name="buildId">The id of the build to retrieve</param>
		/// <returns>A single build object</returns>
		public RemoteBuild Build_RetrieveById(int releaseId, int buildId)
		{
			const string METHOD_NAME = "Build_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to view Releases (since builds don't have their own permission)
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.View) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewReleases);
			}

			try
			{
				//Call the business object to actually retrieve the build
				BuildManager buildManager = new BuildManager();
				Build build = buildManager.RetrieveById(buildId);

				//Make sure that the project ids match (to avoid returning data to unauthorized users)
				if (build.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Populate the API data object and return
				RemoteBuild remoteBuild = new RemoteBuild();
				PopulationFunctions.PopulateBuild(remoteBuild, build);

				//Now get any revisions and populate
				List<BuildSourceCode> revisions = buildManager.RetrieveRevisionsForBuild(projectId, build.BuildId);
				if (revisions.Count > 0)
				{
					remoteBuild.Revisions = new List<RemoteBuildSourceCode>();
					foreach (BuildSourceCode revision in revisions)
					{
						//Create and populate the row
						RemoteBuildSourceCode remoteBuildSourceCode = new RemoteBuildSourceCode();
						PopulationFunctions.PopulateBuildSourceCode(remoteBuildSourceCode, revision);
						remoteBuild.Revisions.Add(remoteBuildSourceCode);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteBuild;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested build");
				Logger.Flush();
				return null;
			}
		}

		/// <summary>
		/// Creates a new build in the system, including any linked source code revisions
		/// </summary>
		/// <param name="remoteBuild">The new build object (primary key will be empty)</param>
		/// <returns>The populated build object - including the primary key</returns>
		public RemoteBuild Build_Create(RemoteBuild remoteBuild)
		{
			const string METHOD_NAME = "Build_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an primary key specified
			if (remoteBuild.BuildId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("DataObjectPrimaryKeyNotNull", Resources.Messages.Services_BuildIdNotNull);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = AuthenticatedUserId;

			//Make sure we are connected to a project
			if (!IsAuthorized)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", Resources.Messages.Services_NotConnectedToProject);
			}
			int projectId = AuthorizedProject;

			//Make sure we have permissions to create releases (since builds don't have their own permission)
			if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Release, (int)Project.PermissionEnum.Create) == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedCreateReleases);
			}

			//See if we have a creation date specified, otherwise use current date/time
			DateTime creationDate = DateTime.UtcNow;
			if (remoteBuild.CreationDate.HasValue)
			{
				creationDate = GlobalFunctions.UniversalizeDate(remoteBuild.CreationDate.Value);
			}

			//First insert the new build record itself, capturing and populating the id
			//Always use the current project (for security) - i.e. projectId not remoteBuild.ProjectId
			BuildManager buildManager = new BuildManager();
			Build build = buildManager.Insert(
			   projectId,
			   remoteBuild.ReleaseId,
			   remoteBuild.Name,
			   remoteBuild.Description,
			   creationDate,
			   (Build.BuildStatusEnum)remoteBuild.BuildStatusId,
			   userId
			   );
			PopulationFunctions.PopulateBuild(remoteBuild, build);

			//Now add the revisions
			if (remoteBuild.Revisions != null)
			{
				foreach (RemoteBuildSourceCode remoteBuildSourceCode in remoteBuild.Revisions)
				{
					//See if we have a creation date specified, otherwise use current date/time
					if (!remoteBuildSourceCode.CreationDate.HasValue)
					{
						//We convert to UTC later
						remoteBuildSourceCode.CreationDate = DateTime.Now;
					}
					BuildSourceCode buildSourceCode = buildManager.InsertSourceCodeRevision(
						remoteBuild.BuildId.Value,
						remoteBuildSourceCode.RevisionKey,
						GlobalFunctions.UniversalizeDate(remoteBuildSourceCode.CreationDate.Value)
					);
				}
			}

			//Finally return the populated data object
			return remoteBuild;
		}

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
