using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single message in the system (both individual and group messages)
    /// </summary>
    public class RemoteMessage
    {
        /// <summary>
        /// The id of the message
        /// </summary>
        public long MessageId;

        /// <summary>
        /// The sender of the message
        /// </summary>
        public RemoteUser SenderUser;

        /// <summary>
        /// The recipient (null for a group message)
        /// </summary>
        public RemoteUser RecipientUser;

        /// <summary>
        /// The date/time this message was created
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The date/time this message was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The contents of the message
        /// </summary>
        public string Body;

        /// <summary>
        /// Has this message been read
        /// </summary>
        public bool IsRead;

        /// <summary>
        /// The artifact the message was sent to, populated for group messages
        /// </summary>
        public RemoteLinkedArtifact RecipientArtifact;
    }
}