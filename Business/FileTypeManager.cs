using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// Manages the storage and retrieval of the different document file types
	/// </summary>
	public class FileTypeManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.FileTypeManager::";

		protected static List<Filetype> globalFileTypes = null;

		/// <summary>Returns a filetype object that has the requested file information.</summary>
		/// <param name="extension">The extension to return.</param>
		/// <returns>Returns a filetype object of the found file extension, or the default one if none are found. NULL if there's an error or none defined.</returns>
		public Filetype GetFileTypeInfo(string extension)
		{
			//Lazy-load the cached list
			if (globalFileTypes == null)
			{
				RefreshFiletypes();
			}

			Filetype matchedFiletype = null;

			try
			{
				if (!string.IsNullOrWhiteSpace(extension))
				{
					//Strip the leading '.' if necessary..
					extension = extension.Trim();
					if (extension.StartsWith("."))
					{
						extension = extension.Trim(new char[] { '.' });
					}

					//Find the item that matches:
					matchedFiletype = globalFileTypes.FirstOrDefault(f => f.FileExtension.ToLower() == extension.ToLowerInvariant());

					if (matchedFiletype == null && globalFileTypes.Count > 0)
					{
						//Get the default filetype.
						matchedFiletype = globalFileTypes[0];
					}
				}
				else if (globalFileTypes.Count > 0)
				{
					//Get the default filetype.
					matchedFiletype = globalFileTypes[0];
				}
			}
			catch
			{
				//If there was an error, return the default one.
				if (globalFileTypes.Count > 0)
				{
					//Get the default filetype.
					matchedFiletype = globalFileTypes[0];
				}
			}

			return matchedFiletype;
		}

		/// <summary>Returns a filetype object that has the requested extension id.</summary>
		/// <param name="filetypeId">The extension ID to return.</param>
		/// <returns>Returns a filetype object of the found file extension, or the default one if none are found. NULL if there's an error or none defined.</returns>
		public Filetype GetFileTypeInfo(int filetypeId)
		{
			//Lazy-load the cached list
			if (globalFileTypes == null)
			{
				RefreshFiletypes();
			}

			Filetype matchedFiletype = null;
			try
			{
				if (filetypeId > 0)
				{
					//Find the item that matches:
					matchedFiletype = globalFileTypes.FirstOrDefault(f => f.FiletypeId == filetypeId);

					if (matchedFiletype == null && globalFileTypes.Count > 0)
					{
						//Get the default filetype.
						matchedFiletype = globalFileTypes[0];
					}
				}
				else if (globalFileTypes.Count > 0)
				{
					//Get the default filetype.
					matchedFiletype = globalFileTypes[0];
				}
			}
			catch
			{
				//If there was an error, return the default one.
				if (globalFileTypes.Count > 0)
				{
					//Get the default filetype.
					matchedFiletype = globalFileTypes[0];
				}
			}

			return matchedFiletype;
		}

		/// <summary>Returns the whole datatable of available filetypes.</summary>
		/// <returns>List of defined FileTypes</returns>
		public List<Filetype> GetFileTypeValues()
		{
			//Lazy-load the cached list
			if (globalFileTypes == null)
			{
				RefreshFiletypes();
			}
			return globalFileTypes;
		}

		/// <summary>Deleted the given fileTypeId from the system.</summary>
		/// <param name="fileTypeId">The ID to remove.</param>
		/// <returns>int specifying the # of items deleted. Should be 1.</returns>
		public int DeleteFileType(int fileTypeId, int? userId = null)
		{
			const string METHOD_NAME = "DeleteFileType()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (fileTypeId < 1)
			{
				return 0;
			}
			else
			{
				try
				{
					int rowsAffected = 0;
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Get the matched item
						var query = from f in context.Filetypes
									where f.FiletypeId == fileTypeId
									select f;

						Filetype filetype = query.FirstOrDefault();
						if (filetype != null)
						{
							context.DeleteObject(filetype);
							rowsAffected = context.SaveChanges();

							Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
							string adminSectionName = "File type Icons";
							var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

							int adminSectionId = adminSection.ADMIN_SECTION_ID;

							//Add a changeset to mark it as deleted.
							new AdminAuditManager().LogDeletion1((int)userId, filetype.FiletypeId, filetype.Description, adminSectionId, "FileTypeIcon Deleted", DateTime.UtcNow, ArtifactTypeEnum.FileTypeIcon, "FiletypeId");
						}
					}

					//Finally refresh the list
					RefreshFiletypes();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return rowsAffected;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
					return 0;
				}
			}
		}

		/// <summary>
		/// Refreshes the cached list of filetypes
		/// </summary>
		public void RefreshFiletypes()
		{
			const string METHOD_NAME = "RefreshFiletypes()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Load our filetypes.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Loading FileType Settings");

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.Filetypes
								orderby f.FileExtension
								select f;
					globalFileTypes = query.ToList();
				}
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Add or Update an existing filetype definition.</summary>
		/// <param name="fileTypeId">The ID to update. 0 to add a new filetype.</param>
		/// <param name="fileExtension">The extension of the filetype.</param>
		/// <param name="fileMime">The mimetype of the file.</param>
		/// <param name="fileIcon">The icon of the file.</param>
		/// <param name="fileDescription">The description of the file.</param>
		public void UpdateAddFileType(int fileTypeId, string fileExtension, string fileMime, string fileIcon, string fileDescription, int? userId = null)
		{
			const string METHOD_NAME = "UpdateAddFileType()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "File Type Icons";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				if (fileTypeId == 0)
				{
					//We're Adding.
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Create the new object and set the values
						Filetype filetype = new Filetype();

						filetype.FileExtension = fileExtension;
						filetype.Mime = fileMime;
						filetype.Icon = fileIcon;
						filetype.Description = fileDescription;

						//Attach to the context and commit the changes
						context.AddObject("Filetypes", filetype);
						context.SaveChanges();

						TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
						long changeSetId = 0;
						using (AuditTrailEntities context2 = new AuditTrailEntities())
						{
							//Create a new changeset.							
							hsChangeSet.ADMIN_USER_ID = (int)userId;
							hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
							hsChangeSet.CHANGE_DATE = DateTime.UtcNow;
							hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Modified;
							hsChangeSet.ACTION_DESCRIPTION = "Inserted File Type Icon";
							hsChangeSet.ARTIFACT_ID = filetype.FiletypeId;

							changeSetId = adminAuditManager.Insert1(hsChangeSet);
						}

						ArtifactField artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)ArtifactTypeEnum.FileTypeIcon, "FiletypeId");

						changeSetId = adminAuditManager.Insert1(hsChangeSet);

						InsertAuditTrailDetailEntry(changeSetId, "FileExtension", (int)userId, fileExtension, "", artifactField1);
					}
				}
				else
				{
					//We're updating.
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//First retrieve the filetype by its id
						var query = from f in context.Filetypes
									where f.FiletypeId == fileTypeId
									select f;

						Filetype filetype = query.FirstOrDefault();

						//Make sure it exists
						if (filetype == null)
						{
							throw new ArtifactNotExistsException("The filetype " + fileTypeId + " does not exist in the system");
						}

						//Update the values and commit
						filetype.StartTracking();
						long changeSetId = 0;
						using (AuditTrailEntities context2 = new AuditTrailEntities())
						{
							//Create a new changeset.
							TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
							hsChangeSet.ADMIN_USER_ID = (int)userId;
							hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
							hsChangeSet.CHANGE_DATE = DateTime.UtcNow;
							hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Modified;
							hsChangeSet.ACTION_DESCRIPTION = "Updated File Type Icon";
							hsChangeSet.ARTIFACT_ID = filetype.FiletypeId;

							changeSetId = adminAuditManager.Insert1(hsChangeSet);
						}

						ArtifactField artifactFieldExe = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)ArtifactTypeEnum.FileTypeIcon, "FileExtension");

						InsertAuditTrailDetailEntry(changeSetId, "FileExtension", (int)userId, fileExtension, filetype.FileExtension, artifactFieldExe);
						filetype.FileExtension = fileExtension;

						ArtifactField artifactFieldMime = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)ArtifactTypeEnum.FileTypeIcon, "Mime");
						InsertAuditTrailDetailEntry(changeSetId, "Mime", (int)userId, fileMime, filetype.Mime, artifactFieldMime);
						filetype.Mime = fileMime;

						ArtifactField artifactFieldIcon = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)ArtifactTypeEnum.FileTypeIcon, "Icon");
						InsertAuditTrailDetailEntry(changeSetId, "Icon", (int)userId, fileIcon, filetype.Icon, artifactFieldIcon);
						filetype.Icon = fileIcon;

						ArtifactField artifactFieldDescription = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)ArtifactTypeEnum.FileTypeIcon, "Description");
						InsertAuditTrailDetailEntry(changeSetId, "Description", (int)userId, fileDescription, filetype.Description, artifactFieldDescription);
						filetype.Description = fileDescription;
						context.SaveChanges();


						//context.AdminSaveChanges(userId, fileTypeId, null, adminSectionId, action, true, true, null);
					}

					//Finally refresh the list
					RefreshFiletypes();
				}
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		private void InsertAuditTrailDetailEntry(long changeSetId, string fieldName, int userId, string newValue, string oldValue, ArtifactField artifactField)
		{
			if (!oldValue.Equals(newValue))
			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				TST_ADMIN_HISTORY_DETAILS_AUDIT detail = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				detail.ADMIN_CHANGESET_ID = changeSetId;
				detail.ADMIN_ARTIFACT_FIELD_ID = artifactField.ArtifactFieldId;
				detail.ADMIN_ARTIFACT_FIELD_NAME = fieldName;
				detail.ADMIN_ARTIFACT_FIELD_CAPTION = fieldName;
				detail.ADMIN_USER_ID = userId;
				detail.ADMIN_PROPERTY_NAME = fieldName;
				detail.NEW_VALUE = newValue;
				detail.OLD_VALUE = oldValue;

				adminAuditManager.DetailInsert1(detail);
			}
		}
	}
}
