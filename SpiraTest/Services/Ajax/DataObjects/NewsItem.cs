using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a syndicated news item
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class NewsItem
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NewsItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsItem(string url, string headline, string description, DateTime publishDate, string author = "", string category = "")
        {
            this.Url = url;
            this.Headline = headline;
            this.Description = description;
            this.PublishDate = publishDate.ToNiceString(DateTime.UtcNow, "d");
            this.Author = author;
            this.Category = category;
        }

        //The url of the news item
        [DataMember(Name="url", EmitDefaultValue = false)]
        public string Url
        {
            get;
            set;
        }

        //The headline of the news item
        [DataMember(Name = "headline", EmitDefaultValue = false)]
        public string Headline
        {
            get;
            set;
        }

        //The description of the news item
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }

        //The date of the news item in display format
        [DataMember(Name = "publishDate", EmitDefaultValue = false)]
        public string PublishDate
        {
            get;
            set;
        }

        //The author of the news item in display format
        [DataMember(Name = "author", EmitDefaultValue = false)]
        public string Author
        {
            get;
            set;
        }

        //The category of the news item
        [DataMember(Name = "category", EmitDefaultValue = false)]
        public string Category
        {
            get;
            set;
        }
    }
}