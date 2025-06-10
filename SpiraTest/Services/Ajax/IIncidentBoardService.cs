using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Implemented by the service that provides data to the PlanningBoard control on the Incident Board
    /// </summary>
    [ServiceContract(Name = "IncidentBoardService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    public interface IIncidentBoardService : IPlanningBoardService
    {
    }
}
