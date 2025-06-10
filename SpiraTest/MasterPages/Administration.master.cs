using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.MasterPages
{
    public partial class Administration : MasterPageBase
    {
        /// <summary>
        /// Change the ID of the master page to something more meaningful
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "mpAdministration";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
