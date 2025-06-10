using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml; 

namespace EFSetEdmxSqlVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            if (2 != args.Length)
            {
                Console.WriteLine("usage: SetEdmxSqlVersion <edmxFile> <sqlVer>");
                return;
            }
            string edmxFilename = args[0];
            string ver = args[1];
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(edmxFilename);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmlDoc.NameTable);
            mgr.AddNamespace("edmx", "http://schemas.microsoft.com/ado/2008/10/edmx");
            mgr.AddNamespace("ssdl", "http://schemas.microsoft.com/ado/2009/02/edm/ssdl");
            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:StorageModels/ssdl:Schema", mgr);
            if (node == null)
            {
                Console.WriteLine("Could not find Schema node");
            }
            else
            {
                //Only resave the file if it's changed, otherwise every build will effectively force a rebuild
                //which slows down the build process immensely
                if (node.Attributes["ProviderManifestToken"].Value != ver)
                {
                    Console.WriteLine("Setting EDMX version to {0} in file {1}", ver, edmxFilename);
                    node.Attributes["ProviderManifestToken"].Value = ver;
                    xmlDoc.Save(edmxFilename);
                }
            }
        } 
    }
}
