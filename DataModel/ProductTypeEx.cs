using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extended properties to the ProductType entity
    /// </summary>
    public partial class ProductType
    {
        /// <summary>
        /// Maps to the ActiveYn field - converts the Y/N field to/from the boolean
        /// </summary>
        public bool Active
        {
            get
            {
                return (this._activeYn == "Y");
            }
            set
            {
                this._activeYn = (value) ? "Y" : "N";
            }
        }
    }
}
