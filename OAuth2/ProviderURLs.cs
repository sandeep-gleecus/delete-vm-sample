using System;

namespace Inflectra.OAuth2
{
    /// <summary>Holds necessary URLs for OAuth Providers.</summary>
    public class ProviderURLs
    {
        /// <summary>The URL to send the user to enter in their credentials.</summary>
        public Uri Authorization { get; set; }

        /// <summary>The URL to call to get the access token.</summary>
        public Uri Token { get; set; }

        /// <summary>The URL to call to pull the Profile information.</summary>
        public Uri Profile { get; set; }
    }
}
