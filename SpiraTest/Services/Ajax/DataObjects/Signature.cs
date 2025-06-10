using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

using Inflectra.SpiraTest.Business;
using System.Runtime.Serialization;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a digital signature passed to a function
    /// </summary>
    [
    DataContract(Namespace = "tst.signature"),
    ]
    public class Signature
    {
        [DataMember(Name = "login", EmitDefaultValue = false)]
        public string Login = null;

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password = null;

        [DataMember(Name = "meaning", EmitDefaultValue = false)]
        public string Meaning = null;
    }
}