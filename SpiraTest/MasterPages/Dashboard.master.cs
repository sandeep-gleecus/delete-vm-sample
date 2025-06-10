using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.MasterPages
{
    public partial class Dashboard : MasterPageBase
    {
        /// <summary>
        /// Change the ID of the master page to something more meaningful
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "mpDashboard";
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// The URL to the handler that exports images as PNGs
        /// </summary>
        protected string ExportPngUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/JqPlot/GraphImagePng.ashx");
            }
        }
    }
}