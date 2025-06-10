using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents the information needed to post a new message to a single recipient
    /// </summary>
    public class RemoteMessageIndividual
    {
        /// <summary>
        /// The id of the recipient user
        /// </summary>
        public int RecipientUserId;

        /// <summary>
        /// The contents of the message
        /// </summary>
        public string MessageText;
    }
}