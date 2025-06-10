using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the test set parameter entity
    /// </summary>
    public partial class TestSetParameter : Entity
    {
        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name
        {
            get
            {
                if (this.TestCaseParameter != null)
                {
                    return this.TestCaseParameter.Name;
                }
                return "";
            }
        }
    }
}
