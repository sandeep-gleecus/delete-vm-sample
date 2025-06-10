<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestCaseServiceTest.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Test Case Service JsUnit Test Suite</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="divComponent"></div>
	<asp:ScriptManager ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
      </Services>  
    </asp:ScriptManager>  
    <script language="JavaScript" src="/jsunit/app/jsUnitCore.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        //Test Methods
        var webServiceClass = 'Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService';
        var userId = 2; //Fred Bloggs
        var projectId = 1;  //Library System
        var setUpPageStatus;
        var testCount = 7;  //Number of asynchronous tests to be run
        var testIndex;
        var retrieve_data;
        var retrieveNameDesc_data;
        var retrieveLookup_Project_data;
        var retrieveLookup_Release_data;
        var retrieveLookup_TestSet_data;
        var refresh_data;
        var retrievePaginationOptions_data;
        
        //Called first when the page sets-up since we're testing asynchronous Ajax calls
        function setUpPage()
        {
            //Mark the page as running
            setUpPageStatus = 'running';
            testIndex = 0;
            
            //Call all of the test components
            testComponent.retrieve();
            testComponent.retrieveNameDesc();
            testComponent.retrieveLookup_Project();
            testComponent.retrieveLookup_Release();
            testComponent.retrieveLookup_TestSet();
            testComponent.refresh();
            testComponent.retrievePaginationOptions();
        } 
        
        //Test that we can retrieve some data from the hierarchical list service
        function test_retrieve()
        {
            //Make sure that we have some data
            assertTrue ('No data returned', retrieve_data.length > 0);
            
            //Now make sure that we have the fields we expect in the first row (filters)
            assertEquals('currPage', 1, retrieve_data[0].currPage);
            assertEquals('pageCount', 1, retrieve_data[0].pageCount);
            
            //Now make sure that we have the number of data rows we expect
            assertEquals ('Number rows', 14, retrieve_data.length-1);
            
            //Finally verify some of the data returned
            //Test Folder
            assertEquals('primaryKey', 1, retrieve_data[1].primaryKey);
            assertEquals('summary', true, retrieve_data[1].summary);
            assertEquals('expanded', true, retrieve_data[1].expanded);
            assertEquals('attachment', false, retrieve_data[1].attachment);
            assertEquals('alternate', false, retrieve_data[1].alternate);
            assertEquals('indent', 1, retrieve_data[1].indent);
            assertEquals('Name', 'Functional Tests', retrieve_data[1]['Fields']['Name'].textValue);
            assertEquals('ExecutionStatusId', '', retrieve_data[1]['Fields']['ExecutionStatusId'].textValue);
            assertEquals('ExecutionStatusId', 40, retrieve_data[1]['Fields']['ExecutionStatusId'].equalizerGreen);
            assertEquals('ExecutionStatusId', 20, retrieve_data[1]['Fields']['ExecutionStatusId'].equalizerRed);
            assertEquals('ExecutionStatusId', 20, retrieve_data[1]['Fields']['ExecutionStatusId'].equalizerOrange);
            assertEquals('ExecutionStatusId', 20, retrieve_data[1]['Fields']['ExecutionStatusId'].equalizerYellow);
            assertEquals('ExecutionStatusId', 0, retrieve_data[1]['Fields']['ExecutionStatusId'].equalizerGray);
           
            //Test Case With Steps
            assertEquals('primaryKey', 2, retrieve_data[2].primaryKey);
            assertEquals('summary', false, retrieve_data[2].summary);
            assertEquals('expanded', false, retrieve_data[2].expanded);
            assertEquals('attachment', true, retrieve_data[2].attachment);
            assertEquals('alternate', true, retrieve_data[2].alternate);
            assertEquals('indent', 2, retrieve_data[2].indent);
            assertEquals('Name', 'Ability to create new book', retrieve_data[2]['Fields']['Name'].textValue);
            assertEquals('ExecutionStatusId', 'Passed', retrieve_data[2]['Fields']['ExecutionStatusId'].textValue);
            assertEquals('OwnerId', 2, retrieve_data[2]['Fields']['OwnerId'].intValue);
            assertEquals('ExecutionDate', '1-Dec-2003', retrieve_data[2]['Fields']['ExecutionDate'].textValue);
            assertEquals('AuthorId', 2, retrieve_data[2]['Fields']['AuthorId'].intValue);
            assertEquals('ActiveYn', "Y", retrieve_data[2]['Fields']['ActiveYn'].textValue);
        }
        
        //Test that we can retrieve the full name and description of an artifact
        function test_retrieveNameDesc()
        {
            //Make sure that the name and description matches
            assertEquals ('Name/Desc', '<u>Ability to create new book</u><br />\nTests that the user can create a new book in the system', retrieveNameDesc_data);
        }
        
        function test_retrieveLookup_Project()
        {
            //Make sure that we get the list of projects minus the current project
            assertEquals ('ProjectName', 'Sample Application One', retrieveLookup_Project_data['2']);
            assertEquals ('ProjectName', 'Sample Application Two', retrieveLookup_Project_data['3']);
        }
     
        function test_retrieveLookup_Release()
        {
            //Make sure that we get the list of projects minus the current project
            assertEquals ('ReleaseName', '1.0.0.0 - Library System Release 1', retrieveLookup_Release_data['1']);
            assertEquals ('ReleaseName', '\u00a0\u00a01.0.1.0 - Library System Release 1 SP1', retrieveLookup_Release_data['2']);
        }
        
        function test_retrieveLookup_TestSet()
        {
            //Make sure that we get the list of projects minus the current project
            assertEquals ('TestSetName', 'Testing Cycle for Release 1.0', retrieveLookup_TestSet_data['1']);
            assertEquals ('TestSetName', 'Testing Cycle for Release 1.1', retrieveLookup_TestSet_data['2']);
        }
        
        function test_refresh()
        {
            //Verify the artifact's standard data
            assertEquals ('primaryKey', 2, refresh_data.primaryKey);
            assertEquals ('summary', false, refresh_data.summary);
            assertEquals ('expanded', false, refresh_data.expanded);
            assertEquals ('alternate', true, refresh_data.alternate);
            assertEquals ('indent', 2, refresh_data.indent);
            
            //Now verify the fields including any lookups
            assertEquals('Name:Value', 'Ability to create new book', refresh_data.Fields['Name'].textValue);
            assertEquals('ExecutionStatusId:Value', 'Passed', refresh_data.Fields['ExecutionStatusId'].textValue);
            assertEquals('ExecutionStatusId:Required', false, refresh_data.Fields['ExecutionStatusId'].required);
            assertEquals('OwnerId:Value', 2, refresh_data.Fields['OwnerId'].intValue);
            assertEquals('OwnerId:Required', false, refresh_data.Fields['OwnerId'].required);
            assertEquals('OwnerId:Lookups', 'Fred Bloggs', refresh_data.Fields['OwnerId'].lookups['2']);
            assertEquals('ExecutionDate:Value', '12/1/2003', refresh_data.Fields['ExecutionDate'].textValue);
            assertEquals('ExecutionDate:Editable', false, refresh_data.Fields['ExecutionDate'].editable);
            assertEquals('AuthorId:Value', 2, refresh_data.Fields['AuthorId'].intValue);
            assertEquals('AuthorId:Required', true, refresh_data.Fields['AuthorId'].required);
            assertEquals('AuthorId:Lookups', 'Fred Bloggs', refresh_data.Fields['AuthorId'].lookups['2']);
            assertEquals('ActiveYn:Value', "Y", refresh_data.Fields['ActiveYn'].textValue);
            assertEquals('ActiveYn:Required', true, refresh_data.Fields['ActiveYn'].required);
            assertEquals('ActiveYn:Lookups', 'Yes', refresh_data.Fields['ActiveYn'].lookups['Y']);
        }
        
        function test_retrievePaginationOptions()
        {
            //Make sure that we get the list of pagination options, including which one is currently selected
            assertEquals ('Pagination-15', 'true', retrievePaginationOptions_data['15']);
            assertEquals ('Pagination-30', 'false', retrievePaginationOptions_data['30']);
        }
        
        // The following component is used to interact with the server ajax web service methods
        
        Type.registerNamespace('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite');
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest = function(element)
        {
            this._element = element;
            Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.initializeBase(this, [this._element]);
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.prototype =
        {
            initialize : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.callBaseMethod(this, 'initialize');
            },
            dispose : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.callBaseMethod(this, 'dispose');
            },
            
            retrieve : function()
            {
                var webServiceMethod = webServiceClass + '.Retrieve';
                var webServiceParams = '(userId, projectId, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieve_success : function (dataSource)
            {
                //Set the data variable
                retrieve_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveNameDesc : function()
            {
                var artifactId = 2; //Artifact being tested
                var webServiceMethod = webServiceClass + '.RetrieveNameDesc';
                var webServiceParams = '(artifactId, Function.createDelegate(this, this.retrieveNameDesc_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveNameDesc_success : function (dataSource)
            {
                //Set the data variable
                retrieveNameDesc_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveLookup_Project : function ()
            {
                var operation = 'Project'; //Lookup being tested
                var webServiceMethod = webServiceClass + '.RetrieveLookup';
                var webServiceParams = '(userId, projectId, operation, Function.createDelegate(this, this.retrieveLookup_Project_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveLookup_Project_success : function (dataSource)
            {
                //Set the data variable
                retrieveLookup_Project_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveLookup_Release : function ()
            {
                var operation = 'Release'; //Lookup being tested
                var webServiceMethod = webServiceClass + '.RetrieveLookup';
                var webServiceParams = '(userId, projectId, operation, Function.createDelegate(this, this.retrieveLookup_Release_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveLookup_Release_success : function (dataSource)
            {
                //Set the data variable
                retrieveLookup_Release_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveLookup_TestSet : function ()
            {
                var operation = 'TestSet'; //Lookup being tested
                var webServiceMethod = webServiceClass + '.RetrieveLookup';
                var webServiceParams = '(userId, projectId, operation, Function.createDelegate(this, this.retrieveLookup_TestSet_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveLookup_TestSet_success : function (dataSource)
            {
                //Set the data variable
                retrieveLookup_TestSet_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            refresh : function ()
            {
                var artifactId = 2; //Artifact being tested
                var webServiceMethod = webServiceClass + '.Refresh';
                var webServiceParams = '(userId, projectId, artifactId, Function.createDelegate(this, this.refresh_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            refresh_success : function (dataSource)
            {
                //Set the data variable
                refresh_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrievePaginationOptions : function ()
            {
                var webServiceMethod = webServiceClass + '.RetrievePaginationOptions';
                var webServiceParams = '(userId, projectId, Function.createDelegate(this, this.retrievePaginationOptions_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrievePaginationOptions_success : function (dataSource)
            {
                //Set the data variable
                retrievePaginationOptions_data = dataSource;
                //Update the count
                this.updateStatus();
            },            
            
            //Generic functions used by all test methods
            failure : function (exception)
            {
                //Log the failure message
                retrieve_data = null;
                setUpPageStatus = 'complete';
            },
            updateStatus : function ()
            {
                testIndex++;
                if (testIndex >= testCount)
                {
                    setUpPageStatus = 'complete';
                }
            }
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.registerClass('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest', Sys.UI.Control);
        var testComponent = $create (Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest, {}, {}, {}, document.getElementById('divComponent'));
        
    </script>
    </form>
</body>
</html>
