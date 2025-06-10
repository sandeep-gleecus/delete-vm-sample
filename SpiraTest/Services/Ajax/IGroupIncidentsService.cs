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
    /// Communicates with the SortableGrid AJAX component for displaying/updating incidents data in a program
    /// </summary>
    [
    ServiceContract(Name = "GroupIncidentsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IGroupIncidentsService: ISortedListService
    {
    }
}
