using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.ServiceModel.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.v4_0.DataObjects;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Rest;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
	/// <summary>
	/// This web service enables the import and export of data to/from the system. Each function is prefixed by the
	/// area of the system that it relates to. For example, the Requirement_Retrieve function relates to the
	/// Requirements module.
	/// </summary>
	/// <remarks>
	/// Unlike the SOAP services you don't authenticate using sessions, instead you pass the username and API Key (RSS Token)
	/// with each message call. It can be passed in the HTTP header or as an extra URL token
	/// </remarks>
	public class RestService : RestServiceBase, IRestService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v4_0.RestService::";

		private const int DEFAULT_PAGINATION_SIZE = 500;

		#region System functions

		/// <summary>
		/// Retrieves the version number of the current installation.
		/// </summary>
		/// <returns>A RemoteVersion data object.</returns>
		public RemoteVersion System_GetProductVersion()
		{
			const string METHOD_NAME = "System_GetProductVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//No authentication/authorization needed

			RemoteVersion returnValues = new RemoteVersion();
			returnValues.Version = GlobalFunctions.DISPLAY_SOFTWARE_VERSION + "." + GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD;
			returnValues.Patch = GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return returnValues;
		}

		#endregion

		#region Automation Host functions

		/// <summary>
		/// Retrieves the list of all the automation hosts in the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>List of automation host objects</returns>
		public List<RemoteAutomationHost> AutomationHost_Retrieve1(string project_id)
		{
			const string METHOD_NAME = "AutomationHost_Retrieve1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();
			List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, "Name", true, 1, Int32.MaxValue, null, 0);

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
		/// Retrieves the list of automation hosts in the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The array of filters</param>
		/// <param name="sort_field">The field to sort by</param>
		/// <param name="sort_direction">The direction to sort by [ASC|DESC]</param>
		/// <param name="starting_row">The starting row (1-based)</param>
		/// <param name="number_of_rows">The number of rows to retrieve</param>
		/// <returns>List of automation host objects</returns>
		public List<RemoteAutomationHost> AutomationHost_Retrieve2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "AutomationHost_Retrieve2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "start_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_rows");
			string sortBy = (String.IsNullOrEmpty(sort_field) ? "Name" : sort_field.Trim());
			bool sortAscending = (sort_direction != null && sort_direction.ToLowerInvariant() != "desc");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();
			List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, sortBy, sortAscending, startingRow, numberOfRows, filters, 0);

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="automation_host_id">The id of the automation host</param>
		/// <returns>The automation host object</returns>
		public RemoteAutomationHost AutomationHost_RetrieveById(string project_id, string automation_host_id)
		{
			const string METHOD_NAME = "AutomationHost_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int automationHostId = RestUtils.ConvertToInt32(automation_host_id, "automation_host_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the host was not found, just return null
			try
			{
				AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId);

				//Make sure that the project ids match
				if (automationHost.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
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
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.AutomationHostsService_AutomationHostNotFound);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves a single automation host by its token name
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="token">The automation host's token name</param>
		/// <returns>The automation host object</returns>
		/// <remarks>Token names are only unique within a project</remarks>
		public RemoteAutomationHost AutomationHost_RetrieveByToken(string project_id, string token)
		{
			const string METHOD_NAME = "AutomationHost_RetrieveByToken";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the automation host dataset
			AutomationManager automationManager = new AutomationManager();

			//If the host was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				AutomationHostView automationHost = automationManager.RetrieveHostByToken(projectId, token);

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.AutomationHostsService_AutomationHostNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.AutomationHostsService_AutomationHostNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>
		/// Creates a new automation host in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteAutomationHost">The new automation host object (primary key will be empty)</param>
		/// <returns>The populated automation host object - including the primary key</returns>
		public RemoteAutomationHost AutomationHost_Create(string project_id, RemoteAutomationHost remoteAutomationHost)
		{
			const string METHOD_NAME = "AutomationHost_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an primary key specified
			if (remoteAutomationHost.AutomationHostId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_AutomationHostIdNotNull, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
			   userId.Value);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteAutomationHost, remoteAutomationHost.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, remoteAutomationHost.AutomationHostId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

			//Finally return the populated data object
			return remoteAutomationHost;
		}


		/// <summary>
		/// Updates an automation host in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteAutomationHost">The updated task object</param>
		public void AutomationHost_Update(string project_id, RemoteAutomationHost remoteAutomationHost)
		{
			const string METHOD_NAME = "AutomationHost_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have a primary key id specified
			if (!remoteAutomationHost.AutomationHostId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_AutomationHostIdMissing, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
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
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateFunctions.UpdateAutomationHostData(automationHost, remoteAutomationHost);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteAutomationHost, remoteAutomationHost.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, remoteAutomationHost.AutomationHostId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

				//Call the business object to actually update the dataset and the custom properties
				automationManager.UpdateHost(automationHost, userId.Value);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.AutomationHostsService_AutomationHostNotFound);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Deletes a automation host in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="automation_host_id">The id of the automation host</param>
		public void AutomationHost_Delete(string project_id, string automation_host_id)
		{
			const string METHOD_NAME = "AutomationHost_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int automationHostId = RestUtils.ConvertToInt32(automation_host_id, "automation_host_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete automation hosts
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateAutomationHosts, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the existing datarow
			try
			{
				AutomationManager automationManager = new AutomationManager();
				AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId);

				//Make sure that the project ids match
				if (automationHost.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
				}

				//Call the business object to actually mark the item as deleted
				automationManager.MarkHostAsDeleted(projectId, automationHostId, userId.Value);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.AutomationHostsService_AutomationHostNotFound);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
		}

		#endregion

		#region Automation Engine functions

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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_AutomationEngineNotExist);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_AutomationEngineNotExist, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>
		/// Retrieves the list of automation engines in the system
		/// </summary>
		/// <param name="active_only">Do we only want the active ones</param>
		/// <returns>List of automation engine data objects</returns>
		public List<RemoteAutomationEngine> AutomationEngine_Retrieve(string active_only)
		{
			const string METHOD_NAME = "AutomationEngine_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			bool activeOnly = RestUtils.ConvertToBoolean(active_only, "active_only");

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

		/// <summary>Creates a new Automation Engine in the system.</summary>
		/// <param name="remoteEngine">The new automation engine</param>
		/// <returns>The newly-created Automation Engine.</returns>
		public RemoteAutomationEngine AutomationEngine_Create(RemoteAutomationEngine remoteEngine)
		{
			const string METHOD_NAME = CLASS_NAME + "AutomationEngine_Create";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(userId.Value);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateAutomationEngines, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Create the engine..
			new AutomationManager().InsertEngine(remoteEngine.Name, remoteEngine.Token, remoteEngine.Description, remoteEngine.Active, userId.Value);

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
		/// Retrieves a single automation engine record by its ID
		/// </summary>
		/// <param name="automation_engine_id">The id of the engine</param>
		/// <returns>The automation engine data object</returns>
		public RemoteAutomationEngine AutomationEngine_RetrieveById(string automation_engine_id)
		{
			const string METHOD_NAME = "AutomationEngine_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int automationEngineId = RestUtils.ConvertToInt32(automation_engine_id, "automation_engine_id");

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_AutomationEngineNotExist);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_AutomationEngineNotExist, System.Net.HttpStatusCode.NotFound);
			}
		}

		#endregion

		#region Project Role functions

		/// <summary>
		/// Retrieves a list of project roles in the system
		/// </summary>
		/// <returns>The list of project roles</returns>
		public List<RemoteProjectRole> ProjectRole_Retrieve()
		{
			const string METHOD_NAME = "ProjectRole_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated || !AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			else
			{
				//Get the list of project roles
				int userId = AuthenticatedUserId.Value;
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

		#endregion

		#region Document functions

		/// <summary>
		/// Returns the actual binary content of a file attachment in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="document_id">The id of the file attachment to be retrieved</param>
		/// <returns>An array of bytes representing the attachment content</returns>
		public byte[] Document_OpenFile(string project_id, string document_id)
		{
			const string METHOD_NAME = "Document_OpenFile";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Call the business object to actually retrieve the attachment data
				AttachmentManager attachmentManager = new AttachmentManager();
				FileStream stream = attachmentManager.OpenById(documentId);

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
				throw new WebFaultException<string>(Resources.Messages.Services_DocumentNotFound, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Adds a new document (file) into the system and associates it with the provided artifact (optional)
		/// and project folder/type (optional)
		/// </summary>
		/// <param name="document_type_id">The id of the document type to associate with</param>
		/// <param name="filename">The filename to give the uploaded file</param>
		/// <param name="folder_id">The id of the document folder to put the file in</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="tags">Any meta-tags, comma separated</param>
		/// <param name="artifact_type_id">The id of the type of artifact to attach it to</param>
		/// <param name="artifact_id">The id of the an artifact to attach it to</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <returns>
		/// The populated document object - including the primary key and default values for project attachment type
		/// and project folder if they were not specified
		/// </returns>
		public RemoteDocument Document_AddFile(string project_id, string filename, string tags, string folder_id, string document_type_id, string artifact_type_id, string artifact_id, byte[] binaryData)
		{
			const string METHOD_NAME = "Document_AddFile";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? folderId = RestUtils.ConvertToInt32Nullable(folder_id, "folder_id");
			int? documentTypeId = RestUtils.ConvertToInt32Nullable(document_type_id, "document_type_id");
			int? artifactTypeId = RestUtils.ConvertToInt32Nullable(artifact_type_id, "artifact_type_id");
			int? artifactId = RestUtils.ConvertToInt32Nullable(artifact_id, "artifact_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddDocuments, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Default to the authenticated user if no author provided
				int authorId = userId.Value;

				//Handle any nullable values safely
				string description = null;
				string version = null;
				if (String.IsNullOrEmpty(tags))
				{
					tags = null;
				}

				DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
				if (artifactTypeId.HasValue)
				{
					artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId.Value;
				}

				//Now insert the attachment
				Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
				int documentId = attachmentManager.Insert(
				   projectId,
				   filename,
				   description,
				   authorId,
				   binaryData,
				   artifactId,
				   artifactType,
				   version,
				   tags,
				   documentTypeId,
				   folderId,
				   null
				   );

				//Retrieve it back to get the full data object
				ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, documentId);
				RemoteDocument remoteDocument = new RemoteDocument();
				PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);

				//Send a notification
				attachmentManager.SendCreationNotification(projectId, remoteDocument.AttachmentId.Value, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocument;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new document (url) into the system and associates it with the provided artifact (optional)
		/// and project folder/type (optional)
		/// </summary>
		/// <param name="document_type_id">The id of the document type to associate with</param>
		/// <param name="url">The full URL to be added</param>
		/// <param name="folder_id">The id of the document folder to put the file in</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="tags">Any meta-tags, comma separated</param>
		/// <param name="artifact_type_id">The id of the type of artifact to attach it to</param>
		/// <param name="artifact_id">The id of the an artifact to attach it to</param>
		/// <returns>
		/// The populated document object - including the primary key and default values for project attachment type
		/// and project folder if they were not specified
		/// </returns>
		public RemoteDocument Document_AddUrl(string project_id, string url, string tags, string folder_id, string document_type_id, string artifact_type_id, string artifact_id)
		{
			const string METHOD_NAME = "Document_AddUrl";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? folderId = RestUtils.ConvertToInt32Nullable(folder_id, "folder_id");
			int? documentTypeId = RestUtils.ConvertToInt32Nullable(document_type_id, "document_type_id");
			int? artifactTypeId = RestUtils.ConvertToInt32Nullable(artifact_type_id, "artifact_type_id");
			int? artifactId = RestUtils.ConvertToInt32Nullable(artifact_id, "artifact_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddDocuments, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Default to the authenticated user if no author provided
				int authorId = userId.Value;

				//Handle any nullable values safely
				string description = null;
				string version = null;
				if (String.IsNullOrEmpty(tags))
				{
					tags = null;
				}

				DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
				if (artifactTypeId.HasValue)
				{
					artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId.Value;
				}

				//Now insert the attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				int documentId = attachmentManager.Insert(
				   projectId,
				   url,
				   description,
				   authorId,
				   artifactId,
				   artifactType,
				   version,
				   tags,
				   documentTypeId,
				   folderId,
				   null
				   );

				//Retrieve it back to get the full data object
				ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, documentId);
				RemoteDocument remoteDocument = new RemoteDocument();
				PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);

				//Send a notification
				attachmentManager.SendCreationNotification(projectId, remoteDocument.AttachmentId.Value, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocument;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new version to a file attachment in the system
		/// </summary>
		/// <param name="document_id">The id of the document that we're adding this new version to</param>
		/// <param name="filename">The filename of the file version being uploaded</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="version">The number of the version being added</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="make_current">Should we make this the current version</param>
		/// <returns>The version id</returns>
		public int Document_AddFileVersion(string project_id, string document_id, string filename, string version, string make_current, byte[] binaryData)
		{
			const string METHOD_NAME = "Document_AddFileVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");
			bool makeCurrent = RestUtils.ConvertToBoolean(make_current, "make_current");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddDocuments, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Retrieve the attachment and make sure that they are of the same type
				AttachmentManager attachmentManager = new AttachmentManager();

				//Default to the authenticated user if no author provided
				int authorId = userId.Value;

				//Handle any nullable values safely
				string description = null;
				if (String.IsNullOrEmpty(version))
				{
					version = null;
				}

				//Now insert the attachment version
				int attachmentVersionId = attachmentManager.InsertVersion(
				   projectId,
				   documentId,
				   filename,
				   description,
				   authorId,
				   binaryData,
				   version,
				   makeCurrent
				   );

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return attachmentVersionId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new version to a URL attachment in the system
		/// </summary>
		/// <param name="document_id">The id of the document that we're adding this new version to</param>
		/// <param name="url">The full URL of the new version being added</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="version">The number of the version being added</param>
		/// <param name="make_current">Should we make this the current version</param>
		/// <returns>The version id</returns>
		public int Document_AddUrlVersion(string project_id, string document_id, string url, string version, string make_current)
		{
			const string METHOD_NAME = "Document_AddUrlVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");
			bool makeCurrent = RestUtils.ConvertToBoolean(make_current, "make_current");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddDocuments, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Retrieve the attachment and make sure that they are of the same type
				AttachmentManager attachmentManager = new AttachmentManager();

				//Default to the authenticated user if no author provided
				int authorId = userId.Value;

				//Handle any nullable values safely
				string description = null;
				if (String.IsNullOrEmpty(version))
				{
					version = null;
				}

				//Now insert the attachment version
				int attachmentVersionId = attachmentManager.InsertVersion(
				   projectId,
				   documentId,
				   url,
				   description,
				   authorId,
				   version,
				   makeCurrent
				   );

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return attachmentVersionId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes an attachment from the project completely
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="document_id">The id of the attachment to delete</param>
		public void Document_Delete(string project_id, string document_id)
		{
			const string METHOD_NAME = "Document_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteDocuments, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Now delete the attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Delete(projectId, documentId);
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
		/// Retrieves a list of all the documents/attachments in a project across all folders
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>List of documents</returns>
		public List<RemoteDocument> Document_Retrieve1(string project_id)
		{
			const string METHOD_NAME = "Document_Retrieve1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, null, "UploadDate", false, 1, DEFAULT_PAGINATION_SIZE, null, 0);

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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a filtered list of documents/attachments in a project across all folders
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="sort_by">The name of the column to sort by (ascending)</param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The first row to return (starting with 1)</param>
		/// <returns>List of documents</returns>
		public List<RemoteDocument> Document_Retrieve2(string project_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Document_Retrieve2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberRows = RestUtils.ConvertToInt32(number_rows, "number_rows");
			string sortBy = (String.IsNullOrEmpty(sort_by) ? "Filename" : sort_by.Trim());

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, null, sortBy, true, startRow, numberRows, filters, 0);

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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a filtered list of documents/attachments in a project for the specified folder
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="sort_by">The name of the column to sort by (ascending)</param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The first row to return (starting with 1)</param>
		/// <param name="folder_id">The id of the project attachment folder</param>
		/// <returns>List of documents</returns>
		public List<RemoteDocument> Document_RetrieveForFolder(string project_id, string folder_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Document_RetrieveForFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberRows = RestUtils.ConvertToInt32(number_rows, "number_rows");
			int folderId = RestUtils.ConvertToInt32(folder_id, "folder_id");
			string sortBy = (String.IsNullOrEmpty(sort_by) ? "Filename" : sort_by.Trim());

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId, folderId, sortBy, true, startRow, numberRows, filters, 0);

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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a single project document by its id
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="document_id">The id of the attachment to retrieve</param>
		/// <returns>The document data object</returns>
		/// <remarks>
		/// 1) For files it does not include the raw file data, you need to use Document_OpenById
		/// 2) It also retrieves the list of document versions
		/// </remarks>
		public RemoteDocument Document_RetrieveById(string project_id, string document_id)
		{
			const string METHOD_NAME = "Document_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int attachmentId = RestUtils.ConvertToInt32(document_id, "document_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
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
				throw new WebFaultException<string>(Resources.Messages.Services_DocumentNotFound, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>Adds an existing attachment to the specified artifact.</summary>
		/// <param name="document_id">The id of the document being added</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_type_id">The id of the type of artifact we want to add the documents to:
		/// (Requirement = 1,
		///	TestCase = 2,
		/// Incident = 3,
		///	Release = 4,
		///	TestRun = 5,
		///	Task = 6,
		/// TestStep = 7,
		/// TestSet = 8)</param>
		/// <param name="artifact_id">The id of the artifact we want the attachments for</param>
		public void Document_AddToArtifactId(string project_id, string artifact_type_id, string artifact_id, string document_id)
		{
			const string METHOD_NAME = CLASS_NAME + "Document_AddToArtifactId";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int artifactId = RestUtils.ConvertToInt32(artifact_id, "artifact_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify or create the artifact type in question (could be either)
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.Modify) && !IsAuthorized(projectId, artifactType, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				new AttachmentManager().InsertArtifactAssociation(projectId, documentId, artifactId, artifactType);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Deletes an attachment from an artifact. The attachment will still remain in the project
		/// </summary>
		/// <param name="document_id">The id of the document being removed</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_type_id">The id of the type of artifact we want to remove the documents from:
		/// (Requirement = 1,
		///	TestCase = 2,
		/// Incident = 3,
		///	Release = 4,
		///	TestRun = 5,
		///	Task = 6,
		/// TestStep = 7,
		/// TestSet = 8)</param>
		/// <param name="artifact_id">The id of the artifact we want to remove the attachment from</param>
		public void Document_DeleteFromArtifact(string project_id, string artifact_type_id, string artifact_id, string document_id)
		{
			const string METHOD_NAME = "Document_DeleteFromArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int artifactId = RestUtils.ConvertToInt32(artifact_id, "artifact_id");
			int documentId = RestUtils.ConvertToInt32(document_id, "document_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify the artifact type in question
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Now delete the attachment from the artifact
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Delete(projectId, documentId, artifactId, artifactType);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Retrieves the list of documents/attachments in a project attached to a specific artifact
		/// </summary>
		/// <param name="artifact_id">The id of the artifact we want the attachments for</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_type_id">
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
		public List<RemoteDocument> Document_RetrieveForArtifact(string project_id, string artifact_type_id, string artifact_id)
		{
			const string METHOD_NAME = "Document_RetrieveForArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int artifactId = RestUtils.ConvertToInt32(artifact_id, "artifact_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view the artifact type in question
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view documents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Call the business object to actually retrieve the project attachment dataset
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, artifactId, artifactType, "Filename", true, 1, Int32.MaxValue, null, 0);

				//Populate the API data object and return
				List<RemoteDocument> remoteDocuments = new List<RemoteDocument>();
				foreach (ProjectAttachmentView projectAttachment in attachments)
				{
					//Create and populate the row
					RemoteDocument remoteDocument = new RemoteDocument();
					PopulationFunctions.PopulateDocument(remoteDocument, projectAttachment);
					remoteDocument.ArtifactId = artifactId;
					remoteDocument.ArtifactTypeId = artifactTypeId;
					remoteDocuments.Add(remoteDocument);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteDocuments;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region Project functions

		/// <summary>
		/// Creates a new project in the system and makes the authenticated user owner of it
		/// </summary>
		/// <param name="remoteProject">The new project object (primary key will be empty)</param>
		/// <param name="existing_project_id">The id of an existing project to use as a template, or null to use the default template</param>
		/// <returns>The populated project object - including the primary key</returns>
		public RemoteProject Project_Create(string existing_project_id, RemoteProject remoteProject)
		{
			const string METHOD_NAME = "Project_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int? existingProjectId = RestUtils.ConvertToInt32Nullable(existing_project_id, "existing_project_id");

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(userId.Value);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateProjects, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteProject;
		}

		/// <summary>
		/// Deletes an existing project in the system
		/// </summary>
		/// <param name="project_id">The project being deleted</param>
		public void Project_Delete(string project_id)
		{
			const string METHOD_NAME = "Project_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(userId.Value);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteProjects, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Now delete the project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			projectManager.Delete(userId.Value, projectId);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();

		}

		/// <summary>
		/// Refreshes the task progress and test execution status for a project.
		/// Typically this needs to be called after TestRun_RecordAutomated3(...) API calls
		/// to ensure the data in the system is consistent
		/// </summary>
		/// <param name="project_id">The id of the project being refreshed</param>
		/// <param name="release_id">The release we want to refresh, or pass NULL for all releases in the project</param>
		/// <param name="run_async">
		/// Do we want to run this in the background. If it runs in the background it will return immediately and continue processing on the server/
		/// Otherwise the caller will have to wait until it finishes. Choosing True is better if you have a large project that will take longer to
		/// refresh that the web service timeout, but False is better if your subsequent code relies on the data being refreshed
		/// </param>
		public void Project_RefreshProgressExecutionStatusCaches(string project_id, string release_id, string run_async)
		{
			const string METHOD_NAME = "Project_RefreshProgressExecutionStatusCaches";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? releaseId = RestUtils.ConvertToInt32Nullable(release_id, "release_id");
			bool runInBackground = RestUtils.ConvertToBoolean(run_async, "run_async");

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(userId.Value);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyProject, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			else
			{
				//Get the list of projects
				int? userId = AuthenticatedUserId;
				if (!userId.HasValue)
				{
					//Throw back an exception
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();

					throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
				}

				try
				{
					Business.ProjectManager projectManager = new Business.ProjectManager();
					List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId.Value);

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
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
				}
			}
		}

		/// <summary>
		/// Retrieves a single project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>The project object</returns>
		public RemoteProject Project_RetrieveById(string project_id)
		{
			const string METHOD_NAME = "Project_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			else
			{
				int? userId = AuthenticatedUserId;
				if (!userId.HasValue)
				{
					//Throw back an exception
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();

					throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
				}

				//Convert the parameters into their native types
				int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

				//Make sure we're authorized
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
				if (!authProjects.Any(p => p.ProjectId == projectId))
				{
					//Throw back an exception
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();

					throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
				}

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
				catch (ArtifactNotExistsException exception)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Service_ProjectNotFound);
					Logger.Flush();
					throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
				}
			}
		}

		/// <summary>
		/// Retrieves the list of active users that are members of the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>List of ProjectUser objects</returns>
		public List<RemoteProjectUser> Project_RetrieveUserMembership(string project_id)
		{
			const string METHOD_NAME = "Project_RetrieveUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Retrieve the list of users that are members of the current project
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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region Incident Functions

		/// <summary>
		/// Creates a new incident priority in the specified project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteIncidentPriority">The incident priority resource data</param>
		/// <returns>The incident priority resource data with the incident priority id populated</returns>
		public RemoteIncidentPriority Incident_AddPriority(string project_id, RemoteIncidentPriority remoteIncidentPriority)
		{
			const string METHOD_NAME = "Incident_AddPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create incident priorities (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident priority
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentPriority.PriorityId = incidentManager.InsertIncidentPriority(projectTemplateId, remoteIncidentPriority.Name, remoteIncidentPriority.Color, remoteIncidentPriority.Active);

				//Finally return the populated incident type object
				return remoteIncidentPriority;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident severities for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The list of active incident severities for the current project</returns>
		public List<RemoteIncidentSeverity> Incident_RetrieveSeverities(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrieveSeverities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
				foreach (IncidentSeverity incidentSeverity in incidentSeverities)
				{
					RemoteIncidentSeverity remoteIncidentSeverity = new RemoteIncidentSeverity();
					remoteIncidentSeverities.Add(remoteIncidentSeverity);
					//Populate fields
					remoteIncidentSeverity.SeverityId = incidentSeverity.SeverityId;
					remoteIncidentSeverity.Name = incidentSeverity.Name;
					remoteIncidentSeverity.Color = incidentSeverity.Color;
					remoteIncidentSeverity.Active = incidentSeverity.IsActive;
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
		/// Creates a new incident severity in the specified project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteIncidentSeverity">The incident severity resource data</param>
		/// <returns>The incident severity resource data with the incident severity id populated</returns>
		public RemoteIncidentSeverity Incident_AddSeverity(string project_id, RemoteIncidentSeverity remoteIncidentSeverity)
		{
			const string METHOD_NAME = "Incident_AddSeverity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create incident severities (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident severity
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentSeverity.SeverityId = incidentManager.InsertIncidentSeverity(projectTemplateId, remoteIncidentSeverity.Name, remoteIncidentSeverity.Color, remoteIncidentSeverity.Active);

				//Finally return the populated incident type object
				return remoteIncidentSeverity;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident statuses for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The list of active incident statuses for the current project</returns>
		public List<RemoteIncidentStatus> Incident_RetrieveStatuses(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrieveStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
				foreach (IncidentStatus incidentStatus in incidentStatuses)
				{
					RemoteIncidentStatus remoteIncidentStatus = new RemoteIncidentStatus();
					remoteIncidentStatuses.Add(remoteIncidentStatus);
					//Populate fields
					remoteIncidentStatus.IncidentStatusId = incidentStatus.IncidentStatusId;
					remoteIncidentStatus.Name = incidentStatus.Name;
					remoteIncidentStatus.Open = incidentStatus.IsOpenStatus;
					remoteIncidentStatus.Active = incidentStatus.IsActive;
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
		/// Creates a new incident status in the specified project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteIncidentStatus">The incident status resource data</param>
		/// <returns>The incident status resource data with the incident status id populated</returns>
		public RemoteIncidentStatus Incident_AddStatus(string project_id, RemoteIncidentStatus remoteIncidentStatus)
		{
			const string METHOD_NAME = "Incident_AddStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create incident statuses (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident status
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentStatus.IncidentStatusId = incidentManager.IncidentStatus_Insert(projectTemplateId, remoteIncidentStatus.Name, remoteIncidentStatus.Open, false, remoteIncidentStatus.Active);

				//Finally return the populated incident type object
				return remoteIncidentStatus;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Creates a new incident type in the specified project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteIncidentType">The incident type resource data</param>
		/// <returns>The incident type resource data with the incident type id populated</returns>
		public RemoteIncidentType Incident_AddType(string project_id, RemoteIncidentType remoteIncidentType)
		{
			const string METHOD_NAME = "Incident_AddType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create incident types (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now insert the incident type
				IncidentManager incidentManager = new IncidentManager();
				remoteIncidentType.IncidentTypeId = incidentManager.InsertIncidentType(projectTemplateId, remoteIncidentType.Name, null, remoteIncidentType.Issue, remoteIncidentType.Risk, false, remoteIncidentType.Active);

				//Finally return the populated incident type object
				return remoteIncidentType;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Searches the list of incidents in the project. This operation allows you to specify a list of filters, the sort column and the pagination range
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="sort_by">The column to sort on</param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The starting row (1-based)</param>
		/// <param name="remoteFilters">the list of filters to apply to the search</param>
		/// <returns>The list of incidents</returns>
		public List<RemoteIncident> Incident_Retrieve3(string project_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Incident_Retrieve3";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberRows = RestUtils.ConvertToInt32(number_rows, "number_rows");
			string sortBy = (String.IsNullOrEmpty(sort_by) ? "CreationDate" : sort_by.Trim());
			bool sortAscending = true;

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();

			//If the incident was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<IncidentView> incidents = incidentManager.Retrieve(projectId, sortBy, sortAscending, startRow, numberRows, filters, 0);

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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of comments that belong to the specified incident
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="incident_id">The id of the incident</param>
		/// <returns>The list of comments</returns>
		public List<RemoteComment> Incident_RetrieveComments(string project_id, string incident_id)
		{
			const string METHOD_NAME = "Incident_RetrieveComments";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentId = RestUtils.ConvertToInt32(incident_id, "incident_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the incident resolutions
			IncidentManager incidentManager = new IncidentManager();
			try
			{
				Incident incident = incidentManager.RetrieveById(incidentId, true);

				//Sort the resolutions by oldest first
				List<IncidentResolution> resolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();

				//Populate the API data object and return
				List<RemoteComment> remoteIncidentResolutions = new List<RemoteComment>();
				foreach (IncidentResolution incidentResolution in resolutions)
				{
					//Create and populate the row
					RemoteComment remoteIncidentResolution = new RemoteComment();
					remoteIncidentResolution.CommentId = incidentResolution.IncidentResolutionId;
					remoteIncidentResolution.ArtifactId = incidentResolution.IncidentId;
					remoteIncidentResolution.UserId = incidentResolution.CreatorId;
					remoteIncidentResolution.Text = incidentResolution.Resolution;
					remoteIncidentResolution.CreationDate = incidentResolution.CreationDate;
					remoteIncidentResolution.IsDeleted = false;
					remoteIncidentResolution.UserName = incidentResolution.Creator.FullName;
					remoteIncidentResolutions.Add(remoteIncidentResolution);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentResolutions;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_IncidentNotFound, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a single incident in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="incident_id">The id of the incident</param>
		/// <returns>Incident object</returns>
		public RemoteIncident Incident_RetrieveById(string project_id, string incident_id)
		{
			const string METHOD_NAME = "Incident_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentId = RestUtils.ConvertToInt32(incident_id, "incident_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
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
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
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
				PopulationFunctions.PopulateIncident(remoteIncident, incident);
				PopulationFunctions.PopulateCustomProperties(remoteIncident, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncident;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_IncidentNotFound, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of incidents that match the comma-separated list of ids
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="incident_ids">The comma-separated list of incidents</param>
		/// <returns>The list of incident objects</returns>
		public List<RemoteIncident> Incident_RetrieveByIdList(string project_id, string incident_ids)
		{
			const string METHOD_NAME = "Incident_RetrieveByIdList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			Common.MultiValueFilter incidentIds = RestUtils.ConvertToMultiValueFilter(incident_ids, "incident_ids");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident dataset
				IncidentManager incidentManager = new IncidentManager();
				Hashtable filters = new Hashtable();
				filters.Add("IncidentId", incidentIds);
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "LastUpdateDate", false, 1, Int32.MaxValue, filters, 0);

				//Get the custom property definitions
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

				List<RemoteIncident> remoteIncidents = new List<RemoteIncident>();
				foreach (IncidentView incident in incidents)
				{
					//Make sure that the project ids match
					if (incident.ProjectId == projectId)
					{
						//Populate the API data object and return
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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of the active incident priorities for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The list of active incident priorities for the current project</returns>
		public List<RemoteIncidentPriority> Incident_RetrievePriorities(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrievePriorities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
				foreach (IncidentPriority incidentPriority in incidentPriorities)
				{
					RemoteIncidentPriority remoteIncidentPriority = new RemoteIncidentPriority();
					remoteIncidentPriorities.Add(remoteIncidentPriority);
					//Populate fields
					remoteIncidentPriority.PriorityId = incidentPriority.PriorityId;
					remoteIncidentPriority.Name = incidentPriority.Name;
					remoteIncidentPriority.Color = incidentPriority.Color;
					remoteIncidentPriority.Active = incidentPriority.IsActive;
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
		/// Retrieves a list of the active incident types for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The list of active incident types for the current project</returns>
		public List<RemoteIncidentType> Incident_RetrieveTypes(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrieveTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
				foreach (IncidentType incidentType in incidentTypes)
				{
					RemoteIncidentType remoteIncidentType = new RemoteIncidentType();
					remoteIncidentTypes.Add(remoteIncidentType);
					//Populate fields
					remoteIncidentType.IncidentTypeId = incidentType.IncidentTypeId;
					remoteIncidentType.Name = incidentType.Name;
					remoteIncidentType.Risk = incidentType.IsRisk;
					remoteIncidentType.Issue = incidentType.IsIssue;
					remoteIncidentType.Active = incidentType.IsActive;
					remoteIncidentType.WorkflowId = incidentType.WorkflowId;
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
		/// Retrieves the default incident status for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The default status</returns>
		public RemoteIncidentStatus Incident_RetrieveDefaultStatus(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrieveDefaultStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//All project members can see the list of statuses, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident data
				IncidentManager incidentManager = new IncidentManager();
				IncidentStatus status = incidentManager.IncidentStatus_RetrieveDefault(projectTemplateId);

				//Now populate the list of API data objects
				RemoteIncidentStatus remoteIncidentStatus = new RemoteIncidentStatus();
				//Populate fields
				remoteIncidentStatus.IncidentStatusId = status.IncidentStatusId;
				remoteIncidentStatus.Name = status.Name;
				remoteIncidentStatus.Open = status.IsOpenStatus;
				remoteIncidentStatus.Active = status.IsActive;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteIncidentStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the default incident type for the current project
		/// </summary>
		/// <param name="project_id">The project id</param>
		/// <returns>The default incident type</returns>
		public RemoteIncidentType Incident_RetrieveDefaultType(string project_id)
		{
			const string METHOD_NAME = "Incident_RetrieveDefaultType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//All project members can see the list of types, so no additional check needed.

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the incident data
				IncidentManager incidentManager = new IncidentManager();
				int typeId = incidentManager.GetDefaultIncidentType(projectTemplateId);
				IncidentType type = incidentManager.RetrieveIncidentTypeById(typeId);

				//Now populate the list of API data objects
				RemoteIncidentType remoteIncidentType = new RemoteIncidentType();

				//Populate fields
				remoteIncidentType.IncidentTypeId = type.IncidentTypeId;
				remoteIncidentType.Name = type.Name;
				remoteIncidentType.Risk = type.IsRisk;
				remoteIncidentType.Issue = type.IsIssue;
				remoteIncidentType.Active = type.IsActive;
				remoteIncidentType.WorkflowId = type.WorkflowId;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return remoteIncidentType;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Will retrieve available transitions for the specied status ID for the currently logged-on user.
		/// </summary>
		/// <param name="incident_type_id">The current incident type</param>
		/// <param name="incident_status_id">The current incident status</param>
		/// <param name="is_detector">Is the user the detector of the incident</param>
		/// <param name="is_owner">Is the user the owner of the incident</param>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The list of workflow transitions</returns>
		public List<RemoteWorkflowIncidentTransition> Incident_RetrieveWorkflowTransitions(string project_id, string incident_type_id, string incident_status_id, string is_detector, string is_owner)
		{
			const string METHOD_NAME = "Incident_RetrieveWorkflowTransitions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentTypeId = RestUtils.ConvertToInt32(incident_type_id, "incident_type_id");
			int incidentStatusId = RestUtils.ConvertToInt32(incident_status_id, "incident_status_id");
			bool isDetector = RestUtils.ConvertToBoolean(is_detector, "is_detector");
			bool isOwner = RestUtils.ConvertToBoolean(is_owner, "is_owner");

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			Business.ProjectManager projectManager = new Business.ProjectManager();

			//Make sure we're authorized
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			List<RemoteWorkflowIncidentTransition> retList = new List<RemoteWorkflowIncidentTransition>();

			//Get the use's role in the project.
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, userId.Value);
			if (projectUser == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}
			int roleId = projectUser.ProjectRoleId;

			//Get the workflow ID for the specified status.
			int workflowId = -1;
			List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
			for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
			{
				if (incidentTypes[i].IncidentTypeId == incidentTypeId)
				{
					workflowId = incidentTypes[i].WorkflowId;
				}
			}

			WorkflowManager workflowManager = new Business.WorkflowManager();
			List<WorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, incidentStatusId, roleId, isDetector, isOwner);

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
		/// <param name="incident_type_id">The current incident type</param>
		/// <param name="incident_status_id">The current incident status</param>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The list of incident fields</returns>
		public List<RemoteWorkflowIncidentFields> Incident_RetrieveWorkflowFields(string project_id, string incident_type_id, string incident_status_id)
		{
			const string METHOD_NAME = "Incident_RetrieveWorkflowFields";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentTypeId = RestUtils.ConvertToInt32(incident_type_id, "incident_type_id");
			int incidentStatusId = RestUtils.ConvertToInt32(incident_status_id, "incident_status_id");

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			List<RemoteWorkflowIncidentFields> retList = new List<RemoteWorkflowIncidentFields>();

			try
			{
				//Get the workflow ID for the specified status.
				int workflowId = -1;
				List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
				for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
				{
					if (incidentTypes[i].IncidentTypeId == incidentTypeId)
					{
						workflowId = incidentTypes[i].WorkflowId;
					}
				}

				//Pull fields.
				List<WorkflowField> workflowFields = new WorkflowManager().Workflow_RetrieveFieldStates(workflowId, incidentStatusId);

				foreach (WorkflowField workflowField in workflowFields)
				{
					retList.Add(new RemoteWorkflowIncidentFields(workflowField));
				}

				return retList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves the list of incident custom properties and their workflow state for a given type and status/step.
		/// </summary>
		/// <param name="incident_type_id">The current incident type</param>
		/// <param name="incident_status_id">The current incident status</param>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The list of incident custom property states</returns>
		public List<RemoteWorkflowIncidentCustomProperties> Incident_RetrieveWorkflowCustomProperties(string project_id, string incident_type_id, string incident_status_id)
		{
			const string METHOD_NAME = "Incident_RetrieveWorkflowCustomProperties";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentTypeId = RestUtils.ConvertToInt32(incident_type_id, "incident_type_id");
			int incidentStatusId = RestUtils.ConvertToInt32(incident_status_id, "incident_status_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			List<RemoteWorkflowIncidentCustomProperties> retList = new List<RemoteWorkflowIncidentCustomProperties>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the workflow ID for the specified status.
				int workflowId = -1;
				List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(projectTemplateId, false);
				for (int i = 0; (i < incidentTypes.Count && workflowId < 0); i++)
				{
					if (incidentTypes[i].IncidentTypeId == incidentTypeId)
					{
						workflowId = incidentTypes[i].WorkflowId;
					}
				}

				//Pull custom properties
				List<WorkflowCustomProperty> workflowCustomProperties = new WorkflowManager().Workflow_RetrieveCustomPropertyStates(workflowId, incidentStatusId);

				foreach (WorkflowCustomProperty workflowCustomProperty in workflowCustomProperties)
				{
					retList.Add(new RemoteWorkflowIncidentCustomProperties(workflowCustomProperty));
				}
				return retList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Returns the count of the number of incidents in the project
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>The number of incidents</returns>
		public long Incident_Count(string project_id)
		{
			const string METHOD_NAME = "Incident_Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the incident count
			IncidentManager incidentManager = new IncidentManager();

			try
			{
				long count = incidentManager.Count(projectId, null, 0);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves the list of incidents in the project, ordered by the most recent. Only returns the first 500 results
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>The list of incidents</returns>
		public List<RemoteIncident> Incident_Retrieve1(string project_id)
		{
			const string METHOD_NAME = "Incident_Retrieve2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();

			//If the incident was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "CreationDate", false, 1, DEFAULT_PAGINATION_SIZE, null, 0);

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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves the list of incidents in the project. This operation allows you to specify the sort column and the pagination range
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="sort_by">The column to sort on</param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The starting row (1-based)</param>
		/// <returns>The list of incidents</returns>
		public List<RemoteIncident> Incident_Retrieve2(string project_id, string start_row, string number_rows, string sort_by)
		{
			const string METHOD_NAME = "Incident_Retrieve2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberRows = RestUtils.ConvertToInt32(number_rows, "number_rows");
			string sortBy = (String.IsNullOrEmpty(sort_by) ? "CreationDate" : sort_by.Trim());

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();

			//If the incident was not found, just return null
			try
			{
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, sortBy, true, startRow, numberRows, null, 0);

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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves all new incidents added in the system since the date specified
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="creation_date">
		/// The date after which the incident needs to have been created
		/// (needs to be in UTC using the format: yyyy-MM-ddTHH:mm:ss.fff)
		/// </param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The starting row (1-based)</param>
		/// <returns>The list of incidents</returns>
		public List<RemoteIncident> Incident_RetrieveNew(string project_id, string start_row, string number_rows, string creation_date)
		{
			const string METHOD_NAME = "Incident_RetrieveNew";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberRows = RestUtils.ConvertToInt32(number_rows, "number_rows");
			DateTime creationDate = RestUtils.ConvertToDateTime(creation_date, "creation_date");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the incident dataset
			IncidentManager incidentManager = new IncidentManager();

			//If the incident was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				Hashtable filters = new Hashtable();
				Common.DateRange dateRange = new Common.DateRange();
				dateRange.StartDate = creationDate;
				dateRange.ConsiderTimes = true;
				filters.Add("CreationDate", dateRange);
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "CreationDate", true, startRow, numberRows, filters, 0);

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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Adds new incident comments to an incident in a project in the system
		/// </summary>
		/// <param name="incident_id">The id of the incident</param>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteComments">List of new comments to add</param>
		/// <returns>The list of comments with the ArtifactId populated</returns>
		public List<RemoteComment> Incident_AddComments(string project_id, string incident_id, List<RemoteComment> remoteComments)
		{
			const string METHOD_NAME = "Incident_AddComments";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentId = RestUtils.ConvertToInt32(incident_id, "incident_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Retrieve the existing incident
				IncidentManager incidentManager = new IncidentManager();
				Incident incident = incidentManager.RetrieveById(incidentId, false);

				//Make sure that the project ids match
				if (incident.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
				}

				//Iterate through the provided resolutions, inserting them as needed
				foreach (RemoteComment remoteComment in remoteComments)
				{
					//Ignore the artifact id field, link them to the current incident

					//If the creator is not specified, use the current user
					int creatorId = userId.Value;
					if (remoteComment.UserId.HasValue)
					{
						creatorId = remoteComment.UserId.Value;
					}
					DateTime creationDate = DateTime.UtcNow;
					if (remoteComment.CreationDate.HasValue)
					{
						creationDate = remoteComment.CreationDate.Value;
					}

					remoteComment.CommentId = incidentManager.InsertResolution(
					   incidentId,
					   remoteComment.Text,
					   creationDate,
					   creatorId,
					   true
					   );
				}

				//Finally return the populated incident comment list
				return remoteComments;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Creates a new incident in the specified project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="remoteIncident">The incident resource data</param>
		/// <returns>The incident resource data with the incident id populated</returns>
		public RemoteIncident Incident_Create(string project_id, RemoteIncident remoteIncident)
		{
			const string METHOD_NAME = "Incident_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateIncidents, System.Net.HttpStatusCode.Unauthorized);
			}

			//Default to the authenticated user if we have no opener provided
			int openerId = userId.Value;
			if (remoteIncident.OpenerId.HasValue)
			{
				openerId = remoteIncident.OpenerId.Value;
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Always use the current project
				remoteIncident.ProjectId = projectId;

				//If we don't have a valid creation date, set it to the current date
				DateTime creationDate = DateTime.UtcNow;
				if (remoteIncident.CreationDate.HasValue)
				{
					creationDate = remoteIncident.CreationDate.Value;
				}

				int? incidentStatusId = null;
				//If the remote artifact contains a status AND the template setting allows bulk edit of status, set the status, otherwise leave it null (so manager will use the default)
				if (remoteIncident.IncidentStatusId.HasValue)
				{
					ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
					{
						incidentStatusId = remoteIncident.IncidentStatusId.Value;
					}
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
				   incidentStatusId,
				   creationDate,
				   remoteIncident.StartDate,
				   remoteIncident.ClosedDate,
				   remoteIncident.EstimatedEffort,
				   remoteIncident.ActualEffort,
				   remoteIncident.RemainingEffort,
				   remoteIncident.FixedBuildId,
				   null,
				   userId.Value
				   );

				//Now we need to populate any custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = null;
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

				//Send a notification
				incidentManager.SendCreationNotification(remoteIncident.IncidentId.Value, artifactCustomProperty, null);

				//Finally return the populated incident object
				return remoteIncident;
			}
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Updates an incident already present in the system.
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="incident_id">The id of the incident</param>
		/// <param name="remoteIncident">The incident resource definition</param>
		public void Incident_Update(string project_id, string incident_id, RemoteIncident remoteIncident)
		{
			const string METHOD_NAME = "Incident_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentId = RestUtils.ConvertToInt32(incident_id, "incident_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyIncidents, System.Net.HttpStatusCode.Unauthorized);
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
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateFunctions.UpdateIncidentData(incident, remoteIncident, projectTemplateId);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteIncident, remoteIncident.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, remoteIncident.IncidentId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

				//Get copies of everything..
				Artifact notificationArt = incident.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Call the business object to actually update the incident dataset and the custom properties
				incidentManager.Update(incident, userId.Value);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

				//If the test run step is not linked already, add
				if (remoteIncident.TestRunStepId.HasValue && !incident.TestRunSteps.Any(t => t.TestRunStepId == remoteIncident.TestRunStepId.Value))
				{
					incidentManager.Incident_AssociateToTestRunStep(projectId, remoteIncident.TestRunStepId.Value, new List<int>() { remoteIncident.IncidentId.Value }, userId.Value);
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
			catch (WebFaultException<ValidationFaultMessage> exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Detail.Summary);
				throw;
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Deletes an incident in a specific project in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="incident_id">The id of the incident</param>
		public void Incident_Delete(string project_id, string incident_id)
		{
			const string METHOD_NAME = "Incident_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int incidentId = RestUtils.ConvertToInt32(incident_id, "incident_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete incidents
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the existing datarow
			try
			{
				IncidentManager incidentManager = new IncidentManager();
				Incident incident = incidentManager.RetrieveById(incidentId, false);

				//Make sure that the project ids match
				if (incident.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
				}

				//Call the business object to actually mark the item as deleted
				incidentManager.MarkAsDeleted(projectId, incidentId, userId.Value);
			}
			catch (WebFaultException exception)
			{
				//Throw without converting
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_IncidentNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region Custom Property Methods

		/// <summary>
		/// Adds a new custom list into the project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteCustomList">The new custom list object</param>
		/// <returns>The custom list object with the primary key set</returns>
		/// <remarks>Also adds any custom list values if they are provided</remarks>
		public RemoteCustomList CustomProperty_AddCustomList(string project_id, RemoteCustomList remoteCustomList)
		{
			const string METHOD_NAME = "CustomProperty_AddCustomList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomList.CustomPropertyListId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_CustomListIdNotNull, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create custom lists (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddCustomLists, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Now insert the new custom list
				remoteCustomList.CustomPropertyListId = customPropertyManager.CustomPropertyList_Add(projectTemplateId, remoteCustomList.Name, remoteCustomList.Active).CustomPropertyListId;

				//Now add the values if any are provided
				if (remoteCustomList.Values != null)
				{
					foreach (RemoteCustomListValue remoteCustomListValue in remoteCustomList.Values)
					{
						remoteCustomListValue.CustomPropertyValueId = customPropertyManager.CustomPropertyList_AddValue(remoteCustomList.CustomPropertyListId.Value, remoteCustomListValue.Name).CustomPropertyValueId;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteCustomList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves all the custom lists in the current project
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>A collection of custom list data objects</returns>
		/// <remarks>
		/// Does not return the actual custom list values
		/// </remarks>
		public List<RemoteCustomList> CustomProperty_RetrieveCustomLists(string project_id)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveCustomLists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//All project members can see the list values, so no additional check needed.

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
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Updates a custom list and any associated values in the system
		/// </summary>
		/// <param name="remoteCustomList">The custom list to update</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="custom_list_id">The id of the list being updated</param>
		/// <remarks>This will not add any new custom values, for that you need to use the AddCustomListValue() function</remarks>
		public void CustomProperty_UpdateCustomList(string project_id, string custom_list_id, RemoteCustomList remoteCustomList)
		{
			const string METHOD_NAME = "CustomProperty_UpdateCustomList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int customListId = RestUtils.ConvertToInt32(custom_list_id, "custom_list_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create custom lists (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddCustomLists, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//First we need to get the current definition of the custom list and associated values
				CustomPropertyList customPropertyList = customPropertyManager.CustomPropertyList_RetrieveById(customListId, true, true);

				//Make sure that this list belongs to the current project (for security reasons)
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
								customPropertyValue.StartTracking();
								customPropertyValue.Name = remoteCustomListValue.Name;
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
		/// Adds a new custom property list value into the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="custom_list_id">The id of the custom list</param>
		/// <param name="remoteCustomListValue">The new custom list value object being added</param>
		/// <returns>custom list value object with its primary key set</returns>
		public RemoteCustomListValue CustomProperty_AddCustomListValue(string project_id, string custom_list_id, RemoteCustomListValue remoteCustomListValue)
		{
			const string METHOD_NAME = "CustomProperty_AddCustomListValue";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have a custom list id already set
			if (remoteCustomListValue.CustomPropertyValueId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_CustomPropertyValueIdNotNull, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int customListId = RestUtils.ConvertToInt32(custom_list_id, "custom_list_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create custom list values (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddCustomListValues, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure the custom property ids match
			if (remoteCustomListValue.CustomPropertyListId != customListId)
			{
				throw new WebFaultException<string>(Resources.Messages.Services_CustomListNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the custom property business class
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

				//Make sure the project ids match the list
				CustomPropertyList cpl = customPropertyManager.CustomPropertyList_RetrieveById(customListId, false, true, false);
				if (cpl == null || cpl.ProjectTemplateId != projectTemplateId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_CustomListNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
				}

				//Now insert the new custom property list value
				remoteCustomListValue.CustomPropertyValueId = customPropertyManager.CustomPropertyList_AddValue(remoteCustomListValue.CustomPropertyListId, remoteCustomListValue.Name).CustomPropertyValueId;

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
		/// Adds a new custom property definition to the project for the specified artifact type
		/// </summary>
		/// <param name="custom_list_id">The id of the custom list if it's a list custom property</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteCustomProperty">The new custom property definition</param>
		/// <returns>The custom property definition with the primary key populated</returns>
		public RemoteCustomProperty CustomProperty_AddDefinition(string project_id, string custom_list_id, RemoteCustomProperty remoteCustomProperty)
		{
			const string METHOD_NAME = "CustomProperty_AddDefinition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? customListId = RestUtils.ConvertToInt32Nullable(custom_list_id, "custom_list_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to add custom properties (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedUpdateCustomProperties, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure that we don't have a custom property id specified
			if (remoteCustomProperty.CustomPropertyId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_CustomPropertyIdNotNull, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure that this list belongs to the current project (for security reasons)
			if (remoteCustomProperty.ProjectId != projectId)
			{
				throw new Exception(Resources.Messages.Services_CustomPropertyNotBelongToProject);
			}

			try
			{
				//Instantiate the custom property business class
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Verify that the list id is correct
				if (customListId.HasValue)
				{
					CustomPropertyList customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId.Value, false, true);
					if (customList == null)
					{
						throw new Exception(Resources.Messages.Services_CustomListDoesNotExist);
					}

					//Make sure the project template's match
					if (customList.ProjectTemplateId != projectTemplateId)
					{
						throw new Exception(Resources.Messages.Services_CustomListNotBelongToProject);
					}
				}

				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					(Artifact.ArtifactTypeEnum)remoteCustomProperty.ArtifactTypeId,
					remoteCustomProperty.CustomPropertyTypeId,
					remoteCustomProperty.PropertyNumber,
					remoteCustomProperty.Name,
					null,
					null,
					customListId);

				//Now we need to set the option values
				if (remoteCustomProperty.Options != null && remoteCustomProperty.Options.Count > 0)
				{
					foreach (RemoteCustomPropertyOption remoteCustomPropertyOption in remoteCustomProperty.Options)
					{
						CustomPropertyOptionValue customPropertyOptionValue = new CustomPropertyOptionValue();
						customPropertyOptionValue.CustomPropertyOptionId = remoteCustomPropertyOption.CustomPropertyOptionId;
						customPropertyOptionValue.CustomPropertyId = customProperty.CustomPropertyId;
						customPropertyOptionValue.Value = remoteCustomPropertyOption.Value;
						customPropertyManager.CustomPropertyOptions_Add(customPropertyOptionValue);
					}
				}

				remoteCustomProperty.CustomPropertyId = customProperty.CustomPropertyId;
				return remoteCustomProperty;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a custom property definition, including any associated options
		/// </summary>
		/// <param name="custom_property_id">The id of the custom property</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteCustomProperty">The custom property definition to update</param>
		/// <remarks>
		/// This method updates the custom property Name, IsDeleted flag, and custom property options, but does not update the list values or other fields.
		/// To change the list values, you need to use: CustomProperty_UpdateCustomList(...)
		/// </remarks>
		public void CustomProperty_UpdateDefinition(string project_id, string custom_property_id, RemoteCustomProperty remoteCustomProperty)
		{
			const string METHOD_NAME = "CustomProperty_UpdateDefinition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int customPropertyId = RestUtils.ConvertToInt32(custom_property_id, "custom_property_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update custom properties (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedUpdateCustomProperties, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure the custom property ids match
			if (!remoteCustomProperty.CustomPropertyId.HasValue || remoteCustomProperty.CustomPropertyId.Value != customPropertyId)
			{
				throw new WebFaultException<string>(Resources.Messages.Services_CustomListNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				//Instantiate the custom property business class
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Retrieve the existing definition
				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId, false);
				if (customProperty == null)
				{
					throw new Exception(Resources.Messages.Services_CustomPropertyDoesNotExist);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Make sure that this custom property belongs to the current project (for security reasons)
				if (customProperty.ProjectTemplateId != projectTemplateId)
				{
					throw new Exception(Resources.Messages.Services_CustomPropertyNotBelongToProject);
				}

				//Next we need to update the custom property itself
				customProperty.StartTracking();
				customProperty.Name = remoteCustomProperty.Name;
				customProperty.IsDeleted = remoteCustomProperty.IsDeleted;

				//Now we need to change any of the option values as well (if there are any)
				if (remoteCustomProperty.Options != null)
				{
					//Now populate
					foreach (RemoteCustomPropertyOption remoteCustomPropertyOption in remoteCustomProperty.Options)
					{
						//See if it already exists or not (insert vs. update)
						CustomPropertyOptionValue optionValue = customProperty.Options.FirstOrDefault(o => o.CustomPropertyOptionId == remoteCustomPropertyOption.CustomPropertyOptionId);
						if (optionValue == null)
						{
							CustomPropertyOptionValue newCustomPropertyOptionValue = new CustomPropertyOptionValue();
							newCustomPropertyOptionValue.CustomPropertyOptionId = remoteCustomPropertyOption.CustomPropertyOptionId;
							newCustomPropertyOptionValue.Value = remoteCustomPropertyOption.Value;
							customProperty.Options.Add(newCustomPropertyOptionValue);
						}
						else
						{
							optionValue.StartTracking();
							optionValue.Value = remoteCustomPropertyOption.Value;
						}
					}

					//Finally handle any deletes
					List<CustomPropertyOptionValue> itemsToDelete = new List<CustomPropertyOptionValue>();
					foreach (CustomPropertyOptionValue optionValue in customProperty.Options)
					{
						if (!remoteCustomProperty.Options.Any(o => o.CustomPropertyOptionId == optionValue.CustomPropertyOptionId))
						{
							itemsToDelete.Add(optionValue);
						}
					}
					foreach (CustomPropertyOptionValue optionValue in itemsToDelete)
					{
						customProperty.Options.Remove(optionValue);
					}
				}

				//Finally commit the changes
				customPropertyManager.CustomPropertyDefinition_Update(customProperty);

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
		/// Deletes a custom property definition from the system, including any associated options
		/// </summary>
		/// <param name="customPropertyId">The id of the custom property to delete</param>
		/// <remarks>Does a hard delete</remarks>
		public void CustomProperty_DeleteDefinition(string project_id, string custom_property_id)
		{
			const string METHOD_NAME = "CustomProperty_UpdateDefinition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int customPropertyId = RestUtils.ConvertToInt32(custom_property_id, "custom_property_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update custom properties (project owner)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.ProjectAdmin))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedUpdateCustomProperties, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Instantiate the custom property business class
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Retrieve the existing definition
				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId, true);
				if (customProperty == null)
				{
					throw new Exception(Resources.Messages.Services_CustomPropertyDoesNotExist);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Make sure that this custom property belongs to the current project (for security reasons)
				if (customProperty.ProjectTemplateId != projectTemplateId)
				{
					throw new Exception(Resources.Messages.Services_CustomPropertyNotBelongToProject);
				}

				//Finally delete the custom property definition (physically)
				customPropertyManager.CustomPropertyDefinition_RemoveById(customPropertyId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of custom properties configured for the current project and the specified artifact type
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="artifactTypeName">The name of the type of artifact ('Incident', 'Requirement', 'TestCase', etc.)</param>
		/// <returns>The list of custom properties</returns>
		/// <remarks>
		/// 1) Includes the custom list and custom list value child objects
		/// 2) The custom list values objects will include both active and inactive values, so need to check the flag before displaying
		/// </remarks>
		public List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(string project_id, string artifact_type_name)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveForArtifactType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//All project members can see the list of types, so no additional check needed.

			//Need to convert the artifact type name into the appropriate enumeration
			DataModel.Artifact.ArtifactTypeEnum artifactType;
			if (!Enum.TryParse<DataModel.Artifact.ArtifactTypeEnum>(artifact_type_name, true, out artifactType))
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in 'artifact_type_name' doesn't match a valid artifact type");
				Logger.Flush();
				throw new WebFaultException<string>("The passed in 'artifact_type_name' doesn't match a valid artifact type", System.Net.HttpStatusCode.NotAcceptable);
			}

			//Get the custom properties for the current project and specified artifact
			//We don't get the corresponding custom lists right now, we'll do that in a separate step
			//since we also want the list values as well
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false, false);

			//Populate the API data objects and return
			List<RemoteCustomProperty> remoteCustomProperties = new List<RemoteCustomProperty>();
			foreach (CustomProperty customProperty in customProperties)
			{
				//Create and populate the row
				RemoteCustomProperty remoteCustomProperty = new RemoteCustomProperty(projectId, customProperty);
				if (customProperty.CustomPropertyListId.HasValue)
				{
					//We need to get the custom list and matching values for this custom property
					remoteCustomProperty.CustomList = CustomProperty_RetrieveCustomListById(project_id, customProperty.CustomPropertyListId.Value.ToString());
				}
				else
				{
					remoteCustomProperty.CustomList = null;
				}
				remoteCustomProperties.Add(remoteCustomProperty);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteCustomProperties;
		}

		/// <summary>
		/// Retrieves a custom list by its ID, including any custom list values
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="custom_list_id">The id of the custom list we want to retrieve</param>
		/// <returns>The custom list object (including any custom list values)</returns>
		public RemoteCustomList CustomProperty_RetrieveCustomListById(string project_id, string custom_list_id)
		{
			const string METHOD_NAME = "CustomProperty_RetrieveCustomListById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int customListId = RestUtils.ConvertToInt32(custom_list_id, "custom_list_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//All project members can see the list values, so no additional check needed.

			//Get the custom list for the project and list id
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				CustomPropertyList customPropertyList = customPropertyManager.CustomPropertyList_RetrieveById(customListId, true);

				//Make sure we have a list
				if (customPropertyList == null)
				{
					throw new Exception(Resources.Messages.Services_CustomListDoesNotExist);
				}

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
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region Release Methods

		/// <summary>
		/// Retrieves all the releases belonging to the current project
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>List of releases</returns>
		/// <remarks>Does not include iterations</remarks>
		public List<RemoteRelease> Release_Retrieve1(string project_id)
		{
			const string METHOD_NAME = "Release_Retrieve1";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the release dataset
				ReleaseManager releaseManager = new ReleaseManager();
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);

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
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a list of releases in the system that match the provided filter
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="number_rows">The number of rows to return</param>
		/// <param name="start_row">The first row to return (starting with 1)</param>
		/// <returns>List of releases</returns>
		public List<RemoteRelease> Release_Retrieve2(string project_id, string start_row, string number_rows, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Release_Retrieve2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_rows, "number_rows");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="release_id">The id of the release</param>
		/// <returns>Release object</returns>
		public RemoteRelease Release_RetrieveById(string project_id, string release_id)
		{
			const string METHOD_NAME = "Release_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
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
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_ReleaseNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_ReleaseNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>
		/// Maps a release to a test case, so that the test case is needs to be tested for that release
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="release_id">The id of the release we're mapping the test cases to</param>
		/// <param name="testCaseIds">The list of test cases being mapped</param>
		/// <remarks>If the mapping record already exists no error is raised</remarks>
		public void Release_AddTestMapping(string project_id, string release_id, int[] testCaseIds)
		{
			const string METHOD_NAME = "Release_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//If we have no test cases, just do nothing
			if (testCaseIds == null || testCaseIds.Length < 1)
			{
				return;
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Add the test cases we want to use for mapping
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.AddToRelease(projectId, releaseId, testCaseIds.ToList(), userId.Value);
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

		/// <summary>Returns the number of releases that match the filter.</summary>
		/// <param name="project_id">the id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Release_Count(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Release_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new ReleaseManager().Count(-1, projectId, filters, 0, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Creates a new release in the system at the root level
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteRelease">The new release object (primary key will be empty)</param>
		/// <returns>The populated release object - including the primary key</returns>
		public RemoteRelease Release_Create1(string project_id, RemoteRelease remoteRelease)
		{
			return Release_Create2(project_id, null, remoteRelease);
		}

		/// <summary>
		/// Creates a new release in the system under a specified parent release
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteRelease">The new release object (primary key will be empty)</param>
		/// <param name="parent_release_id">The id of the parent release we want to insert it under</param>
		/// <returns>The populated release object - including the primary key</returns>
		public RemoteRelease Release_Create2(string project_id, string parent_release_id, RemoteRelease remoteRelease)
		{
			const string METHOD_NAME = "Release_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentReleaseId = RestUtils.ConvertToInt32Nullable(parent_release_id, "parent_release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Always use the current project
			remoteRelease.ProjectId = projectId;

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
						throw new WebFaultException<string>(Resources.Messages.Services_ReleaseNotFound, System.Net.HttpStatusCode.NotFound);
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
							remoteRelease.StartDate,
							remoteRelease.EndDate,
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
							remoteRelease.StartDate,
							remoteRelease.EndDate,
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
					   remoteRelease.StartDate,
					   remoteRelease.EndDate,
					   remoteRelease.ResourceCount,
					   remoteRelease.DaysNonWorking,
					   null,
					   true);
				}

				//Now we need to populate any custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = null;
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Send a notification
				releaseManager.SendCreationNotification(remoteRelease.ReleaseId.Value, artifactCustomProperty, null);

				//Finally return the populated release object
				return remoteRelease;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Removes a mapping entry for a specific release and test case
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="release_id">The id of the release we want to unmap the test case from</param>
		/// <param name="test_case_id">The id of the test case that we want to unmap</param>
		public void Release_RemoveTestMapping(string project_id, string release_id, string test_case_id)
		{
			const string METHOD_NAME = "Release_RemoveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Restore the test case we want to remove coverage for
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.RemoveFromRelease(projectId, releaseId, new List<int>() { testCaseId }, userId.Value);
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
		/// Updates a release in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteRelease">The updated release object</param>
		public void Release_Update(string project_id, RemoteRelease remoteRelease)
		{
			const string METHOD_NAME = "Release_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyReleases, System.Net.HttpStatusCode.Unauthorized);
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
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
				}

				//Need to extract the data from the API data object and add to the artifact dataset and custom property dataset
				UpdateFunctions.UpdateReleaseData(release, remoteRelease);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteRelease, remoteRelease.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, remoteRelease.ReleaseId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

				//Get copies of everything..
				Artifact notificationArt = release.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Call the business object to actually update the release dataset and the custom properties
				releaseManager.Update(new List<Release>() { release }, userId.Value, projectId);
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_ReleaseNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_ReleaseNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>
		/// Deletes a release in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="release_id">The id of the release</param>
		public void Release_Delete(string project_id, string release_id)
		{
			const string METHOD_NAME = "Release_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the existing datarow
			try
			{
				ReleaseManager releaseManager = new ReleaseManager();
				ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);

				//Make sure that the project ids match
				if (release.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
				}

				//Call the business object to actually mark the item as deleted
				releaseManager.MarkAsDeleted(userId.Value, projectId, releaseId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.ResetContent);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_ReleaseNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_ReleaseNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Moves a release to another location in the hierarchy
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="release_id">The id of the release we want to move</param>
		/// <param name="destination_release_id">The id of the release it's to be inserted before in the list (or null to be at the end)</param>
		public void Release_Move(string project_id, string release_id, string destination_release_id)
		{
			const string METHOD_NAME = "Release_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");
			int? destinationReleaseId = RestUtils.ConvertToInt32Nullable(destination_release_id, "destination_release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update releases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the releases to make sure they exists and are in the authorized project
			try
			{
				ReleaseManager releaseManager = new ReleaseManager();
				ReleaseView sourceRelease = releaseManager.RetrieveById2(projectId, releaseId);

				//Make sure that the project ids match
				if (sourceRelease.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
				}
				if (destinationReleaseId.HasValue)
				{
					ReleaseView destRelease = releaseManager.RetrieveById2(projectId, destinationReleaseId.Value);
					if (destRelease.ProjectId != projectId)
					{
						throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
					}
				}

				//Call the business object to actually perform the move
				releaseManager.Move(userId.Value, projectId, releaseId, destinationReleaseId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_ReleaseNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_ReleaseNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>Retrieves comments for a specified release.</summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="release_id">The ID of the Release/Iteratyion to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified release.</returns>
		public List<RemoteComment> Release_RetrieveComments(string project_id, string release_id)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			List<RemoteComment> retList = new List<RemoteComment>();

			if (releaseId > 0)
			{
				retList = this.commentRetrieve(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release);
			}

			return retList;

		}

		/// <summary>
		/// Retrieves the mapped test cases for a specific release
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="release_id">The id of the release</param>
		/// <returns>The list of mapped test cases</returns>
		public List<RemoteReleaseTestCaseMapping> Release_RetrieveTestMapping(string project_id, string release_id)
		{
			const string METHOD_NAME = "Release_RetrieveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
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
		/// <param name="project_id">The id of the project</param>
		/// <param name="release_id">The id of the release</param>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Release_CreateComment(string project_id, string release_id, RemoteComment remoteComment)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			RemoteComment retComment = this.createComment(projectId, remoteComment, DataModel.Artifact.ArtifactTypeEnum.Release);

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

		#region Build Methods

		/// <summary>
		/// Retrieves the a single build (and associated source code revisions) by its id
		/// </summary>
		/// <param name="releaseId">The release we're interested in</param>
		/// <param name="buildId">The id of the build to retrieve</param>
		/// <returns>A single build object</returns>
		public RemoteBuild Build_RetrieveById(string project_id, string release_id, string build_id)
		{
			const string METHOD_NAME = "Build_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");
			int buildId = RestUtils.ConvertToInt32(build_id, "build_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases (since builds don't have their own permissions)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Call the business object to actually retrieve the build
				BuildManager buildManager = new BuildManager();
				Build build = buildManager.RetrieveById(buildId);

				//Make sure that the project ids match (to avoid returning data to unauthorized users)
				if (build.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.BuildDetails_BuildNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.BuildDetails_BuildNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves the list of builds that are associated with a specific Release
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <param name="releaseId">The release we're interested in</param>
		/// <param name="remoteSort">The sort to apply (pass null for default)</param>
		/// <returns>List of builds</returns>
		public List<RemoteBuild> Build_RetrieveByReleaseId(string project_id, string release_id)
		{
			const string METHOD_NAME = "Build_RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view releases (since builds don't have their own permissions)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the list of builds
			BuildManager buildManager = new BuildManager();
			List<BuildView> builds = buildManager.RetrieveForRelease(projectId, releaseId, 0);

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
		/// Creates a new build in the system, including any linked source code revisions
		/// </summary>
		/// <param name="remoteBuild">The new build object (primary key will be empty)</param>
		/// <returns>The populated build object - including the primary key</returns>
		public RemoteBuild Build_Create(string project_id, string release_id, RemoteBuild remoteBuild)
		{
			const string METHOD_NAME = "Build_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we don't have an primary key specified
			if (remoteBuild.BuildId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_BuildIdNotNull, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create releases (since builds don't have their own permissions)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateReleases, System.Net.HttpStatusCode.Unauthorized);
			}

			//See if we have a creation date specified, otherwise use current date/time
			DateTime creationDate = DateTime.UtcNow;
			if (remoteBuild.CreationDate.HasValue)
			{
				creationDate = remoteBuild.CreationDate.Value;
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
			   userId.Value
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
						remoteBuildSourceCode.CreationDate = DateTime.UtcNow;
					}
					BuildSourceCode buildSourceCode = buildManager.InsertSourceCodeRevision(
						remoteBuild.BuildId.Value,
						remoteBuildSourceCode.RevisionKey,
						remoteBuildSourceCode.CreationDate.Value
					);
				}
			}

			//Finally return the populated data object
			return remoteBuild;
		}

		#endregion

		#region Comment Methods (Internal)

		/// <summary>
		/// Internal function to create comments
		/// </summary>
		private List<RemoteComment> commentRetrieve(int projectId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "commentRetrieve";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
					newComment.CreationDate = commentRow.CreationDate;
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
		private RemoteComment createComment(int projectId, RemoteComment remoteComment, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "commentRetrieve";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			if (remoteComment.ArtifactId > 0)
			{
				DiscussionManager discussionManager = new DiscussionManager();
				if (remoteComment.CreationDate.HasValue)
				{
					remoteComment.CommentId = discussionManager.Insert(
						(remoteComment.UserId.HasValue) ? remoteComment.UserId.Value : userId,
						remoteComment.ArtifactId,
						artifactType,
						remoteComment.Text,
						remoteComment.CreationDate.Value,
						projectId,
						false,
						true
						);
				}
				else
				{
					remoteComment.CommentId = discussionManager.Insert(
						(remoteComment.UserId.HasValue) ? remoteComment.UserId.Value : userId,
						remoteComment.ArtifactId,
						artifactType,
						remoteComment.Text,
						projectId,
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

		#endregion Comment Methods (Internal)

		#region Association Methods

		/// <summary>
		/// Adds a new association in the system
		/// </summary>
		/// <param name="remoteAssociation">The association to add</param>
		/// <param name="project_id">The ID of the current project</param>
		/// <returns>The association with its primary key populated</returns>
		public RemoteAssociation Association_Create(string project_id, RemoteAssociation remoteAssociation)
		{
			const string METHOD_NAME = "Association_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify the source artifact type in question
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)remoteAssociation.SourceArtifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Default to the authenticated user if no creator provided
				int creatorId = userId.Value;
				if (remoteAssociation.CreatorId.HasValue)
				{
					creatorId = remoteAssociation.CreatorId.Value;
				}

				//Check the comment length
				string comment = remoteAssociation.Comment.MaxLength(255);

				//If the creation date is not specified, use the current one
				if (!remoteAssociation.CreationDate.HasValue)
				{
					remoteAssociation.CreationDate = DateTime.UtcNow;
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
				   remoteAssociation.CreationDate.Value,
				   ArtifactLink.ArtifactLinkTypeEnum.RelatedTo
				   );
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteAssociation;
		}

		/// <summary>
		/// Updates the specified Association's information
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteAssociation">The updated association information</param>
		/// <remarks>
		/// Currently only the comment field is updated
		/// </remarks>
		public void Association_Update(string project_id, RemoteAssociation remoteAssociation)
		{
			const string METHOD_NAME = "Incident_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an artifact link id specified
			if (!remoteAssociation.ArtifactLinkId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_ArtifactLinkIdMissing, System.Net.HttpStatusCode.PreconditionFailed);
			}

			//Make sure that we're not trying update a system-generated association (they have negative primary keys)
			if (remoteAssociation.ArtifactLinkId.Value < 0)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_ArtifactLinkReadOnly, System.Net.HttpStatusCode.PreconditionFailed);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify the source artifact type in question
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)remoteAssociation.SourceArtifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyArtifact, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the existing datarow
			try
			{
				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				ArtifactLink artifactLink = artifactLinkManager.RetrieveById(remoteAssociation.ArtifactLinkId.Value);

				//Need to extract the data from the API data object and add to the internal dataset
				UpdateFunctions.UpdateAssociationData(artifactLink, remoteAssociation);

				//Call the business object to actually update the association
				artifactLinkManager.Update(artifactLink, userId.Value);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Retrieves a set of associations to the specified artifact
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_type_id">The id of the artifact type</param>
		/// <param name="artifact_id">The id of the artifact</param>
		/// <returns>An array of association records</returns>
		/// <remarks>
		/// The source artifact type and id will be the same as the ones passed in
		/// </remarks>
		public List<RemoteAssociation> Association_RetrieveForArtifact(string project_id, string artifact_type_id, string artifact_id)
		{
			const string METHOD_NAME = "Association_RetrieveForArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int artifactId = RestUtils.ConvertToInt32(artifact_id, "artifact_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view the source artifact type in question
			DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
			if (!IsAuthorized(projectId, artifactType, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewArtifact, System.Net.HttpStatusCode.Unauthorized);
			}
			try
			{
				//Call the business object to actually retrieve the association record
				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, "CreationDate", false, null);

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
					remoteAssociation.CreationDate = artifactLink.CreationDate;
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
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region DataMapping functions

		/// <summary>
		/// Retrieves a list of data mappings for artifact ids in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_type_id">The type of artifact we're interested in</param>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		public List<RemoteDataMapping> DataMapping_RetrieveArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="data_sync_system_id">The id of the plug-in we're interested in</param>
		/// <param name="artifact_type_id">The type of artifact the mappings are for</param>
		/// <param name="remoteDataMappings">The list of mapping entries to add</param>
		public void DataMapping_AddArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_AddArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="data_sync_system_id">The id of the plug-in we're interested in</param>
		/// <param name="artifact_type_id">The type of artifact the mappings are for</param>
		/// <param name="remoteDataMappings">The list of mapping entries to remove</param>
		public void DataMapping_RemoveArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_RemoveArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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

		/// <summary>
		/// Adds new user data mapping entries
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in we're interested in</param>
		/// <param name="remoteDataMappings">The list of mapping entries to add</param>
		public void DataMapping_AddUserMappings(string data_sync_system_id, List<RemoteDataMapping> remoteDataMappings)
		{
			const string METHOD_NAME = "DataMapping_AddUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

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
		/// Retrieves a list of data mappings for users in the system
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		public List<RemoteDataMapping> DataMapping_RetrieveUserMappings(string data_sync_system_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

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
		/// Retrieves the data mapping for a custom property
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <param name="artifact_type_id">The id of the type of artifact</param>
		/// <param name="custom_property_id">The id of the custom property</param>
		/// <returns>Data mapping object</returns>
		public RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(string project_id, string data_sync_system_id, string artifact_type_id, string custom_property_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveCustomPropertyMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int customPropertyId = RestUtils.ConvertToInt32(custom_property_id, "custom_property_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <param name="artifact_type_id">The id of the type of artifact</param>
		/// <param name="custom_property_id">The id of the custom property that the values are for</param>
		/// <returns>The list of data mappings</returns>
		public List<RemoteDataMapping> DataMapping_RetrieveCustomPropertyValueMappings(string project_id, string data_sync_system_id, string artifact_type_id, string custom_property_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactTypeId = RestUtils.ConvertToInt32(artifact_type_id, "artifact_type_id");
			int customPropertyId = RestUtils.ConvertToInt32(custom_property_id, "custom_property_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
		/// Retrieves a list of data mappings for artifact field values
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="artifact_field_id">The field we're interested in</param>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		public List<RemoteDataMapping> DataMapping_RetrieveFieldValueMappings(string project_id, string data_sync_system_id, string artifact_field_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");
			int artifactFieldId = RestUtils.ConvertToInt32(artifact_field_id, "artifact_field_id");

			//Make sure we are connected to a project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
		/// Retrieves a list of data mappings for projects in the system
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <returns>The list of data mappings</returns>
		public List<RemoteDataMapping> DataMapping_RetrieveProjectMappings(string data_sync_system_id)
		{
			const string METHOD_NAME = "DataMapping_RetrieveProjectMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

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

		#endregion

		#region DataSync Methods

		/// <summary>
		/// Retrieves a list of data-sync plug-ins that need to be synchronized with
		/// </summary>
		/// <returns>The list of datasync plug-ins</returns>
		public List<RemoteDataSyncSystem> DataSyncSystem_Retrieve()
		{
			const string METHOD_NAME = "DataSyncSystem_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//We don't need to know the user as long as it authenticates since it can only
			//read the list of plug-ins which is not secure/private information

			//Get the list of data syncs in the system
			DataMappingManager dataMappingManager = new DataMappingManager();
			List<DataSyncSystem> dataSyncSystems = dataMappingManager.RetrieveDataSyncSystems();

			//Populate the API data object and return
			List<RemoteDataSyncSystem> remoteDataSyncSystems = new List<RemoteDataSyncSystem>();
			foreach (DataSyncSystem dataSyncSystem in dataSyncSystems)
			{
				//Create and populate the row
				RemoteDataSyncSystem remoteDataSyncSystem = new RemoteDataSyncSystem();
				PopulationFunctions.PopulateDataSyncSystem(remoteDataSyncSystem, dataSyncSystem);
				remoteDataSyncSystems.Add(remoteDataSyncSystem);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteDataSyncSystems;
		}

		/// <summary>
		/// Updates the status for a failed data-sync plug-in
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		public void DataSyncSystem_SaveRunFailure(string data_sync_system_id)
		{
			const string METHOD_NAME = "DataSyncSystem_SaveRunFailure";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

			DataMappingManager dataMappingManager = new DataMappingManager();
			dataMappingManager.SaveRunFailure(dataSyncSystemId);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the status for a successful data-sync plug-in
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <param name="lastRunDate">The date it last ran</param>
		public void DataSyncSystem_SaveRunSuccess(string data_sync_system_id, DateTime lastRunDate)
		{
			const string METHOD_NAME = "DataSyncSystem_SaveRunSuccess";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

			DataMappingManager dataMappingManager = new DataMappingManager();
			dataMappingManager.SaveRunSuccess(dataSyncSystemId, lastRunDate);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the status for a data-sync plug-in that executed with warnings
		/// </summary>
		/// <param name="data_sync_system_id">The id of the plug-in</param>
		/// <param name="lastRunDate">The date it last ran</param>
		public void DataSyncSystem_SaveRunWarning(string data_sync_system_id, DateTime lastRunDate)
		{
			const string METHOD_NAME = "DataSyncSystem_SaveRunWarning";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int dataSyncSystemId = RestUtils.ConvertToInt32(data_sync_system_id, "data_sync_system_id");

			DataMappingManager dataMappingManager = new DataMappingManager();
			dataMappingManager.SaveRunWarning(dataSyncSystemId, lastRunDate);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Writes an error/warning event entry to the SpiraTeam database log
		/// </summary>
		/// <param name="remoteEvent">The details of the event</param>
		public void DataSyncSystem_WriteEvent(RemoteEvent remoteEvent)
		{
			const string METHOD_NAME = "DataSyncSystem_WriteEvent";

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				Logger.WriteToDatabase("DataSync::DataSyncSystem_WriteEvent", remoteEvent.Message, remoteEvent.Details, (System.Diagnostics.EventLogEntryType)remoteEvent.EventLogEntryType, null, Logger.EVENT_CATEGORY_DATA_SYNCHRONIZATION);
			}
			catch
			{
				//Do Nothing
			}
		}

		#endregion

		#region Document Type methods

		/// <summary>
		/// Retrieves a list of all the document types in the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="active_only">Do we only want the active types</param>
		/// <returns>List of document types</returns>
		public List<RemoteDocumentType> Document_RetrieveTypes(string project_id, string active_only)
		{
			const string METHOD_NAME = "Document_RetrieveTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			bool activeOnly = RestUtils.ConvertToBoolean(active_only, "active_only");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

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
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		#endregion

		#region Document Folder methods

		/// <summary>
		/// Retrieves a list of all the document folders in the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>List of document types</returns>
		public List<RemoteDocumentFolder> Document_RetrieveFolders(string project_id)
		{
			const string METHOD_NAME = "Document_RetrieveFolders";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

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
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>Retrieves the folder by the specified ID.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="folder_id">The id of the folder being retrieved</param>
		/// <returns>RemoteDocumentFolder</returns>
		public RemoteDocumentFolder Document_RetrieveFolderById(string project_id, string folder_id)
		{
			const string METHOD_NAME = "Document_RetrieveFolderById";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int folderId = RestUtils.ConvertToInt32(folder_id, "folder_id");

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
			catch (ArtifactNotExistsException exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception exception)
			{
				//Log and then rethrow the converted exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Adds a new document folder into the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteDocumentFolder">The new document folder</param>
		/// <returns>The document folder with the primary key populated</returns>
		public RemoteDocumentFolder Document_AddFolder(string project_id, RemoteDocumentFolder remoteDocumentFolder)
		{
			const string METHOD_NAME = "Document_AddFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to add document folders
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, userId.Value);
			if (projectUser == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}
			int projectRoleId = projectUser.ProjectRoleId;

			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedAddDocumentFolders, System.Net.HttpStatusCode.Unauthorized);
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
		/// Deletes an existing document folder from the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="folder_id">The id of the folder to delete</param>
		/// <remarks>This will delete all child folders and documents/attachements</remarks>
		public void Document_DeleteFolder(string project_id, string folder_id)
		{
			const string METHOD_NAME = "Document_DeleteFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int projectAttachmentFolderId = RestUtils.ConvertToInt32(folder_id, "folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete document folders
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, userId.Value);
			if (projectUser == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}
			int projectRoleId = projectUser.ProjectRoleId;

			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteDocumentFolders, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Now delete the document folder after making sure the folder is actually in the specified project
				AttachmentManager attachmentManager = new AttachmentManager();
				ProjectAttachmentFolder attachmentFolder = attachmentManager.RetrieveFolderById(projectAttachmentFolderId);
				if (attachmentFolder.ProjectId != projectId)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_ItemNotBelongToProject, System.Net.HttpStatusCode.BadRequest);
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
		/// Updates the name and position of an existing folder in the project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="folder_id">The id of the folder being updated</param>
		/// <param name="remoteDocumentFolder">The updated folder information</param>
		public void Document_UpdateFolder(string project_id, string folder_id, RemoteDocumentFolder remoteDocumentFolder)
		{
			const string METHOD_NAME = "Document_UpdateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that we have an folder id specified
			if (!remoteDocumentFolder.ProjectAttachmentFolderId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<String>(Resources.Messages.Services_ProjectAttachmentFolderIdMissing, System.Net.HttpStatusCode.BadRequest);
			}

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int projectAttachmentFolderId = RestUtils.ConvertToInt32(folder_id, "folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to edit document folders
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, userId.Value);
			if (projectUser == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}
			int projectRoleId = projectUser.ProjectRoleId;

			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyDocumentFolders, System.Net.HttpStatusCode.Unauthorized);
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

		#endregion

		#region Requirement Methods

		/// <summary>Returns the number of requirements that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Requirement_Count1(string project_id)
		{
			return Requirement_Count2(project_id, null);
		}

		/// <summary>Returns the number of requirements that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long Requirement_Count2(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Requirement_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewRequirements, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new RequirementManager().Count(-1, projectId, filters, 0, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Maps a requirement to a test case, so that the test case 'covers' the requirement
		/// </summary>
		/// <param name="remoteReqTestCaseMapping">The requirement and test case mapping entry</param>
		/// <remarks>If the coverage record already exists no error is raised</remarks>
		public void Requirement_AddTestCoverage(string project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			const string METHOD_NAME = "Requirement_AddTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Add the test case we want to use for coverage
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReqTestCaseMapping.TestCaseId);
				testCaseManager.AddToRequirement(projectId, remoteReqTestCaseMapping.RequirementId, testCaseIds, userId.Value);
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
		/// Creates a new requirement record in the current project at the end of the list
		/// </summary>
		/// <param name="remoteRequirement">The new requirement object (primary key will be empty)</param>
		/// <param name="indentPosition">The number of columns to indent the requirement by (positive for indent, negative for outdent)</param>
		/// <returns>The populated requirement object - including the primary key</returns>
		/// <remarks>This version is use when you want to specify the relative indentation level</remarks>
		public RemoteRequirement Requirement_Create1(string project_id, RemoteRequirement remoteRequirement)
		{
			return this.Requirement_Create3(project_id, null, remoteRequirement);
		}

		/// <summary>
		/// Creates a new requirement record in the current project using the position offset method
		/// </summary>
		/// <param name="remoteRequirement">The new requirement object (primary key will be empty)</param>
		/// <param name="indentPosition">The number of columns to indent the requirement by (positive for indent, negative for outdent)</param>
		/// <returns>The populated requirement object - including the primary key</returns>
		/// <remarks>This version is use when you want to specify the relative indentation level</remarks>
		public RemoteRequirement Requirement_Create2(string project_id, string indent_position, RemoteRequirement remoteRequirement)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int indentPosition = RestUtils.ConvertToInt32(indent_position, "indent_position");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewRequirements, System.Net.HttpStatusCode.Unauthorized);
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
				//If the template setting allows bulk edit of status, set the status
				ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
				if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
				{
					requirementStatus = (Requirement.RequirementStatusEnum)(remoteRequirement.StatusId.Value);
				}
			}

			//The current project is always used
			remoteRequirement.ProjectId = projectId;

			//If we have a passed in parent requirement, then we need to insert the requirement as a child item
			Business.RequirementManager requirementManager = new Business.RequirementManager();

			//Convert the old planned effort into points
			decimal? estimatePoints = requirementManager.GetEstimatePointsFromEffort(projectId, remoteRequirement.PlannedEffort);

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteRequirement.ImportanceId >= 1 && remoteRequirement.ImportanceId <= 4)
			{
				UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
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
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
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
		public RemoteRequirement Requirement_Create3(string project_id, string parent_requirement_id, RemoteRequirement remoteRequirement)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentRequirementId = RestUtils.ConvertToInt32Nullable(parent_requirement_id, "parent_requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewRequirements, System.Net.HttpStatusCode.Unauthorized);
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
				//If the template setting allows bulk edit of status, set the status
				ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
				if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
				{
					requirementStatus = (Requirement.RequirementStatusEnum)(remoteRequirement.StatusId.Value);
				}
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
				UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
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
					userId
				   );
			}

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			requirementManager.SendCreationNotification(remoteRequirement.RequirementId.Value, artifactCustomProperty, null);

			//Finally return the populated requirement object
			return remoteRequirement;
		}

		/// <summary>
		/// Removes a coverage mapping entry for a specific requirement and test case
		/// </summary>
		/// <param name="remoteReqTestCaseMapping">The requirement and test case mapping entry</param>
		public void Requirement_RemoveTestCoverage(string project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			const string METHOD_NAME = "Requirement_RemoveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Restore the test case we want to remove coverage for
				TestCaseManager testCaseManager = new TestCaseManager();
				List<int> testCaseIds = new List<int>();
				testCaseIds.Add(remoteReqTestCaseMapping.TestCaseId);
				testCaseManager.RemoveFromRequirement(projectId, remoteReqTestCaseMapping.RequirementId, testCaseIds, userId.Value);
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
		/// Retrieves a list of requirements in the system that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of requirements</returns>
		public List<RemoteRequirement> Requirement_Retrieve1(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Requirement_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewRequirements, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Extract the filters from the provided API object
				Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Call the business object to actually retrieve the requirement dataset
				RequirementManager requirementManager = new RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, startingRow, numberOfRows, filters, 0);

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
		/// Retrieves a list of requirements in the system
		/// </summary>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <returns>List of requirements</returns>
		public List<RemoteRequirement> Requirement_Retrieve2(string project_id, string starting_row, string number_of_rows)
		{
			return this.Requirement_Retrieve1(project_id, starting_row, number_of_rows, null);
		}

		/// <summary>
		/// Retrieves a single requirement in the system
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>Requirement object</returns>
		public RemoteRequirement Requirement_RetrieveById(string project_id, string requirement_id)
		{
			const string METHOD_NAME = "Requirement_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewRequirements, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the requirement dataset
			RequirementManager requirementManager = new RequirementManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//If the requirement was not found, just return null
			try
			{
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Updates a requirement in the system
		/// </summary>
		/// <param name="remoteRequirement">The updated requirement object</param>
		public void Requirement_Update(string project_id, RemoteRequirement remoteRequirement)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyRequirements, System.Net.HttpStatusCode.Unauthorized);
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
				UpdateFunctions.UpdateRequirementData(projectTemplateId, requirement, remoteRequirement);

				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteRequirement, remoteRequirement.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteRequirement.RequirementId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query
			try
			{
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
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves the test coverage for a specific requirement
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>The list of mapped test cases</returns>
		public List<RemoteRequirementTestCaseMapping> Requirement_RetrieveTestCoverage(string project_id, string requirement_id)
		{
			const string METHOD_NAME = "Requirement_RetrieveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
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
		/// Deletes a requirement in the system
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		public void Requirement_Delete(string project_id, string requirement_id)
		{
			const string METHOD_NAME = "Requirement_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Moves a requirement to another location in the hierarchy
		/// </summary>
		/// <param name="requirementId">The id of the requirement we want to move</param>
		/// <param name="destinationRequirement">The id of the requirement it's to be inserted before in the list (or null to be at the end)</param>
		public void Requirement_Move(string project_id, string requirement_id, string destination_requirement_id)
		{
			const string METHOD_NAME = "Requirement_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");
			int? destinationRequirementId = RestUtils.ConvertToInt32Nullable(destination_requirement_id, "destination_requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyRequirements, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Indents a requirement one position
		/// </summary>
		/// <param name="requirementId">The id of the requirement to indent</param>
		public void Requirement_Indent(string project_id, string requirement_id)
		{
			const string METHOD_NAME = "Requirement_Indent";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyRequirements, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the requirement to make sure it exists and is in the authorized project
			try
			{
				RequirementManager requirementManager = new RequirementManager();
				RequirementView sourceRequirement = requirementManager.RetrieveById2(projectId, requirementId);

				//Make sure that the project ids match
				if (sourceRequirement.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually perform the indent
				requirementManager.Indent(userId, projectId, requirementId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Outdents a requirement one position
		/// </summary>
		/// <param name="requirementId">The id of the requirement to outdent</param>
		public void Requirement_Outdent(string project_id, string requirement_id)
		{
			const string METHOD_NAME = "Requirement_Outdent";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update requirements
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyRequirements, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the requirement to make sure it exists and is in the authorized project
			try
			{
				RequirementManager requirementManager = new RequirementManager();
				RequirementView sourceRequirement = requirementManager.RetrieveById2(projectId, requirementId);

				//Make sure that the project ids match
				if (sourceRequirement.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Call the business object to actually perform the outdent
				requirementManager.Outdent(userId, projectId, requirementId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_RequirementNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_RequirementNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>Retrieves comments for a specified requirement.</summary>
		/// <param name="RequirementId">The ID of the Requirement to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified requirement.</returns>
		public List<RemoteComment> Requirement_RetrieveComments(string project_id, string requirement_id)
		{
			List<RemoteComment> retList = new List<RemoteComment>();
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			if (requirementId > 0)
			{
				retList = this.commentRetrieve(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a requirement.</summary>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Requirement_CreateComment(string project_id, string requirement_id, RemoteComment remoteComment)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int requirementId = RestUtils.ConvertToInt32(requirement_id, "requirement_id");

			RemoteComment retComment = this.createComment(projectId, remoteComment, DataModel.Artifact.ArtifactTypeEnum.Requirement);

			return retComment;
		}

		#endregion

		#region User Methods

		/// <summary>Creates a new user in the system and adds them to the current project as the specified role</summary>
		/// <param name="password">The new password for the user (leave empty if an LDAP user)</param>
		/// <param name="password_question">The new password retrieval question for the user (leave empty if an LDAP user)</param>
		/// <param name="password_answer">The new password retrieval answer for the user (leave empty if an LDAP user)</param>
		/// <param name="remoteUser">The new user object (primary key will be empty)</param>
		/// <param name="project_role_id">The project role for the user (leave as null to not add user to current project)</param>
		/// <param name="project_id">The id of the project for the user to be added (leave as null to not add user to current project)</param>
		/// <returns>The populated user object - including the primary key</returns>
		public RemoteUser User_Create(string password, string password_question, string password_answer, string project_id, string project_role_id, RemoteUser remoteUser)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int authenticatedUserId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int? projectId = RestUtils.ConvertToInt32Nullable(project_id, "project_id");
			int? projectRoleId = RestUtils.ConvertToInt32Nullable(project_role_id, "project_role_id");
			string passwordQuestion = password_question;
			string passwordAnswer = password_answer;

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(authenticatedUserId);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateUsers, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
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
					if (projectRoleId.HasValue && projectId.HasValue)
					{
						//Ignore any duplicate key errors
						try
						{
							projectManager.InsertUserMembership(existingUserId, projectId.Value, projectRoleId.Value);
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

					return remoteUser;
				}

				//Next create the user, if it exists already ignore the exception
				MembershipCreateStatus status;
				SpiraMembershipProvider membershipProvider = (SpiraMembershipProvider)Membership.Provider;
				membershipUser = membershipProvider.CreateUser(remoteUser.UserName, password, remoteUser.EmailAddress, passwordQuestion, passwordAnswer, remoteUser.Approved, remoteUser.LdapDn, remoteUser.RssToken, out status);
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
					if (projectRoleId.HasValue)
					{
						profile.LastOpenedProjectId = projectId;
					}
					profile.IsEmailEnabled = true;
					profile.Department = remoteUser.Department;
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
				if (projectRoleId.HasValue && projectId.HasValue)
				{
					try
					{
						projectManager.InsertUserMembership(userId, projectId.Value, projectRoleId.Value);
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
		/// Retrieves the details of the current, authenticated user
		/// </summary>
		/// <returns>The user object</returns>
		public RemoteUser User_Retrieve()
		{
			const string METHOD_NAME = "User_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = this.AuthenticatedUserId.Value;

			//See if we're a system administrator
			UserManager userManager = new UserManager();
			bool isSystemAdmin = false;
			try
			{
				User user = userManager.GetUserById(userId);
				if (user != null && user.Profile.IsAdmin && user.IsActive && user.IsApproved && !user.IsLocked)
				{
					isSystemAdmin = true;
				}
			}
			catch (ArtifactNotExistsException)
			{
			}

			//If the user was not found, just return null
			try
			{
				User user = userManager.GetUserById(userId);

				//Populate the API data object and return
				RemoteUser remoteUser = new RemoteUser();
				PopulationFunctions.PopulateUser(remoteUser, user, isSystemAdmin);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteUser;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_UserNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_UserNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>Retrieves a single user in the system</summary>
		/// <param name="user_id">The id of the user</param>
		/// <returns>The user object</returns>
		public RemoteUser User_RetrieveById(string user_id)
		{
			const string METHOD_NAME = "User_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int authenticatedUserId = this.AuthenticatedUserId.Value;

			//See if we're a system administrator
			UserManager userManager = new UserManager();
			bool isSystemAdmin = false;
			try
			{
				User user = userManager.GetUserById(authenticatedUserId);
				if (user != null && user.Profile.IsAdmin && user.IsActive && user.IsApproved && !user.IsLocked)
				{
					isSystemAdmin = true;
				}
			}
			catch (ArtifactNotExistsException)
			{
			}

			//Convert the parameters into their native types
			int userId = RestUtils.ConvertToInt32(user_id, "user_id");

			//If the user was not found, just return null
			try
			{
				User user = userManager.GetUserById(userId);

				//Populate the API data object and return
				RemoteUser remoteUser = new RemoteUser();
				PopulationFunctions.PopulateUser(remoteUser, user, isSystemAdmin);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteUser;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_UserNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_UserNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>Retrieves a single user in the system by user-name</summary>
		/// <param name="user_name">The login of the user</param>
		/// <returns>The user object</returns>
		public RemoteUser User_RetrieveByUserName(string user_name)
		{
			const string METHOD_NAME = "User_RetrieveByUserName";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int authenticatedUserId = this.AuthenticatedUserId.Value;

			//See if we're a system administrator
			UserManager userManager = new UserManager();
			bool isSystemAdmin = false;
			try
			{
				User user = userManager.GetUserById(authenticatedUserId);
				if (user != null && user.Profile.IsAdmin && user.IsActive && user.IsApproved && !user.IsLocked)
				{
					isSystemAdmin = true;
				}
			}
			catch (ArtifactNotExistsException)
			{
			}

			string userName = user_name;

			//If the user was not found, just return null
			try
			{
				User user = userManager.GetUserByLogin(userName);

				//Populate the API data object and return
				RemoteUser remoteUser = new RemoteUser();
				PopulationFunctions.PopulateUser(remoteUser, user, isSystemAdmin);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteUser;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_UserNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_UserNotFound, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>Tries to delete the specified user ID. Note that this function will fail if any other foreign keys (fields) in other tables are assigned to the user that is specified.) Must be connected to the API as the root Administrator.</summary>
		/// <param name="user_id">The user ID to delete.</param>
		public void User_Delete(string user_id)
		{
			const string METHOD_NAME = CLASS_NAME + "User_Delete";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int authenticatedUserId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int userId = RestUtils.ConvertToInt32(user_id, "user_id");

			//Make sure we're a system administrator
			try
			{
				User user = new UserManager().GetUserById(authenticatedUserId);
				if (user == null || !user.Profile.IsAdmin || !user.IsActive || !user.IsApproved || user.IsLocked)
				{
					throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
				}
			}
			catch (ArtifactNotExistsException)
			{
				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				bool success = new Business.UserManager().DeleteUser(userId, false);
				if (!success)
				{
					throw CreateFault("DeleteUserFailed", "Failed to delete user.");
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex, "Trying to delete specified user #" + userId.ToString());
				throw CreateFault(ex.GetType().ToString(), "Unable to delete user.");
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion

		#region Task Methods

		/// <summary>
		/// Creates a new task in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTask">The new task object (primary key will be empty)</param>
		/// <returns>The populated task object - including the primary key</returns>
		public RemoteTask Task_Create(string project_id, RemoteTask remoteTask)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTasks, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Always use the current project
			remoteTask.ProjectId = projectId;

			//Default to the authenticated user if we have no creator provided
			int creatorId = userId.Value;
			if (remoteTask.CreatorId.HasValue)
			{
				creatorId = remoteTask.CreatorId.Value;
			}

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTask.TaskPriorityId >= 1 && remoteTask.TaskPriorityId <= 4)
			{
				UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteTask);
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
			   remoteTask.StartDate,
			   remoteTask.EndDate,
			   remoteTask.EstimatedEffort,
			   remoteTask.ActualEffort,
			   remoteTask.RemainingEffort,
			   userId);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

			//Send a notification
			taskManager.SendCreationNotification(remoteTask.TaskId.Value, artifactCustomProperty, null);

			//Finally return the populated task object
			return remoteTask;
		}

		/// <summary>Returns the number of tasks in the project.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The number of items.</returns>
		public long Task_Count1(string project_id)
		{
			return this.Task_Count2(project_id, null);
		}

		/// <summary>Returns the number of tasks that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The number of items.</returns>
		public long Task_Count2(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "Task_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTasks, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TaskManager().Count(projectId, filters, 0, null, false);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Retrieves a list of tasks in the system that match the provided filter/sort
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="sort_field">The field to sort by</param>
		/// <param name="sort_direction">The direction to sort [ASC|DESC]</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of tasks</returns>
		public List<RemoteTask> Task_Retrieve(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "Task_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");
			string sortBy = (String.IsNullOrEmpty(sort_field) ? "CreationDate" : sort_field.Trim());
			bool sortAscending = (sort_direction != null && sort_direction.ToLowerInvariant() != "desc");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTasks, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.Retrieve(projectId, sortBy, sortAscending, startRow, numberRows, filters, 0);

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
		/// Retrieves a single task in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="task_id">The id of the task</param>
		/// <returns>Task object</returns>
		public RemoteTask Task_RetrieveById(string project_id, string task_id)
		{
			const string METHOD_NAME = "Task_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int taskId = RestUtils.ConvertToInt32(task_id, "task_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTasks, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();

			//If the task was not found, just return null
			try
			{
				TaskView task = taskManager.TaskView_RetrieveById(taskId);

				//Make sure that the project ids match
				if (task.ProjectId != projectId)
				{
					throw CreateFault("ItemNotBelongToProject", Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_TaskNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_TaskNotFound, System.Net.HttpStatusCode.NotFound);
			}
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query
			try
			{
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
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves all new tasks added in the system since the date specified
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="start_row">The starting row</param>
		/// <param name="number_of_rows">The maximum number of rows to return</param>
		/// <param name="creation_date">The date after which the task needs to have been created</param>
		/// <returns>List of tasks</returns>
		public List<RemoteTask> Task_RetrieveNew(string project_id, string creation_date, string start_row, string number_of_rows)
		{
			const string METHOD_NAME = "Task_RetrieveNew";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startRow = RestUtils.ConvertToInt32(start_row, "start_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_rows");
			DateTime creationDate = RestUtils.ConvertToDateTime(creation_date, "creation_date");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTasks, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the task dataset
			TaskManager taskManager = new TaskManager();
			Hashtable filters = new Hashtable();
			Common.DateRange dateRange = new Common.DateRange();
			dateRange.StartDate = creationDate;
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			List<TaskView> tasks = taskManager.Retrieve(projectId, "CreationDate", true, startRow, numberOfRows, filters, 0);

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
		/// Updates a task in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTask">The updated task object</param>
		public void Task_Update(string project_id, RemoteTask remoteTask)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTasks, System.Net.HttpStatusCode.Unauthorized);
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
				UpdateFunctions.UpdateTaskData(projectTemplateId, task, remoteTask);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTask, remoteTask.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.Task, remoteTask.TaskId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_TaskNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_TaskNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Deletes a task in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="task_id">The id of the task</param>
		public void Task_Delete(string project_id, string task_id)
		{
			const string METHOD_NAME = "Task_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int taskId = RestUtils.ConvertToInt32(task_id, "task_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete tasks
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
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
				taskManager.MarkAsDeleted(projectId, taskId, userId.Value);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.Services_TaskNotFound);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.Services_TaskNotFound, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>Retrieves comments for a specified task.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="task_id">The ID of the task to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified task.</returns>
		public List<RemoteComment> Task_RetrieveComments(string project_id, string task_id)
		{
			List<RemoteComment> retList = new List<RemoteComment>();
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int taskId = RestUtils.ConvertToInt32(task_id, "task_id");

			if (taskId > 0)
			{
				retList = this.commentRetrieve(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a task.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="task_id">The ID of the task to add comments to</param>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment Task_CreateComment(string project_id, string task_id, RemoteComment remoteComment)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int taskId = RestUtils.ConvertToInt32(task_id, "task_id");

			//Use the provided task id of the data object does not have one specified
			if (remoteComment.ArtifactId < 1 && taskId > 0)
			{
				remoteComment.ArtifactId = taskId;
			}

			RemoteComment retComment = this.createComment(projectId, remoteComment, DataModel.Artifact.ArtifactTypeEnum.Task);

			return retComment;
		}

		#endregion

		#region Test Case Methods

		/// <summary>
		/// Retrieves a list of testCases in a particular release that match the provided filter
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startingRow">The first row to return (starting with 1)</param>
		/// <param name="releaseId">The release we're interested in</param>
		/// <returns>List of testCases</returns>
		public List<RemoteTestCase> TestCase_RetrieveByReleaseId(string project_id, string release_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "TestCase_RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int releaseId = RestUtils.ConvertToInt32(release_id, "release_id");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test case)
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(projectId, releaseId, "Name", true, startingRow, numberOfRows, filters, 0);

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

		/// <summary>Returns the number of test cases in the project</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The number of items.</returns>
		public long TestCase_Count1(string project_id)
		{
			return this.TestCase_Count2(project_id, null);
		}

		/// <summary>Returns the number of test cases that match the filter.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long TestCase_Count2(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestCase_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestCaseManager().Count(projectId, filters, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Adds a new parameter for a test case
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestCaseParameter">The new test case parameter to add</param>
		/// <returns>The test case parameter object with the primary key populated</returns>
		/// <remarks>The parameter name is always made lower case</remarks>
		public RemoteTestCaseParameter TestCase_AddParameter(string project_id, RemoteTestCaseParameter remoteTestCaseParameter)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
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
		/// Creates a new test case in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestCase">The new test case object (primary key will be empty)</param>
		/// <param name="parent_test_folder_id">Do we want to insert the test case under a parent folder</param>
		/// <returns>The populated test case object - including the primary key</returns>
		public RemoteTestCase TestCase_Create(string project_id, string parent_test_folder_id, RemoteTestCase remoteTestCase)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentTestFolderId = RestUtils.ConvertToInt32Nullable(parent_test_folder_id, "parent_test_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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

			//See if we have any IDs that used to be static and that are now template/project based
			//If so, we need to convert them
			if (remoteTestCase.TestCasePriorityId >= 1 && remoteTestCase.TestCasePriorityId <= 4)
			{
				UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteTestCase);
			}

			//Insert test case inside the folder
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
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			testCaseManager.SendCreationNotification(remoteTestCase.TestCaseId.Value, artifactCustomProperty, null);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestCase;
		}

		/// <summary>
		/// Creates a new test case folder in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestCase">The new test case object (primary key will be empty)</param>
		/// <param name="parent_test_folder_id">Do we want to insert the test case under a parent folder</param>
		/// <returns>The populated test case object - including the primary key</returns>
		public RemoteTestCase TestCase_CreateFolder(string project_id, string parent_test_folder_id, RemoteTestCase remoteTestCase)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentTestFolderId = RestUtils.ConvertToInt32Nullable(parent_test_folder_id, "parent_test_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestFolders, System.Net.HttpStatusCode.Unauthorized);
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
		/// Returns the full token of a test case parameter from its name
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="parameter_name">The name of the parameter</param>
		/// <returns>The tokenized representation of the parameter used for search/replace</returns>
		/// <remarks>We use the same parameter format as Ant/NAnt</remarks>
		public string TestCase_CreateParameterToken(string project_id, string parameter_name)
		{
			return TestCaseManager.CreateParameterToken(parameter_name);
		}

		/// <summary>
		/// Retrieves a list of testCases in the system that match the provided filter
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of testCases</returns>
		public List<RemoteTestCase> TestCase_Retrieve1(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "TestCase_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the testCase dataset
			TestCaseManager testCaseManager = new TestCaseManager();

			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, startingRow, numberOfRows, filters, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

			//Get the complete folder list for the project
			List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);

			//All Items / Filtered List - we need to group under the appropriate folder
			List<IGrouping<int?, TestCaseView>> groupedTestCasesByFolder = testCases.GroupBy(t => t.TestCaseFolderId).ToList();

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
		/// Retrieves a list of testCases in the project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of testCases</returns>
		public List<RemoteTestCase> TestCase_Retrieve2(string project_id, string starting_row, string number_of_rows)
		{
			return this.TestCase_Retrieve1(project_id, starting_row, number_of_rows, null);
		}

		/// <summary>
		/// Retrieves the list of defined parameters for a test case along with the associated default value
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case</param>
		/// <returns>List of parameters</returns>
		public List<RemoteTestCaseParameter> TestCase_RetrieveParameters(string project_id, string test_case_id)
		{
			const string METHOD_NAME = "TestCase_RetrieveParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test case parameters
			TestCaseManager testCaseManager = new TestCaseManager();

			try
			{
				//First retrieve the test case and verify it belongs to the current project (for security reasons)
				TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
				if (testCase.ProjectId != projectId)
				{
					throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
				}

				//Now retrieve the parameters
				List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testCaseId);

				//Loop through the dataset and populate the API object
				List<RemoteTestCaseParameter> remoteTestCaseParameters = new List<RemoteTestCaseParameter>();
				foreach (TestCaseParameter testCaseParameter in testCaseParameters)
				{
					RemoteTestCaseParameter remoteTestCaseParameter = new RemoteTestCaseParameter();
					remoteTestCaseParameter.TestCaseParameterId = testCaseParameter.TestCaseParameterId;
					remoteTestCaseParameter.TestCaseId = testCaseParameter.TestCaseId;
					remoteTestCaseParameter.Name = testCaseParameter.Name;
					remoteTestCaseParameter.DefaultValue = testCaseParameter.DefaultValue;
					remoteTestCaseParameters.Add(remoteTestCaseParameter);
				}

				return remoteTestCaseParameters;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves a single test case/folder in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case/folder</param>
		/// <returns>Test Case object</returns>
		/// <remarks>
		/// This also populates the test steps collection if you have the appropriate permissions
		/// </remarks>
		public RemoteTestCase TestCase_RetrieveById(string project_id, string test_case_id)
		{
			const string METHOD_NAME = "TestCase_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test case dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test case was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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

					//Get the test case custom properties
					List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

					//Populate the API data object and return
					remoteTestCase = new RemoteTestCase();
					PopulationFunctions.PopulateTestCase(remoteTestCase, testCase);
					PopulationFunctions.PopulateCustomProperties(remoteTestCase, testCase, customProperties);

					//See if this user has the permissions to view test steps
					//Make sure we have permissions to view test steps
					if (IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, Project.PermissionEnum.View))
					{
						//Populate the test step API data objects and return
						List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);

						//Get the test step custom property definitions
						customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);

						remoteTestCase.TestSteps = new List<RemoteTestStep>();
						foreach (TestStepView testStep in testSteps)
						{
							RemoteTestStep remoteTestStep = new RemoteTestStep();
							PopulationFunctions.PopulateTestStep(remoteTestStep, testStep, projectId);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query
			try
			{
				//Call the business object to actually retrieve the testCase dataset
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(userId, null);

				//Get the custom property definitions - for all projects
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
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Retrieves all test cases that are stored in a particular test folder
		/// </summary>
		/// <param name="testCaseFolderId">The id of the test case folder</param>
		/// <returns>List of testCases</returns>
		public List<RemoteTestCase> TestCase_RetrieveByFolder(string project_id, string test_case_folder_id)
		{
			const string METHOD_NAME = "TestCase_RetrieveByFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseFolderId = RestUtils.ConvertToInt32(test_case_folder_id, "test_case_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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

			//Next get the test cases in the folder
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, testCaseFolderId);

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
		/// Updates a test case in the system together with its test steps (if populated)
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestCase">The updated test case object</param>
		/// <remarks>Does not currently update test step custom properties</remarks>
		public void TestCase_Update(string project_id, RemoteTestCase remoteTestCase)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//First retrieve the existing datarow
			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
					testCaseFolder.LastUpdateDate = remoteTestCase.LastUpdateDate;
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
					UpdateFunctions.UpdateTestCaseData(projectTemplateId, testCase, remoteTestCase);
					UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestCase, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteTestCase.TestCaseId.Value, projectTemplateId);
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
									UpdateFunctions.UpdateTestStepData(testStep, remoteTestStep);
									UpdateFunctions.UpdateCustomPropertyData(ref testStepCustomProperty, remoteTestStep, remoteTestCase.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, remoteTestStep.TestStepId.Value, projectTemplateId);

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Deletes a test case in the system
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="test_case_id">The id of the test case</param>
		/// <remarks>
		/// Also deletes any child test steps
		/// </remarks>
		public void TestCase_Delete(string project_id, string test_case_id)
		{
			const string METHOD_NAME = "TestCase_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Moves a test case to another location in the hierarchy
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case we want to move</param>
		/// <param name="destination_test_case_folder_id">The id of the test case it's to be inserted inside (or null to be at the root)</param>
		public void TestCase_Move(string project_id, string test_case_id, string destination_test_case_folder_id)
		{
			const string METHOD_NAME = "TestCase_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int? destinationTestCaseFolderId = RestUtils.ConvertToInt32Nullable(destination_test_case_folder_id, "destination_test_case_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
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
				if (destinationTestCaseFolderId.HasValue)
				{
					//Make sure the folder is positive
					if (destinationTestCaseFolderId.Value < 0)
					{
						destinationTestCaseFolderId = -destinationTestCaseFolderId;
					}

					TestCaseFolder destTestCaseFolder = testCaseManager.TestCaseFolder_GetById(destinationTestCaseFolderId.Value);
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
				testCaseManager.TestCase_UpdateFolder(testCaseId, destinationTestCaseFolderId);
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>Adds or updates the automation test script associated with a test case</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case</param>
		/// <param name="automation_engine_id">The id of the automation engine</param>
		/// <param name="url_or_filename">The url or filename for the test script</param>
		/// <param name="description">The description of the automation script</param>
		/// <param name="binaryData">The binary data that comprises the script (only for file attachments)</param>
		/// <param name="version">The version of the test script</param>
		/// <param name="project_attachment_type_id">The attachment type to store the script under (optional)</param>
		/// <param name="project_attachment_folder_id">The attachment folder to store the script under (optional)</param>
		public void TestCase_AddUpdateAutomationScript(string project_id, string test_case_id, string automation_engine_id, string url_or_filename, string description, string version, string project_attachment_type_id, string project_attachment_folder_id, byte[] binaryData)
		{
			const string METHOD_NAME = "TestCase_AddUpdateAutomationScript";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int? automationEngineId = RestUtils.ConvertToInt32Nullable(automation_engine_id, "automation_engine_id");
			string urlOrFilename = url_or_filename;
			int? projectAttachmentTypeId = RestUtils.ConvertToInt32Nullable(project_attachment_type_id, "project_attachment_type_id");
			int? projectAttachmentFolderId = RestUtils.ConvertToInt32Nullable(project_attachment_folder_id, "project_attachment_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to update test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the test case function to update the automation script
			TestCaseManager testCaseManager = new TestCaseManager();
			testCaseManager.AddUpdateAutomationScript(userId.Value, projectId, testCaseId, automationEngineId, urlOrFilename, description, binaryData, version, projectAttachmentTypeId, projectAttachmentFolderId);

			try
			{
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>Gives a count of items within the given folder</summary>
		/// ><param name="project_id">The id of the current project</param>
		/// <param name="test_folder_id">The parent TestCase folder to count in.</param>
		/// <returns>A number of all items (folder objects and test cases) in the given parent folder ID.</returns>
		public long TestCase_CountForFolder1(string project_id, string test_folder_id)
		{
			return TestCase_CountForFolder2(project_id, test_folder_id, null);
		}

		/// <summary>Gives a count of items within the given folder that match the filters.</summary>
		/// ><param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">Filters for the child items.</param>
		/// <param name="test_folder_id">The parent TestCase folder to count in.</param>
		/// <returns>A number of all items (folder objects and test cases) in the given parent folder ID.</returns>
		public long TestCase_CountForFolder2(string project_id, string test_folder_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestCase_CountForFolder";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testFolderId = RestUtils.ConvertToInt32(test_folder_id, "test_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestCaseManager().Count(projectId, filters, testFolderId, 0);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>Retrieves comments for a specified test case.</summary>
		/// <param name="test_case_id">The ID of the test case to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified test case.</returns>
		public List<RemoteComment> TestCase_RetrieveComments(string project_id, string test_case_id)
		{
			List<RemoteComment> retList = new List<RemoteComment>();
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			if (testCaseId > 0)
			{
				retList = this.commentRetrieve(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a test case.</summary>
		/// <param name="test_case_id">The ID of the test case to add comments to</param>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment TestCase_CreateComment(string project_id, string test_case_id, RemoteComment remoteComment)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Use the provided test case id of the data object does not have one specified
			if (remoteComment.ArtifactId < 1 && testCaseId > 0)
			{
				remoteComment.ArtifactId = testCaseId;
			}

			RemoteComment retComment = this.createComment(projectId, remoteComment, DataModel.Artifact.ArtifactTypeEnum.TestCase);

			return retComment;
		}

		#endregion

		#region Test Step Methods

		/// <summary>
		/// Retrieves the list of parameters and provided values for a test link step
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case</param>
		/// <param name="test_step_id">The id of the test step (link)</param>
		/// <returns>List of parameters</returns>
		public List<RemoteTestStepParameter> TestCase_RetrieveStepParameters(string project_id, string test_case_id, string test_step_id)
		{
			const string METHOD_NAME = "TestCase_RetrieveStepParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int testStepId = RestUtils.ConvertToInt32(test_step_id, "test_step_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test case parameters
			TestCaseManager testCaseManager = new TestCaseManager();

			try
			{
				//First retrieve the test case and verify it belongs to the current project (for security reasons)
				//and that the test step is inside the specified test case (for security reasons)
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
				if (testCase.ProjectId != projectId)
				{
					throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
				}
				TestStep testStep = testCase.TestSteps.FirstOrDefault(s => s.TestStepId == testStepId);
				if (testStep == null)
				{
					throw CreateFault("AuthorizationViolation", Resources.Messages.Services_NotAuthorizedViewTestCases);
				}

				//Next actually retrieve the parameters
				List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);

				//Loop through the dataset and populate the API object
				List<RemoteTestStepParameter> testStepParameters = new List<RemoteTestStepParameter>();
				foreach (TestStepParameter dataRow in testStepParameterValues)
				{
					RemoteTestStepParameter testStepParameter = new RemoteTestStepParameter();
					testStepParameter.Name = dataRow.Name;
					testStepParameter.Value = dataRow.Value;
					testStepParameters.Add(testStepParameter);
				}

				return testStepParameters;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw ConvertExceptions(exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Moves a test step to a different position in the test case
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case we're interested in</param>
		/// <param name="source_test_step_id">The id of the test step we want to move</param>
		/// <param name="destination_test_step_id">The id of the test step we want to move it in front of (passing Null means end of the list)</param>
		public void TestCase_MoveStep(string project_id, string test_case_id, string source_test_step_id, string destination_test_step_id)
		{
			const string METHOD_NAME = "TestCase_MoveStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int sourceTestStepId = RestUtils.ConvertToInt32(source_test_step_id, "source_test_step_id");
			int? destinationTestStepId = RestUtils.ConvertToInt32Nullable(destination_test_step_id, "destination_test_step_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Deletes a test step in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case</param>
		/// <param name="test_step_id">The id of the test step</param>
		/// <remarks>Doesn't throw an exception if the step no longer exists</remarks>
		public void TestCase_DeleteStep(string project_id, string test_case_id, string test_step_id)
		{
			const string METHOD_NAME = "TestCase_DeleteStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int testStepId = RestUtils.ConvertToInt32(test_step_id, "test_step_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete test steps
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Adds a new test step to the specified test case
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestStep">The new test step object (primary key will be empty)</param>
		/// <param name="test_case_id">The test case to add it to</param>
		/// <returns>The populated test step object - including the primary key</returns>
		public RemoteTestStep TestCase_AddStep(string project_id, string test_case_id, RemoteTestStep remoteTestStep)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test steps
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestSteps, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, remoteTestStep.TestStepId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, true);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestStep, projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, remoteTestStep.TestStepId.Value, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The test case to add it to</param>
		/// <param name="linked_test_case_id">The test case being linked to</param>
		/// <param name="parameters">Any parameters to passed to the linked test case</param>
		/// <param name="position">The position in the test step list</param>
		/// <returns>The id of the new test step</returns>
		public int TestCase_AddLink(string project_id, string test_case_id, string linked_test_case_id, string position, List<RemoteTestStepParameter> parameters)
		{
			const string METHOD_NAME = "TestCase_AddLink";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int linkedTestCaseId = RestUtils.ConvertToInt32(linked_test_case_id, "linked_test_case_id");
			int? positionNumber = RestUtils.ConvertToInt32Nullable(position, "position");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test steps
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestSteps, System.Net.HttpStatusCode.Unauthorized);
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
				int testStepId = testCaseManager.InsertLink(userId, testCaseId, positionNumber, linkedTestCaseId, parametersDictionary);

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

		#endregion

		#region Test Set Methods

		/// <summary>Retrieves comments for a specified test set.</summary>
		/// <param name="test_set_id">The ID of the test set to retrieve comments for.</param>
		/// <returns>An array of comments associated with the specified test set.</returns>
		public List<RemoteComment> TestSet_RetrieveComments(string project_id, string test_set_id)
		{
			List<RemoteComment> retList = new List<RemoteComment>();
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			if (testSetId > 0)
			{
				retList = this.commentRetrieve(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);
			}

			return retList;
		}

		/// <summary>Creates a new comment for a test set.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The ID of the test set to add comments to</param>
		/// <param name="remoteComment">The remote comment.</param>
		/// <returns>The RemoteComment with the comment's new ID specified.</returns>
		public RemoteComment TestSet_CreateComment(string project_id, string test_set_id, RemoteComment remoteComment)
		{
			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Use the provided test set id of the data object does not have one specified
			if (remoteComment.ArtifactId < 1 && testSetId > 0)
			{
				remoteComment.ArtifactId = testSetId;
			}

			RemoteComment retComment = this.createComment(projectId, remoteComment, DataModel.Artifact.ArtifactTypeEnum.TestSet);

			return retComment;
		}

		/// <summary>
		/// Retrieves all the test cases that are part of a test set
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The id of the test set</param>
		/// <returns>List of Test Case objects</returns>
		public List<RemoteTestCase> TestCase_RetrieveByTestSetId(string project_id, string test_set_id)
		{
			const string METHOD_NAME = "TestCase_RetrieveByTestSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test set test case dataset
			TestSetManager testSetManager = new TestSetManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test case was not found, just return null
			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//We use the option to return with execution data
				List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases3(projectId, testSetId, null);

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves a list of testSets in the current project
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>List of testSets</returns>
		public List<RemoteTestSet> TestSet_Retrieve1(string project_id)
		{
			return this.TestSet_Retrieve2(project_id, 1.ToString(), Int32.MaxValue.ToString(), null);
		}

		/// <summary>
		/// Retrieves a list of testSets in the system that match the provided filter
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of testSets</returns>
		public List<RemoteTestSet> TestSet_Retrieve2(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "TestSet_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestSets, System.Net.HttpStatusCode.Unauthorized);
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
		/// Retrieves a single test set/folder in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The id of the test set/folder</param>
		/// <returns>Test Set object</returns>
		public RemoteTestSet TestSet_RetrieveById(string project_id, string test_set_id)
		{
			const string METHOD_NAME = "TestSet_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestSets, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestSetDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestSetDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;
			//We assume if they are specified as owner, they have permissions since we can't easily
			//check cross-project permissions in one query
			try
			{
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
			catch (Exception exception)
			{
				//Log and convert to FaultException
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw ConvertExceptions(exception);
			}
		}

		/// <summary>
		/// Maps a test set to a test case, so that the test case is part of the test set
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_case_id">The id of the test case being mapped</param>
		/// <param name="test_set_id">The id of the test set being mapped</param>
		/// <param name="owner_id">The optonal owner of the test case within this test set</param>
		/// <param name="existing_test_set_test_case_id">The id of the existing entry we want to insert it before (if not set, will be simply added to the end of the list)</param>
		/// <param name="parameters">Any parameter values to be passed from the test set to the test case</param>
		/// <remarks>
		/// You can only pass in a test case id not a test case folder id
		/// </remarks>
		public List<RemoteTestSetTestCaseMapping> TestSet_AddTestMapping(string project_id, string test_set_id, string test_case_id, string owner_id, string existing_test_set_test_case_id, List<RemoteTestSetTestCaseParameter> parameters)
		{
			const string METHOD_NAME = "TestSet_AddTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");
			int testCaseId = RestUtils.ConvertToInt32(test_case_id, "test_case_id");
			int? existingTestSetTestCaseId = RestUtils.ConvertToInt32Nullable(existing_test_set_test_case_id, "existing_test_set_test_case_id");
			int? ownerId = RestUtils.ConvertToInt32(owner_id, "owner_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Now add the test case to the test set, seeing if we need to set any parameter values or not
				TestSetManager testSetManager = new TestSetManager();
				int testSetTestCaseId;
				if (parameters != null && parameters.Count > 0)
				{
					Dictionary<string, string> parameterValues = new Dictionary<string, string>();
					foreach (RemoteTestSetTestCaseParameter parameter in parameters)
					{
						parameterValues.Add(parameter.Name, parameter.Value);
					}
					testSetTestCaseId = testSetManager.AddTestCase(projectId, testSetId, testCaseId, ownerId, existingTestSetTestCaseId, parameterValues);
				}
				else
				{
					testSetTestCaseId = testSetManager.AddTestCase(projectId, testSetId, testCaseId, ownerId, existingTestSetTestCaseId);
				}

				//Convert the mapping to the remote API object
				List<RemoteTestSetTestCaseMapping> mappings = new List<RemoteTestSetTestCaseMapping>();
				RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
				remoteTestSetTestCaseMapping.TestSetTestCaseId = testSetTestCaseId;
				remoteTestSetTestCaseMapping.TestSetId = testSetId;
				remoteTestSetTestCaseMapping.TestCaseId = testCaseId;
				mappings.Add(remoteTestSetTestCaseMapping);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return mappings;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new test set in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestSet">The new test set object (primary key will be empty)</param>
		/// <param name="parent_test_set_folder_id">Do we want to insert the test set under a parent folder</param>
		/// <returns>The populated test set object - including the primary key</returns>
		public RemoteTestSet TestSet_Create(string project_id, string parent_test_set_folder_id, RemoteTestSet remoteTestSet)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentTestSetFolderId = RestUtils.ConvertToInt32Nullable(parent_test_set_folder_id, "parent_test_set_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestSets, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
				remoteTestSet.PlannedDate,
				TestRun.TestRunTypeEnum.Manual,
				remoteTestSet.AutomationHostId,
				(remoteTestSet.RecurrenceId.HasValue) ? (TestSet.RecurrenceEnum?)remoteTestSet.RecurrenceId : null
				);

			//Now we need to populate any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = null;
			Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value, projectTemplateId);
			if (validationMessages != null && validationMessages.Count > 0)
			{
				//Throw a validation exception
				throw CreateValidationException(validationMessages);
			}
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Send a notification
			testSetManager.SendCreationNotification(remoteTestSet.TestSetId.Value, artifactCustomProperty, null);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestSet;
		}

		/// <summary>
		/// Creates a new test set folder in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestSet">The new test set object (primary key will be empty)</param>
		/// <param name="parent_test_set_folder_id">Do we want to insert the test set under a parent folder</param>
		/// <returns>The populated test set object - including the primary key</returns>
		public RemoteTestSet TestSet_CreateFolder(string project_id, string parent_test_set_folder_id, RemoteTestSet remoteTestSet)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? parentTestSetFolderId = RestUtils.ConvertToInt32Nullable(parent_test_set_folder_id, "parent_test_set_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestFolders, System.Net.HttpStatusCode.Unauthorized);
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
		/// Removes a test case from a test set
		/// </summary>
		/// <param name="test_set_id">The id of the test set</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_test_case_id">The id of the test set and test case mapping entry</param>
		public void TestSet_RemoveTestMapping(string project_id, string test_set_id, string test_set_test_case_id)
		{
			const string METHOD_NAME = "TestSet_RemoveTestMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");
			int testSetTestCaseId = RestUtils.ConvertToInt32(test_set_test_case_id, "test_set_test_case_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestCases, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Now remove the test case from the test set
				TestSetManager testSetManager = new TestSetManager();
				testSetManager.RemoveTestCase(projectId, testSetId, testSetTestCaseId);
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
		/// Retrieves all the test cases that are part of a test set
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The id of the test set</param>
		/// <returns>List of Test Set, Test Case mapping objects</returns>
		public List<RemoteTestSetTestCaseMapping> TestSet_RetrieveTestCaseMapping(string project_id, string test_set_id)
		{
			const string METHOD_NAME = "TestSet_RetrieveTestCaseMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test cases
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestCases, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestCaseDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);

			}
		}

		/// <summary>
		/// Updates a test set in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestSet">The updated test set object</param>
		public void TestSet_Update(string project_id, RemoteTestSet remoteTestSet)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test set
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestSets, System.Net.HttpStatusCode.Unauthorized);
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
					testSetFolder.LastUpdateDate = remoteTestSet.LastUpdateDate;
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
					UpdateFunctions.UpdateTestSetData(testSet, remoteTestSet);
					Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestSet, remoteTestSet.ProjectId.Value, DataModel.Artifact.ArtifactTypeEnum.TestSet, remoteTestSet.TestSetId.Value, projectTemplateId);
					if (validationMessages != null && validationMessages.Count > 0)
					{
						//Throw a validation exception
						throw CreateValidationException(validationMessages);
					}

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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestSetDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestSetDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Deletes a test set in the system
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The id of the test set</param>
		public void TestSet_Delete(string project_id, string test_set_id)
		{
			const string METHOD_NAME = "TestSet_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to delete test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.Delete))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedDeleteArtifactType, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestSetDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestSetDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Moves a test set to another location in the hierarchy
		/// </summary>
		/// <param name="project_id">The id of the project</param>
		/// <param name="test_set_id">The id of the test set we want to move</param>
		/// <param name="destination_test_set_folder_id">The id of the test set it's to be inserted inside (or null to be at the root)</param>
		public void TestSet_Move(string project_id, string test_set_id, string destination_test_set_folder_id)
		{
			const string METHOD_NAME = "TestSet_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");
			int? destinationTestSetFolderId = RestUtils.ConvertToInt32Nullable(destination_test_set_folder_id, "destination_test_set_folder_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to modify test set
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.Modify))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedModifyTestSets, System.Net.HttpStatusCode.Unauthorized);
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
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestSetDetails_ArtifactNotExists);
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestSetDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>Returns the number of test sets in the project</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <returns>The number of items.</returns>
		public long TestSet_Count1(string project_id)
		{
			return this.TestSet_Count2(project_id, null);
		}

		/// <summary>Returns the number of test sets that match the filter.</summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <returns>The number of items.</returns>
		public long TestSet_Count2(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestSet_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test sets
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestSets, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestSetManager().Count(projectId, filters, 0, null, false, true);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		#endregion

		#region Test Run Methods

		/// <summary>
		/// Creates a new test run shell from the provided test case(s)
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="testCaseIds">The list of test cases to create the run for</param>
		/// <param name="release_id">A release to associate the test run with (optional)</param>
		/// <returns>The list of new test case run data objects</returns>
		public List<RemoteManualTestRun> TestRun_CreateFromTestCases(string project_id, string release_id, List<int> testCaseIds)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestCases";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int? releaseId = RestUtils.ConvertToInt32Nullable(release_id, "release_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(userId.Value, projectId, releaseId, testCaseIds, false);

				//Populate the API data object and return
				//We don't have any custom properties to populate at this point
				List<RemoteManualTestRun> remoteTestRuns = new List<RemoteManualTestRun>();
				foreach (TestRun testRun in testRunsPending.TestRuns)
				{
					RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun, projectId);
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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The test set to create the run for</param>
		/// <returns>The list of new test case run data objects</returns>
		public List<RemoteManualTestRun> TestRun_CreateFromTestSet(string project_id, string test_set_id)
		{
			const string METHOD_NAME = "TestRun_CreateFromTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Instantiate the test run business class
				Business.TestRunManager testRunManager = new Business.TestRunManager();

				//Actually create the new test run
				TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(userId.Value, projectId, testSetId, false);

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Populate the API data object and return
				//We don't have any custom properties to populate at this point
				List<RemoteManualTestRun> remoteTestRuns = new List<RemoteManualTestRun>();
				foreach (TestRun testRun in testRunsPending.TestRuns)
				{
					RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun, projectId);
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
		/// Creates a shell set of test runs for an external automated test runner based on the provided automation host token
		/// and the specified date range
		/// </summary>
		/// <param name="project_id">The of the current project</param>
		/// <param name="automation_host_token">The unique token that identifies this host</param>
		/// <param name="dateRange">The range of planned dates that we want to include test sets for</param>
		/// <returns>The list of test run objects</returns>
		public List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(string project_id, string automation_host_token, DataObjects.DateRange dateRange)
		{
			const string METHOD_NAME = "TestRun_CreateForAutomationHost";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			string automationHostToken = automation_host_token;

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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
				foreach (TestSetView testSet in testSets)
				{
					int testSetId = testSet.TestSetId;

					//Now we need to retieve any test set parameter values (used later)
					List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);

					//Iterate through the test cases in the test set
					List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
					foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
					{
						//Get the actual test case record for this item and make sure it has an automation engine and script
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
							remoteTestRun.ScheduledDate = testSet.PlannedDate;
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

							//Now see if we have any default parameter values for parameters not mentioned in the test set
							List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testSetTestCase.TestCaseId);

							//Add parameters/values, test case ones override test set ones
							UpdateFunctions.AddParameterValues(remoteTestRun, testCaseParameters, testSetParameterValues, testSetTestCaseParameterValues);
						}
					}

					//Also copy across any custom property list values
					UpdateFunctions.AddCustomPropertyValuesToAutomatedTestRun(projectId, projectTemplateId, testSetId, remoteTestRuns);
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
		/// Creates a shell set of test runs for an external automated test runner based on the provided test set id
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_set_id">The automated test set we want to execute</param>
		/// <param name="automation_host_token">The unique token that identifies this host</param>
		/// <returns>The list of test run objects</returns>
		/// <remarks>For this method the test set doesn't need an automated host to be set</remarks>
		public List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(string project_id, string test_set_id, string automation_host_token)
		{
			const string METHOD_NAME = "TestRun_CreateForAutomatedTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testSetId = RestUtils.ConvertToInt32(test_set_id, "test_set_id");
			string automationHostToken = automation_host_token;

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

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

						//Now see if we have any default parameter values for parameters not mentioned in the test set
						List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testSetTestCase.TestCaseId);

						//Now we need to add any test parameters
						UpdateFunctions.AddParameterValues(remoteTestRun, testCaseParameters, testSetParameterValues, testSetTestCaseParameterValues);
					}
				}

				//Also copy across any custom property list values
				UpdateFunctions.AddCustomPropertyValuesToAutomatedTestRun(projectId, projectTemplateId, testSetId, remoteTestRuns);

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

		/// <summary>Returns the number of test runs in the project.</summary>
		/// <param name="project_id">The id of the project</param>
		/// <returns>The number of items.</returns>
		public long TestRun_Count1(string project_id)
		{
			return TestRun_Count2(project_id, null);
		}

		/// <summary>Returns the number of test runs that match the filter.</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="project_id">The id of the project</param>
		/// <returns>The number of items.</returns>
		public long TestRun_Count2(string project_id, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_Count";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Call the business object to actually retrieve the incident dataset
			long retNum = new TestRunManager().Count(projectId, filters, 0);

			Logger.LogExitingEvent(METHOD_NAME);
			return retNum;
		}

		/// <summary>
		/// Records the results of executing an automated test
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestRun">The automated test run information</param>
		/// <returns>the test run data object with its primary key populated</returns>
		/// <remarks>
		/// You need to use this overload when you want to be able to set Test Run custom properties
		/// </remarks>
		public RemoteAutomatedTestRun TestRun_RecordAutomated1(string project_id, RemoteAutomatedTestRun remoteTestRun)
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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}


			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Default to the authenticated user if we have no tester provided
				int testerId = userId;
				if (remoteTestRun.TesterId.HasValue)
				{
					testerId = remoteTestRun.TesterId.Value;
				}

				//Default to plain text test run formats if not set
				if (remoteTestRun.TestRunFormatId < 1)
				{
					remoteTestRun.TestRunFormatId = (int)TestRun.TestRunFormatEnum.PlainText;
				}

				//Handle nullable properties
				DateTime endDate = DateTime.UtcNow;
				string runnerTestName = "Unknown?";
				string runnerMessage = "Nothing Reported";
				string runnerStackTrace = "Nothing Reported";
				int runnerAssertCount = 0;
				if (remoteTestRun.EndDate.HasValue)
				{
					endDate = remoteTestRun.EndDate.Value;
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

				//See if we have any test steps that need to be included
				List<TestRunStepInfo> testRunSteps = null;
				if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
				{
					testRunSteps = new List<TestRunStepInfo>();
					for (int i = 0; i < remoteTestRun.TestRunSteps.Count; i++)
					{
						RemoteTestRunStep remoteTestRunStep = remoteTestRun.TestRunSteps[i];
						TestRunStepInfo testRunStep = new TestRunStepInfo();
						testRunStep.TestStepId = remoteTestRunStep.TestStepId;
						testRunStep.Description = remoteTestRunStep.Description;
						testRunStep.ExpectedResult = remoteTestRunStep.ExpectedResult;
						testRunStep.SampleData = remoteTestRunStep.SampleData;
						testRunStep.ActualResult = remoteTestRunStep.ActualResult;
						testRunStep.ExecutionStatusId = remoteTestRunStep.ExecutionStatusId;
						//Generate a position from the index if one is not provided
						if (remoteTestRunStep.Position < 1)
						{
							remoteTestRunStep.Position = i + 1;
						}
						testRunStep.Position = remoteTestRunStep.Position;
						testRunSteps.Add(testRunStep);
					}
				}

				//Actually create the new test run
				int testRunId = testRunManager.Record(
				   projectId,
				   testerId,
				   remoteTestRun.TestCaseId,
				   remoteTestRun.ReleaseId,
				   remoteTestRun.TestSetId,
				   remoteTestRun.TestSetTestCaseId,
				   remoteTestRun.StartDate,
				   endDate,
				   remoteTestRun.ExecutionStatusId,
				   remoteTestRun.RunnerName,
				   runnerTestName,
				   runnerAssertCount,
				   runnerMessage,
				   runnerStackTrace,
				   remoteTestRun.AutomationHostId,
				   remoteTestRun.AutomationEngineId,
				   remoteTestRun.BuildId,
				   (TestRun.TestRunFormatEnum)remoteTestRun.TestRunFormatId,
				   testRunSteps
				   );

				//Need to now save the custom properties if necessary
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);
				Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, projectTemplateId);
				if (validationMessages != null && validationMessages.Count > 0)
				{
					//Throw a validation exception
					throw CreateValidationException(validationMessages);
				}

				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//Populate the API data object with the primary keys and return
				remoteTestRun.TestRunId = testRunId;
				if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
				{
					foreach (TestRunStepInfo testRunStep in testRunSteps)
					{
						RemoteTestRunStep remoteTestRunStep = remoteTestRun.TestRunSteps.FirstOrDefault(r => r.Position == testRunStep.Position);
						if (remoteTestRunStep != null)
						{
							remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestCaseDetails_ArtifactNotExists);
				Logger.Flush();
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
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestRuns">The list of populated automated test runs</param>
		/// <returns>The list of test runs with the TestRunId populated</returns>
		/// <seealso cref="TestRun_RecordAutomated1"/>
		public List<RemoteAutomatedTestRun> TestRun_RecordAutomated2(string project_id, List<RemoteAutomatedTestRun> remoteTestRuns)
		{
			const string METHOD_NAME = "TestRun_RecordAutomated2";

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

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			if (!AuthenticatedUserId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int userId = AuthenticatedUserId.Value;

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			try
			{
				//Instantiate the business classes
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Loop through the test runs
				foreach (RemoteAutomatedTestRun remoteTestRun in remoteTestRuns)
				{
					//Default to the authenticated user if we have no tester provided
					int testerId = userId;
					if (remoteTestRun.TesterId.HasValue)
					{
						testerId = remoteTestRun.TesterId.Value;
					}

					//Default to plain text test run formats if not set
					if (remoteTestRun.TestRunFormatId < 1)
					{
						remoteTestRun.TestRunFormatId = (int)TestRun.TestRunFormatEnum.PlainText;
					}

					//Handle nullable properties
					DateTime endDate = DateTime.UtcNow;
					string runnerTestName = "Unknown?";
					string runnerMessage = "Nothing Reported";
					string runnerStackTrace = "Nothing Reported";
					int runnerAssertCount = 0;
					if (remoteTestRun.EndDate.HasValue)
					{
						endDate = remoteTestRun.EndDate.Value;
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

					//See if we have any test steps that need to be included
					List<TestRunStepInfo> testRunSteps = null;
					if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
					{
						testRunSteps = new List<TestRunStepInfo>();
						for (int i = 0; i < remoteTestRun.TestRunSteps.Count; i++)
						{
							RemoteTestRunStep remoteTestRunStep = remoteTestRun.TestRunSteps[i];
							TestRunStepInfo testRunStep = new TestRunStepInfo();
							testRunStep.TestStepId = remoteTestRunStep.TestStepId;
							testRunStep.Description = remoteTestRunStep.Description;
							testRunStep.ExpectedResult = remoteTestRunStep.ExpectedResult;
							testRunStep.SampleData = remoteTestRunStep.SampleData;
							testRunStep.ActualResult = remoteTestRunStep.ActualResult;
							testRunStep.ExecutionStatusId = remoteTestRunStep.ExecutionStatusId;
							//Generate a position from the index if one is not provided
							if (remoteTestRunStep.Position < 1)
							{
								remoteTestRunStep.Position = i + 1;
							}
							testRunStep.Position = remoteTestRunStep.Position;
							testRunSteps.Add(testRunStep);
						}
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
					   remoteTestRun.StartDate,
					   endDate,
					   remoteTestRun.ExecutionStatusId,
					   remoteTestRun.RunnerName,
					   runnerTestName,
					   runnerAssertCount,
					   runnerMessage,
					   runnerStackTrace,
					   remoteTestRun.AutomationHostId,
					   remoteTestRun.AutomationEngineId,
					   remoteTestRun.BuildId,
					   (TestRun.TestRunFormatEnum)remoteTestRun.TestRunFormatId,
					   testRunSteps,
					   false
					   );

					//Need to now save the custom properties if necessary
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);
					Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, projectTemplateId);
					if (validationMessages != null && validationMessages.Count > 0)
					{
						//Throw a validation exception
						throw CreateValidationException(validationMessages);
					}

					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Populate the API data object with the primary key and return
					remoteTestRun.TestRunId = testRunId;
					if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
					{
						foreach (TestRunStepInfo testRunStep in testRunSteps)
						{
							RemoteTestRunStep remoteTestRunStep = remoteTestRun.TestRunSteps.FirstOrDefault(r => r.Position == testRunStep.Position);
							if (remoteTestRunStep != null)
							{
								remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
							}
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
		/// Saves set of test runs, each containing test run steps
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="remoteTestRuns">The test run objects to persist</param>
		/// <param name="end_date">The effective end-date of the test run (leave null to use the values specified on each test run object)</param>
		/// <returns>The saved copy of the test run objects (contains generated IDs)</returns>
		public List<RemoteManualTestRun> TestRun_Save(string project_id, string end_date, List<RemoteManualTestRun> remoteTestRuns)
		{
			const string METHOD_NAME = "TestRun_Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			DateTime? endDate = RestUtils.ConvertToDateTimeNullable(end_date, "end_date");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to create test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedCreateTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have some test runs
			if (remoteTestRuns == null || remoteTestRuns.Count == 0)
			{
				throw new WebFaultException<string>(Resources.Messages.Services_NoDataProvided, System.Net.HttpStatusCode.BadRequest);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the test run business class and dataset
				TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TestRunsPending testRunsPending = new TestRunsPending();
				List<RemoteManualTestRun> updatedRemoteTestRuns = new List<RemoteManualTestRun>();

				//First populate the pending run
				UpdateFunctions.UpdatePendingTestRun(testRunsPending, remoteTestRuns, projectId, userId.Value);

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
					UpdateFunctions.UpdateManualTestRunData(testRunsPending, remoteTestRun);

					//Update the status of the test run
					testRunManager.UpdateExecutionStatus(projectId, userId.Value, testRunsPending, i, endDate, false);
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
					Dictionary<string, string> validationMessages = UpdateFunctions.UpdateCustomPropertyData(ref artifactCustomProperty, remoteTestRun, projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, remoteTestRun.TestRunId.Value, projectTemplateId);
					if (validationMessages != null && validationMessages.Count > 0)
					{
						//Throw a validation exception
						throw CreateValidationException(validationMessages);
					}

					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId.Value);

					//Finally we need to return the fully populated test run (after saving), it's easiest just to
					//regenerate from the test run dataset
					RemoteManualTestRun updatedRemoteTestRun = new RemoteManualTestRun();
					PopulationFunctions.PopulateManualTestRun(updatedRemoteTestRun, testRunsPending.TestRuns[i], projectId);
					updatedRemoteTestRuns.Add(updatedRemoteTestRun);
				}

				//Also need to complete the pending run so that it is no longer displayed
				testRunManager.CompletePending(testRunsPendingId, userId.Value);

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

		/// <summary>
		/// Retrieves a single test run in the system. Only returns the generic information
		/// that is applicable for both automated and manual tests. Consider using
		/// TestRun_RetrieveAutomatedById or TestRun_RetrieveManualById if you need
		/// the automation/manual specific data for the test run
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_run_id">The id of the test run</param>
		/// <returns>Test Run object</returns>
		public RemoteTestRun TestRun_RetrieveById(string project_id, string test_run_id)
		{
			const string METHOD_NAME = "TestRun_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testRunId = RestUtils.ConvertToInt32(test_run_id, "test_run_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//If the test run was not found, just return null
			try
			{
				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulationFunctions.PopulateTestRun(remoteTestRun, testRun, projectId);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test run");
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestRunDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves a single automated test run in the system including the automation-specific information
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_run_id">The id of the test run</param>
		/// <returns>Test Run object</returns>
		public RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(string project_id, string test_run_id)
		{
			const string METHOD_NAME = "TestRun_RetrieveAutomatedById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testRunId = RestUtils.ConvertToInt32(test_run_id, "test_run_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//If the test run was not found, just return null
			try
			{
				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
				PopulationFunctions.PopulateAutomatedTestRun(remoteTestRun, testRun, projectId);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test run");
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestRunDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves a single manual test run in the system including any associated steps
		/// </summary>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="test_run_id">The id of the test run</param>
		/// <returns>Test Run object</returns>
		public RemoteManualTestRun TestRun_RetrieveManualById(string project_id, string test_run_id)
		{
			const string METHOD_NAME = "TestRun_RetrieveManualById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int testRunId = RestUtils.ConvertToInt32(test_run_id, "test_run_id");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Call the business object to actually retrieve the test run dataset
			TestRunManager testRunManager = new TestRunManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//If the test run was not found, just return null
			try
			{
				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//Populate the API data object and return
				RemoteManualTestRun remoteTestRun = new RemoteManualTestRun();
				PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRun, projectId);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, artifactCustomProperty);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return remoteTestRun;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate requested test run");
				Logger.Flush();
				throw new WebFaultException<string>(Resources.Messages.TestRunDetails_ArtifactNotExists, System.Net.HttpStatusCode.NotFound);
			}
		}

		/// <summary>
		/// Retrieves a list of test runs in the project
		/// </summary>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		public List<RemoteTestRun> TestRun_Retrieve1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction)
		{
			return this.TestRun_Retrieve2(project_id, starting_row, number_of_rows, sort_field, sort_direction, null);
		}

		/// <summary>
		/// Retrieves a list of test runs in the system that match the provided filter/sort
		/// </summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <param name="project_id">The id of the current project</param>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		public List<RemoteTestRun> TestRun_Retrieve2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = "TestRun_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");
			string sortBy = (String.IsNullOrEmpty(sort_field) ? "EndDate" : sort_field.Trim());
			bool sortAscending = (sort_direction != null && sort_direction.ToLowerInvariant() != "desc");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the testRun dataset
			//Doesn't include the test run steps
			TestRunManager testRunManager = new TestRunManager();
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, sortBy, sortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

			//Populate the API data object and return
			List<RemoteTestRun> remoteTestRuns = new List<RemoteTestRun>();
			foreach (TestRunView testRun in testRuns)
			{
				//Create and populate the row
				RemoteTestRun remoteTestRun = new RemoteTestRun();
				PopulationFunctions.PopulateTestRun(remoteTestRun, testRun, projectId);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRun, customProperties);
				remoteTestRuns.Add(remoteTestRun);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return remoteTestRuns;
		}

		/// <summary>Retrieves a list of manual test runs in the project</summary>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Does include the test run steps</remarks>
		public List<RemoteManualTestRun> TestRun_RetrieveManual1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction)
		{
			return TestRun_RetrieveManual2(project_id, starting_row, number_of_rows, sort_field, sort_direction, null);
		}

		/// <summary>Retrieves a list of manual test runs in the system that match the provided filter/sort</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Does include the test run steps</remarks>
		public List<RemoteManualTestRun> TestRun_RetrieveManual2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_RetrieveManual";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");
			string sortBy = (String.IsNullOrEmpty(sort_field) ? "EndDate" : sort_field.Trim());
			bool sortAscending = (sort_direction != null && sort_direction.ToLowerInvariant() != "desc");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object and add the automated filter.
			if (remoteFilters == null) remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { IntValue = (int)TestRun.TestRunTypeEnum.Manual, PropertyName = "TestRunTypeId" });
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Create the manager..
			TestRunManager testRunManager = new TestRunManager();
			//Call the business object to actually retrieve the testRun dataset
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, sortBy, sortAscending, startingRow, numberOfRows, filters, 0);

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
					PopulationFunctions.PopulateManualTestRun(remoteTestRun, testRunWithSteps, projectId);
					PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRunWithSteps, customProperties);
					remoteTestRuns.Add(remoteTestRun);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return remoteTestRuns;
		}

		/// <summary>Retrieves a list of automated test runs in the project</summary>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		public List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction)
		{
			return this.TestRun_RetrieveAutomated2(project_id, starting_row, number_of_rows, sort_field, sort_direction, null);
		}

		/// <summary>Retrieves a list of automated test runs in the system that match the provided filter/sort</summary>
		/// <param name="remoteFilters">The list of filters to apply</param>
		/// <param name="sort_direction">The direction of the sorting (asc|desc)</param>
		/// <param name="sort_field">The field we want to sort on</param>
		/// <param name="number_of_rows">The number of rows to return</param>
		/// <param name="starting_row">The first row to return (starting with 1)</param>
		/// <returns>List of test runs</returns>
		/// <remarks>Doesn't include the test run steps</remarks>
		public List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters)
		{
			const string METHOD_NAME = CLASS_NAME + "TestRun_RetrieveAutomated";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}

			//Convert the parameters into their native types
			int projectId = RestUtils.ConvertToInt32(project_id, "project_id");
			int startingRow = RestUtils.ConvertToInt32(starting_row, "starting_row");
			int numberOfRows = RestUtils.ConvertToInt32(number_of_rows, "number_of_rows");
			string sortBy = (String.IsNullOrEmpty(sort_field) ? "EndDate" : sort_field.Trim());
			bool sortAscending = (sort_direction != null && sort_direction.ToLowerInvariant() != "desc");

			//Make sure we're authorized
			Business.ProjectManager projectManager = new Business.ProjectManager();
			List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(userId.Value);
			if (!authProjects.Any(p => p.ProjectId == projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}

			//Make sure we have permissions to view test runs
			if (!IsAuthorized(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(Resources.Messages.Services_NotAuthorizedViewTestRuns, System.Net.HttpStatusCode.Unauthorized);
			}

			//Extract the filters from the provided API object and add the automated filter.
			if (remoteFilters == null) remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { IntValue = (int)TestRun.TestRunTypeEnum.Automated, PropertyName = "TestRunTypeId" });
			Hashtable filters = PopulationFunctions.PopulateFilters(remoteFilters);

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Call the business object to actually retrieve the testRun dataset
			List<TestRunView> testRuns = new TestRunManager().Retrieve(projectId, sortBy, sortAscending, startingRow, numberOfRows, filters, 0);

			//Get the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);

			//Populate the API data object and return
			List<RemoteAutomatedTestRun> remoteTestRuns = new List<RemoteAutomatedTestRun>();
			foreach (TestRunView testRun in testRuns)
			{
				//Create and populate the row
				RemoteAutomatedTestRun remoteTestRun = new RemoteAutomatedTestRun();
				PopulationFunctions.PopulateAutomatedTestRun(remoteTestRun, testRun, projectId);
				PopulationFunctions.PopulateCustomProperties(remoteTestRun, testRun, customProperties);
				remoteTestRuns.Add(remoteTestRun);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return remoteTestRuns;
		}

		#endregion
	}
}
