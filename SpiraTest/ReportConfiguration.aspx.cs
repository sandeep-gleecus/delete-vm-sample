using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Specialized;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// various reports configuration forms and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Reports, null, "Reports-Center")]
	public partial class ReportConfiguration : PageLayout
	{
        protected List<ArtifactField> artifactFields;
        protected List<CustomProperty> customProperties;
        protected string lastSectionName = "";

        private bool configured = false;
        private int? configured_reportFormatId = null;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReportConfiguration::";

        #region Classes

        /// <summary>
        /// Represents a unique section element combination, used in the element datagrid
        /// </summary>
        protected class ReportSectionElementInfo
        {
            public int ReportSectionId { get; set; }

            public string ReportSectionName { get; set; }

            public string ReportSectionToken { get; set; }

            public int ReportElementId { get; set; }

            public string ReportElementName { get; set; }

            public string ReportElementToken { get; set; }

            public string Description { get; set; }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Adds any event handlers that need to fire before OnLoad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Add other event handlers
            this.grdReportFormats.RowDataBound += new GridViewRowEventHandler(grdReportFormats_RowDataBound);
            this.grdReportElements.RowDataBound += new GridViewRowEventHandler(grdReportElements_RowDataBound);
            this.rptReportSection.ItemCreated += new RepeaterItemEventHandler(rptReportSection_ItemCreated);
            this.rptReportSection.ItemDataBound += new RepeaterItemEventHandler(rptReportSection_ItemDataBound);
        }

        /// <summary>
        /// Updates the state of the various dynamic controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rptReportSection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                if (e.Item.DataItem != null)
                {
                    ReportSectionInstance reportSectionInstance = (ReportSectionInstance)e.Item.DataItem;
                    ReportSection reportSection = reportSectionInstance.Section;
                    if (reportSection.ArtifactTypeId.HasValue)
                    {
                        //Sorting only available on Incident, Test Case, Test Set, Test Run and Task Reports
                        int artifactTypeId = reportSection.ArtifactTypeId.Value;
                        HtmlGenericControl divSorts = (HtmlGenericControl)e.Item.FindControl("divSorts");
                        if (divSorts != null)
                        {
                            if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Incident ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestRun)
                            {
                                divSorts.Visible = true;
                            }
                            else
                            {
                                divSorts.Visible = false;
                            }
                        }

                        //Make sure the folder is visible if the artifact type supports filtering by folder
                        PlaceHolder plcFolder = (PlaceHolder)e.Item.FindControl("plcFolder");
                        DropDownHierarchy ddlFolder = (DropDownHierarchy)e.Item.FindControl("ddlFolder");
                        if (plcFolder != null && ddlFolder != null)
                        {
                            if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task)
                            {
                                plcFolder.Visible = true;
                            }
                            else
                            {
                                plcFolder.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        this.customProperties = null;
                        e.Item.Visible = false;
                    }


					HiddenField hdnReportSection = e.Item.FindControl("hdnReportSection") as HiddenField;
					if(hdnReportSection != null)
					{
						hdnReportSection.Value = reportSectionInstance.ReportSectionId.ToString();
					}

				}
            }
        }

        /// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            //Next get the report we're meant to be configuring
            NameValueCollection query = Request.QueryString;
            int reportId = Int32.Parse(query [GlobalFunctions.PARAMETER_REPORT_ID]);

			//Clear the error messages
			this.lblMessage.Text = "";

            //Set the navigation links back to reports home
            this.pnlSidebar.HeaderUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId);
            //this.btnBack.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId);

            //Specify the context for the sidebar
            this.pnlSidebar.ProjectId = ProjectId;
            this.pnlSidebar.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REPORTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlSidebar.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REPORTS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

			//Add the button event handlers
			this.btnCancel.Click += new EventHandler(btnCancel_Click);
			this.btnCreate.Click += new EventHandler(btnCreate_Click);
           // this.chkSaveGenerated.CheckedChanged += ChkSaveGenerated_CheckedChanged;

            //Only load the data once
            if (!IsPostBack) 
			{
                try
                {
                    //Load the report definition
                    ReportManager reportManager = new ReportManager();
                    Report report = reportManager.RetrieveById(reportId);
                    string reportName = report.Name + " " + Resources.Fields.Report;

                    //Get the list of other reports in the same category
                    int reportCategoryId = report.ReportCategoryId;
                    List<Report> reportList = reportManager.RetrieveByCategoryId(reportCategoryId);
                    this.lstRelatedReports.DataSource = reportList;

                    //Finally we need to set the caption of the page and navigation breadcrumb
                    this.lblReportTitle.Text = Microsoft.Security.Application.Encoder.HtmlEncode(reportName);
                    if (!String.IsNullOrEmpty(report.Description))
                    {
                        this.lblReportTitle.ToolTip = report.Description;
                    }
                    ((MasterPages.Main)(this.Master)).TstGlobalNavigation.BreadcrumbText = reportName;
                    ((MasterPages.Main)(this.Master)).PageTitle = reportName;

                    //Get the list of formats, sections and elements
                    //Need to create a single list that's easier to databind.
                    //Also need to check permissions on the element to make sure that:
                    //(a) the product is licensed to display these items
                    //(b) the user has permissions to view these items
                    List<ReportFormat> formats = report.Formats.OrderBy(f => f.Name).ToList();
                    this.grdReportFormats.DataSource = formats;
                    List<ReportSectionElementInfo> allElements = new List<ReportSectionElementInfo>();
                    Business.ProjectManager projectManager = new Business.ProjectManager();
                    foreach (ReportSectionInstance section in report.SectionInstances)
                    {
                        //Need to explicitly sort the elements
                        IOrderedEnumerable<ReportElement> sortedElements = section.Section.Elements.OrderBy(re => re.Name);
                        foreach (ReportElement element in sortedElements)
                        {
                            if (element.ArtifactTypeId.HasValue)
                            {
                                //Make sure the user has view permissions (limited not sufficient)
                                if (projectManager.IsAuthorized(this.ProjectRoleId, (Artifact.ArtifactTypeEnum)element.ArtifactTypeId, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized)
                                {
                                    ReportSectionElementInfo reportSectionInfo = new ReportSectionElementInfo();
                                    reportSectionInfo.ReportSectionId = section.ReportSectionId;
                                    reportSectionInfo.ReportSectionName = section.Section.Name;
                                    reportSectionInfo.ReportSectionToken = section.Section.Token;
                                    reportSectionInfo.ReportElementId = element.ReportElementId;
                                    reportSectionInfo.ReportElementName = element.Name;
                                    reportSectionInfo.ReportElementToken = element.Token;
                                    reportSectionInfo.Description = element.Description;
                                    allElements.Add(reportSectionInfo);
                                }
                            }
                            else
                            {
                                //If this element is for source code, need to make sure enabled and authorized for this
                                if (element.Token == "SourceCode")
                                {
                                    bool canViewSourceCode = projectManager.IsAuthorizedToViewSourceCode(this.ProjectRoleId);
                                    List<VersionControlProject> versionControlProjects = new SourceCodeManager().RetrieveProjectSettings(this.ProjectId);
									//PCS
									if ((Common.License.LicenseProductName == LicenseProductNameEnum.ValidationMaster) && canViewSourceCode && versionControlProjects.Count > 0)
                                    {
                                        ReportSectionElementInfo reportSectionInfo = new ReportSectionElementInfo();
                                        reportSectionInfo.ReportSectionId = section.ReportSectionId;
                                        reportSectionInfo.ReportSectionName = section.Section.Name;
                                        reportSectionInfo.ReportSectionToken = section.Section.Token;
                                        reportSectionInfo.ReportElementId = element.ReportElementId;
                                        reportSectionInfo.ReportElementName = element.Name;
                                        reportSectionInfo.ReportElementToken = element.Token;
                                        reportSectionInfo.Description = element.Description;
                                        allElements.Add(reportSectionInfo);
                                    }
                                }
                                else
                                {
                                    //This element doesn't have a required permission
                                    ReportSectionElementInfo reportSectionInfo = new ReportSectionElementInfo();
                                    reportSectionInfo.ReportSectionId = section.ReportSectionId;
                                    reportSectionInfo.ReportSectionName = section.Section.Name;
                                    reportSectionInfo.ReportSectionToken = section.Section.Token;
                                    reportSectionInfo.ReportElementId = element.ReportElementId;
                                    reportSectionInfo.ReportElementName = element.Name;
                                    reportSectionInfo.ReportElementToken = element.Token;
                                    reportSectionInfo.Description = element.Description;
                                    allElements.Add(reportSectionInfo);
                                }
                            }
                        }
                    }
                    this.grdReportElements.DataSource = allElements;
                    this.rptReportSection.DataSource = report.SectionInstances;

                    //If we have any custom sections, display the release filter
                    if (report.CustomSections.Count > 0)
                    {
                        this.plcCustomSectionFilters.Visible = true;
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseView> releases = releaseManager.RetrieveByProjectId(ProjectId, true, true);
                        this.ddlCustomSectionReleaseFilter.DataSource = releases;
                    }

                    //See if we have any configuration values specified in the querystring, if so set them on the form
                    if (!String.IsNullOrWhiteSpace(query[GlobalFunctions.PARAMETER_REPORT_CONFIGURATION_SPECIFIED]) && query[GlobalFunctions.PARAMETER_REPORT_CONFIGURATION_SPECIFIED].Trim() == GlobalFunctions.PARAMETER_VALUE_TRUE)
                    {
                        this.configured = true;
                        if (!String.IsNullOrWhiteSpace(query[GlobalFunctions.PARAMETER_REPORT_FORMAT_ID]))
                        {
                            int reportFormatId;
                            if (Int32.TryParse(query[GlobalFunctions.PARAMETER_REPORT_FORMAT_ID], out reportFormatId))
                            {
                                this.configured_reportFormatId = reportFormatId;
                            }
                        }
                    }

                    //Load the list of document folders (to save the generated report)
                    AttachmentManager attachmentManager = new AttachmentManager();
                    //List<ProjectAttachmentFolderHierarchy> documentFolders = attachmentManager.RetrieveFoldersByProjectId(ProjectId);
                    //this.ddlDocumentFolder.DataSource = documentFolders;

                    //Databind the form
                    this.DataBind();

                    //Set the custom section release filter from the querystring
                    const string customSectionReleaseId = "cs_rl";
                    if (!String.IsNullOrEmpty(query[customSectionReleaseId]))
                    {
                        int releaseId;
                        if (Int32.TryParse(query[customSectionReleaseId], out releaseId))
                        {
                            try
                            {
                                this.ddlCustomSectionReleaseFilter.SelectedValue = releaseId.ToString();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                //Ignore
                            }
                        }
                    }

                    //Set the default filename/extension for the generated document
                    //txtReportGeneratedFilename.Text = reportName.Replace(" ", "-");
                    //txtGeneratedFilenameExtension.Text = ReportManager.GetExtensionForFormat(this.configured_reportFormatId);
                }
                catch (ArtifactNotExistsException)
                {
                    //The report doesn't exist, so just display that message and end
                    this.lblMessage.Text = Resources.Messages.ReportConfiguration_ReportIDNotExists;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        private void ChkSaveGenerated_CheckedChanged(object sender, EventArgs e)
        {
            //Either hide or show the saved section section
           // this.plcSaveGeneratedReport.Visible = (this.chkSaveGenerated.Checked);

            //Update the file extension based on the format
            //See if we have a format selected
            int currentFormatId = 0;
            if (this.configured_reportFormatId.HasValue && this.configured_reportFormatId.Value > 0)
            {
                currentFormatId = this.configured_reportFormatId.Value;
            }
            else
            {
                foreach (GridViewRow gridViewRow in grdReportFormats.Rows)
                {
                    //We're only interested in data-rows
                    if (gridViewRow.RowType == DataControlRowType.DataRow)
                    {
                        //Access the column containing the radio buttons and find the matching controls
                        RadioButtonEx radFormat = (RadioButtonEx)gridViewRow.Cells[1].FindControl("radFormat");
                        if (radFormat != null && radFormat.Checked)
                        {
                            currentFormatId = Int32.Parse(radFormat.MetaData);
                        }
                    }
                }
            }
           // txtGeneratedFilenameExtension.Text = ReportManager.GetExtensionForFormat(currentFormatId);
        }

        /// <summary>
        /// Creates the filter inside each section
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rptReportSection_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                if (e.Item.DataItem != null)
                {
                    ReportSectionInstance reportSectionInstance = (ReportSectionInstance)e.Item.DataItem;
                    ReportSection section = reportSectionInstance.Section;
                    if (section.ArtifactTypeId.HasValue)
                    {
                        //Get the type of artifact that this section is related to
                        int artifactTypeId = section.ArtifactTypeId.Value;

                        //Standard Field Filters
                        ArtifactManager artifactManager = new ArtifactManager();
                        this.artifactFields = artifactManager.ArtifactField_RetrieveForReporting((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);

                        //Custom Property Filters
                        CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                        this.customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(this.ProjectTemplateId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, true);

                        //Add the 'folder' as ane extra filter on certain artifact types
                        if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase ||
                            artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet ||
                            artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task)
                        {
                            PlaceHolder plcFolder = (PlaceHolder)e.Item.FindControl("plcFolder");
                            DropDownHierarchy ddlFolder = (DropDownHierarchy)e.Item.FindControl("ddlFolder");
                            if (plcFolder != null && ddlFolder != null)
                            {
                                plcFolder.Visible = true;

                                //A folder filter need to be populated for tasks, test cases and test sets
                                switch (artifactTypeId)
                                {
                                    case (int)DataModel.Artifact.ArtifactTypeEnum.TestCase:
                                        {
                                            TestCaseManager testCaseManager = new TestCaseManager();
                                            List<TestCaseFolderHierarchyView> folders = testCaseManager.TestCaseFolder_GetList(ProjectId);
                                            folders.Insert(0, new TestCaseFolderHierarchyView() { TestCaseFolderId = 0, IndentLevel = "", Name = Resources.Main.Global_Root, ProjectId = ProjectId });
                                            ddlFolder.DataSource = folders;
                                            ddlFolder.DataTextField = "Name";
                                            ddlFolder.DataValueField = "TestCaseFolderId";
                                            ddlFolder.IndentLevelField = "IndentLevel";
                                        }
                                        break;

                                    case (int)DataModel.Artifact.ArtifactTypeEnum.TestSet:
                                        {
                                            TestSetManager testSetManager = new TestSetManager();
                                            List<TestSetFolderHierarchyView> folders = testSetManager.TestSetFolder_GetList(ProjectId);
                                            folders.Insert(0, new TestSetFolderHierarchyView() { TestSetFolderId = 0, IndentLevel = "", Name = Resources.Main.Global_Root, ProjectId = ProjectId });
                                            ddlFolder.DataSource = folders;
                                            ddlFolder.DataTextField = "Name";
                                            ddlFolder.DataValueField = "TestSetFolderId";
                                            ddlFolder.IndentLevelField = "IndentLevel";
                                        }
                                        break;

                                    case (int)DataModel.Artifact.ArtifactTypeEnum.Task:
                                        {
                                            TaskManager taskManager = new TaskManager();
                                            List<TaskFolderHierarchyView> folders = taskManager.TaskFolder_GetList(ProjectId);
                                            folders.Insert(0, new TaskFolderHierarchyView() { TaskFolderId = 0, IndentLevel = "", Name = Resources.Main.Global_Root, ProjectId = ProjectId });
                                            ddlFolder.DataSource = folders;
                                            ddlFolder.DataTextField = "Name";
                                            ddlFolder.DataValueField = "TaskFolderId";
                                            ddlFolder.IndentLevelField = "IndentLevel";
                                        }
                                        break;
                                }

                                //Set the folder if passed in the query string
                                int reportSectionId = section.ReportSectionId;
                                string folderKey = "fl_" + reportSectionId;
                                if (!String.IsNullOrWhiteSpace(Request.QueryString[folderKey]))
                                {
                                    int folderId;
                                    if (Int32.TryParse(Request.QueryString[folderKey], out folderId))
                                    {
                                        ddlFolder.SelectedValue = folderId.ToString();
                                    }
                                }
                            }
                        }

                        //Sorting available on Test Case, Test Set Incident, Test Run and Task Reports
                        DropDownListEx ddlSortField = (DropDownListEx)e.Item.FindControl("ddlSortField");
                        CheckBoxEx chkSortAscending = (CheckBoxEx)e.Item.FindControl("chkSortAscending");
                        if (ddlSortField != null)
                        {
                            if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Incident ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet ||
                                artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestRun)
                            {
                                //Resort fields by name, merge both custom properties and standard fields
                                var sortedFields = this.artifactFields.Select(a => new { FieldId = a.ArtifactFieldId, Caption = a.Caption }).ToList();
                                sortedFields.AddRange(this.customProperties.Select(c => new { FieldId = -c.CustomPropertyId, Caption = c.Name }).ToList());
                                ddlSortField.DataSource = sortedFields.OrderBy(f => f.Caption).ThenBy(f => f.FieldId);

                                //Select the current sort if provided in the querystring
                                int reportSectionId = section.ReportSectionId;
                                foreach(var field in sortedFields)
                                {
                                    string sortKey = "st_" + reportSectionId + "_" + field.FieldId;
                                    if (!String.IsNullOrWhiteSpace(Request.QueryString[sortKey]))
                                    {
                                        ddlSortField.SelectedValue = field.FieldId.ToString();
                                        if (chkSortAscending != null)
                                        {
                                            if (Request.QueryString[sortKey].ToLowerInvariant() == "asc")
                                            {
                                                chkSortAscending.Checked = true;
                                            }
                                            if (Request.QueryString[sortKey].ToLowerInvariant() == "desc")
                                            {
                                                chkSortAscending.Checked = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies selective formatting to the elements grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdReportElements_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                //Display the label for all rows that are the first in their section
                LabelEx label = (LabelEx)e.Row.Cells[0].FindControl("lblLegend");
                ReportSectionElementInfo reportElement = (ReportSectionElementInfo)e.Row.DataItem;
                if (label != null)
                {
					label.Text = reportElement.ReportSectionName;

					if (reportElement.ReportSectionName == this.lastSectionName)
                    {
                        label.Visible = false;
                    }
                    this.lastSectionName = reportElement.ReportSectionName;
                }

				CheckBoxEx chkDisplayElement = e.Row.FindControl("chkDisplayElement") as CheckBoxEx;
				if(chkDisplayElement != null)
				{
					chkDisplayElement.MetaData = $"{reportElement.ReportSectionId}_{reportElement.ReportElementId}";
				}

				LabelEx lblElementName = e.Row.FindControl("lblElementName") as LabelEx;
				if (lblElementName != null)
				{
					lblElementName.ToolTip = reportElement.Description;
					lblElementName.Text = reportElement.ReportElementName;
				}

				//If we have configuration values set, override the checked flag
				if (this.configured)
                {
                    bool isChecked = !String.IsNullOrWhiteSpace(Request.QueryString["e_"+ reportElement.ReportSectionId + "_" + reportElement.ReportElementId]);
                    CheckBoxEx checkBox = (CheckBoxEx)e.Row.Cells[1].FindControl("chkDisplayElement");
                    if (checkBox != null)
                    {
                        checkBox.Checked = isChecked;
                    }
                }
                else
                {
                    //For performance/usability reasons, default some elements to OFF
                    //History and Attachments are OFF by default and test runs for requirements
                    CheckBoxEx checkBox = (CheckBoxEx)e.Row.Cells[1].FindControl("chkDisplayElement");
                    if (checkBox != null)
                    {
                        if (reportElement.ReportElementToken == "History" ||
                            reportElement.ReportElementToken == "Attachments" ||
                            (reportElement.ReportElementToken == "TestRuns" && reportElement.ReportSectionToken == "RequirementDetails"))
                        {
                            checkBox.Checked = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies selective formatting to the format grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdReportFormats_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Display the label and select the radio button for the HTML format (if available), otherwise
                //check the first row
                RadioButtonEx radioButton = (RadioButtonEx)e.Row.Cells[1].FindControl("radFormat");
                ReportFormat reportFormat = (ReportFormat)e.Row.DataItem;
                if (e.Row.RowIndex == 0)
                {
                    if (radioButton != null && !configured_reportFormatId.HasValue)
                    {
                        radioButton.Checked = true;
                    }
                }
                else
                {
                    e.Row.Cells[0].FindControl("lblLegend").Visible = false;
                    if (reportFormat.Token == "Html" && radioButton != null && !configured_reportFormatId.HasValue)
                    {
                        radioButton.Checked = true;
                    }
                }

				LabelEx lblFormatName = e.Row.FindControl("lblFormatName") as LabelEx;
				if(lblFormatName != null)
				{
					lblFormatName.ToolTip = reportFormat.Description;
					lblFormatName.Text = reportFormat.Name;
				}

				ImageEx imgFormatFiletype = e.Row.FindControl("imgFormatFiletype") as ImageEx;
				if(imgFormatFiletype != null)
				{
					imgFormatFiletype.ImageUrl = $"Images/Filetypes/{reportFormat.IconFilename}";
					imgFormatFiletype.AlternateText = reportFormat.Name;
				}

				if (this.configured_reportFormatId.HasValue && reportFormat.ReportFormatId == this.configured_reportFormatId.Value && radioButton != null)
                {
                    radioButton.Checked = true;
                }
            }
        }

        /// <summary>
        /// Databinds the child items that need to be displayed in the custom property datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void grdCustomPropertyFilters_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdCustomPropertyFilters_RowDataBound";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We only want to modify the actual datarows
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                LabelEx lblCustomPropertyFilter = (LabelEx)e.Row.Cells[0].FindControl("lblCustomPropertyFilter");
                if (lblCustomPropertyFilter != null)
                {
                    //Set the appropriate associated control id
                    CustomProperty customProperty = (CustomProperty)e.Row.DataItem;
                    int customPropertyTypeId = customProperty.CustomPropertyTypeId;

                    string fieldQueryStringKey = "xxx"; // Doesn't match deliberately
                    HiddenField hdnReportSection = (HiddenField)e.Row.Parent.Parent.Parent.FindControl("hdnReportSection");
                    if (hdnReportSection != null)
                    {
                        int reportSectionId;
                        if (Int32.TryParse(hdnReportSection.Value, out reportSectionId))
                        {
                            fieldQueryStringKey = "cp_" + reportSectionId + "_" + ((CustomProperty)e.Row.DataItem).CustomPropertyId;
                        }
                    }

                    switch ((CustomProperty.CustomPropertyTypeEnum)customPropertyTypeId)
                    {
                        case CustomProperty.CustomPropertyTypeEnum.Boolean:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "boolCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownListEx filter = (DropDownListEx)e.Row.Cells[2].FindControl("boolCustomProperty");
                                    string filterValue = Request.QueryString[fieldQueryStringKey].Trim();
                                    if (filterValue == "Y" || filterValue == "N")
                                    {
                                        filter.SelectedValue = filterValue;
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Date:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "datCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DateRangeFilter filter = (DateRangeFilter)e.Row.Cells[2].FindControl("datCustomProperty");
                                    DateRange filterValue;
                                    if (DateRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Integer:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "intCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    IntRangeFilter filter = (IntRangeFilter)e.Row.Cells[2].FindControl("intCustomProperty");
                                    IntRange filterValue;
                                    if (IntRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }

                        case CustomProperty.CustomPropertyTypeEnum.Decimal:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "decimalCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DecimalRangeFilter filter = (DecimalRangeFilter)e.Row.Cells[2].FindControl("decimalCustomProperty");
                                    DecimalRange filterValue;
                                    if (DecimalRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }

                        case CustomProperty.CustomPropertyTypeEnum.User:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "usrCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownMultiList filter = (DropDownMultiList)e.Row.Cells[2].FindControl("usrCustomProperty");
                                    MultiValueFilter filterValue;
                                    if (MultiValueFilter.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.SelectedValue = filterValue.ToString();
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.List:
                        case CustomProperty.CustomPropertyTypeEnum.MultiList:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "lstCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownMultiList filter = (DropDownMultiList)e.Row.Cells[2].FindControl("lstCustomProperty");
                                    MultiValueFilter filterValue;
                                    if (MultiValueFilter.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.SelectedValue = filterValue.ToString();
                                    }
                                }
                            }
                            break;

                        case CustomProperty.CustomPropertyTypeEnum.Text:
                            {
                                //Set the associated control
                                lblCustomPropertyFilter.AssociatedControlID = "txtCustomProperty";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    TextBoxEx filter = (TextBoxEx)e.Row.Cells[2].FindControl("txtCustomProperty");
                                    filter.Text = Request.QueryString[fieldQueryStringKey].Trim();
                                }
                            }
                            break;
                    }
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Databinds the child items that need to be displayed in the standard field datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void grdStandardFieldFilters_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdStandardFieldFilters_RowDataBound";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            //We only want to modify the actual datarows
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                //See if this is a lookup field
                int artifactFieldTypeId = ((ArtifactField)e.Row.DataItem).ArtifactFieldTypeId;
                string artifactFieldName = ((ArtifactField)(e.Row.DataItem)).Name;
                string artifactFieldCaption = ((ArtifactField)(e.Row.DataItem)).Caption;
                if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup)
                {
                    //Get the name of the artifact field and load the appropriate lookup
                    DropDownHierarchy ddlHierarchyFilter = (DropDownHierarchy)e.Row.Cells[2].FindControl("ddlHierarchyFilter");
                    if (artifactFieldName == "ReleaseId" || artifactFieldName == "TestCaseReleaseId" || artifactFieldName == "DetectedReleaseId" || artifactFieldName == "ResolvedReleaseId" || artifactFieldName == "VerifiedReleaseId")
                    {
                        ddlHierarchyFilter.ItemImage = "Images/artifact-Release.svg";
                        ddlHierarchyFilter.SummaryItemImage = "Images/artifact-Release.svg";
                        ddlHierarchyFilter.AlternateItemImage = "Images/artifact-Iteration.svg";

                        //If we have the Release or Resolved Release, add code to potentially load the build list
                        if (artifactFieldName == "ReleaseId" || artifactFieldName == "ResolvedReleaseId")
                        {
                            ddlHierarchyFilter.ClientScriptMethod = "ddlRelease_selectedItemChanged";
                        }
                    }
                    if (artifactFieldName == "TestSetId")
                    {
                        ddlHierarchyFilter.ItemImage = "Images/artifact-TestSet.svg";
                        ddlHierarchyFilter.SummaryItemImage = "Images/FolderOpen.svg";
                    }
                    if (artifactFieldName == "RequirementId")
                    {
                        ddlHierarchyFilter.ItemImage = "Images/artifact-Requirement.svg";
                        ddlHierarchyFilter.AlternateItemImage = "Images/artifact-UseCase.svg";
                        ddlHierarchyFilter.SummaryItemImage = "Images/artifact-RequirementSummary.svg";
                    }
                }

                //Set the caption name, using the localized version if available
                LabelEx lblStandardFieldFilter = (LabelEx)e.Row.Cells[0].FindControl("lblStandardFieldFilter");
                if (lblStandardFieldFilter != null)
                {
                    //See if we can find a localized version
                    string caption = artifactFieldCaption;
                    string localizedCaption = Resources.Fields.ResourceManager.GetString(artifactFieldName);
                    if (!String.IsNullOrEmpty(localizedCaption))
                    {
                        caption = localizedCaption;
                    }
                    //For IDs, override the setting since we want to indicate it's an ID
                    if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.Identifier)
                    {
                        caption = Resources.Fields.ID;
                    }
                    lblStandardFieldFilter.Text = caption + ":";

                    //Set the appropriate associated control
                    string fieldQueryStringKey = "xxx"; // Doesn't match deliberately
                    HiddenField hdnReportSection = (HiddenField)e.Row.Parent.Parent.Parent.FindControl("hdnReportSection");
                    if (hdnReportSection != null)
                    {
                        int reportSectionId;
                        if (Int32.TryParse(hdnReportSection.Value, out reportSectionId))
                        {
                            fieldQueryStringKey = "af_" + reportSectionId + "_" + ((ArtifactField)e.Row.DataItem).ArtifactFieldId;
                        }
                    }
                    switch ((DataModel.Artifact.ArtifactFieldTypeEnum)artifactFieldTypeId)
                    {
                        case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "datStandardFilter";
                                
                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DateRangeFilter filter = (DateRangeFilter)e.Row.Cells[2].FindControl("datStandardFilter");
                                    DateRange filterValue;
                                    if (DateRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "ddlHierarchyFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownHierarchy filter = (DropDownHierarchy)e.Row.Cells[2].FindControl("ddlHierarchyFilter");
                                    int filterValue;
                                    if (Int32.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.SelectedValue = filterValue.ToString();
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
                        case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "lstStandardFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownMultiList filter = (DropDownMultiList)e.Row.Cells[2].FindControl("lstStandardFilter");
                                    MultiValueFilter filterValue;
                                    if (MultiValueFilter.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.SelectedValue = filterValue.ToString();
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "ddlFlagFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownListEx filter = (DropDownListEx)e.Row.Cells[2].FindControl("ddlFlagFilter");
                                    string filterValue = Request.QueryString[fieldQueryStringKey].Trim();
                                    if (filterValue == "Y" || filterValue == "N")
                                    {
                                        filter.SelectedValue = filterValue;
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "ddlEqualizerFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DropDownListEx filter = (DropDownListEx)e.Row.Cells[2].FindControl("ddlEqualizerFilter");
                                    int filterValue;
                                    if (Int32.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.SelectedValue = filterValue.ToString();
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
                        case DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "txtStandardFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    TextBoxEx filter = (TextBoxEx)e.Row.Cells[2].FindControl("txtStandardFilter");
                                    filter.Text = Request.QueryString[fieldQueryStringKey].Trim();
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "effortRangeFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    EffortRangeFilter filter = (EffortRangeFilter)e.Row.Cells[2].FindControl("effortRangeFilter");
                                    EffortRange filterValue;
                                    if (EffortRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }
                        
                        case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "intRangeFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    IntRangeFilter filter = (IntRangeFilter)e.Row.Cells[2].FindControl("intRangeFilter");
                                    IntRange filterValue;
                                    if (IntRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Decimal:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "decimalRangeFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    DecimalRangeFilter filter = (DecimalRangeFilter)e.Row.Cells[2].FindControl("decimalRangeFilter");
                                    DecimalRange filterValue;
                                    if (DecimalRange.TryParse(Request.QueryString[fieldQueryStringKey].Trim(), out filterValue))
                                    {
                                        filter.Value = filterValue;
                                    }
                                }
                                break;
                            }

                        case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
                            {
                                //Set the associated control
                                lblStandardFieldFilter.AssociatedControlID = "txtIDFilter";

                                //Populate any passed-in filter values
                                if (this.configured && !String.IsNullOrWhiteSpace(Request.QueryString[fieldQueryStringKey]))
                                {
                                    TextBoxEx filter = (TextBoxEx)e.Row.Cells[2].FindControl("txtIDFilter");
                                    filter.Text = Request.QueryString[fieldQueryStringKey].Trim();
                                }
                                break;
                            }
                    }
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the child items that need to be displayed in the standard field datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void grdStandardFieldFilters_RowCreated(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdStandardFieldFilters_RowCreated";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We only want to modify the actual datarows
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                //See if this is a lookup field
                ArtifactField artifactField = (ArtifactField)e.Row.DataItem;
                int artifactFieldTypeId = artifactField.ArtifactFieldTypeId;
                string artifactFieldName = artifactField.Name;
                int artifactTypeId = artifactField.ArtifactTypeId;
                if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup || artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.MultiList)
                {
                    //Get the name of the artifact field and load the appropriate lookup
                    DropDownMultiList lstStandardFilter = (DropDownMultiList)e.Row.Cells[2].FindControl("lstStandardFilter");
                    if (artifactFieldName == "RequirementStatusId")
                    {
                        RequirementManager requirementManager = new RequirementManager();
                        List<RequirementStatus> requirementStatuses  = requirementManager.RetrieveStatuses();
                        lstStandardFilter.DataSource = requirementStatuses;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RequirementStatusId";
                    }
                    if (artifactFieldName == "RequirementTypeId")
                    {
                        RequirementManager requirementManager = new RequirementManager();
                        List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(ProjectTemplateId, false);
                        lstStandardFilter.DataSource = requirementTypes;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RequirementTypeId";
                    }
                    if (artifactFieldName == "BuildId")
                    {
                        //No data source, but set the fields (loaded through AJAX)
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "BuildId";
                    }
                    if (artifactFieldName == "ComponentId" || artifactFieldName == "ComponentIds")
                    {
                        ComponentManager componentManager = new ComponentManager();
                        List<DataModel.Component> components = componentManager.Component_Retrieve(ProjectId);
                        lstStandardFilter.DataSource = components;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "ComponentId";
                    }
                    if (artifactFieldName == "OpenerId" || artifactFieldName == "TesterId" || artifactFieldName == "AuthorId" || artifactFieldName == "OwnerId" || artifactFieldName == "CreatorId")
                    {
                        Business.UserManager userManager = new Business.UserManager();
                        List<DataModel.User> users = userManager.RetrieveActiveByProjectId(ProjectId);
                        lstStandardFilter.DataSource = users;
                        lstStandardFilter.DataTextField = "FullName";
                        lstStandardFilter.DataValueField = "UserId";
                    }
                    if (artifactFieldName == "ImportanceId")
                    {
                        RequirementManager requirementManager = new RequirementManager();
                        List<Importance> importances = requirementManager.RequirementImportance_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = importances;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "ImportanceId";
                    }
                    if (artifactFieldName == "TestRunTypeId")
                    {
                        TestRunManager testRunManager = new TestRunManager();
                        List<TestRunType> testRunTypes = testRunManager.RetrieveTypes();
                        lstStandardFilter.DataSource = testRunTypes;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TestRunTypeId";
                    }
                    if (artifactFieldName == "AutomationHostId")
                    {
                        AutomationManager automationManager = new AutomationManager();
                        List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(ProjectId);
                        lstStandardFilter.DataSource = automationHosts;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "AutomationHostId";
                    }
                    if (artifactFieldName == "TestCasePriorityId")
                    {
                        TestCaseManager testCaseManager = new TestCaseManager();
                        List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = priorities;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TestCasePriorityId";
                    }
                    if (artifactFieldName == "TestCaseStatusId")
                    {
                        TestCaseManager testCaseManager = new TestCaseManager();
                        List<TestCaseStatus> stati = testCaseManager.RetrieveStatuses();
                        lstStandardFilter.DataSource = stati;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TestCaseStatusId";
                    }
                    if (artifactFieldName == "TestCaseTypeId")
                    {
                        TestCaseManager testCaseManager = new TestCaseManager();
                        List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = types;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TestCaseTypeId";
                    }
                    if (artifactFieldName == "ExecutionStatusId")
                    {
                        TestCaseManager testCaseManager = new TestCaseManager();
                        List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
                        lstStandardFilter.DataSource = executionStati;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "ExecutionStatusId";
                    }
                    if (artifactFieldName == "TestSetStatusId")
                    {
                        TestSetManager testSetManager = new TestSetManager();
                        List<TestSetStatus> testSetStati = testSetManager.RetrieveStatuses();
                        lstStandardFilter.DataSource = testSetStati;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TestSetStatusId";
                    }
                    if (artifactFieldName == "RecurrenceId")
                    {
                        TestSetManager testSetManager = new TestSetManager();
                        List<Recurrence> recurrences = testSetManager.RetrieveRecurrences();
                        lstStandardFilter.DataSource = recurrences;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RecurrenceId";
                    }
                    if (artifactFieldName == "PriorityId")
                    {
                        IncidentManager incidentManager = new IncidentManager();
                        List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(ProjectTemplateId, true);
                        lstStandardFilter.DataSource = incidentPriorities;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "PriorityId";
                    }
                    if (artifactFieldName == "SeverityId")
                    {
                        IncidentManager incidentManager = new IncidentManager();
                        List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(ProjectTemplateId, true);
                        lstStandardFilter.DataSource = incidentSeverities;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "SeverityId";
                    }
                    if (artifactFieldName == "IncidentStatusId")
                    {
                        IncidentManager incidentManager = new IncidentManager();
                        List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(ProjectTemplateId, true);
                        //Add the composite (All Open) and (All Closed) items to the incident status filter
                        Dictionary<string, string> lookupValues = new Dictionary<string, string>();
                        lookupValues.Add(IncidentManager.IncidentStatusId_AllOpen.ToString(), Resources.Fields.IncidentStatus_AllOpen);
                        lookupValues.Add(IncidentManager.IncidentStatusId_AllClosed.ToString(), Resources.Fields.IncidentStatus_AllClosed);
                        //Now add the real lookup values
                        AddLookupValues(lookupValues, incidentStati.OfType<Entity>().ToList(), "IncidentStatusId", "Name");
                        lstStandardFilter.DataSource = lookupValues;
                        lstStandardFilter.DataTextField = "Value";
                        lstStandardFilter.DataValueField = "Key";
                    }
                    if (artifactFieldName == "IncidentTypeId")
                    {
                        IncidentManager incidentManager = new IncidentManager();
                        List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(ProjectTemplateId, true);
                        //Add the composite (All Issues) and (All Risks) items to the incident type filter
                        Dictionary<string, string> lookupValues = new Dictionary<string, string>();
                        lookupValues.Add(IncidentManager.IncidentTypeId_AllIssues.ToString(), Resources.Fields.IncidentType_AllIssues);
                        lookupValues.Add(IncidentManager.IncidentTypeId_AllRisks.ToString(), Resources.Fields.IncidentType_AllRisks);
                        //Now add the real lookup values
                        AddLookupValues(lookupValues, incidentTypes.OfType<Entity>().ToList(), "IncidentTypeId", "Name");
                        lstStandardFilter.DataSource = lookupValues;
                        lstStandardFilter.DataTextField = "Value";
                        lstStandardFilter.DataValueField = "Key";
                    }

                    // tasks
                    if (artifactFieldName == "TaskStatusId")
                    {
                        TaskManager taskManager = new TaskManager();
                        List<TaskStatus> statuses = taskManager.RetrieveStatuses();
                        lstStandardFilter.DataSource = statuses;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TaskStatusId";
                    }
                    if (artifactFieldName == "TaskTypeId")
                    {
                        TaskManager taskManager = new TaskManager();
                        List<TaskType> types = taskManager.TaskType_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = types;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TaskTypeId";
                    }
                    if (artifactFieldName == "TaskPriorityId")
                    {
                        TaskManager taskManager = new TaskManager();
                        List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = priorities;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "TaskPriorityId";
                    }

                    // releases
                    if (artifactFieldName == "ReleaseTypeId")
                    {
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseType> types = releaseManager.RetrieveTypes();
                        lstStandardFilter.DataSource = types;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "ReleaseTypeId";
                    }
                    if (artifactFieldName == "ReleaseStatusId")
                    {
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseStatus> stati = releaseManager.RetrieveStatuses();
                        lstStandardFilter.DataSource = stati;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "ReleaseStatusId";
                    }

                    // risks
                    if (artifactFieldName == "RiskStatusId")
                    {
                        RiskManager riskManager = new RiskManager();
                        List<RiskStatus> stati = riskManager.RiskStatus_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = stati;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RiskStatusId";
                    }
                    if (artifactFieldName == "RiskTypeId")
                    {
                        RiskManager riskManager = new RiskManager();
                        List<RiskType> types = riskManager.RiskType_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = types;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RiskTypeId";
                    }
                    if (artifactFieldName == "RiskImpactId")
                    {
                        RiskManager riskManager = new RiskManager();
                        List<RiskImpact> impacts = riskManager.RiskImpact_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = impacts;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RiskImpactId";
                    }
                    if (artifactFieldName == "RiskProbabilityId")
                    {
                        RiskManager riskManager = new RiskManager();
                        List<RiskProbability> probabilities = riskManager.RiskProbability_Retrieve(ProjectTemplateId);
                        lstStandardFilter.DataSource = probabilities;
                        lstStandardFilter.DataTextField = "Name";
                        lstStandardFilter.DataValueField = "RiskProbabilityId";
                    }

                }

                if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.Flag)
                {
                    DropDownListEx ddlFlagFilter = (DropDownListEx)e.Row.Cells[2].FindControl("ddlFlagFilter");
                    ddlFlagFilter.DataTextField = "Value";
                    ddlFlagFilter.DataValueField = "Key";
                    ManagerBase dataAccess = new ManagerBase();
                    Dictionary<string, string> lookupCollection = dataAccess.RetrieveFlagLookupDictionary();
                    ddlFlagFilter.DataSource = lookupCollection;
                }

                if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer)
                {
                    DropDownListEx ddlEqualizerFilter = (DropDownListEx)e.Row.Cells[2].FindControl("ddlEqualizerFilter");
                    if (artifactFieldName == "CoverageId")
                    {
                        RequirementManager requirement = new RequirementManager();
                        Dictionary<string, string> lookupCollection = requirement.RetrieveCoverageFiltersLookup();
                        ddlEqualizerFilter.DataSource = lookupCollection;
                        ddlEqualizerFilter.DataTextField = "Value";
                        ddlEqualizerFilter.DataValueField = "Key";
                    }
                    if (artifactFieldName == "ProgressId")
                    {
                        TaskManager taskManager = new TaskManager();
                        Dictionary<string, string> lookupCollection = taskManager.RetrieveProgressFiltersLookup();
                        ddlEqualizerFilter.DataSource = lookupCollection;
                        ddlEqualizerFilter.DataTextField = "Value";
                        ddlEqualizerFilter.DataValueField = "Key";
                    }
                    if (artifactFieldName == "ExecutionStatusId")
                    {
                        //The filters for test sets and test cases are different
                        if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase)
                        {
                            TestCaseManager testCaseManager = new TestCaseManager();
                            List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
                            ddlEqualizerFilter.DataSource = executionStati;
                            ddlEqualizerFilter.DataTextField = "Name";
                            ddlEqualizerFilter.DataValueField = "ExecutionStatusId";
                        }
                        if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet)
                        {
                            TestSetManager testSetManager = new TestSetManager();
                            SortedList<int, string> lookupCollection = testSetManager.RetrieveExecutionStatusFiltersLookup();
                            ddlEqualizerFilter.DataSource = lookupCollection;
                            ddlEqualizerFilter.DataTextField = "Value";
                            ddlEqualizerFilter.DataValueField = "Key";
                        }
                    }
                }
                if (artifactFieldTypeId == (int)DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup)
                {
                    //Get the name of the artifact field and load the appropriate lookup
                    DropDownHierarchy ddlHierarchyFilter = (DropDownHierarchy)e.Row.Cells[2].FindControl("ddlHierarchyFilter");
                    if (artifactFieldName == "ReleaseId" || artifactFieldName == "TestCaseReleaseId" || artifactFieldName == "DetectedReleaseId" || artifactFieldName == "ResolvedReleaseId" || artifactFieldName == "VerifiedReleaseId")
                    {
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseView> releases = releaseManager.RetrieveByProjectId(ProjectId, true, true);
                        ddlHierarchyFilter.DataSource = releases;
                        ddlHierarchyFilter.DataTextField = "FullName";
                        ddlHierarchyFilter.DataValueField = "ReleaseId";
                        ddlHierarchyFilter.IndentLevelField = "IndentLevel";
                        ddlHierarchyFilter.SummaryItemField = "IsSummary";
                        ddlHierarchyFilter.AlternateItemField = "IsIteration";
                    }

                    if (artifactFieldName == "TestSetId")
                    {
                        TestSetManager testSetManager = new TestSetManager();
                        List<TestSetManager.TestSetLookupEntry> testSetLookups = testSetManager.RetrieveForLookups2(ProjectId);
                        ddlHierarchyFilter.DataSource = testSetLookups;
                        ddlHierarchyFilter.DataTextField = "Name";
                        ddlHierarchyFilter.DataValueField = "TestSetId";
                        ddlHierarchyFilter.IndentLevelField = "IndentLevel";
                        ddlHierarchyFilter.SummaryItemField = "IsFolder";
                    }

                    if (artifactFieldName == "RequirementId")
                    {
                        RequirementManager requirementManager = new RequirementManager();
                        List<RequirementManager.RequirementLookupEntry> requirementLookups = requirementManager.RetrieveForLookups2(ProjectId);
                        ddlHierarchyFilter.DataSource = requirementLookups;
                        ddlHierarchyFilter.DataTextField = "Name";
                        ddlHierarchyFilter.DataValueField = "RequirementId";
                        ddlHierarchyFilter.IndentLevelField = "IndentLevel";
                        ddlHierarchyFilter.SummaryItemField = "IsSummary";
                        ddlHierarchyFilter.AlternateItemField = "IsAlternate";
                    }
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the child items that need to be displayed in the custom property datagrid
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void grdCustomPropertyFilters_RowCreated(object sender, GridViewRowEventArgs e)
        {
            const string METHOD_NAME = "grdCustomPropertyFilters_RowCreated";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We only want to modify the actual datarows
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                //Get the id of the custom list that belongs to the property (we only do this for list types)
                CustomProperty customProperty = (CustomProperty)e.Row.DataItem;

                //See if we need to load any lists
                switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
                {
                    case CustomProperty.CustomPropertyTypeEnum.List:
                    case CustomProperty.CustomPropertyTypeEnum.MultiList:
                        {
                            //Get the dropdown list and load the appropriate lookup list
                            DropDownMultiList lstCustomProperty = (DropDownMultiList)e.Row.Cells[2].FindControl("lstCustomProperty");
                            if (lstCustomProperty != null && customProperty.List != null)
                            {
                                //We need to sort appropriately
                                IOrderedEnumerable<CustomPropertyValue> sortedValues;
                                if (customProperty.List.IsSortedOnValue)
                                    sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.Name);
                                else
                                    sortedValues = customProperty.List.Values.OrderBy(cpv => cpv.CustomPropertyValueId);
                                lstCustomProperty.DataSource = sortedValues;
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.Boolean:
                        {
                            //Get the dropdown list and load the appropriate lookup list
                            DropDownListEx boolCustomProperty = (DropDownListEx)e.Row.Cells[2].FindControl("boolCustomProperty");
                            if (boolCustomProperty != null)
                            {
                                boolCustomProperty.DataSource = new ManagerBase().RetrieveFlagLookupDictionary();
                            }
                        }
                        break;

                    case CustomProperty.CustomPropertyTypeEnum.User:
                        {
                            //Get the dropdown list and load the appropriate lookup list
                            DropDownMultiList usrCustomProperty = (DropDownMultiList)e.Row.Cells[2].FindControl("usrCustomProperty");
                            if (usrCustomProperty != null)
                            {
                                usrCustomProperty.DataSource = new UserManager().RetrieveActiveByProjectId(ProjectId);
                            }
                        }
                        break;
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }


        /// <summary>
        /// Returns to the reports home page when the button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Return to the reports home page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, this.ProjectId), true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Generates the report when the button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCreate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure all validators were successful (dates handled later)
            if (!IsValid)
            {
                return;
            }

            //Get the report id from the querystring and build the viewer Url
            int reportId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID]);
            string viewerUrl = ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId, reportId, GlobalFunctions.PARAMETER_TAB_REPORT_VIEWER));
            string queryStringSuffix = "";

            //First we need to find which report format was chosen and add to the report viewer URL
            int reportFormatId = -1;
            //Next we need to iterate through all the report elements and add to the report viewer URL
            foreach (GridViewRow gridViewRow in grdReportFormats.Rows)
            {
                //We're only interested in data-rows
                if (gridViewRow.RowType == DataControlRowType.DataRow)
                {
                    //Access the column containing the radio buttons and find the matching controls
                    RadioButtonEx radFormat = (RadioButtonEx)gridViewRow.Cells[1].FindControl("radFormat");
                    if (radFormat != null && radFormat.Checked)
                    {
                        reportFormatId = Int32.Parse(radFormat.MetaData);
                        if (queryStringSuffix == "")
                        {
                            queryStringSuffix += GlobalFunctions.PARAMETER_REPORT_FORMAT_ID + "=" + reportFormatId;
                        }
                        else
                        {
                            queryStringSuffix += "&" + GlobalFunctions.PARAMETER_REPORT_FORMAT_ID + "=" + reportFormatId;
                        }
                    }
                }
            }

            //Next we need to iterate through all the report elements and add to the report viewer URL
            foreach (GridViewRow gridViewRow in grdReportElements.Rows)
            {
                //We're only interested in data-rows
                if (gridViewRow.RowType == DataControlRowType.DataRow)
                {
                    //Access the column containing the checkboxes and find the matching controls
                    CheckBoxEx chkDisplayElement = (CheckBoxEx)gridViewRow.Cells[1].FindControl("chkDisplayElement");
                    if (chkDisplayElement != null && chkDisplayElement.Checked)
                    {
                        string reportSectionAndElementId = chkDisplayElement.MetaData;
                        queryStringSuffix += "&e_" + reportSectionAndElementId + "=1";
                    }
                }
            }

            //Next iterate through all the report sections
            foreach (RepeaterItem repeaterItem in this.rptReportSection.Items)
            {
                if (repeaterItem.ItemType == ListItemType.Item || repeaterItem.ItemType == ListItemType.AlternatingItem)
                {
                    HiddenField hdnReportSection = (HiddenField)repeaterItem.FindControl("hdnReportSection");
                    if (hdnReportSection != null)
                    {
                        //Get the report section id
                        int reportSectionId = Int32.Parse(hdnReportSection.Value);

                        //First get the various standard artifact field (AF) filters
                        GridViewEx grdStandardFieldFilters = (GridViewEx)repeaterItem.FindControl("grdStandardFieldFilters");
                        foreach (GridViewRow gridViewRow in grdStandardFieldFilters.Rows)
                        {
                            //Only look at the data rows
                            if (gridViewRow.RowType == DataControlRowType.DataRow)
                            {
                                //Get a handle to each of the controls
                                TextBoxEx txtStandardFilter = (TextBoxEx)gridViewRow.Cells[2].FindControl("txtStandardFilter");
                                TextBoxEx txtIDFilter = (TextBoxEx)gridViewRow.Cells[2].FindControl("txtIDFilter");
                                DropDownListEx ddlFlagFilter = (DropDownListEx)gridViewRow.Cells[2].FindControl("ddlFlagFilter");
                                DropDownMultiList lstStandardFilter = (DropDownMultiList)gridViewRow.Cells[2].FindControl("lstStandardFilter");
                                DropDownListEx ddlEqualizerFilter = (DropDownListEx)gridViewRow.Cells[2].FindControl("ddlEqualizerFilter");
                                DropDownHierarchy ddlHierarchyFilter = (DropDownHierarchy)gridViewRow.Cells[2].FindControl("ddlHierarchyFilter");
                                DateRangeFilter datStandardFilter = (DateRangeFilter)gridViewRow.Cells[2].FindControl("datStandardFilter");
                                DecimalRangeFilter decimalRangeFilter = (DecimalRangeFilter)gridViewRow.Cells[2].FindControl("decimalRangeFilter");
                                IntRangeFilter intRangeFilter = (IntRangeFilter)gridViewRow.Cells[2].FindControl("intRangeFilter");
                                EffortRangeFilter effortRangeFilter = (EffortRangeFilter)gridViewRow.Cells[2].FindControl("effortRangeFilter");

                                //Add their filter values to the querysting if they have a meta-data value set
                                if (!String.IsNullOrEmpty(txtStandardFilter.MetaData))
                                {
                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(txtStandardFilter.MetaData);
                                    string filterValue = txtStandardFilter.Text.Trim();
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(txtIDFilter.MetaData))
                                {
                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(txtIDFilter.MetaData);
                                    string filterValue = txtIDFilter.Text.Trim();
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(ddlFlagFilter.MetaData))
                                {
                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(ddlFlagFilter.MetaData);
                                    string filterValue = ddlFlagFilter.SelectedValue;
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(lstStandardFilter.MetaData))
                                {
                                    int artifactFieldId = Int32.Parse(lstStandardFilter.MetaData);
                                    //Get the custom property id and list of values as a comma-separated list of ids
                                    string filterValue = lstStandardFilter.RawSelectedValues;
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + filterValue;
                                    }
                                }
                                if (!String.IsNullOrEmpty(ddlEqualizerFilter.MetaData))
                                {
                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(ddlEqualizerFilter.MetaData);
                                    string filterValue = ddlEqualizerFilter.SelectedValue;
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(ddlHierarchyFilter.MetaData))
                                {
                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(ddlHierarchyFilter.MetaData);
                                    string filterValue = ddlHierarchyFilter.SelectedValue;
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(datStandardFilter.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!datStandardFilter.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblStandardFieldFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(datStandardFilter.MetaData);
                                    DateRange dateRange = datStandardFilter.Value;
                                    if (dateRange != null)
                                    {
                                        string filterValue = dateRange.ToString();
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(decimalRangeFilter.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!decimalRangeFilter.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblStandardFieldFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(decimalRangeFilter.MetaData);
                                    DecimalRange decimalRange = decimalRangeFilter.Value;
                                    if (decimalRange != null)
                                    {
                                        string filterValue = decimalRange.ToString();
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(intRangeFilter.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!intRangeFilter.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblStandardFieldFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(intRangeFilter.MetaData);
                                    IntRange intRange = intRangeFilter.Value;
                                    if (intRange != null)
                                    {
                                        string filterValue = intRange.ToString();
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(effortRangeFilter.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!effortRangeFilter.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblStandardFieldFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int artifactFieldId = Int32.Parse(effortRangeFilter.MetaData);
                                    EffortRange effortRange = effortRangeFilter.Value;
                                    if (effortRange != null)
                                    {
                                        string filterValue = effortRange.ToString();
                                        queryStringSuffix += "&af_" + reportSectionId + "_" + artifactFieldId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                            }
                        }

                        //Next get the various custom property (CP) filters
                        GridViewEx grdCustomPropertyFilters = (GridViewEx)repeaterItem.FindControl("grdCustomPropertyFilters");
                        foreach (GridViewRow gridViewRow in grdCustomPropertyFilters.Rows)
                        {
                            //Only look at the data rows
                            if (gridViewRow.RowType == DataControlRowType.DataRow)
                            {
                                //Get a handle to each of the controls
                                TextBoxEx txtCustomProperty = (TextBoxEx)gridViewRow.Cells[2].FindControl("txtCustomProperty");
                                DropDownMultiList lstCustomProperty = (DropDownMultiList)gridViewRow.Cells[2].FindControl("lstCustomProperty");
                                DropDownMultiList usrCustomProperty = (DropDownMultiList)gridViewRow.Cells[2].FindControl("usrCustomProperty");
                                DropDownListEx boolCustomProperty = (DropDownListEx)gridViewRow.Cells[2].FindControl("boolCustomProperty");
                                DateRangeFilter datCustomProperty = (DateRangeFilter)gridViewRow.Cells[2].FindControl("datCustomProperty");
                                DecimalRangeFilter decimalCustomProperty = (DecimalRangeFilter)gridViewRow.Cells[2].FindControl("decimalCustomProperty");
                                IntRangeFilter intCustomProperty = (IntRangeFilter)gridViewRow.Cells[2].FindControl("intCustomProperty");

                                //Add their filter values to the querysting if they have a meta-data value set
                                if (!String.IsNullOrEmpty(txtCustomProperty.MetaData))
                                {
                                    //Get the custom property id and value
                                    int customPropertyId = Int32.Parse(txtCustomProperty.MetaData);
                                    string filterValue = txtCustomProperty.Text.Trim();
                                     if (filterValue != "")
                                    {
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(lstCustomProperty.MetaData))
                                {
                                    int customPropertyId = Int32.Parse(lstCustomProperty.MetaData);
                                    //Get the custom property id and list of values as a comma-separated list of ids
                                    string filterValue = "";
                                    for (int k = 0; k < lstCustomProperty.Items.Count; k++)
                                    {
                                        if (lstCustomProperty.Items[k].Selected)
                                        {
                                            if (filterValue == "")
                                            {
                                                filterValue = lstCustomProperty.Items[k].Value;
                                            }
                                            else
                                            {
                                                filterValue += "," + lstCustomProperty.Items[k].Value;
                                            }
                                        }
                                    }
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + filterValue;
                                    }
                                }
                                if (!String.IsNullOrEmpty(decimalCustomProperty.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!decimalCustomProperty.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblCustomPropertyFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int customPropertyId = Int32.Parse(decimalCustomProperty.MetaData);
                                    DecimalRange decimalRange = decimalCustomProperty.Value;
                                    if (decimalRange != null)
                                    {
                                        string filterValue = decimalRange.ToString();
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(intCustomProperty.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!intCustomProperty.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblCustomPropertyFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int customPropertyId = Int32.Parse(intCustomProperty.MetaData);
                                    IntRange intRange = intCustomProperty.Value;
                                    if (intRange != null)
                                    {
                                        string filterValue = intRange.ToString();
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(usrCustomProperty.MetaData))
                                {
                                    int customPropertyId = Int32.Parse(usrCustomProperty.MetaData);
                                    //Get the custom property id and list of values as a comma-separated list of ids
                                    string filterValue = "";
                                    for (int k = 0; k < usrCustomProperty.Items.Count; k++)
                                    {
                                        if (usrCustomProperty.Items[k].Selected)
                                        {
                                            if (filterValue == "")
                                            {
                                                filterValue = usrCustomProperty.Items[k].Value;
                                            }
                                            else
                                            {
                                                filterValue += "," + usrCustomProperty.Items[k].Value;
                                            }
                                        }
                                    }
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + filterValue;
                                    }
                                }

                                if (!String.IsNullOrEmpty(boolCustomProperty.MetaData))
                                {
                                    //Get the name and value
                                    int customPropertyId = Int32.Parse(boolCustomProperty.MetaData);
                                    string filterValue = boolCustomProperty.SelectedValue;
                                    if (filterValue != "")
                                    {
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                                if (!String.IsNullOrEmpty(datCustomProperty.MetaData))
                                {
                                    //First make sure it validated OK
                                    if (!datCustomProperty.IsValid)
                                    {
                                        LabelEx label = (LabelEx)gridViewRow.Cells[0].FindControl("lblCustomPropertyFilter");
                                        if (label != null)
                                        {
                                            this.lblMessage.Text = String.Format(Resources.Messages.ReportConfiguration_Needs, label.MetaData);
                                            this.lblMessage.Type = MessageBox.MessageType.Error;
                                        }
                                        return;
                                    }

                                    //Get the name and value
                                    int customPropertyId = Int32.Parse(datCustomProperty.MetaData);
                                    DateRange dateRange = datCustomProperty.Value;
                                    if (dateRange != null)
                                    {
                                        string filterValue = dateRange.ToString();
                                        queryStringSuffix += "&cp_" + reportSectionId + "_" + customPropertyId + "=" + Server.UrlEncode(filterValue);
                                    }
                                }
                            }
                        }

                        //For certain artifacts we need to capture the folder id as well
                        DropDownHierarchy ddlFolder = (DropDownHierarchy)repeaterItem.FindControl("ddlFolder");
                        if (ddlFolder != null)
                        {
                            //See if we have a value specified
                            if (!String.IsNullOrEmpty(ddlFolder.RawSelectedValue))
                            {
                                int folderId;
                                if (Int32.TryParse(ddlFolder.RawSelectedValue, out folderId))
                                {
                                    queryStringSuffix += "&fl_" + reportSectionId + "=" + folderId;
                                }
                            }
                        }

                        //Finally for various artifacts, get the chosen sort option (ST)
                        HtmlGenericControl divSorts = (HtmlGenericControl)repeaterItem.FindControl("divSorts");
                        if (divSorts != null && divSorts.Visible)
                        {
                            //Get a handle to the dropdown and direction checkbox
                            DropDownListEx ddlSortField = (DropDownListEx)repeaterItem.FindControl("ddlSortField");
                            CheckBoxEx chkSortAscending = (CheckBoxEx)repeaterItem.FindControl("chkSortAscending");
                            if (ddlSortField != null && chkSortAscending != null && !String.IsNullOrWhiteSpace(ddlSortField.SelectedValue))
                            {
                                int artifactFieldId = Int32.Parse(ddlSortField.SelectedValue);
                                string direction = (chkSortAscending.Checked) ? "asc" : "desc";
                                queryStringSuffix += "&st_" + reportSectionId + "_" + artifactFieldId + "=" + direction;
                            }
                        }
                    }
                }
            }

            //See if we need to add a custom section release filter
            if (!String.IsNullOrEmpty(this.ddlCustomSectionReleaseFilter.SelectedValue))
            {
                int releaseId;
                if (Int32.TryParse(this.ddlCustomSectionReleaseFilter.SelectedValue, out releaseId))
                {
                    const string customSectionReleaseId = "cs_rl";
                    queryStringSuffix += "&" + customSectionReleaseId + "=" + releaseId;
                }
            }

            //Make sure at least one format
            if (reportFormatId == -1)
            {
                this.lblMessage.Text = Resources.Messages.ReportConfiguration_NeedToSelectFormat;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //See if we need to save the report or not
            if (this.txtReportName.Text.Trim() != "")
            {
                //See if we need to share it or not
                ReportManager reportManager = new ReportManager();
                bool shareReport = this.chkShareReport.Checked;

                //Save the report
                string reportName = GlobalFunctions.HtmlScrubInput(this.txtReportName.Text.Trim());
                reportManager.InsertSaved(
                    reportId,
                    reportFormatId,
                    this.UserId,
                    this.ProjectId,
                    reportName,
                    queryStringSuffix,
                    shareReport
                    );
            }

			//BEGIN PCS
			//See if we want to save/store the rendered report into a document folder
			//if (this.chkSaveGenerated.Checked)
			//{
			//    //Get the document folder, default to root if not provided
			//    if (String.IsNullOrWhiteSpace(this.ddlDocumentFolder.SelectedValue))
			//    {
			//        if (!String.IsNullOrWhiteSpace(this.txtReportGeneratedFilename.Text))
			//        {
			//            int documentFolderId = new AttachmentManager().GetDefaultProjectFolder(ProjectId);
			//            string generatedReportFilename = this.txtReportGeneratedFilename.Text.Trim();
			//            queryStringSuffix += "&sg=" + documentFolderId + "|" + HttpUtility.UrlEncode(generatedReportFilename);
			//        }
			//    }
			//    else
			//    {
			//        int documentFolderId;
			//        if (Int32.TryParse(this.ddlDocumentFolder.SelectedValue, out documentFolderId))
			//        {
			//            if (documentFolderId > 0 && !String.IsNullOrWhiteSpace(this.txtReportGeneratedFilename.Text))
			//            {
			//                string generatedReportFilename = this.txtReportGeneratedFilename.Text.Trim();
			//                queryStringSuffix += "&sg=" + documentFolderId + "|" + HttpUtility.UrlEncode(generatedReportFilename);
			//            }
			//        }
			//    }
			//}

			//END PCS

			//Finally actually go to the report rendering page
			Response.Redirect(viewerUrl + "?" + queryStringSuffix, true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		#endregion

		#region Methods Used Internally

        /// <summary>
        /// Gets the client id of the build dropdown if it's displayed
        /// </summary>
        /// <returns></returns>
        protected string GetBuildClientId()
        {
            string clientId = "";
            foreach (RepeaterItem ri in rptReportSection.Items)
            {
                GridViewEx grdStandardFieldFilters = (GridViewEx)ri.FindControl("grdStandardFieldFilters");
                if (grdStandardFieldFilters != null)
                {
                    foreach (GridViewRow gridViewRow in grdStandardFieldFilters.Rows)
                    {
                        //Only look at the data rows
                        if (gridViewRow.RowType == DataControlRowType.DataRow)
                        {
                            DropDownMultiList lstStandardFilter = (DropDownMultiList)gridViewRow.FindControl("lstStandardFilter");
                            if (lstStandardFilter != null && lstStandardFilter.Visible && lstStandardFilter.DataValueField == "BuildId")
                            {
                                clientId = lstStandardFilter.ClientID;
                                break;
                            }
                        }
                    }
                }
            }

            return clientId;
        }

        /// <summary>
        /// Returns the dataset property corresponding to a given database column name
        /// </summary>
        /// <param name="columnName">The column name (e.g. TEXT_01)</param>
        /// <returns>The corresponding property name (e.g. Text01)</returns>
        protected string GetPropertyName(string columnName)
        {
            //Delegate to the CustomProperty business object
            //Business.CustomPropertyOldManager customProperty = new Business.CustomPropertyOldManager();
            return "";//customProperty.GetPropertyName(columnName);
        }

         /// <summary>
         /// Adds a dataset lookup to the array that will be consumed by the webservice
         /// </summary>
         /// <param name="lookupValues">Partially filled dictionary that we add the values to</param>
         /// <param name="entities">The lookup list</param>
         /// <param name="nameField">The name of the field used to store the lookup name</param>
         /// <param name="valueField">The name of the field used to store the lookup value</param>
         /// <remarks>This version is used when you need to partially populate the dictionary first</remarks>
         protected void AddLookupValues(Dictionary<string, string> lookupValues, List<Entity> entities, string nameField, string valueField)
         {
             //Iterate through the rows to extract the data
             foreach (Entity entity in entities)
             {
                 if (entity[nameField] != null && entity[valueField] != null)
                 {
                     lookupValues.Add(entity[nameField].ToString(), entity[valueField].ToString());
                 }
             }
         }

		#endregion

	}
}
