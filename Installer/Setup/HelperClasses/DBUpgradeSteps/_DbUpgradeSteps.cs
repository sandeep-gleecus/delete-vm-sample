using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;
using static Inflectra.SpiraTest.Installer.Threads.ProcessThread;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	public interface IUpgradeDB
	{
		/// <summary>The ending DB Revision for these steps.</summary>
		int DBRevision { get; }

		/// <summary>The number of steps this stage has.</summary>
		int NumberSteps { get; }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		int DBRevisionUpgradeMin { get; }

		/// <summary>The allowable upper-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		int DBRevisionUpgradeMax { get; }

		/// <summary>The full path where this upgrade code should look for DB files. If null, then
		/// this upgrade has no external DB files to worry about.</summary>
		string DatabaseFilePath { get; }

		/// <summary>Actually do the work needed!</summary>
		/// <param name="ConnectionInfo">THe connection information to connect to the DB with.</param>
		/// <param name="CurrentJobNumber">The current job number, for progress reports.</param>
		/// <param name="TotalJobs">The total number of jobs, for progress reports.</param>
		bool UpgradeDB(
			DBConnection ConnectionInfo,
			Action<object> ProgressFunction,
			int CurrentJobNumber,
			float TotalJobs
		);

		/// <summary>Returns whether this class can upgrade the given database.</summary>
		/// <param name="ConnInfo">The database connection info.</param>
		/// <param name="StreamWriter">The logger.</param>
		/// <param name="ExistingDBRevision">Returns the existing DB revision.</param>
		/// <returns>True if this upgrader can upgrade the database. False if it cannot.</returns>
		bool VerifyDatabaseIsCorrectVersionToUpgrade(DBConnection ConnInfo, StreamWriter StreamWriter);
	}

	public abstract class IUpgradeDBInit
	{
		protected StreamWriter _logger;
		protected int _taskNum = 11;
		protected DBConnection _connection;
		internal ProgressArgs progress = null;
		internal float _totJob;
		internal string workFolder;
		protected Action<object> ProgressHandler;

		/// <summary>Instantiates the class.</summary>
		/// <param name="LogWriter">Our StreamWriter to output text.</param>
		/// <param name="ProgressFunction">The ProgressUpdate function.</param>
		/// <param name="ConnectionInfo">THe database connection information.</param>
		/// <param name="Process">THe main process thread.</param>
		public IUpgradeDBInit(StreamWriter LogWriter, string WorkFolder)
		{
			//save the properties we're handed.
			_logger = LogWriter;
			WorkFolder = workFolder;

			//Create our progress object.
			progress = new ProgressArgs()
			{
				ErrorText = "",
				Progress = 0,
				Status = ItemProgress.ProcessStatusEnum.Processing,
				TaskNum = _taskNum,
				IsIndetrerminate = false
			};
		}

		#region Common Functions
		/// <summary>Updates the DB revision number to the current version</summary>
		internal void UpdateDatabaseRevisionNumber(int db_rev)
		{
			//Update progress bar.
			UpdateProgress();

			string sqlCmd = "UPDATE [TST_GLOBAL_SETTING] SET VALUE = '" +
						SQLUtilities.SqlEncode(db_rev.ToString()) +
						"' WHERE NAME = 'Database_Revision'";
			SQLUtilities.ExecuteCommand(_connection, sqlCmd);
		}

		/// <summary>Adds one to our cound for the progress, and raises event.</summary>
		internal void UpdateProgress()
		{
			if (progress != null && ProgressHandler != null)
			{
				//Add one to the count. (Or one/total)
				progress.Progress += (1 / _totJob);
				ProgressHandler(progress);
			}
		}

		#endregion Common Functions
	}
}
