using System;
using System.Collections.Generic;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the License Key Management System (License class)
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class LicenseTest
	{
		private const string LICENSE_VERSION_TO_TEST = "v6.0.0.0";

		[SetUp]
		public void Init()
		{
		}

		[
		Test,
		SpiraTestCase(23)
		]
		public void _01_ConcurrentLicense()
		{
			//First lets test a SpiraTest 5-user concurrent Common.License that is perpetual
			string testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":002:0005:PERPETUAL");
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual (LicenseProductNameEnum.SpiraTest, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.ConcurrentUsers, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(5, Common.License.Number);
			Assert.IsFalse(Common.License.Expiration.HasValue);

			//First lets test a SpiraPlan 10-user concurrent Common.License that expires in 1 year
			DateTime expiryDate = DateTime.UtcNow.Date.AddYears(1);
			testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":002:0010:" + expiryDate.ToString("yyyyMMdd"));
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual (LicenseProductNameEnum.SpiraPlan, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.ConcurrentUsers, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(10, Common.License.Number);
			Assert.AreEqual(expiryDate.Date, Common.License.Expiration.Value.Date);
		}

		[
		Test,
		SpiraTestCase(48)
		]
		public void _02_EnterpriseLicense()
		{
			//First lets test a SpiraTeam enterprise license that is perpetual
			string testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":004:PERPETUAL");
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual(LicenseProductNameEnum.SpiraTeam, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.Enterprise, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(1, Common.License.Number);
			Assert.IsFalse(Common.License.Expiration.HasValue);

			//Next lets test a SpiraPlan enterprise license that expires in 1-year
			DateTime expiryDate = DateTime.UtcNow.Date.AddYears(1);
			testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":004:" + expiryDate.ToString("yyyyMMdd"));
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual(LicenseProductNameEnum.SpiraPlan, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.Enterprise, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(1, Common.License.Number);
			Assert.AreEqual(expiryDate.Date, Common.License.Expiration.Value.Date);

		}

		[
		Test,
		SpiraTestCase(157)
		]
		public void _03_DemonstrationLicense()
		{
			//Lets test a SpiraTest v1.0 3-user demonstration License that expires in 30 days
			DateTime expiryDate = DateTime.UtcNow.Date.AddDays(30);
			string testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":001:0003:" + expiryDate.ToString("yyyyMMdd"));
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual(LicenseProductNameEnum.SpiraTest, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.Demonstration, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(3, Common.License.Number);
			Assert.AreEqual(expiryDate.Date, Common.License.Expiration.Value.Date);

			//Lets test a SpiraPlan v1.0 5-user demonstration License that expires in 60 days
			expiryDate = DateTime.UtcNow.Date.AddDays(60);
			testLicenceKey = Common.License.Encrypt("OnShore Technology Group:ValidationMaster:" + LICENSE_VERSION_TO_TEST + ":001:0005:" + expiryDate.ToString("yyyyMMdd"));
			Common.License.Clear();
			Common.License.Validate("OnShore Technology Group", testLicenceKey);
			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//Assert.AreEqual(LicenseProductNameEnum.SpiraPlan, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseProductNameEnum.ValidationMaster, Common.License.LicenseProductName);
			Assert.AreEqual(LicenseTypeEnum.Demonstration, Common.License.LicenseType);
			Assert.AreEqual("OnShore Technology Group", Common.License.Organization);
			Assert.AreEqual("" + LICENSE_VERSION_TO_TEST + "", Common.License.Version);
			Assert.AreEqual(5, Common.License.Number);
			Assert.AreEqual(expiryDate.Date, Common.License.Expiration.Value.Date);
		}

		/// <summary>
		/// Lets test that we can retrieve the list of possible product license types
		/// </summary>
		[
		Test,
		SpiraTestCase(380)
		]
		public void _04_RetrieveProductTypes()
		{
			List<ProductType> productTypes = Common.License.RetrieveProductTypes();
			Assert.AreEqual(1, productTypes.Count);
			Assert.AreEqual("ValidationMaster", productTypes[0].Name);

		}
	}
}
