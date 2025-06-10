using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Linq.Expressions;
using System.Data;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Automation Hosts and Automation Engines that are created and managed in the system
	/// </summary>
	public class AutomationManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.AutomationManager::";

		#region Automation Hosts

		/// <summary>Handles any automation host specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The current project template (not used)</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleAutomationHostSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//No custom filters currently supported
			return false;
		}

		/// <summary>Counts all the automation hosts in the project</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted hosts in the count.</param>
		/// <returns>The total number of hosts</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int CountHosts(int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "CountHosts";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from a in context.AutomationHostsView
								where (!a.IsDeleted || includeDeleted) && a.ProjectId == projectId
								select a;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<AutomationHostView, bool>> filterClause = CreateFilterExpression<AutomationHostView>(projectId, null, Artifact.ArtifactTypeEnum.AutomationHost, filters, utcOffset, null, HandleAutomationHostSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<AutomationHostView>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all the active hosts in the project (used for lookups)</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="includeDeleted">Whether or not to include deleted hosts.</param>
		/// <returns>The automation dataset</returns>
		public List<AutomationHostView> RetrieveHosts(int projectId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveHosts()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AutomationHostView> automationHosts;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationHostsView
								where a.ProjectId == projectId && (!a.IsDeleted || includeDeleted) && a.IsActive
								orderby a.Name, a.AutomationHostId
								select a;

					automationHosts = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHosts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a filtered, sorted list of hosts (inactive and active)</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="sortProperty">The property to sort by</param>
		/// <param name="sortAscending">Should we sort ascending or descending</param>
		/// <param name="startRow">The first row to return</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="filters">The filters to apply</param>
		/// <param name="includeDeleted">Wether or not to include deleted hosts.</param>
		/// <returns>List of hosts</returns>
		public List<AutomationHostView> RetrieveHosts(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveHosts()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AutomationHostView> automationHosts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from a in context.AutomationHostsView
								where (!a.IsDeleted || includeDeleted) && a.ProjectId == projectId
								select a;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by name ascending
						query = query.OrderByDescending(a => a.Name).ThenBy(a => a.AutomationHostId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "AutomationHostId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template associated with the project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<AutomationHostView, bool>> filterClause = CreateFilterExpression<AutomationHostView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, filters, utcOffset, null, HandleAutomationHostSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<AutomationHostView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						return new List<AutomationHostView>();
					}

					//Execute the query
					automationHosts = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHosts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns></returns>
		public List<AutomationHostView> RetrieveDeletedHosts(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AutomationHostView> deletedHosts;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationHostsView
								where a.IsDeleted && a.ProjectId == projectId
								orderby a.AutomationHostId
								select a;

					//Actually execute the query and return the dataset
					deletedHosts = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return deletedHosts;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return new List<AutomationHostView>();
			}
		}

		/// <summary>Retrieves a single automation host by its project and token</summary>
		/// <param name="token">The token name of the host to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>The automation dataset</returns>
		/// <remarks>projectId is needed because tokens are only unique per-project</remarks>
		public AutomationHostView RetrieveHostByToken(int projectId, string token, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveHostByToken()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AutomationHostView automationHost;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationHostsView
								where
									a.ProjectId == projectId &&
									(!a.IsDeleted || includeDeleted) &&
									a.Token.ToLower() == token.ToLower()
								select a;

					automationHost = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (automationHost == null)
				{
					throw new ArtifactNotExistsException("Automation Host with token '" + token + "' doesn't exist in project PR" + projectId + ".");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHost;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't log this because often the caller doesn't know which host is in which project
				//and we were getting too many erroneus warning messages
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single automation host by its ID</summary>
		/// <param name="automationHostId">The id of the host to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>The automation view entity</returns>
		public AutomationHostView RetrieveHostById(int automationHostId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveHostById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AutomationHostView automationHost;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationHostsView
								where
									(!a.IsDeleted || includeDeleted) &&
									a.AutomationHostId == automationHostId
								select a;

					automationHost = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (automationHost == null)
				{
					throw new ArtifactNotExistsException("Automation Host AH" + automationHostId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHost;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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

		/// <summary>Retrieves a single automation host by its ID</summary>
		/// <param name="automationHostId">The id of the host to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>The automation entity</returns>
		public AutomationHost RetrieveHostById2(int automationHostId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveHostById2()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AutomationHost automationHost;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationHosts
								where
									(!a.IsDeleted || includeDeleted) &&
									a.AutomationHostId == automationHostId
								select a;

					automationHost = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (automationHost == null)
				{
					throw new ArtifactNotExistsException("Automation Host AH" + automationHostId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHost;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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

		/// <summary>Updates an automation host entity, handling concurrent updates correctly</summary>
		/// <param name="automationHost">The automation host entity</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="changeSetId">The id of the changeset if a rollback</param>
		public void UpdateHost(AutomationHost automationHost, int userId, long? changeSetId = null)
		{
			const string METHOD_NAME = "UpdateHost()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (automationHost == null)
			{
				return;
			}

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					automationHost.StartTracking();

					//Update the last-update and concurrency dates
					automationHost.LastUpdateDate = DateTime.UtcNow;
					automationHost.ConcurrencyDate = DateTime.UtcNow;

					//Now apply the changes
					context.AutomationHosts.ApplyChanges(automationHost);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, changeSetId);
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes an automation host in the current project</summary>
		/// <param name="automationHostId">The id of the host</param>
		public void DeleteHostFromDatabase(int automationHostId, int userId)
		{
			const string METHOD_NAME = "DeleteHostFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First make sure it exists
					var query = from a in context.AutomationHosts
								where a.AutomationHostId == automationHostId
								select a;

					AutomationHost automationHost = query.FirstOrDefault();
					if (automationHost != null)
					{
						//Capture the project id
						int projectId = automationHost.ProjectId;

						//Log the purge.
						new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, DateTime.UtcNow, automationHost.Name);

						//Actually perform the delete
						context.Automation_DeleteHost(automationHostId);
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
		}

		/// <summary>Undeletes an automation host, making it available to users.</summary>
		/// <param name="automationHostId">automation host ID</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void UnDeleteHost(int automationHostId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDeleteHost()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int projectId = -1;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//We need to initially retrieve the host (needs to be marked as deleted)
				var query = from a in context.AutomationHosts
							where a.AutomationHostId == automationHostId && a.IsDeleted
							select a;

				//Get the automation host
				AutomationHost automationHost = query.FirstOrDefault();
				if (automationHost != null)
				{
					projectId = automationHost.ProjectId;

					//Mark as undeleted
					automationHost.StartTracking();
					automationHost.LastUpdateDate = DateTime.UtcNow;
					automationHost.IsDeleted = false;

					//Save changes, no history logged, that's done later
					context.SaveChanges();
				}
			}

			//Log the undelete
			if (logHistory && projectId > 0)
			{
				//Okay, mark it as being undeleted.
				new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, rollbackId, DateTime.UtcNow);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Marks an Automation Host as being deleted.</summary>
		/// <param name="automationEngineId">The host ID.</param>
		/// <param name="userId">The user performing the delete.</param>
		public void MarkHostAsDeleted(int projectId, int automationHostId, int userId)
		{
			const string METHOD_NAME = "MarkHostAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the automation host to see that it exists
				bool deletePerformed = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AutomationHosts
								where a.AutomationHostId == automationHostId && !a.IsDeleted
								select a;

					AutomationHost automationHost = query.FirstOrDefault();
					if (automationHost != null)
					{
						//Mark as deleted
						automationHost.StartTracking();
						automationHost.LastUpdateDate = DateTime.UtcNow;
						automationHost.IsDeleted = true;
						context.SaveChanges();
						deletePerformed = true;
					}
				}

				if (deletePerformed)
				{
					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Inserts a new automation host into the specified project</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="name">The name of the host</param>
		/// <param name="token">The unique token for the host</param>
		/// <param name="description">The description of the host (optional)</param>
		/// <param name="active">Whether the host is active or not</param>
		/// <param name="userId">The user creating the Automation Host.</param>
		/// <returns>The id of the new automation host</returns>
		public int InsertHost(int projectId, string name, string token, string description, bool active, int userId)
		{
			const string METHOD_NAME = "InsertHost()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int automationHostId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Populate the new entity
					AutomationHost automationHost = new AutomationHost();
					automationHost.ProjectId = projectId;
					automationHost.Name = name;
					automationHost.Description = description;
					automationHost.Token = token;
					automationHost.IsActive = active;
					automationHost.LastUpdateDate = DateTime.UtcNow;
					automationHost.ConcurrencyDate = DateTime.UtcNow;
					automationHost.IsDeleted = false;

					//Persist the automation host and get the new id
					context.AutomationHosts.AddObject(automationHost);
					context.SaveChanges();
					automationHostId = automationHost.AutomationHostId;
				}

				//Add a history record..
				new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, automationHostId, DateTime.UtcNow);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationHostId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Automation Engines

		/// <summary>Inserts a new automation engine into the system</summary>
		/// <param name="name">The name of the engine</param>
		/// <param name="token">The unique token for the engine</param>
		/// <param name="description">The description of the engine (optional)</param>
		/// <param name="active">Whether the engine is active or not</param>
		/// <param name="userId">The userid creating the AutomationEngine</param>
		/// <returns>The id of the new automation engine</returns>
		public int InsertEngine(string name, string token, string description, bool active, int userId, int? currentUserId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "InsertEngine";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			string newValue = "";
			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				int automationEngineId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Populate the new entity
					AutomationEngine automationEngine = new AutomationEngine();
					automationEngine.Name = name;
					automationEngine.Description = description;
					automationEngine.Token = token;
					automationEngine.IsActive = active;
					automationEngine.IsDeleted = false;

					//Persist the new automation engine
					context.AutomationEngines.AddObject(automationEngine);
					context.SaveChanges();
					automationEngineId = automationEngine.AutomationEngineId;
					newValue = automationEngine.Name;
				}

				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				details.NEW_VALUE = newValue;

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(currentUserId), Convert.ToInt32(adminSectionId), automationEngineId, action, details, DateTime.UtcNow, ArtifactTypeEnum.AutomationEngine, "AutomationEngineId");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationEngineId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the automation engine record by its id</summary>
		/// <param name="automationEngineId">The id of the automation engine</param>
		/// <param name="includeDeleted">Witehr or not to include deleted items. Default:FALSE</param>
		/// <returns>The automation engine</returns>
		public AutomationEngine RetrieveEngineById(int automationEngineId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveEngineById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AutomationEngine automationEngine;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationEngines
								where
									a.AutomationEngineId == automationEngineId &&
									(!a.IsDeleted || includeDeleted)
								orderby a.Name, a.AutomationEngineId
								select a;

					automationEngine = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (automationEngine == null)
				{
					throw new ArtifactNotExistsException("Automation Engine " + automationEngineId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationEngine;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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

		/// <summary>Retrieves the automation engine record by its token</summary>
		/// <param name="token">The token of the automation engine</param>
		/// <param name="includeDeleted">Whether or not to include delete items. Default:FALSE</param>
		/// <returns>The automation dataset</returns>
		public AutomationEngine RetrieveEngineByToken(string token, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveEngineByToken()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AutomationEngine automationEngine;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationEngines
								where
									a.Token == token &&
									(!a.IsDeleted || includeDeleted)
								orderby a.Name, a.AutomationEngineId
								select a;

					automationEngine = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (automationEngine == null)
				{
					throw new ArtifactNotExistsException("Automation Engine with Token = '" + token + "' doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationEngine;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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

		/// <summary>Updates an automation engine.</summary>
		/// <param name="automationEngine">The automation engine entity</param>
		/// <param name="userId">The user updating the engine.</param>
		public void UpdateEngine(AutomationEngine automationEngine, int userId, int? currentUserId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "UpdateEngine()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach to the context, keeping any changes
					context.AutomationEngines.ApplyChanges(automationEngine);
					//context.SaveChanges(userId, false, false, null);
					context.AdminSaveChanges(currentUserId, automationEngine.AutomationEngineId, null, adminSectionId, action, true, true, null);
					context.SaveChanges(userId, false, false, null);
					//historyChangeSets = historyManager.LogAdminHistoryAction_Begin(this.ObjectStateManager, userId.Value, 0, guidId.Value, adminSectionId.Value, action, rollbackId);
				}
				//AdminAuditManager adminAuditManager = new AdminAuditManager();
				//adminAuditManager.LogAdminHistoryAction_Begin(this.ObjectStateManager, userId, 0, guidId.Value, adminSectionId.Value, action, rollbackId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves all the active automation engines in the system</summary>
		/// <param name="activeOnly">Do we only want active engines in the list?</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default: FALSE</param>
		/// <returns>The automation engine list</returns>
		public List<AutomationEngine> RetrieveEngines(bool activeOnly = true, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveEngines()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AutomationEngine> automationEngines;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record(s)
					var query = from a in context.AutomationEngines
								where
									(a.IsActive || !activeOnly) &&
									(!a.IsDeleted || includeDeleted)
								orderby a.Name, a.AutomationEngineId
								select a;

					automationEngines = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return automationEngines;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes an automation engine in the system</summary>
		/// <param name="automationEngineId">The id of the engine</param>
		public void DeleteEngine(int automationEngineId, int userId)
		{
			const string METHOD_NAME = "DeleteEngine()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from v in context.AutomationEngines
								where v.AutomationEngineId == automationEngineId
								select v;

					//Make sure we have an existing entry
					AutomationEngine automationEngine = query.FirstOrDefault();

					context.Automation_DeleteEngine(automationEngineId);

					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
					string adminSectionName = "Test Automation";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;

					//Add a changeset to mark it as deleted.
					new AdminAuditManager().LogDeletion1((int)userId, automationEngine.AutomationEngineId, automationEngine.Name, adminSectionId, "Test Automation Deleted", DateTime.UtcNow, ArtifactTypeEnum.AutomationEngine, "AutomationEngineId");
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		#endregion
	}
}
