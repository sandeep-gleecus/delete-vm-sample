using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls
{
	public partial class ArtifactEmail : ArtifactUserControlBase, IArtifactUserControl
    {
        #region Properties

        /// <summary>
        /// The id of the message box client id
        /// </summary>
        public string MessageBoxClientId
        {
            get
            {
                if (ViewState["MessageBoxClientId"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["MessageBoxClientId"];
                }
            }
            set
            {
                ViewState["MessageBoxClientId"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Returns the subscribe button text
        /// </summary>
        protected string SubscribeText
        {
            get
            {
                return Resources.Buttons.Subscribe;
            }
        }

        /// <summary>
        /// Returns the unsubscribe button text
        /// </summary>
        protected string UnSubscribeText
        {
            get
            {
                return Resources.Buttons.Unsubscribe;
            }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			//Add commands and data to Send Notification dialog..
			List<ProjectUser> projectUsers = new ProjectManager().RetrieveUserMembershipById(this.ProjectId, false);
            this.ddlEmailProjUsers.DataSource = projectUsers;
			this.ddlEmailProjUsers.DataBind();
			this.txbSendEmailToAddressList.Attributes.Add("onChange", "txbSendEmailToAddressList_Changed(event)");
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("displayed", "pnlSendEmail_displayed");
			this.pnlSendEmail.SetClientEventHandlers(handlers);
		}

		void IArtifactUserControl.LoadAndBindData(bool dataBind)
		{
		}
	}
}