using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Globalization;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Allows the graphing components to download the data-grid in CSV format
    /// </summary>
    public class GraphDataDownload : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.GraphDataDownload::";

        public override void ProcessRequest(HttpContext context)
        {
            //Call the base class functionality
            base.ProcessRequest(context);

            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to get the project id
                int projectId;
                if (Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID], out projectId))
                {
                    //Get the current user id
                    if (context.User == null || context.User.Identity == null || !CurrentUserId.HasValue)
                    {
                        //Not authenticated
                        return;
                    }

                    //Make sure the user is authorized to view data in this project
                    ProjectManager projectManager = new ProjectManager();
                    ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, CurrentUserId.Value);
                    if (projectUser == null)
                    {
                        //Not authorized, display nothing
                        return;
                    }

                    //Now get the graph id, unless we have a custom graph ID specified
                    int graphId;
                    if (!String.IsNullOrEmpty(context.Request.QueryString[GlobalFunctions.PARAMETER_CUSTOM_GRAPH_ID]) && Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_CUSTOM_GRAPH_ID], out graphId))
                    {
                        //Get the name of the graph
                        GraphManager graphManager = new GraphManager();
                        GraphCustom graph = graphManager.GraphCustom_RetrieveById(graphId);
                        string graphName = graph.Name;
                        IGraphingService graphingService = new GraphingService();
                        GraphData graphData = graphingService.CustomGraph_Retrieve(projectId, graphId);

                        //Format into CSV
                        if (graphData != null)
                        {
                            context.Response.ContentType = "text/csv";
                            //Can't use context.Response.AddHeader because we need to support IIS6 classic pipeline as well
                            //as IIS7 integrated pipeline
                            context.Response.AddHeader("Content-Type", "text/csv");
                            context.Response.AddHeader("Content-Disposition", "attachment; filename=data.csv");

                            //First the name of the graph
                            context.Response.Write(graphName + "\n");

                            //Next the various columns
                            context.Response.Write("\"" + graphData.XAxisCaption + "\"");
                            foreach (DataSeries dataSeries in graphData.Series)
                            {
                                context.Response.Write(",\"" + dataSeries.Caption + "\"");
                            }
                            context.Response.Write("\n");

                            //Now the data
                            foreach (GraphAxisPosition pos in graphData.XAxis)
                            {
                                context.Response.Write(pos.StringValue);
                                foreach (DataSeries dataSeries in graphData.Series)
                                {
                                    context.Response.Write("," + dataSeries.Values[pos.Id.ToString()]);
                                }
                                context.Response.Write("\n");
                            }
                        }
                    }
                    else if (Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_GRAPH_ID], out graphId))
                    {
                        //Get the name of the graph
                        GraphManager graphManager = new GraphManager();
                        Graph graph = graphManager.RetrieveById(graphId);
                        string graphName = graph.Name;

                        //Check the artifact permissions, return null if not authorized
                        Artifact.ArtifactTypeEnum artifactType = (Artifact.ArtifactTypeEnum)graph.ArtifactTypeId;
                        if (artifactType != Artifact.ArtifactTypeEnum.None && projectManager.IsAuthorized(projectUser.ProjectRoleId, artifactType, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
                        {
                            //Not authorized, display nothing
                            return;
                        }

                        //See what type of graph we have and make the appropriate call to the ajax web service function
                        Inflectra.SpiraTest.Web.Services.Ajax.DataObjects.GraphData graphData = null;
                        if (graph.GraphTypeId == (int)Graph.GraphTypeEnum.DateRangeGraphs)
                        {
                            string dateRange = context.Request.QueryString["dateRange"];
                            //We actually call the AJAX web service from the handler so that we don't have to deal with
                            //converting the dataset and interpreting which type of graph we have

                            JsonDictionaryOfStrings filters = new JsonDictionaryOfStrings();
                            //See if we have any filters (they are already json serialized)
                            foreach (string key in context.Request.QueryString)
                            {
                                if (key.ToLowerInvariant() != GlobalFunctions.PARAMETER_GRAPH_ID.ToLowerInvariant() && key.ToLowerInvariant() != GlobalFunctions.PARAMETER_PROJECT_ID.ToLowerInvariant() && key.ToLowerInvariant() != "culturename" && key.ToLowerInvariant() != "daterange")
                                {
                                    filters.Add(key, context.Request.QueryString[key].Trim());
                                }
                            }
                            IGraphingService graphingService = new GraphingService();
                            graphData = graphingService.RetrieveDateRange(projectId, graphId, dateRange, filters);
                        }
                        if (graph.GraphTypeId == (int)Graph.GraphTypeEnum.SummaryGraphs)
                        {
                            int artifactTypeId;
                            if (Int32.TryParse(context.Request.QueryString["artifactTypeId"], out artifactTypeId))
                            {
                                string xAxisField = context.Request.QueryString["xAxisField"];
                                string groupByField = context.Request.QueryString["groupByField"];
                                //We actually call the AJAX web service from the handler so that we don't have to deal with
                                //converting the dataset and interpreting which type of graph we have
                                IGraphingService graphingService = new GraphingService();
                                graphData = graphingService.RetrieveSummary(projectId, artifactTypeId, xAxisField, groupByField);
                            }
                        }
                        if (graph.GraphTypeId == (int)Graph.GraphTypeEnum.SnapshotGraphs)
                        {
                            //We actually call the AJAX web service from the handler so that we don't have to deal with
                            //converting the dataset and interpreting which type of graph we have
                            JsonDictionaryOfStrings filters = new JsonDictionaryOfStrings();
                            //See if we have any filters (they are already json serialized)
                            foreach (string key in context.Request.QueryString)
                            {
                                if (key.ToLowerInvariant() != GlobalFunctions.PARAMETER_GRAPH_ID.ToLowerInvariant() && key.ToLowerInvariant() != GlobalFunctions.PARAMETER_PROJECT_ID.ToLowerInvariant() && key.ToLowerInvariant() != "culturename")
                                {
                                    filters.Add(key, context.Request.QueryString[key].Trim());
                                }
                            }
                            IGraphingService graphingService = new GraphingService();
                            graphData = graphingService.RetrieveSnapshot(projectId, graphId, filters);
                        }
                        if (graphData != null)
                        {
                            //Format into CSV
                            context.Response.ContentType = "text/csv";
                            //Can't use context.Response.AddHeader because we need to support IIS6 classic pipeline as well
                            //as IIS7 integrated pipeline
                            context.Response.AddHeader("Content-Type", "text/csv");
                            context.Response.AddHeader("Content-Disposition", "attachment; filename=data.csv");

                            //First the name of the graph
                            context.Response.Write(graphName + "\n");

                            //Next the various columns
                            context.Response.Write("\"" + graphData.XAxisCaption + "\"");
                            foreach (DataSeries dataSeries in graphData.Series)
                            {
                                context.Response.Write(",\"" + dataSeries.Caption + "\"");
                            }
                            context.Response.Write("\n");

                            //Now the data
                            foreach (GraphAxisPosition pos in graphData.XAxis)
                            {
                                context.Response.Write(pos.StringValue);
                                foreach (DataSeries dataSeries in graphData.Series)
                                {
                                    context.Response.Write("," + dataSeries.Values[pos.Id.ToString()]);
                                }
                                context.Response.Write("\n");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();

                //Display the error message on the screen
                context.Response.Write(exception.Message);
            }
        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}