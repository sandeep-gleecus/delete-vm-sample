using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;


namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortedGrid AJAX component for displaying/updating requirements data in a sortable list format
    /// </summary>
    [
    ServiceContract(Name = "RequirementsListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRequirementsListService : ISortedListService
    {
    }
}
