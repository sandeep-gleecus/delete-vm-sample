using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Notification business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class NotificationTest
	{
		#region Static Event Data
		//Requirement data
		private const int REQ_ARTIFACT_TYPE = 1;
		private const string REQ_NAME = "Test Requirement Event";
		private const bool REQ_ACTIVE = true;
		private const bool REQ_CREATE = true;
		private const string REQ_SUBJECT = "New Requirement Created!";
		//Incident data
		private const int INC_ARTIFACT_TYPE = 3;
		private const string INC_NAME = "Test Incident Event";
		private const bool INC_ACTIVE = false;
		private const bool INC_CREATE = true;
		private const string INC_SUBJECT = "New Incident Created!";
		//Test Case data
		private const int TSC_ARTIFACT_TYPE = 2;
		private const string TSC_NAME = "Test TestCase Event";
		private const bool TSC_ACTIVE = true;
		private const bool TSC_CREATE = false;
		private const string TSC_SUBJECT = "Test Case Updated";
		//Task data
		private const int TSK_ARTIFACT_TYPE = 6;
		private const string TSK_NAME = "Test Task Event";
		private const bool TSK_ACTIVE = false;
		private const bool TSK_CREATE = false;
		private const string TSK_SUBJECT = "Task Updated";
		//Alternate Requirement data
		private const string REQ2_NAME = "Test Changed Requirement Event";
		private const bool REQ2_ACTIVE = false;
		private const bool REQ2_CREATE = false;
		#endregion
		#region Static Project Data
		//Project ID
		private const int PROJECT_NONEXIST = 99999;
		// Project Roles
		private const int ROLE_TESTER = 4;
		private const int ROLE_DEVELOPER = 3;
		private const int ROLE_PROJOWNER = 1;
		// Project Artifact Users
		private const int ARTUSER_CREATOR = 1;
		private const int ARTUSER_OWNER = 2;
		// Project Artifact Fields
		private const int ARTFIELD_RQ_COVERAGE = 68;
		private const int ARTFIELD_RQ_NAME = 86;
		private const int ARTFIELD_RQ_DESCRIPTION = 104;
		private const int ARTFIELD_RQ_OWNER = 73;
		private const int ARTFIELD_IN_SEVERITY = 1;
		private const int ARTFIELD_IN_PRIORITY = 2;
		private const int ARTFIELD_IN_STATUS = 3;
		private const int ARTFIELD_IN_NAME = 10;
		private const int ARTFIELD_IN_DESCRIPTION = 11;
		private const int ARTFIELD_TC_OWNER = 23;
		private const int ARTFIELD_TC_PRIORITY = 24;
		private const int ARTFIELD_TC_EXECUTION = 25;
		private const int ARTFIELD_TC_ACTIVE = 10;
		private const int ARTFIELD_TC_NAME = 29;
		private const int ARTFIELD_TC_DESCRIPTION = 105;
		private const int ARTFIELD_TK_ACTUALEFF = 66;
		private const int ARTFIELD_TK_STARTDATE = 62;
		private const int ARTFIELD_TK_STATUS = 57;
		private const int ARTFIELD_TK_DESCRIPTION = 106;
		#endregion

		#region Static Template Text
		private const string TEMPLATE_TEXT = @"This is a new template text for a random Artifact Type.
This item: ${ItemId} - ${ItemName} has been updated!
View the item here:
${ItemURL}
";
		#endregion

		//Index IDs.
		private static int notifyId_RQ = -1;
		private static int notifyId_IN = -1;
		private static int notifyId_TC = -1;
		private static int notifyId_TK = -1;

		//Project IDs
		private static int projectId;
		private static int projectTemplateId;

		//Original TK Template Text
		private static string notifyTemplateText;
		private static int notifyTemplateId = -1;

		//User IDs
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		private static NotificationManager notificationManager;

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate business class
			notificationManager = new NotificationManager();

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("NotificationTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		#region Notification Event Handlers

		[Test]
		[SpiraTestCase(691)]
		[Description("Creates four new events for each of the four main artifact types. (Req, Inc, TestCase, Task)")]
		public void _101_CreateNew()
		{
			//Create a new Test Event for a Requirement.
			notifyId_RQ = notificationManager.InsertEvent(
				REQ_NAME,
				(REQ_ACTIVE == true),
				(REQ_CREATE == true),
				projectTemplateId,
				REQ_ARTIFACT_TYPE,
				REQ_SUBJECT);

			//Create a new Test Event for a Incident.
			notifyId_IN = notificationManager.InsertEvent(
				INC_NAME,
				(INC_ACTIVE == true),
				(INC_CREATE == true),
				projectTemplateId,
				INC_ARTIFACT_TYPE,
				INC_SUBJECT);

			//Create a new Test Event for a Test Case.
			notifyId_TC = notificationManager.InsertEvent(
				TSC_NAME,
				(TSC_ACTIVE == true),
				(TSC_CREATE == true),
				projectTemplateId,
				TSC_ARTIFACT_TYPE,
				TSC_SUBJECT);

			//Create a new Test Event for a Test Case.
			notifyId_TK = notificationManager.InsertEvent(
				TSK_NAME,
				(TSK_ACTIVE == true),
				(TSK_CREATE == true),
				projectTemplateId,
				TSK_ARTIFACT_TYPE,
				TSK_SUBJECT);

			//Verify each one got a number.
			Assert.GreaterOrEqual(notifyId_RQ, 1, "Requirement Event Creation Error - ID less than 1.");
			Assert.GreaterOrEqual(notifyId_IN, 1, "Incident Event Creation Error - ID less than 1.");
			Assert.GreaterOrEqual(notifyId_TC, 1, "Test Case Event Creation Error - ID less than 1.");
			Assert.GreaterOrEqual(notifyId_TK, 1, "Task Event Creation Error - ID less than 1.");
		}

		[Test]
		[SpiraTestCase(692)]
		[Description("Retrieves one of the new requirements made. Verifies an event can be pulled from system.")]
		public void _102_Retrieve()
		{
			List<NotificationEvent> notifyEvents = notificationManager.RetrieveEvents(projectTemplateId);

			Assert.IsNotNull(notifyEvents, "Could not retrieve a notification event. Event was null.");
			Assert.GreaterOrEqual(notifyEvents.Count, 4, "Did not retrieve at least 4 events!");

			//Grab the template text for use later.
			if (notifyEvents.Count > 0)
			{
				NotificationEvent notifyEvent = notificationManager.RetrieveEventById(notifyEvents[0].NotificationEventId);
				if (notifyEvent != null)
				{
					NotificationArtifactTemplate template = notificationManager.RetrieveTemplateById(projectTemplateId, notifyEvent.ArtifactTypeId);
					Assert.IsNotNull(template);
					notifyTemplateId = template.ArtifactTypeId;
					notifyTemplateText = template.TemplateText;
				}
				else
				{
					Assert.Fail("Could not retrieve individual event!");
				}
			}

			//Verify that we can retrieve a single event by its ID
			NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_RQ);

			//Verify its data and that no fields, users or roles are set
			Assert.IsNotNull(notify);
			Assert.AreEqual(REQ_NAME, notify.Name);
			Assert.AreEqual(REQ_ACTIVE, notify.IsActive);
			Assert.AreEqual(REQ_CREATE, notify.IsArtifactCreation);
			Assert.AreEqual(projectTemplateId, notify.ProjectTemplateId);
			Assert.AreEqual(REQ_ARTIFACT_TYPE, notify.ArtifactTypeId);
			Assert.AreEqual(REQ_SUBJECT, notify.EmailSubject);

			Assert.AreEqual(0, notify.ProjectRoles.Count);
			Assert.AreEqual(0, notify.NotificationArtifactUserTypes.Count);
			Assert.AreEqual(0, notify.ArtifactFields.Count);
		}

		[Test]
		[SpiraTestCase(693)]
		[Description("Update the base info for an event.")]
		public void _103_UpdateEvent()
		{
			//Get the first requirement event.
			NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_RQ);

			//Make changes to the event itself
			notify.IsActive = REQ2_ACTIVE;
			notify.IsArtifactCreation = REQ2_CREATE;
			notify.Name = REQ2_NAME;

			//Update event
			notificationManager.SaveEvent(notify);

			//Verify
			notify = notificationManager.RetrieveEventById(notifyId_RQ);
			Assert.IsNotNull(notify);
			Assert.AreEqual(REQ2_NAME, notify.Name);
			Assert.AreEqual(REQ2_ACTIVE, notify.IsActive);
			Assert.AreEqual(REQ2_CREATE, notify.IsArtifactCreation);
			Assert.AreEqual(projectTemplateId, notify.ProjectTemplateId);
			Assert.AreEqual(REQ_ARTIFACT_TYPE, notify.ArtifactTypeId);
			Assert.AreEqual(REQ_SUBJECT, notify.EmailSubject);
		}

		[Test]
		[SpiraTestCase(694)]
		[Description("Adds a few roles to the existing events.")]
		public void _104_AddEventRole()
		{
			//Requirement
			NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_RQ);
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_DEVELOPER });
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_PROJOWNER });
			notificationManager.SaveEvent(notify);

			//Incident
			notify = notificationManager.RetrieveEventById(notifyId_IN);
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_DEVELOPER });
			notificationManager.SaveEvent(notify);

			//Test Case
			notify = notificationManager.RetrieveEventById(notifyId_TC);
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_DEVELOPER });
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_TESTER });
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_PROJOWNER });
			notificationManager.SaveEvent(notify);

			//Task
			notify = notificationManager.RetrieveEventById(notifyId_TK);
			notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_DEVELOPER });
			notificationManager.SaveEvent(notify);

			//Check that we don't throw exceptions if we try and add duplicates (they are just ignored)
			bool exNew1 = false;
			bool exNew2 = false;
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_RQ);
				notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_DEVELOPER });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew1 = true;
			}

			//Check that we do throw an exception if the role doesn't exist
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_TC);
				notify.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = PROJECT_NONEXIST });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew2 = true;
			}
			Assert.IsFalse(exNew1, "Could insert duplicate keys in Event_Role.");
			Assert.IsTrue(exNew2, "Could insert invalid keys in Event_Role.");
		}

		[Test]
		[SpiraTestCase(695)]
		[Description("Adds users to the existing events.")]
		public void _105_AddEventUser()
		{
			NotificationEvent notify;

			//Requirement
			notify = notificationManager.RetrieveEventById(notifyId_RQ);
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_CREATOR });
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_OWNER });
			notificationManager.SaveEvent(notify);

			//Incident
			notify = notificationManager.RetrieveEventById(notifyId_IN);
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_CREATOR });
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_OWNER });
			notificationManager.SaveEvent(notify);

			//Test Case
			notify = notificationManager.RetrieveEventById(notifyId_TC);
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_OWNER });
			notificationManager.SaveEvent(notify);

			//Task
			notify = notificationManager.RetrieveEventById(notifyId_TK);
			notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_OWNER });
			notificationManager.SaveEvent(notify);

			//Check that we don't throw exceptions if we try and add duplicates (they are just ignored)
			bool exNew1 = false;
			bool exNew2 = false;
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_RQ);
				notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = ARTUSER_OWNER });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew1 = true;
			}

			//Check that we do throw an exception if the user type doesn't exist
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_TC);
				notify.NotificationArtifactUserTypes.Add(new NotificationArtifactUserType() { ProjectArtifactNotifyTypeId = PROJECT_NONEXIST });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew2 = true;
			}
			Assert.IsFalse(exNew1, "Could insert duplicate keys in Event_User.");
			Assert.IsTrue(exNew2, "Could insert invalid keys in Event_User.");
		}

		[Test]
		[SpiraTestCase(696)]
		[Description("Adds fields to the existing events.")]
		public void _106_AddEventField()
		{
			NotificationEvent notify;

			//Requirement fields.
			notify = notificationManager.RetrieveEventById(notifyId_RQ);
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_RQ_COVERAGE });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_RQ_DESCRIPTION });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_RQ_NAME });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_RQ_OWNER });
			notificationManager.SaveEvent(notify);

			//Incident fields.
			notify = notificationManager.RetrieveEventById(notifyId_IN);
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_DESCRIPTION });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_NAME });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_PRIORITY });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_SEVERITY });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_STATUS });
			notificationManager.SaveEvent(notify);

			//Test Case fields.
			notify = notificationManager.RetrieveEventById(notifyId_TC);
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_ACTIVE });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_DESCRIPTION });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_EXECUTION });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_NAME });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_OWNER });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TC_PRIORITY });
			notificationManager.SaveEvent(notify);

			//Task fields.
			notify = notificationManager.RetrieveEventById(notifyId_TK);
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TK_ACTUALEFF });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TK_DESCRIPTION });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TK_STARTDATE });
			notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_TK_STATUS });
			notificationManager.SaveEvent(notify);

			//Check that we don't throw exceptions if we try and add duplicates (they are just ignored)
			bool exNew1 = false;
			bool exNew2 = false;
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_RQ);
				notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_RQ_NAME });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew1 = true;
			}

			//Check that we do throw an exception if the field doesn't exist
			try
			{
				notify = notificationManager.RetrieveEventById(notifyId_TC);
				notify.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = PROJECT_NONEXIST });
				notificationManager.SaveEvent(notify);
			}
			catch
			{
				exNew2 = true;
			}
			Assert.IsFalse(exNew1, "Could insert duplicate keys in Event_Field.");
			Assert.IsTrue(exNew2, "Could insert invalid keys in Event_Field.");
		}

		[Test]
		[SpiraTestCase(697)]
		[Description("Modifies an existing event and sub-items.")]
		public void _107_UpdateEventDetails()
		{
			//Load an event..
			NotificationEvent notificationEvent = notificationManager.RetrieveEventById(notifyId_IN);

			//Let's remove the SEVERITY field, then add it back in.
			notificationEvent.ArtifactFields.Remove(notificationEvent.ArtifactFields[0]);
			notificationEvent.ArtifactFields.Add(new ArtifactField() { ArtifactFieldId = ARTFIELD_IN_SEVERITY });

			//Now let's add the ProjectOwner role and remove the Developer
			notificationEvent.ProjectRoles.Remove(notificationEvent.ProjectRoles[0]);
			notificationEvent.ProjectRoles.Add(new ProjectRole() { ProjectRoleId = ROLE_PROJOWNER });

			//Okay, let's turn off Creator notification.
			notificationEvent.NotificationArtifactUserTypes.Remove(notificationEvent.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_CREATOR));

			//Let's try saving it.
			notificationManager.SaveEvent(notificationEvent);
		}

		[Test]
		[SpiraTestCase(698)]
		[Description("Retrieves all of the newly created events, and verifies their event data is as expected.")]
		public void _108_VerifyEventData()
		{
			//NOTE: Should be almost last, this will check all entered data.

			//Get each of the four items we just created
			NotificationEvent notifyRQ = notificationManager.RetrieveEventById(notifyId_RQ);
			NotificationEvent notifyIN = notificationManager.RetrieveEventById(notifyId_IN);
			NotificationEvent notifyTC = notificationManager.RetrieveEventById(notifyId_TC);
			NotificationEvent notifyTK = notificationManager.RetrieveEventById(notifyId_TK);

			//Verify the root event data in each.
			// - Requirement. (Changed data.)
			Assert.AreEqual(notifyRQ.Name, REQ2_NAME, "RQ Event is not the same. Field: Name");
			Assert.AreEqual(notifyRQ.IsActive, REQ2_ACTIVE, "RQ Event is not the same. Field: IsActive");
			Assert.AreEqual(notifyRQ.IsArtifactCreation, REQ2_CREATE, "RQ Event is not the same. Field: IsArtifactCreation");
			Assert.AreEqual(notifyRQ.ArtifactTypeId, REQ_ARTIFACT_TYPE, "RQ Event is not the same. Field: ArtifactTypeId");
			Assert.AreEqual(notifyRQ.ProjectTemplateId, projectTemplateId, "RQ Event is not the same. Field: ProjectId");
			// - Incident.
			Assert.AreEqual(notifyIN.Name, INC_NAME, "IN Event is not the same. Field: Name");
			Assert.AreEqual(notifyIN.IsActive, INC_ACTIVE, "IN Event is not the same. Field: IsActive");
			Assert.AreEqual(notifyIN.IsArtifactCreation, INC_CREATE, "IN Event is not the same. Field: IsArtifactCreation");
			Assert.AreEqual(notifyIN.ArtifactTypeId, INC_ARTIFACT_TYPE, "IN Event is not the same. Field: ArtifactTypeId");
			Assert.AreEqual(notifyIN.ProjectTemplateId, projectTemplateId, "IN Event is not the same. Field: ProjectId");
			// - Test Case.
			Assert.AreEqual(notifyTC.Name, TSC_NAME, "TC Event is not the same. Field: Name");
			Assert.AreEqual(notifyTC.IsActive, TSC_ACTIVE, "TC Event is not the same. Field: IsActive");
			Assert.AreEqual(notifyTC.IsArtifactCreation, TSC_CREATE, "TC Event is not the same. Field: IsArtifactCreation");
			Assert.AreEqual(notifyTC.ArtifactTypeId, TSC_ARTIFACT_TYPE, "TC Event is not the same. Field: ArtifactTypeId");
			Assert.AreEqual(notifyTC.ProjectTemplateId, projectTemplateId, "TC Event is not the same. Field: ProjectId");
			// - Task.
			Assert.AreEqual(notifyTK.Name, TSK_NAME, "TK Event is not the same. Field: Name");
			Assert.AreEqual(notifyTK.IsActive, TSK_ACTIVE, "TK Event is not the same. Field: IsActive");
			Assert.AreEqual(notifyTK.IsArtifactCreation, TSK_CREATE, "TK Event is not the same. Field: IsArtifactCreation");
			Assert.AreEqual(notifyTK.ArtifactTypeId, TSK_ARTIFACT_TYPE, "TK Event is not the same. Field: ArtifactTypeId");
			Assert.AreEqual(notifyTK.ProjectTemplateId, projectTemplateId, "TK Event is not the same. Field: ProjectId");
		}

		[Test]
		[SpiraTestCase(699)]
		[Description("Retrieves all of the newly created events, and verifies their field data is as expected.")]
		public void _109_VerifyFieldData()
		{
			//Get each of the four items we just created
			NotificationEvent notifyRQ = notificationManager.RetrieveEventById(notifyId_RQ);
			NotificationEvent notifyIN = notificationManager.RetrieveEventById(notifyId_IN);
			NotificationEvent notifyTC = notificationManager.RetrieveEventById(notifyId_TC);
			NotificationEvent notifyTK = notificationManager.RetrieveEventById(notifyId_TK);

			//Verify fields are as expected.
			// * Requirement
			Assert.AreEqual(4, notifyRQ.ArtifactFields.Count, "RQ - Number of fields do not match.");
			Assert.IsNotNull(notifyRQ.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_RQ_COVERAGE), "RQ - Field COVERAGE not found.");
			Assert.IsNotNull(notifyRQ.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_RQ_DESCRIPTION), "RQ - Field DESCRIPTION not found.");
			Assert.IsNotNull(notifyRQ.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_RQ_NAME), "RQ - Field NAME not found.");
			Assert.IsNotNull(notifyRQ.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_RQ_OWNER), "RQ - Field OWNER not found.");
			// * Incident (changed above)
			Assert.AreEqual(5, notifyIN.ArtifactFields.Count, "IN - Number of fields do not match.");
			Assert.IsNotNull(notifyIN.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_IN_NAME), "IN - Field NAME not found.");
			Assert.IsNotNull(notifyIN.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_IN_SEVERITY), "IN - Field SEVERITY not found.");
			Assert.IsNotNull(notifyIN.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_IN_PRIORITY), "IN - Field PRIORITY not found.");
			Assert.IsNotNull(notifyIN.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_IN_DESCRIPTION), "IN - Field DESCRIPTION not found.");
			Assert.IsNotNull(notifyIN.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_IN_STATUS), "IN - Field STATUS not found.");
			// * Test Case
			Assert.AreEqual(6, notifyTC.ArtifactFields.Count, "TC - Number of fields do not match.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_ACTIVE), "TC - Field ACTIVE not found.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_DESCRIPTION), "TC - Field DESCRIPTION not found.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_EXECUTION), "TC - Field EXECUTION not found.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_NAME), "TC - Field NAME not found.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_OWNER), "TC - Field OWNER not found.");
			Assert.IsNotNull(notifyTC.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TC_PRIORITY), "TC - Field PRIORITY not found.");
			// * Task
			Assert.AreEqual(4, notifyTK.ArtifactFields.Count, "TK - Number of fields do not match.");
			Assert.IsNotNull(notifyTK.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TK_ACTUALEFF), "TK - Field ACTUAL_EFEORT not found.");
			Assert.IsNotNull(notifyTK.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TK_DESCRIPTION), "TK - Field DESCRIPTION not found.");
			Assert.IsNotNull(notifyTK.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TK_STARTDATE), "TK - Field START_DATE not found.");
			Assert.IsNotNull(notifyTK.ArtifactFields.FirstOrDefault(n => n.ArtifactFieldId == ARTFIELD_TK_STATUS), "TK - Field STATUS not found.");
		}

		[Test]
		[SpiraTestCase(700)]
		[Description("Retrieves all of the newly created events, and verifies their specified roles are as expected.")]
		public void _110_VerifyRoleData()
		{
			//Get each of the four items we just created
			NotificationEvent notifyRQ = notificationManager.RetrieveEventById(notifyId_RQ);
			NotificationEvent notifyIN = notificationManager.RetrieveEventById(notifyId_IN);
			NotificationEvent notifyTC = notificationManager.RetrieveEventById(notifyId_TC);
			NotificationEvent notifyTK = notificationManager.RetrieveEventById(notifyId_TK);

			//Verify roles are as expected.
			// * Requirement
			Assert.AreEqual(2, notifyRQ.ProjectRoles.Count, "RQ - Number of roles do not match.");
			Assert.IsNotNull(notifyRQ.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_DEVELOPER), "RQ - Could not find Role DEVELOPER.");
			Assert.IsNotNull(notifyRQ.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_PROJOWNER), "RQ - Could not find Role PROJECT OWNER.");
			// * Incident
			Assert.AreEqual(1, notifyIN.ProjectRoles.Count, "IN - Number of roles do not match.");
			Assert.IsNotNull(notifyIN.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_PROJOWNER), "IN - Could not find Role PROJECT OWNER.");
			// * Test Case
			Assert.AreEqual(3, notifyTC.ProjectRoles.Count, "TC - Number of roles do not match.");
			Assert.IsNotNull(notifyTC.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_DEVELOPER), "TC - Could not find Role DEVELOPER.");
			Assert.IsNotNull(notifyTC.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_TESTER), "TC - Could not find Role TESTER.");
			Assert.IsNotNull(notifyTC.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_PROJOWNER), "TC - Could not find Role PROJECT OWNER.");
			// * Task
			Assert.AreEqual(1, notifyTK.ProjectRoles.Count, "TK - Number of roles do not match.");
			Assert.IsNotNull(notifyTK.ProjectRoles.FirstOrDefault(p => p.ProjectRoleId == ROLE_DEVELOPER), "TK - Could not find Role DEVELOPER.");

		}

		[Test]
		[SpiraTestCase(701)]
		[Description("Retrieves all of the newly created events, and verifies their users are as expected.")]
		public void _111_VerifyUserData()
		{
			//Get each of the four items we just created
			NotificationEvent notifyRQ = notificationManager.RetrieveEventById(notifyId_RQ);
			NotificationEvent notifyIN = notificationManager.RetrieveEventById(notifyId_IN);
			NotificationEvent notifyTC = notificationManager.RetrieveEventById(notifyId_TC);
			NotificationEvent notifyTK = notificationManager.RetrieveEventById(notifyId_TK);

			//Verify roles are as expected.
			// * Requirement
			Assert.AreEqual(2, notifyRQ.NotificationArtifactUserTypes.Count, "RQ - Number of users do not match.");
			Assert.IsNotNull(notifyRQ.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_CREATOR), "RQ - Could not find user CREATOR.");
			Assert.IsNotNull(notifyRQ.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_OWNER), "RQ - Could not find user OWNER.");
			// * Incident
			Assert.AreEqual(1, notifyIN.NotificationArtifactUserTypes.Count, "IN - Number of users do not match.");
			Assert.IsNotNull(notifyIN.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_OWNER), "IN - Could not find user OWNER.");
			// * Test Case
			Assert.AreEqual(1, notifyTC.NotificationArtifactUserTypes.Count, "TC - Number of users do not match.");
			Assert.IsNotNull(notifyTC.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_OWNER), "TC - Could not find user OWNER.");
			// * Task
			Assert.AreEqual(1, notifyTK.NotificationArtifactUserTypes.Count, "TK - Number of users do not match.");
			Assert.IsNotNull(notifyTK.NotificationArtifactUserTypes.FirstOrDefault(n => n.ProjectArtifactNotifyTypeId == ARTUSER_OWNER), "TK - Could not find user OWNER.");
		}

		[Test]
		[SpiraTestCase(702)]
		[Description("Changes an artifact's template text.")]
		public void _112_VerifyChangeTemplate()
		{
			if (notifyTemplateId > 0)
			{
				//Change the template text..
				NotificationArtifactTemplate template = notificationManager.RetrieveTemplateById(projectTemplateId, notifyTemplateId);
				template.StartTracking();
				template.TemplateText = TEMPLATE_TEXT;
				notificationManager.UpdateTemplate(template);

				//Verify the template text was saved properly.
				string savedTemplate = notificationManager.RetrieveTemplateTextById(projectTemplateId, notifyTemplateId);
				Assert.AreEqual(TEMPLATE_TEXT, savedTemplate, "Saved template did not match retrieved template!");

				//Now restore it.
				template = notificationManager.RetrieveTemplateById(projectTemplateId, notifyTemplateId);
				template.StartTracking();
				template.TemplateText = notifyTemplateText;
				notificationManager.UpdateTemplate(template);
				string restoredTemplate = notificationManager.RetrieveTemplateTextById(projectTemplateId, notifyTemplateId);
				Assert.AreEqual(notifyTemplateText, restoredTemplate, "Restored template did not save properly!");
			}
			else
			{
				//Will report back as 'Blocked'
				Assert.Inconclusive("Could not run test, original Template Text was not saved.");
			}
		}

		[Test]
		[SpiraTestCase(703)]
		[Description("Tests that we can copy a project's notification events")]
		public void _113_CopyProjectNotification()
		{
			//Create a project to copy to (different templates)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			int projectId2 = projectManager.Insert("NotificationTest Project 2", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			int projectTemplateId2 = new TemplateManager().RetrieveForProject(projectId2).ProjectTemplateId;

			//Get original counts..
			int numToProjectBeforeCopy = notificationManager.RetrieveEvents(projectTemplateId2).Count;
			int numFromProjectBeforeCopy = notificationManager.RetrieveEvents(projectTemplateId).Count;

			//Now call the command to copy from first project to second.
			notificationManager.CopyProjectEvents(projectTemplateId, projectTemplateId2);

			//Get new counts.
			int numToProjectAfterCopy = notificationManager.RetrieveEvents(projectTemplateId2).Count;
			int numFromProjectAfterCopy = notificationManager.RetrieveEvents(projectTemplateId).Count;

			//Verify counts are accurate.
			Assert.AreEqual((int)(numToProjectBeforeCopy + numFromProjectBeforeCopy), numToProjectAfterCopy, "Not all events were copied!");
			Assert.AreEqual(numFromProjectBeforeCopy, numFromProjectAfterCopy, "Events were added or missing from original project!");

			//Delete created project and template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId2);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId2);
		}

		[Test]
		[SpiraTestCase(704)]
		[Description("Adds Requirement #1 to the user's subscription list.")]
		public void _114_AddUserSubscription()
		{
			//Add a subscription request for a user.
			notificationManager.AddUserSubscription(ARTUSER_CREATOR, REQ_ARTIFACT_TYPE, 1);
		}

		[Test]
		[SpiraTestCase(711)]
		[Description("Verifies the user was added correctly.")]
		public void _115_CheckUserSubscription()
		{
			//Test that we can check that a user is subscribed and also get the list of subscriptions for a user
			bool isUserSubbed = notificationManager.IsUserSubscribed(ARTUSER_CREATOR, REQ_ARTIFACT_TYPE, 1);
			List<NotificationUserSubscriptionView> userSubscriptions = notificationManager.RetrieveSubscriptionsForUser(ARTUSER_CREATOR, null);

			Assert.IsTrue(isUserSubbed, "User was not subscribed to ArtifactType 1, ArtifactId 1!");
			Assert.AreEqual(1, userSubscriptions.Count, "User had more entries than expected!");

			//ARTUSER_CREATOR, REQ_ARTIFACT_TYPE, 1

			//Now check that we can get the list of subscriptions together with the appropriate lookup values (all projects)
			userSubscriptions = notificationManager.RetrieveSubscriptionsForUser(ARTUSER_CREATOR, null);
			Assert.AreEqual(1, userSubscriptions.Count);
			Assert.AreEqual(1, userSubscriptions[0].ArtifactId);
			Assert.AreEqual(1, userSubscriptions[0].ProjectId);
			Assert.AreEqual(REQ_ARTIFACT_TYPE, userSubscriptions[0].ArtifactTypeId);
			Assert.AreEqual("Functional System Requirements", userSubscriptions[0].ArtifactName);
			Assert.AreEqual("Library Information System (Sample)", userSubscriptions[0].ProjectName);

			//Now check that we can get the list of subscriptions together with the appropriate lookup values (specific project)
			userSubscriptions = notificationManager.RetrieveSubscriptionsForUser(ARTUSER_CREATOR, 1);
			Assert.AreEqual(1, userSubscriptions.Count);
			Assert.AreEqual(1, userSubscriptions[0].ArtifactId);
			Assert.AreEqual(1, userSubscriptions[0].ProjectId);
			Assert.AreEqual(REQ_ARTIFACT_TYPE, userSubscriptions[0].ArtifactTypeId);
			Assert.AreEqual("Functional System Requirements", userSubscriptions[0].ArtifactName);
			Assert.AreEqual("Library Information System (Sample)", userSubscriptions[0].ProjectName);

			//Now check that it doesn't return results if we specify the wrong project
			userSubscriptions = notificationManager.RetrieveSubscriptionsForUser(ARTUSER_CREATOR, 2);
			Assert.AreEqual(0, userSubscriptions.Count);
		}

		[Test]
		[SpiraTestCase(705)]
		[Description("Removes Requirement #1 from the user's subscription list.")]
		public void _116_RemoveUserSubscription()
		{
			//Add a subscription request for a user.
			notificationManager.RemoveUserSubscription(ARTUSER_CREATOR, REQ_ARTIFACT_TYPE, 1);
		}

		[Test]
		[SpiraTestCase(706)]
		[Description("Verifies the user was removed correctly.")]
		public void _117_CheckUserSubscription()
		{
			bool isUserSubbed = notificationManager.IsUserSubscribed(ARTUSER_CREATOR, REQ_ARTIFACT_TYPE, 1);
			List<NotificationUserSubscriptionView> userSubscriptions = notificationManager.RetrieveSubscriptionsForUser(ARTUSER_CREATOR, null);

			Assert.IsFalse(isUserSubbed, "User was still subscribed to ArtifactType 1, ArtifactId 1!");
			Assert.AreEqual(0, userSubscriptions.Count, "User had more entries than expected!");
		}

		#endregion

		#region E-Mail Sending

		[Test]
		[SpiraTestCase(707)]
		[Description("Tests template tokens.")]
		public void _201_TestNotification()
		{
			string newName = "Test Incident";
			IncidentView incident = new IncidentManager().Incident_New(1, 1);
			incident.ActualEffort = 129;
			incident.IsAttachments = false;
			incident.ClosedDate = DateTime.UtcNow.AddHours(2);
			incident.CreationDate = DateTime.UtcNow;
			incident.CompletionPercent = 0;
			incident.Description = "None";
			incident.DetectedReleaseId = 11;
			incident.EstimatedEffort = 216;
			incident.IncidentStatusId = 1;
			incident.IncidentStatusName = "Assigned";
			incident.IncidentTypeId = 1;
			incident.IncidentTypeName = "Enhancement";
			incident.LastUpdateDate = DateTime.Now;
			incident.Name = newName;
			incident.OpenerId = 1;
			incident.OpenerName = "Administrator";
			incident.OwnerId = 2;
			incident.OwnerName = "Joe Bloggs";
			incident.PriorityId = 1;
			incident.PriorityName = "Critical";
			incident.ProjectId = 1;
			incident.ResolvedReleaseId = 12;
			incident.SeverityId = 1;
			incident.SeverityName = "High";
			incident.StartDate = DateTime.Now.AddHours(-12);
			incident.VerifiedReleaseId = 13;

			//Get data needed.
			string projName = new ProjectManager().RetrieveById(incident.ProjectId).Name;

			//Get the incident fields
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveAll((int)Artifact.ArtifactTypeEnum.Incident);

			//Test for case-sensitivity.
			string test_cap1 = notificationManager.TranslateTemplate(incident, "${name}", artifactFields);
			string test_cap2 = notificationManager.TranslateTemplate(incident, "${Name}", artifactFields);
			string test_cap3 = notificationManager.TranslateTemplate(incident, "${nAME}", artifactFields);
			string test_cap4 = notificationManager.TranslateTemplate(incident, "${NAME}", artifactFields);
			Assert.AreEqual(newName, test_cap1, "Case-Sensitivity test #1 did not match!");
			Assert.AreEqual(newName, test_cap2, "Case-Sensitivity test #2 did not match!");
			Assert.AreEqual(newName, test_cap3, "Case-Sensitivity test #3 did not match!");
			Assert.AreEqual(newName, test_cap4, "Case-Sensitivity test #4 did not match!");

			//Test all available fields. Get incident to test from, and pull other needed data to verify.
			ReleaseView relRow1 = new ReleaseManager().RetrieveById2(incident.ProjectId, incident.DetectedReleaseId.Value);
			ReleaseView relRow2 = new ReleaseManager().RetrieveById2(incident.ProjectId, incident.ResolvedReleaseId.Value);
			ReleaseView relRow3 = new ReleaseManager().RetrieveById2(incident.ProjectId, incident.VerifiedReleaseId.Value);
			incident.DetectedReleaseVersionNumber = relRow1.VersionNumber;
			incident.ResolvedReleaseVersionNumber = relRow2.VersionNumber;
			incident.VerifiedReleaseVersionNumber = relRow3.VersionNumber;


			//Fixed fields
			string url = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Incidents, incident.ProjectId, incident.IncidentId, null, true);
			string href = "<a href=\"" + url + "\" target=\"_blank\">" + url + "</a>";
			Assert.AreEqual(href, notificationManager.TranslateTemplate(incident, "${url}", artifactFields), "Token URL did mot match!");
			Assert.AreEqual(ConfigurationSettings.Default.License_ProductType, notificationManager.TranslateTemplate(incident, "${product}", artifactFields), "Token PRODUCT did mot match!");
			Assert.AreEqual("${1}", notificationManager.TranslateTemplate(incident, "${notifyto}", artifactFields), "Token NOTIFYTO did not match!");
			Assert.AreEqual(projName, notificationManager.TranslateTemplate(incident, "${projectname}", artifactFields), "Token PROJECTNAME did not match.");
			Assert.AreEqual("${2}", notificationManager.TranslateTemplate(incident, "${eventname}", artifactFields), "Token EVENTNAME did not match.");

			//Now, artifact fields.
			Assert.AreEqual(((int)(incident.ActualEffort / 60)).ToString() + " hours and " + ((int)(incident.ActualEffort % 60)).ToString() + " minutes", notificationManager.TranslateTemplate(incident, "${actualeffort}", artifactFields), "Token ACTUALEFFORT did not match!");
			Assert.AreEqual(incident.ClosedDate.Value.ToShortDateString(), notificationManager.TranslateTemplate(incident, "${closeddate}", artifactFields), "Token CLOSEDDATE did not match.");
			Assert.AreEqual(incident.CompletionPercent.ToString(), notificationManager.TranslateTemplate(incident, "${completionpercent}", artifactFields), "Token COMPLETIONPERCENT did not match.");
			Assert.AreEqual(incident.Description, notificationManager.TranslateTemplate(incident, "${description}", artifactFields), "Token DESCRIPTION did not match.");
			Assert.AreEqual(relRow1.VersionNumber + " - " + relRow1.Name, notificationManager.TranslateTemplate(incident, "${detectedrelease}", artifactFields), "Token DETECTEDRELEASE did not match.");
			Assert.AreEqual(((int)(incident.EstimatedEffort / 60)).ToString() + " hours and " + ((int)(incident.EstimatedEffort % 60)).ToString() + " minutes", notificationManager.TranslateTemplate(incident, "${estimatedeffort}", artifactFields), "Token ESTIMATEDEFFORT did not match.");
			Assert.AreEqual(incident.IncidentTypeName, notificationManager.TranslateTemplate(incident, "${incidenttype}", artifactFields), "Token INCIDENTTYPE did not match.");
			Assert.AreEqual(incident.Name, notificationManager.TranslateTemplate(incident, "${name}", artifactFields), "Token NAME did not match.");
			Assert.AreEqual(incident.OpenerName, notificationManager.TranslateTemplate(incident, "${opener}", artifactFields), "Token OPENER did not match.");
			Assert.AreEqual(incident.OwnerName, notificationManager.TranslateTemplate(incident, "${owner}", artifactFields), "Token OWNER did not match.");
			Assert.AreEqual(incident.PriorityName, notificationManager.TranslateTemplate(incident, "${priority}", artifactFields), "Token PRIORITY did not match.");
			Assert.AreEqual(relRow2.VersionNumber + " - " + relRow2.Name, notificationManager.TranslateTemplate(incident, "${resolvedrelease}", artifactFields), "Token RESOLVEDRELEASE did not match.");
			Assert.AreEqual(incident.SeverityName, notificationManager.TranslateTemplate(incident, "${severity}", artifactFields), "Token SEVERITY did not match.");
			Assert.AreEqual(incident.StartDate.Value.ToShortDateString(), notificationManager.TranslateTemplate(incident, "${startdate}", artifactFields), "Token STARTDATE did not match.");
			Assert.AreEqual(relRow3.VersionNumber + " - " + relRow3.Name, notificationManager.TranslateTemplate(incident, "${verifiedrelease}", artifactFields), "Token VERIFIEDRELEASE did not match.");

			//Invalid fields & Text.
			Assert.AreEqual("", notificationManager.TranslateTemplate(incident, "${bobshair}", artifactFields), "Token BOBSHAIR did not return empty string.");
			Assert.AreEqual("Bob's hair is the best.", notificationManager.TranslateTemplate(incident, "Bob's hair is the best.", artifactFields), "Constant string did not return original.");
		}

		[Test]
		[SpiraTestCase(708)]
		[Description("Creates a new incident row, and tries to send an e-mail notification.")]
		public void _202_NewIncident()
		{
			string newName = "Test Incident";
			IncidentView incident = new IncidentManager().Incident_New(1, 1);
			incident.ActualEffort = 129;
			incident.IsAttachments = false;
			incident.ClosedDate = DateTime.Now.AddHours(2);
			incident.CreationDate = DateTime.Now;
			incident.CompletionPercent = 0;
			incident.Description = "None";
			incident.DetectedReleaseId = 11;
			incident.EstimatedEffort = 216;
			incident.IncidentStatusId = 1;
			incident.IncidentStatusName = "Assigned";
			incident.IncidentTypeId = 1;
			incident.IncidentTypeName = "Enhancement";
			incident.LastUpdateDate = DateTime.Now;
			incident.Name = newName;
			incident.OpenerId = 1;
			incident.OpenerName = "Administrator";
			incident.OwnerId = 2;
			incident.OwnerName = "Joe Bloggs";
			incident.PriorityId = 1;
			incident.PriorityName = "Critical";
			incident.ProjectId = 1;
			incident.ResolvedReleaseId = 12;
			incident.SeverityId = 1;
			incident.SeverityName = "High";
			incident.StartDate = DateTime.Now.AddHours(-12);
			incident.VerifiedReleaseId = 13;

			try
			{
				notificationManager.SendNotificationForArtifact(incident);
			}
			catch (Exception ex)
			{
				string exMsg = ex.Message;
				string exStack = ex.StackTrace;
				while (ex.InnerException != null)
				{
					exMsg += Environment.NewLine + ex.InnerException.Message;
					ex = ex.InnerException;
				}

				string repMsg = "Sending notification threw an exception!" +
					Environment.NewLine + Environment.NewLine +
					exMsg +
					Environment.NewLine + Environment.NewLine +
					exStack;

				Assert.Fail(repMsg);
			}

		}

		#endregion

		[Test]
		[SpiraTestCase(709)]
		[Description("Deletes the test events created, and verifies all data linked to them are deleted as well.")]
		public void _XXX_DeleteData()
		{
			//NOTE: Should be last

			//Delete each newly-created event.
			notificationManager.DeleteEvent(notifyId_RQ);
			notificationManager.DeleteEvent(notifyId_IN);
			notificationManager.DeleteEvent(notifyId_TC);
			notificationManager.DeleteEvent(notifyId_TK);

			//Not try to retrieve each. Verify that each one throws an exception.
			bool exRQ = false;
			bool exIN = false;
			bool exTC = false;
			bool exTK = false;
			try
			{
				NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_RQ);
			}
			catch
			{
				exRQ = true;
			}
			try
			{
				NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_IN);
			}
			catch
			{
				exIN = true;
			}
			try
			{
				NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_TC);
			}
			catch
			{
				exTC = true;
			}
			try
			{
				NotificationEvent notify = notificationManager.RetrieveEventById(notifyId_TK);
			}
			catch
			{
				exTK = true;
			}

			Assert.IsTrue(exRQ, "Problem deleting " + REQ_NAME + ".");
			Assert.IsTrue(exIN, "Problem deleting " + INC_NAME + ".");
			Assert.IsTrue(exTC, "Problem deleting " + TSC_NAME + ".");
			Assert.IsTrue(exTK, "Problem deleting " + TSK_NAME + ".");
		}
	}
}
