using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation
{
    /// <summary>
    /// A single HTTP request header
    /// </summary>
    public class RestHeader
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
        /// The name of the header
        /// </summary>
        public string Name
        {
            get{return name;}
            set{name = value; SetModified(true); }
        }

        private string _value;
        /// <summary>
        /// The value of header
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; SetModified(true); }
        }
    }
}
