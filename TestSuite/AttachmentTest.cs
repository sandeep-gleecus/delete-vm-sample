using System;
using System.IO;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the artifact attachment functionality of the Attachment business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class AttachmentTest
	{
		protected static Business.AttachmentManager attachmentManager;
		protected static UnicodeEncoding unicodeEncoding;

		private static int projectId;
		private static int projectTemplateId;

		private const int PROJECT_ID = 1;
		private const int PROJECT_EMPTY_ID = 3;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		[TestFixtureSetUp]
		public void Init()
		{
			attachmentManager = new Business.AttachmentManager();
			unicodeEncoding = new UnicodeEncoding();

			//Create new projects for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("AttachmentTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");
			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary projects and templates
			new ProjectManager().Delete(USER_ID_FRED_BLOGGS, projectId);
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		/// <summary>
		/// Tests the main list retrieves, relies on the sample project
		/// </summary>
		[
		Test,
		SpiraTestCase(135)
		]
		public void _01_Retrieves()
		{
			//Lets test that we can retrieve an individual attachment
			Attachment attachment = attachmentManager.RetrieveById(1);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual("Book Management Functional Spec.doc", attachment.Filename);
			Assert.AreEqual("This document outlines the functional specification for the book management part of the library management system.", attachment.Description);
			//Assert.IsTrue(attachment.UploadDate >= DateTime.UtcNow.AddDays(-151));
			Assert.AreEqual(285, attachment.Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachment.AttachmentTypeId);

			//Test that we can get the lookups if we use the view
			ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(PROJECT_ID, 1);
			Assert.AreEqual("Book Management Functional Spec.doc", projectAttachment.Filename);
			Assert.AreEqual("This document outlines the functional specification for the book management part of the library management system.", projectAttachment.Description);
			//Assert.IsTrue(projectAttachment.UploadDate >= DateTime.UtcNow.AddDays(-151));
			Assert.AreEqual(285, projectAttachment.Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, projectAttachment.AttachmentTypeId);
			Assert.AreEqual("File", projectAttachment.AttachmentTypeName);
			Assert.AreEqual("Fred Bloggs", projectAttachment.AuthorName);

			//Lets test that we can retrieve the attachments associated with a requirement
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "UploadDate", true, 1, Int32.MaxValue, null, 0);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(4, attachments.Count);
			Assert.AreEqual("Book Management Functional Spec.doc", attachments[0].Filename);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual("This document outlines the functional specification for the book management part of the library management system.", attachments[0].Description);
			//Assert.IsTrue(attachments[0].UploadDate >= DateTime.UtcNow.AddDays(-151));
			Assert.AreEqual(285, attachments[0].Size);
			Assert.AreEqual("Graphical Design Mockups.psd", attachments[2].Filename);
			Assert.AreEqual("http://www.inflectra.com", attachments[3].Filename);
			Assert.AreEqual("Book Management Screen Wireframe.ai", attachments[1].Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachments[3].AttachmentTypeId);
			Assert.AreEqual("URL", attachments[3].AttachmentTypeName);

			//Lets test that we can retrieve the attachments associated with a test case
			attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, 2, DataModel.Artifact.ArtifactTypeEnum.TestCase, "UploadDate", false, 1, Int32.MaxValue, null, 0);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("Sequence Diagram for Book Mgt.pdf", attachments[0].Filename);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual("Sequence diagram in UML format that provides additional detail surrounding the book managament use-case / test case", attachments[0].Description);
			//Assert.IsTrue(attachments[0].UploadDate >= DateTime.UtcNow.AddDays(-149));
			Assert.AreEqual(35, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Lets test that we can retrieve the attachments associated with an incident
			attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, 1, DataModel.Artifact.ArtifactTypeEnum.Incident, "UploadDate", false, 1, Int32.MaxValue, null, 0);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(2, attachments.Count);
			Assert.AreEqual("Bug Stack Trace.txt", attachments[0].Filename);
			Assert.AreEqual("Joe P Smith", attachments[0].AuthorName);
			Assert.IsTrue(attachments[0].Description.IsNull());
			//Assert.IsTrue(attachments[0].UploadDate >= DateTime.UtcNow.AddDays(-144));
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual("Error Logging-in Screen-shot.gif", attachments[1].Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[1].AttachmentTypeId);
			Assert.AreEqual("File", attachments[1].AttachmentTypeName);

			//Verify we can retrieve the open attachments for a specific opener
			attachments = attachmentManager.RetrieveOpenByOpenerId(USER_ID_FRED_BLOGGS, null);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(14, attachments.Count, "incorrect count for open docs for Fred Bloggs");

			Assert.AreEqual(46, attachments[0].AttachmentId, "attachmentId was not returned as expected for Fred's 1st open document");
			Assert.AreEqual("Test Plan.md", attachments[0].Filename, "filename was not returned as expected for Fred's 1st open document");
			Assert.AreEqual("Fred Bloggs", attachments[0].EditorName, "Fred was not the editor of his own document");
			Assert.IsTrue(attachments[0].Description.IsNull(), "a description was returned when it should not have been for Fred's 1st open document");
			Assert.AreEqual(1, attachments[0].Size, "incorrect attachment filesize returned");

			Assert.AreEqual(47, attachments[1].AttachmentId, "attachmentId was not returned as expected for Fred's 2nd open document");
			Assert.AreEqual("Test Areas.html", attachments[1].Filename, "filename was not returned as expected for Fred's 2nd open document");
			Assert.AreEqual("Fred Bloggs", attachments[1].EditorName, "Fred was not the editor of his own document");
		}

		[
		Test,
		SpiraTestCase(106)
		]
		public void _02_UploadIncidentAttachment()
		{
			//Create the new incident
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident", "Test Incident", null, null, null, 1, null, DateTime.Now, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			//First verify that the incident has no existing attachments
			Incident incident = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual(false, incident.IsAttachments);

			//Lets try adding and attachment to an incident
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", 2, attachmentData, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.txt", attachments[0].Filename);
			Assert.AreEqual("Test Incident Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Verify that the attachment flag was also updated
			incident = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual(true, incident.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment from the incident and verify
			attachmentManager.Delete(projectId, attachmentId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			incident = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual(false, incident.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Verify that we can attach a document to a Placeholder, then move it to a real incident afterwards
			//this is used to support attaching a document to an unsaved incident
			PlaceholderManager placeholderManager = new PlaceholderManager();
			int placeholderId = placeholderManager.Placeholder_Create(projectId).PlaceholderId;
			attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", USER_ID_FRED_BLOGGS, attachmentData, placeholderId, DataModel.Artifact.ArtifactTypeEnum.Placeholder, null, null, null, null, null);
			attachmentManager.Attachment_Move(USER_ID_FRED_BLOGGS, projectId, placeholderId, DataModel.Artifact.ArtifactTypeEnum.Placeholder, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident);

			//Verify that the attachment moved
			incident = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual(true, incident.IsAttachments);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.txt", attachments[0].Filename);
			Assert.AreEqual("Test Incident Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Now delete the attachment from the project
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);

			//Verify the incident changed back
			incident = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual(false, incident.IsAttachments);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Delete the incident
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(111)
		]
		public void _03_UploadRequirementAttachment()
		{
			//Create a new requirement
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Test Requirement", null, null, USER_ID_FRED_BLOGGS);

			//First verify that the requirement has no existing attachments
			RequirementView requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(false, requirement.IsAttachments);

			//Lets try adding and attachment to a requirement
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "", 3, attachmentData, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.txt", attachments[0].Filename);
			Assert.IsTrue(attachments[0].Description.IsNull());
			Assert.AreEqual("Joe P Smith", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Verify that the attachment flag was also updated
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(true, requirement.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment and verify
			attachmentManager.Delete(projectId, attachmentId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(false, requirement.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Finally delete the requirement
			requirementManager.DeleteFromDatabase(requirementId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(113)
		]
		public void _04_UploadTestCaseAttachment()
		{
			//Create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//First verify that the test case has no existing attachments
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(false, testCase.IsAttachments);

			//Lets try adding and attachment to a test case
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.xls", "Sample Test Case Attachment", 2, attachmentData, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Case Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Verify that the attachment flag was also updated
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(true, testCase.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment and verify
			attachmentManager.Delete(projectId, attachmentId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(false, testCase.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Delete the test case
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(133)
		]
		public void _05_UploadReleaseAttachment()
		{
			//Create a new release and attach an artifact
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Test Release Version 1", null, "1.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			//First verify that the release has no existing attachments
			ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(false, release.IsAttachments);

			//Lets try adding and attachment to a test case
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.xls", "Sample Test Case Attachment", 2, attachmentData, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Case Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Verify that the attachment flag was also updated
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(true, release.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment and verify
			attachmentManager.Delete(projectId, attachmentId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(false, release.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Delete the release
			releaseManager.DeleteFromDatabase(releaseId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(335)
		]
		public void _06_UploadTestStepAttachment()
		{
			//Create a new test case with step
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Test Step With Attachment", null, null);

			//First verify that the test step has no existing attachments
			TestStepView testStep = testCaseManager.RetrieveStepById2(testCaseId, testStepId);
			Assert.AreEqual(false, testStep.IsAttachments);

			//Lets try adding and attachment to a test step
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.xls", "Sample Test Step Attachment", 2, attachmentData, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Step Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachments[0].AttachmentTypeId);
			Assert.AreEqual("File", attachments[0].AttachmentTypeName);

			//Verify that the attachment flag was also updated
			testStep = testCaseManager.RetrieveStepById2(testCaseId, testStepId);
			Assert.AreEqual(true, testStep.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment and verify
			attachmentManager.Delete(projectId, attachmentId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			testStep = testCaseManager.RetrieveStepById2(testCaseId, testStepId);
			Assert.AreEqual(false, testStep.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Delete the test case / step
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(107)
		]
		public void _07_DeleteIncidentWithAttachment()
		{
			//Create a new incident and attach an artifact
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident", "Test Incident", null, null, null, 1, null, DateTime.Now, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", USER_ID_FRED_BLOGGS, attachmentData, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, null, null, null, null);

			//Now delete the incident and purge
			incidentManager.MarkAsDeleted(projectId, incidentId, USER_ID_FRED_BLOGGS);
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}
		[
		Test,
		SpiraTestCase(110)
		]
		public void _08_DeleteRequirementWithAttachment()
		{
			//Create a new requirement and attach an artifact
			Business.RequirementManager requirement = new Business.RequirementManager();
			int requirementId = requirement.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Test Requirement", null, null, USER_ID_FRED_BLOGGS);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", USER_ID_FRED_BLOGGS, attachmentData, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, null, null, null, null);

			//Now delete the requirement and purge
			requirement.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement.DeleteFromDatabase(requirementId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}

		[
		Test,
		SpiraTestCase(114)
		]
		public void _09_DeleteTestCaseWithAttachment()
		{
			//Create a new test case and attach an artifact
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", USER_ID_FRED_BLOGGS, attachmentData, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, null, null, null, null);

			//Now delete the test case and purge
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}

		[
		Test,
		SpiraTestCase(132)
		]
		public void _10_DeleteReleaseWithAttachment()
		{
			//Create a new release and attach an artifact
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Test Release Version 1", null, "1.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Incident Attachment", USER_ID_FRED_BLOGGS, attachmentData, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, null, null, null, null);

			//Now delete the release and purge
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
			releaseManager.DeleteFromDatabase(releaseId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

		}

		[
		Test,
		SpiraTestCase(337)
		]
		public void _11_DeleteTestStepWithAttachment()
		{
			//Create a new test case with step and attach an attachment
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Test Step With Attachment", null, null);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Step Attachment", USER_ID_FRED_BLOGGS, attachmentData, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, null, null, null, null);

			//Now delete the test case and associated step then purge
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}

		[
		Test,
		SpiraTestCase(346)
		]
		public void _12_AttachDocumentToTestSet()
		{
			//Create a new test set
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//First verify that the test set has no existing attachments
			TestSetView testSet = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual(false, testSet.IsAttachments);

			//Lets try adding and attachment to a test set
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.xls", "Sample Test Set Attachment", USER_ID_FRED_BLOGGS, attachmentData, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Set Attachment", attachments[0].Description);
			Assert.AreEqual("Fred Bloggs", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);

			//Verify that the attachment flag was also updated
			testSet = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual(true, testSet.IsAttachments);

			//Now retrieve the attachment data itself
			FileStream fileStream = attachmentManager.OpenById(attachmentId);
			byte[] retrievedArray = new byte[fileStream.Length];
			fileStream.Read(retrievedArray, 0, (int)fileStream.Length);
			fileStream.Close();
			string retrievedData = unicodeEncoding.GetString(retrievedArray, 0, retrievedArray.Length);
			Assert.AreEqual("Test Attachment Data To Be Stored", retrievedData);

			//Finally clean-up by deleting the attachment and verify
			attachmentManager.Delete(projectId, attachmentId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment flag was also updated
			testSet = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual(false, testSet.IsAttachments);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Delete the test set
			testSetManager.DeleteFromDatabase(testSetId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(347)
		]
		public void _13_DeleteTestSetWithAttachment()
		{
			//Create a new test set and attach an attachment
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = attachmentManager.Insert(projectId, "test_data.txt", "Test Set Attachment", USER_ID_FRED_BLOGGS, attachmentData, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, null, null, null, null);

			//Now delete the test set and purge
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testSetId);
			testSetManager.DeleteFromDatabase(testSetId, USER_ID_FRED_BLOGGS);

			//Verify that the attachment association was deleted
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Verify that the attachment itself wasn't deleted (just the association)
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}

		[
		Test,
		SpiraTestCase(355)
		]
		public void _14_CopyAttachments()
		{
			//Create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Create an incident and task
			IncidentManager incidentManager = new IncidentManager();
			TaskManager taskManager = new TaskManager();
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident", "Test Incident", null, null, null, 1, null, DateTime.Now, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int taskId = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Test Task", null, null, null, null, null, null);

			//Attach two documents to an artifact
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId1 = attachmentManager.Insert(projectId, "test_data.txt", "Test Case Attachment 1", USER_ID_FRED_BLOGGS, attachmentData, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, null, null, null, null);
			int attachmentId2 = attachmentManager.Insert(projectId, "test_data.txt", "Test Case Attachment 2", USER_ID_FRED_BLOGGS, attachmentData, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, null, null, null, null);

			//Verify that it has two attachments
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);

			//Now copy to a different artifact
			attachmentManager.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, testCaseId2);

			//Verify that it has two attachments
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);

			//Now lets test that deleting the first artifact's attachments works (it only deletes the link)
			attachmentManager.Delete(projectId, attachmentId1, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, USER_ID_FRED_BLOGGS);
			attachmentManager.Delete(projectId, attachmentId2, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, USER_ID_FRED_BLOGGS);

			//The second artifact's attachments should still be retrievable
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);

			//Now lets test that deleting the second instance of the attachment actually removes the file
			attachmentManager.Delete(projectId, attachmentId1, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, USER_ID_FRED_BLOGGS);
			attachmentManager.Delete(projectId, attachmentId2, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, USER_ID_FRED_BLOGGS);

			//Both artifacts should now have no attachments
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);

			//Now we need to test that we can copy an attachment (in this case a URL) from artifacts of
			//different types - e.g. task to incident

			//First attach the URL to a task
			attachmentId1 = attachmentManager.Insert(projectId, "http://www.myurl.com", "My URL", USER_ID_FRED_BLOGGS, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, null, null, null, null, null);

			//Now copy the URL to an incident
			attachmentManager.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId);

			//Now verify that the task and incident have attachments
			//Task
			attachments = attachmentManager.RetrieveByArtifactId(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachments[0].AttachmentTypeId);
			Assert.AreEqual("http://www.myurl.com", attachments[0].Filename);
			//Incident
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachments[0].AttachmentTypeId);
			Assert.AreEqual("http://www.myurl.com", attachments[0].Filename);

			//Delete the attachment from the task and verify it's still linked to the incident
			attachmentManager.Delete(projectId, attachmentId1, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);

			//Delete the attachment from the incident and verify that it's not linked to either but still exists in the project
			attachmentManager.Delete(projectId, attachmentId1, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, USER_ID_FRED_BLOGGS);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(0, attachments.Count);
			Attachment attachment = attachmentManager.RetrieveById(attachmentId1);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId1, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");

			//Delete the items
			testCaseManager.DeleteFromDatabase(testCaseId1, USER_ID_FRED_BLOGGS);
			testCaseManager.DeleteFromDatabase(testCaseId2, USER_ID_FRED_BLOGGS);
			taskManager.DeleteFromDatabase(taskId, USER_ID_FRED_BLOGGS);
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can insert, delete and copy URL attachments (vs. file attachments)
		/// </summary>
		[
		Test,
		SpiraTestCase(383)
		]
		public void _15_UrlAttachments()
		{
			//First create a new requirement
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Test Requirement", null, null, USER_ID_FRED_BLOGGS);

			//First verify that the requirement has no existing attachments
			RequirementView requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(false, requirement.IsAttachments);

			//Lets try adding a URL attachment to this requirement
			int attachmentId = attachmentManager.Insert(projectId, "http://www.sharepoint.com/Test.php?Document=1", "Link to SharePoint document", 3, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, null, null, null, null);

			//Now lets get the attachment meta-data and verify
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("http://www.sharepoint.com/Test.php?Document=1", attachments[0].Filename);
			Assert.AreEqual("Link to SharePoint document", attachments[0].Description);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachments[0].AttachmentTypeId);
			Assert.AreEqual("URL", attachments[0].AttachmentTypeName);
			Assert.AreEqual("Joe P Smith", attachments[0].AuthorName);
			Assert.AreEqual(0, attachments[0].Size);

			//Now verify that the requirement flag was updated
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(true, requirement.IsAttachments);

			//Now delete the attachment
			attachmentManager.Delete(projectId, attachmentId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, USER_ID_FRED_BLOGGS);

			//Next verify that the requirement flag was updated
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
			Assert.AreEqual(false, requirement.IsAttachments);

			//Attach it again
			attachmentId = attachmentManager.Insert(projectId, "http://www.sharepoint.com/Test.php?Document=1", "Link to SharePoint document", 3, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, null, null, null, null);

			//Finally delete the requirement (and verify that attachment remains in the project)
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.IsNotNull(attachment);

			//Now delete the attachment from the project and verify its deletion
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			bool artifactExists = true;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Attachment Not Deleted");
		}
	}
}
