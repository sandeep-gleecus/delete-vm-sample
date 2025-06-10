using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// uploading and retrieving attachments in the system
	/// </summary>
	public class AttachmentManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.AttachmentManager::";

		#region Enumerations

		public enum DisplayMode
		{
			CurrentFolder = 1,
			AllItems = 2
		}

		#endregion

		protected const string DEFAULT_VERSION_NUMBER = "1.0";
		public const string DEFAULT_FOLDER_NAME = "Root Folder";
		public const string DEFAULT_TYPE_NAME = "Default";
		public static List<DocumentStatus> _staticReleaseStatuses = null;
		#region Internal Methods

		/// <summary>
		/// Copies across the document fields and workflows from one project template to another
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="documentWorkflowMapping">The workflow mapping</param>
		/// <param name="documentTypeMapping">The type mapping</param>
		/// <param name="documentStatusMapping">The status mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> documentWorkflowMapping, Dictionary<int, int> documentStatusMapping, Dictionary<int, int> documentTypeMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			//First we need to copy across the document statuses
			List<DocumentStatus> documentStati = this.DocumentStatus_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < documentStati.Count; i++)
			{
				int oldStatusId = documentStati[i].DocumentStatusId;
				int newStatusId = this.DocumentStatus_Insert(
					newProjectTemplateId,
					documentStati[i].Name,
					documentStati[i].IsOpenStatus,
					documentStati[i].IsDefault,
					documentStati[i].IsActive,
					documentStati[i].Position);
				documentStatusMapping.Add(oldStatusId, newStatusId);
			}

			//***** Now we need to copy across the document workflows *****
			DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
			workflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, documentWorkflowMapping, documentStatusMapping);

			//***** Now we need to copy across the document types *****
			List<DocumentType> documentTypes = this.RetrieveDocumentTypes(existingProjectTemplateId, false);
			for (int i = 0; i < documentTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (documentWorkflowMapping.ContainsKey(documentTypes[i].DocumentWorkflowId))
				{
					int workflowId = (int)documentWorkflowMapping[documentTypes[i].DocumentWorkflowId];
					int newDocumentTypeId = this.InsertDocumentType(
						newProjectTemplateId,
						documentTypes[i].Name,
						documentTypes[i].Description,
						documentTypes[i].IsActive,
						documentTypes[i].IsDefault,
						workflowId
						);
					documentTypeMapping.Add(documentTypes[i].DocumentTypeId, newDocumentTypeId);
				}
			}
		}

		/// <summary>
		/// Creates the document types, statuses, default workflow, transitions and field states
		/// for a new project template using the default template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project</param>
		internal void CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateDefaultEntriesForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to create the document statuses
				this.DocumentStatus_Insert(projectTemplateId, "Draft", true, true, true, 1);
				this.DocumentStatus_Insert(projectTemplateId, "Under Review", true, false, true, 2);
				this.DocumentStatus_Insert(projectTemplateId, "Approved", true, false, true, 3);
				this.DocumentStatus_Insert(projectTemplateId, "Completed", false, false, true, 4);
				this.DocumentStatus_Insert(projectTemplateId, "Rejected", false, false, true, 5);
				this.DocumentStatus_Insert(projectTemplateId, "Retired", false, false, true, 6);
				this.DocumentStatus_Insert(projectTemplateId, "Checked Out", true, false, true, 7);

				//Next we need to create a default workflow for a project
				DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
				int workflowId = workflowManager.Workflow_InsertWithDefaultEntries(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true).DocumentWorkflowId;

				//Next we need to create the document types, associated with this workflow
				InsertDocumentType(projectTemplateId, DEFAULT_TYPE_NAME, null, true, true, workflowId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Attachment functions

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int projectId, int attachmentId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				ProjectAttachmentView notificationArt = RetrieveForProjectById2(projectId, attachmentId);
				notificationArt.MarkAsAdded();
				new NotificationManager().SendNotificationForArtifact(notificationArt, artifactCustomProperty, newComment);
			}
			catch (Exception exception)
			{
				//Log, but don't throw;
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Inserts a new URL attachment into the system and associates it with the provided artifact
		/// </summary>
		/// <param name="url">The URL to be attachment</param>
		/// <param name="description">An optional detailed description of the attachment</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="artifactId">The id of the artifact to associate the attachment with</param>
		/// <param name="artifactType">The type of artifact to associate the attachment with</param>
		/// <param name="version">The version number</param>
		/// <param name="tags">Any comma-separated meta-tags</param>
		/// <param name="projectId">The project we're uploading the document into</param>
		/// <param name="tags">The meta-tags to associate with the document</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="documentTypeId">The type of document being attached (optional)</param>
		/// <param name="projectAttachmentFolderId">The project folder to put the document into (optional)</param>
		/// <param name="documentStatusId">The document status (null = default)</param>
		/// <returns>The id of the attachment</returns>
		/// <remarks>This overload is used for URL attachments</remarks>
		public int Insert(int projectId, string url, string description, int authorId, int? artifactId, Artifact.ArtifactTypeEnum artifactType, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId, int? documentStatusId, int? userId = null)
		{
			const string METHOD_NAME = "Insert(Url)";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int size = 0; // All URL attachments are displayed as 0KB

			try
			{
				int attachmentId;

				//If no version specified, default to 1.0
				if (string.IsNullOrWhiteSpace(version))
				{
					version = DEFAULT_VERSION_NUMBER;
				}

				//Default status if none specified
				if (!documentStatusId.HasValue)
				{
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					documentStatusId = GetDefaultDocumentStatus(projectTemplateId);
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First the attachment object
					Attachment attachment = new Attachment();
					attachment.AttachmentTypeId = (int)Attachment.AttachmentTypeEnum.URL;
					attachment.AuthorId = authorId;
					attachment.EditorId = authorId;
					attachment.Filename = url;
					attachment.Description = description;
					attachment.UploadDate = DateTime.UtcNow;
					attachment.EditedDate = DateTime.UtcNow;
					attachment.ConcurrencyDate = DateTime.UtcNow;
					attachment.Size = size;
					attachment.CurrentVersion = version;
					attachment.DocumentStatusId = documentStatusId.Value;

					//Actually save the object
					context.Attachments.AddObject(attachment);
					context.SaveChanges();
					attachmentId = attachment.AttachmentId;

					//Next the tags object
					ArtifactTag attachmentTag = new ArtifactTag();
					attachmentTag.Tags = tags;
					attachmentTag.ProjectId = projectId;
					attachmentTag.ArtifactId = attachmentId;
					attachmentTag.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document;
					context.ArtifactTags.AddObject(attachmentTag);
					context.SaveChanges();
				}

				//Add a new history entry for the creation
				HistoryManager historyManager = new HistoryManager();
				historyManager.LogCreation(projectId, authorId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow);

				//Now insert the initial entry into the AttachmentVersion table
				//on the filesystem, linked to the attachment version id
				InsertVersion(projectId, attachmentId, url, description, authorId, version, true, false);

				//Now we need to associate the attachment with the appropriate artifact
				if (artifactId.HasValue && artifactType != Artifact.ArtifactTypeEnum.None)
				{
					InsertArtifactAssociation(projectId, attachmentId, artifactId.Value, artifactType);
				}

				//Finally we need to associate it with the project
				InsertProjectAssociation(projectId, attachmentId, projectAttachmentFolderId, documentTypeId);

				//If we have tags, need to update the tag frequency table
				if (!string.IsNullOrEmpty(tags))
				{
					UpdateTagFrequency(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new file attachment into the system and associates it with the provided artifact
		/// </summary>
		/// <param name="filename">The filename of the attachment</param>
		/// <param name="description">An optional detailed description of the attachment</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="artifactId">The id of the artifact to associate the attachment with</param>
		/// <param name="artifactType">The type of artifact to associate the attachment with</param>
		/// <param name="projectId">The project we're uploading the document into</param>
		/// <param name="tags">The meta-tags to associate with the document</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="documentTypeId">The type of document being attached (optional)</param>
		/// <param name="projectAttachmentFolderId">The project folder to put the document into (optional)</param>
		/// <param name="documentStatusId">The document status (null = default)</param>
		/// <returns>The id of the attachment</returns>
		/// <remarks>This overload is used for file attachments</remarks>
		public int Insert(int projectId, string filename, string description, int authorId, byte[] binaryData, int? artifactId, Artifact.ArtifactTypeEnum artifactType, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId, int? documentStatusId)
		{
			const string METHOD_NAME = CLASS_NAME + "Insert()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int size = 1; // Minimum 1KB

			try
			{
				int attachmentId;

				//Get the file size in KB
				if (binaryData.Length > 1024)
				{
					size = binaryData.Length / 1024;
				}

				//If no version specified, default to 1.0
				if (string.IsNullOrWhiteSpace(version))
				{
					version = DEFAULT_VERSION_NUMBER;
				}

				//Default status if none specified
				if (!documentStatusId.HasValue)
				{
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					documentStatusId = GetDefaultDocumentStatus(projectTemplateId);
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Attachment attachment = new Attachment();
					attachment.AttachmentTypeId = (int)Attachment.AttachmentTypeEnum.File;
					attachment.AuthorId = authorId;
					attachment.EditorId = authorId;
					attachment.Filename = filename;
					attachment.Description = description;
					attachment.UploadDate = DateTime.UtcNow;
					attachment.EditedDate = DateTime.UtcNow;
					attachment.ConcurrencyDate = DateTime.UtcNow;
					attachment.Size = size;
					attachment.CurrentVersion = version;
					attachment.DocumentStatusId = documentStatusId.Value;

					//Actually save the object
					context.Attachments.AddObject(attachment);
					context.SaveChanges();
					attachmentId = attachment.AttachmentId;

					//Next the tags object
					ArtifactTag attachmentTag = new ArtifactTag();
					attachmentTag.Tags = tags;
					attachmentTag.ProjectId = projectId;
					attachmentTag.ArtifactId = attachmentId;
					attachmentTag.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document;
					context.ArtifactTags.AddObject(attachmentTag);
					context.SaveChanges();
				}

				//Add a new history entry for the creation
				HistoryManager historyManager = new HistoryManager();
				historyManager.LogCreation(projectId, authorId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow);

				//Now insert the initial entry into the AttachmentVersion table and physically store the attachment data
				//on the filesystem, linked to the attachment version id
				InsertVersion(projectId, attachmentId, filename, description, authorId, binaryData, version, true, false);

				//Now we need to associate the attachment with the appropriate artifact
				if (artifactId.HasValue && artifactType != Artifact.ArtifactTypeEnum.None)
				{
					InsertArtifactAssociation(projectId, attachmentId, artifactId.Value, artifactType, authorId);
				}

				//Next we need to associate it with the project
				InsertProjectAssociation(projectId, attachmentId, projectAttachmentFolderId, documentTypeId);

				//If we have tags, need to update the tag frequency table
				if (!string.IsNullOrEmpty(tags))
				{
					UpdateTagFrequency(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Associates an attachment with an artifact in the system
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">The type of artifact</param>
		public void InsertArtifactAssociation(int projectId, int attachmentId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, int? userId = null)
		{
			const string METHOD_NAME = "InsertArtifactAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to verify that the destination artifact actually exists
				ArtifactManager artifactManager = new ArtifactManager();
				if (!artifactManager.VerifyArtifactExists(artifactType, artifactId, projectId))
				{
					//We can't associate with a non-existant artifact
					throw new ArtifactLinkDestNotFoundException(string.Format("The artifact id {0} of type {1} specified doesn't exist in project PR{2}", artifactId, (int)artifactType, projectId));
				}

				//Fill out the entity with data for new artifact attachment association
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ArtifactAttachment artifactAttachment = new ArtifactAttachment();
					artifactAttachment.AttachmentId = attachmentId;
					artifactAttachment.ArtifactId = artifactId;
					artifactAttachment.ArtifactTypeId = (int)artifactType;
					artifactAttachment.ProjectId = projectId;
					context.ArtifactAttachments.AddObject(artifactAttachment);
					context.SaveChanges();

					if (userId != null)
					{
						new HistoryManager().LogCreation(projectId, (int)userId, artifactType , artifactId , DateTime.UtcNow, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId);
					}
				}

				//Ensure that the appropriate artifact is marked as having attachments
				UpdateArtifactFlag(artifactType, artifactId, true);
			}
			catch (ArtifactLinkDestNotFoundException)
			{
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Inserts an association between an attachment and the project</summary>
		/// <param name="projectId">The project we're associating with</param>
		/// <param name="attachmentId">The attachment being associated</param>
		/// <param name="projectAttachmentFolderId">The folder id (optional)</param>
		/// <param name="documentTypeId">The document type (optional)</param>
		public void InsertProjectAssociation(int projectId, int attachmentId, int? projectAttachmentFolderId, int? documentTypeId)
		{
			const string METHOD_NAME = "InsertProjectAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//If we don't have a folder or type specified, get the default for the project
				if (!projectAttachmentFolderId.HasValue)
				{
					projectAttachmentFolderId = GetDefaultProjectFolder(projectId);
				}
				if (!documentTypeId.HasValue)
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					documentTypeId = GetDefaultDocumentType(projectTemplateId);
				}

				//Populate the new record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//make sure it's not ALREADY IN THE PROJECT...
					if (context.ProjectAttachments.Count(pa => pa.AttachmentId == attachmentId && pa.ProjectId == projectId) < 1)
					{
						ProjectAttachment projectAttachment = new ProjectAttachment();
						projectAttachment.AttachmentId = attachmentId;
						projectAttachment.ProjectId = projectId;
						projectAttachment.DocumentTypeId = documentTypeId.Value;
						projectAttachment.ProjectAttachmentFolderId = projectAttachmentFolderId.Value;
						context.ProjectAttachments.AddObject(projectAttachment);
						context.SaveChanges();
					}
					else
						Logger.LogInformationalEvent(METHOD_NAME, "Document #" + attachmentId + " already in Project #" + projectId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		///	Counts all the documents/urls attached to a specific artifact
		/// </summary>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <returns>The number of attachments</returns>
		/// <param name="filters">The list of filters</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <remarks>Used to display the 'has-data' indication on the project tabs</remarks>
		public int CountByArtifactId(int projectId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountByArtifactId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int artifactCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query, joinng on the artifact table
					var query = from p in context.ProjectAttachmentsView
								join a in context.ArtifactAttachments on p.AttachmentId equals a.AttachmentId
								where
									p.ProjectId == projectId &&
									a.ArtifactTypeId == (int)artifactType &&
									a.ArtifactId == artifactId
									&& !p.IS_DELETED
								
								select p;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<ProjectAttachmentView, bool>> filterClause = CreateFilterExpression<ProjectAttachmentView>(projectId, null, Artifact.ArtifactTypeEnum.Document, filters, utcOffset, null, HandleAttachmentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<ProjectAttachmentView>)query.Where(filterClause);
						}
					}

					//Get the count
					artifactCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Counts all the documents in a project folder
		/// </summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="folderId">The folder we're interested in (or null for all folders)</param>
		/// <returns>The number of attachments</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int CountForProject(int projectId, int? folderId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int artifactCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from p in context.ProjectAttachmentsView
								where p.ProjectId == projectId
							    && !p.IS_DELETED
								select p;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<ProjectAttachmentView, bool>> filterClause = CreateFilterExpression<ProjectAttachmentView>(projectId, null, Artifact.ArtifactTypeEnum.Document, filters, utcOffset, null, HandleAttachmentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<ProjectAttachmentView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						query = query.Where(p => p.ProjectAttachmentFolderId == folderIdValue);
					}

					//Get the count
					artifactCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a list of all the documents in a project for a specific document folder or all folders
		/// </summary>
		/// <param name="folderId">The document folder we're interested in (or null for all folders)</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <returns>List of attachments and custom properties</returns>
		public List<ProjectAttachmentView> RetrieveForProject(int projectId, int? folderId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectAttachmentView> attachments;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from p in context.ProjectAttachmentsView
								where p.ProjectId == projectId
								&& !p.IS_DELETED
								select p;

					//Add the dynamic sort
					if (string.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by filename ascending
						query = query.OrderBy(p => p.Filename).ThenBy(p => p.AttachmentId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "AttachmentId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<ProjectAttachmentView, bool>> filterClause = CreateFilterExpression<ProjectAttachmentView>(projectId, null, Artifact.ArtifactTypeEnum.Document, filters, utcOffset, null, HandleAttachmentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<ProjectAttachmentView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						query = query.Where(p => p.ProjectAttachmentFolderId == folderIdValue);
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						//Return nothing
						return new List<ProjectAttachmentView>();
					}

					//Execute the query
					attachments = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachments;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of attachments by associated with a particular artifact in the context of a project with filtering/sorting
		/// </summary>
		/// <param name="projectId">The ID of the project</param>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <param name="filters">The list of filters</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <returns>A project attachment list</returns>
		public List<ProjectAttachmentView> RetrieveByArtifactId(int projectId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveByArtifactId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectAttachmentView> attachments;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query, joinng on the artifact table
					var query = from p in context.ProjectAttachmentsView
								join a in context.ArtifactAttachments on p.AttachmentId equals a.AttachmentId
								where
									p.ProjectId == projectId &&
									a.ArtifactTypeId == (int)artifactType &&
									a.ArtifactId == artifactId
									&& !p.IS_DELETED
								select p;

					//Add the dynamic sort
					if (string.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by filename ascending
						query = query.OrderBy(p => p.Filename).ThenBy(p => p.AttachmentId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "AttachmentId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<ProjectAttachmentView, bool>> filterClause = CreateFilterExpression<ProjectAttachmentView>(projectId, null, Artifact.ArtifactTypeEnum.Document, filters, utcOffset, null, HandleAttachmentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<ProjectAttachmentView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						return new List<ProjectAttachmentView>();
					}

					//Execute the query
					attachments = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachments;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_ARTIFACT_SIGNATURE RetrieveDocumentSignature(int documentId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveDocumentSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of document in the project
				TST_ARTIFACT_SIGNATURE documentSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ArtifactSignatures
								where t.ARTIFACT_ID == documentId && t.ARTIFACT_TYPE_ID == artifactTypeId
								select t;

					query = query.OrderByDescending(r => r.UPDATE_DATE);

					documentSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return documentSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void DocumentSignatureInsert(int projectId, int currentStatusId, Attachment document, string meaning, int? loggedinUserId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				var newReqSignature = new TST_ARTIFACT_SIGNATURE
				{
					STATUS_ID = currentStatusId,
					ARTIFACT_ID = document.AttachmentId,
					ARTIFACT_TYPE_ID = (int)ArtifactTypeEnum.Document,
					USER_ID = (int)loggedinUserId,
					UPDATE_DATE = DateTime.Now,
					MEANING = meaning,
				};

				context.ArtifactSignatures.AddObject(newReqSignature);

				context.SaveChanges();
				//log history
				new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.DocumentSignature, document.AttachmentId, DateTime.UtcNow);

			}
		}

		public DocumentStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				DocumentStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.DocumentStati
								where t.DocumentStatusId == statusId
								select t;

					status = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Handles any Attachment specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplate">The current project template (not used)</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleAttachmentSpecificFilters(int? projectId, int? projectTemplate, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>
		/// Retrieves a single attachment by attachment id
		/// </summary>
		/// <param name="attachmentId">The ID of the attachment to retrieve</param>
		/// <param name="includeTags">Should we get also retrieve the related tags object</param>
		/// <returns>An attachment entity</returns>
		public Attachment RetrieveById(int attachmentId, bool includeTags = true)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Attachment attachment;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.Attachments
									.Include(p => p.ProjectAttachments)
								where a.AttachmentId == attachmentId
								select a;

					attachment = query.FirstOrDefault();

					//See if we have a tags object as well
					if (includeTags && attachment != null)
					{
						var query2 = from a in context.ArtifactTags
									 where a.ArtifactId == attachmentId && a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document
									 select a.Tags;

						attachment.Tags = query2.FirstOrDefault();
					}
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (attachment == null)
				{
					throw new ArtifactNotExistsException("Attachment " + attachmentId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the entity
				return attachment;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single project attachment by attachment id
		/// </summary>
		/// <param name="projectId">The ID of the project</param>
		/// <param name="attachmentId">The ID of the attachment to retrieve</param>
		/// <returns>A project attachment entity</returns>
		/// <param name="includeTags">Should we also retrieve the meta-tags</param>
		/// <remarks>Also includes the attachment record</remarks>
		public ProjectAttachment RetrieveForProjectById(int projectId, int attachmentId, bool includeTags = true)
		{
			const string METHOD_NAME = "RetrieveForProjectById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectAttachment attachment;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectAttachments
									.Include(p => p.Attachment)
								where p.AttachmentId == attachmentId && p.ProjectId == projectId
								select p;

					attachment = query.FirstOrDefault();

					//See if we have a tags object as well
					if (includeTags && attachment != null && attachment.Attachment != null)
					{
						var query2 = from a in context.ArtifactTags
									 where a.ArtifactId == attachmentId && a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document
									 select a.Tags;

						attachment.Attachment.Tags = query2.FirstOrDefault();
					}
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (attachment == null)
				{
					throw new ArtifactNotExistsException("Attachment " + attachmentId + " doesn't exist in project " + projectId + ".");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the entity
				return attachment;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single attachment view by attachment id
		/// </summary>
		/// <param name="attachmentId">The ID of the attachment to retrieve</param>
		/// <returns>An attachment view entity</returns>
		public ProjectAttachmentView RetrieveForProjectById2(int projectId, int attachmentId)
		{
			const string METHOD_NAME = "RetrieveForProjectById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectAttachmentView attachment;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ProjectAttachmentsView
								where a.AttachmentId == attachmentId && a.ProjectId == projectId
								select a;

					attachment = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (attachment == null)
				{
					throw new ArtifactNotExistsException("Attachment " + attachmentId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the entity
				return attachment;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open-status attachments for a given editor.</summary>
		/// <param name="openerId">The ID of the person who is marked as the editor of the document</param>
		/// <param name="projectId">The id of the project, or null for all</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Attachment List</returns>
		/// <remarks>The documents are sorted by last updated date descending. Only displays for active projects</remarks>
		public List<ProjectAttachmentView> RetrieveOpenByOpenerId(int openerId, int? projectId, int numberRows = 500)
		{
			const string METHOD_NAME = "RetrieveOpenByOpenerId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectAttachmentView> attachments;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the attachment records
					var query = from i in context.ProjectAttachmentsView
								where i.EditorId == openerId && i.ProjectIsActive && i.DocumentStatusIsOpenStatus
								select i;

					//Add the project filter if necessary
					if (projectId.HasValue)
					{
						query = query.Where(i => i.ProjectId == projectId.Value);
					}

					//Order by last updated date then id
					query = query.OrderByDescending(i => i.EditedDate).ThenBy(i => i.AttachmentId);

					//Execute the query
					attachments = query.Take(numberRows).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachments;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the attachment record itself
		/// </summary>
		/// <param name="updHist">Whether to record history or not</param>
		/// <param name="projectAttachment">The project attachment record to be updated</param>
		/// <param name="attachment">The attachment record to be updated</param>
		/// <remarks>
		/// This method performs the necessary updates.
		/// The projectAttachment record needs to also contain the Attachment object as its property
		/// </remarks>
		/// <param name="userId">The user making the change</param>
		/// <param name="isRollback">Whether the update is a rollback or not. Default: FALSE</param>
		/// <param name="rollbackId">Whether or not to update history. Default: TRUE</param>
		public void Update(ProjectAttachment projectAttachment, int userId, long? rollbackId = null, bool updHist = true)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					if (projectAttachment != null)
					{
						projectAttachment.StartTracking();
					}
					//Start tracking changes
					if (projectAttachment.Attachment != null)
					{
						projectAttachment.Attachment.StartTracking();
						//Update the edited and concurrency dates
						projectAttachment.Attachment.EditedDate = DateTime.UtcNow;
						projectAttachment.Attachment.ConcurrencyDate = DateTime.UtcNow;

						//Store the project id for use by the history manager
						projectAttachment.Attachment.ProjectId = projectAttachment.ProjectId;
					}

					if (projectAttachment != null)
					{
						context.ProjectAttachments.ApplyChanges(projectAttachment);
					}

					bool tagsChanged = false;
					ArtifactTag attachmentTags = null;
					if (projectAttachment != null && projectAttachment.Attachment != null)
					{
						//Update the tags table as well
						var query = from t in context.ArtifactTags
									where t.ArtifactId == projectAttachment.AttachmentId && t.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document
									select t;

						attachmentTags = query.FirstOrDefault();
						if (string.IsNullOrEmpty(projectAttachment.Attachment.Tags))
						{
							//Delete the tags if they exist
							if (attachmentTags != null)
							{
								context.ArtifactTags.DeleteObject(attachmentTags);
							}
						}
						else
						{
							//Add/update the tags
							if (attachmentTags == null)
							{
								attachmentTags = new ArtifactTag();
								attachmentTags.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document;
								attachmentTags.ProjectId = projectAttachment.ProjectId;
								attachmentTags.ArtifactId = projectAttachment.AttachmentId;
								attachmentTags.Tags = projectAttachment.Attachment.Tags;
								context.ArtifactTags.AddObject(attachmentTags);
								tagsChanged = true;
							}
							else if (attachmentTags.Tags != projectAttachment.Attachment.Tags)
							{
								attachmentTags.StartTracking();
								attachmentTags.Tags = projectAttachment.Attachment.Tags;
								tagsChanged = true;
							}
						}
					}

					//Save the changes, recording history as well
					context.SaveChanges(userId, true, false, rollbackId);
					

						//Now we need to get the list of projects to update if the tags changed
						if (projectAttachment.Attachment != null && tagsChanged)
						{
							List<int> projectIds = new List<int>();
							List<ProjectAttachment> projectAttachments = RetrieveProjectsByAttachmentId(projectAttachment.AttachmentId);
							foreach (ProjectAttachment projectAttachmentRow in projectAttachments)
							{
								if (!projectIds.Contains(projectAttachmentRow.ProjectId))
								{
									projectIds.Add(projectAttachmentRow.ProjectId);
								}
							}

							//Need to update the tag frequency table in case tags were changed
							foreach (int projectId in projectIds)
							{
								UpdateTagFrequency(projectId, userId, rollbackId);
							}
						}
					
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Opens up a binary filestream to the attachment itself
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to open</param>
		/// <returns>A filestream to the attachment</returns>
		/// <remarks>Will get the currently active version</remarks>
		public FileStream OpenById(int attachmentId)
		{
			const string METHOD_NAME = CLASS_NAME + "OpenById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//First get the folder used to store attachments
			string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;
			Console.Write("AttachmentFolder:"+attachmentPath);
			Logger.LogEnteringEvent("AttachmentFolder:" + attachmentPath);
			//Now get the active version record
			AttachmentVersionView attachmentVersion = RetrieveActiveVersion(attachmentId);
			if (attachmentVersion == null)
			{
				throw new AttachmentDefaultVersionException("Can't locate a default version for attachment " + attachmentId);
			}

			//Now add on the attachment filename itself (based on the attachment version id key)
			attachmentPath += @"\" + attachmentVersion.AttachmentVersionId.ToString() + ".dat";
			Console.Write("File full path:"+attachmentPath);
			Logger.LogEnteringEvent("File full path:" + attachmentPath);
			//Finally open up the filesteam
			FileStream fileStream = File.OpenRead(attachmentPath);

			Logger.LogExitingEvent(METHOD_NAME);
			return fileStream;
		}

		/// <summary>
		/// Retrieves all the artifacts linked to a particular attachment in a project
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>List of artifact attachments</returns>
		protected List<ArtifactAttachment> RetrieveArtifactsByAttachmentId(int attachmentId, int projectId)
		{
			const string METHOD_NAME = "RetrieveArtifactsByAttachmentId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactAttachment> artifactAttachments;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactAttachments
								where a.AttachmentId == attachmentId && a.ProjectId == projectId
								orderby a.ArtifactTypeId, a.ArtifactId
								select a;

					//Actually execute the query and return the list
					artifactAttachments = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactAttachments;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Document Status Functions

		/// <summary>Gets the default (i.e. initial) document status for all newly created documents</summary>
		/// <param name="projectTemplateId">The current Project Template ID</param>
		/// <returns>The document status</returns>
		/// <remarks>Returns null is there is no default document status for the project template (shouldn't really happen)</remarks>
		public DocumentStatus DocumentStatusRetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "DocumentStatusRetrieveDefault";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DocumentStatus documentStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.DocumentStati
								where i.IsDefault && i.ProjectTemplateId == projectTemplateId
								select i;

					documentStatus = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return documentStatus;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the DocumentStatus by the given ID.</summary>
		/// <param name="documentStatusId">The status ID to retrieve.</param>
		/// <param name="workflowId">Set the value if you only want workflow fields/custom properties for a specific workflow</param>
		/// <returns>The DocumentStatus, or null if not found.</returns>
		/// <remarks>Will return deleted items.</remarks>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		public DocumentStatus DocumentStatus_RetrieveById(int documentStatusId, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "DocumentStatus_RetrieveById(int,[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			DocumentStatus retStatus = null;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				ObjectQuery<DocumentStatus> documentStati = context.DocumentStati;
				if (includeWorkflowFields)
				{
					documentStati = documentStati.Include("WorkflowFields").Include("WorkflowCustomProperties");
				}
				var query = from ts in documentStati
							where ts.DocumentStatusId == documentStatusId
							select ts;

				try
				{
					retStatus = query.First();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Retrieving Document Status ID #" + documentStatusId + ":");
					retStatus = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		public List<DocumentStatus> RetrieveStatuses(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_staticReleaseStatuses == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.DocumentStati
									where r.IsActive && r.ProjectTemplateId == projectTemplateId
									orderby r.Position, r.DocumentStatusId
									select r;

						_staticReleaseStatuses = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _staticReleaseStatuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the open status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> DocumentStatus_RetrieveOpenIds(int projectTemplateId)
		{
			const string METHOD_NAME = "DocumentStatus_RetrieveOpenIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.DocumentStati
								where i.ProjectTemplateId == projectTemplateId && i.IsOpenStatus && i.IsActive
								orderby i.DocumentStatusId
								select i.DocumentStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the closed status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> DocumentStatus_RetrieveClosedIds(int projectTemplateId)
		{
			const string METHOD_NAME = "DocumentStatus_RetrieveClosedIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.DocumentStati
								where i.ProjectTemplateId == projectTemplateId && !i.IsOpenStatus && i.IsActive
								orderby i.DocumentStatusId
								select i.DocumentStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the all the document statuses in the project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="activeOnly">Do we only want active statuses</param>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		/// <returns>The DocumentStatuses</returns>
		public List<DocumentStatus> DocumentStatus_Retrieve(int projectTemplateId, bool activeOnly = true, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "DocumentStatus_Retrieve(int,[bool],[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<DocumentStatus> retStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<DocumentStatus> documentStati = context.DocumentStati;
					if (includeWorkflowFields)
					{
						documentStati = documentStati
							.Include(d => d.WorkflowFields)
							.Include(d => d.WorkflowCustomProperties);
					}
					var query = from i in documentStati
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.DocumentStatusId, i.Position
								select i;

					retStatus = query.OrderByDescending(i=>i.DocumentStatusId).ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return retStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Updates an document status in the system.</summary>
		/// <param name="documentStatus">The status to update.</param>
		/// <returns>The updated status.</returns>
		public DocumentStatus DocumentStatus_Update(DocumentStatus documentStatus)
		{
			const string METHOD_NAME = CLASS_NAME + "DocumentStatus_Update(DocumentStatus)";

			Logger.LogEnteringEvent(METHOD_NAME);

			DocumentStatus retStatus = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.DocumentStati.ApplyChanges(documentStatus);
					context.SaveChanges();

					retStatus = documentStatus;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Saving Status");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		/// <summary>Inserts a new document status for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the document status belongs to</param>
		/// <param name="name">The display name of the document status</param>
		/// <param name="active">Whether the document status is active or not</param>
		/// <param name="open">Is this document status considered 'open'</param>
		/// <param name="defaultStatus">Is this the default (initial) status of newly created documents</param>
		/// <param name="position">The position, null = end of the list</param>
		/// <returns>The newly created document status id</returns>
		public int DocumentStatus_Insert(int projectTemplateId, string name, bool open, bool defaultStatus, bool active, int? position = null)
		{
			const string METHOD_NAME = "DocumentStatus_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int documentStatusId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					DocumentStatus documentStatus = new DocumentStatus();
					documentStatus.ProjectTemplateId = projectTemplateId;
					documentStatus.Name = name.MaxLength(20);
					documentStatus.IsDefault = defaultStatus;
					documentStatus.IsOpenStatus = open;
					documentStatus.IsActive = active;

					if (position.HasValue)
					{
						documentStatus.Position = position.Value;
					}
					else
					{
						//Get the last position
						var query = from d in context.DocumentStati
									where d.ProjectTemplateId == projectTemplateId
									orderby d.Position descending
									select d;

						DocumentStatus lastStatus = query.FirstOrDefault();
						if (lastStatus == null)
						{
							documentStatus.Position = 1;
						}
						else
						{
							documentStatus.Position = lastStatus.Position + 1;
						}
					}

					context.DocumentStati.AddObject(documentStatus);
					context.SaveChanges();
					documentStatusId = documentStatus.DocumentStatusId;
				}

				//Now capture the newly created id and return
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return documentStatusId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		#endregion

		#region Project Attachment Type functions

		/// <summary>
		/// Copies all the project attachment types from one project to another
		/// </summary>
		/// <param name="sourceProjectId">The source project</param>
		/// <param name="destProjectId">The destination project</param>
		/// <remarks>
		/// It assumes that there are no types in the destination project and doesn't check for duplicate default flags
		/// </remarks>
		protected internal Dictionary<int, int> CopyTypes(int sourceProjectId, int destProjectId)
		{
			const string METHOD_NAME = "CopyTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the types for the existing project
				List<DocumentType> documentTypes = RetrieveDocumentTypes(sourceProjectId, false);

				//Now add them to the destination project
				Dictionary<int, int> typeMapping = new Dictionary<int, int>();
				foreach (DocumentType documentType in documentTypes)
				{
					//Capture the new type id and add to the mapping
					int typeId = this.InsertDocumentType(destProjectId, documentType.Name, documentType.Description, documentType.IsActive, documentType.IsDefault);
					typeMapping.Add(documentType.DocumentTypeId, typeId);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return typeMapping;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the default (i.e. initial) project attachment status for a particular project template
		/// </summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <returns>The default project attachment status id for the project template</returns>
		public int GetDefaultDocumentStatus(int projectTemplateId)
		{
			const string METHOD_NAME = "GetDefaultDocumentStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int statusId;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record
					var query = from p in context.DocumentStati
								where
									p.ProjectTemplateId == projectTemplateId &&
									p.IsActive &&
									p.IsDefault
								select p;

					//Execute the query
					DocumentStatus defaultStatus = query.FirstOrDefault();
					if (defaultStatus == null)
					{
						throw new ProjectDefaultAttachmentTypeException("Can't locate the default document status for project template PT" + projectTemplateId);
					}
					statusId = defaultStatus.DocumentStatusId;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the default (i.e. initial) project attachment type for a particular project template
		/// </summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <returns>The default project attachment type id for the project template</returns>
		public int GetDefaultDocumentType(int projectTemplateId)
		{
			const string METHOD_NAME = "GetDefaultDocumentType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int typeId;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the record
					var query = from p in context.DocumentTypes
								where
									p.ProjectTemplateId == projectTemplateId &&
									p.IsActive &&
									p.IsDefault
								select p;

					//Execute the query
					DocumentType defaultType = query.FirstOrDefault();
					if (defaultType == null)
					{
						throw new ProjectDefaultAttachmentTypeException("Can't locate the default project attachment type for project template PT" + projectTemplateId);
					}
					typeId = defaultType.DocumentTypeId;
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return typeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of project attachment types for the specified project template
		/// </summary>
		/// <param name="projectTemplateId">The ID of the project template we're interested in</param>
		/// <param name="activeOnly">Do we want just active types or all types</param>
		/// <returns>An attachment type list</returns>
		public List<DocumentType> RetrieveDocumentTypes(int projectTemplateId, bool activeOnly)
		{
			const string METHOD_NAME = "RetrieveDocumentTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new document type list
			List<DocumentType> documentTypes;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the records
					var query = from p in context.DocumentTypes
								where p.ProjectTemplateId == projectTemplateId
								select p;

					if (activeOnly)
					{
						query = query.Where(p => p.IsActive);
					}
					query = query.OrderBy(p => p.DocumentTypeId).ThenBy(p => p.Name);

					//Actually execute the query and return the list
					documentTypes = query.OrderByDescending(i=> i.DocumentTypeId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return documentTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of project attachment types for the specified project
		/// </summary>
		/// <param name="documentTypeId">The ID of the type we're interested in</param>
		/// <returns>An attachment type entity</returns>
		public DocumentType RetrieveDocumentTypeById(int documentTypeId)
		{
			const string METHOD_NAME = "RetrieveDocumentTypeById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new attachment type list
			DocumentType attachmentType;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the records
					var query = from p in context.DocumentTypes
								where p.DocumentTypeId == documentTypeId
								select p;

					//Actually execute the query and return the object
					attachmentType = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the item
				return attachmentType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the list of all project attachment types
		/// </summary>
		/// <param name="documentTypes">The list of project attachment types to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void UpdateDocumentTypes(List<DocumentType> documentTypes)
		{
			const string METHOD_NAME = "UpdateDocumentTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Before we make the update, need to perform some validation checks and throw the appropriate exceptions
				int defaultCount = 0;
				foreach (DocumentType typeRow in documentTypes)
				{
					//Only look at modified rows
					if (typeRow.ChangeTracker.State == ObjectState.Modified)
					{
						//If any of the types are marked as default and inactive, throw an exception
						if (typeRow.IsDefault && !typeRow.IsActive)
						{
							throw new ProjectDefaultAttachmentTypeException("Should not be able to make an inactive type default");
						}
					}

					//Count to see how many default entries we have
					if (typeRow.IsDefault)
					{
						defaultCount++;
					}
				}

				//Make sure we have exactly one default
				if (defaultCount != 1)
				{
					throw new ProjectDefaultAttachmentTypeException("Need to have one default type per project template");
				}

				//Actually perform the updates
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (DocumentType typeRow in documentTypes)
					{
						context.DocumentTypes.ApplyChanges(typeRow);
					}
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
			Logger.Flush();
		}

		/// <summary>
		/// Updates a single project attachment type
		/// </summary>
		/// <param name="documentTypes">The project attachment type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void UpdateDocumentType(DocumentType documentType)
		{
			const string METHOD_NAME = "UpdateDocumentType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int defaultDocumentTypeId = this.GetDefaultDocumentType(documentType.ProjectTemplateId);

				//Before we make the update, need to perform some validation checks and throw the appropriate exceptions
				//If any of the types are marked as default and inactive, throw an exception
				if (documentType.IsDefault)
				{
					if (!documentType.IsActive)
					{
						throw new ProjectDefaultAttachmentTypeException("Should not be able to make an inactive type default");
					}
					//If this type is the default - check if it should be the new default (and therefore the old default should be updated)
					else
					{
						//If there is a new default then we need to update all types to change the default from the old to the new
						if (documentType.DocumentTypeId != defaultDocumentTypeId)
						{
							List<DocumentType> documentTypes = this.RetrieveDocumentTypes(documentType.ProjectTemplateId, false);
							foreach (DocumentType typeRow in documentTypes)
							{
								if (typeRow.DocumentTypeId == documentType.DocumentTypeId)
								{
									typeRow.StartTracking();
									typeRow.Name = documentType.Name;
									typeRow.IsDefault = documentType.IsDefault;
									typeRow.IsActive = documentType.IsActive;
									typeRow.Description = documentType.Description;
									typeRow.DocumentWorkflowId = typeRow.DocumentWorkflowId;
								}
								else if (typeRow.DocumentTypeId == defaultDocumentTypeId)
								{
									typeRow.StartTracking();
									typeRow.IsDefault = false;
								}
							}
							this.UpdateDocumentTypes(documentTypes);
						}
						else
						{
							//Actually perform the update
							using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
							{
								context.DocumentTypes.ApplyChanges(documentType);
								context.SaveChanges();
							}
						}
					}
				}
				//Cannot make the current default no longer be the default
				else if (defaultDocumentTypeId == documentType.DocumentTypeId)
				{
					throw new ProjectDefaultAttachmentTypeException("Need to have at least one default type per project");
				}
				else
				{
					//Actually perform the update
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.DocumentTypes.ApplyChanges(documentType);
						context.SaveChanges();
					}
				}

			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Inserts a new document type
		/// </summary>
		/// <param name="projectTemplateId">The project template that it belongs to</param>
		/// <param name="name">The display name of the type</param>
		/// <param name="description">The description of the type</param>
		/// <param name="active">Is it an active type</param>
		/// <param name="isDefault">Is it the default type for the project</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <returns>The newly created project attachment type id</returns>
		public int InsertDocumentType(int projectTemplateId, string name, string description, bool active, bool isDefault, int? workflowId = null)
		{
			const string METHOD_NAME = "InsertDocumentType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).DocumentWorkflowId;
				}
				//Cannot add a type that is not active but is meant to be the default
				if (!active && isDefault)
				{
					throw new ProjectDefaultAttachmentTypeException("Should not be able to make an inactive type default");
				}

				int typeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new type - if this is the first type in the template it has to be the default and active
					List<DocumentType> documentTypes = this.RetrieveDocumentTypes(projectTemplateId, true);
					bool firstTypeInTemplate = documentTypes.Count == 0;

					DocumentType documentType = new DocumentType();
					documentType.ProjectTemplateId = projectTemplateId;
					documentType.Name = name;
					documentType.Description = description;
					documentType.IsActive = firstTypeInTemplate ? true : active;
					documentType.IsDefault = firstTypeInTemplate ? true : isDefault;
					documentType.DocumentWorkflowId = workflowId.Value;

					//Actually perform the insert into the table
					context.DocumentTypes.AddObject(documentType);
					context.SaveChanges();

					//Now capture the newly created id and return
					typeId = documentType.DocumentTypeId;

					//If attempting to insert a new type and it be the default, need to update all types - if this is not the first type in the template
					if (isDefault && !firstTypeInTemplate)
					{
						documentType.IsDefault = isDefault;
						this.UpdateDocumentType(documentType);
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return typeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}



		#endregion

		#region Internal Utility Functions

		/// <summary>
		/// Deletes all the folders and attachment types used in a project
		/// as well as all the actual attachments used in the project
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <remarks>
		/// Typically used when deleting a project
		/// </remarks>
		protected internal void DeleteAllProjectAttachmentInfo(int projectId, int userId)
		{
			const string METHOD_NAME = "DeleteAllProjectAttachmentInfo";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First need to remove all the attachments in the project (in batches of 250)
				List<ProjectAttachmentView> projectAttachments = new List<ProjectAttachmentView>();
				const int PAGE_SIZE = 250;
				int count = CountForProject(projectId, null, null, 0);
				for (int startRow = 1; startRow <= count; startRow += PAGE_SIZE)
				{
					projectAttachments.AddRange(RetrieveForProject(projectId, null, null, true, startRow, PAGE_SIZE, null, 0));
				}
				foreach (ProjectAttachmentView projectAttachment in projectAttachments)
				{
					this.Delete(projectId, projectAttachment.AttachmentId, userId);
				}

				//Now delete all the types and folders
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Attachment_DeleteTypesFoldersForProject(projectId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Tag Frequencies

		/// <summary>
		/// Retrieves the project tags and their frequency
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <returns>The tags and frequencies</returns>
		public List<ProjectTagFrequency> RetrieveTagFrequency(int projectId)
		{
			const string METHOD_NAME = "RetrieveTagFrequency";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectTagFrequency> tagFrequencies;

				//Create select command for retrieving the attachment folder records
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectTagFrequencies
								where p.ProjectId == projectId
								orderby p.Name
								select p;

					tagFrequencies = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tagFrequencies;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the table of tag frequencies for the current project
		/// </summary>
		/// <param name="projectId">The specified project</param>
		protected internal void UpdateTagFrequency(int projectId, int? userId = null, long? rollbackId = null, bool updHist = true)
		{
			const string METHOD_NAME = "UpdateTagFrequency";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Dictionary<string, int> tagFrequency = new Dictionary<string, int>();

				//We need to retrieve all the tags in the current project and then count them
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the existing list of tags
					var query1 = from p in context.ProjectTagFrequencies
								 where p.ProjectId == projectId
								 orderby p.Name
								 select p;
					List<ProjectTagFrequency> projectTags = query1.ToList();

					//Now get the current attachment list
					var query2 = from p in context.ProjectAttachmentsView
								 where p.ProjectId == projectId && p.Tags != null && p.Tags.Length > 0
								 orderby p.AttachmentId
								 select p;

					List<ProjectAttachmentView> attachments = query2.ToList();

					foreach (ProjectAttachmentView attachment in attachments)
					{
						if (!string.IsNullOrWhiteSpace(attachment.Tags))
						{
							//The tags should be separated by commas
							string tagString = attachment.Tags;
							string[] tags = tagString.Split(',');
							foreach (string tag in tags)
							{
								string tagName = tag.Trim().ToLowerInvariant();
								if (tagFrequency.ContainsKey(tagName))
								{
									int newFrequency = tagFrequency[tagName] + 1;
									tagFrequency[tagName] = newFrequency;
									Logger.LogTraceEvent("Debug1", "Updating tag: " + tagName + "=" + newFrequency);
								}
								else
								{
									Logger.LogTraceEvent("Debug1", "Adding tag: " + tagName);
									tagFrequency.Add(tagName, 1);
								}
							}
						}
					}

					//First see if we have any new tags to add
					foreach (KeyValuePair<string, int> kvp in tagFrequency)
					{
						if (!projectTags.Any(p => p.ProjectId == projectId && p.Name == kvp.Key))
						{
							ProjectTagFrequency newTag = new ProjectTagFrequency();
							newTag.Name = kvp.Key;
							newTag.ProjectId = projectId;
							newTag.Frequency = kvp.Value;
							context.ProjectTagFrequencies.AddObject(newTag);
						}
					}

					//Now we need to update the existing tags
					List<ProjectTagFrequency> tagsToDelete = new List<ProjectTagFrequency>();
					foreach (ProjectTagFrequency projectTag in projectTags)
					{
						//See if the row needs to be deleted
						if (!tagFrequency.ContainsKey(projectTag.Name))
						{
							tagsToDelete.Add(projectTag);
						}
						else if (tagFrequency[projectTag.Name] != projectTag.Frequency)
						{
							projectTag.StartTracking();
							projectTag.Frequency = tagFrequency[projectTag.Name];
						}
					}
					foreach (ProjectTagFrequency tagToDelete in tagsToDelete)
					{
						context.ProjectTagFrequencies.DeleteObject(tagToDelete);
					}

					//Finally persist the changes
					context.SaveChanges(userId, true, false, rollbackId,projectId, (int)Artifact.ArtifactTypeEnum.Document);
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

		#endregion

		#region Attachment Folders

		/// <summary>
		/// Copies all the project attachment folders from one project to another
		/// </summary>
		/// <param name="sourceProjectId">The source project</param>
		/// <param name="destProjectId">The destination project</param>
		/// <param name="folderMapping">The mapping of folder ids between the two projects</param>
		/// <remarks>
		/// It assumes that there are no folders in the destination project and doesn't check for duplicate roots
		/// </remarks>
		protected internal Dictionary<int, int> CopyFolders(int sourceProjectId, int destProjectId)
		{
			const string METHOD_NAME = "CopyFolders";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the folders for the existing project (needs to be sorted by hierarchy)
				List<ProjectAttachmentFolderHierarchy> folderHierarchy = RetrieveFoldersByProjectId(sourceProjectId);

				//Now add them to the destination project (need to convert parent ids over)
				Dictionary<int, int> folderMapping = new Dictionary<int, int>();
				foreach (ProjectAttachmentFolderHierarchy folderRow in folderHierarchy)
				{
					//Need to handle the parent folder id
					if (folderRow.ParentProjectAttachmentFolderId.HasValue)
					{
						//Insert the root item and add its ID to the mapping dictionary
						if (folderMapping.ContainsKey(folderRow.ParentProjectAttachmentFolderId.Value))
						{
							int parentFolderId = folderMapping[folderRow.ParentProjectAttachmentFolderId.Value];
							int folderId = this.InsertProjectAttachmentFolder(destProjectId, folderRow.Name, parentFolderId);
							folderMapping.Add(folderRow.ProjectAttachmentFolderId, folderId);
						}
					}
					else
					{
						//Insert the root item and add its ID to the mapping dictionary
						int folderId = this.InsertProjectAttachmentFolder(destProjectId, folderRow.Name, null);
						folderMapping.Add(folderRow.ProjectAttachmentFolderId, folderId);
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folderMapping;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Delete a project attachment folder. This will delete all associated attachments
		/// and any child folders
		/// </summary>
		/// <param name="folderId">The id of the folder to delete</param>
		public void DeleteFolder(int projectId, int folderId, int? userId = null)
		{
			const string METHOD_NAME = "DeleteFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First need to make sure that we're not deleting the root folder
				int rootFolderId = this.GetDefaultProjectFolder(projectId);
				if (rootFolderId == folderId)
				{
					throw new ProjectDefaultAttachmentFolderException("Cannot delete the root folder in a project");
				}

				//See if we have any child folders that we need to delete
				List<ProjectAttachmentFolder> childFolders = RetrieveFoldersByParentId(projectId, folderId);
				if (childFolders.Count > 0)
				{
					//Recursively call this function for all child folders
					List<int> foldersToDelete = new List<int>();
					foreach (ProjectAttachmentFolder childFolder in childFolders)
					{
						foldersToDelete.Add(childFolder.ProjectAttachmentFolderId);
					}
					foreach (int folderToDeleteId in foldersToDelete)
					{
						this.DeleteFolder(projectId, folderToDeleteId, userId);
					}
				}

				//Next need to remove all the attachments associated with the folder
				List<ProjectAttachmentView> projectAttachments = RetrieveForProject(projectId, folderId, null, true, 1, int.MaxValue, null, 0);
				List<int> attachmentsToDelete = new List<int>();
				foreach (ProjectAttachmentView attachmentRow in projectAttachments)
				{
					//Delete the attachment from the project / folder
					attachmentsToDelete.Add(attachmentRow.AttachmentId);
				}
				foreach (int attachmentId in attachmentsToDelete)
				{
					this.Delete(projectId, attachmentId, userId);
				}

				//Finally delete the folder itself
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectAttachmentFolders
								where p.ProjectAttachmentFolderId == folderId
								select p;

					ProjectAttachmentFolder folder = query.FirstOrDefault();
					if (folder != null)
					{
						context.ProjectAttachmentFolders.DeleteObject(folder);
						context.SaveChanges();
					}
				}

				//Next refresh the folder hierarchy cache
				ProjectAttachmentFolder_RefreshHierarchy(projectId);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new project attachment folder
		/// </summary>
		/// <param name="projectId">The project that it belongs to</param>
		/// <param name="name">The display name of the folder</param>
		/// <param name="parentFolderId">The parent folder id (null if root)</param>
		/// <returns>The newly created project attachment type id</returns>
		/// <remarks>Doesn't check to make sure that we already have a root or not</remarks>
		public int InsertProjectAttachmentFolder(int projectId, string name, int? parentFolderId)
		{
			const string METHOD_NAME = "InsertProjectAttachmentFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int folderId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ProjectAttachmentFolder folder = new ProjectAttachmentFolder();
					folder.ProjectId = projectId;
					folder.Name = name;
					folder.ParentProjectAttachmentFolderId = parentFolderId;
					context.ProjectAttachmentFolders.AddObject(folder);
					context.SaveChanges();

					//Get the new generated ID
					folderId = folder.ProjectAttachmentFolderId;
				}

				//Next refresh the folder hierarchy cache
				ProjectAttachmentFolder_RefreshHierarchy(projectId);

				//Return the new folder id
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folderId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the folder for a project
		/// </summary>
		/// <param name="folder">The folder to update</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void UpdateFolder(ProjectAttachmentFolder folder)
		{
			const string METHOD_NAME = "UpdateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Before we make the update, need to perform some validation checks and throw the appropriate exceptions

				if (folder.ChangeTracker.OriginalValues.ContainsKey("ParentProjectAttachmentFolderId"))
				{
					//First make sure that we're not setting the parent id of the root folder
					if (folder.ChangeTracker.OriginalValues["ParentProjectAttachmentFolderId"] == null && folder.ParentProjectAttachmentFolderId.HasValue)
					{
						throw new ProjectDefaultAttachmentFolderException("You can't make a root folder a child of another folder");
					}

					//Next make sure that all non-root items are not being made root
					if (folder.ChangeTracker.OriginalValues["ParentProjectAttachmentFolderId"] != null && !folder.ParentProjectAttachmentFolderId.HasValue)
					{
						throw new ProjectDefaultAttachmentFolderException("You can't make a non-root folder the project root folder");
					}
				}

				//Actually perform the update
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.ProjectAttachmentFolders.ApplyChanges(folder);
					context.SaveChanges();
				}

				//Next refresh the folder hierarchy cache
				ProjectAttachmentFolder_RefreshHierarchy(folder.ProjectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all parents of the specified folder in hierarchy order
		/// </summary>
		/// <param name="folderId">The id of the document folder</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="includeSelf">Should we include the specified folder itself</param>
		/// <returns>List of folders</returns>
		public List<ProjectAttachmentFolderHierarchyView> RetrieveParentFolders(int projectId, int folderId, bool includeSelf = false)
		{
			const string METHOD_NAME = "RetrieveParentFolders";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectAttachmentFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					folders = context.Attachment_RetrieveParentFolders(projectId, folderId, includeSelf).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single project folder record
		/// </summary>
		/// <param name="folderId">The folder we're interested in</param>
		/// <returns>An project folder attachment dataset</returns>
		public ProjectAttachmentFolder RetrieveFolderById(int folderId)
		{
			const string METHOD_NAME = "RetrieveFolderById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectAttachmentFolder folder;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the attachment folder record
					var query = from p in context.ProjectAttachmentFolders
								where p.ProjectAttachmentFolderId == folderId
								select p;

					folder = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (folder == null)
				{
					throw new ArtifactNotExistsException("Document Folder" + folderId + " doesn't exist in the system.");
				}

				//Return the folder
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary> 
		/// Checks if a folder exists by its id in the specified project 
		/// </summary> 
		/// <param name="projectId">The folder id</param> 
		/// <param name="folderId">The folder id</param> 
		/// <returns>bool of true if the folder exists in the project</returns> 
		public bool ProjectAttachmentFolder_Exists(int projectId, int folderId)
		{
			const string METHOD_NAME = "ProjectAttachmentFolder_Exists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectAttachmentFolder folder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.ProjectAttachmentFolders
								where f.ProjectAttachmentFolderId == folderId && f.ProjectId == projectId
								select f;
					folder = query.FirstOrDefault();
				}

				//Make sure data was returned 
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder == null ? false : true;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "Looking for ProjectAttachmentFolder");
				throw;
			}
		}

		/// <summary>
		/// Refreshes the attachment folder hierachy in a project after folders are changed
		/// </summary>
		/// <param name="projectId">The ID of the current project</param>
		public void ProjectAttachmentFolder_RefreshHierarchy(int projectId)
		{
			const string METHOD_NAME = "ProjectAttachmentFolder_RefreshHierarchy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a longer timeout for this as it's run infrequently to speed up retrieves
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.Attachment_RefreshFolderHierarchy(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}

		}

		/// <summary>
		/// Retrieves a single project folder record by its project and name (finds the first match if more than one)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="name">The name of the folder we're interested in</param>
		/// <returns>An project folder attachment or NULL if no match</returns>
		public ProjectAttachmentFolder RetrieveFolderByName(int projectId, string name)
		{
			const string METHOD_NAME = "RetrieveFolderByName";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectAttachmentFolder folder;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the attachment folder record
					var query = from p in context.ProjectAttachmentFolders
								where p.Name == name && p.ProjectId == projectId
								select p;

					folder = query.FirstOrDefault();
				}

				//Return the folder
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the document folders in a project, with the indents calculated to enable easy display
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>An project folder attachment list</returns>
		public List<ProjectAttachmentFolderHierarchy> RetrieveFoldersByProjectId(int projectId)
		{
			const string METHOD_NAME = "RetrieveFoldersByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new list
			List<ProjectAttachmentFolderHierarchy> folders;

			try
			{
				//Get the list of folders
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.ProjectAttachmentFolderHierarchies
								where f.ProjectId == projectId
								orderby f.IndentLevel, f.ProjectAttachmentFolderId
								select f;

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return folders;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the default (i.e. initial) document folder for a particular project
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>The default folder id for the project</returns>
		public int GetDefaultProjectFolder(int projectId)
		{
			const string METHOD_NAME = "GetDefaultProjectFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int folderId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the attachment folder records
					var query = from p in context.ProjectAttachmentFolders
								where p.ProjectId == projectId
								&& !p.ParentProjectAttachmentFolderId.HasValue
								select p;

					//Actually execute the query and return the data
					ProjectAttachmentFolder defaultFolder = query.FirstOrDefault();
					if (defaultFolder == null)
					{
						throw new ProjectDefaultAttachmentFolderException("Can't locate the default project folder for project " + projectId);
					}
					folderId = defaultFolder.ProjectAttachmentFolderId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folderId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a set of project attachment folders that have a specific parent id (only direct children included)
		/// </summary>
		/// <param name="parentFolderId">The ID of the parent folder (null for the root)</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A project folder list</returns>
		public List<ProjectAttachmentFolder> RetrieveFoldersByParentId(int projectId, int? parentFolderId)
		{
			const string METHOD_NAME = "RetrieveFoldersByParentId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new list
			List<ProjectAttachmentFolder> folders;

			try
			{
				//Create select command for retrieving the attachment folder records
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectAttachmentFolders
								where p.ProjectId == projectId
								select p;

					if (parentFolderId.HasValue)
					{
						query = query.Where(p => p.ParentProjectAttachmentFolderId == parentFolderId.Value);
					}
					else
					{
						query = query.Where(p => !p.ParentProjectAttachmentFolderId.HasValue);
					}

					//Add on the sort
					query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectAttachmentFolderId);

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return folders;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Attachment Versions

		/// <summary>
		/// Inserts a new file attachment verson into the system and associates it with the provided attachment id
		/// </summary>
		/// <param name="filename">The filename of the attachment</param>
		/// <param name="description">An optional detailed description of the version</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="attachmentId">The attachment id to associate the version record with</param>
		/// <param name="currentVersion">Is this the current version of the attachment</param>
		/// <param name="updateAttachmentRecord">Should we update the attachment record</param>
		/// <returns>The id of the attachment version</returns>
		/// <remarks>This overload is used for file attachments</remarks>
		public int InsertVersion(int projectId, int attachmentId, string filename, string description, int authorId, byte[] binaryData, string version, bool currentVersion)
		{
			return this.InsertVersion(projectId, attachmentId, filename, description, authorId, binaryData, version, currentVersion, true);
		}

		/// <summary>
		/// Inserts a new file attachment verson into the system and associates it with the provided attachment id
		/// </summary>
		/// <param name="filename">The filename of the attachment</param>
		/// <param name="description">An optional detailed description of the version</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="attachmentId">The attachment id to associate the version record with</param>
		/// <param name="currentVersion">Is this the current version of the attachment</param>
		/// <param name="updateAttachmentRecord">Should we update the attachment record</param>
		/// <returns>The id of the attachment version</returns>
		/// <remarks>This overload is used for file attachments</remarks>
		protected int InsertVersion(int projectId, int attachmentId, string filename, string description, int authorId, byte[] binaryData, string version, bool currentVersion, bool updateAttachmentRecord)
		{
			const string METHOD_NAME = "InsertVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int size = 1; // Minimum 1KB

			try
			{
				int attachmentVersionId;

				//Get the file size in KB
				if (binaryData.Length > 1024)
				{
					size = binaryData.Length / 1024;
				}

				//Fill out entity with data for new attachment version
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Make sure the attachment exists
					var query = from a in context.Attachments
								where a.AttachmentId == attachmentId
								select a;
					Attachment attachment = query.FirstOrDefault();

					if (attachment != null)
					{
						AttachmentVersion attachmentVersion = new AttachmentVersion();
						attachmentVersion.AttachmentId = attachmentId;
						attachmentVersion.AuthorId = authorId;
						attachmentVersion.Filename = filename;
						attachmentVersion.Description = description;
						attachmentVersion.UploadDate = DateTime.UtcNow;
						attachmentVersion.Size = size;
						attachmentVersion.IsCurrent = currentVersion;
						attachmentVersion.VersionNumber = version;

						//Save the object
						context.AttachmentVersions.AddObject(attachmentVersion);
						context.SaveChanges();
						attachmentVersionId = attachmentVersion.AttachmentVersionId;

						//Now get the folder used to store attachments
						string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;
						Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME+"Saved Path "+ attachmentPath);
						Console.Write(CLASS_NAME + METHOD_NAME + "Saved Path " + attachmentPath);
						try
						{
							if (!Directory.Exists(attachmentPath))
							{
								Console.Write(attachmentPath);
								Directory.CreateDirectory(attachmentPath);
								//Now add on the attachment filename itself
								attachmentPath += @"\" + attachmentVersionId.ToString() + ".dat";

								//Now we need to save the attachment itself
								FileStream fileStream = new FileStream(attachmentPath, FileMode.Create);

								//Write data to the file stream
								fileStream.Write(binaryData, 0, binaryData.Length);

								//Close file stream
								fileStream.Close();								
							}
							else
							{
								//Now add on the attachment filename itself
								attachmentPath += @"\" + attachmentVersionId.ToString() + ".dat";

								//Now we need to save the attachment itself
								FileStream fileStream = new FileStream(attachmentPath, FileMode.Create);

								//Write data to the file stream
								fileStream.Write(binaryData, 0, binaryData.Length);

								//Close file stream
								fileStream.Close();
							}
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
							throw;
						}

						//If this is the new current version, we need to make make other versions
						//'not current' and update the attachment record itself
						if (currentVersion && updateAttachmentRecord)
						{
							UpdateWithNewCurrentVersion(projectId, attachmentId, attachmentVersion, Attachment.AttachmentTypeEnum.File);
						}

						HistoryManager historyManager = new HistoryManager();
						historyManager.LogCreation(projectId, authorId, DataModel.Artifact.ArtifactTypeEnum.DocumentVersion, attachmentVersionId, DateTime.UtcNow);


						//Record the history of adding a new version
						//new HistoryManager().History_RecordDocumentVersionChange(projectId, attachmentId, filename, authorId, Project.PermissionEnum.Create, version, currentVersion, attachment.Filename, attachment.CurrentVersion);
					}
					else
					{
						throw new ArtifactNotExistsException("Attachment DC" + attachmentId + " does not exist!");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentVersionId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new URL attachment verson into the system and associates it with the provided attachment id
		/// </summary>
		/// <param name="url">The url of the attachment</param>
		/// <param name="description">An optional detailed description of the version</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="attachmentId">The attachment id to associate the version record with</param>
		/// <param name="currentVersion">Is this the current version of the attachment</param>
		/// <returns>The id of the attachment version</returns>
		/// <remarks>This overload is used for URL attachments</remarks>
		public int InsertVersion(int projectId, int attachmentId, string url, string description, int authorId, string version, bool currentVersion)
		{
			return this.InsertVersion(projectId, attachmentId, url, description, authorId, version, currentVersion, true);
		}

		/// <summary>
		/// Inserts a new URL attachment verson into the system and associates it with the provided attachment id
		/// </summary>
		/// <param name="url">The url of the attachment</param>
		/// <param name="description">An optional detailed description of the version</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="version">The name of the initial version of the document (optional)</param>
		/// <param name="attachmentId">The attachment id to associate the version record with</param>
		/// <param name="currentVersion">Is this the current version of the attachment</param>
		/// <param name="updateAttachmentRecord">Should we update the attachment record</param>
		/// <returns>The id of the attachment version</returns>
		/// <remarks>This overload is used for URL attachments</remarks>
		protected int InsertVersion(int projectId, int attachmentId, string url, string description, int authorId, string version, bool currentVersion, bool updateAttachmentRecord = true)
		{
			const string METHOD_NAME = "InsertVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int size = 0; // All URL attachments are displayed as 0KB

			try
			{
				int attachmentVersionId;

				//Fill out entity with data for new attachment version
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Make sure the attachment exists
					var query = from a in context.Attachments
								where a.AttachmentId == attachmentId
								select a;
					Attachment attachment = query.FirstOrDefault();

					if (attachment != null)
					{
						AttachmentVersion attachmentVersion = new AttachmentVersion();
						attachmentVersion.AttachmentId = attachmentId;
						attachmentVersion.AuthorId = authorId;
						attachmentVersion.Filename = url;
						attachmentVersion.Description = description;
						attachmentVersion.UploadDate = DateTime.UtcNow;
						attachmentVersion.Size = size;
						attachmentVersion.IsCurrent = currentVersion;
						attachmentVersion.VersionNumber = version;

						//Save the object
						context.AttachmentVersions.AddObject(attachmentVersion);
						context.SaveChanges();
						attachmentVersionId = attachmentVersion.AttachmentVersionId;

						//If this is the new current version, we need to make make other versions
						//'not current' and update the attachment record itself
						if (currentVersion && updateAttachmentRecord)
						{
							UpdateWithNewCurrentVersion(projectId, attachmentId, attachmentVersion, Attachment.AttachmentTypeEnum.URL);
						}

						//Add a new history entry for the creation
						//HistoryManager historyManager = new HistoryManager();
						//historyManager.LogCreation(projectId, authorId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentVersionId, DateTime.UtcNow);

						//Record the history of adding a new version
						//new HistoryManager().History_RecordDocumentVersionChange(projectId, attachmentId, url, authorId, Project.PermissionEnum.Create, version, currentVersion, attachment.Filename, attachment.CurrentVersion);
					}
					else
					{
						throw new ArtifactNotExistsException("Attachment DC" + attachmentId + " does not exist!");
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentVersionId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Counts the number of versions of a document
		/// </summary>
		/// <param name="attachmentId">The id of the document</param>
		/// <returns>The count</returns>
		public int CountVersions(int attachmentId)
		{
			const string METHOD_NAME = "CountVersions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersionsView
								where a.AttachmentId == attachmentId
								select a;

					count = query.Count();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the versions for an attachment
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <returns>The list of versions for this attachment</returns>
		public List<AttachmentVersionView> RetrieveVersions(int attachmentId)
		{
			const string METHOD_NAME = "RetrieveVersions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<AttachmentVersionView> attachmentVersions;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersionsView
								where a.AttachmentId == attachmentId
								orderby a.UploadDate descending, a.AttachmentVersionId
								select a;

					attachmentVersions = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentVersions;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a string of the max version number for an attachment
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <returns>The list of versions for this attachment</returns>
		public string RetrieveMaxVersionNumber(int attachmentId)
		{
			const string METHOD_NAME = "RetrieveMaxVersionNumber";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AttachmentVersionView attachmentVersion;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersionsView
								where a.AttachmentId == attachmentId
								orderby a.VersionNumber descending, a.AttachmentVersionId
								select a;

					attachmentVersion = query.FirstOrDefault();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentVersion.VersionNumber;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the most recently uploaded version for an attachment
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <returns>The most recently uploaded version for this attachment</returns>
		public AttachmentVersionView RetrieveVersionLastUploaded(int attachmentId)
		{
			const string METHOD_NAME = "RetrieveVersionLastUploaded";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AttachmentVersionView attachmentVersion;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					attachmentVersion = context.AttachmentVersionsView
						.Where(a => a.AttachmentId == attachmentId)
						.OrderByDescending(a => a.UploadDate)
						.ThenBy(a => a.AttachmentVersionId)
						.FirstOrDefault();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachmentVersion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single attachment version record
		/// </summary>
		/// <param name="attachmentVersionId">The attachment version we're interested in</param>
		/// <returns>The attchment version record</returns>
		/// <remarks>Also includes the parent attachment</remarks>
		public AttachmentVersion RetrieveVersionById(int attachmentVersionId)
		{
			const string METHOD_NAME = "RetrieveVersionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the attachment record
				AttachmentVersion attachmentVersion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersions.Include("Attachment")
								where a.AttachmentVersionId == attachmentVersionId
								select a;

					attachmentVersion = query.FirstOrDefault();
				}

				//Throw an exception if we can't find the attachment version
				if (attachmentVersion == null)
				{
					throw new ArtifactNotExistsException("Cannot locate attachment version " + attachmentVersionId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the record
				return attachmentVersion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single attachment version record
		/// </summary>
		/// <param name="attachmentVersionId">The attachment version we're interested in</param>
		/// <returns>The attchment version record</returns>
		public AttachmentVersionView RetrieveVersionById2(int attachmentVersionId)
		{
			const string METHOD_NAME = "RetrieveVersionById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the attachment record
				AttachmentVersionView attachmentVersion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersionsView
								where a.AttachmentVersionId == attachmentVersionId
								select a;

					attachmentVersion = query.FirstOrDefault();
				}

				//Throw an exception if we can't find the attachment version
				if (attachmentVersion == null)
				{
					throw new ArtifactNotExistsException("Cannot locate attachment version " + attachmentVersionId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the record
				return attachmentVersion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a specific version
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The ID of the user doing the delete</param>
		/// <param name="attachmentVersionId">The id of the version to delete</param>
		/// <remarks>You can't delete the current version</remarks>
		public void DeleteVersion(int projectId, int attachmentVersionId, int userId)
		{
			const string METHOD_NAME = "DeleteVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the version in question
					var query = from a in context.AttachmentVersions
								where a.AttachmentVersionId == attachmentVersionId
								select a;

					AttachmentVersion attachmentVersion = query.FirstOrDefault();
					string strName = attachmentVersion.Filename;

					//It has been deleted already so just ignore and fail quietly
					if (attachmentVersion != null)
					{
						//See if this is the current version
						if (attachmentVersion.IsCurrent)
						{
							throw new AttachmentDefaultVersionException("You can't delete the currently active version");
						}

						//Now get the folder used to store attachments
						string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;

						//Now add on the attachment filename itself
						attachmentPath += @"\" + attachmentVersionId.ToString() + ".dat";

						//Now delete the file from the filesystem
						//Exception is not thrown if file doesn't exist so no need for separate code to handle URLs
						File.Delete(attachmentPath);

						//Now delete the entries from the version table itself
						context.AttachmentVersions.DeleteObject(attachmentVersion);
						context.SaveChanges();

						new HistoryManager().LogDeletion(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.DocumentVersion, attachmentVersionId, DateTime.UtcNow, null, null, strName);

						//Record the history of deleting the version
						//new HistoryManager().History_RecordDocumentVersionChange(projectId, attachmentVersion.AttachmentId, attachmentVersion.Filename, userId, Project.PermissionEnum.Delete, attachmentVersion.VersionNumber);
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
		/// Retrieves the currently active version record for an attachment
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <returns>The active version for this attachment</returns>
		public AttachmentVersionView RetrieveActiveVersion(int attachmentId)
		{
			const string METHOD_NAME = "RetrieveActiveVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AttachmentVersionView attachmentVersion;

				//Create select command for retrieving the attachment record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.AttachmentVersionsView
								where a.AttachmentId == attachmentId && a.IsCurrent
								select a;

					attachmentVersion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the data
				return attachmentVersion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Opens up a binary filestream to the attachment version itself
		/// </summary>
		/// <param name="attachmentVersionId">The id of the attachment version to open</param>
		/// <returns>A filestream to the attachment</returns>
		public FileStream OpenByVersionId(int attachmentVersionId)
		{
			const string METHOD_NAME = "OpenByVersionId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First get the folder used to store attachments
			string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;

			//Now add on the attachment filename itself (based on the attachment version id key)
			attachmentPath += @"\" + attachmentVersionId.ToString() + ".dat";
			Console.Write("Full path:" + attachmentPath);
			Logger.LogEnteringEvent("Full path:" + attachmentPath);
			//Finally open up the filesteam
			FileStream fileStream = File.OpenRead(attachmentPath);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return fileStream;
		}

		#endregion

		/// <summary>
		/// Exports all the attachments from one project to another
		/// </summary>
		/// <param name="userId">The userId of the user carrying out the operation</param>
		/// <param name="sourceProjectId">The project we're copying attachment associations from</param>
		/// <param name="destProjectId">The project we're copying attachment associations to</param>
		/// <param name="folderMappings">The dictionary of mapping between folder ids</param>
		/// <param name="typeMappings">The dictionary of mapping between type ids</param>
		/// <param name="statusMappings">The dictionary of mapping between status ids</param>
		/// <param name="attachmentsIdMapping">the mapping between ids</param>
		/// <param name="sameTemplates">Are we using the same or different project templates</param>
		/// <param name="useAttachmentDates">Use the dates of the attachments themselves otherwise use the current date time</param>
		/// <param name="useDefaultStatus">Use the default status for that project, otherwise use the matching status of the attachments themselves</param>
		/// <remarks>
		/// It physically creates a new copy of the attachment (.dat) files
		/// </remarks>
		protected internal void ExportAllProjectAttachments(
			int userId,
			int sourceProjectId,
			int destProjectId,
			bool sameTemplates,
			Dictionary<int, int> folderMappings,
			Dictionary<int, int> typeMappings,
			Dictionary<int, int> statusMappings,
			Dictionary<int, int> attachmentsIdMapping,
			bool useAttachmentDates = false,
			bool useDefaultStatus = false)
		{
			const string METHOD_NAME = CLASS_NAME + "ExportAllProjectAttachments()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//First get a list of all the documents in the source project
				const int PAGE_SIZE = 250;
				int count = CountForProject(sourceProjectId, null, null, 0);

				//Get the default status once - in case we need to use it
				TemplateManager templateManager = new TemplateManager();
				ProjectTemplate projectTemplate = templateManager.RetrieveForProject(destProjectId);
				DocumentStatus defaultStatus = this.DocumentStatusRetrieveDefault(projectTemplate.ProjectTemplateId);

				for (int startRow = 1; startRow <= count; startRow += PAGE_SIZE)
				{
					List<ProjectAttachmentView> sourceProjectAttachments = RetrieveForProject(sourceProjectId, null, null, true, startRow, PAGE_SIZE, null, 0);

					//Now iterate through and add to the destination project (modifying the status and type ids id the templates are different)
					foreach (ProjectAttachmentView sourceProjectAttachment in sourceProjectAttachments)
					{
						//Convert the folder id in all cases (since not templates based)
						if (folderMappings.ContainsKey(sourceProjectAttachment.ProjectAttachmentFolderId))
						{
							int destFolderId = folderMappings[sourceProjectAttachment.ProjectAttachmentFolderId];

							//Now check the type and status
							int? destTypeId = null;
							int? destStatusId = null;
							if (sameTemplates)
							{
								destTypeId = sourceProjectAttachment.DocumentTypeId;
								destStatusId = sourceProjectAttachment.DocumentStatusId;
							}
							else
							{
								if (typeMappings.ContainsKey(sourceProjectAttachment.DocumentTypeId))
								{
									destTypeId = typeMappings[sourceProjectAttachment.DocumentTypeId];
								}
								if (statusMappings.ContainsKey(sourceProjectAttachment.DocumentStatusId))
								{
									destStatusId = statusMappings[sourceProjectAttachment.DocumentStatusId];
								}
							}

							if (useDefaultStatus)
							{
								destStatusId = defaultStatus.DocumentStatusId;
							}

							if (destStatusId.HasValue && destTypeId.HasValue)
							{
								//See if we have already physically copied this attachment id
								if (attachmentsIdMapping != null && attachmentsIdMapping.ContainsKey(sourceProjectAttachment.AttachmentId))
								{
									int destAttachmentId = attachmentsIdMapping[sourceProjectAttachment.AttachmentId];
									this.InsertProjectAssociation(destProjectId, destAttachmentId, destFolderId, destTypeId);
								}
								else
								{
									//See if we have URL or File (ignore source code)
									int destAttachmentId = 0;
									if (sourceProjectAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
									{
										//++ OpenFile() code.
										//Get the filename.
										string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;

										//Now get the active version record
										AttachmentVersionView attachmentVersion = RetrieveActiveVersion(sourceProjectAttachment.AttachmentId);
										if (attachmentVersion == null)
										{
											throw new AttachmentDefaultVersionException("Can't locate a default version for attachment " + sourceProjectAttachment.AttachmentId);
										}

										//Now add on the attachment filename itself (based on the attachment version id key)
										string attachmentFile = Path.Combine(attachmentPath, (attachmentVersion.AttachmentVersionId.ToString() + ".dat"));
										//-- OpenFile() code.

										//Make sure the file exists, first.
										if (File.Exists(attachmentFile))
										{
											int newAttachmentVersionId;
											//++ Insert() code
											using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
											{
												Attachment newAttachment = new Attachment
												{
													AttachmentTypeId = (int)Attachment.AttachmentTypeEnum.File,
													AuthorId = sourceProjectAttachment.AuthorId,
													EditorId = sourceProjectAttachment.AuthorId,
													Filename = sourceProjectAttachment.Filename,
													Description = sourceProjectAttachment.Description,
													UploadDate = useAttachmentDates ? sourceProjectAttachment.UploadDate : DateTime.UtcNow,
													EditedDate = useAttachmentDates ? sourceProjectAttachment.EditedDate : DateTime.UtcNow,
													ConcurrencyDate = useAttachmentDates ? sourceProjectAttachment.ConcurrencyDate : DateTime.UtcNow,
													Size = sourceProjectAttachment.Size,
													CurrentVersion = DEFAULT_VERSION_NUMBER,
													DocumentStatusId = destStatusId.Value
												};

												//Actually save the object
												context.Attachments.AddObject(newAttachment);
												context.SaveChanges();
												destAttachmentId = newAttachment.AttachmentId;

												//Create the Version object.
												//++ InsertVersion() code
												AttachmentVersion newAttachmentVersion = new AttachmentVersion();
												newAttachmentVersion.AttachmentId = destAttachmentId;
												newAttachmentVersion.AuthorId = newAttachment.AuthorId;
												newAttachmentVersion.Filename = newAttachment.Filename;
												newAttachmentVersion.Description = newAttachment.Description;
												newAttachmentVersion.UploadDate = DateTime.UtcNow;
												newAttachmentVersion.Size = newAttachment.Size;
												newAttachmentVersion.IsCurrent = true;
												newAttachmentVersion.VersionNumber = DEFAULT_VERSION_NUMBER;

												//Save the object
												context.AttachmentVersions.AddObject(newAttachmentVersion);
												context.SaveChanges();
												newAttachmentVersionId = newAttachmentVersion.AttachmentVersionId;
												//-- InsertVersion() code

												//Insert any tags object
												if (sourceProjectAttachment.Tags != null && sourceProjectAttachment.Tags.Count() > 0)
												{
													ArtifactTag attachmentTag = new ArtifactTag
													{
														Tags = sourceProjectAttachment.Tags,
														ProjectId = destProjectId,
														ArtifactId = newAttachment.AttachmentId,
														ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document
													};
													context.ArtifactTags.AddObject(attachmentTag);
													context.SaveChanges();
												}

												//++ InsertProjectAssociation()
												if (!destTypeId.HasValue)
												{
													//Get the template associated with the project
													int projectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
													destTypeId = GetDefaultDocumentType(projectTemplateId);
												}

												//make sure it's not ALREADY IN THE PROJECT...
												if (!context.ProjectAttachments.Any(pa => pa.AttachmentId == destAttachmentId && pa.ProjectId == destProjectId))
												{
													ProjectAttachment projectAttachment = new ProjectAttachment
													{
														AttachmentId = destAttachmentId,
														ProjectId = destProjectId,
														DocumentTypeId = destTypeId.Value,
														ProjectAttachmentFolderId = destFolderId
													};
													context.ProjectAttachments.AddObject(projectAttachment);
													context.SaveChanges();
												}
												//-- InsertProjectAssociation()
											}

											//Add a new history entry for the creation
											HistoryManager historyManager = new HistoryManager();
											historyManager.LogCreation(destProjectId, userId, Artifact.ArtifactTypeEnum.Document, destAttachmentId, DateTime.UtcNow);

											//If the destination file exists, delete it.
											string newFileName = Path.Combine(attachmentPath, newAttachmentVersionId.ToString() + ".dat");
											if (File.Exists(newFileName))
												File.Delete(newFileName);
											//Now copy the file.
											File.Copy(attachmentFile, newFileName);
										}
										else
										{
											Logger.LogErrorEvent(METHOD_NAME, "File '" + attachmentPath + "' missing from file system.");
											// Skip doing anything. Move onto the next one.
										}
									}
									else if (sourceProjectAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
									{
										destAttachmentId = this.Insert(
											destProjectId,
											sourceProjectAttachment.Filename,
											sourceProjectAttachment.Description,
											sourceProjectAttachment.AuthorId,
											null,
											Artifact.ArtifactTypeEnum.None,
											null,
											sourceProjectAttachment.Tags,
											destTypeId,
											destFolderId,
											destStatusId
											);
									}

									if (destAttachmentId > 0 && attachmentsIdMapping != null)
									{
										attachmentsIdMapping.Add(sourceProjectAttachment.AttachmentId, destAttachmentId);
									}
								}
							}
						}
					}
				}

				// update the tag cloud
				this.UpdateTagFrequency(destProjectId);
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
		/// Moves all the attachments from one artifact to another
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="sourceArtifactId">The source artfiact id</param>
		/// <param name="sourceArtifactType">The source artifact type</param>
		/// <param name="destArtifactId">The destination artifact id</param>
		/// <param name="destArtifactType">The destination artifact type</param>
		/// <param name="userId">The id of the user making the change (for history)</param>
		/// <remarks>Typically used to move attachments from a placeholder to an incident when the incident is actually submitted</remarks>
		public void Attachment_Move(int userId, int projectId, int sourceArtifactId, Artifact.ArtifactTypeEnum sourceArtifactType, int destArtifactId, Artifact.ArtifactTypeEnum destArtifactType)
		{
			const string METHOD_NAME = "Attachment_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int rowsAffected = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get all of the attachments linked to the source artifact
					var query = from a in context.ArtifactAttachments
								where
									a.ArtifactId == sourceArtifactId &&
									a.ArtifactTypeId == (int)sourceArtifactType &&
									a.ProjectId == projectId
								orderby a.AttachmentId
								select a;

					//First add the new entries
					List<ArtifactAttachment> artifactAttachments = query.ToList();
					List<ArtifactAttachment> entriesToDelete = new List<ArtifactAttachment>();
					foreach (ArtifactAttachment oldArtifactAttachment in artifactAttachments)
					{
						ArtifactAttachment newArtifactAttachment = new ArtifactAttachment();
						newArtifactAttachment.AttachmentId = oldArtifactAttachment.AttachmentId;
						newArtifactAttachment.ArtifactTypeId = (int)destArtifactType;
						newArtifactAttachment.ArtifactId = destArtifactId;
						newArtifactAttachment.ProjectId = oldArtifactAttachment.ProjectId;
						context.ArtifactAttachments.AddObject(newArtifactAttachment);
						entriesToDelete.Add(oldArtifactAttachment);
						rowsAffected++;
					}

					//Next remove the old ones
					foreach (ArtifactAttachment oldArtifactAttachment in entriesToDelete)
					{
						context.ArtifactAttachments.DeleteObject(oldArtifactAttachment);
					}

					//Commit the changes
					context.SaveChanges();
				}

				//Also update the attachment flag on the artifacts
				if (rowsAffected > 0)
				{
					UpdateArtifactFlag(sourceArtifactType, sourceArtifactId, false);
					UpdateArtifactFlag(destArtifactType, destArtifactId, true);
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
		/// Copies attachments from one artifact to another (this overload is used for same type of artifact and same project)
		/// </summary>
		/// <param name="sourceArtifactType">The type of artifact being copied from</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying from</param>
		/// <param name="artifactType">The type of artifact being copying attachments between</param>
		/// <param name="projectId">The id of the project we're copying within</param>
		public void Copy(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int sourceArtifactId, int destArtifactId)
		{
			//Call the more general overload
			this.Copy(projectId, artifactType, sourceArtifactId, artifactType, destArtifactId);
		}

		/// <summary>
		/// Copies attachments from one artifact to another (same project, potentially different artifact types)
		/// </summary>
		/// <param name="sourceArtifactType">The type of artifact being copied from</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying from</param>
		/// <param name="destArtifactType">The type of artifact being copied to</param>
		/// <param name="destArtifactId">The id of the artifact we're copying to</param>
		/// <param name="projectId">The id of the project we're copying within</param>
		public void Copy(int projectId, DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, int sourceArtifactId, DataModel.Artifact.ArtifactTypeEnum destArtifactType, int destArtifactId)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to get the list of attachments belonging to the source artifact
				List<ProjectAttachmentView> sourceAttachments = RetrieveByArtifactId(projectId, sourceArtifactId, sourceArtifactType, null, true, 1, int.MaxValue, null, 0);

				//Next we need to get the list of attachments belonging to the dest artifact (if any)
				List<ProjectAttachmentView> destAttachments = RetrieveByArtifactId(projectId, destArtifactId, destArtifactType, null, true, 1, int.MaxValue, null, 0);

				//Now we need to add a link between these attachments and the destination artifact
				bool attachmentAdded = false;
				foreach (ProjectAttachmentView sourceAttachment in sourceAttachments)
				{
					//Make sure we don't already have a link to this attachment for the dest artifact
					int attachmentId = sourceAttachment.AttachmentId;

					if (!destAttachments.Any(a => a.AttachmentId == attachmentId))
					{
						InsertArtifactAssociation(projectId, attachmentId, destArtifactId, destArtifactType);
						attachmentAdded = true;
					}
				}

				//Finally change the artifact's attachment flag if it's needed
				if (attachmentAdded)
				{
					UpdateArtifactFlag(destArtifactType, destArtifactId, true);
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
		/// Exports a single attachment (by its id) to another project.
		/// </summary>
		/// <param name="destProjectId">The id of the project to copy to</param>
		/// <param name="sourceAttachmentId">The id of the attachment to copy</param>
		public int Export(int sourceAttachmentId, int destProjectId)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to get the specified attachment
				Attachment sourceAttachment = RetrieveById(sourceAttachmentId, true);

				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceAttachment.ProjectAttachments[0].ProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//See if we have URL or File (ignore source code)
				int destAttachmentId = -1;
				if (sourceAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
				{
					//Get the binary data
					using (FileStream fileStream = this.OpenById(sourceAttachmentId))
					{
						byte[] attachmentBytes = new byte[fileStream.Length];
						fileStream.Read(attachmentBytes, 0, (int)fileStream.Length);
						fileStream.Close();

						destAttachmentId = this.Insert(
							destProjectId,
							sourceAttachment.Filename,
							sourceAttachment.Description,
							sourceAttachment.AuthorId,
							attachmentBytes,
							null,
							Artifact.ArtifactTypeEnum.None,
							null,
							sourceAttachment.Tags,
							(templatesSame) ? (int?)sourceAttachment.ProjectAttachments[0].DocumentTypeId : null,
							null,
							(templatesSame) ? (int?)sourceAttachment.DocumentStatusId : null
							);
					}
				}
				if (sourceAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
				{
					destAttachmentId = this.Insert(
						destProjectId,
						sourceAttachment.Filename,
						sourceAttachment.Description,
						sourceAttachment.AuthorId,
						null,
						Artifact.ArtifactTypeEnum.None,
						null,
						sourceAttachment.Tags,
						(templatesSame) ? (int?)sourceAttachment.ProjectAttachments[0].DocumentTypeId : null,
						null,
						(templatesSame) ? (int?)sourceAttachment.DocumentStatusId : null
						);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return destAttachmentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Exports attachments from one artifact to another in another project (same artifact type)
		/// </summary>
		/// <param name="sourceArtifactType">The type of artifact being copied from</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying from</param>
		/// <param name="artifactType">The type of artifact being copying attachments between</param>
		/// <param name="destProjectId">The id of the destination project</param>
		/// <param name="sourceProjectId">The id of the source project</param>
		public void Export(int sourceProjectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int sourceArtifactId, int destProjectId, int destArtifactId, Dictionary<int, int> attachmentsIdMapping = null)
		{
			//Call the more general overload
			this.Export(sourceProjectId, artifactType, sourceArtifactId, destProjectId, artifactType, destArtifactId, attachmentsIdMapping);
		}

		/// <summary>
		/// Exports attachments from one artifact to another in different projects (physically copies)
		/// </summary>
		/// <param name="sourceArtifactType">The type of artifact being copied from</param>
		/// <param name="sourceArtifactId">The id of the artifact we're copying from</param>
		/// <param name="destArtifactType">The type of artifact being copied to</param>
		/// <param name="destArtifactId">The id of the artifact we're copying to</param>
		/// <param name="destProjectId">The id of the destination project</param>
		/// <param name="sourceProjectId">The id of the source project</param>
		/// <remarks>
		/// It physically copies the attachment and creates a new .DAT file
		/// </remarks>
		public void Export(int sourceProjectId, DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, int sourceArtifactId, int destProjectId, DataModel.Artifact.ArtifactTypeEnum destArtifactType, int destArtifactId, Dictionary<int, int> attachmentsIdMapping = null)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//First we need to get the list of attachments belonging to the source project/artifact
				List<ProjectAttachmentView> sourceAttachments = RetrieveByArtifactId(sourceProjectId, sourceArtifactId, sourceArtifactType, null, true, 1, int.MaxValue, null, 0);

				//Now we need to add a link between these attachments and the destination artifact
				foreach (ProjectAttachmentView sourceAttachment in sourceAttachments)
				{
					//Make sure we don't already have a link to this attachment for the dest artifact
					int sourceAttachmentId = sourceAttachment.AttachmentId;

					//If we have already exported this attachment to the dest project (physically copied)
					//use that ID instead of creating a new attachment
					int? destAttachmentId = null;
					if (attachmentsIdMapping != null && attachmentsIdMapping.ContainsKey(sourceAttachmentId))
					{
						destAttachmentId = attachmentsIdMapping[sourceAttachmentId];
						InsertArtifactAssociation(destProjectId, destAttachmentId.Value, destArtifactId, destArtifactType);
					}
					else
					{
						//See if we have URL or File (ignore source code)
						if (sourceAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
						{
							AttachmentVersionView attachmentVersion = RetrieveActiveVersion(sourceAttachment.AttachmentId);
							if (attachmentVersion != null)
							{
								string attachmentFile = Path.Combine(ConfigurationSettings.Default.General_AttachmentFolder, (attachmentVersion.AttachmentVersionId.ToString() + ".dat"));

								//Make sure the file exists, first.
								if (File.Exists(attachmentFile))
								{

									//Get the binary data
									using (FileStream fileStream = this.OpenById(sourceAttachmentId))
									{
										byte[] attachmentBytes = new byte[fileStream.Length];
										fileStream.Read(attachmentBytes, 0, (int)fileStream.Length);
										fileStream.Close();

										destAttachmentId = this.Insert(
											destProjectId,
											sourceAttachment.Filename,
											sourceAttachment.Description,
											sourceAttachment.AuthorId,
											attachmentBytes,
											destArtifactId,
											destArtifactType,
											null,
											sourceAttachment.Tags,
											(templatesSame) ? (int?)sourceAttachment.DocumentTypeId : null,
											null,
											(templatesSame) ? (int?)sourceAttachment.DocumentStatusId : null
											);
									}
								}
							}
						}
						if (sourceAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
						{
							destAttachmentId = this.Insert(
								destProjectId,
								sourceAttachment.Filename,
								sourceAttachment.Description,
								sourceAttachment.AuthorId,
								destArtifactId,
								destArtifactType,
								null,
								sourceAttachment.Tags,
								(templatesSame) ? (int?)sourceAttachment.DocumentTypeId : null,
								null,
								(templatesSame) ? (int?)sourceAttachment.DocumentStatusId : null
								);
						}

						//If added, add to the mapping
						if (destAttachmentId.HasValue && attachmentsIdMapping != null)
						{
							attachmentsIdMapping.Add(sourceAttachmentId, destAttachmentId.Value);
						}
					}
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
		/// Deletes all the attachment associations for a specific artifact
		/// </summary>
		/// <param name="artifactId">The ID of the artifact to delete</param>
		/// <param name="artifactType">The type of artifact being deleted</param>
		protected internal void DeleteByArtifactId(int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "DeleteByArtifactId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//Since we're deleting the entire artifact, no need to update its attachment flag
				context.Attachment_RemoveFromArtifact((int)artifactType, artifactId, null, null);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Deletes an attachment from an artifact (not from the project/system)
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to delete</param>
		/// <param name="artifactId">The ID of the artifact to delete</param>
		/// <param name="artifactType">The type of artifact being deleted</param>
		/// <param name="projectId">The id of the current project</param>
		/// <remarks>
		/// Only removes the association with the artifact, it will still be part of the project
		/// and still in the system
		/// The public method always updates the flags
		/// </remarks>
		public void Delete(int projectId, int attachmentId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, int? userId = null,bool isattachemtdelete=false)
		{
			this.Delete(projectId, attachmentId, artifactId, artifactType, true, userId, isattachemtdelete);
		}

		/// <summary>Marks a test run set as being deleted.</summary>
		/// <param name="testRunId">The test run set ID.</param>
		/// <param name="userId">The user performing the delete.</param>
		public void MarkAttachmentAsDeleted(int projectId, int attachmentId, int userId)
		{
			const string METHOD_NAME = "MarkTestRunAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the automation host to see that it exists
				bool deletePerformed = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.Attachments
								where t.AttachmentId == attachmentId && !t.IS_DELETED
								select t;

					Attachment attachment = query.FirstOrDefault();
					if (attachment != null)
					{
						//Mark as deleted
						attachment.StartTracking();
						attachment.IS_DELETED = true;
						context.SaveChanges();
						deletePerformed = true;
					}
				}

				if (deletePerformed)
				{
					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}


		/// <summary>
		/// Deletes an attachment from an artifact (not from the project/system)
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="attachmentId">The id of the attachment to delete</param>
		/// <param name="artifactId">The ID of the artifact to delete</param>
		/// <param name="artifactType">The type of artifact being deleted</param>
		/// <param name="updateFlags">Should we update the flags on the artifact</param>
		protected void Delete(int projectId, int attachmentId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, bool updateFlags, int? userId = null,bool isattachemtdelete=false)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to delete the association between the artifact and the attachment
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Attachment attachment = RetrieveById(attachmentId);
					context.Attachment_RemoveFromArtifact((int)artifactType, artifactId, attachmentId, projectId);
					if (isattachemtdelete)
					{
						new HistoryManager().LogDeletion(projectId, (int)userId, artifactType, artifactId, DateTime.UtcNow, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId);
					}
					else
					{
						//Log the purge.
						new HistoryManager().LogPurge(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow, attachment.Filename);
					}
					
				}

				//Now we need to update the AttachmentYn flag on the artifact if appropriate
				if (updateFlags)
				{
					List<ProjectAttachmentView> projectAttachments = RetrieveByArtifactId(projectId, artifactId, artifactType, null, true, 1, int.MaxValue, null, 0);
					if (projectAttachments.Count == 0)
					{
						//If we have no attachments left then set the flag to false
						UpdateArtifactFlag(artifactType, artifactId, false);
					}
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
		/// Deletes an attachment from a project and potentially from the system
		/// </summary>
		/// <param name="attachmentId">The id of the attachment to delete</param>
		/// <param name="projectId">The ID of the project we're using</param>
		/// <remarks>
		/// If no other projects are using the attachment then it will be physically deleted
		/// This also deletes all versions of the attachment
		/// </remarks>
		public void Delete(int projectId, int attachmentId, int? userId = null)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to get the list of projects that this attachment is used in (before the delete)
				List<ProjectAttachment> projectAttachments = RetrieveProjectsByAttachmentId(attachmentId);

				string strName = new AttachmentManager().RetrieveById(attachmentId).Filename;
				
				//If we have only one link and it matches the one being marked for delete
				//then this delete will actually delete the file itself. If no links, then delete (clean-up case)
				bool deleteFile = false;
				if (projectAttachments.Count == 0)
				{
					deleteFile = true;
				}
				if (projectAttachments.Count == 1)
				{
					if (projectAttachments[0].ProjectId == projectId)
					{
						//We have a match so delete the physical file
						deleteFile = true;
					}
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Now we need to update the flag on any linked artifacts, so find a list of all artifacts linked to this attachment/project
					List<ArtifactAttachment> artifactAttachments = RetrieveArtifactsByAttachmentId(attachmentId, projectId);
					if (artifactAttachments.Count > 0)
					{
						//Now we need to remove the reference to the attachment for all the remaining artifacts                    
						if (deleteFile)
						{
							context.Attachment_RemoveFromArtifact(null, null, attachmentId, null);
						}
						else
						{
							context.Attachment_RemoveFromArtifact(null, null, attachmentId, projectId);
						}

						//Now we need to update the AttachmentYn flag on the artifact if appropriate
						foreach (ArtifactAttachment artifactAttachment in artifactAttachments)
						{
							List<ProjectAttachmentView> projectArtifactAttachments = RetrieveByArtifactId(projectId, artifactAttachment.ArtifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactAttachment.ArtifactTypeId, null, true, 1, int.MaxValue, null, 0);
							if (projectArtifactAttachments.Count == 0)
							{
								//If we have no attachments left then set the flag to false
								UpdateArtifactFlag((DataModel.Artifact.ArtifactTypeEnum)artifactAttachment.ArtifactTypeId, artifactAttachment.ArtifactId, false);
							}
						}
					}

					if (deleteFile)
					{
						//We need to delete the attachment including all its versions completely from the system

						//First we need to delete all the versions (this will remove the file from the filesystem if necessary)
						DeleteAllVersions(attachmentId);

						//We need to remove all the comments from this document.
						List<DocumentDiscussion> comments = context.DocumentDiscussions.Where(d => d.ArtifactId == attachmentId).ToList();
						if (comments.Count > 0)
						{
							DiscussionManager dMgr = new DiscussionManager();
							foreach (DocumentDiscussion comment in comments)
								// force delete all the comments otherwise it can throw an exception (in the case where a comment is permanent because it is from a digital signature)
								dMgr.DeleteDiscussionId(comment.DiscussionId, Artifact.ArtifactTypeEnum.Document, true, userId, projectId);
						}

						//Delete the attachment from the system (all projects)
						context.Attachment_Delete(attachmentId, null);
					}
					else
					{
						//We just delete the association to the project
						context.Attachment_Delete(attachmentId, projectId);
					}
				}

				new HistoryManager().LogPurge(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow, strName);
				//new HistoryManager().LogDeletion(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.Document, attachmentId, DateTime.UtcNow, null, null, strName);

				//Need to update the tag frequency table in case tags were changed
				foreach (ProjectAttachment projectAttachment in projectAttachments)
				{
					UpdateTagFrequency(projectAttachment.ProjectId);
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

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>List of soft-deleted document</returns>
		public List<Attachment> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Attachment> attachments;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.Attachments
								where t.IS_DELETED
								orderby t.AttachmentId
								select t;

					attachments = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return attachments;
			}
			catch (Exception ex)
			{
				//Do not rethrow, just return an empty list
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				return new List<Attachment>();
			}
		}

		/// <summary>
		/// Deletes all the version records of an attachment
		/// </summary>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <remarks>If this is a file attachment, it will physically delete the files</remarks>
		protected void DeleteAllVersions(int attachmentId)
		{
			const string METHOD_NAME = "DeleteAllVersions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to get a list of all the versions that exist for this attachment
					var query = from a in context.AttachmentVersions
								where a.AttachmentId == attachmentId
								orderby a.AttachmentId
								select a;

					List<AttachmentVersion> attachmentVersions = query.ToList();
					foreach (AttachmentVersion attachmentVersion in attachmentVersions)
					{
						//Now get the folder used to store attachments
						string attachmentPath = ConfigurationSettings.Default.General_AttachmentFolder;

						//Now add on the attachment filename itself
						attachmentPath += @"\" + attachmentVersion.AttachmentVersionId.ToString() + ".dat";

						//Now delete the file from the filesystem
						//Exception is not thrown if file doesn't exist so no need for separate code to handle URLs
						try
						{
							File.Delete(attachmentPath);
						}
						catch (System.Exception ex)
						{
							string ExMsg = "Error deleting attachment: '" + attachmentPath + "'\n" + ex.Message;
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ExMsg);
						}

						context.AttachmentVersions.DeleteObject(attachmentVersion);
					}

					//Now delete the entries from the version table
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
		/// Makes a specific version of an attachment the current one
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="attachmentId">The attachment we're interested in</param>
		/// <param name="attachmentVersionId">The version to make current</param>
		/// <param name="userId">The id of the user making the change</param>
		public void SetCurrentVersion(int projectId, int attachmentId, int attachmentVersionId, int userId)
		{
			const string METHOD_NAME = "SetCurrentVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the current version record and update its current flag
					var query = from a in context.AttachmentVersions
											.Include(a => a.Attachment)
								where a.AttachmentVersionId == attachmentVersionId
								select a;

					AttachmentVersion attachmentVersion = query.FirstOrDefault();
					if (attachmentVersion != null)
					{
						Attachment.AttachmentTypeEnum attachmentType = (attachmentVersion.Size == 0) ? Attachment.AttachmentTypeEnum.URL : Attachment.AttachmentTypeEnum.File;
						if (!attachmentVersion.IsCurrent)
						{
							//Make this version current
							attachmentVersion.StartTracking();
							attachmentVersion.IsCurrent = true;

							//Now persist the change
							context.SaveChanges();

							//Now update the other versions to be not-current and update the attachment record itself
							UpdateWithNewCurrentVersion(projectId, attachmentId, attachmentVersion, attachmentType);

							//HistoryManager historyManager = new HistoryManager();
							//historyManager.LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.DocumentVersion, attachmentVersionId, DateTime.UtcNow);


							//Record the history of adding a new version
							//new HistoryManager().History_RecordDocumentVersionChange(projectId, attachmentId, attachmentVersion.Filename, userId, Project.PermissionEnum.Modify, attachmentVersion.VersionNumber, true, attachmentVersion.Attachment.Filename, attachmentVersion.Attachment.CurrentVersion);
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the attachment record to account for a new version that is to be the new current one.
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <param name="attachmentVersion">The new version row</param>
		/// <param name="attachmentType">What type of attachment is the new version</param>
		protected void UpdateWithNewCurrentVersion(int projectId, int attachmentId, AttachmentVersion attachmentVersion, Attachment.AttachmentTypeEnum attachmentType)
		{
			const string METHOD_NAME = "UpdateWithNewCurrentVersion";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to first unset any existing current versions
					var query1 = from a in context.AttachmentVersions
								 where
									a.AttachmentId == attachmentId &&
									a.AttachmentVersionId != attachmentVersion.AttachmentVersionId &&
									a.IsCurrent
								 orderby a.AttachmentVersionId
								 select a;

					List<AttachmentVersion> currentVersions = query1.ToList();
					foreach (AttachmentVersion currentVersion in currentVersions)
					{
						currentVersion.StartTracking();
						currentVersion.IsCurrent = false;
					}

					//Now make the passed in one current
					var query2 = from a in context.Attachments
								 where a.AttachmentId == attachmentId
								 select a;

					Attachment attachment = query2.FirstOrDefault();
					if (attachment != null)
					{
						attachment.StartTracking();

						//Update the various fields
						attachment.Filename = attachmentVersion.Filename;
						if (!string.IsNullOrWhiteSpace(attachmentVersion.Description))
						{
							attachment.Description = attachmentVersion.Description;
						}
						attachment.EditorId = attachmentVersion.AuthorId;
						attachment.EditedDate = attachmentVersion.UploadDate;
						attachment.CurrentVersion = attachmentVersion.VersionNumber;
						attachment.AttachmentTypeId = (int)attachmentType;
						attachment.Size = attachmentVersion.Size;
					}


					//Save the changes, capture the new version for notification
					context.SaveChanges();

					//Fire notification of the change, force the change
					attachment.StartTracking();
					string currentVersionNew = attachment.CurrentVersion;
					attachment.ChangeTracker.RecordOriginalValue("CurrentVersion", "");
					attachment.CurrentVersion = currentVersionNew;
					attachment.ProjectId = projectId;

					new NotificationManager().SendNotificationForArtifact(attachment, null, null);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Updates the artifact flag for a particular artifact
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="attachments">The true/false value of the flag</param>
		protected void UpdateArtifactFlag(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, bool attachments)
		{
			const string METHOD_NAME = "UpdateArtifactFlag";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Call the stored procedure to update the flag
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Attachment_UpdateArtifactFlag((int)artifactType, artifactId, attachments);
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
		/// Finds a list of all the projects that an attachment belongs to
		/// </summary>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <returns>The list of project attachment records</returns>
		public List<ProjectAttachment> RetrieveProjectsByAttachmentId(int attachmentId)
		{
			const string METHOD_NAME = "RetrieveProjectsByAttachmentId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectAttachment> projectAttachments;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from p in context.ProjectAttachments
								where p.AttachmentId == attachmentId
								select p;

					projectAttachments = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectAttachments;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}

	/// <summary>
	/// This exception is thrown if the project doesn't have a default folder (shouldn't happen)
	/// </summary>
	public class ProjectDefaultAttachmentFolderException : ApplicationException
	{
		public ProjectDefaultAttachmentFolderException()
		{
		}
		public ProjectDefaultAttachmentFolderException(string message)
			: base(message)
		{
		}
		public ProjectDefaultAttachmentFolderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown the if project doesn't have a default attachment type (shouldn't happen)
	/// </summary>
	public class ProjectDefaultAttachmentTypeException : ApplicationException
	{
		public ProjectDefaultAttachmentTypeException()
		{
		}
		public ProjectDefaultAttachmentTypeException(string message)
			: base(message)
		{
		}
		public ProjectDefaultAttachmentTypeException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown the if the system can't access the default version of an attachment
	/// </summary>
	public class AttachmentDefaultVersionException : ApplicationException
	{
		public AttachmentDefaultVersionException()
		{
		}
		public AttachmentDefaultVersionException(string message)
			: base(message)
		{
		}
		public AttachmentDefaultVersionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
