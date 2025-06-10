using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;


namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    public class HierarchicalListServiceBase : ListServiceBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.HierarchicalListServiceBase::";

        protected const int UPDATE_TIME_BUFFER_MINUTES = -5;

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
        /// <param name="isShared">Should this filter be shared</param>
        /// <returns>Validation/error message (or empty string if none)</returns>
        protected string SaveFilter(int userId, int projectId, string name, DataModel.Artifact.ArtifactTypeEnum artifactType, string filterCollection, bool isShared, int? existingSavedFilterId, bool includeColumns)
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
                    DataModel.SavedFilter savedFilter = savedFilterManager.RetrieveById(existingSavedFilterId.Value);
                    if (savedFilter == null)
                    {
                        return Resources.Messages.SavedSearches_UnableToRetrieveSeach;
                    }
                    savedFilterManager.UpdateExisting(existingSavedFilterId.Value, artifactType, filterProjectSettings, null, isShared, includeColumns);

                    return "";  //Success
                }
                else
                {
                    if (String.IsNullOrEmpty(name))
                    {
                        return "Please enter a valid name for the filter and try again";
                    }

                    //Now we need to restore the filter project settings collection and save the values
                    ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, userId, filterCollection);
                    savedFilterManager.Save(name, artifactType, filterProjectSettings, isShared, includeColumns);

                    return "";  //Success
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return exception.Message;
            }
        }
    }
}
