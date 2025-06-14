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
    public partial class R_RiskMitigation: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int RISK_MITIGATION_ID
        {
            get { return _rISK_MITIGATION_ID; }
            set
            {
                if (_rISK_MITIGATION_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RISK_MITIGATION_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _rISK_MITIGATION_ID = value;
                    OnPropertyChanged("RISK_MITIGATION_ID");
                }
            }
        }
        private int _rISK_MITIGATION_ID;
    
        [DataMember]
        public int RISK_ID
        {
            get { return _rISK_ID; }
            set
            {
                if (_rISK_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RISK_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _rISK_ID = value;
                    OnPropertyChanged("RISK_ID");
                }
            }
        }
        private int _rISK_ID;
    
        [DataMember]
        public int POSITION
        {
            get { return _pOSITION; }
            set
            {
                if (_pOSITION != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'POSITION' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _pOSITION = value;
                    OnPropertyChanged("POSITION");
                }
            }
        }
        private int _pOSITION;
    
        [DataMember]
        public string DESCRIPTION
        {
            get { return _dESCRIPTION; }
            set
            {
                if (_dESCRIPTION != value)
                {
                    ChangeTracker.RecordOriginalValue("DESCRIPTION", _dESCRIPTION);
                    _dESCRIPTION = value;
                    OnPropertyChanged("DESCRIPTION");
                }
            }
        }
        private string _dESCRIPTION;
    
        [DataMember]
        public bool IS_DELETED
        {
            get { return _iS_DELETED; }
            set
            {
                if (_iS_DELETED != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'IS_DELETED' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _iS_DELETED = value;
                    OnPropertyChanged("IS_DELETED");
                }
            }
        }
        private bool _iS_DELETED;
    
        [DataMember]
        public System.DateTime CREATION_DATE
        {
            get { return _cREATION_DATE; }
            set
            {
                if (_cREATION_DATE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CREATION_DATE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _cREATION_DATE = value;
                    OnPropertyChanged("CREATION_DATE");
                }
            }
        }
        private System.DateTime _cREATION_DATE;
    
        [DataMember]
        public System.DateTime LAST_UPDATE_DATE
        {
            get { return _lAST_UPDATE_DATE; }
            set
            {
                if (_lAST_UPDATE_DATE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'LAST_UPDATE_DATE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _lAST_UPDATE_DATE = value;
                    OnPropertyChanged("LAST_UPDATE_DATE");
                }
            }
        }
        private System.DateTime _lAST_UPDATE_DATE;
    
        [DataMember]
        public System.DateTime CONCURRENCY_DATE
        {
            get { return _cONCURRENCY_DATE; }
            set
            {
                if (_cONCURRENCY_DATE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CONCURRENCY_DATE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _cONCURRENCY_DATE = value;
                    OnPropertyChanged("CONCURRENCY_DATE");
                }
            }
        }
        private System.DateTime _cONCURRENCY_DATE;
    
        [DataMember]
        public bool IS_ACTIVE
        {
            get { return _iS_ACTIVE; }
            set
            {
                if (_iS_ACTIVE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'IS_ACTIVE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _iS_ACTIVE = value;
                    OnPropertyChanged("IS_ACTIVE");
                }
            }
        }
        private bool _iS_ACTIVE;
    
        [DataMember]
        public Nullable<System.DateTime> REVIEW_DATE
        {
            get { return _rEVIEW_DATE; }
            set
            {
                if (_rEVIEW_DATE != value)
                {
                    ChangeTracker.RecordOriginalValue("REVIEW_DATE", _rEVIEW_DATE);
                    _rEVIEW_DATE = value;
                    OnPropertyChanged("REVIEW_DATE");
                }
            }
        }
        private Nullable<System.DateTime> _rEVIEW_DATE;
    
        [DataMember]
        public string RISK_NAME
        {
            get { return _rISK_NAME; }
            set
            {
                if (_rISK_NAME != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'RISK_NAME' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _rISK_NAME = value;
                    OnPropertyChanged("RISK_NAME");
                }
            }
        }
        private string _rISK_NAME;
    
        [DataMember]
        public Nullable<System.DateTime> RISK_REVIEW_DATE
        {
            get { return _rISK_REVIEW_DATE; }
            set
            {
                if (_rISK_REVIEW_DATE != value)
                {
                    ChangeTracker.RecordOriginalValue("RISK_REVIEW_DATE", _rISK_REVIEW_DATE);
                    _rISK_REVIEW_DATE = value;
                    OnPropertyChanged("RISK_REVIEW_DATE");
                }
            }
        }
        private Nullable<System.DateTime> _rISK_REVIEW_DATE;
    
        [DataMember]
        public bool PROJECT_IS_ACTIVE
        {
            get { return _pROJECT_IS_ACTIVE; }
            set
            {
                if (_pROJECT_IS_ACTIVE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'PROJECT_IS_ACTIVE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _pROJECT_IS_ACTIVE = value;
                    OnPropertyChanged("PROJECT_IS_ACTIVE");
                }
            }
        }
        private bool _pROJECT_IS_ACTIVE;
    
        [DataMember]
        public int PROJECT_PROJECT_GROUP_ID
        {
            get { return _pROJECT_PROJECT_GROUP_ID; }
            set
            {
                if (_pROJECT_PROJECT_GROUP_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'PROJECT_PROJECT_GROUP_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _pROJECT_PROJECT_GROUP_ID = value;
                    OnPropertyChanged("PROJECT_PROJECT_GROUP_ID");
                }
            }
        }
        private int _pROJECT_PROJECT_GROUP_ID;
    
        [DataMember]
        public string PROJECT_NAME
        {
            get { return _pROJECT_NAME; }
            set
            {
                if (_pROJECT_NAME != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'PROJECT_NAME' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _pROJECT_NAME = value;
                    OnPropertyChanged("PROJECT_NAME");
                }
            }
        }
        private string _pROJECT_NAME;

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
        }

        #endregion

    }
}
