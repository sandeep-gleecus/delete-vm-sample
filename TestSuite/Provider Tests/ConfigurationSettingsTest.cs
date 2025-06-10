using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

namespace Inflectra.SpiraTest.TestSuite.Provider_Tests
{
    /// <summary>
    /// Tests the ConfigurationSettings Provider
    /// </summary>
    [
    TestFixture,
    SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
    ]
    public class ConfigurationSettingsTest
    {
        [TestFixtureSetUp]
        public void Init()
        {
            //Do Nothing
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
            //Cleans up the secure settings
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_GLOBAL_SETTING_SECURE");
        }

        /// <summary>
        /// Tests that we can retrieve and save settings to the database using our custom provider
        /// </summary>
        [
        Test,
        SpiraTestCase(802)
        ]
        public void _01_RetrieveSaveSettings()
        {
            //Lets get the default values and verify
            //We do one for each data type
            ConfigurationSettings.Default.Reload();
           // Assert.AreEqual("gallifrey.corp.inflectra.com", ConfigurationSettings.Default.EmailSettings_MailServer);
            Assert.AreEqual(25, ConfigurationSettings.Default.EmailSettings_MailServerPort);
            Assert.AreEqual(false, ConfigurationSettings.Default.EmailSettings_UseSSL);

            //Make some changes and save
            ConfigurationSettings.Default.EmailSettings_MailServer = "mail.mycompany.com";
            ConfigurationSettings.Default.EmailSettings_MailServerPort = 585;
            ConfigurationSettings.Default.EmailSettings_UseSSL = true;
            ConfigurationSettings.Default.Save();

            //Verify
            ConfigurationSettings.Default.Reload();
            Assert.AreEqual("mail.mycompany.com", ConfigurationSettings.Default.EmailSettings_MailServer);
            Assert.AreEqual(585, ConfigurationSettings.Default.EmailSettings_MailServerPort);
            Assert.AreEqual(true, ConfigurationSettings.Default.EmailSettings_UseSSL);

            //Put the data back
            ConfigurationSettings.Default.EmailSettings_MailServer = "gallifrey.corp.inflectra.com";
            ConfigurationSettings.Default.EmailSettings_MailServerPort = 25;
            ConfigurationSettings.Default.EmailSettings_UseSSL = false;
            ConfigurationSettings.Default.Save();

            //Verify
            ConfigurationSettings.Default.Reload();
            Assert.AreEqual("gallifrey.corp.inflectra.com", ConfigurationSettings.Default.EmailSettings_MailServer);
            Assert.AreEqual(25, ConfigurationSettings.Default.EmailSettings_MailServerPort);
            Assert.AreEqual(false, ConfigurationSettings.Default.EmailSettings_UseSSL);
        }

        /// <summary>
        /// Tests that we can save encrypted settings for things such as passwords, etc.
        /// </summary>
        [Test]
        [SpiraTestCase(1377)]
        public void _02_RetrieveSaveEncryptedSettings()
        {
            //First lets verify the initial values of a secure setting that will be in clear
            Assert.AreEqual("ClearValue", SecureSettings.Default.TestSecureSetting);

            //Now update the database to set it manually as clear.
            InternalRoutines.ExecuteNonQuery("INSERT INTO TST_GLOBAL_SETTING_SECURE (NAME, VALUE, IS_ENCRYPTED) VALUES ('TestSecureSetting', 'ClearValue2', 0)");

            //Verify it can still be read
            SecureSettings.Default.Reload();
            Assert.AreEqual("ClearValue2", SecureSettings.Default.TestSecureSetting);

            //Now update the value (should be encrypted)
            SecureSettings.Default.TestSecureSetting = "SecureValue";
            SecureSettings.Default.Save();

            //Verify
            SecureSettings.Default.Reload();
            Assert.AreEqual("SecureValue", SecureSettings.Default.TestSecureSetting);

            //Verify the encrypted value
            string encryptedValue = (string)InternalRoutines.ExecuteScalar("SELECT VALUE FROM TST_GLOBAL_SETTING_SECURE WHERE NAME = 'TestSecureSetting'");
            Assert.AreEqual("214188230217243223023023075139236080020148156077", encryptedValue);
        }
    }
}
