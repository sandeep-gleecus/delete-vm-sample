using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a comment associated with an artifact
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class CommentItem
    {
        /// <summary>
        /// The id of the comment
        /// </summary>
        [DataMember]
        public int primaryKey { get; set; }

        /// <summary>
        /// The actual comment text (may include markup)
        /// </summary>
        [DataMember]
        public string text { get; set; }

        /// <summary>
        /// The id of the user that created the comment
        /// </summary>
        [DataMember]
        public int creatorId { get; set; }

        /// <summary>
        /// The name of the user that created the comment
        /// </summary>
        [DataMember]
        public string creatorName { get; set; }

        /// <summary>
        /// The date/time that the comment was created (in local timezone)
        /// </summary>
        [DataMember]
        public DateTime creationDate { get; set; }

        /// <summary>
        /// The display text date/time that the comment was created (in local timezone)
        /// </summary>
        [DataMember]
        public string creationDateText { get; set; }

        /// <summary>
        /// The sort direction of the entire list
        /// </summary>
        [DataMember]
        public int sortDirection { get; set; }

        /// <summary>
        /// Can the current user delete this comment
        /// </summary>
        [DataMember]
        public bool deleteable { get; set; }

        /// <summary>
        /// Has this comment not yet been read
        /// </summary>
        [DataMember]
        public bool? isUnread { get; set; }
    }
}