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
    [KnownType(typeof(Requirement))]
    public partial class RequirementStep: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int RequirementStepId
        {
            get { return _requirementStepId; }
            set
            {
                if (_requirementStepId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RequirementStepId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _requirementStepId = value;
                    OnPropertyChanged("RequirementStepId");
                }
            }
        }
        private int _requirementStepId;
    
        [DataMember]
        public int RequirementId
        {
            get { return _requirementId; }
            set
            {
                if (_requirementId != value)
                {
                    ChangeTracker.RecordOriginalValue("RequirementId", _requirementId);
                    if (!IsDeserializing)
                    {
                        if (Requirement != null && Requirement.RequirementId != value)
                        {
                            Requirement = null;
                        }
                    }
                    _requirementId = value;
                    OnPropertyChanged("RequirementId");
                }
            }
        }
        private int _requirementId;
    
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
        public System.DateTime CreationDate
        {
            get { return _creationDate; }
            set
            {
                if (_creationDate != value)
                {
                    ChangeTracker.RecordOriginalValue("CreationDate", _creationDate);
                    _creationDate = value;
                    OnPropertyChanged("CreationDate");
                }
            }
        }
        private System.DateTime _creationDate;
    
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
        public int Position
        {
            get { return _position; }
            internal set
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

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Requirement Requirement
        {
            get { return _requirement; }
            set
            {
                if (!ReferenceEquals(_requirement, value))
                {
                    var previousValue = _requirement;
                    _requirement = value;
                    FixupRequirement(previousValue);
                    OnNavigationPropertyChanged("Requirement");
                }
            }
        }
        private Requirement _requirement;

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
            Requirement = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupRequirement(Requirement previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Steps.Contains(this))
            {
                previousValue.Steps.Remove(this);
            }
    
            if (Requirement != null)
            {
                if (!Requirement.Steps.Contains(this))
                {
                    Requirement.Steps.Add(this);
                }
    
                RequirementId = Requirement.RequirementId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Requirement")
                    && (ChangeTracker.OriginalValues["Requirement"] == Requirement))
                {
                    ChangeTracker.OriginalValues.Remove("Requirement");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Requirement", previousValue);
                }
                if (Requirement != null && !Requirement.ChangeTracker.ChangeTrackingEnabled)
                {
                    Requirement.StartTracking();
                }
            }
        }

        #endregion

    }
}
