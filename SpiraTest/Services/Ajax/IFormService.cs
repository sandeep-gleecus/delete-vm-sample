using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// All AJAX web services that are called by the AjaxFormManager need to implement this interface
    /// </summary>
    [ServiceContract(Name = "IFormService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface IFormService
    {
        [OperationContract]
        DataItem Form_Retrieve(int projectId, int? artifactId);

        [OperationContract]
        List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature);

        [OperationContract]
        List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId);

        [OperationContract]
        int? Form_Delete(int projectId, int artifactId);

        [OperationContract]
        int? Form_Clone(int projectId, int artifactId);

        [OperationContract]
        int? Form_New(int projectId, int artifactId);
    }
}
