using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using System.Web;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used to hold any global Ajax web services that don't fit anywhere else
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class GlobalService : AjaxWebServiceBase, IGlobalService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.GlobalService::";

        #region TabControl Methods

        /// <summary>
        /// Updates which tab should be displayed by a specific tab control (stored in user settings)
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="controlId">The id of the control</param>
        /// <param name="selectedTab">the name of the selected tab</param>
        public void TabControl_UpdateState(string pageId, string controlId, string selectedTab)
        {
            const string METHOD_NAME = "TabControl_UpdateState";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the user setting (based on page id + control id)
                string entryKey = pageId + "-" + controlId;
                UserSettingsCollection settingsCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_TAB_CONTROL_STATE);
                settingsCollection[entryKey] = selectedTab;
                settingsCollection.Save();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region CollapsiblePanel Methods

        /// <summary>
        /// Determines whether a collapsible panel should be expanded or collapsed based on user settings
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="panelId">The id of the panel</param>
        /// <returns>The state value</returns>
        public bool CollapsiblePanel_RetrieveState(string pageId, string panelId)
        {
            const string METHOD_NAME = "CollapsiblePanel_RetrieveState";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the user setting (based on page id + panel id)
                string entryKey = pageId + "-" + panelId;
                bool isCollapsed = GetUserSetting(userId, GlobalFunctions.USER_SETTINGS_COLLAPSIBLE_PANEL_STATE, entryKey, false);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return isCollapsed;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a collection of collapsible panels for a page should be expanded or collapsed based on user settings
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <returns>The state values</returns>
        public List<KeyValuePair> CollapsiblePanel_RetrieveStateAll(string pageId)
        {
            const string METHOD_NAME = "CollapsiblePanel_RetrieveStateAll";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the user setting (based on page id + panel id)
                string entryKey = pageId + "-";
                UserSettingsCollection panelStateCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_COLLAPSIBLE_PANEL_STATE);

                //Create a simplified user list to add releveant data from the above into
                List<KeyValuePair> pageStates = new List<KeyValuePair>();

                foreach (DictionaryEntry dic in panelStateCollection)
                {
                    string key = (string)dic.Key;
                    bool isCollapsed = (bool)dic.Value;
                    if (key.StartsWith(entryKey))
                    {
                        KeyValuePair pageState = new KeyValuePair();
                        pageState.Key = key.Substring(entryKey.Length);
                        pageState.Value = isCollapsed.ToDatabaseSerialization();

                        pageStates.Add(pageState);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return pageStates;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates whether a collapsible panel should be expanded or collapsed (stored in user settings)
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="panelId">The id of the panel</param>
        /// <param name="isCollapsed">the state of the expander</param>
        public void CollapsiblePanel_UpdateState(string pageId, string panelId, bool isCollapsed)
        {
            const string METHOD_NAME = "CollapsiblePanel_UpdateState";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the user setting (based on page id + panel id)
                string entryKey = pageId + "-" + panelId;
                UserSettingsCollection settingsCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_COLLAPSIBLE_PANEL_STATE);
                settingsCollection[entryKey] = isCollapsed;
                settingsCollection.Save();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region NavigationBar Methods

        /// <summary>
        /// Not used because we only currently have to support the SidebarPanel not the NavigationBar
        /// </summary>
        public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not used because we only currently have to support the SidebarPanel not the NavigationBar
        /// </summary>
        public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
        {
            throw new NotImplementedException();
        }

        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the display settings used by the Navigation Bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayMode">The current display mode</param>
        /// <param name="displayWidth">The display width</param>
        /// <param name="minimized">Is the navigation bar minimized or visible</param>
        public void NavigationBar_UpdateSettings(int projectId, Nullable<int> displayMode, Nullable<int> displayWidth, Nullable<bool> minimized)
        {
            const string METHOD_NAME = "NavigationBar_UpdateSettings";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the user's project settings
                bool changed = false;
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL);
                if (displayMode.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = displayMode.Value;
                    changed = true;
                }
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

        #endregion

        #region UserSettings Methods

         /// <summary>
        /// Returns true if the passed in tour has been seen
        /// </summary>
        public bool UserSettings_GuidedTour_RetrieveSeen(string tour)
        {
            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //First get the guided tour settings collection
            UserSettingsCollection guidedTourCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);

            //Then define the new user settings list
            bool guidedTour = false;

            if (guidedTourCollection[tour] is bool)
            {
                guidedTour = (bool)guidedTourCollection[tour];
            }

            return guidedTour;
        }

        /// <summary>
        /// Set the user setting for a particular string tour to the current UTC date in format YYYY-MM-DD
        /// </summary>
        public void UserSettings_GuidedTour_SetSeen(string tour)
        {
            const string METHOD_NAME = "UserSettings_GuidedTour_SetSeen";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            
            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Validate that the tour name only has allowed characters (to prevent XSS injection)
                Regex regex = new Regex(GlobalFunctions.VALIDATION_REGEX_TOUR_NAME);
                if (regex.IsMatch(tour))
                {
                    //Update the user's project settings
                    UserSettingsCollection guidedTourCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);

                    //Use the current date
                    string nowString = DateTime.UtcNow.ToString("yyyy-MM-dd"); ;

                    guidedTourCollection[tour] = nowString;
                    guidedTourCollection.Save();
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
        /// Set the user setting for a particular string navigation link (eg MyPage, or Requirements) to increment by 1
        /// </summary>
        public void UserSettings_GuidedTour_SetNavigationLinkCount(string navigationCount)
        {
            const string METHOD_NAME = "UserSettings_GuidedTour_SetNavigationLinkCount";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Validate that the tour name only has allowed characters (to prevent XSS injection)
                Regex regex = new Regex(GlobalFunctions.VALIDATION_REGEX_TOUR_NAME);
                if (regex.IsMatch(navigationCount))
                {
                    //Update the user's project settings
                    UserSettingsCollection guidedTourCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);

                    //See if we have a link count stored - ie has the user visited this page before
                    string linkText = guidedTourCollection.ContainsKey(navigationCount) ? guidedTourCollection[navigationCount].ToString() : null;
                    int linkCount = 1;
                    // if we have a previous value it should be an int so try and increment it
                    if (!string.IsNullOrWhiteSpace(linkText))
                    {
                        int.TryParse(linkText, out linkCount);
                        linkCount = linkCount + 1;
                    }
                    guidedTourCollection[navigationCount] = linkCount;

                    guidedTourCollection.Save();
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
        /// Set the user setting for a color mode (eg dark or light)
        /// </summary>
        public void UserSettings_ColorMode_Set(string colorMode)
        {
            const string METHOD_NAME = "UserSettings_ColorMode_Set";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Make sure it's only one of the allowed values, to avoid XSS injection
                colorMode = colorMode.ToLowerInvariant().Trim();
                if (colorMode == "auto" || colorMode == "dark" || colorMode == "light")
                {
                    //Update the user's project settings
                    UserSettingsCollection userProfileCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
                    userProfileCollection[GlobalFunctions.USER_SETTINGS_KEY_CURRENT_COLOR_MODE] = colorMode;
                    userProfileCollection.Save();
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
        /// Updates the user settings to store the currently viewed artifact, used to display recent artifacts on the My Page
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="artifactTypeId">The ID of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        public void UserSettings_UpdateRecentArtifact(int projectId, int artifactTypeId, int artifactId)
        {
            const string METHOD_NAME = "UserSettings_UpdateRecentArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Make sure we're authorized (limited is OK)
                Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
                if (authorizationState != Project.AuthorizationState.Prohibited)
                {
					//Add/Update the recently accessed artifact list
					new UserManager().AddUpdateRecentArtifact(userId, projectId, artifactTypeId, artifactId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                //Fail quietly
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }
        }

        #endregion

        #region URL Retrieval methods

        /// <summary>
        /// Returns the URL for one of the 'standard reports' with all elements and sections displayed
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="reportToken">The report to launch</param>
        /// <remarks>This is used when clicking the 'Print' button various artifact screens</remarks>
        /// <param name="filter">The report filterpart of the querystring</param>
        /// <param name="reportFormatId">The format (default=HTML)</param>
        public string Global_GetStandardReportUrl(int projectId, string reportToken, string filter, int? reportFormatId)
        {
            const string METHOD_NAME = "Global_GetStandardReportUrl";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for the project (limited View is not OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get all the elements for this report and turn them on
                ReportManager reportManager = new ReportManager();
                Report report = reportManager.RetrieveByToken(reportToken);
                string reportElements = "";
                foreach (ReportSectionInstance reportSectionInstance in report.SectionInstances)
                {
                    int reportSectionId = reportSectionInstance.ReportSectionId;
                    foreach (ReportElement reportElement in reportSectionInstance.Section.Elements)
                    {
                        int reportElementId = reportElement.ReportElementId;
                        reportElements += "&e_" + reportSectionId + "_" + reportElementId + "=1";
                    }
                }

                //The default format is HTML
                if (!reportFormatId.HasValue)
                {
                    reportFormatId = (int)Report.ReportFormatEnum.Html;
                }

                //Create the URL for the Reports Viewer window with the configuration parameters passed in the querystring
                string url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, projectId, report.ReportId, GlobalFunctions.PARAMETER_TAB_REPORT_VIEWER);
                url += "?" + GlobalFunctions.PARAMETER_REPORT_FORMAT_ID + "=" + reportFormatId.Value + reportElements + filter;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return UrlRewriterModule.ResolveUrl(HttpContext.Current.Request.ApplicationPath, url);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion
    }
}
