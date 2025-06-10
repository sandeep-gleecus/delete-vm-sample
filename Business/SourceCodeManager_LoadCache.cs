using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.PlugIns;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    public partial class SourceCodeManager : IDisposable
    {
        private static string folderName = "VersionControlCache";
        private static bool cacheUpdateRunning;
        private static readonly object lockObj = new object();
        private int load_folderId, load_fileId, load_commitId;

        /// <summary>Is a cache update running</summary>
        public static bool IsCacheUpdateRunning
        {
            get
            {
                return cacheUpdateRunning;
            }
        }

        public static bool IsCacheUpdateAlive
        {
            get
            {
                return cacheUpdate.IsAlive;
            }
        }

        /// <summary>
        /// Is the cache initialized for the specific project
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <returns>True if we have a cache already</returns>
        public static bool IsInitialized(int projectId)
        {
            return projectCaches.ContainsKey(projectId);
        }

        /// <summary>The static variable to hold the last cache check date/time.</summary>
        private static Dictionary<string, DateTime> cache_dates = new Dictionary<string, DateTime>();

        /// <summary>
        /// A lot of work is done here. Cache files are deleted from the hard disk, then new information is gotten from the SourceControlProvider.
        /// Some of the data that is either shown frequently, or used with other artifacts (e.g. associations)
        /// May also be cached selectively in the database.
        /// </summary>
        private void RefreshCache()
        {
            const string METHOD_NAME = CLASS_NAME + "RefreshCache()";
            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                //Initialize the setting, first..
                if (String.IsNullOrWhiteSpace(ConfigurationSettings.Default.Cache_Folder))
                {
                    ConfigurationSettings.Default.Cache_Folder = Common.Global.CACHE_FOLDERPATH;
                    ConfigurationSettings.Default.Save();
                }
                String cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);

                //Set flag..
                if (!SourceCodeManager.cacheUpdateRunning)
                {
                    SourceCodeManager.cacheUpdateRunning = true;
                    Logger.LogTraceEvent(METHOD_NAME, "Launching refresh of cache. Cache Count: " + this.cachedData.Count);

                    bool lockTaken = false;
                    try
                    {
                        Monitor.TryEnter(lockObj, new TimeSpan(0, 0, 1), ref lockTaken);
                        if (!lockTaken)
                            throw new TimeoutException();

                        //Create directory if necessary..
                        string wildcardMatch = this.repositoryName + "_" + this.projectId + "_*.cache";
                        Logger.LogInformationalEvent(METHOD_NAME, "Starting background thread to update Source code Cache '" + this.repositoryName + "_" + this.projectId + "'");
                        if (!Directory.Exists(cacheFolder))
                        {
                            //No folder exists, first create the directory.
                            Directory.CreateDirectory(cacheFolder);
                        }

                        //Launch the v1 or v2 method..
                        if (isV2)
                        {
                            //V2 providers support full and partial rebuilds and multiple branches
                            this.RefreshCache_v2(folderName, wildcardMatch);

                            //The V2 version includes the code to write out the cache file, as each branch is ready
                        }
                        else
                        {
                            //V1 providers only support full rebuilds and single branches
                            this.RefreshCache_v1();

                            //Now we have our data, save it to the files..
                            Logger.LogInformationalEvent(METHOD_NAME, "Writing updated cache for '" + this.repositoryName + "_" + this.projectId + "'");
                            //Delete existing files first..
                            string[] files = Directory.GetFiles(cacheFolder, wildcardMatch);
                            foreach (string file in files)
                            {
                                File.Delete(file);
                            }
                            //Now write out the cache to our files..
                            foreach (KeyValuePair<string, SourceCodeCache> kvp in this.cachedData)
                            {
                                WriteCacheFile(cacheFolder, kvp);
                            }

                            //Finally remove any in-memory cache
                            if (projectCaches.ContainsKey(this.projectId))
                            {
                                projectCaches.Remove(this.projectId);
                            }
                        }
                    }
                    catch (TimeoutException ex1)
                    {
                        Logger.LogWarningEvent(METHOD_NAME, ex1, "Another SourceCodeUpdate already running.");
                        throw;
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogErrorEvent(METHOD_NAME, ex2, "Error while running SourceCodeUpdate.");
                        throw;
                    }
                    finally
                    {
                        //Clear out the cache 
                        if (lockTaken)
                            Monitor.Exit(lockObj);
                    }
                    SourceCodeManager.cacheUpdateRunning = false;
                }
                else
                {
                    Logger.LogTraceEvent(METHOD_NAME, "Asked to update cache when update already in progress.");
                }
            }
            catch (Exception ex3)
            {
                //Log don't throw since it's a background process (causes unhandled exception
                Logger.LogErrorEvent(METHOD_NAME, ex3, "Error while running SourceCodeUpdate.");
                SourceCodeManager.cacheUpdateRunning = false;
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        private void WriteCacheFile(string cacheFolder, KeyValuePair<string, SourceCodeCache> kvp)
        {
            const string METHOD_NAME = CLASS_NAME + "WriteCacheFile()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Update the cache date here. We use the end-of-cache date instead of when the update was originally fired off.
            kvp.Value.CacheDate = DateTime.UtcNow;

            //Need to make the branch filename safe for writing to the cache. (replace all non-alpha with underscores)
            string branchName = kvp.Key;
            string branchCacheFilename = Regex.Replace(branchName, @"\W|_", "_", RegexOptions.CultureInvariant);

            //Generate our filename, open the file..
            string file = Path.Combine(cacheFolder, this.repositoryName + "_" + this.projectId + "_" + branchCacheFilename + ".cache");
            StreamWriter writer = new StreamWriter(file, false);

            //Create the serializer..
            XmlSerializerFactory fact = new XmlSerializerFactory();
            XmlSerializer ser = fact.CreateSerializer(typeof(SourceCodeCache));
            ser.Serialize(writer.BaseStream, kvp.Value);

            //Close the file.
            writer.Flush();
            writer.Close();

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>The code that refreshes v2 Cache.</summary>
        private void RefreshCache_v2(string folderName, string wildcardMatch)
        {
            const string METHOD_NAME = CLASS_NAME + "RefreshCache_v2()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Okay, first get a list of the available branches..
            List<PlugIns.VersionControlBranch> branches = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveBranches(this.interfaceToken);
            Logger.LogInformationalEvent(METHOD_NAME, String.Format("Received list of branches ({0}) for project PR{1} and repository '{2}'", branches.Count, this.projectId, this.repositoryName));

            //First loop through cache and remove any branches we have that have been removed.
            for (int i = 0; i < this.cachedData.Count; i++)
            {
                KeyValuePair<string, SourceCodeCache> kvpCache = this.cachedData.ElementAt(i);

                if (branches.Count(b => b.BranchKey.Equals(kvpCache.Key)) < 1)
                {
                    //It did not exist in our loaded cache, so we need to remove it from our loaded data.
                    this.cachedData.Remove(kvpCache.Key);

                    //Remove the cached file.
                    string branchCacheFilename = Regex.Replace(kvpCache.Key, @"\W|_", "_", RegexOptions.CultureInvariant);
                    string cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);
                    string file = Path.Combine(cacheFolder, this.repositoryName + "_" + this.projectId + "_" + branchCacheFilename + ".cache");
                    int tryCount = 0;
                    do
                    {
                        //Try to delete the file.
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            //Error deleting file. Sleep for a little bit and try again.
                            tryCount++;
                            Thread.Sleep(1000);
                        }
                    }
                    while (File.Exists(file) && tryCount < 10);

                    //Finally make the database copy inactive
                    using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                    {
                        var query = from b in context.VersionControlBranches
                                    where b.Name == kvpCache.Key && b.IsActive && b.ProjectId == this.projectId
                                    select b;

                        DataModel.VersionControlBranch vcb = query.FirstOrDefault();
                        if (vcb != null)
                        {
                            //Make inactive
                            vcb.StartTracking();
                            vcb.IsActive = false;
                            context.SaveChanges();
                        }
                    }
                }
            }

            //Now we need to add new branches..
            bool changesMade = false;
            foreach (PlugIns.VersionControlBranch verBranch in branches)
            {
                bool branchChanged = false;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.VersionControlBranches
                                where b.Name == verBranch.BranchKey && b.ProjectId == this.projectId
                                select b;

                    DataModel.VersionControlBranch vcb = query.FirstOrDefault();

                    //Check is branch is in loaded data (both DB and cache files)
                    if (!this.cachedData.ContainsKey(verBranch.BranchKey) || vcb == null)
                    {
                        //Make sure we don't have an IGNORE file in place for this branch
                        string cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);
                        string branchCacheFilename = Regex.Replace(verBranch.BranchKey, @"\W|_", "_", RegexOptions.CultureInvariant);
                        string ignoreFile = Path.Combine(cacheFolder, this.repositoryName + "_" + this.projectId + "_" + branchCacheFilename + ".ignore");
                        if (File.Exists(ignoreFile))
                        {
                            Logger.LogInformationalEvent(METHOD_NAME, "Skipping branch '" + verBranch.BranchKey + "' in project PR" + projectId + " due to .ignore file being present!");
                        }
                        else
                        {
                            //Add to the database as a new branch (or make old one active)
                            if (vcb == null)
                            {
                                vcb = new DataModel.VersionControlBranch();
                                vcb.Name = verBranch.BranchKey;
                                vcb.IsActive = true;
                                vcb.Path = branchCacheFilename;
                                vcb.IsHead = verBranch.IsDefault;
                                vcb.ProjectId = this.projectId;
                                vcb.VersionControlSystemId = this.versionControlSystemId;
                                context.VersionControlBranches.AddObject(vcb);
                                Logger.LogInformationalEvent(METHOD_NAME, "Adding new branch '" + verBranch.BranchKey + "' to project PR" + this.projectId);
                            }
                            else
                            {
                                vcb.StartTracking();
                                vcb.IsActive = true;
                                vcb.Path = branchCacheFilename;
                                vcb.IsHead = verBranch.IsDefault;
                                Logger.LogInformationalEvent(METHOD_NAME, "Making inactive branch '" + verBranch.BranchKey + "' active in project PR" + this.projectId);
                            }
                            context.SaveChanges();

                            //It's not loaded, so it's new.
                            this.LoadCacheNewBranch(verBranch, vcb);
                            branchChanged = true;
                        }
                    }
                    else
                    {
                        //Already exists in our cache. Need to make sure that we have all revisions.
                        //We used to simply remove and then re-add, but too slow to be practical
                        //this.cachedData.Remove(verBranch.BranchKey);
                        //this.loadCacheNewBranch(verBranch);
                        branchChanged = this.LoadCacheUpdateBranch(verBranch, vcb);
                    }
                }

                if (branchChanged)
                {
                    //Now we have our data, save it to the files..
                    Logger.LogInformationalEvent(METHOD_NAME, "Writing updated cache for '" + this.repositoryName + "_" + this.projectId + "_" + verBranch.BranchKey + "'");

                    //Delete existing file first..
                    //Need to make the branch filename safe for writing to the cache. (replace all non-alpha with underscores)
                    string cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);
                    string branchCacheFilename = Regex.Replace(verBranch.BranchKey, @"\W|_", "_", RegexOptions.CultureInvariant);
                    string file = Path.Combine(cacheFolder, branchCacheFilename);
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    //Now write out the cache to our file
                    if (this.cachedData.Any(c => c.Key == verBranch.BranchKey))
                    {
                        KeyValuePair<string, SourceCodeCache> kvp = this.cachedData.Where(c => c.Key == verBranch.BranchKey).FirstOrDefault();
                        WriteCacheFile(cacheFolder, kvp);
                    }

                    //At least one branch has changed
                    changesMade = true;
                }
                else
                {
                    //We need to at least update the date in the cache file, so we write out the file back
                    if (this.cachedData.Any(c => c.Key == verBranch.BranchKey))
                    {
                        string cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);
                        KeyValuePair<string, SourceCodeCache> kvp = this.cachedData.Where(c => c.Key == verBranch.BranchKey).FirstOrDefault();
                        WriteCacheFile(cacheFolder, kvp);
                    }
                }
            }

            if (changesMade)
            {
                //Reload into memory as well
                projectCaches[this.projectId] = this.cachedData;
            }
        }

        /// <summary>The code that refreshes v1 Cache.</summary>
        private void RefreshCache_v1()
        {
            const string METHOD_NAME = CLASS_NAME + "RefreshCache_v1()";
            Logger.LogEnteringEvent(METHOD_NAME);

            if (this.interfaceLibrary != null)
            {
                //Create the main (only) branch..
                this.cachedData = new Dictionary<string, SourceCodeCache>();
                SourceCodeCache cache = new SourceCodeCache();
                cache.BranchKey = DEFAULT_BRANCH;
                cache.CacheDate = DateTime.UtcNow;
                cache.IsBranchDefault = true;
                this.cachedData.Add(DEFAULT_BRANCH, cache);

                //Also add to the database as a new branch (or make existing one active)
                string branchCacheFilename = Regex.Replace(DEFAULT_BRANCH, @"\W|_", "_", RegexOptions.CultureInvariant);
                DataModel.VersionControlBranch vcb;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.VersionControlBranches
                                where b.Name == DEFAULT_BRANCH && b.ProjectId == this.projectId
                                select b;

                    vcb = query.FirstOrDefault();
                    if (vcb == null)
                    {
                        vcb = new DataModel.VersionControlBranch();
                        vcb.Name = DEFAULT_BRANCH;
                        vcb.IsActive = true;
                        vcb.Path = branchCacheFilename;
                        vcb.IsHead = true;
                        vcb.ProjectId = this.projectId;
                        vcb.VersionControlSystemId = this.versionControlSystemId;
                        context.VersionControlBranches.AddObject(vcb);
                    }
                    else
                    {
                        vcb.StartTracking();
                        vcb.IsActive = true;
                        vcb.Path = branchCacheFilename;
                        vcb.IsHead = true;
                    }
                    context.SaveChanges();
                }

                //Reset our indexes..
                this.load_folderId = -1;
                this.load_fileId = -1;
                this.load_commitId = -1;

                List<SourceCodeCommit> commitsAdded = new List<SourceCodeCommit>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Attach branch to context
                    context.VersionControlBranches.Attach(vcb);

                    //We don't delete all the existing revisions because otherwise the associations will get lost,
                    //so we have to do it selectively

                    //Get all our revisions..
                    List < VersionControlRevision > revs = ((IVersionControlPlugIn)this.interfaceLibrary).RetrieveRevisions(this.interfaceToken, FIELD_LASTUPDATED, true, new Hashtable());
                    foreach (VersionControlRevision rev in revs)
                    {
                        //See if this commit already exists
                        var query = from c in context.SourceCodeCommits.Include(s => s.Branches)
                                    where c.Revisionkey == rev.RevisionKey && c.ProjectId == this.projectId
                                    select c;

                        SourceCodeCommit commit = query.FirstOrDefault();
                        if (commit == null)
                        {
                            //Add the new commit
                            //Get the source-code equavialent.
                            commit = this.FromVersionControlRevision(rev);

                            //Associate with the one branch
                            commit.Branches.Add(vcb);

                            //Add it to our cache..
                            context.SourceCodeCommits.AddObject(commit);

                            //Add the commit for use later
                            commitsAdded.Add(commit);
                        }
                        //There is no ELSE case for v1 caches because there is only ever 1 branch.
                    }

                    //Save
                    context.SaveChanges();
                }

                //We need to get the root folder information, first..
                VersionControlFolder rootVFolder = ((IVersionControlPlugIn)this.interfaceLibrary).RetrieveFolder(this.interfaceToken, this.interfaceSettings.Connection);
                SourceCodeFolder rootFolder = this.FromVersionControlFolder(rootVFolder);
                rootFolder.IsRoot = true;

                //Add it to our list.
                this.cachedData[DEFAULT_BRANCH].Folders.Add(rootFolder);

                //Now call our recursive function..
                this.GetFoldersAndFilesforFolder(rootFolder);

                //Now make a second pass through the commits to add the file references
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    foreach (SourceCodeCommit commit in commitsAdded)
                    {
                        //Get all files in the revision
                        List<VersionControlFile> vcFiles = ((IVersionControlPlugIn)this.interfaceLibrary).RetrieveFilesForRevision(this.interfaceToken, commit.Revisionkey, FIELD_LASTUPDATED, true, new Hashtable());
                        foreach (VersionControlFile vcFile in vcFiles)
                        {
                            //We don't need to convert it, we need to find it already in our list..
                            SourceCodeFile scFile = this.cachedData[DEFAULT_BRANCH].Files.FirstOrDefault(f => f.FileKey.Equals(vcFile.FileKey));
                            if (scFile != null)
                            {
                                SourceCodeFileEntry scFileEntry = new SourceCodeFileEntry();
                                context.SourceCodeFileEntries.AddObject(scFileEntry);
                                scFileEntry.FileKey = scFile.FileKey;
                                scFileEntry.Action = vcFile.Action.ToSafeString();
                                scFileEntry.RevisionId = commit.RevisionId;
                                scFileEntry.ProjectId = commit.ProjectId;
                                scFileEntry.VersionControlSystemId = commit.VersionControlSystemId;
                            }
                        }
                    }

                    //Commit the changes
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Returns the parent directory of a path, ignoring invalid characters
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>Uses / as the path separator, not \</remarks>
        private static string GetDirectoryNameAnyChars(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return "";
            }
            int index = path.LastIndexOf('/');
            if (index < 0)
            {
                return path;
            }
            return path.Substring(0, index);
        }

        /// <summary>Gets all folders and files that exist in the given folderkey.</summary>
        /// <param name="rootFolder">The root folder</param>
        /// <param name="branchKey">The branch</param>
        /// <remarks>
        /// </remarks>
        private void GetFoldersAndFilesV3(SourceCodeFolder rootFolder, string branchKey)
        {
            const string METHOD_NAME = "GetFoldersAndFilesV3";

            //Load up the entire file system
            List<VersionControlDirectoryEntry> vcDirectoryEntries = ((IVersionControlPlugIn3)this.interfaceLibrary).RetrieveFolderFileHierarchy(this.interfaceToken, branchKey, rootFolder.FolderKey);
            Logger.LogInformationalEvent(METHOD_NAME, String.Format("Adding {1} directory entries to branch '{0}' in project PR{2}.", branchKey, vcDirectoryEntries.Count, this.projectId));

            //First we need to get all the files
            List<VersionControlDirectoryEntry> vcFiles = vcDirectoryEntries.Where(d => !d.IsFolder).ToList();

            //For each file, convert it to a SourceCodeFile and add it to the cache
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                foreach (VersionControlDirectoryEntry vcFile in vcFiles)
                {
                    SourceCodeFile file = this.FromVersionControlFile(vcFile);

                    //Get the latest revision's ID..
                    //Some providers return the latest revision for the file, others require us to deduce it from the cache
                    if (String.IsNullOrEmpty(vcFile.RevisionKey))
                    {
                        //Get the latest revision for this file in the branch
                        var query = from s in context.SourceCodeCommits
                                    where
                                        s.Files.Any(f => f.FileKey == vcFile.PathKey) &&
                                        s.ProjectId == this.projectId &&
                                        s.VersionControlSystemId == this.versionControlSystemId &&
                                        s.Branches.Any(b => b.Name == branchKey)
                                    orderby s.UpdateDate descending
                                    select s;

                        SourceCodeCommit comm = query.FirstOrDefault();
                        if (comm != null)
                        {
                            file.RevisionId = comm.RevisionId;
                            file.RevisionKey = comm.Revisionkey;
                            file.RevisionName = comm.Name;
                            file.LastUpdateDate = comm.UpdateDate;
                            file.AuthorName = comm.AuthorName;
                        }
                    }
                    else
                    {
                        //Just get the commit by its key
                        var query = from c in context.SourceCodeCommits
                                    where c.Revisionkey == vcFile.RevisionKey && c.ProjectId == this.projectId && c.VersionControlSystemId == this.versionControlSystemId
                                    select c;

                        SourceCodeCommit comm = query.FirstOrDefault();
                        if (comm != null)
                        {
                            file.RevisionId = comm.RevisionId;
                            file.RevisionKey = comm.Revisionkey;
                            file.RevisionName = comm.Name;
                            file.LastUpdateDate = comm.UpdateDate;
                            file.AuthorName = comm.AuthorName;
                        }
                    }

                    //Add it to the cache and the folder's files..
                    this.cachedData[branchKey].Files.Add(file);
                }
            }

            //First we need to get all the folders
            List<VersionControlDirectoryEntry> vcFolders = vcDirectoryEntries.Where(d => d.IsFolder).ToList();

            //For each folder, convert it to a SourceCodeFolder and add it to the given folder.
            foreach (VersionControlDirectoryEntry vcFolder in vcFolders)
            {
                SourceCodeFolder folder = this.FromVersionControlFolder(vcFolder);

                //Add the folder to the cache
                this.cachedData[branchKey].Folders.Add(folder);
            }

            //Now we do a second pass through the folders to link them to parent folders
            foreach (VersionControlDirectoryEntry vcFolder in vcFolders)
            {
                //Locate the folder
                SourceCodeFolder subfolder = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderKey == vcFolder.PathKey);
                if (subfolder != null)
                {
                    //See if we have a parent folder
                    if (vcFolder.PathKey.Contains("/"))
                    {
                        string folderKey = GetDirectoryNameAnyChars(vcFolder.PathKey);
                        SourceCodeFolder parentFolder = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderKey == folderKey);
                        if (parentFolder != null)
                        {
                            parentFolder.ContainedFolders.Add(subfolder.FolderId);
                            subfolder.ParentFolderId = parentFolder.FolderId;
                        }
                        else
                        {
                            rootFolder.ContainedFolders.Add(subfolder.FolderId);
                            subfolder.ParentFolderId = rootFolder.FolderId;
                        }
                    }
                    else
                    {
                        rootFolder.ContainedFolders.Add(subfolder.FolderId);
                        subfolder.ParentFolderId = rootFolder.FolderId;
                    }
                }
            }

            //Now we do a second pass through the files to link them to folders
            foreach (VersionControlDirectoryEntry vcFile in vcFiles)
            {
                //Locate the file
                SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey == vcFile.PathKey);
                if (file != null)
                {
                    //See if we have a parent folder
                    if (vcFile.PathKey.Contains("/"))
                    {
                        string folderKey = GetDirectoryNameAnyChars(vcFile.PathKey);
                        SourceCodeFolder folder = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderKey == folderKey);
                        if (folder != null)
                        {
                            folder.ContainedFiles.Add(file.FileId);
                            file.ParentFolderId = folder.FolderId;
                        }
                        else
                        {
                            rootFolder.ContainedFiles.Add(file.FileId);
                            file.ParentFolderId = rootFolder.FolderId;
                        }
                    }
                    else
                    {
                        rootFolder.ContainedFiles.Add(file.FileId);
                        file.ParentFolderId = rootFolder.FolderId;
                    }
                }
            }
        }

        /// <summary>Gets all folders and files that exist in the given folderkey.</summary>
        /// <param name="folderKey"></param>
        private void GetFoldersAndFilesforFolder(SourceCodeFolder folder, string branchKey)
        {
            if (folder != null)
            {
                //Load up all files for the folder, first.
                List<VersionControlFile> containedFiles = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveFilesByFolder(this.interfaceToken, folder.FolderKey, branchKey);

                //For each file, convert it to a SourceCodeFile and add it to the given folder.
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    foreach (VersionControlFile vFile in containedFiles)
                    {
                        int? fileId = null;
                        //if (existFiles.ContainsKey(vFile.FileKey))
                        //fileId = existFiles[vFile.FileKey];

                        SourceCodeFile file = this.FromVersionControlFile(vFile, fileId);
                        file.ParentFolderId = folder.FolderId;

                        //Get the latest revision's ID..
                        var query = from c in context.SourceCodeCommits
                                    where c.Name == file.RevisionName && c.ProjectId == this.projectId
                                    select c;

                        SourceCodeCommit comm = query.FirstOrDefault();
                        if (comm != null)
                        {
                            file.RevisionId = comm.RevisionId;
                            file.RevisionKey = comm.Revisionkey;
                        }

                        //Add it to the cache and the folder's files..
                        this.cachedData[branchKey].Files.Add(file);
                        folder.ContainedFiles.Add(file.FileId);
                    }
                }

                //Now load up all folders that this folder contains.
                List<VersionControlFolder> containedFolders = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveFolders(this.interfaceToken, folder.FolderKey, branchKey);

                //For each one, convert it, add it to cache, and find items it contains.
                foreach (VersionControlFolder vFolder in containedFolders)
                {
                    //Set folder info..
                    SourceCodeFolder subFolder = this.FromVersionControlFolder(vFolder);
                    subFolder.ParentFolderId = folder.FolderId;

                    //Add it to cache and update main folder.
                    this.cachedData[branchKey].Folders.Add(subFolder);
                    folder.ContainedFolders.Add(subFolder.FolderId);

                    //Call same function, getting info for this folder.
                    this.GetFoldersAndFilesforFolder(subFolder, branchKey);
                }
            }
        }

        /// <summary>Gets all folders and files that exist in the given folderkey.</summary>
        /// <param name="folderKey"></param>
        private void GetFoldersAndFilesforFolder(SourceCodeFolder folder)
        {
            if (folder != null)
            {
                //Load up all files for the folder, first.
                List<VersionControlFile> containedFiles = ((IVersionControlPlugIn)this.interfaceLibrary).RetrieveFilesByFolder(this.interfaceToken, folder.FolderKey, "Name", true, new Hashtable());

                //For each file, convert it to a SourceCodeFile and add it to the given folder.
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    foreach (VersionControlFile vFile in containedFiles)
                    {
                        SourceCodeFile file = this.FromVersionControlFile(vFile);
                        file.ParentFolderId = folder.FolderId;

                        //Get the latest revision's ID..
                        var query = from c in context.SourceCodeCommits
                                    where c.Name == file.RevisionName && c.ProjectId == this.projectId
                                    select c;

                        SourceCodeCommit comm = query.FirstOrDefault();
                        if (comm != null)
                        {
                            file.RevisionId = comm.RevisionId;
                            file.RevisionKey = comm.Revisionkey;
                        }

                        //Add it to the cache and the folder's files..
                        this.cachedData[DEFAULT_BRANCH].Files.Add(file);
                        folder.ContainedFiles.Add(file.FileId);
                    }
                }

                //Now load up all folders that this folder contains.
                List<VersionControlFolder> containedFolders = ((IVersionControlPlugIn)this.interfaceLibrary).RetrieveFolders(this.interfaceToken, folder.FolderKey);

                //For each one, convert it, add it to cache, and find items it contains.
                foreach (VersionControlFolder vFolder in containedFolders)
                {
                    //Set folder info..
                    SourceCodeFolder subFolder = this.FromVersionControlFolder(vFolder);
                    subFolder.ParentFolderId = folder.FolderId;

                    //Add it to cache and update main folder.
                    this.cachedData[DEFAULT_BRANCH].Folders.Add(subFolder);
                    folder.ContainedFolders.Add(subFolder.FolderId);

                    //Call same function, getting info for this folder.
                    this.GetFoldersAndFilesforFolder(subFolder);
                }
            }
        }

        /// <summary>Converts a VersionControlDirectoryEntry to a SourceCodeFolder</summary>
        /// <param name="vFolder">The VersionControlFolder to translate.</param>
        /// <returns>A translated SourceCodeFolder.</returns>
        private SourceCodeFolder FromVersionControlFolder(VersionControlDirectoryEntry vFolder, int? existingId = null)
        {
            SourceCodeFolder retValue = new SourceCodeFolder();

            //Copy over the values..
            retValue.Name = vFolder.Name;
            retValue.FolderKey = vFolder.PathKey;
            if (existingId.HasValue)
                retValue.FolderId = existingId.Value;
            else
                retValue.FolderId = ++this.load_folderId;

            return retValue;
        }

        /// <summary>Converts a VersionControlFolder to a SourceCodeFolder</summary>
        /// <param name="vFolder">The VersionControlFolder to translate.</param>
        /// <returns>A translated SourceCodeFolder.</returns>
        private SourceCodeFolder FromVersionControlFolder(VersionControlFolder vFolder, int? existingId = null)
        {
            SourceCodeFolder retValue = new SourceCodeFolder();

            //Copy over the values..
            retValue.Name = vFolder.Name;
            retValue.FolderKey = vFolder.FolderKey;
            if (existingId.HasValue)
                retValue.FolderId = existingId.Value;
            else
                retValue.FolderId = ++this.load_folderId;

            return retValue;
        }

        /// <summary>Converts the given VersionControlDirectoryEntry into a SourceCodeFile.</summary>
        /// <param name="vFile">The Version file to convert.</param>
        /// <returns>A converted Source file.</returns>
        private SourceCodeFile FromVersionControlFile(VersionControlDirectoryEntry vFile, int? existingId = null)
        {
            SourceCodeFile file = new SourceCodeFile();
            file.AuthorName = vFile.Author;
            file.LastUpdateDate = DateTime.MinValue;
            file.FileKey = vFile.PathKey;
            //file.Path = this.Connection + vFile.PathKey; -- not used and saves space
            file.Name = vFile.Name;
            file.Size = vFile.Size;
            file.Action = "";
            file.RevisionName = vFile.Revision;
            if (existingId.HasValue)
                file.FileId = existingId.Value;
            else
                file.FileId = ++this.load_fileId;

            return file;
        }

        /// <summary>Converts the given versionControlFile into a SourceCodeFile.</summary>
        /// <param name="vFile">The Version file to convert.</param>
        /// <returns>A converted Source file.</returns>
        private SourceCodeFile FromVersionControlFile(VersionControlFile vFile, int? existingId = null)
        {
            SourceCodeFile file = new SourceCodeFile();
            file.AuthorName = vFile.Author;
            file.LastUpdateDate = vFile.LastUpdated;
            file.FileKey = vFile.FileKey;
            //file.Path = this.Connection + vFile.FileKey; -- not used and saves space
            file.Name = vFile.Name;
            file.Size = vFile.Size;
            file.Action = vFile.Action.ToSafeString();
            file.RevisionName = vFile.Revision;
            if (existingId.HasValue)
                file.FileId = existingId.Value;
            else
                file.FileId = ++this.load_fileId;

            return file;
        }

        /// <summary>Converts a VersionControlRevision to a SourceCodeCommit.</summary>
        /// <param name="revision"></param>
        /// <returns></returns>
        private SourceCodeCommit FromVersionControlRevision(PlugIns.VersionControlRevision revision)
        {
            SourceCodeCommit sourceCodeCommit = new SourceCodeCommit();
            sourceCodeCommit.ProjectId = this.projectId;
            sourceCodeCommit.VersionControlSystemId = this.versionControlSystemId;
            sourceCodeCommit.AuthorName = String.IsNullOrEmpty(revision.Author) ? "-" : revision.Author;
            sourceCodeCommit.ContentChanged = revision.ContentChanged;
            sourceCodeCommit.Message = revision.Message;
            sourceCodeCommit.Name = revision.Name;
            sourceCodeCommit.PropertiesChanged = revision.PropertiesChanged;
            sourceCodeCommit.Revisionkey = revision.RevisionKey;
            sourceCodeCommit.UpdateDate = revision.UpdateDate;
            sourceCodeCommit.RevisionId = ++this.load_commitId;

            //Parse the message and add the associations
            ParseCommitMessageForAssociations(sourceCodeCommit);

            return sourceCodeCommit;
        }

        /// <summary>
        /// Parses the commit log for association tokens in the message text
        /// </summary>
        /// <param name="sourceCodeCommit">The revision</param>
        private void ParseCommitMessageForAssociations(SourceCodeCommit sourceCodeCommit)
        {
            //See if we have a message and parse by regex
            if (!String.IsNullOrEmpty(sourceCodeCommit.Message))
            {
                //Get a list of all artifact types
                List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();

                //Search the commit's message for any tokens.
                Regex reg = new Regex(@"\[(?<key>[A-Z]{2})[:\-](?<id>\d*?)\]", RegexOptions.IgnoreCase);

                //Find any matches in the comments..
                if (!string.IsNullOrWhiteSpace(sourceCodeCommit.Message) && reg.IsMatch(sourceCodeCommit.Message))
                {
                    //Get all matches..
                    MatchCollection matches = reg.Matches(sourceCodeCommit.Message);

                    //Loop through each one, and create a record for it.
                    for (int i = 0; i < matches.Count; i++)
                    {
                        //Get our values.
                        string artifactKey = matches[i].Groups["key"].Value.ToUpperInvariant();
                        string artifactId = matches[i].Groups["id"].Value;
                        int numId = 0;
                        if (int.TryParse(artifactId, out numId))
                        {
                            //Get the artifact type..
                            ArtifactType artifactType = artifactTypes.FirstOrDefault(a => a.Prefix == artifactKey);
                            if (artifactType != null && !sourceCodeCommit.AssociatedArtifacts.Any(a => a.ArtifactId == numId && a.ArtifactTypeId == artifactType.ArtifactTypeId))
                            {
                                //The new link object.
                                SourceCodeCommitArtifact sourceCodeCommitArtifact = new SourceCodeCommitArtifact();
                                sourceCodeCommit.AssociatedArtifacts.Add(sourceCodeCommitArtifact);

                                //Set reference to the artifact
                                sourceCodeCommitArtifact.ArtifactId = numId;
                                sourceCodeCommitArtifact.ArtifactTypeId = artifactType.ArtifactTypeId;
                            }
                        }
                    }
                }
            }
        }

        #region New Cache Load Functions (partial cache loads)
        /// <summary>Here we have to load a whole new branch that wasn't originally in our cache.</summary>
        /// <param name="branchKey">The new VersionControlBranch.</param>
        /// <param name="branch">The branch as stored in the database</param>
        private void LoadCacheNewBranch(PlugIns.VersionControlBranch branch, DataModel.VersionControlBranch vcb)
        {
            const string METHOD_NAME = CLASS_NAME + "LoadCacheNewBranch()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Create our Cache object, first. and add it to our cache list.
            if (this.cachedData.ContainsKey(branch.BranchKey))
            {
                Logger.LogWarningEvent(METHOD_NAME, String.Format("Skipping branch '{0}' since it is a duplicate", branch.BranchKey));
            }
            else
            {
                SourceCodeCache newCache = new SourceCodeCache();
                newCache.BranchKey = branch.BranchKey;
                newCache.CacheDate = DateTime.UtcNow;
                newCache.IsBranchDefault = branch.IsDefault;
                this.cachedData.Add(branch.BranchKey, newCache);

                //Set our highest counts. Search existing branches for highest numbers.
                this.load_folderId = -1;
                this.load_fileId = -1;
                this.load_commitId = this.GetLatestRevision();
                if (this.cachedData.Count > 0)
                {
                    foreach (KeyValuePair<string, SourceCodeCache> kvpCache in this.cachedData)
                    {
                        if (kvpCache.Value.HighestFolderId > this.load_folderId) this.load_folderId = kvpCache.Value.HighestFolderId;
                        if (kvpCache.Value.HighestFileId > this.load_fileId) this.load_fileId = kvpCache.Value.HighestFileId;
                    }
                }

                //Okay, now load up this branch.
                string branchKey = branch.BranchKey;

                //Get all our revisions..
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Adding revisions to branch '{0}' in project PR{1} in repository '{2}'", branchKey, this.projectId, this.repositoryName));

                List<SourceCodeCommit> commitsAdded = new List<SourceCodeCommit>();
                try
                {
                    using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                    {
                        //Attach branch to context
                        context.VersionControlBranches.Attach(vcb);

                        List<VersionControlRevision> revs = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveRevisions(this.interfaceToken, branchKey);
                        foreach (VersionControlRevision rev in revs)
                        {
                            //See if this commit already exists
                            var query = from c in context.SourceCodeCommits.Include(s => s.Branches)
                                        where c.Revisionkey == rev.RevisionKey && c.ProjectId == this.projectId
                                        select c;

                            SourceCodeCommit commit = query.FirstOrDefault();
                            if (commit == null)
                            {
                                //Add the new commit
                                //Get the source-code equivalent.
                                commit = this.FromVersionControlRevision(rev);

                                //Associate with the one branch
                                commit.Branches.Add(vcb);

                                //Add it to our cache..
                                context.SourceCodeCommits.AddObject(commit);
                            }
                            else
                            {
                                //Associate with the one branch if not already associated
                                if (!commit.Branches.Any(b => b.Name == vcb.Name))
                                {
                                    commit.Branches.Add(vcb);
                                }
                            }

                            //Add the commit for use later. We include ones already linked to other branches
                            //since files live in each branch, but revisions exist once for all branches
                            commitsAdded.Add(commit);
                        }

                        //Commit changes
                        context.SaveChanges();
                    }
                }
                catch (VersionControlArtifactNotFoundException exception)
                {
                    //Log and continue
                    Logger.LogWarningEvent(METHOD_NAME, exception.Message);
                }
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished adding {1} revisions to branch '{0}' in project PR{2}", branchKey, commitsAdded.Count, this.projectId));

                //Now load our folder info..
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Loading folders and files to branch '{0}' in project PR{1} and repository '{2}'", branchKey, this.projectId, this.repositoryName));
                try
                {
                    this.LoadCacheUpdateBranchFolders(branchKey, new Dictionary<string, int>(), new Dictionary<string, int>());
                }
                catch (VersionControlArtifactNotFoundException exception)
                {
                    //Log and continue
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                }
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished loading folders and files to branch '{0}' in project PR{1} and repository '{2}'", branchKey, this.projectId, this.repositoryName));

                //Now make a second pass through the commits to add the file references
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Adding file references to branch '{0}' in project PR{1} and repository '{2}'", branchKey, this.projectId, this.repositoryName));

                this.LinkCommitsToFileReferences(branchKey, commitsAdded);
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished adding file references to branch '{0}' in project PR{1} and repository '{2}'", branchKey, this.projectId, this.repositoryName));

                //Save the highest IDs we have..
                this.cachedData[branchKey].HighestFolderId = load_folderId;
                this.cachedData[branchKey].HighestFileId = load_fileId;
                //Save the last checked date to the static and the cache object.
                string branchCacheName = this.projectId + "_" + Regex.Replace(branchKey, @"\W|_", "_", RegexOptions.CultureInvariant);
                if (SourceCodeManager.cache_dates.ContainsKey(branchCacheName))
                    SourceCodeManager.cache_dates[branchCacheName] = DateTime.UtcNow;
                else
                    SourceCodeManager.cache_dates.Add(branchCacheName, DateTime.UtcNow);
                newCache.CacheDate = DateTime.UtcNow;
            }

            Logger.LogEnteringEvent(METHOD_NAME);
        }

        /// <summary>This loads up the files & folders for the given BranchKey.</summary>
        /// <param name="branchKey">The branch's key to load the information for.</param>
        private void LoadCacheUpdateBranchFolders(string branchKey, Dictionary<string, int> existFiles, Dictionary<string, int> existFolders)
        {
            const string METHOD_NAME = CLASS_NAME + "loadCacheUpdateBranchFolders()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //We need to get the root folder information, first, and check it's ID..
            VersionControlFolder rootVFolder = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveFolder(this.interfaceToken, this.interfaceSettings.Connection, branchKey);
            int? folderId = null;
            if (existFolders.ContainsKey(rootVFolder.FolderKey))
                folderId = existFolders[rootVFolder.FolderKey];
            SourceCodeFolder rootFolder = this.FromVersionControlFolder(rootVFolder, folderId);
            rootFolder.IsRoot = true;

            //Add it to our list.
            this.cachedData[branchKey].Folders.Add(rootFolder);

            //See if we have the V3 API fast hierarchical viewing function, otherwise we have to use the slower V2 recursive function
            if (this.interfaceLibrary is IVersionControlPlugIn3)
            {
                this.GetFoldersAndFilesV3(rootFolder, branchKey);
            }
            else
            {
                this.GetFoldersAndFilesforFolder(rootFolder, branchKey);
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>A specific branch needs to be updated from the repository.</summary>
        /// <param name="branch">The branch we're updating.</param>
        /// <param name="vcb">The branch as stored in the database</param>
        /// <returns>True if the branch changed</returns>
        private bool LoadCacheUpdateBranch(PlugIns.VersionControlBranch branch, DataModel.VersionControlBranch vcb)
        {
            const string METHOD_NAME = CLASS_NAME + "LoadCacheUpdateBranch()";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Find the date of the latest commit in this branch.. 
            // We cannot change the Interface here, and since different providers
            // handle the 'latest' differently, we need to do processing here.
            List<VersionControlRevision> newRevs = new List<VersionControlRevision>();
            bool branchChanged = false;
            DateTime checkDate = DateTime.MinValue;
            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from c in context.SourceCodeCommits
                                where c.VersionControlProject.ProjectId == projectId && c.Branches.Any(b => b.Name == branch.BranchKey)
                                orderby c.UpdateDate descending, c.RevisionId
                                select c;

                    SourceCodeCommit mostRecentCommit = query.FirstOrDefault();
                    if (mostRecentCommit != null)
                    {
                        checkDate = mostRecentCommit.UpdateDate;
                    }
                }

                //Make sure the date is treated as UTC not local
                checkDate = DateTime.SpecifyKind(checkDate, DateTimeKind.Utc);

                //Log the found date.
                //Logger.LogTraceEvent(METHOD_NAME, String.Format("Looking for revisions on/after: {0:MM/dd/yy H:mm:ss zzz}", checkDate));

                //Get all revisions since then.
                newRevs = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveRevisionsSince(this.interfaceToken, branch.BranchKey, checkDate);
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(METHOD_NAME, ex, "Error getting revisions by date.");
                //We caught an error. Load ALL revisions.
                newRevs = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveRevisions(this.interfaceToken, branch.BranchKey);
            }

            //Set our counts and existing data - 
            // - Since we're ADDING to existing commits, we need the highest existing commit #.
            // - Since we're completely rebuilding files & folders, we need to get a dictionary of key/id
            //     for re-use existing IDs so that existing commits don't need to be remapped.

            //Set start ID's. Since numbers are reused, we get the latest of each one.
            this.load_folderId = -1;
            this.load_fileId = -1;
            this.load_commitId = this.GetLatestRevision();
            if (this.cachedData.Count > 0)
            {
                foreach (KeyValuePair<string, SourceCodeCache> kvpCache in this.cachedData)
                {
                    if (kvpCache.Value.HighestFolderId > this.load_folderId)
                    {
                        this.load_folderId = kvpCache.Value.HighestFolderId;
                    }
                    if (kvpCache.Value.HighestFileId > this.load_fileId)
                    {
                        this.load_fileId = kvpCache.Value.HighestFileId;
                    }
                }
            }

            //Save mapping of existing files & folders..
            Dictionary<string, int> existingFiles = new Dictionary<string, int>();
            foreach (SourceCodeFile file in this.cachedData[branch.BranchKey].Files)
            {
                if (!existingFiles.ContainsKey(file.FileKey))
                {
                    existingFiles.Add(file.FileKey, file.FileId);
                }
            }
            Dictionary<string, int> existingFolders = new Dictionary<string, int>();
            foreach (SourceCodeFolder folder in this.cachedData[branch.BranchKey].Folders)
            {
                if (!existingFolders.ContainsKey(folder.FolderKey))
                {
                    existingFolders.Add(folder.FolderKey, folder.FolderId);
                }
            }

            //Loop through each one and see if it exists in our cache already.
            Logger.LogInformationalEvent(METHOD_NAME, String.Format("Checking {0} revisions found in branch '{2}' in project PR{3} since {1:MM/dd/yy H:mm:ss zzz} to see if they need to be added to cache", newRevs.Count, checkDate, branch.BranchKey, this.projectId));
            bool needtoReprocess = false;
            int addedCount = 0;
            List<SourceCodeCommit> commitsAdded = new List<SourceCodeCommit>();
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                //Attach branch to context
                context.VersionControlBranches.Attach(vcb);

                foreach (VersionControlRevision newRev in newRevs)
                {
                    //See if this commit already exists
                    var query = from c in context.SourceCodeCommits.Include(s => s.Branches)
                                where c.Revisionkey == newRev.RevisionKey && c.ProjectId == this.projectId
                                select c;

                    SourceCodeCommit commit = query.FirstOrDefault();
                    if (commit == null)
                    {
                        //Set flag.
                        needtoReprocess = true;
                        branchChanged = true;

                        //Get the source-code equavialent.
                        commit = this.FromVersionControlRevision(newRev);

                        //Associate with the one branch
                        commit.Branches.Add(vcb);

                        //Add it to our cache..
                        context.SourceCodeCommits.AddObject(commit);

                        //Update the highest ID
                        addedCount++;
                    }
                    else
                    {
                        //Associate with the one branch if not already associated
                        if (!commit.Branches.Any(b => b.Name == vcb.Name))
                        {
                            commit.Branches.Add(vcb);
                        }
                    }

                    //Add the commit for use later. We include ones already linked to other branches
                    //since files live in each branch, but revisions exist once for all branches
                    commitsAdded.Add(commit);
                }

                //Save
                context.SaveChanges();
            }
            Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished adding {0} new revisions to cache for branch '{1}' for project PR{2} and repository '{3}'", addedCount, branch.BranchKey, this.projectId, this.repositoryName));

            if (needtoReprocess)
            {
                //Clear all the files and folders and reload them all.
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Reloading all files/folders for branch '{0}' for project PR{1} and repository '{2}'", branch.BranchKey, this.projectId, this.repositoryName));
                this.cachedData[branch.BranchKey].Files.Clear();
                this.cachedData[branch.BranchKey].Folders.Clear();

                //Reload all files & folders..
                this.LoadCacheUpdateBranchFolders(branch.BranchKey, existingFiles, existingFolders);

                //Now make a second pass through the commits to add the file references
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Adding file references to branch '{0}'for project PR{1} and repository '{2}'", branch.BranchKey, this.projectId, this.repositoryName));
                this.LinkCommitsToFileReferences(branch.BranchKey, commitsAdded);
                Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished adding file references to branch '{0}'for project PR{1} and repository '{2}'", branch.BranchKey, this.projectId, this.repositoryName));
            }
            Logger.LogInformationalEvent(METHOD_NAME, String.Format("Finished reloading all files/folders for project PR{0} and repository '{1}'", this.projectId, this.repositoryName));

            //Save the last checked date to the static and the cache object.
            string branchCacheName = Regex.Replace(branch.BranchKey, @"\W|_", "_", RegexOptions.CultureInvariant);
            if (SourceCodeManager.cache_dates.ContainsKey(branchCacheName))
                SourceCodeManager.cache_dates[branchCacheName] = DateTime.UtcNow;
            else
                SourceCodeManager.cache_dates.Add(branchCacheName, DateTime.UtcNow);

            //Update the last check date in the cache file itself
            this.cachedData[branch.BranchKey].CacheDate = DateTime.UtcNow;

            //Save the highest IDs we have..
            this.cachedData[branch.BranchKey].HighestFolderId = load_folderId;
            this.cachedData[branch.BranchKey].HighestFileId = load_fileId;

            return branchChanged;
        }

        /// <summary>
        /// Link commits to file references
        /// </summary>
        private void LinkCommitsToFileReferences(string branchKey, List<SourceCodeCommit> commitsAdded)
        {
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                //Loop through commits in reverse order
                for (int i = commitsAdded.Count - 1; i >= 0; i--)
                {
                    SourceCodeCommit commit = commitsAdded[i];
                    //Get all files in the revision
                    List<VersionControlFile> vcFiles = ((IVersionControlPlugIn2)this.interfaceLibrary).RetrieveFilesForRevision(this.interfaceToken, commit.Revisionkey, branchKey);
                    foreach (VersionControlFile vcFile in vcFiles)
                    {
                        //We don't need to convert it, we need to find it already in our list..
                        SourceCodeFile scFile = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey.Equals(vcFile.FileKey));
                        if (scFile != null)
                        {
                            //Make sure file not already added
                            var query = from scf in context.SourceCodeFileEntries
                                        where
                                            scf.ProjectId == commit.ProjectId &&
                                            scf.VersionControlSystemId == commit.VersionControlSystemId &&
                                            scf.FileKey == scFile.FileKey &&
                                            scf.RevisionId == commit.RevisionId
                                        select scf;

                            if (query.Count() == 0)
                            {
                                SourceCodeFileEntry scFileEntry = new SourceCodeFileEntry();
                                context.SourceCodeFileEntries.AddObject(scFileEntry);
                                scFileEntry.FileKey = scFile.FileKey;
                                scFileEntry.Action = vcFile.Action.ToSafeString();
                                scFileEntry.RevisionId = commit.RevisionId;
                                scFileEntry.ProjectId = commit.ProjectId;
                                scFileEntry.VersionControlSystemId = commit.VersionControlSystemId;

                                //Set the latest commit, date, author if not set (TaraVault)
                                if (scFile.RevisionId == 0 || scFile.LastUpdateDate < commit.UpdateDate)
                                {
                                    scFile.RevisionId = commit.RevisionId;
                                    scFile.RevisionName = commit.Name;
                                    scFile.RevisionKey = commit.Revisionkey;
                                    scFile.LastUpdateDate = commit.UpdateDate;
                                }
                                if (String.IsNullOrEmpty(scFile.AuthorName))
                                {
                                    scFile.AuthorName = commit.AuthorName;
                                }
                                if (String.IsNullOrEmpty(scFile.Action))
                                {
                                    scFile.Action = vcFile.Action.ToString();
                                }
                            }
                            else
                            {
                                //May need to update the latest commit
                                if (scFile.RevisionId == 0 || scFile.LastUpdateDate < commit.UpdateDate)
                                {
                                    scFile.RevisionId = commit.RevisionId;
                                    scFile.RevisionName = commit.Name;
                                    scFile.RevisionKey = commit.Revisionkey;
                                    scFile.LastUpdateDate = commit.UpdateDate;
                                    //Only set once since author not latest commiter
                                    if (String.IsNullOrEmpty(scFile.AuthorName))
                                    {
                                        scFile.AuthorName = commit.AuthorName;
                                    }
                                    scFile.Action = vcFile.Action.ToString();
                                }
                            }
                        }
                    }
                }

                //Commit the changes
                context.SaveChanges();
            }
        }

        #endregion
    }
}
