using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Contains information on unread messages and list of online users
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class MessageInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageInfo()
        {
        }

        /// <summary>
        /// The number of unread messages
        /// </summary>
        [DataMember]
        public int? UnreadMessages
        {
            get;
            set;
        }

        /// <summary>
        /// The number of online users
        /// </summary>
        [DataMember]
        public List<int> OnlineUsers
        {
            get;
            set;
        }
    }
}