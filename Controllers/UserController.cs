using Microsoft.AspNetCore.Mvc;
using OthelloProject.Models.Methods;
using OthelloProject.Models;
using Microsoft.AspNetCore.Identity;


namespace OthelloProject.Controllers
{
	public class UserController : Controller
	{
		[HttpGet]
		public IActionResult RegisterUser()
		{
			return View("Register");
		}

		[HttpPost]
		public IActionResult RegisterUser(UserDetails userDetail)
		{

			UserMethods userMethods = new UserMethods();

			int i = userMethods.InsertUser(userDetail, out string message);

			switch (i)
			{
				case -2:
					ViewBag.Message = "Email is already in use!";
					break;
				case -1:
					ViewBag.Message = "User already exists.";
					break;
				case 1:
					ViewBag.Message = "Registrition succesful .";
					break;
				default:
					ViewBag.Message = "Unexpected error.";
					break;
			}

			return View("Register");
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View("LoginPage");
		}

		[HttpPost]
		public IActionResult Login(UserDetails ud)
		{
			var uMethod = new UserMethods();
			UserDetails retrievedUser = uMethod.VerifyLogin(ud.Username!, out string errormsg);

			if (retrievedUser != null)
			{
				var passwordHasher = new PasswordHasher<UserDetails>();
				var verificationResult = passwordHasher.VerifyHashedPassword(retrievedUser, retrievedUser.Password, ud.Password);

				if (verificationResult == PasswordVerificationResult.Success)
				{
					// Inloggning lyckades
					HttpContext.Session.SetInt32("UserID", retrievedUser.UserID);
					Console.WriteLine("UserID: " + retrievedUser.UserID);
					Console.WriteLine("Username: " + retrievedUser.Username);
					return RedirectToAction("Games", "Games");
				}
				else
				{
					// Fel lösenord
					ViewBag.ErrorMessage = "Felaktigt användarnamn eller lösenord.";
					return View("LoginPage");
				}
			}
			else
			{
				// Användaren hittades inte
				ViewBag.ErrorMessage = "Användaren hittades inte.";
				return View("LoginPage");
			}
		}
	}
}
