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
    [KnownType(typeof(Incident))]
    [KnownType(typeof(User))]
    public partial class IncidentResolution: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int IncidentResolutionId
        {
            get { return _incidentResolutionId; }
            set
            {
                if (_incidentResolutionId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'IncidentResolutionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _incidentResolutionId = value;
                    OnPropertyChanged("IncidentResolutionId");
                }
            }
        }
        private int _incidentResolutionId;
    
        [DataMember]
        public int IncidentId
        {
            get { return _incidentId; }
            set
            {
                if (_incidentId != value)
                {
                    ChangeTracker.RecordOriginalValue("IncidentId", _incidentId);
                    if (!IsDeserializing)
                    {
                        if (Incident != null && Incident.IncidentId != value)
                        {
                            Incident = null;
                        }
                    }
                    _incidentId = value;
                    OnPropertyChanged("IncidentId");
                }
            }
        }
        private int _incidentId;
    
        [DataMember]
        public int CreatorId
        {
            get { return _creatorId; }
            set
            {
                if (_creatorId != value)
                {
                    ChangeTracker.RecordOriginalValue("CreatorId", _creatorId);
                    if (!IsDeserializing)
                    {
                        if (Creator != null && Creator.UserId != value)
                        {
                            Creator = null;
                        }
                    }
                    _creatorId = value;
                    OnPropertyChanged("CreatorId");
                }
            }
        }
        private int _creatorId;
    
        [DataMember]
        public string Resolution
        {
            get { return _resolution; }
            set
            {
                if (_resolution != value)
                {
                    ChangeTracker.RecordOriginalValue("Resolution", _resolution);
                    _resolution = value;
                    OnPropertyChanged("Resolution");
                }
            }
        }
        private string _resolution;
    
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

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Incident Incident
        {
            get { return _incident; }
            set
            {
                if (!ReferenceEquals(_incident, value))
                {
                    var previousValue = _incident;
                    _incident = value;
                    FixupIncident(previousValue);
                    OnNavigationPropertyChanged("Incident");
                }
            }
        }
        private Incident _incident;
    
        [DataMember]
        public User Creator
        {
            get { return _creator; }
            set
            {
                if (!ReferenceEquals(_creator, value))
                {
                    var previousValue = _creator;
                    _creator = value;
                    FixupCreator(previousValue);
                    OnNavigationPropertyChanged("Creator");
                }
            }
        }
        private User _creator;

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
            Incident = null;
            Creator = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupIncident(Incident previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Resolutions.Contains(this))
            {
                previousValue.Resolutions.Remove(this);
            }
    
            if (Incident != null)
            {
                if (!Incident.Resolutions.Contains(this))
                {
                    Incident.Resolutions.Add(this);
                }
    
                IncidentId = Incident.IncidentId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Incident")
                    && (ChangeTracker.OriginalValues["Incident"] == Incident))
                {
                    ChangeTracker.OriginalValues.Remove("Incident");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Incident", previousValue);
                }
                if (Incident != null && !Incident.ChangeTracker.ChangeTrackingEnabled)
                {
                    Incident.StartTracking();
                }
            }
        }
    
        private void FixupCreator(User previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.IncidentResolutions.Contains(this))
            {
                previousValue.IncidentResolutions.Remove(this);
            }
    
            if (Creator != null)
            {
                if (!Creator.IncidentResolutions.Contains(this))
                {
                    Creator.IncidentResolutions.Add(this);
                }
    
                CreatorId = Creator.UserId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Creator")
                    && (ChangeTracker.OriginalValues["Creator"] == Creator))
                {
                    ChangeTracker.OriginalValues.Remove("Creator");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Creator", previousValue);
                }
                if (Creator != null && !Creator.ChangeTracker.ChangeTrackingEnabled)
                {
                    Creator.StartTracking();
                }
            }
        }

        #endregion

    }
}
