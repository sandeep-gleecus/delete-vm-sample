using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents one row in the returned JSON serialized resultset for sorted list data
    /// </summary>
    /// <remarks>
    /// The first row returned always contains the filter values rather than actual data values, and
    /// is always returned even if we have no data. That way we know which columns to display!
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class SortedDataItem : DataItem
    {
        [DataMember(Name = "folder", EmitDefaultValue = false)]
        public bool Folder = false;

        /// <summary>
        /// Creates a copy of shape of the current item (field definitions only, with no values)
        /// </summary>
        /// <returns>A shell dataitem that is based off the current one</returns>
        public new SortedDataItem Clone()
        {
            SortedDataItem clonedItem = new SortedDataItem();
            //Copy across just the field definitions
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in this.fields)
            {
                DataItemField clonedField = new DataItemField();
                DataItemField dataItemField = dataItemFieldKVP.Value;
                clonedField.FieldName = dataItemField.FieldName;
                clonedField.LookupName = dataItemField.LookupName;
                clonedField.Caption = dataItemField.Caption;
                clonedField.FieldType = dataItemField.FieldType;
                clonedField.AllowDragAndDrop = dataItemField.AllowDragAndDrop;
                clonedItem.Fields.Add(clonedField.FieldName, clonedField);
            }
            return clonedItem;
        }
    }
}
