using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;
using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlPrerequisites.xaml</summary>
	public partial class cntrlPrerequisites : UserControl, IProcedureComponent
	{
		public cntrlPrerequisites()
		{
			InitializeComponent();
			Loaded += cntrlPrerequisites_Loaded;
		}

		private void cntrlPrerequisites_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				bool res = checkPrereqs();

				//If we're not okay, display the message at the bottom.
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		private bool checkPrereqs()
		{
			//Our status.
			PreReqCheckValueEnum finalStat = PreReqCheckValueEnum.Unset;

			//Loop through each TreeView Item. If it has a tag, we call it and get the status of it being installed.
			foreach (var item in tvPres.Items)
			{
				if (item is TreeViewItem)
				{
					PreReqCheckValueEnum subStat = checkTreeItem((TreeViewItem)item);

					//Set this current value.
					if (subStat > finalStat)
						finalStat = subStat;
				}
			}

			//Return whether we're okay or not.
			return finalStat == PreReqCheckValueEnum.Ok;
		}

		/// <summary>Checks the individual item for any needed prereqs. Recursively calls itself for children.</summary>
		/// <param name="item">The treeview item to check! </param>
		/// <returns>0 for successful. 1 for warning (first level okay, children not). >1 for error.</returns>
		private PreReqCheckValueEnum checkTreeItem(TreeViewItem item)
		{
			PreReqCheckValueEnum thisValue = PreReqCheckValueEnum.Unset;

			//Check children first.
			foreach (var subitem in item.Items)
				if (subitem is TreeViewItem)
				{
					PreReqCheckValueEnum subItem = checkTreeItem((TreeViewItem)subitem);

					//Set this current value.
					if (subItem > thisValue)
					{
						//If any are in error, this is a wrning.
						if (subItem == PreReqCheckValueEnum.Error)
							subItem = PreReqCheckValueEnum.Warning;

						thisValue = subItem;
					}
				}

			//Now that children are taken care of, let's do this item.
			if (!string.IsNullOrWhiteSpace(item.Tag?.ToString()))
			{
				if (item.Tag.ToString().StartsWith("IIS"))
				{
					PreReqCheckValueEnum subItem = PreReqCheck.IsIISFeatureInstalled(item.Tag.ToString()) ? PreReqCheckValueEnum.Ok : PreReqCheckValueEnum.Error;

					//Set this current value.
					if (subItem > thisValue)
						thisValue = subItem;
				}
				else
				{
					thisValue = PreReqCheckValueEnum.Unknwon;
				}

				Console.WriteLine(item.Tag?.ToString() + " -- " + thisValue.ToString());
			}

			DockPanel dock = item.Header as DockPanel;
			if (dock != null)
			{
				Image img = dock.Children[0] as Image;
				if (img != null)
				{
					string imgtype = "";
					if (thisValue == PreReqCheckValueEnum.Ok)
						imgtype = "Success.png";
					else if (thisValue == PreReqCheckValueEnum.Warning)
						imgtype = "Warning.png";
					else if (thisValue == PreReqCheckValueEnum.Error)
						imgtype = "Error.png";
					else
						imgtype = "Unknown.png";

					img.Source = new BitmapImage(
						new Uri(
							"/" + Assembly.GetEntryAssembly().GetName().Name + ";component/Themes/Inflectra/" + imgtype,
							UriKind.Relative)
					);
				}
			}

			//Set return value.
			return thisValue;
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.Prerequisites_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlPrerequisites";
		public bool AllowBack => true;
		public bool AllowNext => true;
		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//Only true if we're installing or upgrading an application.
				return (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.AddApplication ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade);
			}
		}

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid() => true;
		#endregion

		/// <summary>Possible outcomes for the pre req checks.</summary>
		private enum PreReqCheckValueEnum : int
		{
			Unset = -1,
			Unknwon = 3,
			Error = 2,
			Ok = 0,
			Warning = 1
		}
	}
}
