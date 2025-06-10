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
    /// Used for making Ajax calls that display portfolio data
    /// </summary>
    [
    ServiceContract(Name = "PortfolioService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IPortfolioService : IWorkspaceService
    {
        [OperationContract]
        WorkspaceData Portfolio_RetrieveBuilds(int portfolioId, int rowsToDisplay);
    }
}
