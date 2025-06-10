using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side artifact association AJAX components
    /// </summary>
    [
    ServiceContract(Name = "BackgroundProcessService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    public interface IBackgroundProcessService
    {
        [OperationContract]
        string LaunchNewProcess(Nullable<int> projectId, string operation, Nullable<int> parameter1, List<int> parameter2, string parameter3);

        [OperationContract]
        ProcessStatus GetProcessStatus(string processId);
    }
}
