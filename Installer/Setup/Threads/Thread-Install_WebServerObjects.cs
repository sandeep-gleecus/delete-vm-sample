using Inflectra.SpiraTest.Installer.UI;
using Microsoft.Web.Administration;
using System;
using System.IO;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Creates the IIS Application Pool and Application Entries</summary>
		private bool CreateWebServerObjects(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 7;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Creating the IIS Application Pool and Application Entries");

			try
			{
				//Create the application pool (make sure it doesn't already exist)
				CreateAppPool(App._installationOptions.IISApplicationPool, streamWriter);


				//Instantiate the IIS Administration Manager
				CreateVirtApp(
					App._installationOptions.IISWebsite,
					App._installationOptions.IISApplication,
					App._installationOptions.InstallationFolder,
					App._installationOptions.IISApplicationPool,
					streamWriter
				);

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to create the IIS application pool and application entries:" + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to create the IIS application pool and application entries:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}

		/// <summary>Creates the specified app pool if it does not already exist.</summary>
		/// <param name="AppPool">The name of the new App Pool.</param>
		private void CreateAppPool(string AppPool, StreamWriter streamWriter)
		{
			using (ServerManager iisManager = new ServerManager())
			{
				var appPool = iisManager.ApplicationPools.FirstOrDefault(p => p.Name.Equals(AppPool));
				if (appPool == null)
				{
					streamWriter.WriteLine(string.Format("Creating the IIS Application Pool with name '{0}'", AppPool));
					//Create a ASP.NET 4.0 Integrated Pipeline AppPool using NETWORK SERVICE
					appPool = iisManager.ApplicationPools.Add(AppPool);
					appPool.AutoStart = true;
					appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
					appPool.ManagedRuntimeVersion = "v4.0";
					appPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
					iisManager.CommitChanges();
				}
			}
		}

		/// <summary>Creates the virtual application in the website.</summary>
		/// <remarks>App._installationOptions.IISWebsite</remarks>
		private void CreateVirtApp(string WebSite, string AppPath, string InstallFolder, string AppPool, StreamWriter streamWriter)
		{
			using (ServerManager iisManager = new ServerManager())
			{
				//Locate the specified website
				Site webSite = iisManager.Sites[WebSite];
				if (webSite == null)
					throw new ApplicationException(string.Format("Unable to find IIS website with name '{0}' so stopping.", App._installationOptions.IISWebsite));

				//Create the application or use the root website
				if (AppPath == "/")
					streamWriter.WriteLine("Installing " + App._installationOptions.ProductName + " on the website root");
				else
					streamWriter.WriteLine(string.Format("Installing {1} as an IIS Application with name '{0}'", AppPath, App._installationOptions.ProductName));

				Application application = webSite.Applications.FirstOrDefault(b => b.Path.Equals(AppPath));
				if (application == null)
				{
					streamWriter.WriteLine(string.Format("Creating the IIS Application with name '{0}'", AppPath));
					application = webSite.Applications.Add(AppPath, InstallFolder);
					//Set the new virtual directory path and app properties
					application.ApplicationPoolName = AppPool;
				}
				else
				{
					//Update the location and app-pool name
					application.ApplicationPoolName = AppPool;
					application.VirtualDirectories[0].PhysicalPath = InstallFolder;
				}

				//Save changes.
				iisManager.CommitChanges();

			}
		}
	}
}
