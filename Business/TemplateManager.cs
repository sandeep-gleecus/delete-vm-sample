using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// Manages project templates in the system
	/// </summary>
	public class TemplateManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TemplateManager::";

		private static ConcurrentDictionary<int, ProjectTemplate> cachedLookup = new ConcurrentDictionary<int, ProjectTemplate>();

		private const int REMAP_TIMEOUT_SECONDS = 120 * 60 * 60; //120 minutes

		/// <summary>
		/// Overrides the default timeout for transactions as set by machine.config
		/// </summary>
		/// <param name="timeOut">The new default timeout value</param>
		/// <remarks>
		/// https://blogs.msdn.microsoft.com/ajit/2008/06/18/override-the-system-transactions-default-timeout-of-10-minutes-in-the-code/
		/// </remarks>
		private void OverrideTransactionScopeMaximumTimeout(TimeSpan timeOut)
		{
			// 1. create a object of the type specified by the fully qualified name

			Type oSystemType = typeof(global::System.Transactions.TransactionManager);
			System.Reflection.FieldInfo oCachedMaxTimeout = oSystemType.GetField("_cachedMaxTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo oMaximumTimeout = oSystemType.GetField("_maximumTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			oCachedMaxTimeout.SetValue(null, true);
			oMaximumTimeout.SetValue(null, timeOut);

			// For testing to confirm value was changed
			// MessageBox.Show(string.Format(&quot;DEBUG SUCCESS!! &nbsp;Maximum Timeout for transactions is &#39;{0}&#39;&quot;, TransactionManager.MaximumTimeout.ToString()));        
		}

		/// <summary>
		/// Checks what standard fields will lose data if a project changes template
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="newTemplateId">The id of the new template</param>
		public List<TemplateRemapStandardFieldsInfo> RetrieveStandardFieldMappingInformation(int projectId, int newTemplateId)
		{
			const string METHOD_NAME = "RetrieveStandardFieldMappingInformation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure the project exists and retrieve it to get the current template ID
				Project project = new ProjectManager().RetrieveById(projectId);
				if (project == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Project_ArtifactNotExists, projectId));
				}
				int oldTemplateId = project.ProjectTemplateId;

				List<TemplateRemapStandardFieldsInfo> fieldInfo;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					fieldInfo = context.Template_RemapStandardFieldsInformation(projectId, oldTemplateId, newTemplateId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return fieldInfo;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Changes the template used by a project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="newTemplateId">The id of the new template</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <param name="userId">The ID of the user making the change</param>
		/// <remarks>
		/// Not only does it change the template being used, but ensures that the various IDs in the project are correct after the move
		/// </remarks>
		public void ChangeProjectTemplate(int projectId, int newTemplateId, int userId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "ChangeProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGE_SIZE = 50;

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.Template_Remap_Starting);
				}

				//First retrieve the project and template to make sure they exist
				ProjectTemplate newTemplate = this.RetrieveById(newTemplateId);
				if (newTemplate == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.ProjectTemplate_NotExists, newTemplateId));
				}
				if (!newTemplate.IsActive)
				{
					throw new ApplicationException(String.Format(GlobalResources.Messages.ProjectTemplate_NotActive, newTemplateId));
				}

				Project project = new ProjectManager().RetrieveById(projectId);
				if (project == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Project_ArtifactNotExists, projectId));
				}
				int oldTemplateId = project.ProjectTemplateId;

				//If the templates are the same, just log a warning and end
				if (oldTemplateId == newTemplateId)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.ProjectTemplate_ProjectTemplateIdTheSame);
					return;
				}

				//Log that we are changing the project template
				Logger.LogSuccessAuditEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Template_Remap_StartingInfo, projectId, oldTemplateId, newTemplateId), true);

				//Instantiate other managers
				ArtifactManager artifactManager = new ArtifactManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				HistoryManager historyManager = new HistoryManager();
				DataMappingManager dataMappingManager = new DataMappingManager();

				//Override the default maximum transaction timeout value
				TimeSpan scopeTimeOut = TimeSpan.FromSeconds(REMAP_TIMEOUT_SECONDS);
				OverrideTransactionScopeMaximumTimeout(scopeTimeOut);

				//Start the database context block
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a long timeout
					context.CommandTimeout = REMAP_TIMEOUT_SECONDS;

					//We need to make all the changes in a transaction so that they are not partially done
					using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew, scopeTimeOut))
					{
						//First disable this project for data synchronization if already active
						var query = from d in context.DataSyncProjects
									where d.ProjectId == projectId && d.ActiveYn == "Y"
									orderby d.DataSyncSystemId
									select d;

						List<DataSyncProject> dataSyncProjects = query.ToList();
						foreach (DataSyncProject dataSyncProject in dataSyncProjects)
						{
							dataSyncProject.StartTracking();
							dataSyncProject.ActiveYn = "N";
						}
						context.SaveChanges();

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(10, GlobalResources.Messages.Template_Remap_DataSyncDisabled);
						}

						//Incident Fields
						context.Template_RemapIncidentFields(projectId, oldTemplateId, newTemplateId);

						//Requirement Fields
						context.Template_RemapRequirementFields(projectId, oldTemplateId, newTemplateId);

						//Task Fields
						context.Template_RemapTaskFields(projectId, oldTemplateId, newTemplateId);

						//Test Case Fields
						context.Template_RemapTestCaseFields(projectId, oldTemplateId, newTemplateId);

						//Document fields
						context.Template_RemapDocumentFields(projectId, oldTemplateId, newTemplateId);

						//Risk fields
						context.Template_RemapRiskFields(projectId, oldTemplateId, newTemplateId);

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(20, GlobalResources.Messages.Template_Remap_StandardFields_Completed);
						}

						//Loop through all the artifact fields for custom properties and history records
						List<ArtifactType> artifactTypes = artifactManager.ArtifactType_RetrieveAll(false, false, false, false, true);
						int artifactTypeIndex = 1;
						foreach (ArtifactType artifactType in artifactTypes)
						{
							//Get all the custom properties for this artifact type (old and new template)
							List<CustomProperty> oldCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(oldTemplateId, (Artifact.ArtifactTypeEnum)artifactType.ArtifactTypeId, true, false, true);
							List<CustomProperty> newCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(newTemplateId, (Artifact.ArtifactTypeEnum)artifactType.ArtifactTypeId, true, false, true);

							//Now get all of the artifact custom properties in the project
							List<ArtifactCustomProperty> artifactCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveForArtifactType(projectId, oldTemplateId, (Artifact.ArtifactTypeEnum)artifactType.ArtifactTypeId);

							//Loop through each row
							foreach (ArtifactCustomProperty artifactCustomProperty in artifactCustomProperties)
							{
								//Track changes
								artifactCustomProperty.StartTracking();
								bool changesMade = false;

								//Loop through each of the custom properties in the old template
								foreach (CustomProperty oldCustomProperty in oldCustomProperties.Where(c => c.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || c.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList))
								{
									//Make sure we have the same type and display name in the new template
									int propertyNumber = oldCustomProperty.PropertyNumber;
									CustomProperty newCustomProperty = newCustomProperties
										.FirstOrDefault(c =>
											c.PropertyNumber == propertyNumber &&
											c.Name == oldCustomProperty.Name &&
											c.CustomPropertyTypeId == oldCustomProperty.CustomPropertyTypeId
									);
									if (newCustomProperty != null && newCustomProperty.List != null && oldCustomProperty.List != null)
									{
										//Make sure the lists are the same name
										if (oldCustomProperty.List.Name == newCustomProperty.List.Name)
										{
											//Handle each type

											//List
											if (oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
											{
												if (artifactCustomProperty.CustomProperty(propertyNumber) is Nullable<Int32>)
												{
													int? oldValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
													if (oldValue.HasValue)
													{
														CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == oldValue.Value);
														if (oldCPV != null)
														{
															//Find a list value with the same name
															CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
															if (newCPV != null)
															{
																artifactCustomProperty.SetCustomProperty(propertyNumber, newCPV.CustomPropertyValueId);
																changesMade = true;
															}
														}
													}
												}
											}

											//MultiList
											if (oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
											{
												if (artifactCustomProperty.CustomProperty(propertyNumber) is List<Int32>)
												{
													List<Int32> oldValues = (List<Int32>)artifactCustomProperty.CustomProperty(propertyNumber);
													if (oldValues != null && oldValues.Count > 0)
													{
														List<int> newValues = new List<int>();
														foreach (int oldValue in oldValues)
														{
															CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == oldValue);
															if (oldCPV != null)
															{
																//Find a list value with the same name
																CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
																if (newCPV != null)
																{
																	newValues.Add(newCPV.CustomPropertyValueId);
																}
															}
														}
														if (newValues.Count > 0)
														{
															artifactCustomProperty.SetCustomProperty(propertyNumber, newValues);
															changesMade = true;
														}
													}
												}
											}

										}
									}
								}

								//Save any changes
								if (changesMade)
								{
									//We must not record history or that creates false history records
									customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId, null, false);
								}
							}

							//History

							//First get a list of all the modify history changes in the project for this artifact type
							long totalCount = Int64.MaxValue;
							List<HistoryChangeSet> changeSets = new List<HistoryChangeSet>();
							for (int startRow = 1; (long)startRow < totalCount; startRow += PAGE_SIZE)
							{
								changeSets.AddRange(historyManager.HistoryChangeSet_RetrieveChangesForProjectAndArtifact(projectId, (Artifact.ArtifactTypeEnum)artifactType.ArtifactTypeId, out totalCount, startRow, PAGE_SIZE));
							}

							//Now loop through all of the change records
							foreach (HistoryChangeSet changeSet in changeSets)
							{
								//Loop through each of the detail change records
								foreach (HistoryDetail changeDetailItem in changeSet.Details)
								{
									//Get a fresh copy of the detail to avoid EF4 trying to save the whole changeset
									HistoryDetail changeDetail = historyManager.HistoryDetail_Retrieve_ById(changeDetailItem.ArtifactHistoryId);

									bool changesMade = false;

									//See if we have a custom field
									if (changeDetail.CustomPropertyId.HasValue)
									{
										#region Custom Property

										int customPropertyId = changeDetail.CustomPropertyId.Value;

										//Find the matching property in the old template
										CustomProperty oldCustomProperty = oldCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId);

										//Find the same property in the new template
										if (oldCustomProperty != null)
										{
											int propertyNumber = oldCustomProperty.PropertyNumber;
											CustomProperty newCustomProperty = newCustomProperties.FirstOrDefault(c => c.PropertyNumber == propertyNumber && c.Name == oldCustomProperty.Name && c.CustomPropertyTypeId == oldCustomProperty.CustomPropertyTypeId);
											if (newCustomProperty != null)
											{
												//Update the custom property ID to the one for the new template
												changeDetail.StartTracking();
												changeDetail.CustomPropertyId = newCustomProperty.CustomPropertyId;
												changesMade = true;

												//See if it is a list or multilist
												if (oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
												{
													if (newCustomProperty.List != null && oldCustomProperty.List != null)
													{
														//Make sure the lists are the same name
														if (oldCustomProperty.List.Name == newCustomProperty.List.Name)
														{
															//Handle each type

															//List
															if (oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
															{
																//See if we have match and change the old list value
																int? oldValue = changeDetail.OldValue.FromDatabaseSerialization_Int32();
																if (oldValue.HasValue)
																{
																	CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == oldValue.Value);
																	if (oldCPV != null)
																	{
																		//Find a list value with the same name
																		CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
																		if (newCPV != null)
																		{
																			changeDetail.StartTracking();
																			changeDetail.OldValue = newCPV.CustomPropertyValueId.ToDatabaseSerialization();
																			changeDetail.OldValueInt = newCPV.CustomPropertyValueId;
																			changesMade = true;
																		}
																	}
																}

																//See if we have match and change the new list value
																int? newValue = changeDetail.NewValue.FromDatabaseSerialization_Int32();
																if (newValue.HasValue)
																{
																	CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == newValue.Value);
																	if (oldCPV != null)
																	{
																		//Find a list value with the same name
																		CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
																		if (newCPV != null)
																		{
																			changeDetail.StartTracking();
																			changeDetail.NewValue = newCPV.CustomPropertyValueId.ToDatabaseSerialization();
																			changeDetail.NewValueInt = newCPV.CustomPropertyValueId;
																			changesMade = true;
																		}
																	}
																}
															}

															//MultiList
															if (oldCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
															{
																//See if we have match and change the old list value
																List<int> oldValues = changeDetail.OldValue.FromDatabaseSerialization_List_Int32();
																if (oldValues != null && oldValues.Count > 0)
																{
																	List<int> replacementValues = new List<int>();
																	foreach (int oldValue in oldValues)
																	{
																		CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == oldValue);
																		if (oldCPV != null)
																		{
																			//Find a list value with the same name
																			CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
																			if (newCPV != null)
																			{
																				replacementValues.Add(newCPV.CustomPropertyValueId);
																			}
																		}
																	}
																	if (replacementValues.Count > 0)
																	{
																		changeDetail.StartTracking();
																		changeDetail.OldValue = replacementValues.ToDatabaseSerialization();
																		changesMade = true;
																	}
																}

																//See if we have match and change the new list value
																List<int> newValues = changeDetail.NewValue.FromDatabaseSerialization_List_Int32();
																if (newValues != null && newValues.Count > 0)
																{
																	List<int> replacementValues = new List<int>();
																	foreach (int newValue in newValues)
																	{
																		CustomPropertyValue oldCPV = oldCustomProperty.List.Values.FirstOrDefault(v => v.CustomPropertyValueId == newValue);
																		if (oldCPV != null)
																		{
																			//Find a list value with the same name
																			CustomPropertyValue newCPV = newCustomProperty.List.Values.FirstOrDefault(v => v.Name == oldCPV.Name);
																			if (newCPV != null)
																			{
																				replacementValues.Add(newCPV.CustomPropertyValueId);
																			}
																		}
																	}
																	if (replacementValues.Count > 0)
																	{
																		changeDetail.StartTracking();
																		changeDetail.NewValue = replacementValues.ToDatabaseSerialization();
																		changesMade = true;
																	}
																}
															}
														}
													}
												}
											}
										}

										#endregion
									}
									else if (changeDetail.FieldId.HasValue)
									{
										#region Standard Field

										int artifactFieldId = changeDetail.FieldId.Value;
										RemapHistoryStandardField(projectId, oldTemplateId, newTemplateId, artifactType.ArtifactTypeId, artifactFieldId, changeDetail, ref changesMade);

										#endregion
									}

									//Save any changes
									if (changesMade)
									{
										historyManager.HistoryDetail_Update(changeDetail, userId);
									}
								}
							}

							if (updateBackgroundProcessStatus != null)
							{
								int percentComplete = 20 + ((artifactTypeIndex * 30) / artifactTypes.Count);
								updateBackgroundProcessStatus(percentComplete, String.Format(GlobalResources.Messages.Template_Remap_CustomFieldForArtifactType_Completed, artifactType.Name));
								artifactTypeIndex++;
							}
						}

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(60, GlobalResources.Messages.Template_Remap_CustomFieldsFields_Completed);
						}

						#region Test Configurations

						TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
						testConfigurationManager.RemapToNewTemplate(projectId, oldTemplateId, newTemplateId);

						#endregion

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(80, GlobalResources.Messages.Template_Remap_TestConfigurations_Completed);
						}

						#region Data Synchronization Mappings

						//Clear the data synchronization mappings
						dataMappingManager.DeleteDataSyncProjectMappings(projectId);

						#endregion

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(90, GlobalResources.Messages.Template_Remap_DataSyncMappingsCleared);
						}

						//Clear the user saved view settings
						this.ClearUserViewSettings(context, projectId);

						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(95, GlobalResources.Messages.Template_Remap_UserViewSettingsCleared);
						}

						//Next we need to actually change the project's template id
						context.Projects.Attach(project);
						project.StartTracking();
						project.ProjectTemplateId = newTemplateId;
						context.SaveChanges();

						//Complete the transaction
						transaction.Complete();

						//Finally clear the cache of project/template mappings
						cachedLookup.Clear();
					}
				}

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(100, GlobalResources.Messages.Template_Remap_Completed);
				}

				//Log that we finished changing the project template
				Logger.LogSuccessAuditEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Template_Remap_FinishingInfo, projectId, oldTemplateId, newTemplateId), true);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Clears all the user's view settings for filters, saved filters, sorts, column selection since they are template specific
		/// </summary>
		/// <param name="context">The current database context</param>
		/// <param name="projectId">The id of the project we need to delete them from</param>
		protected void ClearUserViewSettings(SpiraTestEntitiesEx context, int projectId)
		{
			const string METHOD_NAME = "ClearUserViewSettings";

			try
			{
				context.Template_RemapClearUserSettings(projectId);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Remaps a change history detail row's standard fields
		/// </summary>
		/// <param name="projectId">The id of the old project</param>
		/// <param name="oldTemplateId">The id of the existing template</param>
		/// <param name="newTemplateId">The id of the new template</param>
		/// <param name="artifactFieldId">The id of the field</param>
		/// <param name="artifactTypeId">The id of the artifact type</param>
		/// <param name="changeDetail">The history record</param>
		/// <param name="changesMade">Have we made any changes</param>
		/// <remarks>
		/// We only need to deal with lists not multi lists because the only multi-list is Component and that's per project not per template
		/// </remarks>
		protected void RemapHistoryStandardField(int projectId, int oldTemplateId, int newTemplateId, int artifactTypeId, int artifactFieldId, HistoryDetail changeDetail, ref bool changesMade)
		{
			//See if we have a list field, other types don't need to be remapped
			ArtifactManager artifactManager = new ArtifactManager();
			ArtifactField artifactField = artifactManager.ArtifactField_RetrieveById(artifactFieldId);
			if (artifactField != null && artifactField.ArtifactFieldTypeId == (int)Artifact.ArtifactFieldTypeEnum.Lookup)
			{
				//Old Value
				if (changeDetail.OldValueInt.HasValue)
				{
					int oldListValue = changeDetail.OldValueInt.Value;

					//Get the lookup name for this
					string lookupName = artifactManager.GetLookupValue((Artifact.ArtifactTypeEnum)artifactTypeId, projectId, oldTemplateId, artifactField.Name, oldListValue);

					//See if we have the matching name in the new template
					int? oldListValueNewTemplate = artifactManager.GetLookupId((Artifact.ArtifactTypeEnum)artifactTypeId, newTemplateId, artifactField.Name, lookupName);
					if (oldListValueNewTemplate.HasValue)
					{
						changeDetail.StartTracking();
						changesMade = true;
						changeDetail.OldValueInt = oldListValueNewTemplate.Value;
						changeDetail.OldValue = lookupName;
					}
					else
					{
						//We need to blank out the value to prevent it reverting back to a value in the wrong template
						changeDetail.StartTracking();
						changesMade = true;
						changeDetail.OldValueInt = null;
						changeDetail.OldValue = null;
					}
				}

				//New Value
				if (changeDetail.NewValueInt.HasValue)
				{
					int newListValue = changeDetail.NewValueInt.Value;

					//Get the lookup name for this
					string lookupName = artifactManager.GetLookupValue((Artifact.ArtifactTypeEnum)artifactTypeId, projectId, oldTemplateId, artifactField.Name, newListValue);

					//See if we have the matching name in the new template
					int? newListValueNewTemplate = artifactManager.GetLookupId((Artifact.ArtifactTypeEnum)artifactTypeId, newTemplateId, artifactField.Name, lookupName);
					if (newListValueNewTemplate.HasValue)
					{
						changeDetail.StartTracking();
						changesMade = true;
						changeDetail.NewValueInt = newListValueNewTemplate.Value;
						changeDetail.NewValue = lookupName;
					}
					else
					{
						//We need to blank out the value to prevent it reverting back to a value in the wrong template
						changeDetail.StartTracking();
						changesMade = true;
						changeDetail.NewValueInt = null;
						changeDetail.NewValue = null;
					}
				}
			}
		}

		/// <summary>
		/// Is the user authorized to view artifacts that are part of the template (incident types, etc.)
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>True if the user is a member of any project that uses this template</returns>
		public bool IsAuthorizedToViewTemplate(int userId, int projectTemplateId)
		{
			const string METHOD_NAME = "IsAuthorizedToViewTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isAuthorized;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure
					isAuthorized = context.Template_IsAuthorizedToView(userId, projectTemplateId).FirstOrDefault().Value;
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isAuthorized;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Is the user authorized to edit artifacts that are part of the template (incident types, etc.)
		/// as well as edit the template itself
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>True if the user is a Template Admin of any project that uses this template</returns>
		public bool IsAuthorizedToEditTemplate(int userId, int projectTemplateId)
		{
			const string METHOD_NAME = "IsAuthorizedToEditTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isAuthorized;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure
					isAuthorized = context.Template_IsAuthorizedToEdit(userId, projectTemplateId).FirstOrDefault().Value;
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isAuthorized;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of templates that the current user is an owner of
		/// </summary>
		/// <param name="templateAdminUserId">The id of the user</param>
		/// <returns>The list of templates</returns>
		public List<ProjectTemplate> RetrieveTemplatesByAdmin(int templateAdminUserId)
		{
			const string METHOD_NAME = "RetrieveTemplatesByAdmin";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectTemplate> templates;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure
					templates = context.Template_RetrieveByOwner(templateAdminUserId).ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return templates;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new project template in the system
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of an existing template to copy settings from, null = create default template</param>
		/// <param name="name">The name of the template</param>
		/// <param name="description">The description of the template</param>
		/// <param name="active">Is the template active</param>
		/// <returns>The id of the new project template</returns>
		public int Insert(string name, string description, bool active, int? existingProjectTemplateId = null, int? userId = null, int? adminSectionId = null, string action = null)
		{
			//Declare the mapping dictionaries (not used in this overload)
			Dictionary<int, int> incidentStatusMapping;
			Dictionary<int, int> incidentTypeMapping;
			Dictionary<int, int> incidentPriorityMapping;
			Dictionary<int, int> incidentSeverityMapping;
			Dictionary<int, int> requirementTypeMapping;
			Dictionary<int, int> requirementImportanceMapping;
			Dictionary<int, int> taskTypeMapping;
			Dictionary<int, int> taskPriorityMapping;
			Dictionary<int, int> testCaseTypeMapping;
			Dictionary<int, int> testCasePriorityMapping;
			Dictionary<int, int> documentTypeMapping;
			Dictionary<int, int> documentStatusMapping;
			Dictionary<int, int> riskStatusMapping;
			Dictionary<int, int> riskTypeMapping;
			Dictionary<int, int> riskProbabilityMapping;
			Dictionary<int, int> riskImpactMapping;
			Dictionary<int, int> customPropertyIdMapping;
			Dictionary<int, int> propertyValueMapping;

			//Call the more general overload
			return this.Insert(name,
				description,
				active,
				existingProjectTemplateId,
				out incidentStatusMapping,
				out incidentTypeMapping,
				out incidentPriorityMapping,
				out incidentSeverityMapping,
				out requirementTypeMapping,
				out requirementImportanceMapping,
				out taskTypeMapping,
				out taskPriorityMapping,
				out testCaseTypeMapping,
				out testCasePriorityMapping,
				out documentTypeMapping,
				out documentStatusMapping,
				out riskTypeMapping,
				out riskStatusMapping,
				out riskProbabilityMapping,
				out riskImpactMapping,
				out customPropertyIdMapping,
				out propertyValueMapping,
				userId,
				adminSectionId,
				action
				);
		}

		/// <summary>
		/// Creates a new project template in the system
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of an existing template to copy settings from, null = create default template</param>
		/// <param name="name">The name of the template</param>
		/// <param name="description">The description of the template</param>
		/// <param name="active">Is the template active</param>
		/// <returns>The id of the new project template</returns>
		protected internal int Insert(string name, string description, bool active, int? existingProjectTemplateId,
			out Dictionary<int, int> incidentStatusMapping,
			out Dictionary<int, int> incidentTypeMapping,
			out Dictionary<int, int> incidentPriorityMapping,
			out Dictionary<int, int> incidentSeverityMapping,
			out Dictionary<int, int> requirementTypeMapping,
			out Dictionary<int, int> requirementImportanceMapping,
			out Dictionary<int, int> taskTypeMapping,
			out Dictionary<int, int> taskPriorityMapping,
			out Dictionary<int, int> testCaseTypeMapping,
			out Dictionary<int, int> testCasePriorityMapping,
			out Dictionary<int, int> documentTypeMapping,
			out Dictionary<int, int> documentStatusMapping,
			out Dictionary<int, int> riskTypeMapping,
			out Dictionary<int, int> riskStatusMapping,
			out Dictionary<int, int> riskProbabilityMapping,
			out Dictionary<int, int> riskImpactMapping,
			out Dictionary<int, int> customPropertyIdMapping,
			out Dictionary<int, int> propertyValueMapping, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//AdminAudit manager, in case.
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				string newValue = String.Empty;
				int projectTemplateId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new project template
					ProjectTemplate template = new ProjectTemplate();
					template.Name = name;
					template.Description = description;
					template.IsActive = active;

					//Save the new object
					context.ProjectTemplates.AddObject(template);
					context.SaveChanges();

					//Now capture the newly created project template id
					projectTemplateId = template.ProjectTemplateId;
					newValue = template.Name;

					details.NEW_VALUE = newValue;
					//See if we should create the default template entries or copy them from another template
					if (existingProjectTemplateId.HasValue)
					{
						//Next we need to copy across any template settings
						List<ProjectTemplateSettingValue> projectTemplateSettingValues = context.ProjectTemplateSettingValues.Where(p => p.ProjectTemplateId == existingProjectTemplateId.Value).ToList();
						foreach (ProjectTemplateSettingValue projectTemplateSettingValue in projectTemplateSettingValues)
						{
							//Add the setting to the new project
							ProjectTemplateSettingValue newProjectTemplateSettingValue = new ProjectTemplateSettingValue();
							newProjectTemplateSettingValue.ProjectTemplateId = projectTemplateId;
							newProjectTemplateSettingValue.Value = projectTemplateSettingValue.Value;
							newProjectTemplateSettingValue.ProjectTemplateSettingId = projectTemplateSettingValue.ProjectTemplateSettingId;
							template.Settings.Add(newProjectTemplateSettingValue);
						}
						context.SaveChanges();
					}
				}

				if (userId != null)
				{
					//Log history.
					if (logHistory)
						adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), projectTemplateId, action, details, DateTime.UtcNow, ArtifactTypeEnum.ProjectTemplate, "ProjectTemplateId");
				}

				//Clear the cache
				cachedLookup.Clear();

				//See if we should create the default template entries or copy them from another template
				if (existingProjectTemplateId.HasValue)
				{
					//Now we need to clone the settings from the other template
					CopySettingsFromExistingTemplate(existingProjectTemplateId.Value,
						projectTemplateId,
						out incidentStatusMapping,
						out incidentTypeMapping,
						out incidentPriorityMapping,
						out incidentSeverityMapping,
						out requirementTypeMapping,
						out requirementImportanceMapping,
						out taskTypeMapping,
						out taskPriorityMapping,
						out testCaseTypeMapping,
						out testCasePriorityMapping,
						out documentTypeMapping,
						out documentStatusMapping,
						out riskTypeMapping,
						out riskStatusMapping,
						out riskProbabilityMapping,
						out riskImpactMapping,
						out customPropertyIdMapping,
						out propertyValueMapping
						);
				}
				else
				{
					//Now we need to create the initial set of project lists, workflows and other configurable meta-data

					//Default document fields and workflow
					AttachmentManager attachmentManager = new AttachmentManager();
					attachmentManager.CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Default incident fields and workflow
					IncidentManager incidentManager = new IncidentManager();
					incidentManager.CreateforNewProjectTemplate(projectTemplateId);

					//Notifications events and templates
					NotificationManager notificationManager = new NotificationManager();
					notificationManager.NotificationEvent_CreateforNewProjectTemplate(projectTemplateId);
					notificationManager.NotificationTemplate_CreateforNewProjectTemplate(projectTemplateId);

					//Default requirement fields and workflow
					RequirementManager requirementManager = new RequirementManager();
					requirementManager.CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Default release fields and workflow
					ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();
					releaseWorkflowManager.Workflow_CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Default task fields and workflow
					TaskManager taskManager = new TaskManager();
					taskManager.CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Default test case fields and workflow
					TestCaseManager testCaseManager = new TestCaseManager();
					testCaseManager.CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Default risk fields and workflow
					RiskManager riskManager = new RiskManager();
					riskManager.CreateDefaultEntriesForProjectTemplate(projectTemplateId);

					//Populate empty mappings (not used in this case)
					incidentStatusMapping = new Dictionary<int, int>();
					incidentTypeMapping = new Dictionary<int, int>();
					incidentPriorityMapping = new Dictionary<int, int>();
					incidentSeverityMapping = new Dictionary<int, int>();
					requirementTypeMapping = new Dictionary<int, int>();
					requirementImportanceMapping = new Dictionary<int, int>();
					taskTypeMapping = new Dictionary<int, int>();
					taskPriorityMapping = new Dictionary<int, int>();
					testCaseTypeMapping = new Dictionary<int, int>();
					testCasePriorityMapping = new Dictionary<int, int>();
					documentTypeMapping = new Dictionary<int, int>();
					documentStatusMapping = new Dictionary<int, int>();
					riskStatusMapping = new Dictionary<int, int>();
					riskTypeMapping = new Dictionary<int, int>();
					riskProbabilityMapping = new Dictionary<int, int>();
					riskImpactMapping = new Dictionary<int, int>();
					customPropertyIdMapping = new Dictionary<int, int>();
					propertyValueMapping = new Dictionary<int, int>();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectTemplateId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Copies the various customizations from the old template to the new one
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing template</param>
		/// <param name="newProjectTemplateId">The id of the new template</param>
		protected void CopySettingsFromExistingTemplate(int existingProjectTemplateId, int newProjectTemplateId,
			out Dictionary<int, int> incidentStatusMapping,
			out Dictionary<int, int> incidentTypeMapping,
			out Dictionary<int, int> incidentPriorityMapping,
			out Dictionary<int, int> incidentSeverityMapping,
			out Dictionary<int, int> requirementTypeMapping,
			out Dictionary<int, int> requirementImportanceMapping,
			out Dictionary<int, int> taskTypeMapping,
			out Dictionary<int, int> taskPriorityMapping,
			out Dictionary<int, int> testCaseTypeMapping,
			out Dictionary<int, int> testCasePriorityMapping,
			out Dictionary<int, int> documentTypeMapping,
			out Dictionary<int, int> documentStatusMapping,
			out Dictionary<int, int> riskTypeMapping,
			out Dictionary<int, int> riskStatusMapping,
			out Dictionary<int, int> riskProbabilityMapping,
			out Dictionary<int, int> riskImpactMapping,
			out Dictionary<int, int> customPropertyIdMapping,
			out Dictionary<int, int> propertyValueMapping)
		{
			const string METHOD_NAME = "CopySettingsFromExistingTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the mapping hashtables for storing old vs new IDs

				//Incident Fields
				Dictionary<int, int> incidentWorkflowMapping = new Dictionary<int, int>();
				incidentStatusMapping = new Dictionary<int, int>();
				incidentTypeMapping = new Dictionary<int, int>();
				incidentPriorityMapping = new Dictionary<int, int>();
				incidentSeverityMapping = new Dictionary<int, int>();

				//Requirement Fields
				Dictionary<int, int> requirementWorkflowMapping = new Dictionary<int, int>();
				requirementTypeMapping = new Dictionary<int, int>();
				requirementImportanceMapping = new Dictionary<int, int>();

				//Task Fields
				Dictionary<int, int> taskWorkflowMapping = new Dictionary<int, int>();
				taskTypeMapping = new Dictionary<int, int>();
				taskPriorityMapping = new Dictionary<int, int>();

				//Test Case Fields
				Dictionary<int, int> testCaseWorkflowMapping = new Dictionary<int, int>();
				testCaseTypeMapping = new Dictionary<int, int>();
				testCasePriorityMapping = new Dictionary<int, int>();

				//Document fields
				Dictionary<int, int> documentWorkflowMapping = new Dictionary<int, int>();
				documentTypeMapping = new Dictionary<int, int>();
				documentStatusMapping = new Dictionary<int, int>();

				//Risk fields
				Dictionary<int, int> riskWorkflowMapping = new Dictionary<int, int>();
				riskTypeMapping = new Dictionary<int, int>();
				riskStatusMapping = new Dictionary<int, int>();
				riskProbabilityMapping = new Dictionary<int, int>();
				riskImpactMapping = new Dictionary<int, int>();

				//Custom Fields
				customPropertyIdMapping = new Dictionary<int, int>();
				propertyValueMapping = new Dictionary<int, int>();

				//***** Now we need to copy across the custom properties *****
				Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
				customPropertyManager.CustomPropertyDefinition_CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, propertyValueMapping);

				//***** Now we need to copy across the incident fields *****
				IncidentManager incidentManager = new IncidentManager();
				incidentManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, incidentWorkflowMapping, incidentStatusMapping, incidentTypeMapping, incidentPriorityMapping, incidentSeverityMapping, customPropertyIdMapping);

				//***** Now we need to copy across the requirement fields *****
				RequirementManager requirementManager = new RequirementManager();
				requirementManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, requirementWorkflowMapping, requirementTypeMapping, requirementImportanceMapping, customPropertyIdMapping);

				//Now we need to copy across the release workflows
				ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();
				releaseWorkflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping);

				//***** Now we need to copy across the task fields *****
				TaskManager taskManager = new TaskManager();
				taskManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, taskWorkflowMapping, taskTypeMapping, taskPriorityMapping, customPropertyIdMapping);

				//***** Now we need to copy across the test case fields *****
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, testCaseWorkflowMapping, testCaseTypeMapping, testCasePriorityMapping, customPropertyIdMapping);

				//***** Now we need to copy across the document fields and workflows *****
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, documentWorkflowMapping, documentStatusMapping, documentTypeMapping, customPropertyIdMapping);

				//***** Now we need to copy across the risk fields *****
				RiskManager riskManager = new RiskManager();
				riskManager.CopyToProjectTemplate(existingProjectTemplateId, newProjectTemplateId, riskWorkflowMapping, riskStatusMapping, riskTypeMapping, riskImpactMapping, riskProbabilityMapping, customPropertyIdMapping);

				//** Copy notification events and templates
				NotificationManager notificationManager = new NotificationManager();
				notificationManager.CopyProjectEvents(existingProjectTemplateId, newProjectTemplateId);
				notificationManager.NotificationTemplate_CopyForProjectTemplate(existingProjectTemplateId, newProjectTemplateId);

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
		/// Updates a project template
		/// </summary>
		/// <param name="projectTemplate">The project template to be updated</param>
		public void Update(ProjectTemplate projectTemplate, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach the entity to the context and save
					context.ProjectTemplates.ApplyChanges(projectTemplate);
					context.AdminSaveChanges(userId, projectTemplate.ProjectTemplateId, null, adminSectionId, action, true, true, null);
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
		/// Retrieves the id of the project template associated with a specific project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The associatd template</returns>
		/// <remarks>To improve performance we cache this looking in memory</remarks>
		public ProjectTemplate RetrieveForProject(int projectId)
		{
			const string METHOD_NAME = "RetrieveForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectTemplate projectTemplate = null;

				//See if we have a cached lookup
				if (cachedLookup != null && cachedLookup.ContainsKey(projectId))
				{
					return cachedLookup[projectId];
				}

				//Create select command for retrieving the project record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Projects.Include(p => p.Template)
								where p.ProjectId == projectId
								select p;

					Project project = query.FirstOrDefault();
					if (project != null)
					{
						projectTemplate = project.Template;
						if (cachedLookup != null && projectTemplate != null && !cachedLookup.ContainsKey(projectId))
						{
							projectTemplate = cachedLookup.GetOrAdd(projectId, projectTemplate);
						}
					}
				}

				//Return the project template
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectTemplate;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the project template by its ID
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The template</returns>
		public ProjectTemplate RetrieveById(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectTemplate projectTemplate = null;
				//Create select command for retrieving the project record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectTemplates
								where p.ProjectTemplateId == projectTemplateId
								select p;

					projectTemplate = query.FirstOrDefault();
				}

				//Return the project template
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectTemplate;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all project templates that match the passed-in filter
		/// </summary>
		/// <param name="filters">The hashtable of filters to apply to the project template list</param>
		/// <param name="sortExpression">The sort expression to use, pass null for default</param>
		/// <returns>A project template list</returns>
		/// <remarks>
		/// Pass filters = null for all project templates.
		/// The filters supported are for name, id and active flag only
		/// </remarks>
		public List<ProjectTemplate> Retrieve(Hashtable filters, string sortExpression)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectTemplate> projectTemplates;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving all the project group records
					var query = from p in context.ProjectTemplates
								select p;

					//Now add any specified filters
					if (filters != null)
					{
						//Template Name
						if (filters["Name"] != null)
						{
							//Break up the name into keywords
							MatchCollection keywordMatches = Regex.Matches((string)filters["Name"], Common.Global.REGEX_KEYWORD_MATCHER);
							foreach (Match keywordMatch in keywordMatches)
							{
								string keyword = keywordMatch.Value.Replace("\"", "");
								query = query.Where(p => p.Name.Contains(keyword));
							}
						}

						//Active Flag
						if (filters["IsActive"] != null)
						{
							string isActive = (string)filters["IsActive"];
							query = query.Where(p => p.IsActive == (isActive == "Y"));
						}

						//Project Template ID
						if (filters["ProjectTemplateId"] != null)
						{
							//The value might be an Int32 or String, so use ToString() to be on the safe side
							//Need to make sure that the project group id is numeric
							int projectTemplateId;
							if (Int32.TryParse(filters["ProjectTemplateId"].ToString(), out projectTemplateId))
							{
								query = query.Where(p => p.ProjectTemplateId == projectTemplateId);
							}
						}
					}

					//Add the sorts and execute
					if (String.IsNullOrEmpty(sortExpression))
					{
						query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectTemplateId);
					}
					else
					{
						query = query.OrderUsingSortExpression(sortExpression, "ProjectTemplateId");
					}
					projectTemplates = query.ToList();
				}

				//Return the dataset
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectTemplates;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the active templates ordered by name
		/// </summary>
		/// <returns>The list of templates</returns>
		public List<ProjectTemplate> RetrieveActive()
		{
			const string METHOD_NAME = "RetrieveActive";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project record
				List<ProjectTemplate> templates;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectTemplates
								where p.IsActive
								orderby p.Name, p.ProjectTemplateId
								select p;

					templates = query.ToList();
				}

				//Return the project templates
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return templates;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a project template from the system including all associated artifacts
		/// </summary>
		/// <param name="userId">The user we're performing the operation as</param>
		/// <param name="projectTemplateId">The project template to be deleted</param>
		public void Delete(int userId, int projectTemplateId)
		{
			const string METHOD_NAME = "Delete";

			try
			{
				//Make sure no projects are using this template
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the project template
					var query1 = from p in context.ProjectTemplates
								 where p.ProjectTemplateId == projectTemplateId
								 select p;

					ProjectTemplate projectTemplate = query1.FirstOrDefault();
					if (projectTemplate == null)
					{
						//Deleted already, so just return
						return;
					}

					var query2 = from p in context.Projects
								 where p.ProjectTemplateId == projectTemplateId
								 select p;

					if (query2.Count() > 0)
					{
						throw new ProjectTemplateInUseException(GlobalResources.Messages.ProjectTemplate_CannotDeleteActive);
					}

					//Now we need to delete all the configurable types associated with this project template
					//Needs to happen before workflow deletions, since otherwise workflow will be 'in use'
					context.Template_DeleteConfigurableTypes(projectTemplateId);

					//Now we need to delete all the incident workflows associated with this project (non cascadable)
					WorkflowManager workflowManager = new WorkflowManager();
					workflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the requirement workflows associated with this project (non cascadable)
					RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();
					requirementWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the task workflows associated with this project (non cascadable)
					TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();
					taskWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the release workflows associated with this project (non cascadable)
					ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();
					releaseWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the test case workflows associated with this project (non cascadable)
					TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();
					testCaseWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the document workflows (non-cascadable)
					DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();
					documentWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the risk workflows (non-cascadable)
					RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();
					riskWorkflowManager.Workflow_DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete all the custom properties and then custom lists. The dependent entities should then cascade
					context.Template_DeleteCustomProperties(projectTemplateId);

					///Now delete the notification templates and events
					new NotificationManager().DeleteAllForProjectTemplate(projectTemplateId);

					//Now we need to delete the template itself                    
					context.ProjectTemplates.DeleteObject(projectTemplate);
					context.SaveChanges();

					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
					string adminSectionName = "View / Edit Templates";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;

					//Add a changeset to mark it as deleted.
					new AdminAuditManager().LogDeletion1((int)userId, projectTemplate.ProjectTemplateId, projectTemplate.Name, adminSectionId, "Template Deleted", DateTime.UtcNow, ArtifactTypeEnum.ProjectTemplate, "ProjectTemplateId");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ProjectTemplateInUseException)
			{
				//Don't log
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw new Exception("Cannot Delete Project Template", exception);
			}
		}
	}

	/// <summary>This exception is thrown when you try and delete a project template that is used by projects</summary>
	public class ProjectTemplateInUseException : ApplicationException
	{
		public ProjectTemplateInUseException()
		{
		}
		public ProjectTemplateInUseException(string message)
			: base(message)
		{
		}
		public ProjectTemplateInUseException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

}
