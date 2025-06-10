using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Updates the web.config file with the license and connection string info</summary>
		private bool Install_UpdateWebConfig(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 3;
			status = FinalStatusEnum.OK;
			ProgressUpdate(
				this,
				new ProgressArgs()
				{
					Progress = -1,
					Status = ItemProgress.ProcessStatusEnum.Processing,
					TaskNum = TaskDisplayLine
				}
			);
			streamWriter.WriteLine("Updating the Web.config file with the connection and other information.");

			try
			{
				App.logFile.WriteLine("DEBUG 1: Loading values from App._installationOptions");
				//Get the various properties
				string configFilePath = App._installationOptions.InstallationFolder;
				string dbServer = App._installationOptions.SQLServerAndInstance;
				string dbName = App._installationOptions.DatabaseName;
				AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbNewLogin = App._installationOptions.DatabaseUser;
				string themeName = Themes.Inflectra.Resources.Global_ThemeName;
				string eventLogSource = App._installationOptions.ProductName;
				string licenseEditable = (Properties.Settings.Default.LicenseEditable).ToString();

				//Open as a XML doc
				string fileName = configFilePath + "\\web.config";
				if (!File.Exists(fileName))
				{
					App.logFile.WriteLine("The web.config file does not exist at: '" + fileName + "'");
					throw new Exception("The web.config file does not exist at: '" + fileName + "'");

				}
				App.logFile.WriteLine("DEBUG 2: File exists. Reading.");
				XmlDocument xmlDoc = Utilities.LoadXMLDoc(fileName);

				if (xmlDoc == null)
				{
					App.logFile.WriteLine("Unable to open web.config, returned NULL");
					throw new Exception("Unable to open web.config, returned NULL");
				}
				App.logFile.WriteLine("DEBUG 3: File loaded in memory. Creating connection string.");

				//Create the appropriate connection string
				string providerConnectionString = "Initial Catalog=" + dbName + ";Data Source=" + dbServer + ";MultipleActiveResultSets=True;";
				if (dbAuthType == AuthenticationMode.Windows)
					providerConnectionString += "Trusted_Connection=True;";
				else
					providerConnectionString += "Password=" + Properties.Settings.Default.SQLUserPassword + ";User ID=" + dbNewLogin + ";";

				string entityConnectionString = "metadata=res://DataModel/SpiraDataModel.csdl|res://DataModel/SpiraDataModel.ssdl|res://DataModel/SpiraDataModel.msl;provider=System.Data.SqlClient;provider connection string=\"" + providerConnectionString + "\"";

				//Now search and replace our needed values.
				// - Connection String
				App.logFile.WriteLine("DEBUG 4: Now loading values into web.config. ConnectionString");
				XmlNode node = xmlDoc.SelectSingleNode("/configuration/connectionStrings/add[@name='SpiraTestEntities']") as XmlElement;
				if (node != null) ((XmlElement)node).SetAttribute("connectionString", entityConnectionString);
				else App.logFile.WriteLine("Updating " + fileName + ": cannot find ConnectionString token.");

				// - Application Theme
				App.logFile.WriteLine("DEBUG 5: Now loading values into web.config. Application Theme");
				node = xmlDoc.SelectSingleNode("/configuration/system.web/pages") as XmlElement;
				if (node != null) ((XmlElement)node).SetAttribute("theme", themeName);
				else App.logFile.WriteLine("Updating " + fileName + ": cannot find Theme token.");

				// - Event Log Source
				App.logFile.WriteLine("DEBUG 6: Now loading values into web.config. EventLog Source");
				node = xmlDoc.SelectSingleNode("/configuration/applicationSettings/Inflectra.SpiraTest.Common.Properties.Settings/setting[@name='EventLogSource']/value") as XmlElement;
				if (node != null) node.InnerText = eventLogSource;
				else App.logFile.WriteLine("Updating " + fileName + ": cannot find EventLogSource token.");

				// - License Editble
				App.logFile.WriteLine("DEBUG 7: Now loading values into web.config. License Editable");
				node = xmlDoc.SelectSingleNode("/configuration/applicationSettings/Inflectra.SpiraTest.Common.Properties.Settings/setting[@name='LicenseEditable']/value") as XmlElement;
				if (node != null) node.InnerText = licenseEditable;
				else App.logFile.WriteLine("Updating " + fileName + ": cannot find LicenseEditable token.");

				// - Default DB revision.
				App.logFile.WriteLine("DEBUG 8: Now loading values into web.config. Defaut DB Revision");
				node = xmlDoc.SelectSingleNode("/configuration/userSettings/Inflectra.SpiraTest.Common.ConfigurationSettings/setting[@name='Database_Revision']/value") as XmlElement;
				if (node != null) node.InnerText = Properties.Settings.Default.SQLUserPassword;
				else App.logFile.WriteLine("Updating " + fileName + ": cannot find DBRev token.");

				// - Version Section
				App.logFile.WriteLine("DEBUG 8: Now loading values into web.config. Installer Version");
				node = xmlDoc.SelectSingleNode("/configuration/installerSettingsGroup/installerSettings/version") as XmlElement;
				if (node != null)
				{
					App.logFile.WriteLine("DEBUG 8A: Now loading values into web.config. Update Existing Tag");
					((XmlElement)node).SetAttribute("program", Properties.Settings.Default.ProductVersion);
					((XmlElement)node).SetAttribute("installer", Assembly.GetExecutingAssembly().GetName().Version.ToString());
					((XmlElement)node).SetAttribute("flavor", App._installationOptions.ProductName.Replace("Spira", ""));
					((XmlElement)node).SetAttribute("type", App._installationOptions.InstallationType.ToString());
					((XmlElement)node).SetAttribute("date", DateTime.UtcNow.Ticks.ToString());
					//The InstallUID is not touched if it exists. If it does not, create it.
					if (node.Attributes["inst_uid"] == null)
						((XmlElement)node).SetAttribute("inst_uid", App.InstallationUID.ToString());
				}
				else
				{
					App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag");
					//We have to create it. Get it's parent.
					node = xmlDoc.SelectSingleNode("/configuration") as XmlElement;
					if (node != null)
					{
						App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag");
						//Make our version ele,ment.
						XmlElement nodeVer = xmlDoc.CreateElement("version");
						nodeVer.SetAttribute("program", Properties.Settings.Default.ProductVersion);
						nodeVer.SetAttribute("installer", Assembly.GetExecutingAssembly().GetName().Version.ToString());
						nodeVer.SetAttribute("flavor", App._installationOptions.ProductName.Replace("Spira", ""));
						nodeVer.SetAttribute("type", App._installationOptions.InstallationType.ToString());
						nodeVer.SetAttribute("date", DateTime.UtcNow.Ticks.ToString());
						nodeVer.SetAttribute("inst_uid", App.InstallationUID.ToString());

						//Now create its parent element, 'installerSettings'.
						App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag - installerSettings");
						XmlElement nodeInstall = xmlDoc.CreateElement("installerSettings");
						nodeInstall.AppendChild(nodeVer);

						//Now create its parent element, 'installerSettingsGroup'
						App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag - installerSettingsGroup");
						XmlElement nodeGroup = xmlDoc.CreateElement("installerSettingsGroup");
						nodeGroup.AppendChild(nodeInstall);

						//Now add that to our configuration node.
						node.AppendChild(nodeGroup);

						//Now we ALSO need to create the definition part.
						App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag - configurations");
						node = node.SelectSingleNode("configSections") as XmlElement;
						if (node != null)
						{
							App.logFile.WriteLine("DEBUG 8B: Now loading values into web.config. New Tag - configurations");
							//Create the section node.
							XmlElement nodeSec = xmlDoc.CreateElement("section");
							nodeSec.SetAttribute("name", "installerSettings");
							nodeSec.SetAttribute("type", "Inflectra.SpiraTest.Web.Classes.Config_InstallerSettings");

							//Now create the group node.
							XmlElement nodeGrp = xmlDoc.CreateElement("sectionGroup");
							nodeGrp.SetAttribute("name", "installerSettingsGroup");
							nodeGrp.AppendChild(nodeSec);

							//And add that to our configSection.
							node.AppendChild(nodeGrp);
						}
						else 
							App.logFile.WriteLine("Updating " + fileName + ": cannot add new section to config/configSections.");
					}
				}

				//Save the file.
				App.logFile.WriteLine("DEBUG 9: Saving file.");
				bool fileSucc = Utilities.SaveXMLDoc(xmlDoc, fileName);
				if (!fileSucc)
					throw new Exception("Error writing file.");

				App.logFile.WriteLine("DEBUG 10: Update Progress.");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });

				//We need to write out the datasync config file here.
				if (App._installationOptions.DataSyncFile != null)
				{
					App.logFile.WriteLine("DEBUG 11: Update Progress.");
					DataSyncConfig.SaveSettingsToFile(
					   Path.Combine(configFilePath, "DataSync", Constants.DATASYNC_CONFIG),
					   App._installationOptions.DataSyncFile
				   );
				}

				App.logFile.WriteLine("DEBUG 1@: DONE");
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to update the Web.config file: " + ex.Message);

				string ErrorMsg = "Unable to update the Web.config file:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}

		private static XmlTextReader openWebConfig(string webconfig)
		{
			var xmlReader = new XmlTextReader(File.OpenRead(webconfig))
			{
				//Namespaces = false
			};

			return xmlReader;
		}
	}
}
