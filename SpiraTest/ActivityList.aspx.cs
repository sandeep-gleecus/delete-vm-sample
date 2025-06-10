using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Project Source Code Revision List and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.ProjectHome, "ActivityList_Title", null)]
    public partial class ActivityList : PageLayout
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Set the context on the grid
            this.grdHistory.ProjectId = ProjectId;

            //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
            this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
            this.btnEnterCatch.Attributes.Add("onclick", "return false;");
            this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");

            //Set the return URL
            this.lnkProjectHomeText.Text = Resources.Main.Global_BackTo + " " + Resources.Main.SiteMap_ProjectHome;
            this.lnkProjectHome.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId);
        }
    }
}