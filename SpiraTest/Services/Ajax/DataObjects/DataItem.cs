using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using System.Runtime.Serialization;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents one row in the returned JSON serialized resultset
    /// </summary>
    /// <remarks>
    /// The first row returned always contains the filter values rather than actual data values, and
    /// is always returned even if we have no data. That way we know which columns to display!
    /// </remarks>
    [
    DataContract(Namespace = "tst.dataObjects"),
    KnownType(typeof(HierarchicalDataItem)),
    KnownType(typeof(SortedDataItem)),
    KnownType(typeof(OrderedDataItem))
    ]
    public class DataItem
    {
        [DataMember(Name="primaryKey")]
        public int PrimaryKey = -1;

        [DataMember(Name = "attachment")]
        public bool Attachment = false;

        [DataMember(Name = "alternate")]
        public bool Alternate = false;

        [DataMember(Name = "concurrencyValue", EmitDefaultValue=false)]
        public string ConcurrencyValue = null;

        [DataMember(Name = "customUrl", EmitDefaultValue = false)]
        public string CustomUrl = null;   //If not specified, uses the grid's base URL

        [DataMember(Name = "childCount", EmitDefaultValue = false)]
        public int? ChildCount = null;

        [DataMember(Name = "readOnly", EmitDefaultValue = false)]
        public bool? ReadOnly = null;

        [DataMember(Name = "notSelectable", EmitDefaultValue = false)]
        public bool? NotSelectable = null;

        protected JsonDictionaryOfFields fields;

        /// <summary>
        /// Constructor method
        /// </summary>
        public DataItem()
        {
            fields = new JsonDictionaryOfFields();
        }

        /// <summary>
        /// Creates a copy of shape of the current item (field definitions only, with no values)
        /// </summary>
        /// <returns>A shell dataitem that is based off the current one</returns>
        public DataItem Clone()
        {
            DataItem clonedItem = new DataItem();
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

        /// <summary>
        /// Gets the collection of fields indexed off the field name
        /// </summary>
        [DataMember(Name="Fields")]
        public JsonDictionaryOfFields Fields
        {
            get
            {
                return this.fields;
            }
            set
            {
                this.fields = value;
            }
        }
    }

    /// <summary>
    /// Represents one row field in the returned dataset
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class DataItemField
    {
        [DataMember(Name = "fieldType")]
        public DataModel.Artifact.ArtifactFieldTypeEnum FieldType;

        [DataMember(Name = "fieldName")]
        public string FieldName = "";

        [DataMember(Name = "lookupName", EmitDefaultValue = false)]
        public string LookupName = "";

        [DataMember(Name = "caption", EmitDefaultValue = false)]
        public string Caption = null;

        [DataMember(Name = "textValue", EmitDefaultValue = false)]
        public string TextValue = null;

        [DataMember(Name = "intValue", EmitDefaultValue = false)]
        public int? IntValue = null;

        [DataMember(Name = "dateValue", EmitDefaultValue = false)]
        public DateTime? DateValue = null;

        [DataMember(Name = "allowDragAndDrop", EmitDefaultValue = false)]
        public bool? AllowDragAndDrop = false;

        [DataMember(Name = "editable", EmitDefaultValue = false)]
        public bool? Editable = false;

        [DataMember(Name = "required", EmitDefaultValue = false)]
        public bool? Required = false;

        [DataMember(Name = "hidden", EmitDefaultValue = false)]
        public bool? Hidden = false;

        [DataMember(Name = "tooltip", EmitDefaultValue = false)]
        public string Tooltip = null;

        [DataMember(Name = "cssClass", EmitDefaultValue = false)]
        public string CssClass = null;

        [DataMember(Name = "lookups", EmitDefaultValue = false)]
        public JsonDictionaryOfStrings Lookups = null;

        [DataMember(Name = "equalizerGreen", EmitDefaultValue = false)]
        public int? EqualizerGreen = null;

        [DataMember(Name = "equalizerRed", EmitDefaultValue = false)]
        public int? EqualizerRed = null;

        [DataMember(Name = "equalizerOrange", EmitDefaultValue = false)]
        public int? EqualizerOrange = null;

        [DataMember(Name = "equalizerYellow", EmitDefaultValue = false)]
        public int? EqualizerYellow = null;

        [DataMember(Name = "equalizerGray", EmitDefaultValue = false)]
        public int? EqualizerGray = null;

        [DataMember(Name = "notSortable", EmitDefaultValue = false)]
        public bool? NotSortable = null;

        [DataMember(Name = "width", EmitDefaultValue = false)]
        public int? Width = null;
    }
}
