using System;
using System.Xml;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	static class DataSyncConfig
	{
		/// <summary>Loads the initial settings from Web.config if it exists</summary>
		/// <param name="fileName">The file to read the settings from.</param>
		/// <returns>A colleciton of settings loaded from the file.</returns>
		static internal SettingsDataSync LoadSettingsFromFile(string fileName)
		{
			//The return value.
			SettingsDataSync retVal = new SettingsDataSync();

			//Open as a XML doc
			XmlDocument xmlDoc = Utilities.LoadXMLDoc(fileName);

			if (xmlDoc != null)
			{
				//Load our settings.
				XmlNodeList docSettings = xmlDoc.SelectNodes("configuration/applicationSettings/Inflectra.SpiraTest.DataSyncService.Properties.Settings/setting");

				foreach (XmlElement setting in docSettings)
				{
					//For each element ("setting"), we need to get the name first..
					string settingName = setting.GetAttribute("name");
					//Let's get the value, then!
					string settingValue = setting["value"].InnerText;

					//Now process the string as needed.
					switch (settingName)
					{
						case "PollingInterval":
							// Should be a number. If it's unreadable, we default to 60000. 
							retVal.PollingInterval = 600000;
							int tryVal = 0;

							if (int.TryParse(settingValue, out tryVal))
								retVal.PollingInterval = tryVal;

							break;

						case "WebServiceUrl":
							//If the value contains an untranslated token, we will replace it. 
							//  Otherwise, copy it over.
							if (settingValue.Contains("[VDIR]"))
								retVal.WebServiceUrl = null;
							else
								try
								{
									retVal.WebServiceUrl = new Uri(settingValue, UriKind.Absolute);
								}
								catch
								{
									retVal.WebServiceUrl = null;
								}
							break;

						case "Login":
							//Copied over as-in.
							retVal.SpiraLogin = settingValue;
							break;

						case "Password":
							//Copied over as-in.
							retVal.SpiraPassword = settingValue;
							break;

						case "EventLogSource":
							//Need to specifiy proper value if it still has the token in it.
							if (settingValue.Equals("[EVENTLOGSOURCE]"))
								retVal.EventLogSource = string.Format(Constants.EVENT_LOG_SOURCE_DATA_SYNC, App._installationOptions.ProductName);
							else
								retVal.EventLogSource = settingValue;
							break;

						case "TraceLogging":
							//Try to convert the string into a boolan. Otherwise, default to false.
							bool tryBol;
							if (bool.TryParse(settingValue, out tryBol))
								retVal.TraceLogging = tryBol;

							break;
					}
				}
			}

			return retVal;
		}

		/// <summary>Given the settings and the filename, will update values in the file with the given new values.</summary>
		/// <param name="fileName">The file to update.</param>
		/// <param name="newSettings">New settings to write out.</param>
		/// <returns>Status of successful write.</returns>
		static public bool SaveSettingsToFile(string fileName, SettingsDataSync newSettings)
		{
			App.logFile.WriteLine("Saving data to DataSync Service file.");

			bool retVal = false;

			//Open as a XML doc
			App.logFile.WriteLine("DEBUG 1: Loading document.");
			XmlDocument xmlDoc = Utilities.LoadXMLDoc(fileName);
			if (xmlDoc != null)
			{
				//Now we get our node.
				XmlNodeList docSettings = xmlDoc.SelectNodes("configuration/applicationSettings/Inflectra.SpiraTest.DataSyncService.Properties.Settings/setting");

				App.logFile.WriteLine("DEBUG 2: Scanning docSettings.");
				foreach (XmlElement setting in docSettings)
				{
					//For each element ("setting"), we need to get the name first..
					string settingName = setting.GetAttribute("name");

					switch (settingName)
					{
						case "PollingInterval":
							App.logFile.WriteLine("DEBUG 2A: docSettings: pollingInterval");
							setting["value"].InnerText = newSettings.PollingInterval.ToString();
							break;

						case "WebServiceUrl":
							App.logFile.WriteLine("DEBUG 2B: docSettings: WebServiceUrl");
							if (newSettings.WebServiceUrl == null)
								setting["value"].InnerText = new Uri("http://localhost/" + App._installationOptions.IISApplication + "/")
									.ToString()
									.Replace("//", "/");
							else
								setting["value"].InnerText = newSettings.WebServiceUrl.ToString();
							break;

						case "Login":
							App.logFile.WriteLine("DEBUG 2C: docSettings: Login");
							setting["value"].InnerText = newSettings.SpiraLogin;
							break;

						case "Password":
							App.logFile.WriteLine("DEBUG 2D: docSettings: Password");
							setting["value"].InnerText = newSettings.SpiraPassword;
							break;

						case "EventLogSource":
							App.logFile.WriteLine("DEBUG 2D: docSettings: EventLogSource");
							//Need to specifiy proper value if it still has the token in it.
							if (newSettings.EventLogSource.Equals("[EVENTLOGSOURCE]"))
								setting["value"].InnerText = string.Format(Constants.EVENT_LOG_SOURCE_DATA_SYNC, App._installationOptions.ProductName);
							else
								setting["value"].InnerText = newSettings.EventLogSource;
							break;

						case "TraceLogging":
							App.logFile.WriteLine("DEBUG 2D: docSettings: TraceLogging");
							setting["value"].InnerText = newSettings.TraceLogging.ToString();
							break;
					}
				}

				App.logFile.WriteLine("DEBUG 3: Saving file.");
				retVal = Utilities.SaveXMLDoc(xmlDoc, fileName);
			}

			return retVal;
		}
	}
}
