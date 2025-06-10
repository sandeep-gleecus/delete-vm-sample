using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
    /// <summary>
    /// This is structure is used to define the custom soap fault message.
    /// Custom SOAP Fault Message class Name :ServiceFaultMessage 
    /// </summary>
    [DataContract]
    public struct ServiceFaultMessage
    {
        /// <summary>
        /// The short description of the fault
        /// </summary>
        [DataMember]
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// The type of the fault - normally the full name of the server exception class
        /// </summary>
        [DataMember]
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// The detailed stack trace of the error message
        /// </summary>
        [DataMember]
        public string StackTrace
        {
            get;
            set;
        }
    }
}
