using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;

/* Performance Timings
 * Start:           3/9/2020 23:34:53.908
 * Completed 01:    3/9/2020 23:34:54.18 > portfolios created in 1 sec
 * Completed 02:    3/9/2020 23:36:00.814 > products and programs created in 1 min
 *  - Releases:     3/9/2020 23:40:35.964 > releases/iterations created in 4 min
 * Completed 03:    3/10/2020 00:06:30.655 > requirements created in 25 min
 * End:             3/10/2020 00:06:31.025 
 * 
 * v1.0: Releases take between 12s and 44s (per project)
 * v1.1: Releases take between 16s and 28s (per project)
 * v1.2: Releases take between 12s and 39s (per project)
 */

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This tests the portfolio management functionality, including the association of programs into portfolios.
	/// It also tests that you can rollup requirememts correctly from sprints/releases all the way up to portfolios/enterprise views
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class PortfolioTest
	{
		protected static Business.PortfolioManager portfolioManager;
		protected static Business.ProjectGroupManager projectGroupManager;
		protected static Business.ProjectManager projectManager;
		protected static Business.ReleaseManager releaseManager;
		protected static Business.RequirementManager requirementManager;
		protected static Business.TemplateManager templateManager;

		private static List<ProjectReleaseInfo> projectReleaseRefs;

		protected static int portfolioId1 = 0;
		protected static int portfolioId2 = 0;
		protected static int portfolioId3 = 0;

		protected static int programId1 = 0;
		protected static int programId2 = 0;
		protected static int programId3 = 0;
		protected static int programId4 = 0;
		protected static int programId5 = 0;
		protected static int programId6 = 0;

		protected static int projectId1 = 0;
		protected static int projectId2 = 0;
		protected static int projectId3 = 0;
		protected static int projectId4 = 0;
		protected static int projectId5 = 0;
		protected static int projectId6 = 0;
		protected static int projectId7 = 0;
		protected static int projectId8 = 0;
		protected static int projectId9 = 0;
		protected static int projectId10 = 0;
		protected static int projectId11 = 0;
		protected static int projectId12 = 0;

		protected static int templateId1 = 0;
		protected static int templateId2 = 0;
		protected static int templateId3 = 0;
		protected static int templateId4 = 0;
		protected static int templateId5 = 0;
		protected static int templateId6 = 0;
		protected static int templateId7 = 0;
		protected static int templateId8 = 0;
		protected static int templateId9 = 0;
		protected static int templateId10 = 0;
		protected static int templateId11 = 0;
		protected static int templateId12 = 0;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		#region Setup and Teardown

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate business objects
			portfolioManager = new PortfolioManager();
			projectGroupManager = new ProjectGroupManager();
			projectManager = new ProjectManager();
			releaseManager = new ReleaseManager();
			requirementManager = new RequirementManager();
			templateManager = new TemplateManager();

			//Create tracking collection of releases/sprints in a product
			projectReleaseRefs = new List<ProjectReleaseInfo>();
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete any programs, portfolios and/or projects that might still be around

			//First products
			foreach (ProjectReleaseInfo projectReleaseInfo in projectReleaseRefs)
			{
				projectManager.Delete(USER_ID_SYS_ADMIN, projectReleaseInfo.ProjectId);
			}

			//Next, programs
			if (programId1 > 0)
			{
				projectGroupManager.Delete(programId1, 1);
			}
			if (programId2 > 0)
			{
				projectGroupManager.Delete(programId2, 1);
			}
			if (programId3 > 0)
			{
				projectGroupManager.Delete(programId3, 1);
			}
			if (programId4 > 0)
			{
				projectGroupManager.Delete(programId4, 1);
			}
			if (programId5 > 0)
			{
				projectGroupManager.Delete(programId5, 1);
			}
			if (programId6 > 0)
			{
				projectGroupManager.Delete(programId6, 1);
			}

			//Next, portfolios
			if (portfolioId1 > 0)
			{
				portfolioManager.Portfolio_Delete(portfolioId1, 1);
			}
			if (portfolioId2 > 0)
			{
				portfolioManager.Portfolio_Delete(portfolioId2, 1);
			}
			if (portfolioId3 > 0)
			{
				portfolioManager.Portfolio_Delete(portfolioId3, 1);
			}

			//Templates
			if (templateId1 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId1); }
			if (templateId2 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId2); }
			if (templateId3 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId3); }
			if (templateId4 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId4); }
			if (templateId5 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId5); }
			if (templateId6 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId6); }
			if (templateId7 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId7); }
			if (templateId8 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId8); }
			if (templateId9 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId9); }
			if (templateId10 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId10); }
			if (templateId11 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId11); }
			if (templateId12 > 0) { templateManager.Delete(USER_ID_SYS_ADMIN, templateId12); }
		}

		#endregion

		#region Tests

		/// <summary>
		/// Tests that we can create, modify and delete portfolios
		/// </summary>
		[
		Test,
		SpiraTestCase(2393)
		]
		public void _01_CreateAndManagePortfolios()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Portfolios";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Lets create some portfolios
			portfolioId1 = portfolioManager.Portfolio_Insert("Portfolio 1", "Portfolio 1 Description", true, 1, adminSectionId, "Inserted Portfolio");
			portfolioId2 = portfolioManager.Portfolio_Insert("Portfolio 2", "Portfolio 2 Description", true, 1, adminSectionId, "Inserted Portfolio");
			portfolioId3 = portfolioManager.Portfolio_Insert("Portfolio 3", null, true, 1, adminSectionId, "Inserted Portfolio");

			//Verify that they were created ok
			Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.IsNotNull(portfolio);
			Assert.AreEqual("Portfolio 1", portfolio.Name);
			Assert.AreEqual("Portfolio 1 Description", portfolio.Description);
			Assert.AreEqual(true, portfolio.IsActive);

			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.IsNotNull(portfolio);
			Assert.AreEqual("Portfolio 2", portfolio.Name);
			Assert.AreEqual("Portfolio 2 Description", portfolio.Description);
			Assert.AreEqual(true, portfolio.IsActive);

			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId3);
			Assert.IsNotNull(portfolio);
			Assert.AreEqual("Portfolio 3", portfolio.Name);
			Assert.IsNull(portfolio.Description);
			Assert.AreEqual(true, portfolio.IsActive);

			//Verify that we can retrieve a list of portfolios (5 sample data and 3 created)
			List<Portfolio> portfolios = portfolioManager.Portfolio_Retrieve(true);
			Assert.AreEqual(4, portfolios.Count);
			portfolios = portfolioManager.Portfolio_Retrieve(false);
			Assert.AreEqual(4, portfolios.Count);

			//Next modify one of the portfolios, make inactive
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId3);
			portfolio.StartTracking();
			portfolio.Name = "Portfolio 3a";
			portfolio.IsActive = false;
			portfolioManager.Portfolio_Update(portfolio, 1, adminSectionId, "Updated Portfolio");

			//Verify changes
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId3);
			Assert.IsNotNull(portfolio);
			Assert.AreEqual("Portfolio 3a", portfolio.Name);
			Assert.IsNull(portfolio.Description);
			Assert.AreEqual(false, portfolio.IsActive);

			//Verify that we can retrieve a list of portfolios
			portfolios = portfolioManager.Portfolio_Retrieve(true);
			Assert.AreEqual(3, portfolios.Count);
			portfolios = portfolioManager.Portfolio_Retrieve(false);
			Assert.AreEqual(4, portfolios.Count);

			//Finally delete a portfolio
			portfolioManager.Portfolio_Delete(portfolioId3, 1);
			portfolioId3 = 0;   //So clean up doesn't try and delete it again

			//Verify delete
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId3);
			Assert.IsNull(portfolio);
			portfolios = portfolioManager.Portfolio_Retrieve(true);
			Assert.AreEqual(3, portfolios.Count);
			portfolios = portfolioManager.Portfolio_Retrieve(false);
			Assert.AreEqual(3, portfolios.Count);
		}

		/// <summary>
		/// Tests that we can associate programs and products (technically programs contain the products).
		/// </summary>
		[
		Test,
		SpiraTestCase(2394)
		]
		public void _02_AssociateProgramsAndProducts()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Programs";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId1 = adminSection.ADMIN_SECTION_ID;
			//Lets create some programs under the two new portfolios
			programId1 = projectGroupManager.Insert("Program 1.1", null, null, true, false, null, 1, portfolioId1, adminSectionId1, "Inserted Program");
			programId2 = projectGroupManager.Insert("Program 1.2", null, null, true, false, null, 1, portfolioId1, adminSectionId1, "Inserted Program");
			programId3 = projectGroupManager.Insert("Program 2.3", null, null, true, false, null, 1, portfolioId2, adminSectionId1, "Inserted Program");
			programId4 = projectGroupManager.Insert("Program 2.4", null, null, true, false, null, 1, portfolioId2, adminSectionId1, "Inserted Program");

			//Lets create some programs that are not in any portfolio (i.e. (No Portfolio))
			programId5 = projectGroupManager.Insert("Program 0.5", null, null, true, false, null, 1, portfolioId2, adminSectionId1, "Inserted Program");
			programId6 = projectGroupManager.Insert("Program 0.6", null, null, true, false, null, 1, portfolioId2, adminSectionId1, "Inserted Program");

			//Verify they were added correctly
			ProjectGroup program = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(portfolioId1, program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(portfolioId1, program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId3);
			Assert.AreEqual(portfolioId2, program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId4);
			Assert.AreEqual(portfolioId2, program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId5);
			//Assert.IsNull(program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId6);
			//Assert.IsNull(program.PortfolioId);

			//Next lets move one of the other programs into the first portfolio
			program = projectGroupManager.RetrieveById(programId5);
			program.StartTracking();
			program.Name = "1.5";
			program.PortfolioId = portfolioId1;
			projectGroupManager.Update(program, 1, adminSectionId1, "Updated Program");

			//Verify
			program = projectGroupManager.RetrieveById(programId5);
            Assert.AreEqual(portfolioId1, program.PortfolioId);

			string adminSectionName1 = "View / Edit Projects";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId = adminSection1.ADMIN_SECTION_ID;

			//Next we create some products under each program in the portfolio
			projectId1 = projectManager.Insert("Project 1.1.1", programId1, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId2 = projectManager.Insert("Project 1.1.2", programId1, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId3 = projectManager.Insert("Project 1.2.3", programId2, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId4 = projectManager.Insert("Project 1.2.4", programId2, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId5 = projectManager.Insert("Project 2.3.5", programId3, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId6 = projectManager.Insert("Project 2.3.6", programId3, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId7 = projectManager.Insert("Project 2.4.7", programId4, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId8 = projectManager.Insert("Project 2.4.8", programId4, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId9 = projectManager.Insert("Project 1.5.9", programId5, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId10 = projectManager.Insert("Project 1.5.10", programId5, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId11 = projectManager.Insert("Project 0.6.11", programId6, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId12 = projectManager.Insert("Project 0.6.12", programId6, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Need to capture the templates
			templateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
			templateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;
			templateId3 = templateManager.RetrieveForProject(projectId3).ProjectTemplateId;
			templateId4 = templateManager.RetrieveForProject(projectId4).ProjectTemplateId;
			templateId5 = templateManager.RetrieveForProject(projectId5).ProjectTemplateId;
			templateId6 = templateManager.RetrieveForProject(projectId6).ProjectTemplateId;
			templateId7 = templateManager.RetrieveForProject(projectId7).ProjectTemplateId;
			templateId8 = templateManager.RetrieveForProject(projectId8).ProjectTemplateId;
			templateId9 = templateManager.RetrieveForProject(projectId9).ProjectTemplateId;
			templateId10 = templateManager.RetrieveForProject(projectId10).ProjectTemplateId;
			templateId11 = templateManager.RetrieveForProject(projectId11).ProjectTemplateId;
			templateId12 = templateManager.RetrieveForProject(projectId12).ProjectTemplateId;

			//This is already tested in the project and project group unit tests, so we can just assume they worked
			//if no errors were reported

			//Verify that we can retrieve the programs by portfolio and the default (no) portfolio
			List<ProjectGroup> programs;
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId1);
			Assert.AreEqual(3, programs.Count);
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId2);
			Assert.AreEqual(3, programs.Count);
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(null);
			Assert.AreEqual(2, programs.Count);

			//Now without the active flag
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId1, false);
			Assert.AreEqual(3, programs.Count);
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId2, false);
			Assert.AreEqual(3, programs.Count);
			programs = projectGroupManager.ProjectGroup_RetrieveByPortfolio(null, false);
			Assert.AreEqual(2, programs.Count);
		}

		/// <summary>
		/// Tests that we can add releases and sprints to the products
		/// </summary>
		[
		Test,
		SpiraTestCase(2395)
		]
		public void _03_AddReleasesAndSprints()
		{
			//First create the releases and sprints (using a helper function)
			CreateReleasesSprints(projectId1);
			CreateReleasesSprints(projectId2);
			CreateReleasesSprints(projectId3);
			CreateReleasesSprints(projectId4);
			CreateReleasesSprints(projectId5);
			CreateReleasesSprints(projectId6);
			CreateReleasesSprints(projectId7);
			CreateReleasesSprints(projectId8);
			CreateReleasesSprints(projectId9);
			CreateReleasesSprints(projectId10);
			CreateReleasesSprints(projectId11);
			CreateReleasesSprints(projectId12);
		}

		/// <summary>
		/// Tests that we can add requirements to the releases/sprints in the products,
		/// and that the counts roll-up correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(2396)
		]
		public void _04_AddRequirementsToProducts()
		{
			//Next lets loop through and add our standard set of requirements to each release and sprint
			foreach (ProjectReleaseInfo projectReleaseInfo in projectReleaseRefs)
			{
				int projectId = projectReleaseInfo.ProjectId;
				foreach (int releaseId in projectReleaseInfo.ReleaseIds)
				{
					CreateRequirementsForReleaseOrSprint(projectId, releaseId);
				}
				foreach (int iterationId in projectReleaseInfo.IterationIds)
				{
					CreateRequirementsForReleaseOrSprint(projectId, iterationId);
				}

				//Now verify that they were created
				int count = requirementManager.Count(User.UserInternal, projectId, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(64, count);

				//Now verify that the product totals are correct
				Project project = projectManager.RetrieveById(projectId);
				Assert.AreEqual(64, project.RequirementCount);
				Assert.AreEqual(480M, project.ReqPointEffort);
				Assert.AreEqual(0, project.PercentComplete);
				//Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, project.StartDate.Value.Date);
				//Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, project.EndDate.Value.Date);
			}

			//Now verify that the program totals are correct
			ProjectGroup projectGroup;
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			projectGroup = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			projectGroup = projectGroupManager.RetrieveById(programId3);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			projectGroup = projectGroupManager.RetrieveById(programId4);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			projectGroup = projectGroupManager.RetrieveById(programId5);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			projectGroup = projectGroupManager.RetrieveById(programId6);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			Portfolio portfolio;
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);
		}

		/// <summary>
		/// Tests that we can modify the requirements and releases in a product, and the changes roll up correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(2405)
		]
		public void _05_ModifyReleasesInProduct()
		{
			#region First lets change the status of a requirement

			//Modify the status of the first two sets of child items to be completed
			List<RequirementView> requirementResults = requirementManager.Retrieve(USER_ID_SYS_ADMIN, projectId1, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			List<Requirement> requirements = new List<Requirement>();
			foreach (RequirementView requirementsResult in requirementResults.Where(r => (r.IndentLevel.SafeSubstring(0, 3) == "AAA" || r.IndentLevel.SafeSubstring(0, 3) == "AAB") && r.IndentLevel.Length > 3))
			{
				Requirement requirement = requirementsResult.ConvertTo<RequirementView, Requirement>();
				requirement.StartTracking();
				requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Completed;
				requirements.Add(requirement);
			}

			requirementManager.Update(USER_ID_SYS_ADMIN, projectId1, requirements);
			//Now verify that the product totals are correct
			Project project;
			project = projectManager.RetrieveById(projectId1);
			Assert.AreEqual(64, project.RequirementCount);
			Assert.AreEqual(480M, project.ReqPointEffort);
			Assert.AreEqual(12, project.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, project.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, project.EndDate.Value.Date);

			//Now verify that the program totals are correct
			ProjectGroup projectGroup;
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(6, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			Portfolio portfolio;
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(2, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			#endregion

			#region Next, lets change the dates of a release

			//Modify the end date of the second major release and its sprints out by 30 days
			{
				Hashtable filters = new Hashtable();
				filters.Add("VersionNumber", "2.0.0.0");
				List<ReleaseView> releaseResults = releaseManager.RetrieveByProjectId(USER_ID_SYS_ADMIN, projectId1, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
				List<Release> releases = new List<Release>();
				foreach (ReleaseView releaseView in releaseResults)
				{
					Release release = releaseView.ConvertTo<ReleaseView, Release>();
					releases.Add(release);
					release.StartTracking();
					release.EndDate = release.EndDate.AddDays(30);
				}
				releaseManager.Update(releases, USER_ID_SYS_ADMIN, projectId1);
			}

			//Now verify that the product totals are correct
			project = projectManager.RetrieveById(projectId1);
			Assert.AreEqual(64, project.RequirementCount);
			Assert.AreEqual(480M, project.ReqPointEffort);
			Assert.AreEqual(12, project.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, project.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, project.EndDate.Value.Date);

			//Now verify that the program totals are correct
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(6, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(2, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);

			#endregion

			#region Finally, lets make a release inactive (completed)

			//Modify the status of the first minor release and its sprints to be closed
			{
				Hashtable filters = new Hashtable();
				filters.Add("VersionNumber", "1.1.0.0");
				List<ReleaseView> releaseResults = releaseManager.RetrieveByProjectId(USER_ID_SYS_ADMIN, projectId1, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
				List<Release> releases = new List<Release>();
				foreach (ReleaseView releaseView in releaseResults)
				{
					Release release = releaseView.ConvertTo<ReleaseView, Release>();
					releases.Add(release);
					release.StartTracking();
					release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Closed;
				}
				releaseManager.Update(releases, USER_ID_SYS_ADMIN, projectId1);
			}

			//Now verify that the product totals are correct
			project = projectManager.RetrieveById(projectId1);
			Assert.AreEqual(48, project.RequirementCount);
			Assert.AreEqual(480M, project.ReqPointEffort);
			Assert.AreEqual(8, project.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, project.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, project.EndDate.Value.Date);

			//Now verify that the program totals are correct
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64 + 48, projectGroup.RequirementCount);
			Assert.AreEqual(3, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);

			#endregion
		}

		/// <summary>
		/// Tests that we can modify the products in a program, and the changes roll up correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(2406)
		]
		public void _06_ModifyProductsInProgram()
		{
			//First lets get the current roll-up data

			//Now verify that the product totals are correct
			Project project;
			project = projectManager.RetrieveById(projectId1);
			Assert.AreEqual(48, project.RequirementCount);
			Assert.AreEqual(480M, project.ReqPointEffort);
			Assert.AreEqual(8, project.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, project.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, project.EndDate.Value.Date);

			//Now verify that the program totals are correct
			ProjectGroup projectGroup;
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64 + 48, projectGroup.RequirementCount);
			Assert.AreEqual(3, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, projectGroup.EndDate.Value.Date);
			projectGroup = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			Portfolio portfolio;
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Next we need to change the program that the project is in
			project = projectManager.RetrieveById(projectId1);
			project.StartTracking();
			project.ProjectGroupId = programId2;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now verify that the program totals are correct
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);
			projectGroup = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(64 * 2 + 48, projectGroup.RequirementCount);
			Assert.AreEqual(2, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//Next we need to deactivate the project
			project = projectManager.RetrieveById(projectId1);
			project.StartTracking();
			project.IsActive = false;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now verify that the program totals are correct
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);
			projectGroup = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(64 * 2, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//We reactivate it
			project = projectManager.RetrieveById(projectId1);
			project.StartTracking();
			project.IsActive = true;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now verify that the program totals are correct
			projectGroup = projectGroupManager.RetrieveById(programId1);
			Assert.AreEqual(64, projectGroup.RequirementCount);
			Assert.AreEqual(0, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, projectGroup.EndDate.Value.Date);
			projectGroup = projectGroupManager.RetrieveById(programId2);
			Assert.AreEqual(64 * 2 + 48, projectGroup.RequirementCount);
			Assert.AreEqual(2, projectGroup.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, projectGroup.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, projectGroup.EndDate.Value.Date);

			//Now verify that the portfolio totals are correct
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);
		}

		/// <summary>
		/// Tests that we can modify the programs in a portfolio, and the changes roll up correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(2407)
		]
		public void _07_ModifyProgramsInPortfolio()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Programs";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//First verify that the portfolio totals are correct
			Portfolio portfolio;
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//Now move a program from one portfolio to another
			ProjectGroup program;
			program = projectGroupManager.RetrieveById(programId1);
			program.StartTracking();
			program.PortfolioId = portfolioId2;
			projectGroupManager.Update(program, 1, adminSectionId, "Updated Program");

			//Verify
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 4 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(448, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//Move it back
			program = projectGroupManager.RetrieveById(programId1);
			program.StartTracking();
			program.PortfolioId = portfolioId1;
			projectGroupManager.Update(program, 1, adminSectionId, "Updated Program");

			//Verify
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//Now make a program inactive
			program = projectGroupManager.RetrieveById(programId1);
			program.StartTracking();
			program.IsActive = false;
			projectGroupManager.Update(program, 1, adminSectionId, "Updated Program");

			//Verify
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 4 + 48, portfolio.RequirementCount);
			Assert.AreEqual(1, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);

			//Now make the program active again
			program = projectGroupManager.RetrieveById(programId1);
			program.StartTracking();
			program.IsActive = true;
			projectGroupManager.Update(program, 1, adminSectionId, "Updated Program");

			//Verify
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.AreEqual(64 * 5 + 48, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180 + 30).Date, portfolio.EndDate.Value.Date);
			portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			Assert.AreEqual(64 * 6, portfolio.RequirementCount);
			Assert.AreEqual(0, portfolio.PercentComplete);
			Assert.AreEqual(DateTime.UtcNow.AddDays(-30).Date, portfolio.StartDate.Value.Date);
			Assert.AreEqual(DateTime.UtcNow.AddDays(180).Date, portfolio.EndDate.Value.Date);
		}

		/// <summary>
		/// Tests that we can modify the porfolios in the system, and the changes roll up correctly to the enterprise view
		/// </summary>
		[
		Test,
		SpiraTestCase(2408)
		]
		public void _08_ModifyPortfoliosInEnterprise()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Portfolios";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Get a list of the active portfolios
			List<Portfolio> portfolios = portfolioManager.Portfolio_Retrieve(true);

			//Make sure our portfolios are included
			Assert.IsTrue(portfolios.Any(p => p.PortfolioId == portfolioId1));
			Assert.IsTrue(portfolios.Any(p => p.PortfolioId == portfolioId2));

			//Make one of the portfolios inactive
			Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId2);
			portfolio.StartTracking();
			portfolio.IsActive = false;
			portfolioManager.Portfolio_Update(portfolio, 1, adminSectionId, "Updated Portfolio");

			//Get the new list of active portfolios
			portfolios = portfolioManager.Portfolio_Retrieve(true);

			//Make sure our active portfolios are only included
			Assert.IsTrue(portfolios.Any(p => p.PortfolioId == portfolioId1));
			Assert.IsFalse(portfolios.Any(p => p.PortfolioId == portfolioId2));
		}

		/// <summary>
		/// Tests that we can manage portfolio settings using the special PortfolioSettings configuration provider
		/// </summary>
		[
		Test,
		SpiraTestCase(2736)
		]
		public void _09_StoreRetrievePortfolioSettings()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Portfolios";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Setup - create two new portfolios
			int portfolioForSettings1 = portfolioManager.Portfolio_Insert("Settings Portfolio 1", null, true, 1, adminSectionId, "Inserted Portfolio");
			int portfolioForSettings2 = portfolioManager.Portfolio_Insert("Settings Portfolio 2", null, true, 1, adminSectionId, "Inserted Portfolio");

			//First, get the default settings values for a project
			PortfolioSettings portfolioSettings = new PortfolioSettings(portfolioForSettings1);
			Assert.IsNotNull(portfolioSettings);
			Assert.IsNullOrEmpty(portfolioSettings.TestSetting);

			//Next test that we can store some settings
			portfolioSettings.TestSetting = "Test123";
			portfolioSettings.Save();

			//Verify
			portfolioSettings = new PortfolioSettings(portfolioForSettings1);
			Assert.IsNotNull(portfolioSettings);
			Assert.AreEqual("Test123", portfolioSettings.TestSetting);

			//Next test that the settings are truly project-specific
			portfolioSettings = new PortfolioSettings(portfolioForSettings2);
			Assert.IsNotNull(portfolioSettings);
			Assert.IsNullOrEmpty(portfolioSettings.TestSetting);

			//Finally check that you cannot use the 'default' static instance of the settings class
			bool exceptionCaught = false;
			try
			{
				portfolioSettings = PortfolioSettings.Default;
				portfolioSettings.TestSetting = "XYZ";
				portfolioSettings.Save();
			}
			catch (InvalidOperationException)
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught);

			//Delete the portfolios
			portfolioManager.Portfolio_Delete(portfolioForSettings1, 1);
			portfolioManager.Portfolio_Delete(portfolioForSettings2, 1);
		}

		/// <summary>
		/// Tests that we can delete a portfolio that has programs in it (it should null out the portfolio)
		/// </summary>
		[
		Test,
		SpiraTestCase(2397)
		]
		public void _XX_DeletePortfolioWithPrograms()
		{
			//Delete one of the portfolios with at least one program in it
			portfolioManager.Portfolio_Delete(portfolioId1, 1);
			portfolioId1 = 0;   //So clean up doesn't try and delete it again

			//Verify delete
			Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId1);
			Assert.IsNull(portfolio);

			//Verify that the programs are still there and has the ID nulled out
			ProjectGroup program = projectGroupManager.RetrieveById(programId1);
			Assert.IsNotNull(program);
			Assert.IsNull(program.PortfolioId);
			program = projectGroupManager.RetrieveById(programId2);
			Assert.IsNotNull(program);
			Assert.IsNull(program.PortfolioId);
		}

		#endregion

		#region Helper Functions

		/// <summary>
		/// Creates the release/sprint data for a specific project
		/// </summary>
		/// <param name="projectId"></param>
		private void CreateReleasesSprints(int projectId)
		{
			DateTime startTime = DateTime.UtcNow;

			//We create two major releases
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(90), 10, 0, null);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow.AddDays(90), DateTime.UtcNow.AddDays(180), 10, 0, null);

			//Create minor releases under one
			int releaseId11 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", null, "1.1.0.0", releaseId1, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.MinorRelease, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30), 10, 0, null);
			int releaseId12 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", null, "1.2.0.0", releaseId1, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.MinorRelease, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(90), 10, 0, null);

			//Create sprints under all
			int iterationId1_01 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 01", null, "1.0.0.0 01", releaseId1, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-20), 10, 0, null);
			int iterationId1_02 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 02", null, "1.0.0.0 02", releaseId1, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(-10), 10, 0, null);
			int iterationId1_03 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 03", null, "1.0.0.0 03", releaseId1, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(0), 10, 0, null);

			int iterationId11_01 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.1 01", null, "1.1.0.0 01", releaseId11, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(0), DateTime.UtcNow.AddDays(10), 10, 0, null);
			int iterationId11_02 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.1 02", null, "1.1.0.0 02", releaseId11, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(20), 10, 0, null);
			int iterationId11_03 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.1 03", null, "1.1.0.0 03", releaseId11, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(20), DateTime.UtcNow.AddDays(30), 10, 0, null);

			int iterationId12_01 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.2 01", null, "1.2.0.0 01", releaseId12, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(40), 10, 0, null);
			int iterationId12_02 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.2 02", null, "1.2.0.0 02", releaseId12, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(40), DateTime.UtcNow.AddDays(50), 10, 0, null);
			int iterationId12_03 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.2 03", null, "1.2.0.0 03", releaseId12, Release.ReleaseStatusEnum.InProgress, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(50), DateTime.UtcNow.AddDays(60), 10, 0, null);

			int iterationId2_01 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2.0 01", null, "2.0.0.0 01", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(90), DateTime.UtcNow.AddDays(110), 10, 0, null);
			int iterationId2_02 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2.0 02", null, "2.0.0.0 02", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(110), DateTime.UtcNow.AddDays(130), 10, 0, null);
			int iterationId2_03 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2.0 03", null, "2.0.0.0 03", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddDays(130), DateTime.UtcNow.AddDays(150), 10, 0, null);

			//Collapse so that future inserts are at the correct level
			releaseManager.Collapse(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			releaseManager.Collapse(USER_ID_FRED_BLOGGS, projectId, releaseId2);

			//Add them to the mapping collection for this project
			ProjectReleaseInfo projectReleaseInfo = new ProjectReleaseInfo();
			projectReleaseInfo.ProjectId = projectId;
			projectReleaseInfo.ReleaseIds = new List<int>();
			projectReleaseInfo.ReleaseIds.Add(releaseId1);
			projectReleaseInfo.ReleaseIds.Add(releaseId11);
			projectReleaseInfo.ReleaseIds.Add(releaseId12);
			projectReleaseInfo.ReleaseIds.Add(releaseId2);
			projectReleaseInfo.IterationIds = new List<int>();
			projectReleaseInfo.IterationIds.Add(iterationId1_01);
			projectReleaseInfo.IterationIds.Add(iterationId1_02);
			projectReleaseInfo.IterationIds.Add(iterationId1_03);
			projectReleaseInfo.IterationIds.Add(iterationId11_01);
			projectReleaseInfo.IterationIds.Add(iterationId11_02);
			projectReleaseInfo.IterationIds.Add(iterationId11_03);
			projectReleaseInfo.IterationIds.Add(iterationId12_01);
			projectReleaseInfo.IterationIds.Add(iterationId12_02);
			projectReleaseInfo.IterationIds.Add(iterationId12_03);
			projectReleaseInfo.IterationIds.Add(iterationId2_01);
			projectReleaseInfo.IterationIds.Add(iterationId2_02);
			projectReleaseInfo.IterationIds.Add(iterationId2_03);
			projectReleaseRefs.Add(projectReleaseInfo);

			DateTime endTime = DateTime.UtcNow;
			TimeSpan timeSpan = endTime.Subtract(startTime);
			Logger.LogTraceEvent("CreateReleaseSprints::Duration", "Project PR" + projectId + ", duration=" + timeSpan.TotalSeconds + "s");
		}

		/// <summary>
		/// Creates a set of requirements in the specified release/sprint
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release or sprint</param>
		protected void CreateRequirementsForReleaseOrSprint(int projectId, int releaseId)
		{
			DateTime startTime = DateTime.UtcNow;

			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId, null, (int?)null, Requirement.RequirementStatusEnum.InProgress, null, USER_ID_FRED_BLOGGS, null, null, "Requirement " + releaseId + "-1", null, null, USER_ID_SYS_ADMIN, true);
			int requirementId1_1 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, releaseId, null, requirementId1, Requirement.RequirementStatusEnum.InProgress, null, USER_ID_FRED_BLOGGS, null, null, "Requirement " + releaseId + "-1.1", null, 1.0M, USER_ID_SYS_ADMIN);
			int requirementId1_2 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, releaseId, null, requirementId1, Requirement.RequirementStatusEnum.InProgress, null, USER_ID_FRED_BLOGGS, null, null, "Requirement " + releaseId + "-1.2", null, 2.0M, USER_ID_SYS_ADMIN);
			int requirementId1_3 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, releaseId, null, requirementId1, Requirement.RequirementStatusEnum.InProgress, null, USER_ID_FRED_BLOGGS, null, null, "Requirement " + releaseId + "-1.3", null, 4.0M, USER_ID_SYS_ADMIN);

			DateTime endTime = DateTime.UtcNow;
			TimeSpan timeSpan = endTime.Subtract(startTime);
			Logger.LogTraceEvent("CreateRequirementsForReleaseOrSprint::Duration", "Project PR" + projectId + ", Release RL" + releaseId + ", duration=" + timeSpan.TotalSeconds + "s");
		}

		#endregion

		#region Helper Classes

		private class ProjectReleaseInfo
		{
			public int ProjectId { get; set; }
			public List<int> ReleaseIds { get; set; }
			public List<int> IterationIds { get; set; }
		}

		#endregion
	}
}
