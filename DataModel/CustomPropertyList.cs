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
    [KnownType(typeof(CustomProperty))]
    [KnownType(typeof(CustomPropertyValue))]
    [KnownType(typeof(ProjectTemplate))]
    public partial class CustomPropertyList: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int CustomPropertyListId
        {
            get { return _customPropertyListId; }
            set
            {
                if (_customPropertyListId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CustomPropertyListId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _customPropertyListId = value;
                    OnPropertyChanged("CustomPropertyListId");
                }
            }
        }
        private int _customPropertyListId;
    
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
        public bool IsSortedOnValue
        {
            get { return _isSortedOnValue; }
            set
            {
                if (_isSortedOnValue != value)
                {
                    ChangeTracker.RecordOriginalValue("IsSortedOnValue", _isSortedOnValue);
                    _isSortedOnValue = value;
                    OnPropertyChanged("IsSortedOnValue");
                }
            }
        }
        private bool _isSortedOnValue;
    
        [DataMember]
        public int ProjectTemplateId
        {
            get { return _projectTemplateId; }
            set
            {
                if (_projectTemplateId != value)
                {
                    ChangeTracker.RecordOriginalValue("ProjectTemplateId", _projectTemplateId);
                    if (!IsDeserializing)
                    {
                        if (ProjectTemplate != null && ProjectTemplate.ProjectTemplateId != value)
                        {
                            ProjectTemplate = null;
                        }
                    }
                    _projectTemplateId = value;
                    OnPropertyChanged("ProjectTemplateId");
                }
            }
        }
        private int _projectTemplateId;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public TrackableCollection<CustomProperty> CustomProperties
        {
            get
            {
                if (_customProperties == null)
                {
                    _customProperties = new TrackableCollection<CustomProperty>();
                    _customProperties.CollectionChanged += FixupCustomProperties;
                }
                return _customProperties;
            }
            set
            {
                if (!ReferenceEquals(_customProperties, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_customProperties != null)
                    {
                        _customProperties.CollectionChanged -= FixupCustomProperties;
                    }
                    _customProperties = value;
                    if (_customProperties != null)
                    {
                        _customProperties.CollectionChanged += FixupCustomProperties;
                    }
                    OnNavigationPropertyChanged("CustomProperties");
                }
            }
        }
        private TrackableCollection<CustomProperty> _customProperties;
    
        [DataMember]
        public TrackableCollection<CustomPropertyValue> Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new TrackableCollection<CustomPropertyValue>();
                    _values.CollectionChanged += FixupValues;
                }
                return _values;
            }
            set
            {
                if (!ReferenceEquals(_values, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_values != null)
                    {
                        _values.CollectionChanged -= FixupValues;
                    }
                    _values = value;
                    if (_values != null)
                    {
                        _values.CollectionChanged += FixupValues;
                    }
                    OnNavigationPropertyChanged("Values");
                }
            }
        }
        private TrackableCollection<CustomPropertyValue> _values;
    
        [DataMember]
        public TrackableCollection<CustomPropertyValue> DependentUponValues
        {
            get
            {
                if (_dependentUponValues == null)
                {
                    _dependentUponValues = new TrackableCollection<CustomPropertyValue>();
                    _dependentUponValues.CollectionChanged += FixupDependentUponValues;
                }
                return _dependentUponValues;
            }
            set
            {
                if (!ReferenceEquals(_dependentUponValues, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_dependentUponValues != null)
                    {
                        _dependentUponValues.CollectionChanged -= FixupDependentUponValues;
                    }
                    _dependentUponValues = value;
                    if (_dependentUponValues != null)
                    {
                        _dependentUponValues.CollectionChanged += FixupDependentUponValues;
                    }
                    OnNavigationPropertyChanged("DependentUponValues");
                }
            }
        }
        private TrackableCollection<CustomPropertyValue> _dependentUponValues;
    
        [DataMember]
        public ProjectTemplate ProjectTemplate
        {
            get { return _projectTemplate; }
            set
            {
                if (!ReferenceEquals(_projectTemplate, value))
                {
                    var previousValue = _projectTemplate;
                    _projectTemplate = value;
                    FixupProjectTemplate(previousValue);
                    OnNavigationPropertyChanged("ProjectTemplate");
                }
            }
        }
        private ProjectTemplate _projectTemplate;

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
            CustomProperties.Clear();
            Values.Clear();
            DependentUponValues.Clear();
            ProjectTemplate = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupProjectTemplate(ProjectTemplate previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.CustomPropertyLists.Contains(this))
            {
                previousValue.CustomPropertyLists.Remove(this);
            }
    
            if (ProjectTemplate != null)
            {
                if (!ProjectTemplate.CustomPropertyLists.Contains(this))
                {
                    ProjectTemplate.CustomPropertyLists.Add(this);
                }
    
                ProjectTemplateId = ProjectTemplate.ProjectTemplateId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("ProjectTemplate")
                    && (ChangeTracker.OriginalValues["ProjectTemplate"] == ProjectTemplate))
                {
                    ChangeTracker.OriginalValues.Remove("ProjectTemplate");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("ProjectTemplate", previousValue);
                }
                if (ProjectTemplate != null && !ProjectTemplate.ChangeTracker.ChangeTrackingEnabled)
                {
                    ProjectTemplate.StartTracking();
                }
            }
        }
    
        private void FixupCustomProperties(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (CustomProperty item in e.NewItems)
                {
                    item.List = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("CustomProperties", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CustomProperty item in e.OldItems)
                {
                    if (ReferenceEquals(item.List, this))
                    {
                        item.List = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("CustomProperties", item);
                    }
                }
            }
        }
    
        private void FixupValues(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (CustomPropertyValue item in e.NewItems)
                {
                    item.List = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Values", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CustomPropertyValue item in e.OldItems)
                {
                    if (ReferenceEquals(item.List, this))
                    {
                        item.List = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Values", item);
                    }
                }
            }
        }
    
        private void FixupDependentUponValues(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (CustomPropertyValue item in e.NewItems)
                {
                    item.DependentList = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("DependentUponValues", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CustomPropertyValue item in e.OldItems)
                {
                    if (ReferenceEquals(item.DependentList, this))
                    {
                        item.DependentList = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("DependentUponValues", item);
                    }
                }
            }
        }

        #endregion

    }
}
