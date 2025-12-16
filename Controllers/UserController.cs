using Microsoft.AspNetCore.Mvc;
using OthelloProject.Models.Methods;
using OthelloProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OthelloProject.ViewModels;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;


namespace OthelloProject.Controllers
{
	public class UserController : Controller
	{
		[AllowAnonymous]
		[HttpGet]
		public IActionResult RegisterUser()
		{
			return View("Register");
		}

		[AllowAnonymous]
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

		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login()
		{
			return View("LoginPage");
		}

		[AllowAnonymous]
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

		[AllowAnonymous]
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View(new ForgotPasswordViewModel());
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ForgotPassword(ForgotPasswordViewModel vm)
		{
			if (!ModelState.IsValid)
			{
				return View(vm);
			}

			var um = new UserMethods();
			var user = um.GetUserByEmail(vm.Email, out string msg);

			// Generera temporärt lösen även om vi inte hittar användare, men uppdatera bara om user != null
			string tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10);

			if (user != null)
			{
				int rowsPwd = um.UpdatePasswordById(user.UserID, tempPassword, out string msgPwd);
				if (rowsPwd != 1)
				{
					ModelState.AddModelError("", "Kunde inte uppdatera lösenord: " + msgPwd);
					return View(vm);
				}

				// Skicka mail
				if (!SendTempPassword(vm.Email, user.Username, tempPassword, out string mailError))
				{
					ModelState.AddModelError("", "Kunde inte skicka e-post: " + mailError);
					return View(vm);
				}
			}

			// Alltid samma svar för att inte läcka om e-post finns
			TempData["ProfileMessage"] = "Om e-postadressen finns har ett temporärt lösenord skickats.";
			return RedirectToAction("Login");
		}

		private bool SendTempPassword(string toEmail, string username, string tempPassword, out string error)
		{
			error = "";
			try
			{
				var config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.Build();

				var host = config["Smtp:Host"];
				var portStr = config["Smtp:Port"];
				var user = config["Smtp:User"];
				var pass = config["Smtp:Pass"];
				var from = config["Smtp:From"];

				if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr) || string.IsNullOrWhiteSpace(from))
				{
					error = "SMTP-konfiguration saknas.";
					return false;
				}

				int port = int.TryParse(portStr, out var p) ? p : 25;

				using var client = new SmtpClient(host, port)
				{
					EnableSsl = true,
					Credentials = (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
						? new NetworkCredential(user, pass)
						: CredentialCache.DefaultNetworkCredentials
				};

				using var mail = new MailMessage(from, toEmail);
				mail.Subject = "Temporärt lösenord";
				mail.Body = $"Hej {username},\n\nDitt temporära lösenord är: {tempPassword}\n\nLogga in och byt lösenord snarast.";
				client.Send(mail);
				return true;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return false;
			}
		}

		[HttpGet]
		public IActionResult UserPage()
		{
			int? userId = HttpContext.Session.GetInt32("UserID");
			if (userId == null)
			{
				return RedirectToAction("Login");
			}

			var um = new UserMethods();
			var gm = new GameMethods();
			var user = um.GetUserInfoByID(userId, out string userMsg);
			if (user == null)
			{
				ViewBag.Error = userMsg;
				return View("UserPage");
			}

			int totalGames, gamesWon;
				int result = um.GetWinningStats(userId.Value, out totalGames, out gamesWon, out string statsMsg);
			int gamesLost = totalGames - gamesWon;

			if (result != 1)
			{
				ViewBag.Error = statsMsg;
			}
 
			ViewBag.Username = user.Username;
			ViewBag.TotalGames = totalGames;
			ViewBag.GamesWon = gamesWon;
			ViewBag.GamesLost = gamesLost;
			ViewBag.RecentGames = gm.GetRecentGamesForUser(userId.Value, 5, out string recentMsg);
			if (!string.IsNullOrEmpty(recentMsg))
			{
				ViewBag.Error = recentMsg;
			}

			return View("UserPage");
		}

		[HttpGet]
		public IActionResult Profile()
		{
			int? userId = HttpContext.Session.GetInt32("UserID");
			if (userId == null) return RedirectToAction("Login");

			var um = new UserMethods();
			var user = um.GetUserInfoByID(userId, out string msg);
			if (user == null) { ViewBag.Error = msg; return View(new ProfileViewModel()); }

			var vm = new ProfileViewModel { Username = user.Username, Email = user.Email };
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Profile(ProfileViewModel vm)
		{
			int? userId = HttpContext.Session.GetInt32("UserID");
			if (userId == null) return RedirectToAction("Login");

			if (!ModelState.IsValid) return View(vm);

			var um = new UserMethods();
			// Uppdatera namn + email
			int rowsProfile = um.UpdateUserProfile(userId.Value, vm.Username, vm.Email, out string msgProfile);

			// Uppdatera lösen vid behov
			if (!string.IsNullOrWhiteSpace(vm.NewPassword))
			{
				if (vm.NewPassword != vm.ConfirmPassword)
				{
					ModelState.AddModelError("", "Lösenorden matchar inte.");
					return View(vm);
				}
				int rowsPwd = um.UpdatePasswordById(userId.Value, vm.NewPassword, out string msgPwd);
				if (rowsPwd != 1)
				{
					ModelState.AddModelError("", msgPwd);
					return View(vm);
				}
			}

			if (rowsProfile != 1)
			{
				ModelState.AddModelError("", msgProfile);
				return View(vm);
			}

			TempData["ProfileMessage"] = "Profil uppdaterad.";
			return RedirectToAction("Profile");
		}
	}
}
