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
    public partial class VW_ADMINHISTORYCHANGE_LIST: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public long CHANGESET_ID
        {
            get { return _cHANGESET_ID; }
            set
            {
                if (_cHANGESET_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CHANGESET_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _cHANGESET_ID = value;
                    OnPropertyChanged("CHANGESET_ID");
                }
            }
        }
        private long _cHANGESET_ID;
    
        [DataMember]
        public Nullable<int> ADMIN_USER_ID
        {
            get { return _aDMIN_USER_ID; }
            set
            {
                if (_aDMIN_USER_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_USER_ID", _aDMIN_USER_ID);
                    _aDMIN_USER_ID = value;
                    OnPropertyChanged("ADMIN_USER_ID");
                }
            }
        }
        private Nullable<int> _aDMIN_USER_ID;
    
        [DataMember]
        public System.DateTime CHANGE_DATE
        {
            get { return _cHANGE_DATE; }
            set
            {
                if (_cHANGE_DATE != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CHANGE_DATE' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _cHANGE_DATE = value;
                    OnPropertyChanged("CHANGE_DATE");
                }
            }
        }
        private System.DateTime _cHANGE_DATE;
    
        [DataMember]
        public int ADMIN_SECTION_ID
        {
            get { return _aDMIN_SECTION_ID; }
            set
            {
                if (_aDMIN_SECTION_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ADMIN_SECTION_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _aDMIN_SECTION_ID = value;
                    OnPropertyChanged("ADMIN_SECTION_ID");
                }
            }
        }
        private int _aDMIN_SECTION_ID;
    
        [DataMember]
        public Nullable<int> HISTORY_CHANGESET_TYPE_ID
        {
            get { return _hISTORY_CHANGESET_TYPE_ID; }
            set
            {
                if (_hISTORY_CHANGESET_TYPE_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("HISTORY_CHANGESET_TYPE_ID", _hISTORY_CHANGESET_TYPE_ID);
                    _hISTORY_CHANGESET_TYPE_ID = value;
                    OnPropertyChanged("HISTORY_CHANGESET_TYPE_ID");
                }
            }
        }
        private Nullable<int> _hISTORY_CHANGESET_TYPE_ID;
    
        [DataMember]
        public string ACTION_DESCRIPTION
        {
            get { return _aCTION_DESCRIPTION; }
            set
            {
                if (_aCTION_DESCRIPTION != value)
                {
                    ChangeTracker.RecordOriginalValue("ACTION_DESCRIPTION", _aCTION_DESCRIPTION);
                    _aCTION_DESCRIPTION = value;
                    OnPropertyChanged("ACTION_DESCRIPTION");
                }
            }
        }
        private string _aCTION_DESCRIPTION;
    
        [DataMember]
        public Nullable<int> ARTIFACT_ID
        {
            get { return _aRTIFACT_ID; }
            set
            {
                if (_aRTIFACT_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ARTIFACT_ID", _aRTIFACT_ID);
                    _aRTIFACT_ID = value;
                    OnPropertyChanged("ARTIFACT_ID");
                }
            }
        }
        private Nullable<int> _aRTIFACT_ID;
    
        [DataMember]
        public Nullable<System.Guid> ARTIFACT_GUID_ID
        {
            get { return _aRTIFACT_GUID_ID; }
            set
            {
                if (_aRTIFACT_GUID_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ARTIFACT_GUID_ID", _aRTIFACT_GUID_ID);
                    _aRTIFACT_GUID_ID = value;
                    OnPropertyChanged("ARTIFACT_GUID_ID");
                }
            }
        }
        private Nullable<System.Guid> _aRTIFACT_GUID_ID;
    
        [DataMember]
        public long ADMIN_HISTORY_DETAIL_ID
        {
            get { return _aDMIN_HISTORY_DETAIL_ID; }
            set
            {
                if (_aDMIN_HISTORY_DETAIL_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ADMIN_HISTORY_DETAIL_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _aDMIN_HISTORY_DETAIL_ID = value;
                    OnPropertyChanged("ADMIN_HISTORY_DETAIL_ID");
                }
            }
        }
        private long _aDMIN_HISTORY_DETAIL_ID;
    
        [DataMember]
        public string ADMIN_ARTIFACT_FIELD_NAME
        {
            get { return _aDMIN_ARTIFACT_FIELD_NAME; }
            set
            {
                if (_aDMIN_ARTIFACT_FIELD_NAME != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_ARTIFACT_FIELD_NAME", _aDMIN_ARTIFACT_FIELD_NAME);
                    _aDMIN_ARTIFACT_FIELD_NAME = value;
                    OnPropertyChanged("ADMIN_ARTIFACT_FIELD_NAME");
                }
            }
        }
        private string _aDMIN_ARTIFACT_FIELD_NAME;
    
        [DataMember]
        public string OLD_VALUE
        {
            get { return _oLD_VALUE; }
            set
            {
                if (_oLD_VALUE != value)
                {
                    ChangeTracker.RecordOriginalValue("OLD_VALUE", _oLD_VALUE);
                    _oLD_VALUE = value;
                    OnPropertyChanged("OLD_VALUE");
                }
            }
        }
        private string _oLD_VALUE;
    
        [DataMember]
        public string ADMIN_ARTIFACT_FIELD_CAPTION
        {
            get { return _aDMIN_ARTIFACT_FIELD_CAPTION; }
            set
            {
                if (_aDMIN_ARTIFACT_FIELD_CAPTION != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_ARTIFACT_FIELD_CAPTION", _aDMIN_ARTIFACT_FIELD_CAPTION);
                    _aDMIN_ARTIFACT_FIELD_CAPTION = value;
                    OnPropertyChanged("ADMIN_ARTIFACT_FIELD_CAPTION");
                }
            }
        }
        private string _aDMIN_ARTIFACT_FIELD_CAPTION;
    
        [DataMember]
        public string NEW_VALUE
        {
            get { return _nEW_VALUE; }
            set
            {
                if (_nEW_VALUE != value)
                {
                    ChangeTracker.RecordOriginalValue("NEW_VALUE", _nEW_VALUE);
                    _nEW_VALUE = value;
                    OnPropertyChanged("NEW_VALUE");
                }
            }
        }
        private string _nEW_VALUE;
    
        [DataMember]
        public Nullable<int> OLD_VALUE_INT
        {
            get { return _oLD_VALUE_INT; }
            set
            {
                if (_oLD_VALUE_INT != value)
                {
                    ChangeTracker.RecordOriginalValue("OLD_VALUE_INT", _oLD_VALUE_INT);
                    _oLD_VALUE_INT = value;
                    OnPropertyChanged("OLD_VALUE_INT");
                }
            }
        }
        private Nullable<int> _oLD_VALUE_INT;
    
        [DataMember]
        public Nullable<System.DateTime> OLD_VALUE_DATE
        {
            get { return _oLD_VALUE_DATE; }
            set
            {
                if (_oLD_VALUE_DATE != value)
                {
                    ChangeTracker.RecordOriginalValue("OLD_VALUE_DATE", _oLD_VALUE_DATE);
                    _oLD_VALUE_DATE = value;
                    OnPropertyChanged("OLD_VALUE_DATE");
                }
            }
        }
        private Nullable<System.DateTime> _oLD_VALUE_DATE;
    
        [DataMember]
        public Nullable<int> NEW_VALUE_INT
        {
            get { return _nEW_VALUE_INT; }
            set
            {
                if (_nEW_VALUE_INT != value)
                {
                    ChangeTracker.RecordOriginalValue("NEW_VALUE_INT", _nEW_VALUE_INT);
                    _nEW_VALUE_INT = value;
                    OnPropertyChanged("NEW_VALUE_INT");
                }
            }
        }
        private Nullable<int> _nEW_VALUE_INT;
    
        [DataMember]
        public Nullable<System.DateTime> NEW_VALUE_DATE
        {
            get { return _nEW_VALUE_DATE; }
            set
            {
                if (_nEW_VALUE_DATE != value)
                {
                    ChangeTracker.RecordOriginalValue("NEW_VALUE_DATE", _nEW_VALUE_DATE);
                    _nEW_VALUE_DATE = value;
                    OnPropertyChanged("NEW_VALUE_DATE");
                }
            }
        }
        private Nullable<System.DateTime> _nEW_VALUE_DATE;
    
        [DataMember]
        public long ADMIN_CHANGESET_ID
        {
            get { return _aDMIN_CHANGESET_ID; }
            set
            {
                if (_aDMIN_CHANGESET_ID != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ADMIN_CHANGESET_ID' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _aDMIN_CHANGESET_ID = value;
                    OnPropertyChanged("ADMIN_CHANGESET_ID");
                }
            }
        }
        private long _aDMIN_CHANGESET_ID;
    
        [DataMember]
        public Nullable<int> ADMIN_ARTIFACT_FIELD_ID
        {
            get { return _aDMIN_ARTIFACT_FIELD_ID; }
            set
            {
                if (_aDMIN_ARTIFACT_FIELD_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_ARTIFACT_FIELD_ID", _aDMIN_ARTIFACT_FIELD_ID);
                    _aDMIN_ARTIFACT_FIELD_ID = value;
                    OnPropertyChanged("ADMIN_ARTIFACT_FIELD_ID");
                }
            }
        }
        private Nullable<int> _aDMIN_ARTIFACT_FIELD_ID;
    
        [DataMember]
        public Nullable<int> ADMIN_CUSTOM_PROPERTY_ID
        {
            get { return _aDMIN_CUSTOM_PROPERTY_ID; }
            set
            {
                if (_aDMIN_CUSTOM_PROPERTY_ID != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_CUSTOM_PROPERTY_ID", _aDMIN_CUSTOM_PROPERTY_ID);
                    _aDMIN_CUSTOM_PROPERTY_ID = value;
                    OnPropertyChanged("ADMIN_CUSTOM_PROPERTY_ID");
                }
            }
        }
        private Nullable<int> _aDMIN_CUSTOM_PROPERTY_ID;
    
        [DataMember]
        public string CHANGETYPE_NAME
        {
            get { return _cHANGETYPE_NAME; }
            set
            {
                if (_cHANGETYPE_NAME != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CHANGETYPE_NAME' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _cHANGETYPE_NAME = value;
                    OnPropertyChanged("CHANGETYPE_NAME");
                }
            }
        }
        private string _cHANGETYPE_NAME;
    
        [DataMember]
        public string USER_NAME
        {
            get { return _uSER_NAME; }
            set
            {
                if (_uSER_NAME != value)
                {
                    ChangeTracker.RecordOriginalValue("USER_NAME", _uSER_NAME);
                    _uSER_NAME = value;
                    OnPropertyChanged("USER_NAME");
                }
            }
        }
        private string _uSER_NAME;
    
        [DataMember]
        public string ADMIN_SECTION_NAME
        {
            get { return _aDMIN_SECTION_NAME; }
            set
            {
                if (_aDMIN_SECTION_NAME != value)
                {
                    ChangeTracker.RecordOriginalValue("ADMIN_SECTION_NAME", _aDMIN_SECTION_NAME);
                    _aDMIN_SECTION_NAME = value;
                    OnPropertyChanged("ADMIN_SECTION_NAME");
                }
            }
        }
        private string _aDMIN_SECTION_NAME;

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
