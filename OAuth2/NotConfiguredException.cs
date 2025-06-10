using System;

namespace Inflectra.OAuth2
{
	/// <summary>Thrown when a provider has not been initialized or configured properly.</summary>
	public class NotConfiguredException : Exception
	{
		public NotConfiguredException()
			: base("The OAuth2 Provider has not been configured.")
		{ }
	}
}
