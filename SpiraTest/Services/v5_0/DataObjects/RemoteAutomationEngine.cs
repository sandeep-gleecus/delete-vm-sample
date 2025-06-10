using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single Automation Engine in the system
    /// </summary>
    public class RemoteAutomationEngine
    {
        /// <summary>
        /// The id of the engine
        /// </summary>
        public Nullable<int> AutomationEngineId;

        /// <summary>
        /// The display name of the engine
        /// </summary>
        public String Name;

        /// <summary>
        /// The token of the engine
        /// </summary>
        /// <remarks>Ths is the name that external systems should refer to it as</remarks>
        public String Token;

        /// <summary>
        /// The detailed description of the engine
        /// </summary>
        /// <remarks>
        /// Optional
        /// </remarks>
        public String Description;

        /// <summary>
        /// Is this host active for the project
        /// </summary>
        public bool Active;
    }
}