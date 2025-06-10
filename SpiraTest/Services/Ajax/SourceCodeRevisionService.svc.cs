using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.PlugIns;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Linq;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side source code revision list AJAX components
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class SourceCodeRevisionService : SortedListServiceBase, ISourceCodeRevisionService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService::";

        #region IFormService methods

        /// <summary>Returns a single test set data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current test set</param>
        /// <returns>A test set data item</returns>
        public DataItem Form_Retrieve(int projectId, int? artifactId)
        {
            const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
                PopulateShape(sourceCodeManager, projectId, CurrentUserId.Value, dataItem, null, false);

                //Retrieve the specific source code revision
                SourceCodeCommit sourceCodeRevision = sourceCodeManager.RetrieveRevisionById(artifactId.Value);

                //Populate the row data item
                PopulateRow(projectId, dataItem, sourceCodeRevision);

                //Provide the revision key as a field
                DataItemField revisionKeyField = new DataItemField();
                revisionKeyField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                revisionKeyField.FieldName = "RevisionKey";
                revisionKeyField.TextValue = sourceCodeRevision.Revisionkey;
                dataItem.Fields.Add(revisionKeyField.FieldName, revisionKeyField);

                //See if we have a build associated with the revision, get the first one in the list
                BuildManager buildManager = new BuildManager();
                Build build = buildManager.RetrieveForRevision(projectId, sourceCodeRevision.Revisionkey).FirstOrDefault();
                if (build != null)
                {
                    DataItemField buildField = new DataItemField();
                    buildField.FieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
                    buildField.FieldName = "BuildId";
                    buildField.TextValue = build.Name;
                    buildField.IntValue = build.BuildId;
                    dataItem.Fields.Add(buildField.FieldName, buildField);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return dataItem;
            }
            catch (SourceCodeProviderArtifactPermissionDeniedException)
            {
                //Just return no data back
                return null;
            }
            catch (ArtifactNotExistsException)
            {
                //Just return no data back
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
        {
            throw new NotImplementedException();
        }

        public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region SourceCodeRevision methods

        /// <summary>
        /// Retrieves the list of branches that a revision belongs to
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="revisionKey">The key of the revision</param>
        /// <returns>The list of branch names</returns>
        public List<string> SourceCodeRevision_RetrieveBranches(int projectId, string revisionKey)
        {
            const string METHOD_NAME = "SourceCodeRevision_RetrieveBranches";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<string> branchNames = new List<string>();

                //Get the list of branches that the current revision belongs to
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                List<SourceCodeBranch> branches = sourceCodeManager.RetrieveBranchesByRevision(revisionKey);
                foreach (SourceCodeBranch branch in branches)
                {
                    string branchName = branch.BranchKey;
                    branchNames.Add(branchName);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return branchNames;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the X most recent revisions for display on the source code file list page
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name/key of the branch</param>
        /// <param name="numberRows">The number of rows</param>
        /// <returns></returns>
        public List<DataItem> SourceCodeRevision_RetrieveRecent(int projectId, string branchKey, int numberRows)
        {
            const string METHOD_NAME = "SourceCodeRevision_RetrieveRecent";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<DataItem> recentRevisions = new List<DataItem>();

                //Get the list of recent revisions
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                int revisionCount;
                List<SourceCodeCommit> commits = sourceCodeManager.RetrieveRevisions(branchKey, SourceCodeManager.FIELD_UPDATE_DATE, false, 1, numberRows, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out revisionCount);
                foreach (SourceCodeCommit commit in commits)
                {
                    DataItem revision = new DataItem();
                    revision.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + commit.Revisionkey);

                    //Name
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = SourceCodeManager.FIELD_NAME;
                    dataItemField.TextValue = commit.Name;
                    revision.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Message
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = SourceCodeManager.FIELD_MESSAGE;
                    dataItemField.TextValue = commit.Message;
                    revision.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Author
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = SourceCodeManager.FIELD_AUTHOR;
                    dataItemField.TextValue = commit.AuthorName;
                    revision.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Update Date
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = SourceCodeManager.FIELD_UPDATE_DATE;
                    dataItemField.DateValue = commit.UpdateDate;
                    dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(commit.UpdateDate));
                    dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(commit.UpdateDate));
                    revision.Fields.Add(dataItemField.FieldName, dataItemField);

                    recentRevisions.Add(revision);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return recentRevisions;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the count of commits/revisions for a specific file
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fileKey">The key of the file</param>
        /// <returns>The count</returns>
        public int SourceCodeRevision_CountForFile(int projectId, string fileKey)
        {
            const string METHOD_NAME = "SourceCodeRevision_CountForFile";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the current branch
                string branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                //Get the count of revisions for this file
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                int count = sourceCodeManager.CountRevisionsForFile(fileKey, branchKey);

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return count;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the count of commits/revisions in a pull request
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fileKey">The key of the file</param>
        /// <returns>The count</returns>
        public int SourceCodeRevision_CountForPullRequest(int projectId, int pullRequestId)
        {
            const string METHOD_NAME = "SourceCodeRevision_CountForPullRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the count of revisions for this pull request
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                int count = sourceCodeManager.CountRevisionsForPullRequest(pullRequestId);

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return count;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the count of commits/revisions for a specific artifact (e.g. build)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifact">The id of the artifact</param>
        /// <returns>The count</returns>
        public int SourceCodeRevision_Count(int projectId, ArtifactReference artifact)
        {
            const string METHOD_NAME = "SourceCodeRevision_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            //Limited OK because we need to display the 'has data' in tabs
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                int count = 0;

                //See what artifact we have
                if (artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Build)
                {
                    //Get the count of revisions for this build
                    count = new BuildManager().CountRevisions(projectId, artifact.ArtifactId);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return count;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of the number of commits for the specified date range
        /// </summary>
        /// <param name="dateRange">The date-range we want</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The graph data</returns>
        public List<DateTimeFrequencyEntry> SourceCodeRevision_RetrieveCommitCounts(int projectId, string dateRange)
        {
            const string METHOD_NAME = "SourceCodeRevision_RetrieveCommitCounts";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<DateTimeFrequencyEntry> graphData = new List<DateTimeFrequencyEntry>();

                //Get the list of commits for the past year by week
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                //We use the current branch commits since the graph widget displays the branch name
                string branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                //Parse the date range (leave as empty if fails and retrieve function will just use the last 90 days)
                Common.DateRange parsedDateRange;
                if (String.IsNullOrEmpty(dateRange) || !Common.DateRange.TryParse(dateRange, out parsedDateRange))
                {
                    parsedDateRange = new Common.DateRange();
                    parsedDateRange.StartDate = DateTime.UtcNow.AddDays(-90);
                    parsedDateRange.EndDate = DateTime.UtcNow;
                }
                parsedDateRange.ConsiderTimes = false;
                IOrderedEnumerable<IGrouping<DateTime, SourceCodeCommit>> commitCountGroups = sourceCodeManager.RetrieveRevisionCountForDateRange(branchKey, parsedDateRange);

                //Now we need to convert into the API object and fill in any missing dates
                DateTime lastDate = DateTime.MinValue;
                if (commitCountGroups != null)
                {
                    int count = 0;  //We only want the first 30 results
                    DateTimeFrequencyEntry dateTimeFrequencyEntry;
                    foreach (IGrouping<DateTime, SourceCodeCommit> commitCountGroup in commitCountGroups)
                    {
                        //Add any missing dates that have no commits
                        int numberMissingWeeks = (int)((commitCountGroup.Key.AddDays(-7) - lastDate).TotalDays / 7);
                        if (lastDate > DateTime.MinValue && numberMissingWeeks > 0)
                        {
                            for (int i = 0; i < numberMissingWeeks; i++)
                            {
                                dateTimeFrequencyEntry = new DateTimeFrequencyEntry();
                                dateTimeFrequencyEntry.Interval = lastDate.AddDays(i * 7);
                                dateTimeFrequencyEntry.Caption = dateTimeFrequencyEntry.Interval.ToString("m");
                                dateTimeFrequencyEntry.Count = 0;
                                graphData.Add(dateTimeFrequencyEntry);
                                count++;
                            }
                        }

                        dateTimeFrequencyEntry = new DateTimeFrequencyEntry();
                        dateTimeFrequencyEntry.Interval = commitCountGroup.Key;
                        dateTimeFrequencyEntry.Caption = commitCountGroup.Key.ToString("m");
                        dateTimeFrequencyEntry.Count = commitCountGroup.Count();
                        graphData.Add(dateTimeFrequencyEntry);

                        lastDate = commitCountGroup.Key;
                        count++;
                        if (count > 30)
                        {
                            break;
                        }
                    }
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region IListService methods

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
        {
            const string METHOD_NAME = "RetrievePaginationOptions()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

            JsonDictionaryOfStrings paginationDictionary = new JsonDictionaryOfStrings();
            try
            {
                //Get the list of options
                SortedList<int, int> paginationOptions = new ManagerBase().GetPaginationOptions();

                //Get the current pagination setting for the project/user
                SourceCodeManager sourceCode = new SourceCodeManager(projectId);
                int paginationSize = 0;
                int pageNum = 0;
                SourceCodeManager.Get_UserPagnationRevisions(userId, projectId, out paginationSize, out pageNum);

                //Reformulate into a dictionary where the value indicates if it's the selected value or not (true/false)
                foreach (KeyValuePair<int, int> kvp in paginationOptions)
                {
                    //See if this is the selected value or not
                    paginationDictionary.Add(kvp.Key.ToString(), (kvp.Key == paginationSize) ? "true" : "false");
                }
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return paginationDictionary;
        }

        /// <summary>Updates the filters stored in the system</summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
        /// <returns>Any error messages</returns>
        public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
        {
            const string METHOD_NAME = CLASS_NAME + "UpdateFilters()";
            Logger.LogEnteringEvent(METHOD_NAME);

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
                //Get the current filters from the source code cached settings
                Hashtable savedFilters = new Hashtable();

                //Iterate through the filters, updating the project collection
                foreach (KeyValuePair<string, string> filter in filters)
                {
                    string filterName = filter.Key;
                    //Now get the type of field that we have. Since source code files are not a true artifact,
                    //these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
                    DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                    switch (filterName)
                    {
                        case SourceCodeManager.FIELD_NAME:
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
                            break;
                        case SourceCodeManager.FIELD_UPDATE_DATE:
                        case SourceCodeManager.FIELD_LASTUPDATED:
                            artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
                            break;
                    }

                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.DateTime)
                    {
                        //If we have date values, need to make sure that they are indeed dates
                        //Otherwise we need to throw back a friendly error message
                        Common.DateRange dateRange;
                        if (!Common.DateRange.TryParse(filter.Value, out dateRange))
                        {
                            return "You need to enter a valid date-range value for '" + filterName + "'";
                        }
                        savedFilters.Add(filterName, dateRange);
                    }
                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Integer)
                    {
                        //If we have integer values, need to make sure that they are indeed integral
                        Int32 intValue;
                        if (!Int32.TryParse(filter.Value, out intValue))
                        {
                            return "You need to enter a valid integer value for '" + filterName + "'";
                        }
                        savedFilters.Add(filterName, intValue);
                    }
                    if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Text || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription)
                    {
                        //For text, just save the value
                        savedFilters.Add(filterName, filter.Value);
                    }
                }

                //Save the filters..
                SourceCodeManager.Set_UserFilterRevisions(userId, projectId, savedFilters, displayTypeId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return "";  //Success
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
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

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Set the pagination settings on the source code object
                SourceCodeManager.Set_UserPagnationRevisions(userId, projectId, pageSize, currentPage);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// We return back the source code name, date, author and revision message as a tooltip
        /// </summary>
        /// <param name="artifactId"></param>
        /// <returns></returns>
        public string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId)
        {
            const string METHOD_NAME = CLASS_NAME + "RetrieveNameDesc()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            if (!projectId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId.Value);
            if (authorizationState == Project.AuthorizationState.Prohibited)
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

            try
            {
                string tooltip = "";
                //Get the revision by its ID
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId.Value);
                SourceCodeCommit commit = sourceCodeManager.RetrieveRevisionById(artifactId);
                tooltip = "<u>" + commit.Name + " - " + String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(commit.UpdateDate)) + "</u>";
                if (!String.IsNullOrEmpty(commit.Message))
                {
                    tooltip += "<br />" + commit.Message;
                }
                tooltip += "<br /><i>- " + commit.AuthorName + "</i>";

                Logger.LogExitingEvent(METHOD_NAME);
                return tooltip;
            }
            catch (ArtifactNotExistsException)
            {
                //Just return empty string
                return "";
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        #endregion

        #region ISortedListService methods

        /// <summary>
        /// Returns a list of source code files in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the documents as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the source code business object and get the list of filters
                Hashtable filterList = SourceCodeManager.Get_UserFilterRevisions(userId, projectId, displayTypeId);
                string sortKey;
                bool sortAsc;
                SourceCodeManager.Get_UserSortRevisions(userId, projectId, displayTypeId, out sortAsc, out sortKey);
                int pageCurrent, numPerPage;
                SourceCodeManager.Get_UserPagnationRevisions(userId, projectId, out numPerPage, out pageCurrent);

                //See if we have a branch specified
                string branchKey = null;
                if (standardFilters != null && standardFilters.ContainsKey("BranchKey"))
                {
                    branchKey = (string)GlobalFunctions.DeSerializeValue(standardFilters["BranchKey"]);
                }
                else
                {
                    //Otherwise get from settings
                    branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);
                }

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;
                sortedData.FilterNames = GetFilterNames(filterList);

                //See if this is for a build or not
                int? buildId = null;
                if (standardFilters != null && standardFilters.ContainsKey("BuildId"))
                {
                    buildId = (int)GlobalFunctions.DeSerializeValue(standardFilters["BuildId"]);
                }

                //See if this is for a file or not
                bool displayAction = false;
                if (standardFilters != null && standardFilters.ContainsKey("FileKey"))
                {
                    //Include the column that contains the specific file action
                    displayAction = true;
                }

                //See if this is for a pull request or not
                int? pullRequestId = null;
				string sourceBranch = "";
				if (standardFilters != null && standardFilters.ContainsKey("PullRequestId"))
                {
                    pullRequestId = (int)GlobalFunctions.DeSerializeValue(standardFilters["PullRequestId"]);
                }

                //Create the filter item first - we can clone it later
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                SortedDataItem filterItem = new SortedDataItem();
                PopulateShape(sourceCodeManager, projectId, userId, filterItem, filterList, displayAction);
                dataItems.Add(filterItem);

                //Now get the pagination information
                int paginationSize, currentPage;
                SourceCodeManager.Get_UserPagnationRevisions(userId, projectId, out paginationSize, out currentPage);

                //Get the source code revisions collection
                //If we have a filekey filter passed in as a standard filter, then we need to get the
                //revisions for a specific file, otherwise just get the revisions for the whole repository
                //If we have a build passed in as a standard filter, then we need to get the list of revisions
                //associated with the specific build
                int artifactCount;
                List<SourceCodeCommit> sourceCodeRevisions = null;
                string fileKey = null;
                int fileId = 0;
                if (standardFilters != null && standardFilters.ContainsKey("FileKey"))
                {
                    fileKey = (string)GlobalFunctions.DeSerializeValue(standardFilters["FileKey"]);
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForFile(
                        fileKey,
                        branchKey,
                        sortKey,
                        sortAsc,
                        (paginationSize * (currentPage - 1) + 1),
                        paginationSize,
                        filterList,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out fileId,
                        out artifactCount); //This is ready to hook up to filters when needed.
                }
                else if (buildId.HasValue)
                {
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForBuild(
                        buildId.Value,
                        sortKey,
                        sortAsc,
                        (paginationSize * (currentPage - 1) + 1),
                        paginationSize,
                        filterList,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out artifactCount); //This is ready to hook up to filters when needed.
                }
                else if (pullRequestId.HasValue)
                {
					//Get the pull request since we need to know the source branch (gets added to the URL)
					PullRequest pullRequest = new PullRequestManager().PullRequest_RetrieveById(pullRequestId.Value);
					if (pullRequest != null)
					{
						sourceBranch = pullRequest.SourceBranchName;
					}

					//Get the revisions
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForPullRequest(
                        pullRequestId.Value,
                        sortKey,
                        sortAsc,
                        (paginationSize * (currentPage - 1) + 1),
                        paginationSize,
                        filterList,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out artifactCount); //This is ready to hook up to filters when needed.
                }
                else
                {
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisions(
                        branchKey,
                        sortKey,
                        sortAsc,
                        (paginationSize * (currentPage - 1) + 1),
                        paginationSize,
                        filterList,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out artifactCount); //This is ready to hook up to filters when needed.
                }

                int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
                //Make sure that the current page is not larger than the number of pages or less than 1
                if (currentPage > pageCount)
                {
                    currentPage = pageCount;
                    SourceCodeManager.Set_UserPagnationRevisions(userId, projectId, paginationSize, currentPage);
                }
                if (currentPage < 1)
                {
                    currentPage = 1;
                    SourceCodeManager.Set_UserPagnationRevisions(userId, projectId, paginationSize, currentPage);
                }

                //**** Now we need to actually populate the rows of data to be returned ****
                int startRow = ((currentPage - 1) * paginationSize) + 1;

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;
                sortedData.VisibleCount = sourceCodeRevisions.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortKey;
                sortedData.SortAscending = sortAsc;

                //Iterate through all the revisions and populate the dataitem
                foreach (SourceCodeCommit sourceCodeRevision in sourceCodeRevisions)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(projectId, dataItem, sourceCodeRevision, fileKey, fileId, sourceBranch);
                    dataItems.Add(dataItem);
                }

                //Also include the pagination info
                sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return sortedData;
            }
            catch (SourceCodeProviderGeneralException)
            {
                //This will be handled by the SpiraErrorHandler WCF behavior
                throw;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Internal Functions

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="sourceCode">Handle to the source code business object</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the files as</param>
        /// <param name="displayAction">Should we display the action column</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        protected void PopulateShape(SourceCodeManager sourceCode, int projectId, int userId, SortedDataItem dataItem, Hashtable filterList, bool displayAction)
        {
            //We need to add the various source code revision fields to be displayed
            //Revision Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_NAME;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.Revision;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Update Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_UPDATE_DATE;
            dataItemField.Caption = Resources.Fields.CommitDate;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                //Need to convert into the displayable date form
                Common.DateRange dateRange = (Common.DateRange)filterList[dataItemField.FieldName];
                string textValue = "";
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

            //Summary of revision message
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_MESSAGE;
            dataItemField.Caption = Resources.Fields.Message;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //If we're retrieving for a file, display the action
            if (displayAction)
            {
                dataItemField = new DataItemField();
                dataItemField.FieldName = SourceCodeManager.FIELD_ACTION;
                dataItemField.Caption = Resources.Fields.Action;
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                dataItemField.AllowDragAndDrop = true;
                dataItemField.NotSortable = true;   //Since a navigation property
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                //Set the filter value (if one is set)
                if (filterList != null && filterList.Contains(dataItemField.FieldName))
                {
                    dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
                }
            }

            /*
            //Content changed flag
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_CONTENT_CHANGED;
            dataItemField.Caption = Resources.Fields.ContentChanged;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Flag;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the list of possible flag values
            dataItemField.Lookups = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Properties changed flag
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_PROP_CHANGED;
            dataItemField.Caption = Resources.Fields.PropertiesChanged;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Flag;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the list of possible flag values
            dataItemField.Lookups = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }*/

            //Author
            dataItemField = new DataItemField();
            dataItemField.FieldName = SourceCodeManager.FIELD_AUTHOR;
            dataItemField.Caption = Resources.Fields.Author;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="fileId">The file ID if these are revisions for a file</param>
        /// <param name="fileKey">The file key if these are revisions for a file</param>
        /// <param name="sourceCodeRevision">The source code revision object</param>
        protected void PopulateRow(int projectId, SortedDataItem dataItem, SourceCodeCommit sourceCodeRevision, string fileKey = null, int fileId = 0, string sourceBranch = null)
        {
            //Set the primary key and URL, depending on whether being used for a file or not, the URL will be different
			//Also see if we need to pass through the branch name
            dataItem.PrimaryKey = sourceCodeRevision.RevisionId;
            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_ACTION) && !String.IsNullOrEmpty(fileKey) && fileId > 0)
            {
                dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeRevision.Revisionkey + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + fileKey);
            }
			else if (!String.IsNullOrEmpty(sourceBranch))
			{
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeRevision.Revisionkey + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY + "=" + HttpUtility.UrlEncode(sourceBranch));
			}
			else
            {
                dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeRevision.Revisionkey);
            }

            //Source Code Revisions don't have an attachment flag
            dataItem.Attachment = false;

            //We need to add the various source code revision fields to be displayed
            //File Name
            DataItemField dataItemField = dataItem.Fields[SourceCodeManager.FIELD_NAME];
            dataItemField.Editable = false;
            dataItemField.Required = false;
            dataItemField.TextValue = sourceCodeRevision.Name;

            //Author / Uploaded By
            dataItemField = dataItem.Fields[SourceCodeManager.FIELD_AUTHOR];
            dataItemField.Editable = false;
            dataItemField.Required = false;
            dataItemField.TextValue = sourceCodeRevision.AuthorName;

            //Summary
            dataItemField = dataItem.Fields[SourceCodeManager.FIELD_MESSAGE];
            dataItemField.Editable = false;
            dataItemField.Required = false;
            dataItemField.TextValue = sourceCodeRevision.Message;

            //Action if the revisions for a file
            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_ACTION) && !String.IsNullOrEmpty(fileKey) && fileId > 0)
            {
                //We display custom icons for this version of the service
                dataItem.Fields[SourceCodeManager.FIELD_NAME].Tooltip = "artifact-Revision.svg";

                SourceCodeFileEntry scfe = sourceCodeRevision.Files.FirstOrDefault(f => f.FileKey == fileKey);
                if (scfe != null)
                {
                    string action = sourceCodeRevision.Files[0].Action;
                    dataItemField = dataItem.Fields[SourceCodeManager.FIELD_ACTION];
                    dataItemField.Editable = false;
                    dataItemField.Required = false;
                    dataItemField.TextValue = action;

                    //For certain actions, change the icon
                    if (action == "Added")
                    {
                        dataItem.Fields[SourceCodeManager.FIELD_NAME].Tooltip = "artifact-Revision-Add.svg";
                    }
                    if (action == "Deleted")
                    {
                        dataItem.Fields[SourceCodeManager.FIELD_NAME].Tooltip = "artifact-Revision-Delete.svg";
                    }
                }
            }

            //Last Edited Date
            dataItemField = dataItem.Fields[SourceCodeManager.FIELD_UPDATE_DATE];
            dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(sourceCodeRevision.UpdateDate));
            dataItemField.Editable = false;
            dataItemField.Required = false;
            dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(sourceCodeRevision.UpdateDate));

            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_CONTENT_CHANGED))
            {
                //Content Changed
                dataItemField = dataItem.Fields[SourceCodeManager.FIELD_CONTENT_CHANGED];
                dataItemField.Editable = false;
                dataItemField.Required = false;
                dataItemField.TextValue = (sourceCodeRevision.ContentChanged) ? "Yes" : "No";
            }

            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_PROP_CHANGED))
            {
                //Properties Changed
                dataItemField = dataItem.Fields[SourceCodeManager.FIELD_PROP_CHANGED];
                dataItemField.Editable = false;
                dataItemField.Required = false;
                dataItemField.TextValue = (sourceCodeRevision.PropertiesChanged) ? "Yes" : "No";
            }
        }

        /// <summary>
        /// Updates the current sort stored in the system (property and direction)
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The artifact property we want to sort on</param>
        /// <param name="sortAscending">Are we sorting ascending or not</param>
        /// <returns>Any error messages</returns>
        public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
        {
            const string METHOD_NAME = "UpdateSort";

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
                //Save the setting..
                SourceCodeManager.Set_UserSortRevisions(userId, projectId, sortAscending, sortProperty, displayTypeId);

                return "";  //Success
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return "Error Updating Sort (" + exception.Message + ")";
            }
        }

        #endregion

        #region INavigationService Methods

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
        {
            //Same implementation as the list service
            return RetrievePaginationOptions(projectId);
        }

        /// <summary>
        /// Updates the size of pages returned and the currently selected page
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
        /// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            //Same implementation as the list service
            this.UpdatePagination(projectId, pageSize, currentPage);
        }

        /// <summary>
        /// Returns a list of revisions for display in the navigation bar
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">Not used since not hierarchical</param>
        /// <returns>List of incidents</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
        /// </param>
        /// <param name="selectedItemId">The id of the currently selected item</param>
        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            const string METHOD_NAME = "NavigationBar_RetrieveList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the source code business object and get the list of filters
                Hashtable filterList = SourceCodeManager.Get_UserFilterRevisions(userId, projectId, null);
                string sortKey;
                bool sortAsc;
                SourceCodeManager.Get_UserSortRevisions(userId, projectId, null, out sortAsc, out sortKey);
                int pageCurrent, numPerPage;
                SourceCodeManager.Get_UserPagnationRevisions(userId, projectId, out numPerPage, out pageCurrent);

                //Get the branch name
                string branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Get the list of revisions
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                int artifactCount;
                List<SourceCodeCommit> sourceCodeRevisions = new List<SourceCodeCommit>();

                //See which display mode we're using
                if (displayMode == 1)
                {
                    //Filtered list
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisions(
                        branchKey,
                        sortKey,
                        sortAsc,
                        (numPerPage * (pageCurrent - 1) + 1),
                        numPerPage,
                        filterList,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out artifactCount); //This is ready to hook up to filters when needed.
                }
                if (displayMode == 2)
                {
                    //All items
                    sourceCodeRevisions = sourceCodeManager.RetrieveRevisions(
                        branchKey,
                        sortKey,
                        sortAsc,
                        (numPerPage * (pageCurrent - 1) + 1),
                        numPerPage,
                        null,
                        GlobalFunctions.GetCurrentTimezoneUtcOffset(),
                        out artifactCount); //This is ready to hook up to filters when needed.
                }

                //Iterate through all the revisions and populate the dataitem (only some columns are needed)
                foreach (SourceCodeCommit sourceCodeRevision in sourceCodeRevisions)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeRevision.Revisionkey);

                    //Populate the necessary fields
                    dataItem.PrimaryKey = sourceCodeRevision.RevisionId;
                    dataItem.Indent = "";
                    dataItem.Expanded = false;

                    //Name/Desc (include the IN #)
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = sourceCodeRevision.Name;
                    dataItem.Summary = false;
                    dataItem.Alternate = false;
                    dataItem.Fields.Add("Name", dataItemField);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return dataItems;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">Not used for this service</param>
        /// <param name="displayWidth">The display width</param>
        /// <param name="minimized">Is the navigation bar minimized or visible</param>
        public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
        {
            const string METHOD_NAME = "NavigationBar_UpdateSettings";

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
                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS);
                if (minimized.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED] = minimized.Value;
                    changed = true;
                }
                if (displayWidth.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH] = displayWidth.Value;
                    changed = true;
                }
                if (changed)
                {
                    settings.Save();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Not Implemented Methods

        public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public void SortedList_Copy(int projectId, List<string> items)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException("Not Implemented for Source Code Revisions");
        }

        #endregion
    }
}
