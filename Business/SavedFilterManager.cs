using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class allows the storage and retrieval of saved project filter/sort settings
    /// </summary>
    public class SavedFilterManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.SavedFilterManager::";

        protected SavedFilter restoredSavedFilter = null;

        #region Properties

        /// <summary>
        /// Gets the id of the project the restored saved filter is associated with
        /// </summary>
        public int? ProjectId
        {
            get
            {
                if (restoredSavedFilter == null)
                {
                    return null;
                }
                return restoredSavedFilter.ProjectId;
            }
        }

        /// <summary>
        /// Gets the id of the user the restored saved filter is associated with
        /// </summary>
        public int? UserId
        {
            get
            {
                if (restoredSavedFilter == null)
                {
                    return null;
                }
                return restoredSavedFilter.UserId;
            }
        }

        /// <summary>
        /// Is this a shared filter or not?
        /// </summary>
        public bool IsShared
        {
            get
            {
                if (restoredSavedFilter == null)
                {
                    return false;
                }
                return restoredSavedFilter.IsShared;
            }
        }

        /// <summary>
        /// The name of the saved filter
        /// </summary>
        public string Name
        {
            get
            {
                if (restoredSavedFilter == null)
                {
                    return "";
                }
                return restoredSavedFilter.Name;

            }
        }

        /// <summary>
        /// Gets the type of artifact the restored saved filter is associated with
        /// </summary>
        public DataModel.Artifact.ArtifactTypeEnum Type
        {
            get
            {
                if (restoredSavedFilter == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                return (DataModel.Artifact.ArtifactTypeEnum)restoredSavedFilter.ArtifactTypeId;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the list of saved filters for a particular user
        /// </summary>
        /// <param name="savedFilterId">The id of the saved filter</param>
        /// <param name="projectId">The id of the project, or null for all</param>
        /// <returns>The dataset of the filter and associated entries</returns>
        public List<SavedFilter> Retrieve(int userId, int? projectId)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<SavedFilter> savedFilters;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from s in context.SavedFilters.Include("Project").Include("ArtifactType")
                                where s.UserId == userId && s.Project.IsActive
                                select s;

                    //Add on the project filter if necessary
                    if (projectId.HasValue)
                    {
                        int projectIdValue = projectId.Value;
                        query = query.Where(s => s.ProjectId == projectIdValue);
                    }

                    //Add the sorts
                    query = query.OrderBy(s => s.ArtifactTypeId).ThenBy(s => s.Name).ThenBy(s => s.SavedFilterId);

                    savedFilters = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return savedFilters;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of saved filters for a particular user, project and artifact type
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="includeShared">Do I want to see my reports only or all ones marked as shareable</param>
        /// <param name="artifactType">The artifact type, or null for all</param>
        /// <returns>The dataset of the filter and associated entries</returns>
        public List<SavedFilter> Retrieve(int userId, int projectId, Nullable<DataModel.Artifact.ArtifactTypeEnum> artifactType, bool includeShared)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<SavedFilter> savedFilters;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from s in context.SavedFilters.Include("Project").Include("ArtifactType").Include("User").Include("User.Profile")
                                where s.Project.IsActive && s.ProjectId == projectId
                                select s;

                    //Filter by artifact type if appropriate
                    if (artifactType.HasValue)
                    {
                        int artifactTypeId = ((int)artifactType.Value);
                        query = query.Where(s => s.ArtifactTypeId == artifactTypeId);
                    }

                    //Create the appropriate user clause
                    if (includeShared)
                    {
                        //Add on an OR condition to include any saved report that has the shared flag set
                        query = query.Where(s => s.UserId == userId || s.IsShared);
                    }
                    else
                    {
                        query = query.Where(s => s.UserId == userId);
                    }

                    //Add the sorts
                    query = query.OrderBy(s => s.Name).ThenBy(s => s.SavedFilterId);

                    savedFilters = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return savedFilters;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing saved filter in the system
        /// </summary>
        /// <param name="savedFilterId">The id of the saved filter being deleted</param>
        /// <remarks>Doesn't complain if there is no item to be deleted</remarks>
        public void Delete(int savedFilterId)
        {
            const string METHOD_NAME = "Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Delete the saved filter and associated entries
                    var query = from s in context.SavedFilters.Include("Entries")
                                where s.SavedFilterId == savedFilterId
                                select s;

                    SavedFilter savedFilter = query.FirstOrDefault();
                    if (savedFilter != null)
                    {
                        context.SavedFilters.DeleteObject(savedFilter);
                        context.SaveChanges();
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

        /// <summary>
        /// Restores back a saved filter to a member instance of that dataset
        /// </summary>
        /// <param name="savedFilterId">The id of the saved filter</param>
        /// <remarks>This must be called before accessing the ProjectId or ArtifactId public properties</remarks>
        public void Restore(int savedFilterId)
        {
            this.restoredSavedFilter = this.RetrieveById(savedFilterId);
        }

        /// <summary>
        /// Saves the supplied project settings collection of filters
        /// </summary>
        /// <param name="isShared">Is this a filter that should be shared with other project members</param>
        /// <param name="name">The name we want to save the filter as</param>
        /// <param name="artifactType">The type of artifact the filter is for</param>
        /// <param name="filterProjectSettings">The collection of filters to be saved</param>
        /// <param name="includeColumns">Should we save the column selection with the filters and sorts</param>
        public void Save(string name, DataModel.Artifact.ArtifactTypeEnum artifactType, ProjectSettingsCollection filterProjectSettings, bool isShared, bool includeColumns)
        {
            this.Save(name, artifactType, filterProjectSettings, null, isShared, includeColumns);
        }

        /// <summary>
        /// Saves the supplied project settings collection of filters
        /// </summary>
        /// <param name="name">The name we want to save the filter as</param>
        /// <param name="isShared">Is this a filter that should be shared with other project members</param>
        /// <param name="artifactType">The type of artifact the filter is for</param>
        /// <param name="filterProjectSettings">The collection of filters to be saved</param>
        /// <param name="includeColumns">Should we save the column selection with the filters and sorts</param>
        /// <param name="sortProjectSettings">The settings collection containing the sort expression</param>
        public void Save(string name, DataModel.Artifact.ArtifactTypeEnum artifactType, ProjectSettingsCollection filterProjectSettings, ProjectSettingsCollection sortProjectSettings, bool isShared, bool includeColumns)
        {
            const string METHOD_NAME = "Save";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First restore the current filters and sorts
                filterProjectSettings.Restore();
                if (sortProjectSettings != null)
                {
                    sortProjectSettings.Restore();
                }

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First create a new saved filter record
                    SavedFilter savedFilter = new SavedFilter();
                    savedFilter.UserId = filterProjectSettings.UserId;
                    savedFilter.ProjectId = filterProjectSettings.ProjectId;
                    savedFilter.ArtifactTypeId = (int)artifactType;
                    savedFilter.Name = name;
                    savedFilter.IsShared = isShared;

                    //Now iterate through the filter values and actually populate the filter entries
                    foreach (DictionaryEntry filterProjectEntry in filterProjectSettings)
                    {
                        //Get the entry key and values (convert to string and typecode)
                        string entryKey = (string)filterProjectEntry.Key;
                        object entryValue = filterProjectEntry.Value;
                        int typeCode;
                        string entryValueString = SerializeValue(entryValue, out typeCode);

                        //Populate the entry collection with a new filter entry
                        SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                        savedFilterEntry.EntryKey = entryKey;
                        savedFilterEntry.EntryValue = entryValueString;
                        savedFilterEntry.EntryTypeCode = typeCode;
                        savedFilter.Entries.Add(savedFilterEntry);
                    }

                    //If we have sorts, also add the sort expression to the saved filter
                    if (sortProjectSettings != null)
                    {
                        foreach (DictionaryEntry sortProjectEntry in sortProjectSettings)
                        {
                            //Get the entry key and values (convert to string and typecode)
                            string entryKey = (string)sortProjectEntry.Key;
                            object entryValue = sortProjectEntry.Value;
                            int typeCode;
                            string entryValueString = SerializeValue(entryValue, out typeCode);

                            //Populate the entry collection with a new filter entry
                            SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                            savedFilterEntry.EntryKey = entryKey;
                            savedFilterEntry.EntryValue = entryValueString;
                            savedFilterEntry.EntryTypeCode = typeCode;
                            savedFilter.Entries.Add(savedFilterEntry);
                        }
                    }

                    //Add the columns to the filter if required
                    //We add each column as a unique entry, and simply use a special format to distinguish from the
                    //fields and sorts (we use an underscore) and the following format:
                    //_FieldName = [Y|N]_Position_Width (width is omitted if not set)
                    if (includeColumns)
                    {
                        ArtifactManager artifactManager = new ArtifactManager();
                        List<ArtifactListFieldDisplay> columns = artifactManager.ArtifactField_RetrieveForLists(filterProjectSettings.ProjectId, filterProjectSettings.UserId, artifactType);
                        foreach (ArtifactListFieldDisplay column in columns)
                        {
                            //Get the entry key and values (convert to string and typecode)
                            string entryKey = "_" + column.Name;
                            string entryValue = "";
                            if (column.Width.HasValue)
                            {
                                entryValue = column.IsVisible.ToDatabaseSerialization() + "_" + column.ListPosition + "_" + column.Width.Value;
                            }
                            else
                            {
                                entryValue = column.IsVisible.ToDatabaseSerialization() + "_" + column.ListPosition;
                            }
                            int typeCode;
                            string entryValueString = SerializeValue(entryValue, out typeCode);

                            //Populate the entry collection with a new column entry
                            SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                            savedFilterEntry.EntryKey = entryKey;
                            savedFilterEntry.EntryValue = entryValueString;
                            savedFilterEntry.EntryTypeCode = typeCode;
                            savedFilter.Entries.Add(savedFilterEntry);
                        }
                    }

                    //Add the object
                    context.SavedFilters.AddObject(savedFilter);
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
        /// Updates the supplied project settings collection of filters
        /// </summary>
        /// <param name="existingSavedFilterId">The id of the filter we want to update</param>
        /// <param name="isShared">Is this a filter that should be shared with other project members</param>
        /// <param name="artifactType">The type of artifact the filter is for</param>
        /// <param name="filterProjectSettings">The collection of filters to be saved</param>
        /// <param name="includeColumns">Should we save the column selection with the filters and sorts</param>
        /// <param name="sortProjectSettings">The settings collection containing the sort expression</param>
        public void UpdateExisting(int existingSavedFilterId, DataModel.Artifact.ArtifactTypeEnum artifactType, ProjectSettingsCollection filterProjectSettings, ProjectSettingsCollection sortProjectSettings, bool isShared, bool includeColumns)
        {
            const string METHOD_NAME = "Save";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First restore the current filters and sorts
                filterProjectSettings.Restore();
                if (sortProjectSettings != null)
                {
                    sortProjectSettings.Restore();
                }

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First retrieve the saved filter record
                    var query = from f in context.SavedFilters.Include(f => f.Entries)
                                where f.SavedFilterId == existingSavedFilterId
                                select f;

                    //Check it exists
                    SavedFilter savedFilter = query.FirstOrDefault();
                    if (savedFilter == null)
                    {
                        throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.SavedFilterManager_UnableToRetrieveFilter, existingSavedFilterId));
                    }
                    savedFilter.StartTracking();
                    savedFilter.IsShared = isShared;
                    context.SaveChanges();

                    //Remove all of the old entries first
                    savedFilter.StartTracking();
                    List<SavedFilterEntry> entriesToDelete = savedFilter.Entries.ToList();
                    foreach (SavedFilterEntry entry in entriesToDelete)
                    {
                        context.SavedFilterEntries.DeleteObject(entry);
                    }
                    context.SaveChanges();

                    //Now iterate through the filter values and actually populate the new filter entries
                    savedFilter.StartTracking();
                    foreach (DictionaryEntry filterProjectEntry in filterProjectSettings)
                    {
                        //Get the entry key and values (convert to string and typecode)
                        string entryKey = (string)filterProjectEntry.Key;
                        object entryValue = filterProjectEntry.Value;
                        int typeCode;
                        string entryValueString = SerializeValue(entryValue, out typeCode);

                        //Populate the entry collection with a new filter entry
                        SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                        savedFilterEntry.EntryKey = entryKey;
                        savedFilterEntry.EntryValue = entryValueString;
                        savedFilterEntry.EntryTypeCode = typeCode;
                        savedFilter.Entries.Add(savedFilterEntry);
                    }

                    //If we have sorts, also add the sort expression to the saved filter
                    if (sortProjectSettings != null)
                    {
                        foreach (DictionaryEntry sortProjectEntry in sortProjectSettings)
                        {
                            //Get the entry key and values (convert to string and typecode)
                            string entryKey = (string)sortProjectEntry.Key;
                            object entryValue = sortProjectEntry.Value;
                            int typeCode;
                            string entryValueString = SerializeValue(entryValue, out typeCode);

                            //Populate the entry collection with a new filter entry
                            SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                            savedFilterEntry.EntryKey = entryKey;
                            savedFilterEntry.EntryValue = entryValueString;
                            savedFilterEntry.EntryTypeCode = typeCode;
                            savedFilter.Entries.Add(savedFilterEntry);
                        }
                    }

                    //Add the columns to the filter if required
                    //We add each column as a unique entry, and simply use a special format to distinguish from the
                    //fields and sorts (we use an underscore) and the following format:
                    //_FieldName = [Y|N]_Position_Width (width is omitted if not set)
                    if (includeColumns)
                    {
                        ArtifactManager artifactManager = new ArtifactManager();
                        List<ArtifactListFieldDisplay> columns = artifactManager.ArtifactField_RetrieveForLists(filterProjectSettings.ProjectId, filterProjectSettings.UserId, artifactType);
                        foreach (ArtifactListFieldDisplay column in columns)
                        {
                            //Get the entry key and values (convert to string and typecode)
                            string entryKey = "_" + column.Name;
                            string entryValue = "";
                            if (column.Width.HasValue)
                            {
                                entryValue = column.IsVisible.ToDatabaseSerialization() + "_" + column.ListPosition + "_" + column.Width.Value;
                            }
                            else
                            {
                                entryValue = column.IsVisible.ToDatabaseSerialization() + "_" + column.ListPosition;
                            }
                            int typeCode;
                            string entryValueString = SerializeValue(entryValue, out typeCode);

                            //Populate the entry collection with a new column entry
                            SavedFilterEntry savedFilterEntry = new SavedFilterEntry();
                            savedFilterEntry.EntryKey = entryKey;
                            savedFilterEntry.EntryValue = entryValueString;
                            savedFilterEntry.EntryTypeCode = typeCode;
                            savedFilter.Entries.Add(savedFilterEntry);
                        }
                    }

                    //Commit the changes
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
        /// Populates the provided project settings collection with the currently restored filter
        /// </summary>
        /// <param name="filterProjectSettings">The project settings collection to be populated by a list of individual filters</param>
        /// <param name="sortProjectSettings">The project settings collection to be populated by a sort expression (optional)</param>
        /// <param name="sortKey">The settings entry key used to hold the sort expression (optional)</param>
        public void Populate(ProjectSettingsCollection filterProjectSettings, ProjectSettingsCollection sortProjectSettings, string sortKey)
        {
            //Reset the collections
            filterProjectSettings.Clear();
            if (sortProjectSettings != null)
            {
                sortProjectSettings.Clear();
            }

            //The saved filter might also contain the list of visible/hidden columns, if so we need to store those separately
            Dictionary<string, string> columns = new Dictionary<string, string>();

            if (this.restoredSavedFilter != null)
            {
                //Iterate through the saved filter, populating the project settings collection
                foreach (SavedFilterEntry entry in this.restoredSavedFilter.Entries)
                {
                    string entryKey = entry.EntryKey;
                    string entryValue = entry.EntryValue;
                    int typeCode = entry.EntryTypeCode;

                    //Columns start with an underscore and use this format:
                    //_FieldName = [Y | N]_Position_Width(width is omitted if not set)
                    if (entryKey.StartsWith("_"))
                    {
                        //It's a column
                        columns.Add(entryKey, (string)DeSerializeValue(entryValue, typeCode));
                    }
                    else
                    {
                        //Need to handle the case of the sort expression separately
                        if (entryKey == sortKey)
                        {
                            if (sortProjectSettings != null)
                            {
                                sortProjectSettings.Add(entryKey, DeSerializeValue(entryValue, typeCode));
                            }
                        }
                        else
                        {
                            filterProjectSettings.Add(entryKey, DeSerializeValue(entryValue, typeCode));
                        }
                    }
                }
            }

            //Persist the changes
            filterProjectSettings.Save();
            if (sortProjectSettings != null)
            {
                sortProjectSettings.Save();
            }

            //Now handle the columns if specified
            if (columns.Count > 0)
            {
                int projectId = this.restoredSavedFilter.ProjectId;
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
                int userId = filterProjectSettings.UserId;
                Artifact.ArtifactTypeEnum artifactType = (Artifact.ArtifactTypeEnum)this.restoredSavedFilter.ArtifactTypeId;
                ArtifactManager artifactManager = new ArtifactManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(filterProjectSettings.ProjectId, filterProjectSettings.UserId, artifactType);
                foreach (KeyValuePair<string, string> column in columns)
                {
                    //Extract the values
                    string fieldName = column.Key.Substring(1); //Removes the leading underscore (_)
                    string[] components = column.Value.Split('_');
                    bool isVisible = components[0].FromDatabaseSerialization_Boolean().Value;
                    int position = components[1].FromDatabaseSerialization_Int32().Value;
                    int? width = null;
                    if (components.Length > 2)
                    {
                        width = components[2].FromDatabaseSerialization_Int32();
                    }

                    //Find the matching record
                    ArtifactListFieldDisplay artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
                    if (artifactField != null)
                    {
                        //See if we're dealing with a custom property or standard field
                        int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(artifactField.Name);
                        if (customPropertyNumber.HasValue)
                        {
                            //Check the position and visibility
                            if (position != artifactField.ListPosition || isVisible != artifactField.IsVisible)
                            {
                                customPropertyManager.CustomProperty_UpdateListPosition(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, position, isVisible);
                            }

                            //Check the width
                            if (width.HasValue && artifactField.Width.HasValue && width.Value != artifactField.Width.Value)
                            {
                                customPropertyManager.CustomProperty_UpdateColumnListWidth(projectId, projectTemplateId, userId, artifactType, customPropertyNumber.Value, width.Value);
                            }
                        }
                        else
                        {
                            //See if the visibility flag needs to change
                            if (isVisible != artifactField.IsVisible)
                            {
                                artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, artifactType, fieldName);
                            }

                            //Check the position
                            if (position != artifactField.ListPosition)
                            {
                                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, artifactType, fieldName, position);
                            }

                            //Check the width
                            if (width.HasValue && artifactField.Width.HasValue && width.Value != artifactField.Width.Value)
                            {
                                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, artifactType, fieldName, width.Value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a string representation into the native object
        /// </summary>
        /// <param name="entryValue">The string version of the data</param>
        /// <param name="typeCode">The type of the object we want</param>
        /// <returns>The data in its native form</returns>
        public object DeSerializeValue(string entryValue, int typeCode)
        {
            //First test the custom typecodes
            if (typeCode == (int)Common.Global.CustomTypeCodes.DateRange)
            {
                DateRange dateRange;
                DateRange.TryParse(entryValue, out dateRange);
                return dateRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.DecimalRange)
            {
                DecimalRange dateRange;
                DecimalRange.TryParse(entryValue, out dateRange);
                return dateRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.IntRange)
            {
                IntRange dateRange;
                IntRange.TryParse(entryValue, out dateRange);
                return dateRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.EffortRange)
            {
                EffortRange dateRange;
                EffortRange.TryParse(entryValue, out dateRange);
                return dateRange;
            }
            if (typeCode == (int)Common.Global.CustomTypeCodes.MultiValueFilter)
            {
                MultiValueFilter multiValueFilter;
                MultiValueFilter.TryParse(entryValue, out multiValueFilter);
                return multiValueFilter;
            }

            //Now the built-in typecodes
            if (typeCode == (int)TypeCode.Boolean)
            {
                return Boolean.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int32)
            {
                return Int32.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int16)
            {
                return Int16.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.Int64)
            {
                return Int64.Parse(entryValue);
            }
            else if (typeCode == (int)TypeCode.DateTime)
            {
                return DateTime.ParseExact(entryValue, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                //Keep as a string value
                return entryValue;
            }
        }

        /// <summary>
        /// Converts the native object into a string and associated type-code
        /// </summary>
        /// <param name="entryValue">The native object value</param>
        /// <param name="typeCode">The type code of the native object [out]</param>
        /// <returns>The string representation of the object</returns>
        protected string SerializeValue(object entryValue, out int typeCode)
        {
            //See if we have one of our custom types that we need to handle
            if (entryValue.GetType() == typeof(DateRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.DateRange;
                return ((DateRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(DecimalRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.DecimalRange;
                return ((DecimalRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(EffortRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.EffortRange;
                return ((EffortRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(IntRange))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.IntRange;
                return ((IntRange)entryValue).ToString();
            }
            if (entryValue.GetType() == typeof(MultiValueFilter))
            {
                typeCode = (int)Common.Global.CustomTypeCodes.MultiValueFilter;
                return ((MultiValueFilter)entryValue).ToString();
            }

            //Now handle the built-in typecodes

            //First we need to get the typecode of the object
            typeCode = (int)System.Type.GetTypeCode(entryValue.GetType());

            //Next we need to convert the value to string. Need to handle date-time differently
            if (typeCode == (int)TypeCode.DateTime)
            {
                return ((DateTime)entryValue).ToString("yyyyMMddTHHmmss");
            }
            else
            {
                return entryValue.ToString();
            }
        }

        /// <summary>
        /// Retrieves a particular saved filter by its id
        /// </summary>
        /// <param name="savedFilterId">The id of the saved filter</param>
        /// <returns>The entity of the filter and associated entries</returns>
        public SavedFilter RetrieveById(int savedFilterId)
        {
            const string METHOD_NAME = "RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                SavedFilter savedFilter;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from s in context.SavedFilters.Include("Entries")
                                where s.SavedFilterId == savedFilterId
                                select s;

                    savedFilter = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return savedFilter;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }
    }
}
