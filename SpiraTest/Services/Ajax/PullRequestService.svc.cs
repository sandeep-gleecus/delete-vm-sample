using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the SortableGrid AJAX component for displaying/updating pull requests
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class PullRequestService : SortedListServiceBase, IPullRequestService
    {
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.PullRequestService::";

        #region IPullRequestService Native Methods

        /// <summary>
        /// Creates a new pull request task in the project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="name">The name of the task</param>
        /// <param name="sourceBranch">The branch we want to merge FROM</param>
        /// <param name="destBranch">The branch we want to merge TO</param>
        /// <param name="ownerId">The owner of the task that will be created [optional]</param>
        /// <param name="releaseId">The id of the release/sprint [optional]</param>
        /// <returns></returns>
        public int PullRequest_Create(int projectId, string name, string sourceBranch, string destBranch, int? ownerId, int? releaseId)
        {
            const string METHOD_NAME = "PullRequest_Create";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business object(s)
                PullRequestManager pullRequestManager = new PullRequestManager();

                //Simply insert the new item
                int pullRequestId = pullRequestManager.PullRequest_Create(projectId, name, userId, sourceBranch, destBranch, releaseId, ownerId);
                return pullRequestId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region ISortedList methods

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
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

        /// <summary>
        /// Deletes a set of pull request tasks
        /// </summary>
        /// <param name="items">The items to delete</param>
        /// <param name="projectId">The id of the project (not used)</param>
        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                TaskManager taskManager = new TaskManager();
                foreach (string itemValue in items)
                {
                    //Get the task ID
                    int taskId = Int32.Parse(itemValue);
                    taskManager.MarkAsDeleted(projectId, taskId, userId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates records of data in the system
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="dataItems">The updated data records</param>
        /// <returns>The list of any validation messages</returns>
        public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Iterate through each data item and make the updates
                PullRequestManager pullRequestManager = new PullRequestManager();

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the task id
					int taskId = dataItem.PrimaryKey;

					//Make sure the source and destination branches are not the same
					if (dataItem.Fields.ContainsKey("SourceBranchId") && dataItem.Fields.ContainsKey("DestBranchId") && dataItem.Fields["SourceBranchId"].IntValue == dataItem.Fields["DestBranchId"].IntValue)
					{
						ValidationMessage newMsg = new ValidationMessage();
						newMsg.FieldName = "SourceBranchId";
						newMsg.Message = Resources.Messages.PullRequests_DifferentBranchesNeeded;
						AddUniqueMessage(validationMessages, newMsg);
					}

					//Retrieve the existing pull request task record - and make sure it still exists
					Task task = pullRequestManager.Task_RetrieveById(taskId);

					if (task != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								task.ConcurrencyDate = concurrencyDateTimeValue;
								task.AcceptChanges();
							}
						}

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("CreationDate");
						UpdateFields(validationMessages, dataItem, task, null, null, projectId, taskId, Artifact.ArtifactTypeEnum.Task, fieldsToIgnore);

                        //Add/Update the source/destination branches that are part of the pull request navigation property
                        if (task.PullRequests.Count > 0)
                        {
                            //We already have an associated branch
                            VersionControlPullRequest vcpr = task.PullRequests.FirstOrDefault();
                            if (dataItem.Fields.ContainsKey("SourceBranchId") && dataItem.Fields["SourceBranchId"].IntValue.HasValue)
                            {
                                vcpr.SourceBranchId = dataItem.Fields["SourceBranchId"].IntValue.Value;
                            }
                            if (dataItem.Fields.ContainsKey("DestBranchId") && dataItem.Fields["DestBranchId"].IntValue.HasValue)
                            {
                                vcpr.DestBranchId = dataItem.Fields["DestBranchId"].IntValue.Value;
                            }
                        }
                        else
                        {
                            if (dataItem.Fields.ContainsKey("SourceBranchId") && dataItem.Fields["SourceBranchId"].IntValue.HasValue && dataItem.Fields.ContainsKey("DestBranchId") && dataItem.Fields["DestBranchId"].IntValue.HasValue)
                            {
                                VersionControlPullRequest vcpr = new VersionControlPullRequest();
                                task.PullRequests.Add(vcpr);
                                vcpr.SourceBranchId = dataItem.Fields["SourceBranchId"].IntValue.Value;
                                vcpr.DestBranchId = dataItem.Fields["DestBranchId"].IntValue.Value;
                            }
                        }

						//Make sure we have no validation messages before updating
						if (validationMessages.Count == 0)
						{
							//Get copies of everything..
							Artifact notificationArt = task.Clone();

							//Persist to database, catching any business exceptions and displaying them
							try
							{
								pullRequestManager.Task_Update(task, userId);
							}
							catch (DataValidationException exception)
							{
								return CreateSimpleValidationMessage(exception.Message);
							}
							catch (OptimisticConcurrencyException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
							}
							catch (TaskDateOutOfBoundsException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.TasksService_DatesOutsideBounds);
							}

							//Call notifications..
							try
							{
								new NotificationManager().SendNotificationForArtifact(notificationArt, null, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Task #" + task.TaskId + ".");
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of pull request tasks in the system for the specific user/project
		/// </summary>
		/// <param name="userId">The user we're viewing the tasks as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <param name="displayTypeId">The location of the list we are on - to distinguish them from one another for display/filtering purposes</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business objects
                PullRequestManager pullRequestManager = new PullRequestManager();

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_FILTERS_LIST;
				string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_GENERAL;

				Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
				string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);
				sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_GENERAL);
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
				//Get the number of pull requests in the project
				int artifactCount = pullRequestManager.PullRequest_Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
				List<PullRequest> pullRequests = pullRequestManager.PullRequest_Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the visible and total count of artifacts
				sortedData.VisibleCount = pullRequests.Count;
				sortedData.TotalCount = artifactCount;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Iterate through all the pull request tasks and populate the dataitem
				foreach (PullRequest pullRequest in pullRequests)
				{
					//We clone the template item as the basis of all the new items
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(dataItem, pullRequest, false);
					dataItems.Add(dataItem);
				}

				//Also include the pagination info
				sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Handles custom list operations used by the task list screen, specifically removing tasks from releases/requirements
        /// </summary>
        /// <param name="operation">
        /// The operation being executed:
        ///     RemoveFromRelease - removes the task from the release specified
        ///     RemoveFromRequirement - removes the task from the requirement specified
        /// </param>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="destId">The destination item id</param>
        /// <param name="items">The list of source items</param>
        public override string CustomListOperation(string operation, int projectId, int destId, List<string> items)
		{
			const string METHOD_NAME = "CustomListOperation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			try
			{
				//See which operation we have
				if (operation == "RemoveFromRelease")
				{
					//Remove their release association
					TaskManager taskManager = new TaskManager();
					List<int> taskIds = new List<int>();
					foreach (string item in items)
					{
						int taskId;
						if (Int32.TryParse(item, out taskId))
						{
							taskIds.Add(taskId);
						}
					}
					taskManager.RemoveReleaseAssociation(taskIds, userId);
				}
				else if (operation == "RemoveFromRequirement")
				{
					//Iterate through all the passed in tasks and remove their requirement association
					TaskManager taskManager = new TaskManager();
					foreach (string item in items)
					{
						int taskId = Int32.Parse(item);
						taskManager.RemoveRequirementAssociation(taskId, userId);
					}
				}
				else if (operation == "RemoveFromRisk")
				{
					//Iterate through all the passed in tasks and remove their risk association
					TaskManager taskManager = new TaskManager();
					foreach (string item in items)
					{
						int taskId = Int32.Parse(item);
						taskManager.RemoveRiskAssociation(taskId, userId);
					}
				}
				else
				{
					throw new NotImplementedException("Operation '" + operation + "' is not currently supported");
				}

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

		/// <summary>
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="taskId">The id of the task to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		public string RetrieveNameDesc(int? projectId, int taskId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the task business object
				TaskManager taskManager = new TaskManager();

				//Now retrieve the specific task - handle quietly if it doesn't exist
				try
				{
					string tooltip = "";
					//See if we have a task or folder
					if (taskId < 0)
					{
						//Task folder IDs are negative
						int taskFolderId = -taskId;

						TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);

						//See if we have any parent folders
						List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskFolder.ProjectId, taskFolder.TaskFolderId, false);
						foreach (TaskFolderHierarchyView parentFolder in parentFolders)
						{
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
						}

						tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskFolder.Name) + "</u>";
					}
					else
					{
						//We have a task
						TaskView taskView = taskManager.TaskView_RetrieveById(taskId);
						if (String.IsNullOrEmpty(taskView.Description))
						{
							//See if we have a requirement or folder it belongs to
							if (taskView.RequirementId.HasValue)
							{
								tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + " &gt; ";
							}
							else if (taskView.TaskFolderId.HasValue)
							{
								List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
								foreach (TaskFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
								}
							}
							tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true);
						}
						else
						{
							//See if we have a requirement or folder it belongs to
							if (taskView.RequirementId.HasValue)
							{
								tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + "</u> &gt; ";
							}
							else if (taskView.TaskFolderId.HasValue)
							{
								List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
								foreach (TaskFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
								}
							}

							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Description);
						}

						//See if we have any comments to append
						IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(taskId, Artifact.ArtifactTypeEnum.Task, false);
						if (comments.Count() > 0)
						{
							IDiscussion lastComment = comments.Last();
							tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
								GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
								GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
								Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
								);
						}
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the task, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for task");
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return Resources.Messages.Global_TooltipNotAvailable;
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
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_GENERAL;

			//Call the base method with the appropriate settings collection
			return UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
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
			if (!CurrentUserId.HasValue)
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);

			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_FILTERS_LIST;

			//Call the base method with the appropriate settings collection
			return UpdateFilters(userId, projectId, filters, filtersSettingsCollection, Artifact.ArtifactTypeEnum.Task);
		}

		public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
		{
            throw new NotImplementedException();
		}

		public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
		{
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the latest information on a single pull request task in the system
        /// </summary>
        /// <param name="userId">The user we're viewing the task as</param>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business objects
                PullRequestManager pullRequestManager = new PullRequestManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the task record for the specific task id
				PullRequest pullRequest = pullRequestManager.PullRequest_RetrieveById(artifactId);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (pullRequest.OwnerId.HasValue)
				{
					ownerId = pullRequest.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && pullRequest.CreatorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Finally populate the dataitem from the dataset
				if (pullRequest != null)
				{
					PopulateRow(dataItem, pullRequest, true);

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("TaskStatusId"))
					{
						dataItem.Fields["TaskStatusId"].Editable = false;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public void ToggleColumnVisibility(int projectId, string fieldName)
		{
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a list of pagination options that the user can choose from
        /// </summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_GENERAL);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
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
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_PULLREQUEST_GENERAL);
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
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="pullRequest">The entity containing the data</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		protected void PopulateRow(SortedDataItem dataItem, PullRequest pullRequest, bool editable)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = pullRequest.TaskId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, pullRequest.ConcurrencyDate);

			//Specify if it has an attachment or not
			dataItem.Attachment = pullRequest.IsAttachments;


			//The date and task effort fields are not editable for tasks
			List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "CompletionPercent", "ComponentId", "ProjectedEffort" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (pullRequest.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					PopulateFieldRow(dataItem, dataItemField, pullRequest, null, null, editable, PopulateEqualizer, null, null, readOnlyFields);

					//Apply the conditional formatting to the priority column (if displayed)
					if (dataItemField.FieldName == "TaskPriorityId" && pullRequest.TaskPriorityId.HasValue)
					{
						dataItemField.CssClass = "#" + pullRequest.TaskPriorityColor;
					}
				}
			}
		}

		/// <summary>
		/// Gets the list of lookup values and names for a specific lookup
		/// </summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
		protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
		{
			const string METHOD_NAME = "GetLookupValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings lookupValues = null;
				ReleaseManager releaseManager = new ReleaseManager();
				UserManager user = new UserManager();
				TaskManager taskManager = new TaskManager();
                SourceCodeManager sourceCodeManager = new SourceCodeManager();

				if (lookupName == "CreatorId" || lookupName == "OwnerId")
				{
					List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
				}
                if (lookupName == "SourceBranchId" || lookupName == "DestBranchId")
                {
                    List<VersionControlBranch> branches = sourceCodeManager.RetrieveBranches2(projectId);
                    lookupValues = ConvertLookupValues(branches.OfType<Entity>().ToList(), "BranchId", "Name");
                }
                if (lookupName == "TaskStatusId")
				{
					List<TaskStatus> statuses = taskManager.RetrieveStatuses();
					lookupValues = ConvertLookupValues(statuses.OfType<Entity>().ToList(), "TaskStatusId", "Name");
				}
				if (lookupName == "TaskPriorityId")
				{
					List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(priorities.OfType<Entity>().ToList(), "TaskPriorityId", "Name");
				}
				if (lookupName == "ReleaseId")
				{
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
					lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
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
		/// Populates the equalizer type graph for the task progress
		/// </summary>
		/// <param name="dataItemField">The field being populated</param>
		/// <param name="artifact">The data row</param>
		protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
		{
			//Explicitly recast the entity to the type we're expecting
			TaskView taskView = (TaskView)artifact;

			//Calculate the information to display
			int percentGreen;
			int percentRed;
			int percentYellow;
			int percentGray;
			Task task = taskView.ConvertTo<TaskView, Task>();
			string tooltipText = TaskManager.CalculateProgress(task, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out percentGreen, out percentRed, out percentYellow, out percentGray);

			//Now populate the equalizer graph
			dataItemField.EqualizerGreen = percentGreen;
			dataItemField.EqualizerRed = percentRed;
			dataItemField.EqualizerYellow = percentYellow;
			dataItemField.EqualizerGray = percentGray;

			//Populate Tooltip
			dataItemField.TextValue = "";
			dataItemField.Tooltip = tooltipText;
		}

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the tasks as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList)
		{
            //Name
            DataItemField dataItemField = new DataItemField();
            dataItemField.FieldName = "Name";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
            dataItemField.Caption = Resources.Fields.Name;
            dataItemField.Editable = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
            }

            //Source Branch
            dataItemField = new DataItemField();
            dataItemField.FieldName = "SourceBranchId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "SourceBranchName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.SourceBranch;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is Int32)
            {
                int intFilter = (int)filterList[dataItemField.FieldName];
                dataItemField.TextValue = intFilter.ToString();
            }
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Dest Branch
            dataItemField = new DataItemField();
            dataItemField.FieldName = "DestBranchId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "DestBranchName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.DestBranch;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is Int32)
            {
                int intFilter = (int)filterList[dataItemField.FieldName];
                dataItemField.TextValue = intFilter.ToString();
            }
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Status
            dataItemField = new DataItemField();
            dataItemField.FieldName = "TaskStatusId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "TaskStatusName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Status;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Owner
            dataItemField = new DataItemField();
            dataItemField.FieldName = "OwnerId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
            dataItemField.LookupName = "OwnerName";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Owner;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is MultiValueFilter)
            {
                MultiValueFilter multiValueFilter = (MultiValueFilter)filterList[dataItemField.FieldName];
                dataItemField.TextValue = multiValueFilter.ToString();
            }

            //Release
            dataItemField = new DataItemField();
            dataItemField.FieldName = "ReleaseId";
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup;
            dataItemField.LookupName = "ReleaseVersionNumber";
            dataItemField.Lookups = GetLookupValues(dataItemField.FieldName, projectId, projectTemplateId);
            dataItemField.Caption = Resources.Fields.Release;
            dataItemField.Editable = true;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName) && filterList[dataItemField.FieldName] is Int32)
            {
                int intVaue = (int)filterList[dataItemField.FieldName];
                dataItemField.IntValue = intVaue;
            }

            //Last Update Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = "LastUpdateDate";
            dataItemField.Caption = Resources.Fields.LastUpdated;
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

            //TaskId
            dataItemField = new DataItemField();
            dataItemField.FieldName = "TaskId";
            dataItemField.Caption = Resources.Fields.ID;
            dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
            dataItemField.AllowDragAndDrop = true;
            dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Set the filter value (if one is set)
            if (filterList != null && filterList.Contains(dataItemField.FieldName))
            {
                dataItemField.IntValue = (int)filterList[dataItemField.FieldName];
            }
        }

		/// <summary>
		/// Used to populate the shape of the special compound fields used to display the information
		/// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
		/// </summary>
		/// <param name="projectTemplateId">the id of the project template</param>
		/// <param name="dataItemField">The field whose shape we're populating</param>
		/// <param name="fieldName">The field name we're handling</param>
		/// <param name="filterList">The list of filters</param>
		/// <param name="projectId">The project we're interested in</param>
		protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
		{
			//Check to see if this is a field we can handle
			if (fieldName == "ProgressId")
			{
				dataItemField.FieldName = "ProgressId";
				string filterLookupName = fieldName;
				dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(filterLookupName))
				{
					dataItemField.IntValue = (int)filterList[filterLookupName];
				}
			}
		}
	}

    #endregion
}
