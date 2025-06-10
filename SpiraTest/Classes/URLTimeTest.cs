#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Inflectra.SpiraTest.Web.Classes
{
	/// <summary>This class does nothing but run some hard-hitting tests on the URL Rewriter.</summary>
	public static class URLTimeTest
	{
		/// <summary>How many time to loo pthrough ALL the URLs.</summary>
		private const int NUM_RUNS = 100;

		/// <summary>The URLs to translate.</summary>
		private static List<string> URLs = new List<string>()
		{
			"/6/TestConfiguration/List.aspx",
			"/6/Resource/43.aspx",
			"/6/TestConfiguration/2.aspx",
			"/SourceCodeRevisionFileDetails.aspx?projectId=6&sourceCodeRevisionKey=401e3a1395189b50bb026602bfa68d637869d49e&sourceCodeFileKey=.nuget/packages.config",
			"/Administration/UserList.aspx",
			"/Administration/ProjectList.aspx",
			"/6/Risk/New.aspx",
			"/6/TestConfiguration/New.aspx",
			"/administration/emailconfiguration.aspx",
			"/6/Document/List/40.aspx",
			"/6/TestExecute/1526.aspx?referrerTestCaseList=true",
			"/6/Release/345.aspx?referrerProjectList=true",
			"/6/TestRun/1132799.aspx?",
			"/6/Task/Board.aspx",
			"/6/TestRun/1137229.aspx",
			"/6/Resource/List.aspx",
			"/6/Requirement/361.aspx",
			"/6/Requirement/Map.aspx",
			"/6/MyPage.aspx",
			"/Administration/ProjectTemplateList.aspx",
			"/6/TestSet/List/19.aspx",
			"/6/TestCase/List.aspx",
			"/Administration/ProgramList.aspx",
			"/6/Test.aspx",
			"/6/Release/14.aspx",
			"/6/Task/91.aspx",
			"/6/Document/List/5.aspx",
			"/6/TestCase/141.aspx",
			"/6/Incident/5897.aspx",
			"/6/TestSet/48.aspx",
			"/Administration/ActiveSessions.aspx",
			"/Administration/LoginProviders.aspx",
			"/6/TestRun/List.aspx",
			"/SourceCodeRevisionDetails.aspx?projectId=6&sourceCodeRevisionKey=401e3a1395189b50bb026602bfa68d637869d49e",
			"/6/TestSet/List.aspx",
			"/Administration/PortfolioList.aspx",
			"/6/Risk/List.aspx#",
			"/Administration/UserRequests.aspx",
			"/6/AttachmentVersion/772.aspx",
			"/6/Risk/6.aspx",
			"/pg/1/Dev.aspx",
			"/6/TestRun/11174.aspx?releaseId=14",
			"/6/AutomationHost/List.asp",
			"/6/Document/748.aspx",
			"/6/TestCase/2728.aspx",
			"/6/Requirement/Document.aspx",
			"/Administration/RoleList.aspx",
			"/6/PlanningBoard.aspx",
			"/6/Task/Gantt.aspx",
			"/SourceCodeFileDetails.aspx?projectId=6&sourceCodeFileKey=.nuget%2fpackages.config",
			"/6/Incident/New.aspx",
			"/6/Release/Tree.aspx",
			"/6/SourceCodeRevision/List.aspx",
			"/6/TestRun/1137914.aspx?releaseId=345",
			"/Administration/LdapConfiguration.aspx",
			"/6/Requirement/Tree.aspx",
			"/6/SourceCode/List.aspx",
			"/6/AutomationHost/5.aspx",
			"/6/Report/List.aspx",
			"/6/Task/Table.aspx",
		};

		private static List<long> RecordedTimes;

		public static string RunTest(string localHost, Services.Ajax.DataObjects.ProcessStatus reportStatus)
		{
			int totalNum = URLs.Count * NUM_RUNS;

			//The string to return for summarizing.
			string retValue =
				"Going through " +
				URLs.Count.ToString() +
				" URLs, for a total of " +
				NUM_RUNS.ToString() +
				" times. (" +
				totalNum.ToString() +
				" total tests):" +
				Environment.NewLine;

			retValue += "Stopwatch is" + (Stopwatch.IsHighResolution ? "" : " not") + " high precision." + Environment.NewLine + Environment.NewLine;

			//Our collection of times.
			RecordedTimes = new List<long>();

			//Create the client.
			HttpClient hClient = new HttpClient();
			hClient.BaseAddress = new Uri(localHost);

			// The overall watch for the while function.
			var watch1 = Stopwatch.StartNew();

			// We loop through the entire list a number of times. This helps equalize out not only all the requests,
			//  BUT, helps even out slight deviations in calls for the SAME url that may occur.
			for (int i = 0; i < NUM_RUNS; i++)
			{
				//Update message.
				reportStatus.Message = "Loop " + (i + 1).ToString() + "/" + NUM_RUNS.ToString() + "...";

				//Start timer.
				var watch2 = Stopwatch.StartNew();

				//Loop.
				foreach (string url in URLs)
				{
					//Update progress.
					reportStatus.Progress = ((i + 1) / totalNum) * 100;

					//Generate the full URL.
					Uri toPing = new Uri(localHost + url);

					//Start timer.
					var watch3 = Stopwatch.StartNew();
					//Run request.
					var run = hClient
						.GetAsync(toPing);
					run.Wait();
					//Stop timer.
					watch3.Stop();

					//Get the result.
					var result = run.Result;
					Debug.WriteLine("ERROR: Request returned: " + result.StatusCode.ToString());

					//Add the time.
					RecordedTimes.Add(watch3.ElapsedMilliseconds);
				}
				//Stop timer.
				watch2.Stop();

				//Output status.
				retValue += "Loop #" + (i + 1).ToString() + " done in " + watch2.ElapsedMilliseconds.ToString() + "ms." + Environment.NewLine;
			}
			//Stop the watch.
			watch1.Stop();

			//Write out report...
			double masterAvg = ((double)watch1.ElapsedMilliseconds / totalNum);
			double eachAvg = ((double)RecordedTimes.Sum() / totalNum);

			retValue += string.Format(
					Environment.NewLine +
					"<strong><u>Results:</u></strong>" +
					Environment.NewLine +
					"<em>Master/Indv Average</em>: {0:F4} ms. / {1:F4} ms." +
					Environment.NewLine +
					"<em>Master/Indv Total</em>: {2} ms. / {3} ms." + Environment.NewLine,
				masterAvg,
				eachAvg,
				watch1.ElapsedMilliseconds,
				RecordedTimes.Sum());

			//And send it back.
			return retValue;
		}
	}
}
#endif
