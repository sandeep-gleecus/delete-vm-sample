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
    [KnownType(typeof(Report))]
    [KnownType(typeof(ReportFormat))]
    [KnownType(typeof(User))]
    public partial class ReportGenerated: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int ReportGeneratedId
        {
            get { return _reportGeneratedId; }
            set
            {
                if (_reportGeneratedId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ReportGeneratedId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _reportGeneratedId = value;
                    OnPropertyChanged("ReportGeneratedId");
                }
            }
        }
        private int _reportGeneratedId;
    
        [DataMember]
        public int ReportId
        {
            get { return _reportId; }
            set
            {
                if (_reportId != value)
                {
                    ChangeTracker.RecordOriginalValue("ReportId", _reportId);
                    if (!IsDeserializing)
                    {
                        if (Report != null && Report.ReportId != value)
                        {
                            Report = null;
                        }
                    }
                    _reportId = value;
                    OnPropertyChanged("ReportId");
                }
            }
        }
        private int _reportId;
    
        [DataMember]
        public int ReportFormatId
        {
            get { return _reportFormatId; }
            set
            {
                if (_reportFormatId != value)
                {
                    ChangeTracker.RecordOriginalValue("ReportFormatId", _reportFormatId);
                    if (!IsDeserializing)
                    {
                        if (Format != null && Format.ReportFormatId != value)
                        {
                            Format = null;
                        }
                    }
                    _reportFormatId = value;
                    OnPropertyChanged("ReportFormatId");
                }
            }
        }
        private int _reportFormatId;
    
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

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Report Report
        {
            get { return _report; }
            set
            {
                if (!ReferenceEquals(_report, value))
                {
                    var previousValue = _report;
                    _report = value;
                    FixupReport(previousValue);
                    OnNavigationPropertyChanged("Report");
                }
            }
        }
        private Report _report;
    
        [DataMember]
        public ReportFormat Format
        {
            get { return _format; }
            set
            {
                if (!ReferenceEquals(_format, value))
                {
                    var previousValue = _format;
                    _format = value;
                    FixupFormat(previousValue);
                    OnNavigationPropertyChanged("Format");
                }
            }
        }
        private ReportFormat _format;
    
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
            Report = null;
            Format = null;
            User = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupReport(Report previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.GeneratedReports.Contains(this))
            {
                previousValue.GeneratedReports.Remove(this);
            }
    
            if (Report != null)
            {
                if (!Report.GeneratedReports.Contains(this))
                {
                    Report.GeneratedReports.Add(this);
                }
    
                ReportId = Report.ReportId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Report")
                    && (ChangeTracker.OriginalValues["Report"] == Report))
                {
                    ChangeTracker.OriginalValues.Remove("Report");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Report", previousValue);
                }
                if (Report != null && !Report.ChangeTracker.ChangeTrackingEnabled)
                {
                    Report.StartTracking();
                }
            }
        }
    
        private void FixupFormat(ReportFormat previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.GeneratedReports.Contains(this))
            {
                previousValue.GeneratedReports.Remove(this);
            }
    
            if (Format != null)
            {
                if (!Format.GeneratedReports.Contains(this))
                {
                    Format.GeneratedReports.Add(this);
                }
    
                ReportFormatId = Format.ReportFormatId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Format")
                    && (ChangeTracker.OriginalValues["Format"] == Format))
                {
                    ChangeTracker.OriginalValues.Remove("Format");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Format", previousValue);
                }
                if (Format != null && !Format.ChangeTracker.ChangeTrackingEnabled)
                {
                    Format.StartTracking();
                }
            }
        }
    
        private void FixupUser(User previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.GeneratedReports.Contains(this))
            {
                previousValue.GeneratedReports.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.GeneratedReports.Contains(this))
                {
                    User.GeneratedReports.Add(this);
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

        #endregion

    }
}
