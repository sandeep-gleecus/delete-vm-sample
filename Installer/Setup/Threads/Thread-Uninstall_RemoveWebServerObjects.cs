using Inflectra.SpiraTest.Installer.UI;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		private bool Uninstall_RemoveWebServerObjects(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Uninstall_RemoveWebServerObjects - Starting");


			int TaskDisplayLine = 17;
			ProgressUpdate(
				this,
				new ProgressArgs()
				{
					ErrorText = "",
					Progress = -1,
					Status = ItemProgress.ProcessStatusEnum.Processing,
					TaskNum = TaskDisplayLine
				}
			);

			//Have to remove the virtual directory, first.
			streamWriter.WriteLine("Removing the IIS Application Pool and Application Entries");

			try
			{
				//Have to remove the Virtual Directory, first, since deleting the AppPool won't work with anything
				// pointing to it.
				using (ServerManager iisManager = new ServerManager())
				{
					string WebSite = App._installationOptions.IISWebsite;

					if (string.IsNullOrWhiteSpace(WebSite) || string.IsNullOrWhiteSpace(App._installationOptions.IISApplication))
					{
						//We don't know it. We need to manually find it.
						//Loop through each website..
						List<Site> sitesToDelete = new List<Site>();
						foreach (var site in iisManager.Sites)
						{
							//Store a list of Applications that meet our search.
							List<Application> applsToDelete = new List<Application>();

							//Loop through each application.
							foreach (var appl in site.Applications)
							{
								//We hve to find the applications that point to our install directory.
								if (appl.VirtualDirectories.Any(f =>
									f.PhysicalPath.Equals(App._installationOptions.InstallationFolder, StringComparison.InvariantCultureIgnoreCase)))
								{
									applsToDelete.Add(appl);
								}
							}

							//Now let's delete the ones we found.
							bool removeSite = false;
							foreach (var appl in applsToDelete)
							{
								if (appl.Path == "/") removeSite = true;
								site.Applications.Remove(appl);
							}

							//Add the site to remove!
							if (removeSite) sitesToDelete.Add(site);
						}

						//Loop through the sites to delete now.
						foreach (var site in sitesToDelete)
						{
							if (iisManager.Sites.Count > 1)
								iisManager.Sites.Remove(site);
						}

						iisManager.CommitChanges();
					}
					else
					{
						//Locate the specified website
						Site webSite = iisManager.Sites[WebSite];
						if (webSite != null)
						{
							streamWriter.WriteLine("Removing " + App._installationOptions.ProductName + " from the website.");
							//If it's the root, we ain't deleting it.
							if (App._installationOptions.IISApplication != "/")
							{
								var appl = webSite.Applications[App._installationOptions.IISApplication];
								if (appl != null)
								{
									webSite.Applications.Remove(appl);

									//Save changes.
									iisManager.CommitChanges();
								}
							}
						}
					}
				}

				//Now remove the app pool, if we can.
				using (ServerManager iisManager = new ServerManager())
				{
					string AppPool = App._installationOptions.IISApplicationPool;
					var appPool = iisManager.ApplicationPools.FirstOrDefault(p => p.Name.Equals(AppPool));
					if (appPool != null)
					{
						iisManager.ApplicationPools.Remove(appPool);

						//Save changes.
						iisManager.CommitChanges();
					}
				}

				streamWriter.WriteLine("Uninstall_RemoveWebServerObjects - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to remote the IIS application pool or application entries:" + Environment.NewLine + Logger.DecodeException(ex));

				streamWriter.WriteLine("Uninstall_RemoveWebServerObjects - Finished");
				string ErrorMsg = "Unable to remote the IIS application pool or application entries:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
