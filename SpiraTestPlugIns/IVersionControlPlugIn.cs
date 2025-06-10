using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Serialization;

namespace Inflectra.SpiraTest.PlugIns
{
    /// <summary>All Version Control providers that are accessed through the Source Code screens need to implement
    /// this interface. It's used for retreiving files, folders and revisions</summary>
    public interface IVersionControlPlugIn
    {
        object Initialize(string connection, NetworkCredential credentials, Dictionary<string, string> parameters, EventLog eventLog, string custom01, string custom02, string custom03, string custom04, string custom05);
        VersionControlFolder RetrieveFolder(object token, string folderKey);
        VersionControlFile RetrieveFile(object token, string fileKey);
        VersionControlFolder RetrieveFolderByFile(object token, string fileKey);
        List<VersionControlFolder> RetrieveFolders(object token, string parentFolderKey);
        string RetrieveParentFolderKey(object token, string folderKey);
        List<VersionControlFile> RetrieveFilesByFolder(object token, string folderKey, string sortProperty, bool sortAscending, Hashtable filters);
        List<VersionControlFile> RetrieveFilesForRevision(object token, string revisionKey, string sortProperty, bool sortAscending, Hashtable filters);
        VersionControlRevision RetrieveRevision(object token, string revisionKey);
        List<VersionControlRevision> RetrieveRevisions(object token, string sortProperty, bool sortAscending, Hashtable filters);
        List<VersionControlRevision> RetrieveRevisionsForFile(object token, string fileKey, string sortProperty, bool sortAscending, Hashtable filters);
        VersionControlFileStream OpenFile(object token, string fileKey, string revisionKey);
        void CloseFile(VersionControlFileStream versionControlFileStream);

        #region Obsolete Functions that are not used anymore

        /// <remarks>Not used any more, so new providers do not need to implement</remarks>
        List<VersionControlRevisionAssociation> RetrieveAssociationsForRevision(object token, string revisionKey);

        /// <remarks>Not used any more, so new providers do not need to implement</remarks>
        List<VersionControlRevision> RetrieveRevisionsForArtifact(object token, string artifactPrefix, int artifactId);

        #endregion
    }

    /// <summary>Represents the underlying data behind a single file</summary>
    public class VersionControlFileStream
    {
        /// <summary>
        /// Constructor with no initial values
        /// </summary>
        public VersionControlFileStream()
        {
            this.IsRedirect = false;
        }

        /// <summary>
        /// Constructor with initial values specified
        /// </summary>
        public VersionControlFileStream(string fileKey, string revisionKey, string localPath, Stream dataStream, bool isRedirect)
        {
            this.FileKey = fileKey;
            this.RevisionKey = revisionKey;
            this.LocalPath = localPath;
            this.DataStream = dataStream;
            this.IsRedirect = isRedirect;
        }

        #region Properties

        /// <summary>
        /// The id of the file
        /// </summary>
        public string FileKey
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the revision
        /// </summary>
        public string RevisionKey
        {
            get;
            set;
        }

        /// <summary>
        /// Where the file is locally stored (internal to provider)
        /// </summary>
        public string LocalPath
        {
            get;
            set;
        }

        /// <summary>
        /// The data behind the file
        /// </summary>
        public Stream DataStream
        {
            get;
            set;
        }

        /// <summary>
        /// True of the 'LocalPath' is a redirect to another net-based URL.
        /// </summary>
        public Boolean IsRedirect
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>Represents a single version control folder</summary>
    public class VersionControlFolder
    {
        #region Properties
        /// <summary>The unique key of the folder object.</summary>
        public string FolderKey
        { get; set; }

        /// <summary>The path of the folder.</summary>
        public string Name
        { get; set; }
        #endregion Properties

        #region Constructors
        /// <summary>Constructor with no initial values</summary>
        public VersionControlFolder()
        {
        }

        /// <summary>Constructor with initial values specified</summary>
        public VersionControlFolder(string folderKey, string name)
        {
            this.FolderKey = folderKey;
            this.Name = name;
        }
        #endregion Constructors
    }

    /// <summary>Represents a single version control file</summary>
    public class VersionControlFile : IComparable<VersionControlFile>
    {
        #region Comparison Delegates
        #region Name
        /// <summary>Compares ascending between the file names.</summary>
        public static Comparison<VersionControlFile> NameAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.Name.CompareTo(file2.Name);
        };

