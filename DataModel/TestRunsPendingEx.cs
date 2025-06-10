using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the TestRunsPending
    /// </summary>
    public partial class TestRunsPending : Entity
    {
        /// <summary>
        /// Returns True if this is a resumed test run pending (dates no longer match)
        /// </summary>
        public bool IsResumed
        {
            get
            {
                return this.LastUpdateDate > this.CreationDate;
            }
        }
    }
}
