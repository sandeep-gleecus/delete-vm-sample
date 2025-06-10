using System;
using System.Web;
using System.Web.Management;
using System.Configuration.Provider;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.DataAccess;
using System.Web.Util;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Provides an interface on the generic logging mechanism in SpiraTest for web events that uses the Microsoft
    /// web event logging mechanism. Code in Business should directly use the Logger class located in the Common assembly
    /// </summary>
    public class SpiraEventProvider : BufferedWebEventProvider
    {
        int _commandTimeout = -1;
        int _connectionCount = 0;

        public SpiraEventProvider() : base()
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            try
            {
                // Remove Debug.Trace from sample Debug.Trace("SqlWebEventProvider", "EventBufferFlush called: " +
                Logger.WriteToDatabase(flushInfo.Events, flushInfo.EventsDiscardedSinceLastNotification,
                    flushInfo.LastNotificationUtc);
            }
            catch
            {
                //Catch everything quietly so that we don't get the function accidentally calling itself in an infinite loop!
            }
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            try
            {
                if (UseBuffering)
                {
                    base.ProcessEvent(eventRaised);
                }
                else
                {
                    Logger.WriteToDatabase(new SpiraWebBaseEventCollection(eventRaised), 0, new DateTime(0));
                }
            }
            catch
            {
                //Catch everything quietly so that we don't get the function accidentally calling itself in an infinite loop!
            }
        }

        public override void Shutdown()
        {
            try
            {
                Flush();
            }
            finally
            {
                base.Shutdown();
            }

            // Need to wait until all connections are gone before returning here
            // Sleep for 2x the command timeout in 1 sec intervals then give up, default timeout is 30 sec
            if (_connectionCount > 0)
            {
                int sleepAttempts = _commandTimeout * 2;
                if (sleepAttempts <= 0)
                {
                    sleepAttempts = 60;
                }
                // Check every second
                while (_connectionCount > 0 && sleepAttempts > 0)
                {
                    --sleepAttempts;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}