using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents one row in the returned JSON serialized resultset for non-hierarchical user-ordered data
    /// </summary>
    /// <remarks>
    /// The first item contains some global information such as pagination and could be used to display filters
    /// if that gets added to the OrderedGrid in the future.
    /// </remarks>
    [DataContract(Namespace = "tst.dataObjects")]
    public class OrderedDataItem : DataItem
    {
        [DataMember(Name="position", EmitDefaultValue=false)]
        public int? Position = null;

        [DataMember(Name = "alternateKey", EmitDefaultValue = false)]
        public int? AlternateKey = null;

        [DataMember(Name = "allowScript", EmitDefaultValue = false)]
        public bool? AllowScript = null;

        /// <summary>
        /// Creates a copy of shape of the current item (field definitions only, with no values)
        /// </summary>
        /// <returns>A shell dataitem that is based off the current one</returns>
        public new OrderedDataItem Clone()
        {
            OrderedDataItem clonedItem = new OrderedDataItem();
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
