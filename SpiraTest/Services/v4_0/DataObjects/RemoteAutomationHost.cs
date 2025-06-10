using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single Automation Host artifact in the system
    /// </summary>
    public class RemoteAutomationHost : RemoteArtifact
    {
        /// <summary>
        /// The id of the host
        /// </summary>
        public Nullable<int> AutomationHostId;

        /// <summary>
        /// The name of the host
        /// </summary>
        public String Name;

        /// <summary>
        /// The token of the host
        /// </summary>
        /// <remarks>Ths is the name that external systems should refer to it as</remarks>
        public String Token;

        /// <summary>
        /// The detailed description of the host
        /// </summary>
        /// <remarks>
        /// Optional
        /// </remarks>
        public String Description;

        /// <summary>
        /// The date/time that the host was last modified
        /// </summary>
        /// <remarks>
        /// This field needs to match the values retrieved to ensure data-concurrency
        /// </remarks>
        public DateTime LastUpdateDate;

        /// <summary>
        /// Is this host active for the project
        /// </summary>
        public bool Active;

        /// <summary>
        /// The last date/time that this host was contacted
        /// </summary>
        public DateTime? LastContactDate;
    }
}