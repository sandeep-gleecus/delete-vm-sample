using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Xml;

namespace Inflectra.SpiraTest.PlugIns
{
	/// <summary>A dummy version control provider used to running the unit tests against</summary>
	public class TestVersionControlProvider2 : IVersionControlPlugIn2
	{
		private static Random rnd = new Random();
		protected static int highestFileId = 0;
		protected static EventLog applicationEventLog = null;
        protected static XmlDocument sampleRepo = null;
        protected const string ROOT_FOLDER_NAME = "Root";

        //We need to store the list of files returned for a specific folder so that we can make sure our data is internally consistent
        private static List<VersionControlFile> files = new List<VersionControlFile>();

		/// <summary>Initializes the provider - connects, authenticates and returns a session token</summary>
		/// <param name="connection"></param>
		/// <param name="credentials"></param>
		/// <param name="parameters"></param>
		/// <returns>A provider-specific object that is passed on subsequent calls. Since this is a dummy provider, we'll just pass back the connection and credentials</returns>
		/// <param name="eventLog">Handle to an event log object</param>
		/// <param name="custom01">Custom parameters that are provider-specific</param>
		/// <param name="custom02">Custom parameters that are provider-specific</param>
		/// <param name="custom03">Custom parameters that are provider-specific</param>
		/// <param name="custom04">Custom parameters that are provider-specific</param>
		/// <param name="custom05">Custom parameters that are provider-specific</param>
		/// <param name="cacheFolder">The location to the folder where any cached data can be stored</param>
		/// <remarks>Throws an exception if unable to connect or authenticate</remarks>
		public object Initialize(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string cacheFolder, string custom01, string custom02, string custom03, string custom04, string custom05)
		{
			if (credentials.UserName != "fredbloggs" && credentials.UserName != "joesmith")
			{
				throw new VersionControlAuthenticationException("Unable to login to version control provider");
			}
			if (connection.Length < 7 || connection.Substring(0, 7) != "test://")
			{
				throw new VersionControlGeneralException("Unable to access version control provider with provided connection information");
			}
			applicationEventLog = eventLog;
			Dictionary<string, object> token = new Dictionary<string, object>();
			token.Add("connection", connection);
			token.Add("credentials", credentials);

            //Initialize the static copy of the sample repo stored in XML

            Assembly assembly = Assembly.GetExecutingAssembly();
            //(e.g. Inflectra.SpiraTest.PlugIns.SampleFiles.SampleRepo.xml)
            string xml;
            using (Stream stream = assembly.GetManifestResourceStream("Inflectra.SpiraTest.PlugIns.SampleFiles.SampleRepo.xml"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    xml = reader.ReadToEnd();
                }
            }
            sampleRepo = new XmlDocument();
            sampleRepo.LoadXml(xml);

            return token;
		}

		/// <summary>
		/// Closes the data stream provided by OpenFile. Clients must NOT CLOSE THE STREAM DIRECTLY
		/// </summary>
		/// <param name="versionControlFileStream">The stream to be closed</param>
		public void CloseFile(VersionControlFileStream versionControlFileStream)
		{
			versionControlFileStream.DataStream.Close();
		}

		public VersionControlFolder RetrieveFolder(object token, string folderKey, string branchKey)
		{
			try
			{
                //Get the connection string..
                string conn = (string)((Dictionary<string, object>)token)["connection"];


                VersionControlFolder versionControlFolder = null;

                //Handle the root case separately
                if (String.IsNullOrEmpty(folderKey) || folderKey.Equals(conn))
                {
                    versionControlFolder = new VersionControlFolder();
                    versionControlFolder.FolderKey = conn;
                    versionControlFolder.Name = ROOT_FOLDER_NAME;
                }
                else
                {
                    if (sampleRepo != null)
                    {
                        XmlNode xmlFolder = sampleRepo.SelectSingleNode("/samplerepo/filesystem/folders//folder[@key='parentFolderKey']");
                        if (xmlFolder != null)
                        {
                            versionControlFolder = new VersionControlFolder();
                            versionControlFolder.FolderKey = xmlFolder.Attributes["key"].Value;
                            versionControlFolder.Name = xmlFolder.Attributes["name"].Value;
                        }
                    }
                }
                return versionControlFolder;
			}
			catch (Exception exception)
			{
				//Throw an unable to get artifact exception
				throw new VersionControlArtifactNotFoundException("Unable to retrieve folder '" + folderKey + "'", exception);
			}
		}

