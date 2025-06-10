using LinqToEdmx.Model.Conceptual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// The base class for all ajax web services that return a list of DataItem data objects
	/// </summary>
	public class ListServiceBase : AjaxWebServiceBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ListServiceBase::";

		/// <summary>
		/// Default implementation of the custom list operation. throws a not implemented exception
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destId">The destination item id</param>
		/// <param name="items">The list of source items</param>
		/// <param name="operation">The name of the custom operation</param>
		public virtual string CustomListOperation(string operation, int projectId, int destId, List<string> items)
		{
			throw new NotImplementedException("This operation is not currently implemented");
		}

		/// <summary>
		/// Returns a list of lookups that are used by the various pop-up dialogs
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="operation">The name of the lookup being retrieved</param>
		/// <returns>dictionary of lookup ids (key) and lookup display names (value)</returns>
		public JsonDictionaryOfStrings RetrieveLookupList(int projectId, string operation)
		{
			const string METHOD_NAME = "RetrieveLookupList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				JsonDictionaryOfStrings lookupList = new JsonDictionaryOfStrings();

				//See which lookup we're being asked to retrieve
				if (operation == "Project")
				{
					//Instantiate the project business object
					Business.ProjectManager projectManager = new Business.ProjectManager();

					//Now retrieve the project list
					List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId);

					//Convert from dataset into simple dictionary
					foreach (ProjectForUserView project in projects)
					{
						//We don't want to add the current project to the list (can't export to itself)
						if (project.ProjectId != projectId)
						{
							lookupList.Add(project.ProjectId.ToString(), project.Name);
						}
					}
				}
				if (operation == "Release")
				{
					//Instantiate the release business object
					ReleaseManager releaseManager = new ReleaseManager();

					//Now retrieve the release/iteration list (active and inactive)
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false, true);

					//Convert from dataset into simple dictionary that has the indent/summary/alternate/active flags encoded into the key
                    foreach (ReleaseView releaseRow in releases)
					{
						//Need to get the indent number
						int indent = releaseRow.IndentLevel.Length / 3;
						string releaseValue = releaseRow.ReleaseId.ToString();
                        releaseValue += "_" + indent.ToString() + "_" + releaseRow.IsSummary.ToFlagValue() + "_" + releaseRow.IsIterationOrPhase.ToFlagValue() + "_" + releaseRow.IsActive.ToFlagValue();
						lookupList.Add(releaseValue, releaseRow.FullName);
					}
				}

				if (operation == "Requirement")
				{
					//Instantiate the requirement business object
					RequirementManager requirementManager = new RequirementManager();

					//Now retrieve the requirement list
					List<RequirementView> requirements = requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, 1, Int32.MaxValue, null, 0);

					//Convert from dataset into simple dictionary
					foreach (RequirementView requirement in requirements)
					{
						//Need to get the indent number
						int indent = requirement.IndentLevel.Length / 3;
						string requirementValue = requirement.RequirementId.ToString();
                        string requirementName = String.IsNullOrWhiteSpace(requirement.Name) ? Resources.ClientScript.Global_None2 : requirement.Name;
						requirementValue += "_" + indent.ToString() + "_" + ((requirement.IsSummary) ? "Y" : "N") + "_N_Y";
						lookupList.Add(requirementValue, requirementName);
					}
				}

                if (operation == "TestSet")
                {
                    //Instantiate the test set business object
                    TestSetManager testSetManager = new TestSetManager();

                    //Now retrieve the requirement list
                    List<TestSetManager.TestSetLookupEntry> testSets = testSetManager.RetrieveForLookups2(projectId);

                    //Convert from dataset into simple dictionary
                    foreach (TestSetManager.TestSetLookupEntry testSet in testSets)
                    {
                        //Need to get the indent number
                        int indent = testSet.IndentLevel.Length / 3;
                        string testSetValue = testSet.TestSetId.ToString();
                        string testSetName = String.IsNullOrWhiteSpace(testSet.Name) ? Resources.ClientScript.Global_None2 : testSet.Name;
                        testSetValue += "_" + indent.ToString() + "_" + ((testSet.IsFolder) ? "Y" : "N") + "_N_Y";
                        lookupList.Add(testSetValue, testSetName);
                    }
                }

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return lookupList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public delegate JsonDictionaryOfStrings LookupRetrieval(string lookupName, int projectId, int projectTemplateId);
		public delegate void EqualizerShapeRetrieval(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId);
		public delegate void EqualizerPopulation(DataItem dataItem, DataItemField dataItemField, Artifact artifact);

		/// <summary>Returns a list of pagination options that the user can choose from</summary>
		/// <param name="projectId">The project in question</param>
		/// <param name="userId">The user in question</param>
		/// <param name="collectionName">The name of the settings collection to use (varies by artifact)</param>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		protected JsonDictionaryOfStrings RetrievePaginationOptions(int projectId, int userId, string collectionName)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions(int,int,string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings retValue = null;

				retValue = this.RetrievePaginationOptions(projectId, userId, collectionName, "");

				Logger.LogExitingEvent(METHOD_NAME);
				return retValue;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns a list of pagination options that the user can choose from</summary>
		/// <param name="projectId">The project in question. -1 to get user settings.</param>
		/// <param name="userId">The user in question</param>
		/// <param name="collectionName">The name of the settings collection to use (varies by artifact)</param>
		/// <param name="keyPrefix">Prefex to the settings key names.</param>
		/// <param name="defaultValue">The default value for the pagination (if no value specified)</param>
		/// <param name="overrideSettingsKey">Used to specify an override settings collection key to use</param>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		protected JsonDictionaryOfStrings RetrievePaginationOptions(int projectId, int userId, string collectionName, string keyPrefix, string overrideSettingsKey = null, int defaultValue = 15)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions(int,int,string,string)";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				string keyName_PageNum1 = ((string.IsNullOrWhiteSpace(keyPrefix)) ? "" : keyPrefix + ".") + "PaginationOption";
				string keyName_PageNum2 = ((string.IsNullOrWhiteSpace(keyPrefix)) ? "" : keyPrefix + ".") + "NumberRowsPerPage";

				//Get the list of options
				ManagerBase manager = new ManagerBase();
                SortedList<int, int> paginationOptions = manager.GetPaginationOptions();

				//Get the current pagination setting for the project/user
				int paginationSize = defaultValue; //Default start value
				if (projectId > 0)
				{

					ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, collectionName);
					paginationSettings.Restore();

					//There are two different settings keys used for different artifact types
					if (String.IsNullOrEmpty(overrideSettingsKey))
					{
						if (paginationSettings[keyName_PageNum1] != null)
							paginationSize = (int)paginationSettings[keyName_PageNum1];
						else if (paginationSettings[keyName_PageNum2] != null)
							paginationSize = (int)paginationSettings[keyName_PageNum2];
					}
					else if (paginationSettings[overrideSettingsKey] != null)
					{
						paginationSize = (int)paginationSettings[overrideSettingsKey];
					}
				}
				else
				{
					UserSettingsCollection userPageSettings = new UserSettingsCollection(userId, collectionName);
					userPageSettings.Restore();

					//There are two different settings keys used for different artifact types
					if (userPageSettings[keyName_PageNum1] != null)
						paginationSize = (int)userPageSettings[keyName_PageNum1];
					else if (userPageSettings[keyName_PageNum2] != null)
						paginationSize = (int)userPageSettings[keyName_PageNum2];

				}

				//Reformulate into a dictionary where the value indicates if it's the selected value or not (true/false)
				JsonDictionaryOfStrings paginationDictionary = new JsonDictionaryOfStrings();
				foreach (KeyValuePair<int, int> kvp in paginationOptions)
				{
					//See if this is the selected value or not
					paginationDictionary.Add(kvp.Key.ToString(), (kvp.Key == paginationSize) ? "true" : "false");
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return paginationDictionary;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Substring function that handles the case of a string that is too short safely</summary>
		/// <param name="input">The input string to be truncated</param>
		/// <param name="maxLength">The max-length to truncate to</param>
		/// <returns>The truncated string</returns>
		protected string SafeSubstring(string input, int maxLength)
		{
			if (input.Length > maxLength)
			{
				string output = input.Substring(0, maxLength);
				return output;
			}
			else
			{
				return input;
			}
		}

		/// <summary>
		/// Populates the generic fields in a data-item from the passed-in artifact entity record
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="entity">The artifact item containing the data</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		///<param name="dataItemField">The field we're currently populating</param>
		///<param name="equalizerPopulation">Delegate to the function used to populate equalizer fields</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <param name="artifactCustomProperty">The record of custom property data (if not provided as part of dataitem) - pass null if not used</param>
		/// <param name="customProperties">The list of custom property definitions, options and values</param>
        /// <param name="readOnlyFields">Any standard fields that are read only</param>
		protected void PopulateFieldRow(DataItem dataItem, DataItemField dataItemField, Entity entity, List<CustomProperty> customProperties, ArtifactCustomProperty artifactCustomProperty, bool editable, EqualizerPopulation equalizerPopulation, List<WorkflowField> workflowFields = null, List<WorkflowCustomProperty> workflowCustomProps = null, List<string> readOnlyFields = null)
		{
			//Default to editable, optional and visible
			dataItemField.Editable = true;
			dataItemField.Required = false;
			dataItemField.Hidden = false;

			//See if we have a custom property or not (since the deserialization is different)
			int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(dataItemField.FieldName);
			if (customPropertyNumber.HasValue)
			{
				//Get the custom property definition
                if (customProperties != null)
                {
                    CustomProperty customProperty = customProperties.Find(cp => cp.PropertyNumber == customPropertyNumber.Value);
                    if (customProperty != null)
                    {
                        //Update the caption since it won't already have been set for custom fields
                        dataItemField.Caption = customProperty.Name;

                        //Set the required field if that option is set
                        if (customProperty.Options != null)
                        {
                            CustomPropertyOptionValue customPropOptionValue = customProperty.Options.FirstOrDefault(co => co.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty);
                            if (customPropOptionValue != null)
                            {
                                bool? allowEmpty = customPropOptionValue.Value.FromDatabaseSerialization_Boolean();
                                if (allowEmpty.HasValue)
                                {
                                    dataItemField.Required = !allowEmpty.Value;
                                }
                            }
                        }

                        //All custom properties are physically stored as text, we first try and get it from the artifact custom property
                        //entity, otherwise get it from the main datarow
                        string rawValue = null;
                        if (artifactCustomProperty != null)
                        {
                            rawValue = (string)artifactCustomProperty[dataItemField.FieldName];
                        }
                        else if (entity[dataItemField.FieldName] != null)
                        {
                            rawValue = (string)entity[dataItemField.FieldName];
                        }

                        if (rawValue != null)
                        {
                            //Handle each of the different custom property types
                            switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
                            {
                                case CustomProperty.CustomPropertyTypeEnum.User:
                                    {
                                        dataItemField.IntValue = (rawValue.FromDatabaseSerialization_Int32().HasValue) ? rawValue.FromDatabaseSerialization_Int32().Value : -1;
                                        //Get the user's name from the user manager (expensive operation but not many people
                                        //use User custom properties)
                                        if (dataItemField.IntValue.HasValue && dataItemField.IntValue.Value > 0)
                                        {
                                            try
                                            {
                                                User user = new UserManager().GetUserById(dataItemField.IntValue.Value);
                                                if (user != null)
                                                {
                                                    dataItemField.TextValue = user.FullName;
                                                }
                                            }
                                            catch (ArtifactNotExistsException)
                                            {
                                                //Leave the user value unset
                                            }
                                        }
                                        break;
                                    }

                                case CustomProperty.CustomPropertyTypeEnum.List:
                                    {
                                        dataItemField.IntValue = (rawValue.FromDatabaseSerialization_Int32().HasValue) ? rawValue.FromDatabaseSerialization_Int32().Value : -1;
                                        //Get the custom property value display text
                                        if (customProperty.List != null && !editable)
                                        {
                                            CustomPropertyValue cpv = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == dataItemField.IntValue);
                                            if (cpv != null)
                                            {
                                                dataItemField.TextValue = cpv.Name;
                                            }
                                        }
                                        break;
                                    }

                                case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                    {
                                        List<int> values = rawValue.FromDatabaseSerialization_List_Int32();
                                        //For an editable results we just need comma-separated values
                                        if (editable)
                                        {
                                            MultiValueFilter multiValue = new MultiValueFilter();
                                            multiValue.Values.AddRange(values);
                                            dataItemField.TextValue = multiValue.ToString();
                                        }
                                        else
                                        {
                                            //Get the custom property value display text
                                            if (customProperty.List != null && values != null && values.Count > 0)
                                            {
                                                //Display the value or the word '(Multiple)'
                                                if (values.Count > 1)
                                                {
                                                    dataItemField.TextValue = "(" + Resources.Main.Global_Multiple + ")";
                                                    string tooltip = "";
                                                    foreach (uint customPropertyValueId in values)
                                                    {
                                                        if (tooltip == "")
                                                        {
                                                            CustomPropertyValue customPropertyValue = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == customPropertyValueId);
                                                            if (customPropertyValue != null)
                                                            {
                                                                tooltip = customPropertyValue.Name;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            CustomPropertyValue customPropertyValue = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == customPropertyValueId);
                                                            if (customPropertyValue != null)
                                                            {
                                                                tooltip += ", " + customPropertyValue.Name;
                                                            }
                                                        }
                                                    }
                                                    dataItemField.Tooltip = tooltip;
                                                }
                                                else
                                                {
                                                    CustomPropertyValue customPropertyValue = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == values[0]);
                                                    if (customPropertyValue != null)
                                                    {
                                                        dataItemField.TextValue = customPropertyValue.Name;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;


                                case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                    {
                                        bool? booleanValue = rawValue.FromDatabaseSerialization_Boolean();
                                        if (booleanValue.HasValue)
                                        {
                                            //The ajax controls expect Y/N in the text
                                            dataItemField.TextValue = (booleanValue.Value) ? "Y" : "N";
                                        }
                                    }
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Integer:
                                    {
                                        int? intValue = rawValue.FromDatabaseSerialization_Int32();
                                        if (intValue.HasValue)
                                        {
                                            dataItemField.IntValue = intValue.Value;
                                            dataItemField.TextValue = intValue.Value.ToString();
                                        }
                                    }
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                    {
                                        decimal? decValue = rawValue.FromDatabaseSerialization_Decimal();
                                        if (decValue.HasValue)
                                        {
                                            //Handle decimal precision
                                            CustomPropertyOptionValue optValue = customProperty.Options.FirstOrDefault(c => c.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Precision);
                                            if (optValue != null)
                                            {
                                                int precision;
                                                if (Int32.TryParse(optValue.Value, out precision))
                                                {
                                                    //If we round, need to display the unrounded version as a tooltip
                                                    decimal roundedDec = Decimal.Round(decValue.Value, precision);
                                                    dataItemField.TextValue = roundedDec.ToString();
                                                    dataItemField.Tooltip = decValue.Value.ToString();
                                                }
                                            }
                                            else
                                            {
                                                dataItemField.TextValue = decValue.Value.ToString();
                                            }
                                        }
                                    }
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Date:
                                    {
                                        DateTime? dateTimeValue = rawValue.FromDatabaseSerialization_DateTime();
                                        if (dateTimeValue.HasValue)
                                        {
                                            //Convert from UTC to local time
                                            dataItemField.DateValue = GlobalFunctions.LocalizeDate(dateTimeValue.Value);
                                            if (editable)
                                            {
                                                dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dataItemField.DateValue);
                                            }
                                            else
                                            {
                                                dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, dataItemField.DateValue);
                                                dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, dataItemField.DateValue);
                                            }
                                        }
                                    }
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Text:
                                    {
                                        //See if we have rich text, if so, change the type for list views (no workflows)
                                        if (customProperty.Options != null && workflowCustomProps == null)
                                        {
                                            CustomPropertyOptionValue customPropOptionValue = customProperty.Options.FirstOrDefault(co => co.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText);
                                            if (customPropOptionValue != null)
                                            {
                                                bool? isRichText = customPropOptionValue.Value.FromDatabaseSerialization_Boolean();
                                                if (isRichText.HasValue && isRichText.Value == true)
                                                {
                                                    //Rich text is not editable in the list views
                                                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Html;
                                                    dataItemField.Editable = false;
                                                }
                                            }
                                        }
                                        dataItemField.TextValue = rawValue.FromDatabaseSerialization_String();
                                    }
                                    break;
                            }
                        }

                        //Specify which fields are editable or required
                        if (workflowCustomProps != null)
                        {
                            if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyFieldName == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
                            {
                                dataItemField.Editable = false;
                            }
                            if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyFieldName == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                            {
                                dataItemField.Required = true;
                            }
                            if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyFieldName == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
                            {
                                dataItemField.Hidden = true;
                            }
                        }
                    }
                }
			}
			else
			{
                //Make sure we have this property on the entity
                if (entity.ContainsProperty(dataItemField.FieldName))
                {
                    //Specify which fields are editable or required
                    dataItemField.Editable = true;
                    dataItemField.Required = false;
                    dataItemField.Hidden = false;
                    if (workflowFields != null)
                    {
                        if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
                        {
                            dataItemField.Editable = false;
                        }
                        if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                        {
                            dataItemField.Required = true;
                        }
                        if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
                        {
                            dataItemField.Hidden = true;
                        }
                    }

                    //Also need to make sure that the physical column allows nulls if not required
                    EntityProperty entityProperty = ManagerBase.GetPropertyInfo(entity.GetType(), dataItemField.FieldName);
                    bool isNullable = entity.Properties[dataItemField.FieldName].IsNullable();
                    if (entityProperty != null && isNullable)
                    {
                        isNullable = entityProperty.Nullable;
                    }
                    if (!isNullable)
                    {
                        //These are required regardless of what workflow says
                        dataItemField.Required = true;
                    }

                    //Also see if any fields are always read-only (regardless of workflow)
                    if (readOnlyFields != null && readOnlyFields.Contains(dataItemField.FieldName))
                    {
                        dataItemField.Editable = false;
                    }

                    //Handle each of the different field types
                    switch (dataItemField.FieldType)
                    {
                        case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
                            {
                                //Only artifacts have this type
                                if (entity is Artifact)
                                {
                                    Artifact artifact = (Artifact)entity;
                                    if (entity[dataItemField.FieldName] is Int32)
                                    {
                                        dataItemField.IntValue = (int)entity[dataItemField.FieldName];
                                    }
                                    if (entity[dataItemField.FieldName] is Int64)
                                    {
                                        dataItemField.IntValue = (int)((long)entity[dataItemField.FieldName]);
                                    }
                                    dataItemField.TextValue = "[" + artifact.ArtifactPrefix + ":" + String.Format(GlobalFunctions.FORMAT_ID, (int)entity[dataItemField.FieldName]) + "]";
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
                            {
                                if (entity is Artifact)
                                {
                                    Artifact artifact = (Artifact)entity;
                                    equalizerPopulation(dataItem, dataItemField, artifact);
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
                        case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
                            {
                                if (entity.ContainsProperty(dataItemField.FieldName) && entity[dataItemField.FieldName] is Int32)
                                {
                                    dataItemField.IntValue = (int)entity[dataItemField.FieldName];
                                }
                                if (dataItemField.LookupName != null && entity.ContainsProperty(dataItemField.LookupName))
                                {
                                    dataItemField.TextValue = (string)entity[dataItemField.LookupName];
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
                            if (entity.ContainsProperty(dataItemField.FieldName))
                            {
                                if (entity[dataItemField.FieldName] is String)
                                {
                                    dataItemField.TextValue = (string)entity[dataItemField.FieldName];
                                }
                                if (entity[dataItemField.FieldName] is Boolean)
                                {
                                    dataItemField.TextValue = GlobalFunctions.GetYnFlagForDropdown((bool)entity[dataItemField.FieldName]);
                                }
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
                            if (entity.ContainsProperty(dataItemField.FieldName) && entity[dataItemField.FieldName] is Int32)
                            {
                                dataItemField.IntValue = (int)entity[dataItemField.FieldName];
                                dataItemField.TextValue = dataItemField.IntValue.ToString();
                            }
                            if (entity.ContainsProperty(dataItemField.FieldName) && entity[dataItemField.FieldName] is Int64)
                            {
                                dataItemField.IntValue = (int)((long)entity[dataItemField.FieldName]);
                                dataItemField.TextValue = dataItemField.IntValue.ToString();
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
                            if (entity.ContainsProperty(dataItemField.FieldName) && entity[dataItemField.FieldName] is Int32)
                            {
                                dataItemField.IntValue = (int)entity[dataItemField.FieldName];
                                //Display as fractional hours
                                decimal fractionalHours = ((decimal)dataItemField.IntValue) / (decimal)60;
                                if (editable && (!dataItemField.Editable.HasValue || dataItemField.Editable.Value))
                                {
                                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS_EDITABLE, fractionalHours);
                                }
                                else
                                {
                                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, fractionalHours);
                                }

                                int hours = EffortUtils.GetEffortHoursComponent(dataItemField.IntValue.Value);
                                int mins = EffortUtils.GetEffortMinutesComponent(dataItemField.IntValue.Value);
                                dataItemField.Tooltip = hours + " " + Resources.Fields.Hours + " " + mins + " " + Resources.Fields.Minutes;
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
                            if (entity.ContainsProperty(dataItemField.FieldName) && entity[dataItemField.FieldName] is DateTime)
                            {
                                //Convert from UTC to local time
                                dataItemField.DateValue = GlobalFunctions.LocalizeDate((DateTime)entity[dataItemField.FieldName]);
                                if (editable)
                                {
                                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dataItemField.DateValue);
                                }
                                else
                                {
                                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, dataItemField.DateValue);
                                }
                                dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, dataItemField.DateValue);
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
                        case DataModel.Artifact.ArtifactFieldTypeEnum.Html:
                            dataItemField.TextValue = (string)entity[dataItemField.FieldName];
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Decimal:
                            {
                                if (entity[dataItemField.FieldName] is decimal)
                                {
                                    dataItemField.TextValue = ((decimal)entity[dataItemField.FieldName]).ToSafeString();
                                }
                                if (entity[dataItemField.FieldName] is double)
                                {
                                    dataItemField.TextValue = ((double)entity[dataItemField.FieldName]).ToSafeString();
                                }
                            }
                            break;

                        case Artifact.ArtifactFieldTypeEnum.MultiList:
                            {
                                if (entity[dataItemField.FieldName] == null)
                                {
                                    dataItemField.TextValue = null;
                                }
                                else
                                {
                                    string rawValue = entity[dataItemField.FieldName].ToString();
                                    List<int> values = rawValue.FromDatabaseSerialization_List_Int32();
                                    //For an editable results we need comma-separated values normalized to have no padding zeros
                                    if (editable)
                                    {
                                        MultiValueFilter multiValue = new MultiValueFilter();
                                        multiValue.Values.AddRange(values);
                                        dataItemField.TextValue = multiValue.ToString();
                                    }
                                    else
                                    {
                                        dataItemField.TextValue = rawValue;
                                    }
                                }
                            }
                            break;

                        default:
                            dataItemField.TextValue = (entity[dataItemField.FieldName] == null) ? null : entity[dataItemField.FieldName].ToString();
                            break;
                    }
                }
			}
		}

		/// <summary>
		/// Updates a newly inserted item to match the existing filters (so that it's visible after insert)
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="filterList">The list of filters</param>
		/// <param name="artifactId">The id of the current artifact</param>
		/// <param name="artifact">The artifact entity that we need to update</param>
		/// <param name="fieldsToIgnore">Any fields to be ignore (e.g. the primary key, creation date)</param>
		/// <param name="artifactCustomProperty">The entity containing the custom property data for the row</param>
		protected void UpdateToMatchFilters(int projectId, ProjectSettingsCollection filterList, int artifactId, DataModel.Artifact artifact, ArtifactCustomProperty artifactCustomProperty, List<string> fieldsToIgnore = null)
		{
			foreach (DictionaryEntry filterEntry in filterList)
			{
				string filterName = (string)filterEntry.Key;
				object filterValue = filterEntry.Value;
				bool ignore = false;

				//See if this field is meant to be ignored
				if (fieldsToIgnore != null && fieldsToIgnore.Contains(filterName))
				{
					ignore = true;
				}

				//Ignore any negative ids as they are not real values
				if (filterValue.GetType() == typeof(int))
				{
					int filterValueInt = (int)filterValue;
					if (filterValueInt < 0)
					{
						ignore = true;
					}
				}

				//If we have a date-range filter need to change into a single-date
				if (filterValue.GetType() == typeof(DateRange))
				{
					DateRange dateRange = (DateRange)filterValue;
					if (dateRange.StartDate.HasValue)
					{
						filterValue = dateRange.StartDate.Value;
					}
					else if (dateRange.EndDate.HasValue)
					{
						filterValue = dateRange.EndDate.Value;
					}
					else
					{
						//Don't set the value
						ignore = true;
					}
				}

                //If we have a effort-range filter need to change into a single-number
                if (filterValue.GetType() == typeof(EffortRange))
                {
                    EffortRange effortRange = (EffortRange)filterValue;
                    if (effortRange.MinValueInMinutes.HasValue)
                    {
                        filterValue = effortRange.MinValueInMinutes.Value;
                    }
                    else if (effortRange.MaxValueInMinutes.HasValue)
                    {
                        filterValue = effortRange.MaxValueInMinutes.Value;
                    }
                    else
                    {
                        //Don't set the value
                        ignore = true;
                    }
                }

                //If we have a number-range filter need to change into a single-number
                if (filterValue.GetType() == typeof(IntRange))
                {
                    IntRange intRange = (IntRange)filterValue;
                    if (intRange.MinValue.HasValue)
                    {
                        filterValue = intRange.MinValue.Value;
                    }
                    else if (intRange.MaxValue.HasValue)
                    {
                        filterValue = intRange.MaxValue.Value;
                    }
                    else
                    {
                        //Don't set the value
                        ignore = true;
                    }
                }

                //If we have a number-range filter need to change into a single-number
                if (filterValue.GetType() == typeof(DecimalRange))
                {
                    DecimalRange decimalRange = (DecimalRange)filterValue;
                    if (decimalRange.MinValue.HasValue)
                    {
                        filterValue = decimalRange.MinValue.Value;
                    }
                    else if (decimalRange.MaxValue.HasValue)
                    {
                        filterValue = decimalRange.MaxValue.Value;
                    }
                    else
                    {
                        //Don't set the value
                        ignore = true;
                    }
                }

				//See if we have a custom property or standard field
				if (!ignore)
				{
					int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(filterName);
					if (customPropertyNumber.HasValue)
					{
						//We have a custom property
						CustomPropertyManager customPropertyManager = new CustomPropertyManager();
						CustomProperty customProperty = artifactCustomProperty.CustomPropertyDefinitions.FirstOrDefault(cp => cp.PropertyNumber == customPropertyNumber.Value);
						if (customProperty != null && filterValue != null)
						{
							//Depending on the type of custom property, update the value accordingly
							switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
							{
								case CustomProperty.CustomPropertyTypeEnum.Boolean:
									{
										if (filterValue is bool)
										{
											bool boolValue = (bool)filterValue;
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, boolValue);
										}
										if (filterValue is string)
										{
											string ynFlag = (string)filterValue;
											bool boolValue = (ynFlag == "Y");
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, boolValue);
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Date:
									{
										if (filterValue is DateTime)
										{
											DateTime dtValue = (DateTime)filterValue;
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, dtValue);
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Decimal:
									{
										if (filterValue is decimal)
										{
											decimal decValue = (decimal)filterValue;
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, decValue);
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Integer:
								case CustomProperty.CustomPropertyTypeEnum.List:
								case CustomProperty.CustomPropertyTypeEnum.User:
									{
										if (filterValue is int)
										{
											int intValue = (int)filterValue;
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, intValue);
										}
										if (filterValue is MultiValueFilter)
										{
											//If we have a multi-value filter need to change into a single-value since we don't have any
											//Otherwise, just pick the first value
											MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
											if (!multiValueFilter.IsNone && multiValueFilter.Values != null && multiValueFilter.Values.Count > 0)
											{
												int intValue = (int)multiValueFilter.Values[0];
												artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, intValue);
											}
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.MultiList:
									{
										if (filterValue is MultiValueFilter)
										{
											MultiValueFilter mvfValue = (MultiValueFilter)filterValue;
											if (!mvfValue.IsNone && mvfValue.Values != null && mvfValue.Values.Count > 0)
											{
												artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, mvfValue.Values);
											}
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Text:
									{
										if (filterValue is string)
										{
											string textValue = (string)filterValue;
											artifactCustomProperty.SetCustomProperty(customPropertyNumber.Value, textValue);
										}
									}
									break;
							}
						}
					}
					else
					{
						//If we have a multi-value filter need to change into a single-value since we don't have any
						//standard single-value fields except Component
						if (filterValue.GetType() == typeof(MultiValueFilter))
						{
                            if (filterName == "ComponentIds")
                            {
                                MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
                                filterValue = multiValueFilter.Values.ToDatabaseSerialization();
                            }
                            else
                            {
                                MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
                                //For the none case, need to convert into null
                                if (multiValueFilter.IsNone)
                                {
                                    filterValue = null;
                                }
                                //Otherwise, just pick the first value
                                if (multiValueFilter.Values != null && multiValueFilter.Values.Count > 0)
                                {
                                    filterValue = (int)multiValueFilter.Values[0];
                                }
                                else
                                {
                                    //Don't set the value
                                    ignore = true;
                                }
                            }
						}

						//We have a standard field
						if (artifact.ContainsProperty(filterName) && artifact.Properties[filterName].CanWrite)
						{
							//Make sure the column accepts nulls if we're passed that
							if (filterValue != null || artifact.Properties[filterName].IsNullable())
							{
								artifact[filterName] = filterValue;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds a validation message to the list of messages, making sure we only report once per field/message combination
		/// </summary>
		/// <param name="validationMessages">The list of messages</param>
		/// <param name="newMessage">The new message to add</param>
		protected static void AddUniqueMessage(List<ValidationMessage> validationMessages, ValidationMessage newMessage)
		{
			if (!validationMessages.Any(v => v.FieldName == newMessage.FieldName && v.Message == newMessage.Message))
			{
				validationMessages.Add(newMessage);
			}
		}

		/// <summary>
		/// Updates the fields of an artifact entity from the data item
		/// </summary>
		/// <param name="dataItem">The data item</param>
		/// <param name="artifact">The artifact being updated</param>
		/// <param name="artifactCustomProperty">The custom property information for the artifact</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact being updated</param>
		/// <param name="artifactType">The type of artifact being updated</param>
		/// <param name="customProperties">The custom property definitions</param>
		/// <param name="fieldsToIgnore">There may be fields that are handled specially and this function should leave alone [optional]</param>
		/// <param name="validationMessages">Collection of validation messages that it adds to</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <remarks>
		/// Replaces the dataset version once all artiacts switched over to entities
		/// </remarks>
		protected void UpdateFields(List<ValidationMessage> validationMessages, DataItem dataItem, Artifact artifact, List<CustomProperty> customProperties, ArtifactCustomProperty artifactCustomProperty, int projectId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, List<string> fieldsToIgnore = null, List<WorkflowField> workflowFields = null, List<WorkflowCustomProperty> workflowCustomProps = null)
		{
			const string METHOD_NAME = "UpdateFields";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				foreach (KeyValuePair<string, DataItemField> kvp in dataItem.Fields)
				{
					DataItemField dataItemField = kvp.Value;
					string fieldName = dataItemField.FieldName;

					//See if we have a special field to ignore
					if ((fieldsToIgnore != null && fieldsToIgnore.Contains(fieldName)))
					{
						continue;
					}

					//See if this is a special data-mapping field
					if (dataItemField.FieldName.SafeSubstring(0, DataMappingManager.FIELD_PREPEND.Length) == DataMappingManager.FIELD_PREPEND)
					{
						continue;
					}

					//Check to see if any fields are required by the workflow
					//We don't rely on the required field on the dataItem because it may have tampered with on the client-side
					bool isRequiredByWorkflow = false;
					if (workflowFields != null && workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						isRequiredByWorkflow = true;
					if (workflowCustomProps != null && workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyFieldName == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						isRequiredByWorkflow = true;

					//See if this is a custom property or a standard field since they are handled differently
					int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(fieldName);
					if (customPropertyNumber.HasValue)
					{
						//This is a custom property so we need to see what type of custom property it is and handle
						//the serialization and validation appropriately
						CustomProperty customProperty = customProperties.Find(cp => cp.PropertyNumber == customPropertyNumber.Value);
                        if (customProperty != null && artifactCustomProperty != null && artifactCustomProperty.ContainsProperty(fieldName))
						{
							string fieldCaption = customProperty.Name;

							//Handle each type separately
							switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
							{
								case CustomProperty.CustomPropertyTypeEnum.Boolean:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											artifactCustomProperty[fieldName] = dataItemField.TextValue.ToDatabaseSerialization();
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Date:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											DateTime dateTime;
											if (!DateTime.TryParse(dataItemField.TextValue, out dateTime))
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
											}
											else
											{
												//Convert into UTC before saving on the datarow
												artifactCustomProperty[fieldName] = GlobalFunctions.UniversalizeDate(dateTime).ToDatabaseSerialization();
											}
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Decimal:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											decimal decValue;
											if (!Decimal.TryParse(dataItemField.TextValue, out decValue))
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
											}
											else
											{
												//Check to see what precision is required and round to that
												CustomPropertyOptionValue optValue = customProperty.Options.FirstOrDefault(c => c.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Precision);
												if (optValue == null)
												{
													//No rounding
													artifactCustomProperty[fieldName] = decValue.ToDatabaseSerialization();
												}
												else
												{
													int precision;
													if (Int32.TryParse(optValue.Value, out precision))
													{
														artifactCustomProperty[fieldName] = Decimal.Round(decValue, precision).ToDatabaseSerialization();
													}
													else
													{
														//No rounding
														artifactCustomProperty[fieldName] = decValue.ToDatabaseSerialization();
													}
												}
											}
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Integer:
									{
										if (!dataItemField.IntValue.HasValue)
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											artifactCustomProperty[fieldName] = dataItemField.IntValue.ToDatabaseSerialization();
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.List:
									{
										if (!dataItemField.IntValue.HasValue)
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											artifactCustomProperty[fieldName] = dataItemField.IntValue.ToDatabaseSerialization();
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.MultiList:
									{
										if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											//Parse the value into a 'multi-value'
											MultiValueFilter multiValue;
											if (!MultiValueFilter.TryParse(dataItemField.TextValue, out multiValue))
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
											}
											else
											{
												List<int> values = multiValue.Values;
												artifactCustomProperty[fieldName] = values.ToDatabaseSerialization();
											}
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.Text:
									{
										if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											artifactCustomProperty[fieldName] = GlobalFunctions.HtmlScrubInput(dataItemField.TextValue).ToDatabaseSerialization();
										}
									}
									break;

								case CustomProperty.CustomPropertyTypeEnum.User:
									{
										if (!dataItemField.IntValue.HasValue)
										{
											//The custom properties are checked for their options later on
											//so we only need to check the workflow required field right now
											if (isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												//All custom properties can physically store nulls, the 'allow empty' option is checked separately in the service itself
												artifactCustomProperty[fieldName] = null;
											}
										}
										else
										{
											artifactCustomProperty[fieldName] = dataItemField.IntValue.ToDatabaseSerialization();
										}
									}
									break;
							}
						}
					}
					else
					{
						string fieldCaption = dataItemField.Caption;
						if (String.IsNullOrWhiteSpace(fieldCaption))
						{
							//If we don't have a caption, just use the field name
							fieldCaption = fieldName;
						}

						//This is a standard field, so make sure it's on the datarow
						if (artifact.ContainsProperty(fieldName))
						{
                            //Query the EF model to see if it is nullable
                            EntityProperty entityProperty = ManagerBase.GetPropertyInfo(artifact.GetType(), fieldName);
                            bool isNullable = artifact.Properties[fieldName].IsNullable();
                            if (entityProperty != null && isNullable)
                            {
                                isNullable = entityProperty.Nullable;
                            }

							switch (dataItemField.FieldType)
							{
								case Artifact.ArtifactFieldTypeEnum.NameDescription:
									{
										//This field is always required so no need to check workflow
										if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
										{
											AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
										}
										else
										{
											string name = GlobalFunctions.HtmlScrubInput(dataItemField.TextValue);
											artifact[fieldName] = name;
										}
									}
									break;

								case Artifact.ArtifactFieldTypeEnum.DateTime:
									{
                                        //Dates are passed as strings, DateTimes are passed as Date objects
                                        //Need to check both
										if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
										{
                                            if (dataItemField.DateValue.HasValue)
                                            {
                                                artifact[fieldName] = dataItemField.DateValue.Value;

                                                //Convert into UTC before saving on the datarow
                                                //artifact[fieldName] = GlobalFunctions.UniversalizeDate(dataItemField.DateValue.Value);
                                            }
                                            else
                                            {
                                                //Make sure that this row can handle nullable values
                                                if (!isNullable || isRequiredByWorkflow)
                                                {
                                                    AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
                                                }
                                                else
                                                {
                                                    artifact[fieldName] = null;
                                                }
                                            }
										}
										else
										{
											DateTime dateTime;
											if (!DateTime.TryParse(dataItemField.TextValue, out dateTime))
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
											}
											else
											{
                                                //To prevent dates changing just because someone modified another field
                                                //we check to see if the UTC datatime actually changed
                                                bool dateChanged = true;
                                                if (dataItemField.DateValue.HasValue && artifact[fieldName] is DateTime)
                                                {
                                                    if (GlobalFunctions.UniversalizeDate(dataItemField.DateValue.Value) == (DateTime)artifact[fieldName])
                                                    {
                                                        dateChanged = false;
                                                    }
                                                }
                                                //Convert into UTC before saving on the datarow
                                                if (dateChanged)
                                                {
                                                    artifact[fieldName] = GlobalFunctions.UniversalizeDate(dateTime);
                                                }
											}
										}
									}
									break;

								case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
								case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
									{
										if (!dataItemField.IntValue.HasValue)
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
											artifact[fieldName] = dataItemField.IntValue;
										}
									}
									break;

								case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
                                            //See if we have a Y/N string field or a boolean
                                            if (artifact[fieldName] is String)
                                            {
                                                artifact[fieldName] = dataItemField.TextValue;
                                            }
                                            if (artifact[fieldName] is Boolean)
                                            {
                                                artifact[fieldName] = dataItemField.TextValue.FromDatabaseSerialization_Boolean();
                                            }
                                        }
									}
									break;

								case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
                                            //Parse the value into a 'multi-value'
                                            MultiValueFilter multiValue;
                                            if (!MultiValueFilter.TryParse(dataItemField.TextValue, out multiValue))
                                            {
                                                AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
                                            }
                                            else
                                            {
                                                List<int> values = multiValue.Values;
                                                artifact[fieldName] = values.ToDatabaseSerialization();
                                            }
                                        }
                                    }
									break;

								case Artifact.ArtifactFieldTypeEnum.Decimal:
									{
										if (String.IsNullOrEmpty(dataItemField.TextValue))
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
											decimal decValue;
											if (!Decimal.TryParse(dataItemField.TextValue, out decValue))
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldIncorrectFormat, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = decValue;
											}
										}
									}
									break;

								case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
								case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
									{
										if (!dataItemField.IntValue.HasValue)
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
											artifact[fieldName] = dataItemField.IntValue;
										}
									}
									break;

								case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
								case DataModel.Artifact.ArtifactFieldTypeEnum.Html:
									{
										if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
										{
											//Make sure that this row can handle nullable values
                                            if (!isNullable || isRequiredByWorkflow)
											{
												AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = fieldName, Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, fieldCaption) });
											}
											else
											{
												artifact[fieldName] = null;
											}
										}
										else
										{
											artifact[fieldName] = GlobalFunctions.HtmlScrubInput(dataItemField.TextValue);
										}
									}
									break;
							}
						}
					}

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds the shape of the dynamic columns to the data item
		/// </summary>
		/// <param name="lookupRetrieval">The delegate used to get lookup values</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="dataItem">The data item</param>
		/// <param name="dataItemField">The data item field</param>
		/// <param name="filterList">The list of filters</param>
		/// <param name="artifactType">The type of artifact</param>
        /// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="equalizerShapeRetrieval">The function that populates the 'equalizer'</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all fields for an artifact</param>
		protected void AddDynamicColumns(DataModel.Artifact.ArtifactTypeEnum artifactType, LookupRetrieval lookupRetrieval, int projectId, int projectTemplateId, int userId, DataItem dataItem, Hashtable filterList, EqualizerShapeRetrieval equalizerShapeRetrieval, bool returnJustListFields = true)
		{
			const string METHOD_NAME = "AddDynamicColumns";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataItemField dataItemField;
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactListFieldDisplay> artifactFields;
				if (returnJustListFields)
				{
					artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId, userId, artifactType);
				}
				else
				{
					List<ArtifactFieldDisplay> artifactFieldsAll = artifactManager.ArtifactField_RetrieveAll(projectId, artifactType);
					//Convert to the same object as for list fields (make the code easier)
					artifactFields = new List<ArtifactListFieldDisplay>();
					foreach (ArtifactFieldDisplay field in artifactFieldsAll)
					{
						ArtifactListFieldDisplay listField = new ArtifactListFieldDisplay();
						listField.ArtifactFieldTypeId = field.ArtifactFieldTypeId;
						listField.Name = field.Name;
						listField.Caption = field.Caption;
						listField.IsVisible = true;
						listField.LookupProperty = field.LookupProperty;
						artifactFields.Add(listField);
					}
				}
				int visibleColumnCount = 0;
				foreach (ArtifactListFieldDisplay artifactField in artifactFields)
				{
					//Only show visible columns unless we have a user id = UserInternal
					if (artifactField.IsVisible || userId == Business.UserManager.UserInternal)
					{
						visibleColumnCount++;
						//We need to get the datatype of this field
						string fieldName = artifactField.Name;
						string lookupField = artifactField.LookupProperty;

						//Create the template item field
						dataItemField = new DataItemField();
						dataItemField.FieldName = fieldName;
						dataItemField.AllowDragAndDrop = true;
						dataItemField.FieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactField.ArtifactFieldTypeId;
                        dataItemField.Width = artifactField.Width;

						//Populate the shape depending on the type of field
						switch (dataItemField.FieldType)
						{
							case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
								dataItemField.LookupName = lookupField;

								//Set the list of possible lookup values
                                //Only do if we are on a list page, otherwise we don't need them
								if (lookupRetrieval != null && returnJustListFields)
								{
									dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
								}

								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName))
								{
									if (filterList[fieldName].GetType() == typeof(int))
									{
										dataItemField.IntValue = (int)filterList[fieldName];
									}
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
								{
									dataItemField.LookupName = lookupField;

                                    //Set the list of possible lookup values
                                    //Only do if we are on a list page, otherwise we don't need them
                                    if (lookupRetrieval != null && returnJustListFields)
									{
										dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
									}

									//Set the filter value (if one is set)
									if (filterList != null && filterList.Contains(fieldName))
									{
										//handle single-value and multi-value cases correctly
										if (filterList[fieldName] is MultiValueFilter)
										{
											MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[fieldName];
											dataItemField.TextValue = multiValueFilter.ToString();
										}
										if (filterList[fieldName] is Int32)
										{
											int singleValueFilter = (int)filterList[fieldName];
											dataItemField.TextValue = singleValueFilter.ToString();
										}
									}
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
								{
									//Set the list of possible lookup values
									if (lookupRetrieval != null)
									{
										dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
									}

									//Set the filter value (if one is set)
									if (filterList != null && filterList.Contains(fieldName))
									{
										//handle single-value and multi-value cases correctly
										if (filterList[fieldName] is MultiValueFilter)
										{
											MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[fieldName];
											dataItemField.TextValue = multiValueFilter.ToString();
										}
										if (filterList[fieldName] is Int32)
										{
											int singleValueFilter = (int)filterList[fieldName];
											dataItemField.TextValue = singleValueFilter.ToString();
										}
									}
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyLookup:
							case DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyMultiList:
								//Set the list of possible lookup values
								if (lookupRetrieval != null)
								{
									dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
								}

								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName))
								{
									//handle single-value and multi-value cases correctly
									if (filterList[fieldName] is MultiValueFilter)
									{
										MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[fieldName];
										dataItemField.TextValue = multiValueFilter.ToString();
									}
									if (filterList[fieldName] is Int32)
									{
										int singleValueFilter = (int)filterList[fieldName];
										dataItemField.TextValue = singleValueFilter.ToString();
									}
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
								if (equalizerShapeRetrieval != null)
								{
									equalizerShapeRetrieval(fieldName, dataItemField, filterList, projectId, projectTemplateId);
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
								//The flag need to be a drop-down despite being text
								if (lookupRetrieval != null)
								{
									dataItemField.Lookups = lookupRetrieval(fieldName, projectId, projectTemplateId);
								}
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName))
								{
									dataItemField.TextValue = (string)filterList[fieldName];
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
							case DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription:
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName))
								{
									dataItemField.TextValue = (string)filterList[fieldName];
								}
								break;
							case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
							case DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyDate:
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName))
								{
									//Need to convert into the displayable date form
									DateRange dateRange = (DateRange)filterList[fieldName];
									string textValue = null;
									if (dateRange.StartDate.HasValue)
									{
										textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.StartDate.Value);
									}
									textValue += "|";
									if (dateRange.EndDate.HasValue)
									{
										textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, dateRange.EndDate.Value);
									}
									dataItemField.TextValue = textValue;
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is EffortRange)
								{
									//Need to convert into the displayable range form
									EffortRange effortRange = (EffortRange)filterList[fieldName];
									dataItemField.TextValue = effortRange.ToString();
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Decimal:
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is DecimalRange)
								{
									//Need to convert into the displayable date form
									DecimalRange decimalRange = (DecimalRange)filterList[fieldName];
									dataItemField.TextValue = decimalRange.ToString();
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
								//Set the filter value (if one is set)
								if (filterList != null && filterList.Contains(fieldName) && filterList[fieldName] is IntRange)
								{
									//Need to convert into the displayable date form
									IntRange intRange = (IntRange)filterList[fieldName];
									dataItemField.TextValue = intRange.ToString();
								}
								break;

							case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
								//Set the filter value
								if (filterList != null && filterList.Contains(fieldName))
								{
									dataItemField.IntValue = (int)filterList[fieldName];
									dataItemField.TextValue = dataItemField.IntValue.ToString();
								}
								break;
						}

						//See if we have a localized caption, otherwise use the default
						//For the primary key fields, need to always use the localized name for ID
						string localizedName = Resources.Fields.ResourceManager.GetString(fieldName);
						if (dataItemField.FieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
						{
							localizedName = Resources.Fields.ID;
						}
						if (String.IsNullOrEmpty(localizedName))
						{
							dataItemField.Caption = artifactField.Caption;
						}
						else
						{
							dataItemField.Caption = localizedName;
						}
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Converts a sorted list in the dictionary that can be consumed by the webservice
		/// </summary>
		/// <param name="lookupValues">sorted list of name/value pairs</param>
		/// <returns>dictionary of name/value pairs</returns>
		protected JsonDictionaryOfStrings ConvertLookupValues(SortedList<int, string> lookupValues)
		{
			//Iterate through the rows to extract the data
			JsonDictionaryOfStrings newLookupValues = new JsonDictionaryOfStrings();
			foreach (KeyValuePair<int, string> lookup in lookupValues)
			{
				newLookupValues.Add(lookup.Key.ToString(), lookup.Value);
			}

			return newLookupValues;
		}

		/// <summary>
		/// Adds a dataset lookup to the array that will be consumed by the webservice
		/// </summary>
		/// <param name="lookupValues">Partially filled dictionary that we add the values to</param>
		/// <param name="entities">The lookup list</param>
		/// <param name="nameField">The name of the field used to store the lookup name</param>
		/// <param name="valueField">The name of the field used to store the lookup value</param>
		/// <remarks>This version is used when you need to partially populate the dictionary first</remarks>
		protected void AddLookupValues(JsonDictionaryOfStrings lookupValues, List<Entity> entities, string nameField, string valueField)
		{
			//Iterate through the rows to extract the data
			foreach (Entity entity in entities)
			{
				if (entity[nameField] != null && entity[valueField] != null)
				{
					lookupValues.Add(entity[nameField].ToString(), entity[valueField].ToString());
				}
			}
		}

		/// <summary>
		/// Converts a list of entities into the array that can be consumed by the webservice
		/// </summary>
		/// <param name="entities">The list of entities</param>
		/// <param name="nameField">The name of the field used to store the lookup name</param>
		/// <param name="valueField">The name of the field used to store the lookup value</param>
		/// <returns>dictionary of name/value pairs</returns>
		protected JsonDictionaryOfStrings ConvertLookupValues(List<Entity> entities, string nameField, string valueField)
		{
			//Iterate through the rows to extract the data
			JsonDictionaryOfStrings lookupValues = new JsonDictionaryOfStrings();
			foreach (Entity entity in entities)
			{
				if (entity[nameField] != null && entity[valueField] != null)
				{
					lookupValues.Add(entity[nameField].ToString(), entity[valueField].ToString());
				}
			}

			return lookupValues;
		}

		/// <summary>
		/// Converts a entity list lookup containing hiearchical data into the array that can be consumed by the webservice
		/// </summary>
		/// <param name="entities">The list of entities</param>
		/// <param name="nameField">The name of the field used to store the lookup name</param>
		/// <param name="valueField">The name of the field used to store the lookup value</param>
		/// <param name="alternateField">The name of the field used to store the alternate item flag</param>
		/// <param name="alternateField">The name of the field used to store the active item flag</param>
		/// <param name="summaryField">The name of the field used to store the summary/folder item flag</param>
		/// <returns>dictionary of name/value pairs</returns>
		/// <param name="indentField">The name of the field that holds the alphanumeric indent level</param>
		protected JsonDictionaryOfStrings ConvertLookupValues(List<Entity> entities, string nameField, string valueField, string indentField, string summaryField = "", string alternateField = "", string activeField = "")
		{
			//Iterate through the rows to extract the data
			JsonDictionaryOfStrings lookupValues = new JsonDictionaryOfStrings();
			foreach (Entity entity in entities)
			{
				if (entity[nameField] != null && entity[valueField] != null)
				{
					//Append the indent level, summary and alternate item flags to the indent level
					string hierarchicalValue = entity[nameField].ToString() +
						"_" + (entity[indentField].ToString().Length / 3).ToString();
					if (summaryField == "")
					{
						hierarchicalValue += "_N";
					}
					else
					{
                        if (entity[summaryField] is bool)
                        {
                            hierarchicalValue += "_" + ((bool)entity[summaryField]).ToFlagValue();
                        }
                        else
                        {
                            hierarchicalValue += "_" + entity[summaryField].ToString();
                        }
					}
					if (alternateField == "")
					{
						hierarchicalValue += "_N";
					}
					else
					{
                        if (entity[alternateField] is bool)
                        {
                            hierarchicalValue += "_" + ((bool)entity[alternateField]).ToFlagValue();
                        }
                        else
                        {
                            hierarchicalValue += "_" + entity[alternateField].ToString();
                        }
					}
					if (activeField == "")
					{
						hierarchicalValue += "_Y";
					}
					else
					{
                        if (entity[activeField] is bool)
                        {
                            hierarchicalValue += "_" + ((bool)entity[activeField]).ToFlagValue();
                        }
                        else
                        {
                            hierarchicalValue += "_" + entity[activeField].ToString();
                        }
					}
					lookupValues.Add(hierarchicalValue, entity[valueField].ToString());
				}
			}

			return lookupValues;
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <param name="collectionName">The name of the filters collection</param>
		/// <param name="initialFilter">Is this the first time a filter was applied?</param>
		/// <returns>Validation message (or empty string if none)</returns>
		protected string UpdateFilters(int userId, int projectId, JsonDictionaryOfStrings filters, string collectionName, DataModel.Artifact.ArtifactTypeEnum artifactType, out bool initialFilter)
		{
			const string METHOD_NAME = "UpdateFilters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                //Get the current filters, check if we have a project or user collection
                Hashtable savedFilters;
                if (projectId < 1)
                {
                    savedFilters = GetUserSettings(userId, collectionName);
                }
                else
                {
                    savedFilters = GetProjectSettings(userId, projectId, collectionName);
                }
				int oldFilterCount = savedFilters.Count;
				savedFilters.Clear(); //Clear the filters

				//We also need to get the list of visible artifact fields so that we can determine the type of data correctly
				//(this also includes any custom property fields)
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId, userId, artifactType);

				//Iterate through the filters, updating the project collection
				if (filters != null)
				{
					foreach (KeyValuePair<string, string> filter in filters)
					{
						//We need to ensure that values are cast to the appropriate types and any validations/conversions performed
						string filterName = filter.Key;
						DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text; //Default to text
						//Now get the type of field that we have
						ArtifactListFieldDisplay artifactField = artifactFields.FirstOrDefault(f => f.Name == filterName);
						if (artifactField == null)
						{
							//This is the case when the field is not a configurable one (or not part of an artifact)
							if (filterName.Length > 2 && filterName.Substring(filterName.Length - 2, 2) == "Id")
							{
								//If the field ends in an Id then it must be the identifier unless the value contains commas
								//when it needs to be set as a lookup
								if (filter.Value.Contains(","))
								{
									artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
								}
								else
								{
									artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
								}
							}
							else if (filterName.Length > 4 && filterName.Substring(filterName.Length - 4, 4) == "Date")
							{
								//If the field ends in 'Date' then it must be a date
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
							}
							else
							{
								//Otherwise assume it's a basic text field
								artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
							}
						}
						else
						{
							int artifactFieldTypeId = artifactField.ArtifactFieldTypeId;
							artifactFieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactFieldTypeId;
						}

						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyLookup || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Lookup ||
							artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyMultiList || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.MultiList)
						{
							//These are strings containing a single value or multiple values (comma-separated)
							MultiValueFilter mutiValueFilter;
							if (MultiValueFilter.TryParse(filter.Value, out mutiValueFilter))
							{
								savedFilters.Add(filterName, mutiValueFilter);
							}
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup)
						{
							//All list custom properties are numeric ids
							int filterValueInt = -1;
							if (Int32.TryParse(filter.Value, out filterValueInt))
							{
								savedFilters.Add(filterName, filterValueInt);
							}
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
						{
							//All identifiers must be numeric
							int filterValueInt = -1;
							if (Int32.TryParse(filter.Value, out filterValueInt))
							{
								savedFilters.Add(filterName, filterValueInt);
							}
							else
							{
								initialFilter = false;
								return String.Format(Resources.Messages.ListServiceBase_EnterValidInteger, filterName);
							}
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Flag)
						{
							savedFilters.Add(filterName, filter.Value);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.DateTime || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyDate)
						{
							//If we have date values, need to make sure that they are indeed date-ranges
							//Otherwise we need to throw back a friendly error message
							//We leave the date/time in Localtime because the filtering code handles the UTC conversion
							DateRange dateRange;
							if (!DateRange.TryParse(filter.Value, out dateRange))
							{
								initialFilter = false;
								return String.Format(Resources.Messages.ListServiceBase_EnterValidDateRange, filterName);
							}
							savedFilters.Add(filterName, dateRange);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Integer)
						{
							//If we have integer values, need to make sure that they are indeed integral
							IntRange intRange;
							if (!IntRange.TryParse(filter.Value, out intRange))
							{
								initialFilter = false;
								return String.Format(Resources.Messages.ListServiceBase_EnterValidInteger, filterName);
							}
							savedFilters.Add(filterName, intRange);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval)
						{
							//If we have effort values, need to make sure that they are indeed decimals
							EffortRange effortRange;
							if (!EffortRange.TryParse(filter.Value, out effortRange))
							{
								initialFilter = false;
								return String.Format(Resources.Messages.ListServiceBase_EnterValidEffortRange, filterName);
							}
							savedFilters.Add(filterName, effortRange);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Decimal)
						{
							//If we have decimal values, need to make sure that they are indeed decimals
							DecimalRange decimalRange;
							if (!DecimalRange.TryParse(filter.Value, out decimalRange))
							{
								initialFilter = false;
								return String.Format(Resources.Messages.ListServiceBase_EnterValidDecimal, filterName);
							}
							savedFilters.Add(filterName, decimalRange);
						}
						if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Text || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription)
						{
							//For text, just save the value
							savedFilters.Add(filterName, filter.Value);
						}
					}
				}
                if (savedFilters is ProjectSettingsCollection)
                {
                    ((ProjectSettingsCollection)savedFilters).Save();
                }
                if (savedFilters is UserSettingsCollection)
                {
                    ((UserSettingsCollection)savedFilters).Save();
                }

                //Specify if this is the first time a filter was applied - some artifacts need this information
                if (oldFilterCount == 0 && savedFilters.Count > 0)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					initialFilter = true;
					return "";  //Success
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				initialFilter = false;
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Handles custom operations that are artifact/page-specific (buttons, drop-downs, etc.)
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="operation">The name of the operation</param>
		/// <param name="value">The parameter value being passed to the operation</param>
		/// <returns>Any error messages</returns>
		public virtual string CustomOperation(int projectId, string operation, string value)
		{
			throw new NotImplementedException("This operation is not available");
		}

		/// <summary>
		/// Changes the order of columns in a grid. Needs to be overidden by the subclass
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="newIndex">The new index of the column's position</param>
		public virtual void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex)
		{
			throw new NotImplementedException("This operation is not available");
		}

        public virtual int? Form_Delete(int projectId, int artifactId)
        {
            throw new NotImplementedException("This operation is not available");
        }

        public virtual int? Form_Clone(int projectId, int artifactId)
        {
            throw new NotImplementedException("This operation is not available");
        }

        public virtual int? Form_New(int projectId, int artifactId)
        {
            throw new NotImplementedException("This operation is not available");
        }

		public int? Form_InsertChild(int projectId, int artifactId)
		{
			throw new NotImplementedException("This operation is not available");
		}

		/// <summary>
		/// Changes the width of a column in a grid. Needs to be overidden by the subclass
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="width">The new width of the column (in pixels)</param>
		public virtual void List_ChangeColumnWidth(int projectId, string fieldName, int width)
        {
            throw new NotImplementedException("This operation is not available");
        }

		/// <summary>
		/// Handles custom operations that are artifact/page-specific (buttons, drop-downs, etc.)
		/// and need more than one simple parameter
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="operation">The name of the operation</param>
		/// <param name="parameters">A name/value dictionary of parameters</param>
		/// <returns>Any error messages</returns>
		public virtual string CustomOperationEx(int projectId, string operation, JsonDictionaryOfStrings parameters)
		{
			throw new NotImplementedException("This operation is not available");
		}

		/// <summary>
		/// Gets the list of display names for the specified filters
		/// </summary>
		/// <param name="projectId">The id of the project (only needed if we want to include custom properties)</param>
		/// <param name="artifactType">The type of artifact (only needed if we want to include custom properties)</param>
		/// <param name="filters">The filter collection</param>
		/// <returns>The list of display names</returns>
        /// <param name="projectTemplateId">The id of the project template</param>
		protected List<string> GetFilterNames(Hashtable filters, int? projectId = null, int? projectTemplateId = null, Artifact.ArtifactTypeEnum artifactType = Artifact.ArtifactTypeEnum.None)
		{
			List<string> filterNames = null;

			if (filters != null && filters.Count > 0)
			{
				//Get the filter names
				filterNames = new List<string>();
				foreach (DictionaryEntry entry in filters)
				{
					if (entry.Key is String)
					{
						string fieldName = (string)entry.Key;
						//See if this is a custom property or not
						int? customPropertyNumber = null;
						if (projectId.HasValue && artifactType != Artifact.ArtifactTypeEnum.None)
						{
							customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(fieldName);
						}
						if (customPropertyNumber.HasValue)
						{
							CustomPropertyManager customPropertyManager = new CustomPropertyManager();
							CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId.Value, artifactType, customPropertyNumber.Value, false);
							if (customProperty != null)
							{
								filterNames.Add(customProperty.Name);
							}
						}
						else
						{
                            //For incidents, if we have the 'TaskProgress' column, need to call it just Progress
                            if (artifactType == Artifact.ArtifactTypeEnum.Incident && fieldName == "ProgressId")
                            {
                                filterNames.Add(Resources.Fields.Progress);
                            }
                            else
                            {
                                string fieldCaption = Resources.Fields.ResourceManager.GetString(fieldName);
                                if (!String.IsNullOrEmpty(fieldCaption))
                                {
                                    filterNames.Add(fieldCaption);
                                }
                            }
						}
					}
				}
			}

			return filterNames;
		}

        /// <summary>
        /// Retrieves a list of saved filters for the current user/project
        /// </summary>
        /// <param name="includeShared">Should we include shared ones</param>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <returns>Dictionary of saved filters</returns>
        protected JsonDictionaryOfStrings RetrieveFilters(int userId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, bool includeShared)
		{
			const string METHOD_NAME = "RetrieveFilters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of saved filters for the current user/project, including any shared ones
				Business.SavedFilterManager savedFilterManager = new Business.SavedFilterManager();
				List<DataModel.SavedFilter> savedFilters = savedFilterManager.Retrieve(userId, projectId, artifactType, includeShared);

				//Convert the dataset into a dictionary and return
				JsonDictionaryOfStrings savedFiltersDic = ConvertLookupValues(savedFilters.OfType<DataModel.Entity>().ToList(), "SavedFilterId", "Name");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return savedFiltersDic;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Restores a previously saved filter and applies that filter to the appropriate artifact
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="savedFilterId">The id of the saved filter to apply</param>
		/// <returns>An error message if there is one</returns>
		public string RestoreSavedFilter(int savedFilterId)
		{
			const string METHOD_NAME = "RestoreSavedFilter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First restore the saved filter to determine the artifact and project id
				Business.SavedFilterManager savedFilterManager = new Business.SavedFilterManager();
				savedFilterManager.Restore(savedFilterId);
				if (!savedFilterManager.ProjectId.HasValue)
				{
					return "Unable to retrieve saved search due to missing project id.";
				}
				int projectId = savedFilterManager.ProjectId.Value;
				DataModel.Artifact.ArtifactTypeEnum artifactType = savedFilterManager.Type;

				//Make sure we're authenticated
				if (!this.CurrentUserId.HasValue)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
				}
				int userId = this.CurrentUserId.Value;

				//Make sure we're authorized
				Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, artifactType);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Get the appropriate settings collections for the artifact type in question
				//Sortable artifacts actually have two collections
				string filterCollectionName = "";
				string sortCollectionName = "";
				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS;
                        break;
					case DataModel.Artifact.ArtifactTypeEnum.Release:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST;
						break;
					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS;
                        break;
					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS;
                        break;
					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
						sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION;
						break;
					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST;
						sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION;
						break;
					case DataModel.Artifact.ArtifactTypeEnum.Task:
						filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
						sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION;
						break;
                    case DataModel.Artifact.ArtifactTypeEnum.Risk:
                        filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL;
                        break;
                    case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
                        filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION;
                        break;
                    case DataModel.Artifact.ArtifactTypeEnum.Document:
                        filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST;
                        sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS;
                        break;
                }

                //Now restore the appropriate project settings collection
                if (projectId == -1 || artifactType == DataModel.Artifact.ArtifactTypeEnum.None || filterCollectionName == "")
				{
					return "Unable to retrieve saved search.";
				}
				ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, userId, filterCollectionName);
				ProjectSettingsCollection sortProjectSettings = null;
				if (sortCollectionName != "")
				{
					sortProjectSettings = new ProjectSettingsCollection(projectId, userId, sortCollectionName);
				}

				//Now populate the collection(s) from the saved filter
				savedFilterManager.Populate(filterProjectSettings, sortProjectSettings, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}
	}
}
