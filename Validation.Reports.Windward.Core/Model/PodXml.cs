using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.Reports.Windward.Core.Model
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("user-defined-tags", Namespace = "", IsNullable = false)]
    public partial class PodXml
    {

        private userdefinedtagsVariable[] variablesField;

        private userdefinedtagsDatasource[] datasourcesField;

        private object itemsField;

        private decimal versionField;

        private string vendorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("variable", IsNullable = false)]
        public userdefinedtagsVariable[] variables
        {
            get
            {
                return this.variablesField;
            }
            set
            {
                this.variablesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("datasource", IsNullable = false)]
        public userdefinedtagsDatasource[] datasources
        {
            get
            {
                return this.datasourcesField;
            }
            set
            {
                this.datasourcesField = value;
            }
        }

        /// <remarks/>
        public object items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string vendor
        {
            get
            {
                return this.vendorField;
            }
            set
            {
                this.vendorField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class userdefinedtagsVariable
    {

        private userdefinedtagsVariableDefaultvalues defaultvaluesField;

        private string nameField;

        private bool requiredField;

        private string typeField;

        private string guidField;

        private string descriptionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("default-values")]
        public userdefinedtagsVariableDefaultvalues defaultvalues
        {
            get
            {
                return this.defaultvaluesField;
            }
            set
            {
                this.defaultvaluesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool required
        {
            get
            {
                return this.requiredField;
            }
            set
            {
                this.requiredField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string guid
        {
            get
            {
                return this.guidField;
            }
            set
            {
                this.guidField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class userdefinedtagsVariableDefaultvalues
    {

        private userdefinedtagsVariableDefaultvaluesValue valueField;

        /// <remarks/>
        public userdefinedtagsVariableDefaultvaluesValue value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class userdefinedtagsVariableDefaultvaluesValue
    {

        private string sourceField;

        private string defaultField;

        private string defaulttypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @default
        {
            get
            {
                return this.defaultField;
            }
            set
            {
                this.defaultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("default-type")]
        public string defaulttype
        {
            get
            {
                return this.defaulttypeField;
            }
            set
            {
                this.defaulttypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class userdefinedtagsDatasource
    {

        private string nameField;

        private string guidField;

        private string fulltypeField;

        private string providerclassField;

        private string providernameField;

        private byte tableownerField;

        private bool tableownerFieldSpecified;

        private string providerField;

        private string connectionstringField;

        private string connectionStringField;

        private bool hasCredsField;

        private bool hasCredsFieldSpecified;

        private string usernameField;

        private string passwordField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string guid
        {
            get
            {
                return this.guidField;
            }
            set
            {
                this.guidField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("full-type")]
        public string fulltype
        {
            get
            {
                return this.fulltypeField;
            }
            set
            {
                this.fulltypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("provider-class")]
        public string providerclass
        {
            get
            {
                return this.providerclassField;
            }
            set
            {
                this.providerclassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("provider-name")]
        public string providername
        {
            get
            {
                return this.providernameField;
            }
            set
            {
                this.providernameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("table-owner")]
        public byte tableowner
        {
            get
            {
                return this.tableownerField;
            }
            set
            {
                this.tableownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tableownerSpecified
        {
            get
            {
                return this.tableownerFieldSpecified;
            }
            set
            {
                this.tableownerFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("connection-string")]
        public string connectionstring
        {
            get
            {
                return this.connectionstringField;
            }
            set
            {
                this.connectionstringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string connectionString
        {
            get
            {
                return this.connectionStringField;
            }
            set
            {
                this.connectionStringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool hasCreds
        {
            get
            {
                return this.hasCredsField;
            }
            set
            {
                this.hasCredsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hasCredsSpecified
        {
            get
            {
                return this.hasCredsFieldSpecified;
            }
            set
            {
                this.hasCredsFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string username
        {
            get
            {
                return this.usernameField;
            }
            set
            {
                this.usernameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }
    }
}
