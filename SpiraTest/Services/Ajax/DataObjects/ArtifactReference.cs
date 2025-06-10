using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Contains a single artifact reference
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class ArtifactReference
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArtifactReference()
        {
        }

        /// <summary>
        /// The id of the project (optional)
        /// </summary>
        [DataMember(Name = "projectId", EmitDefaultValue = false)]
        public int? ProjectId
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the artifact
        /// </summary>
        [DataMember(Name = "artifactId")]
        public int ArtifactId
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the type of the artifact
        /// </summary>
        [DataMember(Name = "artifactTypeId")]
        public int ArtifactTypeId
        {
            get;
            set;
        }
    }
}