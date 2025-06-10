using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [ServiceContract(Name = "SearchService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface ISearchService
    {
        [OperationContract]
        SearchResults RetrieveResults(string keyword, int pageIndex, int pageSize);
    }
}