		public List<VersionControlFolder> RetrieveFolders(object token, string parentFolderKey, string branchKey)
		{
			//Create a list of folders based on what's passed in
			//Get the connection string..
			string conn = (string)((Dictionary<string, object>)token)["connection"];
			List<VersionControlFolder> versionControlFolders = new List<VersionControlFolder>();
			if (String.IsNullOrEmpty(parentFolderKey) || parentFolderKey.Equals(conn))
			{
                //Root folders
                if (sampleRepo != null)
                {
                    XmlNodeList xmlFolders = sampleRepo.SelectNodes("/samplerepo/filesystem/folders/folder");

                    foreach (XmlNode xmlFolder in xmlFolders)
                    {
                        VersionControlFolder folder = new VersionControlFolder();
                        folder.FolderKey = xmlFolder.Attributes["key"].Value;
                        folder.Name = xmlFolder.Attributes["name"].Value;
                        versionControlFolders.Add(folder);
                    }
                }
            }
			else
			{
                //Sub folders
                if (sampleRepo != null)
                {
                    XmlNodeList xmlFolders = sampleRepo.SelectNodes("/samplerepo/filesystem/folders//folder[@key='" + parentFolderKey + "']/folders/folder");

                    foreach (XmlNode xmlFolder in xmlFolders)
                    {
                        VersionControlFolder folder = new VersionControlFolder();
                        folder.FolderKey = xmlFolder.Attributes["key"].Value;
                        folder.Name = xmlFolder.Attributes["name"].Value;
                        versionControlFolders.Add(folder);
                    }
                }
            }
			return versionControlFolders;
		}

