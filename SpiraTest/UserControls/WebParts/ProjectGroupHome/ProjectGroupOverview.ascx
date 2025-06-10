<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectGroupOverview.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectGroupOverview" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<div style="padding-left:4px">
   <tstsc:RichTextLabel id="lblProjectGroupDescription" Runat="server" />  
   <table class="NoBorderTable">
        <tr runat="server" ID="rowPortfolio" visible="false">
		    <td class="pt3">
			    <b><asp:Localize runat="server" Text="<%$Resources:Fields,Portfolio %>" />:</b>
		    </td>
		    <td class="pt3">
                <tstsc:LinkButtonEx ID="btnPortfolio" runat="server" SkinID="ButtonLink"/>
		    </td>
	    </tr>
	    <tr>
		    <td class="pt3">
			    <b><asp:Localize runat="server" Text="<%$Resources:Fields,WebSite %>" />:</b>
		    </td>
		    <td class="pt3">
		        <div class="Spacer"></div>
			    <tstsc:HyperLinkEx 
                    ID="lnkProjectGroupWebsite" 
                    CssClass="url-to-shorten" 
                    Runat="server" 
                    Target="_blank" 
                    />
                &nbsp;
                <i class="far fa-window-restore" title="opens in new windows"></i>
		    </td>
	    </tr>
	    <tr>
		    <td class="pt3">
                <b><asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Owner_s_ %>" />:</b>
		    </td>
		    <td class="pt3">
		        <asp:Repeater Runat="server" ID="rptProjectGroupOwners">
		            <SeparatorTemplate>, </SeparatorTemplate>
				    <ItemTemplate>
					    <tstsc:LabelEx Runat="server" ID="lblOwnerName"><%#: ((ProjectGroupUser) Container.DataItem).FullName %></tstsc:LabelEx>
				    </ItemTemplate>
			    </asp:Repeater>
		    </td>
	    </tr>
    </table>
</div>
