<%@ Page 
    Language="c#" 
    ValidateRequest="false" 
    CodeBehind="EmailPassword.aspx.cs"
	AutoEventWireup="True" 
    Inherits="Inflectra.SpiraTest.Web.EmailPassword" 
    MasterPageFile="~/MasterPages/Login.Master" 
    %>

<%@ Register 
    TagPrefix="tstsc" 
    Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" 
    %>

<asp:Content 
    ContentPlaceHolderID="cplHead" 
    runat="server" 
    ID="Content1"
    >
</asp:Content>



<asp:Content 
    ContentPlaceHolderID="cplMainContent" 
    runat="server" 
    ID="Content2"
    >
    <asp:ValidationSummary 
        ID="PasswordRecoveryValidationSummary" 
        runat="server"
        ValidationGroup="PasswordRecovery" 
        />
    <tstsc:PasswordRecoveryEx 
        ID="PasswordRecovery" 
        runat="server" 
        RenderOuterTable="false" 
        >
        <MailDefinition Priority="Normal" />
        <QuestionTemplate>
            <tstsc:MessageBox 
                ID="FailureText" 
                runat="server" 
                SkinID="MessageBox"
                />

            <asp:RequiredFieldValidator 
                ID="AnswerRequired" 
                runat="server" 
                ControlToValidate="Answer"
                ErrorMessage="<%$Resources:Messages,EmailPassword_AnswerRequired %>" 
                ToolTip="<%$Resources:Messages,EmailPassword_AnswerRequired %>"
                ValidationGroup="PasswordRecovery" 
                />





            <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm tc">
                <h1 class="tc fw-b mt5 mt4-xs mb0 blue-strong">                                    
                    <asp:Localize 
                        ID="Localize2" 
                        runat="server" 
                        Text="<%$Resources:Main,ResetPassword_IdentityConfirmation %>" 
                        />
                </h1>
                <h2 class="ma0 tc mt0 mb4 blue-strong">
                    (<asp:Literal 
                        runat="server" 
                        ID="UserNameLabel" 
                        Text="<%$Resources:Fields,UserName %>" 
                        />:  
                    <strong>
                        <asp:Literal  
                            ID="UserName" 
                            runat="server" 
                            />)
                    </strong>
                </h2>

                <p class="mt0 mb2 ml3 tl">                                        
                    <asp:Localize 
                        ID="Localize3" 
                        runat="server" 
                        Text="<%$Resources:Main,ResetPassword_AnswerQuestionInstructions %>" 
                        />
                </p>
                <p class="mt0 mb5 ml3 tl fs-125">
                    <strong class="text-primary">
                        <tstsc:LabelEx 
                            runat="server" 
                            ID="QuestionLabel" 
                            AssociatedControlID="Question" 
                            Text="<%$Resources:Fields,Question %>" 
                            />
                    </strong>: 
                    <tstsc:LabelEx 
                        ID="Question" 
                        runat="server" 
                        CssClass="text-primary"
                        />
                </p>

                <div class="mb5 mt4 mx3 relative">
                    <tstsc:UnityTextBoxEx
                        CssClass="w-100 u-input u-input-minimal py3 fs-110"
                        ID="Answer" 
                        runat="server" 
                        TextMode="SingleLine" 
                        ClientIDMode="static"
                        />
                    <asp:Label 
                        AssociatedControlID="Answer" 
                        CssClass="label-slideup tl"
                        runat="server"
                        Text="<%$ Resources:Fields,Answer %>"
                        />
                </div>
                                        
                <div class="btn-group">
                    <tstsc:ButtonEx 
                        ID="btnQuestionSubmit" 
                        runat="server" 
                        SkinID="ButtonPrimary" 
                        CommandName="Submit" 
                        Text="<%$Resources:Buttons,Submit %>"
                        ValidationGroup="PasswordRecovery" 
                        ClientIdMode="static"
                        />
                    <tstsc:ButtonEx 
                        ID="ButtonEx2" 
                        runat="server" 
                        Text="<%$Resources:Buttons,Cancel %>" 
                        CausesValidation="false"
                        OnClick="btnCancel_Click" 
                        />
                </div>
            </div>
        </QuestionTemplate>




        <SuccessTemplate>
            <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
                <h1 class="ma0 tc fw-b mt5 mt4-xs mb5 blue-strong">  
                    <asp:Localize 
                        ID="Localize6" 
                        runat="server" 
                        Text="<%$Resources:Main,ResetPassword_SuccessTitle %>" 
                        />                                        
                </h1>
                <div class="btn-wrapper-wide">
                    <tstsc:HyperLinkEx 
                        SkinID="ButtonPrimary" 
                        ID="lnkReturn" 
                        runat="server" 
                        NavigateUrl="<%#ReturnUrl%>" 
                        Text="<%$Resources:Main,ResetPassword_ReturnLink %>" 
                        />
                </div>
            </div>
        </SuccessTemplate>






        <UserNameTemplate> 
            <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm tc">
                <asp:RequiredFieldValidator 
                    ID="UserNameRequired" 
                    runat="server" 
                    ControlToValidate="UserName"
                    ErrorMessage="<%$Resources:Messages,Register_UserNameRequired %>" 
                    ToolTip="<%$Resources:Messages,Register_UserNameRequired %>" 
                    ValidationGroup="PasswordRecovery" 
                    />
                                            
                <tstsc:MessageBox 
                    ID="FailureText" 
                    runat="server" 
                    SkinID="MessageBox"
                    />
                <h1 class="ma0 fw-b mt5 mt4-xs mb4 blue-strong">  
                    <asp:Localize 
                        ID="Localize1" 
                        runat="server" 
                        Text="<%$Resources:Main,ResetPassword_ForgotYourPassword %>" 
                        />
                </h1>
                <p class="mt0 mb5">
                    <asp:Localize 
                        ID="Localize5" 
                        runat="server" 
                        Text="<%$Resources:Main,ResetPassword_EnterUserNameText %>" 
                        />                                        
                </p>

                <div class="my4 mx3 relative">
                    <tstsc:UnityTextBoxEx
                        CssClass="w-100 u-input u-input-minimal py3 px1 fs-110"
                        ID="UserName" 
                        MaxLength="50"
                        runat="server" 
                        TextMode="SingleLine" 
                        ClientIDMode="static"
                        />
                    <asp:Label 
                        AssociatedControlID="UserName" 
                        CssClass="label-slideup tl"
                        runat="server"
                        Text="<%$ Resources:Fields,UserName %>"
                        />
                </div>
                                
                <div class="btn-group">
                    <tstsc:ButtonEx 
                        ID="btnUsernameSubmit" 
                        runat="server" 
                        SkinID="ButtonPrimary" 
                        CommandName="Submit" 
                        Text="<%$Resources:Buttons,Submit %>"
                        ValidationGroup="PasswordRecovery" 
                        ClientIDMode="static"
                        />
                    <tstsc:ButtonEx 
                        ID="btnCancel" runat="server" 
                        Text="<%$Resources:Buttons,Cancel %>" 
                        CausesValidation="false"
                        OnClick="btnCancel_Click" 
                        />
                </div>
            </div>
        </UserNameTemplate>
    </tstsc:PasswordRecoveryEx>



    <script type="text/javascript">
        (function () {
            // disable or enable the button for the first form - username input
            var username = document.getElementById("UserName");
            var btnUsername = document.getElementById("btnUsernameSubmit");
            if (username) {
                checkUsernameButton();
                
                username.addEventListener("input", function () {
                    this.setAttribute('value', this.value)
                    checkUsernameButton();
                }, false);
            }

            function checkUsernameButton() {
                btnUsername.disabled = !username.value;
            }


            // disable or enable the button for the secret question and answer form
            var answer = document.getElementById("Answer");
            var btnQuestion = document.getElementById("btnQuestionSubmit");
            if (answer) {
                checkQuestionButton();

                answer.addEventListener("input", function () {
                    this.setAttribute('value', this.value)
                    checkQuestionButton();
                }, false);
            }

            function checkQuestionButton() {
                console.log(!answer.value)
                btnQuestion.disabled = !answer.value;
            }
        })();
    </script>
</asp:Content>
