using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	public class UserAuditManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.UserAuditManager::";

		#region Internal Methods
		/// <summary>Prepares the history records for saving to the database.</summary>
		/// <param name="objMgr">The object manager that has our changed items.</param>
		/// <param name="changerId">The user id making the change.</param>
		/// <param name="rollbackId">The rollback ID used for recording rollback actions</param>
		/// <returns>A list of changesets and associated list of HistoryRecords.</returns>
		internal List<TST_USER_HISTORY_CHANGESET> LogUserHistoryAction_Begin(ObjectStateManager objMgr, int changerId, int artifactId, int adminSectionId, string action, long? rollbackId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "LogUserHistoryAction_Begin()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<TST_USER_HISTORY_CHANGESET> historyChangeSets = new List<TST_USER_HISTORY_CHANGESET>();

			//Log history..
			foreach (ObjectStateEntry entry in objMgr.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted))
			{
				if (entry.Entity is Artifact)
				{
					//Cast to an artifact
					Artifact artifact = (Artifact)entry.Entity;
					int artifactTypeId = (int)artifact.ArtifactType;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();

					//Create the HistoryChangeSet first..
					//TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
					////historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.UPDATED_BY_USERID = changerId;
					//historyChangeSet.USER_ID = artifactId;
					//historyChangeSet.EVENT_DESCRIPTION = action;
					//historyChangeSet.CHANGE_DATE = DateTime.UtcNow;

					//historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in artifact.EntityChangeTracker.OriginalValues)
								{
									TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.UPDATED_BY_USERID = changerId;
									historyChangeSet.USER_ID = artifactId;
									historyChangeSet.EVENT_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;

									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
									string fieldName = changedField.Key;
									//historyChangeSet.PROPERTY_NAME = fieldName;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = artifact[fieldName];
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											//Get the old and new values
											//New Values
											object newValue = currentValue;
											string newValueString = newValue.ToDatabaseSerialization();
											int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
											DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

											//Old Values
											object oldValue = originalValue;
											string oldValueString = oldValue.ToDatabaseSerialization();
											int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
											DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

											if(oldValueString == "Y")
											{
												oldValueString = "True";
											}
											else if (oldValueString == "N")
											{
												oldValueString = "False";
											}
											if (newValueString == "Y")
											{
												newValueString = "True";
											}
											else if (newValueString == "N")
											{
												newValueString = "False";
											}
											historyChangeSet.OLD_VALUE = oldValueString;
											historyChangeSet.NEW_VALUE = newValueString;

											historyChangeSet.PROPERTY_NAME = fieldName;
											//historyChangeSets.Add(historyChangeSet);
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}

				if (entry.Entity is UserProfile)
				{
					//Cast to an artifact
					UserProfile artifact = (UserProfile)entry.Entity;
					
					//Create the HistoryChangeSet first..
					//TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);					

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in artifact.ChangeTracker.OriginalValues)
								{

									string fieldName = changedField.Key;

									TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);

									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.UPDATED_BY_USERID = changerId;
									historyChangeSet.USER_ID = artifactId;
									historyChangeSet.EVENT_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;

									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									//historyChangeSet.PROPERTY_NAME = fieldName;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = artifact.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in artifact.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(artifact);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the old and new values
											//New Values
											object newValue = currentValue;
											string newValueString = newValue.ToDatabaseSerialization();
											int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
											DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

											//Old Values
											object oldValue = originalValue;
											string oldValueString = oldValue.ToDatabaseSerialization();
											int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
											DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);


											if (oldValueString == "Y")
											{
												oldValueString = "True";
											}
											else if (oldValueString == "N")
											{
												oldValueString = "False";
											}
											if (newValueString == "Y")
											{
												newValueString = "True";
											}
											else if (newValueString == "N")
											{
												newValueString = "False";
											}


											historyChangeSet.OLD_VALUE = oldValueString;
											historyChangeSet.NEW_VALUE = newValueString;

											historyChangeSet.PROPERTY_NAME = fieldName;
											//historyChangeSets.Add(historyChangeSet);
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}

				if (entry.Entity is ProjectUser)
				{
					//Cast to an artifact
					ProjectUser projectUser = (ProjectUser)entry.Entity;

					//Create the HistoryChangeSet first..
					//TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);					

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
								//Deletes are tracked separately using the LogDeletion() function
								ProjectManager pm = new ProjectManager();
								var roleData = pm.RetrieveProjectRoleById(projectUser.ProjectRoleId);
								var userdata = pm.RetrieveUserById(projectUser.UserId);
								var projectdata = pm.RetrieveProjectById(projectUser.ProjectId);
								string newValueString = "Project - " + projectdata.Name + " and Project Role - " + roleData.Name + " Removed from " + userdata.UserName;

								TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
								historyChangeSets.Add(historyChangeSet);
								historyChangeSet.OLD_VALUE = "Project - " + projectdata.Name + " and Project Role - " + roleData.Name;   
								historyChangeSet.NEW_VALUE = newValueString; 
								historyChangeSet.PROPERTY_NAME = "User Membership"; 
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								ProjectManager pm = new ProjectManager();
								var roleData = pm.RetrieveProjectRoleById(projectUser.ProjectRoleId);
								var userdata = pm.RetrieveUserById(projectUser.UserId);
								var projectdata = pm.RetrieveProjectById(projectUser.ProjectId);
								string newValueString = "Project - " + projectdata.Name + " and Project Role - " + roleData.Name + " Assigned to " + userdata.UserName;
								TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
								historyChangeSets.Add(historyChangeSet);

								historyChangeSet.OLD_VALUE = null;
								historyChangeSet.NEW_VALUE = newValueString;
								historyChangeSet.PROPERTY_NAME = "User Membership";

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in projectUser.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									TST_USER_HISTORY_CHANGESET historyChangeSet = new TST_USER_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet = new TST_USER_HISTORY_CHANGESET();
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.UPDATED_BY_USERID = changerId;
									historyChangeSet.USER_ID = artifactId;
									historyChangeSet.EVENT_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;

									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectUser.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectUser.GetType().GetProperties())
										{
											
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectUser);
												break;
											}
										}
										if (currentValue != null)
										{
											object originalValue = changedField.Value;
											Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

											bool fieldHasChanged = false;
											if (currentValue == null)
											{
												if (originalValue != null)
												{
													fieldHasChanged = true;
												}
											}
											else
											{
												if (!currentValue.Equals(originalValue))
												{
													//If it's a string, we don't care about extra spaces added/removed.
													if (currentValue.GetType() == typeof(string))
													{
														if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
														{
															fieldHasChanged = true;
														}
													}
													else
													{
														fieldHasChanged = true;
													}
												}
											}

											if (fieldHasChanged)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);
												if (oldValueString != null && newValueString != null && oldValueString!="error" && newValueString!="error")
												{
													ProjectManager pm = new ProjectManager();
													var oldData = pm.RetrieveProjectRoleById(int.Parse(oldValueString));
													var newData = pm.RetrieveProjectRoleById(int.Parse(newValueString));
													if (oldData.Name == "Y")
													{
														oldData.Name = "True";
													}
													else if (oldData.Name == "N")
													{
														oldData.Name = "False";
													}
													if (newData.Name == "Y")
													{
														newData.Name = "True";
													}
													else if (newValueString == "N")
													{
														newData.Name = "False";
													}

													historyChangeSet.OLD_VALUE = oldData.Name;
													historyChangeSet.NEW_VALUE = newData.Name;

													historyChangeSet.PROPERTY_NAME = fieldName;
													//historyChangeSets.Add(historyChangeSet);
												}
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}

			}
			Logger.LogExitingEvent(METHOD_NAME);
			return historyChangeSets;
		}

		/// <summary>Gets final data from the Object State Manager and saved the records to the history tables.</summary>
		/// <param name="historyChangeSets">The history changesets and associated change details</param>
		internal void LogUserHistoryAction_End(List<TST_USER_HISTORY_CHANGESET> userHistoryChangeSets)
		{
			const string METHOD_NAME = CLASS_NAME + "LogUserHistoryAction_End()";
			Logger.LogEnteringEvent(METHOD_NAME);
			try
			{
				//Save the dataset.
				//Insert(historyChangeSets);

				long userHistoryChangeSetId = 0;
				if (userHistoryChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (TST_USER_HISTORY_CHANGESET userHistoryChangeSet in userHistoryChangeSets)
						{
							TST_USER_HISTORY_CHANGESET_AUDIT userHistoryChangesetData = new TST_USER_HISTORY_CHANGESET_AUDIT();
							if (userHistoryChangeSet.USER_ID != 0)
							{
								userHistoryChangesetData.USER_ID = userHistoryChangeSet.USER_ID;
								userHistoryChangesetData.ADMIN_SECTION_ID = userHistoryChangeSet.ADMIN_SECTION_ID;
								userHistoryChangesetData.EVENT_DESCRIPTION = userHistoryChangeSet.EVENT_DESCRIPTION;
								userHistoryChangesetData.CHANGE_DATE = userHistoryChangeSet.CHANGE_DATE;
								userHistoryChangesetData.HISTORY_CHANGESET_TYPE_ID = userHistoryChangeSet.HISTORY_CHANGESET_TYPE_ID;
								userHistoryChangesetData.PROPERTY_NAME = userHistoryChangeSet.PROPERTY_NAME;
								userHistoryChangesetData.OLD_VALUE = userHistoryChangeSet.OLD_VALUE;
								userHistoryChangesetData.NEW_VALUE = userHistoryChangeSet.NEW_VALUE;
								userHistoryChangesetData.UPDATED_BY_USERID = userHistoryChangeSet.UPDATED_BY_USERID;

								context.TST_USER_HISTORY_CHANGESET_AUDIT.Add(userHistoryChangesetData);
							}

						}
						context.SaveChanges();

						userHistoryChangeSetId = context.TST_USER_HISTORY_CHANGESET_AUDIT.OrderByDescending(q => q.CHANGESET_ID)
.FirstOrDefault().CHANGESET_ID;
					}
				}

						Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Logs a creation into the ChangeSet for the specified adminSection id, type.</summary>
		/// <param name="userId">The userid who performed the creation.</param>
		/// <param name="adminSectionId">The adminSectionId.</param>
		/// <param name="changeDate">The date of the creation. If null, uses current date/time.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogCreation(int userId, int adminSectionId, int updatedUserId, string userName, string action, DateTime? changeDate = null)
		{
			const string METHOD_NAME = "LogCreation()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new changeset.
			TST_USER_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_USER_HISTORY_CHANGESET_AUDIT();
			hsChangeSet.USER_ID = userId;
			hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
			hsChangeSet.CHANGE_DATE = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Added;
			hsChangeSet.EVENT_DESCRIPTION = action;
			hsChangeSet.UPDATED_BY_USERID = updatedUserId;
			hsChangeSet.NEW_VALUE = userName;

			long changeSetId = Insert(hsChangeSet);


			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		#endregion Internal Methods

		/// <summary>
		/// Logs a single change set
		/// </summary>
		/// <param name="userHistoryChangeSet">A single changeset</param>
		/// <returns></returns>
		internal long Insert(TST_USER_HISTORY_CHANGESET_AUDIT userHistoryChangeSet)
		{
			List<TST_USER_HISTORY_CHANGESET_AUDIT> userHistoryChangeSets = new List<TST_USER_HISTORY_CHANGESET_AUDIT>();
			userHistoryChangeSets.Add(userHistoryChangeSet);
			return Insert(userHistoryChangeSets);
		}

		/// <summary>Inserts a History Change set. Needs to have an unsaved ChangeSet row(s), with 0 or more HistoryField rows.</summary>
		/// <param name="userHistoryChangeSets">The changesets.</param>
		/// <returns>The id of the FIRST ChangeSet inserted.</returns>
		protected long Insert(List<TST_USER_HISTORY_CHANGESET_AUDIT> userHistoryChangeSets)
		{
			const string METHOD_NAME = "Insert";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//All we're doing is saving this entity
				long userHistoryChangeSetId = 0;
				if (userHistoryChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (TST_USER_HISTORY_CHANGESET_AUDIT userHistoryChangeSet in userHistoryChangeSets)
						{
							TST_USER_HISTORY_CHANGESET_AUDIT userhistoryDetail = new TST_USER_HISTORY_CHANGESET_AUDIT();
							//historyChangeSet.Details.Add(historyDetail);
							userhistoryDetail.USER_ID = userHistoryChangeSet.USER_ID;
							userhistoryDetail.UPDATED_BY_USERID = userHistoryChangeSet.UPDATED_BY_USERID;    
							userhistoryDetail.CHANGE_DATE = userHistoryChangeSet.CHANGE_DATE;       
							userhistoryDetail.OLD_VALUE = userHistoryChangeSet.OLD_VALUE;          
							userhistoryDetail.EVENT_DESCRIPTION = userHistoryChangeSet.EVENT_DESCRIPTION;
							userhistoryDetail.PROPERTY_NAME = userHistoryChangeSet.PROPERTY_NAME;
							userhistoryDetail.NEW_VALUE = userHistoryChangeSet.NEW_VALUE;   
							userhistoryDetail.HISTORY_CHANGESET_TYPE_ID = userHistoryChangeSet.HISTORY_CHANGESET_TYPE_ID;
							userhistoryDetail.ADMIN_SECTION_ID = userHistoryChangeSet.ADMIN_SECTION_ID;

							context.TST_USER_HISTORY_CHANGESET_AUDIT.Add(userhistoryDetail);
							context.SaveChanges();
							//context.UserHistoryChangesets.AddObject(userHistoryChangeSet);
							//context.UserSaveChanges();
						}

						userHistoryChangeSetId = context.TST_USER_HISTORY_CHANGESET_AUDIT.OrderByDescending(q => q.CHANGESET_ID)
.FirstOrDefault().CHANGESET_ID;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return userHistoryChangeSetId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<UserHistoryChangeSetResponse> RetrieveUserHistoryChangeSets(double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<UserHistoryChangeSetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.UserHistoryChangesView
								join u in context.UserProfiles
								on h.UPDATED_BY_USERID equals u.UserId
								select new UserHistoryChangeSetResponse
								{
									FieldName = h.EVENT_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = h.USER_ID,
									//LastLoginDate = h.LAST_LOGIN_DATE,
									//LastLogoutDate = h.LAST_LOGOUT_DATE,
									PropertyName = h.PROPERTY_NAME,
									UpdatedUserId = h.UPDATED_BY_USERID,
									UpdatedUserName = u.FirstName + " " + u.LastName
								};

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<UserHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<UserHistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<UserHistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
					query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");



					//Get the count
					int count = query.Count();

					var changeSets_1 = query.ToList();

					foreach (var c in changeSets_1)
					{
						DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
						c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					}

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
						}
					}
					//Now sort by Time in memory if needed
					if (sortProperty == "Time")
					{
						if (sortAscending)
						{
							changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
					}

					query = changeSets_1.AsQueryable();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public int CountSet(int? projectId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountSet()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from h in context.UserHistoryChangesView
								join u in context.UserProfiles
								on h.UPDATED_BY_USERID equals u.UserId
								select new UserHistoryChangeSetResponse
								{
									FieldName = h.EVENT_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = u.FirstName + " " + u.LastName,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = h.UPDATED_BY_USERID,
									//LastLoginDate = h.LAST_LOGIN_DATE,
									//LastLogoutDate = h.LAST_LOGOUT_DATE,
									PropertyName = h.PROPERTY_NAME,
									UpdatedUserId = h.UPDATED_BY_USERID,
									UpdatedUserName = u.FirstName + " " + u.LastName
								};

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<UserHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<UserHistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<UserHistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the audit history log associated with a particular ChangeSet</summary>
		/// <param name="changeSetId">The changeset ID to retrieve</param>
		/// <param name="filterList">The list of filters to apply</param>
		/// <param name="sortProperty">What field to sort on</param>
		/// <param name="sortAscending">What direction to sort</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="paginationSize">The number of rows to return</param>
		/// <returns>A history detail list</returns>
		public List<UserHistoryChangeSetResponse> RetrieveByChangeSetId(int projectId, long changeSetId, string sortProperty, bool sortAscending, Hashtable filterList, int startRow, int paginationSize, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveByChangeSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<UserHistoryChangeSetResponse> historyChanges;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.UserHistoryChangesView
								select new UserHistoryChangeSetResponse
								{
									FieldName = h.EVENT_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = h.UPDATED_BY_USERID,
									//LastLoginDate = h.LAST_LOGIN_DATE,
									//LastLogoutDate = h.LAST_LOGOUT_DATE,
									PropertyName = h.PROPERTY_NAME,
									UpdatedUserId = h.UPDATED_BY_USERID,
									UpdatedUserName = h.USER_NAME
								};

					if (changeSetId > 1)
					{
						query = query.Where(h => h.ChangeSetId == changeSetId);
					}

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<UserHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<UserHistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<UserHistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}



					//Get the count
					int count = query.Count();

					var changeSets_1 = query.ToList();

					foreach (var c in changeSets_1)
					{
						DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
						c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					}

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
						}
					}
					//Now sort by Time in memory if needed
					if (sortProperty == "Time")
					{
						if (sortAscending)
						{
							changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
					}

					query = changeSets_1.AsQueryable();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					historyChanges = query
						.Skip(startRow - 1)
						.Take(paginationSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return historyChanges;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public UserHistoryChangeSetResponse RetrieveChangeSetById(long changeSetId, bool includeDetails, bool includeAssociations = false, bool includePositions = false)
		{
			const string METHOD_NAME = "RetrieveChangeSetById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				UserHistoryChangeSetResponse changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.UserHistoryChangesView
								select new UserHistoryChangeSetResponse
								{
									FieldName = h.EVENT_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = h.USER_ID,
									//LastLoginDate = h.LAST_LOGIN_DATE,
									//LastLogoutDate = h.LAST_LOGOUT_DATE,
									PropertyName = h.PROPERTY_NAME,
									UpdatedUserId = h.UPDATED_BY_USERID,
									UpdatedUserName = h.USER_NAME
								};

					changeSet = query.FirstOrDefault(h => h.ChangeSetId == changeSetId);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return changeSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}
}
