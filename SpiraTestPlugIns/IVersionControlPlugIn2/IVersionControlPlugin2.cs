using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace Inflectra.SpiraTest.PlugIns
{
	/// <summary>
	/// All multi-branch Version Control providers that are accessed through the Source Code screens need to implement
	/// this interface. It's used for retreiving files, folders and revisions
	/// </summary>
	/// <remarks>This replaces the older IVersionControlPlugIn interface </remarks>
	public interface IVersionControlPlugIn2
	{
		/// <summary>Initializes the provider</summary>
		/// <param name="connection">The connection info</param>
		/// <param name="credentials">The login/password/domain for the provider</param>
		/// <param name="parameters">Any custom parameters</param>
		/// <param name="eventLog">A handle to the Windows Event Log used by Spira</param>
		/// <param name="custom01">Provider-specific parameter</param>
		/// <param name="custom02">Provider-specific parameter</param>
		/// <param name="custom03">Provider-specific parameter</param>
		/// <param name="custom04">Provider-specific parameter</param>
		/// <param name="custom05">Provider-specific parameter</param>
		/// <param name="cacheFolder">The location to the folder where any cached data can be stored</param>
		/// <returns>The provider token</returns>
		object Initialize(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string cacheFolder, string custom01, string custom02, string custom03, string custom04, string custom05);

		/// <summary>Retrieves the VersionControlFolder for the given keys.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="folderKey">The key of the folder to retrieve.</param>
		/// <param name="branchKey">The key for the branch to pull the folder from.</param>
		/// <returns>A populated VersionControlFolder, or null.</returns>
		VersionControlFolder RetrieveFolder(object token, string folderKey, string branchKey);

		/// <summary>Retrieves all children folders contained in the specified parent folderKey.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="branchKey">The key for the branch to pull the folder from.</param>
		/// <param name="parentFolderKey">The key of the folder to pull child folders from.</param>
		/// <returns></returns>
		List<VersionControlFolder> RetrieveFolders(object token, string parentFolderKey, string branchKey);

		/// <summary>Returns all files contained within the given folderKey.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="branchKey">The key for the branch to pull the folder from.</param>
		/// <param name="folderKey">The folder's unique key to pull files for.</param>
		/// <param name="sortProperty">The field to sort on.</param>
		/// <param name="sortAscending">Whether to sort ascending or not.</param>
		/// <param name="filters">Any filters to apply to the retrieve.</param>
		/// <param name="numberOfRows">Number of rows to retrieve.</param>
		/// <param name="startRow">The row number to start the return from.</param>
		/// <returns>A list of files contained in the folder that ma</returns>
		List<VersionControlFile> RetrieveFilesByFolder(object token, string folderKey, string branchKey);

		/// <summary>Pulls all files that were modified/changed in a given revision.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="branchKey">The key for the branch to pull the folder from.</param>
		/// <param name="revisionKey">The key of the revision to get the list of files for.</param>
		/// <param name="sortProperty">The field to sort on.</param>
		/// <param name="sortAscending">Whether to sort ascending or not.</param>
		/// <param name="filters">Any filters to apply to the retrieve.</param>
		/// <param name="numberOfRows">Number of rows to retrieve.</param>
		/// <param name="startRow">The row number to start the return from.</param>
		/// <returns>List of files that match the criteria.</returns>
		List<VersionControlFile> RetrieveFilesForRevision(object token, string revisionKey, string branchKey);

		/// <summary>Pulls all avauilable revisions/commits from the provider.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="branchKey">The key for the branch to pull the revisions/commits from.</param>
		/// <returns>List of Revisions that match the given criteria.</returns>
		List<VersionControlRevision> RetrieveRevisions(object token, string branckKey);

		/// <summary>Returns all revisions that have occured since the specified date.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="branchKey">The key for the branch to pull the revisions/commits from.</param>
		/// <param name="newerThan">The cutoff date for revisions to return.</param>
		/// <returns>A list of revisions that are newer than the specified date.</returns>
		List<VersionControlRevision> RetrieveRevisionsSince(object token, string branchKey, DateTime newerThan);

		/// <summary>Sends back a stream for data contents of the file to be read.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <param name="fileKey">The file's unique key.</param>
		/// <param name="revisionKey">The revision/commit of the file to pull.</param>
		/// <param name="branchKey">The key for the branch to pull the revisions/commits from.</param>
		/// <returns>A populated object for the given file.</returns>
		VersionControlFileStream OpenFile(object token, string fileKey, string revisionKey, string branchKey);

		/// <summary>Closes the file handle</summary>
		/// <param name="versionControlFileStream">The file stream being used</param>
		void CloseFile(VersionControlFileStream versionControlFileStream);

		/// <summary>Returns a list of all avaiable branches.</summary>
		/// <param name="token">The source control library's unique token.</param>
		/// <returns>A list of avaiable branches.</returns>
		List<VersionControlBranch> RetrieveBranches(object token);

		/// <summary>Tests the given settings to verify connectivity to the repository.</summary>
		/// <param name="connection">The connection info</param>
		/// <param name="credentials">The login/password/domain for the provider</param>
		/// <param name="parameters">Any custom parameters</param>
		/// <param name="eventLog">A handle to the Windows Event Log used by Spira</param>
		/// <param name="custom01">Provider-specific parameter</param>
		/// <param name="custom02">Provider-specific parameter</param>
		/// <param name="custom03">Provider-specific parameter</param>
		/// <param name="custom04">Provider-specific parameter</param>
		/// <param name="custom05">Provider-specific parameter</param>
		/// <param name="cacheFolder">The location to the folder where any cached data can be stored</param>
		/// <remarks>True if connection was successful and good. False, or throws exception if failure.</remarks>
		bool TestConnection(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string cacheFolder, string custom01, string custom02, string custom03, string custom04, string custom05);

	}
}
