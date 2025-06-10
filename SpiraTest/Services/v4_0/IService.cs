using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Wsdl.Documentation;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
    [
    ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v4.0/", SessionMode = SessionMode.Allowed),
    XmlComments(XmlCommentFormat.Default, "Inflectra.SpiraTest.Web.Services.v4_0.ServiceBase")
    ]
    public interface IService
    {
        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        bool Connection_Authenticate(string userName, string password);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        bool Connection_Authenticate2(string userName, string password, string plugInName);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        bool Connection_ConnectToProject(int projectId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void Connection_Disconnect();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        string System_GetProductName();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        DateTime System_GetServerDateTime();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        string System_GetWebServerUrl();
    }
}
