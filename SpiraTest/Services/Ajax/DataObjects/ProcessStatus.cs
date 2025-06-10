using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
	/// <summary>
	/// Used to store the status of a long-running task
	/// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
	public class ProcessStatus
	{
		#region Enumerations

		/// <summary>The condition of the process</summary>
		public enum ProcessStatusCondition
		{
			Running = 1,
			Completed = 2,
			Error = 3,
			Warning = 4
		}

		#endregion

		/// <summary>Constructor</summary>
		/// <param name="name"></param>
		public ProcessStatus(string name)
		{
			this.condition = ProcessStatusCondition.Running;
			this.name = name;
			this.message = "";
			this.processId = Guid.NewGuid().ToString();
		}

		/// <summary>The id of the process</summary>
		[DataMember]
		private string processId;
		public string ProcessId
		{
			get
			{
				return this.processId;
			}
		}

		/// <summary>A number from 0-100 indicating the % complete</summary>
		[DataMember]
		public int Progress
		{
			get
			{
				return this.progress;
			}
			set
			{
				lock (this)
				{
					this.progress = value;
				}
			}
		}
		private int progress = 0;

		/// <summary>The name of the process</summary>
		[DataMember]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				lock (this)
				{
					this.name = value;
				}
			}
		}
		private string name = "";

		/// <summary>A return code used to return a process specific id value to the server control</summary>
		[DataMember]
		public int ReturnCode
		{
			get
			{
				return this.returnCode;
			}
			set
			{
				lock (this)
				{
					this.returnCode = value;
				}
			}
		}
		private int returnCode = 0;

        /// <summary>A return string to give extra meta information to the client about the process - eg, to flag a status or server side check status </summary>
        [DataMember]
        public string ReturnMeta
        {
            get
            {
                return this.returnMeta;
            }
            set
            {
                lock (this)
                {
                    this.returnMeta = value;
                }
            }
        }
        private string returnMeta = "";

        /// <summary>Any message indicating what the process is currently doing, or the final status if completed/errored</summary>
        [DataMember]
		public ProcessStatusCondition Condition
		{
			get
			{
				return this.condition;
			}
			set
			{
				lock (this)
				{
					this.condition = value;
				}
			}
		}
		private ProcessStatusCondition condition;

		/// <summary>Any message indicating what the process is currently doing, or the final status if completed/errored</summary>
		[DataMember]
		public string Message
		{
			get
			{
				return this.message;
			}
			set
			{
				lock (this)
				{
					this.message = value;
				}
			}
		}
		private string message = "";

		/// <summary>Increments the progress by 1%.</summary>
		public void IncrementProgress()
		{
			lock (this)
			{
				this.progress++;
			}
		}

		/// <summary>Implements the delegate that's passed to business classes that have long-running tasks</summary>
		/// <param name="progress">The progress (0-100)</param>
		/// <param name="message">The status message</param>
		public void Update(int progress, string message)
		{
			this.progress = progress;
			this.message = message;
		}

		/// <summary>Sets the status to Running. (Used for other libraries to access ConditionEnum.)</summary>
		public void setStatus_Running()
		{
			this.condition = ProcessStatusCondition.Running;
		}

		/// <summary>Sets the status to Completed. (Used for other libraries to access ConditionEnum.)</summary>
		public void setStatus_Completed()
		{
			this.condition = ProcessStatusCondition.Completed;
		}

		/// <summary>Sets the status to Error. (Used for other libraries to access ConditionEnum.)</summary>
		public void setStatus_Error()
		{
			this.condition = ProcessStatusCondition.Error;
		}

		/// <summary>Sets the status to Warning. (Used for other libraries to access ConditionEnum.)</summary>
		public void setStatus_Warning()
		{
			this.condition = ProcessStatusCondition.Warning;
		}

	}
}