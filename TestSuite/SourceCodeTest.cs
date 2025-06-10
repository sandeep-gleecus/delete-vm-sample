using DiffPlex.DiffBuilder.Model;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>This fixture tests the version control integration functionality of the SourceCode business object</summary>
	[TestFixture]
	[SpiraTestConfiguration(
		InternalRoutines.SPIRATEST_INTERNAL_URL,
		InternalRoutines.SPIRATEST_INTERNAL_LOGIN,
		InternalRoutines.SPIRATEST_INTERNAL_PASSWORD,
		InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID,
		InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class SourceCodeTest
	{
		//Users used for TaraVault tests
		private const int USER_ID_TEST1 = 2;
		private const int USER_ID_TEST2 = 3;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID = 1;

		private const int SOURCE_CODE_PROJECT_ID = 1;  //We use the sample Library Information system project
		private static int taraVaultProjectId;   //The separate project used for the TV tests
		private static int taraVaultTemplateId;

		/// <summary>The master branch in our cached data.</summary>
		private const string masterBranch = "main";
		private const string developBranch = "develop";

		/// <summary>Called at the beginning of test execution. This function removes the cache for the Test Provider,
		/// initializes the test provider to reassemble cache, and then loads the cache
		/// into memory for comparison against results from the manager.</summary>
		[TestFixtureSetUp]
		public void Test_Load()
		{
			Logger.LogInformationalEvent("SourceCodeTester", "Initializing Tests..");

			//Create a new project for the TaraVault tests, we use the sample LIS project for the other tests
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			taraVaultProjectId = projectManager.Insert("TaraVault Test Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			taraVaultTemplateId = new TemplateManager().RetrieveForProject(taraVaultProjectId).ProjectTemplateId;

			//Create the manager for use during the tests..
			Logger.LogInformationalEvent("SourceCodeTester", "Initializing manager for tests.");
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);

			//Now initialize provider and have it regenerate cache files from scratch
			Logger.LogInformationalEvent("SourceCodeTester", "Initializing manager for new cache files, waiting 10 secs.");
			if (!SourceCodeManager.IsCacheUpdateRunning)
			{
				Logger.LogInformationalEvent("SourceCodeTester", "Refreshing Cache");
				sourceCodeManager.ClearCacheAndRefresh();
			}

			//Wait five seconds to make sure the caching is finished.
			Thread.Sleep(5 * 1000);

			//Do one final cache update to make sure IDs are stable
			if (!SourceCodeManager.IsCacheUpdateRunning)
			{
				sourceCodeManager.LaunchCacheRefresh();
			}

			//Wait five seconds to make sure the caching is finished.
			Thread.Sleep(5 * 1000);

			//Now reload the provider (needed to be an accurate test)
			sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
		}

		/// <summary>Called at the end of the test to clean up.</summary>
		[TestFixtureTearDown]
		public void Test_Unload()
		{
			//Delete the TaraVault test project and template
			ProjectManager projectManager = new ProjectManager();
			projectManager.Delete(USER_ID_FRED_BLOGGS, taraVaultProjectId);
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, taraVaultTemplateId);

			//Delete any providers apart from the default one
			SourceCodeManager sourceCodeManager = new SourceCodeManager();
			List<VersionControlSystem> systems = sourceCodeManager.RetrieveSystems();
			foreach (VersionControlSystem system in systems)
			{
				if (system.Name != SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME2)
				{
					sourceCodeManager.DeleteSystem(system.VersionControlSystemId, 1);
				}
			}

			//Deactivate Tara, even though it may already BE deactivated.
			VaultManager vaultManager = new VaultManager();
			vaultManager.Account_Deactivate();

			//Next return TaraVault to 0 licenses, normally done by website
			ConfigurationSettings.Default.TaraVault_UserLicense = 0;
			ConfigurationSettings.Default.Save();

			try
			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "Source Code";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;
				//Finally make sure we have entries for the projects that use the default provider 
				sourceCodeManager.InsertProjectSettings(1, 1, true, "test://MyRepository", "joesmith", "joesmith", "MyCompany", "testvalue", null, null, null, null, 1, adminSectionId, "Inserted Project Settings in Source Code");
				sourceCodeManager.InsertProjectSettings(1, 2, true, null, null, null, null, null, null, null, null, null, 1, adminSectionId, "Inserted Project Settings in Source Code");
				sourceCodeManager.InsertProjectSettings(1, 3, true, null, null, null, null, null, null, null, null, null, 1, adminSectionId, "Inserted Project Settings in Source Code");
			}
			catch (Exception) { }

			//Finally clear and rebuild the cache for the API tests that come next
			sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			if (!SourceCodeManager.IsCacheUpdateRunning)
			{
				Logger.LogInformationalEvent("SourceCodeTester", "Refreshing Cache");
				sourceCodeManager.ClearCacheAndRefresh();
			}
		}

		/// <summary>This tests that the branches returned by the provider are the same that we loaded from cache,
		/// checking names and 'IsDefault' statuses.</summary>
		[Test]
		[SpiraTestCase(1308)]
		public void _01_CheckBranches()
		{
			//Get branches from manager.
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<SourceCodeBranch> mgrBranches = sourceCodeManager.RetrieveBranches();

			//Check that we have the expected number and that only one is defult
			Assert.AreEqual(11, mgrBranches.Count, "Number of returned branches is incorrect");
			int count = mgrBranches.Count(b => b.IsDefault);
			Assert.AreEqual(1, count, "Returned branches had more than one IsDefault flag!");

			//Check some of the items in the branch list
			SourceCodeBranch branch = mgrBranches.FirstOrDefault(b => b.BranchKey == "release/1.0.0.0");
			Assert.AreEqual("release/1.0.0.0", branch.BranchKey);
			Assert.AreEqual(false, branch.IsDefault);
			branch = mgrBranches.FirstOrDefault(b => b.BranchKey == "main");
			Assert.AreEqual("main", branch.BranchKey);
			Assert.AreEqual(true, branch.IsDefault);

			//Now view that we can retrieve a branch by its name
			branch = sourceCodeManager.RetrieveBranchByName("develop");
			Assert.AreEqual("develop", branch.BranchKey);
			Assert.AreEqual(false, branch.IsDefault);
			branch = sourceCodeManager.RetrieveBranchByName("feature/sprint-2");
			Assert.AreEqual("feature/sprint-2", branch.BranchKey);
			Assert.AreEqual(false, branch.IsDefault);
			branch = sourceCodeManager.RetrieveBranchByName("main");
			Assert.AreEqual("main", branch.BranchKey);
			Assert.AreEqual(true, branch.IsDefault);

			//Verify that returning back a non-existant branch is handled OK
			branch = sourceCodeManager.RetrieveBranchByName("XXX");
			Assert.IsNull(branch);
		}

		/// <summary>Tests that we can get a list of files both for the repository as a whole and
		/// also for a specific revision. Need to test filtering, sorting operations as well.</summary>
		[Test]
		[SpiraTestCase(2562)]
		public void _02a_ViewFolders()
		{
			//First verify that we can get the list of folders in the repo - MASTER branch
			string branchName = masterBranch;

			//First test that we can get root folder in 'main' branch
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<SourceCodeFolder> sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(null, branchName);
			Assert.AreEqual(1, sourceCodeFolders.Count);
			Assert.AreEqual("Root", sourceCodeFolders[0].Name);
			Assert.AreEqual("test://MyRepository", sourceCodeFolders[0].FolderKey);
			Assert.IsTrue(sourceCodeFolders[0].IsRoot);
			string rootFolderKey = sourceCodeFolders[0].FolderKey;
			int rootFolderId = sourceCodeFolders[0].FolderId;

			//Next test that we can get a list of top-level folders in 'main' branch
			sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(rootFolderId, branchName);
			Assert.AreEqual(4, sourceCodeFolders.Count);
			Assert.AreEqual("Code", sourceCodeFolders[0].Name);
			Assert.AreEqual("/Root/Code", sourceCodeFolders[0].FolderKey);
			Assert.AreEqual("Samples", sourceCodeFolders[1].Name);
			Assert.AreEqual("/Root/Samples", sourceCodeFolders[1].FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolders[2].Name);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolders[2].FolderKey);
			Assert.AreEqual("Tests", sourceCodeFolders[3].Name);
			Assert.AreEqual("/Root/Tests", sourceCodeFolders[3].FolderKey);

			//Now test that we can retrieve a folder by its key
			SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Documentation", branchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);

			//Now test that we can retrieve a folder by its cache ID
			string testBranchName;
			sourceCodeFolder = sourceCodeManager.RetrieveFolderById(sourceCodeFolder.FolderId, out testBranchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//Test that we can get the folder of a specific file
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByFileKey("/Root/Documentation/Overview.txt", branchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//Test that we can retrieve the parent folder of a folder, both non-root and root
			//Non-root
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Code/api", branchName);
			int? parentFolderId = sourceCodeManager.RetrieveParentFolder(sourceCodeFolder.FolderId, branchName);
			Assert.IsNotNull(parentFolderId);
			sourceCodeFolder = sourceCodeManager.RetrieveFolderById(parentFolderId.Value, out testBranchName);
			Assert.AreEqual("/Root/Code", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Code", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//root
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("test://MyRepository", branchName);
			parentFolderId = sourceCodeManager.RetrieveParentFolder(sourceCodeFolder.FolderId, branchName);
			Assert.IsNull(parentFolderId);

			//test that we can retrieve all the parent folders of a file, used in breadcrumbing
			sourceCodeFolders = sourceCodeManager.RetrieveParentFolders("/Root/Code/api/package-info.java", branchName);
			Assert.AreEqual(3, sourceCodeFolders.Count);
			Assert.AreEqual("api", sourceCodeFolders[0].Name);
			Assert.AreEqual("/Root/Code/api", sourceCodeFolders[0].FolderKey);
			Assert.AreEqual("Code", sourceCodeFolders[1].Name);
			Assert.AreEqual("/Root/Code", sourceCodeFolders[1].FolderKey);
			Assert.AreEqual("Root", sourceCodeFolders[2].Name);
			Assert.AreEqual("test://MyRepository", sourceCodeFolders[2].FolderKey);

			//Now repeat for a named branch
			branchName = "feature/sprint-2";

			//First test that we can get root folder in 'main' branch
			sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(null, branchName);
			Assert.AreEqual(1, sourceCodeFolders.Count);
			Assert.AreEqual("Root", sourceCodeFolders[0].Name);
			Assert.AreEqual("test://MyRepository", sourceCodeFolders[0].FolderKey);
			Assert.IsTrue(sourceCodeFolders[0].IsRoot);
			rootFolderKey = sourceCodeFolders[0].FolderKey;
			rootFolderId = sourceCodeFolders[0].FolderId;

			//Next test that we can get a list of top-level folders in 'main' branch
			sourceCodeFolders = sourceCodeManager.RetrieveFoldersByParentId(rootFolderId, branchName);
			Assert.AreEqual(4, sourceCodeFolders.Count);
			Assert.AreEqual("Code", sourceCodeFolders[0].Name);
			Assert.AreEqual("/Root/Code", sourceCodeFolders[0].FolderKey);
			Assert.AreEqual("Samples", sourceCodeFolders[1].Name);
			Assert.AreEqual("/Root/Samples", sourceCodeFolders[1].FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolders[2].Name);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolders[2].FolderKey);
			Assert.AreEqual("Tests", sourceCodeFolders[3].Name);
			Assert.AreEqual("/Root/Tests", sourceCodeFolders[3].FolderKey);

			//Now test that we can retrieve a folder by its key
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Documentation", branchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);

			//Now test that we can retrieve a folder by its cache ID
			sourceCodeFolder = sourceCodeManager.RetrieveFolderById(sourceCodeFolder.FolderId, out testBranchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//Test that we can get the folder of a specific file
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByFileKey("/Root/Documentation/Overview.txt", branchName);
			Assert.AreEqual("/Root/Documentation", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Documentation", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//Test that we can retrieve the parent folder of a folder, both non-root and root
			//Non-root
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Code/api", branchName);
			parentFolderId = sourceCodeManager.RetrieveParentFolder(sourceCodeFolder.FolderId, branchName);
			Assert.IsNotNull(parentFolderId);
			sourceCodeFolder = sourceCodeManager.RetrieveFolderById(parentFolderId.Value, out testBranchName);
			Assert.AreEqual("/Root/Code", sourceCodeFolder.FolderKey);
			Assert.AreEqual("Code", sourceCodeFolder.Name);
			Assert.AreEqual(branchName, testBranchName);

			//root
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("test://MyRepository", branchName);
			parentFolderId = sourceCodeManager.RetrieveParentFolder(sourceCodeFolder.FolderId, branchName);
			Assert.IsNull(parentFolderId);

			//test that we can retrieve all the parent folders of a file, used in breadcrumbing
			sourceCodeFolders = sourceCodeManager.RetrieveParentFolders("/Root/Code/api/package-info.java", branchName);
			Assert.AreEqual(3, sourceCodeFolders.Count);
			Assert.AreEqual("api", sourceCodeFolders[0].Name);
			Assert.AreEqual("/Root/Code/api", sourceCodeFolders[0].FolderKey);
			Assert.AreEqual("Code", sourceCodeFolders[1].Name);
			Assert.AreEqual("/Root/Code", sourceCodeFolders[1].FolderKey);
			Assert.AreEqual("Root", sourceCodeFolders[2].Name);
			Assert.AreEqual("test://MyRepository", sourceCodeFolders[2].FolderKey);
		}

		/// <summary>Tests that we can get a list of files both for the repository as a whole and
		/// also for a specific revision. Need to test filtering, sorting operations as well.</summary>
		[Test]
		[SpiraTestCase(564)]
		public void _02b_ViewFiles()
		{
			//First using the MASTER branch
			string branchName = masterBranch;

			//Lets retrieve some files in a folder
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Code", branchName);

			//Set defaults, first..
			int numPerPage = 15;
			int pageNo = 1;
			int totalCount = -1;
			string sortField = SourceCodeManager.FIELD_NAME;
			bool sortAsc = true;
			string checkBranch = null;

			//Now, send our query..
			List<SourceCodeFile> sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(7, totalCount);
			Assert.AreEqual(7, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("package-info.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/package-info.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[0].AuthorName);
			//File 3
			Assert.AreEqual("SpiraTestConfiguration.java", sourceCodeFiles[2].Name);
			Assert.AreEqual("/Root/Code/SpiraTestConfiguration.java", sourceCodeFiles[2].FileKey);
			Assert.AreEqual("0003", sourceCodeFiles[2].RevisionKey);
			Assert.AreEqual("rev0003", sourceCodeFiles[2].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[2].AuthorName);

			//Now lets try sorting by another field
			sortField = SourceCodeManager.FIELD_COMMIT;
			sortAsc = false;
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(7, totalCount);
			Assert.AreEqual(7, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("SpiraTestExecute.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/SpiraTestExecute.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0012", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0012", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("Donna Marshall", sourceCodeFiles[0].AuthorName);
			//File 3
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFiles[2].Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFiles[2].FileKey);
			Assert.AreEqual("0010", sourceCodeFiles[2].RevisionKey);
			Assert.AreEqual("rev0010", sourceCodeFiles[2].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[2].AuthorName);

			//Try filtering by file name
			Hashtable filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_NAME, "Spira");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				filters,
				out checkBranch,
				out totalCount);
			//Verify
			Assert.AreEqual(4, totalCount);
			Assert.AreEqual(4, sourceCodeFiles.Count);

			//Try filtering by author
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_AUTHOR, "Fred");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				filters,
				out checkBranch,
				out totalCount);
			//Verify
			Assert.AreEqual(3, totalCount);
			Assert.AreEqual(3, sourceCodeFiles.Count);

			//Test that we can get files in the root folder as well
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("test://MyRepository", branchName);
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(2, totalCount);
			Assert.AreEqual(2, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("README.md", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/README.md", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[0].AuthorName);
			//File 2
			Assert.AreEqual(".gitignore", sourceCodeFiles[1].Name);
			Assert.AreEqual("/Root/.gitignore", sourceCodeFiles[1].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[1].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[1].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[1].AuthorName);

			//Next lets view files associated with a revision, sorted by name ascending
			sortField = SourceCodeManager.FIELD_NAME;
			sortAsc = true;
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0000",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(31, totalCount);
			Assert.AreEqual(15, sourceCodeFiles.Count);

			//Lets try another revision
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0001",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(3, totalCount);
			Assert.AreEqual(3, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("SpiraTestConfiguration.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/SpiraTestConfiguration.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0003", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0003", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[0].AuthorName);
			//File 2
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFiles[1].Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFiles[1].FileKey);
			Assert.AreEqual("0010", sourceCodeFiles[1].RevisionKey);
			Assert.AreEqual("rev0010", sourceCodeFiles[1].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[1].AuthorName);

			//Now lets try filtering by latest revision
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_COMMIT, "0003");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0000",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(31, totalCount);
			Assert.AreEqual(1, sourceCodeFiles.Count);

			//Now verify that we can get a file by its key
			SourceCodeFile sourceCodeFile = sourceCodeManager.RetrieveFileByKey("/Root/Code/SpiraTestListener.java", branchName);

			//Verify
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFile.Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFile.FileKey);
			Assert.AreEqual("0010", sourceCodeFile.RevisionKey);
			Assert.AreEqual("rev0010", sourceCodeFile.RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFile.AuthorName);

			//Now verify that we can get the file by its cache ID
			string testBranchName;
			sourceCodeFile = sourceCodeManager.RetrieveFileById(sourceCodeFile.FileId, out testBranchName);
			Assert.AreEqual(branchName, testBranchName);

			//Verify
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFile.Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFile.FileKey);
			Assert.AreEqual("0010", sourceCodeFile.RevisionKey);
			Assert.AreEqual("rev0010", sourceCodeFile.RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFile.AuthorName);

			//Test that we can count the files in the revisions
			int fileCount = sourceCodeManager.CountFilesForRevision("0000", branchName);
			Assert.AreEqual(31, fileCount);
			fileCount = sourceCodeManager.CountFilesForRevision("0001", branchName);
			Assert.AreEqual(3, fileCount);

			//Now verify that we can get the latest revision of this file (the actual file content)
			SourceCodeFileStream stream = sourceCodeManager.OpenFile("/Root/Code/SpiraTestListener.java", null, branchName);
			Stream fileStream = stream.DataStream;
			byte[] buffer = new byte[fileStream.Length];
			fileStream.Read(buffer, 0, (int)fileStream.Length);
			string text = Encoding.UTF8.GetString(buffer);
			sourceCodeManager.CloseFile(stream);
			Assert.IsTrue(text.Length > 10);

			//Now verify that we can get a specific revision of a file
			stream = sourceCodeManager.OpenFile("/Root/Code/SpiraTestListener.java", "0005", branchName);
			fileStream = stream.DataStream;
			buffer = new byte[fileStream.Length];
			fileStream.Read(buffer, 0, (int)fileStream.Length);
			text = Encoding.UTF8.GetString(buffer);
			sourceCodeManager.CloseFile(stream);
			Assert.IsTrue(text.Length > 10);

			//Now do the same thing using the 'Open As Text' option

			//Now verify that we can get the latest revision of this file (the actual file content)
			text = sourceCodeManager.OpenFileAsText("/Root/Code/SpiraTestListener.java", null, branchName);
			Assert.IsTrue(text.Length > 10);

			//Now verify that we can get a specific revision of a file
			text = sourceCodeManager.OpenFileAsText("/Root/Code/SpiraTestListener.java", "0005", branchName);
			Assert.IsTrue(text.Length > 10);

			//Next using a different branch
			branchName = "feature/sprint-3";

			//Lets retrieve some files in a folder
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("/Root/Code", branchName);

			//Set defaults, first..
			sortField = SourceCodeManager.FIELD_NAME;
			sortAsc = true;

			//Now, send our query..
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(7, totalCount);
			Assert.AreEqual(7, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("package-info.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/package-info.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[0].AuthorName);
			//File 3
			Assert.AreEqual("SpiraTestConfiguration.java", sourceCodeFiles[2].Name);
			Assert.AreEqual("/Root/Code/SpiraTestConfiguration.java", sourceCodeFiles[2].FileKey);
			Assert.AreEqual("0003", sourceCodeFiles[2].RevisionKey);
			Assert.AreEqual("rev0003", sourceCodeFiles[2].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[2].AuthorName);

			//Now lets try sorting by another field
			sortField = SourceCodeManager.FIELD_COMMIT;
			sortAsc = false;
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(7, totalCount);
			Assert.AreEqual(7, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("SpiraTestCase.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/SpiraTestCase.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0016", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0016", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("Roger Ramjet", sourceCodeFiles[0].AuthorName);
			//File 3
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFiles[2].Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFiles[2].FileKey);
			Assert.AreEqual("0016", sourceCodeFiles[2].RevisionKey);
			Assert.AreEqual("rev0016", sourceCodeFiles[2].RevisionName);
			Assert.AreEqual("Roger Ramjet", sourceCodeFiles[2].AuthorName);

			//Try filtering by file name
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_NAME, "Spira");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				filters,
				out checkBranch,
				out totalCount);
			//Verify
			Assert.AreEqual(4, totalCount);
			Assert.AreEqual(4, sourceCodeFiles.Count);

			//Try filtering by author
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_AUTHOR, "Fred");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				filters,
				out checkBranch,
				out totalCount);
			//Verify
			Assert.AreEqual(2, totalCount);
			Assert.AreEqual(2, sourceCodeFiles.Count);

			//Test that we can get files in the root folder as well
			sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey("test://MyRepository", branchName);
			sourceCodeFiles = sourceCodeManager.RetrieveFilesByFolderId(sourceCodeFolder.FolderId,
				sortField,
				sortAsc,
				((numPerPage * (pageNo - 1)) + 1),
				numPerPage,
				null,
				out checkBranch,
				out totalCount);

			//Verify
			Assert.AreEqual(2, totalCount);
			Assert.AreEqual(2, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("README.md", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/README.md", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[0].AuthorName);
			//File 3
			Assert.AreEqual(".gitignore", sourceCodeFiles[1].Name);
			Assert.AreEqual("/Root/.gitignore", sourceCodeFiles[1].FileKey);
			Assert.AreEqual("0000", sourceCodeFiles[1].RevisionKey);
			Assert.AreEqual("rev0000", sourceCodeFiles[1].RevisionName);
			Assert.AreEqual("System Administrator", sourceCodeFiles[1].AuthorName);

			//Next lets view files associated with a revision, sorted by name ascending
			sortField = SourceCodeManager.FIELD_NAME;
			sortAsc = true;
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0000",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(31, totalCount);
			Assert.AreEqual(15, sourceCodeFiles.Count);

			//Lets try another revision
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0001",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(3, totalCount);
			Assert.AreEqual(3, sourceCodeFiles.Count);
			//File 1
			Assert.AreEqual("SpiraTestConfiguration.java", sourceCodeFiles[0].Name);
			Assert.AreEqual("/Root/Code/SpiraTestConfiguration.java", sourceCodeFiles[0].FileKey);
			Assert.AreEqual("0003", sourceCodeFiles[0].RevisionKey);
			Assert.AreEqual("rev0003", sourceCodeFiles[0].RevisionName);
			Assert.AreEqual("Fred Bloggs", sourceCodeFiles[0].AuthorName);
			//File 2
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFiles[1].Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFiles[1].FileKey);
			Assert.AreEqual("0016", sourceCodeFiles[1].RevisionKey);
			Assert.AreEqual("rev0016", sourceCodeFiles[1].RevisionName);
			Assert.AreEqual("Roger Ramjet", sourceCodeFiles[1].AuthorName);

			//Now lets try filtering by latest revision
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_COMMIT, "0003");
			sourceCodeFiles = sourceCodeManager.RetrieveFilesForRevision("0000",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
			   out totalCount
				);

			//Verify
			Assert.AreEqual(31, totalCount);
			Assert.AreEqual(1, sourceCodeFiles.Count);

			//Now verify that we can get a file by its key
			sourceCodeFile = sourceCodeManager.RetrieveFileByKey("/Root/Code/SpiraTestListener.java", branchName);

			//Verify
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFile.Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFile.FileKey);
			Assert.AreEqual("0016", sourceCodeFile.RevisionKey);
			Assert.AreEqual("rev0016", sourceCodeFile.RevisionName);
			Assert.AreEqual("Roger Ramjet", sourceCodeFile.AuthorName);

			//Now verify that we can get the file by its cache ID
			sourceCodeFile = sourceCodeManager.RetrieveFileById(sourceCodeFile.FileId, out testBranchName);
			Assert.AreEqual(branchName, testBranchName);

			//Verify
			Assert.AreEqual("SpiraTestListener.java", sourceCodeFile.Name);
			Assert.AreEqual("/Root/Code/SpiraTestListener.java", sourceCodeFile.FileKey);
			Assert.AreEqual("0016", sourceCodeFile.RevisionKey);
			Assert.AreEqual("rev0016", sourceCodeFile.RevisionName);
			Assert.AreEqual("Roger Ramjet", sourceCodeFile.AuthorName);

			//Test that we can count the files in the revisions
			fileCount = sourceCodeManager.CountFilesForRevision("0000", branchName);
			Assert.AreEqual(31, fileCount);
			fileCount = sourceCodeManager.CountFilesForRevision("0001", branchName);
			Assert.AreEqual(3, fileCount);

			//Now verify that we can get the latest revision of this file (the actual file content)
			stream = sourceCodeManager.OpenFile("/Root/Code/SpiraTestListener.java", null, branchName);
			fileStream = stream.DataStream;
			buffer = new byte[fileStream.Length];
			fileStream.Read(buffer, 0, (int)fileStream.Length);
			text = Encoding.UTF8.GetString(buffer);
			sourceCodeManager.CloseFile(stream);
			Assert.IsTrue(text.Length > 10);

			//Now verify that we can get a specific revision of a file
			stream = sourceCodeManager.OpenFile("/Root/Code/SpiraTestListener.java", "0005", branchName);
			fileStream = stream.DataStream;
			buffer = new byte[fileStream.Length];
			fileStream.Read(buffer, 0, (int)fileStream.Length);
			text = Encoding.UTF8.GetString(buffer);
			sourceCodeManager.CloseFile(stream);
			Assert.IsTrue(text.Length > 10);

			//Now do the same thing using the 'Open As Text' option

			//Now verify that we can get the latest revision of this file (the actual file content)
			text = sourceCodeManager.OpenFileAsText("/Root/Code/SpiraTestListener.java", null, branchName);
			Assert.IsTrue(text.Length > 10);

			//Now verify that we can get a specific revision of a file
			text = sourceCodeManager.OpenFileAsText("/Root/Code/SpiraTestListener.java", "0005", branchName);
			Assert.IsTrue(text.Length > 10);
		}

		/// <summary>Tests that we can get a list of revisions both for the repository as a whole and also for a
		/// specific file. Need to test filtering, sorting operations as well.</summary>
		[Test]
		[SpiraTestCase(565)]
		public void _03_ViewRevisions()
		{
			//First using the MASTER branch
			string branchName = masterBranch;

			//Set pagnation values..
			int numPerPage = 15;
			int pageNo = 1;
			int totalCount = -1;
			string sortField = SourceCodeManager.FIELD_NAME;
			bool sortAsc = true;

			//Get a list of revisions in the branch, unfiltered, sorted by name ascending
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<SourceCodeCommit> sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(13, totalCount);
			Assert.AreEqual(13, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0000", sourceCommits[0].Name);
			Assert.AreEqual("0000", sourceCommits[0].Revisionkey);
			Assert.AreEqual("Initial upload of code to the repository", sourceCommits[0].Message);
			Assert.AreEqual("System Administrator", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0001", sourceCommits[1].Name);
			Assert.AreEqual("0001", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCommits[1].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[1].AuthorName);

			//Get a list of revisions in the branch, unfiltered, sorted by date descending
			sortField = SourceCodeManager.FIELD_UPDATE_DATE;
			sortAsc = false;
			sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(13, totalCount);
			Assert.AreEqual(13, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0012", sourceCommits[0].Name);
			Assert.AreEqual("0012", sourceCommits[0].Revisionkey);
			Assert.AreEqual("[IN:12] fix quote handling issues throughout and [IN:13] fix the tables get cutoff on low-res modes", sourceCommits[0].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0011", sourceCommits[1].Name);
			Assert.AreEqual("0011", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[IN:10] fix doesn't let me add a new category and [IN:11] fix validation on the edit book page", sourceCommits[1].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[1].AuthorName);

			//Get a list of revisions in the branch, filtered by message, sorted by date descending
			sortField = SourceCodeManager.FIELD_UPDATE_DATE;
			sortAsc = false;
			Hashtable filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_MESSAGE, "fix");
			sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(6, totalCount);
			Assert.AreEqual(6, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0012", sourceCommits[0].Name);
			Assert.AreEqual("0012", sourceCommits[0].Revisionkey);
			Assert.AreEqual("[IN:12] fix quote handling issues throughout and [IN:13] fix the tables get cutoff on low-res modes", sourceCommits[0].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0011", sourceCommits[1].Name);
			Assert.AreEqual("0011", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[IN:10] fix doesn't let me add a new category and [IN:11] fix validation on the edit book page", sourceCommits[1].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[1].AuthorName);

			//Now lets test getting the revisions for a specific file, sorted by date descending
			sourceCommits = sourceCodeManager.RetrieveRevisionsForFile(
				"/Root/Code/SpiraTestConfiguration.java",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(3, totalCount);
			Assert.AreEqual(3, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0003", sourceCommits[0].Name);
			Assert.AreEqual("0003", sourceCommits[0].Revisionkey);
			Assert.AreEqual("Fixes [IN:7] and [IN:8] and implements requirement [RQ:5].", sourceCommits[0].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0001", sourceCommits[1].Name);
			Assert.AreEqual("0001", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCommits[1].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[1].AuthorName);

			//Item 3
			Assert.AreEqual("rev0000", sourceCommits[2].Name);
			Assert.AreEqual("0000", sourceCommits[2].Revisionkey);
			Assert.AreEqual("Initial upload of code to the repository", sourceCommits[2].Message);
			Assert.AreEqual("System Administrator", sourceCommits[2].AuthorName);

			//First using a different branch
			branchName = "feature/sprint-3";

			//Set pagnation values..
			sortField = SourceCodeManager.FIELD_NAME;
			sortAsc = true;

			//Get a list of revisions in the branch, unfiltered, sorted by name ascending
			sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(15, totalCount);
			Assert.AreEqual(15, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0000", sourceCommits[0].Name);
			Assert.AreEqual("0000", sourceCommits[0].Revisionkey);
			Assert.AreEqual("Initial upload of code to the repository", sourceCommits[0].Message);
			Assert.AreEqual("System Administrator", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0001", sourceCommits[1].Name);
			Assert.AreEqual("0001", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCommits[1].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[1].AuthorName);

			//Get a list of revisions in the branch, unfiltered, sorted by date descending
			sortField = SourceCodeManager.FIELD_UPDATE_DATE;
			sortAsc = false;
			sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(15, totalCount);
			Assert.AreEqual(15, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0016", sourceCommits[0].Name);
			Assert.AreEqual("0016", sourceCommits[0].Revisionkey);
			Assert.AreEqual("[RQ:10] Add 'delete all' button to book list screen [TK:18]", sourceCommits[0].Message);
			Assert.AreEqual("Roger Ramjet", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0015", sourceCommits[1].Name);
			Assert.AreEqual("0015", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[RQ:17] Create subject object insert method [TK:38]", sourceCommits[1].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[1].AuthorName);

			//Get a list of revisions in the branch, filtered by message, sorted by date descending
			sortField = SourceCodeManager.FIELD_UPDATE_DATE;
			sortAsc = false;
			filters = new Hashtable();
			filters.Add(SourceCodeManager.FIELD_MESSAGE, "fix");
			sourceCommits = sourceCodeManager.RetrieveRevisions(branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(6, totalCount);
			Assert.AreEqual(6, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0012", sourceCommits[0].Name);
			Assert.AreEqual("0012", sourceCommits[0].Revisionkey);
			Assert.AreEqual("[IN:12] fix quote handling issues throughout and [IN:13] fix the tables get cutoff on low-res modes", sourceCommits[0].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0011", sourceCommits[1].Name);
			Assert.AreEqual("0011", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[IN:10] fix doesn't let me add a new category and [IN:11] fix validation on the edit book page", sourceCommits[1].Message);
			Assert.AreEqual("Donna Marshall", sourceCommits[1].AuthorName);

			//Now lets test getting the revisions for a specific file, sorted by date descending
			sourceCommits = sourceCodeManager.RetrieveRevisionsForFile(
				"/Root/Code/SpiraTestConfiguration.java",
				branchName,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(3, totalCount);
			Assert.AreEqual(3, sourceCommits.Count);

			//Item 1
			Assert.AreEqual("rev0003", sourceCommits[0].Name);
			Assert.AreEqual("0003", sourceCommits[0].Revisionkey);
			Assert.AreEqual("Fixes [IN:7] and [IN:8] and implements requirement [RQ:5].", sourceCommits[0].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[0].AuthorName);

			//Item 2
			Assert.AreEqual("rev0001", sourceCommits[1].Name);
			Assert.AreEqual("0001", sourceCommits[1].Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCommits[1].Message);
			Assert.AreEqual("Fred Bloggs", sourceCommits[1].AuthorName);

			//Item 3
			Assert.AreEqual("rev0000", sourceCommits[2].Name);
			Assert.AreEqual("0000", sourceCommits[2].Revisionkey);
			Assert.AreEqual("Initial upload of code to the repository", sourceCommits[2].Message);
			Assert.AreEqual("System Administrator", sourceCommits[2].AuthorName);

			//Now retrieve a revision by its key
			SourceCodeCommit sourceCodeCommit = sourceCodeManager.RetrieveRevisionByKey("0001");
			Assert.AreEqual("rev0001", sourceCodeCommit.Name);
			Assert.AreEqual("0001", sourceCodeCommit.Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCodeCommit.Message);
			Assert.AreEqual("Fred Bloggs", sourceCodeCommit.AuthorName);

			//Now retrieve a revision by its cache ID
			sourceCodeCommit = sourceCodeManager.RetrieveRevisionById(sourceCodeCommit.RevisionId);
			Assert.AreEqual("rev0001", sourceCodeCommit.Name);
			Assert.AreEqual("0001", sourceCodeCommit.Revisionkey);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", sourceCodeCommit.Message);
			Assert.AreEqual("Fred Bloggs", sourceCodeCommit.AuthorName);
		}

		/// <summary>Test that we can get a list of revisions associated with a Spira artifact and also that we can get 
		/// the list of artifact associations for a specific revision</summary>
		/// <remarks>
		/// This is only testing the associations that are deduced from the tokens in the commit text
		/// </remarks>
		[Test]
		[SpiraTestCase(566)]
		public void _04_ViewRevisionLinkedArtifacts()
		{
			//Get the associations for a specific revision
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0005");

			//Verify Count
			Assert.AreEqual(3, artifactLinks.Count);

			//The sorting is done in memory afterwards, so don't assume any order
			ArtifactLinkView artifactLinkView = artifactLinks.FirstOrDefault(a => a.ArtifactId == 2 && a.ArtifactTypeName == "Task");
			Assert.IsNotNull(artifactLinkView);
			Assert.IsNull(artifactLinkView.ArtifactLinkTypeName);
			Assert.AreEqual("Completes task [TK:2] for requirement [RQ:4] and fixes bug [IN:7].", artifactLinkView.Comment);
			Assert.AreEqual("Create book object insert method", artifactLinkView.ArtifactName);

			artifactLinkView = artifactLinks.FirstOrDefault(a => a.ArtifactId == 4 && a.ArtifactTypeName == "Requirement");
			Assert.IsNotNull(artifactLinkView);
			Assert.IsNull(artifactLinkView.ArtifactLinkTypeName);
			Assert.AreEqual("Completes task [TK:2] for requirement [RQ:4] and fixes bug [IN:7].", artifactLinkView.Comment);
			Assert.AreEqual("Ability to add new books to the system", artifactLinkView.ArtifactName);

			artifactLinkView = artifactLinks.FirstOrDefault(a => a.ArtifactId == 7 && a.ArtifactTypeName == "Incident");
			Assert.IsNotNull(artifactLinkView);
			Assert.IsNull(artifactLinkView.ArtifactLinkTypeName);
			Assert.AreEqual("Completes task [TK:2] for requirement [RQ:4] and fixes bug [IN:7].", artifactLinkView.Comment);
			Assert.AreEqual("Cannot add a new book to the system", artifactLinkView.ArtifactName);

			//Get the reverse list of associations FROM the artifact
			List<SourceCodeCommit> sourceCodeRevisions = sourceCodeManager.RetrieveRevisionsForArtifact(Artifact.ArtifactTypeEnum.Requirement, 4);
			Assert.AreEqual(2, sourceCodeRevisions.Count);
			Assert.IsTrue(sourceCodeRevisions.Any(s => s.Revisionkey == "0001"));
			Assert.IsTrue(sourceCodeRevisions.Any(s => s.Revisionkey == "0005"));
		}

		/// <summary>Tests that we can view the list of providers, add a provider and modify an existing provider</summary>
		[Test]
		[SpiraTestCase(563)]
		public void _05_CreateModifyDeleteProviders()
		{
			//Get the list of existing version control systems
			SourceCodeManager sourceCodeManager = new SourceCodeManager();
			List<VersionControlSystem> versionControlSystems = sourceCodeManager.RetrieveSystems();

			//Verify data returned, since it's changed.
			Assert.AreEqual(1, versionControlSystems.Count);
			VersionControlSystem versionControlSystem = versionControlSystems[0];
			Assert.AreEqual(SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME2, versionControlSystem.Name);
			Assert.AreEqual(true, versionControlSystem.IsActive);
			Assert.AreEqual("This provides the dummy version control provider used in testing", versionControlSystem.Description);
			Assert.AreEqual("test://MyRepository", versionControlSystem.ConnectionString);
			Assert.AreEqual("fredbloggs", versionControlSystem.Login);
			Assert.AreEqual("fredbloggs", versionControlSystem.Password);
			Assert.IsTrue(versionControlSystem.Domain.IsNull());
			Assert.IsTrue(versionControlSystem.Custom01.IsNull());
			Assert.IsTrue(versionControlSystem.Custom02.IsNull());
			Assert.IsTrue(versionControlSystem.Custom03.IsNull());
			Assert.IsTrue(versionControlSystem.Custom04.IsNull());
			Assert.IsTrue(versionControlSystem.Custom05.IsNull());

			//Verify that specific projects use this provider
			List<VersionControlProject> versionControlProjects = sourceCodeManager.RetrieveProjectsForSystem(versionControlSystem.VersionControlSystemId);
			Assert.AreEqual(3, versionControlProjects.Count);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Source Code";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Now create a new provider
			int versionControlSystemId = sourceCodeManager.InsertSystem(
				"TestProvider",
				null,
				true,
				"svn://myserver/repository",
				"login",
				"password",
				"domain",
				"custom01",
				"custom02",
				null,
				null,
				null, 1, adminSectionId, "Inserted Source Code"
				);

			//Verify that it saved OK
			versionControlSystem = sourceCodeManager.RetrieveSystemById(versionControlSystemId);
			Assert.AreEqual("TestProvider", versionControlSystem.Name);
			Assert.AreEqual(true, versionControlSystem.IsActive);
			Assert.IsTrue(versionControlSystem.Description.IsNull());
			Assert.AreEqual("svn://myserver/repository", versionControlSystem.ConnectionString);
			Assert.AreEqual("login", versionControlSystem.Login);
			Assert.AreEqual("password", versionControlSystem.Password);
			Assert.AreEqual("domain", versionControlSystem.Domain);
			Assert.AreEqual("custom01", versionControlSystem.Custom01);
			Assert.AreEqual("custom02", versionControlSystem.Custom02);
			Assert.IsTrue(versionControlSystem.Custom03.IsNull());
			Assert.IsTrue(versionControlSystem.Custom04.IsNull());
			Assert.IsTrue(versionControlSystem.Custom05.IsNull());

			//Now make some changes and update
			versionControlSystem.StartTracking();
			versionControlSystem.Name = "TestProvider2";
			versionControlSystem.IsActive = false;
			versionControlSystem.Description = "Description";
			versionControlSystem.ConnectionString = "svn://myserver/repository2";
			versionControlSystem.Login = "login2";
			versionControlSystem.Password = "password2";
			versionControlSystem.Domain = null;
			versionControlSystem.Custom02 = null;
			versionControlSystem.Custom03 = "custom03";
			sourceCodeManager.UpdateSystem(versionControlSystem);

			//Verify that it saved OK
			Assert.AreEqual("TestProvider2", versionControlSystem.Name);
			Assert.AreEqual(false, versionControlSystem.IsActive);
			Assert.AreEqual("Description", versionControlSystem.Description);
			Assert.AreEqual("svn://myserver/repository2", versionControlSystem.ConnectionString);
			Assert.AreEqual("login2", versionControlSystem.Login);
			Assert.AreEqual("password2", versionControlSystem.Password);
			Assert.IsTrue(versionControlSystem.Domain.IsNull());
			Assert.AreEqual("custom01", versionControlSystem.Custom01);
			Assert.IsTrue(versionControlSystem.Custom02.IsNull());
			Assert.AreEqual("custom03", versionControlSystem.Custom03);
			Assert.IsTrue(versionControlSystem.Custom04.IsNull());
			Assert.IsTrue(versionControlSystem.Custom05.IsNull());

			//We need to add a project settings entry to verify that trying to initialize the provider will fail
			//(since no plug-in exists for it) and that the delete correctly removes such dependent entries at the same time
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();

			string adminSectionName1 = "View / Edit Projects";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;

			int projectId = projectManager.Insert(
				"Test Project",
				1,
				null,
				null,
				true,
				null, 1, adminSectionId1, "Inserted Project");
			int templateId = templateManager.RetrieveForProject(projectId).ProjectTemplateId;
			string adminSectionName2 = "Source Code";
			var adminSection2 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName2);

			int adminSectionId2 = adminSection2.ADMIN_SECTION_ID;
			sourceCodeManager.InsertProjectSettings(versionControlSystemId, projectId, true, null, null, null, null, null, null, null, null, null, 1, adminSectionId2, "Inserted Project Settings in Source Code");

			//Try authenticating with this provider
			bool exceptionThrown = false;
			try
			{
				sourceCodeManager = new SourceCodeManager(projectId);
			}
			catch (SourceCodeProviderLoadingException)
			{
				exceptionThrown = true;
			}
			Assert.AreEqual(true, exceptionThrown, "Loading exception should be thrown");
			//Clean up and delete the new project
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);

			//Finally test that we can delete the new provider
			sourceCodeManager.DeleteSystem(versionControlSystemId, 1);
		}

		/// <summary>Tests that we can associate a provider with a project and override the default settings</summary>
		[Test]
		[SpiraTestCase(569)]
		public void _06_ManageProjectSettings()
		{
			//First lets create a brand-new project
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Source Code";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			TemplateManager templateManager = new TemplateManager();
			string adminSectionName1 = "View / Edit Projects";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;
			int projectId = projectManager.Insert(
				"Test Project",
				1,
				null,
				null,
				true,
				null, 1, adminSectionId1, "Inserted Project"
				);
			int templateId = templateManager.RetrieveForProject(projectId).ProjectTemplateId;

			//Now lets verify that no project settings initially exist for the project
			SourceCodeManager sourceCodeManager = new SourceCodeManager();
			List<VersionControlProject> versionControlProjects = sourceCodeManager.RetrieveProjectSettings(projectId);
			Assert.AreEqual(0, versionControlProjects.Count);

			//Now lets add some project settings for this new project
			sourceCodeManager.InsertProjectSettings(
				VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID,
				projectId,
				true,
				"test://ProjectSystem1/test",
				"login1",
				"password1",
				"domain1",
				"custom01",
				null,
				null,
				null,
				null, 1, adminSectionId, "Inserted Project Settings in Source Code"
				);

			//Verify the data
			versionControlProjects = sourceCodeManager.RetrieveProjectSettings(projectId);
			Assert.AreEqual(1, versionControlProjects.Count);
			Assert.AreEqual(true, versionControlProjects[0].IsActive);
			Assert.AreEqual("test://ProjectSystem1/test", versionControlProjects[0].ConnectionString);
			Assert.AreEqual("login1", versionControlProjects[0].Login);
			Assert.AreEqual("password1", versionControlProjects[0].Password);
			Assert.AreEqual("domain1", versionControlProjects[0].Domain);
			Assert.AreEqual("custom01", versionControlProjects[0].Custom01);
			Assert.IsTrue(versionControlProjects[0].Custom02.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom03.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom04.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom05.IsNull());

			//Make some changes and update
			versionControlProjects[0].StartTracking();
			versionControlProjects[0].ConnectionString = null;
			versionControlProjects[0].Login = null;
			versionControlProjects[0].Password = null;
			versionControlProjects[0].Domain = null;
			versionControlProjects[0].Custom01 = null;
			sourceCodeManager.UpdateProjectSettings(versionControlProjects[0], 1, adminSectionId, "Updated Project Settings in Source Code");

			//Verify the data using the other retrieve method
			VersionControlProject versionControlProject = sourceCodeManager.RetrieveProjectSettings(VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID, projectId);
			Assert.IsNotNull(versionControlProject);
			Assert.AreEqual(true, versionControlProjects[0].IsActive);
			Assert.IsTrue(versionControlProjects[0].ConnectionString.IsNull());
			Assert.IsTrue(versionControlProjects[0].Login.IsNull());
			Assert.IsTrue(versionControlProjects[0].Password.IsNull());
			Assert.IsTrue(versionControlProjects[0].Domain.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom01.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom02.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom03.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom04.IsNull());
			Assert.IsTrue(versionControlProjects[0].Custom05.IsNull());

			//Verify that we can delete the project and it removes the version control settings
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);
		}

		/// <summary>Tests that we can authenticate with a provider and that it will send back the expected exception if not</summary>
		[Test]
		[SpiraTestCase(570)]
		public void _07_AuthenticateWithProvider()
		{
			//First we need to create a new project that we can modify the settings for
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Source Code";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			string adminSectionName1 = "View / Edit Projects";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;

			int projectId = projectManager.Insert(
				"Test Project",
				1,
				null,
				null,
				true,
				null, 1, adminSectionId1, "Inserted Project"
				);
			int templateId = templateManager.RetrieveForProject(projectId).ProjectTemplateId;

			//Now turn on source control for this project using the default settings
			SourceCodeManager sourceCodeManager = new SourceCodeManager();
			sourceCodeManager.InsertProjectSettings(
				VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID,
				projectId,
				true,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null, 1, adminSectionId, "Inserted Project Settings in Source Code"
				);

			//Now try authenticating against this project
			sourceCodeManager = new SourceCodeManager(projectId);
			//Verify that it succeeded
			Assert.AreEqual(SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME2, sourceCodeManager.RepositoryName);

			//Now lets change the login to an incorrect one in the project settings and try again
			VersionControlProject versionControlProject = sourceCodeManager.RetrieveProjectSettings(VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID, projectId);
			versionControlProject.StartTracking();
			versionControlProject.Login = "wrong";
			sourceCodeManager.UpdateProjectSettings(versionControlProject, 1, adminSectionId, "Updated Project Settings in Source Code");

			//Now try authenticating again and check that it fails with the expected exception
			bool exceptionThrown = false;
			try
			{
				sourceCodeManager = new SourceCodeManager(projectId);
			}
			catch (SourceCodeProviderAuthenticationException)
			{
				exceptionThrown = true;
			}
			Assert.AreEqual(true, exceptionThrown, "Authentication exception should be thrown");

			//Now we need to verify that changing the connection string throws a provider loading error
			versionControlProject = sourceCodeManager.RetrieveProjectSettings(VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID, projectId);
			versionControlProject.StartTracking();
			versionControlProject.Login = "fredbloggs";
			versionControlProject.ConnectionString = "wrong://";
			sourceCodeManager.UpdateProjectSettings(versionControlProject, 1, adminSectionId, "Updated Project Settings in Source Code");

			//Now try authenticating/connecting again
			exceptionThrown = false;
			try
			{
				sourceCodeManager = new SourceCodeManager(projectId);
			}
			catch (SourceCodeProviderLoadingException)
			{
				exceptionThrown = true;
			}
			Assert.AreEqual(true, exceptionThrown, "Loading exception should be thrown");

			//Now we need to verify that setting the status to inactive throws a provider loading error
			versionControlProject = sourceCodeManager.RetrieveProjectSettings(VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID, projectId);
			versionControlProject.StartTracking();
			versionControlProject.IsActive = false;
			versionControlProject.ConnectionString = "test://MyRepository";
			sourceCodeManager.UpdateProjectSettings(versionControlProject, 1, adminSectionId, "Updated Project Settings in Source Code");

			//Now try authenticating/connecting again
			exceptionThrown = false;
			try
			{
				sourceCodeManager = new SourceCodeManager(projectId);
			}
			catch (SourceCodeProviderLoadingException)
			{
				exceptionThrown = true;
			}
			Assert.AreEqual(true, exceptionThrown, "Loading exception should be thrown");

			//Clean up and delete the new project
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);
		}

		/// <summary>Tests that you can associate source code files with SpiraTeam artifacts</summary>
		[Test]
		[SpiraTestCase(568)]
		public void _08_LinkFilesToArtifacts()
		{
			//First test that we can retrieve a list of source code files associated with a specific requirement and task
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<SourceCodeFile> files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Requirement, 4);
			Assert.AreEqual(0, files.Count);
			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Task, 1);
			Assert.AreEqual(0, files.Count);

			//There are no files initially, so we need to add some
			int sourceCodeAssociation1 = sourceCodeManager.AddFileAssociation(SOURCE_CODE_PROJECT_ID, "/Root/Samples/SampleFile.bat", Artifact.ArtifactTypeEnum.Requirement, 4, DateTime.UtcNow, masterBranch, "This file would be useful for this requirement");
			int sourceCodeAssociation2 = sourceCodeManager.AddFileAssociation(SOURCE_CODE_PROJECT_ID, "/Root/Samples/SampleFile.cpp", Artifact.ArtifactTypeEnum.Task, 1, DateTime.UtcNow, masterBranch, "This file would be useful for this task");
			int sourceCodeAssociation3 = sourceCodeManager.AddFileAssociation(SOURCE_CODE_PROJECT_ID, "/Root/Code/SpiraTestConfiguration.java", Artifact.ArtifactTypeEnum.Requirement, 4, DateTime.UtcNow, masterBranch, null);

			//Verify
			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Requirement, 4);
			Assert.AreEqual(2, files.Count);
			SourceCodeFile file = files.FirstOrDefault(f => f.FileKey == "/Root/Code/SpiraTestConfiguration.java");
			Assert.AreEqual("SpiraTestConfiguration.java", file.Name);
			Assert.AreEqual("Fred Bloggs", file.AuthorName);

			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Task, 1);
			Assert.AreEqual(1, files.Count);
			file = files.FirstOrDefault(f => f.FileKey == "/Root/Samples/SampleFile.cpp");
			Assert.AreEqual("SampleFile.cpp", file.Name);
			Assert.AreEqual("System Administrator", file.AuthorName);

			//Now test that we can filter the results
			Hashtable filters = new Hashtable();
			filters.Add("Filename", "SampleFile");
			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Requirement, 4, filters);
			Assert.AreEqual(1, files.Count);
			file = files.FirstOrDefault(f => f.FileKey == "/Root/Code/SpiraTestConfiguration.java");
			Assert.IsNull(file);
			file = files.FirstOrDefault(f => f.FileKey == "/Root/Samples/SampleFile.bat");
			Assert.AreEqual("SampleFile.bat", file.Name);
			Assert.AreEqual("System Administrator", file.AuthorName);

			//Now test from the opposite direction
			List<ArtifactLinkView> associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Samples/SampleFile.bat", masterBranch);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual("Requirement", associations[0].ArtifactTypeName);
			Assert.AreEqual("Ability to add new books to the system", associations[0].ArtifactName);
			Assert.AreEqual("This file would be useful for this requirement", associations[0].Comment);
			associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Samples/SampleFile.cpp", masterBranch);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual("Task", associations[0].ArtifactTypeName);
			Assert.AreEqual("Develop new book entry screen", associations[0].ArtifactName);
			Assert.AreEqual("This file would be useful for this task", associations[0].Comment);
			associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Code/SpiraTestConfiguration.java", masterBranch);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual("Requirement", associations[0].ArtifactTypeName);
			Assert.AreEqual("Ability to add new books to the system", associations[0].ArtifactName);
			Assert.IsNull(associations[0].Comment);

			//Verify that we can retrieve an association by its unique ID
			ArtifactLink association = sourceCodeManager.RetrieveFileAssociation(SOURCE_CODE_PROJECT_ID, sourceCodeAssociation2);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Task, association.DestArtifactTypeId);
			Assert.AreEqual(1, association.DestArtifactId);
			Assert.AreEqual("This file would be useful for this task", association.Comment);

			//And the alternate method that retrieves the link object
			ArtifactSourceCodeFile artifactSourceCodeFile = sourceCodeManager.RetrieveFileAssociation2(SOURCE_CODE_PROJECT_ID, sourceCodeAssociation2);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Task, artifactSourceCodeFile.ArtifactTypeId);
			Assert.AreEqual(1, artifactSourceCodeFile.ArtifactId);
			Assert.AreEqual("This file would be useful for this task", artifactSourceCodeFile.Comment);

			//Update one of the associations
			sourceCodeManager.UpdateFileAssociation(SOURCE_CODE_PROJECT_ID, sourceCodeAssociation2, "Updated comment");

			//Verify
			association = sourceCodeManager.RetrieveFileAssociation(SOURCE_CODE_PROJECT_ID, sourceCodeAssociation2);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Task, association.DestArtifactTypeId);
			Assert.AreEqual(1, association.DestArtifactId);
			Assert.AreEqual("Updated comment", association.Comment);

			//Delete the various associations
			sourceCodeManager.RemoveFileAssociation(sourceCodeAssociation1);
			sourceCodeManager.RemoveFileAssociation(sourceCodeAssociation2);
			sourceCodeManager.RemoveFileAssociation(sourceCodeAssociation3);

			//Verify
			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Requirement, 4);
			Assert.AreEqual(0, files.Count);
			files = sourceCodeManager.RetrieveFilesForArtifact(DataModel.Artifact.ArtifactTypeEnum.Task, 1);
			Assert.AreEqual(0, files.Count);
			associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Samples/SampleFile.bat", masterBranch);
			Assert.AreEqual(0, associations.Count);
			associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Samples/SampleFile.cpp", masterBranch);
			Assert.AreEqual(0, associations.Count);
			associations = sourceCodeManager.RetrieveAssociationsForFile("/Root/Code/SpiraTestConfiguration.java", masterBranch);
			Assert.AreEqual(0, associations.Count);
		}

		/// <summary>Tests that you can associate source code revisions with SpiraTeam builds</summary>
		[Test]
		[SpiraTestCase(811)]
		public void _09_LinkRevisionsToBuilds()
		{
			//The build ID
			const int BUILD_ID = 1;

			//Set pagnation values..
			int numPerPage = 15;
			int pageNo = 1;
			int totalCount = -1;
			string sortField = "Name";
			bool sortAsc = true;

			//We already have some revisions and builds in the sample project, so just test retreiving them
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<SourceCodeCommit> commits = sourceCodeManager.RetrieveRevisionsForBuild(
				BUILD_ID,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				null,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(1, commits.Count);
			Assert.AreEqual(1, totalCount);
			Assert.AreEqual("rev0001", commits[0].Name);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", commits[0].Message);
			Assert.AreEqual(true, commits[0].ContentChanged);
			Assert.AreEqual(true, commits[0].PropertiesChanged);

			//Use a filter
			Hashtable filters = new Hashtable();
			filters.Add("Message", "book");
			commits = sourceCodeManager.RetrieveRevisionsForBuild(
				BUILD_ID,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(1, commits.Count);
			Assert.AreEqual(1, totalCount);
			Assert.AreEqual("rev0001", commits[0].Name);
			Assert.AreEqual("[RQ:4] Develop new book entry screen [TK:1] and Create book object insert method [TK:2]", commits[0].Message);
			Assert.AreEqual(true, commits[0].ContentChanged);
			Assert.AreEqual(true, commits[0].PropertiesChanged);

			//Use a filter
			filters = new Hashtable();
			filters.Add("Message", "bookzz");
			commits = sourceCodeManager.RetrieveRevisionsForBuild(
				BUILD_ID,
				sortField,
				sortAsc,
				(((pageNo - 1) * numPerPage) + 1),
				numPerPage,
				filters,
				InternalRoutines.UTC_OFFSET,
				out totalCount);

			//Verify
			Assert.AreEqual(0, commits.Count);
			Assert.AreEqual(0, totalCount);
		}

		/// <summary>
		/// Tests that you can associate source code revisions with SpiraTeam artifacts inside SpiraTeam.
		/// Unlike test _04_ViewRevisionLinkedArtifacts that is for testing the case where the link was
		/// made using the commit text and the special token syntax
		/// </summary>
		[Test]
		[SpiraTestCase(1309)]
		public void _10_ManageRevisionAssociations()
		{
			//Get a list of associations with a revision..
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0004");
			Assert.AreEqual(4, artifactLinks.Count);
			//[TK:24]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 24));
			//[TK:25]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 25));
			//[TK:26]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 26));
			//[RQ:15]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 15));

			//Verify the reverse direction (for [RQ:15])
			List<SourceCodeCommit> revisions = sourceCodeManager.RetrieveRevisionsForArtifact(Artifact.ArtifactTypeEnum.Requirement, 15);
			Assert.AreEqual(1, revisions.Count);
			Assert.IsTrue(revisions.Any(r => r.Revisionkey == "0004"));

			//Now we add some additional associations
			int artifactSourceCodeId1 = sourceCodeManager.AddRevisionAssociation(SOURCE_CODE_PROJECT_ID, "0012", Artifact.ArtifactTypeEnum.Incident, 3, DateTime.UtcNow, "Added in Spira1");
			int artifactSourceCodeId2 = sourceCodeManager.AddRevisionAssociation(SOURCE_CODE_PROJECT_ID, "0004", Artifact.ArtifactTypeEnum.Requirement, 5, DateTime.UtcNow, "Added in Spira2");

			//Verify the changes
			artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0012");
			Assert.AreEqual(3, artifactLinks.Count);
			//[IN:3]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 3));
			//[IN:12]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 12));
			//[IN:13]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 13));

			artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0004");
			Assert.AreEqual(5, artifactLinks.Count);
			//[TK:24]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 24));
			//[TK:25]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 25));
			//[TK:26]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 26));
			//[RQ:15]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 15));
			//[RQ:5]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 5));
			Assert.AreEqual("Added in Spira2", artifactLinks.FirstOrDefault(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 5).Comment);

			//Verify the reverse directions as well
			revisions = sourceCodeManager.RetrieveRevisionsForArtifact(Artifact.ArtifactTypeEnum.Requirement, 5);
			Assert.AreEqual(2, revisions.Count);
			Assert.IsTrue(revisions.Any(r => r.Revisionkey == "0003"));
			Assert.IsTrue(revisions.Any(r => r.Revisionkey == "0004"));

			//Verify we can retrieve the association record directly
			ArtifactLink association = sourceCodeManager.RetrieveRevisionAssociation(SOURCE_CODE_PROJECT_ID, artifactSourceCodeId1);
			Assert.AreEqual("Added in Spira1", association.Comment);

			//Update the association
			sourceCodeManager.UpdateRevisionAssociation(SOURCE_CODE_PROJECT_ID, artifactSourceCodeId1, "Added in Spira1b");

			//Verify
			association = sourceCodeManager.RetrieveRevisionAssociation(SOURCE_CODE_PROJECT_ID, artifactSourceCodeId1);
			Assert.AreEqual("Added in Spira1b", association.Comment);

			//Remove the associations
			sourceCodeManager.RemoveRevisionAssociation(artifactSourceCodeId1);
			sourceCodeManager.RemoveRevisionAssociation(artifactSourceCodeId2);

			//Verify the changes
			artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0004");
			Assert.AreEqual(4, artifactLinks.Count);
			//[TK:24]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 24));
			//[TK:25]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 25));
			//[TK:26]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 26));
			//[RQ:15]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 15));
			artifactLinks = sourceCodeManager.RetrieveAssociationsForRevision("0012");
			Assert.AreEqual(2, artifactLinks.Count);
			//[IN:12]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 12));
			//[IN:13]
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 13));
		}

		/// <summary>
		/// Tests that you can view the artifacts associated with a build (through the commited revisions)
		/// </summary>
		[Test]
		[SpiraTestCase(1310)]
		public void _11_ViewBuildArtifactAssociations()
		{
			const int BUILD_ID = 4;
			//Get the list of artifacts associated with build BL:4
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			List<ArtifactLinkView> artifactLinks = sourceCodeManager.RetrieveAssociationsForBuild(SOURCE_CODE_PROJECT_ID, BUILD_ID);
			Assert.AreEqual(7, artifactLinks.Count);

			//TK:2, IN:7, RQ:15, TK:24
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 2));
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && a.ArtifactId == 7));
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == 15));
			Assert.IsTrue(artifactLinks.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == 24));

			//The reverse (Artifacts > Builds) are tested in the BuildManager unit test
		}

		/// <summary>Tests that an account with the TaraVault servers can be set up. (Note this does not actually 
		/// test any communication to the TaraVault hosts.</summary>
		[Test]
		[SpiraTestCase(1318)]
		public void _20_TaraSetupAccount()
		{
			//Our manager..
			VaultManager vaultManager = new VaultManager();

			//Deactivate TaraVault, even though it may already BE deactivated.
			vaultManager.Account_Deactivate();

			//Check settings..
			ConfigurationSettings.Default.Reload();

			//Next initialize TaraVault with 10 licenses, normally done by website
			ConfigurationSettings.Default.TaraVault_UserLicense = 10;
			ConfigurationSettings.Default.Save();

			Assert.AreEqual(10, ConfigurationSettings.Default.TaraVault_UserLicense, "# User Licenses did not match!");
			Assert.AreEqual(false, ConfigurationSettings.Default.TaraVault_HasAccount, "Account active did not match!");
			Assert.AreEqual("", ConfigurationSettings.Default.TaraVault_AccountName, "Account name did not match!");

			//Activate TaraVault..
			vaultManager.Account_Activate(false);
			//Check settings.
			//The number of licenses is set by the Inflectra website and so will remain at zero
			Assert.AreEqual(10, ConfigurationSettings.Default.TaraVault_UserLicense, "# User Licenses did not match!");
			Assert.AreEqual(true, ConfigurationSettings.Default.TaraVault_HasAccount, "Account active did not match!");
			Assert.AreEqual("localhost", ConfigurationSettings.Default.TaraVault_AccountName, "Account name did not match!");
		}

		/// <summary>Runs tests that creates uses in TaraVault system.</summary>
		[Test]
		[SpiraTestCase(1319)]
		public void _21_TaraSetupUsers()
		{
			//Set up a project with TaraVault settings..
			VaultManager vaultManager = new VaultManager();

			//First get user #1 and verify that they're not already a taravault user..
			User user1 = vaultManager.User_RetrieveWithTaraVault(USER_ID_TEST1);
			Assert.IsNull(user1.TaraVault, "User had a TaraVault definition!");

			//Now create the user & verify it..
			vaultManager.User_CreateUpdateTaraAccount(USER_ID_TEST1, "nunit_test", "test_password");
			user1 = vaultManager.User_RetrieveWithTaraVault(USER_ID_TEST1);
			Assert.IsNotNull(user1.TaraVault, "User did not had a TaraVault definition!");
			Assert.AreEqual("2", user1.TaraVault.VaultUserId, "User's Tara Login did not match!");
			Assert.AreEqual(true, user1.TaraVault.IsActive, "User wasn't marked Active!");
			Assert.AreEqual(new SimpleAES().EncryptToString("test_password"), user1.TaraVault.Password, "User's password did not match!");

			//Remove the user from TaraVault.
			vaultManager.User_RemoveTaraAccount(USER_ID_TEST1);
			user1 = vaultManager.User_RetrieveWithTaraVault(USER_ID_TEST1);
			Assert.IsNull(user1.TaraVault, "User had a TaraVault definition!");

			//Now recreate users, for using in the next test.
			vaultManager.User_CreateUpdateTaraAccount(USER_ID_TEST1, "nunit_test", "test_password");
			vaultManager.User_CreateUpdateTaraAccount(USER_ID_TEST2, "nunit_test2", "test_password");
		}

		[Test]
		[SpiraTestCase(1320)]
		public void _22_TaraSetupProjectUsers()
		{
			//Set up a project with TaraVault settings..
			VaultManager vaultManager = new VaultManager();

			//Create the project..
			TaraVaultProject proj = vaultManager.Project_CreateUpdateTaraVault(taraVaultProjectId, VaultManager.VaultTypeEnum.Subversion, "nunit_test");
			Assert.AreEqual("nunit_test", proj.Name, "Project name did not match!");
			Assert.AreEqual(taraVaultProjectId, proj.ProjectId, "Project ID did not match!");
			Assert.AreEqual(1, proj.VaultTypeId, "Project Type did not match!");

			//Add our two users to the project..
			vaultManager.User_AddToTaraVaultProject(USER_ID_TEST1, taraVaultProjectId);
			vaultManager.User_AddToTaraVaultProject(USER_ID_TEST2, taraVaultProjectId);

			//Check that they were properly assigned..
			List<Project> projs = vaultManager.User_RetrieveTaraProjectsForId(USER_ID_TEST1);
			Assert.AreEqual(1, projs.Count, "Project did not return '1'!");
			Assert.AreEqual("nunit_test", projs[0].TaraVault.Name, "Returned project name did not match!");
			Assert.AreEqual((int)VaultManager.VaultTypeEnum.Subversion, projs[0].TaraVault.VaultTypeId, "Returned project type did not match!");
			Assert.AreEqual(taraVaultProjectId, projs[0].TaraVault.ProjectId, "Returned project ID did not match!");
			projs = vaultManager.User_RetrieveTaraProjectsForId(USER_ID_TEST1);
			Assert.AreEqual(1, projs.Count, "Project did not return '1'!");
			Assert.AreEqual("nunit_test", projs[0].TaraVault.Name, "Returned project name did not match!");
			Assert.AreEqual((int)VaultManager.VaultTypeEnum.Subversion, projs[0].TaraVault.VaultTypeId, "Returned project type did not match!");
			Assert.AreEqual(taraVaultProjectId, projs[0].TaraVault.ProjectId, "Returned project ID did not match!");

			//Now unassign one user from a projecty.
			vaultManager.User_RemoveFromTaraVaultProject(USER_ID_TEST1, taraVaultProjectId);
			projs = vaultManager.User_RetrieveTaraProjectsForId(USER_ID_TEST1);
			Assert.AreEqual(0, projs.Count, "Returned projects did not equal '0'!");
		}

		[Test]
		[SpiraTestCase(1321)]
		public void _23_TaraRemoveProjectsAndUsers()
		{
			//Set up a project with TaraVault settings..
			VaultManager vaultManager = new VaultManager();

			//Add the first user back, so we can test removing him..
			vaultManager.User_CreateUpdateTaraAccount(USER_ID_TEST1, "nunit_test", "test_password");

			//Now remove the TaraUser. It should also unsubscribe the user from the project.
			vaultManager.User_RemoveTaraAccount(USER_ID_TEST1);
			List<Project> projs = vaultManager.User_RetrieveTaraProjectsForId(USER_ID_TEST1);
			Assert.AreEqual(0, projs.Count, "Expected 0 projects, got " + projs.Count + " back.");

			//Now deactivate the project, and make sure the users get unassigned.
			vaultManager.Project_DeleteTaraVault(taraVaultProjectId);
			projs = vaultManager.User_RetrieveTaraProjectsForId(USER_ID_TEST2);
			Assert.AreEqual(0, projs.Count, "Expected 0 projects, got " + projs.Count + " back.");
		}

		/// <summary>
		/// Tests that we can get the DIFF of two text files
		/// </summary>
		[Test]
		[SpiraTestCase(2564)]
		public void _12_CreateDiffOfTwoFiles()
		{
			//First get the side by side DIFF for two files that are the same
			SourceCodeManager sourceCodeManager = new SourceCodeManager(SOURCE_CODE_PROJECT_ID);
			SideBySideDiffModel sideBySideDiffModel = sourceCodeManager.GenerateSideBySideDiffBetweenFileRevisions("/Root/Code/SpiraTestConfiguration.java", "0000", "0000", masterBranch);
			Assert.IsFalse(sideBySideDiffModel.NewText.HasDifferences);
			Assert.IsFalse(sideBySideDiffModel.OldText.HasDifferences);
			Assert.AreEqual(21, sideBySideDiffModel.NewText.Lines.Count);
			Assert.AreEqual(21, sideBySideDiffModel.OldText.Lines.Count);

			//Now for two files that are different
			sideBySideDiffModel = sourceCodeManager.GenerateSideBySideDiffBetweenFileRevisions("/Root/Code/SpiraTestConfiguration.java", "0001", "0000", masterBranch);
			Assert.IsTrue(sideBySideDiffModel.NewText.HasDifferences);
			Assert.IsTrue(sideBySideDiffModel.OldText.HasDifferences);
			Assert.AreEqual(27, sideBySideDiffModel.NewText.Lines.Count);
			Assert.AreEqual(27, sideBySideDiffModel.OldText.Lines.Count);

			//First get the inline DIFF for two files that are the same
			DiffPaneModel unifiedDiffModel = sourceCodeManager.GenerateUnifiedDiffBetweenFileRevisions("/Root/Code/SpiraTestConfiguration.java", "0000", "0000", masterBranch);
			Assert.IsFalse(unifiedDiffModel.HasDifferences);
			Assert.AreEqual(21, unifiedDiffModel.Lines.Count);

			//Now for two files that are different
			unifiedDiffModel = sourceCodeManager.GenerateUnifiedDiffBetweenFileRevisions("/Root/Code/SpiraTestConfiguration.java", "0001", "0000", masterBranch);
			Assert.IsTrue(unifiedDiffModel.HasDifferences);
			Assert.AreEqual(28, unifiedDiffModel.Lines.Count);
		}
	}

	/// <summary>Sorting comparer for Commits by Name.</summary>
	public class CommitSorter_Name : IComparer<SourceCodeCommit>
	{
		public int Compare(SourceCodeCommit x, SourceCodeCommit y)
		{
			//Sorting by name, so..
			return x.Name.CompareTo(y.Name);
		}
	}

	/// <summary>Sorting comparer for Files by Name.</summary>
	public class FileSorter_Name : IComparer<SourceCodeFile>
	{
		public int Compare(SourceCodeFile x, SourceCodeFile y)
		{
			//Sorting by name, so..
			return x.Name.CompareTo(y.Name);
		}
	}
}
