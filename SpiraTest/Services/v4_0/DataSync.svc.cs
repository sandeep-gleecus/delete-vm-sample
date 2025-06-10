using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.v4_0.DataObjects;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
    /// <version>v4.0</version>
    public class DataSync : ServiceBase, IDataSync
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v4_0.DataSync::";

        /// <summary>
        /// Retrieves a list of data-sync plug-ins that need to be synchronized with
        /// </summary>
        /// <returns>The list of datasync plug-ins</returns>
        public List<RemoteDataSyncSystem> DataSyncSystem_Retrieve()
        {
            const string METHOD_NAME = "DataSyncSystem_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (!IsAuthenticated)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
            }
            else
            {
                //We don't need to know the user as long as it authenticates since it can only
                //read the list of plug-ins which is not secure/private information

                //Get the list of data syncs in the system
                DataMappingManager dataMappingManager = new DataMappingManager();
                List<DataSyncSystem> dataSyncSystems = dataMappingManager.RetrieveDataSyncSystems();

                //Populate the API data object and return
                List<RemoteDataSyncSystem> remoteDataSyncSystems = new List<RemoteDataSyncSystem>();
                foreach (DataSyncSystem dataSyncRow in dataSyncSystems)
                {
                    //Create and populate the row
                    RemoteDataSyncSystem remoteDataSyncSystem = new RemoteDataSyncSystem();
                    PopulationFunctions.PopulateDataSyncSystem(remoteDataSyncSystem, dataSyncRow);
                    remoteDataSyncSystems.Add(remoteDataSyncSystem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return remoteDataSyncSystems;
            }
        }

        /// <summary>
        /// Updates the status for a failed data-sync plug-in
        /// </summary>
        /// <param name="dataSyncSystemId">The id of the plug-in</param>
        public void DataSyncSystem_SaveRunFailure(int dataSyncSystemId)
        {
            const string METHOD_NAME = "DataSyncSystem_SaveRunFailure";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (!IsAuthenticated)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
            }

            DataMappingManager dataMappingManager = new DataMappingManager();
            dataMappingManager.SaveRunFailure(dataSyncSystemId);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the status for a successful data-sync plug-in
        /// </summary>
        /// <param name="dataSyncSystemId">The id of the plug-in</param>
        /// <param name="lastRunDate">The date it last ran</param>
        public void DataSyncSystem_SaveRunSuccess(int dataSyncSystemId, DateTime lastRunDate)
        {
            const string METHOD_NAME = "DataSyncSystem_SaveRunSuccess";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (!IsAuthenticated)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
            }

            DataMappingManager dataMappingManager = new DataMappingManager();
            dataMappingManager.SaveRunSuccess(dataSyncSystemId, lastRunDate);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the status for a data-sync plug-in that executed with warnings
        /// </summary>
        /// <param name="dataSyncSystemId">The id of the plug-in</param>
        /// <param name="lastRunDate">The date it last ran</param>
        public void DataSyncSystem_SaveRunWarning(int dataSyncSystemId, DateTime lastRunDate)
        {
            const string METHOD_NAME = "DataSyncSystem_SaveRunWarning";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (!IsAuthenticated)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
            }

            DataMappingManager dataMappingManager = new DataMappingManager();
            dataMappingManager.SaveRunWarning(dataSyncSystemId, lastRunDate);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Writes an error/warning event entry to the SpiraTeam database log
        /// </summary>
        /// <param name="details">The details of the error</param>
        /// <param name="message">The short error message</param>
        /// <param name="eventLogEntryType">The event log entry type</param>
        public void DataSyncSystem_WriteEvent(string message, string details, int eventLogEntryType)
        {
            try
            {
                Logger.WriteToDatabase("DataSync::DataSyncSystem_WriteEvent", message, details, (System.Diagnostics.EventLogEntryType)eventLogEntryType, null, Logger.EVENT_CATEGORY_DATA_SYNCHRONIZATION);
            }
            catch
            {
                //Do Nothing
            }
        }
    }
}
