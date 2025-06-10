using System;
using System.Windows;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for the base framework form.</summary>
	public partial class wpfMasterForm : Window
	{
		#region Constructors
		public wpfMasterForm()
		{
			InitializeComponent();
			Visibility = Visibility.Collapsed;
			Closed += Window_Closed;
			IsVisibleChanged += Window_IsVisibleChanged;

			//Create the master form.
			SetWizardWindowProperties();

			//Show the wizard.
			wizard.Show();

		}
		#endregion

		#region Master Window Events
		/// <summary>His when the window is finally unloaded</summary>
		/// <param name="sender">wpfMasterForm</param>
		/// <param name="e">EventArgs</param>
		private void Window_Closed(object sender, EventArgs e)
		{
			//Do nothing
		}

		/// <summary>Hit whenever the master form becomes visible again, for some reason.</summary>
		/// <param name="sender">wpfMasterForm</param>
		/// <param name="e">EventArgs</param>
		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Visibility = Visibility.Collapsed;
		}

		#endregion
	}
}
