using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// The unit test for the session manager class
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class SessionManagerTest
	{
		protected static SessionManager sessionManager;

		/// <summary>
		/// Initializes the business objects being tested and sets up the tests
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			sessionManager = new SessionManager();

			//Make sure we have no sessions to start with
			List<Session> sessions = sessionManager.RetrieveExpiredSessions(DateTime.MaxValue);
			List<string> expiredSessions = new List<string>();
			foreach (Session session in sessions)
			{
				expiredSessions.Add(session.SessionId);
			}
			sessionManager.DeleteSessions(expiredSessions);
		}

		/// <summary>
		/// Cleans up any data if tests fail
		/// </summary>
		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete all sessions
			List<Session> sessions = sessionManager.RetrieveExpiredSessions(DateTime.MaxValue);
			List<string> expiredSessions = new List<string>();
			foreach (Session session in sessions)
			{
				expiredSessions.Add(session.SessionId);
			}
			sessionManager.DeleteSessions(expiredSessions);
		}

		/// <summary>
		/// Tests that we can add session items
		/// </summary>
		[Test]
		[SpiraTestCase(1329)]
		public void _01_AddSessionItems()
		{
			//Create two sessions
			Session session = new Session();
			session.SessionId = "4394C9BB-1035-4F71-859A-F7CDFBEFB39F";   //Made up session id
			session.Expires = DateTime.UtcNow.AddMinutes(30);
			session.Items = "abc";
			session.StaticObjects = "def";
			sessionManager.Insert(session);

			//Create two sessions
			session = new Session();
			session.SessionId = "A5A68AE5-442C-4A9D-AC38-81286308B630";   //Made up session id
			session.Expires = DateTime.UtcNow.AddMinutes(30);
			session.Items = "123";
			session.StaticObjects = "456";
			sessionManager.Insert(session);

			//Verify that they were added
			session = sessionManager.RetrieveById("4394C9BB-1035-4F71-859A-F7CDFBEFB39F");
			Assert.AreEqual("abc", session.Items);
			Assert.AreEqual("def", session.StaticObjects);

			session = sessionManager.RetrieveById("A5A68AE5-442C-4A9D-AC38-81286308B630");
			Assert.AreEqual("123", session.Items);
			Assert.AreEqual("456", session.StaticObjects);
		}

		/// <summary>
		/// Tests that we can update a session
		/// </summary>
		[Test]
		[SpiraTestCase(1330)]
		public void _02_UpdateSessions()
		{
			//Get the original session and verify current state
			Session session = sessionManager.RetrieveById("4394C9BB-1035-4F71-859A-F7CDFBEFB39F");
			Assert.AreEqual("abc", session.Items);
			Assert.AreEqual("def", session.StaticObjects);

			//change the object
			session.StartTracking();
			session.Expires = DateTime.UtcNow.AddMinutes(30);
			session.Items = "xyz";
			session.StaticObjects = "ppp";
			sessionManager.Update(session);

			//Verify the update
			session = sessionManager.RetrieveById("4394C9BB-1035-4F71-859A-F7CDFBEFB39F");
			Assert.AreEqual("xyz", session.Items);
			Assert.AreEqual("ppp", session.StaticObjects);
		}

		/// <summary>
		/// Tests that we can remove expired sessions
		/// </summary>
		[Test]
		[SpiraTestCase(1331)]
		public void _03_ExpiredSessions()
		{
			//Verify that none have really expired
			List<Session> sessions = sessionManager.RetrieveExpiredSessions(DateTime.UtcNow);
			Assert.AreEqual(0, sessions.Count);

			//Get a list of all sessions (using arficial expiration date) and delete them as if they had expired
			sessions = sessionManager.RetrieveExpiredSessions(DateTime.MaxValue);
			Assert.AreEqual(2, sessions.Count);
			List<string> expiredSessions = new List<string>();
			foreach (Session session in sessions)
			{
				expiredSessions.Add(session.SessionId);
			}
			sessionManager.DeleteSessions(expiredSessions);

			//Verify the delete
			sessions = sessionManager.RetrieveExpiredSessions(DateTime.MaxValue);
			Assert.AreEqual(0, sessions.Count);
		}
	}
}
