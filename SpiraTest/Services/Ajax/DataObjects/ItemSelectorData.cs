using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Minimal Data Result for the ItemSelector interface
    /// </summary>
    /// <remarks>
    /// We use a custom namespace to reduce the serialization overhead of a long namespace!!
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class ItemSelectorData
    {
        ///Constructor
        public ItemSelectorData()
        {
            //Create the items
            this.Items = new List<DataItem>();
        }

        /// <summary>
        /// The list of actual data items
        /// </summary>
        [DataMember(Name="items")]
        public List<DataItem> Items;
    }
}