using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using System.ComponentModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    public partial class PendingTestRuns : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.PendingTestRuns::";

        protected IWebPartReloadable testCasesReloadable;
        protected IWebPartReloadable testSetsReloadable;
        bool reloadConnections = false;

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 10
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(10)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                this.rowsToDisplay = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 10;

        #endregion


        /// <summary>
        /// Makes the connection to the test cases webpart
        /// </summary>
        /// <param name="webPartReloadable"></param>
        [ConnectionConsumer("ReloadTestCasesConsumer", "ReloadTestCasesConsumer")]
        public void SetReloadTestCasesConsumer(IWebPartReloadable webPartReloadable)
        {
            this.testCasesReloadable = webPartReloadable;
        }

        /// <summary>
        /// Makes the connection to the test sets webpart
        /// </summary>
        /// <param name="webPartReloadable"></param>
        [ConnectionConsumer("ReloadTestSetsConsumer", "ReloadTestSetsConsumer")]
        public void SetReloadTestSetsConsumer(IWebPartReloadable webPartReloadable)
        {
            this.testSetsReloadable = webPartReloadable;
        }

        /// <summary>
        /// If we need to reload any connected webparts, need to do it here
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (reloadConnections)
            {
                if (this.testCasesReloadable != null)
                {
                    this.testCasesReloadable.LoadAndBindData();
                }
                if (this.testSetsReloadable != null)
                {
                    this.testSetsReloadable.LoadAndBindData();
                }
            }
        }

        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
                //Register event handlers
                this.grdSavedTestRuns.RowCommand += new GridViewCommandEventHandler(grdSavedTestRuns_RowCommand);
                this.grdSavedTestRuns.RowDataBound += new GridViewRowEventHandler(grdSavedTestRuns_RowDataBound);

                //Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }

        /// <summary>
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
        public IWebPartReloadable GetReloadable()
        {
            return this;
        }

        /// <summary>
        /// Loads and binds the data
        /// </summary>
        public void LoadAndBindData()
        {
            //Get all the pending test runs currently open in the project
            TestRunManager testRunManager = new TestRunManager();
            List<TestRunsPending> pendingTestRuns = testRunManager.RetrievePending(ProjectId);

            //Limit the number of rows returned
            IEnumerable<TestRunsPending> testRunsSubset = pendingTestRuns.Take(this.rowsToDisplay);
            grdSavedTestRuns.DataSource = testRunsSubset;
            this.grdSavedTestRuns.DataBind();

            //Databind the empty user list
            this.ddlAssignee.DataBind();
        }

        /// <summary>
        /// This event handler handles any button click inside the datagrid
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The parameters passed to handler</param>
        private void grdSavedTestRuns_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdSavedTestRuns_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to get the pending test run from the command argument
                int testRunsPendingId = Int32.Parse((string)e.CommandArgument);
                TestRunManager testRunManager = new TestRunManager();

                //Identify which command was executed and act accordingly
                if (e.CommandName == "DeletePending")
                {
                    //Mark the pending test run as complete
                    testRunManager.CompletePending(testRunsPendingId, UserId);

                    //Next we need to refresh the saved run dataset and test set / case webparts
                    LoadAndBindData();
                    this.reloadConnections = true;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (System.Threading.ThreadAbortException exception)
            {
                //These are due to response.redirects and are not true error, so log as informational only
                Logger.LogInformationalEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }

        /// <summary>
        /// Applies selective formatting to the pending test run list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdSavedTestRuns_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdSavedTestRuns_RowCommand"; 
            
            try
            {
                //Don't touch headers, footers or subheaders
                if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
                {
                    TestRunsPending testRunsPending = ((TestRunsPending)(e.Row.DataItem));
                    if (testRunsPending != null)
                    {
                        //Populate the hyperlink url to resume
                        HyperLinkEx btnResume = (HyperLinkEx)e.Row.Cells[4].FindControl("btnResume");
                        if (btnResume != null)
                        {
                            //set the initial value of the base url to use
                            UrlRoots.NavigationLinkEnum ulrNavigationLinkEnum = UrlRoots.NavigationLinkEnum.TestExecute;

                            //CHECKS TO SEE IF THIS SHOULD BE RUN AS EXPLORATORY
                            //Make sure user has sufficient permissions to execute exploratory test cases as exploratory
                            ProjectManager projectManager = new ProjectManager();
                            ProjectUserView projectRole = projectManager.RetrieveUserMembershipById(ProjectId, this.UserId);


                            bool canExecuteExploratory = projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Create) != Project.AuthorizationState.Prohibited;

                            //Make sure the test case is of type exploratory
                            if (canExecuteExploratory)
                            {
                                TestRunManager testRunManager = new TestRunManager();
                                TestRunsPending testRunsPendingFull = new TestRunsPending();
                                testRunsPendingFull = testRunManager.RetrievePendingById(testRunsPending.TestRunsPendingId, true);

                                bool hasOnlyOneCase = testRunsPendingFull.TestRuns.Count == 1;

                                if (hasOnlyOneCase)
                                {
                                    try
                                    {
                                        TestCaseManager testCaseManager = new TestCaseManager();
                                        bool isOfTypeExploratory = testCaseManager.RetrieveById2(ProjectId, testRunsPendingFull.TestRuns[0].TestCaseId, true,true).Type.IsExploratory;

                                        if (isOfTypeExploratory)
                                        {
                                            //update the navigation link to the exploratory url
                                            ulrNavigationLinkEnum = UrlRoots.NavigationLinkEnum.TestExecuteExploratory;
                                        }
                                    }
                                    catch (ArtifactNotExistsException)
                                    {
                                        //Fail quietly
                                    }
                                }
                            }


                            //now set the actual url - either as exploratory or not
                            btnResume.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(ulrNavigationLinkEnum, ProjectId, testRunsPending.TestRunsPendingId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                        }

                        //Calculate the information to display in the progress column
                        int totalCount = testRunsPending.CountPassed + testRunsPending.CountFailed + testRunsPending.CountBlocked + testRunsPending.CountCaution + testRunsPending.CountNotRun + testRunsPending.CountNotApplicable;
                        int percentGreen = 0;
                        int percentRed = 0;
                        int percentYellow = 0;
                        int percentOrange = 0;
                        int percentGray = 0;
                        int percentDarkGray = 0;
                        //Need check to handle divide by zero case
                        if (totalCount != 0)
                        {
                            percentGreen = (int)Decimal.Round(((decimal)testRunsPending.CountPassed * (decimal)100) / (decimal)totalCount, 0);
                            percentRed = (int)Decimal.Round(((decimal)testRunsPending.CountFailed * (decimal)100) / (decimal)totalCount, 0);
                            percentOrange = (int)Decimal.Round(((decimal)testRunsPending.CountCaution * (decimal)100) / (decimal)totalCount, 0);
                            percentYellow = (int)Decimal.Round(((decimal)testRunsPending.CountBlocked * (decimal)100) / (decimal)totalCount, 0);
                            percentGray = (int)Decimal.Round(((decimal)testRunsPending.CountNotRun * (decimal)100) / (decimal)totalCount, 0);
                            percentDarkGray = (int)Decimal.Round(((decimal)testRunsPending.CountNotApplicable * (decimal)100) / (decimal)totalCount, 0);
                        }

                        //Create the tooltip text
                        string tooltipText = "# " + Resources.Fields.Passed + "=" + testRunsPending.CountPassed.ToString() + ", # " + Resources.Fields.Failed + "=" + testRunsPending.CountFailed.ToString() + ", # " + Resources.Fields.Caution + "=" + testRunsPending.CountCaution.ToString() + ", # " + Resources.Fields.Blocked + "=" + testRunsPending.CountBlocked.ToString() + ", # " + Resources.Fields.NotRun + "=" + testRunsPending.CountNotRun.ToString() + ", # " + Resources.Fields.NotApplicable + "=" + testRunsPending.CountNotApplicable.ToString();

                        //Now populate the equalizer graph
                        Equalizer eqlProgress = (Equalizer)e.Row.Cells[3].FindControl("eqlProgress");
                        if (eqlProgress != null)
                        {
                            eqlProgress.PercentGreen = percentGreen;
                            eqlProgress.PercentRed = percentRed;
                            eqlProgress.PercentYellow = percentYellow;
                            eqlProgress.PercentOrange = percentOrange;
                            eqlProgress.PercentGray = percentGray;
                            eqlProgress.PercentDarkGray = percentDarkGray;
                        }

                        //Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                        e.Row.Cells[3].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
                        e.Row.Cells[3].Attributes["onMouseOut"] = "hideddrivetip();";
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }
    }
}
