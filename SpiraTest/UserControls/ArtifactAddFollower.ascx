<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArtifactAddFollower.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.ArtifactAddFollower" %>
<tstsc:DialogBoxPanel 
    Height="210px" 
    ID="pnlAddFollower" 
    Modal="true" 
    runat="server" 
    Width="400px"
    Title="<% $Resources:Buttons,AddFollower %>"
    >
	<div class="container-fluid pa0">
        <div class="row">
            <div class="col-sm-12">
				<tstsc:MessageBox 
                    ID="msgAddFollower" 
                    runat="server" 
                    SkinID="MessageBox" 
                    />
            </div>
        </div>
        <div class="row DataEntryForm form-horizontal">
            <div class="form-group col-xs-12">
                <p>
					<asp:Literal 
                        runat="server" 
                        Text="<% $Resources:Main,Followers_SelectProjectUser %>" 
                        />
                </p>
				<tstsc:DropDownListEx 
                    ID="ddlAddProjUser" 
                    runat="server" 
                    DataValueField="UserId" 
                    DataTextField="FullName" 
                    />
            </div>
            <div class="btn-group col-xs-12">
				<tstsc:HyperLinkEx 
                    ClientScriptMethod="pnlAddFollower_lnkAddFollower_click(event)" 
                    ID="lnkAddFollower" 
                    NavigateUrl="javascript:void(0)" 
                    runat="server" 
                    SkinID="ButtonPrimary" 
                    Text="<% $Resources:Buttons,Add %>" 
                    />
				<tstsc:HyperLinkEx 
                    ClientScriptMethod="close()" 
                    ClientScriptServerControlId="pnlAddFollower" 
                    ID="lnkCancel" 
                    NavigateUrl="javascript:void(0)" 
                    runat="server" 
                    SkinID="ButtonDefault" 
                    Text="<% $Resources:Buttons,Cancel %>" 
                    />
            </div>
        </div>
	</div>
</tstsc:DialogBoxPanel>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
	<Services>
		<asp:ServiceReference Path="~/Services/Ajax/NotificationService.svc" />
	</Services>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    //set SpiraContext variables if necessary
    $(document).ready(function() {
        if (!SpiraContext.ArtifactId) SpiraContext.ArtifactId =<%= this.ArtifactId %>;
        if (!SpiraContext.ArtifactTypeId) SpiraContext.ArtifactTypeId = <%= (int)this.ArtifactTypeEnum %>;
    });

	function pnlAddFollower_displayed()
	{
		var ddlEmailProjUsers = $find('<%= this.ddlAddProjUser.ClientID %>');
	}
		
    function pnlAddFollower_lnkAddFollower_click(evt)
	{
		//Get controls.
        var ddlSelectUser = $find('<%= this.ddlAddProjUser.ClientID %>');
        var msgAddFollower = $get('<%=msgAddFollower.ClientID%>');

		//Clear any messages
        globalFunctions.clear_errors(msgAddFollower);
        Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_SubscribeSelectedUserToArtifact(
            SpiraContext.ProjectId, 
            SpiraContext.ArtifactTypeId,
            SpiraContext.ArtifactId,
            ddlSelectUser.get_selectedItem().get_value(), 
            pnlAddFollower_lnkAddFollower_success, 
            pnlAddFollower_lnkAddFollower_failure
        );
	}
    function pnlAddFollower_lnkAddFollower_success(data)
	{
	    var msgAddFollower = $get('<%=msgAddFollower.ClientID%>');
	    globalFunctions.display_info_message(msgAddFollower, Inflectra.SpiraTest.Web.GlobalResources.ArtifactAddFollower_Success);
        
        //refresh UI to show newly added follower
	    pnlAddFollower_updateFollowers(
            SpiraContext.ProjectId,
            SpiraContext.ArtifactTypeId,
            SpiraContext.ArtifactId
        );
	}
    function pnlAddFollower_lnkAddFollower_failure(exception)
	{
	    var msgAddFollower = $get('<%=msgAddFollower.ClientID%>');
	    globalFunctions.display_error(msgAddFollower, exception);
	}

    function ArtifactAddFollower_pnlAddFollower_display()
    {
		//Reset items
		globalFunctions.clear_errors($get('<%= msgAddFollower.ClientID %>'));

		var panel = $find('<%= this.pnlAddFollower.ClientID %>');
		panel.display();
    }

    function pnlAddFollower_updateFollowers(projectId, artifactTypeId, artifactId) {
        if (artifactId && artifactId > 0)
        {
            Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_FollowersOfArtifact(
                projectId,
                artifactTypeId,
                artifactId,
                pnlAddFollower_updateFollowers_success, /*followers is in the js file of the react component (followers.tsx)*/
                pnlAddFollower_updateFollowers_failure
            )
        }
    }
    //NB: pnlAddFollower_updateFollowers_success is properly defined inside the followers.tsx file to allow access of the function from within the react component
    var pnlAddFollower_updateFollowers_success;
    function pnlAddFollower_updateFollowers_failure() {
        //fail quietly
    }
</script>
