using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

namespace Inflectra.SpiraTest.TestSuite
{
    /// <summary>This text fixture tests the PlaceholderManager business class</summary>
    [TestFixture]
    [SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
    public class PlaceholderManagerTest
    {
        protected static PlaceholderManager placeholderManager;

        private const int PROJECT_ID = 1;

		[TestFixtureSetUp]
		public void Init()
		{
            placeholderManager = new PlaceholderManager();
		}

		/// <summary>
		/// Tests that we can create a new placeholder artifact in the system. This is used to associate Attachments to
        /// until such time that we save the actual incident, at which point the attachments get transfered to the incident itself
		/// </summary>
        [
        Test,
        SpiraTestCase(1177)
        ]
        public void _01_CreateNewPlaceholder()
        {
            //Create the new placeholder
            int placeholderId = placeholderManager.Placeholder_Create(PROJECT_ID).PlaceholderId;
            Assert.IsTrue(placeholderId > 0);

            //Verify that it was created successfully
            Placeholder placeholder = placeholderManager.Placeholder_RetrieveById(placeholderId);
            Assert.IsNotNull(placeholder);
            Assert.AreEqual(placeholderId, placeholder.PlaceholderId);
        }
    }
}
