<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuickFilterPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.QuickFilterPanel" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<div class="Content">
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <ul>
        <asp:Repeater ID="rptMyFilters" runat="server">
            <ItemTemplate>
                <li>
                    <tstsc:HyperLinkEx ID="lnkMyFilter" runat="server" NavigateUrl="javascript:void(0)" ToolTip='<%#"<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).Name) + "</u><br /><span class=\"fas fa-arrow-right\"></span> " + Microsoft.Security.Application.Encoder.HtmlEncode(((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).UserName) %>' ClientScriptMethod='<%# "lnkFilter_click(" + ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).SavedFilterId + ")"%>' >
                        <span runat="server" visible="<%# !((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).IsShared%>" class="fas fa-user"></span>
                        <span runat="server" visible="<%# ((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).IsShared%>" class="fas fa-users"></span>
                        <asp:Literal runat="server" Text="<%#:((Inflectra.SpiraTest.DataModel.SavedFilter) Container.DataItem).Name%>" />
                    </tstsc:HyperLinkEx>
                </li>       
            </ItemTemplate>
            <FooterTemplate>
                <span class="NoData">
                    <asp:Localize ID="locNoMyFilters" runat="server" Text="<%$Resources:Main,QuickFilterPanel_NoFilters %>" Visible="false" />
                </span>
            </FooterTemplate>
        </asp:Repeater>
    </ul>

    <asp:PlaceHolder ID="plcComponents" runat="server">
        <h4>
            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Components %>" />
        </h4>
        <ul>
            <asp:Repeater ID="rptComponents" runat="server">
                <ItemTemplate>
                    <li>
                        <tstsc:ImageEx ID="imgComponent" CssClass="w4 h4" runat="server" ImageUrl="Images/org-Component.svg" AlternateText="<%$Resources:Fields,ComponentId %>" />
                        <tstsc:HyperLinkEx ID="lnkComponent" runat="server" Text="<%#:((Inflectra.SpiraTest.DataModel.Component) Container.DataItem).Name%>"
                            NavigateUrl="javascript:void(0)" ToolTip="<%#:((Inflectra.SpiraTest.DataModel.Component) Container.DataItem).Name%>" ClientScriptMethod='<%# "lnkComponent_click(" + ((Inflectra.SpiraTest.DataModel.Component) Container.DataItem).ComponentId + ")"%>' />
                    </li>       
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="plcReleases" runat="server">
        <h4>
            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,SiteMap_Releases %>" />
        </h4>
        <tstsc:UnityDropDownHierarchy ID="ddlRelease" Runat="server" NoValueItem="true" SkinID="UnityDropDownList_ReleaseInQuickFilterPanel"
            NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" AutoPostBack="false" DataTextField="FullName"
            DataValueField="ReleaseId" Width="300px" ListWidth="300px"
            ActiveItemField="IsActive" ClientScriptMethod="ucQuickFilterPanel_ddlRelease_changed" />
    </asp:PlaceHolder>
</div>

<script type="text/javascript">
    var artifactTypeId = <%=(int)ArtifactType %>;
    var artifactType_Incident = <%=(int)Artifact.ArtifactTypeEnum.Incident %>;
    var artifactType_TestCase = <%=(int)Artifact.ArtifactTypeEnum.TestCase %>;
    function lnkFilter_click(savedFilterId)
    {
        var ajaxControl = $find('<%=AjaxServerControlClientId %>');
        ajaxControl.retrieve_filter(savedFilterId);
    }
    function lnkComponent_click(componentId)
    {
        var ajaxControl = $find('<%=AjaxServerControlClientId %>');
        if (artifactTypeId == artifactType_Incident || artifactTypeId == artifactType_TestCase)
        {
            ajaxControl.set_filter('ComponentIds', componentId, true);
        }
        else
        {
            ajaxControl.set_filter('ComponentId', componentId, true);
        }
    }
    function ucQuickFilterPanel_ddlRelease_changed(selectedItem)
    {
        if (selectedItem)
        {
            var releaseId = selectedItem.get_value();
            var ajaxControl = $find('<%=AjaxServerControlClientId %>');

            //Filter by Release
            ajaxControl.set_filter('<%=ReleaseFilterField %>', releaseId, true);
        }
    }
    
</script>
