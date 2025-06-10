using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>Add custom extensions to the FileType entity</summary>
	public partial class Filetype : Entity
	{
		public Filetype()
		{
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Use the AddValue method to specify serialized values.
			info.AddValue("FiletypeId", this.FiletypeId, typeof(int));
		}
		}
}
