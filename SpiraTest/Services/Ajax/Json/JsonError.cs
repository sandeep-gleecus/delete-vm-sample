using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Json
{
    [DataContractFormat]
    public class JsonError
    {
        [DataMember]
        public string ExceptionType { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StackTrace { get; set; }

        [DataMember]
        public int FaultCode { get; set; }
    } 
}