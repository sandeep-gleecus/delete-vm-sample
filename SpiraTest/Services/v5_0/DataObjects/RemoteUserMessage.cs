using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// The number of messages for a specific user
    /// </summary>
    public class RemoteUserMessage
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        public int UserId;

        /// <summary>
        /// The number of unread messages for this user
        /// </summary>
        public int UnreadMessages;
    }
}