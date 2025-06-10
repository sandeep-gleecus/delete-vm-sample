using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This page displays the error message if there is an invalid license
	/// or the number of concurrent users has been exceeded
	/// </summary>
	public partial class InvalidLicense : PageBase
	{
		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//Populate the error message depending on what license key error is detected
			if (Request.QueryString[GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE] == null)
			{
                this.lblMessage.InnerHtml = Resources.Messages.InvalidLicense_InvalidLicenseOnServer;
			}
			else
			{
				if (Request.QueryString[GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE].ToLower(System.Globalization.CultureInfo.InvariantCulture) == "exceededconcurrent")
				{
                    this.lblMessage.InnerHtml = Resources.Messages.InvalidLicense_ExceededConcurrent;
				}
				else if (Request.QueryString[GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE].ToLower(System.Globalization.CultureInfo.InvariantCulture) == "demonstrationexpired")
				{
                    this.lblMessage.InnerHtml = Resources.Messages.InvalidLicense_EvaluationExpired;
				}
				else if (Request.QueryString[GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE].ToLower(System.Globalization.CultureInfo.InvariantCulture) == "licensenexpired")
				{
					this.lblMessage.InnerHtml = Resources.Messages.InvalidLicense_LicenseExpired;
				}
				else
				{
                    this.lblMessage.InnerHtml = Resources.Messages.InvalidLicense_InvalidLicenseOnServer;
				}
			}
		}

		#endregion
	}
}
