using System;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Security;
using System.Web.Mail;
using System.Configuration;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class handles the storage and retrieval of mapping information between SpiraTest and other systems
	/// </summary>
	public class DataMappingManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.DataMappingManager::";

		public const string FIELD_PREPEND = "DataMapping_";

		#region DataSync System functions

		/// <summary>
		/// Deletes a data-sync plug-in together with all its mapping information
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data sync system</param>
		public void DeleteDataSyncSystem(int dataSyncSystemId, int? userId = null)
		{
			const string METHOD_NAME = "DeleteDataSyncSystem";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the data sync system and any dependent data we want to delete
					var query = from d in context.DataSyncSystems
									.Include("ProjectMappings")
									.Include("ProjectMappings.ArtifactMappings")
									.Include("ProjectMappings.ArtifactFieldValueMappings")
									.Include("ProjectMappings.CustomPropertyValueMappings")
									.Include("ProjectMappings.CustomPropertyMappings")
									.Include("UserMappings")
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					DataSyncSystem dataSyncSystem = query.FirstOrDefault();
					if (dataSyncSystem == null)
					{
						//Log warning since the plug-in has been deleted but don't rethrow error
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The plug-in being deleted has already been deleted from the system");
					}
					else
					{
						context.DataSyncSystems.DeleteObject(dataSyncSystem);
						context.SaveChanges();

						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "Data Synchronization";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)userId, dataSyncSystem.DataSyncSystemId, dataSyncSystem.Name, adminSectionId, "DataSynchronization Deleted", DateTime.UtcNow, ArtifactTypeEnum.DataSync, "DataSyncSystemId");
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the last run date/time and status for a successful data-sync
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data-sync plug-in</param>
		/// <param name="lastRunDate">The date/time that the data-sync just ran</param>
		public void SaveRunSuccess(int dataSyncSystemId, DateTime lastRunDate)
		{
			const string METHOD_NAME = "SaveRunSuccess";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to see if an entry already exists for the data-sync in question
					var query = from d in context.DataSyncSystems
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					DataSyncSystem dataSyncSystem = query.FirstOrDefault();
					if (dataSyncSystem == null)
					{
						//Log warning since the plug-in has been deleted but don't rethrow error
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The plug-in being updated has been deleted from the system");
					}
					else
					{
						dataSyncSystem.StartTracking();
						dataSyncSystem.DataSyncStatusId = (int)DataSyncSystem.DataSyncStatusEnum.Success;
						dataSyncSystem.LastSyncDate = lastRunDate;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the status for a failed data-sync
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data-sync plug-in</param>
		public void SaveRunFailure(int dataSyncSystemId)
		{
			const string METHOD_NAME = "SaveRunFailure";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to see if an entry already exists for the data-sync in question
					var query = from d in context.DataSyncSystems
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					DataSyncSystem dataSyncSystem = query.FirstOrDefault();
					if (dataSyncSystem == null)
					{
						//Log warning since the plug-in has been deleted but don't rethrow error
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The plug-in being updated has been deleted from the system");
					}
					else
					{
						dataSyncSystem.StartTracking();
						dataSyncSystem.DataSyncStatusId = (int)DataSyncSystem.DataSyncStatusEnum.Failure;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the last run date/time and status for a data-sync that ran with warnings
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data-sync plug-in</param>
		/// <param name="lastRunDate">The date/time that the data-sync just ran</param>
		public void SaveRunWarning(int dataSyncSystemId, DateTime lastRunDate)
		{
			const string METHOD_NAME = "SaveRunWarning";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to see if an entry already exists for the data-sync in question
					var query = from d in context.DataSyncSystems
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					DataSyncSystem dataSyncSystem = query.FirstOrDefault();
					if (dataSyncSystem == null)
					{
						//Log warning since the plug-in has been deleted but don't rethrow error
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The plug-in being updated has been deleted from the system");
					}
					else
					{
						dataSyncSystem.StartTracking();
						dataSyncSystem.DataSyncStatusId = (int)DataSyncSystem.DataSyncStatusEnum.Warning;
						dataSyncSystem.LastSyncDate = lastRunDate;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Resets a data-sync system back to not-run status
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data-sync plug-in</param>
		public void ResetLastRunInfo(int dataSyncSystemId)
		{
			const string METHOD_NAME = "ResetLastRunInfo";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncSystems
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					DataSyncSystem dataSyncSystem = query.FirstOrDefault();
					if (dataSyncSystem == null)
					{
						//Log warning since the plug-in has been deleted but don't rethrow error
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The plug-in being updated has been deleted from the system");
					}
					else
					{
						//We need to simply update the status of the existing record - and reset the date
						dataSyncSystem.StartTracking();
						dataSyncSystem.DataSyncStatusId = (int)DataSyncSystem.DataSyncStatusEnum.NotRun;
						dataSyncSystem.LastSyncDate = null;

						//Save changes
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Retrieves a dataset containing the list of active data sync systems configured for a specific project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>Datasync</returns>
		public List<DataSyncSystem> RetrieveDataSyncSystemsForProject(int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncSystemsForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncSystem> dataSyncSystems = new List<DataSyncSystem>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.DataSyncProjects
									.Include(p => p.System)
									.Include(p => p.System.Status)
								where p.ProjectId == projectId && p.ActiveYn == "Y"
								orderby p.DataSyncSystemId
								select p;

					List<DataSyncProject> dataSyncProjects = query.ToList();

					//Next convert into the list of data sync projects
					foreach (DataSyncProject dataSyncProject in dataSyncProjects)
					{
						dataSyncSystems.Add(dataSyncProject.System);
					}

				}

				//Resort by name
				dataSyncSystems = dataSyncSystems.OrderBy(d => d.Name).ToList();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncSystems;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single data-sync system record by its physical name (e.g. JiraDataSync)
		/// </summary>
		/// <param name="name">The name of the system</param>
		/// <returns>Datasync system or NULL if no match</returns>
		public DataSyncSystem RetrieveDataSyncSystemByName(string name)
		{
			const string METHOD_NAME = "RetrieveDataSyncSystemById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataSyncSystem dataSyncSystem;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncSystems
									.Include(d => d.Status)
								where d.Name == name
								select d;

					dataSyncSystem = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncSystem;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a data sync system by its id
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the data-sync plugin</param>
		/// <returns>Datasyncs</returns>
		public DataSyncSystem RetrieveDataSyncSystemById(int dataSyncSystemId)
		{
			const string METHOD_NAME = "RetrieveDataSyncSystemById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataSyncSystem dataSyncSystem;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncSystems
									.Include(d => d.Status)
								where d.DataSyncSystemId == dataSyncSystemId
								select d;

					dataSyncSystem = query.FirstOrDefault();
				}

				//Make sure we have a record
				if (dataSyncSystem == null)
				{
					throw new ArtifactNotExistsException("The datasync no longer exists in the system");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncSystem;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new data sync plug-in entry into the system
		/// </summary>
		/// <param name="name">The name of the plug-in</param>
		/// <param name="caption">The friendly caption/name of the plug-in</param>
		/// <param name="description">The description of the plug-in</param>
		/// <param name="connection">The connection information (usually a URL)</param>
		/// <param name="login">The login to the external system</param>
		/// <param name="password">The password for the external system</param>
		/// <param name="timeOffsetHours">Any time zone offset to apply</param>
		/// <param name="autoMapUsers">Do we want to auto-map users</param>
		/// <param name="custom01">Plug-in custom data</param>
		/// <param name="custom02">Plug-in custom data</param>
		/// <param name="custom03">Plug-in custom data</param>
		/// <param name="custom04">Plug-in custom data</param>
		/// <param name="custom05">Plug-in custom data</param>
		/// <param name="isActive">Is the data sync system active (default: True)</param>
		/// <returns>The id of the new plug-in entry</returns>
		public int InsertDataSyncSystem(string name, string caption, string description, string connection, string login, string password, int timeOffsetHours, bool autoMapUsers, string custom01, string custom02, string custom03, string custom04, string custom05, bool isActive = true, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "InsertDataSyncSystem";

			string newValue = "";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int dataSyncSystemId;

				AdminAuditManager adminAuditManager = new AdminAuditManager();

				//Convert any flags
				string autoMapUsersYn = (autoMapUsers) ? "Y" : "N";

				//Fill out dataset with data for new data sync system
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					DataSyncSystem dataSyncSystem = new DataSyncSystem();
					dataSyncSystem.DataSyncStatusId = (int)DataSyncSystem.DataSyncStatusEnum.NotRun;
					dataSyncSystem.Name = name;
					dataSyncSystem.Caption = caption;
					dataSyncSystem.Description = description;
					dataSyncSystem.ConnectionString = connection;
					dataSyncSystem.ExternalLogin = login;
					dataSyncSystem.ExternalPassword = password;
					dataSyncSystem.TimeOffsetHours = timeOffsetHours;
					dataSyncSystem.Custom01 = custom01;
					dataSyncSystem.Custom02 = custom02;
					dataSyncSystem.Custom03 = custom03;
					dataSyncSystem.Custom04 = custom04;
					dataSyncSystem.Custom05 = custom05;
					dataSyncSystem.AutoMapUsersYn = autoMapUsersYn;
					dataSyncSystem.IsActive = isActive;

					//By default, plug-ins are not run
					dataSyncSystem.LastSyncDate = null;

					//Save the object
					context.DataSyncSystems.AddObject(dataSyncSystem);
					context.SaveChanges();
					dataSyncSystemId = dataSyncSystem.DataSyncSystemId;
					newValue = dataSyncSystem.Name;
				}

				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				details.NEW_VALUE = newValue;

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), dataSyncSystemId, action, details, DateTime.UtcNow, ArtifactTypeEnum.DataSync, "DataSyncSystemId");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncSystemId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a data-sync system record
		/// </summary>
		/// <param name="dataSyncSystem">The data to be persisted</param>
		public void UpdateDataSyncSystem(DataSyncSystem dataSyncSystem, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "UpdateDataSyncSystem";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Attach the object to the context and save changes
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.DataSyncSystems.ApplyChanges(dataSyncSystem);
					
					context.AdminSaveChanges(userId, dataSyncSystem.DataSyncSystemId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of existing data-sync plug-ins (default = active only)
		/// </summary>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <returns>Datasyncs</returns>
		public List<DataSyncSystem> RetrieveDataSyncSystems(bool activeOnly = true)
		{
			const string METHOD_NAME = "RetrieveDataSyncSystems";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncSystem> dataSyncSystems;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncSystems
									.Include(d => d.Status)
								where (d.IsActive || !activeOnly)
								orderby d.Name, d.DataSyncSystemId
								select d;

					dataSyncSystems = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncSystems;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region User Mapping functions

		/// <summary>
		/// Retrieves a dataset containing all the data-mappings for a specific user
		/// </summary>
		/// <param name="userId">The user we're interested in</param>
		/// <returns>Datasync dataset of mappings</returns>
		/// <remarks>Returns unmapped system records as well</remarks>
		/// <seealso cref="RetrieveDataSyncUserMappingsForUser"/>
		public List<DataSyncUserMappingView> RetrieveDataSyncUserMappings(int userId)
		{
			const string METHOD_NAME = "RetrieveDataSyncUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Call the procedure to get the data
				List<DataSyncUserMappingView> userMappings;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					userMappings = context.DataSync_RetrieveUserMappings(userId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return userMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list containing all the existing data-mappings for a user
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>Datasync dataset of mappings</returns>
		/// <remarks>Only returns mapped records</remarks>
		/// <seealso cref="RetrieveDataSyncUserMappings"/>
		public List<DataSyncUserMapping> RetrieveDataSyncUserMappingsForUser(int userId)
		{
			const string METHOD_NAME = "RetrieveDataSyncUserMappingsForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncUserMapping> userMappings;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the appropriate select command for retrieving the data mapping records
					var query = from d in context.DataSyncUserMappings
									.Include(d => d.System)
								where d.UserId == userId
								orderby d.DataSyncSystemId
								select d;

					userMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return userMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list containing all the data-mappings for a plug-in
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>Datasync dataset of mappings</returns>
		/// <remarks>Only returns mapped records</remarks>
		public List<DataSyncUserMapping> RetrieveDataSyncUserMappingsForSystem(int dataSyncSystemId)
		{
			const string METHOD_NAME = "RetrieveDataSyncUserMappingsForSystem";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncUserMapping> userMappings;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the appropriate select command for retrieving the data mapping records
					var query = from d in context.DataSyncUserMappings
									.Include(d => d.User)
									.Include(d => d.User.Profile)
								where d.DataSyncSystemId == dataSyncSystemId
								orderby d.UserId
								select d;

					userMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return userMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts/Updates/Deletes a set of data-sync user mapping records
		/// </summary>
		/// <param name="userMappings">The data to be persisted</param>
		public void SaveDataSyncUserMappings(List<DataSyncUserMapping> userMappings)
		{
			const string METHOD_NAME = "SaveDataSyncUserMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to go through the list and remove any items that have their external keys set to null
					//And make any ones that have their external keys set for the first time as inserts
					foreach (DataSyncUserMapping userMapping in userMappings)
					{
						//Attach to the context
						context.DataSyncUserMappings.ApplyChanges(userMapping);

						//Make sure we have an original row (i.e. not already marked as added)
						if (userMapping.ChangeTracker.State != ObjectState.Added)
						{
							if (String.IsNullOrEmpty(userMapping.ExternalKey))
							{
								if (userMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)userMapping.ChangeTracker.OriginalValues["ExternalKey"]))
								{
									//No action should be taken
									userMapping.AcceptChanges();
								}
								else
								{
									context.DataSyncUserMappings.DeleteObject(userMapping);
								}
							}
							else
							{
								if (userMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)userMapping.ChangeTracker.OriginalValues["ExternalKey"]))
								{
									userMapping.AcceptChanges();
									userMapping.MarkAsAdded();
								}
							}
						}
					}

					//Save the changes
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Project Mapping functions

		/// <summary>
		/// Deletes all the data-sync plug mappings for a specific project (for all plugins)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <remarks>
		/// 1. This is called only when a project is moving templates and the mapping data would be invalid
		/// 2. It leaves the project entry and the artifact mappings since they are not dependent on template
		/// </remarks>
		protected internal void DeleteDataSyncProjectMappings(int projectId)
		{
			const string METHOD_NAME = "DeleteDataSyncProjectMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the data sync system and any dependent data we want to delete
					var query = from d in context.DataSyncProjects
									.Include(d => d.ArtifactFieldValueMappings)
									.Include(d => d.CustomPropertyValueMappings)
									.Include(d => d.CustomPropertyMappings)
								where d.ProjectId == projectId
								select d;

					//Get the list of mappings to delete
					List<DataSyncProject> dataSyncProjects = query.ToList();
					List<DataSyncArtifactFieldValueMapping> fieldValueMappings = new List<DataSyncArtifactFieldValueMapping>();
					List<DataSyncCustomPropertyMapping> customPropertyMappings = new List<DataSyncCustomPropertyMapping>();
					List<DataSyncCustomPropertyValueMapping> customPropertyValueMappings = new List<DataSyncCustomPropertyValueMapping>();
					foreach (DataSyncProject dataSyncProject in dataSyncProjects)
					{
						fieldValueMappings.AddRange(dataSyncProject.ArtifactFieldValueMappings);
						customPropertyMappings.AddRange(dataSyncProject.CustomPropertyMappings);
						customPropertyValueMappings.AddRange(dataSyncProject.CustomPropertyValueMappings);
					}

					//Now do the deletes
					foreach (DataSyncArtifactFieldValueMapping mapping in fieldValueMappings)
					{
						context.DataSyncArtifactFieldValueMappings.DeleteObject(mapping);
					}
					foreach (DataSyncCustomPropertyMapping mapping in customPropertyMappings)
					{
						context.DataSyncCustomPropertyMappings.DeleteObject(mapping);
					}
					foreach (DataSyncCustomPropertyValueMapping mapping in customPropertyValueMappings)
					{
						context.DataSyncCustomPropertyValueMappings.DeleteObject(mapping);
					}
					context.SaveChanges();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Inserts a new data sync plug-in project record into the system
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="externalKey">The id of the project in the external system</param>
		/// <param name="active">Is the data-sync active for the project</param>
		public void InsertDataSyncProject(int dataSyncSystemId, int projectId, string externalKey, bool active)
		{
			const string METHOD_NAME = "InsertDataSyncProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the new entry
					DataSyncProject dataSyncProject = new DataSyncProject();
					dataSyncProject.ActiveYn = (active) ? "Y" : "N";
					dataSyncProject.DataSyncSystemId = dataSyncSystemId;
					dataSyncProject.ProjectId = projectId;
					dataSyncProject.ExternalKey = externalKey;

					//Add the object
					context.DataSyncProjects.AddObject(dataSyncProject);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects with their project mappings
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>Datasync project list</returns>
		/// <remarks>Only returns projects marked as active for data-sync</remarks>
		public List<DataSyncProject> RetrieveDataSyncProjects(int dataSyncSystemId)
		{
			const string METHOD_NAME = "RetrieveDataSyncProjects";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncProject> dataSyncProjects;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project data sync record
					//The project has to be active as well as the project mapping needs to be active
					var query = from d in context.DataSyncProjects
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ActiveYn == "Y" &&
									d.Project.IsActive
								orderby d.ProjectId
								select d;

					dataSyncProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing the information about the current data sync system and project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <returns>Datasync project list</returns>
		public List<DataSyncProject> RetrieveDataSyncProjects(int dataSyncSystemId, int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncProjects";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncProject> dataSyncProjects;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project data sync record
					//The project has to be active as well as the project mapping needs to be active
					var query = from d in context.DataSyncProjects
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId
								orderby d.ExternalKey, d.DataSyncSystemId, d.ProjectId
								select d;

					dataSyncProjects = query.ToList();
				}

				//Make sure a row is found, calling code expects an exception to be thrown
				if (dataSyncProjects.Count == 0)
				{
					throw new DataSyncNotConfiguredException(String.Format("Data synchronization is not enabled for project PR{0} and data sync system {1}", projectId, dataSyncSystemId));
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncProjects;
			}
			catch (DataSyncNotConfiguredException exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing the information about the current data sync system and project
		/// </summary>
		/// <returns>Datasync Project</returns>
		public DataSyncProject RetrieveDataSyncProject(int dataSyncSystemId, int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataSyncProject dataSyncProject;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project data sync record
					var query = from d in context.DataSyncProjects
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId
								select d;

					//Execute the query
					dataSyncProject = query.FirstOrDefault();
				}

				//Make sure a row is found
				if (dataSyncProject == null)
				{
					throw new DataSyncNotConfiguredException(String.Format("Data synchronization is not enabled for project PR{0} and data sync system {1}", projectId, dataSyncSystemId));
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSyncProject;
			}
			catch (DataSyncNotConfiguredException exception)
			{
				//Log as a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a data-sync project record
		/// </summary>
		/// <param name="dataSyncProjects">The data to be persisted</param>
		public void UpdateDataSyncProject(DataSyncProject dataSyncProject)
		{
			const string METHOD_NAME = "UpdateDataSyncProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Attach the object and apply changes
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.DataSyncProjects.ApplyChanges(dataSyncProject);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Copies across all the project mapping information from an existing project to a new project
		/// </summary>
		/// <param name="createNewTemplate">Are we creating a new project template (or using an existing)</param>
		/// <param name="sourceProjectId">The id of the project containing the mappings to be copied</param>
		/// <param name="destProjectId">The id of the new project that does NOT contain any data mappings</param>
		/// <param name="incidentPriorityMapping">The mapping of source project to destination project incident priorities</param>
		/// <param name="incidentSeverityMapping">The mapping of source project to destination project incident severities</param>
		/// <param name="incidentStatusMapping">The mapping of source project to destination project incident statuses</param>
		/// <param name="incidentTypeMapping">The mapping of source project to destination project incident types</param>
		/// <param name="propertyValueMapping">The mapping of source project to destination project custom property values</param>
		/// <param name="customPropertyIdMapping">The mapping of source project to destination project custom property ids</param>
		public void CopyProjectMappings(int sourceProjectId, int destProjectId, bool createNewTemplate, Dictionary<int, int> incidentSeverityMapping, Dictionary<int, int> incidentPriorityMapping, Dictionary<int, int> incidentStatusMapping, Dictionary<int, int> incidentTypeMapping, Dictionary<int, int> requirementImportanceMapping, Dictionary<int, int> requirementTypeMapping, Dictionary<int, int> taskPriorityMapping, Dictionary<int, int> taskTypeMapping, Dictionary<int, int> testCasePriorityMapping, Dictionary<int, int> testCaseTypeMapping, Dictionary<int, int> propertyValueMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			const string METHOD_NAME = "CopyProjectMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to iterate through all the data-sync plug-ins
				List<DataSyncSystem> dataSyncSystems = this.RetrieveDataSyncSystems();
				foreach (DataSyncSystem dataSyncSystem in dataSyncSystems)
				{
					int dataSyncSystemId = dataSyncSystem.DataSyncSystemId;

					//See if this project is mapped for that plug-in
					try
					{
						DataSyncProject projectDataMapping = this.RetrieveDataSyncProject(dataSyncSystemId, sourceProjectId);

						//Now add a new project mapping record, always set to inactive to avoid
						//data being synced across before the admin has had a chance to verify configuration
						this.InsertDataSyncProject(dataSyncSystemId, destProjectId, projectDataMapping.ExternalKey, false);

						//Now copy the field value mappings
						List<DataSyncArtifactFieldValueMapping> fieldValueMappings = this.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, sourceProjectId);

						//Iterate through the dataset, changing the project id and the artifact value to match the new project
						//If we are not creating a new template, the IDs will be the same for both projects and the
						//mapping dictionaries are not used
						List<DataSyncArtifactFieldValueMapping> newDataMappings = new List<DataSyncArtifactFieldValueMapping>();
						foreach (DataSyncArtifactFieldValueMapping mappingRow in fieldValueMappings)
						{
							bool insertRow = true;
							DataSyncArtifactFieldValueMapping newMappingRow = new DataSyncArtifactFieldValueMapping();
							newMappingRow.DataSyncSystemId = mappingRow.DataSyncSystemId;
							newMappingRow.ProjectId = destProjectId;
							newMappingRow.ArtifactFieldId = mappingRow.ArtifactFieldId;
							newMappingRow.ArtifactFieldValue = mappingRow.ArtifactFieldValue;
							newMappingRow.PrimaryYn = mappingRow.PrimaryYn;
							newMappingRow.ExternalKey = mappingRow.ExternalKey;

							//Handle the cases where the internal ids vary by project
							if (createNewTemplate)
							{
								switch (mappingRow.ArtifactFieldId)
								{
									case 1:
										{
											//Incident Severity
											if (incidentSeverityMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = incidentSeverityMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 2:
										{
											//Incident Priority
											if (incidentPriorityMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = incidentPriorityMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 3:
										{
											//Incident Status
											if (incidentStatusMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = incidentStatusMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 4:
										{
											//Incident Type
											if (incidentTypeMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = incidentTypeMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 18:
										{
											//Requirement Importance
											if (requirementImportanceMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = requirementImportanceMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 140:
										{
											//Requirement Type
											if (requirementTypeMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = requirementTypeMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 59:
										{
											//Task Priority
											if (taskPriorityMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = taskPriorityMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 145:
										{
											//Task Type
											if (taskTypeMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = taskTypeMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 24:
										{
											//Test Case Priority
											if (testCasePriorityMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = testCasePriorityMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

									case 167:
										{
											//Test Case Type
											if (testCaseTypeMapping.ContainsKey(mappingRow.ArtifactFieldValue))
											{
												newMappingRow.ArtifactFieldValue = testCaseTypeMapping[mappingRow.ArtifactFieldValue];
											}
											else
											{
												//No match so ignore
												insertRow = false;
											}
										}
										break;

								}
							}
							if (insertRow)
							{
								newDataMappings.Add(newMappingRow);
							}
						}
						this.SaveDataSyncFieldValueMappings(newDataMappings);

						//Now copy the custom property mappings, we need to get the custom property definitions for both the source
						//and destination projects if we are creating a new template
						List<DataSyncCustomPropertyMapping> customPropertyMappings = this.RetrieveDataSyncCustomPropertyMappings(dataSyncSystemId, sourceProjectId);
						foreach (DataSyncCustomPropertyMapping mappingRow in customPropertyMappings)
						{
							if (createNewTemplate)
							{
								if (customPropertyIdMapping.ContainsKey(mappingRow.CustomPropertyId))
								{
									//Modify the data to insert into the new project
									mappingRow.CustomPropertyId = customPropertyIdMapping[mappingRow.CustomPropertyId];
									mappingRow.ProjectId = destProjectId;
									mappingRow.AcceptChanges();
									mappingRow.MarkAsAdded();
								}
								else
								{
									//The custom property no longer existed in the template project. Do not copy it over.
								}
							}
							else
							{
								//Modify the data to insert into the new project
								mappingRow.ProjectId = destProjectId;
								mappingRow.AcceptChanges();
								mappingRow.MarkAsAdded();
							}
						}
						this.SaveDataSyncCustomPropertyMappings(customPropertyMappings);

						//Now copy the custom property value mappings
						List<DataSyncCustomPropertyValueMapping> customPropertyValueMappings = this.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, sourceProjectId);
						foreach (DataSyncCustomPropertyValueMapping mappingRow in customPropertyValueMappings)
						{
							if (createNewTemplate)
							{
								//Modify the data to insert into the new project and convert the values
								if (propertyValueMapping.ContainsKey(mappingRow.CustomPropertyValueId))
								{
									mappingRow.CustomPropertyValueId = propertyValueMapping[mappingRow.CustomPropertyValueId];
									mappingRow.ProjectId = destProjectId;
									mappingRow.AcceptChanges();
									mappingRow.MarkAsAdded();
								}
								else
								{
									//The value was removed from the custom list and no longer exists in the project. Do not copy it over.
								}
							}
							else
							{
								//Modify the data to insert into the new project
								mappingRow.ProjectId = destProjectId;
								mappingRow.AcceptChanges();
								mappingRow.MarkAsAdded();
							}
						}
						this.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

						//Note: We don't copy across the artifact mappings (releases, incidents, tasks) since
						//these will be unique per project and per external-system project
					}
					catch (DataSyncNotConfiguredException)
					{
						//Just ignore and continue with the next data-sync
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Standard Field Mapping functions

		/// <summary>
		/// Retrieves a list of field values and their mappings
		/// </summary>
		/// <param name="artifactFieldId">The artifact field we want to retrieve mappings for</param>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <param name="includeUnmapped">Do we want to include unmapped entries or not</param>
		/// <returns>Datasync dataset of mappings</returns>
		public List<DataSyncFieldValueMappingView> RetrieveDataSyncFieldValueMappings(int dataSyncSystemId, int projectId, int artifactFieldId, bool includeUnmapped)
		{
			const string METHOD_NAME = "RetrieveDataSyncFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncFieldValueMappingView> fieldValueMappings;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					fieldValueMappings = context.DataSync_RetrieveFieldValueMappings(dataSyncSystemId, projectId, artifactFieldId, includeUnmapped).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return fieldValueMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all field value mappings in the project. It does not include any unmapped ones
		/// </summary>
		/// <param name="artifactFieldId">The id of the artifact field</param>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <returns>List of mappings</returns>
		public List<DataSyncArtifactFieldValueMapping> RetrieveDataSyncFieldValueMappings(int dataSyncSystemId, int projectId, int artifactFieldId)
		{
			const string METHOD_NAME = "RetrieveDataSyncFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncArtifactFieldValueMapping> fieldValueMappings;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncArtifactFieldValueMappings
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									d.ArtifactFieldId == artifactFieldId
								orderby d.ArtifactFieldId, d.ArtifactFieldValue
								select d;

					fieldValueMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return fieldValueMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all field value mappings in the project
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <returns>List of mappings</returns>
		protected List<DataSyncArtifactFieldValueMapping> RetrieveDataSyncFieldValueMappings(int dataSyncSystemId, int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncArtifactFieldValueMapping> fieldValueMappings;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncArtifactFieldValueMappings
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId
								orderby d.ArtifactFieldId, d.ArtifactFieldValue
								select d;

					fieldValueMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return fieldValueMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts/Updates/Deletes a set of data-sync field value mapping records
		/// </summary>
		/// <param name="newArtifactFieldValueMappings">The data to be persisted</param>
		public void SaveDataSyncFieldValueMappings(List<DataSyncArtifactFieldValueMapping> newArtifactFieldValueMappings, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "SaveDataSyncFieldValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to go through the dataset and remove any items that have their external keys set to null
					//And make any ones that have their external keys set for the first time as inserts
					//Also need to check for duplicate primary external keys and non-primary keys when no duplicates (not allowed)
					bool duplicatePrimaryExternalKeys = false;
					List<string> existingPrimaryExternalKeys = new List<string>();
					foreach (DataSyncArtifactFieldValueMapping newArtifactFieldValueMapping in newArtifactFieldValueMappings)
					{
						//Attach to the context
						context.DataSyncArtifactFieldValueMappings.ApplyChanges(newArtifactFieldValueMapping);

						if (String.IsNullOrEmpty(newArtifactFieldValueMapping.ExternalKey))
						{
							if (newArtifactFieldValueMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)newArtifactFieldValueMapping.ChangeTracker.OriginalValues["ExternalKey"]))
							{
								//No action should be taken
								newArtifactFieldValueMapping.AcceptChanges();
							}
							else
							{
								context.DataSyncArtifactFieldValueMappings.DeleteObject(newArtifactFieldValueMapping);
							}
						}
						else
						{
							if (newArtifactFieldValueMapping.ChangeTracker.State != ObjectState.Added)
							{
								if (newArtifactFieldValueMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)newArtifactFieldValueMapping.ChangeTracker.OriginalValues["ExternalKey"]))
								{
									newArtifactFieldValueMapping.AcceptChanges();
									newArtifactFieldValueMapping.MarkAsAdded();
								}
							}

							//Check for duplicate primaries
							if (newArtifactFieldValueMapping.PrimaryYn == "Y")
							{
								string compositeKey = newArtifactFieldValueMapping.ProjectId + "+" + newArtifactFieldValueMapping.ArtifactFieldId + "+" + newArtifactFieldValueMapping.ExternalKey;
								if (existingPrimaryExternalKeys.Contains(compositeKey))
								{
									duplicatePrimaryExternalKeys = true;
								}
								else
								{
									existingPrimaryExternalKeys.Add(compositeKey);
								}
							}
						}

						context.AdminSaveChanges(userId, newArtifactFieldValueMapping.DataSyncSystemId, null, adminSectionId, action, true, true, null);
					}
					if (duplicatePrimaryExternalKeys)
					{
						throw new DataSyncPrimaryExternalKeyException("You can't have multiple field values mapped to the same external key unless only one is marked as primary");
					}

					//Now check for rows where there are no primaries (needs to be separate loop so that existingPrimaryExternalKeys fully populated)
					foreach (DataSyncArtifactFieldValueMapping newArtifactFieldValueMapping in newArtifactFieldValueMappings)
					{
						if (newArtifactFieldValueMapping.ChangeTracker.State != ObjectState.Deleted)
						{
							if (!String.IsNullOrEmpty(newArtifactFieldValueMapping.ExternalKey) && newArtifactFieldValueMapping.PrimaryYn == "N")
							{
								string compositeKey = newArtifactFieldValueMapping.ProjectId + "+" + newArtifactFieldValueMapping.ArtifactFieldId + "+" + newArtifactFieldValueMapping.ExternalKey;
								if (!existingPrimaryExternalKeys.Contains(compositeKey))
								{
									throw new DataSyncPrimaryExternalKeyException("You must have at least one primary External Key for each mapped value");
								}
							}
						}
						context.AdminSaveChanges(userId, newArtifactFieldValueMapping.DataSyncSystemId, null, adminSectionId, action, true, true, null);
					}

					//Save the changes
					context.SaveChanges();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Custom Property Mapping functions

		/// <summary>
		/// Inserts/Updates/Deletes a set of data-sync custom property mapping records
		/// </summary>
		/// <param name="dataMappings">The data to be persisted</param>
		public void SaveDataSyncCustomPropertyMappings(List<DataSyncCustomPropertyMapping> dataMappings)
		{
			const string METHOD_NAME = "SaveDataSyncCustomPropertyMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					foreach (DataSyncCustomPropertyMapping dataMapping in dataMappings)
					{
						context.DataSyncCustomPropertyMappings.ApplyChanges(dataMapping);
					}

					//Save the changes
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts/Updates/Deletes a set of data-sync custom property value mapping records
		/// </summary>
		/// <param name="dataMappings">The data to be persisted</param>
		public void SaveDataSyncCustomPropertyValueMappings(List<DataSyncCustomPropertyValueMapping> dataMappings)
		{
			const string METHOD_NAME = "SaveDataSyncCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to go through the dataset and remove any items that have their external keys set to null
					//And make any ones that have their external keys set for the first time as inserts
					foreach (DataSyncCustomPropertyValueMapping dataMapping in dataMappings)
					{
						//Attach to the context
						context.DataSyncCustomPropertyValueMappings.ApplyChanges(dataMapping);

						if (String.IsNullOrEmpty(dataMapping.ExternalKey))
						{
							if (dataMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)dataMapping.ChangeTracker.OriginalValues["ExternalKey"]))
							{
								//No action should be taken
								dataMapping.AcceptChanges();
							}
							else
							{
								context.DataSyncCustomPropertyValueMappings.DeleteObject(dataMapping);
							}
						}
						else
						{
							if (dataMapping.ChangeTracker.State != ObjectState.Added)
							{
								if (dataMapping.ChangeTracker.OriginalValues.ContainsKey("ExternalKey") && String.IsNullOrEmpty((string)dataMapping.ChangeTracker.OriginalValues["ExternalKey"]))
								{
									dataMapping.AcceptChanges();
									dataMapping.MarkAsAdded();
								}
							}
						}
					}

					//Save the changes
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves a dataset containing all mappings for a project and plug-in
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <returns>Datasync dataset of mappings</returns>
		public List<DataSyncCustomPropertyMapping> RetrieveDataSyncCustomPropertyMappings(int dataSyncSystemId, int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncCustomPropertyMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncCustomPropertyMapping> customPropertyMappings;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the appropriate select command for retrieving the data mapping records
					var query = from d in context.DataSyncCustomPropertyMappings
									.Include(d => d.CustomProperty)
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									d.CustomProperty.ArtifactType.IsDataSync
								orderby d.DataSyncSystemId, d.ProjectId, d.CustomPropertyId
								select d;

					customPropertyMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return customPropertyMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing any mappings for a specific custom property for a project and plug-in
		/// </summary>
		/// <param name="artifactType">The type of artifact we want to retrieve mappings for</param>
		/// <param name="customPropertyId">The custom property we want to retrieve mappings for</param>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <returns>The matched mapping</returns>
		public DataSyncCustomPropertyMapping RetrieveDataSyncCustomPropertyMapping(int dataSyncSystemId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int customPropertyId)
		{
			const string METHOD_NAME = "RetrieveDataSyncCustomPropertyMapping";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataSyncCustomPropertyMapping customPropertyMapping;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncCustomPropertyMappings
									.Include(d => d.CustomProperty)
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									d.CustomPropertyId == customPropertyId &&
									d.CustomProperty.ArtifactTypeId == (int)artifactType
								select d;

					customPropertyMapping = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return customPropertyMapping;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing the list of custom property values and their mappings
		/// </summary>
		/// <param name="artifactType">The type of artifact we want to retrieve mappings for</param>
		/// <param name="customPropertyId">The custom property we want to retrieve mappings for</param>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The current project</param>
		/// <param name="includeUnmapped">Do we want to include unmapped entries or not</param>
		/// <returns>List of mappings</returns>
		public List<DataSyncCustomPropertyValueMappingView> RetrieveDataSyncCustomPropertyValueMappings(int dataSyncSystemId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int customPropertyId, bool includeUnmapped)
		{
			const string METHOD_NAME = "RetrieveDataSyncCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncCustomPropertyValueMappingView> customPropertyValueMappings = new List<DataSyncCustomPropertyValueMappingView>();

				//First we need to get the custom property list used by the specified custom property
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId);
				if (customProperty != null)
				{
					if (customProperty.CustomPropertyListId.HasValue)
					{
						int customPropertyListId = customProperty.CustomPropertyListId.Value;

						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							customPropertyValueMappings = context.DataSync_RetrieveCustomPropertyValueMappings(dataSyncSystemId, projectId, customPropertyListId, includeUnmapped).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return customPropertyValueMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing the list of custom property values and their mappings
		/// </summary>
		/// <param name="artifactType">The type of artifact we want to retrieve mappings for</param>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <returns>Datasync dataset of mappings</returns>
		public List<DataSyncCustomPropertyValueMapping> RetrieveDataSyncCustomPropertyValueMappings(int dataSyncSystemId, int projectId)
		{
			const string METHOD_NAME = "RetrieveDataSyncCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncCustomPropertyValueMapping> customPropertyMappingValues;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from d in context.DataSyncCustomPropertyValueMappings
									.Include(d => d.CustomPropertyValue)
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									!d.CustomPropertyValue.IsDeleted
								orderby d.DataSyncSystemId, d.ProjectId, d.CustomPropertyValueId
								select d;
					customPropertyMappingValues = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return customPropertyMappingValues;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset containing the list of custom property values and their mappings
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in we're interested in</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="customPropertyId">The id of the custom property</param>
		/// <returns>Datasync dataset of mappings</returns>
		public List<DataSyncCustomPropertyValueMapping> RetrieveDataSyncCustomPropertyValueMappings(int dataSyncSystemId, int projectId, int customPropertyId)
		{
			const string METHOD_NAME = "RetrieveDataSyncCustomPropertyValueMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncCustomPropertyValueMapping> customPropertyMappingValues = new List<DataSyncCustomPropertyValueMapping>();

				//First we need to get the custom property list used by the specified custom property
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId);
				if (customProperty != null)
				{
					if (customProperty.CustomPropertyListId.HasValue)
					{
						int customPropertyListId = customProperty.CustomPropertyListId.Value;

						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							var query = from d in context.DataSyncCustomPropertyValueMappings
										where
											d.DataSyncSystemId == dataSyncSystemId &&
											d.ProjectId == projectId &&
											!d.CustomPropertyValue.IsDeleted &&
											d.CustomPropertyValue.CustomPropertyListId == customPropertyListId
										orderby d.CustomPropertyValueId
										select d;
							customPropertyMappingValues = query.ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return customPropertyMappingValues;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Artifact Mapping functions

		/// <summary>
		/// Retrieves a list containing all the data-mappings for a specific artifact
		/// </summary>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">the type of the artifact</param>
		/// <param name="projectId">The current project</param>
		/// <returns>Datasync dataset of mappings</returns>
		/// <remarks>Also returns unmapped system records</remarks>
		public List<DataSyncArtifactMapping> RetrieveDataSyncArtifactMappings(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
		{
			const string METHOD_NAME = "RetrieveDataSyncArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncArtifactMapping> artifactMappings;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to get the unmapped and mapped entries
					//filtering by only projects and systems that are active
					artifactMappings = context.DataSync_RetrieveArtifactMappings(projectId, (int)artifactType, artifactId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Inserts/Updates/Deletes a set of data-sync artifact mapping records
		/// </summary>
		/// <param name="artifactMappings">The data to be persisted</param>
		public void SaveDataSyncArtifactMappings(List<DataSyncArtifactMapping> artifactMappings)
		{
			const string METHOD_NAME = "SaveDataSyncArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to go through the dataset and remove any items that have their external keys set to null
					//And make any ones that have their external keys set for the first time as inserts
					foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
					{
						//First we need to get the existing mappings
						var query = from d in context.DataSyncArtifactMappings
									where
										d.ArtifactId == artifactMapping.ArtifactId &&
										d.ArtifactTypeId == artifactMapping.ArtifactTypeId &&
										d.ProjectId == artifactMapping.ProjectId &&
										d.DataSyncSystemId == artifactMapping.DataSyncSystemId
									select d;

						DataSyncArtifactMapping existingMapping = query.FirstOrDefault();

						//See if we have an existing mapping or not
						if (existingMapping == null)
						{
							//Add a new mapping if an external key is set
							if (!String.IsNullOrWhiteSpace(artifactMapping.ExternalKey))
							{
								context.DataSyncArtifactMappings.AddObject(artifactMapping);
							}
						}
						else
						{
							//See if we have an external key set
							if (String.IsNullOrWhiteSpace(artifactMapping.ExternalKey))
							{
								//Remove
								context.DataSyncArtifactMappings.DeleteObject(existingMapping);
							}
							else
							{
								//Update
								existingMapping.StartTracking();
								existingMapping.ExternalKey = artifactMapping.ExternalKey;
							}
						}
					}

					//Actually perform the database persist
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns the artifact mappings for a specific external key
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the system</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactType">The id of the artifact type</param>
		/// <param name="externalKey">The external key we're looking for</param>
		/// <returns>List of mapping records</returns>
		public List<DataSyncArtifactMapping> RetrieveDataSyncArtifactMappingByExternalKey(int dataSyncSystemId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, string externalKey)
		{
			const string METHOD_NAME = "RetrieveDataSyncArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncArtifactMapping> artifactMappings;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the appropriate select command for retrieving the data mapping records
					var query = from d in context.DataSyncArtifactMappings
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									d.ArtifactTypeId == (int)artifactType &&
									d.ExternalKey == externalKey
								orderby d.ArtifactId
								select d;

					artifactMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list containing all the data-mappings for a specific plug-in and artifact type
		/// </summary>
		/// <param name="dataSyncSystemId">The id of the plug-in</param>
		/// <param name="artifactType">the type of the artifact</param>
		/// <param name="projectId">The current project</param>
		/// <returns>Datasync dataset of mappings</returns>
		/// <remarks>Only returns mapped records</remarks>
		public List<DataSyncArtifactMapping> RetrieveDataSyncArtifactMappings(int dataSyncSystemId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "RetrieveDataSyncArtifactMappings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<DataSyncArtifactMapping> artifactMappings;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the appropriate select command for retrieving the data mapping records
					var query = from d in context.DataSyncArtifactMappings
								where
									d.DataSyncSystemId == dataSyncSystemId &&
									d.ProjectId == projectId &&
									d.ArtifactTypeId == (int)artifactType
								orderby d.ArtifactId
								select d;

					artifactMappings = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactMappings;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Artifact Retrieval functions

		/// <summary>
		/// Retrieves a list of active artifact fields (that need to be datamapped)
		/// </summary>
		/// <param name="artifactType">The artifact type we're interested in</param>
		/// <returns>List of artifact fields</returns>
		/// <remarks>Only returns fields that have the datamapping field set</remarks>
		public List<ArtifactField> RetrieveArtifactFields(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "RetrieveArtifactFields";

			System.Data.DataSet artifactFieldDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactField> artifactFields;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from a in context.ArtifactFields
								where a.IsActive && a.IsDataMapping && a.ArtifactTypeId == (int)artifactType
								orderby a.Caption, a.ArtifactFieldId
								select a;

					//Actually execute the query and return the list
					artifactFields = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of active artifact types that can take part in data-syncing
		/// </summary>
		/// <returns>list of artifact types</returns>
		public List<ArtifactType> RetrieveArtifactTypes()
		{
			const string METHOD_NAME = "RetrieveArtifactTypes";

			List<ArtifactType> artifactTypes;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactTypes
								where a.IsActive && a.IsDataSync
								orderby a.ArtifactTypeId
								select a;

					artifactTypes = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ArtifactType> RetrieveAllArtifactTypes()
		{
			const string METHOD_NAME = "RetrieveArtifactTypes";

			List<ArtifactType> artifactTypes;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactTypes
								where a.IsActive && a.IsDataSync && a.IsNotify
								orderby a.ArtifactTypeId
								select a;

					artifactTypes = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown when a project doesn't have data-sync configured
	/// </summary>
	public class DataSyncNotConfiguredException : ApplicationException
	{
		public DataSyncNotConfiguredException()
		{
		}
		public DataSyncNotConfiguredException(string message)
			: base(message)
		{
		}
		public DataSyncNotConfiguredException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try and map two internal ids to the same external ids, but make both of them primary
	/// or if you don't have any primary external keys for unique external keys
	/// </summary>
	public class DataSyncPrimaryExternalKeyException : ApplicationException
	{
		public DataSyncPrimaryExternalKeyException()
		{
		}
		public DataSyncPrimaryExternalKeyException(string message)
			: base(message)
		{
		}
		public DataSyncPrimaryExternalKeyException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
