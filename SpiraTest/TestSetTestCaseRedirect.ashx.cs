using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using System.Threading;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Accepts a TestSetTestCase instance ID and redirects to the appropriate test case.
    /// </summary>
    public class TestSetTestCaseRedirect : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestSetTestCaseRedirect::";

        /// <summary>
        /// Does the redirect
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);

            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the test set test case id from the querystring
                if (!String.IsNullOrEmpty(context.Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_TEST_CASE_ID]))
                {
                    int testSetTestCaseId;
                    if (Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_TEST_CASE_ID], out testSetTestCaseId))
                    {
                        //Retrieve the test case
                        TestSetManager testSetManager = new TestSetManager();
                        try
                        {
                            TestSetTestCaseView testSetTestCase = testSetManager.RetrieveTestCaseById2(testSetTestCaseId);
                            //Make sure the project's match
                            if (testSetTestCase.ProjectId == ProjectId)
                            {
                                int testCaseId = testSetTestCase.TestCaseId;
                                context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(Common.UrlRoots.NavigationLinkEnum.TestCases, ProjectId, testCaseId));
                            }
                            else
                            {
                                Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "The test case's project id doesn't match the current project");
                            }
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Do nothing
                            Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to redirect to test case that has TXTC = " + testSetTestCaseId);
                        }

                    }
                    else
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to redirect to test case that has a non-numeric id");
                    }
                }
                else
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to redirect to test case that has a null id");
                }
            }
            catch (ThreadAbortException)
            {
                //Don't log thread aborted exceptions
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}