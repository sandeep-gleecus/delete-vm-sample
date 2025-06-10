using System;

namespace Inflectra.SpiraTest.Installer
{
	/// <summary>
	/// The products which we can have a license for
	/// </summary>
	/// <remarks>The enum values match the ProductLicenseNumber fields in the ProductType table</remarks>
	public enum LicenseProductNameEnum
	{
		None = 0,
		//SpiraTest = 1,
		//SpiraPlan = 2,
		//SpiraTeam = 3,
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
	/// This contains the information regarding the license being used to Spira
	/// </summary>
	public class License
	{
		private LicenseProductNameEnum licenseProductName = LicenseProductNameEnum.None;
		private LicenseTypeEnum licenseType = LicenseTypeEnum.None;
		private string organization = "";
		private string version = "";
		private string licenseKey = "";
		private int number = 0;
		private DateTime? expiration = null;

		private const string DATE_PERPETUAL = "PERPETUAL";

		#region Public Methods

		/// <summary>Resets the license information back to its 'empty state'</summary>
		public void Reset()
		{
			licenseProductName = LicenseProductNameEnum.None;
			licenseType = LicenseTypeEnum.None;
			organization = "";
			version = "";
			licenseKey = "";
			expiration = null;
			Clear();
		}

		/// <summary>Clears the license information</summary>
		public void Clear()
		{
			LicenseProductName = LicenseProductNameEnum.None;
			LicenseType = LicenseTypeEnum.None;
			Number = 0;
			Version = "";
			Organization = "";
			LicenseKey = "";
			number = 0;
		}

		/// <summary>Validates the product name, license key and organization and populates the license object</summary>
		/// <param name="organizationToCheck">The name of the organization owning the license</param>
		/// <param name="licenseKey">The license key hashed string</param>
		/// <returns>True if valid</returns>
		public bool Validate(string organizationToCheck, string licenseKey)
		{
			bool isValid = false;
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
						string organization = licenseParts[0];
						string productName = licenseParts[1];
						string version = licenseParts[2];

						//Make sure the values match for version and organization
						if (organizationToCheck == organization && version == "v" + Properties.Settings.Default.LicenseVersion)
						{
							int licenceTypeId;
							if (int.TryParse(licenseParts[3], out licenceTypeId))
							{
								//Handle the different types
								if (licenceTypeId == (int)LicenseTypeEnum.ConcurrentUsers && licenseParts.Length >= 6)
								{
									int numberUsers;
									if (int.TryParse(licenseParts[4], out numberUsers))
									{
										string date = licenseParts[5];
										if (date == DATE_PERPETUAL)
										{
											LicenseProductName = GetProductNumberForName(productName);
											LicenseType = LicenseTypeEnum.ConcurrentUsers;
											LicenseKey = licenseKey;
											Number = numberUsers;
											Version = version;
											Organization = organization;
											Expiration = null;
											isValid = true;
										}
										else
										{
											System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
											DateTime expiration;
											if (DateTime.TryParseExact(date, "yyyyMMdd", format, System.Globalization.DateTimeStyles.None, out expiration))
											{
												LicenseProductName = GetProductNumberForName(productName);
												LicenseType = LicenseTypeEnum.ConcurrentUsers;
												Number = numberUsers;
												Version = version;
												LicenseKey = licenseKey;
												Organization = organization;
												Expiration = expiration;
												isValid = true;
											}
										}
									}
								}
								if (licenceTypeId == (int)LicenseTypeEnum.Enterprise && licenseParts.Length >= 5)
								{
									string date = licenseParts[4];
									if (date == DATE_PERPETUAL)
									{
										LicenseProductName = GetProductNumberForName(productName);
										LicenseType = LicenseTypeEnum.Enterprise;
										LicenseKey = licenseKey;
										Version = version;
										Organization = organization;
										Expiration = null;
										isValid = true;
									}
									else
									{
										System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
										DateTime expiration;
										if (DateTime.TryParseExact(date, "yyyyMMdd", format, System.Globalization.DateTimeStyles.None, out expiration))
										{
											LicenseProductName = GetProductNumberForName(productName);
											LicenseType = LicenseTypeEnum.Enterprise;
											Version = version;
											LicenseKey = licenseKey;
											Organization = organization;
											Expiration = expiration;
											isValid = true;
										}
									}
								}
								if (licenceTypeId == (int)LicenseTypeEnum.Demonstration && licenseParts.Length >= 6)
								{
									int numberUsers;
									if (int.TryParse(licenseParts[4], out numberUsers))
									{
										//We don't support perpetual demo licenses
										string date = licenseParts[5];
										System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
										DateTime expiration;
										if (DateTime.TryParseExact(date, "yyyyMMdd", format, System.Globalization.DateTimeStyles.None, out expiration))
										{
											LicenseProductName = GetProductNumberForName(productName);
											LicenseType = LicenseTypeEnum.Demonstration;
											Version = version;
											LicenseKey = licenseKey;
											Organization = organization;
											Expiration = expiration;
											Number = numberUsers;
											isValid = true;
										}
									}
								}
							}
						}
					}
				}
				catch (Exception)
				{
					//Unable to decrypt the license key, just ignore and license will be invalid
					Reset();
				}
			}
			return isValid;
		}

		/// <summary>Returns the product enumeration value associated with the name</summary>
		/// <param name="productName">The name of the licensed product</param>
		/// <returns>The corresponding product Number</returns>
		private LicenseProductNameEnum GetProductNumberForName(string productName)
		{
			LicenseProductNameEnum productEnum = LicenseProductNameEnum.None;
			switch (productName.ToLowerInvariant())
			{
				//case "spiratest":
				//	productEnum = LicenseProductNameEnum.SpiraTest;
				//	break;

				//case "spirateam":
				//	productEnum = LicenseProductNameEnum.SpiraTeam;
				//	break;

				//case "spiraplan":
				//	productEnum = LicenseProductNameEnum.SpiraPlan;
				//	break;

				case "validationmaster":
					productEnum = LicenseProductNameEnum.ValidationMaster;
					break;
			}

			return productEnum;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The product being licensed
		/// </summary>
		public LicenseProductNameEnum LicenseProductName
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
		public LicenseTypeEnum LicenseType
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
		public string Version
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
		public string LicenseKey
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
		public DateTime? Expiration
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
		public string Organization
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
		public int Number
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
}
