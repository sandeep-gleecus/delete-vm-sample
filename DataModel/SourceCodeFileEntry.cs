//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.DataModel
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(SourceCodeCommit))]
    public partial class SourceCodeFileEntry: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int VersionControlSystemId
        {
            get { return _versionControlSystemId; }
            set
            {
                if (_versionControlSystemId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'VersionControlSystemId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Commit != null && Commit.VersionControlSystemId != value)
                        {
                            Commit = null;
                        }
                    }
                    _versionControlSystemId = value;
                    OnPropertyChanged("VersionControlSystemId");
                }
            }
        }
        private int _versionControlSystemId;
    
        [DataMember]
        public int ProjectId
        {
            get { return _projectId; }
            set
            {
                if (_projectId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ProjectId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Commit != null && Commit.ProjectId != value)
                        {
                            Commit = null;
                        }
                    }
                    _projectId = value;
                    OnPropertyChanged("ProjectId");
                }
            }
        }
        private int _projectId;
    
        [DataMember]
        public int RevisionId
        {
            get { return _revisionId; }
            set
            {
                if (_revisionId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RevisionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Commit != null && Commit.RevisionId != value)
                        {
                            Commit = null;
                        }
                    }
                    _revisionId = value;
                    OnPropertyChanged("RevisionId");
                }
            }
        }
        private int _revisionId;
    
        [DataMember]
        public string Action
        {
            get { return _action; }
            set
            {
                if (_action != value)
                {
                    ChangeTracker.RecordOriginalValue("Action", _action);
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }
        private string _action;
    
        [DataMember]
        public string FileKey
        {
            get { return _fileKey; }
            set
            {
                if (_fileKey != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'FileKey' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _fileKey = value;
                    OnPropertyChanged("FileKey");
                }
            }
        }
        private string _fileKey;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public SourceCodeCommit Commit
        {
            get { return _commit; }
            set
            {
                if (!ReferenceEquals(_commit, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (VersionControlSystemId != value.VersionControlSystemId || ProjectId != value.ProjectId || RevisionId != value.RevisionId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _commit;
                    _commit = value;
                    FixupCommit(previousValue);
                    OnNavigationPropertyChanged("Commit");
                }
            }
        }
        private SourceCodeCommit _commit;

        #endregion

        #region ChangeTracking
    
        protected virtual void OnPropertyChanged(String propertyName)
        {
            if (ChangeTracker.State != ObjectState.Added && ChangeTracker.State != ObjectState.Deleted)
            {
                ChangeTracker.State = ObjectState.Modified;
            }
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        protected virtual void OnNavigationPropertyChanged(String propertyName)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged{ add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        private event PropertyChangedEventHandler _propertyChanged;
        private ObjectChangeTracker _changeTracker;
    
        [DataMember]
        public ObjectChangeTracker ChangeTracker
        {
            get
            {
                if (_changeTracker == null)
                {
                    _changeTracker = new ObjectChangeTracker();
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
                return _changeTracker;
            }
            set
            {
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging -= HandleObjectStateChanging;
                }
                _changeTracker = value;
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
            }
        }
    
        private void HandleObjectStateChanging(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                ClearNavigationProperties();
            }
        }
    
        protected bool IsDeserializing { get; private set; }
    
        [OnDeserializing]
        public void OnDeserializingMethod(StreamingContext context)
        {
            IsDeserializing = true;
        }
    
        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            IsDeserializing = false;
            ChangeTracker.ChangeTrackingEnabled = true;
        }
    
        // This entity type is the dependent end in at least one association that performs cascade deletes.
        // This event handler will process notifications that occur when the principal end is deleted.
        internal void HandleCascadeDelete(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                this.MarkAsDeleted();
            }
        }
    
        protected virtual void ClearNavigationProperties()
        {
            Commit = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupCommit(SourceCodeCommit previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Files.Contains(this))
            {
                previousValue.Files.Remove(this);
            }
    
            if (Commit != null)
            {
                if (!Commit.Files.Contains(this))
                {
                    Commit.Files.Add(this);
                }
    
                VersionControlSystemId = Commit.VersionControlSystemId;
                ProjectId = Commit.ProjectId;
                RevisionId = Commit.RevisionId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Commit")
                    && (ChangeTracker.OriginalValues["Commit"] == Commit))
                {
                    ChangeTracker.OriginalValues.Remove("Commit");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Commit", previousValue);
                }
                if (Commit != null && !Commit.ChangeTracker.ChangeTrackingEnabled)
                {
                    Commit.StartTracking();
                }
            }
        }

        #endregion

    }
}
