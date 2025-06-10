//*************************************
//** TagCloud
//*************************************
//** Designer: Kamran Ayub
//** Website: http://www.intrepidstudios.com/projects/SearchCloud/
//** Credits: Reciprocity (http://reciprocity.be/ctc/) - PHP tag cloud for colors and font size calculations
//** - 
//****************
//** Inputs:
//****************
// ATTRIBUTES
//** - DataSource        : Required      Specifies dataset to use for parsing Keyword Data
//** - DataIDField       : Required      Specifies field name for Keyword ID
//** - DataKeywordField  : Required      Specifies field name for Keyword Title
//** - DataURLField      : Optional      Specifies field name for Keyword URL (if any)
//** - DataCountField    : Required      Specifies field name for Keyword Count
//** - SortBy            : Optional      Sort by a field name, optional DESC (e.g. keyword_title DESC)
//** - MaxFontSize       : Optional      Specifies Max Font Size (Default: 22)
//** - MinFontSize       : Optional      Specifies Min Font Size (Default: 10)
//** - FontUnit          : Optional      Specifies Font Unit (em,px,pt,%) (Default:pt)
//** - MaxColor          : Optional      Specifies Max Color in Hex
//** - MinColor          : Optional      Specifies Min Color in Hex
//** - KeywordTitleFormat : Optional     Specifies a Custom Keyword Title Format
//** - KeywordURLFormat  : Optional      Specifies a Custom Keyword URL Format
//** - CssClass          : Optional      Specifies a CSS Class for the containing Div
//** - Debug             : Optional      Will throw actual exception instead of friendly error

// METHODS
//** - AddAttribute(attr, value)     : Optional      Add a custom HTML attribute like 'onclick' to the keyword link

//*****************
//** Outputs:
//*****************
//** - CloudHTML     :   The output HTML. Will output an error if there has been one.

