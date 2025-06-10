using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side artifact association AJAX components
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class GraphingService : AjaxWebServiceBase, IGraphingService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.GraphingService::";

        #region Custom Graphs

        /// <summary>
        /// Retrieves the graph data retrieved when executing the specified custom query
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="sql">The query to be executed</param>
        /// <returns>The Graph data</returns>
        /// <remarks>
        /// This command is used by general users to see the custom graphs published by system admins
        /// </remarks>
        public GraphData CustomGraph_Retrieve(int projectId, int graphId)
        {
            const string METHOD_NAME = "CustomGraph_RetrievePreview";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a member of the project
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {

                //Get the overall report dashboard release ID filter
                int? releaseId = null;
                int releaseIdValue = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (releaseIdValue > 0)
                {
                    releaseId = releaseIdValue;
                }

                //Get the project group id of the current project
                Project project = new ProjectManager().RetrieveById(projectId);
                int projectGroupId = project.ProjectGroupId;

                //Execute the query, passing the project id and project group id
                GraphManager graphManager = new GraphManager();
                GraphCustom graph = graphManager.GraphCustom_RetrieveById(graphId);
                DataTable dataTable = graphManager.GraphCustom_ExecuteSQL(projectId, projectGroupId, graphId, releaseId);

                //Create the graph data and populate from the XML, pass the name of the custom graph as the Options
                GraphData graphData = new GraphData();
                graphData.Options = graph.Name;

                //See if we have any data to display
                if (dataTable.Rows.Count == 0)
                {
                    //Display in a friendly format (means the query returnd data not suitable for graphing)
                    throw new DataValidationException(Resources.Messages.GraphingService_NoDataToGraph);
                }

                //Make sure we have at least two columns
                if (dataTable.Columns.Count < 2)
                {
                    //Display in a friendly format (means the query returnd data not suitable for graphing)
                    throw new DataValidationException(Resources.Messages.GraphingService_NeedAtLeastTwoColumns);
                }

                //We assume that the first column is the x-axis (category)
                DataColumn dataColumn = dataTable.Columns[0];
                graphData.XAxisCaption = dataColumn.Caption;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (dataRow[dataColumn] != DBNull.Value)
                    {
                        GraphAxisPosition axisItem = new GraphAxisPosition();
                        axisItem.Id = dataTable.Rows.IndexOf(dataRow);
                        axisItem.StringValue = dataRow[dataColumn].ToString();
                        graphData.XAxis.Add(axisItem);
                    }
                }

                //The subsequent columns are the data ranges
                for (int i = 1; i < dataTable.Columns.Count; i++)
                {
                    dataColumn = dataTable.Columns[i];
                    DataSeries dataSeries = new DataSeries();
                    dataSeries.Caption = dataColumn.Caption;
                    dataSeries.Name = dataColumn.ColumnName;
                    graphData.Series.Add(dataSeries);

                    //Now add the data to the series
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        int rowId = dataTable.Rows.IndexOf(dataRow);
                        decimal? decimalValue = null;
                        if (dataColumn.DataType == typeof(Int32))
                        {
                            decimalValue = (int)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Int64))
                        {
                            decimalValue = (long)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Int16))
                        {
                            decimalValue = (short)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Decimal))
                        {
                            decimalValue = (decimal)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Single))
                        {
                            decimalValue = (decimal)((float)dataRow[dataColumn]);
                        }
                        if (dataColumn.DataType == typeof(Double))
                        {
                            decimalValue = (decimal)((double)dataRow[dataColumn]);
                        }
                        if (decimalValue.HasValue)
                        {
                            dataSeries.Values.Add(rowId.ToString(), decimalValue.Value);
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return graphData;
            }
            catch (ArtifactNotExistsException)
            {
                //Display a more friendly message
                throw new DataValidationException(Resources.Messages.GraphingService_CustomGraphNoLongerExists);
            }
            catch (GraphDataInvalidException exception)
            {
                //Display in a friendly format (means the query returnd data not suitable for graphing)
                throw new DataValidationException(exception.Message);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the graph data retrieved when executing the specified custom query
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="sql">The query to be executed</param>
        /// <returns>The Graph data</returns>
        /// <remarks>
        /// This command is only used by sys-admins on the Graph administration screens
        /// </remarks>
        public GraphData CustomGraph_RetrievePreview(int projectId, string sql)
        {
            const string METHOD_NAME = "CustomGraph_RetrievePreview";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're a system administrator
            if (!this.UserIsAdmin)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Make sure we're a member of the project, if a project is specified
            if (projectId > 0)
            {
                Project.AuthorizationState authorizationState = IsAuthorized(projectId);
                if (authorizationState == Project.AuthorizationState.Prohibited)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }
            }

            try
            {
                Business.ProjectManager projectManager = new Business.ProjectManager();
                //If the user doesn't have a project selected, just grab the first active project that the user
                //is a member of
                if (projectId < 1)
                {
                    List<ProjectForUserView> projects = projectManager.RetrieveForUser(this.CurrentUserId.Value);
                    if (projects.Count > 0)
                    {
                        projectId = projects[0].ProjectId;
                    }
                }

                //Get the project group id if we have a project selected
                int projectGroupId = -1;
                if (projectId > 0)
                {
                    Project project = projectManager.RetrieveById(projectId);
                    projectGroupId = project.ProjectGroupId;
                }

                //Execute the query, passing the project id and project group id
                DataTable dataTable = new GraphManager().GraphCustom_ExecuteSQL(projectId, projectGroupId, sql);

                //Create the graph data and populate from the XML
                GraphData graphData = new GraphData();

                //Make sure we have at least two columns
                if (dataTable.Columns.Count < 2)
                {
                    //Display in a friendly format (means the query returnd data not suitable for graphing)
                    throw new DataValidationException(Resources.Messages.GraphingService_NeedAtLeastTwoColumns);
                }

                //We assume that the first column is the x-axis (category)
                DataColumn dataColumn = dataTable.Columns[0];
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (dataRow[dataColumn] != DBNull.Value)
                    {
                        GraphAxisPosition axisItem = new GraphAxisPosition();
                        axisItem.Id = dataTable.Rows.IndexOf(dataRow);
                        axisItem.StringValue = dataRow[dataColumn].ToString();
                        graphData.XAxis.Add(axisItem);
                    }
                }

                //The subsequent columns are the data ranges
                for (int i = 1; i < dataTable.Columns.Count; i++)
                {
                    dataColumn = dataTable.Columns[i];
                    DataSeries dataSeries = new DataSeries();
                    dataSeries.Caption = dataColumn.Caption;
                    dataSeries.Name = dataColumn.ColumnName;
                    graphData.Series.Add(dataSeries);

                    //Now add the data to the series
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        int rowId = dataTable.Rows.IndexOf(dataRow);
                        decimal? decimalValue = null;
                        if (dataColumn.DataType == typeof(Int32))
                        {
                            decimalValue = (int)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Int64))
                        {
                            decimalValue = (long)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Int16))
                        {
                            decimalValue = (short)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Decimal))
                        {
                            decimalValue = (decimal)dataRow[dataColumn];
                        }
                        if (dataColumn.DataType == typeof(Single))
                        {
                            decimalValue = (decimal)((float)dataRow[dataColumn]);
                        }
                        if (dataColumn.DataType == typeof(Double))
                        {
                            decimalValue = (decimal)((double)dataRow[dataColumn]);
                        }
                        if (decimalValue.HasValue)
                        {
                            dataSeries.Values.Add(rowId.ToString(), decimalValue.Value);
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return graphData;
            }
            catch (GraphDataInvalidException exception)
            {
                //Display in a friendly format (means the query returnd data not suitable for graphing)
                throw new DataValidationException(exception.Message);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Retrieves a date-range set of graphing data
        /// </summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dateRange">The date range (encoded as a string)</param>
        /// <param name="filters">Any graph-specific filters</param>
        /// <param name="graphId">The id of the graph being requested</param>
        /// <returns>The table of graphing data</returns>
        DataObjects.GraphData IGraphingService.RetrieveDateRange(int projectId, int graphId, string dateRange, JsonDictionaryOfStrings filters)
        {
            const string METHOD_NAME = "RetrieveDateRange";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //If no project Id, return null (happens when graph is minimized)
                if (projectId < 1)
                {
                    return null;
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the overall report dashboard release ID filter
                int? releaseId = null;
                int releaseIdValue = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (releaseIdValue > 0)
                {
                    releaseId = releaseIdValue;
                }

                //Create the graph data
                DataObjects.GraphData graphData = new DataObjects.GraphData();

                //Parse the date range (leave as empty if fails and retrieve function will just use the last month)
                DateRange parsedDateRange;
                DateRange.TryParse(dateRange, out parsedDateRange);

                //See if we have any graph specific filters
                int? incidentTypeId = null;
                int? testCaseTypeId = null;
                if (filters != null && filters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(filters);
                    if (deserializedFilters.ContainsKey("IncidentTypeId"))
                    {
                        incidentTypeId = (int)deserializedFilters["IncidentTypeId"];
                    }
                    if (deserializedFilters.ContainsKey("TestCaseTypeId"))
                    {
                        testCaseTypeId = (int)deserializedFilters["TestCaseTypeId"];
                    }
                }

                //If we have a date range that is 15 days or less, display daily
                Graph.ReportingIntervalEnum interval = Graph.ReportingIntervalEnum.Daily;
                graphData.Interval = "1 day";
                if (parsedDateRange.TimeSpan.HasValue && parsedDateRange.TimeSpan.Value.TotalDays > 15)
                {
                    //Aggregate the data by the day, but display by the week
                    graphData.Interval = "1 week";
                }

                //If we have an interval that is greated than 120 days, display monthly
                if (parsedDateRange.TimeSpan.HasValue && parsedDateRange.TimeSpan.Value.TotalDays > 120)
                {
                    //Aggregate the data by the week, but display by the month
                    interval = Graph.ReportingIntervalEnum.Weekly;
                    graphData.Interval = "1 month";
                }

                //Load in the graph data requested
                GraphManager graphManager = new GraphManager();
                DataSet graphDataSet = null;
                double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
                switch ((Graph.GraphEnum)graphId)
                {
                    case Graph.GraphEnum.IncidentProgressRate:
                        graphDataSet = graphManager.RetrieveIncidentProgress(projectId, interval, parsedDateRange, utcOffset, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.IncidentCumulativeCount:
                        graphDataSet = graphManager.RetrieveIncidentCumulativeCount(projectId, interval, parsedDateRange, utcOffset, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.IncidentOpenCount:
                        //Specify that the data is stacked
                        graphData.Options = "stacked";
                        graphDataSet = graphManager.RetrieveIncidentOpenCountByPriority(projectId, projectTemplateId, interval, parsedDateRange, utcOffset, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.IncidentCountByStatus:
                        //Specify that the data is stacked
                        graphData.Options = "stacked";
                        graphDataSet = graphManager.RetrieveIncidentCountByStatus(projectId, projectTemplateId, interval, parsedDateRange, utcOffset, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.TestRunProgressRate:
                        //Specify that the data is stacked
                        graphData.Options = "stacked";
                        graphDataSet = graphManager.RetrieveTestRunCountByExecutionStatus(projectId, interval, parsedDateRange, utcOffset, releaseId, testCaseTypeId);
                        break;

                    case Graph.GraphEnum.TestCasesStatusRate:
                        //Specify that the data is stacked
                        graphDataSet = graphManager.RetrieveTestCaseCountByExecutionStatus(projectId, interval, parsedDateRange, utcOffset, releaseId, testCaseTypeId);
                        break;

                    case Graph.GraphEnum.TestCasesStatusRateCumulative:
                        //Specify that the data is stacked
                        graphData.Options = "stacked";
                        graphDataSet = graphManager.RetrieveTestCaseCountByExecutionStatusCumulative(projectId, interval, parsedDateRange, utcOffset, releaseId, testCaseTypeId);
                        break;
                }

                if (graphDataSet == null)
                {
                    return null;
                }

                //Generate the number of different data-series
                DataTable dataTable = graphDataSet.Tables[0];
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    //We don't add the key columns as they are used for the axis
                    if (dataTable.PrimaryKey.Contains(dataColumn))
                    {
                        graphData.XAxisCaption = dataColumn.Caption;
                    }
                    else
                    {
                        DataObjects.DataSeries series = new DataObjects.DataSeries();
                        series.Name = dataColumn.ColumnName;
                        series.Caption = dataColumn.Caption;
                        //See if a color is included in the series
                        if (dataColumn.ExtendedProperties.ContainsKey("Color"))
                        {
                            series.Color = (string)dataColumn.ExtendedProperties["Color"];
                        }

                        graphData.Series.Add(series);
                    }
                }

                //Populate the data items
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    //First we need to add the actual data x-axis values
                    //Get the key column
                    DataColumn dataColumn = dataTable.PrimaryKey[0];
                    GraphAxisPosition axisPosition = new GraphAxisPosition();
                    axisPosition.Id = dataTable.Rows.IndexOf(dataRow);
                    axisPosition.StringValue = ((DateTime)dataRow[dataColumn]).ToShortDateString();
                    axisPosition.DateValue = ((DateTime)dataRow[dataColumn]);
                    graphData.XAxis.Add(axisPosition);

                    //Now add the data series
                    foreach (DataObjects.DataSeries series in graphData.Series)
                    {
                        if (dataRow[series.Name] != null)
                        {
                            object value = dataRow[series.Name];
                            if (value.GetType() == typeof(decimal))
                            {
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)value);
                            }
                            else if (value.GetType() == typeof(int))
                            {
                                int intValue = (int)value;
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
                            }
                        }
                    }
                }

                //For the test case cumulative, we need to make the Not Run status last to stack correctly
                if ((Graph.GraphEnum)graphId == Graph.GraphEnum.TestCasesStatusRateCumulative)
                {
                    DataSeries notRunSeries = graphData.Series.FirstOrDefault(s => s.Name == ((int)TestCase.ExecutionStatusEnum.NotRun).ToString());
                    if (notRunSeries != null)
                    {
                        graphData.Series.Remove(notRunSeries);
                        graphData.Series.Add(notRunSeries);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the WebPart settings from the JqPlot ajax control
        /// </summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="webPartUniqueId">The id of the webpart that the graph lives in</param>
        /// <param name="settings">The settings collection</param>
        void IGraphingService.UpdateSettings(int projectId, string webPartUniqueId, JsonDictionaryOfStrings settings)
        {
            const string METHOD_NAME = "UpdateSettings";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //If no project Id, do nothing
                if (projectId < 1)
                {
                    return;
                }

                //Get the settings
                Dictionary<string, object> deserializedSettings = GlobalFunctions.DeSerializeCollection(settings);

                //We need to access the WebPart manager
                WebPartManagerEx wpm = new WebPartManagerEx();
                wpm.DashboardName = "Reports";
                wpm.DashboardInstanceId = projectId;
                string webPartPath = wpm.GetPath();

                //We need to get the current user's username
                string userName = CurrentUserLogin;

                //We get the settings from the personalization provider
                SpiraWebPartPersonalizationProvider provider = new SpiraWebPartPersonalizationProvider();
                IDictionary userSettingsDictionary = provider.GetUserPersonalizationInfo(wpm, webPartPath, userName);
                bool webPartFound = false;
                foreach (DictionaryEntry entry in userSettingsDictionary)
                {
                    string controlId = (string)entry.Key;
                    if (controlId == webPartUniqueId)
                    {
                        webPartFound = true;
                        PersonalizationInfo info = (PersonalizationInfo)entry.Value;
                        IDictionary properties = info._properties;
                        //Logger.LogTraceEvent("DEBUGD1", (info._controlType == null) ? "N/A" : info._controlType.ToString());
                        //Logger.LogTraceEvent("DEBUGD2", (info._controlVPath == null) ? "N/A" : info._controlVPath.ToString());
                        //Logger.LogTraceEvent("DEBUGD3", info._isStatic.ToString());
                        if (properties == null)
                        {
                            //Create a new collection if necessary
                            info._properties = new HybridDictionary(false);
                            properties = info._properties;
                        }
                        //See if there is a match
                        foreach (KeyValuePair<string, object> kvp in deserializedSettings)
                        {
                            if (properties.Contains(kvp.Key))
                            {
                                properties[kvp.Key] = kvp.Value;
                            }
                            else
                            {
                                properties.Add(kvp.Key, kvp.Value);
                            }
                        }

                        //See if we need to remove any settings
                        //Don't let the system remove special system settings such as:
                        //GraphId
                        //ArtifactTypeId
                        List<object> propertiesToRemove = new List<object>();
                        foreach (DictionaryEntry property in properties)
                        {
                            if (property.Key is String)
                            {
                                string key = (string)property.Key;
                                if (!deserializedSettings.ContainsKey(key) && key != "GraphId" && key != "ArtifactTypeId")
                                {
                                    propertiesToRemove.Add(property.Key);
                                }
                            }
                        }
                        foreach (object key in propertiesToRemove)
                        {
                            properties.Remove(key);
                        }
                    }
                }

                //We may need to add a setting if the web part has not been modified yet
                if (!webPartFound)
                {
                    PersonalizationInfo info = new PersonalizationInfo();
                    info._controlID = webPartUniqueId;
                    info._properties = new HybridDictionary(false);
                    userSettingsDictionary.Add(webPartUniqueId, info);
                    //Add the new settings
                    foreach (KeyValuePair<string, object> kvp in deserializedSettings)
                    {
                        info._properties.Add(kvp.Key, kvp.Value);
                    }
                }

                //Update the settings
                provider.SetUserPersonalizationInfo(wpm, webPartPath, userName, userSettingsDictionary);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (ArtifactNotExistsException)
            {
                //Ignore and don't save settings (user does not exist)
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a the graphing data for a summary graph
        /// </summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="artifactTypeId">The type of artifact</param>
        /// <param name="xAxisField">The x-axis field</param>
        /// <param name="groupByField">The group-by field</param>
        /// <returns>The table of graphing data</returns>
        DataObjects.GraphData IGraphingService.RetrieveSummary(int projectId, int artifactTypeId, string xAxisField, string groupByField)
        {
            const string METHOD_NAME = "RetrieveSummary";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //If no project Id, return null (happens when graph is minimized)
                if (projectId < 1)
                {
                    return null;
                }

                //Get the overall report dashboard release ID filter
                int? releaseId = null;
                int releaseIdValue = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (releaseIdValue > 0)
                {
                    releaseId = releaseIdValue;
                }

                //Create the graph data
                DataObjects.GraphData graphData = new DataObjects.GraphData();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Load in the graph data requested
                GraphManager graphManager = new GraphManager();
                DataModel.Artifact.ArtifactTypeEnum artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;
                DataSet graphDataSet = graphManager.RetrieveSummaryCount(projectId, projectTemplateId, xAxisField, groupByField, artifactType, releaseId);

                if (graphDataSet == null)
                {
                    return null;
                }

                //Sort by x-axis (need to do in-memory)
                graphDataSet.Tables[0].DefaultView.Sort = "XAxis ASC";

                //Generate the number of different data-series
                DataTable dataTable = graphDataSet.Tables[0];
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    //We don't add the key columns as they are used for the axis
                    if (dataTable.PrimaryKey.Contains(dataColumn))
                    {
                        //See if we can localize the field name
                        if (String.IsNullOrEmpty(Resources.Fields.ResourceManager.GetString(dataColumn.Caption)))
                        {
                            graphData.XAxisCaption = dataColumn.Caption;
                        }
                        else
                        {
                            graphData.XAxisCaption = Resources.Fields.ResourceManager.GetString(dataColumn.Caption);
                        }
                    }
                    else
                    {
                        DataObjects.DataSeries series = new DataObjects.DataSeries();
                        series.Name = dataColumn.ColumnName;
                        series.Caption = dataColumn.Caption;
                        //See if a color is included in the series
                        if (dataColumn.ExtendedProperties.ContainsKey("Color"))
                        {
                            series.Color = (string)dataColumn.ExtendedProperties["Color"];
                        }

                        graphData.Series.Add(series);
                    }
                }

                //Populate the data items
                int i = 0;
                foreach (DataRowView dataRow in dataTable.DefaultView)
                {
                    //First we need to add the actual data x-axis values
                    //Get the key column
                    DataColumn dataColumn = dataTable.PrimaryKey[0];
                    GraphAxisPosition axisPosition = new GraphAxisPosition();
                    axisPosition.Id = i;
                    axisPosition.StringValue = (string)dataRow[dataColumn.ColumnName];
                    graphData.XAxis.Add(axisPosition);

                    //Now add the data series
                    foreach (DataObjects.DataSeries series in graphData.Series)
                    {
                        if (dataRow[series.Name] != null)
                        {
                            object value = dataRow[series.Name];
                            if (value.GetType() == typeof(decimal))
                            {
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)value);
                            }
                            else if (value.GetType() == typeof(int))
                            {
                                int intValue = (int)value;
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
                            }
                        }
                    }
                    i++;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the graphing data for one of the snapshot (not date-range) reports
        /// </summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="filters">Any graph-specific filters</param>
        /// <param name="graphId">The id of the graph being requested</param>
        /// <returns>The table of graphing data</returns>
        DataObjects.GraphData IGraphingService.RetrieveSnapshot(int projectId, int graphId, JsonDictionaryOfStrings filters)
        {
            const string METHOD_NAME = "RetrieveSnapshot";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //If no project Id, return null (happens when graph is minimized)
                if (projectId < 1)
                {
                    return null;
                }

                //Create the graph data
                DataObjects.GraphData graphData = new DataObjects.GraphData();

                //Get the overall report dashboard release ID filter
                int? releaseId = null;
                int releaseIdValue = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (releaseIdValue > 0)
                {
                    releaseId = releaseIdValue;
                }

                Nullable<int> incidentTypeId = null;
                if (filters != null && filters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(filters);
                    if (deserializedFilters.ContainsKey("IncidentTypeId"))
                    {
                        incidentTypeId = (int)deserializedFilters["IncidentTypeId"];
                    }
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Load in the graph data requested
                double utcOffset = GlobalFunctions.GetCurrentTimezoneUtcOffset();
                GraphManager graphManager = new GraphManager();
                DataSet graphDataSet = null;
                switch ((Graph.GraphEnum)graphId)
                {
                    case Graph.GraphEnum.RequirementCoverage:
                        graphDataSet = graphManager.Requirement_RetrieveCoverageByImportance(projectId, projectTemplateId, releaseId);
                        break;

                    case Graph.GraphEnum.IncidentAging:
                        graphDataSet = graphManager.RetrieveIncidentAgingByPriority(projectId, projectTemplateId, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.IncidentTurnaround:
                        graphDataSet = graphManager.RetrieveIncidentTurnaroundByPriority(projectId, projectTemplateId, releaseId, incidentTypeId, false);
                        break;

                    case Graph.GraphEnum.TaskVelocity:
                        graphDataSet = graphManager.RetrieveTaskVelocity(projectId, releaseId);
                        break;

                    case Graph.GraphEnum.TaskBurnDown:
                        graphDataSet = graphManager.RetrieveTaskBurndown(projectId, releaseId);
                        break;

                    case Graph.GraphEnum.TaskBurnUp:
                        graphDataSet = graphManager.RetrieveTaskBurnup(projectId, releaseId);
                        break;

                    case Graph.GraphEnum.RequirementVelocity:
                        graphDataSet = graphManager.Requirement_RetrieveVelocity(projectId, releaseId, utcOffset);
                        break;

                    case Graph.GraphEnum.RequirementBurnDown:
                        graphDataSet = graphManager.Requirement_RetrieveBurnDown(projectId, releaseId, utcOffset);
                        break;

                    case Graph.GraphEnum.RequirementBurnUp:
                        graphDataSet = graphManager.Requirement_RetrieveBurnUp(projectId, releaseId, utcOffset);
                        break;

					case Graph.GraphEnum.TestPreparationSummary:
						graphDataSet = graphManager.RetrieveTestPreparationSummary(projectId);
						break;
				}

                if (graphDataSet == null)
                {
                    return null;
                }

                //Generate the number of different data-series
                DataTable dataTable = graphDataSet.Tables[0];
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    //We don't add the key columns as they are used for the axis
                    if (dataTable.PrimaryKey.Contains(dataColumn))
                    {
                        graphData.XAxisCaption = dataColumn.Caption;
                    }
                    else
                    {
                        DataObjects.DataSeries series = new DataObjects.DataSeries();
                        series.Name = dataColumn.ColumnName;
                        series.Caption = dataColumn.Caption;
                        //See if a color is included in the series
                        if (dataColumn.ExtendedProperties.ContainsKey("Color"))
                        {
                            series.Color = (string)dataColumn.ExtendedProperties["Color"];
                        }

                        //See if a style of series is specified (defaults to bar if not specified)
                        series.Type = (int)Graph.GraphSeriesTypeEnum.Bar;
                        if (dataColumn.ExtendedProperties.ContainsKey("Type"))
                        {
                            series.Type = (int)dataColumn.ExtendedProperties["Type"];
                        }

                        graphData.Series.Add(series);
                    }
                }

                //Populate the data items
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    //First we need to add the actual data x-axis values
                    //Get the key column
                    DataColumn dataColumn = dataTable.PrimaryKey[0];
                    GraphAxisPosition axisPosition = new GraphAxisPosition();
                    axisPosition.Id = dataTable.Rows.IndexOf(dataRow);
                    axisPosition.StringValue = ((string)dataRow[dataColumn]);
                    graphData.XAxis.Add(axisPosition);

                    //Now add the data series
                    foreach (DataObjects.DataSeries series in graphData.Series)
                    {
                        if (dataRow[series.Name] != null)
                        {
                            object value = dataRow[series.Name];
                            if (value.GetType() == typeof(decimal))
                            {
                                series.Values.Add(axisPosition.Id.ToString(), Decimal.Round((decimal)value, 1));
                            }
                            else if (value.GetType() == typeof(double))
                            {
                                double doubleValue = (double)value;
                                series.Values.Add(axisPosition.Id.ToString(), Decimal.Round(new Decimal(doubleValue), 1));
                            }
                            else if (value.GetType() == typeof(int))
                            {
                                int intValue = (int)value;
                                series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
