using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Stores a set of data items retrieves together with a list of other meta-information used by the planning data
    /// </summary>
    /// <remarks>
    /// We use a custom namespace to reduce the serialization overhead of a long namespace!!
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class PlanningData
    {
        ///Constructor
        public PlanningData()
        {
            //Create the items
            this.Items = new List<PlanningDataItem>();
        }

        /// <summary>
        /// The list of actual data items
        /// </summary>
        [DataMember(Name = "items")]
        public List<PlanningDataItem> Items;
 
        /// <summary>
        /// The page count
        /// </summary>
        [DataMember(Name = "pageCount")]
        public int PageCount = 0;

        /// <summary>
        /// The index of the current page
        /// </summary>
        [DataMember(Name = "currPage")]
        public int CurrPage = 0;

        /// <summary>
        /// The image for the artifact types being returned
        /// </summary>
        [DataMember(Name = "artifactImage")]
        public string ArtifactImage = null;

        /// <summary>
        /// The alternate image for the artifact types being returned
        /// </summary>
        [DataMember(Name = "alternateImage")]
        public string AlternateImage = null;
    }
}