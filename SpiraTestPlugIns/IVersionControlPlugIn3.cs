using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.PlugIns
{
    /// <summary>
    /// Extension to the IVersionControlPlugIn interface to retrieve a whole set of folders and files at once
    /// </summary>
    public interface IVersionControlPlugIn3
    {
        /// <summary>Pulls all files that were modified/changed in a given revision.</summary>
        /// <param name="token">The source control library's unique token.</param>
        /// <param name="branchKey">The key for the branch to pull the folder from.</param>
        /// <param name="path">The folder path, leave null for root</param>
        /// <returns>List of files that match the criteria.</returns>
        List<VersionControlDirectoryEntry> RetrieveFolderFileHierarchy(object token, string branchKey, string path = null);
    }
}

/// <summary>
/// Represents a folder/file entry in the filesystem hierarchy
/// </summary>
public class VersionControlDirectoryEntry
{
    /// <summary>
    /// The display name of the file or folder
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The path/key of the file or folder
    /// </summary>
    public string PathKey { get; set; }

    /// <summary>
    /// Is it a folder or file
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// The display name for the latest revision (if supported)
    /// </summary>
    public string Revision { get; set; }

    /// <summary>
    /// The key for the latest revision (if supported)
    /// </summary>
    public string RevisionKey { get; set; }

    /// <summary>
    /// The size of the file
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// The author of the file
    /// </summary>
    public string Author { get; set; }
}
