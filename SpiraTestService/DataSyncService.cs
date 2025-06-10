using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Inflectra.SpiraTest.PlugIns;

using Inflectra.SpiraTest.DataSyncService.SpiraDataSync;

namespace Inflectra.SpiraTest.DataSyncService
{
    partial class DataSyncService : ServiceBase
    {
        private static TimerState timerState;
        private static EventLog eventLog = new EventLog();
        private static bool traceLogging = false;
        private static long pollingInterval = 120000;    // 2 minutes default
        private const string WEB_SERVICE_URL_SUFFIX = "/Services/v4_0/DataSync.svc";

        public DataSyncService()
        {
            //Call the build-in designer support code
            InitializeComponent();

            //Create an event log if one does not already exist
            if (!System.Diagnostics.EventLog.SourceExists("Application"))
            {
                System.Diagnostics.EventLog.CreateEventSource(Properties.Settings.Default.EventLogSource, "Application");
            }

            eventLog.Source = Properties.Settings.Default.EventLogSource;
            eventLog.Log = "Application";
            traceLogging = Properties.Settings.Default.TraceLogging;
        }

        // The main entry point for the process
        static void Main()
        {
            //Get the configuration information
            pollingInterval = Properties.Settings.Default.PollingInterval;

            //
            // create timer state
            //
            timerState = new TimerState();
            System.ServiceProcess.ServiceBase[] ServicesToRun;

            //Register the services that are supported by this process
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new DataSyncService() };

            //Execute the service
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }

        /// <summary>
        /// Called when the service is started
        /// </summary>
        /// <param name="args">List of startup parameters</param>
        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Start " + Properties.Settings.Default.EventLogSource, EventLogEntryType.Information);

