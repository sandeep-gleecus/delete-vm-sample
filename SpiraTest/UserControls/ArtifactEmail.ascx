<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ArtifactEmail.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.ArtifactEmail" %>


<tstsc:DialogBoxPanel 
    ID="pnlSendEmail" 
    runat="server" 
    Height="210px" 
    Modal="true" 
    Title="<% $Resources:Main,Notifications_SendEmailTo %>"
    >




	<ul class="u-box_list u-box_group w9" id="customFieldsDates" runat="server">
        <li class="ma0 mb4 pa0">
				<tstsc:MessageBox ID="msgEmailMessages" runat="server" SkinID="MessageBox" />
        </li>
        <li class="ma0 pa0">
            <asp:RadioButton 
                Checked="true" 
                GroupName="emailOption" 
                ID="rdoSendEmailToUser" 
                onClick="pnlSendEmail_emailOption_Checked(event)" 
                runat="server" 
                Text="<%$Resources:Main,ArtifactEmail_ProjectUser %>" 
                />
        </li>
        <li class="ma0 mb4 pa0">
			<tstsc:UnityDropDownListEx 
                CssClass="u-dropdown is-active" 
                DataTextField="FullName" 
                DataValueField="UserId" 
                DisabledCssClass="u-dropdown disabled" 
                ID="ddlEmailProjUsers" 
                runat="server" 
                />
			<br />
            <small>
				<asp:Literal 
                    runat="server" 
                    Text="<% $Resources:Main,Notifications_SelectProjectUser %>" 
                    />
            </small>
        </li>
        <li class="ma0 pa0">
			<asp:RadioButton 
                GroupName="emailOption" 
                ID="rdoSendEmailToAddress" 
                onClick="pnlSendEmail_emailOption_Checked(event)" 
                runat="server" 
                Text="<%$Resources:Main,ArtifactEmail_EmailAddresses %>" 
                />
        </li>
        <li class="ma0 mb4 pa0">
            <tstsc:UnityTextBoxEx 
                CssClass="u-input is-active"  
                DisabledCssClass="u-input disabled" 
                EnableViewState="False" 
                ID="txbSendEmailToAddressList" 
                RichText="false" 
                runat="server" 
                Width="100%" 
                />
			<br />
            <small>
				<asp:Literal 
                    runat="server" 
                    Text="<% $Resources:Main,Notifications_EnterListofEmails %>" 
                    />
            </small>
        </li>
        <li class="ma0 pa0">
            <tstsc:LabelEx 
                AssociatedControlID="txtSendEmailSubject" 
                ID="lblSendEmailSubjectLabel" 
                runat="server" 
                Text="<%$Resources:Main,ArtifactEmail_MessageSubject %>" 
                />
        </li>
        <li class="ma0 mb3 pa0">
			<tstsc:UnityTextBoxEx 
                CssClass="u-input is-active" 
                EnableViewState="False" 
                DisabledCssClass="u-input disabled"
                ID="txtSendEmailSubject" 
                MaxLength="100" 
                RichText="false" 
                runat="server" 
                TextMode="SingleLine" 
                Width="100%" 
                />
			<br />
            <small>
				<asp:Literal 
                    runat="server" 
                    Text="<% $Resources:Main,Global_BlankforDefault %>" 
                    />
            </small>
        </li>
        <li class="ma0 mb3 pa0 btn-group">
			<tstsc:HyperLinkEx 
                ID="lnkSendEmail" 
                runat="server" 
                NavigateUrl="javascript:void(0)" 
                ClientScriptMethod="pnlSendEmail_lnkSendEmail_click(event)" 
                Text="<% $Resources:Buttons,Send %>" 
                SkinID="ButtonPrimary" 
                />
			<tstsc:HyperLinkEx 
                ID="lnkSendEmailCancel" 
                runat="server" 
                NavigateUrl="javascript:void(0)" 
                ClientScriptServerControlId="pnlSendEmail" 
                ClientScriptMethod="close()" 
                Text="<% $Resources:Buttons,Cancel %>"  
                SkinID="ButtonDefault" 
                />
        </li>
	</ul>
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

	function pnlSendEmail_displayed()
	{
		var ddlEmailProjUsers = $find('<%= this.ddlEmailProjUsers.ClientID %>');
		var txbSendEmailToAddressList = $get('<%= this.txbSendEmailToAddressList.ClientID %>');
		pnlSendEmail_emailOption_Checked(null);
	}
	function pnlSendEmail_emailOption_Checked(evt)
	{
		var rdoSendEmailToUser = $get('<%= this.rdoSendEmailToUser.ClientID %>');
		var rdoSendEmailToAddress = $get('<%= this.rdoSendEmailToAddress.ClientID %>');
		var ddlEmailProjUsers = $find('<%= this.ddlEmailProjUsers.ClientID %>');
		var txbSendEmailToAddressList = $get('<%= this.txbSendEmailToAddressList.ClientID %>');

		//Update the enabled/disabled status
		if (rdoSendEmailToUser.checked)
		{
			ddlEmailProjUsers.set_enabled(true);
			txbSendEmailToAddressList.className = 'u-input disabled';
			txbSendEmailToAddressList.readOnly = true;
		}
		else
		{
			ddlEmailProjUsers.set_enabled(false);
			txbSendEmailToAddressList.className = 'u-input is-active';
			txbSendEmailToAddressList.readOnly = false;
		}
	}
	function pnlSendEmail_lnkSendEmail_click(evt)
	{
		//Get controls.
		var msgEmailMessages = $get('<%= msgEmailMessages.ClientID %>');
		var rdoSendEmailToUser = $get('<%= this.rdoSendEmailToUser.ClientID %>');
		var ddlSelectUser = $find('<%= this.ddlEmailProjUsers.ClientID %>');
		var txbSubject = $get('<%= this.txtSendEmailSubject.ClientID %>');
		var txbToAddr = $get('<%= this.txbSendEmailToAddressList.ClientID %>');

		//Clear any messages
		globalFunctions.clear_errors(msgEmailMessages);
		if (rdoSendEmailToUser.checked)
		    Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.SendMailNotificationToUser(
                ddlSelectUser.get_selectedItem().get_value(),
                SpiraContext.ProjectId,
                SpiraContext.ArtifactTypeId,
                SpiraContext.ArtifactId,
                txbSubject.value,
                sendMailNotificationToUser_success,
                sendMailNotificationToUser_failure
                );
		else
		    Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.SendMailNotificationToEmail(
                txbToAddr.value,
                SpiraContext.ProjectId,
                SpiraContext.ArtifactTypeId,
                SpiraContext.ArtifactId,
                txbSubject.value,
                sendMailNotificationToUser_success,
                sendMailNotificationToUser_failure
                );
	}
	function sendMailNotificationToUser_success(data)
	{
		var msgEmailMessages = $get('<%=msgEmailMessages.ClientID%>');
		globalFunctions.display_info_message(msgEmailMessages, Inflectra.SpiraTest.Web.GlobalResources.ArtifactEmail_SentMessageSuccess);
	}
	function sendMailNotificationToUser_failure(exception)
	{
		var msgEmailMessages = $get('<%=msgEmailMessages.ClientID%>');
		globalFunctions.display_error(msgEmailMessages, exception);
	}
	function ArtifactEmail_pnlSendEmail_display(evt)
	{
		//Reset items.
		$get('<%= this.txtSendEmailSubject.ClientID %>').value = "";
		$get('<%= this.rdoSendEmailToUser.ClientID %>').checked = true;
		globalFunctions.clear_errors($get('<%= msgEmailMessages.ClientID %>'));

		var panel = $find('<%= this.pnlSendEmail.ClientID %>');
		panel.display(evt);
	}
    function ArtifactEmail_subscribeArtifactChange(dropMenu)
    {
        //Call the webservice to subscribe or unsubscribe
        var c = {};
        c.dropMenu = dropMenu;
        globalFunctions.display_spinner();
        if (dropMenu.get_glyphIconCssClass() == 'mr3 fas fa-star')
        {
            Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_UnsubscribeFromArtifact(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactTypeId,
                SpiraContext.ArtifactId,
                ArtifactEmail_subscribeChange_success,
                ArtifactEmail_subscribeChange_failure,
                c
                );
        }
        else
        {
            Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_SubscribeToArtifact(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactTypeId,
                SpiraContext.ArtifactId,
                ArtifactEmail_subscribeChange_success,
                ArtifactEmail_subscribeChange_failure,
                c
                );
        }
    }
    function ArtifactEmail_subscribeChange_success(data, c)
    {
        globalFunctions.hide_spinner();   
        var dropMenu = c.dropMenu;

        //update the dropmenu text and icon as needed
        ArtifactEmail_subscribeChange_UpdateDropMenu (dropMenu)

        //refresh the followers list UI, if present
        //first check to see if followers is present on page
        if (followers && typeof pnlAddFollower_updateFollowers != "undefined") 
        {
            //get the list of updated followers
            pnlAddFollower_updateFollowers(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactTypeId,
                SpiraContext.ArtifactId
                );
        }
        else
        {
            if (typeof pnlAddFollower_updateFollowers == "undefined") console.log('SpiraTeam: pnlAddFollower_updateFollowers could not be found');
        }
    }
    function ArtifactEmail_subscribeChange_failure(ex, c)
    {
        var messageBox = $get('<%=MessageBoxClientId %>');
        globalFunctions.hide_spinner();
        globalFunctions.display_error(messageBox, ex);
    }

    function ArtifactEmail_subscribeChange_UpdateDropMenu (dropMenu) {
        var className = dropMenu.get_glyphIconCssClass();
        var text;
        if (className == 'mr3 fas fa-star')
        {
            className = 'mr3 far fa-star';
            text = '<%=SubscribeText%>';
        }
        else
        {
            className = 'mr3 fas fa-star';
            text = '<%=UnSubscribeText%>';
        }
        dropMenu.set_glyphIconCssClass(className);
        dropMenu.set_text(text);
        dropMenu.update_menu();
    }

    //Change the subscribe state
    function ArtifactEmail_isUserSubscribed(projectId, artifactTypeId, artifactId, dropMenu)
    {
        Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_IsUserSubscribed(
            projectId,
            artifactTypeId,
            artifactId,
            ArtifactEmail_isUserSubscribed_success,
            ArtifactEmail_isUserSubscribed_failure,
            { dropMenu: dropMenu }
            );
    }
    function ArtifactEmail_isUserSubscribed_success(isUserSubscribed, context)
    {
        var dropMenu = context.dropMenu;
        if (isUserSubscribed)
        {
            dropMenu.set_glyphIconCssClass('mr3 fas fa-star');
            dropMenu.set_text('<%=UnSubscribeText%>');
            dropMenu.update_menu();
        }
        else
        {
            dropMenu.set_glyphIconCssClass('mr3 far fa-star');
            dropMenu.set_text('<%=SubscribeText%>');
            dropMenu.update_menu();
        }
    }
    function ArtifactEmail_isUserSubscribed_failure(ex, context)
    {
        globalFunctions.display_error($get('<%=MessageBoxClientId %>'), ex);
    }

</script>
