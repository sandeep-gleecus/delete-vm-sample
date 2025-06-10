using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents one row in the returned JSON serialized resultset for hierarchical data
    /// </summary>
    /// <remarks>
    /// The first row returned always contains the filter values rather than actual data values, and
    /// is always returned even if we have no data. That way we know which columns to display!
    /// We use a custom namespace to reduce the serialization overhead of a long namespace!!
    /// </remarks>
    [DataContract(Namespace="tst.dataObjects")]
    public class HierarchicalDataItem : DataItem
    {
        [DataMember(Name = "summary", EmitDefaultValue = false)]
        public bool Summary = false;

        [DataMember(Name = "expanded", EmitDefaultValue = false)]
        public bool Expanded = false;

        [DataMember(Name = "indent", EmitDefaultValue = false)]
        public string Indent = null;

        /// <summary>
        /// Creates a copy of shape of the current item (field definitions only, with no values)
        /// </summary>
        /// <returns>A shell dataitem that is based off the current one</returns>
        public new HierarchicalDataItem Clone()
        {
            HierarchicalDataItem clonedItem = new HierarchicalDataItem();
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
