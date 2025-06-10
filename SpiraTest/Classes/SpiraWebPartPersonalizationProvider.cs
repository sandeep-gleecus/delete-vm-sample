using System;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Collections;
using System.IO;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Allows the webparts system to use the built-in SpiraTest user/role tables
    /// </summary>
    public class SpiraWebPartPersonalizationProvider : PersonalizationProvider
    {
        protected const string CLASS_NAME = "SpiraWebPartPersonalizationProvider";

        /// <summary>
        /// The name of the application using the personalization provider
        /// </summary>
        /// <remarks>Not supported as this is specific to SpiraTest</remarks>
        public override string ApplicationName
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="name">The name of the provider</param>
        /// <param name="config">The list of configuration parameters</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Verify that config isn't null
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // Assign the provider a default name if it doesn't have one
            if (String.IsNullOrEmpty(name))
            {
                name = "SpiraWebPartPersonalizationProvider";
            }

            // Add a default "description" attribute to config if the
            // attribute doesn't exist or is empty
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Web parts personalization provider that is integrated with the application's native data schema");
            }

            // Call the base class's Initialize method
            base.Initialize(name, config);

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                {
                    throw new ProviderException("Unrecognized attribute: " + attr);
                }
            }
        }

        /// <summary>
        /// Resets the WebPart personalization data for either the global or user settings for a particular path
        /// </summary>
        /// <param name="webPartManager">The webpart manager</param>
        /// <param name="path">The asp.net page path (e.g. ~/ProjectList.aspx)</param>
        /// <param name="userName">The user name</param>
        protected override void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName)
        {
            const string METHOD_NAME = "ResetPersonalizationBlob";

            try
            {
                //Instantiate the business class used to get the data
                DashboardManager dashboardManager = new DashboardManager();

                try
                {
                    //If we have the special extended web part manager, don't use the default path
                    if (webPartManager is WebPartManagerEx)
                    {
                        WebPartManagerEx webPartManagerEx = (WebPartManagerEx)webPartManager;
                        path = webPartManagerEx.GetPath();
                    }

                    //Reset either the user or global settings depending on the provided user name
                    if (String.IsNullOrEmpty(userName))
                    {
                        dashboardManager.DeleteGlobalSettings(path);
                    }
                    else
                    {
                        dashboardManager.DeleteUserSettings(path, userName);
                    }
                }
                finally
                {
                    dashboardManager = null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Serializes the personalization data into binary blobs for persisting
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] SerializeData(IDictionary data)
        {
            byte[] buffer = null;
            if ((data == null) || (data.Count == 0))
            {
                return buffer;
            }
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in data)
            {
                PersonalizationInfo info = (PersonalizationInfo)entry.Value;
                if (((info._properties != null) && (info._properties.Count != 0)) || ((info._customProperties != null) && (info._customProperties.Count != 0)))
                {
                    list.Add(info);
                }
            }
            if (list.Count == 0)
            {
                return buffer;
            }
            ArrayList list2 = new ArrayList();
            list2.Add(2);
            list2.Add(list.Count);
            foreach (PersonalizationInfo info2 in list)
            {
                if (info2._isStatic)
                {
                    list2.Add(info2._controlType);
                    if (info2._controlVPath != null)
                    {
                        list2.Add(info2._controlVPath);
                    }
                }
                list2.Add(info2._controlID);
                int count = 0;
                if (info2._properties != null)
                {
                    count = info2._properties.Count;
                }
                list2.Add(count);
                if (count != 0)
                {
                    foreach (DictionaryEntry entry2 in info2._properties)
                    {
                        list2.Add(new IndexedString((string)entry2.Key));
                        list2.Add(entry2.Value);
                    }
                }
                int num2 = 0;
                if (info2._customProperties != null)
                {
                    num2 = info2._customProperties.Count;
                }
                list2.Add(num2);
                if (num2 != 0)
                {
                    foreach (DictionaryEntry entry3 in info2._customProperties)
                    {
                        list2.Add(new IndexedString((string)entry3.Key));
                        PersonalizationEntry entry4 = (PersonalizationEntry)entry3.Value;
                        list2.Add(entry4.Value);
                        list2.Add(entry4.Scope == PersonalizationScope.Shared);
                        list2.Add(entry4.IsSensitive);
                    }
                    continue;
                }
            }
            if (list2.Count == 0)
            {
                return buffer;
            }
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            MemoryStream outputStream = new MemoryStream(0x400);
            object[] stateGraph = list2.ToArray();
            formatter.Serialize(outputStream, stateGraph);
            return outputStream.ToArray();
        }

        /// <summary>
        /// Deserializes the personalization data used in the provided so that other classes can access it
        /// </summary>
        /// <param name="data">The binary data</param>
        /// <returns></returns>
        protected static IDictionary DeserializeData(byte[] data)
        {
            IDictionary dictionary = null;
            if ((data != null) && (data.Length > 0))
            {
                Exception innerException = null;
                int num = -1;
                object[] objArray = null;
                int num2 = 0;
                try
                {
                    ObjectStateFormatter formatter = new ObjectStateFormatter();
                    objArray = (object[])formatter.Deserialize(new MemoryStream(data));
                    if ((objArray != null) && (objArray.Length != 0))
                    {
                        num = (int)objArray[num2++];
                    }
                }
                catch (Exception exception2)
                {
                    innerException = exception2;
                }
                switch (num)
                {
                    case 1:
                    case 2:
                        try
                        {
                            int initialSize = (int)objArray[num2++];
                            if (initialSize > 0)
                            {
                                dictionary = new HybridDictionary(initialSize, false);
                            }
                            for (int i = 0; i < initialSize; i++)
                            {
                                string str;
                                bool flag;
                                Type type = null;
                                string path = null;
                                object obj2 = objArray[num2++];
                                if (obj2 is string)
                                {
                                    str = (string)obj2;
                                    flag = false;
                                }
                                else
                                {
                                    type = (Type)obj2;
                                    if (type == typeof(UserControl))
                                    {
                                        path = (string)objArray[num2++];
                                    }
                                    str = (string)objArray[num2++];
                                    flag = true;
                                }
                                IDictionary dictionary2 = null;
                                int num5 = (int)objArray[num2++];
                                if (num5 > 0)
                                {
                                    dictionary2 = new HybridDictionary(num5, false);
                                    for (int j = 0; j < num5; j++)
                                    {
                                        string str2 = ((IndexedString)objArray[num2++]).Value;
                                        object obj3 = objArray[num2++];
                                        dictionary2[str2] = obj3;
                                    }
                                }
                                PersonalizationDictionary dictionary3 = null;
                                int num7 = (int)objArray[num2++];
                                if (num7 > 0)
                                {
                                    dictionary3 = new PersonalizationDictionary(num7);
                                    for (int k = 0; k < num7; k++)
                                    {
                                        string str3 = ((IndexedString)objArray[num2++]).Value;
                                        object obj4 = objArray[num2++];
                                        PersonalizationScope scope = ((bool)objArray[num2++]) ? PersonalizationScope.Shared : PersonalizationScope.User;
                                        bool isSensitive = false;
                                        if (num == 2)
                                        {
                                            isSensitive = (bool)objArray[num2++];
                                        }
                                        dictionary3[str3] = new PersonalizationEntry(obj4, scope, isSensitive);
                                    }
                                }
                                PersonalizationInfo info = new PersonalizationInfo();
                                info._controlID = str;
                                info._controlType = type;
                                info._controlVPath = path;
                                info._isStatic = flag;
                                info._properties = dictionary2;
                                info._customProperties = dictionary3;
                                dictionary[str] = info;
                            }
                        }
                        catch (Exception exception3)
                        {
                            innerException = exception3;
                        }
                        break;
                }
                if ((innerException != null) || ((num != 1) && (num != 2)))
                {
                    throw new ArgumentException("BlobPersonalizationState_DeserializeError", innerException);
                }
            }
            if (dictionary == null)
            {
                dictionary = new HybridDictionary(false);
            }
            return dictionary;
        }

        /// <summary>
        /// Returns the user personalization info for a specific page
        /// </summary>
        /// <param name="webPartManager"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <remarks>Useful for accessing webparts personalization info from other pages</remarks>
        public IDictionary GetUserPersonalizationInfo(WebPartManager webPartManager, string path, string userName)
        {
            byte[] userBlob = null;
            byte[] sharedBlob = null;

            this.LoadPersonalizationBlobs(webPartManager, path, userName, ref sharedBlob, ref userBlob);
            return DeserializeData(userBlob);
        }

        /// <summary>
        /// Sets the user personalization info for a specific page
        /// </summary>
        /// <param name="webPartManager"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <param name="userPersonalizationInfo"></param>
        /// <returns></returns>
        /// <remarks>Useful for accessing webparts personalization info from other pages</remarks>
        public void SetUserPersonalizationInfo(WebPartManager webPartManager, string path, string userName, IDictionary userPersonalizationInfo)
        {
            byte[] userBlob = SerializeData(userPersonalizationInfo);
            this.SavePersonalizationBlob(webPartManager, path, userName, userBlob);
        }

        /// <summary>
        /// Loads the WebPart personalization data from either the global or user settings
        /// </summary>
        /// <param name="webPartManager">The webpart manager</param>
        /// <param name="path">The asp.net page path (e.g. ~/ProjectList.aspx)</param>
        /// <param name="userName">The user name</param>
        /// <param name="sharedDataBlob">The global configuration</param>
        /// <param name="userDataBlob">The user-specific configuration</param>
        protected override void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob)
        {
            const string METHOD_NAME = "LoadPersonalizationBlobs";

            sharedDataBlob = null;
            userDataBlob = null;
            try
            {
                //Instantiate the business class used to get the data
                DashboardManager dashboardManager = new DashboardManager();

                try
                {
                    //If we have the special extended web part manager, don't use the default path
                    if (webPartManager is WebPartManagerEx)
                    {
                        WebPartManagerEx webPartManagerEx = (WebPartManagerEx)webPartManager;
                        path = webPartManagerEx.GetPath();
                    }
                    
                    //First load the global blob
                    sharedDataBlob = dashboardManager.RetrieveGlobalSettings(path);
                    if (!string.IsNullOrEmpty(userName))
                    {
                        userDataBlob = dashboardManager.RetrieveUserSettings(path, userName);
                    }
                }
                finally
                {
                    dashboardManager = null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

        }

        /// <summary>
        /// Saves the WebPart personalization data to either the global or user settings
        /// </summary>
        /// <param name="webPartManager">The webpart manager</param>
        /// <param name="path">The asp.net page path (e.g. ~/ProjectList.aspx)</param>
        /// <param name="userName">The user name</param>
        /// <param name="dataBlob">The configuration data</param>
        protected override void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob)
        {
            const string METHOD_NAME = "SavePersonalizationBlob";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the business class used to save the data
                DashboardManager dashboardManager = new DashboardManager();

                try
                {
                    //If we have the special extended web part manager, don't use the default path
                    if (webPartManager is WebPartManagerEx)
                    {
                        WebPartManagerEx webPartManagerEx = (WebPartManagerEx)webPartManager;
                        path = webPartManagerEx.GetPath();
                    }

                    if (String.IsNullOrEmpty(userName))
                    {
                        dashboardManager.SaveGlobalSettings(path, dataBlob);
                    }
                    else
                    {
                        dashboardManager.SaveUserSettings(path, userName, dataBlob);
                    }
                }
                finally
                {
                    dashboardManager = null;
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /* Optional Methods that are not currently implemented */

        public override PersonalizationStateInfoCollection FindState (PersonalizationScope scope, PersonalizationStateQuery query, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query)
        {
            throw new NotSupportedException();
        }

        public override int ResetState(PersonalizationScope scope, string[] paths, string[] usernames)
        {
            throw new NotSupportedException();
        }

        public override int ResetUserState(string path, DateTime userInactiveSinceDate)
        {
            throw new NotSupportedException();
        }
    }

    public class PersonalizationInfo
    {
        // Fields
        public string _controlID;
        public Type _controlType;
        public string _controlVPath;
        public PersonalizationDictionary _customProperties;
        public bool _isStatic;
        public IDictionary _properties;
    }
}
