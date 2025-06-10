using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Implemented by the service that provides data to the Project Group PlanningBoard control
    /// </summary>
    [ServiceContract(Name = "GroupPlanningBoardService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    public interface IGroupPlanningBoardService : IPlanningBoardService
    {
    }
}