		public List<VersionControlFile> RetrieveFilesByFolder(object token, string folderKey, string branchKey)
		{
            //Get the connection string..
            string conn = (string)((Dictionary<string, object>)token)["connection"];

            List<VersionControlFile> versionControlFiles = new List<VersionControlFile>();

            if (sampleRepo != null)
            {
                if (String.IsNullOrEmpty(folderKey) || folderKey.Equals(conn))
                {
                    //Root folder
                    XmlNodeList xmlFiles = sampleRepo.SelectNodes("/samplerepo/filesystem/files/file");

                    foreach (XmlNode xmlFile in xmlFiles)
                    {
                        //Get the file key
                        string fileKey = xmlFile.Attributes["key"].Value;

                        //See which revisions this file is in and get the last one in the current branch
                        XmlNodeList xmlRevisionsWithFile = sampleRepo.SelectNodes("/samplerepo/revisions/revision[files/fileref/@key = '" + fileKey + "']");
                        XmlNode xmlLatestRevisionInBranch = null;
                        for (int i = xmlRevisionsWithFile.Count -1; i >= 0; i--)
                        {
                            XmlNode xmlRevision = xmlRevisionsWithFile[i];
                            string revisionKey = xmlRevision.Attributes["key"].Value;

                            //If this revision is in the specified branch, then stop
                            if (sampleRepo.SelectSingleNode("/samplerepo/branches/branch[@name='" + branchKey + "']/revisions/revisionref[@key='" + revisionKey + "']") != null)
                            {
                                xmlLatestRevisionInBranch = xmlRevision;
                                break;
                            }
                        }

                        if (xmlLatestRevisionInBranch != null)
                        {
                            string revisionKey = xmlLatestRevisionInBranch.Attributes["key"].Value;
                            string revisionName = xmlLatestRevisionInBranch.Attributes["name"].Value;
                            string author = xmlLatestRevisionInBranch.Attributes["author"].Value;
                            int timeOffset = Int32.Parse(xmlLatestRevisionInBranch.Attributes["timeOffset"].Value);
                            DateTime updateDate = DateTime.UtcNow.AddDays(timeOffset);
                            string action = "";
                            //Get the list of files in the revision
                            XmlNode xmlFileRef = xmlLatestRevisionInBranch.SelectSingleNode("files/fileref[@key='" + fileKey + "']");
                            if (xmlFileRef != null)
                            {
                                action = xmlFileRef.Attributes["action"].Value;
                            }

                            VersionControlFile file = new VersionControlFile();
                            file.FileKey = fileKey;
                            file.Name = xmlFile.Attributes["name"].Value;
                            file.RevisionKey = revisionKey;
                            file.Revision = revisionName;
                            file.Size = 1024;
                            file.LastUpdated = updateDate;
                            file.Author = author;
                            switch (action)
                            {
                                case "add":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Added;
                                    }
                                    break;

                                case "delete":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Deleted;
                                    }
                                    break;

                                case "modify":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Modified;
                                    }
                                    break;

                                default:
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Undefined;
                                    }
                                    break;
                            }
                            versionControlFiles.Add(file);
                        }
                    }
                }
                else
                {
                    XmlNodeList xmlFiles = sampleRepo.SelectNodes("/samplerepo/filesystem/folders//folder[@key='" + folderKey + "']/files/file");

                    foreach (XmlNode xmlFile in xmlFiles)
                    {
                        //Get the file key
                        string fileKey = xmlFile.Attributes["key"].Value;

                        //See which revisions this file is in and get the last one in the current branch
                        XmlNodeList xmlRevisionsWithFile = sampleRepo.SelectNodes("/samplerepo/revisions/revision[files/fileref/@key = '" + fileKey + "']");
                        XmlNode xmlLatestRevisionInBranch = null;
                        for (int i = xmlRevisionsWithFile.Count - 1; i >= 0; i--)
                        {
                            XmlNode xmlRevision = xmlRevisionsWithFile[i];
                            string revisionKey = xmlRevision.Attributes["key"].Value;

                            //If this revision is in the specified branch, then stop
                            if (sampleRepo.SelectSingleNode("/samplerepo/branches/branch[@name='" + branchKey + "']/revisions/revisionref[@key='" + revisionKey + "']") != null)
                            {
                                xmlLatestRevisionInBranch = xmlRevision;
                                break;
                            }
                        }

                        if (xmlLatestRevisionInBranch != null)
                        {
                            string revisionKey = xmlLatestRevisionInBranch.Attributes["key"].Value;
                            string revisionName = xmlLatestRevisionInBranch.Attributes["name"].Value;
                            string author = xmlLatestRevisionInBranch.Attributes["author"].Value;
                            int timeOffset = Int32.Parse(xmlLatestRevisionInBranch.Attributes["timeOffset"].Value);
                            DateTime updateDate = DateTime.UtcNow.AddDays(timeOffset);
                            string action = "";
                            //Get the list of files in the revision
                            XmlNode xmlFileRef = xmlLatestRevisionInBranch.SelectSingleNode("files/fileref[@key='" + fileKey + "']");
                            if (xmlFileRef != null)
                            {
                                action = xmlFileRef.Attributes["action"].Value;
                            }

                            VersionControlFile file = new VersionControlFile();
                            file.FileKey = fileKey;
                            file.Name = xmlFile.Attributes["name"].Value;
                            file.RevisionKey = revisionKey;
                            file.Revision = revisionName;
                            file.Size = 1024;
                            file.LastUpdated = updateDate;
                            file.Author = author;
                            switch (action)
                            {
                                case "add":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Added;
                                    }
                                    break;

                                case "delete":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Deleted;
                                    }
                                    break;

                                case "modify":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Modified;
                                    }
                                    break;

                                default:
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Undefined;
                                    }
                                    break;
                            }
                            versionControlFiles.Add(file);
                        }
                    }
                }
            }

            return versionControlFiles;
		}

		public List<VersionControlFile> RetrieveFilesForRevision(object token, string revisionKey, string branchKey)
		{
            //Get the connection string..
            string conn = (string)((Dictionary<string, object>)token)["connection"];

            List<VersionControlFile> versionControlFiles = new List<VersionControlFile>();

            //We only look at the revision key not the branch key

            if (!String.IsNullOrEmpty(revisionKey))
            {
                //Root folder
                if (sampleRepo != null)
                {
                    //Get the revision
                    XmlNode xmlRevision = sampleRepo.SelectSingleNode("/samplerepo/revisions/revision[@key='" + revisionKey + "']");
                    string revisionName = xmlRevision.Attributes["name"].Value;
                    string author = xmlRevision.Attributes["author"].Value;
                    int timeOffset = Int32.Parse(xmlRevision.Attributes["timeOffset"].Value);
                    DateTime updateDate = DateTime.UtcNow.AddDays(timeOffset);

                    //Get the list of files in the revision
                    XmlNodeList xmlFileRefs = xmlRevision.SelectNodes("files/fileref");

                    foreach (XmlNode xmlFileRef in xmlFileRefs)
                    {
                        //Now get the actual file itself based on this file key
                        string fileKey = xmlFileRef.Attributes["key"].Value;
                        string action = xmlFileRef.Attributes["action"].Value;
                        XmlNode xmlFile = sampleRepo.SelectSingleNode("/samplerepo/filesystem//files/file[@key='" + fileKey + "']");

                        if (xmlFile != null)
                        {
                            VersionControlFile file = new VersionControlFile();
                            file.FileKey = xmlFile.Attributes["key"].Value;
                            file.Name = xmlFile.Attributes["name"].Value;
                            file.RevisionKey = revisionKey;
                            file.Revision = revisionName;
                            file.Size = 1024;
                            file.LastUpdated = updateDate;
                            file.Author = author;
                            switch (action)
                            {
                                case "add":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Added;
                                    }
                                    break;

                                case "delete":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Deleted;
                                    }
                                    break;

                                case "modify":
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Modified;
                                    }
                                    break;

                                default:
                                    {
                                        file.Action = VersionControlFile.VersionControlActionEnum.Undefined;
                                    }
                                    break;
                            }
                            versionControlFiles.Add(file);
                        }
                    }
                }
            }

            return versionControlFiles;
        }

		public List<VersionControlRevision> RetrieveRevisions(object token, string branchKey)
		{
			//First create the list
			List<VersionControlRevision> versionControlRevisions = new List<VersionControlRevision>();

            //Now loop through and populate revision list
            if (sampleRepo != null)
            {
                //First get the list of revisions
                XmlNodeList xmlRevisions = sampleRepo.SelectNodes("/samplerepo/revisions/revision");

                foreach (XmlNode xmlRevision in xmlRevisions)
                {
                    //Make sure the revision is in this branch
                    string revisionKey = xmlRevision.Attributes["key"].Value;
                    XmlNode xmlRevisionInBranch = sampleRepo.SelectSingleNode("/samplerepo/branches/branch[@name='" + branchKey + "']/revisions/revisionref[@key='" + revisionKey + "']");
                    if (xmlRevisionInBranch != null)
                    {
                        VersionControlRevision revision = new VersionControlRevision();
                        revision.RevisionKey = revisionKey;
                        revision.Name = xmlRevision.Attributes["name"].Value;
                        revision.Author = xmlRevision.Attributes["author"].Value;
                        revision.Message = xmlRevision.Attributes["message"].Value;
                        revision.PropertiesChanged = (xmlRevision.Attributes["propertiesChanged"].Value == "true");
                        revision.ContentChanged = (xmlRevision.Attributes["contentChanged"].Value == "true");
                        int timeOffset = Int32.Parse(xmlRevision.Attributes["timeOffset"].Value);
                        revision.UpdateDate = DateTime.UtcNow.AddDays(timeOffset);
                        versionControlRevisions.Add(revision);
                    }
                }
            }

            return versionControlRevisions;
		}

		public VersionControlFileStream OpenFile(object token, string fileKey, string revisionKey, string branchKey)
		{
			//For this dummy provider we just need to create a new in-memory stream, one for latest revision
			//and one for a specific one unless we have a specific file
			string dummyText = "";
			if (revisionKey == "")
			{
				dummyText = "Latest Revision";
			}
			else
			{
				dummyText = "Specific Revision: " + revisionKey;
			}

            //See if we have an actual file that matches this type
            //Need to convert /Root/Code/SpiraTestExecute.java ----> Code.SpiraTestExecute.java
            string resourceName = fileKey.Replace("/Root/", "").Replace("/", ".");

            //First we try and get the file with a specific revision
            string text = ReadFileFromAssembly(resourceName + "." + revisionKey);
            if (String.IsNullOrWhiteSpace(text))
            {
                //Fallback to the general versiom
                text = ReadFileFromAssembly(resourceName);
            }

            //Otherwise use a dummy file based on type
            if (String.IsNullOrWhiteSpace(text))
            {
                //If this is one of the known special file types, get the appropriate content
                string sampleFile = null;
                if (fileKey.EndsWith(".java"))
                {
                    sampleFile = "SampleFile.java";
                }
                if (fileKey.EndsWith(".cpp"))
                {
                    sampleFile = "SampleFile.cpp";
                }
                if (fileKey.EndsWith(".cs"))
                {
                    sampleFile = "SampleFile.cs";
                }
                if (fileKey.EndsWith(".php"))
                {
                    sampleFile = "SampleFile.php";
                }
                if (fileKey.EndsWith(".pl"))
                {
                    sampleFile = "SampleFile.pl";
                }
                if (fileKey.EndsWith(".py"))
                {
                    sampleFile = "SampleFile.py";
                }
                if (fileKey.EndsWith(".rb"))
                {
                    sampleFile = "SampleFile.rb";
                }
                if (fileKey.EndsWith(".xml"))
                {
                    sampleFile = "SampleFile.xml";
                }
                if (fileKey.EndsWith(".yml"))
                {
                    sampleFile = "SampleFile.yml";
                }
                if (fileKey.EndsWith(".ts"))
                {
                    sampleFile = "SampleFile.ts";
                }
                if (fileKey.EndsWith(".tsx"))
                {
                    sampleFile = "SampleFile.tsx";
                }
                if (fileKey.EndsWith(".bat"))
                {
                    sampleFile = "SampleFile.bat";
                }
                if (fileKey.EndsWith(".svg"))
                {
                    sampleFile = "SampleFile.svg";
                }
                if (fileKey.EndsWith(".json"))
                {
                    sampleFile = "SampleFile.json";
                }
                if (fileKey.EndsWith(".feature"))
                {
                    sampleFile = "SampleFile.feature";
                }
                if (fileKey.EndsWith(".md"))
                {
                    sampleFile = "SampleFile.md";
                }

                if (!String.IsNullOrEmpty(sampleFile))
                {
                    dummyText = ReadFileFromAssembly(sampleFile);
                }
                text = dummyText;
            }

			byte[] buffer = ASCIIEncoding.UTF8.GetBytes(text);
			MemoryStream memoryStream = new MemoryStream(buffer);

			VersionControlFileStream versionControlFileStream = new VersionControlFileStream();
			versionControlFileStream.FileKey = fileKey;
			versionControlFileStream.RevisionKey = revisionKey;
			versionControlFileStream.LocalPath = "";    //Not used by this provider since memory stream
			versionControlFileStream.DataStream = memoryStream;
			return versionControlFileStream;
		}

        /// <summary>
        /// Returns the text from an embedded sample file
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>The text</returns>
        private string ReadFileFromAssembly(string filePath)
        {
            string text = "";
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                //(e.g. Inflectra.SpiraTest.PlugIns.SampleFiles.SampleFile.java)
                using (Stream stream = assembly.GetManifestResourceStream("Inflectra.SpiraTest.PlugIns.SampleFiles." + filePath))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        text = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                //Do nothing, it just returns ""
            }

            return text;
        }

        /// <summary>
        /// Retrieves the list of branches
        /// </summary>
		public List<VersionControlBranch> RetrieveBranches(object token)
		{
			List<VersionControlBranch> branches = new List<VersionControlBranch>();

            //Get the branches
            if (sampleRepo != null)
            {
                XmlNodeList xmlBranches = sampleRepo.SelectNodes("/samplerepo/branches/branch");

                foreach (XmlNode xmlBranch in xmlBranches)
                {
                    VersionControlBranch branch = new VersionControlBranch();
                    branch.BranchKey = xmlBranch.Attributes["name"].Value;
                    branch.IsDefault = (xmlBranch.Attributes["is-head"] != null && xmlBranch.Attributes["is-head"].Value == "true");
                    branches.Add(branch);
                }
            }

			return branches;
		}

		/// <summary>Currently returns all the revisions in master since it's a small repo</summary>
		public List<VersionControlRevision> RetrieveRevisionsSince(object token, string branchKey, DateTime date)
		{
            return this.RetrieveRevisions(token, branchKey);
		}

		/// <summary>Tests the given connection information.</summary>
		/// <param name="connection">The connection info</param>
		/// <param name="credentials">The login/password/domain for the provider</param>
		/// <param name="parameters">Any custom parameters</param>
		/// <param name="eventLog">A handle to the Windows Event Log used by Spira</param>
		/// <param name="custom01">Provider-specific parameter</param>
		/// <param name="custom02">Provider-specific parameter</param>
		/// <param name="custom03">Provider-specific parameter</param>
		/// <param name="custom04">Provider-specific parameter</param>
		/// <param name="custom05">Provider-specific parameter</param>
		/// <returns>True if connection information is good. False, or throws exception if information is not good.</returns>
		public bool TestConnection(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string cacheFolder, string custom01, string custom02, string custom03, string custom04, string custom05)
		{
			throw new NotImplementedException();
		}
	}
}
