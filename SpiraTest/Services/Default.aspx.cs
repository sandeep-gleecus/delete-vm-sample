using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services
{
    public partial class Default : PageBase
    {
        /// <summary>
        /// Loads any dynamic content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Display the current product name in the title
            this.Title = ConfigurationSettings.Default.License_ProductType + ": Web Services";
            this.ltrProductName.Text = ConfigurationSettings.Default.License_ProductType;

			//Hide link to ODATA if not allowed by the application
			this.trOdata.Visible = Common.Global.Feature_OData;

		}
    }
}
