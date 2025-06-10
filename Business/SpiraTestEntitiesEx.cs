using EFProviderWrapperToolkit;
using EFTracingProvider;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Business
{
	public partial class SpiraTestEntitiesEx : SpiraTestEntities
	{
		private const string CLASS_NAME = "SpiraTestEntitiesEx";

		private TextWriter logOutput;

        #if DEBUG

        /// <summary>
        /// Enables the DB tracing when the system is running in Debug mode
        /// </summary>
        public SpiraTestEntitiesEx()
			: this("name=SpiraTestEntities")
		{
		}

        /// <summary>
        /// Enables the DB tracing when the system is running in Debug mode
        /// </summary>
		public SpiraTestEntitiesEx(string connectionString)
			: base(EntityConnectionWrapperUtils.CreateEntityConnectionWithWrappers(
					connectionString,
					"EFTracingProvider"
			))
		{
		}

        #endif

		#region Overrides

		/// <summary>
		/// Saves any changes to the entity data model
		/// </summary>
		/// <param name="options">The save options</param>
		/// <returns>The number of objects in an Added, Modified, or Deleted state when SaveChanges was called.</returns>
		/// <remarks>Wraps some of the provider-specific exceptions to make then provider agnostic</remarks>
		public override int SaveChanges(SaveOptions options)
		{
			return this.SaveChanges(options, null, false, false, null, null);
		}

		/// <summary>
		/// Saves any changes to the entity data model
		/// </summary>
		/// <returns>The number of objects in an Added, Modified, or Deleted state when SaveChanges was called.</returns>
		/// <remarks>Wraps some of the provider-specific exceptions to make then provider agnostic</remarks>
		public new int SaveChanges()
		{
			return this.SaveChanges(null, false, false, null);
		}

		public new int AdminSaveChanges()
		{
			return this.AdminSaveChanges(null,null, null, null, null, false, false, null);
		}

		public new int UserSaveChanges()
		{
			return this.UserSaveChanges(null, null, null,null, false, false, null);
		}
		public new int UserActivityLogSaveChanges()
		{
			return this.UserActivityLogSaveChanges(null, null, null, null, null, null, false, false, null);
		}
		#endregion

		#region New Methods

		/// <summary>
		/// Gets the column max length of an entity
		/// </summary>
		/// <param name="entityTypeName"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		protected int? GetColumnMaxLength(Type entityType, string columnName)
        {
            int? result = null;

            var q = from meta in this.MetadataWorkspace.GetItems(DataSpace.CSpace)
                              .Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                    from p in (meta as EntityType).Properties
                    .Where(p => p.Name == columnName
                                && p.TypeUsage.EdmType.Name == "String")
                    select p;

            var queryResult = q.Where(p =>
            {
                bool match = p.DeclaringType.Name == entityType.Name;
                if (!match && entityType != null)
                {
                    //Is a fully qualified name....
                    match = entityType.Name == p.DeclaringType.Name;
                }

                return match;

            }).Select(sel => sel.TypeUsage.Facets["MaxLength"].Value).Where (v => v is IConvertible);
            if (queryResult.Any())
            {
                result = Convert.ToInt32(queryResult.First());
            }

            return result;
        }

        /// <summary>
        /// Gets the column max length of an entity
        /// </summary>
        /// <param name="entityTypeName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        protected int? GetColumnMaxLength(string entityTypeName, string columnName)
        {
            Type entType = Type.GetType(entityTypeName);
            if (entType != null)
            {
                return GetColumnMaxLength(entType, columnName);
            }
            return null;
        }

        /// <summary>
        /// Truncates any objects that have lengths exceeding the model
        /// </summary>
        protected void AutoTruncateEntities()
        {
            foreach (ObjectStateEntry entry in this.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                if (entry.Entity is Entity)
                {
					Entity entity = (Entity)(entry.Entity);

                    //Make sure we have a writable string property
                    IEnumerable<PropertyInfo> properties = entity.Properties.Values.Where(p => p.CanWrite && p.PropertyType == typeof(String));
                    foreach (PropertyInfo property in properties)
                    {
                        int? maxlength = GetColumnMaxLength(entity.GetType(), property.Name);
                        if (maxlength.HasValue && entity[property.Name] != null && ((string)entity[property.Name]).Length > maxlength.Value)
                        {
                            entity[property.Name] = ((string)entity[property.Name]).MaxLength(maxlength.Value);
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Saves any changes to the entity data model
		/// </summary>
		/// <param name="updateHistory">Should we update the history with the save</param>
		/// <param name="sendNotification">Should we send a notification (obsolete)</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="rollbackId">The id of the rollback, if doing a rollback</param>
		/// <param name="options">The save options</param>
		/// <returns>The number of objects in an Added, Modified, or Deleted state when SaveChanges was called.</returns>
		/// <remarks>Wraps some of the provider-specific exceptions to make then provider agnostic</remarks>
		public int SaveChanges(SaveOptions options, int? userId, bool updateHistory, bool sendNotification, long? rollbackId, int? projectId = null, int? artifactId = null)
		{
			const string METHOD_NAME = "SaveChanges(options):: ";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            try
            {
                //Return value.
                int retInt = 0;

                //Make sure none of the entities will exceed the max length
                AutoTruncateEntities();

                //Set up transferring history and get history, if needed.
                HistoryManager historyManager = new HistoryManager();
                List<HistoryChangeSet> historyChangeSets = null;
				if (userId.HasValue && updateHistory)
				{
					if (projectId.HasValue && artifactId.HasValue) { 
					historyChangeSets = historyManager.LogHistoryAction_Begin(this.ObjectStateManager, userId.Value, rollbackId, projectId.Value, artifactId.Value);
					}
					else
					{
						historyChangeSets = historyManager.LogHistoryAction_Begin(this.ObjectStateManager, userId.Value, rollbackId, null);

					}

				}

                //Actually perform save.
                retInt = base.SaveChanges(options);

                //Now record the final history..
                if (historyChangeSets != null && historyChangeSets.Count > 0 && updateHistory)
                {
                    historyManager.LogHistoryAction_End(historyChangeSets);
                }

                //Notifications are not fired this way any more, called explicitly instead

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return retInt;
            }
            catch (OptimisticConcurrencyException exception)
            {
                //Log these as warning events
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (EntityCommandExecutionException exception)
            {
                if (exception.InnerException != null && exception.InnerException is SqlException)
                {
                    SqlException sqlException = (SqlException)exception.InnerException;
                    //Need to convert native SQL Server exceptions into a generic one
                    //Certain provider-specific errors get converted into generic ones so that the application
                    //code can be provider agnostic
                    if (sqlException.Number == 2601 || sqlException.Number == 2627)
                    {
                        EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred: " + sqlException.Message, exception);
                        throw ex;
                    }
                    else if (sqlException.Number == 547)
                    {
                        EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred: " + sqlException.Message, exception);
                        throw ex;
                    }
                    else if (sqlException.Number == 530)
                    {
                        throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
                    }
                    else
                    {
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message + " - " + sqlException.Message);
                        throw;
                    }
                }
                else
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
            catch (UpdateException exception)
            {
                if (exception.InnerException != null && exception.InnerException is SqlException)
                {
                    SqlException sqlException = (SqlException)exception.InnerException;
                    //Need to convert native SQL Server exceptions into a generic one
                    //Certain provider-specific errors get converted into generic ones so that the application
                    //code can be provider agnostic
                    if (sqlException.Number == 2601 || sqlException.Number == 2627)
                    {
                        EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
                        throw ex;
                    }
                    else if (sqlException.Number == 547)
                    {
                        EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
                        throw ex;
                    }
                    else
                    {
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                        throw;
                    }
                }
                else
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
		}

		public int AdminSaveChanges(SaveOptions options, int? userId, int? projectId, Guid? guidId, int? adminSectionId, string action, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "SaveChanges(options):: ";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				//Return value.
				int retInt = 0;

				//Make sure none of the entities will exceed the max length
				AutoTruncateEntities();

				//Set up transferring history and get history, if needed.
				AdminAuditManager historyManager = new AdminAuditManager();
				List<TST_ADMIN_HISTORY_CHANGESET> historyChangeSets = null;
				if (userId.HasValue && updateHistory)
				{
					if (guidId != null)
					{
						historyChangeSets = historyManager.LogAdminHistoryAction_Begin(this.ObjectStateManager, userId.Value, 0, guidId.Value, adminSectionId.Value, action, rollbackId);
					}
					else
					{
						historyChangeSets = historyManager.LogAdminHistoryAction_Begin(this.ObjectStateManager, userId.Value, projectId.Value, Guid.Empty, adminSectionId.Value, action, rollbackId);
					}
				}

				
				//Actually perform save.
				//commentedout old db save
				//retInt = base.SaveChanges(options);

				//Now record the final history..
				if (historyChangeSets != null && historyChangeSets.Count > 0 && updateHistory)
				{
					historyManager.LogHistoryAction_End(historyChangeSets);
				}

				//Notifications are not fired this way any more, called explicitly instead

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retInt;
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityCommandExecutionException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 530)
					{
						throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message + " - " + sqlException.Message);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public int UserSaveChanges(SaveOptions options,  int? projectId, int? userId, int? adminSectionId, string action, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "SaveChanges(options):: ";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				//Return value.
				int retInt = 0;

				//Make sure none of the entities will exceed the max length
				AutoTruncateEntities();

				//Set up transferring history and get history, if needed.
				UserAuditManager historyManager = new UserAuditManager();
				List<TST_USER_HISTORY_CHANGESET> historyChangeSets = null;
				if (userId.HasValue && updateHistory)
				{
					historyChangeSets = historyManager.LogUserHistoryAction_Begin(this.ObjectStateManager, userId.Value, projectId.Value, adminSectionId.Value, action, rollbackId);
				}

				//Actually perform save.
				retInt = base.SaveChanges(options);

				//Now record the final history..
				if (historyChangeSets != null && historyChangeSets.Count > 0 && updateHistory)
				{
					historyManager.LogUserHistoryAction_End(historyChangeSets);
				}

				//Notifications are not fired this way any more, called explicitly instead

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retInt;
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityCommandExecutionException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 530)
					{
						throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message + " - " + sqlException.Message);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public int UserActivityLogSaveChanges(SaveOptions options, int? userId, int? projectId, int? adminSectionId, string action, DateTime? lastLoginDate, DateTime? lastLogoutDate, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "SaveChanges(options):: ";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				//Return value.
				int retInt = 0;

				//Make sure none of the entities will exceed the max length
				AutoTruncateEntities();

				//Set up transferring history and get history, if needed.
				UserAuditManager historyManager = new UserAuditManager();
				List<TST_USER_HISTORY_CHANGESET> historyChangeSets = null;
				if (userId.HasValue && updateHistory)
				{
					historyChangeSets = historyManager.LogUserHistoryAction_Begin(this.ObjectStateManager, userId.Value, projectId.Value, adminSectionId.Value, action, rollbackId);
				}

				//Actually perform save.
				retInt = base.SaveChanges(options);

				//Now record the final history..
				if (historyChangeSets != null && historyChangeSets.Count > 0 && updateHistory)
				{
					historyManager.LogUserHistoryAction_End(historyChangeSets);
				}

				//Notifications are not fired this way any more, called explicitly instead

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retInt;
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityCommandExecutionException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred: " + sqlException.Message, exception);
						throw ex;
					}
					else if (sqlException.Number == 530)
					{
						throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.Message + " - " + sqlException.Message);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Saves any changes to the entity data model</summary>
		/// <param name="updateHistory">Should we update the history with the save</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="rollbackId">The id of the rollback, if doing a rollback</param>
		/// <param name="sendNotification">Should we send an email notification (obsolete)</param>
		/// <returns>The number of objects in an Added, Modified, or Deleted state when SaveChanges was called.</returns>
		/// <remarks>Wraps some of the provider-specific exceptions to make then provider agnostic</remarks>
		public int SaveChanges(int? userId, bool updateHistory, bool sendNotification, long? rollbackId, int? projectId = null, int? artifactId = null)
		{
			const string METHOD_NAME = "SaveChanges()";
			try
			{
				return this.SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, userId, updateHistory, sendNotification, rollbackId, projectId, artifactId);
			}
            catch (OptimisticConcurrencyException exception)
            {
                //Log these as warning events
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (SqlException sqlException)
			{
				//Need to convert native SQL Server exceptions into a generic one
				//Certain provider-specific errors get converted into generic ones so that the application
				//code can be provider agnostic
				if (sqlException.Number == 2601 || sqlException.Number == 2627)
				{
					EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", sqlException);
					throw ex;
				}
				else if (sqlException.Number == 547)
				{
					EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", sqlException);
					throw ex;
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, sqlException);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public int AdminSaveChanges(int? userId, int? projectId, Guid? guidId, int? adminsectionId, string action, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "AdminSaveChanges()";
			try
			{
				return this.AdminSaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, userId, projectId, guidId, adminsectionId, action, updateHistory, sendNotification, rollbackId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (SqlException sqlException)
			{
				//Need to convert native SQL Server exceptions into a generic one
				//Certain provider-specific errors get converted into generic ones so that the application
				//code can be provider agnostic
				if (sqlException.Number == 2601 || sqlException.Number == 2627)
				{
					EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", sqlException);
					throw ex;
				}
				else if (sqlException.Number == 547)
				{
					EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", sqlException);
					throw ex;
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, sqlException);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public int UserSaveChanges(int? userId, int? projectId, int? adminsectionId, string action, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "UserSaveChanges()";
			try
			{
				return this.UserSaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, projectId, userId,  adminsectionId, action,updateHistory, sendNotification, rollbackId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (SqlException sqlException)
			{
				//Need to convert native SQL Server exceptions into a generic one
				//Certain provider-specific errors get converted into generic ones so that the application
				//code can be provider agnostic
				if (sqlException.Number == 2601 || sqlException.Number == 2627)
				{
					EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", sqlException);
					throw ex;
				}
				else if (sqlException.Number == 547)
				{
					EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", sqlException);
					throw ex;
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, sqlException);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}


		//
		public int UserActivityLogSaveChanges(int? userId, int? projectId, int? adminsectionId, string action, DateTime? lastLoginDate, DateTime? lastLogoutDate, bool updateHistory, bool sendNotification, long? rollbackId)
		{
			const string METHOD_NAME = "UserActivityLogSaveChanges()";
			try
			{
				return this.UserActivityLogSaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, userId, projectId, adminsectionId, action, lastLoginDate, lastLogoutDate, updateHistory, sendNotification, rollbackId);
			}
			catch (OptimisticConcurrencyException exception)
			{
				//Log these as warning events
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (UpdateException exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
				{
					SqlException sqlException = (SqlException)exception.InnerException;
					//Need to convert native SQL Server exceptions into a generic one
					//Certain provider-specific errors get converted into generic ones so that the application
					//code can be provider agnostic
					if (sqlException.Number == 2601 || sqlException.Number == 2627)
					{
						EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", exception);
						throw ex;
					}
					else if (sqlException.Number == 547)
					{
						EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", exception);
						throw ex;
					}
					else
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			catch (SqlException sqlException)
			{
				//Need to convert native SQL Server exceptions into a generic one
				//Certain provider-specific errors get converted into generic ones so that the application
				//code can be provider agnostic
				if (sqlException.Number == 2601 || sqlException.Number == 2627)
				{
					EntityConstraintViolationException ex = new EntityConstraintViolationException("Database constraint violation occurred", sqlException);
					throw ex;
				}
				else if (sqlException.Number == 547)
				{
					EntityForeignKeyException ex = new EntityForeignKeyException("Database foreign key violation occurred", sqlException);
					throw ex;
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, sqlException);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}
		#endregion

		#region Tracing Extensions

		private EFTracingConnection TracingConnection
		{
			get { return this.UnwrapConnection<EFTracingConnection>(); }
		}

		public event EventHandler<CommandExecutionEventArgs> CommandExecuting
		{
			add { this.TracingConnection.CommandExecuting += value; }
			remove { this.TracingConnection.CommandExecuting -= value; }
		}

		public event EventHandler<CommandExecutionEventArgs> CommandFinished
		{
			add { this.TracingConnection.CommandFinished += value; }
			remove { this.TracingConnection.CommandFinished -= value; }
		}

		public event EventHandler<CommandExecutionEventArgs> CommandFailed
		{
			add { this.TracingConnection.CommandFailed += value; }
			remove { this.TracingConnection.CommandFailed -= value; }
		}

		private void AppendToLog(object sender, CommandExecutionEventArgs e)
		{
			if (this.logOutput != null)
			{
				this.logOutput.WriteLine(e.ToTraceString().TrimEnd());
				this.logOutput.WriteLine();
			}
		}

		public TextWriter Log
		{
			get { return this.logOutput; }
			set
			{
				if ((this.logOutput != null) != (value != null))
				{
					if (value == null)
					{
						CommandExecuting -= AppendToLog;
					}
					else
					{
						CommandExecuting += AppendToLog;
					}
				}

				this.logOutput = value;
			}
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown when a database unique constraint is violated
	/// </summary>
	/// <remarks>
	/// We catch provider native exceptions and throw this instead so that application code can be provider agnostic
	/// </remarks>
	[Serializable]
	public class EntityConstraintViolationException : EntityException
	{
		public EntityConstraintViolationException()
		{
		}
		public EntityConstraintViolationException(string message)
			: base(message)
		{
		}
		public EntityConstraintViolationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

    /// <summary>
    /// This exception is thrown when a SELECT query results in an infinite recursive loop
    /// </summary>
    [Serializable]
    public class EntityInfiniteRecursionException : Exception
    {
        public EntityInfiniteRecursionException()
        {
        }
        public EntityInfiniteRecursionException(string message)
            : base(message)
        {
        }
        public EntityInfiniteRecursionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

	/// <summary>
	/// This exception is thrown when a database foreign key constraint is violated
	/// </summary>
	/// <remarks>
	/// We catch provider native exceptions and throw this instead so that application code can be provider agnostic
	/// </remarks>
	[Serializable]
	public class EntityForeignKeyException : EntityException
	{
		public EntityForeignKeyException()
		{
		}
		public EntityForeignKeyException(string message)
			: base(message)
		{
		}
		public EntityForeignKeyException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
