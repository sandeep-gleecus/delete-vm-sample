﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace Inflectra.SpiraTest.ApiTestSuite.SpiraTestExecute12 {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="TestExecuteSoap", Namespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/")]
    public partial class TestExecute : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback RecordTestRun2OperationCompleted;
        
        private System.Threading.SendOrPostCallback RecordTestRunOperationCompleted;
        
        private System.Threading.SendOrPostCallback ConnectToProjectOperationCompleted;
        
        private System.Threading.SendOrPostCallback RetrieveServerDateTimeOperationCompleted;
        
        private System.Threading.SendOrPostCallback AuthenticateOperationCompleted;
        
        private System.Threading.SendOrPostCallback DisconnectOperationCompleted;
        
        private System.Threading.SendOrPostCallback RetrieveProjectListOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public TestExecute() {
            this.Url = "http://localhost/Spira/Services/v1_2_0/TestExecute.asmx";
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event RecordTestRun2CompletedEventHandler RecordTestRun2Completed;
        
        /// <remarks/>
        public event RecordTestRunCompletedEventHandler RecordTestRunCompleted;
        
        /// <remarks/>
        public event ConnectToProjectCompletedEventHandler ConnectToProjectCompleted;
        
        /// <remarks/>
        public event RetrieveServerDateTimeCompletedEventHandler RetrieveServerDateTimeCompleted;
        
        /// <remarks/>
        public event AuthenticateCompletedEventHandler AuthenticateCompleted;
        
        /// <remarks/>
        public event DisconnectCompletedEventHandler DisconnectCompleted;
        
        /// <remarks/>
        public event RetrieveProjectListCompletedEventHandler RetrieveProjectListCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/RecordTestRun2", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int RecordTestRun2(string userName, string password, int projectId, int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace) {
            object[] results = this.Invoke("RecordTestRun2", new object[] {
                        userName,
                        password,
                        projectId,
                        testerUserId,
                        testCaseId,
                        releaseId,
                        startDate,
                        endDate,
                        executionStatusId,
                        runnerName,
                        runnerTestName,
                        runnerAssertCount,
                        runnerMessage,
                        runnerStackTrace});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public void RecordTestRun2Async(string userName, string password, int projectId, int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace) {
            this.RecordTestRun2Async(userName, password, projectId, testerUserId, testCaseId, releaseId, startDate, endDate, executionStatusId, runnerName, runnerTestName, runnerAssertCount, runnerMessage, runnerStackTrace, null);
        }
        
        /// <remarks/>
        public void RecordTestRun2Async(string userName, string password, int projectId, int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace, object userState) {
            if ((this.RecordTestRun2OperationCompleted == null)) {
                this.RecordTestRun2OperationCompleted = new System.Threading.SendOrPostCallback(this.OnRecordTestRun2OperationCompleted);
            }
            this.InvokeAsync("RecordTestRun2", new object[] {
                        userName,
                        password,
                        projectId,
                        testerUserId,
                        testCaseId,
                        releaseId,
                        startDate,
                        endDate,
                        executionStatusId,
                        runnerName,
                        runnerTestName,
                        runnerAssertCount,
                        runnerMessage,
                        runnerStackTrace}, this.RecordTestRun2OperationCompleted, userState);
        }
        
        private void OnRecordTestRun2OperationCompleted(object arg) {
            if ((this.RecordTestRun2Completed != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RecordTestRun2Completed(this, new RecordTestRun2CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/RecordTestRun", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int RecordTestRun(int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace) {
            object[] results = this.Invoke("RecordTestRun", new object[] {
                        testerUserId,
                        testCaseId,
                        releaseId,
                        startDate,
                        endDate,
                        executionStatusId,
                        runnerName,
                        runnerTestName,
                        runnerAssertCount,
                        runnerMessage,
                        runnerStackTrace});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public void RecordTestRunAsync(int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace) {
            this.RecordTestRunAsync(testerUserId, testCaseId, releaseId, startDate, endDate, executionStatusId, runnerName, runnerTestName, runnerAssertCount, runnerMessage, runnerStackTrace, null);
        }
        
        /// <remarks/>
        public void RecordTestRunAsync(int testerUserId, int testCaseId, int releaseId, System.DateTime startDate, System.DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace, object userState) {
            if ((this.RecordTestRunOperationCompleted == null)) {
                this.RecordTestRunOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRecordTestRunOperationCompleted);
            }
            this.InvokeAsync("RecordTestRun", new object[] {
                        testerUserId,
                        testCaseId,
                        releaseId,
                        startDate,
                        endDate,
                        executionStatusId,
                        runnerName,
                        runnerTestName,
                        runnerAssertCount,
                        runnerMessage,
                        runnerStackTrace}, this.RecordTestRunOperationCompleted, userState);
        }
        
        private void OnRecordTestRunOperationCompleted(object arg) {
            if ((this.RecordTestRunCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RecordTestRunCompleted(this, new RecordTestRunCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/ConnectToProject", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool ConnectToProject(int projectId) {
            object[] results = this.Invoke("ConnectToProject", new object[] {
                        projectId});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void ConnectToProjectAsync(int projectId) {
            this.ConnectToProjectAsync(projectId, null);
        }
        
        /// <remarks/>
        public void ConnectToProjectAsync(int projectId, object userState) {
            if ((this.ConnectToProjectOperationCompleted == null)) {
                this.ConnectToProjectOperationCompleted = new System.Threading.SendOrPostCallback(this.OnConnectToProjectOperationCompleted);
            }
            this.InvokeAsync("ConnectToProject", new object[] {
                        projectId}, this.ConnectToProjectOperationCompleted, userState);
        }
        
        private void OnConnectToProjectOperationCompleted(object arg) {
            if ((this.ConnectToProjectCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ConnectToProjectCompleted(this, new ConnectToProjectCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/RetrieveServerDateTime", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.DateTime RetrieveServerDateTime() {
            object[] results = this.Invoke("RetrieveServerDateTime", new object[0]);
            return ((System.DateTime)(results[0]));
        }
        
        /// <remarks/>
        public void RetrieveServerDateTimeAsync() {
            this.RetrieveServerDateTimeAsync(null);
        }
        
        /// <remarks/>
        public void RetrieveServerDateTimeAsync(object userState) {
            if ((this.RetrieveServerDateTimeOperationCompleted == null)) {
                this.RetrieveServerDateTimeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRetrieveServerDateTimeOperationCompleted);
            }
            this.InvokeAsync("RetrieveServerDateTime", new object[0], this.RetrieveServerDateTimeOperationCompleted, userState);
        }
        
        private void OnRetrieveServerDateTimeOperationCompleted(object arg) {
            if ((this.RetrieveServerDateTimeCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RetrieveServerDateTimeCompleted(this, new RetrieveServerDateTimeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/Authenticate", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool Authenticate(string userName, string password) {
            object[] results = this.Invoke("Authenticate", new object[] {
                        userName,
                        password});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void AuthenticateAsync(string userName, string password) {
            this.AuthenticateAsync(userName, password, null);
        }
        
        /// <remarks/>
        public void AuthenticateAsync(string userName, string password, object userState) {
            if ((this.AuthenticateOperationCompleted == null)) {
                this.AuthenticateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAuthenticateOperationCompleted);
            }
            this.InvokeAsync("Authenticate", new object[] {
                        userName,
                        password}, this.AuthenticateOperationCompleted, userState);
        }
        
        private void OnAuthenticateOperationCompleted(object arg) {
            if ((this.AuthenticateCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AuthenticateCompleted(this, new AuthenticateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/Disconnect", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void Disconnect() {
            this.Invoke("Disconnect", new object[0]);
        }
        
        /// <remarks/>
        public void DisconnectAsync() {
            this.DisconnectAsync(null);
        }
        
        /// <remarks/>
        public void DisconnectAsync(object userState) {
            if ((this.DisconnectOperationCompleted == null)) {
                this.DisconnectOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDisconnectOperationCompleted);
            }
            this.InvokeAsync("Disconnect", new object[0], this.DisconnectOperationCompleted, userState);
        }
        
        private void OnDisconnectOperationCompleted(object arg) {
            if ((this.DisconnectCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DisconnectCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.inflectra.com/SpiraTest/Services/v1.2.0/RetrieveProjectList", RequestNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", ResponseNamespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ProjectData RetrieveProjectList() {
            object[] results = this.Invoke("RetrieveProjectList", new object[0]);
            return ((ProjectData)(results[0]));
        }
        
        /// <remarks/>
        public void RetrieveProjectListAsync() {
            this.RetrieveProjectListAsync(null);
        }
        
        /// <remarks/>
        public void RetrieveProjectListAsync(object userState) {
            if ((this.RetrieveProjectListOperationCompleted == null)) {
                this.RetrieveProjectListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRetrieveProjectListOperationCompleted);
            }
            this.InvokeAsync("RetrieveProjectList", new object[0], this.RetrieveProjectListOperationCompleted, userState);
        }
        
        private void OnRetrieveProjectListOperationCompleted(object arg) {
            if ((this.RetrieveProjectListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RetrieveProjectListCompleted(this, new RetrieveProjectListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3056.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/")]
    public partial class ProjectData {
        
        private ProjectRow[] projectField;
        
        /// <remarks/>
        public ProjectRow[] Project {
            get {
                return this.projectField;
            }
            set {
                this.projectField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3056.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.inflectra.com/SpiraTest/Services/v1.2.0/")]
    public partial class ProjectRow {
        
        private int projectIdField;
        
        private string nameField;
        
        private string descriptionField;
        
        private string websiteField;
        
        private int projectGroupIdField;
        
        private string projectGroupNameField;
        
        private string activeYnField;
        
        private System.DateTime creationDateField;
        
        /// <remarks/>
        public int ProjectId {
            get {
                return this.projectIdField;
            }
            set {
                this.projectIdField = value;
            }
        }
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        /// <remarks/>
        public string Website {
            get {
                return this.websiteField;
            }
            set {
                this.websiteField = value;
            }
        }
        
        /// <remarks/>
        public int ProjectGroupId {
            get {
                return this.projectGroupIdField;
            }
            set {
                this.projectGroupIdField = value;
            }
        }
        
        /// <remarks/>
        public string ProjectGroupName {
            get {
                return this.projectGroupNameField;
            }
            set {
                this.projectGroupNameField = value;
            }
        }
        
        /// <remarks/>
        public string ActiveYn {
            get {
                return this.activeYnField;
            }
            set {
                this.activeYnField = value;
            }
        }
        
        /// <remarks/>
        public System.DateTime CreationDate {
            get {
                return this.creationDateField;
            }
            set {
                this.creationDateField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void RecordTestRun2CompletedEventHandler(object sender, RecordTestRun2CompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RecordTestRun2CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RecordTestRun2CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public int Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void RecordTestRunCompletedEventHandler(object sender, RecordTestRunCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RecordTestRunCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RecordTestRunCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public int Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ConnectToProjectCompletedEventHandler(object sender, ConnectToProjectCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ConnectToProjectCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ConnectToProjectCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void RetrieveServerDateTimeCompletedEventHandler(object sender, RetrieveServerDateTimeCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RetrieveServerDateTimeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RetrieveServerDateTimeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.DateTime Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.DateTime)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void AuthenticateCompletedEventHandler(object sender, AuthenticateCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AuthenticateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal AuthenticateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void DisconnectCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void RetrieveProjectListCompletedEventHandler(object sender, RetrieveProjectListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RetrieveProjectListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RetrieveProjectListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ProjectData Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ProjectData)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591