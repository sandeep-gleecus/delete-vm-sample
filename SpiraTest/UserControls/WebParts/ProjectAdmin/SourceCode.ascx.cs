using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin
{
    /// <summary>
    /// Displays the source code information for the project
    /// </summary>
    public partial class SourceCode : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.SourceCode::";

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
            //See if we have TaraVault or external source code
            if (Common.Global.Feature_TaraVault)
            {
                //TaraVault

                //See if TaraVault enabled
                if (ConfigurationSettings.Default.TaraVault_HasAccount)
                {
                    VaultManager tvManager = new VaultManager();
                    Project project = tvManager.Project_RetrieveWithTaraVault(ProjectId);
                    if (project.TaraVault != null && project.TaraVault.VaultType != null)
                    {
                        this.lnkProvider.Text = Encoder.HtmlEncode(project.TaraVault.VaultType.Name);
                        this.lnkProvider.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "TaraVaultProjectSettings");
                        this.ltrActive.Text = GlobalFunctions.DisplayYnFlag(true);
                        this.ltrConnectionInfo.Text = tvManager.Project_GetConnectionString(ProjectId);
                    }
                    else
                    {
                        //Not activated
                        this.lnkProvider.Text = Resources.Main.Global_NotApplicable;
                        this.ltrConnectionInfo.Text = Resources.Main.Global_NotApplicable;
                        this.ltrActive.Text = GlobalFunctions.DisplayYnFlag(false);
                    }
                }
                else
                {
                    //Not activated
                    this.lnkProvider.Text = Resources.Main.Global_NotApplicable;
                    this.ltrConnectionInfo.Text = Resources.Main.Global_NotApplicable;
                    this.ltrActive.Text = GlobalFunctions.DisplayYnFlag(false);
                }
            }
            else
            {
                //External
                SourceCodeManager sourceCodeManager = new SourceCodeManager();
                List<VersionControlProject> versionControlProjects = sourceCodeManager.RetrieveProjectSettings(ProjectId, true, true);
                if (versionControlProjects.Count < 1)
                {
                    //None configured
                    this.lnkProvider.Text = Resources.Main.Global_NotApplicable;
                    this.ltrConnectionInfo.Text = Resources.Main.Global_NotApplicable;
                    this.ltrActive.Text = GlobalFunctions.DisplayYnFlag(false);
                }
                else
                {
                    //Use the first one
                    VersionControlProject vcp = versionControlProjects[0];
                    this.lnkProvider.Text = Encoder.HtmlEncode(vcp.VersionControlSystem.Name);
                    this.lnkProvider.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "VersionControlProjectSettings") + "?" + GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID + "=" + vcp.VersionControlSystemId;
                    this.ltrActive.Text = GlobalFunctions.DisplayYnFlag(vcp.IsActive);
                    if (String.IsNullOrEmpty(vcp.ConnectionString))
                    {
                        this.ltrConnectionInfo.Text = vcp.VersionControlSystem.ConnectionString;
                    }
                    else
                    {
                        this.ltrConnectionInfo.Text = vcp.ConnectionString;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
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
    }
}