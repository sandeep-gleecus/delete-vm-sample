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
    [KnownType(typeof(GlobalHistoryDetails))]
    [KnownType(typeof(User))]
    [KnownType(typeof(WorkspaceType))]
    [KnownType(typeof(HistoryChangeSetType))]
    public partial class GlobalHistoryChangeset: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public long ChangesetId
        {
            get { return _changesetId; }
            set
            {
                if (_changesetId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ChangesetId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _changesetId = value;
                    OnPropertyChanged("ChangesetId");
                }
            }
        }
        private long _changesetId;
    
        [DataMember]
        public int UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    ChangeTracker.RecordOriginalValue("UserId", _userId);
                    if (!IsDeserializing)
                    {
                        if (User != null && User.UserId != value)
                        {
                            User = null;
                        }
                    }
                    _userId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }
        private int _userId;
    
        [DataMember]
        public int WorkspaceTypeId
        {
            get { return _workspaceTypeId; }
            set
            {
                if (_workspaceTypeId != value)
                {
                    ChangeTracker.RecordOriginalValue("WorkspaceTypeId", _workspaceTypeId);
                    if (!IsDeserializing)
                    {
                        if (WorkspaceType != null && WorkspaceType.WorkspaceTypeId != value)
                        {
                            WorkspaceType = null;
                        }
                    }
                    _workspaceTypeId = value;
                    OnPropertyChanged("WorkspaceTypeId");
                }
            }
        }
        private int _workspaceTypeId;
    
        [DataMember]
        public int WorkspaceId
        {
            get { return _workspaceId; }
            set
            {
                if (_workspaceId != value)
                {
                    ChangeTracker.RecordOriginalValue("WorkspaceId", _workspaceId);
                    _workspaceId = value;
                    OnPropertyChanged("WorkspaceId");
                }
            }
        }
        private int _workspaceId;
    
        [DataMember]
        public System.DateTime ChangeDate
        {
            get { return _changeDate; }
            set
            {
                if (_changeDate != value)
                {
                    ChangeTracker.RecordOriginalValue("ChangeDate", _changeDate);
                    _changeDate = value;
                    OnPropertyChanged("ChangeDate");
                }
            }
        }
        private System.DateTime _changeDate;
    
        [DataMember]
        public int ChangeTypeId
        {
            get { return _changeTypeId; }
            set
            {
                if (_changeTypeId != value)
                {
                    ChangeTracker.RecordOriginalValue("ChangeTypeId", _changeTypeId);
                    if (!IsDeserializing)
                    {
                        if (ChangeType != null && ChangeType.ChangeTypeId != value)
                        {
                            ChangeType = null;
                        }
                    }
                    _changeTypeId = value;
                    OnPropertyChanged("ChangeTypeId");
                }
            }
        }
        private int _changeTypeId;
    
        [DataMember]
        public string ArtifactName
        {
            get { return _artifactName; }
            set
            {
                if (_artifactName != value)
                {
                    ChangeTracker.RecordOriginalValue("ArtifactName", _artifactName);
                    _artifactName = value;
                    OnPropertyChanged("ArtifactName");
                }
            }
        }
        private string _artifactName;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public TrackableCollection<GlobalHistoryDetails> HistoryDetails
        {
            get
            {
                if (_historyDetails == null)
                {
                    _historyDetails = new TrackableCollection<GlobalHistoryDetails>();
                    _historyDetails.CollectionChanged += FixupHistoryDetails;
                }
                return _historyDetails;
            }
            set
            {
                if (!ReferenceEquals(_historyDetails, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_historyDetails != null)
                    {
                        _historyDetails.CollectionChanged -= FixupHistoryDetails;
                        // This is the principal end in an association that performs cascade deletes.
                        // Remove the cascade delete event handler for any entities in the current collection.
                        foreach (GlobalHistoryDetails item in _historyDetails)
                        {
                            ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                        }
                    }
                    _historyDetails = value;
                    if (_historyDetails != null)
                    {
                        _historyDetails.CollectionChanged += FixupHistoryDetails;
                        // This is the principal end in an association that performs cascade deletes.
                        // Add the cascade delete event handler for any entities that are already in the new collection.
                        foreach (GlobalHistoryDetails item in _historyDetails)
                        {
                            ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                        }
                    }
                    OnNavigationPropertyChanged("HistoryDetails");
                }
            }
        }
        private TrackableCollection<GlobalHistoryDetails> _historyDetails;
    
        [DataMember]
        public User User
        {
            get { return _user; }
            set
            {
                if (!ReferenceEquals(_user, value))
                {
                    var previousValue = _user;
                    _user = value;
                    FixupUser(previousValue);
                    OnNavigationPropertyChanged("User");
                }
            }
        }
        private User _user;
    
        [DataMember]
        public WorkspaceType WorkspaceType
        {
            get { return _workspaceType; }
            set
            {
                if (!ReferenceEquals(_workspaceType, value))
                {
                    var previousValue = _workspaceType;
                    _workspaceType = value;
                    FixupWorkspaceType(previousValue);
                    OnNavigationPropertyChanged("WorkspaceType");
                }
            }
        }
        private WorkspaceType _workspaceType;
    
        [DataMember]
        public HistoryChangeSetType ChangeType
        {
            get { return _changeType; }
            set
            {
                if (!ReferenceEquals(_changeType, value))
                {
                    var previousValue = _changeType;
                    _changeType = value;
                    FixupChangeType(previousValue);
                    OnNavigationPropertyChanged("ChangeType");
                }
            }
        }
        private HistoryChangeSetType _changeType;

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
            HistoryDetails.Clear();
            User = null;
            WorkspaceType = null;
            ChangeType = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupUser(User previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
            {
                previousValue.TST_GLOBAL_HISTORY_CHANGESET.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
                {
                    User.TST_GLOBAL_HISTORY_CHANGESET.Add(this);
                }
    
                UserId = User.UserId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("User")
                    && (ChangeTracker.OriginalValues["User"] == User))
                {
                    ChangeTracker.OriginalValues.Remove("User");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("User", previousValue);
                }
                if (User != null && !User.ChangeTracker.ChangeTrackingEnabled)
                {
                    User.StartTracking();
                }
            }
        }
    
        private void FixupWorkspaceType(WorkspaceType previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
            {
                previousValue.TST_GLOBAL_HISTORY_CHANGESET.Remove(this);
            }
    
            if (WorkspaceType != null)
            {
                if (!WorkspaceType.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
                {
                    WorkspaceType.TST_GLOBAL_HISTORY_CHANGESET.Add(this);
                }
    
                WorkspaceTypeId = WorkspaceType.WorkspaceTypeId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("WorkspaceType")
                    && (ChangeTracker.OriginalValues["WorkspaceType"] == WorkspaceType))
                {
                    ChangeTracker.OriginalValues.Remove("WorkspaceType");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("WorkspaceType", previousValue);
                }
                if (WorkspaceType != null && !WorkspaceType.ChangeTracker.ChangeTrackingEnabled)
                {
                    WorkspaceType.StartTracking();
                }
            }
        }
    
        private void FixupChangeType(HistoryChangeSetType previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
            {
                previousValue.TST_GLOBAL_HISTORY_CHANGESET.Remove(this);
            }
    
            if (ChangeType != null)
            {
                if (!ChangeType.TST_GLOBAL_HISTORY_CHANGESET.Contains(this))
                {
                    ChangeType.TST_GLOBAL_HISTORY_CHANGESET.Add(this);
                }
    
                ChangeTypeId = ChangeType.ChangeTypeId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("ChangeType")
                    && (ChangeTracker.OriginalValues["ChangeType"] == ChangeType))
                {
                    ChangeTracker.OriginalValues.Remove("ChangeType");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("ChangeType", previousValue);
                }
                if (ChangeType != null && !ChangeType.ChangeTracker.ChangeTrackingEnabled)
                {
                    ChangeType.StartTracking();
                }
            }
        }
    
        private void FixupHistoryDetails(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (GlobalHistoryDetails item in e.NewItems)
                {
                    item.Changeset = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("HistoryDetails", item);
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Update the event listener to refer to the new dependent.
                    ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (GlobalHistoryDetails item in e.OldItems)
                {
                    if (ReferenceEquals(item.Changeset, this))
                    {
                        item.Changeset = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("HistoryDetails", item);
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Remove the previous dependent from the event listener.
                    ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                }
            }
        }

        #endregion

    }
}
