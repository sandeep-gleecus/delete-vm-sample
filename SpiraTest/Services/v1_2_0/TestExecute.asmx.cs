using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v1_2_0
{
	/// <summary>
	/// This class provides the functionality for executing a test case from an external application
	/// and having the test results populate SpiraTest. This is used to integrate with external
	/// automated testing tools
	/// </summary>
	/// <remarks>
	/// The namespace includes a versioning compatibility parameter. This version number
	/// only changes when the API changes, so it may be lower that the current application version
	/// </remarks>
	[
	WebService
		(
		Namespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/",
		Description="This class provides the functionality for executing a test case from an external application and having the test results populate SpiraTest."
		)
	]
	public class TestExecute : WebServiceBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Execute::";

	    /// <summary>
		/// Records the results of executing an automated test
		/// </summary>
		/// <param name="userName">The username of the user</param>
		/// <param name="password">The unhashed password of the user</param>
		/// <param name="projectId">The project to connect to</param>
		/// <param name="testerUserId">The user id of the person who's running the test (-1 for logged in user)</param>
		/// <param name="testCaseId">The test case being executed</param>
		/// <param name="releaseId">The release being executed against</param>
		/// <param name="executionStatusId">The status of the test run (pass/fail/not run)</param>
		/// <param name="runnerName">The name of the automated testing tool</param>
		/// <param name="runnerAssertCount">The number of assertions</param>
		/// <param name="runnerMessage">The failure message (if appropriate)</param>
		/// <param name="runnerStackTrace">The error stack trace (if any)s</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <returns>The newly created test run id</returns>
		/// <remarks>Use this version of the method for clients that cannot handle session cookies</remarks>
		[
		WebMethod
			(
			Description="Records the results of executing an automated test, use this version when client cannot handle session cookies",
			EnableSession=true
			)
		]
		public int RecordTestRun2 (string userName, string password, int projectId, int testerUserId, int testCaseId, int releaseId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace)
		{
			const string METHOD_NAME = "RecordTestRun2";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//Delegate to the three existing methods
			int testRunId = -1;
			if (this.Authenticate (userName, password))
			{
				if (this.ConnectToProject (projectId))
				{
                    //Make sure we have permissions to create test runs
                    if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
                    {
                        //Throw back an exception
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();

                        throw new SoapException("Not Authorized to Create Test Runs",
                            SoapException.ClientFaultCode,
                            Context.Request.Url.AbsoluteUri);
                    }

                    //Convert any dates from localtime to UTC
                    startDate = GlobalFunctions.UniversalizeDate(startDate);
                    endDate = GlobalFunctions.UniversalizeDate(endDate);

					testRunId = this.RecordTestRun (testerUserId, testCaseId, releaseId, startDate, endDate, executionStatusId, runnerName, runnerTestName, runnerAssertCount, runnerMessage, runnerStackTrace);
				}
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return testRunId;
		}

		/// <summary>
		/// Records the results of executing an automated test
		/// </summary>
		/// <param name="testerUserId">The user id of the person who's running the test (-1 for logged in user)</param>
		/// <param name="testCaseId">The test case being executed</param>
		/// <param name="releaseId">The release being executed against</param>
		/// <param name="executionStatusId">The status of the test run (pass/fail/not run)</param>
		/// <param name="runnerName">The name of the automated testing tool</param>
		/// <param name="runnerAssertCount">The number of assertions</param>
		/// <param name="runnerMessage">The failure message (if appropriate)</param>
		/// <param name="runnerStackTrace">The error stack trace (if any)s</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <returns>The newly created test run id</returns>
		[
		WebMethod
			(
			Description="Records the results of executing an automated test",
			EnableSession=true
			)
		]
		public int RecordTestRun (int testerUserId, int testCaseId, int releaseId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace)
		{
			const string METHOD_NAME = "RecordTestRun";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (Session[Session_Authenticated] == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Session Not Authenticated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int userId = (int)Session[Session_Authenticated];
			
			//Make sure we are connected to a project
			if (Session[Session_Authorized] == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Not Connected to a Project",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			int projectId = (int)Session[Session_Authorized];
			int testRunId = -1;

            //Make sure we have permissions to create test runs
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Runs",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

			try
			{
				//Default to the authenticated user if no tester provided
				if (testerUserId == -1)
				{
					testerUserId = userId;
				}

                //Convert some of the optional fields to the newer nullable types supported in .NET 2.0
                Nullable<int> nullableReleaseId = null;
                if (releaseId != -1)
                {
                    nullableReleaseId = releaseId;
                }

                //Convert any dates from localtime to UTC
                startDate = GlobalFunctions.UniversalizeDate(startDate);
                endDate = GlobalFunctions.UniversalizeDate(endDate);

				//Now record the test run
				TestRunManager testRunManager = new TestRunManager();
                testRunId = testRunManager.Record(projectId,
                    testerUserId,
                    testCaseId,
                    nullableReleaseId,
                    null,
                    null,
                    startDate,
                    endDate,
                    executionStatusId,
                    (String.IsNullOrEmpty(runnerName)) ? null : runnerName,
                    (String.IsNullOrEmpty(runnerTestName)) ? null : runnerTestName,
                    (runnerAssertCount == -1) ? null : (int?) runnerAssertCount,
                    (String.IsNullOrEmpty(runnerMessage)) ? null : runnerMessage,
                    (String.IsNullOrEmpty(runnerStackTrace)) ? null : runnerStackTrace,
                    null,
                    null,
                    null,
                    TestRun.TestRunFormatEnum.PlainText,
                    null);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent (CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return testRunId;
		}
	}
}
