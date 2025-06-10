using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a simple name/value pair
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class NameValue
    {
        /// <summary>
        /// The id of the item
        /// </summary>
        [DataMember(Name="id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the item
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// The list of any child ids
        /// </summary>
        [DataMember(Name = "childIds", EmitDefaultValue = false)]
        public List<int> ChildIds { get; set; }
    }
}