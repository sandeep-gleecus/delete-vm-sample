namespace Inflectra.SpiraTest.DataModel
{
	public partial class GlobalOAuthProvider
	{
		/// <summary>
		/// Returns/sets whether the provider's Logo is editable. (Unused.)
		/// </summary>
		/// <remarks>Stores value in Custom4.</remarks>
		public bool IsLogoEditable
		{
			get
			{
				return !string.IsNullOrWhiteSpace(Custom4) && Custom4.Trim().Equals("1");
			}
			set
			{
				Custom4 = ((value) ? "1" : "0");
			}
		}

		/// <summary>
		/// Returns/sets whether the provider's URls are editable.
		/// </summary>
		/// <remarks>Stores value in Custom5.</remarks>
		public bool IsUrlsEditable
		{
			get
			{
				return !string.IsNullOrWhiteSpace(Custom5) && Custom5.Trim().Equals("1");
			}
			set
			{
				Custom5 = ((value) ? "1" : "0");
			}
		}
	}
}