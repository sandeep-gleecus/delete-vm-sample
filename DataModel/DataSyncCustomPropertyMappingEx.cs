using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the data sync custom property
    /// </summary>
    public partial class DataSyncCustomPropertyMapping : Entity
    {
        public string CustomPropertyName
        {
            get
            {
                if (this.CustomProperty == null)
                {
                    return "";
                }
                else
                {
                    return CustomProperty.FIELD_PREPEND + this.CustomProperty.PropertyNumber;
                }
            }
        }
    }
}
