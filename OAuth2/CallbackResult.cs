namespace Inflectra.OAuth2
{
	using System;
	using System.Collections.Generic;
	using System.Security.Claims;

	public class CallbackResult
	{
		public string Error { get; set; }
		public string ErrorDetails { get; set; }
		public string ReturnUrl { get; set; }
		public Guid ProviderId{ get; set; }
		public Dictionary<int, object> Claims { get; set; }
	}
}
