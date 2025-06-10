using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Syndication;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Ajax web service that returns a list of RSS news items
    /// </summary>
    [ServiceContract(Name = "NewsReaderService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    public interface INewsReaderService
    {
        [OperationContract]
        List<NewsItem> RetrieveFeed(string url, int startingIndex, int rowCount);
    }
}
