using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Xml;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Contains various utilities that are used by the installer</summary>
	public static class Utilities
	{
		//For writing messags out.
		public static StreamWriter logWriter = null;


		/// <summary>Adds a shortcut in Windows to a URL</summary>
		/// <param name="appStartMenuItem">The relative path you want to create (e.g. Inflectra/SpiraPlan/Blah)</param>
		/// <param name="linkName">The name of the shortcut (e.g. Application Home)</param>
		/// <param name="iconPath">The path to an .ico file (optional)</param>
		/// <param name="linkUrl">The actual URL it should point to</param>
		public static void AddUrlShortcutToStartMenu(string appStartMenuItem, string linkName, string linkUrl, string iconPath = null)
		{
			string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
			string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", appStartMenuItem);

			//Let us make sure the directory is created, first.
			if (!Directory.Exists(appStartMenuPath))
				Directory.CreateDirectory(appStartMenuPath);

			using (StreamWriter writer = new StreamWriter(appStartMenuPath + "\\" + linkName + ".url"))
			{
				writer.WriteLine("[InternetShortcut]");
				writer.WriteLine("URL=" + linkUrl);
				if (!string.IsNullOrEmpty(iconPath))
				{
					writer.WriteLine("IconIndex=0");
					string icon = iconPath.Replace('\\', '/');
					writer.WriteLine("IconFile=" + icon);
				}
				writer.Flush();
			}
		}

		/// <summary>Adds a shortcut in Windows to a local EXE file</summary>
		/// <param name="appStartMenuItem">The relative path you want to create (e.g. Inflectra/SpiraPlan/Blah)</param>
		/// <param name="linkName">The name of the shortcut (e.g. Application Home)</param>
		/// <param name="pathToExe">The filepath to the exe</param>
		public static void AddAppShortcutToStartMenu(string appStartMenuItem, string linkName, string pathToExe)
		{
			string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
			string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", appStartMenuItem);

			using (StreamWriter writer = new StreamWriter(appStartMenuPath + "\\" + linkName + ".url"))
			{
				writer.WriteLine("[InternetShortcut]");
				writer.WriteLine("URL=file:///" + pathToExe);
				writer.WriteLine("IconIndex=0");
				string icon = pathToExe.Replace('\\', '/');
				writer.WriteLine("IconFile=" + icon);
				writer.Flush();
			}
		}

		/// <summary>Encodes a string so that all characters are escaped for inclusion into an XML file</summary>
		public static string XmlEncode(string text)
		{
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
		}

		/// <summary>
		/// Does a substring but handles length safely
		/// </summary>
		public static string SafeSubstring(this string str, int startIndex, int length)
		{
			if (length > str.Length)
			{
				return str;
			}
			else
			{
				return str.Substring(startIndex, length);
			}
		}

		/// <summary>Loads the specified XML Document into memory.</summary>
		/// <param name="filename">The file to load.</param>
		/// <returns>An XML document. Or null if the file could not be loaded.</returns>
		public static XmlDocument LoadXMLDoc(string filename)
		{
			XmlDocument retVal = null;

			//Double-check to see if the file exists.
			if (File.Exists(filename))
			{
				try
				{
					//Open as a XML doc
					retVal = new XmlDocument();
					retVal.PreserveWhitespace = true;
					retVal.Load(filename);
				}
				catch { }
			}

			return retVal;
		}

		/// <summary>Writes out the given XML file to the disk.</summary>
		/// <param name="file">The file to write out.</param>
		/// <param name="filename">The filename to write out to.</param>
		/// <returns>Status of the file write operation.</returns>
		public static bool SaveXMLDoc(XmlDocument file, string filename)
		{
			bool retValue = false;

			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				XmlTextWriter xtw = null;
				try
				{
					xtw = new XmlTextWriter(sw)
					{
						IndentChar = '\t',
						Indentation = 1,
						Formatting = Formatting.Indented
					};

					file.WriteTo(xtw);
				}
				catch (Exception ex)
				{
					App.logFile.WriteLine("Generating XML to file " + filename + ": " + Environment.NewLine + Logger.DecodeException(ex));
				}
				finally
				{
					if (xtw != null)
					{
						xtw.Close();

						try
						{
							//Write out the file.
							File.WriteAllText(filename, sb.ToString());
							retValue = true;
						}
						catch (Exception ex)
						{
							App.logFile.WriteLine("Saving XML to file " + filename + ": " + Environment.NewLine + Logger.DecodeException(ex));
						}
					}
				}
			}

			return retValue;
		}

		/// <summary>Sets the service name to the given status.</summary>
		public static bool StartStopService(string serviceName, ServiceControllerStatus status)
		{
			//The return value.
			bool retVal = false;

			//Check that we have a valid status. Right now, only supporting Running and Stopped.
			if (status == ServiceControllerStatus.Running || status == ServiceControllerStatus.Stopped)
			{
				//Create a controller and run the command.
				using (ServiceController sc = new ServiceController())
				{
					sc.ServiceName = serviceName;

					try
					{
						if (sc.Status != status)
						{
							if (status == ServiceControllerStatus.Stopped)
								sc.Stop();
							else if (status == ServiceControllerStatus.Running)
								sc.Start();

							sc.WaitForStatus(status, new TimeSpan(0, 0, 30));
							retVal = true;
						}
						else
						{
							//The service is already at the specified status.
							retVal = true;
						}
					}
					catch (Exception ex)
					{
						WriteLog("Setting service '" + serviceName + "' to " + status.ToString() + ":" + Environment.NewLine + Logger.DecodeException(ex));
						retVal = false;
					}
				}
			}
			else
				retVal = false;

			//Retrun the value.
			return retVal;
		}

		/// <summary>Returns the shortest possible string from a version. (Strips off all '0' points.)</summary>
		/// <param name="ver">The Version to display.</param>
		/// <returns></returns>
		public static string GetTruncatedVersion(Version ver)
		{
			//Always include Major, Minor. (6.2)
			string retStr = ver.Major + "." + ver.Minor;
			if (ver.Build > 0 || ver.Revision > 0)
			{
				//If we have a Build or a Revision, we must then add the Build. (v6.2.1, or v6.2.0.1)
				retStr += "." + ver.Build;

				//And if we have a revision, add it. (v6.2.0.1)
				if (ver.Revision > 0) retStr += "." + ver.Revision;
			}

			return retStr;
		}

		#region File System Functions
		/// <summary>Copies a folder and all its subfolders</summary>
		/// <param name="sourceFolder">The folder to copy</param>
		/// <param name="destFolder">The destination folder</param>
		public static void CopyFolder(string sourceFolder, string destFolder)
		{
			//Create the otput directry, if it's not there.
			if (!Directory.Exists(destFolder))
				Directory.CreateDirectory(destFolder);

			//Get a list of all the files we need to copy over, and move each one.
			string[] files = Directory.GetFiles(sourceFolder);
			foreach (string file in files)
			{
				string name = Path.GetFileName(file);
				string dest = Path.Combine(destFolder, name);
				File.Copy(file, dest, true);
			}

			//Get a list of all subdirectories in this folder, and move each one over.
			string[] folders = Directory.GetDirectories(sourceFolder);
			foreach (string folder in folders)
			{
				string name = Path.GetFileName(folder);
				string dest = Path.Combine(destFolder, name);
				//Recursive call.
				CopyFolder(folder, dest);
			}
		}

		/// <summary>Executes a shell command</summary>
		/// <param name="command">The command to execute</param>
		/// <param name="workingDir">The working directory</param>
		/// <param name="args">Any command-line arguments</param>
		/// <returns></returns>
		public static string ExecuteShellCommand(string command, string workingDir, string args)
		{
			string result = "";

			using (Process process = new Process())
			{
				process.StartInfo.FileName = command;
				process.StartInfo.WorkingDirectory = workingDir;
				process.StartInfo.Arguments = args;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;

				//Get the output..
				StringBuilder o = new StringBuilder();
				bool hasStarted = false;
				while (!hasStarted || !process.HasExited)
				{
					if (!hasStarted)
					{
						process.Start();
						hasStarted = true;
					}
					o.Append(process.StandardOutput.ReadToEnd());
					o.Append(process.StandardError.ReadToEnd());
				}

				//Get the output.
				result = o.ToString();
			}

			//Log the command
			return result;
		}

		/// <summary>Returns the configured File version from a specified DLL.</summary>
		/// <param name="fileName">The filename to query.</param>
		/// <returns>The FileVersion of the file. Null if it did not contain any.</returns>
		public static Version GetFileVersion(string fileName)
		{
			Version retVar = null;

			if (File.Exists(fileName))
			{
				try
				{
					FileVersionInfo fileVer = FileVersionInfo.GetVersionInfo(fileName);
					retVar = new Version(fileVer.FileVersion);
					fileVer = null;
				}
				catch (Exception ex)
				{
					App.logFile.WriteLine("Could not determine previous install version:" + Environment.NewLine + Logger.DecodeException(ex));
				}

			}

			return retVar;
		}
		#endregion File System Functions

		#region Private Methods
		/// <summary>Write out the message to the log</summary>
		/// <param name="message">THe string to write out. Will be appended by a NewLine.</param>
		private static void WriteLog(string message)
		{
			//See if we have the object set.
			if (logWriter != null)
				logWriter.WriteLine(message);
		}

		/// <summary>Writes the given message and exception out to our output.</summary>
		/// <param name="message">The message to write.</param>
		/// <param name="ex">Exception to write.</param>
		private static void WriteLog(string message, Exception ex)
		{
			string msg = message +
				(message.EndsWith(":") ? "" : ":") +
				Environment.NewLine +
				Logger.DecodeException(ex);
			WriteLog(msg);
		}
		#endregion
	}
}
