using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.v2_2.DataObjects;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v2_2
{
    /// <summary>
    /// Used by the datasync service to retrieve the list of data sync systems and update the last run info
    /// </summary>
    [
    WebService(Namespace = "http://www.inflectra.com/SpiraTest/Services/v2.2/", Description = "Used by the datasync service to retrieve the list of data sync systems and update the last run info"),
    WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1),
    ToolboxItem(false)
    ]
    public class DataSync : WebServiceBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v2_2.DataSync::";

        /// <summary>
        /// Retrieves a list of data-sync plug-ins that need to be synchronized with
        /// </summary>
        /// <returns>The list of datasync plug-ins</returns>
        [
        WebMethod
            (
            Description = "Retrieves a list of data-sync plug-ins that need to be synchronized with",
            EnableSession = true
            )
        ]
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

                throw new SoapException("Session Not Authenticated",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            else
            {
                //We don't need to know the user as long as it authenticates since it can only
                //read the list of plug-ins which is not secure/private information

                //Get the list of data syncs in the system
                DataMappingManager dataMappingManager = new DataMappingManager();
                List<DataSyncSystem> dataMappings = dataMappingManager.RetrieveDataSyncSystems();

                //Populate the API data object and return
                List<RemoteDataSyncSystem> remoteDataSyncSystems = new List<RemoteDataSyncSystem>();
                foreach (DataSyncSystem dataSyncRow in dataMappings)
                {
                    //Create and populate the row
                    RemoteDataSyncSystem remoteDataSyncSystem = new RemoteDataSyncSystem();
                    PopulateDataSyncSystem(remoteDataSyncSystem, dataSyncRow);
                    remoteDataSyncSystems.Add(remoteDataSyncSystem);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return remoteDataSyncSystems;
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteDataSyncSystem">The API data object</param>
        /// <param name="dataSyncSystem">The internal datarow</param>
        protected void PopulateDataSyncSystem(RemoteDataSyncSystem remoteDataSyncSystem, DataSyncSystem dataSyncSystem)
        {
            remoteDataSyncSystem.DataSyncSystemId = dataSyncSystem.DataSyncSystemId;
            remoteDataSyncSystem.DataSyncStatusId = dataSyncSystem.DataSyncStatusId;
            remoteDataSyncSystem.Name = dataSyncSystem.Name;
            remoteDataSyncSystem.Description = dataSyncSystem.Description;
            remoteDataSyncSystem.ConnectionString = dataSyncSystem.ConnectionString;
            remoteDataSyncSystem.Login = dataSyncSystem.ExternalLogin;
            remoteDataSyncSystem.Password = dataSyncSystem.ExternalPassword;
            remoteDataSyncSystem.TimeOffsetHours = dataSyncSystem.TimeOffsetHours;
            remoteDataSyncSystem.LastSyncDate = GlobalFunctions.LocalizeDate(dataSyncSystem.LastSyncDate);
            remoteDataSyncSystem.Custom01 = dataSyncSystem.Custom01;
            remoteDataSyncSystem.Custom02 = dataSyncSystem.Custom02;
            remoteDataSyncSystem.Custom03 = dataSyncSystem.Custom03;
            remoteDataSyncSystem.Custom04 = dataSyncSystem.Custom04;
            remoteDataSyncSystem.Custom05 = dataSyncSystem.Custom05;
            remoteDataSyncSystem.AutoMapUsers = (dataSyncSystem.AutoMapUsersYn == "Y");
            remoteDataSyncSystem.DataSyncStatusName = dataSyncSystem.DataSyncStatusName;
        }

        /// <summary>
        /// Updates the status for a failed data-sync plug-in
        /// </summary>
        /// <param name="dataSyncSystemId">The id of the plug-in</param>
         [
        WebMethod
            (
            Description = "Updates the status for a failed data-sync plug-in",
            EnableSession = true
            )
        ]
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

                throw new SoapException("Session Not Authenticated",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
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
         [
        WebMethod
            (
            Description = "Updates the status for a successful data-sync plug-in",
            EnableSession = true
            )
        ]
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

                 throw new SoapException("Session Not Authenticated",
                     SoapException.ClientFaultCode,
                     Context.Request.Url.AbsoluteUri);
             }

             DataMappingManager dataMappingManager = new DataMappingManager();
             dataMappingManager.SaveRunSuccess(dataSyncSystemId, GlobalFunctions.UniversalizeDate(lastRunDate));

             Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
             Logger.Flush();
         }

         /// <summary>
         /// Updates the status for a data-sync plug-in that executed with warnings
         /// </summary>
         /// <param name="dataSyncSystemId">The id of the plug-in</param>
         /// <param name="lastRunDate">The date it last ran</param>
         [
        WebMethod
            (
            Description = "Updates the status for a data-sync plug-in that executed with warnings",
            EnableSession = true
            )
        ]
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

                 throw new SoapException("Session Not Authenticated",
                     SoapException.ClientFaultCode,
                     Context.Request.Url.AbsoluteUri);
             }

             DataMappingManager dataMappingManager = new DataMappingManager();
             dataMappingManager.SaveRunWarning(dataSyncSystemId, GlobalFunctions.UniversalizeDate(lastRunDate));

             Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
             Logger.Flush();
         }
    }
}
