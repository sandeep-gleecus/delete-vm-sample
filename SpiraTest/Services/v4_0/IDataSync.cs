using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.v4_0.DataObjects;
using Inflectra.SpiraTest.Web.Services.Wsdl.Documentation;

#pragma warning disable 1591

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
    [
    ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v4.0/", SessionMode = SessionMode.Allowed),
    XmlComments
    ]
    public interface IDataSync : IService
    {
        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        List<RemoteDataSyncSystem> DataSyncSystem_Retrieve();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunFailure(int dataSyncSystemId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunSuccess(int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunWarning(int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_WriteEvent(string message, string details, int eventLogEntryType);
    }
}
