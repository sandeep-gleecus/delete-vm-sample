using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Common
{
	/// <summary>
	/// The products which we can have a license for
	/// </summary>
	/// <remarks>The enum values match the ProductLicenseNumber fields in the ProductType table</remarks>
	public enum LicenseProductNameEnum
	{
		None = 0,
		SpiraTest = 1,
		SpiraPlan = 2,
		SpiraTeam = 3,
		ValidationMaster = 4
	}

	/// <summary>
	/// The types of license supported
	/// </summary>
	/// <remarks>Only ConcurrentUsers, Demonstration and Enterprise currently supported</remarks>
	public enum LicenseTypeEnum
	{
		None = 0,
		Demonstration = 1,
		ConcurrentUsers = 2,
		NamedUsers = 3,
		Enterprise = 4
	}

	/// <summary>
	/// This contains the information regarding the license being used to SpiraTest
	/// </summary>
	//[System.Diagnostics.DebuggerStepThrough]
	public static class License
	{
		private static LicenseProductNameEnum licenseProductName = LicenseProductNameEnum.None;
		private static LicenseTypeEnum licenseType = LicenseTypeEnum.None;
		private static string organization = "";
		private static string version = "";
		private static string licenseKey = "";
		private static int number = 0;
		private static DateTime? expiration = null;
		private static bool loadAttempted = false;

		private const string DATE_PERPETUAL = "PERPETUAL";

		private const string CLASS_NAME = "Inflectra.SpiraTest.Common.License::";

		//The products currently supported and the current version number
		public const string CURRENT_VERSION = "v7.0.0.0";

		#region Public Methods

		/// <summary>
		/// The constructor method
		/// </summary>
		static License()
		{
			//Call Load() if not already called before
			if (!loadAttempted)
			{
				Load();
				loadAttempted = true;
			}
		}

		/// <summary>
		/// Resets the license information back to its 'empty state'
		/// </summary>
		public static void Reset()
		{
			licenseProductName = LicenseProductNameEnum.None;
			licenseType = LicenseTypeEnum.None;
			organization = "";
			version = "";
			licenseKey = "";
			loadAttempted = false;
			expiration = null;
		}

		/// <summary>
		/// Loads in the license information from the application configuration settings table
		/// and populates the license object with the type of license found (if any)
		/// </summary>
		public static void Load()
		{
			//Get the product name, license key and organization name from the TST_GLOBAL_SETTINGS
			string licenseKey = ConfigurationSettings.Default.License_LicenseKey;
			string organization = ConfigurationSettings.Default.License_Organization;

			//Call the validate method to populate the license object
			Validate(organization, licenseKey);
		}

		/// <summary>
		/// Clears the license information
		/// </summary>
		public static void Clear()
		{
			LicenseProductName = LicenseProductNameEnum.None;
			LicenseType = LicenseTypeEnum.None;
			Number = 0;
			Version = "";
			Organization = "";
			LicenseKey = "";
			number = 0;
		}

		/// <summary>Generates an encrypted license key from the clear contents</summary>
		/// <param name="clearLicense">The clear contents of the license</param>
		/// <returns>The encrypted license key</returns>
		/// <remarks>Used by the unit tests to test the license keys</remarks>
		internal static string Encrypt(string clearLicense)
		{
			SimpleAES simpleAES = new SimpleAES();
			licenseKey = simpleAES.EncryptToBase64String(clearLicense);
			return licenseKey;
		}

		/// <summary>Validates the product name, license key and organization and populates the license object</summary>
		/// <param name="productNameToCheck">The product name to check</param>
		/// <param name="organizationToCheck">The name of the organization owning the license</param>
		/// <param name="licenseKey">The license key hashed string</param>
		public static void Validate(string organizationToCheck, string licenseKey)
		{
			//First we need to do a reset
			Reset();

			//Next decode the license
			if (!string.IsNullOrEmpty(licenseKey))
			{
				try
				{
					SimpleAES simpleAES = new SimpleAES();
					string clearLicense = simpleAES.DecryptBase64String(licenseKey);

					//Split out the elements
					string[] licenseParts = clearLicense.Split(':');
					if (licenseParts.Length >= 5)
					{
						//Set the basic proerties..
						Organization = licenseParts[0].Trim();
						LicenseProductName = GetProductNumberForName(licenseParts[1]);
						Version = licenseParts[2];
						string licenseType = licenseParts[3];
						string numberUsers, expDate;
						if (licenseParts.Length >= 6)
						{
							numberUsers = licenseParts[4];
							expDate = licenseParts[5];
						}
						else
						{
							numberUsers = "1";
							expDate = licenseParts[4];
						}

						//Handle priocessing of the values!
						// - License Type
						int licenseTypeId;
						int.TryParse(licenseType, out licenseTypeId);
						LicenseType = (LicenseTypeEnum)licenseTypeId;
						// - Number of Users
						int numUser;
						if (int.TryParse(numberUsers, out numUser))
						{
							//Check for stupid entry.
							if (numUser < 1 && LicenseType != LicenseTypeEnum.Demonstration) numUser = 1;
							else if (numUser < 3 && LicenseType == LicenseTypeEnum.Demonstration) numUser = 3;
							//Save it.
							Number = numUser;
						}
						// - Expiration Date
						/*if (expDate == DATE_PERPETUAL) 
						*	Expiration = null;
						* else  // Unnecessary. After a Reset(), Expiration is already null.
						*/
						{
							DateTime expiration;
							if (DateTime.TryParseExact(expDate, "yyyyMMdd", new CultureInfo("en-US", true), DateTimeStyles.None, out expiration))
								Expiration = expiration;
						}
					}

					//Now check that the info THEY entered in is accurate. (Only need to check their entered Organization name and the Version.
					if (Version != CURRENT_VERSION || Organization != organizationToCheck)
					{
						Reset();
					}
				}
				catch (Exception ex)
				{
					//Unable to decrypt the license key, just ignore and license will be invalid
					Reset();
				}
			}
		}

		/// <summary>
		/// Retrieves a list of active product types (used for license management screens)
		/// </summary>
		/// <returns>List of active product types</returns>
		public static List<ProductType> RetrieveProductTypes()
		{
			const string METHOD_NAME = "RetrieveProductTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProductType> productTypes;

				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Get the list of active product types
					var query = from p in context.ProductTypes
								where p.IsActive()
								orderby p.ProductLicenseNumber
								select p;

					productTypes = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return productTypes;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns the product enumeration value associated with the name</summary>
		/// <param name="productName">The name of the licensed product</param>
		/// <returns>The corresponding product Number</returns>
		private static LicenseProductNameEnum GetProductNumberForName(string productName)
		{
			const string METHOD_NAME = CLASS_NAME + "GetProductNumberForName()";
			Logger.LogEnteringEvent(METHOD_NAME);

			System.Data.DataSet productTypeDataSet = new System.Data.DataSet();

			try
			{
				//Create the query to get the product enum
				ProductType productType;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Get the list of active product types
					var query = from p in context.ProductTypes
								where p.IsActive() && p.Name == productName
								select p;

					productType = query.FirstOrDefault();
				}

				if (productType == null)
				{
					Logger.LogExitingEvent(METHOD_NAME);
					return LicenseProductNameEnum.None;
				}
				else
				{
					Logger.LogExitingEvent(METHOD_NAME);
					return (LicenseProductNameEnum)productType.ProductLicenseNumber;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The product being licensed
		/// </summary>
		public static LicenseProductNameEnum LicenseProductName
		{
			get
			{
				return licenseProductName;
			}
			set
			{
				licenseProductName = value;
			}
		}

		/// <summary>
		/// The type of license we have
		/// </summary>
		public static LicenseTypeEnum LicenseType
		{
			get
			{
				return licenseType;
			}
			set
			{
				licenseType = value;
			}
		}

		/// <summary>
		/// The version of the product the license covers
		/// </summary>
		public static string Version
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
			}
		}

		/// <summary>
		/// The actual license key code
		/// </summary>
		public static string LicenseKey
		{
			get
			{
				return licenseKey;
			}
			set
			{
				licenseKey = value;
			}
		}

		/// <summary>
		/// The date that the demonstration version expires
		/// </summary>
		/// <remarks>
		/// A null value means either unlicensed or perpetual depending on the license type enum value
		/// </remarks>
		public static DateTime? Expiration
		{
			get
			{
				return expiration;
			}
			set
			{
				expiration = value;
			}
		}

		/// <summary>
		/// The Organization owning the license
		/// </summary>
		public static string Organization
		{
			get
			{
				return organization;
			}
			set
			{
				organization = value;
			}
		}

		/// <summary>
		/// The Number of licences owned
		/// </summary>
		public static int Number
		{
			get
			{
				return number;
			}
			set
			{
				number = value;
			}
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown when you exceed the number of allowed license connections
	/// </summary>
	public class LicenseViolationException : ApplicationException
	{
		public LicenseViolationException()
		{
		}
		public LicenseViolationException(string message)
			: base(message)
		{
		}
		public LicenseViolationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
