using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using System.Xml;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Report business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ReportTest
	{
		private const int PROJECT_ID = 1;
		private const int PROJECT_TEMPLATE_ID = 1;
		private const int PROJECT_ID_EMPTY = 2;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int REQUIREMENTS_REPORT_ID = 2;
		private const string REQUIREMENTS_REPORT_TOKEN = "RequirementDetailed";
		private const int REQUIREMENTS_SUMMARY_REPORT_ID = 1;
		private const int HTML_FORMAT_ID = 1;
		private const int RELEASE_ID = 1;   //1.0.0.0


		private static int reportSavedId1;
		private static int reportSavedId2;
		private static int reportSavedId3;
		private static int reportSavedId4;

		private static int customReportId1;
		private static int customReportId2;

		private static ReportManager reportManager;

		/// <summary>
		/// Initializes the business objects being tested
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			//First we need to instantiate the report business object that we'll be using
			reportManager = new Business.ReportManager();
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete any saved reports
			reportManager.DeleteSaved(reportSavedId1);
			reportManager.DeleteSaved(reportSavedId3);
			reportManager.DeleteSaved(reportSavedId4);

			//Delete any custom reports
			if (customReportId1 > 0)
			{
				reportManager.Report_Delete(customReportId1, 1);
			}
			if (customReportId2 > 0)
			{
				reportManager.Report_Delete(customReportId2, 1);
			}
		}

		/// <summary>
		/// Test that we can get the list of reports, elements and sections 
		/// </summary>
		[
		Test,
		SpiraTestCase(549)
		]
		public void _01_DisplayElementsSections()
		{
			//First test that we can get the list of categories and reports in each category
			List<ReportCategory> reportCategories = reportManager.RetrieveCategories();
			Assert.AreEqual(19, reportCategories.Count);
			Assert.AreEqual("Cleaning Validation Reports", reportCategories[0].Name);
			Assert.AreEqual(100, reportCategories[0].Position);
			Assert.AreEqual("Assessment Reports", reportCategories[4].Name);
			Assert.AreEqual(600, reportCategories[4].Position);

			List<Report> reports = reportManager.RetrieveByCategoryId(1);
			Assert.AreEqual(4, reports.Count);
			Assert.AreEqual("Requirements Summary", reports[2].Name);
			Assert.AreEqual("RequirementSummary", reports[2].Token);
			Assert.AreEqual("Requirements Plan", reports[1].Name);
			Assert.AreEqual("RequirementPlan", reports[1].Token);

			//Next test that we can get the details of a specific report
			Report report = reportManager.RetrieveById(REQUIREMENTS_REPORT_ID);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, report.ReportId);
			Assert.AreEqual(1, report.ReportCategoryId);
			Assert.AreEqual("RequirementDetailed", report.Token);
			Assert.AreEqual("Requirements Detailed", report.Name);
			Assert.AreEqual(true, report.IsActive);

			//Next test we can get the same report by its unique token
			Report reportByToken = reportManager.RetrieveByToken(REQUIREMENTS_REPORT_TOKEN);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, reportByToken.ReportId);
			Assert.AreEqual(REQUIREMENTS_REPORT_TOKEN, reportByToken.Token);
			Assert.AreEqual(1, reportByToken.ReportCategoryId);
			Assert.AreEqual("Requirements Detailed", reportByToken.Name);
			Assert.AreEqual(true, reportByToken.IsActive);

			//Now test that we can get the list of sections
			Assert.AreEqual(2, report.SectionInstances.Count);
			ReportSection reportSection = report.SectionInstances[0].Section;
			Assert.AreEqual(1, reportSection.ReportSectionId);
			Assert.AreEqual("ProjectOverview", reportSection.Token);
			Assert.IsNull(reportSection.ArtifactTypeId);
			reportSection = report.SectionInstances[1].Section;
			Assert.AreEqual(3, reportSection.ReportSectionId);
			Assert.AreEqual("RequirementDetails", reportSection.Token);
			Assert.AreEqual(1, reportSection.ArtifactTypeId);

			//Now test that we can get the list of elements for the various sections (need to explicitly sort)
			Assert.AreEqual(8, reportSection.Elements.Count);
			List<ReportElement> sortedElements = reportSection.Elements.OrderBy(re => re.Name).ToList();
			ReportElement reportElement = sortedElements[0];
			Assert.AreEqual(3, reportElement.ReportSectionId);
			Assert.AreEqual("History", reportElement.Token);
			Assert.AreEqual("Artifact Change History", reportElement.Name);
			Assert.AreEqual(true, reportElement.IsActive);
			reportElement = sortedElements[1];
			Assert.AreEqual(3, reportElement.ReportSectionId);
			Assert.AreEqual("Tasks", reportElement.Token);
			Assert.AreEqual("Associated Tasks", reportElement.Name);
			Assert.AreEqual(true, reportElement.IsActive);

			//Verify that we can get a section by its ID
			reportSection = reportManager.ReportSection_RetrieveById(1);
			Assert.AreEqual(1, reportSection.ReportSectionId);
			Assert.AreEqual("ProjectOverview", reportSection.Token);
			Assert.IsNull(reportSection.ArtifactTypeId);
		}

		/// <summary>
		/// Test that we can get the list of available formats for a report
		/// </summary>
		[
		Test,
		SpiraTestCase(554)
		]
		public void _02_DisplayFormats()
		{
			//Next test that we can get the list of formats for a specific report
			Report report = reportManager.RetrieveById(REQUIREMENTS_SUMMARY_REPORT_ID);
			List<ReportFormat> sortedFormats = report.Formats.OrderBy(f => f.Name).ToList();
			Assert.AreEqual(7, sortedFormats.Count);
			ReportFormat reportFormat = sortedFormats[5];
			Assert.AreEqual(8, reportFormat.ReportFormatId);
			Assert.AreEqual("Pdf", reportFormat.Token);
			Assert.AreEqual("PDF", reportFormat.Name);
			Assert.AreEqual("Acrobat.svg", reportFormat.IconFilename);
			Assert.AreEqual("application/pdf", reportFormat.ContentType);
			Assert.AreEqual("xsl-fo", reportFormat.ContentDisposition);
			Assert.AreEqual(true, reportFormat.IsActive);
			reportFormat = sortedFormats[0];
			Assert.AreEqual(1, reportFormat.ReportFormatId);
			Assert.AreEqual("Html", reportFormat.Token);
			Assert.AreEqual("HTML", reportFormat.Name);
			Assert.AreEqual("HTML.svg", reportFormat.IconFilename);
			Assert.AreEqual("text/html", reportFormat.ContentType);
			Assert.IsNull(reportFormat.ContentDisposition);
			Assert.AreEqual(true, reportFormat.IsActive);
			reportFormat = sortedFormats[4];
			Assert.AreEqual(2, reportFormat.ReportFormatId);
			Assert.AreEqual("MsWord2003", reportFormat.Token);
			Assert.AreEqual("MS-Word (legacy)", reportFormat.Name);
			Assert.AreEqual("Word-Xml.svg", reportFormat.IconFilename);
			Assert.AreEqual("application/msword", reportFormat.ContentType);
			Assert.AreEqual("attachment; filename=Report.doc", reportFormat.ContentDisposition);
			Assert.AreEqual(true, reportFormat.IsActive);

			//Now test that we can get a specific format record by its id
			reportFormat = reportManager.RetrieveFormatById(2);
			Assert.AreEqual(2, reportFormat.ReportFormatId);
			Assert.AreEqual("MsWord2003", reportFormat.Token);
			Assert.AreEqual("MS-Word (legacy)", reportFormat.Name);
			Assert.AreEqual("Word-Xml.svg", reportFormat.IconFilename);
			Assert.AreEqual("application/msword", reportFormat.ContentType);
			Assert.AreEqual("attachment; filename=Report.doc", reportFormat.ContentDisposition);
			Assert.AreEqual(true, reportFormat.IsActive);
		}

		/// <summary>
		/// Test that we can get the list of sorts for each report that supports it
		/// </summary>
		[
		Test,
		SpiraTestCase(550)
		]
		public void _03_DisplaySortOptions()
		{
			//Need to get the list of artifact fields ordered by caption
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveForReporting(Artifact.ArtifactTypeEnum.Incident);
			List<ArtifactField> sortedFields = artifactFields.OrderBy(a => a.Caption).ToList();

			//Verify some of the data returned
			Assert.AreEqual(23, sortedFields.Count);
			//Row 0
			ArtifactField artifactField = sortedFields[0];
			Assert.AreEqual("CompletionPercent", artifactField.Name);
			Assert.AreEqual("% Complete", artifactField.Caption);
			//Row 5
			artifactField = sortedFields[5];
			Assert.AreEqual("CreationDate", artifactField.Name);
			Assert.AreEqual("Detected On", artifactField.Caption);
			//Row 7
			artifactField = sortedFields[7];
			Assert.AreEqual("EstimatedEffort", artifactField.Name);
			Assert.AreEqual("Est. Effort", artifactField.Caption);
			//Row 10
			artifactField = sortedFields[10];
			Assert.AreEqual("LastUpdateDate", artifactField.Name);
			Assert.AreEqual("Last Modified", artifactField.Caption);
			//Row 22
			artifactField = sortedFields[22];
			Assert.AreEqual("VerifiedReleaseId", artifactField.Name);
			Assert.AreEqual("Verified Release", artifactField.Caption);
		}

		/// <summary>
		/// Test that we can get the list of filters for a report that supports sorting
		/// </summary>
		[
		Test,
		SpiraTestCase(551)
		]
		public void _04_DisplayFilters()
		{
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveForReporting(DataModel.Artifact.ArtifactTypeEnum.Requirement);
			List<ArtifactField> sortedFields = artifactFields.OrderBy(a => a.Caption).ToList();
			Assert.AreEqual(19, sortedFields.Count);

			//Verify some of the data returned
			//Row 1
			ArtifactField artifactField = sortedFields[1];
			Assert.AreEqual("Author", artifactField.Caption);
			Assert.AreEqual("AuthorId", artifactField.Name);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.Lookup, (int)artifactField.ArtifactFieldTypeId);
			//Row 3
			artifactField = sortedFields[2];
			Assert.AreEqual("Component", artifactField.Caption);
			Assert.AreEqual("ComponentId", artifactField.Name);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.Lookup, (int)artifactField.ArtifactFieldTypeId);
			//Row 3
			artifactField = sortedFields[3];
			Assert.AreEqual("Created On", artifactField.Caption);
			Assert.AreEqual("CreationDate", artifactField.Name);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.DateTime, (int)artifactField.ArtifactFieldTypeId);
			//Row 10
			artifactField = sortedFields[10];
			Assert.AreEqual("ReleaseId", artifactField.Name);
			Assert.AreEqual("Release", artifactField.Caption);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.HierarchyLookup, (int)artifactField.ArtifactFieldTypeId);
			//Row 12
			artifactField = sortedFields[12];
			Assert.AreEqual("Requirement #", artifactField.Caption);
			Assert.AreEqual("RequirementId", artifactField.Name);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.Identifier, (int)artifactField.ArtifactFieldTypeId);
			//Row 17
			artifactField = sortedFields[17];
			Assert.AreEqual("Test Coverage", artifactField.Caption);
			Assert.AreEqual("CoverageId", artifactField.Name);
			Assert.AreEqual((int)Artifact.ArtifactFieldTypeEnum.Equalizer, (int)artifactField.ArtifactFieldTypeId);
		}

		/// <summary>
		/// Test that we can create and delete saved reports
		/// </summary>
		[
		Test,
		SpiraTestCase(553)
		]
		public void _05_CreateDeleteSaved()
		{
			Report report = new Report();

			//Insert a non-shared report for the project
			reportSavedId1 = reportManager.InsertSaved(
				REQUIREMENTS_REPORT_ID,
				HTML_FORMAT_ID,
				USER_ID_FRED_BLOGGS,
				PROJECT_ID,
				"Requirements Report 1",
				"reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1",
				false
				).ReportSavedId;

			//Verify that it inserted
			SavedReportView savedReport = reportManager.RetrieveSavedById(reportSavedId1);
			Assert.AreEqual(reportSavedId1, savedReport.ReportSavedId);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, savedReport.ReportId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedReport.UserId);
			Assert.AreEqual(PROJECT_ID, savedReport.ProjectId);
			Assert.AreEqual("Requirements Report 1", savedReport.Name);
			Assert.AreEqual("reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1", savedReport.Parameters);

			//Insert another non-shared report for the same project
			reportSavedId2 = reportManager.InsertSaved(
				REQUIREMENTS_REPORT_ID,
				HTML_FORMAT_ID,
				USER_ID_FRED_BLOGGS,
				PROJECT_ID,
				"Requirements Report 2",
				"reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1",
				false
				).ReportSavedId;

			//Verify that it inserted
			savedReport = reportManager.RetrieveSavedById(reportSavedId2);
			Assert.AreEqual(reportSavedId2, savedReport.ReportSavedId);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, savedReport.ReportId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedReport.UserId);
			Assert.AreEqual(PROJECT_ID, savedReport.ProjectId);
			Assert.AreEqual("Requirements Report 2", savedReport.Name);
			Assert.AreEqual("reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1", savedReport.Parameters);

			//Insert a shared report for the same project
			reportSavedId3 = reportManager.InsertSaved(
				REQUIREMENTS_REPORT_ID,
				HTML_FORMAT_ID,
				USER_ID_JOE_SMITH,
				PROJECT_ID,
				"Requirements Report 3",
				"reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1",
				true
				).ReportSavedId;

			//Verify that it inserted
			savedReport = reportManager.RetrieveSavedById(reportSavedId3);
			Assert.AreEqual(reportSavedId3, savedReport.ReportSavedId);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, savedReport.ReportId);
			Assert.AreEqual(USER_ID_JOE_SMITH, savedReport.UserId);
			Assert.AreEqual(PROJECT_ID, savedReport.ProjectId);
			Assert.AreEqual("Requirements Report 3", savedReport.Name);
			Assert.AreEqual("reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1", savedReport.Parameters);

			//Insert a shared report for another project
			reportSavedId4 = reportManager.InsertSaved(
				REQUIREMENTS_REPORT_ID,
				HTML_FORMAT_ID,
				USER_ID_FRED_BLOGGS,
				PROJECT_ID_EMPTY,
				"Requirements Report 4",
				"reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1",
				true
				).ReportSavedId;

			//Verify that it inserted
			savedReport = reportManager.RetrieveSavedById(reportSavedId4);
			Assert.AreEqual(reportSavedId4, savedReport.ReportSavedId);
			Assert.AreEqual(REQUIREMENTS_REPORT_ID, savedReport.ReportId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedReport.UserId);
			Assert.AreEqual(PROJECT_ID_EMPTY, savedReport.ProjectId);
			Assert.AreEqual("Requirements Report 4", savedReport.Name);
			Assert.AreEqual("reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1", savedReport.Parameters);

			//Now delete one of the saved reports and make sure it deleted OK.
			reportManager.DeleteSaved(reportSavedId2);
			bool deleted = false;
			try
			{
				reportManager.RetrieveSavedById(reportSavedId2);
			}
			catch (ArtifactNotExistsException)
			{
				deleted = true;
			}
			Assert.IsTrue(deleted, "Report didn't delete correctly");
		}

		/// <summary>
		/// Test that we can retrieve a saved report
		/// </summary>
		[
		Test,
		SpiraTestCase(552)
		]
		public void _06_RetrieveSaved()
		{
			Report report = new Report();

			//First lets test that we can retrieve the list of saved reports for a user (all projects)
			List<SavedReportView> savedReports = reportManager.RetrieveSaved(USER_ID_FRED_BLOGGS);
			Assert.AreEqual(2, savedReports.Count, "Count1");
			Assert.AreEqual("Requirements Report 1", savedReports[0].Name);
			Assert.AreEqual("Fred Bloggs", savedReports[0].UserName);
			Assert.AreEqual("Library Information System (Sample)", savedReports[0].ProjectName);
			Assert.AreEqual("Requirements Report 4", savedReports[1].Name);
			Assert.AreEqual("Fred Bloggs", savedReports[1].UserName);
			Assert.AreEqual("Sample Empty Product 1", savedReports[1].ProjectName);

			//Next test that we can retrieve the list of saved projects for a project
			//This will include all shared reports and any reports that this user has personally saved
			savedReports = reportManager.RetrieveSaved(USER_ID_FRED_BLOGGS, PROJECT_ID, true, null, null);
			Assert.AreEqual(2, savedReports.Count, "Count2");
			Assert.AreEqual("Requirements Report 1", savedReports[0].Name);
			Assert.AreEqual("Fred Bloggs", savedReports[0].UserName);
			Assert.AreEqual("Library Information System (Sample)", savedReports[0].ProjectName);
			Assert.AreEqual("Requirements Report 3", savedReports[1].Name);
			Assert.AreEqual("Joe P Smith", savedReports[1].UserName);
			Assert.AreEqual("Library Information System (Sample)", savedReports[1].ProjectName);

			//Finally test that we can retrieve the user's reports that are just for this project
			savedReports = reportManager.RetrieveSaved(USER_ID_FRED_BLOGGS, PROJECT_ID, false, null, null);
			Assert.AreEqual(1, savedReports.Count, "Count3");
			Assert.AreEqual("Requirements Report 1", savedReports[0].Name);
			Assert.AreEqual("Fred Bloggs", savedReports[0].UserName);
			Assert.AreEqual("Library Information System (Sample)", savedReports[0].ProjectName);
		}


		/// <summary>
		/// Tests that we can create custom reports from scratch
		/// </summary>
		[
		Test,
		SpiraTestCase(1167)
		]
		public void _07_CreateNewReportFromScratch()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Edit Reports";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create a new custom report that has several formats
			Report report = new Report();
			report.Name = "Custom Report 1";
			report.ReportCategoryId = 1;
			report.Description = "This is a new custom report";
			report.Header = "<p>This is a new custom report</p>";
			report.Footer = "<span>(C) MyCompany 2013</span>";
			report.IsActive = true;
			customReportId1 = reportManager.Report_Insert(report, new List<int>() { (int)Report.ReportFormatEnum.Html, (int)Report.ReportFormatEnum.Pdf, (int)Report.ReportFormatEnum.Xml, (int)Report.ReportFormatEnum.MsWord2003, (int)Report.ReportFormatEnum.MsWord2007 }, 1, adminSectionId, "Inserted Report").ReportId;

			//Verify that it saved successfully
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual("Custom Report 1", report.Name);
			Assert.AreEqual(1, report.ReportCategoryId);
			Assert.AreEqual("This is a new custom report", report.Description);
			Assert.AreEqual("<p>This is a new custom report</p>", report.Header);
			Assert.AreEqual("<span>(C) MyCompany 2013</span>", report.Footer);
			Assert.IsTrue(report.IsActive);

			//Verify formats
			Assert.AreEqual(5, report.Formats.Count);
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Html));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Xml));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Pdf));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsWord2003));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsWord2007));

			//Verify that it has no sections currently
			Assert.AreEqual(0, report.SectionInstances.Count);
			Assert.AreEqual(0, report.CustomSections.Count);

			//Now verify that we can update the report fields and change the formats
			report.StartTracking();
			report.Name = "Custom Report 1A";
			report.ReportCategoryId = 2;
			report.Description = "This is a new custom report modified A";
			report.Header = "<p>This is a new custom report modified A</p>";
			report.Footer = "<span>(C) MyCompany 2000-2013</span>";
			reportManager.Report_Update(report, new List<int>() { (int)Report.ReportFormatEnum.Html, (int)Report.ReportFormatEnum.Pdf, (int)Report.ReportFormatEnum.Xml, (int)Report.ReportFormatEnum.MsExcel2007 }, 1, adminSectionId, "Updated Report");

			//Verify that it updated successfully
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual("Custom Report 1A", report.Name);
			Assert.AreEqual(2, report.ReportCategoryId);
			Assert.AreEqual("This is a new custom report modified A", report.Description);
			Assert.AreEqual("<p>This is a new custom report modified A</p>", report.Header);
			Assert.AreEqual("<span>(C) MyCompany 2000-2013</span>", report.Footer);
			Assert.IsTrue(report.IsActive);

			//Verify formats
			Assert.AreEqual(4, report.Formats.Count);
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Html));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Xml));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Pdf));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsExcel2007));

			//Now verify that we can update the report fields without changing the formats
			report.StartTracking();
			report.Name = "Custom Report 1B";
			report.ReportCategoryId = 3;
			report.Description = "This is a new custom report modified B";
			report.Header = "<p>This is a new custom report modified B</p>";
			report.Footer = "<span>(C) MyCompany 2000-2013</span>";
			reportManager.Report_Update(report, null, 1, adminSectionId, "Updated Report");

			//Verify that it updated successfully
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual("Custom Report 1B", report.Name);
			Assert.AreEqual(3, report.ReportCategoryId);
			Assert.AreEqual("This is a new custom report modified B", report.Description);
			Assert.AreEqual("<p>This is a new custom report modified B</p>", report.Header);
			Assert.AreEqual("<span>(C) MyCompany 2000-2013</span>", report.Footer);
			Assert.IsTrue(report.IsActive);

			//Verify formats
			Assert.AreEqual(4, report.Formats.Count);
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Html));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Xml));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Pdf));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsExcel2007));
		}

		/// <summary>
		/// Tests that we can copy an existing standard report as a new custom report
		/// </summary>
		[
		Test,
		SpiraTestCase(1168)
		]
		public void _08_CopyStandardReportAsCustom()
		{
			//Lets make a copy of the Requirements Detailed Report
			customReportId2 = reportManager.Report_Copy(REQUIREMENTS_REPORT_ID, 1);

			//Verify that it copied correctly, although its token should be null
			Report report = reportManager.RetrieveById(customReportId2);
			Assert.AreEqual(1, report.ReportCategoryId);
			Assert.IsNull(report.Token);
			Assert.AreEqual("Requirements Detailed - Copy", report.Name);
			Assert.AreEqual("This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement's details and coverage status are displayed, along with sub-tables containing the list of covering test cases, linked incidents/requirements, attached documents, associated tasks, linked artifacts and the change history", report.Description);
			Assert.AreEqual("<p>This report displays all of the requirements defined for the current project in the order they appear in the requirements list. The requirement's details and coverage status are displayed, along with sub-tables containing the list of covering test cases, linked incidents/requirements, attached documents, associated tasks, linked artifacts and the change history</p>", report.Header);
			Assert.IsNull(report.Footer);
			Assert.AreEqual(true, report.IsActive);
			Assert.IsTrue(report.IsActive);

			//Verify formats
			Assert.AreEqual(5, report.Formats.Count);
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Html));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Xml));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.Pdf));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsWord2003));
			Assert.IsTrue(report.Formats.Any(r => r.ReportFormatId == (int)Report.ReportFormatEnum.MsWord2007));

			//Verify sections (custom and standard)
			Assert.AreEqual(0, report.CustomSections.Count);
			Assert.AreEqual(2, report.SectionInstances.Count);
			ReportSection reportSection = report.SectionInstances[0].Section;
			Assert.AreEqual(1, reportSection.ReportSectionId);
			Assert.AreEqual("ProjectOverview", reportSection.Token);
			Assert.IsNull(reportSection.ArtifactTypeId);
			reportSection = report.SectionInstances[1].Section;
			Assert.AreEqual(3, reportSection.ReportSectionId);
			Assert.AreEqual("RequirementDetails", reportSection.Token);
			Assert.AreEqual(1, reportSection.ArtifactTypeId);

			//Now test that we can get the list of elements for the various sections (need to explicitly sort)
			Assert.AreEqual(8, reportSection.Elements.Count);
			List<ReportElement> sortedElements = reportSection.Elements.OrderBy(re => re.Name).ToList();
			ReportElement reportElement = sortedElements[0];
			Assert.AreEqual(3, reportElement.ReportSectionId);
			Assert.AreEqual("History", reportElement.Token);
			Assert.AreEqual("Artifact Change History", reportElement.Name);
			Assert.AreEqual(true, reportElement.IsActive);
			reportElement = sortedElements[1];
			Assert.AreEqual(3, reportElement.ReportSectionId);
			Assert.AreEqual("Tasks", reportElement.Token);
			Assert.AreEqual("Associated Tasks", reportElement.Name);
			Assert.AreEqual(true, reportElement.IsActive);
		}

		/// <summary>
		/// Tests that we can add standard sections to a custom report
		/// </summary>
		[
		Test,
		SpiraTestCase(1169)
		]
		public void _09_AddStandardSectionsToReport()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Edit Reports";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First verify that the report has no standard sections already
			Report report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(0, report.SectionInstances.Count);

			//Now lets add a new standard section
			report.StartTracking();
			List<ReportSection> standardSections = reportManager.ReportSection_Retrieve();
			ReportSection standardSection = standardSections.FirstOrDefault(r => r.Token == "RequirementDetails");
			ReportSectionInstance newSection = new ReportSectionInstance();
			newSection.ReportSectionId = standardSection.ReportSectionId;
			newSection.Header = "<p>This is the details of a requirement</p>";
			newSection.Footer = "<span>Copyright Bloggs Inc. 2013</span>";
			newSection.Template = "<xml>some xslt goes here</xml>";
			report.SectionInstances.Add(newSection);
			reportManager.Report_Update(report,null, 1, adminSectionId, "Updated Report");

			//Verify that the section was added successfully
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(1, report.SectionInstances.Count);
			ReportSectionInstance sectionInstance = report.SectionInstances[0];
			Assert.AreEqual("RequirementDetails", sectionInstance.Section.Token);
			Assert.AreEqual("<p>This is the details of a requirement</p>", sectionInstance.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2013</span>", sectionInstance.Footer);
			Assert.AreEqual("<xml>some xslt goes here</xml>", sectionInstance.Template);

			//Now add a section and update the original one
			report.StartTracking();

			//Update the existing section
			sectionInstance.StartTracking();
			sectionInstance.Header = "<h1>This is the details of a requirement</h1>";
			sectionInstance.Footer = "<span>Copyright Bloggs Inc. 2000-2013</span>";
			sectionInstance.Template = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />";

			//Add a new section
			standardSection = standardSections.FirstOrDefault(r => r.Token == "TaskDetails");
			newSection = new ReportSectionInstance();
			newSection.ReportSectionId = standardSection.ReportSectionId;
			newSection.Header = "<p>This is the details of a task</p>";
			newSection.Footer = "<span>Copyright Bloggs Inc. 2013</span>";
			newSection.Template = "<xml><xslt:valueof select=\"Task\" /></xml>";
			report.SectionInstances.Add(newSection);
			reportManager.Report_Update(report,null, 1, adminSectionId, "Updated Report");

			//Verify the changes
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(2, report.SectionInstances.Count);
			sectionInstance = report.SectionInstances[0];
			Assert.AreEqual("RequirementDetails", sectionInstance.Section.Token);
			Assert.AreEqual("<h1>This is the details of a requirement</h1>", sectionInstance.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2000-2013</span>", sectionInstance.Footer);
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />", sectionInstance.Template);
			sectionInstance = report.SectionInstances[1];
			Assert.AreEqual("TaskDetails", sectionInstance.Section.Token);
			Assert.AreEqual("<p>This is the details of a task</p>", sectionInstance.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2013</span>", sectionInstance.Footer);
			Assert.AreEqual("<xml><xslt:valueof select=\"Task\" /></xml>", sectionInstance.Template);

			//Finally delete a section
			report.StartTracking();
			sectionInstance.MarkAsDeleted();
			reportManager.Report_Update(report,null, 1, adminSectionId, "Updated Report");

			//Verify the deletion
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(1, report.SectionInstances.Count);
			sectionInstance = report.SectionInstances[0];
			Assert.AreEqual("RequirementDetails", sectionInstance.Section.Token);
			Assert.AreEqual("<h1>This is the details of a requirement</h1>", sectionInstance.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2000-2013</span>", sectionInstance.Footer);
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />", sectionInstance.Template);
		}

		/// <summary>
		/// Tests that we can add custom sections to a custom report
		/// </summary>
		[
		Test,
		SpiraTestCase(1170)
		]
		public void _10_AddCustomSectionsToReport()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Edit Reports";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//First verify that the report has no custom sections already
			Report report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(0, report.CustomSections.Count);

			//Now lets add a new custom section
			report.StartTracking();
			ReportCustomSection newSection = new ReportCustomSection();
			newSection.Name = "Requirement List";
			newSection.Description = "This section displays a list of requirements";
			newSection.Query = "select value R from SpiraTestEntities.R_Requirements as R where PROJECT_ID = ${ProjectId}";
			newSection.Header = "<p>This is the details of a requirement</p>";
			newSection.Footer = "<span>Copyright Bloggs Inc. 2013</span>";
			newSection.Template = "<xml>some xslt goes here</xml>";
			newSection.IsActive = true;
			report.CustomSections.Add(newSection);
			reportManager.Report_Update(report, null, 1, adminSectionId, "Updated Report");

			//Verify that the section was added successfully
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(1, report.CustomSections.Count);
			ReportCustomSection customSection = report.CustomSections[0];
			Assert.AreEqual("Requirement List", customSection.Name);
			Assert.AreEqual("This section displays a list of requirements", customSection.Description);
			Assert.AreEqual("select value R from SpiraTestEntities.R_Requirements as R where PROJECT_ID = ${ProjectId}", customSection.Query);
			Assert.AreEqual("<p>This is the details of a requirement</p>", customSection.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2013</span>", customSection.Footer);
			Assert.AreEqual("<xml>some xslt goes here</xml>", customSection.Template);

			//Now add a section and update the original one
			report.StartTracking();

			//Update the existing section
			customSection.StartTracking();
			customSection.Description = "This section displays a list of requirement ids and names";
			customSection.Query = "select R.REQUIREMENT_ID, R.NAME from SpiraTestEntities.R_Requirements as R";
			customSection.Header = "<h1>This is the details of a requirement</h1>";
			customSection.Footer = "<span>Copyright Bloggs Inc. 2000-2013</span>";
			customSection.Template = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />";

			//Add a new section
			newSection = new ReportCustomSection();
			newSection.Name = "Task List";
			newSection.Description = "This section displays the list of tasks";
			newSection.Query = "select value T from SpiraTestEntities.R_Task as T where PROJECT_ID = ${ProjectId}";
			newSection.Header = "<p>This is the details of a task</p>";
			newSection.Footer = "<span>Copyright Bloggs Inc. 2013</span>";
			newSection.Template = "<xml><xslt:valueof select=\"Task\" /></xml>";
			newSection.IsActive = true;
			report.CustomSections.Add(newSection);
			reportManager.Report_Update(report,null, 1, adminSectionId, "Updated Report");

			//Verify the changes
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(2, report.CustomSections.Count);
			customSection = report.CustomSections[0];
			Assert.AreEqual("Requirement List", customSection.Name);
			Assert.AreEqual("This section displays a list of requirement ids and names", customSection.Description);
			Assert.AreEqual("select R.REQUIREMENT_ID, R.NAME from SpiraTestEntities.R_Requirements as R", customSection.Query);
			Assert.AreEqual("<h1>This is the details of a requirement</h1>", customSection.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2000-2013</span>", customSection.Footer);
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />", customSection.Template);
			customSection = report.CustomSections[1];
			Assert.AreEqual("Task List", customSection.Name);
			Assert.AreEqual("This section displays the list of tasks", customSection.Description);
			Assert.AreEqual("select value T from SpiraTestEntities.R_Task as T where PROJECT_ID = ${ProjectId}", customSection.Query);
			Assert.AreEqual("<p>This is the details of a task</p>", customSection.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2013</span>", customSection.Footer);
			Assert.AreEqual("<xml><xslt:valueof select=\"Task\" /></xml>", customSection.Template);

			//Finally delete a section
			report.StartTracking();
			customSection.MarkAsDeleted();
			reportManager.Report_Update(report,null, 1, adminSectionId, "Updated Report");

			//Verify the deletion
			report = reportManager.RetrieveById(customReportId1);
			Assert.AreEqual(1, report.CustomSections.Count);
			customSection = report.CustomSections[0];
			Assert.AreEqual("Requirement List", customSection.Name);
			Assert.AreEqual("This section displays a list of requirement ids and names", customSection.Description);
			Assert.AreEqual("select R.REQUIREMENT_ID, R.NAME from SpiraTestEntities.R_Requirements as R", customSection.Query);
			Assert.AreEqual("<h1>This is the details of a requirement</h1>", customSection.Header);
			Assert.AreEqual("<span>Copyright Bloggs Inc. 2000-2013</span>", customSection.Footer);
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" />", customSection.Template);

			//Verify that we can get a list of reportable entities
			Dictionary<string, string> reportableEntities = ReportManager.GetReportableEntities();
			Assert.AreEqual(62, reportableEntities.Count);

			//Verify that we can actually execute a simple custom query against each reportable entity
			foreach (KeyValuePair<string, string> kvp in reportableEntities)
			{
				string entitySetName = kvp.Key;
				string query = "select value R from " + entitySetName + " as R";
				XmlDocument xmlDoc = reportManager.ReportCustomSection_ExecuteSQL(-1, -1, query, null, 0, Int32.MaxValue, null);
				Assert.IsNotNull(xmlDoc.SelectSingleNode("/RESULTS"));
			}

			//Test that we can execute a custom query using a ReleaseId and/or ReleaseIds
			{
				//Single release
				string query = "select value R from SpiraTestEntities.R_Incidents as R where R.PROJECT_ID = ${ProjectId} and R.DETECTED_RELEASE_ID = ${ReleaseId}";
				XmlDocument xmlDoc = reportManager.ReportCustomSection_ExecuteSQL(PROJECT_ID, -1, query, null, 0, Int32.MaxValue, null);
				Assert.IsNotNull(xmlDoc.SelectSingleNode("/RESULTS"));
				xmlDoc = reportManager.ReportCustomSection_ExecuteSQL(PROJECT_ID, -1, query, null, 0, Int32.MaxValue, RELEASE_ID);
				Assert.IsNotNull(xmlDoc.SelectSingleNode("/RESULTS"));

				//Release and children
				query = "select value R from SpiraTestEntities.R_Incidents as R where R.PROJECT_ID = ${ProjectId} and R.DETECTED_RELEASE_ID IN {${ReleaseAndChildIds}}";
				xmlDoc = reportManager.ReportCustomSection_ExecuteSQL(PROJECT_ID, -1, query, null, 0, Int32.MaxValue, null);
				Assert.IsNotNull(xmlDoc.SelectSingleNode("/RESULTS"));
				xmlDoc = reportManager.ReportCustomSection_ExecuteSQL(PROJECT_ID, -1, query, null, 0, Int32.MaxValue, RELEASE_ID);
				Assert.IsNotNull(xmlDoc.SelectSingleNode("/RESULTS"));
			}
		}

		/// <summary>
		/// Tests that we can create a saved report from our new custom report and that deleting the custom report, deletes any
		/// linked saved and generated reports correctly (cascades appropriately)
		/// </summary>
		[
		Test,
		SpiraTestCase(1171)
		]
		public void _11_CreateSavedFromCustom()
		{
			//First create a saved report based on our custom report
			reportManager.InsertSaved(customReportId1, (int)Report.ReportFormatEnum.Html, USER_ID_FRED_BLOGGS, PROJECT_ID, "My Saved Report", "reportFormatId=1", false);

			//Now generate this report
			int reportGeneratedId = reportManager.GenerateReport(USER_ID_FRED_BLOGGS, PROJECT_ID, PROJECT_TEMPLATE_ID, customReportId1, "reportFormatId=1", "", InternalRoutines.APP_LOCATION);

			//Verify that we can retrieve the generated report
			ReportGenerated reportGenerated = reportManager.ReportGenerated_RetrieveById(reportGeneratedId);
			Assert.AreEqual(customReportId1, reportGenerated.ReportId);
			Assert.AreEqual(1, reportGenerated.ReportFormatId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, reportGenerated.UserId);

			//Verify that we can open a previously generated report for output
			string output = reportManager.GetReportText(reportGeneratedId);
			Assert.IsNotEmpty(output);

			//Verify that we can delete the generated report
			reportManager.ReportGenerated_Delete(reportGeneratedId);

			//Verify that it was deleted
			reportGenerated = reportManager.ReportGenerated_RetrieveById(reportGeneratedId);
			Assert.IsNull(reportGenerated);

			//Generate it again as we need to verify that deleting a report itself cascades correctly
			reportGeneratedId = reportManager.GenerateReport(USER_ID_FRED_BLOGGS, PROJECT_ID, PROJECT_TEMPLATE_ID, customReportId1, "reportFormatId=6", "", InternalRoutines.APP_LOCATION);

			//Now delete the entire report, it should cascade correctly
			reportManager.Report_Delete(customReportId1, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that we can store a saved report in the Planning > Documents section
		/// </summary>
		[
		Test,
		SpiraTestCase(2714)
		]
		public void _12_SaveReportAsProjectDocument()
		{
			//Lets test the function that gets the file extension for the report type
			string extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.Html);
			Assert.AreEqual(".htm", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.MsExcel2003);
			Assert.AreEqual(".xls", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.MsExcel2007);
			Assert.AreEqual(".xls", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.MsProj2003);
			Assert.AreEqual(".mpp", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.MsWord2003);
			Assert.AreEqual(".doc", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.MsWord2007);
			Assert.AreEqual(".doc", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.Pdf);
			Assert.AreEqual(".pdf", extension);
			extension = ReportManager.GetExtensionForFormat((int)Report.ReportFormatEnum.Xml);
			Assert.AreEqual(".xml", extension);

			//Next test that we can generate a simple report and have the result be stored in a project document folder
			AttachmentManager attachmentManager = new AttachmentManager();
			int documentFolderId = attachmentManager.GetDefaultProjectFolder(PROJECT_ID);
			string filename = "My-Report";
			int reportGeneratedId = reportManager.GenerateReport(USER_ID_FRED_BLOGGS, PROJECT_ID, PROJECT_TEMPLATE_ID, 1, String.Format("reportFormatId={0}&sg={1}|{2}", (int)Report.ReportFormatEnum.MsExcel2007, documentFolderId, filename), "", InternalRoutines.APP_LOCATION);

			//Make sure it created a report OK
			Assert.IsTrue(reportGeneratedId > 0);

			//Verify the document got created and has the right attachment for the format
			List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveForProject(PROJECT_ID, documentFolderId, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			ProjectAttachmentView projectAttachment = projectAttachments.FirstOrDefault(p => p.Filename == filename + ".xls");
			Assert.IsNotNull(projectAttachment);

			//Now delete this attachment to clean up
			attachmentManager.Delete(PROJECT_ID, projectAttachment.AttachmentId, 1);
		}
	}
}
