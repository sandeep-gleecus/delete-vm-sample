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
    [KnownType(typeof(TestRun))]
    [KnownType(typeof(TestSet))]
    public partial class AutomationHost: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int AutomationHostId
        {
            get { return _automationHostId; }
            set
            {
                if (_automationHostId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'AutomationHostId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _automationHostId = value;
                    OnPropertyChanged("AutomationHostId");
                }
            }
        }
        private int _automationHostId;
    
        [DataMember]
        public int ProjectId
        {
            get { return _projectId; }
            set
            {
                if (_projectId != value)
                {
                    ChangeTracker.RecordOriginalValue("ProjectId", _projectId);
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
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    ChangeTracker.RecordOriginalValue("Description", _description);
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }
        private string _description;
    
        [DataMember]
        public string Token
        {
            get { return _token; }
            set
            {
                if (_token != value)
                {
                    ChangeTracker.RecordOriginalValue("Token", _token);
                    _token = value;
                    OnPropertyChanged("Token");
                }
            }
        }
        private string _token;
    
        [DataMember]
        public System.DateTime LastUpdateDate
        {
            get { return _lastUpdateDate; }
            set
            {
                if (_lastUpdateDate != value)
                {
                    ChangeTracker.RecordOriginalValue("LastUpdateDate", _lastUpdateDate);
                    _lastUpdateDate = value;
                    OnPropertyChanged("LastUpdateDate");
                }
            }
        }
        private System.DateTime _lastUpdateDate;
    
        [DataMember]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    ChangeTracker.RecordOriginalValue("IsDeleted", _isDeleted);
                    _isDeleted = value;
                    OnPropertyChanged("IsDeleted");
                }
            }
        }
        private bool _isDeleted;
    
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    ChangeTracker.RecordOriginalValue("IsActive", _isActive);
                    _isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }
        private bool _isActive;
    
        [DataMember]
        public bool IsAttachments
        {
            get { return _isAttachments; }
            set
            {
                if (_isAttachments != value)
                {
                    ChangeTracker.RecordOriginalValue("IsAttachments", _isAttachments);
                    _isAttachments = value;
                    OnPropertyChanged("IsAttachments");
                }
            }
        }
        private bool _isAttachments;
    
        [DataMember]
        public System.DateTime ConcurrencyDate
        {
            get { return _concurrencyDate; }
            set
            {
                if (_concurrencyDate != value)
                {
                    ChangeTracker.RecordOriginalValue("ConcurrencyDate", _concurrencyDate);
                    _concurrencyDate = value;
                    OnPropertyChanged("ConcurrencyDate");
                }
            }
        }
        private System.DateTime _concurrencyDate;
    
        [DataMember]
        public Nullable<System.DateTime> LastContactDate
        {
            get { return _lastContactDate; }
            set
            {
                if (_lastContactDate != value)
                {
                    ChangeTracker.RecordOriginalValue("LastContactDate", _lastContactDate);
                    _lastContactDate = value;
                    OnPropertyChanged("LastContactDate");
                }
            }
        }
        private Nullable<System.DateTime> _lastContactDate;

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
                    var previousValue = _project;
                    _project = value;
                    FixupProject(previousValue);
                    OnNavigationPropertyChanged("Project");
                }
            }
        }
        private Project _project;
    
        [DataMember]
        public TrackableCollection<TestRun> TestRuns
        {
            get
            {
                if (_testRuns == null)
                {
                    _testRuns = new TrackableCollection<TestRun>();
                    _testRuns.CollectionChanged += FixupTestRuns;
                }
                return _testRuns;
            }
            set
            {
                if (!ReferenceEquals(_testRuns, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_testRuns != null)
                    {
                        _testRuns.CollectionChanged -= FixupTestRuns;
                    }
                    _testRuns = value;
                    if (_testRuns != null)
                    {
                        _testRuns.CollectionChanged += FixupTestRuns;
                    }
                    OnNavigationPropertyChanged("TestRuns");
                }
            }
        }
        private TrackableCollection<TestRun> _testRuns;
    
        [DataMember]
        public TrackableCollection<TestSet> TestSets
        {
            get
            {
                if (_testSets == null)
                {
                    _testSets = new TrackableCollection<TestSet>();
                    _testSets.CollectionChanged += FixupTestSets;
                }
                return _testSets;
            }
            set
            {
                if (!ReferenceEquals(_testSets, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_testSets != null)
                    {
                        _testSets.CollectionChanged -= FixupTestSets;
                    }
                    _testSets = value;
                    if (_testSets != null)
                    {
                        _testSets.CollectionChanged += FixupTestSets;
                    }
                    OnNavigationPropertyChanged("TestSets");
                }
            }
        }
        private TrackableCollection<TestSet> _testSets;

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
    
        protected virtual void ClearNavigationProperties()
        {
            Project = null;
            TestRuns.Clear();
            TestSets.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupProject(Project previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.AutomationHosts.Contains(this))
            {
                previousValue.AutomationHosts.Remove(this);
            }
    
            if (Project != null)
            {
                if (!Project.AutomationHosts.Contains(this))
                {
                    Project.AutomationHosts.Add(this);
                }
    
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
    
        private void FixupTestRuns(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TestRun item in e.NewItems)
                {
                    item.AutomationHost = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TestRuns", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TestRun item in e.OldItems)
                {
                    if (ReferenceEquals(item.AutomationHost, this))
                    {
                        item.AutomationHost = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TestRuns", item);
                    }
                }
            }
        }
    
        private void FixupTestSets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TestSet item in e.NewItems)
                {
                    item.AutomationHost = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TestSets", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TestSet item in e.OldItems)
                {
                    if (ReferenceEquals(item.AutomationHost, this))
                    {
                        item.AutomationHost = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TestSets", item);
                    }
                }
            }
        }

        #endregion

    }
}
