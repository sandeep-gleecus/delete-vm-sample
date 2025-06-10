using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Implemented by the service that provides data to the PlanningBoard control on the Task Board
    /// </summary>
    [ServiceContract(Name = "TaskBoardService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    public interface ITaskBoardService : IPlanningBoardService
    {
    }
}
