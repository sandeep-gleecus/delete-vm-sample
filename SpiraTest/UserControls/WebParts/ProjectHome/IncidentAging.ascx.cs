using System;
using System.ComponentModel;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays the incident aging chart
    /// </summary>
    public partial class IncidentAging : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentAging::";

        protected string RedirectBaseUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/GraphRedirect.ashx");
            }
        }

        #region User Configurable Properties

        /// <summary>
        /// Stores the time interval to be used (in # days)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("IncidentAging_AgingIntervalSetting"),
        LocalizedWebDescription("IncidentAging_AgingIntervalSettingTooltip"),
        DefaultValue(15)
        ]
        public int TimeInterval
        {
            get
            {
                return this.timeInterval;
            }
            set
            {
                this.timeInterval = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int timeInterval = 15;

        /// <summary>
        /// Stores the maximum aging that's displayed in the chart
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("IncidentAging_AgingRangeSetting"),
        LocalizedWebDescription("IncidentAging_AgingRangeSettingTooltip"),
        DefaultValue(90)
        ]
        public int MaxAging
        {
            get
            {
                return this.maxAging;
            }
            set
            {
                this.maxAging = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int maxAging = 90;

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
                //We have to set the subtitle and message box programmatically for items that start out in the catalog
                this.Subtitle = Resources.Main.Global_ViewDetails;
                this.MessageBoxId = "lblMessage";

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
            this.hdnMaxAging.Value = MaxAging.ToString();
            this.hdnTimeInterval.Value = TimeInterval.ToString();
        }
    }
}