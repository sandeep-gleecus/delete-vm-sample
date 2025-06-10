using System;
using System.Data.Metadata.Edm;
using System.IO;

namespace Inflectra.SpiraTest.Common
{
	/// <summary>
	/// Stores any global constants / enums used by the whole application regardless of tier
	/// </summary>
	public static class Global
	{
		public const int PROJECT_ID_SPIRA = 6; //SpiraPlan

		/// <summary>The required database revision for this release.</summary>
		public static int REQUIRED_DATABASE_REVISION = 700; //674
		/// <summary>Different consts used to update the version number in the app itself</summary>
		public const string VERSION_STRING_FULL = "7.0";//"6.7.0.0";
		public const string VERSION_STRING_NO_BUILD = "7.0";//"6.7.0";
		public const string VERSION_STRING_SYSTEM = "0700000";//"06070000";
		public const int VERSION_NUMBER_BUILD = 0;
		public const int RELEASE_ID_SPIRA = 377; // 6.7.0.0
		public const string VERSION = "7.0";//"6.7.0.0";

		public static string associationTabName="";
		public static int Riskid = 0;
		public static int Risk_Releaseid = 0;
		public static int addbuttonCount = 0;
		public static int PlainingBoardUserid = 0;
		public static bool isincident = false;
		public static int Risk_DetailsRiskid = 0;
		public static int Incident_DetailsIncidentid = 0;
		public static int Incident_NewDetailsIncidentid = 0;
		public static int Risk_NewDetailsIncidentid = 0;
		public static string ProfileSucessmessage = null;
		public static int addbuttonclickCount = 0;
		public static bool ISIncident_Details = false;
		public static bool ISrisk_Details = false;
		public static int modifieddocumentid = 0;
		public static string Tgmodifieddocumentname = null;
		public static bool IsOldToolExport = false;
		public static bool BackIsOldToolExport = false;
		public static int incident_Incidentid = 0;
		public static int Assco_TestsetartifactId = 0;
		public static bool ISTestsetFilter = false;
		public static bool ISRisksetFilter = false;
		public static bool ISTaskFilter = false;


		/// <summary>
		/// Stores custom type-codes that we need to serialize/deserialize
		/// </summary>
		public enum CustomTypeCodes
		{
			DateRange = -2,
			MultiValueFilter = -3,
			DecimalRange = -4,
			IntRange = -5,
			EffortRange = -6,
			LongRange = -7
		}

		//Matches keyword strings (e.g. keyword and "keyword phrase")
		public const string REGEX_KEYWORD_MATCHER = @"(\b\w+\b)|(""\w[\s\w]+\w"")";

		//Artifact token regex matcher (lower and upper case)
		public const string REGEX_ARTIFACT_TOKEN_MATCHER = @"^\[?(RQ|TC|IN|RL|TR|TK|TS|TX|AH|AE|PR|PL|RS|DC|US|PG|RK|RM)[:]?([0-9]+)\]?";
		//Matches filenames to make sure they are 'safe'
		public const string VALIDATION_REGEX_FILENAME_XML = @"^[a-zA-Z0-9\-_]+$";

		/// <summary>The folder that we're storing our cache in.</summary>
		public static string CACHE_FOLDERPATH = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			"Inflectra\\Spira");

		/// <summary>
		/// Determines if we should be using TaraVault or an external source code management solution
		/// </summary>
		/// <returns>
		/// True if cloud hosted AND user has not chosen to use a thiry party provider
		/// </returns>
		public static bool Feature_TaraVault
		{
			get
			{
				return (!Properties.Settings.Default.LicenseEditable && ConfigurationSettings.Default.SourceCode_UseTaraVaultOnCloud);
			}
		}

		/// <summary>
		/// Determines if we risks are enabled for the application
		/// </summary>
		/// <returns>
		/// True if risks are enabled (requirement is that the product is SpiraPlan)
		/// </returns>
		public static bool Feature_Risks
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Determines if portfolios are enabled for the application
		/// </summary>
		/// <returns>
		/// True if portfolios are enabled (requirement is that the product is SpiraPlan)
		/// </returns>
		public static bool Feature_Portfolios
		{
			get
			{
				return true;
			}
		}
		/// <summary>
		/// Determines if we tasks are enabled for the application
		/// </summary>
		/// <returns>
		/// True if tasks are enabled (requirement is that the product is SpiraTeam or SpiraPlan)
		/// </returns>
		public static bool Feature_Tasks
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Determines if we tasks are enabled for the application
		/// </summary>
		/// <returns>
		/// True if tasks are enabled (requirement is that the product is SpiraTeam or SpiraPlan)
		/// </returns>
		public static bool Feature_SourceCode
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Determines if should include source code revisions on the documents/artifact associations pages since it can slow down the system 
		/// </summary>
		public static bool SourceCode_IncludeInAssociationsAndDocuments
		{
			get
			{
				return true;
			}
		}


		/// <summary>
		/// Determines if baselining is available to be used in the application
		/// </summary>
		/// <returns>
		/// True if baselining is available to be used (requirement is that the product is not SpiraTest)
		/// </returns>
		public static bool Feature_Baselines
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Determines if ODATA APIs are available for use
		/// </summary>
		/// <returns>
		/// True if ODATA is available (requirement is that the product is SpiraPlan as of 6.9)
		/// </returns>
		public static bool Feature_OData
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Is LDAP enabled for authentication of users. May not be sufficiently configured for importing users
		/// </summary>
		public static bool Ldap_IsEnabledForAuthentication
		{
			get
			{
				return !string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_Host);
			}
		}

		/// <summary>
		/// Is LDAP configured sufficiently to allow an admin to import new users
		/// </summary>
		/// <remarks>
		/// In addition to LDAP being enabled, the following fields need to be populated for import to work:
		/// 
		///- LDAP Server
		///- Base DN
		///- Bind DN
		///- Bind Password
		///- Attributes
		///First Name
		///Last Name
		///Email
		///Login
		/// </remarks>
		public static bool Ldap_IsEnabledForNewUserImport
		{
			get
			{
				bool isEnabled =
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_Host) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_BaseDn) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_BindDn) &&
					!string.IsNullOrWhiteSpace(SecureSettings.Default.Ldap_BindPassword) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_FirstName) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_LastName) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_EmailAddress) &&
					!string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Ldap_Login);

				return isEnabled;
			}
		}
	}
}
