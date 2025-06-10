using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
    public partial class ProjectTestSummary : WebPartBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectTestSummary::";

        #region User Configurable Properties

        /// <summary>
        /// Should we display only the data related to active releases in the projects
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("ProjectGroupHome_ActiveReleasesOnlySetting"),
        LocalizedWebDescription("ProjectGroupHome_ActiveReleasesOnlySettingTooltip"),
        DefaultValue(true)
        ]
        public bool ActiveReleasesOnly
        {
            get
            {
                return this.activeReleasesOnly;
            }
            set
            {
                this.activeReleasesOnly = value;
                LoadAndBindData();
            }
        }
        protected bool activeReleasesOnly = true;

        #endregion

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
                this.grdProjectExecutionStatus.RowCreated += new GridViewRowEventHandler(grdProjectExecutionStatus_RowCreated);

                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectExecutionStatus.Columns[1];
                if (field != null)
                {
                    field.NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, -3);
                }

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
        /// Gets the detailed test execution info for each project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdProjectExecutionStatus_RowCreated(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdProjectExecutionStatus_RowCreated";

            try
            {
                //Get the project id
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    ProjectView projectView = (ProjectView)e.Row.DataItem;
                    int projectId = projectView.ProjectId;

                    //Now get the coverage for this project and populate the equalizer
                    Equalizer eqlReqCoverage = (Equalizer)e.Row.Cells[4].FindControl("eqlReqCoverage");
                    RequirementManager requirementManager = new RequirementManager();
                    List<RequirementCoverageSummary> requirementCoverages = requirementManager.RetrieveCoverageSummary(projectId, (activeReleasesOnly) ? (int?)RequirementManager.RELEASE_ID_ACTIVE_RELEASES_ONLY : null, false);
                    double reqCount = 0;
                    foreach (RequirementCoverageSummary requirementCoverage in requirementCoverages)
                    {
                        if (requirementCoverage.CoverageCount.HasValue)
                        {
                            reqCount += requirementCoverage.CoverageCount.Value;
                        }
                    }
                    //Populate the equalizer with the percentages
                    if (eqlReqCoverage != null && reqCount > 0)
                    {
                        string tooltipText = "";
                        foreach (RequirementCoverageSummary requirementCoverage in requirementCoverages)
                        {
                            int coverageStatus = requirementCoverage.CoverageStatusOrder;
                            string coverageCaption = requirementCoverage.CoverageStatus;
                            double coverageCount = requirementCoverage.CoverageCount.Value;
                            int percentage = (int)(coverageCount / reqCount * 100.0D);
                            if (percentage < 0 || percentage > 100)
                            {
                                percentage = 0;
                            }
                            if (tooltipText != "")
                            {
                                tooltipText += ", ";
                            }
                            tooltipText += "# " + coverageCaption + " = " + coverageCount.ToString("0.0");
                            switch (coverageStatus)
                            {
                                case 1:
                                    //Passed
                                    eqlReqCoverage.PercentGreen = percentage;
                                    break;
                                case 2:
                                    //Failed
                                    eqlReqCoverage.PercentRed = percentage;
                                    break;
                                case 3:
                                    //Blocked
                                    eqlReqCoverage.PercentYellow = percentage;
                                    break;
                                case 4:
                                    //Caution
                                    eqlReqCoverage.PercentOrange = percentage;
                                    break;
                                case 5:
                                    //Not Run
                                    eqlReqCoverage.PercentGray = percentage;
                                    break;
                                case 6:
                                    //Not Covered
                                    eqlReqCoverage.PercentBlue = percentage;
                                    break;
                            }
                        }
                        //Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                        e.Row.Cells[4].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
                        e.Row.Cells[4].Attributes["onMouseOut"] = "hideddrivetip();";
                    }

                    //Populate the requirement count cell
                    e.Row.Cells[3].Text = ((int)reqCount).ToString();

                    //Now get the test status for this project and populate the equalizer
                    Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[6].FindControl("eqlExecutionStatus");
                    Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
                    List<TestCase_ExecutionStatusSummary> executionSummary = testCaseManager.RetrieveExecutionStatusSummary(projectId, (activeReleasesOnly) ? (int?)TestCaseManager.RELEASE_ID_ACTIVE_RELEASES_ONLY : null);
                    double testCount = 0;
                    foreach (TestCase_ExecutionStatusSummary executionSummaryRow in executionSummary)
                    {
                        testCount += executionSummaryRow.StatusCount.Value;
                    }
                    //Populate the equalizer with the percentages
                    if (eqlExecutionStatus != null)
                    {
                        string tooltipText = "";
                        foreach (TestCase_ExecutionStatusSummary executionSummaryRow in executionSummary)
                        {
                            int executionStatusId = executionSummaryRow.ExecutionStatusId;
                            string executionStatusName = executionSummaryRow.ExecutionStatusName;
                            int statusCount = executionSummaryRow.StatusCount.Value;
                            int percentage = (int)(((double)statusCount) / testCount * 100.0D);
                            if (tooltipText != "")
                            {
                                tooltipText += ", ";
                            }
                            tooltipText += "# " + executionStatusName + " = " + statusCount.ToString();
                            switch (executionStatusId)
                            {
                                case (int)TestCase.ExecutionStatusEnum.Passed:
                                    eqlExecutionStatus.PercentGreen = percentage;
                                    break;
                                case (int)TestCase.ExecutionStatusEnum.Failed:
                                    eqlExecutionStatus.PercentRed = percentage;
                                    break;
                                case (int)TestCase.ExecutionStatusEnum.Blocked:
                                    eqlExecutionStatus.PercentYellow = percentage;
                                    break;
                                case (int)TestCase.ExecutionStatusEnum.Caution:
                                    eqlExecutionStatus.PercentOrange = percentage;
                                    break;
                                case (int)TestCase.ExecutionStatusEnum.NotRun:
                                    eqlExecutionStatus.PercentGray = percentage;
                                    break;
                                case (int)TestCase.ExecutionStatusEnum.NotApplicable:
                                    eqlExecutionStatus.PercentDarkGray = percentage;
                                    break;
                            }
                        }
                        //Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                        e.Row.Cells[6].Attributes["onMouseOver"] = "ddrivetip('" + GlobalFunctions.JSEncode(tooltipText) + "');";
                        e.Row.Cells[6].Attributes["onMouseOut"] = "hideddrivetip();";
                    }

                    //Populate the test count cells
                    e.Row.Cells[5].Text = ((int)testCount).ToString();

                    IncidentManager incidentManager = new IncidentManager();
                    List<IncidentOpenCountByPrioritySeverity> openCounts = incidentManager.RetrieveOpenCountByPrioritySeverity(projectId, null, false, false);
                    double incidentCount = 0;
                    foreach (IncidentOpenCountByPrioritySeverity openCount in openCounts)
                    {
                        incidentCount += openCount.Count.Value;
                    }
                    
                    //Generate the equalizer with the percentages
                    TableCell tableCell = e.Row.Cells[8];
                    if (tableCell != null)
                    {
                        Label parentControl = new Label();
                        parentControl.CssClass = "Equalizer";
                        tableCell.Controls.Add(parentControl);
                        string tooltipText = "";
                        foreach (IncidentOpenCountByPrioritySeverity openCount in openCounts)
                        {
                            string colorName = "e0e0e0";
                            if (!String.IsNullOrEmpty(openCount.PrioritySeverityColor))
                            {
                                colorName = openCount.PrioritySeverityColor;
                            }
                            string prioritySeverityName = openCount.PrioritySeverityName;
                            int count = openCount.Count.Value;
                            int percentage = (int)(((double)count) / incidentCount * 100.0D);
                            if (tooltipText != "")
                            {
                                tooltipText += ", ";
                            }
                            tooltipText += "[" + prioritySeverityName + "] = " + count.ToString();

                            //Need to add additional tags in the case of Firefox which doesn't support inline SPAN widths
                            bool isFirefox = false;
                            if (Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "mozilla" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "firefox" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "netscape")
                            {
                                isFirefox = true;
                            }

                            //Add a new label to the end of the bar to build out the "equalizer" graph
                            Label label = new Label();
                            if (isFirefox)
                            {
                                label.Style.Add(HtmlTextWriterStyle.PaddingLeft, percentage.ToString() + "px");
                            }
                            else
                            {
                                label.Width = Unit.Pixel(percentage);
                            }
                            label.CssClass = "EqualizerGeneric";
                            label.BackColor = Color.FromName("#" + colorName);
                            parentControl.Controls.Add(label);
                        }
                        //Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                        tableCell.Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
                        tableCell.Attributes["onMouseOut"] = "hideddrivetip();";
                    }

                    //Populate the total number of open incident
                    e.Row.Cells[7].Text = incidentCount.ToString();
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
        /// Loads the control data
        /// </summary>
        protected void LoadAndBindData()
        {
            //Now get the list of projects
            ProjectManager projectManager = new ProjectManager();
            Hashtable filters = new Hashtable();
            filters.Add("ProjectGroupId", ProjectGroupId);
            filters.Add("ActiveYn", "Y");
            List<ProjectView> projects = projectManager.Retrieve(filters, null);
            this.grdProjectExecutionStatus.DataSource = projects;
            this.grdProjectExecutionStatus.DataBind();
            this.grdProjectExecutionStatus.Visible = true;
        }
    }
}