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

using Inflectra.SpiraTest.Common;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Web.MasterPages
{
    /// <summary>
    /// This is the master page for all the SpiraTest login pages
    /// </summary>
    public partial class Login : MasterPageBase
    {
        /// <summary>
        /// Change the ID of the master page to something more meaningful
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "mpLogin";
        }

        /// <summary>
        /// Called when the page first loads
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //See if we've been passed a title directly
            if (this.PageTitle != "")
            {
                this.Page.Title = Resources.Main.Global_WelcomeTo + " " + Common.ConfigurationSettings.Default.License_ProductType + " | " + this.PageTitle;
            }
            else
            {
                this.Page.Title = Resources.Main.Global_WelcomeTo + " " + Common.ConfigurationSettings.Default.License_ProductType;
            }

            //Set the name and version of the product
            this.ltrProductName.Text = Common.ConfigurationSettings.Default.License_ProductType;
            if (String.IsNullOrEmpty(Common.ConfigurationSettings.Default.License_ProductType))
            {
                this.imgProductIcon.ImageUrl = "Images/product-" + "SpiraTest.svg";
                this.imgProductIcon.AlternateText = "Spira";
            }

			// BEGIN PCS
			//this.imgProductIcon.ImageUrl = "Images/product-" + Common.ConfigurationSettings.Default.License_ProductType + ".svg";
			this.imgProductIcon.ImageUrl = "Images/product-validationmaster.svg";
			//END PCS
			this.imgProductIcon.AlternateText = Common.ConfigurationSettings.Default.License_ProductType;
            this.ltrVersionNumber.Text = GlobalFunctions.DISPLAY_SOFTWARE_VERSION;
            this.ltrBuildNumber.Text = GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD.ToString();

            //Display the copyright and licensee
            this.lblCopyrightYear.Text = GlobalFunctions.CopyrightYear;
            string organization = Common.ConfigurationSettings.Default.License_Organization;
            if (String.IsNullOrEmpty(organization))
            {
                this.lblOrganization.Text = Resources.Messages.Login_UnlicensedProduct;
            }
            else
            {
                this.lblOrganization.Text = GlobalFunctions.SafeSubstring(organization, 35);
            }

            //Display the SEO links depending on the license
			//PCS
            if (Common.ConfigurationSettings.Default.License_ProductType == "ValidationMaster")
            {
                this.plcSpiraTestSEO.Visible = true;
				this.plcSpiraPlanSEO.Visible = true;
				this.plcSpiraTeamSEO.Visible = true;
			}
        }
    }
}
