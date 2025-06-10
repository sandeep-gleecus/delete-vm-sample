using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a test set parameter (used when you have a test set pass parameters values to all of the containes test cases)
    /// </summary>
    public class RemoteTestSetParameter
    {
        /// <summary>
        /// The id of the test set
        /// </summary>
        public int TestSetId;

        /// <summary>
        /// The id of the test case parameter
        /// </summary>
        public int TestCaseParameterId;

        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the parameter to be passed from the test set to the test cases
        /// </summary>
        public string Value;
    }
}