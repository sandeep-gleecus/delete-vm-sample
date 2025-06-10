using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single Test Configuration Set artifact in the system
    /// </summary>
    public class RemoteTestConfigurationSet
    {
        /// <summary>
        /// The id of the test configuration set
        /// </summary>
        public int TestConfigurationSetId;

        /// <summary>
        /// Is the item active
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// The date/time this item was created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date/time this item was updated
        /// </summary>
        public DateTime LastUpdatedDate;

        /// <summary>
        /// The date-time field used to maintain concurrency
        /// </summary>
        public DateTime ConcurrencyDate;
        
        /// <summary>
        /// The ID of the project
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The name of the Test Configuration Set
        /// </summary>
        public string Name;

        /// <summary>
        /// The long description of the Test Configuration Set
        /// </summary>
        public string Description;

        /// <summary>
        /// The list of test configuration entries in the set
        /// </summary>
        public List<RemoteTestConfigurationEntry> Entries;
    }

    /// <summary>
    /// Represents one test configuration entry
    /// </summary>
    public class RemoteTestConfigurationEntry
    {
        /// <summary>
        /// The id of the entry
        /// </summary>
        public int TestConfigurationEntryId;

        /// <summary>
        /// The list of test case parameter name/values provided by the entry
        /// </summary>
        public List<RemoteTestConfigurationParameterValue> ParameterValues;
    }

        /// <summary>
    /// Represents a test set parameter (used when you have a test set pass parameters values to all of the containes test cases)
    /// </summary>
    public class RemoteTestConfigurationParameterValue
    {
        /// <summary>
        /// The id of the test case parameter
        /// </summary>
        public int TestCaseParameterId;

        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the parameter to be passed from the test configuratiin entry to the test cases
        /// </summary>
        public string Value;
    }

}