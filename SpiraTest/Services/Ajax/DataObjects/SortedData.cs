using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Stores a set of sorted data items retrieves together with a list of other meta-information
    /// including total counts, pagination information and filter names
    /// </summary>
    /// <remarks>
    /// We use a custom namespace to reduce the serialization overhead of a long namespace!!
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class SortedData
    {
        [DataMember(Name = "startRow")]
        public int StartRow = 0;

        [DataMember(Name = "sortProperty")]
        public string SortProperty = null;

        [DataMember(Name = "sortAscending")]
        public bool SortAscending = true;

        ///Constructor
        public SortedData()
        {
            //Create the items
            this.Items = new List<SortedDataItem>();
            this.PaginationOptions = null;
            this.FilterNames = null;
        }

        /// <summary>
        /// The list of actual data items
        /// </summary>
        [DataMember(Name="items")]
        public List<SortedDataItem> Items;

        /// <summary>
        /// The pagination information
        /// </summary>
        [DataMember(Name = "paginationOptions", EmitDefaultValue = false)]
        public JsonDictionaryOfStrings PaginationOptions;

        /// <summary>
        /// The list of filter names
        /// </summary>
        [DataMember(Name = "filterNames", EmitDefaultValue = false)]
        public List<string> FilterNames;

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
        /// The visible count of items
        /// </summary>
        [DataMember(Name = "visibleCount")]
        public int VisibleCount = 0;

        /// <summary>
        /// The total count of items
        /// </summary>
        [DataMember(Name = "totalCount")]
        public int TotalCount = 0;
    }
}