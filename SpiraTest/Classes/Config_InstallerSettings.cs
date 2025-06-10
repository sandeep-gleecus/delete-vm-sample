using System;
using System.Configuration;

namespace Inflectra.SpiraTest.Web.Classes
{
	/// <summary>
	/// This is used to be able to define installer setting sin the web.config file. It is not used in code.
	/// It must exist, or there will be an error thrown when the IIS App pool tries to load the config file.
	/// </summary>
	public class Config_InstallerSettings : ConfigurationSection
	{
		/// <summary>The only element allowed in the config section.</summary>
		[ConfigurationProperty("version", IsRequired = true)]
		public VersionElement Version
		{
			get
			{
				return (VersionElement)this["version"];
			}
			set { }
		}
	}

	/// <summary>Class that holds version information.</summary>
	public class VersionElement : ConfigurationElement
	{
		/// <summary>The version of the application being installed. (i.e. "6.4.0.1")</summary>
		[ConfigurationProperty("program", IsRequired = true)]
		public Version Program
		{
			get
			{
				return (Version)this["program"];
			}
			set { }
		}

		/// <summary>The version of the installer that ran this installation. (i.e. "1.0.5.0")</summary>
		[ConfigurationProperty("installer", IsRequired = true)]
		public Version Installer
		{
			get
			{
				return (Version)this["installer"];
			}
			set { }
		}

		/// <summary>The flavor of the applicaiton. (i.e. "SpiraTest")</summary>
		[ConfigurationProperty("flavor", IsRequired = true)]
		public string Flavor
		{
			get
			{
				return (string)this["installer"];
			}
			set { }
		}

		/// <summary>The original type of the installation. (i.e. "FullInstall")</summary>
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get
			{
				return (string)this["type"];
			}
		}

		/// <summary>Unique tracking ID of this installation.summary>
		[ConfigurationProperty("inst_uid", IsRequired = true)]
		public Guid InstallationID { get; set; }
	}
}