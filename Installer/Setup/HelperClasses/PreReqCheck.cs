using Microsoft.Win32;
using System;
using System.Linq;
using System.ServiceProcess;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Offers funcions to check if PreReqs are installed.</summary>
	public static class PreReqCheck
	{
		/// <summary>Returns whether IIS is installed or not.</summary>
		public static bool IsIISInstalled()
		{
			//Checks is the service is installed. Checks the service, and not registry ketys, since
			//  reports show that registry keys are NOT removed after IIS is uninstalled.
			bool retVal = false;
			var svc = ServiceController.GetServices();
			if (svc != null)
			{
				retVal = svc.Any(s =>
					s.ServiceName.Equals("w3svc", StringComparison.InvariantCultureIgnoreCase)
				);
			}

			return retVal;
		}

		/// <summary>Searches the registry key for the given IIS key to see if the feature is installed.</summary>
		/// <param name="key">The key to search for.</param>
		public static bool IsIISFeatureInstalled(string key)
		{
			bool retVal = false;

			//See what we're checking for.
			if (!string.IsNullOrWhiteSpace(key) && key.Contains(':'))
			{
				//Split it..
				string sys = key.Split(':')[0].Trim();
				key = key.Split(':')[1].Trim();

				//It's IIS.
				if (sys == "IIS")
				{
					//Create the Registry Key object.
					RegistryKey regKey = Registry.LocalMachine;
					if (regKey != null)
					{
						regKey = regKey.OpenSubKey(@"Software\Microsoft\InetStp\Components\");
					}

					if (regKey != null)
					{
						object val = regKey.GetValue(key, (int)0);
						if (val is int)
						{
							//See if we have a value and it's 1.
							int val2 = (int)val;
							retVal = (val2 == 1);
						}
					}
				}
				//It's .NET
				else
				{
					//TODO.
					retVal = false;
				}
			}

			return retVal;
		}
	}
}

/* https://johnlnelson.com/2014/06/15/the-microsoft-web-administration-namespace/  */
