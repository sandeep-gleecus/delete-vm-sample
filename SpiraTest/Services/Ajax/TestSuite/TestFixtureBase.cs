using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.Ajax.TestSuite
{
    public class TestFixtureBase : System.Web.UI.Page
    {
        /// <summary>
        /// When the page loads we need to authenticate the user first so that the various
        /// JSON AJAX web services can be called, since they are secured by forms authentication
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //First call the base functionality
            base.OnLoad(e);

            //We log in as fred bloggs to access the system
            FormsAuthentication.SetAuthCookie("fredbloggs", false);
        }
    }
}