//***************
//** Notes
//***************
// You can use the following variables in the KeywordTitleFormat and AddAttribute method
// %k = Keyword Title
// %c = Keyword Count
// %u = Keyword URL
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.ServerControls
{

    [ToolboxData("<{0}:TagCloud runat=server></{0}:TagCloud>")]
    public class TagCloud : WebControl
    {

        #region " Properties "

        // Apperance Properties
        #region " Appearance "
        [Bindable(true), Category("Appearance"), Localizable(true)]
        public int MinFontSize
        {
            get
            {
                if (Convert.ToInt32(ViewState["MinFontSize"]) == 0)
                {
                    return 10;
                }
                else
                {
                    return Convert.ToInt32(ViewState["MinFontSize"]);
                }

            }

            set { ViewState["MinFontSize"] = value; }
        }
        [Bindable(true), Category("Appearance"), Localizable(true)]
        public int MaxFontSize
        {
            get
            {
                if (Convert.ToInt32(ViewState["MaxFontSize"]) == 0)
                {
                    return 22;
                }
                else
                {
                    return Convert.ToInt32(ViewState["MaxFontSize"]);
                }

            }

            set { ViewState["MaxFontSize"] = value; }
        }
        [Bindable(false), Category("Appearance"), Localizable(true)]
        public string FontUnit
        {
            get
            {
                string s = (string)ViewState["FontUnit"];
                if (String.IsNullOrEmpty(s))
                {
                    return "pt";
                }
                else
                {
                    return s;
                }

            }

            set
            {
                switch (value)
                {
                    case "pt":
                        ViewState["FontUnit"] = value;
                        break;
                    case "em":
                        ViewState["FontUnit"] = value;
                        break;
                    case "%":
                        ViewState["FontUnit"] = value;
                        break;
                    case "px":
                        ViewState["FontUnit"] = value;
                        break;
                    default:
                        ViewState["FontUnit"] = "px";
                        break;
                }

            }
        }
        [Bindable(true), Category("Appearance"), Localizable(true)]
        public string MaxColor
        {
            get
            {
                string s = Convert.ToString(ViewState["MaxColor"]);

                if (s == string.Empty)
                {
                    return "#00f";
                }
                else
                {
                    return s;
                }
            }

            set { ViewState["MaxColor"] = value; }
        }
        [Bindable(true), Category("Appearance"), Localizable(true)]
        public string MinColor
        {
            get
            {
                string s = Convert.ToString(ViewState["MinColor"]);

                if (s == string.Empty)
                {
                    return "#000";
                }
                else
                {
                    return s;
                }
            }

            set { ViewState["MinColor"] = value; }
        }
        #endregion

        // Data Properties
        #region " Data "

        /// <summary>
        /// The datasource
        /// </summary>
        /// <remarks>Not stored in ViewState so need to load each time</remarks>
        [Bindable(true), Category("Data"), DefaultValue("")]
        public IList DataSource
        {
            get;
            set;
        }
        [Bindable(true), Category("Data"), DefaultValue(""), Localizable(true)]
        public string DataIDField
        {
            get { return Convert.ToString(ViewState["DataIDField"]); }

            set { ViewState["DataIDField"] = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue(""), Localizable(true)]
        public string DataMember
        {
            get { return Convert.ToString(ViewState["DataMember"]); }

            set { ViewState["DataMember"] = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue(""), Localizable(true)]
        public string DataKeywordField
        {
            get { return Convert.ToString(ViewState["DataKeywordField"]); }

            set { ViewState["DataKeywordField"] = value; }
        }
        [Bindable(true), Category("Data"), Localizable(true)]
        public string DataURLField
        {
            get
            {
                string s = Convert.ToString(ViewState["DataURLField"]);
                return s;
            }

            set { ViewState["DataURLField"] = value; }
        }
        [Bindable(true), Category("Data"), DefaultValue(""), Localizable(true)]
        public string DataCountField
        {
            get { return Convert.ToString(ViewState["DataCountField"]); }

            set { ViewState["DataCountField"] = value; }
        }
        [Bindable(true), Category("Data"), Localizable(true)]
        public string KeywordTitleFormat
        {
            get
            {
                if (Convert.ToString(ViewState["KeywordTitleFormat"]) == string.Empty)
                {
                    return Resources.ServerControls.TagCloud_DefaultFormat;
                }
                else
                {
                    return Convert.ToString(ViewState["KeywordTitleFormat"]);
                }

            }

            set { ViewState["KeywordTitleFormat"] = value; }
        }
        [Bindable(true), Category("Data"), Localizable(true)]
        public string KeywordURLFormat
        {
            get { return Convert.ToString(ViewState["KeywordURLFormat"]); }

            set { ViewState["KeywordURLFormat"] = value; }
        }
        [Bindable(true), Category("Data"), DefaultValue(""), Localizable(true)]
        public string SortBy
        {
            get { return Convert.ToString(ViewState["SortBy"]); }

            set { ViewState["SortBy"] = value; }
        }
        #endregion

        [Bindable(false), Category("Debug"), DefaultValue(false), Localizable(true)]
        public bool Debug
        {
            get
            {
                bool s = Convert.ToBoolean(ViewState["Debug"]);

                return s;
            }

            set { ViewState["Debug"] = value; }
        }

        //****************
        //** Private Properties
        //****************

        private Dictionary<string,string> arrAttributes;
        private string CloudHTML
        {
            get { return Convert.ToString(ViewState["CloudHTML"]); }
            set { ViewState["CloudHTML"] = value; }
        }
        private Dictionary<string,string> KeyAttributes
        {
            get { return arrAttributes; }
            set { arrAttributes = value; }
        }

        #endregion

        // ** Render Control

        // Will output something like:
        // <div [attributes]>
        // <a href="url" title="title">keyword</a>
        // [...]
        // </div>

        protected override void Render(HtmlTextWriter output)
        {
            if (CloudHTML != string.Empty)
            {
                output.WriteBeginTag("div");

                // Write attributes, if any
                if (!(CssClass == string.Empty))
                {
                    output.WriteAttribute("class", CssClass);
                }
                output.Write(">");
                // Close div

                output.Write(CloudHTML);

                output.WriteEndTag("div");

            }
            else
            {
                output.Write("There was no generated HTML. An unhandled error must have occurred.");
            }

        }

        //********************
        //** Main Functions **
        //********************

        protected override void OnLoad(System.EventArgs e)
        {
            // Validate Fields
            if (DataSource == null)
            {
                CloudHTML = "Please specify a DataSource to read from";
                return;
            }
            if (DataKeywordField == string.Empty)
            {
                CloudHTML = "Please specify a Keyword Data Field";
                return;
            }
            if (DataCountField == string.Empty)
            {
                CloudHTML = "Please specify a Keyword Count Field";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(MinColor, "^#([a-f]|[A-F]|[0-9]){3}(([a-f]|[A-F]|[0-9]){3})?$"))
            {
                CloudHTML = "MinColor must be a HEX code and must have leading #. Example: #000 or #ff99cc";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(MaxColor, "^#([a-f]|[A-F]|[0-9]){3}(([a-f]|[A-F]|[0-9]){3})?$"))
            {
                CloudHTML = "MaxColor must be a HEX code and must have leading #. Example: #000 or #ff99cc";
                return;
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                IList dataSource = DataSource;

                /* Sort keywords in descending format */

                // Create an instance of the GenericSorter class passing in the data item type.
                Type dataSourceType = dataSource.GetType();
                Type dataItemType = dataSourceType.GetGenericArguments()[0];
                Type sorterType = typeof(GenericSorter<>).MakeGenericType(dataItemType);
                var sorterObject = Activator.CreateInstance(sorterType);

                //Now actually perform the sort
                string sortExpression = DataCountField;
                bool sortAscending = false;
                IList sortedList = (IList)sorterType.GetMethod("Sort", new Type[] { dataSourceType, typeof(string), typeof(bool) })
                    .Invoke(sorterObject, new object[] { dataSource, sortExpression, sortAscending });

                // Row count
                int Count = sortedList.Count;

                if (Count == 0)
                {
                    CloudHTML = Resources.ServerControls.TagCloud_NoValues;
                    return;
                }

                // Get Largest and Smallest Count Values
                int MaxQty = (int)DataBinder.Eval(sortedList[0], DataCountField);
                int MinQty = (int)DataBinder.Eval(sortedList[Count - 1], DataCountField);

                // Find range of values
                int Spread = MaxQty - MinQty;

                // We don't want to divide by zero
                if (Spread == 0)
                {
                    Spread = 1;
                }

                // Determine font size increment
                int FontSpread = MaxFontSize - MinFontSize;
                if (FontSpread == 0)
                {
                    FontSpread = 1;
                }

                double FontStep = FontSpread / Spread;


                // Sort alphabetically
                IList sortedList2;
                if (SortBy != string.Empty)
                {
                    //Now actually perform the sort
                    sortExpression = SortBy;
                    sortAscending = true;
                }
                else
                {
                    //Now actually perform the sort
                    sortExpression = DataKeywordField;
                    sortAscending = true;
                }
                sortedList2 = (IList)sorterType.GetMethod("Sort", new Type[] { dataSourceType, typeof(string), typeof(bool) })
                    .Invoke(sorterObject, new object[] { dataSource, sortExpression, sortAscending });


                // DEBUG: Max Min Avg
                //sb.Append(String.Format("Max: {0}, Min: {1}, Spread: {2}, MaxColor: {3}, MinColor: {4}, MaxFontSize: {5}, MinFontSize: {6}<br />", MaxQty, MinQty, Spread, MaxColor, MinColor, MaxFontSize, MinFontSize))
                foreach (object dataItem in sortedList2)
                {
                    int sKeyID = -1;
                    if (!String.IsNullOrEmpty(DataIDField))
                    {
                        sKeyID = (int)DataBinder.Eval(dataItem, DataIDField);
                    }
                    string sKeyWord = (string)DataBinder.Eval(dataItem, DataKeywordField);
                    int sKeyCount = (int)DataBinder.Eval(dataItem, DataCountField);
                    string sKeyURL = null;
                    double Weight = MinFontSize + ((sKeyCount - MinQty) * FontStep);
                    int FontDiff = MaxFontSize - MinFontSize;
                    double ColorWeight = Math.Round(99 * (Weight - MinFontSize) / (FontDiff) + 1);

                    // Handle URL
                    if (DataURLField == string.Empty)
                    {
                        if (KeywordURLFormat != string.Empty)
                        {
                            sKeyURL = ReplaceKeyValues(KeywordURLFormat, sKeyID, sKeyWord, "", sKeyCount);
                        }
                        else
                        {
                            sKeyURL = "#";
                        }

                    }
                    else
                    {
                        sKeyURL = (string)DataBinder.Eval(dataItem, DataURLField);
                    }
                    sb.Append(string.Format("<a href=\"{0}\" style=\"font-size:{1}{4};\" title=\"{2}\"{5}>{3}</a> \n", sKeyURL, Math.Round(Weight), ReplaceKeyValues(KeywordTitleFormat, sKeyID, sKeyWord, sKeyURL, sKeyCount), HttpContext.Current.Server.HtmlEncode(sKeyWord), FontUnit, GenerateAttributes(sKeyWord, sKeyID, sKeyURL, sKeyCount)));

                }

                CloudHTML = sb.ToString();
            }
            catch (Exception ex)
            {
                if (!Debug)
                {
                    CloudHTML = Resources.ServerControls.TagCloud_Error;
                }
                else
                {
                    Logger.LogErrorEvent("TagCloud", ex);

                    throw ex;
                }
            }
            finally
            {
                base.OnLoad(e);
            }

        }

        // Add custom HTML attributes
        public void AddAttribute(string value, string text)
        {
            if (KeyAttributes == null)
            {
                KeyAttributes = new Dictionary<string,string>();
            }

            KeyAttributes.Add(value, text);
        }

        private string GenerateAttributes(string k, int id, string u, int c)
        {

            if (KeyAttributes == null)
            {
                return string.Empty;
            }

            StringBuilder s = new StringBuilder();
            ICollection keys = KeyAttributes.Keys;

            foreach (string key in keys)
            {
                s.Append(string.Format(" {0}=\"{1}\"", key, ReplaceKeyValues(KeyAttributes[key], id, k, u, c)));
            }

            return s.ToString();
        }

        // Replace %keyvalues with proper value
        public string ReplaceKeyValues(string txt, int id, string k, string u, int c)
        {
            // In case using k in javascript
            k = k.Replace("'", "&apos;");

            txt = txt.Replace("%i", id.ToString());
            txt = txt.Replace("%k", HttpContext.Current.Server.HtmlEncode(k));
            txt = txt.Replace("%u", u);
            txt = txt.Replace("%c", c.ToString());

            return txt;
        }

        // Generate Color Based on Weight
        // Thanks to Reciprocity for a structure for this code
        private string Colorize(string minc, string maxc, double w)
        {
            w = w / 100;
            string rs = null;
            string gs = null;
            string bs = null;
            string r = null;
            string g = null;
            string b = null;
            int minr = 0;
            int ming = 0;
            int minb = 0;
            int maxr = 0;
            int maxg = 0;
            int maxb = 0;

            // Make #000 into #000000

            if (minc.Length == 4)
            {
                rs = minc.Substring(1, 1);
                gs = minc.Substring(2, 1);
                bs = minc.Substring(3, 1);

                minc = "#" + rs + rs + gs + gs + bs + bs;
            }

            if (maxc.Length == 4)
            {
                rs = maxc.Substring(1, 1);
                gs = maxc.Substring(2, 1);
                bs = maxc.Substring(3, 1);

                maxc = "#" + rs + rs + gs + gs + bs + bs;
            }

            minr = Int32.Parse(minc.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            ming = Int32.Parse(minc.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            minb = Int32.Parse(minc.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            maxr = Int32.Parse(maxc.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            maxg = Int32.Parse(maxc.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            maxb = Int32.Parse(maxc.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            r = ((int)(Math.Round(((maxr - minr) * w) + minr))).ToString("x");
            g = ((int)(Math.Round(((maxg - ming) * w) + ming))).ToString("x");
            b = ((int)(Math.Round(((maxb - minb) * w) + minb))).ToString("x");

            if (r.Length == 1)
                r = "0" + r;
            if (g.Length == 1)
                g = "0" + g;
            if (b.Length == 1)
                b = "0" + b;

            string color = null;
            color = "#" + r + g + b;

            return color;
        }
    }

}
