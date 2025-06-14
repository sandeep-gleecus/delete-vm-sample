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
    [KnownType(typeof(ProjectTemplate))]
    [KnownType(typeof(Risk))]
    public partial class RiskDetectability: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int RiskDetectabilityId
        {
            get { return _riskDetectabilityId; }
            set
            {
                if (_riskDetectabilityId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RiskDetectabilityId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _riskDetectabilityId = value;
                    OnPropertyChanged("RiskDetectabilityId");
                }
            }
        }
        private int _riskDetectabilityId;
    
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
        public string Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    ChangeTracker.RecordOriginalValue("Color", _color);
                    _color = value;
                    OnPropertyChanged("Color");
                }
            }
        }
        private string _color;
    
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
        public int Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    ChangeTracker.RecordOriginalValue("Position", _position);
                    _position = value;
                    OnPropertyChanged("Position");
                }
            }
        }
        private int _position;
    
        [DataMember]
        public int Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    ChangeTracker.RecordOriginalValue("Score", _score);
                    _score = value;
                    OnPropertyChanged("Score");
                }
            }
        }
        private int _score;

        #endregion

        #region Navigation Properties
    
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
    
        [DataMember]
        public TrackableCollection<Risk> Risks
        {
            get
            {
                if (_risks == null)
                {
                    _risks = new TrackableCollection<Risk>();
                    _risks.CollectionChanged += FixupRisks;
                }
                return _risks;
            }
            set
            {
                if (!ReferenceEquals(_risks, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_risks != null)
                    {
                        _risks.CollectionChanged -= FixupRisks;
                    }
                    _risks = value;
                    if (_risks != null)
                    {
                        _risks.CollectionChanged += FixupRisks;
                    }
                    OnNavigationPropertyChanged("Risks");
                }
            }
        }
        private TrackableCollection<Risk> _risks;

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
            ProjectTemplate = null;
            Risks.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupProjectTemplate(ProjectTemplate previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.RiskDetectabilities.Contains(this))
            {
                previousValue.RiskDetectabilities.Remove(this);
            }
    
            if (ProjectTemplate != null)
            {
                if (!ProjectTemplate.RiskDetectabilities.Contains(this))
                {
                    ProjectTemplate.RiskDetectabilities.Add(this);
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
    
        private void FixupRisks(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Risk item in e.NewItems)
                {
                    item.Detectability = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Risks", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Risk item in e.OldItems)
                {
                    if (ReferenceEquals(item.Detectability, this))
                    {
                        item.Detectability = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Risks", item);
                    }
                }
            }
        }

        #endregion

    }
}
