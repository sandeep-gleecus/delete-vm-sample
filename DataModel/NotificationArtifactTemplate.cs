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
    [KnownType(typeof(ProjectTemplate))]
    public partial class NotificationArtifactTemplate: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public string TemplateText
        {
            get { return _templateText; }
            set
            {
                if (_templateText != value)
                {
                    ChangeTracker.RecordOriginalValue("TemplateText", _templateText);
                    _templateText = value;
                    OnPropertyChanged("TemplateText");
                }
            }
        }
        private string _templateText;
    
        [DataMember]
        public int ArtifactTypeId
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
        private int _artifactTypeId;
    
        [DataMember]
        public int NotificationTemplateId
        {
            get { return _notificationTemplateId; }
            set
            {
                if (_notificationTemplateId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'NotificationTemplateId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _notificationTemplateId = value;
                    OnPropertyChanged("NotificationTemplateId");
                }
            }
        }
        private int _notificationTemplateId;
    
        [DataMember]
        public Nullable<int> ProjectTemplateId
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
        private Nullable<int> _projectTemplateId;

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
    
        protected virtual void ClearNavigationProperties()
        {
            ArtifactType = null;
            ProjectTemplate = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupArtifactType(ArtifactType previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.NotificationTemplate.Contains(this))
            {
                previousValue.NotificationTemplate.Remove(this);
            }
    
            if (ArtifactType != null)
            {
                if (!ArtifactType.NotificationTemplate.Contains(this))
                {
                    ArtifactType.NotificationTemplate.Add(this);
                }
    
                ArtifactTypeId = ArtifactType.ArtifactTypeId;
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
    
        private void FixupProjectTemplate(ProjectTemplate previousValue, bool skipKeys = false)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.NotificationTemplates.Contains(this))
            {
                previousValue.NotificationTemplates.Remove(this);
            }
    
            if (ProjectTemplate != null)
            {
                if (!ProjectTemplate.NotificationTemplates.Contains(this))
                {
                    ProjectTemplate.NotificationTemplates.Add(this);
                }
    
                ProjectTemplateId = ProjectTemplate.ProjectTemplateId;
            }
            else if (!skipKeys)
            {
                ProjectTemplateId = null;
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

        #endregion

    }
}
