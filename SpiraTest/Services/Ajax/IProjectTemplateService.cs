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
    /// Provides the web service used to interacting with the various client-side project template AJAX components
    /// </summary>
    [
    ServiceContract(Name = "ProjectTemplateService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IProjectTemplateService
    {
        [OperationContract]
        List<FieldMapping> RetrieveStandardFieldMappingInformation(int projectId, int newTemplateId);
    }
}
