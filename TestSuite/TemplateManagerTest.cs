using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using System.Collections;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// Tests the TemplateManager class
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TemplateManagerTest
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TemplateManagerTest::";

		protected static TemplateManager templateManager;
		protected static int templateId1 = 0;
		protected static int templateId2 = 0;
		protected static int templateId3 = 0;
		protected static int templateId4 = 0;
		protected static int templateId5 = 0;
		protected static int projectId3 = 0;

		//User IDs
		protected const int USER_ID_SYS_ADMIN = 1;
		protected const int USER_ID_FRED_BLOGGS = 2;
		protected const int USER_ID_JOE_SMITH = 3;

		//Project IDs
		protected const int PROJECT_ID_LIS = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the business classes
			templateManager = new TemplateManager();
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete any created templates
			if (templateId1 > 0)
			{
				templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			}
			if (templateId2 > 0)
			{
				templateManager.Delete(USER_ID_SYS_ADMIN, templateId2);
			}
			if (projectId3 > 0)
			{
				new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId3);
			}
			if (templateId3 > 0)
			{
				templateManager.Delete(USER_ID_SYS_ADMIN, templateId3);
			}
			if (templateId4 > 0)
			{
				templateManager.Delete(USER_ID_SYS_ADMIN, templateId4);
			}
			if (templateId5 > 0)
			{
				templateManager.Delete(USER_ID_SYS_ADMIN, templateId5);
			}
		}

		/// <summary>
		/// Tests that you can create new project templates
		/// </summary>
		[
		Test,
		SpiraTestCase(1718)
		]
		public void _01_CreateTemplate()
		{
			//First lets create a new clean project template
			templateId1 = templateManager.Insert("UnitTest: Template 1", "Sample Template 1", true, null);

			//Verify it was created
			ProjectTemplate projectTemplate = templateManager.RetrieveById(templateId1);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 1", projectTemplate.Name);
			Assert.AreEqual("Sample Template 1", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Now create a new template, based on an existing one
			templateId2 = templateManager.Insert("UnitTest: Template 2", "Sample Template 2", true, templateId1);

			//Verify it was created
			projectTemplate = templateManager.RetrieveById(templateId2);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 2", projectTemplate.Name);
			Assert.AreEqual("Sample Template 2", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Clean up
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId2);
			templateId1 = 0;
			templateId2 = 0;
		}

		/// <summary>
		/// Tests that you can modify existing project templates
		/// </summary>
		[
		Test,
		SpiraTestCase(1719)
		]
		public void _02_ModifyTemplate()
		{
			//First lets create a new clean project template
			templateId1 = templateManager.Insert("UnitTest: Template 1", "Sample Template 1", true, null);

			//Verify it was created
			ProjectTemplate projectTemplate = templateManager.RetrieveById(templateId1);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 1", projectTemplate.Name);
			Assert.AreEqual("Sample Template 1", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Make some changes
			projectTemplate.StartTracking();
			projectTemplate.Name = "UnitTest: Template 1a";
			projectTemplate.Description = "Sample unit test template";
			projectTemplate.IsActive = false;
			templateManager.Update(projectTemplate);

			//Verify the changes were saved
			projectTemplate = templateManager.RetrieveById(templateId1);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 1a", projectTemplate.Name);
			Assert.AreEqual("Sample unit test template", projectTemplate.Description);
			Assert.AreEqual(false, projectTemplate.IsActive);

			//Clean up
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateId1 = 0;
		}

		/// <summary>
		/// Tests that you can retrieve/view project templates in different ways
		/// </summary>
		/// <remarks>This test relies on the sample data</remarks>
		[
		Test,
		SpiraTestCase(1720)
		]
		public void _03_Retrieve_View_Templates()
		{
			List<ProjectTemplate> templates;
			ProjectTemplate template;
			bool isAuthorized;

			//First lets retrieve a list of active templates
			templates = templateManager.RetrieveActive();
			Assert.AreEqual(7, templates.Count);
			Assert.AreEqual("Default", templates[0].Name);
			Assert.AreEqual("Library Information System (Sample)", templates[2].Name);

			//Now find out which template is being used for a certain project
			template = templateManager.RetrieveForProject(PROJECT_ID_LIS);
			Assert.IsNotNull(template);
			Assert.AreEqual("Library Information System (Sample)", template.Name);

			//Find out which template a certain user 'owns' - any template that is applied to at least one product that sys admin is an admin of
			templates = templateManager.RetrieveTemplatesByAdmin(USER_ID_SYS_ADMIN);
			Assert.AreEqual(3, templates.Count);
			Assert.AreEqual("Default", templates[0].Name);
			Assert.AreEqual("Library Information System (Sample)", templates[1].Name);

			//Retrieve a single template
			template = templateManager.RetrieveById(1);
			Assert.IsNotNull(template);
			Assert.AreEqual("Library Information System (Sample)", template.Name);
			template = templateManager.RetrieveById(2);
			Assert.IsNotNull(template);
			Assert.AreEqual("Default", template.Name);

			//Check that we can retrieve a filtered/sorted list of project templates
			Hashtable filters = null;
			templates = templateManager.Retrieve(filters, "ProjectTemplateId ASC");
			Assert.AreEqual(7, templates.Count);
			filters = new Hashtable();
			filters.Add("Name", "Library");
			templates = templateManager.Retrieve(filters, "ProjectTemplateId ASC");
			Assert.AreEqual(2, templates.Count);
			filters.Clear();
			filters.Add("Name", "XXXX");
			templates = templateManager.Retrieve(filters, "ProjectTemplateId ASC");
			Assert.AreEqual(0, templates.Count);

			//See if a user is authorized to edit a template
			isAuthorized = templateManager.IsAuthorizedToEditTemplate(USER_ID_SYS_ADMIN, 1);
			Assert.IsTrue(isAuthorized);
			isAuthorized = templateManager.IsAuthorizedToEditTemplate(USER_ID_FRED_BLOGGS, 1);
			Assert.IsFalse(isAuthorized);

			//See if a user is authorized to view/access a template
			isAuthorized = templateManager.IsAuthorizedToViewTemplate(USER_ID_SYS_ADMIN, 1);
			Assert.IsTrue(isAuthorized);
			isAuthorized = templateManager.IsAuthorizedToViewTemplate(USER_ID_FRED_BLOGGS, 2);
			Assert.IsTrue(isAuthorized);
			isAuthorized = templateManager.IsAuthorizedToViewTemplate(USER_ID_JOE_SMITH, 3);
			Assert.IsFalse(isAuthorized);
		}

		/// <summary>
		/// Tests that a new template has all the required default entries (incident, requirement, test case fields, etc.)
		/// </summary>
		[
		Test,
		SpiraTestCase(1721)
		]
		public void _04_Create_Default_Entries()
		{
			//First lets create a new clean project template
			templateId1 = templateManager.Insert("UnitTest: Template 1", "Sample Template 1", true, null);

			//Verify it was created
			ProjectTemplate projectTemplate = templateManager.RetrieveById(templateId1);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 1", projectTemplate.Name);
			Assert.AreEqual("Sample Template 1", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Now verify that the appropriate template entries were actually created

			#region Incident Fields

			IncidentManager incidentManager = new IncidentManager();
			WorkflowManager incidentWorkflowManager = new WorkflowManager();

			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(templateId1);
			Assert.AreEqual(8, incidentStati.Count);
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(templateId1, true);
			Assert.AreEqual(8, incidentTypes.Count);
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(templateId1, true);
			Assert.AreEqual(4, incidentPriorities.Count);
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(templateId1, true);
			Assert.AreEqual(4, incidentSeverities.Count);
			List<Workflow> incidentWorkflows = incidentWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, incidentWorkflows.Count);

			#endregion

			#region Requirement Fields

			RequirementManager requirementManager = new RequirementManager();
			RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();

			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(templateId1, true);
			Assert.AreEqual(7, requirementTypes.Count);
			requirementTypes = requirementManager.RequirementType_Retrieve(templateId1, false);
			Assert.AreEqual(6, requirementTypes.Count);
			List<Importance> requirementPriorities = requirementManager.RequirementImportance_Retrieve(templateId1);
			Assert.AreEqual(4, requirementPriorities.Count);
			List<RequirementWorkflow> requirementWorkflows = requirementWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, requirementWorkflows.Count);

			#endregion

			#region Task Fields

			TaskManager taskManager = new TaskManager();
			TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();

			List<TaskType> taskTypes = taskManager.TaskType_Retrieve(templateId1);
			Assert.AreEqual(7, taskTypes.Count);
			List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(templateId1);
			Assert.AreEqual(4, taskPriorities.Count);
			List<TaskWorkflow> taskWorkflows = taskWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, taskWorkflows.Count);

			#endregion

			#region Risk Fields

			RiskManager riskManager = new RiskManager();
			RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();

			List<RiskType> riskTypes = riskManager.RiskType_Retrieve(templateId1);
			Assert.AreEqual(5, riskTypes.Count);
			List<RiskStatus> riskStatuses = riskManager.RiskStatus_Retrieve(templateId1);
			Assert.AreEqual(6, riskStatuses.Count);
			List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(templateId1);
			Assert.AreEqual(5, riskProbabilities.Count);
			List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(templateId1);
			Assert.AreEqual(5, riskImpacts.Count);
			List<RiskWorkflow> riskWorkflows = riskWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, riskWorkflows.Count);

			#endregion

			#region Test Case Fields

			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();

			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(templateId1);
			Assert.AreEqual(12, testCaseTypes.Count);
			List<TestCasePriority> testCasePriorities = testCaseManager.TestCasePriority_Retrieve(templateId1);
			Assert.AreEqual(4, testCasePriorities.Count);
			List<TestCaseWorkflow> testCaseWorkflows = testCaseWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, testCaseWorkflows.Count);

			#endregion

			#region Release Workflows

			ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();

			List<ReleaseWorkflow> releaseWorkflows = releaseWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, releaseWorkflows.Count);

			#endregion

			#region Document Fields

			AttachmentManager attachmentManager = new AttachmentManager();
			DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();

			List<DocumentType> documentTypes = attachmentManager.RetrieveDocumentTypes(templateId1, true);
			Assert.AreEqual(1, documentTypes.Count);
			List<DocumentStatus> documentStatuses = attachmentManager.DocumentStatus_Retrieve(templateId1);
			Assert.AreEqual(7, documentStatuses.Count);
			List<DocumentWorkflow> documentWorkflows = documentWorkflowManager.Workflow_Retrieve(templateId1);
			Assert.AreEqual(1, documentWorkflows.Count);

			#endregion

			#region Notifications

			NotificationManager notificationManager = new NotificationManager();
			List<NotificationEvent> notificationEvents = notificationManager.RetrieveEvents(templateId1);
			Assert.AreEqual(10, notificationEvents.Count);
			List<NotificationArtifactTemplate> notificationTemplates = notificationManager.RetrieveTemplates(templateId1);
			Assert.AreEqual(8, notificationTemplates.Count);

			#endregion

			//Clean up
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateId1 = 0;
		}

		/// <summary>
		/// Tests that a new template based on a changed existing one, has the same initial artifact entries
		/// (incident, requirement, test case fields, etc.)
		/// </summary>
		[
		Test,
		SpiraTestCase(1722)
		]
		public void _05_Clone_Template()
		{
			//Declare business classes
			IncidentManager incidentManager = new IncidentManager();
			WorkflowManager incidentWorkflowManager = new WorkflowManager();
			RequirementManager requirementManager = new RequirementManager();
			RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();
			TaskManager taskManager = new TaskManager();
			TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();
			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();
			ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();
			AttachmentManager attachmentManager = new AttachmentManager();
			DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();
			NotificationManager notificationManager = new NotificationManager();

			//First lets create a new clean project template
			templateId1 = templateManager.Insert("UnitTest: Template 1", "Sample Template 1", true, null);

			//Verify it was created
			ProjectTemplate projectTemplate = templateManager.RetrieveById(templateId1);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 1", projectTemplate.Name);
			Assert.AreEqual("Sample Template 1", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Make some changes to the fields

			#region Incident Fields

			incidentManager.IncidentStatus_Insert(templateId1, "Test Status", true, false, true);
			incidentManager.InsertIncidentPriority(templateId1, "Test Priority", "#eeeeee", true);
			incidentManager.InsertIncidentSeverity(templateId1, "Test Severity", "#eeeeee", true);
			incidentManager.InsertIncidentType(templateId1, "Test Type", null, false, false, false, true);

			#endregion

			#region Requirement Fields

			requirementManager.RequirementType_Insert(templateId1, "Test Type", null, false, true);
			requirementManager.RequirementImportance_Insert(templateId1, "Test Importance", "#eeeeee", true);

			#endregion

			#region Task Fields

			taskManager.TaskType_Insert(templateId1, "Test Type", null, false, true);
			taskManager.TaskPriority_Insert(templateId1, "Test Priority", "#eeeeee", true);

			#endregion

			#region Test Cases

			testCaseManager.TestCasePriority_Insert(templateId1, "Test Priority", "#eeeeee", true);
			testCaseManager.TestCaseType_Insert(templateId1, "Test Type", null, false, true);

			#endregion

			#region Documents

			attachmentManager.InsertDocumentType(templateId1, "Test Type", null, true, false);
			attachmentManager.DocumentStatus_Insert(templateId1, "Test Status", true, false, true);

			#endregion

			#region Notifications

			notificationManager.InsertEvent("Test Event", true, false, templateId1, (int)Artifact.ArtifactTypeEnum.Requirement, "Test Subject Line");

			#endregion

			#region Custom Properties

			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			int customPropertyListId1 = customPropertyManager.CustomPropertyList_Add(templateId1, "OS").CustomPropertyListId;
			customPropertyManager.CustomPropertyList_AddValue(customPropertyListId1, "Windows");
			customPropertyManager.CustomPropertyList_AddValue(customPropertyListId1, "MacOS");
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId1, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId1, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Text, 2, "OS", null, null, customPropertyListId1);

			#endregion

			//Now create a new template, based on an existing one
			templateId2 = templateManager.Insert("UnitTest: Template 2", "Sample Template 2", true, templateId1);

			//Verify it was created
			projectTemplate = templateManager.RetrieveById(templateId2);
			Assert.IsNotNull(projectTemplate);
			Assert.AreEqual("UnitTest: Template 2", projectTemplate.Name);
			Assert.AreEqual("Sample Template 2", projectTemplate.Description);
			Assert.AreEqual(true, projectTemplate.IsActive);

			//Now verify that the appropriate template entries were actually created

			#region Incident Fields

			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(templateId2);
			Assert.AreEqual(9, incidentStati.Count);
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(templateId2, true);
			Assert.AreEqual(9, incidentTypes.Count);
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(templateId2, true);
			Assert.AreEqual(5, incidentPriorities.Count);
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(templateId2, true);
			Assert.AreEqual(5, incidentSeverities.Count);
			List<Workflow> incidentWorkflows = incidentWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, incidentWorkflows.Count);

			#endregion

			#region Requirement Fields

			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(templateId2, true);
			Assert.AreEqual(8, requirementTypes.Count);
			requirementTypes = requirementManager.RequirementType_Retrieve(templateId2, false);
			Assert.AreEqual(7, requirementTypes.Count);
			List<Importance> requirementPriorities = requirementManager.RequirementImportance_Retrieve(templateId2);
			Assert.AreEqual(5, requirementPriorities.Count);
			List<RequirementWorkflow> requirementWorkflows = requirementWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, requirementWorkflows.Count);

			#endregion

			#region Risk Fields

			RiskManager riskManager = new RiskManager();
			RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();

			List<RiskType> riskTypes = riskManager.RiskType_Retrieve(templateId2);
			Assert.AreEqual(5, riskTypes.Count);
			List<RiskStatus> riskStatuses = riskManager.RiskStatus_Retrieve(templateId2);
			Assert.AreEqual(6, riskStatuses.Count);
			List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(templateId2);
			Assert.AreEqual(5, riskProbabilities.Count);
			List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(templateId2);
			Assert.AreEqual(5, riskImpacts.Count);
			List<RiskWorkflow> riskWorkflows = riskWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, riskWorkflows.Count);

			#endregion

			#region Task Fields

			List<TaskType> taskTypes = taskManager.TaskType_Retrieve(templateId2);
			Assert.AreEqual(8, taskTypes.Count);
			List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(templateId2);
			Assert.AreEqual(5, taskPriorities.Count);
			List<TaskWorkflow> taskWorkflows = taskWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, taskWorkflows.Count);

			#endregion

			#region Test Case Fields

			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(templateId2);
			Assert.AreEqual(13, testCaseTypes.Count);
			List<TestCasePriority> testCasePriorities = testCaseManager.TestCasePriority_Retrieve(templateId2);
			Assert.AreEqual(5, testCasePriorities.Count);
			List<TestCaseWorkflow> testCaseWorkflows = testCaseWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, testCaseWorkflows.Count);

			#endregion

			#region Release Workflows

			List<ReleaseWorkflow> releaseWorkflows = releaseWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, releaseWorkflows.Count);

			#endregion

			#region Document Fields

			List<DocumentType> documentTypes = attachmentManager.RetrieveDocumentTypes(templateId2, true);
			Assert.AreEqual(2, documentTypes.Count);
			List<DocumentStatus> documentStatuses = attachmentManager.DocumentStatus_Retrieve(templateId2);
			Assert.AreEqual(8, documentStatuses.Count);
			List<DocumentWorkflow> documentWorkflows = documentWorkflowManager.Workflow_Retrieve(templateId2);
			Assert.AreEqual(1, documentWorkflows.Count);

			#endregion

			#region Notifications

			List<NotificationEvent> notificationEvents = notificationManager.RetrieveEvents(templateId2);
			Assert.AreEqual(11, notificationEvents.Count);
			List<NotificationArtifactTemplate> notificationTemplates = notificationManager.RetrieveTemplates(templateId2);
			Assert.AreEqual(8, notificationTemplates.Count);

			#endregion

			#region Custom Properties

			List<CustomPropertyList> customLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(templateId2, true);
			Assert.AreEqual(1, customLists.Count);
			Assert.AreEqual("OS", customLists[0].Name);
			Assert.AreEqual(2, customLists[0].Values.Count);
			Assert.IsTrue(customLists[0].Values.Any(c => c.Name == "Windows"));
			Assert.IsTrue(customLists[0].Values.Any(c => c.Name == "MacOS"));

			#endregion

			//Clean up
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId2);
			templateId1 = 0;
			templateId2 = 0;
		}

		/// <summary>
		/// Tests that you can move a project from one template to another and the system will intelligently remap the various list IDs
		/// </summary>
		[
		Test,
		SpiraTestCase(2045)
		]
		public void _06_Change_Project_Template()
		{
			const int DATA_SYNC_SYSTEM_ID_JIRA = 1;
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Declare business classes
			ProjectManager projectManager = new ProjectManager();
			IncidentManager incidentManager = new IncidentManager();
			RequirementManager requirementManager = new RequirementManager();
			TaskManager taskManager = new TaskManager();
			TestCaseManager testCaseManager = new TestCaseManager();
			AttachmentManager attachmentManager = new AttachmentManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			RiskManager riskManager = new RiskManager();
			HistoryManager historyManager = new HistoryManager();
			TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
			DataMappingManager dataMappingManager = new DataMappingManager();

			//First create a project with a new template
			projectId3 = projectManager.Insert("Project 3", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			Project project = projectManager.RetrieveById(projectId3);
			templateId3 = project.ProjectTemplateId;

			//Now create list custom properties
			int customListId1 = customPropertyManager.CustomPropertyList_Add(templateId3, "Browsers").CustomPropertyListId;
			int customValueId_Safari = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Safari").CustomPropertyValueId;
			int customValueId_Edge = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Edge").CustomPropertyValueId;
			int customValueId_Chrome = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Chrome").CustomPropertyValueId;
			int customValueId_Firefox = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Firefox").CustomPropertyValueId;
			int customListId2 = customPropertyManager.CustomPropertyList_Add(templateId3, "Operating Systems").CustomPropertyListId;
			int customValueId_Windows = customPropertyManager.CustomPropertyList_AddValue(customListId2, "Windows").CustomPropertyValueId;
			int customValueId_Linux = customPropertyManager.CustomPropertyList_AddValue(customListId2, "Linux").CustomPropertyValueId;

			//We only use the browser list for custom properties right now
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Browser", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.TestCase, (int)CustomProperty.CustomPropertyTypeEnum.MultiList, 1, "Browser", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Task, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Browser", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Risk, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Browser", null, null, customListId1);
			int customPropertyId_incident = customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.MultiList, 1, "Browser", null, null, customListId1).CustomPropertyId;

			//We also add a text, integer and user custom property as well to make sure all custom property ids are being remapped regardless of type
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Text, 2, "Text", null, null, null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.User, 3, "User", null, null, null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId3, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Integer, 4, "Integer", null, null, null);

			//Now lets create one of each artifact, using the various list values and custom properties

			//Requirement
			int requirementTypeId = requirementManager.RequirementType_Retrieve(templateId3, true, true).FirstOrDefault(p => p.Name == "User Story").RequirementTypeId;
			int requirementImportanceId = requirementManager.RequirementImportance_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "2 - High").ImportanceId;
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId3, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, requirementTypeId, USER_ID_FRED_BLOGGS, null, requirementImportanceId, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Release - only need for executing tests
			int releaseId = new ReleaseManager().Insert(USER_ID_FRED_BLOGGS, projectId3, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 0, null);

			//Test Case
			int testCaseTypeId = testCaseManager.TestCaseType_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Security").TestCaseTypeId;
			int testCasePriorityId = testCaseManager.TestCasePriority_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "2 - High").TestCasePriorityId;
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId3, USER_ID_FRED_BLOGGS, null, "Test Case", null, testCaseTypeId, TestCase.TestCaseStatusEnum.Approved, testCasePriorityId, null, null, null, null);

			//Task
			int taskTypeId = taskManager.TaskType_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Development").TaskTypeId;
			int taskPriorityId = taskManager.TaskPriority_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "2 - High").TaskPriorityId;
			int taskId = taskManager.Insert(projectId3, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, taskTypeId, null, null, null, null, taskPriorityId, "Task", null, null, null, null, null, null);

			//Incident
			int incidentStatusId = incidentManager.IncidentStatus_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Assigned").IncidentStatusId;
			int incidentTypeId = incidentManager.RetrieveIncidentTypes(templateId3, true).FirstOrDefault(p => p.Name == "Issue").IncidentTypeId;
			int incidentPriorityId = incidentManager.RetrieveIncidentPriorities(templateId3, true).FirstOrDefault(p => p.Name == "1 - Critical").PriorityId;
			int incidentSeverityId = incidentManager.RetrieveIncidentSeverities(templateId3, true).FirstOrDefault(p => p.Name == "3 - Medium").SeverityId;
			int incidentId = incidentManager.Insert(projectId3, incidentPriorityId, incidentSeverityId, USER_ID_FRED_BLOGGS, null, null, "Incident", "Description", releaseId, null, null, incidentTypeId, incidentStatusId, DateTime.UtcNow, null, null, null, null, null, null, null);

			//Risk
			int riskStatusId = riskManager.RiskStatus_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Evaluated").RiskStatusId;
			int riskTypeId = riskManager.RiskType_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Financial").RiskTypeId;
			int riskProbabilityId = riskManager.RiskProbability_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Likely").RiskProbabilityId;
			int riskImpactId = riskManager.RiskImpact_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Serious").RiskImpactId;
			int riskId = riskManager.Risk_Insert(projectId3, riskStatusId, riskTypeId, riskProbabilityId, riskImpactId, USER_ID_FRED_BLOGGS, null, "Risk", null, releaseId, null, DateTime.UtcNow, null, null);

			//Document
			int documentStatusId = attachmentManager.DocumentStatus_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Approved").DocumentStatusId;
			int documentTypeId = attachmentManager.InsertDocumentType(templateId3, "Specification", null, true, false);
			int documentId = attachmentManager.Insert(projectId3, "test.htm", null, USER_ID_FRED_BLOGGS, null, Artifact.ArtifactTypeEnum.None, "1.0", null, documentTypeId, null, documentStatusId);

			//Add some custom property values
			//Requirement
			ArtifactCustomProperty requirementCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, requirementId, Artifact.ArtifactTypeEnum.Requirement);
			if (requirementCP == null) { requirementCP = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId3, Artifact.ArtifactTypeEnum.Requirement, requirementId); }
			requirementCP.StartTracking();
			requirementCP.Custom_01 = customValueId_Safari.ToDatabaseSerialization();
			requirementCP.Custom_02 = "Some text";
			requirementCP.Custom_03 = USER_ID_FRED_BLOGGS.ToDatabaseSerialization();
			requirementCP.Custom_04 = 123.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(requirementCP, USER_ID_FRED_BLOGGS);

			//Test Case
			ArtifactCustomProperty testCaseCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, testCaseId, Artifact.ArtifactTypeEnum.TestCase);
			if (testCaseCP == null) { testCaseCP = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId3, Artifact.ArtifactTypeEnum.TestCase, testCaseId); }
			testCaseCP.StartTracking();
			testCaseCP.Custom_01 = new List<int>() { customValueId_Chrome, customValueId_Firefox }.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(testCaseCP, USER_ID_FRED_BLOGGS);

			//Task
			ArtifactCustomProperty taskCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, taskId, Artifact.ArtifactTypeEnum.Task);
			if (taskCP == null) { taskCP = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId3, Artifact.ArtifactTypeEnum.Task, taskId); }
			taskCP.StartTracking();
			taskCP.Custom_01 = customValueId_Chrome.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(taskCP, USER_ID_FRED_BLOGGS);

			//Risk
			ArtifactCustomProperty riskCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, riskId, Artifact.ArtifactTypeEnum.Risk);
			if (riskCP == null) { riskCP = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId3, Artifact.ArtifactTypeEnum.Risk, riskId); }
			riskCP.StartTracking();
			riskCP.Custom_01 = customValueId_Firefox.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(riskCP, USER_ID_FRED_BLOGGS);

			//Incident
			ArtifactCustomProperty incidentCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, incidentId, Artifact.ArtifactTypeEnum.Incident);
			if (incidentCP == null) { incidentCP = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId3, Artifact.ArtifactTypeEnum.Incident, incidentId); }
			incidentCP.StartTracking();
			incidentCP.Custom_01 = new List<int>() { customValueId_Edge, customValueId_Safari }.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(incidentCP, USER_ID_FRED_BLOGGS);

			//Make some history changes to some of the artifacts
			//Incident
			int incidentStatusId2 = incidentManager.IncidentStatus_Retrieve(templateId3, true).FirstOrDefault(p => p.Name == "Reopen").IncidentStatusId;
			int incidentTypeId2 = incidentManager.RetrieveIncidentTypes(templateId3, true).FirstOrDefault(p => p.Name == "Training").IncidentTypeId;
			int incidentPriorityId2 = incidentManager.RetrieveIncidentPriorities(templateId3, true).FirstOrDefault(p => p.Name == "2 - High").PriorityId;
			int incidentSeverityId2 = incidentManager.RetrieveIncidentSeverities(templateId3, true).FirstOrDefault(p => p.Name == "4 - Low").SeverityId;
			Incident incident = incidentManager.RetrieveById(incidentId, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentStatusId2;
			incident.IncidentTypeId = incidentTypeId2;
			incident.PriorityId = incidentPriorityId2;
			incident.SeverityId = incidentSeverityId2;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Also change its custom properties
			//Incident: multilist
			incidentCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, incidentId, Artifact.ArtifactTypeEnum.Incident);
			incidentCP.StartTracking();
			incidentCP.Custom_01 = new List<int>() { customValueId_Chrome, customValueId_Firefox }.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(incidentCP, USER_ID_FRED_BLOGGS);

			//Requirement: list, text, user, integer
			requirementCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId3, requirementId, Artifact.ArtifactTypeEnum.Requirement);
			requirementCP.StartTracking();
			requirementCP.Custom_01 = customValueId_Firefox.ToDatabaseSerialization();
			requirementCP.Custom_02 = "Some text 2";
			requirementCP.Custom_03 = USER_ID_JOE_SMITH.ToDatabaseSerialization();
			requirementCP.Custom_04 = 456.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(requirementCP, USER_ID_FRED_BLOGGS);

			//Now enable this for data synchronization with Jira and add some mappings
			dataMappingManager.InsertDataSyncProject(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, "PROJ3", true);

			string adminSectionName2 = "Data Synchronization";
			var adminSection2 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName2);

			int adminSectionId2 = adminSection2.ADMIN_SECTION_ID;

			string action2 = "Data Synchronization Updated";

			//Standard Field - Type
			List<DataSyncArtifactFieldValueMapping> fieldValueMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, /* Incident Type */4);
			DataSyncArtifactFieldValueMapping newFieldValueMapping = new DataSyncArtifactFieldValueMapping();
			newFieldValueMapping.DataSyncSystemId = DATA_SYNC_SYSTEM_ID_JIRA;
			newFieldValueMapping.ArtifactFieldId = /* Incident Type */4;
			newFieldValueMapping.ArtifactFieldValue = incidentTypeId;
			newFieldValueMapping.ProjectId = projectId3;
			newFieldValueMapping.ExternalKey = "ISSUE";
			newFieldValueMapping.PrimaryYn = "Y";
			newFieldValueMapping.MarkAsAdded();
			fieldValueMappings.Add(newFieldValueMapping);
			dataMappingManager.SaveDataSyncFieldValueMappings(fieldValueMappings, 1, adminSectionId2, action2);

			//Custom Property - Browser
			DataSyncCustomPropertyMapping customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId_incident);
			List<DataSyncCustomPropertyValueMapping> customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, customPropertyId_incident);

			//the Property mapping
			customPropertyMapping = new DataSyncCustomPropertyMapping();
			customPropertyMapping.MarkAsAdded();
			customPropertyMapping.DataSyncSystemId = DATA_SYNC_SYSTEM_ID_JIRA;
			customPropertyMapping.ProjectId = projectId3;
			customPropertyMapping.CustomPropertyId = customPropertyId_incident;
			customPropertyMapping.ExternalKey = "BROWSER";

			//The Value mapping
			DataSyncCustomPropertyValueMapping customPropertyValueMapping = new DataSyncCustomPropertyValueMapping();
			customPropertyValueMapping.MarkAsAdded();
			customPropertyValueMapping.DataSyncSystemId = DATA_SYNC_SYSTEM_ID_JIRA;
			customPropertyValueMapping.ProjectId = projectId3;
			customPropertyValueMapping.CustomPropertyValueId = customValueId_Chrome;
			customPropertyValueMapping.ExternalKey = "CHROME";
			customPropertyValueMappings.Add(customPropertyValueMapping);

			//Save
			dataMappingManager.SaveDataSyncCustomPropertyMappings(new List<DataSyncCustomPropertyMapping>() { customPropertyMapping });
			dataMappingManager.SaveDataSyncCustomPropertyValueMappings(customPropertyValueMappings);

			//Next we need to create some test configurations that use the custom property values
			//First we create two parameters, both will use the browser list
			int parameter1 = testCaseManager.InsertParameter(projectId3, testCaseId, "browser1", null);
			int parameter2 = testCaseManager.InsertParameter(projectId3, testCaseId, "browser2", null);

			//Next the test configuration set
			Dictionary<int, int> testParametersMapping = new Dictionary<int, int>();
			testParametersMapping.Add(parameter1, customListId1);
			testParametersMapping.Add(parameter2, customListId2);
			int testConfigurationSetId = testConfigurationManager.InsertSet(projectId3, "Test Configuration Set", null, true, USER_ID_FRED_BLOGGS);
			testConfigurationManager.PopulateTestConfigurations(projectId3, testConfigurationSetId, testParametersMapping);

			//Verify it created OK
			List<TestConfigurationEntry> testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId3, testConfigurationSetId);
			Assert.AreEqual(16, testConfigurationEntries.Count);

			//Now create a new template, based on an existing one
			templateId4 = templateManager.Insert("Template 4", null, true, templateId3);

			//Before we move the template, make sure we can get the list of affected fields
			List<TemplateRemapStandardFieldsInfo> standardFieldsInfo = templateManager.RetrieveStandardFieldMappingInformation(projectId3, templateId4);
			Assert.AreEqual(16, standardFieldsInfo.Count);
			Assert.AreEqual("Document", standardFieldsInfo[0].ArtifactType);
			Assert.AreEqual("Status", standardFieldsInfo[0].ArtifactField);
			Assert.AreEqual(0, standardFieldsInfo[0].AffectedItemsCount);

			//Now move the project between the two templates
			templateManager.ChangeProjectTemplate(projectId3, templateId4, USER_ID_SYS_ADMIN);

			//Verify the changes
			//Requirement
			int requirementTypeId4 = requirementManager.RequirementType_Retrieve(templateId4, true, true).FirstOrDefault(p => p.Name == "User Story").RequirementTypeId;
			int requirementImportanceId4 = requirementManager.RequirementImportance_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "2 - High").ImportanceId;
			RequirementView requirement = requirementManager.RetrieveById(User.UserInternal, projectId3, requirementId);
			Assert.AreEqual(requirementTypeId4, requirement.RequirementTypeId);
			Assert.AreEqual("User Story", requirement.RequirementTypeName);
			Assert.AreEqual(requirementImportanceId4, requirement.ImportanceId);
			Assert.AreEqual("2 - High", requirement.ImportanceName);

			//Test Case
			int testCaseTypeId4 = testCaseManager.TestCaseType_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Security").TestCaseTypeId;
			int testCasePriorityId4 = testCaseManager.TestCasePriority_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "2 - High").TestCasePriorityId;
			TestCaseView testCase = testCaseManager.RetrieveById(projectId3, testCaseId);
			Assert.AreEqual(testCaseTypeId4, testCase.TestCaseTypeId);
			Assert.AreEqual("Security", testCase.TestCaseTypeName);
			Assert.AreEqual(testCasePriorityId4, testCase.TestCasePriorityId);
			Assert.AreEqual("2 - High", testCase.TestCasePriorityName);

			//Task
			int taskTypeId4 = taskManager.TaskType_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Development").TaskTypeId;
			int taskPriorityId4 = taskManager.TaskPriority_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "2 - High").TaskPriorityId;
			TaskView task = taskManager.TaskView_RetrieveById(taskId);
			Assert.AreEqual(taskTypeId4, task.TaskTypeId);
			Assert.AreEqual("Development", task.TaskTypeName);
			Assert.AreEqual(taskPriorityId4, task.TaskPriorityId);
			Assert.AreEqual("2 - High", task.TaskPriorityName);

			//Incident
			int incidentStatusId4 = incidentManager.IncidentStatus_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Reopen").IncidentStatusId;
			int incidentTypeId4 = incidentManager.RetrieveIncidentTypes(templateId4, true).FirstOrDefault(p => p.Name == "Training").IncidentTypeId;
			int incidentPriorityId4 = incidentManager.RetrieveIncidentPriorities(templateId4, true).FirstOrDefault(p => p.Name == "2 - High").PriorityId;
			int incidentSeverityId4 = incidentManager.RetrieveIncidentSeverities(templateId4, true).FirstOrDefault(p => p.Name == "4 - Low").SeverityId;
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId);
			Assert.AreEqual(incidentTypeId4, incidentView.IncidentTypeId);
			Assert.AreEqual("Training", incidentView.IncidentTypeName);
			Assert.AreEqual(incidentStatusId4, incidentView.IncidentStatusId);
			Assert.AreEqual("Reopen", incidentView.IncidentStatusName);
			Assert.AreEqual(incidentPriorityId4, incidentView.PriorityId);
			Assert.AreEqual("2 - High", incidentView.PriorityName);
			Assert.AreEqual(incidentSeverityId4, incidentView.SeverityId);
			Assert.AreEqual("4 - Low", incidentView.SeverityName);

			//Risk
			int riskStatusId4 = riskManager.RiskStatus_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Evaluated").RiskStatusId;
			int riskTypeId4 = riskManager.RiskType_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Financial").RiskTypeId;
			int riskProbabilityId4 = riskManager.RiskProbability_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Likely").RiskProbabilityId;
			int riskImpactId4 = riskManager.RiskImpact_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Critical").RiskImpactId;
			RiskView risk = riskManager.Risk_RetrieveById2(riskId);
			Assert.AreEqual(riskStatusId4, risk.RiskStatusId);
			Assert.AreEqual("Evaluated", risk.RiskStatusName);
			Assert.AreEqual(riskTypeId4, risk.RiskTypeId);
			Assert.AreEqual("Financial", risk.RiskTypeName);
			Assert.AreEqual(riskProbabilityId4, risk.RiskProbabilityId);
			Assert.AreEqual("Likely", risk.RiskProbabilityName);
			Assert.AreEqual(riskImpactId4, risk.RiskImpactId);
			Assert.AreEqual("Critical", risk.RiskImpactName);

			//Document
			int documentStatusId4 = attachmentManager.DocumentStatus_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Approved").DocumentStatusId;
			int documentTypeId4 = attachmentManager.RetrieveDocumentTypes(templateId4, true).FirstOrDefault(p => p.Name == "Specification").DocumentTypeId;
			ProjectAttachmentView attachment = attachmentManager.RetrieveForProjectById2(projectId3, documentId);
			Assert.AreEqual(documentStatusId4, attachment.DocumentStatusId);
			Assert.AreEqual("Approved", attachment.DocumentStatusName);
			Assert.AreEqual(documentTypeId4, attachment.DocumentTypeId);
			Assert.AreEqual("Specification", attachment.DocumentTypeName);

			//Custom Properties
			CustomProperty requirementCustomPropertyDefinition1 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(templateId4, Artifact.ArtifactTypeEnum.Requirement, 1, false);
			CustomProperty requirementCustomPropertyDefinition2 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(templateId4, Artifact.ArtifactTypeEnum.Requirement, 2, false);
			CustomProperty requirementCustomPropertyDefinition3 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(templateId4, Artifact.ArtifactTypeEnum.Requirement, 3, false);
			CustomProperty requirementCustomPropertyDefinition4 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(templateId4, Artifact.ArtifactTypeEnum.Requirement, 4, false);
			CustomProperty incidentCustomPropertyDefinition = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(templateId4, Artifact.ArtifactTypeEnum.Incident, 1, false);
			List<CustomPropertyList> customListsInNewTemplate = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(templateId4, true);
			CustomPropertyList customListInNewTemplate = customListsInNewTemplate.FirstOrDefault(c => c.Name == "Browsers");

			//Requirement
			requirementCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId4, requirementId, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId, requirementCP.Custom_01.FromDatabaseSerialization_Int32());

			//Test Case
			testCaseCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId4, testCaseId, Artifact.ArtifactTypeEnum.TestCase);
			List<int> newValues = testCaseCP.Custom_01.FromDatabaseSerialization_List_Int32();
			Assert.IsTrue(newValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId));
			Assert.IsTrue(newValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Chrome").CustomPropertyValueId));

			//Task
			taskCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId4, taskId, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Chrome").CustomPropertyValueId, taskCP.Custom_01.FromDatabaseSerialization_Int32());

			//Risk
			riskCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId4, riskId, Artifact.ArtifactTypeEnum.Risk);
			Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId, riskCP.Custom_01.FromDatabaseSerialization_Int32());

			//Incident
			incidentCP = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId3, templateId4, incidentId, Artifact.ArtifactTypeEnum.Incident);
			newValues = incidentCP.Custom_01.FromDatabaseSerialization_List_Int32();
			Assert.IsTrue(newValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId));
			Assert.IsTrue(newValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Chrome").CustomPropertyValueId));

			//History Changes
			List<HistoryChangeSetResponse> incidentChangeSets = historyManager.RetrieveByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			List<HistoryChangeSetResponse> requirementChangeSets = historyManager.RetrieveByArtifactId(requirementId, Artifact.ArtifactTypeEnum.Requirement);

			//Incident
			HistoryChangeSetResponse incidentCPChanges = incidentChangeSets[0];
			//HistoryDetail incidentCPChange1 = incidentCPChanges.Details.FirstOrDefault(h => h.CustomPropertyId.HasValue);
			//Assert.AreEqual(incidentCustomPropertyDefinition.CustomPropertyId, incidentCPChange1.CustomPropertyId.Value);
			//List<int> oldCPValues = incidentCPChange1.OldValue.FromDatabaseSerialization_List_Int32();
			//List<int> newCPValues = incidentCPChange1.NewValue.FromDatabaseSerialization_List_Int32();
			//Assert.IsTrue(oldCPValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Safari").CustomPropertyValueId));
			//Assert.IsTrue(oldCPValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Edge").CustomPropertyValueId));
			//Assert.IsTrue(newCPValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId));
			//Assert.IsTrue(newCPValues.Contains(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Chrome").CustomPropertyValueId));

			//Get the old incident field values in the new template
			int incidentStatusId5 = incidentManager.IncidentStatus_Retrieve(templateId4, true).FirstOrDefault(p => p.Name == "Assigned").IncidentStatusId;
			int incidentTypeId5 = incidentManager.RetrieveIncidentTypes(templateId4, true).FirstOrDefault(p => p.Name == "Issue").IncidentTypeId;
			int incidentPriorityId5 = incidentManager.RetrieveIncidentPriorities(templateId4, true).FirstOrDefault(p => p.Name == "1 - Critical").PriorityId;
			int incidentSeverityId5 = incidentManager.RetrieveIncidentSeverities(templateId4, true).FirstOrDefault(p => p.Name == "3 - Medium").SeverityId;

			//Need to re-retrieve to get the field names
			List<HistoryChangeSetResponse> incidentFieldChanges = historyManager.RetrieveByChangeSetId(projectId3, incidentChangeSets[1].ChangeSetId, null, true, null, 1, Int32.MaxValue, InternalRoutines.UTC_OFFSET);
			//Status
			HistoryChangeSetResponse incidentFieldchange1 = incidentFieldChanges.FirstOrDefault(h => h.FieldName == "IncidentStatusId");
			//Assert.AreEqual(incidentStatusId5, incidentFieldchange1.OldValueInt);
			Assert.AreEqual("Assigned", incidentFieldchange1.OldValue);
			//Assert.AreEqual(incidentStatusId4, incidentFieldchange1.NewValueInt);
			Assert.AreEqual("Reopen", incidentFieldchange1.NewValue);

			//Type
			HistoryChangeSetResponse incidentFieldchange2 = incidentFieldChanges.FirstOrDefault(h => h.FieldName == "IncidentTypeId");
			//Assert.AreEqual(incidentTypeId5, incidentFieldchange2.OldValueInt);
			Assert.AreEqual("Issue", incidentFieldchange2.OldValue);
			//Assert.AreEqual(incidentTypeId4, incidentFieldchange2.NewValueInt);
			//Assert.AreEqual(incidentTypeId4, incidentFieldchange2.NewValueInt);
			Assert.AreEqual("Training", incidentFieldchange2.NewValue);

			//Priority
			HistoryChangeSetResponse incidentFieldchange3 = incidentFieldChanges.FirstOrDefault(h => h.FieldName == "PriorityId");
			//Assert.AreEqual(incidentPriorityId5, incidentFieldchange3.OldValueInt);
			Assert.AreEqual("1 - Critical", incidentFieldchange3.OldValue);
			//Assert.AreEqual(incidentPriorityId4, incidentFieldchange3.NewValueInt);
			Assert.AreEqual("2 - High", incidentFieldchange3.NewValue);

			//Severity
			HistoryChangeSetResponse incidentFieldchange4 = incidentFieldChanges.FirstOrDefault(h => h.FieldName == "SeverityId");
			//Assert.AreEqual(incidentSeverityId5, incidentFieldchange4.OldValueInt);
			Assert.AreEqual("3 - Medium", incidentFieldchange4.OldValue);
			//Assert.AreEqual(incidentSeverityId4, incidentFieldchange4.NewValueInt);
			Assert.AreEqual("4 - Low", incidentFieldchange4.NewValue);

			//Requirement
			HistoryChangeSetResponse requirementCPchanges = requirementChangeSets[0];
			//HistoryDetail requirementCPchange1 = requirementCPchanges.Details.FirstOrDefault(h => h.CustomPropertyId.HasValue && h.FieldCaption == "Browser");
			//Assert.AreEqual(requirementCustomPropertyDefinition1.CustomPropertyId, requirementCPchange1.CustomPropertyId.Value);
			//Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Safari").CustomPropertyValueId, requirementCPchange1.OldValueInt);
			//Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Safari").CustomPropertyValueId, requirementCPchange1.OldValue.FromDatabaseSerialization_Int32());
			//Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId, requirementCPchange1.NewValueInt);
			//Assert.AreEqual(customListInNewTemplate.Values.FirstOrDefault(c => c.Name == "Firefox").CustomPropertyValueId, requirementCPchange1.NewValue.FromDatabaseSerialization_Int32());

			//Also check that text/user/integer fields remapped OK
			//HistoryDetail requirementCPchange2 = requirementCPchanges.Details.FirstOrDefault(h => h.CustomPropertyId.HasValue && h.FieldCaption == "Text");
			//Assert.AreEqual(requirementCustomPropertyDefinition2.CustomPropertyId, requirementCPchange2.CustomPropertyId.Value);
			//HistoryDetail requirementCPchange3 = requirementCPchanges.Details.FirstOrDefault(h => h.CustomPropertyId.HasValue && h.FieldCaption == "User");
			//Assert.AreEqual(requirementCustomPropertyDefinition3.CustomPropertyId, requirementCPchange3.CustomPropertyId.Value);
			//HistoryDetail requirementCPchange4 = requirementCPchanges.Details.FirstOrDefault(h => h.CustomPropertyId.HasValue && h.FieldCaption == "Integer");
			//Assert.AreEqual(requirementCustomPropertyDefinition4.CustomPropertyId, requirementCPchange4.CustomPropertyId.Value);

			//Verify that the data mapping is still listed for the project, but inactive, and that all the field/custom property mappings have been erased
			DataSyncProject dataSyncProject = dataMappingManager.RetrieveDataSyncProject(DATA_SYNC_SYSTEM_ID_JIRA, projectId3);
			Assert.IsNotNull(dataSyncProject);
			Assert.AreEqual("PROJ3", dataSyncProject.ExternalKey);
			Assert.AreEqual("N", dataSyncProject.ActiveYn);
			fieldValueMappings = dataMappingManager.RetrieveDataSyncFieldValueMappings(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, /* Incident Type */4);
			Assert.AreEqual(0, fieldValueMappings.Count);
			customPropertyMapping = dataMappingManager.RetrieveDataSyncCustomPropertyMapping(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, DataModel.Artifact.ArtifactTypeEnum.Incident, customPropertyId_incident);
			Assert.IsNull(customPropertyMapping);
			customPropertyValueMappings = dataMappingManager.RetrieveDataSyncCustomPropertyValueMappings(DATA_SYNC_SYSTEM_ID_JIRA, projectId3, customPropertyId_incident);
			Assert.AreEqual(0, customPropertyValueMappings.Count);

			//Verify that the test configurations are pointing to the correct values
			testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId3, testConfigurationSetId);
			Assert.AreEqual(16, testConfigurationEntries.Count);
			foreach (TestConfigurationEntry entry in testConfigurationEntries)
			{
				bool matchFound = false;
				foreach (CustomPropertyList list in customListsInNewTemplate)
				{
					if (list.Values != null && list.Values.Count > 0)
					{
						if (list.Values.Any(l => l.CustomPropertyValueId == entry.CustomPropertyValueId))
						{
							matchFound = true;
							break;
						}
					}
				}

				//If no match, then that is incorrect, all custom values should be in the new template or have been deleted
				Assert.IsTrue(matchFound);
			}

			//Now create a completely blank template and switch the project to that. It should not throw errors, but "lose" data cleanly

			//Now create a new template, based on an existing one
			templateId5 = templateManager.Insert("Template 5", null, true);

			//Now move the project between the two templates
			templateManager.ChangeProjectTemplate(projectId3, templateId5, USER_ID_SYS_ADMIN);

			//Verify that the test configurations were wiped
			testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId3, testConfigurationSetId);
			Assert.AreEqual(0, testConfigurationEntries.Count);

			//Clean Up
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId3);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId3);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId4);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId5);
		}

		/// <summary>
		/// Tests that we can manage project template settings using the special ProjectTemplateSettings configuration provider
		/// </summary>
		[
		Test,
		SpiraTestCase(2737)
		]
		public void _07_StoreRetrieveTemplateSettings()
		{
			//Setup - create two new projectTemplates
			int projectTemplateForSettings1 = templateManager.Insert("Settings ProjectTemplate 1", null, true);
			int projectTemplateForSettings2 = templateManager.Insert("Settings ProjectTemplate 2", null, true);

			//First, get the default settings values for a project
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateForSettings1);
			Assert.IsNotNull(projectTemplateSettings);
			Assert.IsNullOrEmpty(projectTemplateSettings.TestSetting);

			//Next test that we can store some settings
			projectTemplateSettings.TestSetting = "Test123";
			projectTemplateSettings.Save();

			//Verify
			projectTemplateSettings = new ProjectTemplateSettings(projectTemplateForSettings1);
			Assert.IsNotNull(projectTemplateSettings);
			Assert.AreEqual("Test123", projectTemplateSettings.TestSetting);

			//Next test that the settings are truly project-specific
			projectTemplateSettings = new ProjectTemplateSettings(projectTemplateForSettings2);
			Assert.IsNotNull(projectTemplateSettings);
			Assert.IsNullOrEmpty(projectTemplateSettings.TestSetting);

			//Test that if we create a template based on an existing one, the settings get copied over
			int projectTemplateForSettings3 = templateManager.Insert("Settings ProjectTemplate 3", null, true, projectTemplateForSettings1);

			//Verify settings
			projectTemplateSettings = new ProjectTemplateSettings(projectTemplateForSettings3);
			Assert.IsNotNull(projectTemplateSettings);
			Assert.AreEqual("Test123", projectTemplateSettings.TestSetting);

			//Finally check that you cannot use the 'default' static instance of the settings class
			bool exceptionCaught = false;
			try
			{
				projectTemplateSettings = ProjectTemplateSettings.Default;
				projectTemplateSettings.TestSetting = "XYZ";
				projectTemplateSettings.Save();
			}
			catch (InvalidOperationException)
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught);

			//Delete the projectTemplates
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateForSettings1);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateForSettings2);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateForSettings3);
		}
	}
}
