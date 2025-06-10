using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation
{
    /// <summary>
    /// Contains a single REST parameter, used for parameterizing the web service calls
    /// </summary>
    public class RestParameter
    {
        private bool isModified = true;
        public bool IsModified()
        {
            return isModified;
        }
        public void SetModified(bool val)
        {
            isModified = val;
        }

        private string name;
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; SetModified(true); }
        }

        /// <summary>
        /// Returns the token name for the parameter (used in URLs)
        /// </summary>
        public string TokenName
        {
            get
            {
                return "{" + Name + "}";
            }
        }

        private string _value;
        /// <summary>
        /// The value of parameter
        /// </summary
        public string Value
        {
            get { return _value; }
            set { _value = value; SetModified(true); }
        }
    }
}
