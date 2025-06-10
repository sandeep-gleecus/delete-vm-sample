using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	public class AdminAuditManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.AdminSectionManager::";

		/// <summary>
		/// Loads in the static list of AdminSections
		/// </summary>
		public List<TST_ADMIN_SECTION_AUDIT> RetrieveAdminSection()
		{
			const string METHOD_NAME = "RetrieveAdminSection";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<TST_ADMIN_SECTION_AUDIT> adminSections;

			try
			{
				//Build the base query
				using (AuditTrailEntities context = new AuditTrailEntities())
				{
					var query = from a in context.TST_ADMIN_SECTION_AUDIT
								orderby a.ADMIN_SECTION_ID
								select a;

					adminSections = query.ToList();
				}

				return adminSections;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_ADMIN_SECTION_AUDIT RetrieveAdminSectionBySectionId(int adminSectionId)
		{
			const string METHOD_NAME = "RetrieveAdminSection";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			TST_ADMIN_SECTION_AUDIT adminSection;

			try
			{
				//Build the base query
				using (AuditTrailEntities context = new AuditTrailEntities())
				{
					var query = from a in context.TST_ADMIN_SECTION_AUDIT
								where a.ADMIN_SECTION_ID == adminSectionId
								orderby a.ADMIN_SECTION_ID
								select a;

					adminSection = query.FirstOrDefault();
				}

				return adminSection;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific admin section and all available fields for it.</summary>
		/// <param name="parentId">ID of the Admin Section to pull.</param>
		/// <returns>Admin Section</returns>
		public List<TST_ADMIN_SECTION_AUDIT> AdminSection_RetrieveByParentId(int parentId)
		{
			const string METHOD_NAME = "AdminSection_RetrieveByParentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				List<TST_ADMIN_SECTION_AUDIT> adminSetions;

				using (AuditTrailEntities context = new AuditTrailEntities())
				{
					var query = from a in context.TST_ADMIN_SECTION_AUDIT
								where a.PARENT_ID == parentId
								select a;

					adminSetions = query.ToList();
				}

				return adminSetions;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific admin section and all available fields for it.</summary>
		/// <param name="Name">Name of the Admin Section to pull.</param>
		/// <returns>Admin Section</returns>
		public TST_ADMIN_SECTION_AUDIT AdminSection_RetrieveByName(string name)
		{
			const string METHOD_NAME = "AdminSection_RetrieveByParentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set props.
			try
			{
				TST_ADMIN_SECTION_AUDIT adminSetion;

				using (AuditTrailEntities context = new AuditTrailEntities())
				{
					var query = from a in context.TST_ADMIN_SECTION_AUDIT
								where a.NAME == name
								select a;

					adminSetion = query.FirstOrDefault();
				}

				return adminSetion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#region Internal Methods
		/// <summary>Prepares the history records for saving to the database.</summary>
		/// <param name="objMgr">The object manager that has our changed items.</param>
		/// <param name="changerId">The user id making the change.</param>
		/// <returns>A list of changesets and associated list of HistoryRecords.</returns>
		internal List<TST_ADMIN_HISTORY_CHANGESET> LogAdminHistoryAction_Begin(ObjectStateManager objMgr, int changerId, int artifactId, Guid guidId, int adminSectionId, string action, long? rollbackId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "LogAdminHistoryAction_Begin()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<TST_ADMIN_HISTORY_CHANGESET> historyChangeSets = new List<TST_ADMIN_HISTORY_CHANGESET>();

			var data = objMgr.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

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
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//Get the various fields needed to record history
					int projectId = (int)artifact["ProjectId"];    //The artifact has a reference to the project id
					//int artifactId1 = (int)artifact[primaryKeyField];
					//string artifactName = String.Format(GlobalResources.General.History_ArtifactIdFormat, artifact.ArtifactPrefix, artifactId1); //Used if no name available

					////Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);


									string fieldName = changedField.Key;

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
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, projectId, projectTemplateId, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, projectId, projectTemplateId, fieldName, newValueInt.Value);
													}
												}


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



												//Add a new history detail entry
												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is Portfolio)
				{
					//Cast to an artifact
					Portfolio portfolio = (Portfolio)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.Portfolios;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in portfolio.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										//object currentValue = artifact[fieldName];
										object currentValue = portfolio.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in portfolio.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(portfolio);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ProjectGroup)
				{
					//Cast to an artifact
					ProjectGroup projectGroup = (ProjectGroup)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.Program;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in projectGroup.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectGroup.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectGroup.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectGroup);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ProjectGroupUser)
				{
					//Cast to an artifact
					ProjectGroupUser projectGroupUser = (ProjectGroupUser)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ProjectGroupUser;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
					//historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
					//historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
								//Deletes are tracked separately using the LogDeletion() function
								//Inserts are tracked separately using the LogCreation() function
								TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
								historyChangeSets.Add(historyChangeSet);
								historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
								historyChangeSet.ADMIN_USER_ID = changerId;
								historyChangeSet.ARTIFACT_ID = artifactId;
								historyChangeSet.ARTIFACT_GUID_ID = guidId;
								historyChangeSet.ACTION_DESCRIPTION = action;
								historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
								historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Deleted);

								ProjectManager pm = new ProjectManager();
								ProjectGroupManager pgm = new ProjectGroupManager();
								var roleData = pgm.RetrieveProjectGoupRoleById(projectGroupUser.ProjectGroupRoleId);
								var userdata = pm.RetrieveUserById(projectGroupUser.UserId);
								var projectGroupdata = pm.RetrieveProjectGroupById(projectGroupUser.ProjectGroupId);
								string oldValueString = "User - " + userdata.UserName + " and Program Role - " + roleData.Name + " Removed from " + projectGroupdata.Name;

								TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
								historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
								historyDetail.ADMIN_USER_ID = changerId;
								historyDetail.ADMIN_ARTIFACT_FIELD_NAME = "User Membership";   //Field Name
								historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = "User Membership";     //Field's Caption
								historyDetail.OLD_VALUE = oldValueString;          //The string representation
								historyDetail.OLD_VALUE_INT = null;
								historyDetail.OLD_VALUE_DATE = null;
								historyDetail.NEW_VALUE = null;    //The string representation
								historyDetail.NEW_VALUE_INT = null;
								historyDetail.NEW_VALUE_DATE = null;
								historyDetail.ADMIN_ARTIFACT_FIELD_ID = projectGroupUser.ProjectGroupId;   //The FieldID, 
								historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
								historyChangeSets.Add(historyChangeSet);
								historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
								historyChangeSet.ADMIN_USER_ID = changerId;
								historyChangeSet.ARTIFACT_ID = artifactId;
								historyChangeSet.ARTIFACT_GUID_ID = guidId;
								historyChangeSet.ACTION_DESCRIPTION = action;
								historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
								historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Added);

								ProjectManager pm = new ProjectManager();
								ProjectGroupManager pgm = new ProjectGroupManager();
								var roleData = pgm.RetrieveProjectGoupRoleById(projectGroupUser.ProjectGroupRoleId);
								var userdata = pm.RetrieveUserById(projectGroupUser.UserId);
								var projectGroupdata = pm.RetrieveProjectGroupById(projectGroupUser.ProjectGroupId);
								string newValueString = "User - " + userdata.UserName + " and Program Role - " + roleData.Name + " Assigned to " + projectGroupdata.Name;

								TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
								historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
								historyDetail.ADMIN_USER_ID = changerId;
								historyDetail.ADMIN_ARTIFACT_FIELD_NAME = "User Membership";   //Field Name
								historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = "User Membership";     //Field's Caption
								historyDetail.OLD_VALUE = null;          //The string representation
								historyDetail.OLD_VALUE_INT = null;
								historyDetail.OLD_VALUE_DATE = null;
								historyDetail.NEW_VALUE = newValueString;    //The string representation
								historyDetail.NEW_VALUE_INT = null;
								historyDetail.NEW_VALUE_DATE = null;
								historyDetail.ADMIN_ARTIFACT_FIELD_ID = projectGroupUser.ProjectGroupId;   //The FieldID, 
								historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property


							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in projectGroupUser.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectGroupUser.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectGroupUser.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectGroupUser);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ProjectTemplate)
				{
					//Cast to an artifact
					ProjectTemplate projectTemplate = (ProjectTemplate)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ProjectTemplate;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					////int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in projectTemplate.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectTemplate.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectTemplate.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectTemplate);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ProjectRole)
				{
					//Cast to an artifact
					ProjectRole projectRole = (ProjectRole)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ProjectRole;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in projectRole.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectRole.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectRole.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectRole);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											// 
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ProjectRolePermission)
				{
					//Cast to an artifact
					ProjectRolePermission projectRolePermission = (ProjectRolePermission)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ProjectRolePermission;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
					//historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
					//historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
								foreach (KeyValuePair<string, object> changedField in projectRolePermission.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Deleted);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										ProjectRolePermission convertedProjectRolePermission = (ProjectRolePermission)projectRolePermission;

										object originalValue = changedField.Value;

										if (convertedProjectRolePermission != null)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = convertedProjectRolePermission.ArtifactTypeId;
												ProjectManager pm = new ProjectManager();
												var permissionData = pm.RetrievePermissionById(convertedProjectRolePermission.PermissionId);
												ArtifactManager am = new ArtifactManager();
												var artifactdata = am.ArtifactType_RetrieveById(convertedProjectRolePermission.ArtifactTypeId);
												string oldValueString = permissionData.Name + " Removed from " + artifactdata.Name;

												//if (oldValueString == "Y")
												//{
												//	oldValueString = "True";
												//}
												//else if (oldValueString == "N")
												//{
												//	oldValueString = "False";
												//}
												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = null;
												historyDetail.OLD_VALUE_DATE = null;
												historyDetail.NEW_VALUE = null;    //The string representation
												historyDetail.NEW_VALUE_INT = null;
												historyDetail.NEW_VALUE_DATE = null;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								foreach (KeyValuePair<string, ObjectList> changedField in projectRolePermission.ProjectRole.EntityChangeTracker.ObjectsAddedToCollectionProperties)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Added);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										ProjectRolePermission convertedProjectRolePermission = (ProjectRolePermission)projectRolePermission;

										object originalValue = changedField.Value;

										if (convertedProjectRolePermission != null)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = convertedProjectRolePermission.ArtifactTypeId;
												ProjectManager pm = new ProjectManager();
												var permissionData = pm.RetrievePermissionById(convertedProjectRolePermission.PermissionId);
												ArtifactManager am = new ArtifactManager();
												var artifactdata = am.ArtifactType_RetrieveById(convertedProjectRolePermission.ArtifactTypeId);
												string newValueString = permissionData.Name + " Assigned to " + artifactdata.Name;

												//if (oldValueString == "Y")
												//{
												//	oldValueString = "True";
												//}
												//else if (oldValueString == "N")
												//{
												//	oldValueString = "False";
												//}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = null;          //The string representation
												historyDetail.OLD_VALUE_INT = null;
												historyDetail.OLD_VALUE_DATE = null;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = null;
												historyDetail.NEW_VALUE_DATE = null;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in projectRolePermission.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectRolePermission.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectRolePermission.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectRolePermission);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is Filetype)
				{
					//Cast to an artifact
					Filetype filetype = (Filetype)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.FileTypeIcon;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in filetype.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = filetype.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in filetype.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(filetype);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is VersionControlSystem)
				{
					//Cast to an artifact
					VersionControlSystem versionControlSystem = (VersionControlSystem)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.SourceCode;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in versionControlSystem.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = versionControlSystem.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in versionControlSystem.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(versionControlSystem);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is VersionControlProject)
				{
					//Cast to an artifact
					VersionControlProject versionControlProject = (VersionControlProject)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.SourceCode;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
								historyChangeSets.Add(historyChangeSet);
								historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
								historyChangeSet.ADMIN_USER_ID = changerId;
								historyChangeSet.ARTIFACT_ID = artifactId;
								historyChangeSet.ARTIFACT_GUID_ID = guidId;
								historyChangeSet.ACTION_DESCRIPTION = action;
								historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
								historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Added);

								//Inserts are tracked separately using the LogCreation() function
								ProjectManager pm = new ProjectManager();
								var roleData = pm.RetrieveProjectById(versionControlProject.ProjectId);
								string newValueString = roleData.Name + " Assigned to Source Code";

								TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
								historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
								historyDetail.ADMIN_USER_ID = changerId;
								historyDetail.ADMIN_ARTIFACT_FIELD_NAME = "User Membership";   //Field Name
								historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = "User Membership";     //Field's Caption
								historyDetail.OLD_VALUE = null;          //The string representation
								historyDetail.OLD_VALUE_INT = null;
								historyDetail.OLD_VALUE_DATE = null;
								historyDetail.NEW_VALUE = newValueString;    //The string representation
								historyDetail.NEW_VALUE_INT = null;
								historyDetail.NEW_VALUE_DATE = null;
								historyDetail.ADMIN_ARTIFACT_FIELD_ID = versionControlProject.ProjectId;   //The FieldID, 
								historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in versionControlProject.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = versionControlProject.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in versionControlProject.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(versionControlProject);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is DataSyncSystem)
				{
					//Cast to an artifact
					DataSyncSystem dataSyncSystem = (DataSyncSystem)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.DataSync;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in dataSyncSystem.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = dataSyncSystem.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in dataSyncSystem.GetType().GetProperties())
										{//Propery info name differnt .that why ia added if condiion.
											if(propertyInfo.Name== "ExternalPassword" && fieldName== "EncryptedPassword")
											{
												currentValue = (Object)propertyInfo.GetValue(dataSyncSystem);
												break;
											}
											else if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(dataSyncSystem);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is AutomationEngine)
				{
					//Cast to an artifact
					AutomationEngine automationEngine = (AutomationEngine)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.AutomationEngine;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in automationEngine.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);


									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = automationEngine.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in automationEngine.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(automationEngine);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is Report)
				{
					//Cast to an artifact
					Report report = (Report)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.Report;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in report.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = report.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in report.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(report);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ReportSectionInstance)
				{
					//Cast to an artifact
					ReportSectionInstance reportSectionInstance = (ReportSectionInstance)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ReportSectionInstance;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					////int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, ObjectList> changedField in reportSectionInstance.Report.EntityChangeTracker.ObjectsAddedToCollectionProperties)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Added);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										ReportSectionInstance convertedReportSectionInstance = (ReportSectionInstance)reportSectionInstance;

										object originalValue = changedField.Value;

										if (convertedReportSectionInstance != null)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = convertedReportSectionInstance.ReportSectionId;
												ReportManager rm = new ReportManager();
												var reportData = rm.ReportSection_RetrieveById(convertedReportSectionInstance.ReportSectionId);
												ArtifactManager am = new ArtifactManager();
												string newValueString = reportData.Name + " Assigned to Standard section";

												//if (oldValueString == "Y")
												//{
												//	oldValueString = "True";
												//}
												//else if (oldValueString == "N")
												//{
												//	oldValueString = "False";
												//}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = null;          //The string representation
												historyDetail.OLD_VALUE_INT = null;
												historyDetail.OLD_VALUE_DATE = null;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = null;
												historyDetail.NEW_VALUE_DATE = null;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in reportSectionInstance.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = reportSectionInstance.ChangeTracker.OriginalValues[fieldName];

										foreach (PropertyInfo propertyInfo in reportSectionInstance.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(reportSectionInstance);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												if (artifactField.Name == "Template")
												{
													var comparer = new Comparer(System.Globalization.CultureInfo.CurrentCulture);
												}

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = "Template Field is updated";    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is ReportCustomSection)
				{
					//Cast to an artifact
					ReportCustomSection reportCustomSection = (ReportCustomSection)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ReportCustomSection;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								string fieldName = reportCustomSection.Name;
								try
								{
									//ReportCustomSectionId
									string newValueString = fieldName + " Assigned to Custom section";

									ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name =="ReportCustomSectionId");
									if (artifactField != null && artifactField.IsHistoryRecorded)
									{
										TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
										historyChangeSets.Add(historyChangeSet);
										historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
										historyChangeSet.ADMIN_USER_ID = changerId;
										historyChangeSet.ARTIFACT_ID = artifactId;
										historyChangeSet.ARTIFACT_GUID_ID = guidId;
										historyChangeSet.ACTION_DESCRIPTION = action;
										historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
										historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Added);

										TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
										historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
										historyDetail.ADMIN_USER_ID = changerId;
										historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
										historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;
										historyDetail.OLD_VALUE = null;          //The string representation
										historyDetail.OLD_VALUE_INT = null;
										historyDetail.OLD_VALUE_DATE = null;
										historyDetail.NEW_VALUE = newValueString;    //The string representation
										historyDetail.NEW_VALUE_INT = null;
										historyDetail.NEW_VALUE_DATE = null;
										historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;
										historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;
										//The custom property id, NULL since not a custom property
									}

								}
								catch (Exception ex)
								{
									Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
									throw ex;
								}
							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in reportCustomSection.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = reportCustomSection.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in reportCustomSection.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(reportCustomSection);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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
				else if (entry.Entity is GraphCustom)
				{
					//Cast to an artifact
					GraphCustom graphCustom = (GraphCustom)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.Graph;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					//string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//var artifact = artifactFields.FirstOrDefault();
					//int artifactId1 = artifact.ArtifactTypeId;
					//string artifactName = "";

					////Get the template associated with the project
					//if (String.IsNullOrEmpty(artifactNameField))
					//{
					//	//See if it has the field 'Name' available
					//	if (artifact.ContainsProperty("Name"))
					//	{
					//		artifactName = (string)artifact["Name"];
					//	}
					//}
					//else
					//{
					//	artifactName = (string)artifact[artifactNameField];
					//}

					//Create the HistoryChangeSet first..
					//TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
					//historyChangeSets.Add(historyChangeSet);
					//historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
					//historyChangeSet.ADMIN_USER_ID = changerId;
					//historyChangeSet.ARTIFACT_ID = artifactId;
					//historyChangeSet.ARTIFACT_GUID_ID = guidId;
					//historyChangeSet.ACTION_DESCRIPTION = action;
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
								foreach (KeyValuePair<string, object> changedField in graphCustom.ChangeTracker.OriginalValues)
								{
									TST_ADMIN_HISTORY_CHANGESET historyChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
									historyChangeSets.Add(historyChangeSet);
									historyChangeSet.ADMIN_SECTION_ID = adminSectionId;
									historyChangeSet.ADMIN_USER_ID = changerId;
									historyChangeSet.ARTIFACT_ID = artifactId;
									historyChangeSet.ARTIFACT_GUID_ID = guidId;
									historyChangeSet.ACTION_DESCRIPTION = action;
									historyChangeSet.CHANGE_DATE = DateTime.UtcNow;
									historyChangeSet.HISTORY_CHANGESET_TYPE_ID = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);

									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = graphCustom.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in graphCustom.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(graphCustom);
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
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
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

												TST_ADMIN_HISTORY_DETAILS historyDetail = new TST_ADMIN_HISTORY_DETAILS();
												historyChangeSet.TST_ADMIN_HISTORY_DETAILS.Add(historyDetail);
												historyDetail.ADMIN_USER_ID = changerId;
												historyDetail.ADMIN_ARTIFACT_FIELD_NAME = artifactField.Name;   //Field Name
												historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField.Caption;     //Field's Caption
												historyDetail.OLD_VALUE = oldValueString;          //The string representation
												historyDetail.OLD_VALUE_INT = oldValueInt;
												historyDetail.OLD_VALUE_DATE = oldValueDateTime;
												historyDetail.NEW_VALUE = newValueString;    //The string representation
												historyDetail.NEW_VALUE_INT = newValueInt;
												historyDetail.NEW_VALUE_DATE = newValueDateTime;
												historyDetail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.ADMIN_CUSTOM_PROPERTY_ID = null;       //The custom property id, NULL since not a custom property
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

		private string ParseXMLData(string data)
		{
			string result = string.Empty;

			result = data.Replace("<th>", " ");
			result = result.Replace("</th>", " ");

			var reg = new Regex("\".*?\"");
			var matches = reg.Matches(result);
			foreach (var item in matches)
			{
				result = string.Empty;
				result += item + "\\n";
			}

			return result;
		}

		public bool XMLCompare(XElement primary, XElement secondary)
		{
			if (primary.HasAttributes)
			{
				if (primary.Attributes().Count() != secondary.Attributes().Count())
					return false;
				foreach (XAttribute attr in primary.Attributes())
				{
					if (secondary.Attribute(attr.Name.LocalName) == null)
						return false;
					if (attr.Value.ToLower() != secondary.Attribute(attr.Name.LocalName).Value.ToLower())
						return false;
				}
			}
			if (primary.HasElements)
			{
				if (primary.Elements().Count() != secondary.Elements().Count())
					return false;
				for (var i = 0; i <= primary.Elements().Count() - 1; i++)
				{
					if (XMLCompare(primary.Elements().Skip(i).Take(1).Single(), secondary.Elements().Skip(i).Take(1).Single()) == false)
						return false;
				}
			}
			return true;
		}

		/// <summary>Gets final data from the Object State Manager and saved the records to the history tables.</summary>
		/// <param name="historyChangeSets">The history changesets and associated change details</param>
		internal void LogHistoryAction_End(List<TST_ADMIN_HISTORY_CHANGESET> adminHistoryChangeSets)
		{
			const string METHOD_NAME = CLASS_NAME + "LogHistoryAction_End()";
			Logger.LogEnteringEvent(METHOD_NAME);
			try
			{
				//Save the dataset.
				//Insert1(historyChangeSets);

				long adminHistoryChangeSetId = 0;
				if (adminHistoryChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (TST_ADMIN_HISTORY_CHANGESET adminHistoryChangeSet in adminHistoryChangeSets)
						{
							TST_ADMIN_HISTORY_CHANGESET_AUDIT adminHistoryChangesetData = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();

							adminHistoryChangesetData.ADMIN_USER_ID = adminHistoryChangeSet.ADMIN_USER_ID;
							adminHistoryChangesetData.ADMIN_SECTION_ID = adminHistoryChangeSet.ADMIN_SECTION_ID;
							adminHistoryChangesetData.ARTIFACT_ID = adminHistoryChangeSet.ARTIFACT_ID;
							adminHistoryChangesetData.CHANGE_DATE = adminHistoryChangeSet.CHANGE_DATE;
							adminHistoryChangesetData.HISTORY_CHANGESET_TYPE_ID = adminHistoryChangeSet.HISTORY_CHANGESET_TYPE_ID;
							adminHistoryChangesetData.ACTION_DESCRIPTION = adminHistoryChangeSet.ACTION_DESCRIPTION;
							adminHistoryChangesetData.ARTIFACT_GUID_ID = adminHistoryChangeSet.ARTIFACT_GUID_ID;

							context.TST_ADMIN_HISTORY_CHANGESET_AUDIT.Add(adminHistoryChangesetData);

						}
						context.SaveChanges();

						adminHistoryChangeSetId = context.TST_ADMIN_HISTORY_CHANGESET_AUDIT.OrderByDescending(q => q.CHANGESET_ID)
.FirstOrDefault().CHANGESET_ID;

						foreach (TST_ADMIN_HISTORY_CHANGESET adminHistoryChangeSet in adminHistoryChangeSets)
						{
							TST_ADMIN_HISTORY_DETAILS_AUDIT historyDetail = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
							var historyDetail1 = adminHistoryChangeSet.TST_ADMIN_HISTORY_DETAILS;
							//historyChangeSet.Details.Add(historyDetail);
							foreach (var data in historyDetail1)
							{
								historyDetail.ADMIN_CHANGESET_ID = adminHistoryChangeSetId;
								historyDetail.ADMIN_ARTIFACT_FIELD_NAME = data.ADMIN_ARTIFACT_FIELD_NAME;    //Field Name
								historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = data.ADMIN_ARTIFACT_FIELD_CAPTION;       //Field's Caption
								historyDetail.OLD_VALUE = data.OLD_VALUE;          //The string representation
								historyDetail.OLD_VALUE_INT = data.OLD_VALUE_INT;
								historyDetail.OLD_VALUE_DATE = data.OLD_VALUE_DATE;
								historyDetail.NEW_VALUE = data.NEW_VALUE;    //The string representation
								historyDetail.NEW_VALUE_INT = data.NEW_VALUE_INT;
								historyDetail.NEW_VALUE_DATE = data.NEW_VALUE_DATE;
								historyDetail.ADMIN_ARTIFACT_FIELD_ID = data.ADMIN_ARTIFACT_FIELD_ID;
								historyDetail.ADMIN_USER_ID = data.ADMIN_USER_ID;
								historyDetail.ADMIN_PROPERTY_NAME = data.ADMIN_PROPERTY_NAME;

								context.TST_ADMIN_HISTORY_DETAILS_AUDIT.Add(historyDetail);
							}
						}
						context.SaveChanges();

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
		internal long LogCreation1(int userId, int adminSectionId, int artifactId, string action, TST_ADMIN_HISTORY_DETAILS_AUDIT details, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, string fieldName = null)
		{
			const string METHOD_NAME = "LogCreation()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new changeset.
			TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
			hsChangeSet.ADMIN_USER_ID = userId;
			hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
			hsChangeSet.CHANGE_DATE = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Added;
			hsChangeSet.ACTION_DESCRIPTION = action;
			hsChangeSet.ARTIFACT_ID = artifactId;
			//historyChangeSet.ARTIFACT_GUID_ID = guidId;

			long changeSetId = Insert1(hsChangeSet);

			ArtifactField artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, fieldName);

			//Create a new AdminHistory Details.
			TST_ADMIN_HISTORY_DETAILS_AUDIT hsDetails = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
			hsDetails.ADMIN_USER_ID = userId;
			if (artifactField1 != null)
			{
				hsDetails.ADMIN_ARTIFACT_FIELD_ID = artifactField1.ArtifactFieldId;
				hsDetails.ADMIN_ARTIFACT_FIELD_NAME = artifactField1.Name;
				hsDetails.ADMIN_ARTIFACT_FIELD_CAPTION = artifactField1.Name;
			}
			hsDetails.NEW_VALUE = details.NEW_VALUE;
			hsDetails.ADMIN_CHANGESET_ID = changeSetId;
			
			hsDetails.ADMIN_PROPERTY_NAME = details.ADMIN_PROPERTY_NAME;

			long detailId = DetailInsert1(hsDetails);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		//internal long LogCreation(int userId, int adminSectionId, int artifactId, string action, TST_ADMIN_HISTORY_DETAILS details, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, string fieldName = null)
		//{
		//	const string METHOD_NAME = "LogCreation()";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	//Create a new changeset.
		//	TST_ADMIN_HISTORY_CHANGESET hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
		//	hsChangeSet.ADMIN_USER_ID = userId;
		//	hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
		//	hsChangeSet.CHANGE_DATE = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
		//	hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Added;
		//	hsChangeSet.ACTION_DESCRIPTION = action;
		//	hsChangeSet.ARTIFACT_ID = artifactId;
		//	//historyChangeSet.ARTIFACT_GUID_ID = guidId;

		//	long changeSetId = Insert(hsChangeSet);

		//	ArtifactField artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, fieldName);

		//	//Create a new AdminHistory Details.
		//	TST_ADMIN_HISTORY_DETAILS hsDetails = new TST_ADMIN_HISTORY_DETAILS();
		//	hsDetails.ADMIN_USER_ID = userId;
		//	hsDetails.ADMIN_ARTIFACT_FIELD_ID = artifactField1.ArtifactFieldId;
		//	hsDetails.ADMIN_ARTIFACT_FIELD_NAME = artifactField1.Name;
		//	hsDetails.NEW_VALUE = details.NEW_VALUE;
		//	hsDetails.ADMIN_CHANGESET_ID = changeSetId;

		//	long detailId = DetailInsert(hsDetails);

		//	Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//	return changeSetId;
		//}


		///// <summary>Logs a deletion into the ChangeSet for the specified artifact id, type.</summary>
		///// <param name="userId">The userid who performed the deletion.</param>
		///// <param name="artType">The artifact type.</param>
		///// <param name="artifactId">The artifact ID.</param>
		///// <param name="changeDate">The date of the deletion. If null, uses current date/time.</param>
		///// <returns>The ID of the changeset.</returns>
		//internal long LogDeletion(int userId, int artifactId, string name, int adminSectionId, string action, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, string fieldName = null)
		//{
		//	const string METHOD_NAME = "LogDeletion()";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	//Create a new changeset.
		//	TST_ADMIN_HISTORY_CHANGESET hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET();
		//	hsChangeSet.ADMIN_USER_ID = userId;
		//	hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
		//	hsChangeSet.CHANGE_DATE = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
		//	hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Deleted;
		//	hsChangeSet.ACTION_DESCRIPTION = action;

		//	hsChangeSet.ARTIFACT_ID = artifactId;

		//	long changeSetId = Insert(hsChangeSet);

		//	ArtifactField artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, fieldName);

		//	//Create a new AdminHistory Details.
		//	TST_ADMIN_HISTORY_DETAILS hsDetails = new TST_ADMIN_HISTORY_DETAILS();
		//	hsDetails.ADMIN_USER_ID = userId;
		//	hsDetails.ADMIN_ARTIFACT_FIELD_ID = artifactField1.ArtifactFieldId;
		//	hsDetails.ADMIN_ARTIFACT_FIELD_NAME = artifactField1.Name;
		//	hsDetails.NEW_VALUE = name + " Deleted";
		//	hsDetails.ADMIN_CHANGESET_ID = changeSetId;

		//	long detailId = DetailInsert(hsDetails);

		//	Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//	return changeSetId;
		//}

		public long LogDeletion1(int userId, int artifactId, string name, int adminSectionId, string action, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, string fieldName = null)
		{
			const string METHOD_NAME = "LogDeletion()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new changeset.
			TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
			hsChangeSet.ADMIN_USER_ID = userId;
			hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
			hsChangeSet.CHANGE_DATE = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Deleted;
			hsChangeSet.ACTION_DESCRIPTION = action;

			hsChangeSet.ARTIFACT_ID = artifactId;

			long changeSetId = Insert1(hsChangeSet);

			ArtifactField artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, fieldName);

			//Create a new AdminHistory Details.
			TST_ADMIN_HISTORY_DETAILS_AUDIT hsDetails = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
			hsDetails.ADMIN_USER_ID = userId;
			hsDetails.ADMIN_ARTIFACT_FIELD_ID = artifactField1.ArtifactFieldId;
			hsDetails.ADMIN_ARTIFACT_FIELD_NAME = artifactField1.Name;
			if (action == "Cleared Cache")
			{
				hsDetails.NEW_VALUE = name + " - Cleared Cache";
			}
			else
			{
				hsDetails.NEW_VALUE = name + " Deleted";
			}
			if (action == "Cleared Cache")
			{
				hsDetails.OLD_VALUE = name;
			}
			else
			{
				hsDetails.OLD_VALUE = name;
			}

			hsDetails.ADMIN_CHANGESET_ID = changeSetId;

			long detailId = DetailInsert1(hsDetails);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}


		#endregion Internal Methods

		/// <summary>
		/// Logs a single change set
		/// </summary>
		/// <param name="adminHistoryChangeSet">A single changeset</param>
		/// <returns></returns>
		public long Insert1(TST_ADMIN_HISTORY_CHANGESET_AUDIT adminHistoryChangeSet)
		{
			List<TST_ADMIN_HISTORY_CHANGESET_AUDIT> adminHistoryChangeSets = new List<TST_ADMIN_HISTORY_CHANGESET_AUDIT>();
			adminHistoryChangeSets.Add(adminHistoryChangeSet);
			return Insert1(adminHistoryChangeSets);
		}

		//public long Insert(TST_ADMIN_HISTORY_CHANGESET adminHistoryChangeSet)
		//{
		//	List<TST_ADMIN_HISTORY_CHANGESET> adminHistoryChangeSets = new List<TST_ADMIN_HISTORY_CHANGESET>();
		//	adminHistoryChangeSets.Add(adminHistoryChangeSet);
		//	return Insert(adminHistoryChangeSets);
		//}

		/// <summary>Inserts a History Change set. Needs to have an unsaved ChangeSet row(s), with 0 or more HistoryField rows.</summary>
		/// <param name="adminHistoryChangeSets">The changesets.</param>
		/// <returns>The id of the FIRST ChangeSet inserted.</returns>
		protected long Insert1(List<TST_ADMIN_HISTORY_CHANGESET_AUDIT> adminHistoryChangeSets)
		{
			const string METHOD_NAME = "Insert";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//All we're doing is saving this entity
				long adminHistoryChangeSetId = 0;
				if (adminHistoryChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (TST_ADMIN_HISTORY_CHANGESET_AUDIT adminHistoryChangeSet in adminHistoryChangeSets)
						{
							TST_ADMIN_HISTORY_CHANGESET_AUDIT adminHistoryChangesetData = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();

							adminHistoryChangesetData.ADMIN_USER_ID = adminHistoryChangeSet.ADMIN_USER_ID;
							adminHistoryChangesetData.ADMIN_SECTION_ID = adminHistoryChangeSet.ADMIN_SECTION_ID;
							adminHistoryChangesetData.ARTIFACT_ID = adminHistoryChangeSet.ARTIFACT_ID;
							adminHistoryChangesetData.CHANGE_DATE = adminHistoryChangeSet.CHANGE_DATE;
							adminHistoryChangesetData.HISTORY_CHANGESET_TYPE_ID = adminHistoryChangeSet.HISTORY_CHANGESET_TYPE_ID;
							adminHistoryChangesetData.ACTION_DESCRIPTION = adminHistoryChangeSet.ACTION_DESCRIPTION;
							adminHistoryChangesetData.ARTIFACT_GUID_ID = adminHistoryChangeSet.ARTIFACT_GUID_ID;

							context.TST_ADMIN_HISTORY_CHANGESET_AUDIT.Add(adminHistoryChangesetData);

						}
						context.SaveChanges();

						adminHistoryChangeSetId = context.TST_ADMIN_HISTORY_CHANGESET_AUDIT.OrderByDescending(q => q.CHANGESET_ID)
.FirstOrDefault().CHANGESET_ID;


					}

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return adminHistoryChangeSetId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public long DetailInsert1(TST_ADMIN_HISTORY_DETAILS_AUDIT adminHistoryDetails)
		{
			const string METHOD_NAME = "Insert";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//All we're doing is saving this entity
				long adminHistoryDetailId = 0;
				using (AuditTrailEntities context = new AuditTrailEntities())
				{
					TST_ADMIN_HISTORY_DETAILS_AUDIT historyDetail = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					//historyChangeSet.Details.Add(historyDetail);
					historyDetail.ADMIN_CHANGESET_ID = adminHistoryDetails.ADMIN_CHANGESET_ID;
					historyDetail.ADMIN_ARTIFACT_FIELD_NAME = adminHistoryDetails.ADMIN_ARTIFACT_FIELD_NAME;    //Field Name
					historyDetail.ADMIN_ARTIFACT_FIELD_CAPTION = adminHistoryDetails.ADMIN_ARTIFACT_FIELD_CAPTION;       //Field's Caption
					historyDetail.OLD_VALUE = adminHistoryDetails.OLD_VALUE;          //The string representation
					historyDetail.OLD_VALUE_INT = adminHistoryDetails.OLD_VALUE_INT;
					historyDetail.OLD_VALUE_DATE = adminHistoryDetails.OLD_VALUE_DATE;
					historyDetail.NEW_VALUE = adminHistoryDetails.NEW_VALUE;    //The string representation
					historyDetail.NEW_VALUE_INT = adminHistoryDetails.NEW_VALUE_INT;
					historyDetail.NEW_VALUE_DATE = adminHistoryDetails.NEW_VALUE_DATE;
					historyDetail.ADMIN_ARTIFACT_FIELD_ID = adminHistoryDetails.ADMIN_ARTIFACT_FIELD_ID;
					historyDetail.ADMIN_USER_ID = adminHistoryDetails.ADMIN_USER_ID;
					historyDetail.ADMIN_PROPERTY_NAME = adminHistoryDetails.ADMIN_PROPERTY_NAME;

					context.TST_ADMIN_HISTORY_DETAILS_AUDIT.Add(historyDetail);
					context.SaveChanges();


					//context.AdminHistoryDetails.AddObject(adminHistoryDetails);

					//context.SaveChanges();

					adminHistoryDetailId = context.TST_ADMIN_HISTORY_DETAILS_AUDIT.OrderByDescending(q => q.ADMIN_HISTORY_DETAIL_ID)
.FirstOrDefault().ADMIN_HISTORY_DETAIL_ID;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return adminHistoryDetailId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		//protected long Insert(List<TST_ADMIN_HISTORY_CHANGESET> adminHistoryChangeSets)
		//{
		//	const string METHOD_NAME = "Insert";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	try
		//	{
		//		//All we're doing is saving this entity
		//		long adminHistoryChangeSetId = 0;
		//		if (adminHistoryChangeSets.Count > 0)
		//		{
		//			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//			{
		//				foreach (TST_ADMIN_HISTORY_CHANGESET adminHistoryChangeSet in adminHistoryChangeSets)
		//				{
		//					context.AdminHistoryChangesets.AddObject(adminHistoryChangeSet);

		//				}
		//				context.AdminSaveChanges();

		//				adminHistoryChangeSetId = adminHistoryChangeSets.First().CHANGESET_ID;
		//			}
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		return adminHistoryChangeSetId;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		throw;
		//	}
		//}

		//public long DetailInsert(TST_ADMIN_HISTORY_DETAILS adminHistoryDetails)
		//{
		//	const string METHOD_NAME = "Insert";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	try
		//	{
		//		//All we're doing is saving this entity
		//		long adminHistoryDetailId = 0;
		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			context.AdminHistoryDetails.AddObject(adminHistoryDetails);

		//			context.AdminSaveChanges();

		//			adminHistoryDetailId = adminHistoryDetails.ADMIN_HISTORY_DETAIL_ID;
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		return adminHistoryDetailId;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		throw;
		//	}
		//}

		/// <summary></summary>
		/// <param name="adminHistoryDetailId"></param>
		/// <returns></returns>
		//protected internal TST_ADMIN_HISTORY_DETAILS AdminHistoryDetail_Retrieve_ById(long adminHistoryDetailId)
		//{
		//	const string METHOD_NAME = "AdminHistoryDetail_Retrieve_ById()";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	try
		//	{
		//		TST_ADMIN_HISTORY_DETAILS historyDetail;
		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			var query = from h in context.AdminHistoryDetails
		//						where h.ADMIN_HISTORY_DETAIL_ID == adminHistoryDetailId
		//						select h;

		//			historyDetail = query.FirstOrDefault();
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		return historyDetail;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		Logger.Flush();
		//		throw;
		//	}
		//}

		public List<AdminHistoryChangeSetResponse> RetrieveAdminHistoryChangeSets(double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AdminHistoryChangeSetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.AdminHistoryChangesView
								select new AdminHistoryChangeSetResponse
								{
									ActionDescription = h.ACTION_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = (int)h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = (int)h.ADMIN_USER_ID,
									AdminSectionName = h.ADMIN_SECTION_NAME,
									FieldId = h.ADMIN_ARTIFACT_FIELD_ID,
									FieldName = h.ADMIN_ARTIFACT_FIELD_NAME
								};

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<AdminHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<AdminHistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<AdminHistoryChangeSetResponse>)query.Where(filterClause);
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

		/// <summary>Retrieves a single changeset by its id along with associated change records</summary>
		/// <param name="changeSetId">The ID of the changeset</param>
		/// <param name="includeDetails">Should we include the details</param>
		/// <returns>A history changeset</returns>
		//public TST_ADMIN_HISTORY_CHANGESET RetrieveAdminAuditChangeSetById(long changeSetId, bool includeDetails)
		//{
		//	const string METHOD_NAME = "RetrieveChangeSetById()";
		//	Logger.LogEnteringEvent(METHOD_NAME);

		//	try
		//	{
		//		TST_ADMIN_HISTORY_CHANGESET changeSet;
		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			ObjectQuery<TST_ADMIN_HISTORY_CHANGESET> historyChangeSets = context.AdminHistoryChangesets
		//				//.Include(h => h.Type)
		//				//.Include(h => h.Artifact)
		//				.Include(h => h.TST_USER)
		//				.Include(h => h.TST_USER.Profile);


		//			changeSet = historyChangeSets.FirstOrDefault(h => h.CHANGESET_ID == changeSetId);
		//		}

		//		Logger.LogExitingEvent(METHOD_NAME);
		//		return changeSet;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(METHOD_NAME, exception);
		//		Logger.Flush();
		//		throw;
		//	}
		//}


		public int CountSet(int? projectId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountSet()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.AdminHistoryChangesView
								select new AdminHistoryChangeSetResponse
								{
									ActionDescription = h.ACTION_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = (int)h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = (int)h.ADMIN_USER_ID,
									AdminSectionName = h.ADMIN_SECTION_NAME,
									FieldId = h.ADMIN_ARTIFACT_FIELD_ID,
									FieldName = h.ADMIN_ARTIFACT_FIELD_NAME
								};

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<AdminHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<AdminHistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<AdminHistoryChangeSetResponse>)query.Where(filterClause);
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
		public List<AdminHistoryChangeSetResponse> RetrieveByChangeSetId(int projectId, long changeSetId, string sortProperty, bool sortAscending, Hashtable filterList, int startRow, int paginationSize, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveByChangeSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AdminHistoryChangeSetResponse> historyChanges;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.AdminHistoryChangesView
								select new AdminHistoryChangeSetResponse
								{
									ActionDescription = h.ACTION_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = (int)h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = (int)h.ADMIN_USER_ID,
									AdminSectionName = h.ADMIN_SECTION_NAME,
									FieldId = h.ADMIN_ARTIFACT_FIELD_ID,
									FieldName = h.ADMIN_ARTIFACT_FIELD_NAME
								};

					if (changeSetId > 1)
					{
						query = query.Where(h => h.ChangeSetId == changeSetId);
					}

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<AdminHistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<AdminHistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<AdminHistoryChangeSetResponse>)query.Where(filterClause);
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

		public AdminHistoryChangeSetResponse RetrieveChangeSetById(long changeSetId, bool includeDetails, bool includeAssociations = false, bool includePositions = false)
		{
			const string METHOD_NAME = "RetrieveChangeSetById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				AdminHistoryChangeSetResponse changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//ObjectQuery<TST_ADMIN_HISTORY_CHANGESET_AUDIT> historyChangeSets = context.TST_ADMIN_HISTORY_CHANGESET_AUDIT
					//	.Include(h => h.TST_HISTORY_CHANGESET_TYPE)
					//.Include(h => h.TST_USER);


					var query = from h in context.AdminHistoryChangesView
								select new AdminHistoryChangeSetResponse
								{
									ActionDescription = h.ACTION_DESCRIPTION,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									TimeZone = "UTC",
									ChangeTypeId = (int)h.HISTORY_CHANGESET_TYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									UserName = h.USER_NAME,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									Time = "",
									UserId = (int)h.ADMIN_USER_ID,
									AdminSectionName = h.ADMIN_SECTION_NAME,
									FieldId = h.ADMIN_ARTIFACT_FIELD_ID,
									FieldName = h.ADMIN_ARTIFACT_FIELD_NAME,
									ARTIFACT_ID = h.ARTIFACT_ID,
									ARTIFACT_GUID_ID=h.ARTIFACT_GUID_ID

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
