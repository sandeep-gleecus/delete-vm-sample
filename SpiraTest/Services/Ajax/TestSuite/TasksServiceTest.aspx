<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TasksServiceTest.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Tasks Service JsUnit Test Suite</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="divComponent"></div>
	<asp:ScriptManager ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
      </Services>  
    </asp:ScriptManager>  
    <script language="JavaScript" src="/jsunit/app/jsUnitCore.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        //Test Methods
        var webServiceClass = 'Inflectra.SpiraTest.Web.Services.Ajax.TasksService';
        var userId = 2; //Fred Bloggs
        var projectId = 1;  //Library System
        var setUpPageStatus;
        var testCount = 5;  //Number of asynchronous tests to be run
        var testIndex;
        var retrieve_data;
        var retrieveNameDesc_data;
        var retrieveLookup_data;
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
            testComponent.retrieveLookup();
            testComponent.refresh();
            testComponent.retrievePaginationOptions();
        } 
        
        //Test that we can retrieve some data from the sorted list service
        function test_retrieve()
        {
            //Make sure that we have some data
            assertTrue ('No data returned', retrieve_data.length > 0);
            
            //Now make sure that we have the fields we expect in the first row (filters/sorts)
            assertEquals('currPage', 1, retrieve_data[0].currPage);
            assertEquals('pageCount', 3, retrieve_data[0].pageCount);
            assertEquals('sortProperty', 'TaskId', retrieve_data[0].sortProperty);
            assertEquals('sortAscending', true, retrieve_data[0].sortAscending);
            
            //Now make sure that we have the number of data rows we expect
            assertEquals ('Number rows', 15, retrieve_data.length-1);
            
            //Finally verify some of the data returned
            assertEquals('primaryKey', 1, retrieve_data[1].primaryKey);
            assertEquals('attachment', false, retrieve_data[1].attachment);
            assertEquals('Name', 'Develop new book entry screen', retrieve_data[1]['Fields']['Name'].textValue);
            assertEquals('ProgressId', 100, retrieve_data[1]['Fields']['ProgressId'].equalizerGreen);
            assertEquals('ProgressId', 0, retrieve_data[1]['Fields']['ProgressId'].equalizerGray);
            assertEquals('TaskStatusId', 'Completed', retrieve_data[1]['Fields']['TaskStatusId'].textValue);
            assertEquals('TaskPriorityId', '1 - Critical', retrieve_data[1]['Fields']['TaskPriorityId'].textValue);
            assertEquals('OwnerId', 'Fred Bloggs', retrieve_data[1]['Fields']['OwnerId'].textValue);
            assertEquals('ReleaseId', '1.0.0.0.0001', retrieve_data[1]['Fields']['ReleaseId'].textValue);
        }
        
        //Test that we can retrieve the full name and description of an artifact
        function test_retrieveNameDesc()
        {
            //Make sure that the name and description matches
            assertEquals ('Name/Desc', '<u>Develop new book entry screen</u><br />\nCreate a new dynamic page that allows the user to enter the details of a new book', retrieveNameDesc_data);
        }
        
        function test_retrieveLookup()
        {
            //Make sure that we get the list of projects minus the current project
            assertEquals ('ProjectName', 'Sample Application One', retrieveLookup_data['2']);
            assertEquals ('ProjectName', 'Sample Application Two', retrieveLookup_data['3']);
        }
        
        function test_refresh()
        {
            //Verify the artifact's standard data
            assertEquals ('primaryKey', 1, refresh_data.primaryKey);
            
            //Now verify the fields including any lookups
            assertEquals ('Name:Value', 'Develop new book entry screen', refresh_data.Fields['Name'].textValue);
            assertEquals ('TaskStatusId:Value', 'Completed', refresh_data.Fields['TaskStatusId'].textValue);
            assertEquals ('TaskStatusId:Required', true, refresh_data.Fields['TaskStatusId'].required);
            assertEquals ('TaskStatusId:Lookups', 'Not Started', refresh_data.Fields['TaskStatusId'].lookups['1']);         
            assertEquals ('TaskPriorityId:Value', '1 - Critical', refresh_data.Fields['TaskPriorityId'].textValue);
            assertEquals ('TaskPriorityId:Required', false, refresh_data.Fields['TaskPriorityId'].required);
            assertEquals ('TaskPriorityId:Lookups', '2 - High', refresh_data.Fields['TaskPriorityId'].lookups['2']);         
            assertEquals ('OwnerId:Value', 'Fred Bloggs', refresh_data.Fields['OwnerId'].textValue);
            assertEquals ('OwnerId:Required', false, refresh_data.Fields['OwnerId'].required);
            assertEquals ('OwnerId:Lookups', 'Joe P Smith', refresh_data.Fields['OwnerId'].lookups['3']);         
            assertEquals ('ReleaseId:Value', '1.0.0.0.0001', refresh_data.Fields['ReleaseId'].textValue);
            assertEquals ('ReleaseId:Required', false, refresh_data.Fields['ReleaseId'].required);
            assertEquals ('ReleaseId:Lookups', '1.0.0.0', refresh_data.Fields['ReleaseId'].lookups['1']);         
        }
        
        function test_retrievePaginationOptions()
        {
            //Make sure that we get the list of pagination options, including which one is currently selected
            assertEquals ('Pagination-15', 'true', retrievePaginationOptions_data['15']);
            assertEquals ('Pagination-30', 'false', retrievePaginationOptions_data['30']);
        }
        
        // The following component is used to interact with the server ajax web service methods
        
        Type.registerNamespace('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite');
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest = function(element)
        {
            this._element = element;
            Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest.initializeBase(this, [this._element]);
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest.prototype =
        {
            initialize : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest.callBaseMethod(this, 'initialize');
            },
            dispose : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest.callBaseMethod(this, 'dispose');
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
                var artifactId = 1; //Artifact being tested
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
            
            retrieveLookup : function ()
            {
                var operation = 'Project'; //Lookup being tested
                var webServiceMethod = webServiceClass + '.RetrieveLookup';
                var webServiceParams = '(userId, projectId, operation, Function.createDelegate(this, this.retrieveLookup_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveLookup_success : function (dataSource)
            {
                //Set the data variable
                retrieveLookup_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            refresh : function ()
            {
                var artifactId = 1; //Artifact being tested
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
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest.registerClass('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest', Sys.UI.Control);
        var testComponent = $create (Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TasksServiceTest, {}, {}, {}, document.getElementById('divComponent'));
        
    </script>
    </form>
</body>
</html>
