using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace VMScheduler.Process
{
    public class AuthContext
    {
        private AuthenticationContext authContext;
        private string tenantName;
        private string resourceUri;
        private string clientId;
        private string clientSecret;
        private string authority;
        private ClientCredential clientCredential;

        public string AccessToken { get; set; }
        public string ExpiresOn { get; set; }

        public AuthContext(string tenantName, string resourceUri, string clientId, string clientSecret, string authority)
        {
           
            try
            {
                clientCredential = new ClientCredential(clientId, clientSecret);

                AuthenticationContext authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
                AuthenticationResult result = authContext.AcquireToken(resourceUri, clientCredential);
                AccessToken = result.AccessTokenType + " " + result.AccessToken;
                ExpiresOn = result.ExpiresOn.ToUniversalTime().ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            //doIt();
            //var x = GetAccessTokenX();
            //authContext = new AuthenticationContext(Authority);
            //var task = GetAdTokenAsync();
            //var result = task.Wait(5);
            //token = task.Result;
            //token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSIsImtpZCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSJ9.eyJhdWQiOiJodHRwczovL3VsaW8tZGV2LmF6dXJld2Vic2l0ZXMubmV0IiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvZjFlNjVlMWQtYWUwOC00Y2ViLThkZTYtYjRlNTNlNDQ3NzA1LyIsImlhdCI6MTQ2ODUyNTMwMiwibmJmIjoxNDY4NTI1MzAyLCJleHAiOjE0Njg1MjkyMDIsImFjciI6IjEiLCJhbXIiOlsicHdkIl0sImFwcGlkIjoiNGM0MzEwMGQtYzA5NC00Mzc3LTg0ZTctNWQ1OGQ3MDk4ZDg5IiwiYXBwaWRhY3IiOiIxIiwiZW1haWwiOiI0NzMxMUBnbG9iYWwudWwuY29tIiwiZmFtaWx5X25hbWUiOiJaYWtob2RpbiIsImdpdmVuX25hbWUiOiJBbGV4IiwiaWRwIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNzAxMTU5NTQtMGNjZC00NWYwLTg3YmQtMDNiMmEzNTg3NTY5LyIsImlwYWRkciI6IjUwLjIwMS4yMDEuNzAiLCJuYW1lIjoiWmFraG9kaW4sIEFsZXgiLCJvaWQiOiIzNTU5YjA1ZS1kNWNiLTRmMzMtYjVjOC1kZjAxOTk2MTcwZDciLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJMOEN2d0lKN0RHTy1WVVFjSmd0UzdpR0tDcFlablcwSkpjY0RTUkxrX2RBIiwidGlkIjoiZjFlNjVlMWQtYWUwOC00Y2ViLThkZTYtYjRlNTNlNDQ3NzA1IiwidW5pcXVlX25hbWUiOiI0NzMxMUBnbG9iYWwudWwuY29tIiwidmVyIjoiMS4wIn0.pxueyxotmAWvl7ORCf3zsB_GzgGb7xa2oZJegYbMN5MeXN-CGTHkMX_PIE0i1kVrC5_pC3eIfBbfk0egi4lbfitMnHeO7iq0q4vJqnYir42sY-FT2tFN3cPnq4OQzkFECUjQfZie0MOcdjRLtXQn_G073llaQHivn5W62SNQRDpJ44oKswA9gpfyOVYBpdpoM92ZPimzYvUbnKH7CWQqXtHcBR55uCXSoKRpO_fzWIwTIj2YR4a9TCQ8rA94W-raxobRUvvIGlAHW4ZM7VBPOjFNZZHVVdKbuWp6wdX1-VMEMMZBRcoyS04Mvrb0tedtonVMifXRSRxFgYY8uGmglg";


        }
    }
}
