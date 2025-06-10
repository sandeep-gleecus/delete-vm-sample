using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using DiffPlex.DiffBuilder.Model;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Provides the web service used to interacting with the various client-side source code file/folder list AJAX components
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class SourceCodeFileService : SortedListServiceBase, ISourceCodeFileService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService::";

        #region Source Code Methods

        /// <summary>
        /// Retrieves the source code file by its key and returns as text (if possible)
        /// Gets the latest revision for that branch
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fileKey">The id of the file</param>
        /// <param name="branchKey">the name of the branch</param>
        /// <returns>The raw content</returns>
        public string SourceCodeFile_OpenText(int projectId, string branchKey, string fileKey)
        {
            const string METHOD_NAME = "SourceCodeFile_OpenText";

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
                //Get the source code file as text
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                string text = sourceCodeManager.OpenFileAsText(fileKey, null, branchKey);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return text;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the source code file by its key and returns the markdown as HTML for display inline
        /// Gets the latest revision for that branch
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fileKey">The id of the file</param>
        /// <param name="branchKey">The current branch</param>
        /// <returns>The markdown converted to an HTML preview</returns>
        public string SourceCodeFile_OpenMarkdown(int projectId, string branchKey, string fileKey)
        {
            const string METHOD_NAME = "SourceCodeFile_OpenMarkdown";

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
                //Get the source code file
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                string text = sourceCodeManager.OpenFileAsText(fileKey, null, branchKey);

                //Finally convert the markdown to HTML
                string html = GlobalFunctions.Md2Html(text);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return html;
            }
            catch (ArtifactNotExistsException)
            {
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the count of files for a specific revision/commit
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="revisionKey">The key of the revision</param>
        /// <returns>The count</returns>
        public int SourceCodeFile_CountForRevision(int projectId, string revisionKey)
        {
            const string METHOD_NAME = "SourceCodeFile_CountForRevision";

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
                string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                int count = new SourceCodeManager(projectId).CountFilesForRevision(revisionKey, currentBranchKey);

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
        /// Sets the current branch for the project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="branchKey">The name/path of the branch</param>
        public void SourceCode_SetSelectedBranch(int projectId, string branchKey)
        {
            const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions()";
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
                //Set the current branch
                SourceCodeManager.Set_UserSelectedBranch(userId, projectId, branchKey);

                //We also need to update the current folder to the root (since the IDs are branch-specific)
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                SourceCodeFolder rootFolder = sourceCodeManager.RetrieveFoldersByParentId(null, branchKey).FirstOrDefault();
                if (rootFolder != null)
                {
                    SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, rootFolder.FolderKey);
                }

                Logger.LogExitingEvent(METHOD_NAME);
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

        #region IFormService methods

        /// <summary>Returns a single source code file for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current file</param>
        /// <returns>A file data item</returns>
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
                PopulateShape(projectId, CurrentUserId.Value, dataItem, null, false);

                //Retrieve the specific source code file
                string branchKey;
                SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileById(artifactId.Value, out branchKey);

                //Populate the row data item
                PopulateRow(projectId, dataItem, sourceCodeFile);
                
                //Provide the file key as a field
                DataItemField fileKeyField = new DataItemField();
                fileKeyField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                fileKeyField.FieldName = "FileKey";
                fileKeyField.TextValue = sourceCodeFile.FileKey;
                dataItem.Fields.Add(fileKeyField.FieldName, fileKeyField);

                //Provide the file type name as a field
                string imageUrl = "Filetypes/" + GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);
                Dictionary<string, string> fileTypeInfo = GlobalFunctions.GetFileTypeInformation(sourceCodeFile.Name);
                DataItemField fileTypeField = new DataItemField();
                fileTypeField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
                fileTypeField.FieldName = "FileType";
                fileTypeField.TextValue = fileTypeInfo["description"];
                fileTypeField.Tooltip = imageUrl;
                dataItem.Fields.Add(fileTypeField.FieldName, fileTypeField);

                //Provide the mime type as a field
                string mimeType = GlobalFunctions.GetFileMimeType(sourceCodeFile.Name);
                dataItem.Fields.Add("_MimeType", new DataItemField() { FieldName = "_MimeType", TextValue = mimeType });

                //Populate the folder path as a special field
                List<SourceCodeFolder> parentFolders = sourceCodeManager.RetrieveParentFolders(sourceCodeFile.FileKey, branchKey);
                parentFolders.Reverse();    //We want it root first
                string pathArray = "[";
                bool isFirst = true;
                foreach (SourceCodeFolder parentFolder in parentFolders)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        pathArray += ",";
                    }
                    pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "\", \"id\": " + parentFolder.FolderId + " }";
                }
                pathArray += "]";
                dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });

                //Override the URL for the 'Latest Revision' on the details page (vs in the file grid)
                dataItem.Fields[SourceCodeManager.FIELD_COMMIT].Tooltip = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeFile.RevisionKey + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + sourceCodeFile.FileKey);

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

        #region IItemSelectorService Methods

        /// <summary>
        /// Retrieves the list of existing source code files (no folders)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="standardFilters"></param>
        /// <returns>The list of source code files</returns>
        /// <remarks>
        /// Used in the 'Add Existing Files' option where we don't want any folders to appear
        /// </remarks>
        public ItemSelectorData ItemSelector_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "ItemSelector_Retrieve";

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
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

            try
            {
                //Instantiate the source code business object
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);

                //Create the array of data items (including the first filter item)
                ItemSelectorData itemSelectorData = new ItemSelectorData();
                List<DataItem> dataItems = itemSelectorData.Items;

                //Get the current folder
                string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(userId, projectId);

                //See if we have a branch specified
                string branchKey = null;
                if (standardFilters != null && standardFilters.ContainsKey("BranchKey"))
                {
                    branchKey = (string)GlobalFunctions.DeSerializeValue(standardFilters["BranchKey"]);
                }
                else
                {
                    //Use the current branch instead
                    branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);
                }

                //Get the id of the folder
                int selectedFolderId = 1;
                SourceCodeFolder folder = sourceCodeManager.RetrieveFolderByKey(selectedFolderKey, branchKey);
                if (folder != null)
                {
                    selectedFolderId = folder.FolderId;
                }

                int artifactCount;
                List<SourceCodeFile> sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(
                    selectedFolderId,
                    SourceCodeManager.FIELD_NAME,
                    true,
                    1,
                    Int32.MaxValue,
                    null,
                    out branchKey,
                    out artifactCount);

                //Iterate through all the files and populate the dataitem (only some columns are needed)
                foreach (SourceCodeFile sourceCodeFile in sourceCodeFiles)
                {
                    //Create the data-item
                    DataItem dataItem = new DataItem();

                    //Populate the necessary fields
                    dataItem.PrimaryKey = sourceCodeFile.FileId;

                    //Attachment Id
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Id";
                    dataItemField.IntValue = sourceCodeFile.FileId;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Name
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = sourceCodeFile.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //Icon
                    dataItemField.Tooltip = "Filetypes/" + GlobalFunctions.GetFileTypeImage(dataItemField.TextValue);

                    //Add to the items collection
                    dataItems.Add(dataItem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return itemSelectorData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region IListService methods

        /// <summary>Returns a list of pagination options that the user can choose from</summary>
        /// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
        public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrievePaginationOptions()";
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
				//Get the list of options
                SortedList<int, int> paginationOptions = new ManagerBase().GetPaginationOptions();

				//Get the current pagination setting for the project/user
				int numPerPage, numPage;
				SourceCodeManager.Get_UserPagnationFiles(userId, projectId, out numPerPage, out numPage);

				//Reformulate into a dictionary where the value indicates if it's the selected value or not (true/false)
				JsonDictionaryOfStrings paginationDictionary = new JsonDictionaryOfStrings();
				foreach (KeyValuePair<int, int> kvp in paginationOptions)
				{
					//See if this is the selected value or not
					paginationDictionary.Add(kvp.Key.ToString(), (kvp.Key == numPerPage) ? "true" : "false");
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return paginationDictionary;
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
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			const string METHOD_NAME = "UpdateFilters()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			try
			{
				//Get the current filters from the source code cached settings
				Hashtable savedFilters = new Hashtable();

                //Don't allow them to set a filter on size, it is not suported
                if (filters.ContainsKey(SourceCodeManager.FIELD_SIZE))
                {
                    throw new DataValidationException(String.Format(Resources.Messages.Services_CannotFilterByField, SourceCodeManager.FIELD_SIZE));
                }

                //Iterate through the filters, updating the project collection
                foreach (KeyValuePair<string, string> filter in filters)
				{
					string filterName = filter.Key;
					//Now get the type of field that we have. Since source code files are not a true artifact,
					//these values have to be hardcoded, as they're not stored in the TST_ARTIFACT_FIELD table
					DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					switch (filterName)
					{
						case "Name":
							artifactFieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
							break;
						case "LastUpdated":
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
				SourceCodeManager.Set_UserFilterFiles(userId, projectId, savedFilters);

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
			const string METHOD_NAME = CLASS_NAME + "UpdatePagination()";

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
				//Set the pagination settings on the source code object
				SourceCodeManager.Set_UserPagnationFiles(userId, projectId, pageSize, currentPage);
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
		}

		/// <summary>
		/// We return back the full path of the file.
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

                //If we have a negative ID it's a folder
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId.Value);
                string branchKey;
                if (artifactId < 0)
                {
                    //Get the folder by its ID
                    int folderId = -artifactId;
                    SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderById(folderId, out branchKey);
                    tooltip = sourceCodeFolder.FolderKey;
                }
                else
                {
                    //Get the file by its ID
                    SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileById(artifactId, out branchKey);
                    tooltip = sourceCodeFile.FileKey;
                }

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

        #endregion

        #region ISortedListService methods

        /// <summary>
        /// Allows sorted lists with folders to focus on a specific item and open its containing folder
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="artifactId">Id of a source code file (or negative for a folder)</param>
        /// <returns>The id of the folder (if any)</returns>
        public override int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters)
        {
            const string METHOD_NAME = "SortedList_FocusOn";

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
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

            try
            {
                //This method is only used for folders, so needs to be negative
                if (artifactId < 0)
                {
                    int sourceCodeFolderId = -artifactId;

                    //Retrieve this source code folder
                    SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                    string branchKey;
                    SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderById(sourceCodeFolderId, out branchKey);
                    if (sourceCodeFolder == null)
                    {
                        SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, null);
                    }
                    else
                    {
                        SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, sourceCodeFolder.FolderKey);
                    }
                    return sourceCodeFolderId;
                }
                return null;
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (ArtifactNotExistsException)
            {
                //Ignore, do not log
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Returns a list of source code files in the system for the specific user/project</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "SortedList_Retrieve()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			try
			{
				//Instantiate the source code business object and get the list of filters
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				Hashtable filters = SourceCodeManager.Get_UserFilterFiles(userId, projectId);
				int pageNum, numPerPage;
				string sortKey;
				bool sortAsc;
				SourceCodeManager.Get_UserPagnationFiles(userId, projectId, out numPerPage, out pageNum);
				SourceCodeManager.Get_UserSortFiles(userId, projectId, out sortAsc, out sortKey);
				string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(userId, projectId);

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//If we don't have a folder or revision key filter, just return the filter and nothing else
				string revisionKey = "";
				bool displayAction = false;
				if (standardFilters != null && standardFilters.ContainsKey("RevisionKey"))
				{
					revisionKey = (string)GlobalFunctions.DeSerializeValue(standardFilters["RevisionKey"]);
					displayAction = true;
				}

				//See if we have a branch specified
				string branchKey = null;
				if (standardFilters != null && standardFilters.ContainsKey("BranchKey"))
				{
					branchKey = (string)GlobalFunctions.DeSerializeValue(standardFilters["BranchKey"]);
				}
                else
                {
                    //Use the current branch instead
                    branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);
                }

                //We don't allow filtering on Size because we display different units (KB, MB, etc.) so it doesn't make sense
                if (filters.Contains(SourceCodeManager.FIELD_SIZE))
                {
                    throw new DataValidationException(String.Format(Resources.Messages.Services_CannotFilterByField, SourceCodeManager.FIELD_SIZE));
                }

                //Create the filter item first - we can clone it later
                SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, userId, filterItem, filters, displayAction);
				dataItems.Add(filterItem);
				sortedData.FilterNames = GetFilterNames(filters);

				if (String.IsNullOrWhiteSpace(selectedFolderKey) && String.IsNullOrWhiteSpace(revisionKey))
				{
					//Display the pagination information
					sortedData.CurrPage = 1;
					sortedData.PageCount = 0;
					sortedData.StartRow = 1;
					sortedData.VisibleCount = 0;
					sortedData.TotalCount = 0;

					//Display the sort information
					sortedData.SortProperty = sortKey;
					sortedData.SortAscending = sortAsc;
				}
				else
				{
					int selectedFolderId = 1;
					SourceCodeFolder folder = sourceCodeManager.RetrieveFolderByKey(selectedFolderKey, branchKey);
					if (folder != null)
					{
						selectedFolderId = folder.FolderId;
					}
                    
					//Get the source code files collection
					//If we have a revisionkey filter passed in as a standard filter, then we need to get the
					//files for a specific revision, otherwise just get the files for the current folder
					int artifactCount;
					List<SourceCodeFile> sourceCodeFiles = null;
					if (revisionKey == "")
					{
						sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(
							selectedFolderId,
							sortKey,
							sortAsc,
							(numPerPage * (pageNum - 1)) + 1,
							numPerPage,
							filters,
							out                            branchKey,
							out artifactCount);

						if (sourceCodeFiles.Count < 1 && artifactCount > 0)
							sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(
								selectedFolderId,
								sortKey,
								sortAsc,
								1,
								numPerPage,
								filters,
								out                                 branchKey,
								out artifactCount);
					}
					else
					{
						sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision(
							revisionKey,
							branchKey,
							sortKey,
							sortAsc,
							(numPerPage * (pageNum - 1)) + 1,
							numPerPage,
							filters,
							out artifactCount);

						//If we're past page 1 and there's nothing to view, reset to page 1.
						if (sourceCodeFiles.Count < 1 && artifactCount > 0)
							sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision(
								revisionKey,
								branchKey,
								sortKey,
								sortAsc,
								1,
								numPerPage,
								filters,
								out artifactCount);

					}

					int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)numPerPage);
					//Make sure that the current page is not larger than the number of pages or less than 1
					if (pageNum > pageCount)
					{
						pageNum = pageCount;
						SourceCodeManager.Set_UserPagnationFiles(userId, projectId, numPerPage, pageNum);
					}
					if (pageNum < 1)
					{
						pageNum = 1;
						SourceCodeManager.Set_UserPagnationFiles(userId, projectId, numPerPage, pageNum);
					}

					//**** Now we need to actually populate the rows of data to be returned ****
					int startRow = ((pageNum - 1) * numPerPage) + 1;

					//Display the pagination information
					sortedData.CurrPage = pageNum;
					sortedData.PageCount = pageCount;
					sortedData.StartRow = startRow;
					sortedData.VisibleCount = sourceCodeFiles.Count;
					sortedData.TotalCount = artifactCount;

					//Display the sort information
					sortedData.SortProperty = sortKey;
					sortedData.SortAscending = sortAsc;

                    //When viewing for a folder we also need to include the direct folders/subfolders
                    if (revisionKey == "")
                    {
                        List<SourceCodeFolder> sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(selectedFolderId, branchKey);

                        //Iterate through all the source code folders and populate the data items
                        foreach (SourceCodeFolder sourceCodeFolder in sourceCodeFolders)
                        {
                            //We clone the template item as the basis of all the new items
                            SortedDataItem dataItem = filterItem.Clone();

                            //Now populate with the data
                            PopulateRow(projectId, dataItem, sourceCodeFolder);
                            dataItems.Add(dataItem);
                        }
                    }

                    //Iterate through all the files and populate the dataitem
                    foreach (SourceCodeFile sourceCodeFile in sourceCodeFiles)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(projectId, dataItem, sourceCodeFile, revisionKey);
						dataItems.Add(dataItem);
					}
				}

				//Also include the pagination info
				sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

				Logger.LogExitingEvent(METHOD_NAME);
				return sortedData;
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
        /// Updates the current sort stored in the system (property and direction)
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The artifact property we want to sort on</param>
        /// <param name="sortAscending">Are we sorting ascending or not</param>
        /// <returns>Any error messages</returns>
        public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
        {
            const string METHOD_NAME = CLASS_NAME + "UpdateSort()";

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
                SourceCodeManager.Set_UserSortFiles(userId, projectId, sortAscending, sortProperty);
            }
            catch (SourceCodeCacheInvalidException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                return "Error Updating Sort (" + exception.Message + ")";
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return "";  //Success
        }

        #endregion

        #region Internal Functions

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="userId">The user we're viewing the files as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="displayAction">Should we display the action field</param>
        protected void PopulateShape(int projectId, int userId, SortedDataItem dataItem, Hashtable filterList, bool displayAction)
		{
			//We need to add the various source code file fields to be displayed
			//File Name
			DataItemField dataItemField = new DataItemField();
			dataItemField.FieldName = "Name";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.Filename;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}

			//File Size
			dataItemField = new DataItemField();
			dataItemField.FieldName = "Size";
			dataItemField.Caption = Resources.Fields.Size;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
            //Filtering is not supported

			//Author / Uploaded By
			dataItemField = new DataItemField();
			dataItemField.FieldName = SourceCodeManager.FIELD_AUTHOR;
			dataItemField.Caption = Resources.Fields.AuthorId;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}

			//Revision
			dataItemField = new DataItemField();
			dataItemField.FieldName = "Revision";
			dataItemField.Caption = Resources.Fields.LatestRevision;
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
			dataItemField.AllowDragAndDrop = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			//Set the filter value (if one is set)
			if (filterList != null && filterList.Contains(dataItemField.FieldName))
			{
				dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
			}

            //Last Edited Date
            dataItemField = new DataItemField();
            dataItemField.FieldName = "LastUpdated";
            dataItemField.Caption = Resources.Fields.LastEdited;
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

            //If we're retrieving for a revision, display the action
            if (displayAction)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = SourceCodeManager.FIELD_ACTION;
				dataItemField.Caption = Resources.Fields.Action;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField.AllowDragAndDrop = true;

                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(dataItemField.FieldName))
				{
					dataItemField.TextValue = (string)filterList[dataItemField.FieldName];
				}
			}
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="sourceCodeFile">The source code file object</param>
        /// <param name="revisionKey">The current revision (if files for a specific revision)</param>
		protected void PopulateRow(int projectId, SortedDataItem dataItem, SourceCodeFile sourceCodeFile, string revisionKey = null)
		{
			//Set the primary key
			dataItem.PrimaryKey = sourceCodeFile.FileId;

            //Set the custom URL, depends on whether we are in the context of a revision or not
            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_ACTION))
            {
                dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + revisionKey + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + sourceCodeFile.FileKey);
            }
            else
            {
                dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + HttpUtility.UrlEncode(sourceCodeFile.FileKey));
            }

            //Source Code Files don't have an attachment flag
            dataItem.Attachment = false;

			//We need to add the various source code file fields to be displayed
			//File Name
			DataItemField dataItemField = dataItem.Fields[SourceCodeManager.FIELD_NAME];
			dataItemField.Editable = false;
			dataItemField.Required = false;
			//The filetype image is passed as a 'tooltip'
			dataItemField.TextValue = sourceCodeFile.Name;
			dataItemField.Tooltip = "Filetypes/" + GlobalFunctions.GetFileTypeImage(sourceCodeFile.Name);

			//File Size
			dataItemField = dataItem.Fields[SourceCodeManager.FIELD_SIZE];
			dataItemField.Editable = false;
			dataItemField.Required = false;
			dataItemField.IntValue = sourceCodeFile.Size;
            dataItemField.TextValue = sourceCodeFile.Size.DisplayFileSizeBytes();

            //Author / Uploaded By
            dataItemField = dataItem.Fields[SourceCodeManager.FIELD_AUTHOR];
			dataItemField.Editable = false;
			dataItemField.Required = false;
			dataItemField.TextValue = sourceCodeFile.AuthorName;

			//Revision
			dataItemField = dataItem.Fields[SourceCodeManager.FIELD_COMMIT];
			dataItemField.Editable = false;
			dataItemField.Required = false;
            dataItemField.IntValue = sourceCodeFile.RevisionId;
			dataItemField.TextValue = sourceCodeFile.RevisionName;
			//Pass a url in the 'tooltip' field
			dataItemField.Tooltip = UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "=" + sourceCodeFile.RevisionKey);

            //Last Edited Date
            //If we have Min Date (1/1/0001 display as blank vs. weird date)
            dataItemField = dataItem.Fields[SourceCodeManager.FIELD_LASTUPDATED];
            dataItemField.Editable = false;
            dataItemField.Required = false;
            if (sourceCodeFile.LastUpdateDate == DateTime.MinValue)
            {
                dataItemField.TextValue = "-";
                dataItemField.Tooltip = "-";
            }
            else
            {
                dataItemField.Tooltip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(sourceCodeFile.LastUpdateDate));
                dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(sourceCodeFile.LastUpdateDate));
            }
            //Action
            if (dataItem.Fields.ContainsKey(SourceCodeManager.FIELD_ACTION))
			{
				dataItemField = dataItem.Fields[SourceCodeManager.FIELD_ACTION];
				dataItemField.Editable = false;
				dataItemField.Required = false;
				dataItemField.TextValue = sourceCodeFile.Action;
            }
		}

        /// <summary>
        /// Populates a folder data item from a source code folder object
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="sourceCodeFolder">The source code folder object</param>
        protected void PopulateRow(int projectId, SortedDataItem dataItem, SourceCodeFolder sourceCodeFolder)
        {
            //Set the primary key (negative for folders)
            dataItem.PrimaryKey = -sourceCodeFolder.FolderId;
            dataItem.Folder = true;

            //Source Code Folders don't have an attachment flag
            dataItem.Attachment = false;

            //We need to add the various source code file fields to be displayed
            //File Name
            DataItemField dataItemField = dataItem.Fields[SourceCodeManager.FIELD_NAME];
            dataItemField.Editable = false;
            dataItemField.Required = false;
            //The filetype image is passed as a 'tooltip'
            dataItemField.TextValue = sourceCodeFolder.Name;
            dataItemField.Tooltip = "Folder.svg";
        }

        #endregion

        #region ITreeViewService methods

        /// <summary>
        /// Returns the list of document folders contained in a parent node
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="parentId">The id of the parent folder</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of treeview nodes to display</returns>
        public List<TreeViewNode> TreeView_GetNodes(int projectId, string parentId)
		{
			const string METHOD_NAME = "TreeView_GetNodes";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			int userId = this.CurrentUserId.Value;

			try
			{
				List<TreeViewNode> nodes = new List<TreeViewNode>();

				//Get the list of source code file folders from the business object
				SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
				string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                //See if we have a branch
                SourceCodeBranch branch = null;
                if (String.IsNullOrEmpty(currentBranchKey))
                {
                    //Get the default branch
                    branch = sourceCodeManager.RetrieveBranches().FirstOrDefault(b => b.IsDefault);
                }
                else
                {
                    branch = sourceCodeManager.RetrieveBranchByName(currentBranchKey);
                }
                if (branch == null)
                {
                    //Try the default branch
                    branch = sourceCodeManager.RetrieveBranches().FirstOrDefault(b => b.IsDefault);
                }
                if (branch == null)
                {
                    //No branches, even default
                    throw new SourceCodeCacheInvalidException();
                }

                List<SourceCodeFolder> sourceCodeFolders;
				int? sourceCodeFolderId = null;
				if (!String.IsNullOrEmpty(parentId))
					sourceCodeFolderId = Int32.Parse(parentId);
				sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(sourceCodeFolderId, currentBranchKey);

				foreach (SourceCodeFolder sourceCodeFolder in sourceCodeFolders)
				{
					//Add the node
					nodes.Add(new TreeViewNode(sourceCodeFolder.FolderId.ToString(), SafeSubstring(sourceCodeFolder.Name, 22), sourceCodeFolder.Name));
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return nodes;
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

		/// <summary>
		/// Sets the currently selected node so that it can be persisted for future page loads
		/// </summary>
		/// <param name="nodeId">The id of the node to persist</param>
		/// <param name="projectId">The id of the project</param>
		public void TreeView_SetSelectedNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = CLASS_NAME + "TreeView_SetSelectedNode()";
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
				//Try to convert the node ID to a number, first..
				int selFolderId = 0;
				if (int.TryParse(nodeId, out selFolderId))
				{
					//Get the folder key
					SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
					string ignBranch = null;
					SourceCodeFolder folder = sourceCodeManager.RetrieveFolderById(selFolderId, out ignBranch);
					if (folder == null)
					{
						SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, null);
					}
					else
					{
						SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, folder.FolderKey);
					}
				}
				else
				{
					SourceCodeManager.Set_UserSelectedSourceFolder(userId, projectId, null);
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
		}

		/// <summary>
		/// Gets a comma-separated list of parent nodes that are to be expanded based on the selected node stored
		/// in the project settings collection. Used when the page is first loaded or when refresh is clicked
		/// </summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the project</param>
		public List<string> TreeView_GetExpandedNodes(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "TreeView_GetExpandedNodes()";
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
				List<string> nodeList = new List<string>();

				//Get the currently selected node (if there is one) and branch
				SourceCodeManager sourceCode = new SourceCodeManager(projectId);
				string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(userId, projectId);
				string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);
				if (!String.IsNullOrEmpty(selectedFolderKey))
				{
					//Get the folder id
					SourceCodeFolder folder = sourceCode.RetrieveFolderByKey(selectedFolderKey, currentBranchKey);
					if (folder != null)
					{
						int selectedNodeId = folder.FolderId;
						//Need to iterate through all the parent folders in the list until we get back to the root
						nodeList.Add(selectedNodeId.ToString());
						Logger.LogTraceEvent(METHOD_NAME, "Added node " + selectedNodeId.ToSafeString());
						int? parentNodeId = sourceCode.RetrieveParentFolder(selectedNodeId, currentBranchKey);
						while (parentNodeId.HasValue)
						{
							Logger.LogTraceEvent(METHOD_NAME, "Inserted node " + parentNodeId);
							nodeList.Insert(0, parentNodeId.ToString());
							parentNodeId = sourceCode.RetrieveParentFolder(parentNodeId.Value, currentBranchKey);
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return nodeList;
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
		/// Returns the tooltip for a node (used if not provided when node created)
		/// </summary>
		/// <param name="nodeId">The id of the node</param>
		/// <returns>The tooltip</returns>
		public string TreeView_GetNodeTooltip(string nodeId)
		{
			//Not needed since all nodes have tooltips on created
			return null;
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
        /// Returns a list of source code files for display in the navigation bar
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
                Hashtable filterList = SourceCodeManager.Get_UserFilterFiles(userId, projectId);
                string sortKey;
                bool sortAsc;
                SourceCodeManager.Get_UserSortFiles(userId, projectId, out sortAsc, out sortKey);
                int pageCurrent, numPerPage;
                SourceCodeManager.Get_UserPagnationFiles(userId, projectId, out numPerPage, out pageCurrent);

                //Get the branch name
                string branchKey = SourceCodeManager.Get_UserSelectedBranch(userId, projectId);

                //Get the currently selected folder
                SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
                string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(userId, projectId);
                int selectedFolderId = 0;
                SourceCodeFolder folder = sourceCodeManager.RetrieveFolderByKey(selectedFolderKey, branchKey);
                if (folder != null)
                {
                    selectedFolderId = folder.FolderId;
                }

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Get the list of revisions
                int artifactCount;
                List<SourceCodeFile> sourceCodeFiles = new List<SourceCodeFile>();

                //See which display mode we're using
                if (displayMode == 1)
                {
                    //Filtered list
                    sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(
                        selectedFolderId,
                        sortKey,
                        sortAsc,
                        (numPerPage * (pageCurrent - 1)) + 1,
                        numPerPage,
                        filterList,
                        out branchKey,
                        out artifactCount);
                }
                if (displayMode == 2)
                {
                    //All items
                    sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(
                        selectedFolderId,
                        sortKey,
                        sortAsc,
                        (numPerPage * (pageCurrent - 1)) + 1,
                        numPerPage,
                        null,
                        out branchKey,
                        out artifactCount);
                }

                //Iterate through all the revisions and populate the dataitem (only some columns are needed)
                foreach (SourceCodeFile sourceCodeFile in sourceCodeFiles)
                {
                    //Create the data-item
                    HierarchicalDataItem dataItem = new HierarchicalDataItem();
                    dataItem.CustomUrl = UrlRewriterModule.ResolveUrl("~/SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "=" + sourceCodeFile.FileKey);

                    //Populate the necessary fields
                    dataItem.PrimaryKey = sourceCodeFile.FileId;
                    dataItem.Indent = "";
                    dataItem.Expanded = false;

                    //Name/Desc
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Name";
                    dataItemField.TextValue = sourceCodeFile.Name;
                    dataItem.Summary = false;
                    dataItem.Alternate = false;

                    //The image is passed through the 'tooltip' field
                    dataItemField.Tooltip = "Images/Filetypes/" + GlobalFunctions.GetFileTypeImage(dataItemField.TextValue);
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

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
			Project.AuthorizationState authorizationState = IsAuthorizedToViewSourceCode(projectId);
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

        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public void ToggleColumnVisibility(int projectId, string fieldName)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public void SortedList_Copy(int projectId, List<string> items)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public void SortedList_Export(int destProjectId, List<string> items)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        public void TreeView_DragDestination(int projectId, int[] artifactIds, int nodeId)
		{
			//Source code doesn't allow drag and drops
			throw new NotImplementedException();
		}

		public JsonDictionaryOfStrings TreeView_GetAllNodes(int containerId)
		{
			throw new NotImplementedException();
		}

        public string TreeView_AddNode(int containerId, string name, string parentNodeId, string description)
		{
			throw new NotImplementedException();
		}

		public void TreeView_UpdateNode(int containerId, string nodeId, string name, string parentNodeId, string description)
		{
			throw new NotImplementedException();
		}

		public void TreeView_DeleteNode(int containerId, string nodeId)
		{
			throw new NotImplementedException();
		}

		public string TreeView_GetParentNode(int containerId, string nodeId)
		{
			throw new NotImplementedException();
		}

        public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
        {
            throw new NotImplementedException("Not Implemented for Source Code Files");
        }

        #endregion
    }
}
