using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>
		/// This system function creates a symbiolic link. 
		/// Used for linking a couple files into the DataSync directory.
		/// </summary>
		[DllImport("kernel32.dll")]
		static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

		/// <summary>Used for the CreateSymbolicLink function.</summary>
		enum SymbolicLink
		{
			File = 0,
			Directory = 1
		}

		/// <summary>Unzips the application program files and copies the zipfie to the target folder</summary>
		/// <param name="streamWriter">Used to write out to the logs</param>
		/// <param name="status">The status of the process</param>
		/// <returns>True if successful, False if failure</returns>
		private bool Install_CopyApplicationFiles(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 1;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Installing Filesystem.");

			try
			{
				#region Pre-File Copy Actions
				PreCopyEvents();
				#endregion Pre-File Copy Actions
				streamWriter.WriteLine("Downloadable Path Start.");
				downloadablePath();
				streamWriter.WriteLine("Downloadable Path End.");

				#region Main File Copy
				//Unzip our application filesystem into the temporary directory. 
				//  TODO: Can we skip this temporary directory/copy? Why not just copy straight into final location?

				// - Set the output directory as 'AppFiles'.
				string sourceFolder = Path.Combine(TempFolder, "AppFiles");
				ZipReader.UnzipFileToDirectory("base.Files.zip", TempFolder, sourceFolder);


				//Next we need to copy them to their destination location
				streamWriter.WriteLine("Copying the program files to the installation location.");
				if (!Directory.Exists(sourceFolder))
				{
					throw new ApplicationException("Unable to locate application installation folder at location: " + sourceFolder);
				}
				string programFilesFolder = App._installationOptions.InstallationFolder;

				//Call the recursive function to copy everything over.
				Utilities.CopyFolder(sourceFolder, programFilesFolder);
				#endregion Main File Copy

				#region Post-File Copy Actions
				#endregion Post-File Copy Actions

				//Update progress, and end.
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });

				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to copy over application files: " + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to copy over application files:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}

		private bool Install_CopyApplicationFilesForUpgrade(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 1;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Installing Filesystem Upgrade new.");

			try
			{
				#region Pre-File Copy Actions
				PreCopyEvents();
				#endregion Pre-File Copy Actions
				downloadablePath();
				//Unzip our application filesystem into the temporary directory. 
				//  TODO: Can we skip this temporary directory/copy? Why not just copy straight into final location?

				// - Set the output directory as 'AppFiles'.
				string sourceFolder = Path.Combine(TempFolder, "AppFiles");
				//ZipReader.UnzipFileToDirectory("base.Files.zip", TempFolder, sourceFolder);
				//string folderToSkip = @"Attachments\";  // Folder you want to skip
				string zipFilePath = "base.Files.zip";

				//string zipFilePath = @"C:\path\to\your\base.Files.zip";  // Path to ZIP file
				string sourceFolderPath = @"C:\Program Files (x86)\ValidationMaster";  // Source directory (where you want to unzip)
				string folderToBackup = @"Attachments";  // Folder you want to back up (relative path inside source folder)
				string tempBackupPath = @"C:\tempFolder";  // Backup location (temporary)

				//C:\Program Files (x86)\ValidationMaster\Reporting\templatefiles
				string sourceTempFilesFolderPath = @"C:\Program Files (x86)\ValidationMaster\Reporting"; 
				string templateFolderToBackup = @"templatefiles"; 
				string tempTemplateBackupPath = @"C:\tempTemplateFolder"; 

				//C:\Program Files (x86)\ValidationMaster\Reporting\podfiles
				string sourcePODFilesFolderPath = @"C:\Program Files (x86)\ValidationMaster\Reporting"; 
				string templatePODToBackup = @"podfiles"; 
				string tempPODBackupPath = @"C:\tempPODFolder";

				//C:\Program Files (x86)\ValidationMaster\Reporting\downloadablefiles
				string sourceDownloadableFilesFolderPath = @"C:\Program Files (x86)\ValidationMaster\Reporting";
				string templateDownloadableToBackup = @"downloadablefiles";
				string tempDownloadableBackupPath = @"C:\tempDownloadFolder";

				try
				{
					// Step 1: Copy the folder to backup location before unzipping
					string sourceFolderToBackup = Path.Combine(sourceFolderPath, folderToBackup);
					string backupFolderPath = Path.Combine(tempBackupPath, folderToBackup);

					string sourceTempFolderToBackup = Path.Combine(sourceTempFilesFolderPath, templateFolderToBackup);
					string backupTempFolderPath = Path.Combine(tempTemplateBackupPath, templateFolderToBackup);

					string sourcePodFolderToBackup = Path.Combine(sourcePODFilesFolderPath, templatePODToBackup);
					string backupPodFolderPath = Path.Combine(tempPODBackupPath, templatePODToBackup);

					string sourceDownloadedFolderToBackup = Path.Combine(sourceDownloadableFilesFolderPath, templateDownloadableToBackup);
					string backupDownloadedFolderPath = Path.Combine(tempDownloadableBackupPath, templateDownloadableToBackup);

					// Ensure the backup folder path exists
					if (Directory.Exists(sourceFolderToBackup))
					{
						streamWriter.WriteLine($"Backing up folder: {sourceFolderToBackup}");
						if (!Directory.Exists(backupFolderPath))
						{
							Console.Write(backupFolderPath);
							streamWriter.WriteLine($"Backing up folder --: {backupFolderPath}");
							Directory.CreateDirectory(backupFolderPath);
						}
						DirectoryCopy(sourceFolderToBackup, backupFolderPath, true);
						streamWriter.WriteLine($"After copy");
					}
					else
					{
						streamWriter.WriteLine($"Folder not found: {sourceFolderToBackup}");
					}

					// Ensure the template backup folder path exists
					if (Directory.Exists(sourceTempFolderToBackup))
					{
						streamWriter.WriteLine($"Template Backing up folder: {sourceTempFolderToBackup}");
						if (!Directory.Exists(backupTempFolderPath))
						{
							Console.Write(backupTempFolderPath);
							streamWriter.WriteLine($"Template Backing up folder --: {backupTempFolderPath}");
							Directory.CreateDirectory(backupTempFolderPath);
						}
						DirectoryCopy(sourceTempFolderToBackup, backupTempFolderPath, true);
						streamWriter.WriteLine($"After copy Template");
					}
					else
					{
						streamWriter.WriteLine($"Template Folder not found: {sourceTempFolderToBackup}");
					}

					// Ensure the POD backup folder path exists
					if (Directory.Exists(sourcePodFolderToBackup))
					{
						streamWriter.WriteLine($"POD Backing up folder: {sourcePodFolderToBackup}");
						if (!Directory.Exists(backupPodFolderPath))
						{
							Console.Write(backupPodFolderPath);
							streamWriter.WriteLine($"POD Backing up folder --: {backupPodFolderPath}");
							Directory.CreateDirectory(backupPodFolderPath);
						}
						DirectoryCopy(sourcePodFolderToBackup, backupPodFolderPath, true);
						streamWriter.WriteLine($"After copy POD");
					}
					else
					{
						streamWriter.WriteLine($"POD Folder not found: {sourcePodFolderToBackup}");
					}

					// Ensure the Downloadable backup folder path exists
					if (Directory.Exists(sourceDownloadedFolderToBackup))
					{
						streamWriter.WriteLine($"Downloadable Backing up folder: {sourceDownloadedFolderToBackup}");
						if (!Directory.Exists(backupDownloadedFolderPath))
						{
							Console.Write(backupDownloadedFolderPath);
							streamWriter.WriteLine($"Downloadable Backing up folder --: {backupDownloadedFolderPath}");
							Directory.CreateDirectory(backupDownloadedFolderPath);
						}
						DirectoryCopy(sourceDownloadedFolderToBackup, backupDownloadedFolderPath, true);
						streamWriter.WriteLine($"After copy Downloadable");
					}
					else
					{
						streamWriter.WriteLine($"Downloadable Folder not found: {sourceDownloadedFolderToBackup}");
					}

					// Step 2: Unzip the file to the source directory
					ZipReader.UnzipFileToDirectory("base.Files.zip", TempFolder, sourceFolder);
			
					streamWriter.WriteLine("Unzip and restore operation completed.");

					//Next we need to copy them to their destination location
					streamWriter.WriteLine("Copying the program files to the installation location.");
					if (!Directory.Exists(sourceFolder))
					{
						throw new ApplicationException("Unable to locate application installation folder at location: " + sourceFolder);
					}
					string programFilesFolder = App._installationOptions.InstallationFolder;

					//Call the recursive function to copy everything over.
					Utilities.CopyFolder(sourceFolder, programFilesFolder);

					#region Post-File Copy Actions
					// Step 3: Restore the backed - up folder after unzipping
					if (Directory.Exists(backupFolderPath))
					{
						streamWriter.WriteLine($"Restoring folder from backup: {backupFolderPath}");
						
						// Copy the backed-up folder back to its original location
						DirectoryCopy(backupFolderPath, sourceFolderToBackup, true);
						streamWriter.WriteLine("Folder restored successfully.");
					}
					if (Directory.Exists(backupTempFolderPath))
					{
						streamWriter.WriteLine($"Restoring template folder from backup: {backupTempFolderPath}");

						// Copy the backed-up folder back to its original location
						DirectoryCopy(backupTempFolderPath, sourceTempFolderToBackup, true);
						streamWriter.WriteLine("Template Folder restored successfully.");
					}
					if (Directory.Exists(backupPodFolderPath))
					{
						streamWriter.WriteLine($"Restoring POD folder from backup: {backupPodFolderPath}");

						// Copy the backed-up folder back to its original location
						DirectoryCopy(backupPodFolderPath, sourcePodFolderToBackup, true);
						streamWriter.WriteLine("POD Folder restored successfully.");
					}
					if (Directory.Exists(backupDownloadedFolderPath))
					{
						streamWriter.WriteLine($"Restoring Downloaded folder from backup: {backupDownloadedFolderPath}");

						// Copy the backed-up folder back to its original location
						DirectoryCopy(backupDownloadedFolderPath, sourceDownloadedFolderToBackup, true);
						streamWriter.WriteLine("Downloaded Folder restored successfully.");
					}

					try
					{
						// Attempt to delete the folder and its contents
						Directory.Delete(backupFolderPath, recursive: true);
						Directory.Delete(backupTempFolderPath, recursive: true);
						Directory.Delete(backupPodFolderPath, recursive: true);
						Directory.Delete(backupDownloadedFolderPath, recursive: true);
						streamWriter.WriteLine("Folder and its contents deleted successfully.");
					}
					catch (UnauthorizedAccessException)
					{
						streamWriter.WriteLine("You do not have permission to delete this folder.");
					}
					catch (DirectoryNotFoundException)
					{
						streamWriter.WriteLine("The specified folder does not exist.");
					}
					catch (IOException ex)
					{
						streamWriter.WriteLine("An error occurred while deleting the folder: " + ex.Message);
					}
					#endregion Post-File Copy Actions

					//Update progress, and end.
					ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });

					return true;
				}
				catch (Exception ex)
				{
					streamWriter.WriteLine("An error occurred: " + ex.Message);
					return false;
				}
			}	
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to copy over application files: " + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to copy over application files:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}

		/// <summary>
		/// Used to create a new "dependentAssembly" XmlNode with children for the given assembly name.
		/// Note that the minVersion will always be "0.0.0.0", and culture will always be "neutral".
		/// </summary>
		/// <param name="name">The assembly name</param>
		/// <param name="publicKey">The assembly public key.</param>
		/// <param name="xmlDoc">The XML Documemnt, used to create our XmlElement</param>
		/// <param name="newVersion">The high/new version to redirect.</param>
		/// <returns>An XML node to be inserted into the document.</returns>
		private static XmlElement createAssemblyBinding(string name, string publicKey, string newVersion, XmlDocument xmlDoc)
		{
			//Create the two children.
			// - <assemblyIdentity>
			XmlElement assemblyIdentity = xmlDoc.CreateElement("assemblyIdentity");
			assemblyIdentity.SetAttribute("name", name);
			assemblyIdentity.SetAttribute("publicKeyToken", publicKey);
			assemblyIdentity.SetAttribute("culture", "neutral");

			// - <bindingRedirect>
			XmlElement bindingRedirect = xmlDoc.CreateElement("bindingRedirect");
			bindingRedirect.SetAttribute("newVersion", newVersion);
			bindingRedirect.SetAttribute("oldVersion", "0.0.0.0-" + newVersion);

			//Create our container and add the two kids.
			// - <dependentAssembly>
			XmlElement dependentAssembly = xmlDoc.CreateElement("dependentAssembly");
			dependentAssembly.AppendChild(assemblyIdentity);
			dependentAssembly.AppendChild(bindingRedirect);

			//Return it.
			return dependentAssembly;
		}


		// Method to copy directory and its contents
		static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the source directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Copy all the files in the directory.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, true);
			}

			// Copy subdirectories and their contents.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string tempPath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
				}
			}
		}

		private void downloadablePath()
		{
			App.logFile.WriteLine("Downlodable---");
			string DownloadablePath = @"C:\Program Files (x86)\ValidationMaster\Reporting\downloadablefiles\";
			string outputPath = @"C:\Program Files (x86)\ValidationMaster\Reporting\output";
			string templatePath = @"C:\Program Files (x86)\ValidationMaster\Reporting\templatefiles";
			string podPath = @"C:\Program Files (x86)\ValidationMaster\Reporting\podfiles";

			if (!Directory.Exists(DownloadablePath))
				Directory.CreateDirectory(DownloadablePath);

			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			if (!Directory.Exists(templatePath))
				Directory.CreateDirectory(templatePath);

			if (!Directory.Exists(podPath))
				Directory.CreateDirectory(podPath);


			if (System.IO.Directory.Exists(DownloadablePath))
			{
				App.logFile.WriteLine("DownloadablePath---");
				//Change file permissions to give NETWORK SERVICE Full Control
				DirectoryInfo dInfo = new DirectoryInfo(DownloadablePath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				//dSecurity.SetAccessRuleProtection(false, true);
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), FileSystemRights.FullControl | FileSystemRights.CreateFiles | FileSystemRights.Modify | FileSystemRights.Read | FileSystemRights.ListDirectory | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);
			}
			if (System.IO.Directory.Exists(outputPath))
			{
				App.logFile.WriteLine("outputPath---");
				//Change file permissions to give NETWORK SERVICE Full Control
				DirectoryInfo dInfo = new DirectoryInfo(outputPath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				//dSecurity.SetAccessRuleProtection(false, true);
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), FileSystemRights.FullControl | FileSystemRights.CreateFiles | FileSystemRights.Modify | FileSystemRights.Read | FileSystemRights.ListDirectory | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);
			}
			if (System.IO.Directory.Exists(templatePath))
			{
				App.logFile.WriteLine("templatePath---");
				//Change file permissions to give NETWORK SERVICE Full Control
				DirectoryInfo dInfo = new DirectoryInfo(templatePath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				//dSecurity.SetAccessRuleProtection(false, true);
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), FileSystemRights.FullControl | FileSystemRights.CreateFiles | FileSystemRights.Modify | FileSystemRights.Read | FileSystemRights.ListDirectory | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);
			}
			if (System.IO.Directory.Exists(podPath))
			{
				App.logFile.WriteLine("podPath---");
				//Change file permissions to give NETWORK SERVICE Full Control
				DirectoryInfo dInfo = new DirectoryInfo(podPath);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				//dSecurity.SetAccessRuleProtection(false, true);
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), FileSystemRights.FullControl | FileSystemRights.CreateFiles | FileSystemRights.Modify | FileSystemRights.Read | FileSystemRights.ListDirectory | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);
			}

		}

		private void PreCopyEvents()
		{
			//First we handle any pre-copying we need to do. This can include any moving files and deleting unused files.
			if (App._installationOptions.existingSettings != null && App._installationOptions.existingSettings.VersionProgram < new Version("7.0.0.1"))
			{
				App.logFile.WriteLine("Installing Filesystem - v7.0.0.0");

				//In v6.9 installer, we are moving some files OUT of /bin and into the /datasync folder.
				//Get the list of potential files that will need to be moved OUT of /bin.
				string fileList = ZipReader.GetContents("FS_v690.zip", "datasync_files.txt");

				//Create the directory if needed.
				string datasyncPath = Path.Combine(App._installationOptions.InstallationFolder, "DataSync");
				if (!Directory.Exists(datasyncPath))
					Directory.CreateDirectory(datasyncPath);

				try
				{
					//Log it.
					App.logFile.WriteLine("Creating link to Common & DataModel.");

					//Create links to Common.dll & DataModel.dll over to the new directory.
					// - Common.dll
					string strMainDllNew = Path.Combine(datasyncPath, "Common.dll");
					string strMainDllOld = Path.Combine(App._installationOptions.InstallationFolder, "bin", "Common.dll");
					if (!File.Exists(strMainDllNew))
						CreateSymbolicLink(strMainDllNew, strMainDllOld, SymbolicLink.File);
					// - DataModel.dll
					strMainDllNew = Path.Combine(datasyncPath, "DataModel.dll");
					strMainDllOld = Path.Combine(App._installationOptions.InstallationFolder, "bin", "DataModel.dll");
					if (!File.Exists(strMainDllNew))
						CreateSymbolicLink(strMainDllNew, strMainDllOld, SymbolicLink.File);
				}
				catch (Exception ex)
				{
					App.logFile.WriteLine("Error creating links: " + Logger.DecodeException(ex));
				}

				//Loop through each file that should be moved.
				foreach (string file in fileList.Split('\n'))
				{
					//Strip out any spaces or remaining EOL.
					string fileName = file.Trim();

					//Generate full file paths.
					string fullFileBin = Path.Combine(App._installationOptions.InstallationFolder, "bin", fileName);
					string fullFileDS = Path.Combine(datasyncPath, fileName);

					if (File.Exists(fullFileBin))
					{
						//File exists, we will move it over.  If it already exists - which it shouldn't! - we simply delete
						//  the one from the bin directory.
						if (!File.Exists(fullFileDS))
						{
							App.logFile.WriteLine("Moving file '" + fileName + "' from \\bin to \\datasync.");
							File.Move(fullFileBin, fullFileDS);
						}
						else
						{
							App.logFile.WriteLine("Removing file '" + fileName + "' from \\bin. File already existed; not overwriting.");
							File.Delete(fullFileBin);
						}
					}
				}

				//Update DataSync Service location in Registry.
				using (RegistryKey dsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\DataSyncService", true))
				{
					if (dsKey != null)
					{
						App.logFile.WriteLine("Updating 'DataSyncService' executable location.");
						dsKey.SetValue("ImagePath", Path.Combine(datasyncPath, "DataSyncService.exe"), RegistryValueKind.String);
						dsKey.Close();
					}
				}
				using (RegistryKey dsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SpiraPlan Data Sync Service", true))
				{
					if (dsKey != null)
					{
						App.logFile.WriteLine("Updating 'SpiraPlan Data Sync Service' executable location.");
						dsKey.SetValue("ImagePath", Path.Combine(datasyncPath, "DataSyncService.exe"), RegistryValueKind.String);
						dsKey.Close();
					}
				}
				using (RegistryKey dsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SpiraTeam Data Sync Service", true))
				{
					if (dsKey != null)
					{
						App.logFile.WriteLine("Updating 'SpiraTeam Data Sync Service' executable location.");
						dsKey.SetValue("ImagePath", Path.Combine(datasyncPath, "DataSyncService.exe"), RegistryValueKind.String);
						dsKey.Close();
					}
				}
				using (RegistryKey dsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SpiraTest Data Sync Service", true))
				{
					if (dsKey != null)
					{
						App.logFile.WriteLine("Updating 'SpiraTest Data Sync Service' executable location.");
						dsKey.SetValue("ImagePath", Path.Combine(datasyncPath, "DataSyncService.exe"), RegistryValueKind.String);
						dsKey.Close();
					}
				}
			}
		}
	}
}
