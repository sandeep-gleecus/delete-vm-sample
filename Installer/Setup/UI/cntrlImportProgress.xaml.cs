using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.Threads;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlImportProgress.xaml</summary>
	public partial class cntrlInstallProgress : UserControl, IProcedureProcessComponent
	{
		//Private storage.
		Thread _Thread;
		ProcessThread _ThreadClass;

		private List<ProcedureCompleteDelegate> procedureCompleteDelegates = new List<ProcedureCompleteDelegate>();

		//Static lists of the actions each option performs.
		/// <summary>Jobs done for a Clean, full install.</summary>
		private readonly List<int> jobsCleanInstall = new List<int> {
			1,2,3,4,5,6,7,8,9,10
		};
		/// <summary>Jobs done for an Application Only install.</summary>
		private readonly List<int> jobsAppOnly = new List<int> {
			1,3,6,7,8,9,10
		};
		/// <summary>Jobs done for a full Upgrade.</summary>
		private readonly List<int> jobsUpgrade = new List<int> {
			11,1,3,14
		};
		/// <summary>Jobs done for a database-only Upgrade.</summary>
		private readonly List<int> jobsDBUpgrade = new List<int> {
			11,14
		};
		/// <summary>Jobs done for a database-only Upgrade.</summary>
		private readonly List<int> jobsUninstall = new List<int>
		{
			12,13,14,15,16,17
		};

		internal cntrlInstallProgress()
		{
			InitializeComponent();
			Loaded += cntrlInstallProgress_Loaded;
		}

		/// <summary>Hit when the screen is loaded, and filters/hides the unused action items.</summary>
		private void cntrlInstallProgress_Loaded(object sender, RoutedEventArgs e)
		{
			//Adjust the items visible for display based on the Install Type.
			//Get the active list.
			List<int> ActiveItems = null;
			switch (App._installationOptions.InstallationType)
			{
				case HelperClasses.InstallationTypeOption.CleanInstall:
					{
						txtTitle.Text = Themes.Inflectra.Resources.InstallTasks_HeaderInstall;
						ActiveItems = jobsCleanInstall;
						pnlUpgrade.Visibility = Visibility.Collapsed;
						pnlUninstall.Visibility = Visibility.Collapsed;
						break;
					}
				case HelperClasses.InstallationTypeOption.AddApplication:
					{
						txtTitle.Text = Themes.Inflectra.Resources.InstallTasks_HeaderInstall;
						ActiveItems = jobsAppOnly;
						pnlUpgrade.Visibility = Visibility.Collapsed;
						pnlUninstall.Visibility = Visibility.Collapsed;
						break;
					}
				case HelperClasses.InstallationTypeOption.DatabaseUpgrade:
					{
						txtTitle.Text = Themes.Inflectra.Resources.InstallTasks_HeaderUpgrade;
						ActiveItems = jobsDBUpgrade;
						pnlInstall.Visibility = Visibility.Collapsed;
						pnlUninstall.Visibility = Visibility.Collapsed;

						//If no database backup, remove that item.
						if (App._installationOptions.NoBackupDB)
							ActiveItems.Remove(14);

						break;
					}
				case HelperClasses.InstallationTypeOption.Upgrade:
					{
						txtTitle.Text = Themes.Inflectra.Resources.InstallTasks_HeaderUpgrade;
						ActiveItems = jobsUpgrade;
						pnlInstall.Visibility = Visibility.Collapsed;
						pnlUninstall.Visibility = Visibility.Collapsed;

						//If no database backup, remove that item.
						if (App._installationOptions.NoBackupDB)
							ActiveItems.Remove(14);

						break;
					}
				case HelperClasses.InstallationTypeOption.Uninstall:
					{
						txtTitle.Text = Themes.Inflectra.Resources.InstallTasks_HeaderUninstall;
						ActiveItems = jobsUninstall;
						pnlInstall.Visibility = Visibility.Collapsed;
						pnlUpgrade.Visibility = Visibility.Collapsed;
						break;
					}
			}

			//Now hide the items that are NOT in this list. Loop through the three stackpanels..
			foreach (var pnl in itemsToDo.Children.OfType<StackPanel>())
			{
				if (pnl.Visibility != Visibility.Collapsed)
					foreach (ItemProgress item in pnl.Children.OfType<ItemProgress>())
					{
						item.Visibility = (ActiveItems.Any(f => f.ToString() == item.Tag.ToString()) ?
							Visibility.Visible :
							Visibility.Collapsed);
					}
			}
		}

		#region IProcedureProcessComponent Members
		List<ProcedureCompleteDelegate> IProcedureProcessComponent.ProcedureCompleteDelegates
		{
			get
			{
				return procedureCompleteDelegates;
			}
		}
		public void startProcedure()
		{
			// Using thread.start, this is where the call to the main importing function is done.
			ProcessThread.WantCancel = false;
			_ThreadClass = new ProcessThread();
			_ThreadClass.ProgressUpdate += _ThreadClass_ProgressUpdate;
			_ThreadClass.ProgressFinished += _ThreadClass_ProgressFinished;
			_Thread = new Thread(_ThreadClass.StartProcess);
			_Thread.Name = Themes.Inflectra.Resources.Thread_Name;

			//Need to set the TempFolder.
			_ThreadClass.TempFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Themes.Inflectra.Resources.Global_ManufacturerName,
				"ValidationMaster");

			_Thread.Start();
		}

		private delegate void _ThreadClass_ProgressFinishedCallback(object sender, ProcessThread.ProgressArgs e);
		void _ThreadClass_ProgressFinished(object sender, ProcessThread.ProgressArgs e)
		{
			if (Dispatcher.CheckAccess())
			{
				//Set progress bar color.
				switch (e.Status)
				{
					case ItemProgress.ProcessStatusEnum.Error:
						barProgress.Foreground = new SolidColorBrush(Colors.Red);
						break;
					case ItemProgress.ProcessStatusEnum.Processing:
						barProgress.Foreground = new SolidColorBrush(Colors.Yellow);
						break;
				}
				barProgress.IsIndeterminate = e.IsIndetrerminate;
				if (e.Progress >= 0)
					barProgress.Value = e.Progress;

				for (int i = 0; i < procedureCompleteDelegates.Count; i++)
					procedureCompleteDelegates[i].Invoke(e.Status, e.ErrorText);
			}
			else
			{
				_ThreadClass_ProgressFinishedCallback callB = new _ThreadClass_ProgressFinishedCallback(_ThreadClass_ProgressFinished);
				Dispatcher.Invoke(callB, new object[] { sender, e });
			}
		}

		/// <summary>Delegate to handle the intra-thread call to _ThreadClass_ProgressUpdate</summary>
		/// <param name="sender">ThreadClass</param>
		/// <param name="e">ProcessThread.ProgressArgs</param>
		private delegate void _ThreadClass_ProgressUpdate_Callback(object sender, ProcessThread.ProgressArgs e);
		/// <summary>Hit in the other thread when an update is requested.</summary>
		/// <param name="sender">ThreadClass</param>
		/// <param name="e">ProcessThread.ProgressArgs</param>
		private void _ThreadClass_ProgressUpdate(object sender, ProcessThread.ProgressArgs e)
		{
			if (Dispatcher.CheckAccess())
			{
				//Update form!
				// - Progress bar.
				if (e.Progress > -2)
				{
					barProgress.IsIndeterminate = (e.Progress == -1);
					barProgress.Value = ((e.Progress == -1) ? 0 : e.Progress);
					if (e.Status == ItemProgress.ProcessStatusEnum.Error)
						barProgress.Foreground = Brushes.Red;
				}

				// - Task item.
				var pnl = itemsToDo.Children
					.OfType<StackPanel>()
					.FirstOrDefault(p => p.Visibility != Visibility.Collapsed) as StackPanel;
				if (pnl != null)
				{
					ItemProgress itm = pnl
						.Children.OfType<ItemProgress>()
						.FirstOrDefault(g => g.Tag.Equals(e.TaskNum.ToString())) as ItemProgress;
					if (itm != null)
					{
						itm.SetActionStatus = e.Status;
						itm.SetErrorString = e.ErrorText;
					}
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Progress: " + e.Progress.ToString());
				_ThreadClass_ProgressUpdate_Callback callB = new _ThreadClass_ProgressUpdate_Callback(_ThreadClass_ProgressUpdate);
				Dispatcher.Invoke(callB, new object[] { sender, e });
			}
		}

		/// <summary>Called when the user wants to cancel the install!</summary>
		/// <returns>Status of cancellation</returns>
		public bool cancelProcedure()
		{
			try
			{
				ProcessThread.WantCancel = true;
				return true;
			}
			catch
			{
				//Logger.LogMessage(ex, "Trying to cancel background worker.");
				return false;
			}
		}
		#endregion

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.Progress_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlInstallProgress";
		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }
		public bool AllowBack => true;
		public bool AllowNext => true;
		public bool IsAvailable => true;
		/// <summary>Can we proceed to the next step in the wizard</summary>
		/// <returns>True if ready to proceed and all settings OK</returns>
		public bool IsValid() => true;
		#endregion

		/// <summary>Called when the progress bar changes value, to update the label.</summary>
		private void barProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			txbProgress.Visibility = ((barProgress.IsIndeterminate) ? Visibility.Collapsed : Visibility.Visible);
			txbProgress.Text = (barProgress.Value * 100).ToString("##0") + "%";
		}
	}
}
