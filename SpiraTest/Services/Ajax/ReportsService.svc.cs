using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System.Xml;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to display the navigation bar for the reports module
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class ReportsService : AjaxWebServiceBase, IReportsService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ReportsService::";

        #region IReportService methods

        /// <summary>
        /// Retrieves the description for a given standard section
        /// </summary>
        /// <param name="reportSectionId">The id of the standard section</param>
        /// <returns>The description</returns>
        public string Reports_RetrieveSectionDescription(int reportSectionId)
        {
            const string METHOD_NAME = "Reports_RetrieveSectionDescription";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!this.UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the report section
                ReportManager reportManager = new ReportManager();
                ReportSection reportSection = reportManager.ReportSection_RetrieveById(reportSectionId);
                if (reportSection == null)
                {
                    return "";
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return reportSection.Description;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the default XSLT template for a given standard section
        /// </summary>
        /// <param name="reportSectionId">The id of the standard section</param>
        /// <returns>The default XSLT</returns>
        public string Reports_RetrieveSectionDefaultTemplate(int reportSectionId)
        {
            const string METHOD_NAME = "Reports_RetrieveSectionDefaultTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!this.UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the report section
                ReportManager reportManager = new ReportManager();
                ReportSection reportSection = reportManager.ReportSection_RetrieveById(reportSectionId);
                if (reportSection == null)
                {
                    return "";
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return reportSection.DefaultTemplate;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the XML data retrieved when executing the specified query
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="sql">The query to be executed</param>
        /// <returns>The XML document serialized as text</returns>
        /// <remarks>
        /// This command is only used by sys-admins on the Report administration screens
        /// </remarks>
        public string Reports_RetrieveCustomQueryData(int projectId, string sql)
        {
            const string METHOD_NAME = "Reports_RetrieveCustomQueryData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!this.UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Make sure we're a member of the project, if a project is specified
            if (projectId > 0)
            {
                Project.AuthorizationState authorizationState = IsAuthorized(projectId);
                if (authorizationState == Project.AuthorizationState.Prohibited)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }
            }

            try
            {
                Business.ProjectManager projectManager = new Business.ProjectManager();
                //If the user doesn't have a project selected, just grab the first active project that the user
                //is a member of
                if (projectId < 1)
                {
                    List<ProjectForUserView> projects = projectManager.RetrieveForUser(this.CurrentUserId.Value);
                    if (projects.Count > 0)
                    {
                        projectId = projects[0].ProjectId;
                    }
                }

                //Get the project group id if we have a project selected
                int projectGroupId = -1;
                if (projectId > 0)
                {
                    Project project = projectManager.RetrieveById(projectId);
                    projectGroupId = project.ProjectGroupId;
                }

                //Execute the query, passing the project id and project group id
                XmlDocument xmlDoc = new ReportManager().ReportCustomSection_ExecuteSQL(projectId, projectGroupId, sql, null, 0, 10);

                //Next we need to convert this into HTML that can be displayed in the results container
                string html = "<table class=\"DataGrid\">";
                XmlNodeList xmlRows = xmlDoc.SelectNodes("/RESULTS/ROW");
                bool isFirstRow = true;
                foreach(XmlNode xmlRow in xmlRows)
                {
                    //If this is the first row, add the header row
                    if (isFirstRow)
                    {
                        html += "<tr class=\"Header\">";
                        foreach (XmlNode xmlField in xmlRow.ChildNodes)
                        {
                            html += "<th>" + HttpUtility.HtmlEncode(xmlField.Name) + "</th>";
                        }
                        html += "</tr>";
                        isFirstRow = false;
                    }

                    html += "<tr>";
                    foreach (XmlNode xmlField in xmlRow.ChildNodes)
                    {
                        html += "<td>" + HttpUtility.HtmlEncode(xmlField.InnerText) + "</td>";
                    }
                    html += "</tr>";
                }

                html += "</table>";
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return html;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the default XSLT template for the specified query
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="sql">The query to be executed</param>
        /// <returns>The XSLT template serialized as text</returns>
        /// <remarks>
        /// This command is only used by sys-admins on the Report administration screens
        /// </remarks>
        public string Reports_RetrieveCustomQueryTemplate(int projectId, string sql)
        {
            const string METHOD_NAME = "Reports_RetrieveCustomQueryData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!this.UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Make sure we're a member of the project, if a project is specified
            if (projectId > 0)
            {
                Project.AuthorizationState authorizationState = IsAuthorized(projectId);
                if (authorizationState == Project.AuthorizationState.Prohibited)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }
            }

            try
            {
                Business.ProjectManager projectManager = new Business.ProjectManager();
                //If the user doesn't have a project selected, just grab the first active project that the user
                //is a member of
                if (projectId < 1)
                {
                    List<ProjectForUserView> projects = projectManager.RetrieveForUser(this.CurrentUserId.Value);
                    if (projects.Count > 0)
                    {
                        projectId = projects[0].ProjectId;
                    }
                }

                //Get the project group id if we have a project selected
                int projectGroupId = -1;
                if (projectId > 0)
                {
                    Project project = projectManager.RetrieveById(projectId);
                    projectGroupId = project.ProjectGroupId;
                }

                //Execute the query, passing the project id and project group id
                XmlDocument xmlDoc = new ReportManager().ReportCustomSection_ExecuteSQL(projectId, projectGroupId, sql, null, 0, 10);

                //Next we need to generate the XSLT that displays this data in a report
                string html =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" exclude-result-prefixes=""msxsl"">
  <xsl:template match=""/RESULTS"">
    <table class=""DataGrid"">";

                XmlNodeList xmlRows = xmlDoc.SelectNodes("/RESULTS/ROW");
                //We only need one row
                if (xmlRows.Count > 0)
                {
                    XmlNode xmlRow = xmlRows[0];
                    //First add the header rows
                    html += "<tr>";
                    foreach (XmlNode xmlField in xmlRow.ChildNodes)
                    {
                        html += "<th>" + HttpUtility.HtmlEncode(xmlField.Name) + "</th>";
                    }
                    html += "</tr>";

                    //Now add the actual XPATH selector for the data rows
                    html += @"
      <xsl:for-each select=""ROW"">
        <tr>";
                    foreach (XmlNode xmlField in xmlRow.ChildNodes)
                    {
                        html += "<td><xsl:value-of select=\"" + HttpUtility.HtmlEncode(xmlField.Name) + "\"/></td>";
                    }
                    html += @"
        </tr>
      </xsl:for-each>";
                }

                html += @"
        </table>
    </xsl:template>
</xsl:stylesheet>";

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return html;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region INavigationService methods

        /// <summary>
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">Not used for this service</param>
        /// <param name="displayWidth">The display width</param>
        /// <param name="minimized">Is the navigation bar minimized or visible</param>
        public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
        {
            const string METHOD_NAME = "NavigationBar_UpdateSettings";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view the project
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REPORTS_GENERAL_SETTINGS);
                if (minimized.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED] = minimized.Value;
                    changed = true;
                }
                if (displayWidth.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH] = displayWidth.Value;
                    changed = true;
                }
                if (changed)
                {
                    settings.Save();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Not used because we only currently have to support the SidebarPanel not the NavigationBar
        /// </summary>
        public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
        {
            throw new NotImplementedException();
        }

        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            throw new NotImplementedException();
        }

        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
