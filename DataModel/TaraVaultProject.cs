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
    [KnownType(typeof(Project))]
    [KnownType(typeof(TaraVaultUser))]
    [KnownType(typeof(TaraVaultType))]
    public partial class TaraVaultProject: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
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
                        if (Project != null && Project.ProjectId != value)
                        {
                            Project = null;
                        }
                    }
                    _projectId = value;
                    OnPropertyChanged("ProjectId");
                }
            }
        }
        private int _projectId;
    
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    ChangeTracker.RecordOriginalValue("Name", _name);
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _name;
    
        [DataMember]
        public int VaultTypeId
        {
            get { return _vaultTypeId; }
            set
            {
                if (_vaultTypeId != value)
                {
                    ChangeTracker.RecordOriginalValue("VaultTypeId", _vaultTypeId);
                    if (!IsDeserializing)
                    {
                        if (VaultType != null && VaultType.VaultTypeId != value)
                        {
                            VaultType = null;
                        }
                    }
                    _vaultTypeId = value;
                    OnPropertyChanged("VaultTypeId");
                }
            }
        }
        private int _vaultTypeId;
    
        [DataMember]
        public long VaultId
        {
            get { return _vaultId; }
            set
            {
                if (_vaultId != value)
                {
                    ChangeTracker.RecordOriginalValue("VaultId", _vaultId);
                    _vaultId = value;
                    OnPropertyChanged("VaultId");
                }
            }
        }
        private long _vaultId;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Project Project
        {
            get { return _project; }
            set
            {
                if (!ReferenceEquals(_project, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (ProjectId != value.ProjectId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _project;
                    _project = value;
                    FixupProject(previousValue);
                    OnNavigationPropertyChanged("Project");
                }
            }
        }
        private Project _project;
    
        [DataMember]
        public TrackableCollection<TaraVaultUser> VaultUsers
        {
            get
            {
                if (_vaultUsers == null)
                {
                    _vaultUsers = new TrackableCollection<TaraVaultUser>();
                    _vaultUsers.CollectionChanged += FixupVaultUsers;
                }
                return _vaultUsers;
            }
            set
            {
                if (!ReferenceEquals(_vaultUsers, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_vaultUsers != null)
                    {
                        _vaultUsers.CollectionChanged -= FixupVaultUsers;
                    }
                    _vaultUsers = value;
                    if (_vaultUsers != null)
                    {
                        _vaultUsers.CollectionChanged += FixupVaultUsers;
                    }
                    OnNavigationPropertyChanged("VaultUsers");
                }
            }
        }
        private TrackableCollection<TaraVaultUser> _vaultUsers;
    
        [DataMember]
        public TaraVaultType VaultType
        {
            get { return _vaultType; }
            set
            {
                if (!ReferenceEquals(_vaultType, value))
                {
                    var previousValue = _vaultType;
                    _vaultType = value;
                    FixupVaultType(previousValue);
                    OnNavigationPropertyChanged("VaultType");
                }
            }
        }
        private TaraVaultType _vaultType;

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
            Project = null;
            VaultUsers.Clear();
            VaultType = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupProject(Project previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && ReferenceEquals(previousValue.TaraVault, this))
            {
                previousValue.TaraVault = null;
            }
    
            if (Project != null)
            {
                Project.TaraVault = this;
                ProjectId = Project.ProjectId;
            }
    
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Project")
                    && (ChangeTracker.OriginalValues["Project"] == Project))
                {
                    ChangeTracker.OriginalValues.Remove("Project");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Project", previousValue);
                }
                if (Project != null && !Project.ChangeTracker.ChangeTrackingEnabled)
                {
                    Project.StartTracking();
                }
            }
        }
    
        private void FixupVaultType(TaraVaultType previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Projects.Contains(this))
            {
                previousValue.Projects.Remove(this);
            }
    
            if (VaultType != null)
            {
                if (!VaultType.Projects.Contains(this))
                {
                    VaultType.Projects.Add(this);
                }
    
                VaultTypeId = VaultType.VaultTypeId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("VaultType")
                    && (ChangeTracker.OriginalValues["VaultType"] == VaultType))
                {
                    ChangeTracker.OriginalValues.Remove("VaultType");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("VaultType", previousValue);
                }
                if (VaultType != null && !VaultType.ChangeTracker.ChangeTrackingEnabled)
                {
                    VaultType.StartTracking();
                }
            }
        }
    
        private void FixupVaultUsers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TaraVaultUser item in e.NewItems)
                {
                    if (!item.VaultProject.Contains(this))
                    {
                        item.VaultProject.Add(this);
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("VaultUsers", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TaraVaultUser item in e.OldItems)
                {
                    if (item.VaultProject.Contains(this))
                    {
                        item.VaultProject.Remove(this);
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("VaultUsers", item);
                    }
                }
            }
        }

        #endregion

    }
}
