namespace ToHeBE.Models.Auth
{
	public class ResetPasswordRequest
	{
		public string Token { get; set; }
		public string Password { get; set; }
	}
}
