using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a manual test run in the system
    /// </summary>
    public class RemoteManualTestRun : RemoteTestRun
    {
        /// <summary>
        /// The list of test steps that comprise the manual test
        /// </summary>
        public List<RemoteTestRunStep> TestRunSteps;
    }
}