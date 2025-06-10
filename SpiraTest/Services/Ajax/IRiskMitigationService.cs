using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of mitigations in a risk
    /// </summary>
    [
    ServiceContract(Name = "RiskMitigationService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRiskMitigationService: IOrderedListService
    {
    }
}
