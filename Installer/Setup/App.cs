using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.HelperClasses.CommandLine;

namespace Inflectra.SpiraTest.Installer
{
	partial class App
	{
		public static InstallationOptions _installationOptions;
		public static readonly string CurrentDBRevision = "600";
		public const string CANCELSTRING = "Process Canceled";
		private static Application _App;
		internal static StreamWriter logFile;
		public static FinishSummaryArgs FinishArgs;
		internal static List<IUpgradeDBInit> UpgradeSteps;
		internal static string WorkFolder = "";
		public static Guid InstallationUID = Guid.NewGuid();

		/// <summary>Initial launch, creates the actual Application class.</summary>
		[STAThread]
		public static void Main(string[] args)
		{
			//Set our thread name for tracing.
			Thread.CurrentThread.Name = Themes.Inflectra.Resources.App_Name;

			//Load and upgrade the PROGRAM settings from previous versions
			Properties.Settings.Default.Upgrade();

			//Generate application.
			_App = new Application();
			_App.DispatcherUnhandledException += App_DispatcherUnhandledException;

			//Get the temporary folder.
			WorkFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Themes.Inflectra.Resources.Global_ManufacturerName,
				"ValidationMaster");

			//Create and open our logfile.
			Directory.CreateDirectory(WorkFolder);
			string LogFile = Path.Combine(WorkFolder, "InstallLog_" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".log");
			logFile = File.CreateText(LogFile);

			//Log start!
			logFile.WriteLine("Installation Ran: " + DateTime.Now.ToString("MM-dd-yyyy HH:mm"));

			//Instantiate instance of settings
			_installationOptions = new InstallationOptions();

			//Decode the command-line options.
			var res = Parser.Default.ParseArguments<UninstallOptions, UpgradeOptions, InstallOptions>(args);

			//Get the common options first..
			res.WithParsed<CommonOptions>((opts) =>
			{
				_installationOptions.IsAdvancedInstall = opts.Advanced;
				_installationOptions.CommandLine = opts;
			});

			//Load up the DB upgrade step classes.
			loadDBUpgradeLibraries(WorkFolder, logFile);

			//Run the form! (This is blocking.)
			_App.Run(new UI.wpfMasterForm());

			//Close and finish.
			logFile.WriteLine("Installation Finished: " + DateTime.Now.ToString("MM-dd-yyyy HH:mm"));
			logFile.Flush();
			logFile.Close();
			logFile.Dispose();
		}

		/// <summary>Hit whenever an uncaught exception occurs. Writes to the eventlog, displays a message, and terminates the program.</summary>
		/// <param name="sender">App</param>
		/// <param name="e">EventArgs</param>
		internal static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			//Exit.
			MessageBox.Show("An error occurred that was unrecoverable." + Environment.NewLine + "The full error details were saved to the error log. Contact support if the error happens again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
			MessageBox.Show(Logger.DecodeException(e.Exception), "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
#endif
			//Write it to the log, if we can.
			try
			{
				if (logFile != null)
				{
					logFile.WriteLine("System error in application:" + Environment.NewLine + Logger.DecodeException(e.Exception));
					logFile.Flush();
					logFile.Close();
					logFile.Dispose();
				}
			}
			catch { }

			Environment.Exit(-1);
		}

		/// <summary>Loads all the available DB Upgrade libraries into memory at startup.
		/// This will ensure we know what versions are available to be upgraded to.</summary>
		/// <returns></returns>
		private static void loadDBUpgradeLibraries(string workFolder, StreamWriter logger)
		{
			//Load all possibile classes that can convert the database.
			Type[] types = Assembly.Load(Assembly.GetExecutingAssembly().GetName())
				.GetExportedTypes()
				.Where(t => t.Name.StartsWith("UpgradeTasks"))
				.ToArray();

			//Initialize List..
			UpgradeSteps = new List<IUpgradeDBInit>();

			foreach (Type type in types)
			{
				//Needs to inherit the class, *AND* be based on the Interface.
				if (typeof(IUpgradeDBInit).IsAssignableFrom(type) && typeof(IUpgradeDB).IsAssignableFrom(type) && type.IsClass)
				{
					try
					{
						//Create an instance of the class.
						IUpgradeDBInit upgClass = (IUpgradeDBInit)Activator.CreateInstance(
							type,
							new object[] {
								logFile,
								workFolder }
						);

						//Add it to the collection.
						UpgradeSteps.Add(upgClass);

					}
					catch (Exception ex)
					{
						logFile.WriteLine(
							"Error while trying to activate Database Upgrade Class [" +
							type.ToString() + "] :" +
							Environment.NewLine +
							Logger.DecodeException(ex));
					}
				}
			}
		}
	}
}
