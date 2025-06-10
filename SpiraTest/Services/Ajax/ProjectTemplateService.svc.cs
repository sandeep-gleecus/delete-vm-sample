using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class ProjectTemplateService : AjaxWebServiceBase, IProjectTemplateService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ProjectTemplateService::";

        /// <summary>
        /// Retrieves the list of artifacts and fields that could be affected by a change of project template.
        /// Displays the number of artifacts affected for each field/artifact type combination
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="newTemplateId"></param>
        /// <returns>The artifact type, field name, and number of affected items</returns>
        public List<FieldMapping> RetrieveStandardFieldMappingInformation(int projectId, int newTemplateId)
        {
            const string METHOD_NAME = "RetrieveStandardFieldMappingInformation";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the business object
                TemplateManager templateManager = new TemplateManager();
                List<TemplateRemapStandardFieldsInfo> standardFields = templateManager.RetrieveStandardFieldMappingInformation(projectId, newTemplateId);

                List<FieldMapping> fieldMappings = new List<FieldMapping>();
                foreach (TemplateRemapStandardFieldsInfo standardField in standardFields)
                {
                    // only send data that is going to be affected by changing template
                    if (standardField.AffectedItemsCount > 0)
                    {
                        FieldMapping fieldItem = new FieldMapping()
                        {
                            artifactType = standardField.ArtifactType,
                            artifactField = standardField.ArtifactField,
                            affectedCount = standardField.AffectedItemsCount
                        };
                        fieldMappings.Add(fieldItem);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return fieldMappings;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }

    /// <summary>Simple JSON object to send to client</summary>
    public class FieldMapping
    {
        /// <summary>The name of the artifact type - eg "Requirement".</summary>
        public string artifactType { get; set; }

        /// <summary>The name of the artifact field - eg "Requirement Type"</summary>
        public string artifactField { get; set; }

        /// <summary>The number of field values that will be changed for this specific field</summary>
        public int? affectedCount { get; set; }
    }
}
