using Inflectra.SpiraTest.Installer.ControlForm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlLicenseKey.xaml</summary>
	public partial class cntrlLicenseKey : UserControl, IProcedureComponent
	{
		public cntrlLicenseKey()
		{
			InitializeComponent();

			//Clear any initial information
			ClearLicenseInfo();

			//Hook up event to the text boxes.
			txbLicenseKey.TextChanged += txbLicense_TextChanged;
			txbOrganization.TextChanged += txbLicense_TextChanged;
		}

		private void txbLicense_TextChanged(object sender, TextChangedEventArgs e)
		{
			ValidateLicense(false);
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.LicenseKey_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlLicenseKey";
		public bool AllowBack => true;
		public bool AllowNext => true;
		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				bool ret = true;

				//We only need them to enter their license info in:
				//  - NewInstall, Upgrade & SkipLic command line false.
				ret = (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade);

				ret = ret &&
					!((App._installationOptions.CommandLine?.SkipLicenseCheck).HasValue
						? App._installationOptions.CommandLine.SkipLicenseCheck
						: false
					);

				return ret;
			}
		}

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid()
		{
			return ValidateLicense(true);
		}
		#endregion

		/// <summary>Clears the license info</summary>
		private void ClearLicenseInfo()
		{
			//txbOrganization.Text = "";
			//txbLicenseKey.Text = "";
			txtProductName.Content = "";
			txtExpiration.Content = "";
			txtLicenseType.Content = "";
			txbOrganization.Tag = null;
			txbLicenseKey.Tag = null;
		}

		/// <summary>Called when the user wants to validate the license key</summary>
		private void btnValidate_Click(object sender, RoutedEventArgs e)
		{
			ValidateLicense(true);
		}

		/// <summary>Validates the information given.</summary>
		private bool ValidateLicense(bool alert)
		{
			bool retValue = false;

			//Get the organization and license key
			string organization = txbOrganization.Text.Trim();
			string licenseKey = txbLicenseKey.Text.Trim();

			//Check that bothvalues are filled out.
			if (!string.IsNullOrWhiteSpace(organization) && !string.IsNullOrWhiteSpace(licenseKey))
			{
				//Check the license and get the information
				License license = new License();
				bool isValid = license.Validate(organization, licenseKey);

				//See if valid
				if (isValid)
				{
					//See if expired
					if (license.Expiration.HasValue && license.Expiration <= DateTime.UtcNow)
					{
						if (alert)
						{
							MessageBox.Show(
								Themes.Inflectra.Resources.LicenseKey_LicenseIsExpired,
								Themes.Inflectra.Resources.LicenseKey_LicenseIsExpiredCaption,
								MessageBoxButton.OK,
								MessageBoxImage.Exclamation);
							txbOrganization.Tag = "1";
							txbLicenseKey.Tag = "1";
						}
						retValue = false;
					}
					else
					{
						//Display the license info.
						// - Expiration date.
						if (license.Expiration.HasValue)
							txtExpiration.Content = license.Expiration.Value.ToLongDateString();
						else
							txtExpiration.Content = Themes.Inflectra.Resources.LicenseKey_Perpetual;

						//The level of Spira.
						txtProductName.Content = license.LicenseProductName.ToString();

						//The user licenses.
						switch (license.LicenseType)
						{
							case LicenseTypeEnum.ConcurrentUsers:
								txtLicenseType.Content = license.Number.ToString() + " Concurrent Users";
								break;
							case LicenseTypeEnum.Demonstration:
								txtLicenseType.Content = license.Number.ToString() + " Trial Users";
								break;
							case LicenseTypeEnum.Enterprise:
								txtLicenseType.Content = "Enterprise (Unlimited Users)";
								break;
							case LicenseTypeEnum.NamedUsers:
								txtLicenseType.Content = license.Number.ToString() + " Named Users";
								break;
							case LicenseTypeEnum.None:
								txtLicenseType.Content = "No License, Unuseable";
								break;
						}

						//Clear error status.
						txbOrganization.Tag = null;
						txbLicenseKey.Tag = null;

						//Also store in the installation options
						App._installationOptions.ProductName = license.LicenseProductName.ToString();
						App._installationOptions.LicenseKey = license.LicenseKey;
						App._installationOptions.Organization = license.Organization;
						retValue = true;
					}
				}
				else
				{
					ClearLicenseInfo();
					if (alert)
					{
						MessageBox.Show(Themes.Inflectra.Resources.LicenseKey_LicenseIsInvalid, Themes.Inflectra.Resources.LicenseKey_LicenseIsInvalidCaption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						txbOrganization.Tag = "1";
						txbLicenseKey.Tag = "1";
					}
					retValue = false;
				}
			}
			else
			{
				//They, uh, need to enter things in.
				if (alert)
				{
					MessageBox.Show(Themes.Inflectra.Resources.LicenseKey_LicenseIsInvalid, Themes.Inflectra.Resources.LicenseKey_LicenseIsInvalidCaption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					txbOrganization.Tag = "1";
					txbLicenseKey.Tag = "1";
				}
				retValue = false;
			}

			//Return the value we got. If in DEBUG mode, display message informing tha tlicense key is not
			// bring checked.
#if DEBUG
			if (alert)
				MessageBox.Show(
					"Installer is in debug mode. License information is not being verified.",
					"Notice",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			return true;
#else
			return retValue;
#endif
		}
	}
}
