using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extended properties of the UserProfile entity
    /// </summary>
    public partial class UserProfile
    {
        /// <summary>
        /// Returns the full name of the user
        /// </summary>
        public string FullName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(MiddleInitial))
                {
                    return FirstName + " " + LastName;
                }
                else
                {
                    return FirstName + " " + MiddleInitial + " " + LastName;
                }
            }
        }
    }
}
