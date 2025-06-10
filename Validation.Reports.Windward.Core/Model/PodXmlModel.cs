using System.IO;
using System.Xml.Serialization;

namespace Validation.Reports.Windward.Core.Model
{
	public class PodXmlModel
	{
		public PodXml GetPodModelFromXml(Stream textReader)
		{

			XmlSerializer deserializer = new XmlSerializer(typeof(PodXml));
			PodXml pods;
			pods = (PodXml)deserializer.Deserialize(textReader);
			textReader.Close();

			return pods;
		}

	}
}
