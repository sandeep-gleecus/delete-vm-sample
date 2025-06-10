using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a simple hierarchical Item
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class HierarchicalItem
    {
        /// <summary>
        /// The id of the item
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the item
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The indent level of the item
        /// </summary>
        [DataMember(Name = "indentLevel", EmitDefaultValue = false)]
        public string IndentLevel { get; set; }

        /// <summary>
        /// The parent id of the item
        /// </summary>
        [DataMember(Name = "parentId", EmitDefaultValue = false)]
        public int? ParentId { get; set; }

        /// <summary>
        /// The project id of the item
        /// </summary>
        [DataMember(Name = "projectId", EmitDefaultValue = false)]
        public int? ProjectId { get; set; }

        /// <summary>
        /// The project name of the item
        /// </summary>
        [DataMember(Name = "projectName", EmitDefaultValue = false)]
        public string ProjectName { get; set; }

        /// <summary>
        /// The artifact id of the item
        /// </summary>
        [DataMember(Name = "artifactTypeId", EmitDefaultValue = false)]
        public int? ArtifactTypeId { get; set; }

        /// <summary>
        /// The artifact sub typ id of the item, if present
        /// </summary>
        [DataMember(Name = "artifactSubTypeId", EmitDefaultValue = false)]
        public int? ArtifactSubTypeId { get; set; }

        /// <summary>
        /// Whether the artifact is the alternate type - ie use case or iteration, if present
        /// </summary>
        [DataMember(Name = "artifactSubType", EmitDefaultValue = false)]
        public string ArtifactSubType { get; set; }
    }
}