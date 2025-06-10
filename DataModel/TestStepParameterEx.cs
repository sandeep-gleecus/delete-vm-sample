using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the test step parameter entity
    /// </summary>
    public partial class TestStepParameter : Entity
    {
        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name
        {
            get
            {
                if (this.Parameter != null)
                {
                    return this.Parameter.Name;
                }
                return "";
            }
        }
    }
}
