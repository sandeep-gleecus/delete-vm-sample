using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the ComponentManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ComponentManagerTest
	{
		private static ComponentManager componentManager;
		private static int projectId;
		private static int projectTemplateId;
		private static int componentId1;
		private static int componentId2;
		private static int componentId3;

		private const int USER_ID_SYSTEM_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Create a new project for our testing and instantiate the component manager class being tested
			componentManager = new ComponentManager();
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("ComponentManagerTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");
			
			//Get the template associated with the project
			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYSTEM_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can create new components in a project
		/// </summary>
		[Test]
		[SpiraTestCase(1207)]
		public void _01_CreateComponents()
		{
			//Lets create three new components in the project, with two being active
			componentId1 = componentManager.Component_Insert(projectId, "Component 1");
			componentId2 = componentManager.Component_Insert(projectId, "Component 2");
			componentId3 = componentManager.Component_Insert(projectId, "Component 3", false);

			//Verify that they created successfully.
			//First check for active undeleted components:
			List<Component> components = componentManager.Component_Retrieve(projectId);
			Assert.AreEqual(2, components.Count);
			Assert.AreEqual("Component 1", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);

			//Now check for all undeleted components
			components = componentManager.Component_Retrieve(projectId, false);
			Assert.AreEqual(3, components.Count);
			Assert.AreEqual("Component 1", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);
		}

		/// <summary>
		/// Tests that we can update existing components in a project
		/// </summary>
		[Test]
		[SpiraTestCase(1208)]
		public void _02_UpdateComponents()
		{
			//Let's retrieve the components and make some updates
			Component component = componentManager.Component_RetrieveById(componentId1);
			component.StartTracking();
			component.Name = "Component 1a";
			componentManager.Component_Update(component);

			component = componentManager.Component_RetrieveById(componentId3);
			component.StartTracking();
			component.IsActive = true;
			componentManager.Component_Update(component);

			//Verify the updates

			//First check for active undeleted components:
			List<Component> components = componentManager.Component_Retrieve(projectId);
			Assert.AreEqual(3, components.Count);
			Assert.AreEqual("Component 1a", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);

			//Now check for all undeleted components
			components = componentManager.Component_Retrieve(projectId, false);
			Assert.AreEqual(3, components.Count);
			Assert.AreEqual("Component 1a", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);
		}

		/// <summary>
		/// Tests that we can view a list of components in a project
		/// </summary>
		[Test]
		[SpiraTestCase(1209)]
		public void _03_RetrieveComponents()
		{
			//Lets add another inactive component
			componentManager.Component_Insert(projectId, "Component 4", false);

			//First check for active undeleted components:
			List<Component> components = componentManager.Component_Retrieve(projectId);
			Assert.AreEqual(3, components.Count);
			Assert.AreEqual("Component 1a", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);

			//Now check for all undeleted components
			components = componentManager.Component_Retrieve(projectId, false);
			Assert.AreEqual(4, components.Count);
			Assert.AreEqual("Component 1a", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);
			Assert.AreEqual("Component 4", components[3].Name);

			//Now check for all components (inc. deleted)
			components = componentManager.Component_Retrieve(projectId, false, true);
			Assert.AreEqual(4, components.Count);
			Assert.AreEqual("Component 1a", components[0].Name);
			Assert.AreEqual("Component 2", components[1].Name);
			Assert.AreEqual("Component 3", components[2].Name);
			Assert.AreEqual("Component 4", components[3].Name);
		}

		/// <summary>
		/// Tests that we can delete components in a project
		/// </summary>
		[Test]
		[SpiraTestCase(1210)]
		public void _04_DeleteComponents()
		{
			//Test that we can delete one of the components
			componentManager.Component_Delete(componentId2);

			//Now verify that it has been deleted
			Component component = componentManager.Component_RetrieveById(componentId2);
			Assert.IsNull(component);

			//Now verify that it can be retrieved if you specify the option to include deleted
			component = componentManager.Component_RetrieveById(componentId2, true);
			Assert.AreEqual(true, component.IsDeleted);

			//Verify that it no longer appears in the lists
			List<Component> components = componentManager.Component_Retrieve(projectId, false);
			Assert.AreEqual(3, components.Count);

			components = componentManager.Component_Retrieve(projectId, false, true);
			Assert.AreEqual(4, components.Count);

			//Finally undelete the component and verify that it can now be retrieved
			componentManager.Component_Undelete(componentId2);
			component = componentManager.Component_RetrieveById(componentId2);
			Assert.AreEqual(false, component.IsDeleted);

			//Verify that it now appears in the lists
			components = componentManager.Component_Retrieve(projectId, false);
			Assert.AreEqual(4, components.Count);
			components = componentManager.Component_Retrieve(projectId, false, true);
			Assert.AreEqual(4, components.Count);
		}
	}
}
