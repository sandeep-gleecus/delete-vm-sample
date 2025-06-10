using System;
using System.Web;
using System.Web.SessionState;
using System.Threading;
using System.Web.Configuration;
using System.Configuration;
using System.Collections.Generic;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Provides an implementation of a fast, scalable database-backed session state system for use in SpiraTeam
    /// </summary>
    /// <remarks>
    /// Since the session is stored in the database, it allows SpiraTeam to be used in a web-farm with
    /// full load-balancing without need for processor/server affinity.
    /// 
    /// Code was taken from: http://msdn.microsoft.com/en-us/library/system.web.sessionstate.sessionstateutility.aspx
    /// and modified to use a database table rather than Hashtable
    /// 
    /// Notes:
    /// 1) Doesn't throw an exception where you have read-only session specified on a page and try and write a session value.
    /// </remarks>
    public sealed class SpiraSessionStateModule : IHttpModule, IDisposable
    {
        private Timer pTimer;
        private int pTimerSeconds = 60;
        private bool pInitialized = false;
        private int pTimeout;
        private HttpCookieMode pCookieMode = HttpCookieMode.UseCookies;
        //private ReaderWriterLockSlim pHashtableLock = new ReaderWriterLockSlim();
        private ISessionIDManager pSessionIDManager;
        private SessionStateSection pConfig;

        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Classes.SpiraSessionStateModule::";

        // // IHttpModule.Init //
        public void Init(HttpApplication app)
        {
            // Add event handlers.
			app.AcquireRequestState += new EventHandler(this.OnAcquireRequestState);
			app.ReleaseRequestState += new EventHandler(this.OnReleaseRequestState);
            //app.EndRequest += new EventHandler(app_EndRequest);

			// Create a SessionIDManager.
			pSessionIDManager = new SessionIDManager();
			pSessionIDManager.Initialize();

			// If not already initialized, initialize timer and configuration.
			if (!pInitialized)
			{
                lock (typeof(SpiraSessionStateModule))
                {
                    if (!pInitialized)
                    {
						// Create a Timer to invoke the ExpireCallback method based on
                        // the pTimerSeconds value (e.g. every 10 seconds).

                        pTimer = new Timer(new TimerCallback(this.ExpireCallback),
                                           null,
                                           0,
                                           pTimerSeconds * 1000);

                        // Get the configuration section and set timeout and CookieMode values.
                        Configuration cfg =
                          WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
                        pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");

                        pTimeout = (int)pConfig.Timeout.TotalMinutes;
                        pCookieMode = pConfig.Cookieless;

                        pInitialized = true;
                    }
                }
            }
        }

        /*
        /// <summary>
        /// Called when the request ends
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void app_EndRequest(object source, EventArgs e)
        {
            const string METHOD_NAME = "app_EndRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;

            // Read the session state from the context
            if (context != null)
            {
                IHttpSessionState sessionState = SessionStateUtility.GetHttpSessionStateFromContext(context);
                if (sessionState != null)
                {
                    HttpSessionStateContainer stateProvider = (HttpSessionStateContainer)sessionState;

                    string sessionId;
                    try
                    {
                        //pHashtableLock.EnterWriteLock();

                        sessionId = pSessionIDManager.GetSessionID(context);
                        SessionManager sessionManager = new SessionManager();
                        Session session = sessionManager.RetrieveById(sessionId);
                        if (session != null && stateProvider != null)
                        {
                            session.StartTracking();
                            SessionStateItemCollection items = new SessionStateItemCollection();
                            for (int i = 0; i < stateProvider.Count; i++)
                            {
                                string name = stateProvider.Keys[i];
                                items[name] = stateProvider[name];
                            }
                            session.Items = ObjectToByteArray(items);
                            session.StaticObjects = ObjectToByteArray(SessionStateUtility.GetSessionStaticObjects(context));
                            sessionManager.Update(session);
                        }
                    }
                    catch (Exception exception)
                    {
                        //log but don't throw
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    }
                    finally
                    {
                        //pHashtableLock.ExitWriteLock();
                    }
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }*/


        // // IHttpModule.Dispose //
        public void Dispose()
        {
            if (pTimer != null)
            {
                this.pTimer.Dispose();
               ((IDisposable)pTimer).Dispose();
            }
        }

        #region Serialization code

        private string ObjectToByteArray(SessionStateItemCollection sessionStateItemCollection)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(xmlRoot);
            foreach (string prop in sessionStateItemCollection)
            {
                XmlElement xmlProp = xmlDoc.CreateElement("prop");
                XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("name");
                xmlAttribute.Value = prop;
                xmlProp.Attributes.Append(xmlAttribute);

                //Handle the ProjectRole case separately
                object sessionStateItem = sessionStateItemCollection[prop];
                string typeCodeString;
                string val;
                if (sessionStateItem != null)
                {
                    if (sessionStateItem.GetType() == typeof(ProjectRole))
                    {
                        ProjectRole projectRole = (ProjectRole)sessionStateItem;
                        typeCodeString = "1001";    //Custom ones start at 1000+
                        System.Web.UI.ObjectStateFormatter formatter = new System.Web.UI.ObjectStateFormatter();
                        val = formatter.Serialize(projectRole);
                    }
                    else
                    {
                        TypeCode typeCode;
                        val = GlobalFunctions.SerializeValue(sessionStateItem, out typeCode);
                        typeCodeString = ((int)typeCode).ToString("0000");
                    }
                    xmlProp.InnerText = typeCodeString + val;
                    xmlRoot.AppendChild(xmlProp);
                }
            }

            return xmlDoc.OuterXml;
        }

        private string ObjectToByteArray(HttpStaticObjectsCollection httpStaticObjectsCollection)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(xmlRoot);
            foreach (string prop in httpStaticObjectsCollection)
            {
                XmlElement xmlProp = xmlDoc.CreateElement("prop");
                xmlProp.Attributes["name"].Value = prop;
                TypeCode typeCode;
                string val = GlobalFunctions.SerializeValue(httpStaticObjectsCollection[prop], out typeCode);
                string typeCodeString = ((int)typeCode).ToString("0000");
                xmlProp.InnerText = typeCodeString + val;
                xmlRoot.AppendChild(xmlProp);
            }

            return xmlDoc.OuterXml;
        }

        private SessionStateItemCollection ByteArrayToSessionStateItemCollection(string str)
        {
            const string METHOD_NAME = "ByteArrayToSessionStateItemCollection";

            SessionStateItemCollection sessionStateItemCollection = new SessionStateItemCollection();
            if (!String.IsNullOrEmpty(str))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(str);
                    XmlNodeList xmlProps = xmlDoc.SelectNodes("/root/prop");
                    if (xmlProps != null && xmlProps.Count > 0)
                    {
                        foreach (XmlElement xmlProp in xmlProps)
                        {
                            if (xmlProp.Attributes ["name"] != null && xmlProp.Attributes["name"].Value != null)
                            {
                                string name = xmlProp.Attributes["name"].Value;
                                string serializedValue = xmlProp.InnerText;
                                if (!String.IsNullOrEmpty(serializedValue))
                                {
                                    //See if we have any known objects that have to be serialized/deserialized
                                    if (serializedValue.SafeSubstring(0, 4) == "1001")
                                    {
                                        try
                                        {
                                            System.Web.UI.ObjectStateFormatter formatter = new System.Web.UI.ObjectStateFormatter();
                                            ProjectRole projectRole = (ProjectRole)formatter.Deserialize(serializedValue.Substring(4));
                                            sessionStateItemCollection[name] = projectRole;
                                        }
                                        catch (Exception exception)
                                        {
                                            //Log and ignore
                                            Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);

                                        }
                                    }
                                    else
                                    {
                                        object val = GlobalFunctions.DeSerializeValue(xmlProp.InnerText);
                                        sessionStateItemCollection[name] = val;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (XmlException exception)
                {
                    //Log and ignore
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                }
            }
            return sessionStateItemCollection;
        }

        private HttpStaticObjectsCollection ByteArrayToHttpStaticObjectsCollection(string str)
        {
            HttpStaticObjectsCollection httpStaticObjectsCollection = new HttpStaticObjectsCollection();
            /*
             * //This collection is static and cannot be added to
             * 
            if (!String.IsNullOrEmpty(str))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(str);
                    XmlNodeList xmlProps = xmlDoc.ChildNodes;
                    if (xmlProps != null && xmlProps.Count > 0)
                    {
                        foreach (XmlElement xmlProp in xmlProps)
                        {
                            if (xmlProp.Attributes ["name"] != null && xmlProp.Attributes["name"].Value != null)
                            {
                                string name = xmlProp.Attributes["name"].Value;
                                object val = GlobalFunctions.DeSerializeValue(xmlProp.Value);
                                httpStaticObjectsCollection[name] = val; 
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    //Ignore
                }
            }*/
            return httpStaticObjectsCollection;
        }

        #endregion
        // // Called periodically by the Timer created in the Init method to check for 
        // expired sessions and remove expired data. //
        void ExpireCallback(object state)
        {
            const string METHOD_NAME = "ExpireCallback";

            try
            {
                //pHashtableLock.EnterWriteLock();

                this.RemoveExpiredSessionData();

            }
            catch (Exception exception)
            {
                //log but don't throw
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
            }
            finally
            {
                //pHashtableLock.ExitWriteLock();
            }
        }


        // // Recursively remove expired session data from session collection. //
        private void RemoveExpiredSessionData()
        {
            const string METHOD_NAME = "RemoveExpiredSessionData";

            try
            {
                string sessionID;
                SessionManager sessionManager = new SessionManager();
                List<Session> sessions = sessionManager.RetrieveExpiredSessions(DateTime.UtcNow);
                List<string> sessionsIdsToDelete = new List<string>();
                foreach (Session session in sessions)
                {
                    sessionID = session.SessionId;
                    sessionsIdsToDelete.Add(sessionID);

                    HttpSessionStateContainer stateProvider =
                        new HttpSessionStateContainer(sessionID,
                                                    ByteArrayToSessionStateItemCollection(session.Items),
                                                    ByteArrayToHttpStaticObjectsCollection(session.StaticObjects),
                                                    pTimeout,
                                                    false,
                                                    pCookieMode,
                                                    SessionStateMode.Custom,
                                                    false);

                    SessionStateUtility.RaiseSessionEnd(stateProvider, this, EventArgs.Empty);
                }

                sessionManager.DeleteSessions(sessionsIdsToDelete);
            }
            catch (Exception exception)
            {
                //log but don't throw
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
            }
        }


        // // Event handler for HttpApplication.AcquireRequestState //
        private void OnAcquireRequestState(object source, EventArgs args)
        {
            const string METHOD_NAME = "OnAcquireRequestState";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            bool isNew = false;
            string sessionID;
            Session sessionData = null;
            bool supportSessionIDReissue = true;

            pSessionIDManager.InitializeRequest(context, false, out supportSessionIDReissue);
            sessionID = pSessionIDManager.GetSessionID(context);

            if (sessionID != null)
            {
                try
                {
                    SessionManager sessionManager = new SessionManager();
                    //pHashtableLock.EnterReadLock();
                    sessionData = sessionManager.RetrieveById(sessionID);

                    if (sessionData != null)
                    {
                        sessionData.StartTracking();
                        sessionData.Expires = DateTime.UtcNow.AddMinutes(pTimeout);
                        sessionManager.Update(sessionData);
                    }
                }
                catch (Exception exception)
                {
                    //log but don't throw
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                }
                finally
                {
                    //pHashtableLock.ExitReadLock();
                }
            }
            else
            {
                bool redirected, cookieAdded;

                sessionID = pSessionIDManager.CreateSessionID(context);
                pSessionIDManager.SaveSessionID(context, sessionID, out redirected, out cookieAdded);

                if (redirected)
                    return;
            }

            if (sessionData == null)
            {
                // Identify the session as a new session state instance. Create a new SessionItem
                // and add it to the database

                isNew = true;

                sessionData = new Session();
                sessionData.SessionId = sessionID;
                sessionData.Items = ObjectToByteArray(new SessionStateItemCollection());
                sessionData.StaticObjects = ObjectToByteArray(SessionStateUtility.GetSessionStaticObjects(context));
                sessionData.Expires = DateTime.UtcNow.AddMinutes(pTimeout);

                try
                {
                    SessionManager sessionManager = new SessionManager();
                    //pHashtableLock.EnterWriteLock();
                    sessionManager.Insert(sessionData);
                }
                catch (Exception exception)
                {
                    //log but don't throw
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                }
                finally
                {
                    //pHashtableLock.ExitWriteLock();
                }
            }

            // Add the session data to the current HttpContext.
            SessionStateItemCollection sessionStateItemCollection = ByteArrayToSessionStateItemCollection(sessionData.Items);
            HttpStaticObjectsCollection httpStaticObjectsCollection = ByteArrayToHttpStaticObjectsCollection(sessionData.StaticObjects);
            Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "SessionStateItemCollection.Count=" + sessionStateItemCollection.Count);
            Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "HttpStaticObjectsCollection.Count=" + httpStaticObjectsCollection.Count);
            SessionStateUtility.AddHttpSessionStateToContext(context,
                             new HttpSessionStateContainer(sessionID,
                                                          sessionStateItemCollection,
                                                          httpStaticObjectsCollection,
                                                          pTimeout,
                                                          isNew,
                                                          pCookieMode,
                                                          SessionStateMode.Custom,
                                                          false));

            // Execute the Session_OnStart event for a new session.
            if (isNew && Start != null)
            {
                Start(this, EventArgs.Empty);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        // // Event for Session_OnStart event in the Global.asax file. //
        public event EventHandler Start;


        // // Event handler for HttpApplication.ReleaseRequestState //
        private void OnReleaseRequestState(object source, EventArgs args)
        {
            const string METHOD_NAME = "OnReleaseRequestState";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            string sessionID;

            // Read the session state from the context
            if (context != null)
            {
                IHttpSessionState sessionState = SessionStateUtility.GetHttpSessionStateFromContext(context);
                if (sessionState != null)
                {
                    HttpSessionStateContainer stateProvider = (HttpSessionStateContainer)sessionState;

                    // If Session.Abandon() was called, remove the session data from the local Hashtable
                    // and execute the Session_OnEnd event from the Global.asax file.
                    if (stateProvider.IsAbandoned)
                    {
                        try
                        {
                            //pHashtableLock.EnterWriteLock();

                            sessionID = pSessionIDManager.GetSessionID(context);
                            new SessionManager().DeleteSessions(new List<string>() { sessionID });
                        }
                        catch (Exception exception)
                        {
                            //log but don't throw
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                        }
                        finally
                        {
                            //pHashtableLock.ExitWriteLock();
                        }

                        SessionStateUtility.RaiseSessionEnd(stateProvider, this, EventArgs.Empty);
                    }
                    else
                    {
                        sessionID = pSessionIDManager.GetSessionID(context);
                        SessionManager sessionManager = new SessionManager();
                        Session session = sessionManager.RetrieveById(sessionID);
                        if (session != null && stateProvider != null)
                        {
                            session.StartTracking();
                            SessionStateItemCollection items = new SessionStateItemCollection();
                            for (int i = 0; i < stateProvider.Count; i++)
                            {
                                string name = stateProvider.Keys[i];
                                items[name] = stateProvider[name];
                            }
                            session.Items = ObjectToByteArray(items);
                            session.StaticObjects = ObjectToByteArray(SessionStateUtility.GetSessionStaticObjects(context));
                            sessionManager.Update(session);
                        }
                    }

                    SessionStateUtility.RemoveHttpSessionStateFromContext(context);
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }
    }
}