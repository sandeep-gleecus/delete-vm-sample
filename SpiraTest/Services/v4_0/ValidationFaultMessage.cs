using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
    /// <summary>
    /// This is structure is used to define the custom soap fault message.
    /// Custom SOAP Fault Message class Name :ServiceFaultMessage 
    /// </summary>
    [DataContract]
    public struct ValidationFaultMessage
    {
        /// <summary>
        /// The short description of the fault
        /// </summary>
        [DataMember]
        public string Summary
        {
            get;
            set;
        }

        /// <summary>
        /// The list of actual validation messages
        /// </summary>
        [DataMember]
        public List<ValidationFaultMessageItem> Messages
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a single validation message item
    /// </summary>
    [DataContract]
    public struct ValidationFaultMessageItem
    {
        [DataMember]
        public string FieldName
        {
            get;
            set;
        }

        [DataMember]
        public string Message
        {
            get;
            set;
        }
    }
}
