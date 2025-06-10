using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a test set test case parameter (used when you have a test set pass parameters through to mapped test cases)
    /// </summary>
    public class RemoteTestSetTestCaseParameter
    {
        /// <summary>
        /// The id of the test set test case
        /// </summary>
        public int TestSetTestCaseId;

        /// <summary>
        /// The id of the test case parameter
        /// </summary>
        public int TestCaseParameterId;

        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the parameter to be passed from the test set to the test case
        /// </summary>
        public string Value;
    }
}