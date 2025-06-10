using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using System.Data;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Graph business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class GraphTest
	{
		protected static GraphManager graphManager;

		//Projects
		private const int PROJECT_ID = 1;
		private const int PROJECT_ID_EMPTY = 3;

		//Templates
		private const int PROJECT_TEMPLATE_ID = 1;
		private const int PROJECT_EMPTY_TEMPLATE_ID = 2;

		//Users
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		//Other
		private const int RELEASE_ID = 1;   //1.0.0.0
		private const int SPRINT_ID1 = 8;
		private const int SPRINT_ID2 = 10;
		private const int INCIDENT_TYPE_ID_BUG = 2;
		private const int TEST_CASE_TYPE_ID_FUNCTIONAL = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the business object
			graphManager = new Business.GraphManager();
		}

		[
		Test,
		SpiraTestCase(752)
		]
		public void _01_RetrieveGraphsByType()
		{
			//Get the list of date-range graphs
			List<Graph> graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.DateRangeGraphs);
			Assert.AreEqual(7, graphs.Count);
			Assert.AreEqual("Incident Progress Rate", graphs[0].Name);

			//Get the list of snapshot graphs
			graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.SnapshotGraphs);
			Assert.AreEqual(9, graphs.Count);
			Assert.AreEqual("Requirement Burndown", graphs[0].Name);

			//Get the list of summary graphs
			graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.SummaryGraphs);
			Assert.AreEqual(6, graphs.Count);
			Assert.AreEqual("Requirements Summary", graphs[0].Name);

			//Get the list of date-range graphs for incidents
			graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.DateRangeGraphs, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(4, graphs.Count);
			Assert.AreEqual("Incident Progress Rate", graphs[0].Name);
		}

		[
		Test,
		SpiraTestCase(763)
		]
		public void _02_RetrieveGraphsByID()
		{
			//Get a single graph by its ID
			Graph graph = graphManager.RetrieveById((int)Graph.GraphEnum.IncidentOpenCount);
			Assert.AreEqual("Incident Open Count", graph.Name);
			Assert.AreEqual(true, graph.IsActive);
			Assert.AreEqual(3, graph.Position);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, graph.ArtifactTypeId);
		}

		[
		Test,
		SpiraTestCase(753)
		]
		public void _03_RetrieveIncidentProgressRate()
		{
			//Get the incident progress rate information (used by the incident progress rate graph)
			//First Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			System.Data.DataSet summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["DiscoveredCount"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["ClosedCount"]) >= 0, "Closed Count Daily");

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[50]["DiscoveredCount"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[50]["ClosedCount"]) >= 0, "Closed Count Weekly");

			//Now get the discovery rate for only bugs since 11/5/2003
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			//Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["DiscoveredCount"]) > 0);
			//Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["ClosedCount"]) >= 0, "Closed Count Daily");

			//Now get the discovery rate for only bugs since 11/5/2003 for a specific release
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, (int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["DiscoveredCount"]));
			Assert.AreEqual(0, (int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["ClosedCount"]), "Closed Count Daily");

			//Make sure we can call these methods when there are no incidents without an error
			//Use a different project id, one that's empty
			dateRange.EndDate = DateTime.UtcNow;
			dateRange.StartDate = null;
			summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID_EMPTY, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			summaryDataSet = graphManager.RetrieveIncidentProgress(PROJECT_ID_EMPTY, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
		}

		[
		Test,
		SpiraTestCase(754)
		]
		public void _04_RetrieveIncidentCumulativeCount()
		{
			//Now get the cumulative total information (used by the cumulative total graph)
			//First Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			DataSet summaryDataSet = graphManager.RetrieveIncidentCumulativeCount(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["OpenCount"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["TotalCount"]) > 0);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentCumulativeCount(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["OpenCount"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["TotalCount"]) > 0);

			//Now get the cumulative count for only bugs since X date
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentCumulativeCount(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["OpenCount"]) > 0, "OpenCount");
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["TotalCount"]) > 0, "TotalCount");

			//Now get the cumulative count for only bugs since X date for a specific release
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentCumulativeCount(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["OpenCount"]) > 0, "OpenCount");
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["TotalCount"]) > 0, "TotalCount");

			//Make sure we can call these methods when there are no incidents without an error
			//Use a different project id, one that's empty
			summaryDataSet = graphManager.RetrieveIncidentCumulativeCount(PROJECT_ID_EMPTY, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
		}

		[
		Test,
		SpiraTestCase(755)
		]
		public void _05_RetrieveIncidentOpenCount()
		{
			//Now get the open count by priority (used by the open count by priority chart)
			//First All-Incidents Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			DataSet summaryDataSet = graphManager.RetrieveIncidentOpenCountByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(10, summaryDataSet.Tables["IncidentCount"].Rows[26]["0"], "Count0");
			Assert.AreEqual(11, summaryDataSet.Tables["IncidentCount"].Rows[26]["1"], "Count1");
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[26]["2"]) > 0, "Count2");
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentCount"].Rows[26]["3"], "Count3");
			Assert.AreEqual(6, summaryDataSet.Tables["IncidentCount"].Rows[26]["4"], "Count4");

			//Next just Bug Incidents Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentOpenCountByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["0"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["1"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["2"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["3"]) > 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["4"]) > 0);

			//Next just Bug Incidents Weekly for a specific release
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentOpenCountByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["0"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["1"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["2"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["3"]) >= 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentCount"].Rows[51]["4"]) >= 0);

			//Make sure we can call these methods when there are no incidents without an error
			//Use a different project id, one that's empty
			summaryDataSet = graphManager.RetrieveIncidentOpenCountByPriority(PROJECT_ID_EMPTY, PROJECT_EMPTY_TEMPLATE_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
		}

		[
		Test,
		SpiraTestCase(758)
		]
		public void _06_RetrieveTestRunProgressRate()
		{
			//Get the progress rate information (used by the progress rate graph) for all releases
			//First Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			DataSet summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] > DateTime.UtcNow.Date.AddDays(-10) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] < DateTime.UtcNow.Date);
			//Cannot use exact match because the sample data has time component, and is flaky if exact
			//Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] <= 1);
			//Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] <= 2);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow;
			dateRange.StartDate = DateTime.UtcNow.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[50]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[50]["1"] <= 10);
			//Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[50]["2"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[50]["2"] <= 3);

			//Get the progress rate information (used by the progress rate graph) for a specific release
			//First Daily
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID);
			Assert.AreEqual(31, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] > DateTime.UtcNow.Date.AddDays(-10) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] <= 1);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["2"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["2"] <= 2);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow;
			dateRange.StartDate = DateTime.UtcNow.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID);
			Assert.AreEqual(52, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[51]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[51]["1"] <= 3);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[51]["2"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[51]["2"] <= 3);

			//Get the progress rate information (used by the progress rate graph) for a specific release and specific test case type
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, TEST_CASE_TYPE_ID_FUNCTIONAL);
			Assert.AreEqual(31, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] > DateTime.UtcNow.Date.AddDays(-10) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[23]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["1"] <= 1);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[23]["2"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[23]["2"] <= 2);

			//Verify that the count for a project that has no runs is empty (we previously didn't manage this correctly)
			dateRange.EndDate = DateTime.UtcNow;
			dateRange.StartDate = DateTime.UtcNow.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(PROJECT_ID_EMPTY, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID);
			Assert.AreEqual(52, summaryDataSet.Tables["TestRunCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestRunCount"].Rows[51]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[51]["1"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[51]["1"] <= 3);
			Assert.IsTrue((int)summaryDataSet.Tables["TestRunCount"].Rows[51]["2"] >= 0 && (int)summaryDataSet.Tables["TestRunCount"].Rows[51]["2"] <= 3);
		}

		[Test, SpiraTestCase(756)]
		public void _07_RetrieveIncidentAging()
		{
			//Now get the incident aging by priority (used by the incident aging chart)

			//Verifying last record for all incidents
			DataSet summaryDataSet = graphManager.RetrieveIncidentAgingByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("> 90", summaryDataSet.Tables["IncidentAging"].Rows[13]["Age"]);
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentAging"].Rows[13]["1"], "Count1");
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentAging"].Rows[13]["2"], "Count2");
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentAging"].Rows[13]["3"], "Count3");
			Assert.AreEqual(2, summaryDataSet.Tables["IncidentAging"].Rows[13]["4"], "Count4");
			Assert.AreEqual(2, summaryDataSet.Tables["IncidentAging"].Rows[13]["0"], "Count5");

			//Verifying last record for just bugs
			summaryDataSet = graphManager.RetrieveIncidentAgingByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("> 90", summaryDataSet.Tables["IncidentAging"].Rows[13]["Age"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentAging"].Rows[13]["0"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentAging"].Rows[13]["1"]);
			Assert.AreEqual(1, summaryDataSet.Tables["IncidentAging"].Rows[13]["2"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentAging"].Rows[13]["3"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentAging"].Rows[13]["4"]);

			//Verifying last record for just bugs for a specific release
			summaryDataSet = graphManager.RetrieveIncidentAgingByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("> 90", summaryDataSet.Tables["IncidentAging"].Rows[13]["Age"]);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentAging"].Rows[13]["0"]) == 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentAging"].Rows[13]["1"]) == 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentAging"].Rows[13]["2"]) == 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentAging"].Rows[13]["3"]) == 0);
			Assert.IsTrue((int)(summaryDataSet.Tables["IncidentAging"].Rows[13]["4"]) == 0);

			//Make sure we can call these methods when there are no incidents without an error
			//Use a different project id, one that's empty
			summaryDataSet = graphManager.RetrieveIncidentAgingByPriority(PROJECT_ID_EMPTY, PROJECT_EMPTY_TEMPLATE_ID);
		}

		[Test, SpiraTestCase(757)]
		public void _08_RetrieveIncidentTurnaround()
		{
			//Now get the incident turnaround by priority (used by the incident turnaround chart)
			//Verifying last record for all incidents
			DataSet summaryDataSet = graphManager.RetrieveIncidentTurnaroundByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentTurnaround"].Rows.Count);
			Assert.AreEqual("> 90", summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["Turnaround"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["2"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["3"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["4"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[13]["0"]);

			//Verifying first record for just bugs
			summaryDataSet = graphManager.RetrieveIncidentTurnaroundByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentTurnaround"].Rows.Count);
			Assert.AreEqual("0-7", summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["Turnaround"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["2"]);
			Assert.AreEqual(1, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["3"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["4"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["0"]);

			//Verifying first record for just bugs for a specific release
			summaryDataSet = graphManager.RetrieveIncidentTurnaroundByPriority(PROJECT_ID, PROJECT_TEMPLATE_ID, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(14, summaryDataSet.Tables["IncidentTurnaround"].Rows.Count);
			Assert.AreEqual("0-7", summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["Turnaround"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["2"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["3"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["4"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentTurnaround"].Rows[0]["0"]);

			//Make sure we can call these methods when there are no incidents without an error
			//Use a different project id, one that's empty
			summaryDataSet = graphManager.RetrieveIncidentTurnaroundByPriority(PROJECT_ID_EMPTY, PROJECT_EMPTY_TEMPLATE_ID);
		}

		[Test, SpiraTestCase(764)]
		public void _09_SummaryGraphs()
		{
			//Now test that we can get the incident fields used in the dynamic summary chart
			DataSet summaryDataSet = graphManager.RetrieveSummaryChartFields(PROJECT_ID, PROJECT_TEMPLATE_ID, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(15, summaryDataSet.Tables["ArtifactFields"].Rows.Count);
			Assert.AreEqual("Component", (string)summaryDataSet.Tables["ArtifactFields"].Rows[0]["Caption"]);
			Assert.AreEqual("Detected By", (string)summaryDataSet.Tables["ArtifactFields"].Rows[1]["Caption"]);
			Assert.AreEqual("Detected Release", (string)summaryDataSet.Tables["ArtifactFields"].Rows[2]["Caption"]);
			Assert.AreEqual("Type", (string)summaryDataSet.Tables["ArtifactFields"].Rows[12]["Caption"]);
			Assert.AreEqual("Verified Release", (string)summaryDataSet.Tables["ArtifactFields"].Rows[13]["Caption"]);

			//Now test that we can get the summary graph data (need to sort in-memory to match UI)
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "IncidentStatusId", "PriorityId", DataModel.Artifact.ArtifactTypeEnum.Incident);
			DataView dataView = summaryDataSet.Tables[0].DefaultView;
			dataView.Sort = "XAxis ASC"; ;
			Assert.AreEqual(7, dataView.Count);
			Assert.AreEqual("Assigned", dataView[0]["XAxis"]);
			Assert.AreEqual(4, dataView[0]["1"]);
			Assert.AreEqual("Resolved", dataView[6]["XAxis"]);
			Assert.AreEqual(2, dataView[6]["4"]);

			//Now test that we can get the summary graph data (need to sort in-memory to match UI) - filtered by release
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "IncidentStatusId", "PriorityId", DataModel.Artifact.ArtifactTypeEnum.Incident, RELEASE_ID);
			dataView = summaryDataSet.Tables[0].DefaultView;
			dataView.Sort = "XAxis ASC"; ;
			Assert.AreEqual(2, dataView.Count);
			Assert.AreEqual("Assigned", dataView[0]["XAxis"]);
			Assert.AreEqual(1, dataView[0]["1"]);

			//Now test that we can get the requirement fields used in the dynamic summary chart
			summaryDataSet = graphManager.RetrieveSummaryChartFields(PROJECT_ID, PROJECT_TEMPLATE_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(9, summaryDataSet.Tables["ArtifactFields"].Rows.Count);
			Assert.AreEqual("Author", (string)summaryDataSet.Tables["ArtifactFields"].Rows[0]["Caption"]);
			Assert.AreEqual("Classification", (string)summaryDataSet.Tables["ArtifactFields"].Rows[1]["Caption"]);
			Assert.AreEqual("Component", (string)summaryDataSet.Tables["ArtifactFields"].Rows[2]["Caption"]);
			Assert.AreEqual("Difficulty", (string)summaryDataSet.Tables["ArtifactFields"].Rows[3]["Caption"]);
			Assert.AreEqual("Importance", (string)summaryDataSet.Tables["ArtifactFields"].Rows[4]["Caption"]);
			Assert.AreEqual("Owner", (string)summaryDataSet.Tables["ArtifactFields"].Rows[5]["Caption"]);
			Assert.AreEqual("Release", (string)summaryDataSet.Tables["ArtifactFields"].Rows[6]["Caption"]);
			Assert.AreEqual("Status", (string)summaryDataSet.Tables["ArtifactFields"].Rows[7]["Caption"]);
			Assert.AreEqual("Type", (string)summaryDataSet.Tables["ArtifactFields"].Rows[8]["Caption"]);

			//Now test that we can get the summary graph data
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "RequirementStatusId", "ImportanceId", DataModel.Artifact.ArtifactTypeEnum.Requirement);
			summaryDataSet.Tables["SummaryCount"].DefaultView.Sort = "XAxis ASC";
			Assert.AreEqual(6, summaryDataSet.Tables["SummaryCount"].DefaultView.Count);
			Assert.AreEqual("Developed", summaryDataSet.Tables["SummaryCount"].DefaultView[1]["XAxis"]);
			Assert.AreEqual(0, summaryDataSet.Tables["SummaryCount"].DefaultView[1]["1"]);
			Assert.AreEqual("Requested", summaryDataSet.Tables["SummaryCount"].DefaultView[4]["XAxis"]);
			Assert.AreEqual(4, summaryDataSet.Tables["SummaryCount"].DefaultView[4]["3"]);
			Assert.AreEqual("In Progress", summaryDataSet.Tables["SummaryCount"].DefaultView[2]["XAxis"]);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].DefaultView[2]["2"]);

			//Now test that we can get the summary graph data - for a specific release
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "RequirementStatusId", "ImportanceId", DataModel.Artifact.ArtifactTypeEnum.Requirement, RELEASE_ID);
			summaryDataSet.Tables["SummaryCount"].DefaultView.Sort = "XAxis ASC";
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].DefaultView.Count);
			Assert.AreEqual("Planned", summaryDataSet.Tables["SummaryCount"].DefaultView[1]["XAxis"]);
			Assert.AreEqual(0, summaryDataSet.Tables["SummaryCount"].DefaultView[1]["1"]);
			Assert.AreEqual("Tested", summaryDataSet.Tables["SummaryCount"].DefaultView[2]["XAxis"]);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].DefaultView[2]["2"]);

			//Now test that we can get the summary graph data for custom properties as the x-axis
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "Custom_02", "RequirementStatusId", DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, summaryDataSet.Tables["SummaryCount"].Rows.Count);
			Assert.AreEqual("None", summaryDataSet.Tables["SummaryCount"].Rows[0]["XAxis"]);
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].Rows[0][((int)Requirement.RequirementStatusEnum.InProgress).ToString()]);
			Assert.AreEqual("Moderate", summaryDataSet.Tables["SummaryCount"].Rows[1]["XAxis"]);
			Assert.AreEqual(0, summaryDataSet.Tables["SummaryCount"].Rows[1][((int)Requirement.RequirementStatusEnum.Developed).ToString()]);

			//Now test that we can get the summary graph data for custom properties as the group-by
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "RequirementStatusId", "Custom_02", DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].Rows.Count);
			Assert.AreEqual("Completed", summaryDataSet.Tables["SummaryCount"].Rows[0]["XAxis"]);

			//Find the column that has the caption 'Moderate'
			DataColumn matchedDataColumn = null;
			foreach (DataColumn dataColumn in summaryDataSet.Tables["SummaryCount"].Columns)
			{
				if (dataColumn.Caption == "Moderate")
				{
					matchedDataColumn = dataColumn;
					break;
				}
			}
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].Rows[0][matchedDataColumn]);

			//Now test that we can get the test case fields used in the dynamic summary chart
			summaryDataSet = graphManager.RetrieveSummaryChartFields(PROJECT_ID, PROJECT_TEMPLATE_ID, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(11, summaryDataSet.Tables["ArtifactFields"].Rows.Count, "FieldCount");
			Assert.AreEqual("Author", (string)summaryDataSet.Tables["ArtifactFields"].Rows[0]["Caption"]);
			Assert.AreEqual("Automation Engine", (string)summaryDataSet.Tables["ArtifactFields"].Rows[1]["Caption"]);
			Assert.AreEqual("Component", (string)summaryDataSet.Tables["ArtifactFields"].Rows[2]["Caption"]);
			Assert.AreEqual("Execution Status", (string)summaryDataSet.Tables["ArtifactFields"].Rows[3]["Caption"]);
			Assert.AreEqual("Owner", (string)summaryDataSet.Tables["ArtifactFields"].Rows[4]["Caption"]);
			Assert.AreEqual("Priority", (string)summaryDataSet.Tables["ArtifactFields"].Rows[5]["Caption"]);
			Assert.AreEqual("Status", (string)summaryDataSet.Tables["ArtifactFields"].Rows[7]["Caption"]);
			Assert.AreEqual("Test Type", (string)summaryDataSet.Tables["ArtifactFields"].Rows[9]["Caption"]);

			//Now test that we can get the summary graph data
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ExecutionStatusId", "TestCasePriorityId", DataModel.Artifact.ArtifactTypeEnum.TestCase);
			summaryDataSet.Tables["SummaryCount"].DefaultView.Sort = "XAxis ASC";
			Assert.AreEqual(4, summaryDataSet.Tables["SummaryCount"].DefaultView.Count);
			Assert.AreEqual("Failed", summaryDataSet.Tables["SummaryCount"].DefaultView[1]["XAxis"]);
			Assert.AreEqual(0, summaryDataSet.Tables["SummaryCount"].DefaultView[1]["2"]);
			Assert.AreEqual("Not Run", summaryDataSet.Tables["SummaryCount"].DefaultView[2]["XAxis"]);
			Assert.AreEqual(6, summaryDataSet.Tables["SummaryCount"].DefaultView[2]["0"]);

			//Now test that we can get the summary graph data - for a specific release
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ExecutionStatusId", "TestCasePriorityId", DataModel.Artifact.ArtifactTypeEnum.TestCase, RELEASE_ID);
			summaryDataSet.Tables["SummaryCount"].DefaultView.Sort = "XAxis ASC";
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].DefaultView.Count);
			Assert.AreEqual("Caution", summaryDataSet.Tables["SummaryCount"].DefaultView[0]["XAxis"]);
			Assert.AreEqual(0, summaryDataSet.Tables["SummaryCount"].DefaultView[0]["2"]);
			Assert.AreEqual("Passed", summaryDataSet.Tables["SummaryCount"].DefaultView[2]["XAxis"]);
			Assert.AreEqual(4, summaryDataSet.Tables["SummaryCount"].DefaultView[2]["2"]);

			//Now test that we can get the test run fields used in the dynamic summary chart
			summaryDataSet = graphManager.RetrieveSummaryChartFields(PROJECT_ID, PROJECT_TEMPLATE_ID, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			Assert.AreEqual(9, summaryDataSet.Tables["ArtifactFields"].Rows.Count);
			Assert.AreEqual("Automation Host", (string)summaryDataSet.Tables["ArtifactFields"].Rows[0]["Caption"]);
			Assert.AreEqual("Build", (string)summaryDataSet.Tables["ArtifactFields"].Rows[1]["Caption"]);
			Assert.AreEqual("Operating System", (string)summaryDataSet.Tables["ArtifactFields"].Rows[2]["Caption"]);
			Assert.AreEqual("Release", (string)summaryDataSet.Tables["ArtifactFields"].Rows[3]["Caption"]);
			Assert.AreEqual("Status", (string)summaryDataSet.Tables["ArtifactFields"].Rows[4]["Caption"]);
			Assert.AreEqual("Test Set", (string)summaryDataSet.Tables["ArtifactFields"].Rows[5]["Caption"]);
			Assert.AreEqual("Tester", (string)summaryDataSet.Tables["ArtifactFields"].Rows[6]["Caption"]);
			Assert.AreEqual("Type", (string)summaryDataSet.Tables["ArtifactFields"].Rows[7]["Caption"]);
			Assert.AreEqual("Web Browser", (string)summaryDataSet.Tables["ArtifactFields"].Rows[8]["Caption"]);

			//Now test that we can get the summary graph data
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ExecutionStatusId", "TestRunTypeId", DataModel.Artifact.ArtifactTypeEnum.TestRun);
			Assert.AreEqual(4, summaryDataSet.Tables["SummaryCount"].Rows.Count);
			Assert.AreEqual("Blocked", summaryDataSet.Tables["SummaryCount"].Rows[0]["XAxis"]);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].Rows[0]["2"]);
			Assert.AreEqual("Passed", summaryDataSet.Tables["SummaryCount"].Rows[3]["XAxis"]);
			Assert.AreEqual(14, summaryDataSet.Tables["SummaryCount"].Rows[3]["1"]);

			//Now test that we can get the summary graph data - for a specific release
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ExecutionStatusId", "TestRunTypeId", DataModel.Artifact.ArtifactTypeEnum.TestRun, RELEASE_ID);
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].Rows.Count);
			Assert.AreEqual("Failed", summaryDataSet.Tables["SummaryCount"].Rows[0]["XAxis"]);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].Rows[0]["2"]);
			Assert.AreEqual("Passed", summaryDataSet.Tables["SummaryCount"].Rows[2]["XAxis"]);
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].Rows[2]["1"]);

			//First test the task summary graph data
			DataSet reportData = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "OwnerId", "TaskStatusId", DataModel.Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual("Fred Bloggs", (string)reportData.Tables["SummaryCount"].Rows[0]["XAxis"]);
			Assert.AreEqual(19, (int)reportData.Tables["SummaryCount"].Rows[0][Task.TaskStatusEnum.Completed.GetHashCode().ToString()]);
			Assert.AreEqual(5, (int)reportData.Tables["SummaryCount"].Rows[0][Task.TaskStatusEnum.NotStarted.GetHashCode().ToString()]);
			Assert.AreEqual("Joe P Smith", (string)reportData.Tables["SummaryCount"].Rows[1]["XAxis"]);
			Assert.AreEqual(11, (int)reportData.Tables["SummaryCount"].Rows[1][Task.TaskStatusEnum.Completed.GetHashCode().ToString()]);
			Assert.AreEqual(10, (int)reportData.Tables["SummaryCount"].Rows[1][Task.TaskStatusEnum.NotStarted.GetHashCode().ToString()]);

			//First test the task summary graph data - for a specific release
			reportData = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "OwnerId", "TaskStatusId", DataModel.Artifact.ArtifactTypeEnum.Task, RELEASE_ID);
			Assert.AreEqual("Fred Bloggs", (string)reportData.Tables["SummaryCount"].Rows[0]["XAxis"]);
			Assert.AreEqual(10, (int)reportData.Tables["SummaryCount"].Rows[0][Task.TaskStatusEnum.Completed.GetHashCode().ToString()]);
			Assert.AreEqual(1, (int)reportData.Tables["SummaryCount"].Rows[0][Task.TaskStatusEnum.NotStarted.GetHashCode().ToString()]);
			Assert.AreEqual("Joe P Smith", (string)reportData.Tables["SummaryCount"].Rows[1]["XAxis"]);
			Assert.AreEqual(5, (int)reportData.Tables["SummaryCount"].Rows[1][Task.TaskStatusEnum.Completed.GetHashCode().ToString()]);
			Assert.AreEqual(4, (int)reportData.Tables["SummaryCount"].Rows[1][Task.TaskStatusEnum.NotStarted.GetHashCode().ToString()]);

			//Test that we can get the incident summary with ComponentIds multi-value field as the x-axis
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ComponentIds", "PriorityId", DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].Rows.Count);

			//Test that we can get the test case summary with ComponentIds multi-value field as the x-axis
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "ComponentIds", "TestCasePriorityId", DataModel.Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(3, summaryDataSet.Tables["SummaryCount"].Rows.Count);

			//Test that we can get the test case summary with custom property x axis and execution status as group by
			//filtered by release. This gave an error at one point.
			summaryDataSet = graphManager.RetrieveSummaryCount(PROJECT_ID, PROJECT_TEMPLATE_ID, "Custom_03", "ExecutionStatusId", DataModel.Artifact.ArtifactTypeEnum.TestCase, RELEASE_ID);
			Assert.AreEqual(1, summaryDataSet.Tables["SummaryCount"].Rows.Count);
		}

		[Test, SpiraTestCase(759)]
		public void _10_RequirementsCoverage()
		{
			//Now test that we can get the requirements coverage graph by importance
			DataSet summaryDataSet = graphManager.Requirement_RetrieveCoverageByImportance(PROJECT_ID, PROJECT_TEMPLATE_ID);
			Assert.AreEqual(6, summaryDataSet.Tables["RequirementsCoverage"].Rows.Count);
			Assert.AreEqual("Passed", summaryDataSet.Tables["RequirementsCoverage"].Rows[0]["CoverageStatus"]);
			Assert.AreEqual(4.4, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[0]["1"], 1));
			Assert.AreEqual("Not Run", summaryDataSet.Tables["RequirementsCoverage"].Rows[4]["CoverageStatus"]);
			Assert.AreEqual(1.3, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[4]["1"], 1));
			Assert.AreEqual("Not Covered", summaryDataSet.Tables["RequirementsCoverage"].Rows[5]["CoverageStatus"]);
			Assert.AreEqual(2.0, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[5]["4"], 1));

			//Now for a specific release (which includes child iterations)
			summaryDataSet = graphManager.Requirement_RetrieveCoverageByImportance(PROJECT_ID, PROJECT_TEMPLATE_ID, RELEASE_ID);
			Assert.AreEqual(6, summaryDataSet.Tables["RequirementsCoverage"].Rows.Count);
			Assert.AreEqual("Passed", summaryDataSet.Tables["RequirementsCoverage"].Rows[0]["CoverageStatus"]);
			Assert.AreEqual(1.7, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[0]["1"], 1));
			Assert.AreEqual("Not Run", summaryDataSet.Tables["RequirementsCoverage"].Rows[4]["CoverageStatus"]);
			Assert.AreEqual(0, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[4]["3"], 1));
			Assert.AreEqual("Not Covered", summaryDataSet.Tables["RequirementsCoverage"].Rows[5]["CoverageStatus"]);
			Assert.AreEqual(1.0, Math.Round((double)summaryDataSet.Tables["RequirementsCoverage"].Rows[5]["3"], 1));
		}

		[Test, SpiraTestCase(760)]
		public void _11_TaskVelocity()
		{
			//Next test the task velocity graph data
			//For the project as a whole
			DataSet reportData = graphManager.RetrieveTaskVelocity(PROJECT_ID);
			Assert.AreEqual("Release", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0", (string)reportData.Tables["Velocity"].Rows[0]["XAxis"]);
			Assert.AreEqual(216M, (decimal)reportData.Tables["Velocity"].Rows[0]["ExpectedVelocity"]);
			Assert.AreEqual(94M, Decimal.Round((decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"], 0));

			//For a specific release
			reportData = graphManager.RetrieveTaskVelocity(PROJECT_ID, RELEASE_ID);
			Assert.AreEqual("Sprint", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0.0001", (string)reportData.Tables["Velocity"].Rows[0]["XAxis"]);
			Assert.AreEqual(96M, (decimal)reportData.Tables["Velocity"].Rows[0]["ExpectedVelocity"]);
			Assert.AreEqual(32M, (decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"]);

			//For a specific iteration
			reportData = graphManager.RetrieveTaskVelocity(PROJECT_ID, SPRINT_ID1);
			Assert.AreEqual("Date", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			//Assert.IsTrue(DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]) >= DateTime.UtcNow.Date.AddDays(-45) && DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]) <= DateTime.UtcNow.Date.AddDays(-40), "Actual date: " + DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]));
			Assert.IsTrue((decimal)reportData.Tables["Velocity"].Rows[0]["ExpectedVelocity"] > 0M);
			Assert.IsTrue((decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"] > 0M);
		}

		[Test, SpiraTestCase(761)]
		public void _12_TaskBurnup()
		{
			//Next test the task burnup graph data

			//For the project as a whole
			DataSet reportData = graphManager.RetrieveTaskBurnup(PROJECT_ID, null);
			Assert.AreEqual("Release", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["RemainingEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["EstimatedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealEffort"]);
			Assert.AreEqual("1.2.0.0", (string)reportData.Tables["Burnup"].Rows[4]["XAxis"]);
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["CompletedEffort"], 0));
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["RemainingEffort"], 0));
			Assert.AreEqual(192M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["EstimatedEffort"], 0));
			Assert.AreEqual(192M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["IdealEffort"], 0));

			//For a specific release
			reportData = graphManager.RetrieveTaskBurnup(PROJECT_ID, RELEASE_ID);
			Assert.AreEqual("Sprint", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["RemainingEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["EstimatedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealEffort"]);
			Assert.AreEqual("1.0.0.0.0003", (string)reportData.Tables["Burnup"].Rows[3]["XAxis"]);
			Assert.AreEqual(30M, (decimal)reportData.Tables["Burnup"].Rows[3]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[3]["RemainingEffort"]);
			Assert.AreEqual(94M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[3]["EstimatedEffort"], 0));
			Assert.AreEqual(94M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[3]["IdealEffort"], 0));

			//For a specific iteration
			reportData = graphManager.RetrieveTaskBurnup(PROJECT_ID, SPRINT_ID2);
			Assert.AreEqual("Date", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["RemainingEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["EstimatedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealEffort"]);
			//Assert.IsTrue(DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]) >= DateTime.UtcNow.Date.AddDays(-32) && DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]) <= DateTime.UtcNow.Date.AddDays(-26), "Actual date: " + DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]));
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[1]["CompletedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[1]["RemainingEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[1]["EstimatedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[1]["IdealEffort"] >= 0M);
		}

		[Test, SpiraTestCase(762)]
		public void _13_TaskBurndown()
		{
			//Next test the task burndown graph data

			//For the project as a whole
			DataSet reportData = graphManager.RetrieveTaskBurndown(PROJECT_ID, null);
			Assert.AreEqual("Release", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0", (string)reportData.Tables["Burndown"].Rows[0]["XAxis"]);
			Assert.AreEqual(78.1M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["CompletedEffort"], 1));
			Assert.AreEqual(16.2M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["RemainingEffort"], 1));
			Assert.AreEqual(192.2M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["EstimatedEffort"], 1));
			Assert.AreEqual(192.2M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["IdealEffort"], 1));
			Assert.AreEqual("1.2.0.0", (string)reportData.Tables["Burndown"].Rows[3]["XAxis"]);
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["CompletedEffort"], 0));
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["RemainingEffort"], 0));
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["EstimatedEffort"], 0));
			Assert.AreEqual(48M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["IdealEffort"], 0));

			//For a specific release
			reportData = graphManager.RetrieveTaskBurndown(PROJECT_ID, RELEASE_ID);
			Assert.AreEqual("Sprint", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0.0001", (string)reportData.Tables["Burndown"].Rows[1]["XAxis"]);
			Assert.AreEqual(32M, (decimal)reportData.Tables["Burndown"].Rows[1]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burndown"].Rows[1]["RemainingEffort"]);
			Assert.AreEqual(94M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["EstimatedEffort"], 0));
			Assert.AreEqual(94M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["IdealEffort"], 0));
			Assert.AreEqual("1.0.0.0.0003", (string)reportData.Tables["Burndown"].Rows[3]["XAxis"]);
			Assert.AreEqual(30M, (decimal)reportData.Tables["Burndown"].Rows[3]["CompletedEffort"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burndown"].Rows[3]["RemainingEffort"]);
			//Assert.AreEqual(30M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[2]["EstimatedEffort"], 0));
			//Assert.AreEqual(31.4M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[2]["IdealEffort"], 1));

			//For a specific iteration
			DateTime dataTimeValue;
			reportData = graphManager.RetrieveTaskBurndown(PROJECT_ID, SPRINT_ID2);
			Assert.IsTrue(reportData.Tables["Burndown"].Rows.Count > 0);
			Assert.AreEqual("Date", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			Assert.IsTrue(DateTime.TryParse((string)reportData.Tables["Burndown"].Rows[0]["XAxis"], out dataTimeValue));
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["CompletedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["RemainingEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["EstimatedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["IdealEffort"] >= 0M);
			int lastIndex = reportData.Tables["Burndown"].Rows.Count - 1;
			Assert.IsTrue(DateTime.TryParse((string)reportData.Tables["Burndown"].Rows[lastIndex]["XAxis"], out dataTimeValue));
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[lastIndex]["CompletedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[lastIndex]["RemainingEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[lastIndex]["EstimatedEffort"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[lastIndex]["IdealEffort"] >= 0M);
		}

		[Test, SpiraTestCase(1316)]
		public void _14_RequirementVelocity()
		{
			//Next test the requirement velocity graph data
			//For the project as a whole
			DataSet reportData = graphManager.Requirement_RetrieveVelocity(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Release", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0", (string)reportData.Tables["Velocity"].Rows[0]["XAxis"]);
			Assert.AreEqual(9.5M, (decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"]);
			Assert.AreEqual(4.6M, Decimal.Round((decimal)reportData.Tables["Velocity"].Rows[0]["AverageVelocity"], 1));
			Assert.AreEqual(9.5M, (decimal)reportData.Tables["Velocity"].Rows[0]["RollingAverage"]);

			//For a specific release
			reportData = graphManager.Requirement_RetrieveVelocity(PROJECT_ID, RELEASE_ID, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Sprint", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0.0001", (string)reportData.Tables["Velocity"].Rows[0]["XAxis"]);
			Assert.AreEqual(4.0M, (decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"]);
			Assert.AreEqual(3.2M, Decimal.Round((decimal)reportData.Tables["Velocity"].Rows[0]["AverageVelocity"], 1));
			Assert.AreEqual(4.0M, (decimal)reportData.Tables["Velocity"].Rows[0]["RollingAverage"]);

			//For a specific iteration
			reportData = graphManager.Requirement_RetrieveVelocity(PROJECT_ID, SPRINT_ID1, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Date", reportData.Tables["Velocity"].Columns["XAxis"].Caption);
			//Assert.IsTrue(DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]) >= DateTime.UtcNow.Date.AddDays(-39) && DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]) <= DateTime.UtcNow.Date.AddDays(-35), "Actual date: " + DateTime.Parse((string)reportData.Tables["Velocity"].Rows[0]["XAxis"]));
			Assert.IsTrue((decimal)reportData.Tables["Velocity"].Rows[0]["ActualVelocity"] > 0M);
			Assert.IsTrue((decimal)reportData.Tables["Velocity"].Rows[0]["AverageVelocity"] > 0M);
			Assert.IsTrue((decimal)reportData.Tables["Velocity"].Rows[0]["RollingAverage"] > 0M);
		}

		[Test, SpiraTestCase(1315)]
		public void _15_RequirementBurnup()
		{
			//Next test the requirement burnup graph data

			//For the project as a whole
			DataSet reportData = graphManager.Requirement_RetrieveBurnUp(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Release", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedPoints"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealBurnup"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["ActualBurnup"]);
			Assert.AreEqual("1.2.0.0", (string)reportData.Tables["Burnup"].Rows[4]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[4]["CompletedPoints"]);
			Assert.AreEqual(18.5M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["IdealBurnup"], 1));
			Assert.AreEqual(18.5M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[4]["ActualBurnup"], 1));

			//For a specific release
			reportData = graphManager.Requirement_RetrieveBurnUp(PROJECT_ID, RELEASE_ID, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Sprint", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedPoints"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealBurnup"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["ActualBurnup"]);
			Assert.AreEqual("1.0.0.0.0003", (string)reportData.Tables["Burnup"].Rows[3]["XAxis"]);
			Assert.AreEqual(1.5M, (decimal)reportData.Tables["Burnup"].Rows[3]["CompletedPoints"]);
			Assert.AreEqual(9.5M, (decimal)reportData.Tables["Burnup"].Rows[3]["IdealBurnup"]);
			Assert.AreEqual(9.5M, Decimal.Round((decimal)reportData.Tables["Burnup"].Rows[3]["ActualBurnup"], 1));

			//For a specific iteration
			reportData = graphManager.Requirement_RetrieveBurnUp(PROJECT_ID, SPRINT_ID1, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Date", reportData.Tables["Burnup"].Columns["XAxis"].Caption);
			Assert.AreEqual("Start", (string)reportData.Tables["Burnup"].Rows[0]["XAxis"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["CompletedPoints"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["IdealBurnup"]);
			Assert.AreEqual(0M, (decimal)reportData.Tables["Burnup"].Rows[0]["ActualBurnup"]);
			//Assert.IsTrue(DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]) >= DateTime.UtcNow.Date.AddDays(-39) && DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]) <= DateTime.UtcNow.Date.AddDays(-35), "Actual date: " + DateTime.Parse((string)reportData.Tables["Burnup"].Rows[1]["XAxis"]));
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[0]["CompletedPoints"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[0]["IdealBurnup"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burnup"].Rows[0]["ActualBurnup"] >= 0M);
		}

		[Test, SpiraTestCase(1314)]
		public void _16_RequirementBurndown()
		{
			//Next test the requirement burndown graph data

			//For the project as a whole
			DataSet reportData = graphManager.Requirement_RetrieveBurnDown(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Release", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0", (string)reportData.Tables["Burndown"].Rows[0]["XAxis"]);
			Assert.AreEqual(9.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["CompletedPoints"], 1));
			Assert.AreEqual(18.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["IdealBurndown"], 1));
			Assert.AreEqual(18.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["RemainingPoints"], 1));
			Assert.AreEqual("1.2.0.0", (string)reportData.Tables["Burndown"].Rows[3]["XAxis"]);
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["CompletedPoints"], 1));
			Assert.AreEqual(4.6M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["IdealBurndown"], 1));
			Assert.AreEqual(0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[3]["RemainingPoints"], 1));

			//For a specific release
			reportData = graphManager.Requirement_RetrieveBurnDown(PROJECT_ID, RELEASE_ID, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Sprint", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			Assert.AreEqual("1.0.0.0.0001", (string)reportData.Tables["Burndown"].Rows[0]["XAxis"]);
			Assert.AreEqual(4.0M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["CompletedPoints"], 1));
			Assert.AreEqual(9.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["IdealBurndown"], 1));
			Assert.AreEqual(9.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[0]["RemainingPoints"], 1));
			Assert.AreEqual("1.0.0.0.0003", (string)reportData.Tables["Burndown"].Rows[2]["XAxis"]);
			Assert.AreEqual(1.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[2]["CompletedPoints"], 1));
			Assert.AreEqual(3.2M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[2]["IdealBurndown"], 1));
			Assert.AreEqual(1.5M, Decimal.Round((decimal)reportData.Tables["Burndown"].Rows[2]["RemainingPoints"], 1));

			//For a specific iteration
			reportData = graphManager.Requirement_RetrieveBurnDown(PROJECT_ID, SPRINT_ID1, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Date", reportData.Tables["Burndown"].Columns["XAxis"].Caption);
			//Assert.IsTrue(DateTime.Parse((string)reportData.Tables["Burndown"].Rows[0]["XAxis"]) >= DateTime.UtcNow.Date.AddDays(-39) && DateTime.Parse((string)reportData.Tables["Burndown"].Rows[0]["XAxis"]) <= DateTime.UtcNow.Date.AddDays(-35), "Actual date: " + DateTime.Parse((string)reportData.Tables["Burndown"].Rows[0]["XAxis"]));
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["CompletedPoints"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["IdealBurndown"] >= 0M);
			Assert.IsTrue((decimal)reportData.Tables["Burndown"].Rows[0]["RemainingPoints"] >= 0M);
		}

		[Test, SpiraTestCase(1607)]
		public void _17_IncidentCountByStatus()
		{
			//Get the incident cumulative count by status for all incident types
			//First Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			System.Data.DataSet summaryDataSet = graphManager.RetrieveIncidentCountByStatus(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentCount"].Rows[26]["1"]);
			Assert.AreEqual(15, summaryDataSet.Tables["IncidentCount"].Rows[26]["2"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentCountByStatus(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentCount"].Rows[50]["1"]);
			//Adds a range to handle potential counting issues due to how the 
			Assert.IsTrue((int)summaryDataSet.Tables["IncidentCount"].Rows[50]["2"] >= 8);
			//Assert.IsTrue((int)summaryDataSet.Tables["IncidentCount"].Rows[50]["2"] <= 9);

			//Get the incident cumulative count by status for just bugs
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentCountByStatus(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentCount"].Rows[26]["1"]);
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentCount"].Rows[26]["2"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveIncidentCountByStatus(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, null, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(52, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentCount"].Rows[50]["1"]);
			Assert.IsTrue((int)summaryDataSet.Tables["IncidentCount"].Rows[50]["2"] >= 2);
			Assert.IsTrue((int)summaryDataSet.Tables["IncidentCount"].Rows[50]["2"] <= 3);

			//Get the incident cumulative count by status for just bugs for a specific release
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveIncidentCountByStatus(PROJECT_ID, PROJECT_TEMPLATE_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, INCIDENT_TYPE_ID_BUG);
			Assert.AreEqual(31, summaryDataSet.Tables["IncidentCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["IncidentCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentCount"].Rows[26]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentCount"].Rows[26]["2"]);
		}

		[Test, SpiraTestCase(1608)]
		public void _18_TestCaseProgress()
		{
			//Get the test case cumulative count by status for the entire project
			//First Daily
			DateRange dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			System.Data.DataSet summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatusCumulative(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(3, summaryDataSet.Tables["TestCaseCount"].Rows[26]["1"]);
			Assert.AreEqual(4, summaryDataSet.Tables["TestCaseCount"].Rows[26]["2"]);
			//Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["3"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatusCumulative(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(1, summaryDataSet.Tables["TestCaseCount"].Rows[50]["1"]);
			Assert.AreEqual(7, summaryDataSet.Tables["TestCaseCount"].Rows[50]["2"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[50]["3"]);

			//Get the test case cumulative count by status for a specific release
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatusCumulative(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, null);
			Assert.AreEqual(31, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			//Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(2, summaryDataSet.Tables["TestCaseCount"].Rows[26]["1"]);
			Assert.AreEqual(2, summaryDataSet.Tables["TestCaseCount"].Rows[26]["2"]);
			Assert.AreEqual(3, summaryDataSet.Tables["TestCaseCount"].Rows[26]["3"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatusCumulative(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, null);
			Assert.AreEqual(52, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(2, summaryDataSet.Tables["TestCaseCount"].Rows[50]["1"]);
			Assert.AreEqual(2, summaryDataSet.Tables["TestCaseCount"].Rows[50]["2"]);
			Assert.AreEqual(3, summaryDataSet.Tables["TestCaseCount"].Rows[50]["3"]);

			//Get the test case net count by status for the entire project
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(31, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["2"]);
			Assert.AreEqual(9, summaryDataSet.Tables["TestCaseCount"].Rows[26]["3"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(52, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.IsTrue((int)summaryDataSet.Tables["TestCaseCount"].Rows[50]["1"] >= 0);
			//Assert.IsTrue((int)summaryDataSet.Tables["TestCaseCount"].Rows[50]["1"] <= 1);
			Assert.IsTrue((int)summaryDataSet.Tables["TestCaseCount"].Rows[50]["2"] >= 0);
			//Assert.IsTrue((int)summaryDataSet.Tables["TestCaseCount"].Rows[50]["2"] <= 1);
			Assert.AreEqual(9, summaryDataSet.Tables["TestCaseCount"].Rows[50]["3"]);

			//Get the test case net count by status for a specific release
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, null);
			Assert.AreEqual(31, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["2"]);
			Assert.AreEqual(8, summaryDataSet.Tables["TestCaseCount"].Rows[26]["3"]);

			//Next Weekly
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddYears(-1);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Weekly, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, null);
			Assert.AreEqual(52, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] > DateTime.UtcNow.Date.AddDays(-20) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[50]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[50]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[50]["2"]);
			Assert.AreEqual(8, summaryDataSet.Tables["TestCaseCount"].Rows[50]["3"]);

			//Get the test case net count by status for a specific release and test case type
			//First Daily
			dateRange = new DateRange();
			dateRange.EndDate = DateTime.UtcNow.Date;
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-30);
			summaryDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(PROJECT_ID, Graph.ReportingIntervalEnum.Daily, dateRange, InternalRoutines.UTC_OFFSET, RELEASE_ID, TEST_CASE_TYPE_ID_FUNCTIONAL);
			Assert.AreEqual(31, summaryDataSet.Tables["TestCaseCount"].Rows.Count);
			Assert.IsTrue((DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] > DateTime.UtcNow.Date.AddDays(-8) && (DateTime)summaryDataSet.Tables["TestCaseCount"].Rows[26]["Date"] < DateTime.UtcNow.Date);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["1"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["2"]);
			Assert.AreEqual(0, summaryDataSet.Tables["TestCaseCount"].Rows[26]["3"]);
		}

		/// <summary>
		/// Tests that we can create, edit and delete the custom graph definitions
		/// </summary>
		[Test, SpiraTestCase(1641)]
		public void _19_CreateEditCustomGraphs()
		{
			//First verify that we have two existing active custom graphs
			List<GraphCustom> graphs = graphManager.GraphCustom_Retrieve();
			//Assert.AreEqual(2, graphs.Count);
			//Assert.AreEqual("Test Graph 1", graphs[0].Name);
			//Assert.AreEqual("Test Graph 2", graphs[1].Name);

			//Now lets create a new custom graph
			string query = @"select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId}
group by R.EXECUTION_STATUS_NAME";
			string query2 = @"select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT, (COUNT (R.TEST_RUN_ID) * 2) as COUNT2
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId}
group by R.EXECUTION_STATUS_NAME";
			int graphCustomId1 = graphManager.GraphCustom_Create("Test Graph 3", query, true, null, null);

			//Verify that it was created
			GraphCustom graph = graphManager.GraphCustom_RetrieveById(graphCustomId1);
			Assert.IsNotNull(graph);
			Assert.AreEqual("Test Graph 3", graph.Name);
			Assert.IsNull(graph.Description);
			Assert.AreEqual(query, graph.Query);
			Assert.AreEqual(true, graph.IsActive);
			Assert.AreEqual(1, graph.Position);

			//Clone the graph
			int graphCustomId2 = graphManager.GraphCustom_Clone(graphCustomId1);

			//Verify the clone was created
			graph = graphManager.GraphCustom_RetrieveById(graphCustomId2);
			Assert.IsNotNull(graph);
			Assert.AreEqual("Test Graph 3 - Copy", graph.Name);
			Assert.IsNull(graph.Description);
			Assert.AreEqual(query, graph.Query);
			Assert.AreEqual(true, graph.IsActive);
			Assert.AreEqual(2, graph.Position);

			//Verify the list
			graphs = graphManager.GraphCustom_Retrieve();
			Assert.AreEqual(2, graphs.Count);
			//Assert.AreEqual("Test Graph 1", graphs[0].Name);
			//Assert.AreEqual("Test Graph 2", graphs[1].Name);
			Assert.AreEqual("Test Graph 3", graphs[0].Name);
			Assert.AreEqual("Test Graph 3 - Copy", graphs[1].Name);

			//Modify the clone and make it inactive
			graph.StartTracking();
			graph.Name = "Test Graph 4";
			graph.Query = query2;
			graph.IsActive = false;
			graph.Description = "Test Graph 4 is awesome.";
			graphManager.GraphCustom_Update(graph);

			//Verify the changes
			graph = graphManager.GraphCustom_RetrieveById(graphCustomId2);
			Assert.IsNotNull(graph);
			Assert.AreEqual("Test Graph 4", graph.Name);
			Assert.AreEqual("Test Graph 4 is awesome.", graph.Description);
			Assert.AreEqual(query2, graph.Query);
			Assert.AreEqual(false, graph.IsActive);
			Assert.AreEqual(2, graph.Position);

			//Verify the list
			graphs = graphManager.GraphCustom_Retrieve();
			Assert.AreEqual(1, graphs.Count);
			//Assert.AreEqual("Test Graph 1", graphs[0].Name);
			//Assert.AreEqual("Test Graph 2", graphs[1].Name);
			Assert.AreEqual("Test Graph 3", graphs[0].Name);

			//Verify the list
			graphs = graphManager.GraphCustom_Retrieve(false);
			Assert.AreEqual(2, graphs.Count);
			//Assert.AreEqual("Test Graph 1", graphs[0].Name);
			//Assert.AreEqual("Test Graph 2", graphs[1].Name);
			Assert.AreEqual("Test Graph 3", graphs[0].Name);
			Assert.AreEqual("Test Graph 4", graphs[1].Name);

			//Clean up by deleting the graphs
			graphManager.GraphCustom_Delete(graphCustomId1, 1);
			graphManager.GraphCustom_Delete(graphCustomId2, 1);

			//Verify deletes
			graphs = graphManager.GraphCustom_Retrieve();
			Assert.AreEqual(0, graphs.Count);
			//Assert.AreEqual("Test Graph 1", graphs[0].Name);
			//Assert.AreEqual("Test Graph 2", graphs[1].Name);
		}

		/// <summary>
		/// Tests that we can actually execute the custom graph ESQL and get data back, both as an administrator
		/// previewing the results and and end user, viewing the published reports.
		/// </summary>
		[Test, SpiraTestCase(1642)]
		public void _20_DisplayCustomGraphs()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Edit Graphs";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Now lets create a new custom graph with a valid query for 2 data ranges
			string query = @"select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT, (COUNT (R.TEST_RUN_ID) * 2) as COUNT2
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId}
group by R.EXECUTION_STATUS_NAME";

			int graphCustomId1 = graphManager.GraphCustom_Create("Test Graph 3", query, true, null, null, 1, adminSectionId, "Inserted Graph");

			//Next verify that we can run this query in preview mode
			DataTable previewData = graphManager.GraphCustom_ExecuteSQL(PROJECT_ID, 0, query);
			Assert.AreEqual(3, previewData.Columns.Count);
			Assert.AreEqual("EXECUTION_STATUS_NAME", previewData.Columns[0].ColumnName);
			Assert.AreEqual("COUNT", previewData.Columns[1].ColumnName);
			Assert.AreEqual("COUNT2", previewData.Columns[2].ColumnName);
			Assert.AreEqual(4, previewData.Rows.Count);

			//Next verify that we can run this query in end-user mode
			DataTable actualData = graphManager.GraphCustom_ExecuteSQL(PROJECT_ID, 0, graphCustomId1);
			Assert.AreEqual(3, actualData.Columns.Count);
			Assert.AreEqual("EXECUTION_STATUS_NAME", actualData.Columns[0].ColumnName);
			Assert.AreEqual("COUNT", actualData.Columns[1].ColumnName);
			Assert.AreEqual("COUNT2", actualData.Columns[2].ColumnName);
			Assert.AreEqual(4, previewData.Rows.Count);

			//Clean up by deleting the graphs
			graphManager.GraphCustom_Delete(graphCustomId1, 1);

			//Now lets create a new custom graph with a valid query for 2 data ranges filtered by release
			query = @"select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT, (COUNT (R.TEST_RUN_ID) * 2) as COUNT2
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId} and R.RELEASE_ID = ${ReleaseId} and R.RELEASE_ID in {${ReleaseAndChildIds}}
group by R.EXECUTION_STATUS_NAME";

			int graphCustomId2 = graphManager.GraphCustom_Create("Test Graph 4", query, true, null, null, 1, adminSectionId, "Inserted Graph");

			//Next verify that we can run this query in preview mode
			previewData = graphManager.GraphCustom_ExecuteSQL(PROJECT_ID, 0, query, RELEASE_ID);
			Assert.AreEqual(3, previewData.Columns.Count);
			Assert.AreEqual("EXECUTION_STATUS_NAME", previewData.Columns[0].ColumnName);
			Assert.AreEqual("COUNT", previewData.Columns[1].ColumnName);
			Assert.AreEqual("COUNT2", previewData.Columns[2].ColumnName);
			Assert.AreEqual(3, previewData.Rows.Count);

			//Next verify that we can run this query in end-user mode
			actualData = graphManager.GraphCustom_ExecuteSQL(PROJECT_ID, 0, graphCustomId2, RELEASE_ID);
			Assert.AreEqual(3, actualData.Columns.Count);
			Assert.AreEqual("EXECUTION_STATUS_NAME", actualData.Columns[0].ColumnName);
			Assert.AreEqual("COUNT", actualData.Columns[1].ColumnName);
			Assert.AreEqual("COUNT2", actualData.Columns[2].ColumnName);
			Assert.AreEqual(3, previewData.Rows.Count);

			//Clean up by deleting the graphs
			graphManager.GraphCustom_Delete(graphCustomId2, 1);
		}
	}
}
