using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Security.Application;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin
{
    public partial class ProjectTemplateOverview : WebPartBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin.ProjectTemplateOverview::";

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
                //Now load the content
                if (!IsPostBack)
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
        /// Loads the control data
        /// </summary>
        protected void LoadAndBindData()
        {
            try
            {
                //Retrieve the template
                TemplateManager templateManager = new TemplateManager();
                ProjectTemplate projectTemplate = templateManager.RetrieveById(ProjectTemplateId);
                if (projectTemplate == null)
                {
                    //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                    if (this.Message != null)
                    {
                        this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                        this.Message.Type = MessageBox.MessageType.Error;
                    }
                    return;
                }

                if (String.IsNullOrEmpty(projectTemplate.Description))
                {
                    this.lblProjectTemplateDescription.Text = "";
                }
                else
                {
                    this.lblProjectTemplateDescription.Text = projectTemplate.Description;
                }

                //Get the template admins
                Business.UserManager userManager = new Business.UserManager();
                List<DataModel.User> templateAdmins = userManager.RetrieveProjectTemplateAdmins(ProjectTemplateId);
                this.rptTemplateAdmins.DataSource = templateAdmins;
                this.rptTemplateAdmins.DataBind();

				//show in the ui if bulk change of status is enabled/disabled
				ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(ProjectTemplateId);
				this.lblCanBulkChangeStatus.Text = GlobalFunctions.DisplayYnFlag(templateSettings.Workflow_BulkEditCanChangeStatus);
			}
			catch (ArtifactNotExistsException)
            {
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