            //
            // Create the delegate that invokes methods for the timer.
            //
            timerState.interval = pollingInterval; // 1000 = one second
            TimerCallback timerDelegate = new TimerCallback(CheckStatus);
            System.Threading.Timer timer = new System.Threading.Timer(timerDelegate, timerState, 1000, timerState.interval);
            //
            // Keep a handle to the timer, so it can be disposed.
            //
            timerState.timer = timer;
        }

        /// <summary>
        /// Called when the service is stopped
        /// </summary>
        protected override void OnStop()
        {
            eventLog.WriteEntry("Stop " + Properties.Settings.Default.EventLogSource, EventLogEntryType.Information);

            //Stop the timer and log the stopping of the service
            timerState.timer.Dispose();
            timerState.timer = null;
        }

        /// <summary>
        /// The following method is called by the timer's delegate. 
        /// </summary>
        /// <param name="state">The state of the timer</param>
        static void CheckStatus(Object state)
        {
            //
            // stop timer
            //
            timerState.timer.Change(System.Threading.Timeout.Infinite, timerState.interval);

            try
            {
                //The following should only be used for debugging purposes
                //eventLog.WriteEntry ("Timer Event Fired", EventLogEntryType.SuccessAudit);

                //Make sure we have a login and web server url specified
                if (String.IsNullOrEmpty(Properties.Settings.Default.WebServiceUrl))
                {
                    eventLog.WriteEntry("Unable to read web service URL from .config file", EventLogEntryType.Error);
                    //Leave timer stalled as we don't want endless messages
                    return;
                }
                if (String.IsNullOrEmpty(Properties.Settings.Default.Login))
                {
                    eventLog.WriteEntry("Unable to read web service login from .config file", EventLogEntryType.Error);
                    //Leave timer stalled as we don't want endless messages
                    return;
                }
                string urlBase = Properties.Settings.Default.WebServiceUrl;
                string login = Properties.Settings.Default.Login;
                string password = Properties.Settings.Default.Password;

                //The following should only be used for debugging purposes
                //eventLog.WriteEntry ("Looking for plug-ins to execute", EventLogEntryType.SuccessAudit);

                //Use the web-service to get the list of plug-ins configured
                SpiraDataSync.RemoteDataSyncSystem[] dataSyncSystems;
                SpiraDataSync.DataSyncClient spiraDataSync;
                try
                {
                    spiraDataSync = new Inflectra.SpiraTest.DataSyncService.SpiraDataSync.DataSyncClient();
                    Uri fullUri;
                    if (!Uri.TryCreate(urlBase + WEB_SERVICE_URL_SUFFIX, UriKind.Absolute, out fullUri))
                    {
                        eventLog.WriteEntry("The Server URL entered is not a valid URL (" + urlBase + ")\n", EventLogEntryType.Error);
                        //Leave timer stalled as we don't want endless messages
                        return;
                    }
                    spiraDataSync.Endpoint.Address = new EndpointAddress(fullUri.AbsoluteUri);

                    //Configure the HTTP Binding to handle cookies and HTTPS if appropriate
                    BasicHttpBinding httpBinding = (BasicHttpBinding)spiraDataSync.Endpoint.Binding;
                    httpBinding.AllowCookies = true;

                    //Handle SSL if necessary
                    if (fullUri.Scheme == "https")
                    {
                        httpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
                        httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                        //Allow self-signed certificates
                        PermissiveCertificatePolicy.Enact("");
                    }
                    else
                    {
                        httpBinding.Security.Mode = BasicHttpSecurityMode.None;
                    }

                    //Authenticate and get the list of systems
                    bool authenticated = spiraDataSync.Connection_Authenticate(login, password);
                    if (!authenticated)
                    {
                        //This is a permanent issue, so stop the timer and don't retry
                        eventLog.WriteEntry("Unable to authenticate with server to get list of data-synchronization plugins, please check the Login/Password in the DataSyncService.exe.config file and try again.", EventLogEntryType.Error);
                        return;
                    }
                    dataSyncSystems = spiraDataSync.DataSyncSystem_Retrieve();
                }
                catch (FaultException<ServiceFaultMessage> exception)
                {
                    if (exception.Detail.Equals(null))
                    {
                        eventLog.WriteEntry("Unable to get list of plug-ins to execute (" + exception.Message + ")\n" + exception.StackTrace, EventLogEntryType.Error);
                    }
                    else
                    {
                        ServiceFaultMessage faultMessage = exception.Detail;
                        eventLog.WriteEntry("Unable to get list of plug-ins to execute (" + faultMessage.Message + ")\n" + faultMessage.StackTrace, EventLogEntryType.Error);
                    }
                    //Restart timer in case error was temporary, however make the interval longer
                    timerState.interval = pollingInterval * 10;
                    timerState.timer.Change(timerState.interval, timerState.interval);
                    return;
                }
                catch (FaultException exception)
                {
                    eventLog.WriteEntry("Unable to get list of plug-ins to execute (" + exception.Message + ")\n" + exception.StackTrace, EventLogEntryType.Error);
                    //Restart timer in case error was temporary, however make the interval longer
                    timerState.interval = pollingInterval * 10;
                    timerState.timer.Change(timerState.interval, timerState.interval);
                    return;
                }

                //Get the current time on the server and locally
                DateTime localDateTime = DateTime.Now;  //Event log entries are NOT in UTC
                DateTime serverDateTime = spiraDataSync.System_GetServerDateTime();

                //Iterate through the plug-ins
                for (int i = 0; i < dataSyncSystems.Length; i++)
                {
                    SpiraDataSync.RemoteDataSyncSystem dataSyncSystem = dataSyncSystems[i];
                    try
                    {
                        //The filename is the same as the plug-in name
                        string plugInFileName = dataSyncSystem.Name;

                        //The following should only be used for debugging purposes
                        //eventLog.WriteEntry ("Found plug-in: " + plugInFileName, EventLogEntryType.SuccessAudit);

                        //Instantiate the data-sync class and execute the synchronization functionality
                        Type[] types = Assembly.Load(plugInFileName).GetExportedTypes();
                        bool classFound = false;
                        foreach (Type type in types)
                        {
                            if (typeof(IServicePlugIn).IsAssignableFrom(type))
                            {
                                classFound = true;
                                IServicePlugIn plugIn = (IServicePlugIn)Activator.CreateInstance(type);
                                using (plugIn)
                                {
                                    //Pass the setup information
                                    plugIn.Setup(
                                        eventLog,
                                        traceLogging,
                                        dataSyncSystem.DataSyncSystemId,
                                        urlBase,
                                        login,
                                        password,
                                        dataSyncSystem.ConnectionString,
                                        dataSyncSystem.Login,
                                        dataSyncSystem.Password,
                                        dataSyncSystem.TimeOffsetHours,
                                        dataSyncSystem.AutoMapUsers,
                                        dataSyncSystem.Custom01,
                                        dataSyncSystem.Custom02,
                                        dataSyncSystem.Custom03,
                                        dataSyncSystem.Custom04,
                                        dataSyncSystem.Custom05
                                        );

                                    //Now actually execute the data-synchronization
                                    //This interface expects the dates in localtime
                                    DateTime serverDateTimeLocal;
                                    //Force Dates to Local from UTC
                                    serverDateTime = DateTime.SpecifyKind(serverDateTime, DateTimeKind.Utc);
                                    serverDateTimeLocal = serverDateTime.ToLocalTime();

                                    DateTime? lastSyncDateLocal = null;
                                    if (dataSyncSystem.LastSyncDate.HasValue)
                                    {
                                        DateTime lastSyncDate = dataSyncSystem.LastSyncDate.Value;
                                        //Force Dates to Local from UTC
                                        lastSyncDate = DateTime.SpecifyKind(lastSyncDate, DateTimeKind.Utc);
                                        lastSyncDateLocal = lastSyncDate.ToLocalTime();
                                    }
                                    ServiceReturnType returnType = plugIn.Execute(lastSyncDateLocal, serverDateTimeLocal);

                                    //Update the last-run information
                                    //Reauthenticate to avoid timeout issues
                                    switch (returnType)
                                    {
                                        case ServiceReturnType.Success:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunSuccess(dataSyncSystem.DataSyncSystemId, serverDateTime);
                                            break;

                                        case ServiceReturnType.Warning:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunWarning(dataSyncSystem.DataSyncSystemId, serverDateTime);
                                            break;

                                        case ServiceReturnType.Error:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunFailure(dataSyncSystem.DataSyncSystemId);
                                            break;
                                    }

                                    //Now log any events in the event log since the last run
                                    try
                                    {
                                        var query = from e in eventLog.Entries.Cast<EventLogEntry>()
                                                    where e.Source == Properties.Settings.Default.EventLogSource && e.TimeWritten > localDateTime
                                                    select e;
                                        List<EventLogEntry> entries = query.ToList();
                                        foreach (EventLogEntry entry in entries)
                                        {
                                            string shortMessage;
                                            if (entry.Message.Length > 255)
                                            {
                                                shortMessage = entry.Message.Substring(0, 255);
                                            }
                                            else
                                            {
                                                shortMessage = entry.Message;
                                            }
                                            spiraDataSync.DataSyncSystem_WriteEvent(shortMessage, entry.Message, (int)entry.EntryType);
                                        }
                                    }
                                    catch
                                    {
                                        //Fail quietly
                                    }
                                }
                                //Ensure that the reference is killed (for garbage collection)
                                plugIn = null;
                            }
                            if (typeof(IDataSyncPlugIn).IsAssignableFrom(type))
                            {
                                classFound = true;
                                IDataSyncPlugIn plugIn = (IDataSyncPlugIn)Activator.CreateInstance(type);
                                using (plugIn)
                                {
                                    //Pass the setup information
                                    plugIn.Setup(
                                        eventLog,
                                        traceLogging,
                                        dataSyncSystem.DataSyncSystemId,
                                        urlBase,
                                        login,
                                        password,
                                        dataSyncSystem.ConnectionString,
                                        dataSyncSystem.Login,
                                        dataSyncSystem.Password,
                                        dataSyncSystem.TimeOffsetHours,
                                        dataSyncSystem.AutoMapUsers,
                                        dataSyncSystem.Custom01,
                                        dataSyncSystem.Custom02,
                                        dataSyncSystem.Custom03,
                                        dataSyncSystem.Custom04,
                                        dataSyncSystem.Custom05
                                        );

                                    //Now actually execute the data-synchronization
                                    //This interface expects the dates in UTC
                                    ServiceReturnType returnType = plugIn.Execute(dataSyncSystem.LastSyncDate, serverDateTime);

                                    //Update the last-run information
                                    //Reauthenticate to avoid timeout issues
                                    switch (returnType)
                                    {
                                        case ServiceReturnType.Success:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunSuccess(dataSyncSystem.DataSyncSystemId, serverDateTime);
                                            break;

                                        case ServiceReturnType.Warning:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunWarning(dataSyncSystem.DataSyncSystemId, serverDateTime);
                                            break;

                                        case ServiceReturnType.Error:
                                            spiraDataSync.Connection_Authenticate(login, password);
                                            spiraDataSync.DataSyncSystem_SaveRunFailure(dataSyncSystem.DataSyncSystemId);
                                            break;
                                    }

                                    //Now log any events in the event log since the last run
                                    try
                                    {
                                        var query = from e in eventLog.Entries.Cast<EventLogEntry>()
                                                    where e.Source == Properties.Settings.Default.EventLogSource && e.TimeWritten > localDateTime
                                                    select e;
                                        List<EventLogEntry> entries = query.ToList();
                                        foreach (EventLogEntry entry in entries)
                                        {
                                            string shortMessage;
                                            if (entry.Message.Length > 255)
                                            {
                                                shortMessage = entry.Message.Substring(0, 255);
                                            }
                                            else
                                            {
                                                shortMessage = entry.Message;
                                            }
                                            spiraDataSync.DataSyncSystem_WriteEvent(shortMessage, entry.Message, (int)entry.EntryType);
                                        }
                                    }
                                    catch
                                    {
                                        //Fail quietly
                                    }
                                }
                                //Ensure that the reference is killed (for garbage collection)
                                plugIn = null;
                            }
                        }
                        if (!classFound)
                        {
                            //Log that we couldn't find a matching class
                            spiraDataSync.Connection_Authenticate(login, password);
                            spiraDataSync.DataSyncSystem_SaveRunFailure(dataSyncSystem.DataSyncSystemId);
                            eventLog.WriteEntry("Unable to load plugin '" + plugInFileName + "' as it doesn't implement IServicePlugIn\n", EventLogEntryType.Error);
                            spiraDataSync.DataSyncSystem_WriteEvent("Unable to load plugin '" + plugInFileName + "' as it doesn't implement IServicePlugIn\n", "", (int)EventLogEntryType.Error);
                        }
                    }
                    catch (Exception exception)
                    {
                        //Log the error message and reset the timer to try again
                        spiraDataSync.Connection_Authenticate(login, password);
                        spiraDataSync.DataSyncSystem_SaveRunFailure(dataSyncSystem.DataSyncSystemId);
                        eventLog.WriteEntry(exception.Message + "\n" + exception.StackTrace, EventLogEntryType.Error);
                        spiraDataSync.DataSyncSystem_WriteEvent(exception.Message, exception.StackTrace, (int)EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception exception)
            {
                //Log the error message and ignore
                eventLog.WriteEntry(exception.Message + "\n" + exception.StackTrace, EventLogEntryType.Error);
            }

            //
            // restart timer
            //
            timerState.interval = pollingInterval;
            timerState.timer.Change(timerState.interval, timerState.interval);
        }
    }

    /// <summary>
    /// Keeps track of the timer state
    /// </summary>
    class TimerState
    {
        public int counter = 0;
        public System.Threading.Timer timer;
        public long interval = 0;
    }
}
