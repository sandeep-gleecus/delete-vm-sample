using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a simple entry in a single-series graph
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class GraphEntry
    {
        /// <summary>
        /// The physical name of the entry
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// The display name of the caption
        /// </summary>
        [DataMember(Name = "caption", EmitDefaultValue = false)]
        public string Caption { get; set; }

        /// <summary>
        /// The color of the entry
        /// </summary>
        [DataMember(Name = "color", EmitDefaultValue = false)]
        public string Color { get; set; }

        /// <summary>
        /// The count for the interval
        /// </summary>
        [DataMember(Name = "count")]
        public int Count { get; set; }
    }
}