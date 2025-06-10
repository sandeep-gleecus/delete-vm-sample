using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single Component in a SpiraTeam project
    /// </summary>
    public class RemoteComponent
    {
        /// <summary>
        /// The id of the Component (id - integer)
        /// </summary>
        public int? ComponentId;

        /// <summary>
        /// The id of the project the component belongs to (id - integer)
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The name of the Component (string)
        /// </summary>
        public String Name;

        /// <summary>
        /// Is the component active (boolean)
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// Is the component deleted (boolean)
        /// </summary>
        public bool IsDeleted = false;
    }
}