        /// <summary>Compares descending between the file names.</summary>
        public static Comparison<VersionControlFile> NameDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.Name.CompareTo(file1.Name);
        };
        #endregion Name

        #region Size
        /// <summary>Compares ascending between file sizes.</summary>
        public static Comparison<VersionControlFile> SizeAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.Size.CompareTo(file2.Size);
        };

        /// <summary>Compares descending between file sizes.</summary>
        public static Comparison<VersionControlFile> SizeDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.Size.CompareTo(file1.Size);
        };
        #endregion Size

        #region Revision
        /// <summary>Compares ascending between the Revision/Commit keys.</summary>
        public static Comparison<VersionControlFile> RevisionAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.Revision.CompareTo(file2.Revision);
        };

        /// <summary>Compares descending between the Revision/Commit keys.</summary>
        public static Comparison<VersionControlFile> RevisionDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.Revision.CompareTo(file1.Revision);
        };
        #endregion Revision

        #region Author
        /// <summary>Compares ascending between the Author names.</summary>
        public static Comparison<VersionControlFile> AuthorAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.Author.CompareTo(file2.Author);
        };

        /// <summary>Compares descending between the Author names.</summary>
        public static Comparison<VersionControlFile> AuthorDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.Author.CompareTo(file1.Author);
        };
        #endregion Author

        #region LastUpdated
        /// <summary>Compares ascending between the Last Updated date.</summary>
        public static Comparison<VersionControlFile> LastUpdatedAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.LastUpdated.CompareTo(file2.LastUpdated);
        };

        /// <summary>Compares descending between the Last Updated date.</summary>
        public static Comparison<VersionControlFile> LastUpdatedDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.LastUpdated.CompareTo(file1.LastUpdated);
        };
        #endregion LastUpdated

        #region Action
        /// <summary>Compares ascending between the item's 'Action'.</summary>
        public static Comparison<VersionControlFile> ActionAscComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file1.Action.ToString().CompareTo(file2.Action.ToString());
        };

        /// <summary>Compares descending between the item's 'Action'.</summary>
        public static Comparison<VersionControlFile> ActionDescComparison = delegate(VersionControlFile file1, VersionControlFile file2)
        {
            return file2.Action.ToString().CompareTo(file1.Action.ToString());
        };
        #endregion Action
        #endregion Comparison Delegates

        #region Properties
        /// <summary>The unique key to the represented file.</summary>
        public string FileKey
        { get; set; }

        /// <summary>The filename of the file, with extension and path, if necessary.</summary>
        public string Name
        { get; set; }

        /// <summary>The size of the file, in bytes.</summary>
        public int Size
        { get; set; }

        /// <summary>The author's login or id of the file's specified change.</summary>
        public string Author
        { get; set; }

        /// <summary>The file's revision descrictive text.</summary>
        public string Revision
        { get; set; }

        /// <summary>The unique Revision key to this file.</summary>
        public string RevisionKey
        { get; set; }

        /// <summary>The date this file was lastupdated, or the date of the specified revision.</summary>
        public DateTime LastUpdated
        { get; set; }

        /// <summary>The action of this file in the associated revision/commit.</summary>
        public VersionControlActionEnum Action
        { get; set; }
        #endregion

        #region Constructors
        /// <summary>Constructor with no initial values</summary>
        public VersionControlFile()
        {
            this.Action = VersionControlActionEnum.Undefined;
        }

        /// <summary>Constructor with initial values specified</summary>
        public VersionControlFile(string fileKey, string name, int size, string author, string revisionKey, string revision, DateTime lastUpdated)
        {
            this.FileKey = fileKey;
            this.Name = name;
            this.Size = size;
            this.Author = author;
            this.RevisionKey = revisionKey;
            this.Revision = revision;
            this.LastUpdated = lastUpdated;
            this.Action = VersionControlActionEnum.Undefined;
        }

        /// <summary>Constructor with initial values specified</summary>
        public VersionControlFile(string fileKey, string name, int size, string author, string revisionKey, string revision, DateTime lastUpdated, VersionControlActionEnum action)
        {
            this.FileKey = fileKey;
            this.Name = name;
            this.Size = size;
            this.Author = author;
            this.RevisionKey = revisionKey;
            this.Revision = revision;
            this.LastUpdated = lastUpdated;
            this.Action = action;
        }
        #endregion Constructors

        /// <summary>Enumeration of the different actions that can be performed on a file.</summary>
        public enum VersionControlActionEnum : int
        {
            Undefined = -1,
            Added = 0,
            Modified = 1,
            Deleted = 2,
            Replaced = 3,
            Other = 99
        }

        #region IComparable Members
        /// <summary>The default sorting order is by name ascending</summary>
        /// <param name="other">The object to compare this against.</param>
        /// <returns>Comparison result against the other object.</returns>
        public int CompareTo(VersionControlFile other)
        {
            return Name.CompareTo(other.Name);
        }
        #endregion IComparable Members
    }

    /// <summary>Represents a single version control revision</summary>
    public class VersionControlRevision : IComparable<VersionControlRevision>
    {
        #region Comparison Delegates
        #region Name
        /// <summary>Compares ascending between the revision names.</summary>
        public static Comparison<VersionControlRevision> NameAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.Name.CompareTo(revision2.Name);
        };

        /// <summary>Compares descending between the revision names.</summary>
        public static Comparison<VersionControlRevision> NameDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.Name.CompareTo(revision1.Name);
        };
        #endregion Name

        #region Message
        /// <summary>Compares ascending between the messages for the revision/commit.</summary>
        public static Comparison<VersionControlRevision> MessageAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.Message.CompareTo(revision2.Message);
        };

        /// <summary>Compares descending between the messages for the revision/commit.</summary>
        public static Comparison<VersionControlRevision> MessageDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.Message.CompareTo(revision1.Message);
        };
        #endregion Message

        #region Author
        /// <summary>Compares ascending between the author name of the revision/commit.</summary>
        public static Comparison<VersionControlRevision> AuthorAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.Author.CompareTo(revision2.Author);
        };

        /// <summary>Compares descending between the author name of the revision/commit.</summary>
        public static Comparison<VersionControlRevision> AuthorDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.Author.CompareTo(revision1.Author);
        };
        #endregion Author

        #region Update Date
        /// <summary>Compares ascending between the date of the revision/commit.</summary>
        public static Comparison<VersionControlRevision> UpdateDateAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.UpdateDate.CompareTo(revision2.UpdateDate);
        };

        /// <summary>Compares descending between the date of the revision/commit.</summary>
        public static Comparison<VersionControlRevision> UpdateDateDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.UpdateDate.CompareTo(revision1.UpdateDate);
        };

        #endregion Update Date

        #region Content Changed
        /// <summary>Compares ascending between the Content Changed flag.</summary>
        public static Comparison<VersionControlRevision> ContentChangedAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.ContentChanged.CompareTo(revision2.ContentChanged);
        };

        /// <summary>Compares descending between the Content Changed flag.</summary>
        public static Comparison<VersionControlRevision> ContentChangedDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.ContentChanged.CompareTo(revision1.ContentChanged);
        };
        #endregion Content Changed

        #region Properties Changed
        /// <summary>Compares ascending between the Properties Changed flag.</summary>
        public static Comparison<VersionControlRevision> PropertiesChangedAscComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision1.PropertiesChanged.CompareTo(revision2.PropertiesChanged);
        };

        /// <summary>Compares desacending between the Properties Changed flag.</summary>
        public static Comparison<VersionControlRevision> PropertiesChangedDescComparison = delegate(VersionControlRevision revision1, VersionControlRevision revision2)
        {
            return revision2.PropertiesChanged.CompareTo(revision1.PropertiesChanged);
        };
        #endregion Properties Changed
        #endregion Comparison Delegates

        #region Properties
        /// <summary>The unique key to this revision/commit.</summary>
        public string RevisionKey
        { get; set; }

        /// <summary>The name of this revision/commit.</summary>
        public string Name
        { get; set; }

        /// <summary>The author name or login id of the user who commited this.</summary>
        public string Author
        { get; set; }

        /// <summary>The message that was entered with this commit/revision.</summary>
        public string Message
        { get; set; }

        /// <summary>The date of this revision/commit.</summary>
        public DateTime UpdateDate
        { get; set; }

        /// <summary>Flag indicating whether the revision/commit had content changes.</summary>
        public bool ContentChanged
        { get; set; }

        /// <summary>Flag indicating whether the revision/commit had properties changed.</summary>
        public bool PropertiesChanged
        { get; set; }
        #endregion

        #region Constructors
        /// <summary>Constructor with no initial values</summary>
        public VersionControlRevision()
        { }

        /// <summary>Constructor with initial values specified</summary>
        public VersionControlRevision(string revisionKey, string name, string author, string message, DateTime updateDate, bool contentChanged, bool propertiesChanged)
        {
            this.RevisionKey = revisionKey;
            this.Name = name;
            this.Author = author;
            this.Message = message;
            this.UpdateDate = updateDate;
            this.ContentChanged = contentChanged;
            this.PropertiesChanged = propertiesChanged;
        }
        #endregion Constructors

        #region IComparable<VersionControlRevision> Members

        /// <summary>
        /// The default sorting order is by name descending
        /// </summary>
        /// <param name="other">The revision compared against</param>
        /// <returns></returns>
        public int CompareTo(VersionControlRevision other)
        {
            return other.Name.CompareTo(Name);
        }

        #endregion
    }

    /// <summary>Represents a single version control artifact association</summary>
    /// <remarks>Used to denote which SpiraTeam artifacts are linked to a specific revision</remarks>
    public class VersionControlRevisionAssociation
    {
        #region Properties
        /// <summary>The unique key of the revision/commit.</summary>
        public string RevisionKey
        { get; set; }

        /// <summary>The two-letter artifact prefix.</summary>
        public string ArtifactTypePrefix
        { get; set; }

        /// <summary>The artifact's ID.</summary>
        public int ArtifactId
        { get; set; }

        /// <summary>The date they were associated.</summary>
        public DateTime AssociationDate
        { get; set; }

        /// <summary>Any comment added by the user.</summary>
        public string Comment
        { get; set; }
        #endregion Properties

        #region Constructors
        /// <summary>Constructor with no arguments</summary>
        public VersionControlRevisionAssociation()
        { }

        /// <summary>Constructor that includes all the properties</summary>
        /// <param name="revisionKey">The revision identifier</param>
        /// <param name="artifactTypePrefix">The two-character prefix for the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="associationDate">The date the association was made</param>
        /// <param name="comment">Any comment regarding the association</param>
        public VersionControlRevisionAssociation(string revisionKey, string artifactTypePrefix, int artifactId, DateTime associationDate, string comment)
        {
            this.RevisionKey = revisionKey;
            this.ArtifactTypePrefix = artifactTypePrefix;
            this.ArtifactId = artifactId;
            this.AssociationDate = associationDate;
            this.Comment = comment;
        }
        #endregion Constructors
    }

    /// <summary>Class representing a branch.</summary>
    public class VersionControlBranch
    {
       public string BranchKey
        { get; set; }

       public bool IsDefault
       { get; set; }
    }

    #region Internal Exceptions
    ///<summary>This general exception is thrown when any non-specific error occurs in the version control provider</summary>
    public class VersionControlGeneralException : ApplicationException
    {
        public VersionControlGeneralException()
        {
        }
        public VersionControlGeneralException(string message)
            : base(message)
        {
        }
        public VersionControlGeneralException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    ///<summary>This exception is thrown when a revision, folder or file requested cannot be found</summary>
    public class VersionControlArtifactNotFoundException : VersionControlGeneralException
    {
        public VersionControlArtifactNotFoundException()
        {
        }
        public VersionControlArtifactNotFoundException(string message)
            : base(message)
        {
        }
        public VersionControlArtifactNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    ///<summary>This exception is thrown when authentication with the version control provider fails</summary>
    public class VersionControlAuthenticationException : VersionControlGeneralException
    {
        public VersionControlAuthenticationException()
        {
        }
        public VersionControlAuthenticationException(string message)
            : base(message)
        {
        }
        public VersionControlAuthenticationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    ///<summary>This exception is thrown when permission to an artifact is denied</summary>
    public class VersionControlArtifactPermissionDeniedException : VersionControlGeneralException
    {
        public VersionControlArtifactPermissionDeniedException()
        {
        }
        public VersionControlArtifactPermissionDeniedException(string message)
            : base(message)
        {
        }
        public VersionControlArtifactPermissionDeniedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion Internal Exceptions
}
