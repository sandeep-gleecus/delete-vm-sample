using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class displays a drop-down list that allows multiple values to be selected (based on listbox)
    /// </summary>
    [ToolboxData("<{0}:DropDownMultiList runat=server></{0}:DropDownMultiList>")]
    public class DropDownMultiList : ListBox, IAuthorizedControl, IScriptControl
    {
        protected bool noValueItem = false;
        protected string noValueItemText = "";
        protected string metaData = "";
        private string rawSelectedValues = "";
        protected AuthorizedControlBase authorizedControlBase;

        //Viewstate keys
        protected const string ViewStateKey_MetaData_Base = "MetaData_";

        /// <summary>
        /// Constructor - delegates to base class
        /// </summary>
        public DropDownMultiList()
            : base()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Flag to determine if we want to display an initial entry for nullable fields
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Flag to determine if we want to display an initial entry for nullable fields"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool NoValueItem
        {
            get
            {
                return (this.noValueItem);
            }
            set
            {
                this.noValueItem = value;
            }
        }

        /// <summary>
        /// This is the type of artifact that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of artifact that the user's role needs to have permissions for"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum Authorized_ArtifactType
        {
            get
            {
                return authorizedControlBase.Authorized_ArtifactType;
            }
            set
            {
                authorizedControlBase.Authorized_ArtifactType = value;
            }
        }

        /// <summary>
        /// This is the type of action that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of action that the user's role needs to have permissions for"),
        DefaultValue(Project.PermissionEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public Project.PermissionEnum Authorized_Permission
        {
            get
            {
                return authorizedControlBase.Authorized_Permission;
            }
            set
            {
                authorizedControlBase.Authorized_Permission = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to execute the client script method of (leave blank for a global function)")
        ]
        public string ClientScriptServerControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptServerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptServerControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The client side script method that we want to execute")
        ]
        public string ClientScriptMethod
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptMethod"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptMethod"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("A parameter we want to pass to the client-side script (optional)")
        ]
        public string ClientScriptParameter
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptParameter"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptParameter"] = value;
            }
        }

        /// <summary>
        /// Contains the display name of the no-value item if displayed
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Contains the display name of the no-value item if displayed"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string NoValueItemText
        {
            get
            {
                return (this.noValueItemText);
            }
            set
            {
                this.noValueItemText = value;
            }
        }

        /// <summary>
        /// Returns a string list of all selected values, seperated by commas
        /// </summary>
        public string RawSelectedValues
        {
            get
            {
                return this.rawSelectedValues;
            }
        }

        /// <summary>Returns a string list of all selected values.</summary>
        /// <param name="getRaw">Whether to get all selected values, or only the ones that are contained in the list.</param>
        /// <returns>String of values.</returns>
        public List<UInt32> SelectedValues(bool getRaw = false)
        {
            List<UInt32> retList = new List<uint>();

            if (getRaw)
            {
                foreach (string value in this.rawSelectedValues.Split(','))
                {
                    UInt32 tryInt;
                    if (UInt32.TryParse(value, out tryInt))
                        retList.Add(tryInt);
                }
            }
            else
            {
                for (int k = 0; k < this.Items.Count; k++)
                {
                    if (this.Items[k].Selected)
                    {
                        UInt32 tryInt;
                        if (UInt32.TryParse(this.Items[k].Value, out tryInt))
                            retList.Add(tryInt);
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// Event called before rendering control - extended to include a null initial item if requested
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDataBinding(EventArgs e)
        {
            //Execute the base class first
            try
            {
                base.OnDataBinding(e);
            }
            catch (ArgumentOutOfRangeException)
            {
                //There is a bug in .NET FW 2.0 which sometimes causes this error to be thrown
            }

            //Check to see if we want to add an initial item corresponding to no value
            if (this.noValueItem)
            {
                //Check that there's not already a 'no value' item there (in case of postbacks)
                if (this.Items.Count == 0 || this.Items[0].Text != this.NoValueItemText)
                {
                    ListItem item = new ListItem(this.NoValueItemText, "");
                    this.Items.Insert(0, item);
                }
            }
        }

        /// <summary>
        /// Contains meta-data about the field that can be used in dynamic controls (e.g. datagrid)
        /// </summary>
        [
        Bindable(true),
        Category("Misc"),
        Description("Contains meta-data about the field that can be used in dynamic controls"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string MetaData
        {
            get
            {
                if (ViewState[ViewStateKey_MetaData_Base + this.ID] == null)
                {
                    return "";
                }
                else
                {
                    return ((string)ViewState[ViewStateKey_MetaData_Base + this.ID]);
                }
            }
            set
            {
                ViewState[ViewStateKey_MetaData_Base + this.ID] = value;
            }
        }

        /// <summary>
        /// The width of the drop-down part of the list
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The width of the drop-down part of the list"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit ListWidth
        {
            get
            {
                if (ViewState["ListWidth"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["ListWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["ListWidth"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The css class to use for a disabled drop-down list")
        ]
        public new string DisabledCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["DisabledCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["DisabledCssClass"] = value;
            }
        }

        /// <summary>
        /// Make sure that we have the right authorization or configuration to display/enable the control
        /// </summary>
        /// <param name="e">The Event Arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            scriptManager.RegisterScriptControl(this);
            ClientScriptManager clientScriptManager = Page.ClientScript;
            string script;

            //Hook into the loaded event to set the multiple values
            if (this.SelectedItem != null)
            {
                //Set the selected values
                string selectedValueList = "";
                foreach (ListItem listItem in this.Items)
                {
                    if (listItem.Value != "" && listItem.Selected)
                    {
                        if (selectedValueList == "")
                        {
                            selectedValueList = listItem.Value;
                        }
                        else
                        {
                            selectedValueList += "," + listItem.Value;
                        }
                    }
                }
                script = "function " + this.ClientID + "_loaded" + "(args) { $find('" + this.ClientID + "').set_selectedItem('" + selectedValueList + "'); }\n";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }

            //Hook into the selectedItemChanged event if necessary
            if (this.AutoPostBack)
            {
                //Add the special postback client code to force a postback
                script = "function " + this.ClientID + "_selectedItemChanged" + "(args) { " + Page.ClientScript.GetPostBackEventReference(this, "") + "; }\n";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }
            else if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                string codeToExecute = "";
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    //See if we have a parameter to add
                    if (string.IsNullOrEmpty(this.ClientScriptParameter))
                    {
                        codeToExecute = this.ClientScriptMethod + "(args)";
                    }
                    else
                    {
                        codeToExecute = this.ClientScriptMethod + "('" + this.ClientScriptParameter + "',args)";
                    }
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        //See if we have a parameter to add
                        if (string.IsNullOrEmpty(this.ClientScriptParameter))
                        {
                            codeToExecute = "$find('" + clientId + "')." + this.ClientScriptMethod + "(args)";
                        }
                        else
                        {
                            codeToExecute = "$find('" + clientId + "')." + this.ClientScriptMethod + "('" + this.ClientScriptParameter + "',args)";
                        }
                    }
                }
                script = "function " + this.ClientID + "_selectedItemChanged" + "(args) { " + codeToExecute + " }";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }

            //Finally, execute the base class
            base.OnPreRender(e);
        }

        /// <summary>
        /// Overrides the rendering of the built-in select control to just display a div
        /// as the client component does the rest of the rendering for us
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteBeginTag("div");
            writer.WriteAttribute("id", this.ClientID);
            writer.WriteAttribute("class", this.CssClass);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteEndTag("div");
            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
        }

        /// <summary>
        /// Updates the viewstate of the control with data returned from the client control
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            this.rawSelectedValues = "";
            if (!base.IsEnabled)
            {
                return false;
            }

            string[] values = postCollection.GetValues(postDataKey);
            bool flag = false;
            this.EnsureDataBound();
            if (values == null)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }

            //The custom client drop-down list returns a single value containing a comma-separated list
            if (values.Length < 1)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }
            else
            {
                this.rawSelectedValues = values[0];
            }

            //Get the list of values
            string[] multiValues = values[0].Split(',');
            if (multiValues.Length < 1)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }

            if (this.SelectionMode == ListSelectionMode.Single)
            {
                int selectedIndex = FindByValueInternal(multiValues[0], false);
                if (this.SelectedIndex != selectedIndex)
                {
                    base.SetPostDataSelection(selectedIndex);
                    flag = true;
                }
                return flag;
            }
            int length = multiValues.Length;
            //Inflectra.SpiraTest.Common.Logger.LogTraceEvent("DebugB", length.ToString());

            //Iterate through the items setting the ones that match
            foreach (ListItem listItem in this.Items)
            {
                listItem.Selected = false;
                for (int i = 0; i < multiValues.Length; i++)
                {
                    if (listItem.Value == multiValues[i])
                    {
                        listItem.Selected = true;
                    }
                }
                flag = true;
            }

