using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// Displays the administration data-synchronization home page
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "GeneralSettings_Title", "System/#general-settings", "GeneralSettings_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class GeneralSettings : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.GeneralSettings::";

		protected string productName = "";

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Load the page if not postback
			if (!Page.IsPostBack)
			{
				LoadAndBindData();
			}

			//Set one label..
			this.ntsCacheFolder.Text = String.Format(Resources.Main.Admin_GeneralSettings_CacheFolderNotes, Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), this.productName));

			//Add event handlers
			this.btnGeneralUpdate.Click += new EventHandler(btnGeneralUpdate_Click);
			this.btnCancel.Click += new EventHandler(btnCancel_Click);

			//Set the licensed product name (used in several places)
			this.productName = ConfigurationSettings.Default.License_ProductType;
		}

		/// <summary>
		/// Redirect back to the administration home page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnCancel_Click(object sender, EventArgs e)
		{
			Response.Redirect("Default.aspx");
		}

		/// <summary>
		/// Loads and displays the panel's contents when called
		/// </summary>
		protected void LoadAndBindData()
		{
			//If this is a hosted site, don't allow them to update the attachment folder location or use of TaraVault
			if (Common.Properties.Settings.Default.LicenseEditable)
			{
				this.txtGeneralAttachmentsFolder.ReadOnly = false;
				this.txtEmailWebServerUrl.ReadOnly = false;
                this.txtCacheFolder.ReadOnly = false;
                this.plcCloudHosted.Visible = false;

            }
			else
			{
				this.txtGeneralAttachmentsFolder.ReadOnly = true;
				this.txtEmailWebServerUrl.ReadOnly = true;
                this.txtCacheFolder.ReadOnly = true;
                this.plcCloudHosted.Visible = true;
            }

            //Get the list of possible cultures
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			SortedDictionary<string, string> sortedCultures = new SortedDictionary<string, string>();
			foreach (CultureInfo culture in cultures)
			{
				if (!sortedCultures.ContainsKey(culture.Name))
				{
					sortedCultures.Add(culture.Name, culture.DisplayName);
				}
			}
			this.ddlDefaultCulture.DataSource = sortedCultures;

			//Get the list of possible timezones
			ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
			Dictionary<string, string> timeZonesDic = new Dictionary<string, string>();
			foreach (TimeZoneInfo timeZone in timeZones)
			{
				if (!timeZonesDic.ContainsKey(timeZone.Id))
				{
					//The timezone ID contains spaces which we don't want
					timeZonesDic.Add(timeZone.Id.Replace(" ", "-"), timeZone.DisplayName);
				}
			}
			this.ddlDefaultTimezone.DataSource = timeZonesDic;

			//Databind
			this.DataBind();

			//Populate the fields from the global configuration settings
			this.txtEmailWebServerUrl.Text = ConfigurationSettings.Default.General_WebServerUrl;
			this.txtGeneralAttachmentsFolder.Text = ConfigurationSettings.Default.General_AttachmentFolder;
			this.txtCacheFolder.Text = ConfigurationSettings.Default.Cache_Folder;
            this.txtLoginNotice.Text = ConfigurationSettings.Default.General_LoginNotice;
            this.txtAdminMessage.Text = ConfigurationSettings.Default.General_AdminMessage;
			this.ddlDefaultCulture.SelectedValue = ConfigurationSettings.Default.Globalization_DefaultCulture;
			this.ddlDefaultTimezone.SelectedValue = (ConfigurationSettings.Default.Globalization_DefaultTimezone == null) ? "" : ConfigurationSettings.Default.Globalization_DefaultTimezone.Replace(" ", "-");
			this.txtEventDaysToKeep.Text = ConfigurationSettings.Default.Logging_MaximumNumberDaysRetained.ToString();
            this.chkInstantMessenger.Checked = (ConfigurationSettings.Default.Message_Enabled ? true : false);
			this.txtMessageRetention.Text = ConfigurationSettings.Default.Message_RetentionPeriod.ToString();
            this.txtCacheRetention.Text = ConfigurationSettings.Default.Cache_RefreshTime.ToString();
            this.chkEnableFreeTextIndexing.Checked = ConfigurationSettings.Default.Database_UseFreeTextCatalogs;
            this.chkUseTaraVaultForSourceCode.Checked = ConfigurationSettings.Default.SourceCode_UseTaraVaultOnCloud;
			this.chkDisableRollupCalculations.Checked = ConfigurationSettings.Default.General_DisableRollupCalculations;


			//If they are trying to enable free text indexing, make sure SQL Server actually supports it
			if (!ConfigurationSettings.Default.Database_UseFreeTextCatalogs)
            {
                bool supportsFreeTextIndexing = new SystemManager().Database_CheckFreeTextIndexingInstalled();
                this.chkEnableFreeTextIndexing.Enabled = supportsFreeTextIndexing;
            }
        }

		/// <summary>
		/// This event handler updates the stored general system settings
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void btnGeneralUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnGeneralUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure all validators have succeeded
			if (!Page.IsValid)
			{
				return;
			}

			//Make sure the attachment folder exists
			if (!Directory.Exists(this.txtGeneralAttachmentsFolder.Text))
			{
				this.lblMessage.Text = Resources.Messages.GeneralSettings_AttachmentsFolderNotExists;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}

			//Make sure the cache folder exists and is writeable..
			if (!Directory.Exists(this.txtCacheFolder.Text.Trim()))
			{
				try
				{
					//Try creating it..
					DirectoryInfo di = Directory.CreateDirectory(this.txtCacheFolder.Text);
				}
				catch (Exception)
				{
					//Didn't exist. Reset it to default.
					this.txtCacheFolder.Text = Common.Global.CACHE_FOLDERPATH;
				}
			}
			else
			{
				try
				{
					// Attempt to get a list of security permissions from the folder. 
					// This will raise an exception if the path is read only or do not have access to view the permissions. 
					System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(this.txtCacheFolder.Text);
				}
				catch (UnauthorizedAccessException)
				{
					//If we threw an exception, reset it..
					this.txtCacheFolder.Text = Common.Global.CACHE_FOLDERPATH;
				}
			}

            //Update the stored configuration values from the provided information
            int intValue;
            ConfigurationSettings.Default.General_WebServerUrl = this.txtEmailWebServerUrl.Text.Trim();
            ConfigurationSettings.Default.General_AttachmentFolder = this.txtGeneralAttachmentsFolder.Text.Trim();
            ConfigurationSettings.Default.General_LoginNotice = this.txtLoginNotice.Text.Trim();
            ConfigurationSettings.Default.General_AdminMessage = this.txtAdminMessage.Text.Trim();
			ConfigurationSettings.Default.Globalization_DefaultCulture = this.ddlDefaultCulture.SelectedValue;
			ConfigurationSettings.Default.Globalization_DefaultTimezone = this.ddlDefaultTimezone.SelectedValue.Replace("-", " ");
            if (Int32.TryParse(this.txtEventDaysToKeep.Text, out intValue))
            {
                ConfigurationSettings.Default.Logging_MaximumNumberDaysRetained = intValue;
            }
            ConfigurationSettings.Default.Message_Enabled = (this.chkInstantMessenger.Checked);
            if (Int32.TryParse(this.txtMessageRetention.Text, out intValue))
            {
                ConfigurationSettings.Default.Message_RetentionPeriod = intValue;
            }
			ConfigurationSettings.Default.Cache_Folder = this.txtCacheFolder.Text;
            if (Int32.TryParse(this.txtCacheRetention.Text, out intValue))
            {
                ConfigurationSettings.Default.Cache_RefreshTime = intValue;
            }
            ConfigurationSettings.Default.Database_UseFreeTextCatalogs = this.chkEnableFreeTextIndexing.Checked;
            ConfigurationSettings.Default.SourceCode_UseTaraVaultOnCloud = this.chkUseTaraVaultForSourceCode.Checked;
			ConfigurationSettings.Default.General_DisableRollupCalculations = this.chkDisableRollupCalculations.Checked;
			ConfigurationSettings.Default.Save();

			//Let the user know that the settings were saved
			this.lblMessage.Text = Resources.Messages.GeneralSettings_Success;
			this.lblMessage.Type = MessageBox.MessageType.Information;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
