using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>This webform code-behind class is responsible to displaying theAdministration License Details Page and handling all raised events</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "LicenseDetails_Title", "System/#license-details", "LicenseDetails_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class LicenseDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.LicenseDetails";

		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//If we cannot edit license details, redirect
				if (!Common.Properties.Settings.Default.LicenseEditable)
				{
					Response.Redirect("~/Administration");
					return;
				}

				//Register event handlers
				btnLicenseUpdate.Click += new EventHandler(btnLicenseUpdate_Click);

				//Load the license details
				if (!IsPostBack)
				{
					LoadLicenseDetails();
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>This event handler handles changes to the license key form</summary>
		private void btnLicenseUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnLicenseUpdate_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid) return;

			//Get the organization name and license key from the text boxes
			string organization = txtLicenseOrganization.Text.Trim();
			string licenseKey = txtLicenseKey.Text.Trim();

			//Now see if this is a valid license key
			License.Validate(organization, licenseKey);

			if (License.LicenseType == LicenseTypeEnum.None)
			{
				lblMessage.Text = Resources.Messages.LicenseDetails_ProductOrgLicenseNotValid;
				lblMessage.Type = MessageBox.MessageType.Error;
			}
			else
			{
				//Make sure that license has not expired if not perpetual
				if (License.Expiration.HasValue && License.Expiration < DateTime.UtcNow)
				{
					//Display a different message for evaluation vs. production license key expirations
					if (License.LicenseType == LicenseTypeEnum.Demonstration)
					{
						lblMessage.Text = Resources.Messages.LicenseDetails_EvalLicenseExpired;
						lblMessage.Type = MessageBox.MessageType.Error;
					}
					else
					{
						lblMessage.Text = Resources.Messages.LicenseDetails_LicenseExpired;
						lblMessage.Type = MessageBox.MessageType.Error;
					}
				}
				else
				{
					//Now we need to update the license global configuration settings with the new license information
					ConfigurationSettings.Default.License_Organization = organization;
					ConfigurationSettings.Default.License_LicenseKey = licenseKey;
					ConfigurationSettings.Default.License_ProductType = License.LicenseProductName.ToString();
					ConfigurationSettings.Default.Save();

					//Now refresh the page
					LoadLicenseDetails();

					//Display success
					lblMessage.Text = Resources.Messages.LicenseDetails_Success;
					lblMessage.Type = MessageBox.MessageType.Information;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Loads the license details</summary>
		protected void LoadLicenseDetails()
		{
			//Load in the license key information
			License.Load();

			//Populate the license fields
			txtLicenseOrganization.Text = License.Organization;
			txtLicenseKey.Text = License.LicenseKey;
			ddlLicenseProductText.Text = License.LicenseProductName.ToString();

			lblLicenseExpiration.Text = Resources.Main.Global_NotApplicable; //Default to no N/A
			if (License.LicenseType == LicenseTypeEnum.None)
			{
				lblLicenseType.Text = Resources.Main.LicenseDetails_UnlicensedDoNotUse;
			}
			else if (License.LicenseType == LicenseTypeEnum.ConcurrentUsers)
			{
				lblLicenseType.Text = string.Format(Resources.Main.LicenseDetails_UsersConcurrent, License.Number);
				if (License.Expiration.HasValue)
				{
					lblLicenseExpiration.Text = string.Format(GlobalFunctions.FORMAT_DATE, License.Expiration);
				}
				else
				{
					lblLicenseExpiration.Text = Resources.Main.LicenseDetails_Perpetual;
				}
			}
			else if (License.LicenseType == LicenseTypeEnum.Enterprise)
			{
				lblLicenseType.Text = Resources.Main.LicenseDetails_EnterpriseLicense;
				if (License.Expiration.HasValue)
				{
					lblLicenseExpiration.Text = string.Format(GlobalFunctions.FORMAT_DATE, License.Expiration);
				}
				else
				{
					lblLicenseExpiration.Text = Resources.Main.LicenseDetails_Perpetual;
				}
			}
			else if (License.LicenseType == LicenseTypeEnum.Demonstration)
			{
				lblLicenseType.Text = string.Format(Resources.Main.LicenseDetails_EvaluationLicense, License.Number);
				lblLicenseExpiration.Text = string.Format(GlobalFunctions.FORMAT_DATE, License.Expiration);
			}
			lblLicenseVersion.Text = License.Version;

			//Get the number of concurrent users
			lblConcurrentUsers.Text = Global.ConcurrentUsersCount().ToString();

			//Databind the hyperlinks
			lnkActiveSessions.DataBind();
		}
	}
}