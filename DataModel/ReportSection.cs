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
    [KnownType(typeof(ArtifactType))]
    [KnownType(typeof(ReportElement))]
    [KnownType(typeof(ReportSectionInstance))]
    public partial class ReportSection: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int ReportSectionId
        {
            get { return _reportSectionId; }
            set
            {
                if (_reportSectionId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ReportSectionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _reportSectionId = value;
                    OnPropertyChanged("ReportSectionId");
                }
            }
        }
        private int _reportSectionId;
    
        [DataMember]
        public Nullable<int> ArtifactTypeId
        {
            get { return _artifactTypeId; }
            set
            {
                if (_artifactTypeId != value)
                {
                    ChangeTracker.RecordOriginalValue("ArtifactTypeId", _artifactTypeId);
                    if (!IsDeserializing)
                    {
                        if (ArtifactType != null && ArtifactType.ArtifactTypeId != value)
                        {
                            ArtifactType = null;
                        }
                    }
                    _artifactTypeId = value;
                    OnPropertyChanged("ArtifactTypeId");
                }
            }
        }
        private Nullable<int> _artifactTypeId;
    
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
        public string DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                if (_defaultTemplate != value)
                {
                    ChangeTracker.RecordOriginalValue("DefaultTemplate", _defaultTemplate);
                    _defaultTemplate = value;
                    OnPropertyChanged("DefaultTemplate");
                }
            }
        }
        private string _defaultTemplate;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public ArtifactType ArtifactType
        {
            get { return _artifactType; }
            set
            {
                if (!ReferenceEquals(_artifactType, value))
                {
                    var previousValue = _artifactType;
                    _artifactType = value;
                    FixupArtifactType(previousValue);
                    OnNavigationPropertyChanged("ArtifactType");
                }
            }
        }
        private ArtifactType _artifactType;
    
        [DataMember]
        public TrackableCollection<ReportElement> Elements
        {
            get
            {
                if (_elements == null)
                {
                    _elements = new TrackableCollection<ReportElement>();
                    _elements.CollectionChanged += FixupElements;
                }
                return _elements;
            }
            set
            {
                if (!ReferenceEquals(_elements, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_elements != null)
                    {
                        _elements.CollectionChanged -= FixupElements;
                    }
                    _elements = value;
                    if (_elements != null)
                    {
                        _elements.CollectionChanged += FixupElements;
                    }
                    OnNavigationPropertyChanged("Elements");
                }
            }
        }
        private TrackableCollection<ReportElement> _elements;
    
        [DataMember]
        public TrackableCollection<ReportSectionInstance> Instances
        {
            get
            {
                if (_instances == null)
                {
                    _instances = new TrackableCollection<ReportSectionInstance>();
                    _instances.CollectionChanged += FixupInstances;
                }
                return _instances;
            }
            set
            {
                if (!ReferenceEquals(_instances, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_instances != null)
                    {
                        _instances.CollectionChanged -= FixupInstances;
                    }
                    _instances = value;
                    if (_instances != null)
                    {
                        _instances.CollectionChanged += FixupInstances;
                    }
                    OnNavigationPropertyChanged("Instances");
                }
            }
        }
        private TrackableCollection<ReportSectionInstance> _instances;

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
            ArtifactType = null;
            Elements.Clear();
            Instances.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupArtifactType(ArtifactType previousValue, bool skipKeys = false)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.ReportSections.Contains(this))
            {
                previousValue.ReportSections.Remove(this);
            }
    
            if (ArtifactType != null)
            {
                if (!ArtifactType.ReportSections.Contains(this))
                {
                    ArtifactType.ReportSections.Add(this);
                }
    
                ArtifactTypeId = ArtifactType.ArtifactTypeId;
            }
            else if (!skipKeys)
            {
                ArtifactTypeId = null;
            }
    
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("ArtifactType")
                    && (ChangeTracker.OriginalValues["ArtifactType"] == ArtifactType))
                {
                    ChangeTracker.OriginalValues.Remove("ArtifactType");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("ArtifactType", previousValue);
                }
                if (ArtifactType != null && !ArtifactType.ChangeTracker.ChangeTrackingEnabled)
                {
                    ArtifactType.StartTracking();
                }
            }
        }
    
        private void FixupElements(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (ReportElement item in e.NewItems)
                {
                    if (!item.Sections.Contains(this))
                    {
                        item.Sections.Add(this);
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Elements", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (ReportElement item in e.OldItems)
                {
                    if (item.Sections.Contains(this))
                    {
                        item.Sections.Remove(this);
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Elements", item);
                    }
                }
            }
        }
    
        private void FixupInstances(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (ReportSectionInstance item in e.NewItems)
                {
                    item.Section = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Instances", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (ReportSectionInstance item in e.OldItems)
                {
                    if (ReferenceEquals(item.Section, this))
                    {
                        item.Section = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Instances", item);
                    }
                }
            }
        }

        #endregion

    }
}
