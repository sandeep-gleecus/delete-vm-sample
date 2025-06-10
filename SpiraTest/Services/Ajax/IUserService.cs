using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side user management AJAX components
    /// </summary>
    [
    ServiceContract(Name = "UserService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IUserService : ISortedListService, INavigationService
    {
        [OperationContract]
        JsonDictionaryOfStrings User_RetrieveActiveForProject(int projectId);

        [OperationContract]
        string User_LinkUserToLdapDn(int userId, string ldapDn);
    }
}
