using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Management;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary> 
	/// Tests the EventManager business class and the Logger common class 
	/// </summary> 
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class EventManagerTest
	{
		private static EventManager eventManager;

		[TestFixtureSetUp]
		public void Init()
		{
			eventManager = new Business.EventManager();

			//Clear the event log 
			eventManager.Clear();
		}

		[TestFixtureTearDown]
		public void Dispose()
		{
			//Clear the event log 
			eventManager.Clear();
		}

		/// <summary>  
		/// Tests that we can read and write entries to/from the build-in event log  
		/// </summary>  
		[
		Test,
		SpiraTestCase(954)
		]
		public void _01_ReadWriteEntries()
		{
			//First verify that there is just the one entry that the log was cleared.  
			int count;
			List<Event> events = eventManager.GetEvents(null, "EventTimeUtc DESC", 0, 99999, InternalRoutines.UTC_OFFSET, out count);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, events.Count);

			//Now write a couple of events using the two different overloads  
			Logger.WriteToDatabase("MySource", "Issue in Assembly X", "Object variable was null at line 205", System.Diagnostics.EventLogEntryType.Error, typeof(ApplicationException), Logger.EVENT_CATEGORY_APPLICATION);

			//Wait a couple of seconds to avoid timing issues (flaky tests if not)  
			System.Threading.Thread.Sleep(2000);
			List<WebBaseEvent> webEvents = new List<WebBaseEvent>();
			WebBaseEvent newEvent = new SpiraWebBaseEvent("Issue in Assembly Y", null, 0, 0);
			webEvents.Add(newEvent);
			Logger.WriteToDatabase(webEvents, 0, DateTime.UtcNow);

			//Verify that we have two entries  
			events = eventManager.GetEvents(null, "EventTimeUtc DESC", 0, 99999, InternalRoutines.UTC_OFFSET, out count);
			Assert.AreEqual(3, count);
			Assert.AreEqual(3, events.Count);
			Assert.AreEqual("Issue in Assembly Y", events[0].Message);
			Assert.AreEqual("Inflectra.SpiraTest.Common.SpiraWebBaseEvent", events[0].EventCategory);
			Assert.AreEqual("Issue in Assembly X", events[1].Message);
			Assert.AreEqual(Logger.EVENT_CATEGORY_APPLICATION, events[1].EventCategory);
			Assert.AreEqual("Object variable was null at line 205", events[1].Details);

			//Try filtering by some fields  
			Hashtable filters = new Hashtable();
			filters.Add("EventCategory", Logger.EVENT_CATEGORY_APPLICATION);
			events = eventManager.GetEvents(filters, "EventTimeUtc DESC", 0, 99999, InternalRoutines.UTC_OFFSET, out count);
			Assert.AreEqual(2, count);
			Assert.AreEqual(2, events.Count);
			Assert.AreEqual("Issue in Assembly X", events[0].Message);
			Assert.AreEqual(Logger.EVENT_CATEGORY_APPLICATION, events[0].EventCategory);
			Assert.AreEqual("Object variable was null at line 205", events[0].Details);

			//Try clearing the log  
			eventManager.Clear();

			//Verify that it cleared  
			events = eventManager.GetEvents(null, "EventTimeUtc DESC", 0, 99999, InternalRoutines.UTC_OFFSET, out count);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, events.Count);

			//Get the list of types  
			List<EventType> eventTypes = eventManager.GetTypes();
			Assert.AreEqual(5, eventTypes.Count);
		}
	}
}
