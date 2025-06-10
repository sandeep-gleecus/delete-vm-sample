using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a document/attachment type in the system
    /// </summary>
    public class RemoteDocumentType
    {
        /// <summary>
        /// The id of the document type
        /// </summary>
        public int? DocumentTypeId;

        /// <summary>
        /// The id of the project template that the type belongs to
        /// </summary>
        public int ProjectTemplateId;

        /// <summary>
        /// The name of the document type.
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the document type.
        /// </summary>
        /// <remarks>
        /// Optional
        /// </remarks>
        public string Description;

        /// <summary>
        /// Is this type active
        /// </summary>
        public bool Active;

        /// <summary>
        /// Is this the default type for the project
        /// </summary>
        public bool Default;
    }
}