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
    [KnownType(typeof(User))]
    public partial class TST_REQUIREMENT_APPROVAL_USERS: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int REQUIREMENT_APPROVAL_USER_ID
        {
            get { return _rEQUIREMENT_APPROVAL_USER_ID; }
            set
            {
                if (_rEQUIREMENT_APPROVAL_USER_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'REQUIREMENT_APPROVAL_USER_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _rEQUIREMENT_APPROVAL_USER_ID = value;
                    OnPropertyChanged("REQUIREMENT_APPROVAL_USER_ID");
                }
            }
        }
        private int _rEQUIREMENT_APPROVAL_USER_ID;
    
        [DataMember]
        public int PROJECT_ID
        {
            get { return _pROJECT_ID; }
            set
            {
                if (_pROJECT_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("PROJECT_ID", _pROJECT_ID);
                    _pROJECT_ID = value;
                    OnPropertyChanged("PROJECT_ID");
                }
            }
        }
        private int _pROJECT_ID;
    
        [DataMember]
        public int USER_ID
        {
            get { return _uSER_ID; }
            set
            {
                if (_uSER_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("USER_ID", _uSER_ID);
                    if (!IsDeserializing)
                    {
                        if (TST_USER != null && TST_USER.UserId != value)
                        {
                            TST_USER = null;
                        }
                    }
                    _uSER_ID = value;
                    OnPropertyChanged("USER_ID");
                }
            }
        }
        private int _uSER_ID;
    
        [DataMember]
        public int WORKFLOW_TRANSITION_ID
        {
            get { return _wORKFLOW_TRANSITION_ID; }
            set
            {
                if (_wORKFLOW_TRANSITION_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("WORKFLOW_TRANSITION_ID", _wORKFLOW_TRANSITION_ID);
                    _wORKFLOW_TRANSITION_ID = value;
                    OnPropertyChanged("WORKFLOW_TRANSITION_ID");
                }
            }
        }
        private int _wORKFLOW_TRANSITION_ID;
    
        [DataMember]
        public bool IS_ACTIVE
        {
            get { return _iS_ACTIVE; }
            set
            {
                if (_iS_ACTIVE != value)
                {
                    ChangeTracker.RecordOriginalValue("IS_ACTIVE", _iS_ACTIVE);
                    _iS_ACTIVE = value;
                    OnPropertyChanged("IS_ACTIVE");
                }
            }
        }
        private bool _iS_ACTIVE;
    
        [DataMember]
        public System.DateTime UPDATE_DATE
        {
            get { return _uPDATE_DATE; }
            set
            {
                if (_uPDATE_DATE != value)
                {
                    ChangeTracker.RecordOriginalValue("UPDATE_DATE", _uPDATE_DATE);
                    _uPDATE_DATE = value;
                    OnPropertyChanged("UPDATE_DATE");
                }
            }
        }
        private System.DateTime _uPDATE_DATE;
    
        [DataMember]
        public Nullable<short> ORDER_ID
        {
            get { return _oRDER_ID; }
            set
            {
                if (_oRDER_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ORDER_ID", _oRDER_ID);
                    _oRDER_ID = value;
                    OnPropertyChanged("ORDER_ID");
                }
            }
        }
        private Nullable<short> _oRDER_ID;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public User TST_USER
        {
            get { return _tST_USER; }
            set
            {
                if (!ReferenceEquals(_tST_USER, value))
                {
                    var previousValue = _tST_USER;
                    _tST_USER = value;
                    FixupTST_USER(previousValue);
                    OnNavigationPropertyChanged("TST_USER");
                }
            }
        }
        private User _tST_USER;

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
            TST_USER = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupTST_USER(User previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TST_REQUIREMENT_APPROVAL_USERS.Contains(this))
            {
                previousValue.TST_REQUIREMENT_APPROVAL_USERS.Remove(this);
            }
    
            if (TST_USER != null)
            {
                if (!TST_USER.TST_REQUIREMENT_APPROVAL_USERS.Contains(this))
                {
                    TST_USER.TST_REQUIREMENT_APPROVAL_USERS.Add(this);
                }
    
                USER_ID = TST_USER.UserId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("TST_USER")
                    && (ChangeTracker.OriginalValues["TST_USER"] == TST_USER))
                {
                    ChangeTracker.OriginalValues.Remove("TST_USER");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("TST_USER", previousValue);
                }
                if (TST_USER != null && !TST_USER.ChangeTracker.ChangeTrackingEnabled)
                {
                    TST_USER.StartTracking();
                }
            }
        }

        #endregion

    }
}
