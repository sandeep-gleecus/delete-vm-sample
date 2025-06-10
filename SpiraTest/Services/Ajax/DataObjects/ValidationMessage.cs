using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>Represents a single validation message returned from a form or grid</summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class ValidationMessage
    {
        /// <summary>The name of the field that failed validation</summary>
        [DataMember]
        public string FieldName
        {
            get;
            set;
        }

        /// <summary>The name of the validation message</summary>
        [DataMember]
        public string Message
        {
            get;
            set;
        }
    }
}