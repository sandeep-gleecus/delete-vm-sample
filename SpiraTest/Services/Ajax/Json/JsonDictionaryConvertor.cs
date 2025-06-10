using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Json
{
    /// <summary>
    /// Supports the use of the DictionaryOfStrings in AJAX server controls to
    /// send dictionaries of data to the client-side AJAX component (in the descriptor.AddProperty() command)
    /// </summary>    
    public static class JsonDictionaryConvertor
    {
        /// <summary>
        /// Serializes the dictionary as a JSON string
        /// </summary>
        /// <param name="dictionary">The dictionary</param>
        /// <returns>The JSON string</returns>
        public static string Serialize(JsonDictionaryOfStrings dictionary)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(JsonDictionaryOfStrings));
            MemoryStream ms = new MemoryStream();
            dcjs.WriteObject(ms, dictionary);
            byte[] bytes = ms.ToArray();
            string json = Encoding.UTF8.GetString(bytes);
            return json;
        }
    }
}