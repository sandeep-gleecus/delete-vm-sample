using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Collections;
using System.Web.Script.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Json
{
    /// <summary>
    /// Serializes a Dictionary of decimals into a form that can be converted to/from a JavaScript JSON object
    /// (i.e. var dictionary = {}; type of object).
    /// </summary>
    [
    Serializable
    ]
    public class JsonDictionaryOfDecimals : ISerializable
    {
        /// <summary>
        /// The prefix added to json dictionary keys so that numeric items can be safely serialized
        /// </summary>
        public const string KEY_PREFIX = "k";
        
        //Stores the internal representation
        private Dictionary<string, decimal> dict;

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDictionaryOfDecimals()
        {
            dict = new Dictionary<string, decimal>();
        }

        /// <summary>
        /// Constructor that creates an instance based on the standard generic dictionary
        /// </summary>
        public JsonDictionaryOfDecimals(Dictionary<string, decimal> dict)
        {
            this.dict = dict;
        }

        #region ISerializable Members

        /// <summary>
        /// Deserializes the object
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected JsonDictionaryOfDecimals(SerializationInfo info, StreamingContext context)
        {
            dict = new Dictionary<string, decimal>();
            foreach (var entry in info)
            {
                //The first letter of the name needs to be removed if it's a 'k'
                string key = entry.Name;
                if (key != null && key.StartsWith(KEY_PREFIX))
                {
                    key = key.Substring(KEY_PREFIX.Length);
                }
                decimal value = (decimal)entry.Value;
                dict.Add(key, value);
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
                //We prefix all the keys with the letter key (in case they start with numbers)
                info.AddValue(KEY_PREFIX + key, dict[key]);
            }
        }

        #endregion

        #region Public Accessors for Common Dictionary Methods/Properties

        /// <summary>
        /// Adds an item to the dictionary
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void Add(string key, decimal value)
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// Removes an item from the dictionary
        /// </summary>
        /// <param name="key">The key of the item to remove</param>
        public void Remove(string key)
        {
            dict.Remove(key);
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
        public decimal this[string key]
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