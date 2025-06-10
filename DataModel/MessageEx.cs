using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Message entity
    /// </summary>
    public partial class Message : Entity
    {
        #region Lookup Properties

        /// <summary>
        /// Returns the Sender's full name
        /// </summary>
        public string SenderName
        {
            get
            {
                if (Sender != null && Sender.Profile != null)
                {
                    return Sender.Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the Recipient's full name
        /// </summary>
        public string RecipientName
        {
            get
            {
                if (Recipient != null && Recipient.Profile != null)
                {
                    return Recipient.Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
