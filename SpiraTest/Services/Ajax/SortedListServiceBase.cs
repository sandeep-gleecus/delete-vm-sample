using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    public class SortedListServiceBase : ListServiceBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.SortedListServiceBase::";

        /// <summary>
        /// Allows sorted lists with folders to focus on a specific item and open its containing folder
        /// </summary>
        public virtual int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Allows sorted lists with folders to move an item between the folders
        /// </summary>
        public virtual void SortedList_Move(int projectId, List<string> items)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
        /// <param name="collectionName">The name of the filters collection</param>
        /// <returns>Validation message (or empty string if none)</returns>
        protected string UpdateFilters(int userId, int projectId, JsonDictionaryOfStrings filters, string collectionName, DataModel.Artifact.ArtifactTypeEnum artifactType)
        {
            //Delegate to the generic method, except that we don't need to track if this was a new filter or not
            bool newFilter = false;
            return base.UpdateFilters(userId, projectId, filters, collectionName, artifactType, out newFilter);
        }

        /// <summary>
        /// Saves the current filters with the specified name
        /// </summary>
        /// <param name="includeColumns">Should we include the column selection</param>
        /// <param name="existingSavedFilterId">Populated if we're updating an existing saved filter</param>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="name">The name of the filter</param>
        /// <param name="artifactType">The type of artifact the filter is for</param>
        /// <param name="filterCollection">The project settings collection used to store the filters</param>
        /// <param name="sortCollection">The project settings collection used to store the sorts</param>
        /// <param name="isShared">Should this filter be shared</param>
        /// <returns>Validation/error message (or empty string if none)</returns>
        protected string SaveFilter(int userId, int projectId, string name, DataModel.Artifact.ArtifactTypeEnum artifactType, string filterCollection, string sortCollection, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            const string METHOD_NAME = "SaveFilter";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Make sure that we have a valid name for the filter, or an id if we're updating an existing one
                Business.SavedFilterManager savedFilterManager = new Business.SavedFilterManager();
                if (existingSavedFilterId.HasValue)
                {
                    //Now we need to restore the filter project settings collection and save the values
                    ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, userId, filterCollection);
                    ProjectSettingsCollection sortProjectSettings = new ProjectSettingsCollection(projectId, userId, sortCollection);
                    DataModel.SavedFilter savedFilter = savedFilterManager.RetrieveById(existingSavedFilterId.Value);
                    if (savedFilter == null)
                    {
                        return Resources.Messages.SavedSearches_UnableToRetrieveSeach;
                    }
                    savedFilterManager.UpdateExisting(existingSavedFilterId.Value, artifactType, filterProjectSettings, sortProjectSettings, isShared, includeColumns);

                    return "";  //Success
                }
                else
                {
                    if (String.IsNullOrEmpty(name))
                    {
                        return "Please enter a valid name for the filter and try again";
                    }

                    //Now we need to restore the filter and sort project settings collections and save the values
                    ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, userId, filterCollection);
                    ProjectSettingsCollection sortProjectSettings = new ProjectSettingsCollection(projectId, userId, sortCollection);
                    savedFilterManager.Save(name, artifactType, filterProjectSettings, sortProjectSettings, isShared, includeColumns);

                    return "";  //Success
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return exception.Message;
            }
        }

        /// <summary>
        /// Updates the current sort stored in the system (property and direction)
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The artifact property we want to sort on</param>
        /// <param name="sortAscending">Are we sorting ascending or not</param>
        /// <param name="collectionName">The name of the sort collection</param>
        /// <returns>Any error messages</returns>
        protected string UpdateSort(int userId, int projectId, string sortProperty, bool sortAscending, string collectionName)
        {
            const string METHOD_NAME = "UpdateSort";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {

                //First we need to get appropriate settings collection
                if (projectId < 1)
                {
                    //User Settings
                    UserSettingsCollection sortSettings = GetUserSettings(userId, collectionName);
                    //Get the appropriate direction name ASC|DESC
                    string sortDirection = (sortAscending) ? "ASC" : "DESC";
                    sortSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION] = sortProperty + " " + sortDirection;
                    sortSettings.Save();
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return "";  //Success
                }
                else
                {
                    //Project Settings
                    ProjectSettingsCollection sortSettings = GetProjectSettings(userId, projectId, collectionName);
                    //Get the appropriate direction name ASC|DESC
                    string sortDirection = (sortAscending) ? "ASC" : "DESC";
                    sortSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION] = sortProperty + " " + sortDirection;
                    sortSettings.Save();
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return "";  //Success
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return "Error Updating Sort (" + exception.Message + ")";
            }
        }
    }
}
