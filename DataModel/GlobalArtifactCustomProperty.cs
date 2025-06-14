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
    public partial class GlobalArtifactCustomProperty: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int ArtifactId
        {
            get { return _artifactId; }
            set
            {
                if (_artifactId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ArtifactId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _artifactId = value;
                    OnPropertyChanged("ArtifactId");
                }
            }
        }
        private int _artifactId;
    
        [DataMember]
        public int ArtifactTypeId
        {
            get { return _artifactTypeId; }
            set
            {
                if (_artifactTypeId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ArtifactTypeId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
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
        public string Custom_01
        {
            get { return _custom_01; }
            set
            {
                if (_custom_01 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_01", _custom_01);
                    _custom_01 = value;
                    OnPropertyChanged("Custom_01");
                }
            }
        }
        private string _custom_01;
    
        [DataMember]
        public string Custom_02
        {
            get { return _custom_02; }
            set
            {
                if (_custom_02 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_02", _custom_02);
                    _custom_02 = value;
                    OnPropertyChanged("Custom_02");
                }
            }
        }
        private string _custom_02;
    
        [DataMember]
        public string Custom_03
        {
            get { return _custom_03; }
            set
            {
                if (_custom_03 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_03", _custom_03);
                    _custom_03 = value;
                    OnPropertyChanged("Custom_03");
                }
            }
        }
        private string _custom_03;
    
        [DataMember]
        public string Custom_04
        {
            get { return _custom_04; }
            set
            {
                if (_custom_04 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_04", _custom_04);
                    _custom_04 = value;
                    OnPropertyChanged("Custom_04");
                }
            }
        }
        private string _custom_04;
    
        [DataMember]
        public string Custom_05
        {
            get { return _custom_05; }
            set
            {
                if (_custom_05 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_05", _custom_05);
                    _custom_05 = value;
                    OnPropertyChanged("Custom_05");
                }
            }
        }
        private string _custom_05;
    
        [DataMember]
        public string Custom_06
        {
            get { return _custom_06; }
            set
            {
                if (_custom_06 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_06", _custom_06);
                    _custom_06 = value;
                    OnPropertyChanged("Custom_06");
                }
            }
        }
        private string _custom_06;
    
        [DataMember]
        public string Custom_07
        {
            get { return _custom_07; }
            set
            {
                if (_custom_07 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_07", _custom_07);
                    _custom_07 = value;
                    OnPropertyChanged("Custom_07");
                }
            }
        }
        private string _custom_07;
    
        [DataMember]
        public string Custom_08
        {
            get { return _custom_08; }
            set
            {
                if (_custom_08 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_08", _custom_08);
                    _custom_08 = value;
                    OnPropertyChanged("Custom_08");
                }
            }
        }
        private string _custom_08;
    
        [DataMember]
        public string Custom_09
        {
            get { return _custom_09; }
            set
            {
                if (_custom_09 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_09", _custom_09);
                    _custom_09 = value;
                    OnPropertyChanged("Custom_09");
                }
            }
        }
        private string _custom_09;
    
        [DataMember]
        public string Custom_10
        {
            get { return _custom_10; }
            set
            {
                if (_custom_10 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_10", _custom_10);
                    _custom_10 = value;
                    OnPropertyChanged("Custom_10");
                }
            }
        }
        private string _custom_10;
    
        [DataMember]
        public string Custom_11
        {
            get { return _custom_11; }
            set
            {
                if (_custom_11 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_11", _custom_11);
                    _custom_11 = value;
                    OnPropertyChanged("Custom_11");
                }
            }
        }
        private string _custom_11;
    
        [DataMember]
        public string Custom_12
        {
            get { return _custom_12; }
            set
            {
                if (_custom_12 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_12", _custom_12);
                    _custom_12 = value;
                    OnPropertyChanged("Custom_12");
                }
            }
        }
        private string _custom_12;
    
        [DataMember]
        public string Custom_13
        {
            get { return _custom_13; }
            set
            {
                if (_custom_13 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_13", _custom_13);
                    _custom_13 = value;
                    OnPropertyChanged("Custom_13");
                }
            }
        }
        private string _custom_13;
    
        [DataMember]
        public string Custom_14
        {
            get { return _custom_14; }
            set
            {
                if (_custom_14 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_14", _custom_14);
                    _custom_14 = value;
                    OnPropertyChanged("Custom_14");
                }
            }
        }
        private string _custom_14;
    
        [DataMember]
        public string Custom_15
        {
            get { return _custom_15; }
            set
            {
                if (_custom_15 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_15", _custom_15);
                    _custom_15 = value;
                    OnPropertyChanged("Custom_15");
                }
            }
        }
        private string _custom_15;
    
        [DataMember]
        public string Custom_16
        {
            get { return _custom_16; }
            set
            {
                if (_custom_16 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_16", _custom_16);
                    _custom_16 = value;
                    OnPropertyChanged("Custom_16");
                }
            }
        }
        private string _custom_16;
    
        [DataMember]
        public string Custom_17
        {
            get { return _custom_17; }
            set
            {
                if (_custom_17 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_17", _custom_17);
                    _custom_17 = value;
                    OnPropertyChanged("Custom_17");
                }
            }
        }
        private string _custom_17;
    
        [DataMember]
        public string Custom_18
        {
            get { return _custom_18; }
            set
            {
                if (_custom_18 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_18", _custom_18);
                    _custom_18 = value;
                    OnPropertyChanged("Custom_18");
                }
            }
        }
        private string _custom_18;
    
        [DataMember]
        public string Custom_19
        {
            get { return _custom_19; }
            set
            {
                if (_custom_19 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_19", _custom_19);
                    _custom_19 = value;
                    OnPropertyChanged("Custom_19");
                }
            }
        }
        private string _custom_19;
    
        [DataMember]
        public string Custom_20
        {
            get { return _custom_20; }
            set
            {
                if (_custom_20 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_20", _custom_20);
                    _custom_20 = value;
                    OnPropertyChanged("Custom_20");
                }
            }
        }
        private string _custom_20;
    
        [DataMember]
        public string Custom_21
        {
            get { return _custom_21; }
            set
            {
                if (_custom_21 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_21", _custom_21);
                    _custom_21 = value;
                    OnPropertyChanged("Custom_21");
                }
            }
        }
        private string _custom_21;
    
        [DataMember]
        public string Custom_22
        {
            get { return _custom_22; }
            set
            {
                if (_custom_22 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_22", _custom_22);
                    _custom_22 = value;
                    OnPropertyChanged("Custom_22");
                }
            }
        }
        private string _custom_22;
    
        [DataMember]
        public string Custom_23
        {
            get { return _custom_23; }
            set
            {
                if (_custom_23 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_23", _custom_23);
                    _custom_23 = value;
                    OnPropertyChanged("Custom_23");
                }
            }
        }
        private string _custom_23;
    
        [DataMember]
        public string Custom_24
        {
            get { return _custom_24; }
            set
            {
                if (_custom_24 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_24", _custom_24);
                    _custom_24 = value;
                    OnPropertyChanged("Custom_24");
                }
            }
        }
        private string _custom_24;
    
        [DataMember]
        public string Custom_25
        {
            get { return _custom_25; }
            set
            {
                if (_custom_25 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_25", _custom_25);
                    _custom_25 = value;
                    OnPropertyChanged("Custom_25");
                }
            }
        }
        private string _custom_25;
    
        [DataMember]
        public string Custom_26
        {
            get { return _custom_26; }
            set
            {
                if (_custom_26 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_26", _custom_26);
                    _custom_26 = value;
                    OnPropertyChanged("Custom_26");
                }
            }
        }
        private string _custom_26;
    
        [DataMember]
        public string Custom_27
        {
            get { return _custom_27; }
            set
            {
                if (_custom_27 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_27", _custom_27);
                    _custom_27 = value;
                    OnPropertyChanged("Custom_27");
                }
            }
        }
        private string _custom_27;
    
        [DataMember]
        public string Custom_28
        {
            get { return _custom_28; }
            set
            {
                if (_custom_28 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_28", _custom_28);
                    _custom_28 = value;
                    OnPropertyChanged("Custom_28");
                }
            }
        }
        private string _custom_28;
    
        [DataMember]
        public string Custom_29
        {
            get { return _custom_29; }
            set
            {
                if (_custom_29 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_29", _custom_29);
                    _custom_29 = value;
                    OnPropertyChanged("Custom_29");
                }
            }
        }
        private string _custom_29;
    
        [DataMember]
        public string Custom_30
        {
            get { return _custom_30; }
            set
            {
                if (_custom_30 != value)
                {
                    ChangeTracker.RecordOriginalValue("Custom_30", _custom_30);
                    _custom_30 = value;
                    OnPropertyChanged("Custom_30");
                }
            }
        }
        private string _custom_30;

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
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (ArtifactTypeId != value.ArtifactTypeId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _artifactType;
                    _artifactType = value;
                    FixupArtifactType(previousValue);
                    OnNavigationPropertyChanged("ArtifactType");
                }
            }
        }
        private ArtifactType _artifactType;

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
            ArtifactType = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupArtifactType(ArtifactType previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.GlobalArtifactCustomProperties.Contains(this))
            {
                previousValue.GlobalArtifactCustomProperties.Remove(this);
            }
    
            if (ArtifactType != null)
            {
                if (!ArtifactType.GlobalArtifactCustomProperties.Contains(this))
                {
                    ArtifactType.GlobalArtifactCustomProperties.Add(this);
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

        #endregion

    }
}
