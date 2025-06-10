using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single Project Template in the system
    /// </summary>
    public class RemoteProjectTemplate
    {
        /// <summary>
        /// The id of the project template
        /// </summary>
        public int? ProjectTemplateId;

        /// <summary>
        /// The name of the project template
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the project template
        /// </summary>
        public string Description;

        /// <summary>
        /// Is this template active or not
        /// </summary>
        public bool IsActive;
    }
}