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
    /// Tests the FileTypeManager
    /// </summary>
    [
    TestFixture,
    SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
    ]
    public class FiletypeTest
    {
        protected static Business.FileTypeManager fileTypeManager;

        /// <summary>
        /// Initializes the business objects being tested
        /// </summary>
        [TestFixtureSetUp]
        public void Init()
        {
            fileTypeManager = new Business.FileTypeManager();
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
            //Make sure the new filetype is deleted if test 2 failed midway
            Filetype filetype = fileTypeManager.GetFileTypeValues().Find(f => f.FileExtension == "tst");
            if (filetype != null)
            {
                fileTypeManager.DeleteFileType(filetype.FiletypeId);
            }
        }

        /// <summary>
        /// Test that we can display the list of filetypes
        /// </summary>
        [
        Test,
        SpiraTestCase(804)
        ]
        public void _01_DisplayFileTypeList()
        {
            List<Filetype> filetypes = fileTypeManager.GetFileTypeValues();

            //Test that we get the list
            Assert.IsTrue(filetypes.Count > 0);
            
            //Check the default type
            Assert.AreEqual("Other File Type", filetypes[0].Description);
            Assert.AreEqual("", filetypes[0].FileExtension.Trim());
            Assert.AreEqual("application/octet-stream", filetypes[0].Mime);
            Assert.AreEqual("Unknown.svg", filetypes[0].Icon);

            //Check the GIF type
            Filetype filetype = filetypes.Find(f => f.FileExtension == "gif");
            Assert.AreEqual("GIF Image", filetype.Description);
            Assert.AreEqual("gif", filetype.FileExtension);
            Assert.AreEqual("image/gif", filetype.Mime);
            Assert.AreEqual("GIF-Image.svg", filetype.Icon);
        }

        /// <summary>
        /// Test that we can add, edit and delete a filetype
        /// </summary>
        [
        Test,
        SpiraTestCase(805)
        ]
        public void _02_EditFileType()
        {
            //First lets add a new filetype
            fileTypeManager.UpdateAddFileType(0, "tst", "application/remotelaunch", "Unknown.png", "RemoteLaunch file", 1);

            //Verify that it was added
            fileTypeManager.RefreshFiletypes();
            Filetype filetype = fileTypeManager.GetFileTypeValues().Find(f => f.FileExtension == "tst");
            Assert.AreEqual("RemoteLaunch file", filetype.Description);
            Assert.AreEqual("tst", filetype.FileExtension);
            Assert.AreEqual("application/remotelaunch", filetype.Mime);
            Assert.AreEqual("Unknown.png", filetype.Icon);

            //Make a change
            fileTypeManager.UpdateAddFileType(filetype.FiletypeId, "tst", "application/rapiselauncher", "Unknown.png", "RapiseLauncher file", 1);

            //Verify the change
            filetype = fileTypeManager.GetFileTypeValues().Find(f => f.FileExtension == "tst");
            Assert.AreEqual("RapiseLauncher file", filetype.Description);
            Assert.AreEqual("tst", filetype.FileExtension);
            Assert.AreEqual("application/rapiselauncher", filetype.Mime);
            Assert.AreEqual("Unknown.png", filetype.Icon);

            //Delete the filetype
            fileTypeManager.DeleteFileType(filetype.FiletypeId, 1);

            //Verify that it was deleted
            filetype = fileTypeManager.GetFileTypeValues().Find(f => f.FileExtension == "tst");
            Assert.IsNull(filetype);
        }
    }
}
