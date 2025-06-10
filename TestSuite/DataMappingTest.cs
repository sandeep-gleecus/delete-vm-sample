using System;
using System.Linq;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the base methods of the DataMapping business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class DataMappingTest
	{
		protected static Business.DataMappingManager dataMappingManager;

		protected static int dataSyncSystemId;
		protected static int projectId;
		protected static int projectTemplateId;

		protected static int customListId1;
		protected static int customListId2;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		/// <summary>
		/// Initializes the business objects being tested
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			dataMappingManager = new DataMappingManager();

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("DataMappingTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//We need to add a couple of custom lists for the project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			customListId1 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Operating Systems", true, true).CustomPropertyListId;
			customListId2 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Web Browsers", true, true).CustomPropertyListId;

			//Values
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Linux");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Mac OS X");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows Server");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 8");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 10");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "iOS");
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "Android");
			customPropertyManager.CustomPropertyList_AddValue(customListId2, "Internet Explorer");
			customPropertyManager.CustomPropertyList_AddValue(customListId2, "Firefox");
			customPropertyManager.CustomPropertyList_AddValue(customListId2, "Chrome");
			customPropertyManager.CustomPropertyList_AddValue(customListId2, "Safari");
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can create / modify new plug-ins
		/// </summary>
		[
		Test,
		SpiraTestCase(272)
		]
		public void _01_CreateModifyPlugInList()
		{
			//First lets get the list of existing plug-ins
			//Active Only
			List<DataSyncSystem> dataSyncSystems = dataMappingManager.RetrieveDataSyncSystems();
			Assert.AreEqual(3, dataSyncSystems.Count);
			//All
			dataSyncSystems = dataMappingManager.RetrieveDataSyncSystems(false);
			Assert.AreEqual(3, dataSyncSystems.Count);
			DataSyncSystem dataRow = dataSyncSystems[1];
			Assert.AreEqual("JiraDataSync", dataRow.Name);
			Assert.AreEqual("Jira", dataRow.Caption);
			Assert.AreEqual((int)DataSyncSystem.DataSyncStatusEnum.NotRun, dataRow.DataSyncStatusId);
			Assert.AreEqual("This plug-in allows incidents in the system to be synchronized with the JIRA issue-tracking system", dataRow.Description);
			Assert.IsTrue(dataRow.LastSyncDate.IsNull());

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Data Synchronization";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Now lets try creating a new data-sync
			dataSyncSystemId = dataMappingManager.InsertDataSyncSystem(
				"TestDataSync",
				"Test Sync",
				null,
				"http://myserver/api.aspx",
				"rogerdodger",
				"test123",
				1,
				true,
				"ValueXYZ",
				null,
				null,
				null,
				null,true, 1, adminSectionId, "Inserted Data Synchronization"
				);

			//Now retrieve the data sync system to verify that it has been inserted
			dataRow = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual((int)DataSyncSystem.DataSyncStatusEnum.NotRun, dataRow.DataSyncStatusId);
			Assert.AreEqual("Not Run", dataRow.DataSyncStatusName);
			Assert.AreEqual("TestDataSync", dataRow.Name);
			Assert.IsTrue(dataRow.Description.IsNull());
			Assert.AreEqual("http://myserver/api.aspx", dataRow.ConnectionString);
			Assert.AreEqual("rogerdodger", dataRow.ExternalLogin);
			Assert.AreEqual("test123", dataRow.ExternalPassword);
			Assert.AreEqual(1, dataRow.TimeOffsetHours);
			Assert.AreEqual("Y", dataRow.AutoMapUsersYn);
			Assert.IsTrue(dataRow.LastSyncDate.IsNull());
			Assert.AreEqual("ValueXYZ", dataRow.Custom01);
			Assert.IsTrue(dataRow.Custom02.IsNull());
			Assert.IsTrue(dataRow.Custom03.IsNull());
			Assert.IsTrue(dataRow.Custom04.IsNull());
			Assert.IsTrue(dataRow.Custom05.IsNull());

			//Now lets test that we can make changes to the existing plugin
			dataRow.StartTracking();
			dataRow.Name = "TestingDataSync";
			dataRow.Description = "Tests the data mapping system";
			dataRow.ConnectionString = "http://myserver/api.cgi";
			dataRow.ExternalLogin = "roger@roger.com";
			dataRow.ExternalPassword = null;
			dataRow.TimeOffsetHours = 0;
			dataRow.AutoMapUsersYn = "N";
			dataRow.Custom02 = "Custom123";
			dataMappingManager.UpdateDataSyncSystem(dataRow, 1, adminSectionId, "Updated Data Synchronization");

			//Verify the changes
			dataRow = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual("TestingDataSync", dataRow.Name);
			Assert.AreEqual("Tests the data mapping system", dataRow.Description);
			Assert.AreEqual("http://myserver/api.cgi", dataRow.ConnectionString);
			Assert.AreEqual("roger@roger.com", dataRow.ExternalLogin);
			Assert.IsTrue(dataRow.ExternalPassword.IsNull());
			Assert.AreEqual(0, dataRow.TimeOffsetHours);
			Assert.AreEqual("N", dataRow.AutoMapUsersYn);
			Assert.AreEqual("ValueXYZ", dataRow.Custom01);
			Assert.AreEqual("Custom123", dataRow.Custom02);
			Assert.IsTrue(dataRow.Custom03.IsNull());
			Assert.IsTrue(dataRow.Custom04.IsNull());
			Assert.IsTrue(dataRow.Custom05.IsNull());
		}

		/// <summary>
		/// Tests that we can create / modify the project mappings records
		/// </summary>
		[
		Test,
		SpiraTestCase(448)
		]
		public void _02_CreateModifyProjectsBeingSynched()
		{
			//First lets try to retrieve the existing mapping info (it will throw an exception)
			bool exceptionThrown = false;
			List<DataSyncProject> dataSyncProjects;
			try
			{
				dataSyncProjects = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId);
			}
			catch (DataSyncNotConfiguredException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "DataSync should not be enabled for project");

			//Now we need to insert the new record
			dataMappingManager.InsertDataSyncProject(dataSyncSystemId, projectId, "TEST123", false);

			//Verify the insert
			dataSyncProjects = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId);
			DataSyncProject dataRow = dataSyncProjects[0];
			Assert.AreEqual(dataSyncSystemId, dataRow.DataSyncSystemId);
			Assert.AreEqual(projectId, dataRow.ProjectId);
			Assert.AreEqual("TEST123", dataRow.ExternalKey);
			Assert.AreEqual("N", dataRow.ActiveYn);

			//Now update the key and status and verify the changes
			dataRow.StartTracking();
			dataRow.ActiveYn = "Y";
			dataRow.ExternalKey = "TEST456";
			dataMappingManager.UpdateDataSyncProject(dataRow);

			//Verify the changes
			dataSyncProjects = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId);
			dataRow = dataSyncProjects[0];
			Assert.AreEqual(dataSyncSystemId, dataRow.DataSyncSystemId);
			Assert.AreEqual(projectId, dataRow.ProjectId);
			Assert.AreEqual("TEST456", dataRow.ExternalKey);
			Assert.AreEqual("Y", dataRow.ActiveYn);

			//Next lets verify that we can get a list of mapped projects for a specific plug-in (used by the APIs)
			dataSyncProjects = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId);
			Assert.AreEqual(1, dataSyncProjects.Count);
			Assert.AreEqual(projectId, dataSyncProjects[0].ProjectId);

			//Next lets verify that we can get a list of systems configured for a specific project
			List<DataSyncSystem> dataSyncSystems = dataMappingManager.RetrieveDataSyncSystemsForProject(projectId);
			Assert.AreEqual(1, dataSyncSystems.Count);
			Assert.AreEqual(dataSyncSystemId, dataSyncSystems[0].DataSyncSystemId);
		}

		/// <summary>
		/// Tests that we can create / modify the user mappings records
		/// </summary>
		[
		Test,
		SpiraTestCase(449)
		]
		public void _03_EditUserMappings()
		{
			//First retrieve all the existing user mappings for a single user
			List<DataSyncUserMappingView> userMappingsView = dataMappingManager.RetrieveDataSyncUserMappings(USER_ID_FRED_BLOGGS);
			//GitHub plug-in
			DataSyncUserMappingView dataRow = userMappingsView[0];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, dataRow.UserId);
			Assert.AreEqual("GitHubDataSync", dataRow.DataSyncSystemName);
			Assert.AreEqual("fredbloggs", dataRow.ExternalKey);

			//Our new plug-in
			dataRow = userMappingsView[3];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, dataRow.UserId);
			Assert.AreEqual("TestingDataSync", dataRow.DataSyncSystemName);
			Assert.IsTrue(dataRow.ExternalKey.IsNull());

			//Now retrieve the mapping for a single plug-in
			List<DataSyncUserMapping> userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForSystem(dataSyncSystemId);
			Assert.AreEqual(0, userMappings.Count);

			//Add the external user key and update
			userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForUser(USER_ID_FRED_BLOGGS);
			DataSyncUserMapping userMapping = new DataSyncUserMapping();
			userMapping.UserId = USER_ID_FRED_BLOGGS;
			userMapping.DataSyncSystemId = dataSyncSystemId;
			userMapping.ExternalKey = "fredbloggs123";
			userMappings.Add(userMapping);
			dataMappingManager.SaveDataSyncUserMappings(userMappings);

			//Verify the change
			userMappingsView = dataMappingManager.RetrieveDataSyncUserMappings(USER_ID_FRED_BLOGGS);
			dataRow = userMappingsView[3];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, dataRow.UserId);
			Assert.AreEqual("TestingDataSync", dataRow.DataSyncSystemName);
			Assert.AreEqual("fredbloggs123", dataRow.ExternalKey);

			//Now retrieve the mapping for a single plug-in
			userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForSystem(dataSyncSystemId);
			Assert.AreEqual(1, userMappings.Count);
			userMapping = userMappings[0];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, userMapping.UserId);
			Assert.AreEqual("fredbloggs123", userMapping.ExternalKey);

			//Change the external user key and update
			userMapping.StartTracking();
			userMapping.ExternalKey = "fredbloggs456";
			dataMappingManager.SaveDataSyncUserMappings(userMappings);

			//Verify the change
			userMappingsView = dataMappingManager.RetrieveDataSyncUserMappings(USER_ID_FRED_BLOGGS);
			dataRow = userMappingsView[3];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, dataRow.UserId);
			Assert.AreEqual("TestingDataSync", dataRow.DataSyncSystemName);
			Assert.AreEqual("fredbloggs456", dataRow.ExternalKey);

			//Remove the external user key and update
			userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForUser(USER_ID_FRED_BLOGGS);
			userMapping = userMappings.FirstOrDefault(d => d.DataSyncSystemId == dataSyncSystemId);
			userMapping.StartTracking();
			userMapping.ExternalKey = null;
			dataMappingManager.SaveDataSyncUserMappings(userMappings);

			//Verify the change
			userMappingsView = dataMappingManager.RetrieveDataSyncUserMappings(USER_ID_FRED_BLOGGS);
			dataRow = userMappingsView[3];
			Assert.AreEqual(USER_ID_FRED_BLOGGS, dataRow.UserId);
			Assert.AreEqual("TestingDataSync", dataRow.DataSyncSystemName);
			Assert.IsTrue(dataRow.ExternalKey.IsNull());

			//Finally put the user back so that we can verify that the plug-in delete cascades correctly later
			userMappings = dataMappingManager.RetrieveDataSyncUserMappingsForUser(USER_ID_FRED_BLOGGS);
			userMapping = new DataSyncUserMapping();
			userMapping.UserId = USER_ID_FRED_BLOGGS;
			userMapping.DataSyncSystemId = dataSyncSystemId;
			userMapping.ExternalKey = "fredbloggs123";
			userMappings.Add(userMapping);
			dataMappingManager.SaveDataSyncUserMappings(userMappings);
		}

		/// <summary>
		/// Tests that we can create / modify the field value mapping records
		/// </summary>
		[
		Test,
		SpiraTestCase(450)
		]
		public void _04_EditFieldValueMappings()
		{
			//First retrieve the existing field value mappings for our new plug-in for a field (status) - includes any unmapped rows
			List<DataSyncFieldValueMappingView> dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3, true);
			Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
			//Row 0
			Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
			Assert.IsTrue(dataSyncFieldMappingsView[0].ExternalKey.IsNull());
			//Row 1
			Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
			Assert.IsTrue(dataSyncFieldMappingsView[1].ExternalKey.IsNull());
			//Row 2
			Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
			Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

			//Now lets add the mapping entries for this field
			List<DataSyncArtifactFieldValueMapping> dataSyncFieldMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3);

			DataSyncArtifactFieldValueMapping newDataSyncFieldMapping = new DataSyncArtifactFieldValueMapping();
			newDataSyncFieldMapping.MarkAsAdded();
			newDataSyncFieldMapping.ArtifactFieldValue = dataSyncFieldMappingsView[0].ArtifactFieldValue.Value;
			newDataSyncFieldMapping.DataSyncSystemId = dataSyncSystemId;
			newDataSyncFieldMapping.ArtifactFieldId = 3;
			newDataSyncFieldMapping.ProjectId = projectId;
			newDataSyncFieldMapping.PrimaryYn = "Y";
			newDataSyncFieldMapping.ExternalKey = "X-ASSIGNED";
			dataSyncFieldMappings.Add(newDataSyncFieldMapping);

			newDataSyncFieldMapping = new DataSyncArtifactFieldValueMapping();
			newDataSyncFieldMapping.MarkAsAdded();
			newDataSyncFieldMapping.ArtifactFieldValue = dataSyncFieldMappingsView[1].ArtifactFieldValue.Value;
			newDataSyncFieldMapping.DataSyncSystemId = dataSyncSystemId;
			newDataSyncFieldMapping.ArtifactFieldId = 3;
			newDataSyncFieldMapping.ProjectId = projectId;
			newDataSyncFieldMapping.PrimaryYn = "Y";
			newDataSyncFieldMapping.ExternalKey = "X-CLOSED";
			dataSyncFieldMappings.Add(newDataSyncFieldMapping);

			newDataSyncFieldMapping = new DataSyncArtifactFieldValueMapping();
			newDataSyncFieldMapping.MarkAsAdded();
			newDataSyncFieldMapping.ArtifactFieldValue = dataSyncFieldMappingsView[2].ArtifactFieldValue.Value;
			newDataSyncFieldMapping.DataSyncSystemId = dataSyncSystemId;
			newDataSyncFieldMapping.ArtifactFieldId = 3;
			newDataSyncFieldMapping.ProjectId = projectId;
			newDataSyncFieldMapping.PrimaryYn = "Y";
			newDataSyncFieldMapping.ExternalKey = "X-DUPLICATE";
			dataSyncFieldMappings.Add(newDataSyncFieldMapping);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Data Synchronization";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			string action = "Data Synchronization Updated";

			dataMappingManager.SaveDataSyncFieldValueMappings(dataSyncFieldMappings, 1, adminSectionId, action);

			//Verify the changes
			dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3, true);
			Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
			//Row 0
			Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
			Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
			Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
			//Row 2
			Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
			Assert.AreEqual("X-DUPLICATE", dataSyncFieldMappingsView[2].ExternalKey);

			//Now change one of the rows
			int artifactFieldValue = dataSyncFieldMappingsView[2].ArtifactFieldValue.Value;   //Duplicate
			dataSyncFieldMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3);
			DataSyncArtifactFieldValueMapping dataSyncFieldMapping = dataSyncFieldMappings.FirstOrDefault(d => d.ArtifactFieldValue == artifactFieldValue);
			dataSyncFieldMapping.StartTracking();
			dataSyncFieldMapping.ExternalKey = "X-CLOSED";
			dataSyncFieldMapping.PrimaryYn = "N";
			dataMappingManager.SaveDataSyncFieldValueMappings(dataSyncFieldMappings, 1, adminSectionId, action);

			//Verify the changes
			dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3, true);
			Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
			//Row 0
			Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
			Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
			Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
			//Row 2
			Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
			Assert.AreEqual("N", dataSyncFieldMappingsView[2].PrimaryYn);
			Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[2].ExternalKey);

			//Now delete one of the rows by setting its external key to null
			dataSyncFieldMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3);
			dataSyncFieldMapping = dataSyncFieldMappings.FirstOrDefault(d => d.ArtifactFieldValue == artifactFieldValue);
			dataSyncFieldMapping.StartTracking();
			dataSyncFieldMapping.ExternalKey = null;
			dataMappingManager.SaveDataSyncFieldValueMappings(dataSyncFieldMappings, 1, adminSectionId, action);

			//Verify the changes
			dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3, true);
			Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
			//Row 0
			Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
			Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
			Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
			//Row 2
			Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
			Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

			//Now test that we can get a list of only the mapped ones (used by the APIs)
			dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3, false);
			Assert.AreEqual(2, dataSyncFieldMappingsView.Count);
			//Row 0
			Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
			Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
			Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
			Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
			Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);

			//Now try and make two items map to the same external key with both set as primary yn
			//This is not allowed since it creates an ambiguous mapping, verify that the exception was thrown
			artifactFieldValue = dataSyncFieldMappingsView[0].ArtifactFieldValue.Value;
			dataSyncFieldMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3);
			dataSyncFieldMapping = dataSyncFieldMappings.FirstOrDefault(d => d.ArtifactFieldValue == artifactFieldValue);
			Assert.IsNotNull(dataSyncFieldMapping);
			dataSyncFieldMapping.StartTracking();
			dataSyncFieldMapping.ExternalKey = "X-CLOSED";
			dataSyncFieldMapping.PrimaryYn = "Y";
			bool exceptionThrown = false;
			try
			{
				dataMappingManager.SaveDataSyncFieldValueMappings(dataSyncFieldMappings, 1, adminSectionId, action);
			}
			catch (DataSyncPrimaryExternalKeyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not allow duplicate primary external keys");

			//Now try and set primary = false when we don't have duplicate values
			dataSyncFieldMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId, 3);
			dataSyncFieldMapping = dataSyncFieldMappings.FirstOrDefault(d => d.ArtifactFieldValue == artifactFieldValue);
			Assert.IsNotNull(dataSyncFieldMapping);
			dataSyncFieldMapping.StartTracking();
			dataSyncFieldMapping.ExternalKey = "X-DUPLICATE";
			dataSyncFieldMapping.PrimaryYn = "N";
			exceptionThrown = false;
			try
			{
				dataMappingManager.SaveDataSyncFieldValueMappings(dataSyncFieldMappings, 1, adminSectionId, action);
			}
			catch (DataSyncPrimaryExternalKeyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should require unique external keys to be primary");
		}

		/// <summary>
		/// Tests that we can create / modify the custom property mapping records
		/// </summary>
		[
		Test,
		SpiraTestCase(451)
		]
		public void _05_EditCustomPropertyMappings()
		{
			//First we need to create a new project custom property
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, DataModel.Artifact.ArtifactTypeEnum.Incident, false, false);
			Assert.AreEqual(0, customProperties.Count);
			int customPropertyId = customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.List, 10, "Web Browser", null, null, customListId2).CustomPropertyId;

			//Verify that it added
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, DataModel.Artifact.ArtifactTypeEnum.Incident, false, false);
			Assert.AreEqual(1, customProperties.Count);

			//Now retrieve the custom property mapping for this new custom property (should be no data)
			DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.IsNull(customPropertyMapping);

			//Next we need to add a mapping entry for this custom property
			List<DataSyncCustomPropertyMapping> customPropertyMappings = new List<DataSyncCustomPropertyMapping>();
			DataSyncCustomPropertyMapping newCustomPropertyMapping = new DataSyncCustomPropertyMapping();
			newCustomPropertyMapping.MarkAsAdded();
			newCustomPropertyMapping.DataSyncSystemId = dataSyncSystemId;
			newCustomPropertyMapping.ProjectId = projectId;
			newCustomPropertyMapping.CustomPropertyId = customPropertyId;
			newCustomPropertyMapping.ExternalKey = "field_00001";
			customPropertyMappings.Add(newCustomPropertyMapping);
			dataMappingManager.SaveDataSyncCustomPropertyMappings(customPropertyMappings);

			//Verify the changes
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.AreEqual("Web Browser", customPropertyMapping.CustomProperty.Name);
			Assert.AreEqual("field_00001", customPropertyMapping.ExternalKey);

			//Verify that we can change the external key
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			customPropertyMapping.StartTracking();
			customPropertyMapping.ExternalKey = "web_browser";
			dataMappingManager.SaveDataSyncCustomPropertyMappings(new List<DataSyncCustomPropertyMapping>() { customPropertyMapping });

			//Verify the changes
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.AreEqual("Web Browser", customPropertyMapping.CustomProperty.Name);
			Assert.AreEqual("web_browser", customPropertyMapping.ExternalKey);

			//Verify that we can remove the external key
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			customPropertyMapping.StartTracking();
			customPropertyMapping.MarkAsDeleted();
			customPropertyMapping.ExternalKey = null;
			dataMappingManager.SaveDataSyncCustomPropertyMappings(new List<DataSyncCustomPropertyMapping>() { customPropertyMapping });

			//Verify the deletion
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.IsNull(customPropertyMapping);

			//Restore the external key for the custom property value mappings
			customPropertyMappings.Clear();
			newCustomPropertyMapping = new DataSyncCustomPropertyMapping();
			newCustomPropertyMapping.MarkAsAdded();
			newCustomPropertyMapping.DataSyncSystemId = dataSyncSystemId;
			newCustomPropertyMapping.ProjectId = projectId;
			newCustomPropertyMapping.CustomPropertyId = customPropertyId;
			newCustomPropertyMapping.ExternalKey = "field_00002";
			customPropertyMappings.Add(newCustomPropertyMapping);
			dataMappingManager.SaveDataSyncCustomPropertyMappings(customPropertyMappings);

			//Verify the changes
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.AreEqual("Web Browser", customPropertyMapping.CustomProperty.Name);
			Assert.AreEqual("field_00002", customPropertyMapping.ExternalKey);

			//*** Now we need to test the value mappings ***

			//First retrieve the custom property value mappings for this new custom property (includes unmapped)
			List<DataSyncCustomPropertyValueMappingView> customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId, true);
			Assert.AreEqual(4, customPropertyValueMappingsView.Count);
			//Row 0
			Assert.AreEqual("Chrome", customPropertyValueMappingsView[0].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[0].IsActive);
			Assert.IsTrue(customPropertyValueMappingsView[0].ExternalKey.IsNull());
			//Row 1
			Assert.AreEqual("Firefox", customPropertyValueMappingsView[1].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[1].IsActive);
			Assert.IsTrue(customPropertyValueMappingsView[1].ExternalKey.IsNull());
			//Row 2
			Assert.AreEqual("Internet Explorer", customPropertyValueMappingsView[2].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[2].IsActive);
			Assert.IsTrue(customPropertyValueMappingsView[2].ExternalKey.IsNull());

			//Now lets add the mapping entries for some of the values
			List<DataSyncCustomPropertyValueMapping> customPropertyValueMappings = new List<DataSyncCustomPropertyValueMapping>();
			//Chrome
			DataSyncCustomPropertyValueMapping customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[0].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "CHROME";
			customPropertyValueMappings.Add(customPropertyValueMapping);

			//Firefox
			customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[1].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "FIREFOX";
			customPropertyValueMappings.Add(customPropertyValueMapping);

			//Internet Explorer
			customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[2].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "MSIE";
			customPropertyValueMappings.Add(customPropertyValueMapping);

			dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

			//Verify the changes
			customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId, true);
			Assert.AreEqual(4, customPropertyValueMappingsView.Count);
			//Row 0
			Assert.AreEqual("Chrome", customPropertyValueMappingsView[0].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[0].IsActive);
			Assert.AreEqual("CHROME", customPropertyValueMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Firefox", customPropertyValueMappingsView[1].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[1].IsActive);
			Assert.AreEqual("FIREFOX", customPropertyValueMappingsView[1].ExternalKey);
			//Row 2
			Assert.AreEqual("Internet Explorer", customPropertyValueMappingsView[2].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[2].IsActive);
			Assert.AreEqual("MSIE", customPropertyValueMappingsView[2].ExternalKey);

			//Now change one of the rows
			customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, customPropertyId);
			customPropertyValueMapping = customPropertyValueMappings.FirstOrDefault(d => d.CustomPropertyValueId == customPropertyValueMappingsView[1].CustomPropertyValueId);
			customPropertyValueMapping.StartTracking();
			customPropertyValueMapping.ExternalKey = "MOZILLA";
			dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

			//Verify the changes
			customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId, true);
			Assert.AreEqual(4, customPropertyValueMappingsView.Count);
			//Row 0
			Assert.AreEqual("Chrome", customPropertyValueMappingsView[0].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[0].IsActive);
			Assert.AreEqual("CHROME", customPropertyValueMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Firefox", customPropertyValueMappingsView[1].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[1].IsActive);
			Assert.AreEqual("MOZILLA", customPropertyValueMappingsView[1].ExternalKey);
			//Row 2
			Assert.AreEqual("Internet Explorer", customPropertyValueMappingsView[2].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[2].IsActive);
			Assert.AreEqual("MSIE", customPropertyValueMappingsView[2].ExternalKey);

			//Now delete one of the rows by setting its external key to null
			customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, customPropertyId);
			customPropertyValueMapping = customPropertyValueMappings.FirstOrDefault(d => d.CustomPropertyValueId == customPropertyValueMappingsView[1].CustomPropertyValueId);
			customPropertyValueMapping.StartTracking();
			customPropertyValueMapping.ExternalKey = null;
			dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

			//Verify the changes
			customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId, true);
			Assert.AreEqual(4, customPropertyValueMappingsView.Count);
			//Row 0
			Assert.AreEqual("Chrome", customPropertyValueMappingsView[0].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[0].IsActive);
			Assert.AreEqual("CHROME", customPropertyValueMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Firefox", customPropertyValueMappingsView[1].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[1].IsActive);
			Assert.IsTrue(customPropertyValueMappingsView[1].ExternalKey.IsNull());
			//Row 2
			Assert.AreEqual("Internet Explorer", customPropertyValueMappingsView[2].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[2].IsActive);
			Assert.AreEqual("MSIE", customPropertyValueMappingsView[2].ExternalKey);

			//Now test that we can get a list of just the mapped values (this overload is used by the APIs)
			customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId, false);
			Assert.AreEqual(2, customPropertyValueMappingsView.Count);
			//Row 0
			Assert.AreEqual("Chrome", customPropertyValueMappingsView[0].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[0].IsActive);
			Assert.AreEqual("CHROME", customPropertyValueMappingsView[0].ExternalKey);
			//Row 1
			Assert.AreEqual("Internet Explorer", customPropertyValueMappingsView[1].CustomPropertyValueName);
			Assert.AreEqual(true, customPropertyValueMappingsView[1].IsActive);
			Assert.AreEqual("MSIE", customPropertyValueMappingsView[1].ExternalKey);

			//Finally test that deleting the project custom property cascades correctly
			customPropertyManager.CustomPropertyDefinition_RemoveById(customPropertyId);

			//Verify the deletion
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId);
			Assert.IsNull(customProperty);

			//Verify that the mapping is also removed
			DataSyncCustomPropertyMapping dataSyncCustomPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId);
			Assert.IsNull(dataSyncCustomPropertyMapping);

			//Now add a custom property mapping to another new custom properties so that we
			//can verify that deleting the plug-in (later on) cascades correctly
			customPropertyId = customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.List, 2, "Operating System", null, null, customListId1).CustomPropertyId;

			//First the custom property mapping
			int existingCustomPropertyId = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, 2, false).CustomPropertyId;
			customPropertyMappings = new List<DataSyncCustomPropertyMapping>();
			newCustomPropertyMapping = new DataSyncCustomPropertyMapping();
			newCustomPropertyMapping.MarkAsAdded();
			newCustomPropertyMapping.DataSyncSystemId = dataSyncSystemId;
			newCustomPropertyMapping.ProjectId = projectId;
			newCustomPropertyMapping.CustomPropertyId = existingCustomPropertyId;
			newCustomPropertyMapping.ExternalKey = "field_os";
			customPropertyMappings.Add(newCustomPropertyMapping);
			dataMappingManager.SaveDataSyncCustomPropertyMappings(customPropertyMappings);

			//Next the custom property value mappings
			customPropertyValueMappingsView = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, existingCustomPropertyId, true);
			customPropertyValueMappings = new List<DataSyncCustomPropertyValueMapping>();
			//ANDROID
			customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[0].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "ANDROID";
			customPropertyValueMappings.Add(customPropertyValueMapping);
			//MACOS
			customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[3].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "MACOS";
			customPropertyValueMappings.Add(customPropertyValueMapping);
			//IOS
			customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = dataSyncSystemId;
			customPropertyValueMapping.ProjectId = projectId;
			customPropertyValueMapping.CustomPropertyValueId = customPropertyValueMappingsView[1].CustomPropertyValueId;
			customPropertyValueMapping.ExternalKey = "IOS";
			customPropertyValueMappings.Add(customPropertyValueMapping);

			dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);
		}

		/// <summary>
		/// Tests that we can create / modify the custom property mapping records
		/// </summary>
		[
		Test,
		SpiraTestCase(452)
		]
		public void _06_EditArtifactMappings()
		{
			//First verify the list of artifacts that can be data-mapped
			List<ArtifactType> artifactTypes = dataMappingManager.RetrieveArtifactTypes();
			Assert.AreEqual(29, artifactTypes.Count);

			//First retrieve the release mappings for the new plug-in for release 1
			List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, 1);

			//Verify that the plug-in is listed, but that there is no external key set
			Assert.AreEqual(1, artifactMappings.Count);
			DataSyncArtifactMapping dataRow = artifactMappings[0];
			Assert.AreEqual(1, dataRow.ArtifactId);
			Assert.IsTrue(dataRow.ExternalKey.IsNull());

			//Verify the system id
			DataSyncSystem dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataRow.DataSyncSystemId);
			Assert.AreEqual("TestingDataSync", dataSyncSystem.Name);

			//Now add the external key and verify
			artifactMappings[0].StartTracking();
			artifactMappings[0].ExternalKey = "Release_01";
			dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);

			//Verify the change
			artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, 1);
			Assert.AreEqual("Release_01", artifactMappings[0].ExternalKey);

			//Now change the external key and verify
			artifactMappings[0].StartTracking();
			artifactMappings[0].ExternalKey = "Release_001";
			dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);

			//Verify the change
			artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, 1);
			Assert.AreEqual("Release_001", artifactMappings[0].ExternalKey);

			//Now delete the external key and verify
			artifactMappings[0].StartTracking();
			artifactMappings[0].ExternalKey = null;
			dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);

			//Verify the change
			artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, 1);
			Assert.IsTrue(artifactMappings[0].ExternalKey.IsNull());

			//Finally add an external key so that we can test that deleting the datasync cascades correctly
			artifactMappings[0].StartTracking();
			artifactMappings[0].ExternalKey = "Release_01";
			dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);
		}

		/// <summary>
		/// Tests that copying a project (or using as a template) copies across the project mappings
		/// with any necessary ID translations taking place
		/// </summary>
		[
		Test,
		SpiraTestCase(453)
		]
		public void _07_ProjectMappingsCopyAndDelete()
		{
			//First we try the case of using the same template
			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;
				//First we need to create a new project from the existing project
				Business.ProjectManager projectManager = new Business.ProjectManager();
				int projectId1 = projectManager.CreateFromExisting("New Project 1", "", "", projectId, true, false, 1,
					adminSectionId,
					"Inserted Project");

				//Now make a copy of this project
				int projectId2 = projectManager.Copy(USER_ID_FRED_BLOGGS, projectId1, true, null, adminSectionId, "Project Cloned");

				//Get the templates
				TemplateManager templateManager = new TemplateManager();
				int projectTemplateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
				int projectTemplateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;

				//*** Now verify the mappings in project 1 ***

				//Project Mapping Entry
				List<DataSyncProject> projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId1);
				DataSyncProject dataRow = projectMappings[0];
				Assert.AreEqual("TEST456", dataRow.ExternalKey);
				Assert.AreEqual("N", dataRow.ActiveYn);

				//Field Value Mappings
				List<DataSyncFieldValueMappingView> dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId1, 3, true);
				Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
				//Row 0
				Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
				Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
				//Row 1
				Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
				Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
				//Row 2
				Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
				Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

				//Custom Property Mappings
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				int newCustomPropertyId = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId1, Artifact.ArtifactTypeEnum.Incident, 2, false).CustomPropertyId;
				DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId1, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId);
				Assert.AreEqual("Operating System", customPropertyMapping.CustomProperty.Name);
				Assert.AreEqual("field_os", customPropertyMapping.ExternalKey);

				//Custom Property Value Mappings
				List<DataSyncCustomPropertyValueMappingView> customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId1, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId, true);
				Assert.AreEqual(7, customPropertyValueMappings.Count);
				//Row 0
				Assert.AreEqual("Android", customPropertyValueMappings[0].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[0].IsActive);
				Assert.AreEqual("ANDROID", customPropertyValueMappings[0].ExternalKey);
				//Row 3
				Assert.AreEqual("Mac OS X", customPropertyValueMappings[3].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[3].IsActive);
				Assert.AreEqual("MACOS", customPropertyValueMappings[3].ExternalKey);
				//Row 1
				Assert.AreEqual("iOS", customPropertyValueMappings[1].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[1].IsActive);
				Assert.AreEqual("IOS", customPropertyValueMappings[1].ExternalKey);

				//*** Now verify the mappings in project 2 ***

				//Project Mapping Entry
				projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId2);
				dataRow = projectMappings[0];
				Assert.AreEqual("TEST456", dataRow.ExternalKey);
				Assert.AreEqual("N", dataRow.ActiveYn);

				//Field Value Mappings
				dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId2, 3, true);
				Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
				//Row 0
				Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
				Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
				//Row 1
				Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
				Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
				//Row 2
				Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
				Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

				//Custom Property Mappings
				newCustomPropertyId = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId2, Artifact.ArtifactTypeEnum.Incident, 2, false).CustomPropertyId;
				customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId2, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId);
				Assert.AreEqual("Operating System", customPropertyMapping.CustomProperty.Name);
				Assert.AreEqual("field_os", customPropertyMapping.ExternalKey);

				//Custom Property Value Mappings
				customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId2, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId, true);
				Assert.AreEqual(7, customPropertyValueMappings.Count);
				//Row 0
				Assert.AreEqual("Android", customPropertyValueMappings[0].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[0].IsActive);
				Assert.AreEqual("ANDROID", customPropertyValueMappings[0].ExternalKey);
				//Row 3
				Assert.AreEqual("Mac OS X", customPropertyValueMappings[3].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[3].IsActive);
				Assert.AreEqual("MACOS", customPropertyValueMappings[3].ExternalKey);
				//Row 1
				Assert.AreEqual("iOS", customPropertyValueMappings[1].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[1].IsActive);
				Assert.AreEqual("IOS", customPropertyValueMappings[1].ExternalKey);

				//Make the mappings for project 1 active
				projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId1);
				DataSyncProject projectMapping = projectMappings[0];
				projectMapping.StartTracking();
				projectMapping.ActiveYn = "Y";
				dataMappingManager.UpdateDataSyncProject(projectMapping);

				//Now add a release mapping to verify that the project delete will remove it successfully
				ReleaseManager releaseManager = new ReleaseManager();
				int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId1, USER_ID_FRED_BLOGGS, "Rel 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddDays(10), 1, 0, null, false);
				List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId1, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId);
				artifactMappings[0].StartTracking();
				artifactMappings[0].ExternalKey = "Release_01";
				dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);

				//Finally delete both projects
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId1);
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId2);
			}

			//Now do the same thing again, but this time, have the projects use their own new templates
			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;
				//First we need to create a new project from the existing project
				Business.ProjectManager projectManager = new Business.ProjectManager();
				int projectId1 = projectManager.CreateFromExisting("New Project 1", "", "", projectId, true, true);

				//Now make a copy of this project
				int projectId2 = projectManager.Copy(USER_ID_FRED_BLOGGS, projectId1, true, null, adminSectionId, "Project Cloned");

				//Get the templates
				TemplateManager templateManager = new TemplateManager();
				int projectTemplateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
				int projectTemplateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;

				//*** Now verify the mappings in project 1 ***

				//Project Mapping Entry
				List<DataSyncProject> projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId1);
				DataSyncProject dataRow = projectMappings[0];
				Assert.AreEqual("TEST456", dataRow.ExternalKey);
				Assert.AreEqual("N", dataRow.ActiveYn);

				//Field Value Mappings
				List<DataSyncFieldValueMappingView> dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId1, 3, true);
				Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
				//Row 0
				Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
				Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
				//Row 1
				Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
				Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
				//Row 2
				Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
				Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

				//Custom Property Mappings
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				int newCustomPropertyId = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId1, Artifact.ArtifactTypeEnum.Incident, 2, false).CustomPropertyId;
				DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId1, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId);
				Assert.AreEqual("Operating System", customPropertyMapping.CustomProperty.Name);
				Assert.AreEqual("field_os", customPropertyMapping.ExternalKey);

				//Custom Property Value Mappings
				List<DataSyncCustomPropertyValueMappingView> customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId1, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId, true);
				Assert.AreEqual(7, customPropertyValueMappings.Count);
				//Row 0
				Assert.AreEqual("Android", customPropertyValueMappings[0].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[0].IsActive);
				Assert.AreEqual("ANDROID", customPropertyValueMappings[0].ExternalKey);
				//Row 3
				Assert.AreEqual("Mac OS X", customPropertyValueMappings[3].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[3].IsActive);
				Assert.AreEqual("MACOS", customPropertyValueMappings[3].ExternalKey);
				//Row 1
				Assert.AreEqual("iOS", customPropertyValueMappings[1].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[1].IsActive);
				Assert.AreEqual("IOS", customPropertyValueMappings[1].ExternalKey);

				//*** Now verify the mappings in project 2 ***

				//Project Mapping Entry
				projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId2);
				dataRow = projectMappings[0];
				Assert.AreEqual("TEST456", dataRow.ExternalKey);
				Assert.AreEqual("N", dataRow.ActiveYn);

				//Field Value Mappings
				dataSyncFieldMappingsView = dataMappingManager.RetrieveDataSyncFieldValueMappings(dataSyncSystemId, projectId2, 3, true);
				Assert.AreEqual(8, dataSyncFieldMappingsView.Count);
				//Row 0
				Assert.AreEqual("Assigned", dataSyncFieldMappingsView[0].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[0].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[0].PrimaryYn);
				Assert.AreEqual("X-ASSIGNED", dataSyncFieldMappingsView[0].ExternalKey);
				//Row 1
				Assert.AreEqual("Closed", dataSyncFieldMappingsView[1].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[1].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[1].PrimaryYn);
				Assert.AreEqual("X-CLOSED", dataSyncFieldMappingsView[1].ExternalKey);
				//Row 2
				Assert.AreEqual("Duplicate", dataSyncFieldMappingsView[2].ArtifactFieldValueName);
				Assert.AreEqual(true, dataSyncFieldMappingsView[2].IsActive);
				Assert.AreEqual("Y", dataSyncFieldMappingsView[2].PrimaryYn);
				Assert.IsTrue(dataSyncFieldMappingsView[2].ExternalKey.IsNull());

				//Custom Property Mappings
				newCustomPropertyId = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId2, Artifact.ArtifactTypeEnum.Incident, 2, false).CustomPropertyId;
				customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(dataSyncSystemId, projectId2, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId);
				Assert.AreEqual("Operating System", customPropertyMapping.CustomProperty.Name);
				Assert.AreEqual("field_os", customPropertyMapping.ExternalKey);

				//Custom Property Value Mappings
				customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(dataSyncSystemId, projectId2, DataModel.Artifact.ArtifactTypeEnum.Incident, newCustomPropertyId, true);
				Assert.AreEqual(7, customPropertyValueMappings.Count);
				//Row 0
				Assert.AreEqual("Android", customPropertyValueMappings[0].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[0].IsActive);
				Assert.AreEqual("ANDROID", customPropertyValueMappings[0].ExternalKey);
				//Row 3
				Assert.AreEqual("Mac OS X", customPropertyValueMappings[3].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[3].IsActive);
				Assert.AreEqual("MACOS", customPropertyValueMappings[3].ExternalKey);
				//Row 1
				Assert.AreEqual("iOS", customPropertyValueMappings[1].CustomPropertyValueName);
				Assert.AreEqual(true, customPropertyValueMappings[1].IsActive);
				Assert.AreEqual("IOS", customPropertyValueMappings[1].ExternalKey);

				//Make the mappings for project 1 active
				projectMappings = dataMappingManager.RetrieveDataSyncProjects(dataSyncSystemId, projectId1);
				DataSyncProject projectMapping = projectMappings[0];
				projectMapping.StartTracking();
				projectMapping.ActiveYn = "Y";
				dataMappingManager.UpdateDataSyncProject(projectMapping);

				//Now add a release mapping to verify that the project delete will remove it successfully
				ReleaseManager releaseManager = new ReleaseManager();
				int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId1, USER_ID_FRED_BLOGGS, "Rel 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddDays(10), 1, 0, null, false);
				List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId1, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId);
				artifactMappings[0].StartTracking();
				artifactMappings[0].ExternalKey = "Release_01";
				dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);

				//Finally delete both projects and templates
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId1);
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId2);
				templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId1);
				templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId2);
			}
		}


		/// <summary>
		/// Tests that we can store the date of last sync and retrieve the list of all synchronizations
		/// </summary>
		[
		Test,
		SpiraTestCase(332)
		]
		public void _08_SaveAndRetrieveLastRunInfo()
		{
			int SECONDS_BUFFER = 2;
			//First lets update the data-sync information for our new plug-in that succeeded
			DateTime lastSyncDate1 = DateTime.Now;
			dataMappingManager.SaveRunSuccess(dataSyncSystemId, lastSyncDate1);
			//Verify the status and date
			DataSyncSystem dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate1.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Success", dataSyncSystem.DataSyncStatusName);

			//Now lets test that we can run it again and have the date be updated
			lastSyncDate1 = DateTime.Now;
			dataMappingManager.SaveRunSuccess(dataSyncSystemId, lastSyncDate1);
			//Verify the status and date
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate1.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Success", dataSyncSystem.DataSyncStatusName);

			//Next lets update the data-sync information for a new plug-in that succeeded with warnings
			DateTime lastSyncDate2 = DateTime.Now;
			dataMappingManager.SaveRunWarning(dataSyncSystemId, lastSyncDate2);
			//Verify the status and date
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate2.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Warning", dataSyncSystem.DataSyncStatusName);

			//Now lets test that we can run it again and have the date be updated
			lastSyncDate2 = DateTime.Now;
			dataMappingManager.SaveRunWarning(dataSyncSystemId, lastSyncDate2);
			//Verify the status and date
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate2.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Warning", dataSyncSystem.DataSyncStatusName);

			//Next lets update the data-sync information for a new plug-in that failed (date doesn't change)
			DateTime lastSyncDate3 = DateTime.Now;
			dataMappingManager.SaveRunFailure(dataSyncSystemId);
			//Verify the status and date
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate2.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Failure", dataSyncSystem.DataSyncStatusName);

			//Now lets test that we can run it again and have the date be updated when it succeeds
			lastSyncDate3 = DateTime.Now;
			dataMappingManager.SaveRunSuccess(dataSyncSystemId, lastSyncDate3);
			//Verify the status
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate3.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Success", dataSyncSystem.DataSyncStatusName);

			//Now lets test that we can run it again and have the status be updated
			dataMappingManager.SaveRunFailure(dataSyncSystemId);
			//Verify the status
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.AreEqual(true, lastSyncDate3.AddSeconds(SECONDS_BUFFER) > dataSyncSystem.LastSyncDate);
			Assert.AreEqual("Failure", dataSyncSystem.DataSyncStatusName);

			//Finally verify that we can reset one of the data-syncs in the list and verify that it gets reset
			dataMappingManager.ResetLastRunInfo(dataSyncSystemId);
			//Verify the status and date
			dataSyncSystem = dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			Assert.IsTrue(dataSyncSystem.LastSyncDate.IsNull());
			Assert.AreEqual("Not Run", dataSyncSystem.DataSyncStatusName);
		}

		/// <summary>
		/// Tests that we can delete a plug-in and have it cascade correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(454)
		]
		public void _09_DeletePlugIns()
		{
			//Delete the datasync plug-in and verify it deleted
			dataMappingManager.DeleteDataSyncSystem(dataSyncSystemId, USER_ID_FRED_BLOGGS);

			bool exceptionThrown = false;
			try
			{
				dataMappingManager.RetrieveDataSyncSystemById(dataSyncSystemId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "DataSync not deleted correctly");
		}
	}
}
