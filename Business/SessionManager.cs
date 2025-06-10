using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Data;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Handles the data access to the Session store used by the custom Spira Session Module
    /// </summary>
    public class SessionManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.SessionManager::";

        /// <summary>
        /// Retrieves a session record by its session ID
        /// </summary>
        /// <param name="sessionId">The session id</param>
        /// <returns>The session record</returns>
        public Session RetrieveById(string sessionId)
        {
            const string METHOD_NAME = "RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Session session = null;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the session record
                    var query = from s in context.Sessions
                                where s.SessionId == sessionId
                                select s;

                    session = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return session;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates a specific session object
        /// </summary>
        /// <param name="session">The session entry</param>
        public void Update(Session session)
        {
            const string METHOD_NAME = "Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Apply the changes
                    context.Sessions.ApplyChanges(session);
                    context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Insertrs a specific session object
        /// </summary>
        /// <param name="session">The session entry</param>
        public Session Insert(Session session)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Add the object
                    context.Sessions.AddObject(session);
                    context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return session;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes the list of sessions by their id
        /// </summary>
        /// <param name="sessionIds">The sessions</param>
        public void DeleteSessions(List<string> sessionIds)
        {
            const string METHOD_NAME = "DeleteSessions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Session> sessions;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the session records
                    var query = from s in context.Sessions
                                where sessionIds.Contains(s.SessionId)
                                select s;

                    sessions = query.ToList();
                    foreach (Session session in sessions)
                    {
                        context.Sessions.DeleteObject(session);
                    }
                    context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all sessions older than a specific date
        /// </summary>
        /// <param name="expirationDate">The date to compare against</param>
        public List<Session> RetrieveExpiredSessions(DateTime expirationDate)
        {
            const string METHOD_NAME = "RetrieveExpiredSessions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Session> expiredSessions;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the session records
                    var query = from s in context.Sessions
                                where s.Expires <= expirationDate
                                orderby s.SessionId
                                select s;

                    expiredSessions = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return expiredSessions;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
