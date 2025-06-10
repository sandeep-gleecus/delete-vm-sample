using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using System.Diagnostics;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Represents a 'right-click' context menu item that may be used by various other server controls
    /// </summary>
    [
    ToolboxData("<{0}:ContextMenuItem runat=server></{0}:ContextMenuItem>")
    ]
    public class ContextMenuItem : Control, IAuthorizedControl
    {
        protected AuthorizedControlBase authorizedControlBase;

        public ContextMenuItem()
            : base()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Returns true if the user is authorized to use the item in the menu
        /// </summary>
        /// <param name="currentRoleId">Current role</param>
        /// <param name="isSystemAdmin">Is user a system admin</param>
        /// <param name="isGroupAdmin">Is the user a group admin</param>
        /// <returns>true if authorized</returns>
        public bool IsAuthorized(int currentRoleId, bool isSystemAdmin, bool isGroupAdmin)
        {
            return (this.authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
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

        /// <summary>
        /// Any confirmation message that needs to be displayed. Leave empty if no confirmation needed
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        Description("Any confirmation message that needs to be displayed. Leave empty if no confirmation needed")
        ]
        public string ConfirmationMessage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["ConfirmationMessage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["ConfirmationMessage"] = value;
            }
        }

        /// <summary>
        /// The client side script method that we want to execute
        /// </summary>
        /// <remarks>
        /// You can only set a ClientScriptMethod OR a ClientComponentMethod, not both
        /// </remarks>
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

        /// <summary>
        /// The name of the client component command that we want to execute
        /// </summary>
        /// <remarks>
        /// You can only set a ClientScriptMethod OR a CommandName, not both
        /// The command name is passed to a specific pre-defined function on the control
        /// </remarks>
        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The name of the client component command that we want to execute")
        ]
        public string CommandName
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["CommandName"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["CommandName"] = value;
            }
        }

        /// <summary>
        /// Any command arguments that we want to pass
        /// </summary>
        /// <remarks>
        /// You can need to also pass a CommandName to use this
        /// </remarks>
        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The name of the client component command that we want to execute")
        ]
        public string CommandArgument
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["CommandArgument"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["CommandArgument"] = value;
            }
        }

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The CSS for the glyph icon to use in the menu entry"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string GlyphIconCssClass
        {
            get
            {
                if (ViewState["GlyphIconCssClass"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["GlyphIconCssClass"];
                }
            }
            set
            {
                ViewState["GlyphIconCssClass"] = value;
            }
        }

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The URL of the image that should be displayed in the menu entry"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string ImageUrl
        {
            get
            {
                if (ViewState["ImageUrl"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["ImageUrl"];
                }
            }
            set
            {
                ViewState["ImageUrl"] = value;
            }
        }

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The menu entry item text"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string Caption
        {
            get
            {
                if (ViewState["Caption"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["Caption"];
                }
            }
            set
            {
                ViewState["Caption"] = value;
            }
        }

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Is this just a divider / separator entry"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(false)
        ]
        public bool Divider
        {
            get
            {
                if (ViewState["Divider"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["Divider"];
                }
            }
            set
            {
                ViewState["Divider"] = value;
            }
        }

        /// <summary>
        /// Returns the JSON representation of a context menu item
        /// </summary>
        /// <returns>Its json representation</returns>
        public string ToJsonString()
        {
            string json = "caption: \"" + Caption.Replace("\"", "\\\"") + "\", ";
            json += "divider: " + Divider.ToString().ToLowerInvariant() + ", ";
            json += "imageUrl: \"" + ImageUrl.Replace("\"", "\\\"") + "\", ";
            json += "glyphIconCssClass: \"" + GlyphIconCssClass.Replace("\"", "\\\"") + "\", ";
            json += "confirmationMessage: \"" + ConfirmationMessage.Replace("\"", "\\\"") + "\", ";
            json += "clientScriptMethod: \"" + ClientScriptMethod.Replace("\"", "\\\"") + "\", ";
            json += "commandName: \"" + CommandName.Replace("\"", "\\\"") + "\", ";
            json += "commandArgument: \"" + CommandArgument.Replace("\"", "\\\"") + "\"";
            return "{ " + json + " }";
        }
    }
}
