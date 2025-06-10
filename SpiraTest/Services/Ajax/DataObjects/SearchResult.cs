using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// The list of search results together with any high-level meta-information
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class SearchResults
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResults()
        {
            //Instantiate the list of values
            this.Values = new List<SearchResult>();
        }

        /// <summary>
        /// The total count of matching results
        /// </summary>
        [DataMember]
        public int Count { get; set; }

        /// <summary>
        /// The list of actual search results
        /// </summary>
        [DataMember]
        public List<SearchResult> Values { get; set; }
    }
    /// <summary>
    /// A single search result returned to the SearchResults AJAX server control
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class SearchResult
    {
        /// <summary>
        /// The ArtifactTypeID the artifact (prefix & ID)
        /// </summary>
        [DataMember]
        public int ArtifactTypeId { get; set; }

        /// <summary>
        /// The token of the artifact (prefix & ID)
        /// </summary>
        [DataMember]
        public string Token { get; set; }

        /// <summary>
        /// The image of the icon to display
        /// </summary>
        [DataMember]
        public string Icon { get; set; }

        /// <summary>
        /// The ALT text for the image of the icon
        /// </summary>
        [DataMember]
        public string IconAlt { get; set; }

        /// <summary>
        /// The URL to get the individual search result
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// The title of the search result
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// The long description of the search result
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The last updated date in a 'pretty' display format
        /// </summary>
        [DataMember]
        public string LastUpdateDate { get; set; }

        /// <summary>
        /// The project name
        /// </summary>
        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// The project id
        /// </summary>
        [DataMember]
        public int ProjectId { get; set; }

        /// <summary>
        /// The ranked result
        /// </summary>
        [DataMember]
        public int Rank { get; set; }
    }
}