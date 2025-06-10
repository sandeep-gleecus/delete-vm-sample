using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.TestSuite
{
    /// <summary>
    /// This fixture tests the base methods of the Dashboard business object
    /// </summary>
    [
    TestFixture,
    SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
    ]
    public class DashboardTest
    {
        protected static Business.DashboardManager dashboardManager;

        private const string DASHBOARD_TEST_PATH = "TestDashboard/1";

        private const string USER_NAME_FRED_BLOGGS = "fredbloggs";
        /// <summary>
        /// Initializes the business objects being tested
        /// </summary>
        [TestFixtureSetUp]
        public void Init()
        {
            dashboardManager = new Business.DashboardManager();
        }

		/// <summary>
        /// Tests that we can retrieve, store and reset the global dashboard settings
		/// </summary>
        [
        Test,
        SpiraTestCase(544)
        ]
        public void _01_RetrieveAndStoreGlobalSettings()
        {
            //First verify that we don't have any global data for the path
            byte[] blob1 = dashboardManager.RetrieveGlobalSettings(DASHBOARD_TEST_PATH);
            Assert.IsNull (blob1);

            //Now store some data
            byte[] blob2 = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            dashboardManager.SaveGlobalSettings(DASHBOARD_TEST_PATH, blob2);

            //Verify that it stored correctly
            byte[] blob3 = dashboardManager.RetrieveGlobalSettings(DASHBOARD_TEST_PATH);
            Assert.AreEqual(blob2.ToString(), blob3.ToString());

            //Now delete the data
            dashboardManager.DeleteGlobalSettings(DASHBOARD_TEST_PATH);
            
            //Verify that it deleted successfully
            byte[] blob4 = dashboardManager.RetrieveGlobalSettings(DASHBOARD_TEST_PATH);
            Assert.IsNull(blob4);
        }

        /// <summary>
        /// Tests that we can retrieve, store and reset the user-specific dashboard settings
        /// </summary>
        [
        Test,
        SpiraTestCase(546)
        ]
        public void _02_RetrieveAndStoreUserSettings()
        {
            //First verify that we don't have any user data for the path
            byte[] blob1 = dashboardManager.RetrieveUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS);
            Assert.IsNull(blob1);

            //Now store some data
            byte[] blob2 = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            dashboardManager.SaveUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS, blob2);

            //Verify that it stored correctly
            byte[] blob3 = dashboardManager.RetrieveUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS);
            Assert.AreEqual(blob2.ToString(), blob3.ToString());

            //Now delete the data
            dashboardManager.DeleteUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS);

            //Verify that it deleted successfully
            byte[] blob4 = dashboardManager.RetrieveUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS);
            Assert.IsNull(blob4);
        }

        [Test]
        public void _XX_CleanUp()
        {
            //Reset the global and per-user settings
            dashboardManager.DeleteGlobalSettings(DASHBOARD_TEST_PATH);
            dashboardManager.DeleteUserSettings(DASHBOARD_TEST_PATH, USER_NAME_FRED_BLOGGS);
        }
    }
}
