using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Web.Script.Serialization;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for modifying a specific report definition
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ReportDetails", "System-Reporting/#edit-reports", "Admin_ReportDetails"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ReportAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ReportDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ReportDetails::";

        protected int reportId;
        protected Report report;

        /// <summary>
        /// Gets the list of custom sections as a JSON object array
        /// </summary>
        protected string CustomSectionsJson
        {
            get
            {
                if (this.report == null || this.report.CustomSections == null)
                {
                    //Handle nulls
                    return "[]";
                }


                //We need to get rid of the circular reference back to Report and the trackable sub-collections so we create a new list
                List<ReportCustomSection> customSections = new List<ReportCustomSection>();
                foreach (ReportCustomSection customSection in report.CustomSections)
                {
                    ReportCustomSection clonedCustomSection = new ReportCustomSection();
                    clonedCustomSection.ReportCustomSectionId = customSection.ReportCustomSectionId;
                    clonedCustomSection.ReportId = customSection.ReportId;
                    clonedCustomSection.Name = customSection.Name;
                    clonedCustomSection.Description = customSection.Description;
                    clonedCustomSection.Header = customSection.Header;
                    clonedCustomSection.Footer = customSection.Footer;
                    clonedCustomSection.Template = customSection.Template;
                    clonedCustomSection.IsActive = customSection.IsActive;
                    clonedCustomSection.Query = customSection.Query;
                    customSections.Add(clonedCustomSection);
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Serialize(customSections);
            }
        }

        /// <summary>
        /// Gets the list of standard sections as a JSON object array
        /// </summary>
        protected string StandardSectionsJson
        {
            get
            {
                if (this.report == null || this.report.SectionInstances == null)
                {
                    //Handle nulls
                    return "[]";
                }

                //We need to get rid of the circular reference back to Report and the trackable sub-collections so we create a new list
                List<ReportSectionInstance> sectionInstances = new List<ReportSectionInstance>();
                foreach (ReportSectionInstance sectionInstance in report.SectionInstances)
                {
                    ReportSectionInstance clonedSectionInstance = new ReportSectionInstance();
                    clonedSectionInstance.ReportId = sectionInstance.ReportId;
                    clonedSectionInstance.ReportSectionId = sectionInstance.ReportSectionId;
                    clonedSectionInstance.Header = sectionInstance.Header;
                    clonedSectionInstance.Footer = sectionInstance.Footer;
                    if (String.IsNullOrWhiteSpace(sectionInstance.Template))
                    {
                        clonedSectionInstance.Template = sectionInstance.Section.DefaultTemplate;
                    }
                    else
                    {
                        clonedSectionInstance.Template = sectionInstance.Template;
                    }
                    sectionInstances.Add(clonedSectionInstance);
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Serialize(sectionInstances);
            }
        }

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Get the report id unless blank
            if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID]))
            {
                this.reportId = -1;
            }
            else
            {
                if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID], out this.reportId))
                {
                    //Return to the report list page
                    Response.Redirect("Reports.aspx", true);
                }
            }

            //Add the event handlers
            this.grdFormats.RowDataBound += new GridViewRowEventHandler(grdFormats_RowDataBound);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
            this.btnActivate.Click += new EventHandler(btnActivate_Click);
            this.btnDeactivate.Click += new EventHandler(btnDeactivate_Click);

            //We need to store the sections in special JSON hidden fields before submit
            this.ClientScript.RegisterOnSubmitStatement(this.GetType(), "Form_OnSubmit", "return reportDetails_onSubmit();");

            //Load and bind data
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
        
        /// <summary>
        /// Deactivates a standard report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDeactivate_Click(object sender, EventArgs e)
        {
            //First we need to load the report from the database
            ReportManager reportManager = new ReportManager();
            report = reportManager.RetrieveById(this.reportId);

            //Now make inactive if not already
            if (report.IsActive)
            {
                report.StartTracking();
                report.IsActive = false;
                reportManager.Report_Update(report);
            }
            //Return to the report list page
            Response.Redirect("Reports.aspx", true);
        }

        /// <summary>
        /// Activates a standard report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnActivate_Click(object sender, EventArgs e)
        {
            //First we need to load the report from the database
            ReportManager reportManager = new ReportManager();
            report = reportManager.RetrieveById(this.reportId);

            //Now make active if not already
            if (!report.IsActive)
            {
                report.StartTracking();
                report.IsActive = true;
                reportManager.Report_Update(report);
            }
            //Return to the report list page
            Response.Redirect("Reports.aspx", true);
        }

        /// <summary>
        /// Updates the report configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //See if we have a report id or not
                ReportManager reportManager = new ReportManager();
                if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID]))
                {
                    //Insert Case
                    this.reportId = -1;
                    report = new Report();
                }
                else
                {
                    //Update Case

                    //Get the report id
                    if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID], out this.reportId))
                    {
                        //Return to the report list page
                        Response.Redirect("Reports.aspx", true);
                    }

                    //Next we need to load the report from the database
                    report = reportManager.RetrieveById(this.reportId, true);

                    //Make sure this is not a standard report (they cannot be updated)
                    if (!String.IsNullOrEmpty(report.Token))
                    {
                        //Return to the report list page
                        Response.Redirect("Reports.aspx", true);
                        return;
                    }
                    report.StartTracking();
                }

                //First we need to update the report itself
                report.Name = this.txtName.Text.Trim();
                report.Description = this.txtDescription.Text.Trim();
                report.Header = this.txtHeader.Text;
                report.Footer = this.txtFooter.Text;
                report.IsActive = this.chkActive.Checked;
                int reportCategoryId;
                if (Int32.TryParse(this.ddlCategory.SelectedValue, out reportCategoryId))
                {
                    report.ReportCategoryId = reportCategoryId;
                }

                //Next the formats
                List<int> reportFormatIds = new List<int>();
                foreach (GridViewRow gvr in this.grdFormats.Rows)
                {
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Locate the checkbox
                        CheckBoxEx chkFormat = (CheckBoxEx)gvr.FindControl("chkFormat");
                        if (chkFormat != null && chkFormat.Checked)
                        {
                            int reportFormatId = Int32.Parse(chkFormat.MetaData);
                            reportFormatIds.Add(reportFormatId);
                        }
                    }
                }

                //Next the standard sections, we need to deserialize from a hidden field
                string standardSectionsJson = this.hdnStandardSections.Value;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<ReportSectionInstance> standardSections = (List<ReportSectionInstance>)serializer.Deserialize(standardSectionsJson, typeof(List<ReportSectionInstance>));

                //Make any updates
                foreach (ReportSectionInstance standardSection in standardSections)
                {
                    //First the additions/updates
                    ReportSectionInstance existingSection = report.SectionInstances.FirstOrDefault(r => r.ReportSectionId == standardSection.ReportSectionId);
                    if (existingSection == null)
                    {
                        //Addition
                        ReportSectionInstance newSection = new ReportSectionInstance();
                        newSection.ReportSectionId = standardSection.ReportSectionId;
                        newSection.Header = standardSection.Header;
                        newSection.Footer = standardSection.Footer;
                        newSection.Template = standardSection.Template;
                        report.SectionInstances.Add(newSection);
                        
                    }
                    else
                    {
                        //Update
                        existingSection.StartTracking();
                        existingSection.Header = standardSection.Header;
                        existingSection.Footer = standardSection.Footer;
                        existingSection.Template = standardSection.Template;
                    }
                }

                //Finally any deletes
                List<ReportSectionInstance> sectionsToDelete = new List<ReportSectionInstance>();
                for (int i = 0; i < report.SectionInstances.Count; i++)
                {
                    ReportSectionInstance existingSection2 = report.SectionInstances[i];
                    if (!standardSections.Any(r => r.ReportSectionId == existingSection2.ReportSectionId))
                    {
                        sectionsToDelete.Add(existingSection2);
                    }
                }
                foreach (ReportSectionInstance existingSection2 in sectionsToDelete)
                {
                    existingSection2.MarkAsDeleted();
                }

                //Next the custom sections
                string customSectionsJson = this.hdnCustomSections.Value;
                List<ReportCustomSection> customSections = (List<ReportCustomSection>)serializer.Deserialize(customSectionsJson, typeof(List<ReportCustomSection>));

                //Make any updates
                foreach (ReportCustomSection customSection in customSections)
                {
                    //First the additions/updates
                    ReportCustomSection existingSection = report.CustomSections.FirstOrDefault(r => r.ReportCustomSectionId == customSection.ReportCustomSectionId);
                    if (existingSection == null)
                    {
                        //Addition
                        ReportCustomSection newSection = new ReportCustomSection();
                        newSection.Name = customSection.Name;
                        newSection.Description = customSection.Description;
                        newSection.Header = customSection.Header;
                        newSection.Footer = customSection.Footer;
                        newSection.Template = customSection.Template;
                        newSection.Query = customSection.Query;
                        newSection.IsActive = customSection.IsActive;
                        report.CustomSections.Add(newSection);
                    }
                    else
                    {
                        //Update
                        existingSection.StartTracking();
                        existingSection.Name = customSection.Name;
                        existingSection.Description = customSection.Description;
                        existingSection.Header = customSection.Header;
                        existingSection.Footer = customSection.Footer;
                        existingSection.Template = customSection.Template;
                        existingSection.Query = customSection.Query;
                        existingSection.IsActive = customSection.IsActive;
                    }
                }

                //Finally any deletes
                List<ReportCustomSection> sectionsToDelete2 = new List<ReportCustomSection>();
                for (int i = 0; i < report.CustomSections.Count; i++)
                {
                    ReportCustomSection existingSection2 = report.CustomSections[i];
                    if (existingSection2.ReportCustomSectionId > 0 && !customSections.Any(r => r.ReportCustomSectionId == existingSection2.ReportCustomSectionId))
                    {
                        sectionsToDelete2.Add(existingSection2);
                    }
                }
                foreach (ReportCustomSection existingSection2 in sectionsToDelete2)
                {
                    existingSection2.MarkAsDeleted();
                }

                //Commit the changes
                if (this.reportId > 0)
                {
                    reportManager.Report_Update(report, reportFormatIds);
                }
                else
                {
                    reportManager.Report_Insert(report, reportFormatIds);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                //Return to the report list page
                Response.Redirect("Reports.aspx", true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Called when Cancel is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            //Redirect back to the reports page
            Response.Redirect("Reports.aspx");
        }

        /// <summary>
        /// Check the appropriate formats for the report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdFormats_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Make sure we have a data row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBoxEx chkFormat = (CheckBoxEx)e.Row.FindControl("chkFormat");
                if (chkFormat != null)
                {
                    ReportFormat reportFormat = (ReportFormat)e.Row.DataItem;
                    if (this.report == null)
                    {
                        chkFormat.Checked = (reportFormat.ReportFormatId == (int)Report.ReportFormatEnum.Xml || reportFormat.ReportFormatId == (int)Report.ReportFormatEnum.Html);
                    }
                    else
                    {
                        chkFormat.Checked = (this.report.Formats.Any(f => f.ReportFormatId == reportFormat.ReportFormatId));
                    }

                    //All reports have to support XML (since its the raw data)
                    if (reportFormat.ReportFormatId == (int)Report.ReportFormatEnum.Xml)
                    {
                        chkFormat.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the data on the page
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Load in the specific report (include any inactive sections)
                ReportManager reportManager = new ReportManager();

                if (this.reportId > 0)
                {
                    report = reportManager.RetrieveById(this.reportId, true);

                    //Disable the update buttons/operations if this is one of the standard reports
                    if (String.IsNullOrEmpty(report.Token))
                    {
                        //Custom Report
                        this.btnUpdate.Visible = true;
                        this.btnActivate.Visible = false;
                        this.btnDeactivate.Visible = false;
                        this.btnStandardSectionUpdate.Visible = true;
                        this.btnCustomSectionUpdate.Visible = true;
                    }
                    else
                    {
                        //Standard Report
                        this.btnUpdate.Visible = false;
                        this.btnStandardSectionUpdate.Visible = false;
                        this.btnCustomSectionUpdate.Visible = false;
                        if (report.IsActive)
                        {
                            this.btnActivate.Visible = false;
                            this.btnDeactivate.Visible = true;
                        }
                        else
                        {
                            this.btnActivate.Visible = true;
                            this.btnDeactivate.Visible = false;
                        }
                        this.grdStandardSections.ShowFooter = false;
                        this.grdCustomSections.ShowFooter = false;
                    }
                }
                else
                {
                    //All new reports are effectively custom reports
                    this.btnUpdate.Visible = true;
                    this.btnActivate.Visible = false;
                    this.btnDeactivate.Visible = false;
                    this.btnStandardSectionUpdate.Visible = true;
                    this.btnCustomSectionUpdate.Visible = true;
                }

                //Load the list of report formats
                List<ReportFormat> formats = reportManager.ReportFormat_Retrieve();
                this.grdFormats.DataSource = formats;

                //Load the list of report categories
                List<ReportCategory> categories = reportManager.RetrieveCategories();
                this.ddlCategory.DataSource = categories;

                if (this.reportId > 0)
                {
                    //Bind the standard and customs sections
                    this.grdStandardSections.DataSource = report.SectionInstances;
                    this.grdCustomSections.DataSource = report.CustomSections;
                }

                //Load the list of reportable entities (for custom queries)
                Dictionary<string,string> reportableEntities = ReportManager.GetReportableEntities();
                this.ddlCustomSectionQueryNew.DataSource = reportableEntities;

                //Load the list of available standard sections
                this.ddlStandardSectionName.DataSource = reportManager.ReportSection_Retrieve();

                //Databind the page
                this.DataBind();

                //Load the form values
                if (this.reportId > 0)
                {
                    this.lblReportName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(report.Name);
                    this.txtName.Text = report.Name;
                    this.txtDescription.Text = String.IsNullOrEmpty(report.Description) ? "" : report.Description;
                    this.txtHeader.Text = String.IsNullOrEmpty(report.Header) ? "" : report.Header;
                    this.txtFooter.Text = String.IsNullOrEmpty(report.Footer) ? "" : report.Footer;
                    this.chkActive.Checked = report.IsActive;
                    this.ddlCategory.SelectedValue = report.ReportCategoryId.ToString();
                }
                else
                {
                    this.lblReportName.Text = Resources.Main.ReportDetails_NewReport;
                    this.chkActive.Checked = true;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (ArtifactNotExistsException)
            {
                //Return to the report list page
                Response.Redirect("Reports.aspx", true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
