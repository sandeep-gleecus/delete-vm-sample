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
    /// Used for making Ajax calls that display program data
    /// </summary>
    [
    ServiceContract(Name = "ProgramService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IProgramService : IWorkspaceService
    {
        [OperationContract]
        WorkspaceData Program_RetrieveBuilds(int projectGroupId, int rowsToDisplay);
    }
}
