using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the test set test case parameter entity
    /// </summary>
    public partial class TestSetTestCaseParameter : Entity
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
