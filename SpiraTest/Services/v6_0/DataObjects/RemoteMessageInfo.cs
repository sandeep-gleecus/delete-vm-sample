using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Contains information on unread messages and list of online users
    /// </summary>
    public class RemoteMessageInfo
    {
        /// <summary>
        /// The total number of unread messages
        /// </summary>
        public int? UnreadMessages;

        /// <summary>
        /// The ids of online users
        /// </summary>
        public List<int> OnlineUsers;
    }
}