using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

using Inflectra.SpiraTest.Installer.ControlForm;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>
	/// Interaction logic for cntrlEula.xaml
	/// </summary>
	public partial class cntrlEula : UserControl, IProcedureComponent
	{
		public cntrlEula()
		{
			InitializeComponent();

			//Load the EULA text from resources
			Encoding windows1252 = Encoding.GetEncoding("Windows-1252");
			Uri eulaResource = new Uri(
				"/" + Assembly.GetEntryAssembly().GetName().Name + ";component/Themes/Inflectra/EULA.txt",
				UriKind.Relative);
			StreamResourceInfo streamInfo = Application.GetResourceStream(eulaResource);
			using (StreamReader reader = new StreamReader(streamInfo.Stream, windows1252))
			{
				string eulaText = reader.ReadToEnd();
				reader.Close();
				txtEula.Text = eulaText;
			}

			//Default to unchecked
			chkAcceptEula.IsChecked = false;
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.Eula_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlEula";
		public bool AllowBack => true;
		public bool AllowNext => true;
		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//Only good on everything EXCEPT Uninstall.
				return (App._installationOptions.InstallationType != HelperClasses.InstallationTypeOption.Uninstall);
			}
		}
		#endregion

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid()
		{
			//The user has to agree to the EULA to proceed
			if (chkAcceptEula.IsChecked.Value)
			{
				return true;
			}
			else
			{
				MessageBox.Show(
					Themes.Inflectra.Resources.Eula_AcceptToContinue,
					Themes.Inflectra.Resources.Global_ValidationError,
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				return false;
			}
		}

		/// <summary>When they check the EULA, enable the Next button</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chkAcceptEula_Checked(object sender, RoutedEventArgs e)
		{
			App._installationOptions.EulaAccepted = true;
		}
	}
}
