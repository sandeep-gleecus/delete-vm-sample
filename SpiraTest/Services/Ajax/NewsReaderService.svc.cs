using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.Xml.Linq;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class NewsReaderService : INewsReaderService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.NewsReaderService::";

        /// <summary>
        /// Retrieves an RSS/ATOM newsfeed for a specific URL
        /// </summary>
        /// <param name="rowCount">The number of records to retrieve</param>
        /// <param name="startingIndex">The starting index (0-based)</param>
        /// <param name="url">The URL for the newsfeed</param>
        /// <returns>A list of newsfeed items</returns>
        public List<NewsItem> RetrieveFeed(string url, int startingIndex, int rowCount)
        {
            const string METHOD_NAME = "RetrieveFeed";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			//Convert into a list of serializable objects
			List<NewsItem> newsItems = new List<NewsItem>();

			try
            {
                XmlReader xmlReader = System.Xml.XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(xmlReader);

                
                int i = 0;
                foreach (SyndicationItem item in feed.Items)
                {
                    //See if this item is in the pagination range
                    if (i >= startingIndex && i < (startingIndex + rowCount))
                    {
                        //Resolve the url
                        string itemUrl = "";
                        foreach (SyndicationLink link in item.Links)
                        {
                            if (link.RelationshipType == "alternate" || String.IsNullOrEmpty(link.RelationshipType))
                            {
                                itemUrl = link.Uri.ToString();
                            }
                        }

                        //See if this is a real uri
                        Uri uri;
                        if (!Uri.TryCreate(itemUrl, UriKind.Absolute, out uri))
                        {
                            //Set the URL to empty
                            itemUrl = "";
                        }

                        //Make sure the headline and summary are plain text, to avoid XSS attacks
                        string safeHeadline = GlobalFunctions.HtmlRenderAsPlainText(item.Title.Text);
                        string safeSummary = GlobalFunctions.HtmlRenderAsPlainText(item.Summary.Text);

                        //Resolve the publish date and localize for display
                        DateTime pubDate = GlobalFunctions.LocalizeDate(item.PublishDate.UtcDateTime);

                        NewsItem newsItem = new NewsItem();
                        newsItem.Url = itemUrl;
                        newsItem.Headline = safeHeadline;
                        newsItem.Description = safeSummary;
                        newsItem.PublishDate = pubDate.ToNiceString(DateTime.UtcNow, "d");

                        //Resolve the author name
                        if (item.Authors.Count > 0)
                        {
                            string author = item.Authors[0].Name;
                            if (String.IsNullOrEmpty(author))
                            {
                                author = item.Authors[0].Email;
                            }
                            newsItem.Author = author;
                        }
                        //Resolve the category
                        if (item.Categories.Count > 0)
                        {
                            string safeCategory = GlobalFunctions.HtmlRenderAsPlainText(item.Categories[0].Name);
                            newsItem.Category = safeCategory;
                        }
                        
                        newsItems.Add(newsItem);
                    }
                    i++;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
               
            }
            catch (System.Net.WebException exception)
            {
                //The RSS/ATOM feed is not available
                //Log the real error and display a 'pretty one'
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                //throw new DataValidationException(Resources.Messages.NewsReaderService_FeedNotAccessible); //PCS
            }
            catch (UriFormatException exception)
            {
                //The RSS/ATOM URL is incorrect
                //Log the real error and display a 'pretty one'
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw new DataValidationException(Resources.Messages.NewsReaderService_FeedUrlIncorrect);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                //throw;
            } 
			
			return newsItems;
        }
    }
}
