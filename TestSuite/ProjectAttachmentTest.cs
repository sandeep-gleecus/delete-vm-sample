using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the project document management functionality of the Attachment business object
	/// </summary>
	/// <remarks>Was separated from the AttachmentTest fixture to make it easier to manage</remarks>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ProjectAttachmentTest
	{
		protected static Business.AttachmentManager attachmentManager;
		protected static UnicodeEncoding unicodeEncoding;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static int projectId;
		protected static int projectId2;
		protected static int projectId3;
		protected static int projectTemplateId;
		protected static int projectTemplateId2;
		protected static int projectTemplateId3;
		protected static int newFolderId1;
		protected static int newFolderId2;
		protected static int newFolderId3;
		protected static int newFolderId4;
		protected static int folderId1;
		protected static int folderId2;
		protected static int folderId3;
		protected static int folderId4;
		protected static int typeId1;
		protected static int typeId2;
		protected static int typeId3;
		protected static int attachmentId1;
		protected static int attachmentId2;
		protected static int attachmentId3;
		protected static int attachmentId4;
		protected static int attachmentId5;
		protected static int attachmentVersionId1;
		protected static int attachmentVersionId2;
		protected static int attachmentVersionId3;
		protected static int customPropertyId1;
		protected static int customPropertyId2;

		private const int PROJECT_ID = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		[TestFixtureSetUp]
		public void Init()
		{
			attachmentManager = new Business.AttachmentManager();
			unicodeEncoding = new UnicodeEncoding();

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any deleted items in the projects
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);

			//We need to delete any artifact history items created during the test run
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());

			//Delete the temporary project and  its template
			if (projectId > 0)
			{
				new ProjectManager().Delete(USER_ID_FRED_BLOGGS, projectId);
			}
			if (projectTemplateId > 0)
			{
				new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
			}
		}

		/// <summary>
		/// Tests that we can create a new project and that it has a default folder and document type
		/// </summary>
		[
		Test,
		SpiraTestCase(422)
		]
		public void _01_CreateNewProject()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Lets create the new project (with new template)
			Business.ProjectManager project = new Business.ProjectManager();
			projectId = project.Insert("New Project", null, "", "", true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Verify that it has an default root folder and document type
			List<ProjectAttachmentFolderHierarchy> projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual(1, projectAttachmentFolders.Count);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.IsTrue(projectAttachmentFolders[0].ParentProjectAttachmentFolderId.IsNull(), "IsParentProjectAttachmentFolderIdNull");
			//Assign it for later user
			folderId1 = projectAttachmentFolders[0].ProjectAttachmentFolderId;

			//Verify that it has an default document type
			List<DocumentType> projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			Assert.AreEqual(1, projectAttachmentTypes.Count);
			Assert.AreEqual("Default", projectAttachmentTypes[0].Name);
			Assert.AreEqual(true, projectAttachmentTypes[0].IsActive);
			Assert.AreEqual(true, projectAttachmentTypes[0].IsDefault);
			//Assign it for later user
			typeId1 = projectAttachmentTypes[0].DocumentTypeId;

			//Now lets add some custom properties so that we can verify that they get retrieved
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			customPropertyId1 = customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Document, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Comments", null, null, null).CustomPropertyId;
			customPropertyId2 = customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Document, (int)CustomProperty.CustomPropertyTypeEnum.Date, 2, "Review Date", null, null, null).CustomPropertyId;
		}

		/// <summary>
		/// Tests that we can add/modify folders and retrieve the tree (deletes tested later)
		/// </summary>
		[
		Test,
		SpiraTestCase(423)
		]
		public void _02_CreateModifyFolders()
		{
			//Lets try creating a couple of subfolders
			folderId2 = attachmentManager.InsertProjectAttachmentFolder(projectId, "Folder 1", folderId1);
			folderId3 = attachmentManager.InsertProjectAttachmentFolder(projectId, "Folder 2", folderId1);

			//Verify that they created successfully and that we have three items
			List<ProjectAttachmentFolderHierarchy> projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual(3, projectAttachmentFolders.Count);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.AreEqual("Folder 1", projectAttachmentFolders[1].Name);
			Assert.AreEqual("Folder 2", projectAttachmentFolders[2].Name);

			//Verify that we can get the name of the folder quickly as well (gets first match)
			ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderByName(projectId, "Folder 1");
			Assert.AreEqual("Folder 1", folder.Name);
			Assert.AreEqual(folderId2, folder.ProjectAttachmentFolderId);

			//Now verify that we can get the same data with a calculate indent level (used for diplaying)
			projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.AreEqual("AAA", projectAttachmentFolders[0].IndentLevel);
			Assert.AreEqual("Folder 1", projectAttachmentFolders[1].Name);
			Assert.AreEqual("AAAAAA", projectAttachmentFolders[1].IndentLevel);
			Assert.AreEqual("Folder 2", projectAttachmentFolders[2].Name);
			Assert.AreEqual("AAAAAB", projectAttachmentFolders[2].IndentLevel);

			//Now lets add a new subfolder under one of the folders and verify that it can display the indents correctly
			folderId4 = attachmentManager.InsertProjectAttachmentFolder(projectId, "Folder 1.1", folderId2);

			//Now verify the modified hierarchy
			projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.AreEqual("AAA", projectAttachmentFolders[0].IndentLevel);
			Assert.AreEqual("Folder 1", projectAttachmentFolders[1].Name);
			Assert.AreEqual("AAAAAA", projectAttachmentFolders[1].IndentLevel);
			Assert.AreEqual("Folder 1.1", projectAttachmentFolders[2].Name);
			Assert.AreEqual("AAAAAAAAA", projectAttachmentFolders[2].IndentLevel);
			Assert.AreEqual("Folder 2", projectAttachmentFolders[3].Name);
			Assert.AreEqual("AAAAAB", projectAttachmentFolders[3].IndentLevel);

			//Now we need to modify the name of a folder 
			ProjectAttachmentFolder projectAttachmentFolder = attachmentManager.RetrieveFolderById(folderId4);
			projectAttachmentFolder.StartTracking();
			projectAttachmentFolder.Name = "Folder 2a";
			attachmentManager.UpdateFolder(projectAttachmentFolder);

			//Now verify the update
			projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.AreEqual("AAA", projectAttachmentFolders[0].IndentLevel);
			Assert.AreEqual("Folder 1", projectAttachmentFolders[1].Name);
			Assert.AreEqual("AAAAAA", projectAttachmentFolders[1].IndentLevel);
			Assert.AreEqual("Folder 2a", projectAttachmentFolders[2].Name);
			Assert.AreEqual("AAAAAAAAA", projectAttachmentFolders[2].IndentLevel);
			Assert.AreEqual("Folder 2", projectAttachmentFolders[3].Name);
			Assert.AreEqual("AAAAAB", projectAttachmentFolders[3].IndentLevel);

			//Now we need to move a subfolder from one parent folder to another
			projectAttachmentFolder = attachmentManager.RetrieveFolderById(folderId4);
			projectAttachmentFolder.StartTracking();
			projectAttachmentFolder.ParentProjectAttachmentFolderId = folderId3;
			attachmentManager.UpdateFolder(projectAttachmentFolder);

			//Now verify the update
			projectAttachmentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
			Assert.AreEqual("Root Folder", projectAttachmentFolders[0].Name);
			Assert.AreEqual("AAA", projectAttachmentFolders[0].IndentLevel);
			Assert.AreEqual("Folder 1", projectAttachmentFolders[1].Name);
			Assert.AreEqual("AAAAAA", projectAttachmentFolders[1].IndentLevel);
			Assert.AreEqual("Folder 2", projectAttachmentFolders[2].Name);
			Assert.AreEqual("AAAAAB", projectAttachmentFolders[2].IndentLevel);
			Assert.AreEqual("Folder 2a", projectAttachmentFolders[3].Name);
			Assert.AreEqual("AAAAABAAA", projectAttachmentFolders[3].IndentLevel);

			//Verify that we can get the parent folders for a folder (including self)
			List<ProjectAttachmentFolderHierarchyView> projectAttachmentFoldersView = attachmentManager.RetrieveParentFolders(projectId, folderId4, true);
			Assert.AreEqual(3, projectAttachmentFoldersView.Count, "attachmentManager.RetrieveParentFolders returned incorrect folder count.");
			Assert.AreEqual("Root Folder", projectAttachmentFoldersView[0].Name, "attachmentManager.RetrieveParentFolders returned incorrect name of a projectAttachment folder.");
			Assert.AreEqual("AAA", projectAttachmentFoldersView[0].IndentLevel, "attachmentManager.RetrieveParentFolders returned incorrect Indent Level.");
			Assert.AreEqual("Folder 2", projectAttachmentFoldersView[1].Name, "attachmentManager.RetrieveParentFolders returned incorrect name of a projectAttachment folder.");
			Assert.AreEqual("AAAAAB", projectAttachmentFoldersView[1].IndentLevel, "attachmentManager.RetrieveParentFolders returned incorrect Indent Level.");
			Assert.AreEqual("Folder 2a", projectAttachmentFoldersView[2].Name, "attachmentManager.RetrieveParentFolders returned incorrect name of a projectAttachment folder.");
			Assert.AreEqual("AAAAABAAA", projectAttachmentFoldersView[2].IndentLevel, "attachmentManager.RetrieveParentFolders returned incorrect Indent Level.");

			//Verify that we can get the parent folders for a folder (excluding self)
			projectAttachmentFoldersView = attachmentManager.RetrieveParentFolders(projectId, folderId4, false);
			Assert.AreEqual(2, projectAttachmentFoldersView.Count, "attachmentManager.RetrieveParentFolders returned incorrect folder count.");
			Assert.AreEqual("Root Folder", projectAttachmentFoldersView[0].Name, "attachmentManager.RetrieveParentFolders returned incorrect name of a projectAttachment folder.");
			Assert.AreEqual("AAA", projectAttachmentFoldersView[0].IndentLevel, "attachmentManager.RetrieveParentFolders returned incorrect Indent Level.");
			Assert.AreEqual("Folder 2", projectAttachmentFoldersView[1].Name, "attachmentManager.RetrieveParentFolders returned incorrect name of a projectAttachment folder.");
			Assert.AreEqual("AAAAAB", projectAttachmentFoldersView[1].IndentLevel, "attachmentManager.RetrieveParentFolders returned incorrect Indent Level.");

			//Verify that the root folder can't be made unroot
			bool exceptionThrown = false;
			try
			{
				projectAttachmentFolder = attachmentManager.RetrieveFolderById(folderId1);
				projectAttachmentFolder.StartTracking();
				projectAttachmentFolder.ParentProjectAttachmentFolderId = folderId2;
				attachmentManager.UpdateFolder(projectAttachmentFolder);
			}
			catch (ProjectDefaultAttachmentFolderException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make non-root");

			//Verify that non-root folders can't be made root folders
			exceptionThrown = false;
			try
			{
				projectAttachmentFolder = attachmentManager.RetrieveFolderById(folderId2);
				projectAttachmentFolder.StartTracking();
				projectAttachmentFolder.ParentProjectAttachmentFolderId = null;
				attachmentManager.UpdateFolder(projectAttachmentFolder);
			}
			catch (ProjectDefaultAttachmentFolderException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make root");

			//Check if a folder exists or not
			Assert.AreEqual(true, attachmentManager.ProjectAttachmentFolder_Exists(projectId, folderId2), "Task folder should exist");
			Assert.AreEqual(false, attachmentManager.ProjectAttachmentFolder_Exists(projectId + 1, folderId2), "Task folder does not exist in specified product");
			Assert.AreEqual(false, attachmentManager.ProjectAttachmentFolder_Exists(projectId, folderId2 + 10000000), "Task folder should NOT exist");
		}

		/// <summary>
		/// Tests that we can add/modify project attachment types
		/// </summary>
		[
		Test,
		SpiraTestCase(424)
		]
		public void _03_CreateModifyTypes()
		{
			//Lets try creating a couple of types for the project (one active, one not)
			typeId2 = attachmentManager.InsertDocumentType(projectTemplateId, "Type 1", "", true, false);
			typeId3 = attachmentManager.InsertDocumentType(projectTemplateId, "Type 2", "Type Description", false, false);

			//Verify that they were loaded - show both active and inactive types
			List<DocumentType> projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			Assert.AreEqual(3, projectAttachmentTypes.Count);
			//Default Type
			Assert.AreEqual("Default", projectAttachmentTypes[2].Name);
			Assert.IsTrue(projectAttachmentTypes[2].Description.IsNull());
			Assert.AreEqual(true, projectAttachmentTypes[2].IsActive);
			Assert.AreEqual(true, projectAttachmentTypes[2].IsDefault);
			//First Type
			Assert.AreEqual("Type 1", projectAttachmentTypes[1].Name);
			Assert.IsTrue(projectAttachmentTypes[1].Description.IsNull());
			Assert.AreEqual(true, projectAttachmentTypes[1].IsActive);
			Assert.AreEqual(false, projectAttachmentTypes[1].IsDefault);
			//Second Type
			Assert.AreEqual("Type 2", projectAttachmentTypes[0].Name);
			Assert.AreEqual("Type Description", projectAttachmentTypes[0].Description);
			Assert.AreEqual(false, projectAttachmentTypes[0].IsActive);
			Assert.AreEqual(false, projectAttachmentTypes[0].IsDefault);

			//Verify that we can retrieve a single type by its id
			DocumentType projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(typeId2);
			Assert.AreEqual("Type 1", projectAttachmentType.Name);
			Assert.IsTrue(projectAttachmentType.Description.IsNull());
			Assert.AreEqual(true, projectAttachmentType.IsActive);
			Assert.AreEqual(false, projectAttachmentType.IsDefault);

			//Test that it won't retrieve a non-existant ID
			projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(0);
			Assert.IsNull(projectAttachmentType);

			//Now verify that we can display just active types
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, true);
			Assert.AreEqual(2, projectAttachmentTypes.Count);
			Assert.AreEqual("Default", projectAttachmentTypes[1].Name);
			Assert.IsTrue(projectAttachmentTypes[1].Description.IsNull());
			Assert.AreEqual(true, projectAttachmentTypes[1].IsActive);
			Assert.AreEqual(true, projectAttachmentTypes[1].IsDefault);

			//Now try and edit the name/desc of a type and verify that it changed
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			DocumentType typeRow = projectAttachmentTypes.FirstOrDefault(p => p.DocumentTypeId == typeId3);
			typeRow.StartTracking();
			typeRow.Name = "Type 2a";
			typeRow.Description = "Type 2a is good";
			attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);

			//Verify update
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			Assert.AreEqual("Type 2a", projectAttachmentTypes[0].Name);
			Assert.AreEqual("Type 2a is good", projectAttachmentTypes[0].Description);

			//Verify that we can change the active flag for a type
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			typeRow = projectAttachmentTypes.FirstOrDefault(p => p.DocumentTypeId == typeId3);
			typeRow.StartTracking();
			typeRow.IsActive = true;
			attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);

			//Verify that we now have 3 active types
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, true);
			Assert.AreEqual(3, projectAttachmentTypes.Count);

			//Verify that we can change the default type for the project
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			typeRow = projectAttachmentTypes.FirstOrDefault(p => p.DocumentTypeId == typeId1);
			typeRow.StartTracking();
			typeRow.IsDefault = false;
			typeRow = projectAttachmentTypes.FirstOrDefault(p => p.DocumentTypeId == typeId2);
			typeRow.StartTracking();
			typeRow.IsDefault = true;
			attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);

			//Verify that the default type changed
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			Assert.AreEqual(false, projectAttachmentTypes[0].IsDefault);
			Assert.AreEqual(true, projectAttachmentTypes[1].IsDefault);
			Assert.AreEqual(false, projectAttachmentTypes[2].IsDefault);

			//Verify that an exception is thrown if you try and deactivate the default type
			bool exceptionThrown = false;
			try
			{
				projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
				projectAttachmentTypes[1].StartTracking();
				projectAttachmentTypes[1].IsActive = false;
				attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to deactivate the default type");

			//Verify that an exception is thrown if you try and make an inactive type the default
			exceptionThrown = false;
			try
			{
				projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
				projectAttachmentTypes[0].StartTracking();
				projectAttachmentTypes[0].IsActive = false;
				projectAttachmentTypes[0].IsDefault = true;
				attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make an inactive type default");

			//Verify that an exception is thrown if you try and have two default types
			exceptionThrown = false;
			try
			{
				projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
				projectAttachmentTypes[0].StartTracking();
				projectAttachmentTypes[0].IsDefault = true;
				attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to have multiple default types");

			//Verify that an exception is thrown if you try and have no default types
			exceptionThrown = false;
			try
			{
				projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
				projectAttachmentTypes[1].StartTracking();
				projectAttachmentTypes[1].IsDefault = false;
				attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Need to have at least one default type per project");

			//Verify you can add a new type and set it to be the new default
			int type4 = attachmentManager.InsertDocumentType(projectTemplateId, "Type 3 - Default", "Type Description - new Default", true, true);
			projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(type4);
			Assert.AreEqual("Type 3 - Default", projectAttachmentType.Name, "name of inserted document type matches: Type 3 - Default");
			Assert.AreEqual("Type Description - new Default", projectAttachmentType.Description, "description of inserted document type matches: Type Description - new Default");
			Assert.AreEqual(true, projectAttachmentType.IsActive, "inserted document type should be active");
			Assert.AreEqual(true, projectAttachmentType.IsDefault, "inserted document type should be the default");

			//Verify it is the default
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			Assert.AreEqual(4, projectAttachmentTypes.Count, "should be 4 project attachment types after insertion of a new default");
			int defaultTypeId = attachmentManager.GetDefaultDocumentType(projectTemplateId);
			Assert.AreEqual(type4, defaultTypeId, "new type id should also be the id of the default type");

			//Verify you can't add a new type as the default and it be inactve
			exceptionThrown = false;
			try
			{
				int type5 = attachmentManager.InsertDocumentType(projectTemplateId, "Type 4 - Default but inactive", "Type Description", false, true);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make an inactive type default");

			//Verify you can update a single type
			projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(type4);
			projectAttachmentType.StartTracking();
			projectAttachmentType.Name = "Type 3 - Updated";
			projectAttachmentType.Description = "Type Description - Updated";
			attachmentManager.UpdateDocumentType(projectAttachmentType);
			projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(type4);
			Assert.AreEqual("Type 3 - Updated", projectAttachmentType.Name, "name of updated document type matches: Type 3 - Updated");
			Assert.AreEqual("Type Description - Updated", projectAttachmentType.Description, "description of updated document type matches: Type Description - Updated");

			//Verify that an exception is thrown if you try and have no default types
			exceptionThrown = false;
			try
			{
				projectAttachmentType = attachmentManager.RetrieveDocumentTypeById(type4);
				projectAttachmentType.StartTracking();
				projectAttachmentType.IsDefault = false;
				attachmentManager.UpdateDocumentType(projectAttachmentType);
			}
			catch (ProjectDefaultAttachmentTypeException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Need to have one default type per project template");

			//Verify you can update a single type for it to be the new default (resets to the initial default);
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, false);
			projectAttachmentType = projectAttachmentTypes[1];
			projectAttachmentType.StartTracking();
			projectAttachmentType.IsDefault = true;
			attachmentManager.UpdateDocumentType(projectAttachmentType);
			defaultTypeId = attachmentManager.GetDefaultDocumentType(projectTemplateId);
			Assert.AreEqual(projectAttachmentType.DocumentTypeId, defaultTypeId, "updated type id should also be the id of the default type");
		}

		/// <summary>
		/// Tests that we can upload project documents associated with a folder and type
		/// </summary>
		[
		Test,
		SpiraTestCase(425)
		]
		public void _04_AddProjectDocuments()
		{
			//Lets upload a couple of file attachments to the project to some of the folders (not associate with any artifacts at this time)
			byte[] attachmentData1 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored1");
			byte[] attachmentData2 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored2");
			byte[] attachmentData3 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored3");
			byte[] attachmentData4 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored4");
			byte[] attachmentData5 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored5");
			attachmentId1 = attachmentManager.Insert(projectId, "test_data1.txt", "Test Attachment 1", USER_ID_FRED_BLOGGS, attachmentData1, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, attachment", typeId2, folderId2, null);
			attachmentId2 = attachmentManager.Insert(projectId, "test_data2.txt", "Test Attachment 2", USER_ID_JOE_SMITH, attachmentData2, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, attachment", typeId2, folderId3, null);
			attachmentId3 = attachmentManager.Insert(projectId, "test_data3.htm", "Test Attachment 3", USER_ID_JOE_SMITH, attachmentData3, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, web page", typeId3, folderId4, null);
			attachmentId4 = attachmentManager.Insert(projectId, "test_data4.htm", "Test Attachment 4", USER_ID_FRED_BLOGGS, attachmentData4, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, web page", typeId3, folderId2, null);
			attachmentId5 = attachmentManager.Insert(projectId, "test_data5.txt", "Test Attachment 5", USER_ID_FRED_BLOGGS, attachmentData5, null, Artifact.ArtifactTypeEnum.None, "1.0", "test", typeId3, folderId3, null);

			//Lets add some custom property values to the second attachment
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customPropertyDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, true);
			ArtifactCustomProperty acp = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Document, attachmentId2, customPropertyDefinitions);
			acp.Custom_01 = "This is a nice document".ToDatabaseSerialization();
			acp.Custom_02 = DateTime.Parse("5/8/2014").ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(acp, USER_ID_FRED_BLOGGS);

			//Verify that they uploaded correctly
			//Attachment 1
			ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId1);
			Assert.AreEqual(attachmentId1, projectAttachment.AttachmentId);
			Assert.AreEqual(folderId2, projectAttachment.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 1", projectAttachment.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachment.CurrentVersion);
			Assert.AreEqual("test_data1.txt", projectAttachment.Filename);
			//Attachment 2
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId2);
			Assert.AreEqual(attachmentId2, projectAttachment.AttachmentId);
			Assert.AreEqual(folderId3, projectAttachment.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 1", projectAttachment.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachment.CurrentVersion);
			Assert.AreEqual("test_data2.txt", projectAttachment.Filename);
			Assert.AreEqual("This is a nice document", projectAttachment.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(DateTime.Parse("5/8/2014"), projectAttachment.Custom_02.FromDatabaseSerialization_DateTime());
			//Attachment 3
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId3);
			Assert.AreEqual(attachmentId3, projectAttachment.AttachmentId);
			Assert.AreEqual(folderId4, projectAttachment.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachment.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachment.CurrentVersion);
			Assert.AreEqual("test_data3.htm", projectAttachment.Filename);
			//Attachment 4
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId4);
			Assert.AreEqual(attachmentId4, projectAttachment.AttachmentId);
			Assert.AreEqual(folderId2, projectAttachment.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachment.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachment.CurrentVersion);
			Assert.AreEqual("test_data4.htm", projectAttachment.Filename);
			//Attachment 5
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId5);
			Assert.AreEqual(attachmentId5, projectAttachment.AttachmentId);
			Assert.AreEqual(folderId3, projectAttachment.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachment.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachment.CurrentVersion);
			Assert.AreEqual("test_data5.txt", projectAttachment.Filename);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId1);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored1", retrievedData);
		}

		/// <summary>
		/// Tests that we can retrieve the list of documents for a project folder, with filtering, sorting and pagination
		/// </summary>
		[
		Test,
		SpiraTestCase(426)
		]
		public void _05_RetrieveFilterAndSort()
		{
			//First get all the documents in folder 2 sorted by filename ascending with no filter
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveForProject(projectId, folderId2, "Filename", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, attachments.Count, "Folder2 ASC");
			Assert.AreEqual(attachmentId1, attachments[0].AttachmentId);
			Assert.AreEqual(attachmentId4, attachments[1].AttachmentId);

			//First get all the documents in folder 2 sorted by filename descending with no filter
			attachments = attachmentManager.RetrieveForProject(projectId, folderId2, "Filename", false, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, attachments.Count, "Folder2 DESC");
			Assert.AreEqual(attachmentId4, attachments[0].AttachmentId);
			Assert.AreEqual(attachmentId1, attachments[1].AttachmentId);

			//First get all the documents in all folders sorted by filename ascending filtered by editor = Fred
			Hashtable filters = new Hashtable();
			filters.Add("EditorId", USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveForProject(projectId, null, "Filename", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, attachments.Count);
			Assert.AreEqual(attachmentId5, attachments[0].AttachmentId);
			Assert.AreEqual(attachmentId4, attachments[1].AttachmentId);
			Assert.AreEqual(attachmentId1, attachments[2].AttachmentId);

			//First get all the documents in folder 3 sorted by filename ascending filtered by editor = Fred
			attachments = attachmentManager.RetrieveForProject(projectId, folderId3, "Filename", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual(attachmentId5, attachments[0].AttachmentId);

			//Now get all the documents in folder 2 sorted by type ascending filtered by editor = Joe
			filters.Clear();
			filters.Add("EditorId", USER_ID_JOE_SMITH);
			attachments = attachmentManager.RetrieveForProject(projectId, folderId3, "DocumentTypeName", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual(attachmentId2, attachments[0].AttachmentId);

			//Now get all the documents in folder 2 sorted by type ascending filtered by tags like 'test'
			filters.Clear();
			filters.Add("Tags", "test");
			attachments = attachmentManager.RetrieveForProject(projectId, folderId2, "DocumentTypeName", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, attachments.Count, "Tags");
			Assert.AreEqual(attachmentId1, attachments[0].AttachmentId);
			Assert.AreEqual(attachmentId4, attachments[1].AttachmentId);

			//Now get all the documents in folder 2 sorted by type ascending filtered by editor = Joe or Fred
			filters.Clear();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add(USER_ID_JOE_SMITH);
			multiValueFilter.Values.Add(USER_ID_FRED_BLOGGS);
			filters.Add("EditorId", multiValueFilter);
			attachments = attachmentManager.RetrieveForProject(projectId, folderId2, "DocumentTypeName", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, attachments.Count);

			//Now get all the documents in folder 2 sorted by type ascending editing in range >= 5/1/2006
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("5/1/2006");
			filters.Add("EditedDate", dateRange);
			attachments = attachmentManager.RetrieveForProject(projectId, folderId2, "DocumentTypeName", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, attachments.Count);
		}

		/// <summary>
		/// Tests that we can upload multiple versions of the document, change the active version and delete old versions
		/// </summary>
		[
		Test,
		SpiraTestCase(427)
		]
		public void _06_ManageMultipleVersions()
		{
			//First view the current versions of attachment 1
			List<AttachmentVersionView> attachmentVersions = attachmentManager.RetrieveVersions(attachmentId1);
			Assert.AreEqual(1, attachmentVersions.Count);
			Assert.AreEqual("test_data1.txt", attachmentVersions[0].Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, attachmentVersions[0].AuthorId);
			Assert.AreEqual("Test Attachment 1", attachmentVersions[0].Description);
			Assert.AreEqual(1, attachmentVersions[0].Size);
			Assert.AreEqual("1.0", attachmentVersions[0].VersionNumber);
			Assert.AreEqual(true, attachmentVersions[0].IsCurrent);
			attachmentVersionId1 = attachmentVersions[0].AttachmentVersionId;

			//Verify that we can actually open the attachment version file itself
			FileStream fileStream = attachmentManager.OpenByVersionId(attachmentVersionId1);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored1", retrievedData);

			//Verify the current max version number
			string maxVersionNumber = attachmentManager.RetrieveMaxVersionNumber(attachmentId1);
			Assert.AreEqual("1.0", maxVersionNumber);

			//Now upload a new version of the document - has to be the same type (file vs url) as the existing version
			byte[] attachmentData6 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored6");
			attachmentVersionId2 = attachmentManager.InsertVersion(projectId, attachmentId1, "test_data6.txt", "Revised version of test data", USER_ID_JOE_SMITH, attachmentData6, "2.0", true);

			//Verify that we have two versions
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId1);
			Assert.AreEqual(2, attachmentManager.CountVersions(attachmentId1));
			Assert.AreEqual(2, attachmentVersions.Count);
			//Version 2.0
			Assert.AreEqual("test_data6.txt", attachmentVersions[0].Filename);
			Assert.AreEqual(USER_ID_JOE_SMITH, attachmentVersions[0].AuthorId);
			Assert.AreEqual("Revised version of test data", attachmentVersions[0].Description);
			Assert.AreEqual(1, attachmentVersions[0].Size);
			Assert.AreEqual("2.0", attachmentVersions[0].VersionNumber);
			Assert.AreEqual(true, attachmentVersions[0].IsCurrent);
			//Version 1.0
			Assert.AreEqual("test_data1.txt", attachmentVersions[1].Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, attachmentVersions[1].AuthorId);
			Assert.AreEqual("Test Attachment 1", attachmentVersions[1].Description);
			Assert.AreEqual(1, attachmentVersions[1].Size);
			Assert.AreEqual("1.0", attachmentVersions[1].VersionNumber);
			Assert.AreEqual(false, attachmentVersions[1].IsCurrent);
			//Verify the current max version number
			maxVersionNumber = attachmentManager.RetrieveMaxVersionNumber(attachmentId1);
			Assert.AreEqual("2.0", maxVersionNumber);

			//Verify that the project attachment and attachment records were also updated
			ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId1);
			Assert.AreEqual(attachmentId1, projectAttachment.AttachmentId);
			Assert.AreEqual("test_data6.txt", projectAttachment.Filename);
			Assert.AreEqual(USER_ID_JOE_SMITH, projectAttachment.EditorId);
			Assert.AreEqual("2.0", projectAttachment.CurrentVersion);

			//Now upload another new version of the document - this time not to be the current one
			byte[] attachmentData7 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored7");
			attachmentVersionId3 = attachmentManager.InsertVersion(projectId, attachmentId1, "test_data7.txt", "Older version of the test data", USER_ID_FRED_BLOGGS, attachmentData7, "1.1", false);

			//Verify that we have three versions
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId1);
			Assert.AreEqual(3, attachmentManager.CountVersions(attachmentId1));
			Assert.AreEqual(3, attachmentVersions.Count);
			//Version 1.1
			Assert.AreEqual("test_data7.txt", attachmentVersions[0].Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, attachmentVersions[0].AuthorId);
			Assert.AreEqual("Older version of the test data", attachmentVersions[0].Description);
			Assert.AreEqual(1, attachmentVersions[0].Size);
			Assert.AreEqual("1.1", attachmentVersions[0].VersionNumber);
			Assert.AreEqual(false, attachmentVersions[0].IsCurrent);
			//Version 2.0
			Assert.AreEqual("test_data6.txt", attachmentVersions[1].Filename);
			Assert.AreEqual(USER_ID_JOE_SMITH, attachmentVersions[1].AuthorId);
			Assert.AreEqual("Revised version of test data", attachmentVersions[1].Description);
			Assert.AreEqual(1, attachmentVersions[1].Size);
			Assert.AreEqual("2.0", attachmentVersions[1].VersionNumber);
			Assert.AreEqual(true, attachmentVersions[1].IsCurrent);
			//Version 1.0
			Assert.AreEqual("test_data1.txt", attachmentVersions[2].Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, attachmentVersions[2].AuthorId);
			Assert.AreEqual("Test Attachment 1", attachmentVersions[2].Description);
			Assert.AreEqual(1, attachmentVersions[2].Size);
			Assert.AreEqual("1.0", attachmentVersions[2].VersionNumber);
			Assert.AreEqual(false, attachmentVersions[2].IsCurrent);

			//Verify that we can retrieve only the most recently uploaded version
			AttachmentVersionView attachmentVersion = attachmentManager.RetrieveVersionLastUploaded(attachmentId1);
			Assert.AreEqual("test_data7.txt", attachmentVersions[0].Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, attachmentVersions[0].AuthorId);
			Assert.AreEqual("Older version of the test data", attachmentVersions[0].Description);
			Assert.AreEqual(1, attachmentVersions[0].Size);
			Assert.AreEqual("1.1", attachmentVersions[0].VersionNumber);
			Assert.AreEqual(false, attachmentVersions[0].IsCurrent);

			//Verify that the project attachment and attachment records were not updated in this case
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId1);
			Assert.AreEqual(attachmentId1, projectAttachment.AttachmentId);
			Assert.AreEqual("test_data6.txt", projectAttachment.Filename);
			Assert.AreEqual(USER_ID_JOE_SMITH, projectAttachment.EditorId);
			Assert.AreEqual("2.0", projectAttachment.CurrentVersion);

			//Test that we can make a different version of the document the current one
			attachmentManager.SetCurrentVersion(projectId, attachmentId1, attachmentVersionId3, USER_ID_FRED_BLOGGS);
			//Verify attachment versions
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId1);
			Assert.AreEqual(true, attachmentVersions[0].IsCurrent);
			Assert.AreEqual(false, attachmentVersions[1].IsCurrent);
			Assert.AreEqual(false, attachmentVersions[2].IsCurrent);

			//Verify attachment
			projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId1);
			Assert.AreEqual(attachmentId1, projectAttachment.AttachmentId);
			Assert.AreEqual("test_data7.txt", projectAttachment.Filename);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachment.EditorId);
			Assert.AreEqual("1.1", projectAttachment.CurrentVersion);

			//Test that we can delete one of the existing versions (not the current one)
			attachmentManager.DeleteVersion(projectId, attachmentVersionId2, USER_ID_FRED_BLOGGS);
			//Verify attachment versions
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId1);
			Assert.AreEqual(2, attachmentManager.CountVersions(attachmentId1));
			Assert.AreEqual(2, attachmentVersions.Count);
			Assert.AreEqual("1.1", attachmentVersions[0].VersionNumber);
			Assert.AreEqual("1.0", attachmentVersions[1].VersionNumber);

			//Test that we can't delete the current version
			bool exceptionThrown = false;
			try
			{
				attachmentManager.DeleteVersion(projectId, attachmentVersionId3, USER_ID_FRED_BLOGGS);
			}
			catch (AttachmentDefaultVersionException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to delete the current version");
		}

		/// <summary>
		/// Tests that we can edit the project attachment and attachment records
		/// </summary>
		[
		Test,
		SpiraTestCase(428)
		]
		public void _07_EditProjectAttachments()
		{
			//Lets try updating one of the attachments, first verify its current state
			ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId2);
			Assert.AreEqual(attachmentId2, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId3, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 1", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data2.txt", projectAttachmentView.Filename);
			Assert.AreEqual("Test Attachment 2", projectAttachmentView.Description);
			Assert.AreEqual("test, attachment", projectAttachmentView.Tags);
			Assert.AreEqual(USER_ID_JOE_SMITH, projectAttachmentView.AuthorId);
			Assert.AreEqual(USER_ID_JOE_SMITH, projectAttachmentView.EditorId);

			//Now make some updates
			ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId2);
			Attachment attachment = projectAttachment.Attachment;
			attachment.StartTracking();
			attachment.Filename = "test_data2.dat";
			attachment.Description = "Test Attachment Two";
			attachment.Tags = "test, data";
			attachment.AuthorId = USER_ID_FRED_BLOGGS;
			attachment.EditorId = USER_ID_FRED_BLOGGS;
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify the updates succeeded
			projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId2);
			Assert.AreEqual(attachmentId2, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId3, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 1", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data2.dat", projectAttachmentView.Filename);
			Assert.AreEqual("Test Attachment Two", projectAttachmentView.Description);
			Assert.AreEqual("test, data", projectAttachmentView.Tags);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachmentView.AuthorId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachmentView.EditorId);

			//Now try updating the project attachment information for that item
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId2);
			projectAttachment.StartTracking();
			projectAttachment.ProjectAttachmentFolderId = folderId4;
			projectAttachment.DocumentTypeId = typeId3;
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify the updates succeeded
			projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId2);
			Assert.AreEqual(attachmentId2, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId4, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data2.dat", projectAttachmentView.Filename);
			Assert.AreEqual("Test Attachment Two", projectAttachmentView.Description);
			Assert.AreEqual("test, data", projectAttachmentView.Tags);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachmentView.AuthorId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachmentView.EditorId);
		}

		/// <summary>
		/// Tests that we can view the list of existing artifact associations and make changes
		/// </summary>
		[
		Test,
		SpiraTestCase(429)
		]
		public void _08_ViewEditArtifactAssociations()
		{
			//First view the list of artifact associations for an attachment (should be none)
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			List<ArtifactAttachmentView> associations = artifactLinkManager.RetrieveByAttachmentId(projectId, attachmentId3);
			Assert.AreEqual(0, artifactLinkManager.CountByAttachmentId(projectId, attachmentId3));
			Assert.AreEqual(0, associations.Count);

			//Now lets associate this attachment with a requirement, will need to create one first
			RequirementManager requirement = new RequirementManager();
			int requirementId = requirement.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, 120, USER_ID_FRED_BLOGGS);
			attachmentManager.InsertArtifactAssociation(projectId, attachmentId3, requirementId, Artifact.ArtifactTypeEnum.Requirement);

			//Verify that the association was successfully added
			associations = artifactLinkManager.RetrieveByAttachmentId(projectId, attachmentId3);
			Assert.AreEqual(1, artifactLinkManager.CountByAttachmentId(projectId, attachmentId3));
			Assert.AreEqual(1, associations.Count);
			//Requirement
			Assert.AreEqual(requirementId, associations[0].ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Requirement, associations[0].ArtifactTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, associations[0].CreatorId);

			//Now lets associate this attachment with a test case, will need to create one first
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_JOE_SMITH, projectId, USER_ID_JOE_SMITH, null, "Test Case For Linking", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			attachmentManager.InsertArtifactAssociation(projectId, attachmentId3, testCaseId, Artifact.ArtifactTypeEnum.TestCase);

			//Verify that the association was successfully added
			associations = artifactLinkManager.RetrieveByAttachmentId(projectId, attachmentId3);
			Assert.AreEqual(2, artifactLinkManager.CountByAttachmentId(projectId, attachmentId3));
			Assert.AreEqual(2, associations.Count);
			//Requirement
			Assert.AreEqual(requirementId, associations[0].ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Requirement, associations[0].ArtifactTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, associations[0].CreatorId);
			//Test Case
			Assert.AreEqual(testCaseId, associations[1].ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, associations[1].ArtifactTypeId);
			Assert.AreEqual(USER_ID_JOE_SMITH, associations[1].CreatorId);

			//Now verify that we can delete one of the associations
			attachmentManager.Delete(projectId, attachmentId3, requirementId, Artifact.ArtifactTypeEnum.Requirement, 1);

			//Verify that the association was successfully deleted
			associations = artifactLinkManager.RetrieveByAttachmentId(projectId, attachmentId3);
			Assert.AreEqual(1, artifactLinkManager.CountByAttachmentId(projectId, attachmentId3));
			Assert.AreEqual(1, associations.Count);
			//Test Case
			Assert.AreEqual(testCaseId, associations[0].ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, associations[0].ArtifactTypeId);
			Assert.AreEqual(USER_ID_JOE_SMITH, associations[0].CreatorId);

			//Finally reattach the attachment with the requirement (used in later tests)
			attachmentManager.InsertArtifactAssociation(projectId, attachmentId3, requirementId, Artifact.ArtifactTypeEnum.Requirement, 1);
		}

		/// <summary>
		/// Tests that we can move the project attachment between folders and delete it from the project
		/// </summary>
		[
		Test,
		SpiraTestCase(430)
		]
		public void _09_MoveDeleteDocuments()
		{
			//First lets try moving a document between folders
			//Verify its current state
			ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId4);
			Assert.AreEqual(folderId2, projectAttachment.ProjectAttachmentFolderId);

			//Now move it to a different folder
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId4);
			projectAttachment.StartTracking();
			projectAttachment.ProjectAttachmentFolderId = folderId4;
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify the updates succeeded
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId4);
			Assert.AreEqual(folderId4, projectAttachment.ProjectAttachmentFolderId);

			//Now verify that we can delete the document from the project
			attachmentManager.Delete(projectId, attachmentId4, 1);
			bool exceptionThrown = false;
			try
			{
				projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId4);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "The artifact was not successfully deleted from the project");
		}

		/// <summary>
		/// Tests that we can make a new project based on the current project and/or copy a project and have the attachment
		/// information move across correctly. Need to replicate the types and folder hierarchy in the new project
		/// </summary>
		[
		Test,
		SpiraTestCase(431)
		]
		public void _10_ProjectAttachmentCopy()
		{
			//First lets verify that making a new project that is based on the original will bring across the types and folders
			//We want to test that folders and types get copied across so we have it create a unique template
			Business.ProjectManager projectManager = new Business.ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			projectId2 = projectManager.CreateFromExisting("Project - Copy", "", "", projectId, true, true, 1, adminSectionId, "Inserted Project");
			//Get the template associated with the project
			projectTemplateId2 = new TemplateManager().RetrieveForProject(projectId2).ProjectTemplateId;

			//Lets retrieve the folders in this project and verify that they're the same as the original project
			List<ProjectAttachmentFolderHierarchy> projectFolders = attachmentManager.RetrieveFoldersByProjectId(projectId2);
			Assert.AreEqual("Root Folder", projectFolders[0].Name, "Original Root folder had wrong name.");
			Assert.AreEqual("AAA", projectFolders[0].IndentLevel, "Original Root folder had wrong indent.");
			Assert.AreEqual("Folder 1", projectFolders[1].Name, "Original Child folder 1 had wrong name.");
			Assert.AreEqual("AAAAAA", projectFolders[1].IndentLevel, "Original Child folder 1 had wrong indent.");
			Assert.AreEqual("Folder 2", projectFolders[2].Name, "Original Child folder 2 had wrong name.");
			Assert.AreEqual("AAAAAB", projectFolders[2].IndentLevel, "Original Child folder 2a had wrong indent.");
			Assert.AreEqual("Folder 2a", projectFolders[3].Name, "Original Child folder 2a had wrong name.");
			Assert.AreEqual("AAAAABAAA", projectFolders[3].IndentLevel, "Original Child folder 2a had wrong indent.");

			//Now lets retrieve the types and make sure they are the same as the original project
			List<DocumentType> projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId2, false);
			Assert.AreEqual(4, projectAttachmentTypes.Count, "Original Number of Document Types was wrong.");
			Assert.AreEqual("Default", projectAttachmentTypes[0].Name, "Original Name of Default incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[0].IsActive, "Original Default IsActive flag wrong.");
			Assert.AreEqual(false, projectAttachmentTypes[0].IsDefault, "Original Default IsDefault flag wrong.");
			Assert.AreEqual("Type 1", projectAttachmentTypes[1].Name, "Original Name of Type1 incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[1].IsActive, "Original Type1 IsActive flag wrong.");
			Assert.AreEqual(false, projectAttachmentTypes[1].IsDefault, "Original Type2a IsDefault flag wrong.");
			Assert.AreEqual("Type 2a", projectAttachmentTypes[2].Name, "Original Name of Type2a incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[2].IsActive, "Original Type2a IsActive flag wrong.");
			Assert.AreEqual(true, projectAttachmentTypes[2].IsDefault, "Original Type2a IsDefault flag wrong.");

			//Now lets test that we can copy the project and have the folders, types and documents be migrated across
			//as well as the association to any copied artifacts (requirements, releases and test cases)
			//Again we want to have its own template
			projectId3 = projectManager.Copy(USER_ID_FRED_BLOGGS, projectId, true, null, adminSectionId, "Project Cloned");

			//Get the template associated with the project
			projectTemplateId3 = new TemplateManager().RetrieveForProject(projectId3).ProjectTemplateId;

			//Lets retrieve the folders in this project and verify that they're the same as the original project
			projectFolders = attachmentManager.RetrieveFoldersByProjectId(projectId3);
			Assert.AreEqual("Root Folder", projectFolders[0].Name);
			Assert.AreEqual("AAA", projectFolders[0].IndentLevel, "Copy Root folder had wrong indent.");
			Assert.AreEqual("Folder 1", projectFolders[1].Name, "Copy folder 1 had wrong name.");
			Assert.AreEqual("AAAAAA", projectFolders[1].IndentLevel, "Copy folder 1 had wrong indent.");
			Assert.AreEqual("Folder 2", projectFolders[2].Name, "Copy folder 2 had wrong name.");
			Assert.AreEqual("AAAAAB", projectFolders[2].IndentLevel, "Copy folder 2a had wrong indent.");
			Assert.AreEqual("Folder 2a", projectFolders[3].Name, "Copy folder 2a had wrong name.");
			Assert.AreEqual("AAAAABAAA", projectFolders[3].IndentLevel, "Copy folder 2a had wrong indent.");

			//Store the new folder ids for verifying later
			newFolderId1 = projectFolders[0].ProjectAttachmentFolderId;
			newFolderId2 = projectFolders[1].ProjectAttachmentFolderId;
			newFolderId3 = projectFolders[2].ProjectAttachmentFolderId;
			newFolderId4 = projectFolders[3].ProjectAttachmentFolderId;

			//Now lets retrieve the types and make sure they are the same as the original project
			projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId3, false);
			Assert.AreEqual(4, projectAttachmentTypes.Count, "Copy Number of Document Types was wrong.");
			Assert.AreEqual("Default", projectAttachmentTypes[0].Name, "Copy Name of Default incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[0].IsActive, "Copy Default IsActive flag wrong.");
			Assert.AreEqual(false, projectAttachmentTypes[0].IsDefault, "Copy Default IsDefault flag wrong.");
			Assert.AreEqual("Type 1", projectAttachmentTypes[1].Name, "Copy Name of Type1 incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[1].IsActive, "Copy Type1 IsActive flag wrong.");
			Assert.AreEqual(false, projectAttachmentTypes[1].IsDefault, "Copy Type2a IsDefault flag wrong.");
			Assert.AreEqual("Type 2a", projectAttachmentTypes[2].Name, "Copy Name of Type2a incorrect.");
			Assert.AreEqual(true, projectAttachmentTypes[2].IsActive, "Copy Type2a IsActive flag wrong.");
			Assert.AreEqual(true, projectAttachmentTypes[2].IsDefault, "Copy Type2a IsDefault flag wrong.");

			//Now we need to verify that the documents were copied across and associated with the correct folders and types
			//Also make sure the attachment ids are not the same since they are physical copies (since v5.0)
			List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(projectId3, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			//Attachment 1
			ProjectAttachmentView projectAttachmentView = projectAttachments.FirstOrDefault(d => d.Filename == "test_data7.txt");
			Assert.AreNotEqual(attachmentId1, projectAttachmentView.AttachmentId, "Attach1 did not have different ID.");
			Assert.AreEqual(newFolderId2, projectAttachmentView.ProjectAttachmentFolderId, "Attach1 had different FolderId.");
			Assert.AreEqual("Type 1", projectAttachmentView.DocumentTypeName, "Attach1 had wrong DocumentTypeName.");
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion, "Attach1 had wton CurrentVersion");
			Assert.AreEqual("test_data7.txt", projectAttachmentView.Filename, "Attach1 had wront FileName");
			//Attachment 2
			projectAttachmentView = projectAttachments.FirstOrDefault(d => d.Filename == "test_data2.dat");
			Assert.AreNotEqual(attachmentId2, projectAttachmentView.AttachmentId, "Attach2 did not have different ID.");
			Assert.AreEqual(newFolderId4, projectAttachmentView.ProjectAttachmentFolderId, "Attach2 had different FolderId.");
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName, "Attach2 had wrong DocumentTypeName.");
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion, "Attach2 had wton CurrentVersion");
			Assert.AreEqual("test_data2.dat", projectAttachmentView.Filename, "Attach2 had wront FileName");
			//Attachment 3
			projectAttachmentView = projectAttachments.FirstOrDefault(d => d.Filename == "test_data3.htm");
			Assert.AreNotEqual(attachmentId3, projectAttachmentView.AttachmentId, "Attach3 did not have different ID.");
			Assert.AreEqual(newFolderId4, projectAttachmentView.ProjectAttachmentFolderId, "Attach3 had different FolderId.");
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName, "Attach3 had wrong DocumentTypeName.");
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion, "Attach3 had wton CurrentVersion");
			Assert.AreEqual("test_data3.htm", projectAttachmentView.Filename, "Attach3 had wront FileName");
			int newAttachmentId3 = projectAttachmentView.AttachmentId;
			//Attachment 5
			projectAttachmentView = projectAttachments.FirstOrDefault(d => d.Filename == "test_data5.txt");
			Assert.AreNotEqual(attachmentId5, projectAttachmentView.AttachmentId, "Attach5 did not have different ID.");
			Assert.AreEqual(newFolderId3, projectAttachmentView.ProjectAttachmentFolderId, "Attach5 had different FolderId.");
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName, "Attach5 had wrong DocumentTypeName.");
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion, "Attach5 had wton CurrentVersion");
			Assert.AreEqual("test_data5.txt", projectAttachmentView.Filename, "Attach5 had wront FileName");

			//Also need to verify that they were associated with the correct requirements and test case artifacts

			//First get the get the id of the new requirement and test case (i.e. the copy made)
			RequirementManager requirementManager = new RequirementManager();
			List<RequirementView> requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId3, 1, Int32.MaxValue, null, 0);
			int requirementId = requirements[0].RequirementId;
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId3, "Name", true, 1, 1, null, 0);
			int testCaseId = testCases[0].TestCaseId;

			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			List<ArtifactAttachmentView> associations = artifactLinkManager.RetrieveByAttachmentId(projectId3, newAttachmentId3);
			Assert.AreEqual(2, associations.Count, "Artifact Associations for Attach3 was wrong.");
			//Requirement
			Assert.AreEqual(requirementId, associations[0].ArtifactId, "Req Assoc had wrong Artifact ID.");
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Requirement, associations[0].ArtifactTypeId, "Req Assoc had wrong Artifact Type ID.");
			Assert.AreEqual(USER_ID_FRED_BLOGGS, associations[0].CreatorId, "Req Assoc had wrong Creator ID.");
			//Test Case
			Assert.AreEqual(testCaseId, associations[1].ArtifactId, "TC Assoc had wrong Artifact ID.");
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, associations[1].ArtifactTypeId, "TC Assoc had wrong Artifact Type ID.");
			Assert.AreEqual(USER_ID_JOE_SMITH, associations[1].CreatorId, "TC Assoc had wrong Creator ID.");
		}

		/// <summary>
		/// Tests that we can delete project attachment folders, which will delete any child folders and attachments
		/// </summary>
		[
		Test,
		SpiraTestCase(432)
		]
		public void _11_DeleteFolders()
		{
			//Lets use the copy of the project to test out the deletes

			//Let's delete a folder that is child that contains other child folders
			attachmentManager.DeleteFolder(projectId3, newFolderId3, USER_ID_FRED_BLOGGS);

			//Verify that both the folder and its child folder were deleted
			//Main Folder
			bool exceptionThrown = false;
			try
			{
				ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderById(newFolderId3);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Folder was not deleted correctly");

			//Child Folder
			exceptionThrown = false;
			try
			{
				ProjectAttachmentFolder folder = attachmentManager.RetrieveFolderById(newFolderId4);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Folder was not deleted correctly");

			//Verify that we can't delete the root folder
			exceptionThrown = false;
			try
			{
				attachmentManager.DeleteFolder(projectId3, newFolderId1, 1);
			}
			catch (ProjectDefaultAttachmentFolderException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to delete root folder for a project");
		}

		/// <summary>
		/// Tests that we can delete the project and all the attachment information gets deleted safely
		/// and that the attachments only get physically deleted only if removed from all referenced projects
		/// </summary>
		[
		Test,
		SpiraTestCase(433)
		]
		public void _12_DeleteProject()
		{
			Business.ProjectManager projectManager = new Business.ProjectManager();
			//Delete the copied projects and templates
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId2);
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId3);

			TemplateManager templateManager = new TemplateManager();
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId2);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId3);

			//Verify that the attachments still exist in the main project
			//Verify that they uploaded correctly
			//Attachment 1
			ProjectAttachmentView projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId1);
			Assert.AreEqual(attachmentId1, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId2, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 1", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.1", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data7.txt", projectAttachmentView.Filename);
			//Attachment 2
			projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId2);
			Assert.AreEqual(attachmentId2, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId4, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data2.dat", projectAttachmentView.Filename);
			//Attachment 3
			projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId3);
			Assert.AreEqual(attachmentId3, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId4, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data3.htm", projectAttachmentView.Filename);
			//Attachment 5
			projectAttachmentView = attachmentManager.RetrieveForProjectById2(projectId, attachmentId5);
			Assert.AreEqual(attachmentId5, projectAttachmentView.AttachmentId);
			Assert.AreEqual(folderId3, projectAttachmentView.ProjectAttachmentFolderId);
			Assert.AreEqual("Type 2a", projectAttachmentView.DocumentTypeName);
			Assert.AreEqual("1.0", projectAttachmentView.CurrentVersion);
			Assert.AreEqual("test_data5.txt", projectAttachmentView.Filename);

			//Delete the main project and template
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId);

			//Verify that the attachments were actually deleted completely
			//Attachment 1
			bool exceptionThrown = false;
			try
			{
				Attachment attachment = attachmentManager.RetrieveById(attachmentId1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Attachment1 was not deleted");

			//Attachment 2
			exceptionThrown = false;
			try
			{
				Attachment attachment = attachmentManager.RetrieveById(attachmentId2);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Attachment2 was not deleted");

			//Attachment 3
			exceptionThrown = false;
			try
			{
				Attachment attachment = attachmentManager.RetrieveById(attachmentId3);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Attachment3 was not deleted");

			//Attachment 5
			exceptionThrown = false;
			try
			{
				Attachment attachment = attachmentManager.RetrieveById(attachmentId5);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Attachment5 was not deleted");
		}

		/// <summary>
		///Now we need to test that we can get a filterable/sortable list for a specific artifact
		///rather than a specific folder
		/// </summary>
		[
		Test,
		SpiraTestCase(574)
		]
		public void _13_RetrieveByArtifactFiltered()
		{
			//Get a list of attachments for a requirement (no filters)
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, 4, Artifact.ArtifactTypeEnum.Requirement, "EditedDate", false, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);

			//Verify the count
			int count = attachmentManager.CountByArtifactId(PROJECT_ID, 4, Artifact.ArtifactTypeEnum.Requirement, null, 0);
			Assert.AreEqual(4, count, "Count1");
			Assert.AreEqual(4, attachments.Count, "Count2");

			//Verify the items
			//Record 0
			Assert.AreEqual("Book Management Screen Wireframe.ai", attachments[0].Filename);
			Assert.AreEqual("Screen Layout", attachments[0].DocumentTypeName);
			Assert.AreEqual(392, attachments[0].Size);
			//Record 1
			Assert.AreEqual("http://www.inflectra.com", attachments[1].Filename);
			Assert.AreEqual("Functional Specification", attachments[1].DocumentTypeName);
			Assert.AreEqual(0, attachments[1].Size);

			//Now lets try filtering the results
			Hashtable filters = new Hashtable();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add(6);
			filters.Add("DocumentTypeId", multiValueFilter);
			attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, 4, Artifact.ArtifactTypeEnum.Requirement, "Filename", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);

			//Verify the count
			count = attachmentManager.CountByArtifactId(PROJECT_ID, 4, Artifact.ArtifactTypeEnum.Requirement, null, 0);
			Assert.AreEqual(4, count, "Count1");
			Assert.AreEqual(2, attachments.Count, "Count2");

			//Verify the items
			//Record 0
			Assert.AreEqual("Book Management Screen Wireframe.ai", attachments[0].Filename);
			Assert.AreEqual("Screen Layout", attachments[0].DocumentTypeName);
			Assert.AreEqual(392, attachments[0].Size);
			//Record 2
			Assert.AreEqual("Graphical Design Mockups.psd", attachments[1].Filename);
			Assert.AreEqual("Screen Layout", attachments[1].DocumentTypeName);
			Assert.AreEqual(1009, attachments[1].Size);
		}

		/// <summary>
		///Test that we can view the tag frequency values for use in the tag cloud
		/// </summary>
		[
		Test,
		SpiraTestCase(750)
		]
		public void _14_CreateAndViewTagList()
		{
			//Lets create the new project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			int projectId4 = projectManager.Insert("New Project", null, "", "", true, null, 1, adminSectionId, "Inserted Project");
			int projectTemplateId4 = new TemplateManager().RetrieveForProject(projectId4).ProjectTemplateId;

			//Add some documents with tags
			attachmentId1 = attachmentManager.Insert(projectId4, "www.url1.com", "Test Attachment 1", USER_ID_FRED_BLOGGS, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, attachment", null, null, null);
			attachmentId2 = attachmentManager.Insert(projectId4, "www.url1.com", "Test Attachment 2", USER_ID_JOE_SMITH, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, attachment", null, null, null);
			attachmentId3 = attachmentManager.Insert(projectId4, "www.url1.com", "Test Attachment 3", USER_ID_JOE_SMITH, null, Artifact.ArtifactTypeEnum.None, "1.0", "test, web page", null, null, null);

			//Verify the tag count
			List<ProjectTagFrequency> projectAttachmentTags = attachmentManager.RetrieveTagFrequency(projectId4);
			Assert.AreEqual(3, projectAttachmentTags.Count);
			Assert.AreEqual("attachment", projectAttachmentTags[0].Name);
			Assert.AreEqual(2, projectAttachmentTags[0].Frequency);
			Assert.AreEqual("test", projectAttachmentTags[1].Name);
			Assert.AreEqual(3, projectAttachmentTags[1].Frequency);
			Assert.AreEqual("web page", projectAttachmentTags[2].Name);
			Assert.AreEqual(1, projectAttachmentTags[2].Frequency);

			//Now update the tags of one of the documents
			ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId4, attachmentId3);
			Attachment attachment = projectAttachment.Attachment;
			attachment.StartTracking();
			attachment.Tags = "web page, attachment, roger dalton";
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify the tag count
			projectAttachmentTags = attachmentManager.RetrieveTagFrequency(projectId4);
			Assert.AreEqual(4, projectAttachmentTags.Count);
			Assert.AreEqual("attachment", projectAttachmentTags[0].Name);
			Assert.AreEqual(3, projectAttachmentTags[0].Frequency);
			Assert.AreEqual("roger dalton", projectAttachmentTags[1].Name);
			Assert.AreEqual(1, projectAttachmentTags[1].Frequency);
			Assert.AreEqual("test", projectAttachmentTags[2].Name);
			Assert.AreEqual(2, projectAttachmentTags[2].Frequency);
			Assert.AreEqual("web page", projectAttachmentTags[3].Name);
			Assert.AreEqual(1, projectAttachmentTags[3].Frequency);

			//Now delete one of the documents
			attachmentManager.Delete(projectId4, attachmentId3, 1);

			//Verify the tag count
			projectAttachmentTags = attachmentManager.RetrieveTagFrequency(projectId4);
			Assert.AreEqual(2, projectAttachmentTags.Count);
			Assert.AreEqual("attachment", projectAttachmentTags[0].Name);
			Assert.AreEqual(2, projectAttachmentTags[0].Frequency);
			Assert.AreEqual("test", projectAttachmentTags[1].Name);
			Assert.AreEqual(2, projectAttachmentTags[1].Frequency);

			//Now test that we can blank out the tags for a document and readd them
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId4, attachmentId2, true);
			Assert.AreEqual("test, attachment", projectAttachment.Attachment.Tags);

			//Set to null
			projectAttachment.Attachment.StartTracking();
			projectAttachment.Attachment.Tags = null;
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId4, attachmentId2, true);
			Assert.IsNull(projectAttachment.Attachment.Tags);

			//Put them back
			projectAttachment.Attachment.StartTracking();
			projectAttachment.Attachment.Tags = "test, attachment";
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId4, attachmentId2, true);
			Assert.AreEqual("test, attachment", projectAttachment.Attachment.Tags);

			//Change them (not null)
			projectAttachment.Attachment.StartTracking();
			projectAttachment.Attachment.Tags = "test2";
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS);

			//Verify
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId4, attachmentId2, true);
			Assert.AreEqual("test2", projectAttachment.Attachment.Tags);

			//Delete the project to clean-up
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId4);

			//Make sure it cascaded correctly
			projectAttachmentTags = attachmentManager.RetrieveTagFrequency(projectId4);
			Assert.AreEqual(0, projectAttachmentTags.Count);

			//Delete the template as well
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId4);
		}
	}
}
