using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents one key value pair
    /// </summary>
    [
    DataContract(Namespace = "tst.dataObjects"),
    ]
    public class KeyValuePair
    {
        [DataMember(EmitDefaultValue = false, Name = "key")]
        public string Key;

        [DataMember(EmitDefaultValue = false, Name = "value")]
        public string Value;
    }
}