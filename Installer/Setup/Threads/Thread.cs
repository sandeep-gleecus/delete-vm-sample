using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;
using System.ServiceProcess;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/* Thread ID numbers, for progress bar.
		 * 1 - Install: Copy Application Files
		 * 2 - Install: Create Database
		 * 3 - Install: Web.Config File
		 * 5 - install: Application Global Settings
		 * 6 - Install: Event Log Source
		 * 7 - Install: Web Server Objects
		 * 8 - Install: DataSync Service
		 * 9 - Install: Start Menu items
		 * 10 - Install: Control Panel Item
		 * 11 - Upgrade: Upgrade Database
		 * 12 - Uninstall: Remove Application Files
		 * 13 - Uninstall: Remove Start Menu items
		 * 14 - Uninstall: Backup Database
		 * 15 - Uninstall: Delete Database
		 * 16 - Uninstall: Data Sync Service
		 * 17 - Uninstall: Web Server Objects
		 * 99 - Cleanup
		 */


		//Keeps track of statuses for when we're asked for the summary page.
		private FinalStatusEnum CanCopyApplicationFiles = FinalStatusEnum.OK;
		private FinalStatusEnum CanInstallDatabase = FinalStatusEnum.OK;
		private FinalStatusEnum CanUpdateWebConfig = FinalStatusEnum.OK;
		private FinalStatusEnum CanUpdateLicenseKey = FinalStatusEnum.OK;
		private FinalStatusEnum CanCreateEventLogSource = FinalStatusEnum.OK;
		private FinalStatusEnum CanCreateWebServerObjects = FinalStatusEnum.OK;
		private FinalStatusEnum CanInstallDataSyncService = FinalStatusEnum.OK;
		private FinalStatusEnum CanCreateProgramMenuEntries = FinalStatusEnum.OK;
		private FinalStatusEnum CanRegisterInControlPanel = FinalStatusEnum.OK;
		//HACK: Used temporarily until threads can be re-written.
		float numSteps = 0f;
		float curStep = 0f;

		public event EventHandler<ProgressArgs> ProgressUpdate;
		public event EventHandler<ProgressArgs> ProgressFinished;

		/// <summary>The temp folder used by the installer</summary>
		public string TempFolder { get; set; }

		/// <summary>Are they waiting to cancel?</summary>
		public static bool WantCancel;

		/// <summary>Creates a new instance of the class to perform the work.</summary>
		public ProcessThread()
		{
			//Nothing needs to be stored here, since everything is in App._installationOptions already
		}

		/// <summary>Called to start processing.</summary>
		public void StartProcess()
		{
			//Assign the writer to our Utility functions.
			Utilities.logWriter = App.logFile;
			SQLUtilities.logWriter = App.logFile;

			ProgressArgs finishArgs = new ProgressArgs
			{
				ErrorText = null,
				Progress = 1,
				IsIndetrerminate = false,
				TaskNum = 0,
				Status = ItemProgress.ProcessStatusEnum.Success
			};

			try
			{
				//See what options we have selected since that will determine the exact sequence
				switch (App._installationOptions.InstallationType)
				{
					#region Clean Install
					case InstallationTypeOption.CleanInstall:
						{
							//Copy Application Files
							if (Install_CopyApplicationFiles(App.logFile, out CanCopyApplicationFiles) && !WantCancel)
							{
								//Install Database
								if (Install_Database(App.logFile, out CanInstallDatabase) && !WantCancel)
								{
									//Update Web.config
									if (Install_UpdateWebConfig(App.logFile, out CanUpdateWebConfig) && !WantCancel)
									{
										//Update License Key
										if (Install_ApplicationGlobalSettings(App.logFile, out CanUpdateLicenseKey, true) && !WantCancel)
										{
											//Create the web server application pool & directory
											if (CreateWebServerObjects(App.logFile, out CanCreateWebServerObjects) && !WantCancel)
											{
												//Create Event Log Source - not a reason to abort the whole thing.
												Install_EventLogSource(App.logFile, out CanCreateEventLogSource);
												if (!WantCancel)
												{
													Install_DataSyncService(App.logFile, out CanInstallDataSyncService);
													if (!WantCancel)
													{
														//Install the DataSync Service - not a reason to abort the whole thing.
														Install_CreateProgramMenuEntries(App.logFile, out CanCreateProgramMenuEntries);
														if (!WantCancel)
														{
															//Register the uninstall application in the Control Panel.
															Install_RegisterInControlPanel(App.logFile, out CanRegisterInControlPanel);
														}
													}

												}
											}
											else
											{
												//We failed to create the web server objects.
												Uninstall_RemoveWebServerObjects(App.logFile);
												Uninstall_DeleteDatabase(App.logFile);
												Uninstall_RemoveFiles(App.logFile);

												//Set the end message.
												finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
													Themes.Inflectra.Resources.InstallResult_Message_ErrorInsWebObj);
												finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
											}
										}
										else
										{
											//We failed to upodate license info.
											Uninstall_DeleteDatabase(App.logFile);
											Uninstall_RemoveFiles(App.logFile);

											//Set the end message.
											finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
												Themes.Inflectra.Resources.InstallResult_Message_ErrorInsSettings);
											finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
										}
									}
									else
									{
										//We failed to update the web.config.
										Uninstall_DeleteDatabase(App.logFile);
										Uninstall_RemoveFiles(App.logFile);

										//Set the end message.
										finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
											Themes.Inflectra.Resources.InstallResult_Message_ErrorInsSettings);
										finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
									}
								}
								else
								{
									//We failed databse install. Try remving database and files.
									Uninstall_DeleteDatabase(App.logFile);
									Uninstall_RemoveFiles(App.logFile);

									//Set the end message.
									finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
										Themes.Inflectra.Resources.InstallResult_Message_ErrorInsDatabase);
									finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
								}
							}
							else
							{
								//We failed to copy the files. Remove what we can.
								Uninstall_RemoveFiles(App.logFile);

								//Set the end message.
								finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
									Themes.Inflectra.Resources.InstallResult_Message_ErrorInsFiles);
								finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
							}

							//Perform final cleanup.
							Final_CleanupInstallFiles(App.logFile);

							if (finishArgs.Status == ItemProgress.ProcessStatusEnum.Error)
							{
								finishArgs.UserCencel = WantCancel;

								//Start Needed Services
								ProgressUpdate(
									this,
									new ProgressArgs()
									{
										Progress = 1,
										Status = ItemProgress.ProcessStatusEnum.Error,
									});

							}
						}
						break;
					#endregion Clean Install

					#region Application Only Install
					case InstallationTypeOption.AddApplication:
						{
							//Copy Application Files
							if (Install_CopyApplicationFiles(App.logFile, out CanCopyApplicationFiles) && !WantCancel)
							{
								//Update Web.config
								if (Install_UpdateWebConfig(App.logFile, out CanUpdateWebConfig) && !WantCancel)
								{
									//Create the IIS Web Server Objects
									if (CreateWebServerObjects(App.logFile, out CanCreateWebServerObjects) && !WantCancel)
									{
										//Create Event Log Source - not a reason to abort the whole thing.
										Install_EventLogSource(App.logFile, out CanCreateEventLogSource);
										if (!WantCancel)
										{
											Install_DataSyncService(App.logFile, out CanInstallDataSyncService);
											if (!WantCancel)
											{
												//Install the DataSync Service - not a reason to abort the whole thing.
												Install_CreateProgramMenuEntries(App.logFile, out CanCreateProgramMenuEntries);
												if (!WantCancel)
												{
													//Register the uninstall application in the Control Panel.
													Install_RegisterInControlPanel(App.logFile, out CanRegisterInControlPanel);
												}
											}
										}
									}
									else
									{
										//We failed to create the web server objects.
										Uninstall_RemoveWebServerObjects(App.logFile);
										Uninstall_RemoveFiles(App.logFile);

										//Set the end message.
										finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
											Themes.Inflectra.Resources.InstallResult_Message_ErrorInsWebObj);
										finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
									}
								}
								else
								{
									//We failed to update the web.config.
									Uninstall_RemoveFiles(App.logFile);

									//Set the end message.
									finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
										Themes.Inflectra.Resources.InstallResult_Message_ErrorInsSettings);
									finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
								}
							}
							else
							{
								//We failed to copy the files. Remove what we can.
								Uninstall_RemoveFiles(App.logFile);

								//Set the end message.
								finishArgs.ErrorText = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorInsRollback,
									Themes.Inflectra.Resources.InstallResult_Message_ErrorInsFiles);
								finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
							}

							//Perform final cleanup.
							Final_CleanupInstallFiles(App.logFile);

							if (finishArgs.Status == ItemProgress.ProcessStatusEnum.Error)
							{
								finishArgs.UserCencel = WantCancel;

								//Start Needed Services
								ProgressUpdate(
									this,
									new ProgressArgs()
									{
										Progress = 1,
										Status = ItemProgress.ProcessStatusEnum.Error,
									});

							}
						}
						break;
					#endregion Application Only

					#region Full Upgrade
					case InstallationTypeOption.Upgrade:
						{
							//HACK: Hard code # of steps here.
							//TODO: When seperating these into threads, let each one report it's number properly.
							numSteps = 5;

							//In case we run into an error.
							string errMsg = null;

							//Stop Needed Services
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									Progress = ++curStep / numSteps,
									Status = ItemProgress.ProcessStatusEnum.Processing
								});

							//The name of the data-sync service can vary, so just try and stop any of them
							Utilities.StartStopService("DataSyncService", ServiceControllerStatus.Stopped);
							Utilities.StartStopService("SpiraTest Data Sync Service", ServiceControllerStatus.Stopped);
							Utilities.StartStopService("SpiraTeam Data Sync Service", ServiceControllerStatus.Stopped);
							Utilities.StartStopService("SpiraPlan Data Sync Service", ServiceControllerStatus.Stopped);
							Utilities.StartStopService("W3SVC", ServiceControllerStatus.Stopped);

							//Backup the DB first, if needed.
							bool succBackup = false;
							if (!App._installationOptions.NoBackupDB && !WantCancel)
							{
								numSteps++;

								string name = "v" + App._installationOptions.DBExistingRevision.ToString();
								succBackup = Uninstall_BackupDatabase(App.logFile, name, required: true);
							}

							//Upgrade Database
							if ((succBackup || App._installationOptions.NoBackupDB) && !WantCancel)
							{
								bool succ = Upgrade_UpgradeDatabase(App.logFile, out CanInstallDatabase);

								if (succ && !WantCancel)
								{
									//succ = Install_CopyApplicationFiles(App.logFile, out CanCopyApplicationFiles);									
									succ = Install_CopyApplicationFilesForUpgrade(App.logFile, out CanCopyApplicationFiles);
									if (succ && !WantCancel)
									{
										succ = Install_UpdateWebConfig(App.logFile, out CanUpdateWebConfig);

										if (succ && !WantCancel)
										{
											succ = Install_ApplicationGlobalSettings(App.logFile, out CanUpdateLicenseKey, false);

											if (succ && !WantCancel)
											{
												//Update in Control Panel
												//TODO: Implement
												//this.UpdateControlPanel(streamWriter, out this.CanRegisterInControlPanel);

												//Finished
												//TODO: Remove the zip file and other files from the C:\ProgramData\Inflectra folder
											}
											else
											{
												finishArgs.ErrorText = Themes.Inflectra.Resources.InstallResult_Message_ErrorAppSettings;
												finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
											}
										}
										else
										{
											finishArgs.ErrorText = Themes.Inflectra.Resources.InstallResult_Message_ErrorWebConfig;
											finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
										}
									}
									else
									{
										finishArgs.ErrorText = Themes.Inflectra.Resources.InstallResult_Message_ErrorUpgApplFiles;
										finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
									}
								}
								else
								{
									finishArgs.ErrorText = Themes.Inflectra.Resources.InstallResult_Message_ErrorUpgDatabase;
									finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
								}
							}
							else
							{
								finishArgs.ErrorText = string.Format(
									Themes.Inflectra.Resources.InstallResult_Message_ErrorBkpDatabase,
									null,
									App._installationOptions.InstallationType.ToString());
								finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
							}

							if (finishArgs.Status != ItemProgress.ProcessStatusEnum.Error)
							{
								//Start Needed Services
								ProgressUpdate(
									this,
									new ProgressArgs()
									{
										Progress = ++curStep / numSteps,
										Status = ItemProgress.ProcessStatusEnum.Success
									});

								//Restart the W3C services after everything is done.
								//Don't start the data sync service since they may need to do some configuration
								//and it's safer to not start it
								//(because if the app has any issues, the load from data sync service could completely kill the machine)
								//The old installer did the same thing, so no change from a user perspective.
								//Maybe in the future add a message on Finish to mention they need to do so.
								Utilities.StartStopService("W3SVC", ServiceControllerStatus.Running);
							}
							else
							{
								finishArgs.UserCencel = WantCancel;

								//Start Needed Services
								ProgressUpdate(
									this,
									new ProgressArgs()
									{
										Progress = 1,
										Status = ItemProgress.ProcessStatusEnum.Error,
										ErrorText = errMsg
									});
							}
						}
						break;

					#endregion Full Upgrade

					#region Database Upgrade
					case InstallationTypeOption.DatabaseUpgrade:
						{
							//HACK: Hard code # of steps here.
							//TODO: When seperating these into threads, let each one report it's number properly.
							numSteps = 2;

							//Stop Needed Services
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									Progress = ++curStep / numSteps,
									Status = ItemProgress.ProcessStatusEnum.Processing
								});
							Utilities.StartStopService("DataSyncService", ServiceControllerStatus.Stopped);
							//Utilities.StartStopService("W3SVC", ServiceControllerStatus.Stopped);

							//Backup the DB first, if needed.
							if (!App._installationOptions.NoBackupDB && !WantCancel)
							{
								numSteps++;

								string name = "v" + App._installationOptions.DBExistingRevision.ToString();
								Uninstall_BackupDatabase(App.logFile, name);
							}

							//Upgrade Database
							if (Upgrade_UpgradeDatabase(App.logFile, out CanInstallDatabase) && !WantCancel)
							{
								//Update License Key
								if (Install_ApplicationGlobalSettings(App.logFile, out CanUpdateLicenseKey, false) && !WantCancel)
								{
									//Finished
									Final_CleanupInstallFiles(App.logFile);
								}
							}
							else
							{
								//Set the end message.
								finishArgs.ErrorText = Themes.Inflectra.Resources.InstallResult_Message_ErrorUpgDatabase;
								finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
							}

							//Report progress.
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									Progress = ++curStep / numSteps,
									Status = ItemProgress.ProcessStatusEnum.Success
								});


							//Start Needed Services regardless of status of above.
							Utilities.StartStopService("W3SVC", ServiceControllerStatus.Running);
							Utilities.StartStopService("DataSyncService", ServiceControllerStatus.Running);

							if (finishArgs.Status == ItemProgress.ProcessStatusEnum.Error)
							{
								finishArgs.UserCencel = WantCancel;

								//Start Needed Services
								ProgressUpdate(
									this,
									new ProgressArgs()
									{
										Progress = 1,
										Status = ItemProgress.ProcessStatusEnum.Error,
									});

							}


						}
						break;
					#endregion  Database Upgrade

					#region Uninstall
					case InstallationTypeOption.Uninstall:
						{
							//HACK: Hard code # of steps here.
							//TODO: When seperating these into threads, let each one report it's number properly.
							numSteps = 6;

							//Stop Needed Services
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									Progress = ++curStep / numSteps,
									Status = ItemProgress.ProcessStatusEnum.Processing
								});
							//Lets get the Web.Config settings, first.
							App._installationOptions.existingSettings = ReadWebConfig.LoadSettingsFromWebConfig(
								Path.Combine(App._installationOptions.InstallationFolder, Constants.WEB_CONFIG)
							//We do not need to get the datasync settings here for uninstalltion.
							);
							InstallationTypeOption installType = InstallationTypeOption.Unknown;
							Enum.TryParse(App._installationOptions.existingSettings.InstallType, out installType);

							//We pretty much try to do everything we can. In reverse. We dont care if any one thing fails, the next will be run.

							// - Start Menu Shortcuts. Clean or App Only
							if (!WantCancel && (installType == InstallationTypeOption.CleanInstall || installType == InstallationTypeOption.AddApplication))
								Uninstall_ProgramMenuEntries(App.logFile);

							// - TODO: Data Sync Service
							if (!WantCancel)
								Uninstall_DataSyncService(App.logFile);
							Console.Write("Data Sync service completed");
							// - TODO: Event Log 

							// - Web Server Objects
							if (!WantCancel)
								Uninstall_RemoveWebServerObjects(App.logFile);

							// - Remove the App Files
							if (!WantCancel)
								Uninstall_RemoveFiles(App.logFile);
							Console.Write(" ----- ");
							// - Remove the Database
							//if (!WantCancel && installType == InstallationTypeOption.CleanInstall) commentted out for upgrading uninstall
							if (!WantCancel)
							{
								Console.Write(" After upgrage uninstall delete db starts:");
								bool backup = true;

								//We are doing the backup if:
								// - Uninstall and backup was selected.
								bool doBackup = (App._installationOptions.CommandLine is UninstallOptions &&
									((UninstallOptions)App._installationOptions.CommandLine).BackupDB);
								doBackup = doBackup || !App._installationOptions.NoBackupDB;

								if (doBackup)
									backup = Uninstall_BackupDatabase(App.logFile, "remove");

								//Now remove the database.
								Uninstall_DeleteDatabase(App.logFile);
							}

							//Stop Needed Services
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									Progress = ++curStep / numSteps,
									Status = ItemProgress.ProcessStatusEnum.Processing
								});
						}
						break;
					#endregion Uninstall

					default:
						break;
				}
			}
			catch (Exception ex)
			{
				App.logFile.WriteLine("Error in application:" + Environment.NewLine + Logger.DecodeException(ex));
				throw;
			}

			//Call the finish.
			ProgressFinished(this, finishArgs);
		}

		#region Helper Classes
		/// <summary>Class to hold progress information.</summary>
		internal class ProgressArgs : EventArgs
		{
			/// <summary>The number of the task that we're working on.</summary>
			public int TaskNum;
			/// <summary>The status to show.</summary>
			public ItemProgress.ProcessStatusEnum Status;
			/// <summary>The % of progress - 0 through 1. </summary>
			public double Progress;
			/// <summary>Any error text to display.</summary>
			public string ErrorText;
			/// <summary>Whether or not the progress is unknown.</summary>
			public bool IsIndetrerminate;
			/// <summary>Whether the user chose to cancel.</summary>
			public bool UserCencel;
		}
		#endregion

		#region Enumerations
		private enum FinalStatusEnum
		{
			OK = 0,
			Error = 1,
			Warning = 2
		}
		#endregion
	}
}
