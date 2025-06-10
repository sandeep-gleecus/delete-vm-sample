using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays the incident open count by priority/severity
    /// </summary>
    public partial class IncidentOpenCount : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentOpenCount::";

        protected string RedirectBaseUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/GraphRedirect.ashx");
            }
        }

        #region Enumerations

        public enum IncidentCountOrganizeBy
        {
            Priority = 1,
            Severity = 2
        }

        public enum IncidentCountReleaseType
        {
            DetectedRelease = 1,
            ResolvedRelease = 2
        }

        #endregion

        #region User Configurable Properties

        /// <summary>
        /// Determines whether to display priority or severity on the x-axis
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_UsePrioritySeveritySetting"),
        LocalizedWebDescription("Global_UsePrioritySeveritySettingTooltip")
        ]
        public IncidentCountOrganizeBy OrganizeBy
        {
            get
            {
                return this.organizeBy;
            }
            set
            {
                this.organizeBy = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected IncidentCountOrganizeBy organizeBy = IncidentCountOrganizeBy.Priority;

        /// <summary>
        /// Determines whether to filter by detected release or resolved release
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_ChooseReleaseTypeSetting"),
        LocalizedWebDescription("Global_ChooseReleaseTypeSettingTooltip")
        ]
        public IncidentCountReleaseType ReleaseType
        {
            get
            {
                return this.releaseType;
            }
            set
            {
                this.releaseType = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected IncidentCountReleaseType releaseType = IncidentCountReleaseType.DetectedRelease;

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
                //We have to set the subtitle programmatically for items that start out in the catalog
                this.Subtitle = Resources.Main.Global_ViewDetails;

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
        /// Loads the control data
        /// </summary>
        public void LoadAndBindData()
        {
            int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);
            if (releaseId.HasValue && releaseId < 0)
            {
                releaseId = null;
            }

            //Set the release field for the graph
            if (releaseId < 1)
            {
                this.hdnReleaseId.Value = "";
            }
            else
            {
                this.hdnReleaseId.Value = releaseId.ToString();
            }

            //Set the other values
            this.hdnUseSeverity.Value = (OrganizeBy == IncidentCountOrganizeBy.Severity).ToString().ToLowerInvariant();
            this.hdnUseResolvedRelease.Value = (ReleaseType == IncidentCountReleaseType.ResolvedRelease).ToString().ToLowerInvariant();
        }
    }
}