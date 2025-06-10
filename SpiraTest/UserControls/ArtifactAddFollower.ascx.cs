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
    public partial class ArtifactAddFollower : ArtifactUserControlBase, IArtifactUserControl
    {
        #region Properties

        /// <summary>
        /// The id of the message box client id
        /// </summary>
        public string AddFollowerBoxClientId
        {
            get
            {
                if (ViewState["AddFollowerBoxClientId"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["AddFollowerBoxClientId"];
                }
            }
            set
            {
                ViewState["AddFollowerBoxClientId"] = value;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //Add commands and data to Send Notification dialog..
            List<ProjectUser> projectUsers = new ProjectManager().RetrieveUserMembershipById(this.ProjectId, false);
            this.ddlAddProjUser.DataSource = projectUsers;
            this.ddlAddProjUser.DataBind();
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("displayed", "pnlAddFollower_displayed");
            this.pnlAddFollower.SetClientEventHandlers(handlers);
        }

        void IArtifactUserControl.LoadAndBindData(bool dataBind)
        {
        }
    }
}