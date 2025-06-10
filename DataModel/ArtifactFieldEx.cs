using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the ArtifactField entity
    /// </summary>
    public partial class ArtifactField : Entity
    {
        public ArtifactField()
        {
        }

        public ArtifactField(int artifactFieldId, int artifactFieldTypeId, int artifactTypeId, string name, string caption, bool isWorkflowConfig, bool isActive, bool isListConfig, bool isListDefault, int listDefaultPosition, bool isDataMapping, bool isReport, bool isNotify, string description)
            : base()
        {
            this.ArtifactFieldId = artifactFieldId;
            this.ArtifactFieldTypeId = artifactFieldTypeId;
            this.ArtifactTypeId = artifactTypeId;
            this.Name = name;
            this.Caption = caption;
            this.IsWorkflowConfig = isWorkflowConfig;
            this.IsActive = isActive;
            this.IsListConfig = isListConfig;
            this.ListDefaultPosition = listDefaultPosition;
            this.IsDataMapping = isDataMapping;
            this.IsReport = isReport;
            this.IsNotify = isNotify;
            this.Description = description;
        }
    }
}
