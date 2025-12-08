using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Identity.Client;

namespace OthelloProject.Models.Methods
{
	public class UserDetails
	{
		public UserDetails() { }
		public int UserID { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
	}
}