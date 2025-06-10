using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in datalist to include ajax loading of tooltips
    /// </summary>
    [ToolboxData("<{0}:DataListEx runat=server></{0}:DataListEx>")]
    public class DataListEx : DataList, IScriptControl
    {
        protected object cachedSelectedValue = null;

        #region Properties

        [
        Category("Data"),
        DefaultValue(null),
        Description("Limits how many rows of data are databound in the list, default is no limit")
        ]
        public Nullable<int> ItemMaxCount
        {
            get
            {
                return (Nullable<int>)ViewState["itemMaxCount"];
            }
            set
            {
                ViewState["itemMaxCount"] = value;
            }
        }

        [
         Category("Data"),
         DefaultValue(""),
         Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")
         ]
        public string WebServiceClass
        {
            get
            {
                object obj = ViewState["webServiceClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["webServiceClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the datakeyfield value associated with the selected item
        /// </summary>
        public new object SelectedValue
        {
            get
            {
                if (this.DataKeyField.Length == 0)
                {
                    throw new InvalidOperationException("DataList_DataKeyField Must Be Specified!");
                }
                DataKeyCollection dataKeys = base.DataKeys;
                int selectedIndex = this.SelectedIndex;
                if (((dataKeys != null) && (selectedIndex < dataKeys.Count)) && (selectedIndex > -1))
                {
                    return dataKeys[selectedIndex];
                }
                return null;
            }
            set
            {
                if (this.DataKeyField.Length == 0)
                {
                    throw new InvalidOperationException("DataList_DataKeyField Must Be Specified!");
                }
                DataKeyCollection dataKeys = base.DataKeys;
                if (dataKeys == null)
                {
                    //We don't yet have a keys collection, so need to store the selected value for later
                    this.cachedSelectedValue = value;
                }
                else
                {
                    this.cachedSelectedValue = null;
                    //Find the matching item and set the selected index if it's already created
                    if (dataKeys.Count == 0)
                    {
                        this.cachedSelectedValue = value;
                    }
                    else
                    {
                        for (int i = 0; i < dataKeys.Count; i++)
                        {
                            if (dataKeys[i].ToString() == value.ToString())
                            {
                                this.SelectedIndex = i;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region IScriptControl Interface

        /// <summary>
        /// Sets the various properties on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DataList", ClientID);
            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                desc.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            yield return desc;
        }

        /// <summary>
        /// Returns the web service references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DataList.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
        }

        #endregion

        #region Methods and Event Handlers

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //See if we need to register a client component to go with the server control
            if (!String.IsNullOrEmpty(this.WebServiceClass))
            {
                ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

                if (scriptManager == null)
                {
                    throw new InvalidOperationException("ScriptManager required on the page.");
                }

                scriptManager.RegisterScriptControl(this);
            }

            //If we have a specified max-count, don't render the other items
            if (this.ItemMaxCount.HasValue)
            {
				bool didRemove = false;
                for (int i = this.Items.Count - 1; i >= this.ItemMaxCount.Value; i--)
                {
                    DataListItem dataListItem = this.Items[i];
                    while (dataListItem.Controls.Count > 0)
                    {
                        dataListItem.Controls.Remove(dataListItem.Controls[0]);
                    }
					didRemove = true;

                }
				if (didRemove)
				{
					// Add an elipses. Later, we can make this a clickable item to load the next 'x' rows.
					DataListItem ellipItem = new DataListItem(this.Items.Count, ListItemType.Footer);
					Literal litEllipItem = new Literal();
					litEllipItem.Text = "<span style=\"font-weight: bold;\">...</span>";
					ellipItem.Controls.Add(litEllipItem);
					this.Items[this.ItemMaxCount.Value].Controls.Add(ellipItem);
				}
            }
        }

        /// <summary>
        /// Renders out the script descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (!DesignMode && !String.IsNullOrEmpty(this.WebServiceClass) && this.Items != null && this.Items.Count > 0)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }
        }

        /// <summary>
        /// Handles the case where we've been passed a selected item before the control hierarchy was created
        /// </summary>
        /// <param name="useDataSource"></param>
        protected override void CreateControlHierarchy(bool useDataSource)
        {
            //See if we have a cached selected value to set
            if (this.cachedSelectedValue != null)
            {
                IEnumerable data = this.GetData();
                if (data != null)
                {
                    //Find the matching item and set the selected index if it's already created
                    int index = 0;
                    foreach (object obj in data)
                    {
                        object propertyValue = DataBinder.GetPropertyValue(obj, this.DataKeyField);
                        if (propertyValue.ToString() == this.cachedSelectedValue.ToString())
                        {
                            this.SelectedIndex = index;
                        }
                        index++;
                    }
                }
            }

            //First create the list of controls
            base.CreateControlHierarchy(useDataSource);
        }

        #endregion
    }
}
