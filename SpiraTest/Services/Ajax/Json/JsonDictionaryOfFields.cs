using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Collections;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Json
{
    /// <summary>
    /// Serializes a Dictionary of DataItemFields into a form that can be converted to/from a JavaScript JSON object
    /// (i.e. var dictionary = {}; type of object)
    /// </summary>
    [
    Serializable,
    KnownType(typeof(DataItemField))
    ]
    public class JsonDictionaryOfFields : ISerializable, IEnumerable
    {
        //Stores the internal representation
        private Dictionary<string, DataItemField> dict;

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDictionaryOfFields()
        {
            dict = new Dictionary<string, DataItemField>();
        }

        #region ISerializable Members

        /// <summary>
        /// Deserializes the object
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected JsonDictionaryOfFields(SerializationInfo info, StreamingContext context)
        {
            dict = new Dictionary<string, DataItemField>();
            foreach (var entry in info)
            {
                DataItemField value = entry.Value as DataItemField;
                dict.Add(entry.Name, value);
            }
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (string key in dict.Keys)
            {
                info.AddValue(key, dict[key]);
            }
        }

        #endregion

        #region Public Accessors for Common Dictionary Methods/Properties

        /// <summary>
        /// Adds an item to the dictionary
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void Add(string key, DataItemField value)
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// Returns the count of items in the dictionary
        /// </summary>
        public int Count
        {
            get
            {
                return dict.Count;
            }
        }

        /// <summary>
        /// Determines if the key is present in the dictionary
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>True if present</returns>
        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value associated with a specific key
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>The value that has this key</returns>
        public DataItemField this[string key]
        {
            get
            {
                return dict[key];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an instance of the Dictionary enumerator
        /// </summary>
        /// <returns>Returns an instance of the Dictionary enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion
    }
}