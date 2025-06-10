using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		private const string STATIC_DATA = "static-data_{0}.zip";
		private const string DATABASE_OBJECTS = "database-objects_{0}.zip";
		private const int DB_REV = 600;

		/// <summary>Holds all the available DB Upgrader classes.</summary>
		private SortedList<int, IUpgradeDB> upgradeSteps = new SortedList<int, IUpgradeDB>();


		/// <summary>Updates an older version of the product database to the latest version</summary>
		/// <param name="streamWriter">Used to write out to the logs</param>
		/// <param name="status">The status of the process</param>
		/// <returns>True if successful, False if failure</returns>
		private bool Upgrade_UpgradeDatabase(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			status = FinalStatusEnum.OK;
			bool retValue = true;
			// - The process number.
			int TaskDisplayLine = 11;

			try
			{
				//Our Progress Argument object.
				ProgressArgs prog = new ProgressArgs
				{
					ErrorText = "",
					Progress = 0,
					Status = ItemProgress.ProcessStatusEnum.Processing,
					TaskNum = TaskDisplayLine
				};

				//Get various properties needed for processing.
				// - DB information.
				DBConnection conninfo = new DBConnection(
					App._installationOptions.DatabaseName,
					App._installationOptions.AuditDatabaseName,
					App._installationOptions.SQLServerAndInstance,
					App._installationOptions.DatabaseAuthentication,
					App._installationOptions.SQLInstallLogin,
					App._installationOptions.SQLInstallPassword,
					App._installationOptions.AuditSQLServerAndInstance,
					App._installationOptions.AuditDatabaseAuthentication,
					App._installationOptions.AuditSQLInstallLogin,
					App._installationOptions.AuditSQLInstallPassword
				);

				//Go through all DB upgrade classes, and only use the ones that should be used.
				int? dbRev = SQLUtilities.GetExistingDBRevision(conninfo);
				foreach (IUpgradeDB item in App.UpgradeSteps.ToList()
					.OrderBy(d => ((IUpgradeDB)d).DBRevisionUpgradeMin)
					.ThenByDescending(d => ((IUpgradeDB)d).DBRevision))
				{
					//Only add this class if the Minimum Upgrade Version isn't already contained.
					if (!upgradeSteps.Values.Any(d => d.DBRevisionUpgradeMin.Equals(item.DBRevisionUpgradeMin)))
					{
						//It doesn't already exist. If we have a dbRev, check it, otherwise add it.
						if (!dbRev.HasValue || (item.DBRevision >= dbRev.Value))
							upgradeSteps.Add(item.DBRevision, item);
					}
				}

				//Get the database versions..
				int existingDBVersion = dbRev.Value;
				int upgradedDBVersion = DB_REV;
				int minimumDBVersionNeeded = 540;
				if (upgradeSteps.Count > 0)
				{
					upgradedDBVersion = upgradeSteps.Max(f => f.Key);
					minimumDBVersionNeeded = upgradeSteps.Min(f => f.Key);
				}

				//Now loop through each available upgrader we have, and get total number of steps. (For progress!)
				foreach (KeyValuePair<int, IUpgradeDB> kvpUpgrader in upgradeSteps)
				{
					numSteps += kvpUpgrader.Value.NumberSteps;

					//Add one if we have to extract files. 
					if (!string.IsNullOrWhiteSpace(kvpUpgrader.Value.DatabaseFilePath))
						numSteps++;
				}

				//If we have no upgrade steps, then throw an error.
				if (numSteps == 0)
				{
					//If we're below the min version..
					if (existingDBVersion < minimumDBVersionNeeded)
						throw new WrongDBVersionMinimumException()
						{
							CurrentVer = existingDBVersion,
							NeededVer = minimumDBVersionNeeded,
							MaximumVer = upgradedDBVersion
						};
					else if (existingDBVersion >= upgradedDBVersion)
						throw new WrongDBVersionMaximumException()
						{
							CurrentVer = existingDBVersion,
							MaximumVer = upgradedDBVersion
						};
					else
						throw new WrongDBVersionException()
						{
							CurrentVer = existingDBVersion,
							MaximumVer = upgradedDBVersion
						};
				}

				//The current step we're performing.
				int curStep = 0;

				//Only loop this if there's any actual work to do.
				if (numSteps > 0)
				{
					//Add in the setup/teardown steps.
					numSteps += 3;

					// 1 - Force compatibility level.
					prog.Progress = (++curStep / numSteps);
					ProgressUpdate(this, prog);
					SetCompatibilityLevel(conninfo, streamWriter);

					// 2 - Drop Progammble Objects 
					prog.Progress = (++curStep / numSteps);
					ProgressUpdate(this, prog);
					SQLUtilities.DropProgrammableObjects(conninfo, streamWriter);

					//Okay, now run through each one, and do our update.
					foreach (KeyValuePair<int, IUpgradeDB> kvpUpgrader in upgradeSteps.Where(k => k.Key >= existingDBVersion))
					{
						try
						{
							//The upgrader.
							IUpgradeDB upgrader = kvpUpgrader.Value;

							//Make sure this upgrader is still valid after the last update.
							if (upgrader.VerifyDatabaseIsCorrectVersionToUpgrade(conninfo, streamWriter))
							{
								//Now run the upgrader's process.
								upgrader.UpgradeDB(conninfo, ProgressOverride, curStep, numSteps);

								//If we made it this far, we need to update the current DB revision! (Get it from DB, and
								//  not programatically, in case there is a bug in upgrader code, or it errors silently.
								dbRev = SQLUtilities.GetExistingDBRevision(conninfo);
							}
						}
						catch (Exception ex)
						{
							string msg = "Error upgrading database from " + existingDBVersion + " to " + kvpUpgrader.Value.DBRevision + ":" +
								Environment.NewLine +
								Logger.DecodeException(ex);
							streamWriter.WriteLine(msg);
							status = FinalStatusEnum.Error;
							retValue = false;
							ProgressUpdate(
								this,
								new ProgressArgs()
								{
									ErrorText = ex.Message,
									Progress = 1,
									Status = ItemProgress.ProcessStatusEnum.Error,
									TaskNum = TaskDisplayLine
								}
							);

							//Stop. No need to continue trying to upgrade.
							break;
						}
					}

					//Now handle restoring our programmable objects.
					prog.Progress = (++curStep / numSteps);
					ProgressUpdate(this, prog);
					SQLUtilities.CreateProgrammableObjects(conninfo, streamWriter);

					//See if we need to regenerate any data. 
					// - v6.5, Roll up Task, Requirement, Release status.
					if (minimumDBVersionNeeded < 650 && dbRev >= 650)
					{
						numSteps++; //Add one to our step count.

						// Report progress.
						prog.Progress = (++curStep / numSteps);
						ProgressUpdate(this, prog);

						App.logFile.WriteLine("Database Stats Refresh - Starting");
						string sqlExec = "exec MIGRATION_POPULATE_REQUIREMENT_COMPLETION;";
						SQLUtilities.ExecuteCommand(conninfo, sqlExec);
						App.logFile.WriteLine("Database Stats Refresh - Finished");
					}
				}

				//Display final successhere.
				ProgressUpdate(
					this,
					new ProgressArgs()
					{
						Progress = 1,
						Status = ItemProgress.ProcessStatusEnum.Success,
						TaskNum = TaskDisplayLine
					}
				);

			}
			catch (Exception ex)
			{
				//There was an error upgrading.
				string msg = "";
				if (ex is WrongDBVersionMaximumException)
				{
					msg = string.Format(Themes.Inflectra.Resources.Progress_DBUpgrade_WrongVersionMaximum,
								((WrongDBVersionException)ex).CurrentVer);
				}
				else if (ex is WrongDBVersionMinimumException)
				{
					msg = string.Format(
						Themes.Inflectra.Resources.Progress_DBUpgrade_WrongVersionMinimum,
						((WrongDBVersionException)ex).CurrentVer,
						((WrongDBVersionMinimumException)ex).NeededVer,
						((WrongDBVersionException)ex).MaximumVer);
				}
				else if (ex is WrongDBVersionException)
				{
					msg = string.Format(Themes.Inflectra.Resources.Progress_DBUpgrade_WrongVersionMaximum,
						((WrongDBVersionException)ex).CurrentVer);
				}
				else
				{
					msg = "Could not update database:" + Environment.NewLine + ex.Message;
				}

				ProgressUpdate(
					this,
					new ProgressArgs()
					{
						ErrorText = msg,
						Progress = 1,
						Status = ItemProgress.ProcessStatusEnum.Error,
						TaskNum = TaskDisplayLine
					}
				);

				retValue = false;
			}

			return retValue;
		}

		#region Global DB Functions
		/// <summary>Sets the compatibility level of the DB</summary>
		private void SetCompatibilityLevel(DBConnection connection, StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Database Upgrade: SetCompatibilityLevel - Starting");

			//Sets the compatibility to SQLServer 2008.
			string changeDbComptLevel =
				"EXEC dbo.sp_dbcmptlevel @dbname=N'" + connection.DatabaseName + @"', @new_cmptlevel=100";
			SQLUtilities.ExecuteSqlCommands(connection, changeDbComptLevel);

			streamWriter.WriteLine("Database Upgrade: SetCompatibilityLevel - Finished");
		}
		#endregion

		private void ProgressOverride(object progressArgs)
		{
			if (progressArgs is ProgressArgs)
			{
				ProgressUpdate(
					this,
					(ProgressArgs)progressArgs
				);
			}
		}
	}
}
