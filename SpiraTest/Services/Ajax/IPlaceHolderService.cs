using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides access to new placeholder ids for attaching things to artifacts (e.g. incidents) before they are saved
    /// </summary>
    [
    ServiceContract(Name = "PlaceHolderService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    public interface IPlaceHolderService
    {
        [OperationContract]
        int PlaceHolder_GetNextId(int projectId);
    }
}
