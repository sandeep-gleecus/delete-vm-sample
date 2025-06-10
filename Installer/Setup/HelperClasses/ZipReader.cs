using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	static public class ZipReader
	{
		private static Assembly currExe = null;
		private static string currExeAssemblyName = null;
		private const string packageDirectoryPart = "Package";

		static ZipReader()
		{
			//Get the current Assebly into memory. 
			currExe = Assembly.GetExecutingAssembly();
			currExeAssemblyName = "Inflectra.SpiraTest.Installer";
		}

		/// <summary> 
		/// Returns the contents of a file inside a zipped file that is Embedded into the installer. 
		/// </summary> 
		/// <param name="assemblyFIle">THe assembly file to pull from.</param> 
		/// <param name="fileName">The file within the Zipped file to pull. If null (default), will pull first file in archive.</param> 
		/// <returns>A string </returns> 
		public static string GetContents(string assemblyFile, string fileName = null)
		{
			//The return value. Default it to nothing. 
			string retValue = "";
			ZipArchiveEntry zipFile = null;

			//If the asked-for Assembly file does NOT include the assembly name, add it. 
			if (!assemblyFile.StartsWith(currExeAssemblyName))
				assemblyFile = currExeAssemblyName + "." + packageDirectoryPart + "." + assemblyFile;

			try
			{
				if (currExe != null)
				{
					//Create the stream of the embedded resource. 
					using (Stream stream = currExe.GetManifestResourceStream(assemblyFile))
					{
						//Load the embedded resource up as a Zip file. 
						using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
						{
							//Pull the file they're looking for, or the first one in the ZIP. 
							
							if (!string.IsNullOrWhiteSpace(fileName))
								zipFile = zip.Entries.
									SingleOrDefault(n => n.FullName.Equals(fileName));
							else
								zipFile = zip.Entries.FirstOrDefault();

							if (zipFile != null)
							{
								//Opwn the file. 
								using (var zipStr = zipFile.Open())
								{
									retValue = new StreamReader(zipStr).ReadToEnd();
								}
							}
							else
								throw new Exception("Internal error. Z2");
						}
					}
				}
				else
				{
					throw new Exception("Internal error. Z1");
				}
			}
			catch (Exception ex)
			{
				//An error occured.  
				string msg = "While looking for '{0}' in '{1}':" + Environment.NewLine + Logger.DecodeException(ex);
				App.logFile.WriteLine(
					string.Format(msg,
						fileName,
						assemblyFile.Replace(currExeAssemblyName + "." + packageDirectoryPart + ".", ""))
					);
				App.logFile.WriteLine(
					string.Format("Assembly File '{0}'  Zipfile '{1}'",
						assemblyFile,
						zipFile)
					);
				throw;
			}

			return retValue;
		}

		/// <summary>Returns a list of all the files in the Embedded ZIpfile.</summary> 
		/// <param name="assemblyFIle">The embedded resource to open and peer at.</param> 
		/// <returns>A list of all the available files.</returns> 
		public static List<string> GetFilesIn(string assemblyFile)
		{
			//The return list. 
			List<string> retList = new List<string>();

			//If the asked-for Assembly file does NOT include the assembly name, add it. 
			if (!assemblyFile.StartsWith(currExeAssemblyName))
				assemblyFile = currExeAssemblyName + "." + packageDirectoryPart + "." + assemblyFile;

			try
			{
				//Open the data stream. 
				using (Stream stream = currExe.GetManifestResourceStream(assemblyFile))
				{
					//Pass it to the Zip reader. 
					using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
					{
						//Generate the list. 
						retList = zip.Entries.
							Select(n => n.Name).
							ToList();
					}
				}
			}
			catch (Exception ex)
			{
				//An error occured.  
				string msg = "While listing files in '{0}':" + Environment.NewLine + Logger.DecodeException(ex);
				App.logFile.WriteLine(
					string.Format(msg,
						assemblyFile.Replace(currExeAssemblyName + "." + packageDirectoryPart + ".", ""))
					);
			}

			return retList;
		}

		/// <summary>Unzips a file using the embedded resource</summary> 
		/// <param name="destinationPath">The folder to put the unzipped files in</param> 
		/// <param name="assemblyFile">The file in the Assembly to unzip.</param> 
		/// <param name="tempFolder">The temporary install directory.</param>
		public static void UnzipFileToDirectory(string assemblyFile, string tempFolder, string destinationPath)
		{
			try
			{
				//If the asked-for Assembly file does NOT include the assembly name, add it. 
				if (!assemblyFile.StartsWith(currExeAssemblyName))
					assemblyFile = currExeAssemblyName + "." + packageDirectoryPart + "." + assemblyFile;

				//This is out ourput ZIP filename in the Temp directory.
				string sourceFile = Path.Combine(tempFolder, Properties.Settings.Default.ApplicationPackage);
				//We need to save this file to the filesystem. This library (similat to SharpZip) can not unzip
				// every file into a directory at once. This is fine, too, since we don't really want to load a
				// 50+Mb file into memory!
				using (Stream stream = currExe.GetManifestResourceStream(assemblyFile))
				{

					//Write our code form the assembly into the ZIP file.
					using (FileStream fileStream = new FileStream(sourceFile, FileMode.Create, FileAccess.ReadWrite))
					{
						stream.CopyTo(fileStream);
						fileStream.Close();
					}
				}

				//Delete our extraction directory. So that we don't have any unwanted files hanging around.
				if (Directory.Exists(destinationPath))
					Directory.Delete(destinationPath, true);

				//Now, go ahead and unzip our sourceZip to the directory.
				ZipFile.ExtractToDirectory(sourceFile, destinationPath);
			}
			catch (Exception ex)
			{
				throw new Exception("Error unzipping file '" + assemblyFile + "' to: " + destinationPath, ex);
			}
		}
	}
}
