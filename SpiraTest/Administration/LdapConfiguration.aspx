<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="LdapConfiguration.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.LdapConfiguration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,LdapConfiguration_Title %>" />
                </h2>
                <p class="my4">
                    <asp:Localize runat="server" Text="<%$Resources:Main,LdapConfiguration_String1 %>" />
                    <%=ProductName%>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,LdapConfiguration_String2 %>" />
                    <%=ProductName%>
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,LdapConfiguration_String3 %>" />
                    <%=ProductName%>
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,LdapConfiguration_String4 %>" />
                    <%=ProductName%>, <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,LdapConfiguration_String5 %>" />
                </p>
                <tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" />
                <p class="my4">
                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,LdapConfiguration_String6 %>" />
                </p>
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend><asp:Localize runat="server" Text="<%$Resources:Main,GeneralSettings_Title %>" /></legend>
                <div class="col-sm-12">
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_LdapHost %>" Required="true" ID="txtLdapHostLabel" AssociatedControlID="txtLdapHost" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtLdapHost" runat="server" Width="100%" CssClass="text-box" /><br />
                            <p class="help-block"><asp:Localize runat="server" Text="<%$Resources:Main,LdapConfiguration_LdapHostNotes %>" />
                                (e.g. myserver, myserver:389 or myserver:636)</p>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_UseSSL %>" ID="chkLdapUseSSLLabel" AssociatedControlID="chkLdapUseSSL" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:CheckBoxYnEx ID="chkLdapUseSSL" runat="server"  />
                        </div>
                    </div> 
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">                        
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_BaseDn %>" Required="true" ID="txtLdapBaseDnLabel" AssociatedControlID="txtLdapBaseDn" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtLdapBaseDn" runat="server" /><br />
                            <p class="help-block"><asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,LdapConfiguration_BaseDnNotes %>" /> (e.g. CN=Users,DC=MyCompany,DC=Com)</p>
                         </div>
                    </div> 
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">   
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_BindDn %>" Required="false" ID="txtLdapBindDnLabel" AssociatedControlID="txtLdapBindDn" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9 data-very-wide">
                            <tstsc:TextBoxEx ID="txtLdapBindDn" runat="server" /><br />
                            <p class="help-block"><asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,LdapConfiguration_BindDnNotes1 %>" /> (e.g.
                                CN=Fred Bloggs,CN=Users,DC=MyCompany,DC=Com) <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Main,LdapConfiguration_BaseDnNotes %>" /><asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Main,LdapConfiguration_BindDnNotes2 %>" /></p>
                        </div>
                    </div> 
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_BindPassword %>" Required="false" ID="txtLdapBindPasswordLabel" AssociatedControlID="txtLdapBindPassword" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtLdapBindPassword" runat="server" TextMode="SingleLine" autocomplete="off" Type="Password" /><br />
                            <p class="help-block"><asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Main,LdapConfiguration_BindPasswordNotes %>" /></p>
                        </div>
                    </div> 
                </div>
            </fieldset>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend>
                    <asp:Localize ID="Localize11" runat="server" Text="<%$Resources:Main,LdapConfiguration_Attributes %>" />
                </legend>
                <div class="col-md-6">
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_Login %>" Required="true" ID="txtLdapLoginLabel" AssociatedControlID="txtLdapLogin" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtLdapLogin" runat="server" /><br />
                            <p class="help-block"><asp:Localize ID="Localize12" runat="server" Text="<%$Resources:Main,LdapConfiguration_LoginNotes %>" />
                                (e.g. uid or sAMAccountName for ActiveDirectory)</p>
                        </div>
                    </div> 
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_EmailAddress %>" Required="false" ID="txtLdapEmailAddressLabel" AssociatedControlID="txtLdapEmailAddress" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtLdapEmailAddress" runat="server" /><br />
                            <p class="help-block"><asp:Localize ID="Localize16" runat="server" Text="<%$Resources:Main,LdapConfiguration_EmailAddressNotes %>" /></p>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_FirstName %>" Required="false" ID="txtLdapFirstNameLabel" AssociatedControlID="txtLdapFirstName" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtLdapFirstName" runat="server" /><br />
                            <p class="help-block"><asp:Localize ID="Localize13" runat="server" Text="<%$Resources:Main,LdapConfiguration_FirstNameNotes %>" /></p>
                        </div>
                    </div> 
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_MiddleInitial %>" Required="false" ID="txtLdapMiddleInitialLabel" AssociatedControlID="txtLdapMiddleInitial" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtLdapMiddleInitial" runat="server" SkinID="NarrowPlusFormControl" /><br />
                            <p class="help-block"><asp:Localize ID="Localize15" runat="server" Text="<%$Resources:Main,LdapConfiguration_MiddleInitialNotes %>" /></p>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-4 col-lg-3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_LastName %>" Required="false" ID="txtLdapLastNameLabel" AssociatedControlID="txtLdapLastName" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-8">
                            <tstsc:TextBoxEx ID="txtLdapLastName" runat="server" Width="100%" CssClass="text-box" /><br />
                            <p class="help-block"><asp:Localize ID="Localize14" runat="server" Text="<%$Resources:Main,LdapConfiguration_LastNameNotes %>" /></p>
                        </div>
                    </div> 
                </div>
            </fieldset>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <fieldset class="col-sm-11 fieldset-gray">
                <legend>
                    <asp:Localize ID="Localize21" runat="server" Text="<%$Resources:Main,LdapConfiguration_SampleUser %>" />
                </legend>
                <p>
                    <asp:Localize ID="Localize19" runat="server" Text="<%$Resources:Main,LdapConfiguration_String7 %>" />
                    <%=ProductName%>
                    <asp:Localize ID="Localize20" runat="server" Text="<%$Resources:Main,LdapConfiguration_String8 %>" />
                </p>
                <div class="col-sm-12">
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_SampleUser %>" Required="false" ID="txtLdapSampleUserLabel" AssociatedControlID="txtLdapSampleUser" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtLdapSampleUser" runat="server" Width="100%" CssClass="text-box" /><br />
                            <p class="help-block"><asp:Localize ID="Localize18" runat="server" Text="<%$Resources:Main,LdapConfiguration_SampleUserNotes %>" /> (e.g. CN=Fred
                                Bloggs,CN=Users,DC=MyCompany,DC=Com)</p>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="DataLabel col-sm-3 col-lg-2">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LdapConfiguration_SamplePassword %>" Required="false" ID="txtLdapSamplePasswordLabel" AssociatedControlID="txtLdapSamplePassword" AppendColon="true" />
                        </div>
                        <div class="DataEntry col-sm-9">
                            <tstsc:TextBoxEx ID="txtLdapSamplePassword" runat="server" Width="500px" CssClass="text-box"
                                TextMode="Password" /><br />
                        </div>
                    </div> 
                </div>
            </fieldset>
        </div>
        <div class="row">
            <div class="col-sm-11">
                <tstsc:ButtonEx ID="btnLdapUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
            </div>
        </div>
    </div>
</asp:Content>
