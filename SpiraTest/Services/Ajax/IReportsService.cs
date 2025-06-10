using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used in the Reporting module to display the navigation bar
    /// </summary>
    [
    ServiceContract(Name = "ReportsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IReportsService : INavigationService
    {
        [OperationContract]
        string Reports_RetrieveCustomQueryData(int projectId, string sql);

        [OperationContract]
        string Reports_RetrieveCustomQueryTemplate(int projectId, string sql);

        [OperationContract]
        string Reports_RetrieveSectionDefaultTemplate(int reportSectionId);

        [OperationContract]
        string Reports_RetrieveSectionDescription(int reportSectionId);
    }
}
