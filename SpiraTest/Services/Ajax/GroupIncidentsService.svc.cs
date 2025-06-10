using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web.Security;
using Microsoft.Security.Application;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System.Collections;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>Communicates with the SortableGrid AJAX component for displaying/updating incidents data</summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class GroupIncidentsService : SortedListServiceBase, IGroupIncidentsService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.GroupIncidentsService::";

        #region SortedList methods

        /// <summary>Returns a list of incidents in the system for the specific project group</summary>
        /// <param name="userId">The user we're viewing the incidents as</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(/*Project Group*/ int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Store project group id in correctly named variable to make code easier to understand
            int projectGroupId = projectId;

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the incident business objects
                IncidentManager incidentManager = new IncidentManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Get the template associated with the program
                int? projectTemplateId = new ProjectGroupManager().RetrieveById(projectGroupId).ProjectTemplateId;

                //Now get the list of populated filters and the current sort
                Hashtable filterList = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_FILTERS);
                string sortCommand = GetUserSetting(userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_GENERAL, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "IncidentId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Add any standard filters
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    foreach (KeyValuePair<string, object> filter in deserializedFilters)
                    {
                        filterList[filter.Key] = filter.Value;
                    }
                }

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(projectGroupId, userId, filterItem, filterList);
                dataItems.Add(filterItem);
                sortedData.FilterNames = GetFilterNames(filterList, projectGroupId, projectTemplateId, Artifact.ArtifactTypeEnum.Incident);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information
                UserSettingsCollection paginationSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_GENERAL);
                paginationSettings.Restore();
                //Default values
                int paginationSize = 15;
                int currentPage = 1;
                if (paginationSettings["NumberRowsPerPage"] != null)
                {
                    paginationSize = (int)paginationSettings["NumberRowsPerPage"];
                }
                if (paginationSettings["CurrentPage"] != null)
                {
                    currentPage = (int)paginationSettings["CurrentPage"];
                }
                //Get the number of incidents in the project
                int artifactCount = incidentManager.Incident_CountForGroup(projectGroupId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
                //Make sure that the current page is not larger than the number of pages or less than 1
                if (currentPage > pageCount)
                {
                    currentPage = pageCount;
                    paginationSettings["CurrentPage"] = currentPage;
                    paginationSettings.Save();
                }
                if (currentPage < 1)
                {
                    currentPage = 1;
                    paginationSettings["CurrentPage"] = currentPage;
                    paginationSettings.Save();
                }

                //**** Now we need to actually populate the rows of data to be returned ****
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                List<IncidentView> incidents = incidentManager.Incident_RetrieveForGroup(projectGroupId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = incidents.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Iterate through all the incidents and populate the dataitem
                foreach (IncidentView incident in incidents)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, incident);
                    dataItems.Add(dataItem);
                }

                //Also include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectGroupId);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return sortedData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectGroupId">The project group we're interested in</param>
        /// <param name="userId">The user we're viewing the incidents as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
        protected void PopulateShape(int projectGroupId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //Get the list of fields to display for the incident group list (vs. the project list)
            IncidentManager incidentManager = new IncidentManager();
            List<ArtifactListFieldDisplay> artifactFields = incidentManager.RetrieveFieldsForProjectGroupLists(projectGroupId, userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);

            int visibleColumnCount = 0;
            DataItemField dataItemField;
            foreach (ArtifactListFieldDisplay artifactField in artifactFields)
            {
                //Only show visible columns
                if (artifactField.IsVisible)
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
                            dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

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
                                dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

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
                                dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);

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

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
                            {
                                PopulateEqualizerShape(fieldName, dataItemField, filterList, projectGroupId);
                            }
                            break;

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
                            //Set the list of possible lookup values
                            dataItemField.Lookups = GetLookupValues(fieldName, projectGroupId);


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

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="incidentView">The entity containing the data</param>
        protected void PopulateRow(SortedDataItem dataItem, IncidentView incidentView)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = incidentView.IncidentId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, incidentView.ConcurrencyDate);

            //Set the custom URL (needed since each project may be different)
            dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, incidentView.ProjectId, incidentView.IncidentId));

            //Specify if it has an attachment or not
            dataItem.Attachment = incidentView.IsAttachments;

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (incidentView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, incidentView, null, null, false, PopulateEqualizer);

                    //Apply the conditional formatting to the priority and severity columns (if displayed)
                    if (dataItemField.FieldName == "PriorityId" && incidentView.PriorityId.HasValue)
                    {
                        //Despite the name, cssClass can store either color or CSS class for SortedDataItem's
                        dataItemField.CssClass = "#" + incidentView.PriorityColor;
                    }
                    if (dataItemField.FieldName == "SeverityId" && incidentView.SeverityId.HasValue)
                    {
                        //Despite the name, cssClass can store either color or CSS class for SortedDataItem's
                        dataItemField.CssClass = "#" + incidentView.SeverityColor;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of lookup values and names for a specific lookup
        /// </summary>
        /// <param name="lookupName">The name of the lookup</param>
        /// <param name="projectGroupId">The id of the project group - needed for some lookups</param>
        /// <returns>The name/value pairs</returns>
        protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectGroupId)
        {
            const string METHOD_NAME = "GetLookupValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                JsonDictionaryOfStrings lookupValues = null;
                ReleaseManager release = new ReleaseManager();
                Business.UserManager user = new Business.UserManager();
                IncidentManager incidentManager = new IncidentManager();

                if (lookupName == "OpenerId" || lookupName == "OwnerId")
                {
                    List<ProjectResourceView> resources = new ProjectGroupManager().RetrieveResourcesForGroup(projectGroupId);
                    lookupValues = ConvertLookupValues(resources.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "ProgressId")
                {
                    lookupValues = new JsonDictionaryOfStrings(incidentManager.RetrieveProgressFiltersLookup());
                }
                if (lookupName == "ProjectId")
                {
                    List<ProjectView> projects = new ProjectManager().Project_RetrieveByGroup(projectGroupId);
                    lookupValues = ConvertLookupValues(projects.OfType<DataModel.Entity>().ToList(), "ProjectId", "Name");
                }
                if (lookupName == "IncidentStatusId")
                {
                    lookupValues = new JsonDictionaryOfStrings();
                    //Add the composite (All Open) and (All Closed) items to the incident status filter
                    //For project group lists, that's all we want to have
                    lookupValues.Add(IncidentManager.IncidentStatusId_AllOpen.ToString(), Resources.Fields.IncidentStatus_AllOpen);
                    lookupValues.Add(IncidentManager.IncidentStatusId_AllClosed.ToString(), Resources.Fields.IncidentStatus_AllClosed);
                }

                return lookupValues;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings RetrievePaginationOptions(int /*Project Group*/projectId)
        {
            const string METHOD_NAME = "RetrievePaginationOptions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Delegate to the generic method in the base class - passing the correct collection name
            JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(-1, userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_GENERAL);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return paginationDictionary;
        }

        /// <summary>
        /// Populates the equalizer type graph for the incident progress
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            const string METHOD_NAME = "PopulateEqualizer";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Explicitly recast the data-row to the type we're expecting
            IncidentView incidentView = (IncidentView)artifact;

            //Calculate the information to display
            int percentGreen;
            int percentRed;
            int percentYellow;
            int percentGray;
            Incident incident = incidentView.ConvertTo<IncidentView, Incident>();
            string tooltipText = IncidentManager.CalculateProgress(incident, out percentGreen, out percentRed, out percentYellow, out percentGray);

            //Now populate the equalizer graph
            dataItemField.EqualizerGreen = percentGreen;
            dataItemField.EqualizerRed = percentRed;
            dataItemField.EqualizerYellow = percentYellow;
            dataItemField.EqualizerGray = percentGray;

            //Populate Tooltip
            dataItemField.TextValue = "";
            dataItemField.Tooltip = tooltipText;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectGroupId">The project group we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectGroupId)
        {
            //Check to see if this is a field we can handle
            if (fieldName == "ProgressId")
            {
                dataItemField.FieldName = "ProgressId";
                string filterLookupName = fieldName;
                dataItemField.Lookups = GetLookupValues(filterLookupName, projectGroupId);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(filterLookupName))
                {
                    dataItemField.IntValue = (int)filterList[filterLookupName];
                }
            }
        }

        /// <summary>Returns a plain-text version of the artifact name/description typically used in dynamic tooltips</summary>
        /// <param name="incidentId">The id of the incident to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        /// <remarks>For incidents also includes the most recent resolution</remarks>
        public string RetrieveNameDesc(int? projectId, int incidentId, int? displayTypeId)
        {
            const string METHOD_NAME = "RetrieveNameDesc";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            if (!projectId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId.Value;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the incident business object
                IncidentManager incidentManager = new IncidentManager();

                //Now retrieve the specific incident - handle quietly if it doesn't exist
                try
                {
                    Incident incident = incidentManager.RetrieveById(incidentId, true);
                    string tooltip;
                    tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(incident.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(incident.Description);

                    //See if we have any comments to append
                    if (incident.Resolutions.Count > 0)
                    {
                        IncidentResolution resolution = incident.Resolutions.OrderByDescending(r => r.CreationDate).First();

                        tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
                            GlobalFunctions.LocalizeDate(resolution.CreationDate).ToShortDateString(),
                            GlobalFunctions.HtmlRenderAsPlainText(resolution.Resolution),
                            resolution.Creator.FullName
                            );
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the incident, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for incident");
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return Resources.Messages.Global_UnableRetrieveTooltip;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the current sort stored in the system (property and direction)
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="sortProperty">The artifact property we want to sort on</param>
        /// <param name="sortAscending">Are we sorting ascending or not</param>
        /// <returns>Any error messages</returns>
        public string SortedList_UpdateSort(int /*ProjectGroupId*/ projectId, string sortProperty, bool sortAscending, int? displayTypeId)
        {
            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Call the base method with the appropriate settings collection  (-1 = user settings instead of project settings)
            return base.UpdateSort(userId, -1, sortProperty, sortAscending, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_GENERAL);
        }

        /// <summary>
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
        /// <returns>Any error messages</returns>
        public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
        {
            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Call the base method with the appropriate settings collection (-1 = user settings instead of project settings)
            return base.UpdateFilters(userId, -1, filters, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_FILTERS, DataModel.Artifact.ArtifactTypeEnum.Incident);
        }

        /// <summary>
        /// Updates the size of pages returned and the currently selected page
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
        /// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
        public void UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            const string METHOD_NAME = "UpdatePagination";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the pagination settings collection and update
                UserSettingsCollection paginationSettings = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_GENERAL);
                paginationSettings.Restore();
                if (pageSize != -1)
                {
                    paginationSettings["NumberRowsPerPage"] = pageSize;
                }
                if (currentPage != -1)
                {
                    paginationSettings["CurrentPage"] = currentPage;
                }
                paginationSettings.Save();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds/removes a column from the list of fields displayed in the current view
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project group we're interested in</param>
        /// <param name="fieldName">The name of the column we displaying/hiding</param>
        public void ToggleColumnVisibility(int /*ProjectGroupId*/projectId, string fieldName)
        {
            const string METHOD_NAME = "ToggleColumnVisibility";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            int projectGroupId = projectId;
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Toggle the status of the appropriate field name
                IncidentManager incidentManager = new IncidentManager();
                incidentManager.ToggleProjectGroupColumnVisibility(userId, projectGroupId, fieldName, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region NotImplemented methods

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException();
        }

        public void SortedList_Copy(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException();
        }

        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
