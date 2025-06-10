using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// The base class for all discussion entities
    /// </summary>
    public class DiscussionBase : Entity
    {
        #region Lookup Properties

        /// <summary>
        /// Returns the Creator's Full Name
        /// </summary>
        public string CreatorName
        {
            get
            {
                if (this["Creator"]  == null)
                {
                    return null;
                }
                User user = (User)this["Creator"];
                if (user.Profile == null)
                {
                    return null;
                }
                return user.Profile.FullName;
            }
        }

        #endregion
    }
}
