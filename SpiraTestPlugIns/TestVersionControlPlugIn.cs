using System;
using System.Net;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Inflectra.SpiraTest.PlugIns
{
    /// <summary>A dummy version control provider used to running the unit tests against</summary>
    public class TestVersionControlProvider : IVersionControlPlugIn
    {
        private static Random rnd = new Random();
        protected static int highestFileId = 0;
        protected static EventLog applicationEventLog = null;

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
        /// <remarks>Throws an exception if unable to connect or authenticate</remarks>
        public object Initialize(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string custom01, string custom02, string custom03, string custom04, string custom05)
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
            return token;
        }

        /// <summary>
        /// Retrieves the parent folder of the passed-in file
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="fileKey">The file identifier</param>
        /// <returns>Single version control folder</returns>
        public VersionControlFolder RetrieveFolderByFile(object token, string fileKey)
        {
            //We just get the file path and remove the last part (the file node)
            Uri uri = new Uri(fileKey);
            VersionControlFolder versionControlFolder = new VersionControlFolder();
            if (uri.Segments.Length < 2)
            {
                versionControlFolder.Name = "Root Folder";
                versionControlFolder.FolderKey = "test://";
            }
            else
            {
                versionControlFolder.Name = uri.Segments[uri.Segments.Length - 2].Replace("/", "");
                string folderKey = "";
                for (int i = 0; i < uri.Segments.Length - 1; i++)
                {
                    folderKey += uri.Segments[i];
                }
                versionControlFolder.FolderKey = folderKey;
            }
            return versionControlFolder;
        }

        /// <summary>
        /// Retrieves a single file by its unique key
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="fileKey">The file identifier</param>
        /// <returns>Single version control file</returns>
        public VersionControlFile RetrieveFile(object token, string fileKey)
        {
            //For this test provider, just get the list of files and then find the matching one
            List<VersionControlFile> versionControlFiles = this.RetrieveFilesByFolder(token, "", "", true, null);
            foreach (VersionControlFile versionControlFile in versionControlFiles)
            {
                if (versionControlFile.FileKey == fileKey)
                {
                    return versionControlFile;
                }
            }
            //Otherwise throw a not found exception
            throw new VersionControlArtifactNotFoundException("Could not find file '" + fileKey + "'");
        }

        /// <summary>
        /// Opens the contents of a single file by its key, if the revision is specified, need to return the
        /// details of the file for that specific revision, otherwise just return the most recent
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="fileKey">The file identifier</param>
        /// <param name="revisionKey">The revision identifier (optional)</param>
        /// <returns></returns>
        public VersionControlFileStream OpenFile(object token, string fileKey, string revisionKey)
        {
            //For this dummy provider we just need to create a new in-memory stream, one for latest revision
            //and one for a specific one
            string dummyText = "";
            if (revisionKey == "")
            {
                dummyText = "Latest Revision";
            }
            else
            {
                dummyText = "Specific Revision";
            }

            byte[] buffer = ASCIIEncoding.UTF8.GetBytes(dummyText);
            MemoryStream memoryStream = new MemoryStream(buffer);

            VersionControlFileStream versionControlFileStream = new VersionControlFileStream();
            versionControlFileStream.FileKey = fileKey;
            versionControlFileStream.RevisionKey = revisionKey;
            versionControlFileStream.LocalPath = "";    //Not used by this provider since memory stream
            versionControlFileStream.DataStream = memoryStream;
            return versionControlFileStream;
        }

        /// <summary>
        /// Closes the data stream provided by OpenFile. Clients must NOT CLOSE THE STREAM DIRECTLY
        /// </summary>
        /// <param name="versionControlFileStream">The stream to be closed</param>
        public void CloseFile(VersionControlFileStream versionControlFileStream)
        {
            versionControlFileStream.DataStream.Close();
        }

        /// <summary>
        /// Retrieves a single folder by its unique key
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="folderKey">The folder identifier</param>
        /// <returns>Single version control folder</returns>
        public VersionControlFolder RetrieveFolder(object token, string folderKey)
        {
            try
            {
                //Just strip of the last part of the fake URI
                Uri uri = new Uri(folderKey);
                VersionControlFolder versionControlFolder = new VersionControlFolder();
                versionControlFolder.FolderKey = folderKey;
                versionControlFolder.Name = uri.Segments[uri.Segments.Length - 1];
                return versionControlFolder;
            }
            catch (Exception exception)
            {
                //Throw an unable to get artifact exception
                throw new VersionControlArtifactNotFoundException("Unable to retrieve folder '" + folderKey + "'", exception);
            }
        }

        /// <summary>
        /// Retrieves a list of folders under the passed in parent folder
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="parentFolderKey">The parent folder (or NullString if root folders requested)</param>
        /// <returns>List of version control folders</returns>
        public List<VersionControlFolder> RetrieveFolders(object token, string parentFolderKey)
        {
            //Create a list of folders based on what's passed in
            //Ge tthe connection string..
            string conn = (string)((Dictionary<string, object>)token)["connection"];
            List<VersionControlFolder> versionControlFolders = new List<VersionControlFolder>();
            if (String.IsNullOrEmpty(parentFolderKey) || parentFolderKey.Equals(conn))
            {
                versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Design", "Design"));
                versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Development", "Development"));
                versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Test", "Test"));
                versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Documentation", "Documentation"));
                versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Training", "Training"));
            }
            else
            {
                if (parentFolderKey == "test://Server/Root/Design")
                {
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Design/Business", "Business Design"));
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Design/Technical", "Technical Design"));
                }
                if (parentFolderKey == "test://Server/Root/Documentation")
                {
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Documentation/EndUser", "End User"));
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Documentation/Technical", "Technical"));
                }
                if (parentFolderKey == "test://Server/Root/Documentation/EndUser")
                {
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Documentation/EndUser/Presentations", "Presentations"));
                    versionControlFolders.Add(new VersionControlFolder("test://Server/Root/Documentation/EndUser/Manuals", "Manuals"));
                }
            }
            return versionControlFolders;
        }

        /// <summary>
        /// Retrieves a list of revisions for a specific file
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="filters">Any filters to apply to the results</param>
        /// <param name="sortAscending">Do we want to sort the results ascending or descending</param>
        /// <param name="sortProperty">What fields do we want to sort the results on</param>
        /// <param name="fileKey">The key of the file</param>
        /// <returns>List of revisions</returns>
        /// <remarks>For this test provider, it always returns the same list, which is a subset of the total list of revisions</remarks>
        public List<VersionControlRevision> RetrieveRevisionsForFile(object token, string fileKey, string sortProperty, bool sortAscending, Hashtable filters)
        {
            List<VersionControlRevision> retList = new List<VersionControlRevision>();

            //Randomly select a set of revisions for the file being asked for..
            int numRev = rnd.Next(1, 16);

            //Get all revisions.
            List<VersionControlRevision> revisions = this.RetrieveRevisions(token, sortProperty, sortAscending, filters);

            //Now get the revisions..
            for (int i = 1; i <= numRev; i++)
            {
                //Get a random revision..
                int revId = rnd.Next(1, 16);
                VersionControlRevision rev = revisions[revId];
                if (!retList.Any(r => r.RevisionKey.Equals(rev.RevisionKey)))
                    retList.Add(rev);
            }

            return retList;
        }

        /// <summary>
        /// Retrieves a list of source code revisions associated with a specific SpiraTeam artifact
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="artifactPrefix">The two-letter prefix for the artifact</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <returns>A list of associated revisions</returns>
        /// <remarks>No longer used, the SourceCodeManager now handles internally</remarks>
        public List<VersionControlRevision> RetrieveRevisionsForArtifact(object token, string artifactPrefix, int artifactId)
        {
            throw new NotImplementedException("Not Used Anymore");
        }

        /// <summary>
        /// Retrieves a list of artifact associations for the specified revision
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="revisionKey">The key of the revision</param>
        /// <returns>A list of artifact associations</returns>
        /// <remarks>No longer used, the SourceCodeManager now handles internally</remarks>
        public List<VersionControlRevisionAssociation> RetrieveAssociationsForRevision(object token, string revisionKey)
        {
            throw new NotImplementedException("Not Used Anymore");
        }

        /// <summary>
        /// Retrieves a single revision by its key
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="revisionKey">The revision identifier</param>
        /// <returns>The revision requested</returns>
        public VersionControlRevision RetrieveRevision(object token, string revisionKey)
        {
            //For this test provider, just get the list of revisions and then find the matching one
            List<VersionControlRevision> versionControlRevisions = this.RetrieveRevisions(token, "", true, null);
            foreach (VersionControlRevision versionControlRevision in versionControlRevisions)
            {
                if (versionControlRevision.RevisionKey == revisionKey)
                {
                    return versionControlRevision;
                }
            }
            //Otherwise throw a not found exception
            throw new VersionControlArtifactNotFoundException("Could not find revision '" + revisionKey + "'");
        }

        /// <summary>
        /// Retrieves a list of revisions for the current repository
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="filters">Any filters to apply to the results</param>
        /// <param name="sortAscending">Do we want to sort the results ascending or descending</param>
        /// <param name="sortProperty">What fields do we want to sort the results on</param>
        /// <returns>List of revisions</returns>
        /// <remarks>For this test provider, it always returns the same list</remarks>
        public List<VersionControlRevision> RetrieveRevisions(object token, string sortProperty, bool sortAscending, Hashtable filters)
        {
            //First create the list
            List<VersionControlRevision> versionControlRevisions = new List<VersionControlRevision>();
            versionControlRevisions.Add(new VersionControlRevision("0001", "rev0001", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow, true, true));
            versionControlRevisions.Add(new VersionControlRevision("0002", "rev0002", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(1), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0003", "rev0003", "Fred Bloggs", "Fixes [IN:7] and [IN:8] and implements requirement [RQ:5].", DateTime.UtcNow.AddSeconds(2), false, false));
            versionControlRevisions.Add(new VersionControlRevision("0004", "rev0004", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(3), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0005", "rev0005", "Fred Bloggs", "Completes task [TK:2] and fixes bug [IN:7].", DateTime.UtcNow.AddSeconds(4), false, false));
            versionControlRevisions.Add(new VersionControlRevision("0006", "rev0006", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(5), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0007", "rev0007", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(6), true, true));
            versionControlRevisions.Add(new VersionControlRevision("0008", "rev0008", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(7), true, true));
            versionControlRevisions.Add(new VersionControlRevision("0009", "rev0009", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(8), true, true));
            versionControlRevisions.Add(new VersionControlRevision("0010", "rev0010", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(9), true, true));
            versionControlRevisions.Add(new VersionControlRevision("0011", "rev0011", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(10), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0012", "rev0012", "Fred Bloggs", "Implements requirement [RQ:5] and also completes task [TK:1].", DateTime.UtcNow.AddSeconds(11), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0013", "rev0013", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(12), false, true));
            versionControlRevisions.Add(new VersionControlRevision("0014", "rev0014", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(13), false, true));
            versionControlRevisions.Add(new VersionControlRevision("0015", "rev0015", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(14), true, false));
            versionControlRevisions.Add(new VersionControlRevision("0016", "rev0016", "Fred Bloggs", "The artifact was changed in this version to fix the issue with the data access component", DateTime.UtcNow.AddSeconds(15), true, false));

            //Now see if we need to filter the results
            if (filters != null)
            {
                List<VersionControlRevision> filteredVersionControlRevisions = new List<VersionControlRevision>();
                foreach (VersionControlRevision versionControlRevision in versionControlRevisions)
                {
                    bool match = true;
                    //Name filtering
                    if (filters["Name"] != null)
                    {
                        string filterText = (string)filters["Name"];
                        if (!versionControlRevision.Name.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //Author filtering
                    if (filters["Author"] != null)
                    {
                        string filterText = (string)filters["Author"];
                        if (!versionControlRevision.Author.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //Message filtering
                    if (filters["Message"] != null)
                    {
                        string filterText = (string)filters["Message"];
                        if (!versionControlRevision.Message.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //UpdateDate filtering
                    if (filters["UpdateDate"] != null)
                    {
                        DateRange filterRange = (DateRange)filters["UpdateDate"];
                        if (filterRange.StartDate.HasValue && versionControlRevision.UpdateDate < filterRange.StartDate.Value.Date)
                        {
                            match = false;
                        }
                        if (filterRange.EndDate.HasValue && versionControlRevision.UpdateDate > filterRange.EndDate.Value.Date)
                        {
                            match = false;
                        }
                    }
                    //ContentChanged filtering
                    if (filters["ContentChanged"] != null)
                    {
                        string flagYn = (string)filters["ContentChanged"];
                        if ((versionControlRevision.ContentChanged && flagYn == "N") || (!versionControlRevision.ContentChanged && flagYn == "Y"))
                        {
                            match = false;
                        }
                    }
                    //PropertiesChanged filtering
                    if (filters["PropertiesChanged"] != null)
                    {
                        string flagYn = (string)filters["PropertiesChanged"];
                        if ((versionControlRevision.PropertiesChanged && flagYn == "N") || (!versionControlRevision.PropertiesChanged && flagYn == "Y"))
                        {
                            match = false;
                        }
                    }

                    //Add the item if we have a match
                    if (match)
                    {
                        filteredVersionControlRevisions.Add(versionControlRevision);
                    }
                }
                versionControlRevisions = filteredVersionControlRevisions;
            }

            //Now see if we need to sort it
            if (String.IsNullOrEmpty(sortProperty))
            {
                //Use the default sort
                versionControlRevisions.Sort();
            }
            else
            {
                if (sortProperty == "Name")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.NameAscComparison : VersionControlRevision.NameDescComparison);
                }
                if (sortProperty == "Author")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.AuthorAscComparison : VersionControlRevision.AuthorDescComparison);
                }
                if (sortProperty == "Message")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.MessageAscComparison : VersionControlRevision.MessageDescComparison);
                }
                if (sortProperty == "UpdateDate")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.UpdateDateAscComparison : VersionControlRevision.UpdateDateDescComparison);
                }
                if (sortProperty == "ContentChanged")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.ContentChangedAscComparison : VersionControlRevision.ContentChangedDescComparison);
                }
                if (sortProperty == "PropertiesChanged")
                {
                    versionControlRevisions.Sort((sortAscending) ? VersionControlRevision.PropertiesChangedAscComparison : VersionControlRevision.PropertiesChangedDescComparison);
                }
            }

            return versionControlRevisions;
        }

        /// <summary>
        /// Retrieves a list of files for a specific revision
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="revisionKey">The revision we want the files for</param>
        /// <param name="filters">Any filters to apply to the results</param>
        /// <param name="sortAscending">Do we want to sort the results ascending or descending</param>
        /// <param name="sortProperty">What fields do we want to sort the results on</param>
        /// <returns>List of files</returns>
        /// <remarks>For this test provider, it always returns the same list</remarks>
        public List<VersionControlFile> RetrieveFilesForRevision(object token, string revisionKey, string sortProperty, bool sortAscending, Hashtable filters)
        {
            return this.RetrieveFilesByFolder(token, "", sortProperty, sortAscending, filters);
        }

        /// <summary>
        /// Retrieves a list of files for a specific folder
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="folderKey">The folder we want the files for</param>
        /// <param name="filters">Any filters to apply to the results</param>
        /// <param name="sortAscending">Do we want to sort the results ascending or descending</param>
        /// <param name="sortProperty">What fields do we want to sort the results on</param>
        /// <returns>List of files</returns>
        /// <remarks>For this test provider, it always returns the same list</remarks>
        public List<VersionControlFile> RetrieveFilesByFolder(object token, string folderKey, string sortProperty, bool sortAscending, Hashtable filters)
        {
            List<VersionControlFile> versionControlFiles = new List<VersionControlFile>();

            //Generate a random list of files..
            int numFile = rnd.Next(5, 30);
            for (int i = 0; i <= numFile; i++)
            {
                //Add one to our main counter.
                TestVersionControlProvider.highestFileId++;

                VersionControlFile newFile = new VersionControlFile();
                newFile.FileKey = folderKey + "/" + TestVersionControlProvider.highestFileId.ToString() + ".ext";
                newFile.Size = rnd.Next(1, 4000);
                newFile.LastUpdated = DateTime.Now;

                //Rev #
                int revNum = rnd.Next(1, 16);
                newFile.Revision = "rev" + revNum.ToString("0000");
                newFile.RevisionKey = revNum.ToString("0000");

                //Filename..
                int extNum = rnd.Next(1, 24);
                newFile.Name = "Document Filename " + TestVersionControlProvider.highestFileId.ToString() + ".";
                switch (extNum)
                {
                    case 1:
                        newFile.Name += "doc";
                        break;
                    case 2:
                        newFile.Name += "xls";
                        break;
                    case 3:
                        newFile.Name += "docx";
                        break;
                    case 4:
                        newFile.Name += "xlsx";
                        break;
                    case 5:
                        newFile.Name += "ppt";
                        break;
                    case 6:
                        newFile.Name += "txt";
                        break;
                    case 7:
                        newFile.Name += "ai";
                        break;
                    case 8:
                        newFile.Name += "pdf";
                        break;
                    case 9:
                        newFile.Name += "vsd";
                        break;
                    case 10:
                        newFile.Name += "pptx";
                        break;
                    case 11:
                        newFile.Name += "htm";
                        break;
                    case 12:
                        newFile.Name += "cs";
                        break;
                    case 13:
                        newFile.Name += "vb";
                        break;
                    case 14:
                        newFile.Name += "cpp";
                        break;
                    case 15:
                        newFile.Name += "java";
                        break;
                    case 16:
                        newFile.Name += "pl";
                        break;
                    case 17:
                        newFile.Name += "php";
                        break;
                    case 18:
                        newFile.Name += "exe";
                        break;
                    case 19:
                        newFile.Name += "rb";
                        break;
                    case 20:
                        newFile.Name += "aspx";
                        break;
                    case 21:
                        newFile.Name += "asp";
                        break;
                    case 22:
                        newFile.Name += "py";
                        break;
                    case 23:
                        newFile.Name += "h";
                        break;
                    case 24:
                        newFile.Name += "obj";
                        break;
                }

                //Action..
                int actNum = rnd.Next(1, 6);
                switch (actNum)
                {
                    case 1:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Added;
                        break;
                    case 2:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Deleted;
                        break;
                    case 3:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Modified;
                        break;
                    case 4:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Other;
                        break;
                    case 5:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Replaced;
                        break;
                    case 6:
                        newFile.Action = VersionControlFile.VersionControlActionEnum.Undefined;
                        break;
                }

                //Author..
                int autNum = rnd.Next(1, 3);
                switch (autNum)
                {
                    case 1:
                        newFile.Author = "Administrator";
                        break;
                    case 2:
                        newFile.Author = "John Adams";
                        break;
                    case 3:
                        newFile.Author = "Malcolm Reynolds";
                        break;
                }

                //Add it to the list.
                versionControlFiles.Add(newFile);
            }

            //Now see if we need to filter the results
            if (filters != null)
            {
                List<VersionControlFile> filteredVersionControlFiles = new List<VersionControlFile>();
                foreach (VersionControlFile versionControlFile in versionControlFiles)
                {
                    bool match = true;
                    //Name filtering
                    if (filters["Name"] != null)
                    {
                        string filterText = (string)filters["Name"];
                        if (!versionControlFile.Name.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //Size filtering
                    if (filters["Size"] != null)
                    {
                        int filterValue = (int)filters["Size"];
                        if (versionControlFile.Size != filterValue)
                        {
                            match = false;
                        }
                    }
                    //Author filtering
                    if (filters["Author"] != null)
                    {
                        string filterText = (string)filters["Author"];
                        if (!versionControlFile.Author.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //Revision filtering
                    if (filters["Revision"] != null)
                    {
                        string filterText = (string)filters["Revision"];
                        if (!versionControlFile.Revision.Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //Action filtering
                    if (filters["Action"] != null)
                    {
                        string filterText = (string)filters["Action"];
                        if (!versionControlFile.Action.ToString().Contains(filterText))
                        {
                            match = false;
                        }
                    }
                    //LastUpdated filtering
                    if (filters["LastUpdated"] != null)
                    {
                        DateRange filterRange = (DateRange)filters["LastUpdated"];
                        if (filterRange.StartDate.HasValue && versionControlFile.LastUpdated < filterRange.StartDate.Value.Date)
                        {
                            match = false;
                        }
                        if (filterRange.EndDate.HasValue && versionControlFile.LastUpdated > filterRange.EndDate.Value.Date)
                        {
                            match = false;
                        }
                    }

                    //Add the item if we have a match
                    if (match)
                    {
                        filteredVersionControlFiles.Add(versionControlFile);
                    }
                }
                versionControlFiles = filteredVersionControlFiles;
            }

            //Now see if we need to sort it
            if (String.IsNullOrEmpty(sortProperty))
            {
                //Use the default sort
                versionControlFiles.Sort();
            }
            else
            {
                if (sortProperty == "Name")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.NameAscComparison : VersionControlFile.NameDescComparison);
                }
                if (sortProperty == "Author")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.AuthorAscComparison : VersionControlFile.AuthorDescComparison);
                }
                if (sortProperty == "Revision")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.RevisionAscComparison : VersionControlFile.RevisionDescComparison);
                }
                if (sortProperty == "Size")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.SizeAscComparison : VersionControlFile.SizeDescComparison);
                }
                if (sortProperty == "LastUpdated")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.LastUpdatedAscComparison : VersionControlFile.LastUpdatedDescComparison);
                }
                if (sortProperty == "Action")
                {
                    versionControlFiles.Sort((sortAscending) ? VersionControlFile.ActionAscComparison : VersionControlFile.ActionDescComparison);
                }
            }

            return versionControlFiles;
        }

        /// <summary>
        /// Gets the key of the parent folder to the folder passed in
        /// </summary>
        /// <param name="token">The connection token for the provider</param>
        /// <param name="folderKey">The key of the folder whos parent we want</param>
        /// <returns>The key of the parent folder (nullstring if no parent)</returns>
        public string RetrieveParentFolderKey(object token, string folderKey)
        {
            //Hard code the data since this is a test provider
            switch (folderKey)
            {
                case "test://Server/Root/Design":
                case "test://Server/Root/Development":
                case "test://Server/Root/Test":
                case "test://Server/Root/Documentation":
                case "test://Server/Root/Training":
                    return "";

                case "test://Server/Root/Design/Business":
                case "test://Server/Root/Design/Technical":
                    return "test://Server/Root/Design";

                case "test://Server/Root/Documentation/EndUser":
                case "test://Server/Root/Documentation/Technical":
                    return "test://Server/Root/Documentation";

                case "test://Server/Root/Documentation/EndUser/Presentations":
                case "test://Server/Root/Documentation/EndUser/Manuals":
                    return "test://Server/Root/Documentation/EndUser";
            }
            return "";
        }
    }
}
