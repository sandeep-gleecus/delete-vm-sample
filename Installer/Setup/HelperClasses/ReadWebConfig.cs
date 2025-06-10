using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	static class ReadWebConfig
	{
		/// <summary>Loads the initial settings from Web.config if it exists</summary>
		static public WebConfigSettings LoadSettingsFromWebConfig(string fileName)
		{
			//The return value.
			WebConfigSettings retVal = new WebConfigSettings();

			//The Web.Config file should be one level up
			if (File.Exists(fileName))
			{
				//Open as a XML doc
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(fileName);

				//Locate the connection string
				XmlElement connectNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/connectionStrings/add[@name='SpiraTestEntities']");
				//Get the connect node
				if (connectNode != null)
				{
					//Get the connection string
					string entityConnectionString = connectNode.Attributes["connectionString"].Value;
					if (!string.IsNullOrEmpty(entityConnectionString))
					{
						//Now get the provider connection string
						EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder(entityConnectionString);
						retVal.ConnectionString = ecsb.ProviderConnectionString;
					}
				}

				XmlElement auditConnectNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/connectionStrings/add[@name='AuditTrailEntities']");
				//Get the connect node
				if (auditConnectNode != null)
				{
					//Get the connection string
					string entityConnectionString = auditConnectNode.Attributes["connectionString"].Value;
					if (!string.IsNullOrEmpty(entityConnectionString))
					{
						//Now get the provider connection string
						EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder(entityConnectionString);
						retVal.AuditConnectionString = ecsb.ProviderConnectionString;
					}
				}

				//Now get the installer Settings.
				XmlElement instNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/installerSettingsGroup/installerSettings/version");
				if (instNode != null)
				{
					//Get the Program Version.
					string pgmVer = instNode.Attributes["program"].Value;
					string instVer = instNode.Attributes["installer"].Value;
					retVal.AppFlavor = instNode.Attributes["flavor"].Value;
					retVal.InstallType = instNode.Attributes["type"].Value;

					if (pgmVer != null)
						retVal.VersionProgram = new Version(pgmVer);
					if (instVer != null)
						retVal.VersionInstaller = new Version(instVer);
				}

				//Set default program ver.
				if (retVal.VersionProgram == null)
					retVal.VersionProgram = new Version("5.4.0.0");

				//Get the App Theme
				XmlElement themeNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/system.web/pages");
				if (themeNode != null)
				{
					//Get the App Theme
					retVal.AppTheme = themeNode.Attributes["theme"].Value;
				}

				//Get whether the license is editable or not.
				XmlElement editNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/applicationSettings/Inflectra.SpiraTest.Common.Properties.Settings/setting[@name='LicenseEditable']/value");
				if (editNode != null)
				{
					string boolVal = editNode.InnerText;
					bool isGood;
					if (bool.TryParse(boolVal, out isGood))
						retVal.LicenseEditable = isGood;
				}

				//Get the current event log source.
				XmlElement evtNode = (XmlElement)xmlDoc.SelectSingleNode("configuration/applicationSettings/Inflectra.SpiraTest.Common.Properties.Settings/setting[@name='EventLogSource']/value");
				if (evtNode != null)
				{
					retVal.EventLogSource = evtNode.InnerText;
				}

				//In case the written version is wrong, let's load the web.dll file and get the AssemblyFileVersion
				string strWebDll = Path.Combine(Path.GetDirectoryName(fileName), "bin", "web.dll");
				if (File.Exists(strWebDll))
				{
					FileVersionInfo webVersionInfo = FileVersionInfo.GetVersionInfo(strWebDll);
					if (webVersionInfo != null)
					{
						Version webVersion = new Version(webVersionInfo.FileVersion);

						if (!webVersion.Equals(retVal.VersionProgram))
						{
							App.logFile.WriteLine("Version in webconfig not equal DLL version.");
							retVal.VersionProgram = webVersion;
						}
					}
				}
			}

			return retVal;
		}

		static public void LoadAppSettingsFromWeb(WebConfigSettings settings)
		{
			//First, modify the string to add the persist security info.
			// See: https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlconnection.connectionstring?view=netframework-4.8#remarks
			string conStr = "Persist Security Info=True;" +
				settings.ConnectionString;
			// Added note - apparently, adding 'Persist Security Info' still does not create the credential.

			//Save it for later.
			App._installationOptions.ExistingConnectionString = settings.ConnectionString;

			//Save the whole webconfig settings into its object for later.
			App._installationOptions.existingSettings = settings;

			//Load up the settings we need. (Database information, mostly.)
			//Parse the connection string for the server and database 
			SqlConnection sqlConnection = new SqlConnection(settings.ConnectionString);
			SqlConnection auditSqlConnection = new SqlConnection(settings.AuditConnectionString);

			//SQL Server
			App._installationOptions.SQLServerAndInstance = sqlConnection.DataSource;

			App._installationOptions.AuditSQLServerAndInstance = auditSqlConnection.DataSource;
			//Database Name
			App._installationOptions.DatabaseName =
				App._installationOptions.DatabaseUser = sqlConnection.Database;

			App._installationOptions.AuditDatabaseName =
				App._installationOptions.AuditDatabaseUser = auditSqlConnection.Database;

			//Exract the user and password, in case it's needed.

			if (sqlConnection.Credential != null)
			{
				App._installationOptions.DatabaseAuthentication = AuthenticationMode.SqlServer;
				App._installationOptions.DatabaseUser = sqlConnection.Credential.UserId;
				App._installationOptions.SQLInstallLogin = sqlConnection.Credential.UserId;
				App._installationOptions.SQLInstallPassword = sqlConnection.Credential.Password.ToString();
			}
			else
			{
				//See if the connection string contains User Id and Password.
				if (conStr.ToLowerInvariant().Contains("user id"))
				{
					//Split the string up into components..
					List<string> props = conStr.Split(';').ToList();
					//Go through each one, if we find the user/pass, save it!
					foreach (var prop in props)
					{
						//Split the values on the '='.
						string[] kvp = prop.Split('=');
						//Get key/value.
						string key = kvp[0];
						string value = "";
						if (kvp.Count() > 1) value = kvp[1];
						if (key.ToLowerInvariant().Equals("user id"))
							App._installationOptions.SQLInstallLogin = value;
						else if (key.ToLowerInvariant().Equals("password"))
							App._installationOptions.SQLInstallPassword = value;
					}

					//Now check that we have at least a user. (Not sure if it's possible to have a user w/o pass,
					//  so now we will assume Password isn't necessary.
					if (!string.IsNullOrWhiteSpace(App._installationOptions.SQLInstallLogin))
						App._installationOptions.DatabaseAuthentication = AuthenticationMode.SqlServer;
					else
						App._installationOptions.DatabaseAuthentication = AuthenticationMode.Windows;
				}
				else
				{
					App._installationOptions.DatabaseAuthentication = AuthenticationMode.Windows;
				}
			}

			if (auditSqlConnection.Credential != null)
			{
				App._installationOptions.AuditDatabaseAuthentication = AuthenticationMode.SqlServer;
				App._installationOptions.AuditDatabaseUser = auditSqlConnection.Credential.UserId;
				App._installationOptions.AuditSQLInstallLogin = auditSqlConnection.Credential.UserId;
				App._installationOptions.AuditSQLInstallPassword = auditSqlConnection.Credential.Password.ToString();
			}
			else
			{
				//See if the connection string contains User Id and Password.
				if (conStr.ToLowerInvariant().Contains("user id"))
				{
					//Split the string up into components..
					List<string> props = conStr.Split(';').ToList();
					//Go through each one, if we find the user/pass, save it!
					foreach (var prop in props)
					{
						//Split the values on the '='.
						string[] kvp = prop.Split('=');
						//Get key/value.
						string key = kvp[0];
						string value = "";
						if (kvp.Count() > 1) value = kvp[1];
						if (key.ToLowerInvariant().Equals("user id"))
							App._installationOptions.AuditSQLInstallLogin = value;
						else if (key.ToLowerInvariant().Equals("password"))
							App._installationOptions.AuditSQLInstallPassword = value;
					}

					//Now check that we have at least a user. (Not sure if it's possible to have a user w/o pass,
					//  so now we will assume Password isn't necessary.
					if (!string.IsNullOrWhiteSpace(App._installationOptions.AuditSQLInstallLogin))
						App._installationOptions.AuditDatabaseAuthentication = AuthenticationMode.SqlServer;
					else
						App._installationOptions.AuditDatabaseAuthentication = AuthenticationMode.Windows;
				}
				else
				{
					App._installationOptions.AuditDatabaseAuthentication = AuthenticationMode.Windows;
				}
			}
		}
	}
}
