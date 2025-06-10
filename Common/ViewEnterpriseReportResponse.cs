using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Common
{
	public class ViewEnterpriseReportResponse
	{

		[DataMember]
		public long DOWNLOADABLE_REPORT_ID
		{
			get { return _dOWNLOADABLE_REPORT_ID; }
			set
			{
				_dOWNLOADABLE_REPORT_ID = value;
			}
		}
		private long _dOWNLOADABLE_REPORT_ID;

		[DataMember]
		public string REPORT_NUMBER
		{
			get { return _rEPORT_NUMBER; }
			set
			{
				_rEPORT_NUMBER = value;
			}
		}
		private string _rEPORT_NUMBER;

		[DataMember]
		public string REPORT_NAME
		{
			get { return _rEPORT_NAME; }
			set
			{
				_rEPORT_NAME = value;
			}
		}
		private string _rEPORT_NAME;

		[DataMember]
		public string REPORT_CATEGORY
		{
			get { return _rEPORT_CATEGORY; }
			set
			{
				_rEPORT_CATEGORY = value;
			}
		}
		private string _rEPORT_CATEGORY;

		[DataMember]
		public bool APPROVED
		{
			get { return _aPPROVED; }
			set
			{
				_aPPROVED = value;
			}
		}
		private bool _aPPROVED;

		[DataMember]
		public string REPORT_OWNER
		{
			get { return _rEPORT_OWNER; }
			set
			{
				_rEPORT_OWNER = value;
			}
		}
		private string _rEPORT_OWNER;

		[DataMember]
		public Nullable<System.DateTime> EFFECTIVE_DATE
		{
			get { return _eFFECTIVE_DATE; }
			set
			{
				_eFFECTIVE_DATE = value;
			}
		}
		private Nullable<System.DateTime> _eFFECTIVE_DATE;

		[DataMember]
		public Nullable<System.DateTime> APPROVAL_DATE
		{
			get { return _aPPROVAL_DATE; }
			set
			{
				_aPPROVAL_DATE = value;
			}
		}
		private Nullable<System.DateTime> _aPPROVAL_DATE;

		[DataMember]
		public string CREATED_BY
		{
			get { return _cREATED_BY; }
			set
			{
				_cREATED_BY = value;
			}
		}
		private string _cREATED_BY;

		[DataMember]
		public System.DateTime CREATED_DATE
		{
			get { return _cREATED_DATE; }
			set
			{
				_cREATED_DATE = value;
			}
		}
		private System.DateTime _cREATED_DATE;
	}
}
