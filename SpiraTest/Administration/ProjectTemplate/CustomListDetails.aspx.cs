using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    /// This webform code-behind class is responsible to displaying the
    /// Administration Custom List Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "CustomListDetails_Title", "Template-Custom-Properties/#edit-custom-lists", "CustomListDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class CustomListDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.CustomListDetails::";

        //Member variables
        protected int customPropertyListId;

        //Bound data for the grid
        protected SortedList<string, string> flagList;

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error message
            this.lblMessage.Text = "";

            if (String.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_LIST_ID]))
            {
                //If they wanted a new list, create it, and then redirect the user..
                CustomPropertyList newList = new CustomPropertyManager().CustomPropertyList_Add(ProjectTemplateId, Resources.Main.CustomLists_NewList, true, true);
                Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomListDetails") + "?" + GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_LIST_ID + "=" + newList.CustomPropertyListId.ToString(), true);
                return;
            }

            //Get the custom list id from the querystring
            this.customPropertyListId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_LIST_ID]);

            //Events..
            this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
            this.grdCustomListValues.RowCommand += new GridViewCommandEventHandler(grdCustomListValues_RowCommand);
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            //Set the return URL
            this.lnkCustomList.NavigateUrl = UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomLists");

            if (!this.IsPostBack)
            {
                this.LoadAndDataBind();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();

        }

        /// <summary>
        /// Returns back to the list page without saving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomLists"));
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            ValidateAndSaveData(false);
            LoadAndDataBind();
        }

        /// <summary>Hit when the user performs an action on the row.</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Arguments for the command.</param>
        internal void grdCustomListValues_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdCustomListValues_RowCommand";

            try
            {
                //Depends on what we're doing..
                switch (e.CommandName.ToLowerInvariant().Trim())
                {
                    case "createnew":
                        //Save the rows we already have, first..
                        this.SaveData();

                        //Add a new row..
                        CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                        customPropertyManager.CustomPropertyList_AddValue(this.customPropertyListId, "");
                        this.List = customPropertyManager.CustomPropertyList_RetrieveById(this.customPropertyListId, true, true);
                        this.grdCustomListValues.DataSource = this.List.Values;
                        this.grdCustomListValues.DataBind();

                        break;

                    case "remove":
                        {
                            //delete selected row..
                            int customValueId = int.Parse((string)e.CommandArgument);

                            //Remove it from the list..
                            this.List = new CustomPropertyManager().CustomPropertyList_DeleteValue(customValueId);
                            this.LoadAndDataBind();
                        }
                        break;

                    case "undelete":
                        {
                            //undelete selected row..
                            int customValueId = int.Parse((string)e.CommandArgument);
                            new CustomPropertyManager().CustomPropertyList_UndeleteValue(customValueId);
                            this.LoadAndDataBind();
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the status of the custom property list value
        /// </summary>
        /// <param name="cpv"></param>
        /// <returns></returns>
        protected string GetStatus(CustomPropertyValue cpv)
        {
            if (cpv.IsDeleted)
            {
                return Resources.Main.Global_Deleted;
            }
            else
            {
                if (cpv.IsActive)
                {
                    return Resources.Fields.ActiveYn;
                }
                else
                {
                    return Resources.Main.Global_Inactive;
                }
            }
        }


        /// <summary>Loads the data.</summary>
        internal void LoadAndDataBind()
        {
            const string METHOD_NAME = CLASS_NAME + "LoadAndDataBind()";

            try
            {
                //Get the filter type
                string filterType = this.ddlFilterType.SelectedValue;
                bool activeOnly = (filterType == "allactive");
                bool includeDeleted = (filterType == "all");

                //Populate the active flag                
                this.flagList = new ComponentManager().RetrieveFlagLookup();

                //Load the list..
                CustomPropertyList list = new CustomPropertyManager().CustomPropertyList_RetrieveById(this.customPropertyListId, true, !activeOnly, includeDeleted);
                if (list == null)
                    throw new ArgumentException("Invalid List ID", "listID");

                this.List = list;

                //Set datasources..
                this.grdCustomListValues.DataSource = this.List.Values;
                this.listSort.DataSource = GlobalFunctions.CustomListSortByList();
                this.listName.Text = this.List.Name;
                this.listSort.SelectedIndex = ((this.List.IsSortedOnValue) ? 1 : 0);
                this.lblListName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(this.List.Name);
                this.DataBind();
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex, "Could not load page: " + ex.Message);
                this.lblMessage.Text = ex.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>Hit when the user wants to save their values.</summary>
        /// <param name="sender">btnSave</param>
        /// <param name="e">EventArgs</param>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ValidateAndSaveData();
        }

        /// <summary>
        /// Validates and saves the data
        /// </summary>
        protected void ValidateAndSaveData(bool redirectBackToList = true)
        {
            const string METHOD_NAME = CLASS_NAME + "ValidateAndSaveData";

            //Error checking!
            bool hasError = false;
            bool hasEmptyError = false;
            this.lblMessage.Text = "";
            List<string> valueNames = new List<string>();
            List<CustomPropertyValue> listSaveValues = new List<CustomPropertyValue>();

            //First, check the list name..
            if (string.IsNullOrWhiteSpace(this.listName.Text))
            {
                hasError = true;
                this.lblMessage.Text = Resources.Main.Admin_System_CustomListValue_Error_CustomListNameIsEmpty;
                this.listName.CssClass += " TextBoxInvalid";
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }

            //Loop through all entered items and find issues.
            for (int i = 0; i < this.grdCustomListValues.Rows.Count; i++)
            {
                if (grdCustomListValues.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the TextBox & get the entered value.
                    TextBoxEx txtCustomListValue = (TextBoxEx)grdCustomListValues.Rows[i].Cells[2].FindControl("txtName");
                    string customListValue = txtCustomListValue.Text.Trim().ToLowerInvariant();
                    int customValueId = Int32.Parse(txtCustomListValue.MetaData);

                    //Check to make sure it's not empty..
                    if (string.IsNullOrWhiteSpace(customListValue))
                    {
                        if (!hasEmptyError)
                        {
                            if (hasError)
                                this.lblMessage.Text += "<br />";
                            this.lblMessage.Text += Resources.Main.Admin_System_CustomListValue_Error_ValueNameIsEmpty;
                        }
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        txtCustomListValue.CssClass += " TextBoxInvalid";
                        hasError = true;
                        hasEmptyError = true;
                    }
                    else if (valueNames.Contains(customListValue))
                    {
                        if (hasError) this.lblMessage.Text += "<br />";
                        this.lblMessage.Text += String.Format(Resources.Main.Admin_System_CustomListValue_Error_ValueNameIsDuplicate, txtCustomListValue.Text.Trim());
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        txtCustomListValue.CssClass += " TextBoxInvalid";
                        hasError = true;
                    }
                    else
                    {
                        //Add the name to the list..
                        valueNames.Add(customListValue);
                        //Add the custom property value to the list..
                        listSaveValues.Add(new CustomPropertyValue() { CustomPropertyListId = this.customPropertyListId, CustomPropertyValueId = customValueId, Name = txtCustomListValue.Text.Trim() });
                        //Reset textbox..
                        txtCustomListValue.CssClass = txtCustomListValue.CssClass.Replace("TextBoxInvalid", "").Trim();
                    }
                }
            }

            try
            {
                this.SaveData();

                if (!hasError && redirectBackToList)
                {
                    //Send them back to the list page..
                    Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomLists"), true);
                }
            }
            catch (EntityException exSql)
            {
                //Make error message.
                if (!String.IsNullOrWhiteSpace(this.lblMessage.Text))
                    this.lblMessage.Text += "<br />";
                if (exSql.GetType() == typeof(EntityConstraintViolationException))
                {
                    this.lblMessage.Text += Resources.Main.Admin_System_CustomListValue_Error_CustomListNameExists;
                    this.listName.CssClass += " TextBoxInvalid";
                }
                else
                {
                    this.lblMessage.Text += Resources.Main.Admin_System_CustomListValue_Error_SavingCustomList;
                }

                //Log all error messages..
                Logger.LogErrorEvent(METHOD_NAME, exSql, "Saving custom list.");
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Actually records tyhe values the user entered in..</summary>
        private void SaveData()
        {
            const string METHOD_NAME = CLASS_NAME + "SaveData()";
            try
            {
                //Values First.
                CustomPropertyList origList = new CustomPropertyManager().CustomPropertyList_RetrieveById(this.customPropertyListId, true, true, true);

                if (origList != null)
                {
                    origList.StartTracking();

                    for (int i = 0; i < this.grdCustomListValues.Rows.Count; i++)
                    {
                        if (this.grdCustomListValues.Rows[i].RowType == DataControlRowType.DataRow)
                        {
                            CustomPropertyValue entItem = origList.Values.Where(clv => clv.CustomPropertyValueId == int.Parse(((TextBoxEx)grdCustomListValues.Rows[i].Cells[2].FindControl("txtName")).MetaData)).FirstOrDefault();

                            if (entItem == null)
                            {
                                //New, add it.
                                CustomPropertyValue newValue = new CustomPropertyValue();
                                newValue.Name = ((TextBoxEx)grdCustomListValues.Rows[i].Cells[2].FindControl("txtName")).Text.Trim();
                                newValue.IsActive = (((CheckBoxYnEx)(grdCustomListValues.Rows[i].FindControl("chkActive"))).Checked);
                                newValue.CustomPropertyListId = this.customPropertyListId;
                                origList.Values.Add(newValue);
                            }
                            else
                            {
                                //Exists, update it..
                                entItem.StartTracking();
                                entItem.Name = ((TextBoxEx)grdCustomListValues.Rows[i].Cells[2].FindControl("txtName")).Text.Trim();
                                entItem.IsActive = (((CheckBoxYnEx)(grdCustomListValues.Rows[i].FindControl("chkActive"))).Checked);
                            }
                        }
                    }

                    //Now set the name and desc..
                    origList.Name = this.listName.Text.Trim();
                    origList.IsSortedOnValue = (this.listSort.SelectedValue.Trim() == "1");

                    //Lets try to save this..
                    new CustomPropertyManager().CustomPropertyList_Update(origList);
                    this.LoadAndDataBind();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>The list and its values that we're editing.</summary>
        protected CustomPropertyList List
        {
            get;
            set;
        }
    }
}
