using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;


namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the RiskManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class RiskManagerTest
	{
		private static int projectTemplateId;
		private static int projectId;
		private static int projectId2;

		private const int PROJECT_GROUP_ID_DEFAULT = 1;

		private static RiskManager riskManager;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			riskManager = new Business.RiskManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create two new projects for testing with
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("RiskManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Use the same template as the first one
			projectId2 = projectManager.Insert("RiskManagerTest Project 2", null, null, null, true, projectTemplateId, 1, adminSectionId, "Inserted Project");
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary projects and their template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId2);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Tests that you can create, edit, delete risk types
		/// </summary>
		[Test]
		[SpiraTestCase(1785)]
		public void _01_EditTypes()
		{
			//First lets get the list of types in the current template
			List<RiskType> types = riskManager.RiskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(5, types.Count);

			//Next lets add a new type
			int riskTypeId1 = riskManager.RiskType_Insert(
				projectTemplateId,
				"Conceptual",
				null,
				false,
				true
				);

			//Verify that it was created
			RiskType riskType = riskManager.RiskType_RetrieveById(riskTypeId1);
			Assert.IsNotNull(riskType);
			Assert.AreEqual("Conceptual", riskType.Name);
			Assert.AreEqual(true, riskType.IsActive);
			Assert.AreEqual(false, riskType.IsDefault);

			//Next lets add another new type
			int riskTypeId2 = riskManager.RiskType_Insert(
				projectTemplateId,
				"Management",
				null,
				false,
				true
				);

			//Verify that it was created
			riskType = riskManager.RiskType_RetrieveById(riskTypeId2);
			Assert.IsNotNull(riskType);
			Assert.AreEqual("Management", riskType.Name);
			Assert.AreEqual(true, riskType.IsActive);
			Assert.AreEqual(false, riskType.IsDefault);

			//Make changes
			riskType.StartTracking();
			riskType.Name = "Operational";
			riskType.IsActive = false;
			riskManager.RiskType_Update(riskType);

			//Verify the changes
			//Verify that it was created
			riskType = riskManager.RiskType_RetrieveById(riskTypeId2);
			Assert.IsNotNull(riskType);
			Assert.AreEqual("Operational", riskType.Name);
			Assert.AreEqual(false, riskType.IsActive);
			Assert.AreEqual(false, riskType.IsDefault);

			//Verify that we can get the total count of types
			types = riskManager.RiskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(7, types.Count);

			//Verify that we can get the total count of active types
			types = riskManager.RiskType_Retrieve(projectTemplateId, true);
			Assert.AreEqual(6, types.Count);

			//Verify that we can get the default type
			riskType = riskManager.RiskType_RetrieveDefault(projectTemplateId);
			Assert.IsNotNull(riskType);
			Assert.AreEqual("Business", riskType.Name);
			Assert.AreEqual(true, riskType.IsActive);
			Assert.AreEqual(true, riskType.IsDefault);

			//Delete our new types (internal function only, not possible in the UI)
			riskManager.RiskType_Delete(riskTypeId1);
			riskManager.RiskType_Delete(riskTypeId2);

			//Verify the count
			types = riskManager.RiskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(5, types.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete risk statuses
		/// </summary>
		[Test]
		[SpiraTestCase(1786)]
		public void _02_EditStatuses()
		{
			//First lets get the list of statuses in the current template
			List<RiskStatus> statuses = riskManager.RiskStatus_Retrieve(projectTemplateId, false);
			Assert.AreEqual(6, statuses.Count);
			List<int> ids = riskManager.RiskStatus_RetrieveOpenIds(projectTemplateId);
			Assert.AreEqual(4, ids.Count);
			ids = riskManager.RiskStatus_RetrieveClosedIds(projectTemplateId);
			Assert.AreEqual(2, ids.Count);

			//Next lets add a new status
			int riskStatusId1 = riskManager.RiskStatus_Insert(
				projectTemplateId,
				"Bogus",
				true,
				false,
				true
				);

			//Verify that it was created
			RiskStatus riskStatus = riskManager.RiskStatus_RetrieveById(riskStatusId1);
			Assert.IsNotNull(riskStatus);
			Assert.AreEqual("Bogus", riskStatus.Name);
			Assert.AreEqual(true, riskStatus.IsActive);
			Assert.AreEqual(true, riskStatus.IsOpen);
			Assert.AreEqual(false, riskStatus.IsDefault);

			//Next lets add another new status
			int riskStatusId2 = riskManager.RiskStatus_Insert(
				projectTemplateId,
				"Escalated",
				false,
				false,
				true
				);

			//Verify that it was created
			riskStatus = riskManager.RiskStatus_RetrieveById(riskStatusId2);
			Assert.IsNotNull(riskStatus);
			Assert.AreEqual("Escalated", riskStatus.Name);
			Assert.AreEqual(true, riskStatus.IsActive);
			Assert.AreEqual(false, riskStatus.IsDefault);
			Assert.AreEqual(false, riskStatus.IsOpen);

			//Make changes
			riskStatus.StartTracking();
			riskStatus.Name = "Parked";
			riskStatus.IsActive = false;
			riskManager.RiskStatus_Update(riskStatus);

			//Verify the changes
			//Verify that it was created
			riskStatus = riskManager.RiskStatus_RetrieveById(riskStatusId2);
			Assert.IsNotNull(riskStatus);
			Assert.AreEqual("Parked", riskStatus.Name);
			Assert.AreEqual(false, riskStatus.IsActive);
			Assert.AreEqual(false, riskStatus.IsDefault);
			Assert.AreEqual(false, riskStatus.IsOpen);

			//Verify that we can get the total count of statuses
			statuses = riskManager.RiskStatus_Retrieve(projectTemplateId, false);
			Assert.AreEqual(8, statuses.Count);

			//Verify that we can get the total count of active statuses
			statuses = riskManager.RiskStatus_Retrieve(projectTemplateId, true);
			Assert.AreEqual(7, statuses.Count);

			//Verify that we can get the default status
			riskStatus = riskManager.RiskStatus_RetrieveDefault(projectTemplateId);
			Assert.IsNotNull(riskStatus);
			Assert.AreEqual("Identified", riskStatus.Name);
			Assert.AreEqual(true, riskStatus.IsActive);
			Assert.AreEqual(true, riskStatus.IsDefault);
			Assert.AreEqual(true, riskStatus.IsOpen);

			//Delete our new statuses (internal function only, not possible in the UI)
			riskManager.RiskStatus_Delete(riskStatusId1);
			riskManager.RiskStatus_Delete(riskStatusId2);

			//Verify the count
			statuses = riskManager.RiskStatus_Retrieve(projectTemplateId, false);
			Assert.AreEqual(6, statuses.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete risk probabilities
		/// </summary>
		[Test]
		[SpiraTestCase(1787)]
		public void _03_EditProbabilities()
		{
			//First lets get the list of probabilities in the current template
			List<RiskProbability> probabilities = riskManager.RiskProbability_Retrieve(projectTemplateId);
			Assert.AreEqual(5, probabilities.Count);

			//Next lets add a new probability
			int probabilityId1 = riskManager.RiskProbability_Insert(
				projectTemplateId,
				"Impossible",
				"eeeeee",
				true,
				1
				);

			//Verify that it was created
			RiskProbability probability = riskManager.RiskProbability_RetrieveById(probabilityId1);
			Assert.IsNotNull(probability);
			Assert.AreEqual("Impossible", probability.Name);
			Assert.AreEqual(true, probability.IsActive);
			Assert.AreEqual("eeeeee", probability.Color);
			Assert.AreEqual(1, probability.Score);
			Assert.AreEqual(6, probability.Position);

			//Make changes
			probability.StartTracking();
			probability.Name = "Hell freezes over";
			probability.Color = "dddddd";
			probability.Score = 2;
			riskManager.RiskProbability_Update(probability);

			//Verify the changes
			probability = riskManager.RiskProbability_RetrieveById(probabilityId1);
			Assert.IsNotNull(probability);
			Assert.AreEqual("Hell freezes over", probability.Name);
			Assert.AreEqual(true, probability.IsActive);
			Assert.AreEqual("dddddd", probability.Color);
			Assert.AreEqual(2, probability.Score);

			//Verify that we can get the total count of probabilities
			probabilities = riskManager.RiskProbability_Retrieve(projectTemplateId);
			Assert.AreEqual(6, probabilities.Count);

			//Delete our new probabilities (internal function only, not possible in the UI)
			riskManager.RiskProbability_Delete(probabilityId1);

			//Verify the count
			probabilities = riskManager.RiskProbability_Retrieve(projectTemplateId);
			Assert.AreEqual(5, probabilities.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete risk impacts
		/// </summary>
		[Test]
		[SpiraTestCase(1788)]
		public void _04_EditImpacts()
		{
			//First lets get the list of impacts in the current template
			List<RiskImpact> impacts = riskManager.RiskImpact_Retrieve(projectTemplateId);
			Assert.AreEqual(5, impacts.Count);

			//Next lets add a new impact
			int impactId1 = riskManager.RiskImpact_Insert(
				projectTemplateId,
				"Apocalyptic",
				"eeeeee",
				true,
				5
				);

			//Verify that it was created
			RiskImpact impact = riskManager.RiskImpact_RetrieveById(impactId1);
			Assert.IsNotNull(impact);
			Assert.AreEqual("Apocalyptic", impact.Name);
			Assert.AreEqual(true, impact.IsActive);
			Assert.AreEqual("eeeeee", impact.Color);
			Assert.AreEqual(5, impact.Score);
			Assert.AreEqual(6, impact.Position);

			//Make changes
			impact.StartTracking();
			impact.Name = "Zombie Invasion";
			impact.Color = "dddddd";
			impact.Score = 10;
			riskManager.RiskImpact_Update(impact);

			//Verify the changes
			impact = riskManager.RiskImpact_RetrieveById(impactId1);
			Assert.IsNotNull(impact);
			Assert.AreEqual("Zombie Invasion", impact.Name);
			Assert.AreEqual(true, impact.IsActive);
			Assert.AreEqual("dddddd", impact.Color);
			Assert.AreEqual(10, impact.Score);

			//Verify that we can get the total count of impacts
			impacts = riskManager.RiskImpact_Retrieve(projectTemplateId);
			Assert.AreEqual(6, impacts.Count);

			//Delete our new impacts (internal function only, not possible in the UI)
			riskManager.RiskImpact_Delete(impactId1);

			//Verify the count
			impacts = riskManager.RiskImpact_Retrieve(projectTemplateId);
			Assert.AreEqual(5, impacts.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete, view risks
		/// </summary>
		[Test]
		[SpiraTestCase(1789)]
		public void _05_CreateEditViewDeleteRisks()
		{
			//We need to create a component and release
			int releaseId1 = new ReleaseManager().Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", "1.0.0.0", null, (int?)null, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1M, 0M, null);
			int componentId1 = new ComponentManager().Component_Insert(projectId, "Component 1");

			//Get the appropriate risk statuses, types, probabilities and impacts
			List<RiskStatus> statuses = riskManager.RiskStatus_Retrieve(projectTemplateId);
			List<RiskType> types = riskManager.RiskType_Retrieve(projectTemplateId);
			List<RiskImpact> impacts = riskManager.RiskImpact_Retrieve(projectTemplateId);
			List<RiskProbability> probabilities = riskManager.RiskProbability_Retrieve(projectTemplateId);

			//First lets create a couple of risks, one set to the defaults, one with values specified
			int riskId1 = riskManager.Risk_Insert(projectId, null, null, null, null, USER_ID_FRED_BLOGGS, null, "Sample Risk 1", "This is sample risk 1", null, null, DateTime.UtcNow, null, null);
			DateTime reviewDate = DateTime.UtcNow.AddMonths(1);
			int riskId2 = riskManager.Risk_Insert(
				projectId,
				statuses.FirstOrDefault(s => s.Name == "Analyzed").RiskStatusId,
				types.FirstOrDefault(s => s.Name == "Technical").RiskTypeId,
				probabilities.FirstOrDefault(s => s.Name == "Likely").RiskProbabilityId,
				impacts.FirstOrDefault(s => s.Name == "Marginal").RiskImpactId,
				USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Sample Risk 2", "This is sample risk 2", releaseId1, componentId1, DateTime.UtcNow, reviewDate, null);

			//Verify the inserts
			RiskView risk1 = riskManager.Risk_RetrieveById2(riskId1);
			RiskView risk2 = riskManager.Risk_RetrieveById2(riskId2);

			//Verify the data
			//Risk #1
			Assert.AreEqual("Sample Risk 1", risk1.Name);
			Assert.AreEqual("This is sample risk 1", risk1.Description);
			Assert.AreEqual("Identified", risk1.RiskStatusName);
			Assert.AreEqual("Business", risk1.RiskTypeName);
			Assert.IsNull(risk1.RiskImpactName);
			Assert.IsNull(risk1.RiskProbabilityName);
			Assert.IsNull(risk1.RiskExposure);
			Assert.IsTrue(risk1.RiskStatusIsOpen);
			Assert.IsNull(risk1.ComponentName);
			Assert.IsNull(risk1.ReleaseVersionNumber);
			Assert.IsNull(risk1.ReviewDate);
			Assert.IsNull(risk1.ClosedDate);

			//Risk #2
			Assert.AreEqual("Sample Risk 2", risk2.Name);
			Assert.AreEqual("This is sample risk 2", risk2.Description);
			Assert.AreEqual("Analyzed", risk2.RiskStatusName);
			Assert.AreEqual("Technical", risk2.RiskTypeName);
			Assert.AreEqual("Marginal", risk2.RiskImpactName);
			Assert.AreEqual("Likely", risk2.RiskProbabilityName);
			Assert.AreEqual(8, risk2.RiskExposure);
			Assert.IsTrue(risk2.RiskStatusIsOpen);
			Assert.AreEqual("Component 1", risk2.ComponentName);
			Assert.AreEqual("1.0.0.0", risk2.ReleaseVersionNumber);
			Assert.AreEqual(reviewDate.Date, risk2.ReviewDate.Value.Date);
			Assert.IsNull(risk2.ClosedDate);

			//Next we need to modify a risk
			Risk risk = riskManager.Risk_RetrieveById(riskId1);
			risk.StartTracking();
			risk.ReleaseId = releaseId1;
			risk.ComponentId = componentId1;
			risk.ReviewDate = reviewDate;
			risk.RiskStatusId = statuses.FirstOrDefault(s => s.Name == "Closed").RiskStatusId;
			risk.RiskTypeId = types.FirstOrDefault(s => s.Name == "Schedule").RiskTypeId;
			risk.RiskProbabilityId = probabilities.FirstOrDefault(s => s.Name == "Rare").RiskProbabilityId;
			risk.RiskImpactId = impacts.FirstOrDefault(s => s.Name == "Catastrophic").RiskImpactId;
			riskManager.Risk_Update(risk, USER_ID_FRED_BLOGGS);

			//Verify
			risk1 = riskManager.Risk_RetrieveById2(riskId1);
			Assert.AreEqual("Sample Risk 1", risk1.Name);
			Assert.AreEqual("This is sample risk 1", risk1.Description);
			Assert.AreEqual("Closed", risk1.RiskStatusName);
			Assert.AreEqual("Schedule", risk1.RiskTypeName);
			Assert.AreEqual("Catastrophic", risk1.RiskImpactName);
			Assert.AreEqual("Rare", risk1.RiskProbabilityName);
			Assert.AreEqual(5, risk1.RiskExposure);
			Assert.IsFalse(risk1.RiskStatusIsOpen);
			Assert.AreEqual("Component 1", risk1.ComponentName);
			Assert.AreEqual("1.0.0.0", risk1.ReleaseVersionNumber);
			Assert.AreEqual(reviewDate.Date, risk1.ReviewDate.Value.Date);
			Assert.IsNull(risk1.ClosedDate);

			//Verify that concurrency is handled correctly
			Risk risk_copy1 = riskManager.Risk_RetrieveById(riskId1);
			Risk risk_copy2 = riskManager.Risk_RetrieveById(riskId1);

			//Now try making a change using the out of date data (has the wrong ConcurrencyDate)
			risk_copy1.StartTracking();
			risk_copy1.Name = "Sample Risk 1a";
			riskManager.Risk_Update(risk_copy1, USER_ID_FRED_BLOGGS);

			bool exceptionThrown = false;
			try
			{
				risk_copy2.StartTracking();
				risk_copy2.Name = "Sample Risk 1b";
				riskManager.Risk_Update(risk_copy2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Copy the risk and verify
			int riskId3 = riskManager.Risk_Copy(USER_ID_FRED_BLOGGS, riskId2);

			//Verify
			RiskView risk3 = riskManager.Risk_RetrieveById2(riskId3);
			Assert.AreEqual("Sample Risk 2 - Copy", risk3.Name);
			Assert.AreEqual("This is sample risk 2", risk3.Description);
			Assert.AreEqual("Analyzed", risk3.RiskStatusName);
			Assert.AreEqual("Technical", risk3.RiskTypeName);
			Assert.AreEqual("Marginal", risk3.RiskImpactName);
			Assert.AreEqual("Likely", risk3.RiskProbabilityName);
			Assert.AreEqual(8, risk3.RiskExposure);
			Assert.IsTrue(risk3.RiskStatusIsOpen);
			Assert.AreEqual("Component 1", risk3.ComponentName);
			Assert.AreEqual("1.0.0.0", risk3.ReleaseVersionNumber);
			Assert.AreEqual(reviewDate.Date, risk3.ReviewDate.Value.Date);
			Assert.IsNull(risk3.ClosedDate);

			//Export the risk to another project and verify
			int riskId4 = riskManager.Risk_Export(riskId2, projectId2, USER_ID_FRED_BLOGGS);

			//Verify
			RiskView risk4 = riskManager.Risk_RetrieveById2(riskId4);
			Assert.AreEqual(projectId2, risk4.ProjectId);
			Assert.AreEqual("Sample Risk 2", risk4.Name);
			Assert.AreEqual("This is sample risk 2", risk4.Description);
			Assert.AreEqual("Analyzed", risk4.RiskStatusName);
			Assert.AreEqual("Technical", risk4.RiskTypeName);
			Assert.AreEqual("Marginal", risk4.RiskImpactName);
			Assert.AreEqual("Likely", risk4.RiskProbabilityName);
			Assert.AreEqual(8, risk4.RiskExposure);
			Assert.IsTrue(risk4.RiskStatusIsOpen);
			Assert.IsNull(risk4.ComponentName);
			Assert.IsNull(risk4.ReleaseVersionNumber);
			Assert.AreEqual(reviewDate.Date, risk4.ReviewDate.Value.Date);
			Assert.IsNull(risk4.ClosedDate);

			//Next we need to get a filtered list of risks
			Hashtable filters = new Hashtable();
			List<RiskView> risks;
			int count;

			//All open risks
			filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
			risks = riskManager.Risk_Retrieve(projectId, "RiskExposure", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			count = riskManager.Risk_Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, count);
			Assert.AreEqual(2, risks.Count);

			//All closed risks
			filters.Clear();
			filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllClosed);
			risks = riskManager.Risk_Retrieve(projectId, "RiskExposure", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			count = riskManager.Risk_Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, risks.Count);

			//Risks within a certain exposure range
			filters.Clear();
			IntRange intRange = new IntRange();
			intRange.MinValue = 7;
			intRange.MaxValue = 9;
			filters.Add("RiskExposure", intRange);
			risks = riskManager.Risk_Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			count = riskManager.Risk_Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, count);
			Assert.AreEqual(2, risks.Count);

			//Next we need to get a list of risks created by a user (cross-project and per-project)
			risks = riskManager.Risk_RetrieveOpenByCreatorId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(9, risks.Count);
			risks = riskManager.Risk_RetrieveOpenByCreatorId(USER_ID_FRED_BLOGGS, projectId);
			Assert.AreEqual(2, risks.Count);

			//Next we need to get a list of risks owned by a user (cross-project and per-project)
			risks = riskManager.Risk_RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, null, null);
			Assert.AreEqual(4, risks.Count);
			risks = riskManager.Risk_RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, projectId, null);
			Assert.AreEqual(0, risks.Count);
			risks = riskManager.Risk_RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(0, risks.Count);

			//Next we need to get a list of risks in a project group
			risks = riskManager.Risk_RetrieveOpenForGroup(PROJECT_GROUP_ID_DEFAULT, Int32.MaxValue);
			Assert.AreEqual(3, risks.Count);

			//Next we need to get a list of risks in the entire system (enterprise dashboard)
			risks = riskManager.Risk_RetrieveAllOpen(Int32.MaxValue);
			Assert.AreEqual(9, risks.Count);

			//Finally we need to delete one of the risks and verify it is soft deleted
			riskManager.Risk_MarkAsDeleted(projectId, riskId1, USER_ID_FRED_BLOGGS);
			risk1 = riskManager.Risk_RetrieveById2(riskId1);
			Assert.IsNull(risk1);
			risk1 = riskManager.Risk_RetrieveById2(riskId1, true);
			Assert.IsNotNull(risk1);

			//Get the list of deleted risks
			risks = riskManager.Risk_RetrieveDeleted(projectId);
			Assert.AreEqual(1, risks.Count);

			//Now purge and verify fully deleted
			riskManager.Risk_DeleteFromDatabase(riskId1, USER_ID_FRED_BLOGGS);
			risk1 = riskManager.Risk_RetrieveById2(riskId1);
			Assert.IsNull(risk1);
			risk1 = riskManager.Risk_RetrieveById2(riskId1, true);
			Assert.IsNull(risk1);
		}

		/// <summary>
		/// Tests that you can create, edit, delete, view risk mitigations
		/// </summary>
		[Test]
		[SpiraTestCase(1790)]
		public void _06_CreateEditViewDeleteRiskMitigations()
		{
			//Create a new risk
			int riskId1 = riskManager.Risk_Insert(projectId, null, null, null, null, USER_ID_FRED_BLOGGS, null, "Sample Risk 1", "This is sample risk 1", null, null, DateTime.UtcNow, null, null);

			//Add some mitigations
			int mitigationId1 = riskManager.RiskMitigation_Insert(projectId, riskId1, null, "Mitigation 1", USER_ID_FRED_BLOGGS);
			int mitigationId2 = riskManager.RiskMitigation_Insert(projectId, riskId1, null, "Mitigation 2", USER_ID_FRED_BLOGGS);
			int mitigationId3 = riskManager.RiskMitigation_Insert(projectId, riskId1, null, "Mitigation 3", USER_ID_FRED_BLOGGS);

			//Now add one in the middle
			int mitigationId2b = riskManager.RiskMitigation_Insert(projectId, riskId1, mitigationId3, "Mitigation 2b", USER_ID_FRED_BLOGGS);

			//View the mitigations
			Risk risk = riskManager.Risk_RetrieveById(riskId1, true);
			Assert.AreEqual(4, risk.Mitigations.Count);

			List<RiskMitigation> mitigations = riskManager.RiskMitigation_Retrieve(riskId1);
			Assert.AreEqual(4, mitigations.Count);
			Assert.AreEqual("Mitigation 1", mitigations[0].Description);
			Assert.IsFalse(mitigations[0].ReviewDate.HasValue);
			Assert.AreEqual("Mitigation 2", mitigations[1].Description);
			Assert.AreEqual("Mitigation 2b", mitigations[2].Description);
			Assert.AreEqual("Mitigation 3", mitigations[3].Description);

			//Modify a mitigation
			DateTime reviewDate = DateTime.UtcNow.AddDays(10);
			RiskMitigation mitigation = riskManager.RiskMitigation_RetrieveById(mitigationId1);
			mitigation.StartTracking();
			mitigation.Description = "Mitigation 1a";
			mitigation.ReviewDate = reviewDate;
			riskManager.RiskMitigation_Update(projectId, mitigation, USER_ID_FRED_BLOGGS);

			//Verify
			mitigation = riskManager.RiskMitigation_RetrieveById(mitigationId1);
			Assert.AreEqual("Mitigation 1a", mitigation.Description);
			Assert.AreEqual(reviewDate.Date, mitigation.ReviewDate.Value.Date);

			//Delete a mitigation
			riskManager.RiskMitigation_Delete(projectId, mitigationId2, USER_ID_FRED_BLOGGS);

			//Verify
			mitigations = riskManager.RiskMitigation_Retrieve(riskId1);
			Assert.AreEqual(3, mitigations.Count);
			Assert.AreEqual("Mitigation 1a", mitigations[0].Description);
			Assert.AreEqual("Mitigation 2b", mitigations[1].Description);
			Assert.AreEqual("Mitigation 3", mitigations[2].Description);

			//Delete the entire risk
			riskManager.Risk_DeleteFromDatabase(riskId1, USER_ID_FRED_BLOGGS);
			try
			{
				risk = riskManager.Risk_RetrieveById(riskId1, true);

			}
			catch (ArtifactNotExistsException)
			{
				risk = null;
			}
			Assert.IsNull(risk);
		}

	}
}
