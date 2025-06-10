using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.Installer.ControlForm
{
	/// <summary>Main form handling for the Wizard dialog.</summary>
	public partial class ProcedureDialog : Window
	{
		#region Internal Variables
		private IProcedureComponent introductionComponent = null;
		private List<IProcedureComponent> configurationComponents = null;
		private IProcedureComponent confirmationComponent = null;
		private IProcedureProcessComponent progressComponent = null;
		private IProcedureComponent resultsComponent = null;

		private IProcedureComponent currentComponent = null;

		private bool procedureComplete = false;
		private bool processing = false;

		private bool closeConfirmed = false;
		public bool ignoreMin = false;

		private delegate void RefreshDelegate();
		#endregion

		/// <summary>Is thrown when navigation is in the process of chaning.
		/// Sender can be an int (1 - Next; -1 - Back; 0 - Random), or a string (name of new screen).
		/// Change the .Handled on the RoutedEventArgs to specify whether it is okay to navigate or not.
		/// If .Handled = true, screen will allow changing.</summary>
		public event RoutedEventHandler NavigationChanging;
		/// <summary>Is thrown when the screen is successfully navigated to a new screen.</summary>
		public event RoutedEventHandler NavigationChanged;
		/// <summary>Is thrown before the windows is closed, but when the user acknowledges that the process is finished.</summary>
		public event EventHandler AskedForClose;

		#region initialization
		public ProcedureDialog(
			string title,
			IProcedureComponent introductionComponent,
			List<IProcedureComponent> configurationComponents,
			IProcedureComponent confirmationComponent,
			IProcedureProcessComponent progressComponent,
			IProcedureComponent resultsComponent)
		{
			InitializeComponent();
			Title = title;

			#region store procedure components
			if (introductionComponent == null || confirmationComponent == null ||
				progressComponent == null || resultsComponent == null ||
				configurationComponents == null || configurationComponents.Count == 0)
			{
				throw new Exception("NULL IProcedureComponent or empty configuration components list passed to initialization of ProcedureDialog");
			}

			this.introductionComponent = introductionComponent;
			this.confirmationComponent = confirmationComponent;
			this.progressComponent = progressComponent;
			this.resultsComponent = resultsComponent;

			this.configurationComponents = new List<IProcedureComponent>();
			for (int i = 0; i < configurationComponents.Count; i++)
			{
				this.configurationComponents.Add(configurationComponents[i]);
			}
			#endregion

			#region populate key
			introductionLabel.Content = introductionComponent.KeyText;
			confirmationLabel.Content = confirmationComponent.KeyText;
			progressLabel.Content = progressComponent.KeyText;
			resultsLabel.Content = resultsComponent.KeyText;

			StackPanel sP = new StackPanel();
			sP.Orientation = Orientation.Vertical;
			configurationExpander.Content = sP;

			foreach (var screen in configurationComponents)
			{
				Label configurationLabel = new Label
				{
					Content = screen.KeyText,
					Margin = new Thickness(30, 0, 0, 0),
					Height = 25,
					Tag = screen, //TODO: Change this so that the entire control isn't stored in the label.
					Foreground = Brushes.Black
				};
				if (screen.IsLinkable) //Only allow linking if it's set.
					configurationLabel.MouseUp += configurationLabel_MouseUp;

				//Test something.
				if (screen is IProcedureComponent)
				{
					IProcedureComponent proc = (IProcedureComponent)screen;
					proc.DisplayLabel = configurationLabel;
				}

				//Add to the panel.
				sP.Children.Add(configurationLabel);
			}
			#endregion

			#region load introduction
			contentGrid.Children.Add((UserControl)introductionComponent);
			currentComponent = introductionComponent;
			#endregion

			#region hook finish process
			progressComponent.ProcedureCompleteDelegates.Add(new ProcedureCompleteDelegate(finishProcess));
			#endregion

			updateKeyLinks();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//If we're not in Vista/Win7, throw a dummy exception.
				if (Environment.OSVersion.Version.Major < 6)
					throw new Exception("Under windows Vista.");

				// Obtain the window handle for WPF application
				IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
				HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
				mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 255, 255, 255);

				// Get System Dpi
				System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
				desktop.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				float DesktopDpiX = desktop.DpiX;
				float DesktopDpiY = desktop.DpiY;

				// Set Margins
				NonClientRegionAPI.MARGINS margins = new NonClientRegionAPI.MARGINS();

				// Extend glass frame into client area
				// Note that the default desktop Dpi is 96dpi. The  margins are
				// adjusted for the system Dpi.
				margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
				margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
				margins.cyTopHeight = Convert.ToInt32(((int)75 + 5) * (DesktopDpiX / 96));
				margins.cyBottomHeight = Convert.ToInt32((5 - 10) * (DesktopDpiX / 96));

				int hr = NonClientRegionAPI.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
				if (hr < 0)
				{
					//DwmExtendFrameIntoClientArea Failed
				}

				// set background for menu navigation
				SolidColorBrush backgroundColor = new SolidColorBrush(Color.FromArgb(255, 235, 235, 235));  // light gray 100% transparent
				SolidColorBrush borderColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
				headerBorder.BorderBrush = borderColor;
				headerGrid.Background = backgroundColor;
				keyBorder.Background = backgroundColor;
				keyGrid.Background = backgroundColor;
				buttonPanel.Background = backgroundColor;
				buttonBorder.BorderBrush = borderColor;

				// set text color for menu navigation - all are the same
				SolidColorBrush foregroundColor = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
				headingLabel.Foreground = foregroundColor;
				summaryLabel.Foreground = foregroundColor;
				introductionExpander.Foreground = foregroundColor;
				introductionLabel.Foreground = foregroundColor;
				configurationExpander.Foreground = foregroundColor;
				confirmationExpander.Foreground = foregroundColor;
				confirmationLabel.Foreground = foregroundColor;
				progressExpander.Foreground = foregroundColor;
				resultsExpander.Foreground = foregroundColor;
				progressLabel.Foreground = foregroundColor;
				resultsLabel.Foreground = foregroundColor;

				// set colors for main content
				contentBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // transparent
				contentBorder.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // White, 0% transparent.
			}
			// If not Vista, paint background white.
			catch (Exception)
			{
				Application.Current.MainWindow.Background = SystemColors.ControlBrush;
				headerGrid.Background = SystemColors.ControlBrush;
				buttonBorder.Background = SystemColors.ControlBrush;
				buttonBorder.BorderBrush = SystemColors.ControlLightLightBrush;
				keyBorder.Background = SystemColors.ControlBrush;
				keyBorder.BorderBrush = SystemColors.ControlLightLightBrush;
				contentBorder.Background = SystemColors.ControlBrush;
				contentBorder.BorderBrush = SystemColors.ControlLightLightBrush;
			}
		}
		#endregion

		#region key handlers
		private void introductionLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent != introductionComponent && currentComponent != progressComponent &&
				currentComponent != resultsComponent)
			{
				NavigationChanging((int)-1, e);

				if (e.Handled)
				{

					currentComponent = introductionComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void configurationLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent != (IProcedureComponent)((Label)sender).Tag && currentComponent != progressComponent &&
				currentComponent != resultsComponent)
			{
				NavigationChanging((int)-1, e);

				if (e.Handled)
				{
					currentComponent = (IProcedureComponent)((Label)sender).Tag;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void progressLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent == resultsComponent)
			{
				NavigationChanging((int)-1, e);

				if (e.Handled)
				{
					currentComponent = progressComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void resultsLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (procedureComplete && currentComponent == progressComponent)
			{
				NavigationChanging((int)-1, e);

				if (e.Handled)
				{
					currentComponent = resultsComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}
		#endregion

		#region back / next / finish handlers
		/// <summary>The user clicked the 'Back' button.</summary>
		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			//Log move.
			App.logFile.Write("Moving to previous page: ");

			NavigationChanging?.Invoke((int)-1, e);
			//PreReqCheck.IsIISInstalled();

			if (e.Handled)
			{
				//Check / update the list.
				UpdateScreenAvailability();

				//Get the next screen.
				currentComponent = getBack(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				//Log.
				App.logFile.WriteLine(currentComponent.KeyText);

				//Update our buttons.
				updateButtons();

				//Throw event.
				NavigationChanged?.Invoke(this, e);
			}
			else
			{
				App.logFile.WriteLine("Page did not pass validity.");
			}
		}

		/// <summary>The user clicked the 'Next' button.</summary>
		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationChanging?.Invoke((int)1, e);

			//Log move.
			App.logFile.Write("Moving to next page: ");

			if (e.Handled)
			{
				//Check / update the list.
				UpdateScreenAvailability();

				//Get the next screen.
				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				//Log it.
				App.logFile.WriteLine(currentComponent.KeyText);

				//Update our buttons.
				updateButtons();

				//Throw event.
				NavigationChanged?.Invoke(this, e);
			}
			else
				App.logFile.WriteLine("Did not pass validity check.");
		}

		/// <summary>Ths user clicked the 'Finish'/'Cancel' button.</summary>
		private void finishButton_Click(object sender, RoutedEventArgs e)
		{
			//Check / update the list.
			UpdateScreenAvailability();

			if (currentComponent == confirmationComponent)
			{
				#region Confirm Action
				if (ShowProcessWarning)
				{
					if (MessageBox.Show(this, "Are you sure you wish to continue?\r\n\r\nPerforming this operation may not be reversable or cancellable, and may take considerable time.", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
					{
						return;
					}
				}
				#endregion

				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();

				processing = true;

				progressComponent.startProcedure();
			}
			else if (currentComponent == progressComponent)
			{
				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);
				updateButtons();
			}
			else if (currentComponent == resultsComponent)
			{
				procedureComplete = true;
				AskedForClose?.Invoke(this, new EventArgs());

				//Close the window.
				Close();
			}
		}

		/// <summary>Allows a screen to jump to another screen randomly.</summary>
		/// <param name="screen">The screen's ID to jump to.</param>
		internal void JumpToScreen(string option)
		{
			//Fire the event. This is to mimick an actual 'event' on the buttons. And give the page
			// being left to handle itself.
			RoutedEventArgs evt = new RoutedEventArgs { RoutedEvent = UIElement.MouseLeftButtonDownEvent };
			NavigationChanging?.Invoke((int)-2, evt);

			//Log move.
			App.logFile.Write("Jumping straight to screen '" + option + "': ");

			if (evt.Handled)
			{
				//Check / update the list.
				UpdateScreenAvailability();

				//Get the screen that is being ascked for
				var screen = configurationComponents.FirstOrDefault(t => t.UniqueName == option);
				if (screen != null)
				{
					//Get the next screen.
					currentComponent = screen;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)screen);

					//Log it.
					App.logFile.WriteLine(currentComponent.KeyText);

					//Update our buttons.
					updateButtons();

					//Throw event.
					NavigationChanged?.Invoke(this, evt);
				}
				else
					App.logFile.WriteLine("Could not find page.");
			}
		}

		/// <summary>Scans through all the steps, and asks each one if they are still applicable to the current situation.</summary>
		private void UpdateScreenAvailability()
		{
			foreach (var screen in configurationComponents)
			{
				screen.DisplayLabel.Visibility = (screen.IsAvailable) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		/// <summary>Updates the buttons, also makes sure that correct product logo is displayed</summary>
		private void updateButtons()
		{
			if (currentComponent == confirmationComponent)
			{
				backButton.IsEnabled = true;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = true;
				finishButton.Content = ((App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
					? Themes.Inflectra.Resources.Global_Uninstall
					: ((App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade || App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
							? Themes.Inflectra.Resources.Global_Upgrade
							: Themes.Inflectra.Resources.Global_Install)
					);
			}
			else if (currentComponent == introductionComponent)
			{
				backButton.IsEnabled = false;
				nextButton.IsEnabled = true;
				finishButton.IsEnabled = false;
				finishButton.Content = ((App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
					? Themes.Inflectra.Resources.Global_Uninstall
					: ((App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade || App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
							? Themes.Inflectra.Resources.Global_Upgrade
							: Themes.Inflectra.Resources.Global_Install)
					);
			}
			else if (currentComponent == progressComponent)
			{
				backButton.IsEnabled = false;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = procedureComplete;
				cancelButton.IsEnabled = !procedureComplete;
			}
			else if (currentComponent == resultsComponent)
			{
				backButton.IsEnabled = false;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = true;
				finishButton.Content = Themes.Inflectra.Resources.Global_Finish;

				cancelButton.IsEnabled = false;
			}
			else
			{
				backButton.IsEnabled = currentComponent.AllowBack;
				nextButton.IsEnabled = currentComponent.AllowNext;
				finishButton.IsEnabled = false;
				finishButton.Content = ((App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
					? Themes.Inflectra.Resources.Global_Uninstall
					: ((App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade || App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
							? Themes.Inflectra.Resources.Global_Upgrade
							: Themes.Inflectra.Resources.Global_Install)
					);
			}

			updateKeyLinks();

			updateProductLogo();
		}

		private void updateProductLogo()
		{
			if (string.IsNullOrEmpty(App._installationOptions.ProductName))
			{
				imageIcon.Source = new BitmapImage(new Uri(
					"/" + Assembly.GetEntryAssembly().GetName().Name + ";component/Themes/Inflectra/ValidationMaster.png",
					UriKind.Relative));
			}
			else
			{
				imageIcon.Source = new BitmapImage(new Uri(
					"/" + Assembly.GetEntryAssembly().GetName().Name + ";component/Themes/Inflectra/" + App._installationOptions.ProductName + ".png",
					UriKind.Relative));
			}
		}

		private void updateKeyLinks()
		{
			if (currentComponent == introductionComponent)
			{
				introductionLabel.FontWeight = FontWeights.Bold;
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).FontWeight = FontWeights.Normal;
				}
				confirmationLabel.FontWeight = FontWeights.Normal;
				progressLabel.FontWeight = FontWeights.Normal;
				resultsLabel.FontWeight = FontWeights.Normal;
			}
			else if (currentComponent == confirmationComponent)
			{
				introductionLabel.FontWeight = FontWeights.Normal;
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).FontWeight = FontWeights.Normal;
				}
				confirmationLabel.FontWeight = FontWeights.Bold;
				progressLabel.FontWeight = FontWeights.Normal;
				resultsLabel.FontWeight = FontWeights.Normal;
			}
			else if (currentComponent == progressComponent)
			{
				introductionLabel.FontWeight = FontWeights.Normal;
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).FontWeight = FontWeights.Normal;
				}
				confirmationLabel.FontWeight = FontWeights.Normal;
				progressLabel.FontWeight = FontWeights.Bold;
				resultsLabel.FontWeight = FontWeights.Normal;
			}
			else if (currentComponent == resultsComponent)
			{
				introductionLabel.FontWeight = FontWeights.Normal;
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).FontWeight = FontWeights.Normal;
				}
				confirmationLabel.FontWeight = FontWeights.Normal;
				progressLabel.FontWeight = FontWeights.Normal;
				resultsLabel.FontWeight = FontWeights.Bold;
			}
			else
			{
				introductionLabel.FontWeight = FontWeights.Normal;
				foreach (var label in ((StackPanel)(configurationExpander.Content)).Children.OfType<Label>())
				{
					FontWeight wt = ((currentComponent.UniqueName == ((IProcedureComponent)label.Tag).UniqueName) ?
						FontWeights.Bold :
						FontWeights.Normal);
					label.FontWeight = wt;
				}

				confirmationLabel.FontWeight = FontWeights.Normal;
				progressLabel.FontWeight = FontWeights.Normal;
				resultsLabel.FontWeight = FontWeights.Normal;
			}

			#region update key expanders
			introductionExpander.IsExpanded = false;
			configurationExpander.IsExpanded = false;
			confirmationExpander.IsExpanded = false;
			progressExpander.IsExpanded = false;
			resultsExpander.IsExpanded = false;

			if (currentComponent == introductionComponent)
				introductionExpander.IsExpanded = true;
			else if (configurationComponents.Contains(currentComponent))
				configurationExpander.IsExpanded = true;
			else if (currentComponent == confirmationComponent)
				confirmationExpander.IsExpanded = true;
			else if (currentComponent == progressComponent)
				progressExpander.IsExpanded = true;
			else
				resultsExpander.IsExpanded = true;
			#endregion

		}

		private IProcedureComponent getNext(IProcedureComponent current)
		{
			if (currentComponent == introductionComponent)
			{
				//Return the cirst Config component that is available.
				return configurationComponents.FirstOrDefault(f => f.IsAvailable);
			}
			else if (currentComponent == confirmationComponent)
			{
				//We are moving to actually run the process!
				return progressComponent;
			}
			else if (currentComponent == progressComponent)
			{
				//Process is complete! Send them to Results.
				return resultsComponent;
			}
			else if (currentComponent == resultsComponent)
			{
				//After this.. they can't move forward.
				return null;
			}
			else
			{
				//Configuration screen. We need to find the active one AFTER ours.
				int indx = configurationComponents.FindIndex(f => f.UniqueName == currentComponent.UniqueName);
				IProcedureComponent nextScreen = configurationComponents
					.Skip(indx + 1) //Skip up to our existing one, +1 to get to the next one.
					.FirstOrDefault(f => f.IsAvailable); //Take the first one that is available to us.

				//If it's null, that means there are no more config screens left. So, we go to the confirmation screen.
				if (nextScreen == null)
					nextScreen = confirmationComponent;

				return nextScreen;
			}
		}

		private IProcedureComponent getBack(IProcedureComponent current)
		{
			if (currentComponent == introductionComponent ||
				currentComponent == progressComponent ||
				currentComponent == resultsComponent)
			{
				//The introduction, the in-progress, and the final results screen we cannot go backwards from.
				return null;
			}
			else if (currentComponent == confirmationComponent)
			{
				//Find the LAST config screen that is enabled.
				return configurationComponents.Last(cs => cs.IsAvailable);
			}
			//else if (currentComponent == resultsComponent)
			//{
			//	return progressComponent;
			//}
			else
			{
				//The return object..
				IProcedureComponent retValue = null;

				//We need to find the one right BEFORE our current screen that IS AVAILABLE.
				int indx = configurationComponents.FindIndex(f => f.UniqueName == currentComponent.UniqueName);
				for (int i = indx - 1; i >= 0; i--)
				{
					//Take the first one that is active.
					if (configurationComponents[i].IsAvailable)
					{
						retValue = configurationComponents[i];
						i = -1;
					}
				}

				//If we have no pages, then we need to jump back to the introduction!
				if (retValue == null)
					retValue = introductionComponent;

				return retValue;
			}
		}
		#endregion

		#region cancel / close handlers
		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (processing)
			{
				if (MessageBox.Show(this, "Are you sure you want to cancel processing?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
				{
					Thread t = new Thread(new ThreadStart(waitForCancel));
					t.Name = "Wait for cancel thread";
					t.Priority = ThreadPriority.BelowNormal;
					t.Start();

					ignoreMin = true;
				}
			}
			else if (procedureComplete)
			{
				closeConfirmed = true;
				Close();
			}
			else
			{
				if (MessageBox.Show(this, "Are you sure you want to cancel this wizard?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
				{
					closeConfirmed = true;
					Close();
					ignoreMin = true;
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!closeConfirmed)
			{
				if (processing)
				{
					e.Cancel = true;
					if (MessageBox.Show(this, "Are you sure you want to cancel processing?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
					{
						e.Cancel = true;

						Thread t = new Thread(new ThreadStart(waitForCancel));
						t.Name = "Wait for cancel thread";
						t.Priority = ThreadPriority.BelowNormal;
						t.Start();
					}
				}
				else if (procedureComplete)
				{
					e.Cancel = false;
				}
				else
				{
					if (MessageBox.Show(this, "Are you sure you want to cancel this wizard?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
					{
						e.Cancel = true;
					}
					else
					{
						e.Cancel = false;
						ignoreMin = true;
					}
				}
			}
			else
			{
				e.Cancel = false;
			}
		}

		private void waitForCancel()
		{
			bool result = progressComponent.cancelProcedure();
			//Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(cancelDialog.Hide));

			if (result)
			{
				closeConfirmed = true;
				//Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(this.Close));
			}
			else
			{
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(waitForCancelFail));
			}
		}

		private void waitForCancelFail()
		{
			MessageBox.Show(this, "Error cancelling procedure...\r\n\r\nEither the operation cannot be cancelled or the process cannot be terminated at this time; check the user documentation for more information.", Title, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
		}
		#endregion

		#region processing handlers
		private void finishProcess(ItemProgress.ProcessStatusEnum status, string extraMessage)
		{
			finishButton.Content = "Finish";
			finishButton.IsEnabled = true;
			backButton.IsEnabled = false;
			cancelButton.IsEnabled = false;
			procedureComplete = true;
			processing = false;

			App.FinishArgs = new FinishSummaryArgs
			{
				Success = (status == ItemProgress.ProcessStatusEnum.Success),
				ExtraMessage = extraMessage
			};

			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(updateKeyLinks));
		}
		#endregion

		#region accessors
		public IProcedureComponent IntroductionComponent
		{
			get
			{
				return introductionComponent;
			}
		}

		public List<IProcedureComponent> ConfigurationComponents
		{
			get
			{
				List<IProcedureComponent> cc = new List<IProcedureComponent>();
				for (int i = 0; i < configurationComponents.Count; i++)
				{
					cc.Add(configurationComponents[i]);
				}
				return cc;
			}
		}

		public IProcedureComponent ConfirmationComponent
		{
			get
			{
				return confirmationComponent;
			}
		}

		public IProcedureProcessComponent ProgressComponent
		{
			get
			{
				return progressComponent;
			}
		}

		public IProcedureComponent ResultsComponent
		{
			get
			{
				return resultsComponent;
			}
		}

		/// <summary>
		/// Returns the current component
		/// </summary>
		public IProcedureComponent CurrentComponent
		{
			get
			{
				return currentComponent;
			}
		}

		public string HeaderTitle
		{
			get
			{
				return headingLabel.Content.ToString();
			}
			set
			{
				headingLabel.Content = value;
			}
		}

		public string HeaderDescription
		{
			get
			{
				return summaryLabel.Content.ToString();
			}
			set
			{
				summaryLabel.Content = value;
			}
		}

		public bool ShowProcessWarning
		{
			get;
			set;
		}
		#endregion

		/// <summary>Will return or set the # of the screen the user is on, -1 for introduction, -2 for progress, and -3 for Finished.</summary>
		public IProcedureComponent currentScreen
		{
			get
			{
				return currentComponent;
			}
			set
			{
				currentComponent = value;

				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();
			}
		}
	}
}
