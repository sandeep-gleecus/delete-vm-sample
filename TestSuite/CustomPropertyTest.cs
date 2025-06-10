using System;
using System.Linq;
using System.Collections;
using System.Data;
using System.Collections.Generic;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the CustomPropertyManager business object
	/// </summary>
	[TestFixture]
	[SpiraTestConfiguration(
		InternalRoutines.SPIRATEST_INTERNAL_URL,
		InternalRoutines.SPIRATEST_INTERNAL_LOGIN,
		InternalRoutines.SPIRATEST_INTERNAL_PASSWORD,
		InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID,
		InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class CustomPropertyTest
	{
		protected static CustomPropertyManager customPropertyManager;
		protected static HistoryManager history;
		protected static ProjectManager projectManager;
		protected static TemplateManager templateManager;

		protected static int customListId1;
		protected static int customListId2;
		protected static int customListId3;
		protected static int customListId4;

		protected static int customPropertyValueId1;
		protected static int customPropertyValueId2;
		protected static int customPropertyValueId3;
		protected static int customPropertyValueId4;
		protected static int customPropertyValueId5;
		protected static int customPropertyValueId6;
		protected static int customPropertyValueId7;
		protected static int customPropertyValueId8;
		protected static int customPropertyValueId9;
		protected static int customPropertyValueId10;

		protected static int customPropertyId1;
		protected static int customPropertyId2;
		protected static int customPropertyId3;
		protected static int customPropertyId4;
		protected static int customPropertyId5;
		protected static int customPropertyId6;
		protected static int customPropertyId7;
		protected static int customPropertyId8;
		protected static int customPropertyId9;
		protected static int customPropertyId10;

		protected static int projectId;
		private static int projectTemplateId;
		protected static int copiedProjectId = -1;
		protected static int copiedProjectTemplateId = -1;
		protected static int taskId1;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		/// <summary>
		/// Initializes the business objects being tested
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			//First we need to instantiate the custom property business object that we'll be using
			customPropertyManager = new CustomPropertyManager();
			projectManager = new ProjectManager();
			templateManager = new TemplateManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//For this test fixture we need to create a completely empty project to test the custom properties with
			projectId = projectManager.Insert("Custom Property Test Project", null, "", "", true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = templateManager.RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete our test projects and templates
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId);

			if (copiedProjectId > 0)
			{
				//Finally delete our project
				projectManager.Delete(USER_ID_FRED_BLOGGS, copiedProjectId);

				if (copiedProjectTemplateId > 0)
				{
					templateManager.Delete(USER_ID_FRED_BLOGGS, copiedProjectTemplateId);
				}
			}
		}

		/// <summary>
		/// Tests that you can add, edit and delete custom lists in a project
		/// </summary>
		[
		Test,
		SpiraTestCase(406)
		]
		public void _01_Edit_Custom_Lists()
		{
			//First verify that there are no custom lists already in this project
			List<CustomPropertyList> customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId);
			Assert.AreEqual(0, customLists.Count);

			//Now lets add some custom lists to the project, using both overloads
			customListId1 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Operating Systems", true, true).CustomPropertyListId;
			CustomPropertyList newCustomList = new CustomPropertyList() { Name = "Database Servers", IsActive = true, IsSortedOnValue = true, ProjectTemplateId = projectTemplateId };
			customListId2 = customPropertyManager.CustomPropertyList_Add(newCustomList).CustomPropertyListId;
			customListId3 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Web Browsers", false, true).CustomPropertyListId;
			customListId4 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Network Type", true, false).CustomPropertyListId;

			//Verify that we can retrieve the new custom lists (active and inactive)
			customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId);
			Assert.AreEqual(4, customLists.Count);
			//Item 1
			Assert.AreEqual("Operating Systems", customLists[0].Name);
			Assert.AreEqual(true, customLists[0].IsActive);
			Assert.AreEqual(true, customLists[0].IsSortedOnValue);
			//Item 2
			Assert.AreEqual("Database Servers", customLists[1].Name);
			Assert.AreEqual(true, customLists[1].IsActive);
			Assert.AreEqual(true, customLists[1].IsSortedOnValue);
			//Item 3
			Assert.AreEqual("Web Browsers", customLists[2].Name);
			Assert.AreEqual(false, customLists[2].IsActive);
			Assert.AreEqual(true, customLists[2].IsSortedOnValue);
			//Item 4
			Assert.AreEqual("Network Type", customLists[3].Name);
			Assert.AreEqual(true, customLists[3].IsActive);
			Assert.AreEqual(false, customLists[3].IsSortedOnValue);

			//Verify that we can retrieve and update a single item
			CustomPropertyList customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId2);
			Assert.AreEqual("Database Servers", customList.Name);
			Assert.AreEqual(true, customList.IsActive);
			Assert.AreEqual(true, customList.IsSortedOnValue);

			//Make some updates
			customList.StartTracking();
			customList.Name = "Database Platforms";
			customList.IsActive = false;
			customList.IsSortedOnValue = false;
			customPropertyManager.CustomPropertyList_Update(customList);

			//Verify the updates
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId2);
			Assert.AreEqual("Database Platforms", customList.Name);
			Assert.AreEqual(false, customList.IsActive);
			Assert.AreEqual(false, customList.IsSortedOnValue);

			//Now verify the new project list (will include inactive ones)
			customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId);
			Assert.AreEqual(4, customLists.Count);
			//Item 1
			Assert.AreEqual("Operating Systems", customLists[0].Name);
			Assert.AreEqual(true, customLists[0].IsActive);
			Assert.AreEqual(true, customLists[0].IsSortedOnValue);
			//Item 2
			Assert.AreEqual("Database Platforms", customLists[1].Name);
			Assert.AreEqual(false, customLists[1].IsActive);
			Assert.AreEqual(false, customLists[1].IsSortedOnValue);
			//Item 3
			Assert.AreEqual("Web Browsers", customLists[2].Name);
			Assert.AreEqual(false, customLists[2].IsActive);
			Assert.AreEqual(true, customLists[2].IsSortedOnValue);
			//Item 4
			Assert.AreEqual("Network Type", customLists[3].Name);
			Assert.AreEqual(true, customLists[3].IsActive);
			Assert.AreEqual(false, customLists[3].IsSortedOnValue);

			//Now test that we can delete a list
			customPropertyManager.CustomPropertyList_Remove(customListId4);

			//Verify the deletion
			customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId);
			Assert.AreEqual(3, customLists.Count);
			//Item 1
			Assert.AreEqual("Operating Systems", customLists[0].Name);
			Assert.AreEqual(true, customLists[0].IsActive);
			Assert.AreEqual(true, customLists[0].IsSortedOnValue);
			//Item 2
			Assert.AreEqual("Database Platforms", customLists[1].Name);
			Assert.AreEqual(false, customLists[1].IsActive);
			Assert.AreEqual(false, customLists[1].IsSortedOnValue);
			//Item 3
			Assert.AreEqual("Web Browsers", customLists[2].Name);
			Assert.AreEqual(false, customLists[2].IsActive);
			Assert.AreEqual(true, customLists[2].IsSortedOnValue);
			//By Id
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId4);
			Assert.IsNull(customList);

			//Finally verify that we can retrieve all the lists in the project, even those with null names
			customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(projectTemplateId, false, true);
			Assert.AreEqual(3, customLists.Count);
			//Item 1
			Assert.AreEqual("Operating Systems", customLists[0].Name);
			Assert.AreEqual(true, customLists[0].IsActive);
			Assert.AreEqual(true, customLists[0].IsSortedOnValue);
			//Item 2
			Assert.AreEqual("Database Platforms", customLists[1].Name);
			Assert.AreEqual(false, customLists[1].IsActive);
			Assert.AreEqual(false, customLists[1].IsSortedOnValue);
			//Item 3
			Assert.AreEqual("Web Browsers", customLists[2].Name);
			Assert.AreEqual(false, customLists[2].IsActive);
			Assert.AreEqual(true, customLists[2].IsSortedOnValue);

			//Finally need to make all the lists active since we'll be using them later
			customLists[1].StartTracking();
			customLists[1].IsActive = true;
			customPropertyManager.CustomPropertyList_Update(customLists[1]);
			customLists[2].StartTracking();
			customLists[2].IsActive = true;
			customPropertyManager.CustomPropertyList_Update(customLists[2]);
		}

		[
		Test,
		SpiraTestCase(170)
		]
		public void _02_Add_Custom_List_Values()
		{
			//Lets add some values to each of the lists, use both overloads
			//Operating Systems
			customPropertyValueId1 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows XP").CustomPropertyValueId;
			customPropertyValueId2 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 7").CustomPropertyValueId;
			customPropertyValueId3 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 8").CustomPropertyValueId;
			customPropertyValueId4 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "MacOS X").CustomPropertyValueId;
			customPropertyValueId5 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Linux").CustomPropertyValueId;

			//Verify inserts - these should be sorted by value (i.e. name)
			CustomPropertyList customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true);
			Assert.AreEqual(5, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("Windows 7", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Windows 8", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Windows XP", customList.Values[4].Name);
			Assert.AreEqual(true, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);

			//Database Platforms
			CustomPropertyValue newCustomPropertyValue = new CustomPropertyValue() { CustomPropertyListId = customListId2, Name = "SQL Server", IsActive = true };
			customPropertyValueId6 = customPropertyManager.CustomPropertyList_AddValue(customListId2, newCustomPropertyValue).CustomPropertyValueId;
			newCustomPropertyValue = new CustomPropertyValue() { CustomPropertyListId = customListId2, Name = "Oracle", IsActive = true };
			customPropertyValueId7 = customPropertyManager.CustomPropertyList_AddValue(customListId2, newCustomPropertyValue).CustomPropertyValueId;
			newCustomPropertyValue = new CustomPropertyValue() { CustomPropertyListId = customListId2, Name = "MySQL", IsActive = true };
			customPropertyValueId8 = customPropertyManager.CustomPropertyList_AddValue(customListId2, newCustomPropertyValue).CustomPropertyValueId;

			//Verify inserts - sorted by ID
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId2, true);
			Assert.AreEqual(3, customList.Values.Count);
			Assert.AreEqual("SQL Server", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("Oracle", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("MySQL", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);

			//Web Browsers
			List<CustomPropertyValue> newCustomPropertyValues = new List<CustomPropertyValue>();
			newCustomPropertyValues.Add(new CustomPropertyValue() { CustomPropertyListId = customListId3, Name = "IE", IsActive = true });
			newCustomPropertyValues.Add(new CustomPropertyValue() { CustomPropertyListId = customListId3, Name = "Firefox", IsActive = true });
			newCustomPropertyValues.Add(new CustomPropertyValue() { CustomPropertyListId = customListId3, Name = "Safari", IsActive = true });
			newCustomPropertyValues.Add(new CustomPropertyValue() { CustomPropertyListId = customListId3, Name = "Opera", IsActive = true });
			newCustomPropertyValues.Add(new CustomPropertyValue() { CustomPropertyListId = customListId3, Name = "Chrome", IsActive = true });
			customPropertyManager.CustomPropertyList_AddValues(customListId3, newCustomPropertyValues);

			//Verify inserts - these should be sorted by value (i.e. name)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId3, true);
			Assert.AreEqual(5, customList.Values.Count);
			Assert.AreEqual("Chrome", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("Firefox", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("IE", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Opera", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Safari", customList.Values[4].Name);
			Assert.AreEqual(true, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);
		}

		/// <summary>
		/// Tests that you can edit custom property values that belong to a custom list
		/// </summary>
		[
		Test,
		SpiraTestCase(169)
		]
		public void _03_Edit_Custom_List_Values()
		{
			//First we need to get a list of the existing property values and verify
			//We shall get the values for the Operating System Custom List
			CustomPropertyList customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true);
			Assert.AreEqual(5, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("Windows 7", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Windows 8", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Windows XP", customList.Values[4].Name);
			Assert.AreEqual(true, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);

			//Lets add a value and update an existing value
			customList.Values[4].StartTracking();
			customList.Values[4].Name = "Windows Vista";
			customPropertyManager.CustomPropertyList_UpdateValue(customList.Values[4]);
			customPropertyManager.CustomPropertyList_AddValue(customListId1, "UNIX");

			//Verify the changes
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual(true, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);
			Assert.AreEqual(true, customList.Values[5].IsActive);
			Assert.AreEqual(false, customList.Values[5].IsDeleted);

			//Lets make one of the values inactive
			customList.Values[4].StartTracking();
			customList.Values[4].IsActive = false;
			customPropertyManager.CustomPropertyList_UpdateValue(customList.Values[4]);

			//Verify the changes

			//First get all non-deleted values (active/inactive)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, true, false);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual(false, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);
			Assert.AreEqual(true, customList.Values[5].IsActive);
			Assert.AreEqual(false, customList.Values[5].IsDeleted);

			//Now get all active values
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, false);
			Assert.AreEqual(5, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual(true, customList.Values[0].IsActive);
			Assert.AreEqual(false, customList.Values[0].IsDeleted);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual(true, customList.Values[1].IsActive);
			Assert.AreEqual(false, customList.Values[1].IsDeleted);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual(true, customList.Values[2].IsActive);
			Assert.AreEqual(false, customList.Values[2].IsDeleted);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual(true, customList.Values[3].IsActive);
			Assert.AreEqual(false, customList.Values[3].IsDeleted);
			Assert.AreEqual("Windows Vista", customList.Values[4].Name);
			Assert.AreEqual(true, customList.Values[4].IsActive);
			Assert.AreEqual(false, customList.Values[4].IsDeleted);

			//Now put it back
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, true, false);
			customList.Values[4].StartTracking();
			customList.Values[4].IsActive = true;
			customPropertyManager.CustomPropertyList_UpdateValue(customList.Values[4]);

			//Verify the change
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, false);
			Assert.AreEqual(6, customList.Values.Count);

			//Lets delete one of the values
			customPropertyManager.CustomPropertyList_DeleteValue(customList.Values[0].CustomPropertyValueId);

			//Verify the changes

			//(only active)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, false);
			Assert.AreEqual(5, customList.Values.Count);
			Assert.AreEqual("MacOS X", customList.Values[0].Name);
			Assert.AreEqual("UNIX", customList.Values[1].Name);
			Assert.AreEqual("Windows 7", customList.Values[2].Name);
			Assert.AreEqual("Windows 8", customList.Values[3].Name);
			Assert.AreEqual("Windows Vista", customList.Values[4].Name);

			//All (including deleted)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, true);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);

			//Now do an undelete
			customPropertyManager.CustomPropertyList_UndeleteValue(customList.Values[0].CustomPropertyValueId);

			//Verify the changes

			//(only active)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, false);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);

			//All (including deleted)
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true, false, true);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("Linux", customList.Values[0].Name);
			Assert.AreEqual("MacOS X", customList.Values[1].Name);
			Assert.AreEqual("UNIX", customList.Values[2].Name);
			Assert.AreEqual("Windows 7", customList.Values[3].Name);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);

			//Now re-delete it again (subsequent tests assume this is the case)
			customPropertyManager.CustomPropertyList_DeleteValue(customList.Values[0].CustomPropertyValueId);
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true);

			//Finally lets verify that we can do a bulk update
			//We do not call StartTracking() because this collection does not have to use a trackable collection
			List<CustomPropertyValue> replacementValues = new List<CustomPropertyValue>();
			foreach (CustomPropertyValue cpv in customList.Values)
			{
				replacementValues.Add(cpv);
			}
			replacementValues[2].Name = "Windows 7 (x64)";
			replacementValues.Add(new CustomPropertyValue() { Name = "Windows 7 (x86)", IsActive = true });
			customPropertyManager.CustomPropertyList_UpdateValues(customListId1, replacementValues);

			//Verify the changes
			customList = customPropertyManager.CustomPropertyList_RetrieveById(customListId1, true);
			Assert.AreEqual(6, customList.Values.Count);
			Assert.AreEqual("MacOS X", customList.Values[0].Name);
			Assert.AreEqual("UNIX", customList.Values[1].Name);
			Assert.AreEqual("Windows 7 (x64)", customList.Values[2].Name);
			Assert.AreEqual("Windows 7 (x86)", customList.Values[3].Name);
			Assert.AreEqual("Windows 8", customList.Values[4].Name);
			Assert.AreEqual("Windows Vista", customList.Values[5].Name);
		}

		[
		Test,
		SpiraTestCase(168)
		]
		public void _04_Edit_Project_Custom_Properties()
		{
			//First verify that we don't have any custom properties defined in this project for all artifact types

			//Get the list of artifact types
			List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();
			List<CustomProperty> customProperties;
			foreach (ArtifactType artifactTypeRow in artifactTypes)
			{
				//First verify by ID
				int artifactTypeId = artifactTypeRow.ArtifactTypeId;
				customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeId(projectTemplateId, artifactTypeId, false);
				Assert.AreEqual(0, customProperties.Count);

				//Now verify by enumeration
				Artifact.ArtifactTypeEnum artifactType = (Artifact.ArtifactTypeEnum)artifactTypeId;
				customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false);
				Assert.AreEqual(0, customProperties.Count);
			}

			//Now lets add some custom properties to artifacts - we test all the types and options exhaustively in a separate test

			#region Requirements
			//Text field - Notes
			customPropertyId1 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Requirement,
					(int)CustomProperty.CustomPropertyTypeEnum.Text,
					1,
					"Notes",
					"Test Help 1RQ",
					1,
					null)
				.CustomPropertyId;
			//List field - Operating System
			customPropertyId2 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Requirement,
					(int)CustomProperty.CustomPropertyTypeEnum.List,
					65,
					"OS",
					"Test Help 2RQ",
					null,
					customListId1)
				.CustomPropertyId;

			//Verify the inserts
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Notes", customProperties[0].Name);
			Assert.AreEqual(1, customProperties[0].PropertyNumber);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual(1, customProperties[0].Position);
			Assert.AreEqual("Test Help 1RQ", customProperties[0].Description);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual(65, customProperties[1].PropertyNumber);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual(null, customProperties[1].Position);
			Assert.AreEqual("Test Help 2RQ", customProperties[1].Description);
			#endregion Requirements

			#region Test Cases
			//User field - Reviewer
			customPropertyId3 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.TestCase,
					(int)CustomProperty.CustomPropertyTypeEnum.User,
					1,
					"Reviewer",
					"Test Help 1TC",
					null,
					null)
				.CustomPropertyId;
			//MultiList field - Web Browsers
			customPropertyId4 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.TestCase,
					(int)CustomProperty.CustomPropertyTypeEnum.MultiList,
					98,
					"Browser",
					"What Browser did you use",
					103,
					customListId3)
				.CustomPropertyId;

			//Verify the inserts
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Reviewer", customProperties[0].Name);
			Assert.AreEqual(1, customProperties[0].PropertyNumber);
			Assert.AreEqual("User", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("Test Help 1TC", customProperties[0].Description);
			Assert.AreEqual(null, customProperties[0].Position);
			Assert.AreEqual("Browser", customProperties[1].Name);
			Assert.AreEqual(98, customProperties[1].PropertyNumber);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual("What Browser did you use", customProperties[1].Description);
			Assert.AreEqual(null, customProperties[1].Position); //Saved 103, which should have been cleared because, invalid.
			#endregion Test Cases

			#region Incidents
			//Date field - Review Date
			customPropertyId5 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Incident,
					(int)CustomProperty.CustomPropertyTypeEnum.Date,
					1,
					"Review Date",
					null,
					12,
					null)
				.CustomPropertyId;
			//List field - Database
			customPropertyId6 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Incident,
					(int)CustomProperty.CustomPropertyTypeEnum.List,
					76,
					"Database",
					"IN Help 1",
					13,
					customListId2)
				.CustomPropertyId;

			//Verify the inserts
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Review Date", customProperties[0].Name);
			Assert.AreEqual(1, customProperties[0].PropertyNumber);
			Assert.AreEqual("Date", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual(null, customProperties[0].Description);
			Assert.AreEqual(12, customProperties[0].Position);
			Assert.AreEqual("Database", customProperties[1].Name);
			Assert.AreEqual(76, customProperties[1].PropertyNumber);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual("IN Help 1", customProperties[1].Description);
			Assert.AreEqual(13, customProperties[1].Position);
			#endregion Incidents

			#region Releases
			//Decimal field - Function Points
			customPropertyId7 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId, Artifact.ArtifactTypeEnum.Release,
					(int)CustomProperty.CustomPropertyTypeEnum.Decimal,
					1,
					"Function Points",
					null,
					55,
					null)
				.CustomPropertyId;
			//MultiList field - Operating System
			customPropertyId8 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId, Artifact.ArtifactTypeEnum.Release,
					(int)CustomProperty.CustomPropertyTypeEnum.MultiList,
					54,
					"OS",
					null,
					56,
					customListId1)
				.CustomPropertyId;

			//Verify the inserts
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Function Points", customProperties[0].Name);
			Assert.AreEqual(1, customProperties[0].PropertyNumber);
			Assert.AreEqual("Decimal", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual(null, customProperties[0].Description);
			Assert.AreEqual(55, customProperties[0].Position);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual(54, customProperties[1].PropertyNumber);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual(null, customProperties[1].Description);
			Assert.AreEqual(56, customProperties[1].Position);
			#endregion Releases

			#region Test Sets
			//MultiList field - Web Browsers
			customPropertyId9 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId, Artifact.ArtifactTypeEnum.TestSet,
					(int)CustomProperty.CustomPropertyTypeEnum.MultiList,
					99,
					"Browser",
					"",
					99,
					customListId3)
				.CustomPropertyId;
			//List field - Operating System
			customPropertyId10 = customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId, Artifact.ArtifactTypeEnum.TestSet,
					(int)CustomProperty.CustomPropertyTypeEnum.List,
					31,
					"OS",
					null,
					89,
					customListId1)
				.CustomPropertyId;

			//Verify the inserts
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Browser", customProperties[1].Name);
			Assert.AreEqual(99, customProperties[1].PropertyNumber);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual("", customProperties[1].Description);
			Assert.AreEqual(99, customProperties[1].Position);
			Assert.AreEqual("OS", customProperties[0].Name);
			Assert.AreEqual(31, customProperties[0].PropertyNumber);
			Assert.AreEqual("List", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual(null, customProperties[0].Description);
			Assert.AreEqual(89, customProperties[0].Position);
			#endregion Test Sets

			//Now lets change the requirement custom properties (update one item and add one item)
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId1);
			customProperty.StartTracking();
			customProperty.Name = "URL";
			customPropertyManager.CustomPropertyDefinition_Update(customProperty);
			//We add the item using the alternate overload that we didn't use previously
			customProperty = new CustomProperty
			{
				Name = "Browser",
				ProjectTemplateId = projectTemplateId,
				CustomPropertyTypeId = (int)CustomProperty.CustomPropertyTypeEnum.MultiList,
				CustomPropertyListId = customListId3,
				PropertyNumber = 6699,
				ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Requirement
			};
			int newCustomPropertyId = customPropertyManager.CustomPropertyDefinition_AddToArtifact(customProperty).CustomPropertyId;

			//Verify the changes
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(3, customProperties.Count);
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual("Browser", customProperties[2].Name);
			Assert.IsNull(customProperties[2].Position, "Invalid position was not cleared!");
			Assert.AreEqual("MultiList", customProperties[2].CustomPropertyTypeName);

			//Now lets make one of the custom properties inactive (i.e. soft delete)
			customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(newCustomPropertyId);
			customProperty.StartTracking();
			customProperty.IsDeleted = true;
			customPropertyManager.CustomPropertyDefinition_Update(customProperty);

			//Verify the changes
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);

			//Verify that it still appears when you include deleted items
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false, true);
			Assert.AreEqual(3, customProperties.Count);
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			Assert.AreEqual("Browser", customProperties[2].Name);
			Assert.AreEqual("MultiList", customProperties[2].CustomPropertyTypeName);

			//Finally lets physically delete one of the custom properties
			customPropertyManager.CustomPropertyDefinition_RemoveById(newCustomPropertyId);

			//Verify the changes
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);

			//Verify that it no longer appears when you include deleted items
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false, true);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);

			//Now lets try some of the other delete operations, first some new custom properties to Tasks
			customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Task,
					(int)CustomProperty.CustomPropertyTypeEnum.Text,
					1,
					"Notes",
					null,
					null,
					null);
			customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Task,
					(int)CustomProperty.CustomPropertyTypeEnum.Integer,
					30,
					"Age",
					null,
					null,
					null);
			customPropertyManager
				.CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.Task,
					(int)CustomProperty.CustomPropertyTypeEnum.Decimal,
					31,
					"Weighting",
					null,
					null,
					null);

			//Verify that they exist
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			Assert.AreEqual(3, customProperties.Count);
			Assert.AreEqual("Notes", customProperties[0].Name);
			Assert.AreEqual("Age", customProperties[1].Name);
			Assert.AreEqual("Weighting", customProperties[2].Name);

			//Now delete the first one
			customPropertyManager.CustomPropertyDefinition_RemoveFromArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Task, 1);

			//Verify the delete
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			Assert.AreEqual(2, customProperties.Count);
			Assert.AreEqual("Age", customProperties[0].Name);
			Assert.AreEqual("Weighting", customProperties[1].Name);

			//Now delete the remainder
			customPropertyManager.CustomPropertyDefinition_RemoveAllFromArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Task);
			//Verify the delete
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			Assert.AreEqual(0, customProperties.Count);
		}

		/// <summary>
		/// Tests that we can retrieve the existing custom properties for each artifact type as well as custom properties
		/// for all projects using that type and also for individual property numbers and custom property ids
		/// </summary>
		[
		Test,
		SpiraTestCase(161)
		]
		public void _05_Retrieves()
		{
			//Retrieve the custom properties for a requirement and verify that we have the expected properties, lists and list values
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true);
			Assert.AreEqual(2, customProperties.Count);
			//URL
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			//OS
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Operating Systems", customProperties[1].List.Name);
			Assert.AreEqual(6, customProperties[1].List.Values.Count);
			List<CustomPropertyValue> customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("MacOS X", customPropertyListValues[0].Name);
			Assert.AreEqual("UNIX", customPropertyListValues[1].Name);
			Assert.AreEqual("Windows 7 (x64)", customPropertyListValues[2].Name);
			Assert.AreEqual("Windows 7 (x86)", customPropertyListValues[3].Name);
			Assert.AreEqual("Windows 8", customPropertyListValues[4].Name);
			Assert.AreEqual("Windows Vista", customPropertyListValues[5].Name);

			//Retrieve the custom properties for a test case and verify that we have the expected properties, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true);
			Assert.AreEqual(2, customProperties.Count);
			//Reviewer
			Assert.AreEqual("Reviewer", customProperties[0].Name);
			Assert.AreEqual("User", customProperties[0].CustomPropertyTypeName);
			//Browser
			Assert.AreEqual("Browser", customProperties[1].Name);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Web Browsers", customProperties[1].List.Name);
			Assert.AreEqual(5, customProperties[1].List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("Chrome", customPropertyListValues[0].Name);
			Assert.AreEqual("Firefox", customPropertyListValues[1].Name);
			Assert.AreEqual("IE", customPropertyListValues[2].Name);
			Assert.AreEqual("Opera", customPropertyListValues[3].Name);
			Assert.AreEqual("Safari", customPropertyListValues[4].Name);

			//Retrieve the custom properties for an incident and verify that we have the expected properties, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true);
			Assert.AreEqual(2, customProperties.Count);
			//Review Date
			Assert.AreEqual("Review Date", customProperties[0].Name);
			Assert.AreEqual("Date", customProperties[0].CustomPropertyTypeName);
			//Database
			Assert.AreEqual("Database", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Database Platforms", customProperties[1].List.Name);
			Assert.AreEqual(3, customProperties[1].List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("SQL Server", customPropertyListValues[0].Name);
			Assert.AreEqual("Oracle", customPropertyListValues[1].Name);
			Assert.AreEqual("MySQL", customPropertyListValues[2].Name);

			//Retrieve the custom properties for a release and verify that we have the expected properties, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true);
			Assert.AreEqual(2, customProperties.Count);
			//Review Date
			Assert.AreEqual("Function Points", customProperties[0].Name);
			Assert.AreEqual("Decimal", customProperties[0].CustomPropertyTypeName);
			//Database
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Operating Systems", customProperties[1].List.Name);
			Assert.AreEqual(6, customProperties[1].List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("MacOS X", customPropertyListValues[0].Name);
			Assert.AreEqual("UNIX", customPropertyListValues[1].Name);
			Assert.AreEqual("Windows 7 (x64)", customPropertyListValues[2].Name);
			Assert.AreEqual("Windows 7 (x86)", customPropertyListValues[3].Name);
			Assert.AreEqual("Windows 8", customPropertyListValues[4].Name);
			Assert.AreEqual("Windows Vista", customPropertyListValues[5].Name);

			//Now verify that we can retrieve all the custom properties for an artifact type across all projects
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(5, customProperties.Count);
			Assert.IsTrue(customProperties.Any(cp => cp.ProjectTemplateId == projectTemplateId));
			Assert.IsTrue(customProperties.Any(cp => cp.ProjectTemplateId != projectTemplateId));

			//Now verify that we can retrieve a single custom property by its id
			//Retrieve the custom properties for a requirement and verify that we have the expected properties, lists and list values
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveById(customPropertyId4);
			Assert.AreEqual("Browser", customProperty.Name);
			Assert.AreEqual("MultiList", customProperty.CustomPropertyTypeName);
			Assert.AreEqual(customListId3, customProperty.CustomPropertyListId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, customProperty.ArtifactTypeId);
			Assert.AreEqual(98, customProperty.PropertyNumber);

			//Now verify that we can retrieve a single custom property by its position number
			customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, 98, true);
			Assert.AreEqual("Browser", customProperty.Name);
			Assert.AreEqual("MultiList", customProperty.CustomPropertyTypeName);
			Assert.AreEqual(customListId3, customProperty.CustomPropertyListId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, customProperty.ArtifactTypeId);
			Assert.AreEqual(98, customProperty.PropertyNumber);
			//List and Values
			Assert.AreEqual("Web Browsers", customProperty.List.Name);
			Assert.AreEqual(5, customProperty.List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperty.List);
			Assert.AreEqual("Chrome", customPropertyListValues[0].Name);
			Assert.AreEqual("Firefox", customPropertyListValues[1].Name);
			Assert.AreEqual("IE", customPropertyListValues[2].Name);
			Assert.AreEqual("Opera", customPropertyListValues[3].Name);
			Assert.AreEqual("Safari", customPropertyListValues[4].Name);
		}

		/// <summary>
		/// Test that we can retrieve the list of custom properties that should be displayed in the Incidents list
		/// </summary>
		[
		Test,
		SpiraTestCase(259)
		]
		public void _06_ConfigureArtifactTypeFields()
		{
			//Initially we should not have any of the custom properties visible, only the standard fields
			//We do this for Fred Bloggs. We use the ArtifactManager to get access to all fields
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactListFieldDisplay> artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);

			//Verify that we have no custom properties initially visible
			Assert.AreEqual(25, artifactListFields.Count);
			Assert.IsFalse(artifactListFields.Any(f => f.Name.Contains(CustomProperty.FIELD_PREPEND) && f.IsVisible));

			//Now let's make one of the custom properties visible
			customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "Custom_01");

			//Verify that we have one of the custom properties visible
			artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			ArtifactListFieldDisplay artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_01" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_01", artifactListField.Name);
			Assert.AreEqual("Review Date", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(51, artifactListField.ListPosition);

			//Now let's make the other custom properties visible
			customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "Custom_76");

			//Verify that we have both custom properties visible
			artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			//Custom_01
			artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_01" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_01", artifactListField.Name);
			Assert.AreEqual("Review Date", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(51, artifactListField.ListPosition);
			//Custom_02
			artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_76" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_76", artifactListField.Name);
			Assert.AreEqual("Database", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(126, artifactListField.ListPosition);

			//Verify that they're still hidden for another user
			artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_JOE_SMITH, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(25, artifactListFields.Count);
			Assert.IsFalse(artifactListFields.Any(f => f.Name.Contains(CustomProperty.FIELD_PREPEND) && f.IsVisible));

			//Now verify that we can change the order of the fields for our first user
			artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "Custom_76", 51);

			//Verify the change
			artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			//Custom_01
			artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_01" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_01", artifactListField.Name);
			Assert.AreEqual("Review Date", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(52, artifactListField.ListPosition);
			//Custom_02
			artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_76" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_76", artifactListField.Name);
			Assert.AreEqual("Database", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(51, artifactListField.ListPosition);

			//Verify that you can hide one of the fields
			customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "Custom_76");

			//Verify the change
			artifactListFields = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			artifactListField = artifactListFields.FirstOrDefault(f => f.Name == "Custom_01" && f.IsVisible);
			Assert.IsNotNull(artifactListField);
			Assert.AreEqual("Custom_01", artifactListField.Name);
			Assert.AreEqual("Review Date", artifactListField.Caption);
			Assert.AreEqual(true, artifactListField.IsVisible);
			Assert.AreEqual(52, artifactListField.ListPosition);

			//Verify that you can delete a custom properties that has been added to the visible field list
			//We will create a new custom property for this purpose
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.Text, 3, "Test Prop", null, null, null);
			customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "Custom_03");
			customPropertyManager.CustomPropertyDefinition_RemoveFromArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, 3);

			//Verify the deletion
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, 3, false);
			Assert.IsNull(customProperty);
		}

		[
		Test,
		SpiraTestCase(162)
		]
		public void _07_Edit_Requirement_Custom_Properties()
		{
			//We need to test the ability to set a default custom property value, so lets create a default value on
			//both of the custom properties
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId1, (int)CustomProperty.CustomPropertyOptionEnum.Default, "http://www.x.com");
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId2, (int)CustomProperty.CustomPropertyOptionEnum.Default, customPropertyValueId4.ToDatabaseSerialization());

			//We need to create a new requirement
			RequirementManager requirement = new RequirementManager();
			int requirementId = requirement.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement with Custom Properties", null, 120, USER_ID_FRED_BLOGGS);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Verify that the default values were populated using both access methods
			Assert.AreEqual("http://www.x.com", (string)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual("http://www.x.com", artifactCustomProperty.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(customPropertyValueId4, (int)artifactCustomProperty.CustomProperty(65));
			Assert.AreEqual(customPropertyValueId4, artifactCustomProperty.Custom_65.FromDatabaseSerialization_Int32());

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
			Assert.AreEqual("http://www.x.com", (string)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual("http://www.x.com", artifactCustomProperty.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(customPropertyValueId4, (int)artifactCustomProperty.CustomProperty(65));
			Assert.AreEqual(customPropertyValueId4, artifactCustomProperty.Custom_65.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(requirementId, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//TrackableCollection<HistoryDetail> artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//URL
			//Assert.AreEqual("URL", historyChangeSets[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			Assert.AreEqual("Requirement with Custom Properties", historyChangeSets[0].NewValue);
			//OS
			//Assert.AreEqual("OS", historyChangeSets[1].FieldCaption);
			Assert.IsTrue(historyChangeSets[1].OldValue.IsNull());
			Assert.AreEqual(customPropertyValueId4.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[1].NewValue);

			//Now make a change to the properties, use the two different methods for setting a value
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.Custom_01 = "http://www.y.com".ToDatabaseSerialization();
			artifactCustomProperty.SetCustomProperty(65, customPropertyValueId5);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_JOE_SMITH);

			//Verify the changes (this time retrieve using the option to retrieve the custom property definitions)
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, Artifact.ArtifactTypeEnum.Requirement, true);
			Assert.AreEqual("http://www.y.com", (string)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual("http://www.y.com", artifactCustomProperty.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(customPropertyValueId5, (int)artifactCustomProperty.CustomProperty(65));
			Assert.AreEqual(customPropertyValueId5, artifactCustomProperty.Custom_65.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			historyChangeSets = historyManager.RetrieveByArtifactId(requirementId, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//URL
			//Assert.AreEqual("URL", artifactHistoryRows[0].FieldCaption);
			//Assert.AreEqual("http://www.x.com", historyChangeSets[0].OldValue);
			//Assert.AreEqual("http://www.y.com", historyChangeSets[0].NewValue);
			//OS
			//Assert.AreEqual("OS", historyChangeSets[1].FieldCaption);
			Assert.AreEqual(customPropertyValueId4.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[2].OldValue);
			Assert.AreEqual(customPropertyValueId5.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[2].NewValue);

			//Finally verify that we can delete the artifact and it deletes the associated custom property entry.
			requirement.DeleteFromDatabase(requirementId, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
			Assert.IsNull(artifactCustomProperty);
		}

		[
		Test,
		SpiraTestCase(163)
		]
		public void _08_Edit_TestCase_Custom_Properties()
		{
			//We need to test the ability to set a default custom property value, so lets create a default value on
			//one of the custom properties
			List<CustomPropertyValue> customListValues = customPropertyManager.CustomPropertyList_RetrieveById(customListId3, true).Values.ToList();
			List<int> defaultBrowsers = new List<int>();
			defaultBrowsers.Add(customListValues[0].CustomPropertyValueId);
			defaultBrowsers.Add(customListValues[1].CustomPropertyValueId);
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId4, (int)CustomProperty.CustomPropertyOptionEnum.Default, defaultBrowsers.ToDatabaseSerialization());

			//We need to create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case with Custom Properties", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Verify that the default values were populated using both access methods
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(98)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_98.FromDatabaseSerialization_List_Int32().Count);
			List<int> browsers = (List<int>)artifactCustomProperty.CustomProperty(98);
			Assert.IsTrue(browsers.Contains(customListValues[0].CustomPropertyValueId));
			Assert.IsTrue(browsers.Contains(customListValues[1].CustomPropertyValueId));

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(98)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_98.FromDatabaseSerialization_List_Int32().Count);
			browsers = (List<int>)artifactCustomProperty.CustomProperty(98);
			Assert.IsTrue(browsers.Contains(customListValues[0].CustomPropertyValueId));
			Assert.IsTrue(browsers.Contains(customListValues[1].CustomPropertyValueId));

			//Verify that the history entries were recorded correctly
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(testCaseId, Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//TrackableCollection<HistoryDetail> artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(1, artifactHistoryRows.Count);
			//Browsers
			//Assert.AreEqual("Browser", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(browsers.ToDatabaseSerialization(), historyChangeSets[0].NewValue);

			//Now make a change to the properties, use the two different methods for setting a value
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.Custom_01 = USER_ID_JOE_SMITH.ToDatabaseSerialization();
			List<int> browsers2 = new List<int>();
			browsers2.Add(customListValues[1].CustomPropertyValueId);
			browsers2.Add(customListValues[2].CustomPropertyValueId);
			artifactCustomProperty.SetCustomProperty(98, browsers2);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_JOE_SMITH);

			//Verify the changes (this time retrieve using the option to retrieve the custom property definitions)
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, true);
			Assert.AreEqual(USER_ID_JOE_SMITH, (int)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual(USER_ID_JOE_SMITH, artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(98)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_98.FromDatabaseSerialization_List_Int32().Count);
			browsers2 = (List<int>)artifactCustomProperty.CustomProperty(98);
			Assert.IsTrue(browsers2.Contains(customListValues[1].CustomPropertyValueId));
			Assert.IsTrue(browsers2.Contains(customListValues[2].CustomPropertyValueId));

			//Verify that the history entries were recorded correctly
			historyChangeSets = historyManager.RetrieveByArtifactId(testCaseId, Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//Reviewer
			//Assert.AreEqual("Reviewer", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(USER_ID_JOE_SMITH.ToDatabaseSerialization(), historyChangeSets[0].NewValue);
			//Browsers
			//Assert.AreEqual("Browser", artifactHistoryRows[1].FieldCaption);
			//Assert.AreEqual(browsers.ToDatabaseSerialization(), historyChangeSets[1].OldValue);
			//Assert.AreEqual(browsers2.ToDatabaseSerialization(), historyChangeSets[1].NewValue);

			//Finally verify that we can delete the artifact and it deletes the associated custom property entry.
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
			Assert.IsNull(artifactCustomProperty);
		}

		[
		Test,
		SpiraTestCase(164)
		]
		public void _09_Edit_Incident_Custom_Properties()
		{
			//We need to test the ability to set a default custom property value, so lets create a default value on
			//both of the custom properties
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId5, (int)CustomProperty.CustomPropertyOptionEnum.Default, DateTime.Parse("1/5/2010").ToDatabaseSerialization());
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId6, (int)CustomProperty.CustomPropertyOptionEnum.Default, customPropertyValueId7.ToDatabaseSerialization());

			//We need to create a new incident
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident with Custom Properties", "Incident Description", null, null, null, 1, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Verify that the default values were populated using both access methods
			Assert.AreEqual(DateTime.Parse("1/5/2010"), (DateTime)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual(DateTime.Parse("1/5/2010"), artifactCustomProperty.Custom_01.FromDatabaseSerialization_DateTime());
			Assert.AreEqual(customPropertyValueId7, (int)artifactCustomProperty.CustomProperty(76));
			Assert.AreEqual(customPropertyValueId7, artifactCustomProperty.Custom_76.FromDatabaseSerialization_Int32());

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, Artifact.ArtifactTypeEnum.Incident, false, customProperties);
			Assert.AreEqual(DateTime.Parse("1/5/2010"), (DateTime)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual(DateTime.Parse("1/5/2010"), artifactCustomProperty.Custom_01.FromDatabaseSerialization_DateTime());
			Assert.AreEqual(customPropertyValueId7, (int)artifactCustomProperty.CustomProperty(76));
			Assert.AreEqual(customPropertyValueId7, artifactCustomProperty.Custom_76.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			HistoryManager history = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = history.RetrieveByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//TrackableCollection<HistoryDetail> artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//Review Date
			//Assert.AreEqual("Review Date", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(DateTime.Parse("1/5/2010").ToDatabaseSerialization(), historyChangeSets[0].NewValue);
			//Database
			//Assert.AreEqual("Database", artifactHistoryRows[1].FieldCaption);
			Assert.IsTrue(historyChangeSets[1].OldValue.IsNull());
			//Assert.AreEqual(customPropertyValueId7.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[1].NewValue);

			//Now make a change to the properties, use the two different methods for setting a value
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.Custom_01 = DateTime.Parse("2/15/2011").ToDatabaseSerialization();
			artifactCustomProperty.SetCustomProperty(76, customPropertyValueId8);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_JOE_SMITH);

			//Verify the changes (this time retrieve using the option to retrieve the custom property definitions)
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, Artifact.ArtifactTypeEnum.Incident, true);
			Assert.AreEqual(DateTime.Parse("2/15/2011"), (DateTime)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual(DateTime.Parse("2/15/2011"), artifactCustomProperty.Custom_01.FromDatabaseSerialization_DateTime());
			Assert.AreEqual(customPropertyValueId8, (int)artifactCustomProperty.CustomProperty(76));
			Assert.AreEqual(customPropertyValueId8, artifactCustomProperty.Custom_76.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			historyChangeSets = history.RetrieveByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//Review Date
			//Assert.AreEqual("Review Date", historyChangeSets[0].FieldCaption);
			//Assert.AreEqual(DateTime.Parse("1/5/2010").ToDatabaseSerialization(), historyChangeSets[0].OldValue);
			//Assert.AreEqual(DateTime.Parse("2/15/2011").ToDatabaseSerialization(), historyChangeSets[0].NewValue);
			//Database
			//Assert.AreEqual("Database", artifactHistoryRows[1].FieldCaption);
			//Assert.AreEqual(customPropertyValueId7.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[1].OldValue);
			//Assert.AreEqual(customPropertyValueId8.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), historyChangeSets[1].NewValue);

			//Finally verify that we can delete the artifact and it deletes the associated custom property entry.
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, Artifact.ArtifactTypeEnum.Incident, false, customProperties);
			Assert.IsNull(artifactCustomProperty);
		}

		[
		Test,
		SpiraTestCase(165)
		]
		public void _10_Edit_Release_Custom_Properties()
		{
			//We need to test the ability to set a default custom property value, so lets create a default value on
			//one of the custom properties
			List<int> defaultOperatingSystems = new List<int>();
			defaultOperatingSystems.Add(customPropertyValueId3);
			defaultOperatingSystems.Add(customPropertyValueId4);
			customPropertyManager.CustomPropertyDefinitionOptions_Add(customPropertyId8, (int)CustomProperty.CustomPropertyOptionEnum.Default, defaultOperatingSystems.ToDatabaseSerialization());

			//We need to create a new release
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release with Custom Properties", null, "1.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2, 0, null, false);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, releaseId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Verify that the default values were populated using both access methods
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Decimal());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(54)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_54.FromDatabaseSerialization_List_Int32().Count);
			List<int> operatingSystems = (List<int>)artifactCustomProperty.CustomProperty(54);
			Assert.IsTrue(operatingSystems.Contains(customPropertyValueId3));
			Assert.IsTrue(operatingSystems.Contains(customPropertyValueId3));

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, Artifact.ArtifactTypeEnum.Release, false, customProperties);
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(54)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_54.FromDatabaseSerialization_List_Int32().Count);
			operatingSystems = (List<int>)artifactCustomProperty.CustomProperty(54);
			Assert.IsTrue(operatingSystems.Contains(customPropertyValueId3));
			Assert.IsTrue(operatingSystems.Contains(customPropertyValueId4));

			//Verify that the history entries were recorded correctly
			HistoryManager history = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = history.RetrieveByArtifactId(releaseId, Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//TrackableCollection<HistoryDetail> artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(1, artifactHistoryRows.Count);
			//OS
			//Assert.AreEqual("OS", historyChangeSets[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(operatingSystems.ToDatabaseSerialization(), historyChangeSets[0].NewValue);

			//Now make a change to the properties, use the two different methods for setting a value
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.Custom_01 = ((decimal)1.2).ToDatabaseSerialization();
			List<int> operatingSystems2 = new List<int>();
			operatingSystems2.Add(customPropertyValueId2);
			operatingSystems2.Add(customPropertyValueId3);
			artifactCustomProperty.SetCustomProperty(54, operatingSystems2);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_JOE_SMITH);

			//Verify the changes (this time retrieve using the option to retrieve the custom property definitions)
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, Artifact.ArtifactTypeEnum.Release, true);
			Assert.AreEqual((decimal)1.2, (decimal)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual((decimal)1.2, artifactCustomProperty.Custom_01.FromDatabaseSerialization_Decimal());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(54)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_54.FromDatabaseSerialization_List_Int32().Count);
			operatingSystems2 = (List<int>)artifactCustomProperty.CustomProperty(54);
			Assert.IsTrue(operatingSystems2.Contains(customPropertyValueId2));
			Assert.IsTrue(operatingSystems2.Contains(customPropertyValueId3));

			//Verify that the history entries were recorded correctly
			historyChangeSets = history.RetrieveByArtifactId(releaseId, Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//Function Points
			//Assert.AreEqual("Function Points", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(((decimal)1.2).ToDatabaseSerialization(), historyChangeSets[0].NewValue);
			//OS
			//Assert.AreEqual("OS", artifactHistoryRows[1].FieldCaption);
			//Assert.AreEqual(operatingSystems.ToDatabaseSerialization(), historyChangeSets[1].OldValue);
			//Assert.AreEqual(operatingSystems2.ToDatabaseSerialization(), historyChangeSets[1].NewValue);

			//Finally verify that we can delete the artifact and it deletes the associated custom property entry.
			releaseManager.DeleteFromDatabase(releaseId, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, Artifact.ArtifactTypeEnum.Release, false, customProperties);
			Assert.IsNull(artifactCustomProperty);
		}

		[
		Test,
		SpiraTestCase(349)
		]
		public void _11_Edit_TestSet_Custom_Properties()
		{
			//We need to test the ability to set a default custom property value, so lets create a default value on
			//one of the custom properties
			customPropertyManager.CustomPropertyDefinitionOptions_Add(
				customPropertyId10,
				(int)CustomProperty.CustomPropertyOptionEnum.Default,
				customPropertyValueId3.ToDatabaseSerialization());

			//Next lets create a new test set
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				TestSet.TestSetStatusEnum.NotStarted,
				"Test Set 1",
				null,
				null,
				TestRun.TestRunTypeEnum.Manual,
				null,
				null);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(
				projectTemplateId,
				Artifact.ArtifactTypeEnum.TestSet,
				false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager
				.ArtifactCustomProperty_CreateNew(
					projectId,
					Artifact.ArtifactTypeEnum.TestSet,
					testSetId,
					customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Verify that the default values were populated using both access methods
			Assert.IsNull(artifactCustomProperty.CustomProperty(99));
			Assert.AreEqual(0, artifactCustomProperty.Custom_99.FromDatabaseSerialization_List_Int32().Count);
			Assert.AreEqual(customPropertyValueId3, (int)artifactCustomProperty.CustomProperty(31));
			Assert.AreEqual(customPropertyValueId3, artifactCustomProperty.Custom_31.FromDatabaseSerialization_Int32());

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager
				.ArtifactCustomProperty_RetrieveByArtifactId(
					projectId,
					projectTemplateId,
					testSetId,
					Artifact.ArtifactTypeEnum.TestSet,
					false,
					customProperties);
			Assert.IsNull(artifactCustomProperty.CustomProperty(99));
			Assert.AreEqual(0, artifactCustomProperty.Custom_99.FromDatabaseSerialization_List_Int32().Count);
			Assert.AreEqual(customPropertyValueId3, (int)artifactCustomProperty.CustomProperty(31));
			Assert.AreEqual(customPropertyValueId3, artifactCustomProperty.Custom_31.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			HistoryManager history = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = history.RetrieveByArtifactId(testSetId, Artifact.ArtifactTypeEnum.TestSet);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//TrackableCollection<HistoryDetail> artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(1, artifactHistoryRows.Count);
			//OS
			//Assert.AreEqual("OS", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(customPropertyValueId3.ToDatabaseSerialization(), historyChangeSets[0].NewValue);

			//Now make a change to the properties, use the two different methods for setting a value
			artifactCustomProperty.StartTracking();
			List<CustomPropertyValue> customListValues = customPropertyManager.CustomPropertyList_RetrieveById(customListId3, true).Values.ToList();
			List<int> browsers = new List<int>
			{
				customListValues[1].CustomPropertyValueId,
				customListValues[2].CustomPropertyValueId
			};
			artifactCustomProperty.Custom_99 = browsers.ToDatabaseSerialization();
			artifactCustomProperty.SetCustomProperty(31, null);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_JOE_SMITH);

			//Verify the changes (this time retrieve using the option to retrieve the custom property definitions)
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(
				projectId,
				projectTemplateId,
				testSetId,
				Artifact.ArtifactTypeEnum.TestSet,
				true);
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(99)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_99.FromDatabaseSerialization_List_Int32().Count);
			browsers = (List<int>)artifactCustomProperty.CustomProperty(99);
			Assert.IsTrue(browsers.Contains(customListValues[1].CustomPropertyValueId));
			Assert.IsTrue(browsers.Contains(customListValues[2].CustomPropertyValueId));
			Assert.IsNull(artifactCustomProperty.CustomProperty(31));
			Assert.IsNull(artifactCustomProperty.Custom_31.FromDatabaseSerialization_Int32());

			//Verify that the history entries were recorded correctly
			historyChangeSets = history.RetrieveByArtifactId(testSetId, Artifact.ArtifactTypeEnum.TestSet);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChangeSets[0].UserId);
			//artifactHistoryRows = historyChangeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			//Browser
			//Assert.AreEqual("Browser", artifactHistoryRows[0].FieldCaption);
			Assert.IsTrue(historyChangeSets[0].OldValue.IsNull());
			//Assert.AreEqual(browsers.ToDatabaseSerialization(), historyChangeSets[0].NewValue);
			//OS
			//Assert.AreEqual("OS", artifactHistoryRows[1].FieldCaption);
			//Assert.AreEqual(customPropertyValueId3.ToDatabaseSerialization(), historyChangeSets[1].OldValue);
			//Assert.IsTrue(historyChangeSets[1].NewValue.IsNull());

			//Finally verify that we can delete the artifact and it deletes the associated custom property entry.
			testSetManager.DeleteFromDatabase(testSetId, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, Artifact.ArtifactTypeEnum.TestSet, false, customProperties);
			Assert.IsNull(artifactCustomProperty);
		}

		[
		Test,
		SpiraTestCase(166)
		]
		public void _12_Copy_Requirement_Custom_Properties()
		{
			//First, we need to create a new requirement
			RequirementManager requirementManager = new RequirementManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement with Custom Properties", null, 120, USER_ID_FRED_BLOGGS);

			//Now we need to add some custom properties
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
			Assert.AreEqual("http://www.x.com", (string)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual("http://www.x.com", artifactCustomProperty.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(customPropertyValueId4, (int)artifactCustomProperty.CustomProperty(65));
			Assert.AreEqual(customPropertyValueId4, artifactCustomProperty.Custom_65.FromDatabaseSerialization_Int32());

			//Now we need to make a copy of the requirement (position at the end of the list)
			int copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, projectId, requirementId, null);

			//Now verify that the requirement and its custom properties were copied
			//Requirement
			RequirementView requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, copiedRequirementId);
			Assert.AreEqual("Requirement with Custom Properties - Copy", requirement.Name);
			//Custom Properties
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, copiedRequirementId, Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
			Assert.AreEqual("http://www.x.com", (string)artifactCustomProperty.CustomProperty(1));
			Assert.AreEqual("http://www.x.com", artifactCustomProperty.Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(customPropertyValueId4, (int)artifactCustomProperty.CustomProperty(65));
			Assert.AreEqual(customPropertyValueId4, artifactCustomProperty.Custom_65.FromDatabaseSerialization_Int32());

			//Finally delete the requirement and its copy
			requirementManager.DeleteFromDatabase(requirementId, USER_ID_FRED_BLOGGS);
			requirementManager.DeleteFromDatabase(copiedRequirementId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(167)
		]
		public void _13_Copy_TestCase_Custom_Properties()
		{
			//Get the list of custom values for web browsers (used later)
			List<CustomPropertyValue> customListValues = customPropertyManager.CustomPropertyList_RetrieveById(customListId3, true).Values.ToList();

			//First, we need to create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case with Custom Properties", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Now we need to add some custom properties
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Verify that they saved OK
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(98)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_98.FromDatabaseSerialization_List_Int32().Count);
			List<int> browsers = (List<int>)artifactCustomProperty.CustomProperty(98);
			Assert.IsTrue(browsers.Contains(customListValues[0].CustomPropertyValueId));
			Assert.IsTrue(browsers.Contains(customListValues[1].CustomPropertyValueId));

			//Now we need to make a copy of the test case (position at the end of the list)
			int copiedTestCaseId = testCaseManager.TestCase_Copy(USER_ID_FRED_BLOGGS, projectId, testCaseId, null);

			//Now verify that the test case and its custom properties were copied
			//Test Case
			TestCaseView copiedTestCase = testCaseManager.RetrieveById(projectId, copiedTestCaseId);
			Assert.AreEqual("Test Case with Custom Properties - Copy", copiedTestCase.Name);
			//Custom Properties
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, copiedTestCaseId, Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
			Assert.IsNull(artifactCustomProperty.CustomProperty(1));
			Assert.IsNull(artifactCustomProperty.Custom_01.FromDatabaseSerialization_Int32());
			Assert.AreEqual(2, ((List<int>)artifactCustomProperty.CustomProperty(98)).Count);
			Assert.AreEqual(2, artifactCustomProperty.Custom_98.FromDatabaseSerialization_List_Int32().Count);
			browsers = (List<int>)artifactCustomProperty.CustomProperty(98);
			Assert.IsTrue(browsers.Contains(customListValues[0].CustomPropertyValueId));
			Assert.IsTrue(browsers.Contains(customListValues[1].CustomPropertyValueId));

			//Finally delete the test case and its copy
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
			testCaseManager.DeleteFromDatabase(copiedTestCaseId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Need to test copying for an entire project and verify that everything copied OK.
		/// </summary>
		[
		Test,
		SpiraTestCase(1161)
		]
		public void _14_CopyProjectCustomProperties()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Lets make a complete copy of our project (which internally calls CustomPropertyDefinition_CopyProject())            
			//We need to specify that we want a new template (copied from the original) for this test
			copiedProjectId = projectManager.Copy(USER_ID_FRED_BLOGGS, projectId, true, null, adminSectionId, "Project Cloned");
			copiedProjectTemplateId = templateManager.RetrieveForProject(copiedProjectId).ProjectTemplateId;

			//Lets verify that all the custom property information copied across
			//Retrieve the custom properties for a requirement and verify that we have the expected properties, options, lists and list values
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(copiedProjectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true);
			Assert.AreEqual(2, customProperties.Count);
			//URL
			Assert.AreEqual("URL", customProperties[0].Name);
			Assert.AreEqual("Text", customProperties[0].CustomPropertyTypeName);
			//Options
			Assert.AreEqual(1, customProperties[0].Options.Count);
			Assert.AreEqual((int)CustomProperty.CustomPropertyOptionEnum.Default, customProperties[0].Options[0].CustomPropertyOptionId);
			Assert.AreEqual("http://www.x.com", customProperties[0].Options[0].Value);
			//OS
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Operating Systems", customProperties[1].List.Name);
			Assert.AreEqual(6, customProperties[1].List.Values.Count);
			List<CustomPropertyValue> customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("MacOS X", customPropertyListValues[0].Name);
			Assert.AreEqual("UNIX", customPropertyListValues[1].Name);
			Assert.AreEqual("Windows 7 (x64)", customPropertyListValues[2].Name);
			Assert.AreEqual("Windows 7 (x86)", customPropertyListValues[3].Name);
			Assert.AreEqual("Windows 8", customPropertyListValues[4].Name);
			Assert.AreEqual("Windows Vista", customPropertyListValues[5].Name);
			//Options
			Assert.AreEqual(1, customProperties[1].Options.Count);
			Assert.AreEqual((int)CustomProperty.CustomPropertyOptionEnum.Default, customProperties[1].Options[0].CustomPropertyOptionId);
			Assert.AreEqual(customPropertyListValues[0].CustomPropertyValueId.ToDatabaseSerialization(), customProperties[1].Options[0].Value);

			//Retrieve the custom properties for a test case and verify that we have the expected properties, options, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(copiedProjectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true);
			Assert.AreEqual(2, customProperties.Count);
			//Reviewer
			Assert.AreEqual("Reviewer", customProperties[0].Name);
			Assert.AreEqual("User", customProperties[0].CustomPropertyTypeName);
			//Browser
			Assert.AreEqual("Browser", customProperties[1].Name);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Web Browsers", customProperties[1].List.Name);
			Assert.AreEqual(5, customProperties[1].List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("Chrome", customPropertyListValues[0].Name);
			Assert.AreEqual("Firefox", customPropertyListValues[1].Name);
			Assert.AreEqual("IE", customPropertyListValues[2].Name);
			Assert.AreEqual("Opera", customPropertyListValues[3].Name);
			Assert.AreEqual("Safari", customPropertyListValues[4].Name);

			//Retrieve the custom properties for an incident and verify that we have the expected properties, options, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(copiedProjectTemplateId, Artifact.ArtifactTypeEnum.Incident, true);
			Assert.AreEqual(2, customProperties.Count);
			//Review Date
			Assert.AreEqual("Review Date", customProperties[0].Name);
			Assert.AreEqual("Date", customProperties[0].CustomPropertyTypeName);
			//Database
			Assert.AreEqual("Database", customProperties[1].Name);
			Assert.AreEqual("List", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Database Platforms", customProperties[1].List.Name);
			Assert.AreEqual(3, customProperties[1].List.Values.Count);
			Assert.AreEqual("MySQL", customProperties[1].List.Values[0].Name);
			Assert.AreEqual("Oracle", customProperties[1].List.Values[1].Name);
			Assert.AreEqual("SQL Server", customProperties[1].List.Values[2].Name);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual(false, customProperties[1].List.IsSortedOnValue);
			Assert.AreEqual(3, customPropertyListValues.Count);
			Assert.AreEqual("SQL Server", customPropertyListValues[0].Name);
			Assert.AreEqual("Oracle", customPropertyListValues[1].Name);
			Assert.AreEqual("MySQL", customPropertyListValues[2].Name);

			//Retrieve the custom properties for a release and verify that we have the expected properties, options, lists and list values
			customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(copiedProjectTemplateId, Artifact.ArtifactTypeEnum.Release, true);
			Assert.AreEqual(2, customProperties.Count);
			//Review Date
			Assert.AreEqual("Function Points", customProperties[0].Name);
			Assert.AreEqual("Decimal", customProperties[0].CustomPropertyTypeName);
			//Database
			Assert.AreEqual("OS", customProperties[1].Name);
			Assert.AreEqual("MultiList", customProperties[1].CustomPropertyTypeName);
			//List
			Assert.AreEqual("Operating Systems", customProperties[1].List.Name);
			Assert.AreEqual(6, customProperties[1].List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperties[1].List);
			Assert.AreEqual("MacOS X", customPropertyListValues[0].Name);
			Assert.AreEqual("UNIX", customPropertyListValues[1].Name);
			Assert.AreEqual("Windows 7 (x64)", customPropertyListValues[2].Name);
			Assert.AreEqual("Windows 7 (x86)", customPropertyListValues[3].Name);
			Assert.AreEqual("Windows 8", customPropertyListValues[4].Name);
			Assert.AreEqual("Windows Vista", customPropertyListValues[5].Name);
			//Options
			List<int> defaultOperatingSystems = new List<int>();
			defaultOperatingSystems.Add(customPropertyListValues[4].CustomPropertyValueId);
			defaultOperatingSystems.Add(customPropertyListValues[0].CustomPropertyValueId);
			Assert.AreEqual(1, customProperties[1].Options.Count);
			Assert.AreEqual((int)CustomProperty.CustomPropertyOptionEnum.Default, customProperties[1].Options[0].CustomPropertyOptionId);
			Assert.AreEqual(defaultOperatingSystems.ToDatabaseSerialization(), customProperties[1].Options[0].Value);

			//Now verify that we can retrieve a single custom property by its position number
			CustomProperty customProperty = customPropertyManager
				.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(
				copiedProjectTemplateId,
				Artifact.ArtifactTypeEnum.TestCase,
				98,
				true);
			Assert.AreEqual("Browser", customProperty.Name);
			Assert.AreEqual("MultiList", customProperty.CustomPropertyTypeName);
			Assert.AreEqual(98, customProperty.PropertyNumber);
			Assert.AreEqual(copiedProjectTemplateId, customProperty.ProjectTemplateId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, customProperty.ArtifactTypeId);
			Assert.AreEqual("What Browser did you use", customProperty.Description);
			Assert.AreEqual(null, customProperty.Position);
			//List and Values
			Assert.AreEqual("Web Browsers", customProperty.List.Name);
			Assert.AreEqual(5, customProperty.List.Values.Count);
			customPropertyListValues = CustomPropertyManager.SortCustomListValues(customProperty.List);
			Assert.AreEqual("Chrome", customPropertyListValues[0].Name);
			Assert.AreEqual("Firefox", customPropertyListValues[1].Name);
			Assert.AreEqual("IE", customPropertyListValues[2].Name);
			Assert.AreEqual("Opera", customPropertyListValues[3].Name);
			Assert.AreEqual("Safari", customPropertyListValues[4].Name);
		}

		/// <summary>
		/// Tests that we can specify different custom property types on an artifact
		/// </summary>
		[
		Test,
		SpiraTestCase(1162)
		]
		public void _15_TestDifferentCustomPropertyTypes()
		{
			//We create one custom property of each type for Tasks

			//Now lets try some of the other delete operations, first some new custom properties to Tasks
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.Boolean,
				1,
				"Boolean",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.Date,
				2,
				"Date",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.Decimal,
				3,
				"Decimal",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.Integer,
				4,
				"Integer",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.List,
				5,
				"List",
				null,
				null,
				customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.MultiList,
				6,
				"MultiList",
				null,
				null,
				customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.Text,
				7,
				"Text",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact
				(projectTemplateId, Artifact.ArtifactTypeEnum.Task,
				(int)CustomProperty.CustomPropertyTypeEnum.User,
				8,
				"User",
				null,
				null,
				null);

			//Lets create a new Task
			TaskManager task = new TaskManager();
			taskId1 = task.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", "", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 60, null, null, USER_ID_FRED_BLOGGS);

			//Next lets create the default custom properties for it
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId1, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Now lets specify a value for each of the custom properties
			string rnd_string = "Test string #1";
			int rnd_int = new Random().Next(0, Int32.MaxValue);
			decimal rnd_decimal = (decimal)(new Random().Next(0, Int32.MaxValue) / 1000);
			Boolean rnd_boolean = (new Random().Next(0, Int32.MaxValue) % 2 == 0);
			DateTime rnd_date = new DateTime(new Random().Next(0, Int32.MaxValue));
			int rnd_list = new Random().Next(customPropertyValueId1, customPropertyValueId5 + 1);
			List<int> rnd_multilist = new List<int>() { new Random().Next(customPropertyValueId1, customPropertyValueId5 + 1), new Random().Next(customPropertyValueId1, customPropertyValueId5 + 1) };
			int rnd_user = new Random().Next(1, USER_ID_JOE_SMITH + 1);

			artifactCustomProperty.SetCustomProperty(1, rnd_boolean); //Boolean.
			artifactCustomProperty.SetCustomProperty(2, rnd_date); //DateTime.
			artifactCustomProperty.SetCustomProperty(3, rnd_decimal); //Decimal.
			artifactCustomProperty.SetCustomProperty(4, rnd_int); //Integer.
			artifactCustomProperty.SetCustomProperty(5, rnd_list); //List (Integer).
			artifactCustomProperty.SetCustomProperty(6, rnd_multilist); //MultiList (List<Integer>).
			artifactCustomProperty.SetCustomProperty(7, rnd_string); //String.
			artifactCustomProperty.SetCustomProperty(8, rnd_user); //User ID (Integer).

			//Now save the values
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Now retrieve the artifact back
			ArtifactCustomProperty artifactCustomProperty2 = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check values are equal..
			Assert.AreEqual(artifactCustomProperty.CustomProperty(1), artifactCustomProperty2.CustomProperty(1));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(2), artifactCustomProperty2.CustomProperty(2));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(3), artifactCustomProperty2.CustomProperty(3));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(4), artifactCustomProperty2.CustomProperty(4));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(5), artifactCustomProperty2.CustomProperty(5));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(6), artifactCustomProperty2.CustomProperty(6));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(7), artifactCustomProperty2.CustomProperty(7));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(8), artifactCustomProperty2.CustomProperty(8));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(9), artifactCustomProperty2.CustomProperty(9));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(10), artifactCustomProperty2.CustomProperty(10));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(11), artifactCustomProperty2.CustomProperty(11));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(12), artifactCustomProperty2.CustomProperty(12));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(13), artifactCustomProperty2.CustomProperty(13));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(14), artifactCustomProperty2.CustomProperty(14));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(15), artifactCustomProperty2.CustomProperty(15));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(16), artifactCustomProperty2.CustomProperty(16));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(17), artifactCustomProperty2.CustomProperty(17));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(18), artifactCustomProperty2.CustomProperty(18));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(19), artifactCustomProperty2.CustomProperty(19));
			Assert.AreEqual(artifactCustomProperty.CustomProperty(20), artifactCustomProperty2.CustomProperty(20));
		}

		[
		Test,
		SpiraTestCase(1153)
		]
		public void _16_VerifyOptions_Text()
		{
			int custPropNum = 7;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MaxLength, 10);
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MinLength, 5);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check max length..
			artifactCustomProperty.SetCustomProperty(custPropNum, "Max Length Should Be 10");
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual("Max Length", artifactCustomProperty.CustomProperty(custPropNum));

			//For maxlength we just trim and do not return back any validation error
			Assert.AreEqual(0, validationErrors.Count);

			//Check min length..
			artifactCustomProperty.SetCustomProperty(custPropNum, "Max");
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);

			//Verify that the one saved validation error is a MinLength..
			Assert.IsTrue(validationErrors.Count == 1 && validationErrors[customProperty.CustomPropertyFieldName].Contains("The minimum length"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MinLength));
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MaxLength));
		}

		[
		Test,
		SpiraTestCase(1156)
		]
		public void _17_VerifyOptions_Integer()
		{
			int custPropNum = 4;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MaxValue, 25);
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MinValue, 10);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, 15);
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check below minimum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, 5);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a MinLength..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Int32", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("less than the minimum allowed"));

			//Check above maximum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, 35);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a MaxLength..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Int32", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("more than the maximum allowed"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MinValue));
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MaxValue));
		}

		[
		Test,
		SpiraTestCase(1157)
		]
		public void _18_VerifyOptions_Decimal()
		{
			int custPropNum = 3;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MaxValue, 26.89m);
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MinValue, 5.6m);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, 15m);
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check below minimum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, 5.5m);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a MinLength..
			Assert.IsTrue(validationErrors.Count == 1 && validationErrors[customProperty.CustomPropertyFieldName].Contains("less than the minimum allowed"));

			//Check above maximum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, 26.899m);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a MaxLength..
			Assert.IsTrue(validationErrors.Count == 1 && validationErrors[customProperty.CustomPropertyFieldName].Contains("more than the maximum allowed"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MinValue));
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MaxValue));
		}

		[
		Test,
		SpiraTestCase(1159)
		]
		public void _19_VerifyOptions_Boolean()
		{
			int custPropNum = 1;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty, false);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, true);
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check that it doesn't allow empty
			artifactCustomProperty.SetCustomProperty(custPropNum, null);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a AllowEmpty..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Boolean", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("requires a value"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.AllowEmpty));
		}

		[
		Test,
		SpiraTestCase(1155)
		]
		public void _20_VerifyOptions_DateTime()
		{
			int custPropNum = 2;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MaxValue, DateTime.UtcNow);
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.MinValue, new DateTime(2012, 1, 1));

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, new DateTime(2012, 2, 1));
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check below minimum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, new DateTime(2011, 12, 31));
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);

			//Verify that the one saved exception is a MinLength..
			Assert.IsTrue(validationErrors.Count == 1 && validationErrors[customProperty.CustomPropertyFieldName].Contains("less than the minimum allowed"));

			//Check above maximum value..
			artifactCustomProperty.SetCustomProperty(custPropNum, DateTime.UtcNow.AddMonths(1));
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a MaxLength..
			Assert.IsTrue(validationErrors.Count == 1 && validationErrors[customProperty.CustomPropertyFieldName].Contains("more than the maximum allowed"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MinValue));
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.MaxValue));
		}

		[
		Test,
		SpiraTestCase(1160)
		]
		public void _21_VerifyOptions_List()
		{
			int custPropNum = 5;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty, false);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, 1);
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check that it doesn't allow empty
			artifactCustomProperty.SetCustomProperty(custPropNum, null);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a AllowEmpty..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Boolean", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("requires a value"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.AllowEmpty));
		}

		[
		Test,
		SpiraTestCase(1154)
		]
		public void _22_VerifyOptions_MultiList()
		{
			int custPropNum = 6;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty, false);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, new List<int>() { 1, 2 });
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check that it doesn't allow empty
			artifactCustomProperty.SetCustomProperty(custPropNum, null);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a AllowEmpty..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Boolean", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("requires a value"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.AllowEmpty));
		}

		[
		Test,
		SpiraTestCase(1158)
		]
		public void _23_VerifyOptions_User()
		{
			int custPropNum = 8;

			//Add some constraints onto the custom property definition
			CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, false);
			int custPropId = customProperty.CustomPropertyId;
			customPropertyManager.CustomPropertyOptions_AddForPropertyNumber(custPropId, (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty, false);

			//Retrieve the artifact custom property entity
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId1, Artifact.ArtifactTypeEnum.Task, true);

			//Check proper value.
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(custPropNum, 1);
			Dictionary<string, string> validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			Assert.AreEqual(0, validationErrors.Count);

			//Check that it doesn't allow empty
			artifactCustomProperty.SetCustomProperty(custPropNum, null);
			validationErrors = CustomPropertyManager.CustomProperty_Check(projectTemplateId, artifactCustomProperty);
			//Verify that the one saved exception is a AllowEmpty..
			Assert.AreEqual(1, validationErrors.Count);
			Logger.LogTraceEvent("VerifyOptions_Boolean", validationErrors[customProperty.CustomPropertyFieldName]);
			Assert.IsTrue(validationErrors[customProperty.CustomPropertyFieldName].Contains("requires a value"));

			//Clear the options ready for the next test
			customPropertyManager.CustomPropertyOptions_Remove(customPropertyManager.CustomPropertyOptions_RetrieveOptionForCustomPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, custPropNum, CustomProperty.CustomPropertyOptionEnum.AllowEmpty));
		}

		/// <summary>
		/// Tests that we can sort the integer and decimal fields correctly
		/// </summary>
		[Test]
		[SpiraTestCase(1669)]
		public void _24_TestNumericSorting()
		{
			//Create an integer and decimal field for incidents

			//Now lets try some of the other delete operations, first some new custom properties to Tasks
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId,
				Artifact.ArtifactTypeEnum.Incident,
				(int)CustomProperty.CustomPropertyTypeEnum.Decimal,
				3,
				"Decimal",
				null,
				null,
				null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(
				projectTemplateId,
				Artifact.ArtifactTypeEnum.Incident,
				(int)CustomProperty.CustomPropertyTypeEnum.Integer,
				4,
				"Integer",
				null,
				null,
				null);
			List<CustomProperty> customPropertyDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true);

			//Now create two incidents with custom property values set (we use 9 and 10 to verify proper numeric (vs. alphabetic) sorting)
			IncidentManager incidentManager = new IncidentManager();
			int incidentId1 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident Sort", "Incident Description", null, null, null, 1, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId1, customPropertyDefinitions);
			artifactCustomProperty.SetCustomProperty(3, 9.0M);
			artifactCustomProperty.SetCustomProperty(4, 9);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			int incidentId2 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident Sort", "Incident Description", null, null, null, 1, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId2, customPropertyDefinitions);
			artifactCustomProperty.StartTracking();
			artifactCustomProperty.SetCustomProperty(3, 10.0M);
			artifactCustomProperty.SetCustomProperty(4, 10);
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Now verify the sort order
			Hashtable filters = new Hashtable();
			filters.Add("Name", "Incident Sort");
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Custom_03", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(incidentId1, incidents[0].IncidentId);
			Assert.AreEqual(incidentId2, incidents[1].IncidentId);

			incidents = incidentManager.Retrieve(projectId, "Custom_03", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(incidentId2, incidents[0].IncidentId);
			Assert.AreEqual(incidentId1, incidents[1].IncidentId);

			incidents = incidentManager.Retrieve(projectId, "Custom_04", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(incidentId1, incidents[0].IncidentId);
			Assert.AreEqual(incidentId2, incidents[1].IncidentId);

			incidents = incidentManager.Retrieve(projectId, "Custom_04", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(incidentId2, incidents[0].IncidentId);
			Assert.AreEqual(incidentId1, incidents[1].IncidentId);
		}
	}
}
