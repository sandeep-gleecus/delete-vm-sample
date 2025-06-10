using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used for making Ajax calls that display project data
    /// </summary>
    [
    ServiceContract(Name = "ProjectService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IProjectService : IWorkspaceService
    {
    }
}
