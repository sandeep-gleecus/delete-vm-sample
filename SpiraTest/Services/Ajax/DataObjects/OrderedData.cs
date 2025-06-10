using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Stores a set of ordered data items retrieves together with a list of other meta-information
    /// including total counts, pagination information, etc.
    /// </summary>
    /// <remarks>
    /// We use a custom namespace to reduce the serialization overhead of a long namespace!!
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class OrderedData
    {
        ///Constructor
        public OrderedData()
        {
            //Create the items
            this.Items = new List<OrderedDataItem>();
            this.PaginationOptions = null;
        }

        /// <summary>
        /// The list of actual data items
        /// </summary>
        [DataMember(Name="items")]
        public List<OrderedDataItem> Items;

        /// <summary>
        /// The pagination information
        /// </summary>
        [DataMember(Name = "paginationOptions", EmitDefaultValue = false)]
        public JsonDictionaryOfStrings PaginationOptions;

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