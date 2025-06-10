using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Administration
{
	public partial class SystemInfo : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.SystemInfo::";

		/// <summary>
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads the data in the control
		/// </summary>
		public void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			try
			{
				//- Product Type
				lnkLicenseProduct.Text = ConfigurationSettings.Default.License_ProductType + " " +
					"v" + GlobalFunctions.DISPLAY_SOFTWARE_VERSION + "." + GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD.ToString();

				//Link to SysInfo page if not cloud-hosted
				if (Common.Properties.Settings.Default.LicenseEditable)
				{
					lnkLicenseProduct.NavigateUrl = "~/Administration/SystemInfo.aspx";
				}
				else
				{
					lnkLicenseProduct.Enabled = false;
				}

				//Load in the license key information
				License.Load();
				lblOrganization.Text = License.Organization;

				lblLicenseExpiration.Text = Resources.Main.Global_NotApplicable; //Default to no expiration
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
					lblLicenseType.Text = string.Format(Resources.Main.LicenseDetails_EvaluationLicense, License.Number); ;
					lblLicenseExpiration.Text = string.Format(GlobalFunctions.FORMAT_DATE, License.Expiration);
				}

				//Get the number of concurrent users
				lblConcurrentUsers.Text = Global.ConcurrentUsersCount().ToString();

				//Get the number of projects
				ProjectManager projectManager = new ProjectManager();
				int projectsCount = projectManager.Count();
				lnkNumberProjects.Text = projectsCount.ToString() + " " + Resources.Main.Global_Projects;

                //If we can display the Delete Sample Data section
                if (ConfigurationSettings.Default.Database_SampleDataCanBeDeleted)
                {
                    this.plcDeleteSampleData.Visible = true;
                    Dictionary<string, string> handlers = new Dictionary<string, string>();
                    handlers.Add("succeeded", "deleteSampleData_completed");
                    this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);
                }

				//Databind the form
				DataBind();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (Message != null)
				{
					Message.Text = Resources.Messages.Global_UnableToLoad + " '" + Title + "'";
					Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Loads the page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Now load the content
			if (WebPartVisible)
			{
				LoadAndBindData();
			}
		}
	}
}