/*
            ArrayList selectedIndicesInternal = this.SelectedIndicesInternal;
            ArrayList selectedIndices = new ArrayList(length);
            for (int i = 0; i < length; i++)
            {
                selectedIndices.Add(FindByValueInternal(multiValues[i], false));
            }
            int count = 0;
            if (selectedIndicesInternal != null)
            {
                count = selectedIndicesInternal.Count;
            }
            if (count == length)
            {
                for (int j = 0; j < length; j++)
                {
                    if (((int)selectedIndices[j]) != ((int)selectedIndicesInternal[j]))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                base.SelectInternal(selectedIndices);
            }*/
            return flag;
        }

        private int FindByValueInternal(string value, bool includeDisabled)
        {
            int num = 0;
            foreach (ListItem item in this.Items)
            {
                if (item.Value.Equals(value) && (includeDisabled || item.Enabled))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        /* IScriptControl Members */

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DropDownList", this.ClientID);

            //Populate the datasource that we set on the client
            JsonDictionaryOfStrings dataSource = new JsonDictionaryOfStrings();

            //Iterate through the list items and add to client datasource
            //We don't support mapping to an IsActive/ActiveYn field for this server control
            //so just leave out
            for (int i = 0; i < this.Items.Count; i++)
            {
                string itemValue = this.Items[i].Value;
                dataSource.Add(itemValue, this.Items[i].Text);
            }

            //Set the various attributes
            desc.AddProperty("name", this.UniqueID);
            desc.AddProperty("enabled", this.Enabled);
            desc.AddProperty("enabledCssClass", this.CssClass);
            desc.AddProperty("disabledCssClass", this.DisabledCssClass);
            //Multi-selectable since server control based on standard ListBox
            if (this.SelectionMode == ListSelectionMode.Multiple)
            {
                desc.AddProperty("multiSelectable", true);
            }
            else
            {
                desc.AddProperty("multiSelectable", false);
            }
            desc.AddScriptProperty("dataSource", JsonDictionaryConvertor.Serialize(dataSource));
            if (this.ListWidth == Unit.Empty)
            {
                if (this.Width != Unit.Empty)
                {
                    desc.AddProperty("listWidth", this.Width.ToString());
                }
            }
            else
            {
                desc.AddProperty("listWidth", this.ListWidth.ToString());
            }

            //Add a handler to set the multi-values, since we can't do it at init time as the data's not loaded
            if (this.SelectedItem != null)
            {
                desc.AddEvent("loaded", this.ClientID + "_loaded");
            }

            //Hook into the selectedItemChanged event if necessary
            if (this.AutoPostBack)
            {
                //We need to use the event to trigger a postback
                desc.AddEvent("selectedItemChanged", this.ClientID + "_selectedItemChanged");
            }
            else if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //we need to use the event to fire the custom client method
                desc.AddEvent("selectedItemChanged", this.ClientID + "_selectedItemChanged");
            }

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js"));
        }
    }

    public class UnityDropDownMultiList : DropDownMultiList
    {
    }
}
