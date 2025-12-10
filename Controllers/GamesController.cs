using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OthelloProject.Models;
using OthelloProject.Models.Methods;

namespace OthelloProject
{

	public class GamesController : Controller
	{
		[HttpGet]
		public IActionResult Games()
		{
			string message;
			List<GameDetails> availableGames = new GameMethods().GetAllGames(out message);

			if (availableGames == null || !availableGames.Any())
			{
				ViewBag.NoGames = "There are no games available";
				return View();
			}


			return View(availableGames);
		}

		[HttpGet]
		public IActionResult AddGame()
		{
			int? selectedUser = HttpContext.Session.GetInt32("UserID");
			ViewBag.User1ID = selectedUser;
			return View();
		}

		[HttpPost]
		public IActionResult AddGame(GameDetails newGame)
		{
			string message;
			int result = new GameMethods().InsertGame(newGame, out message);

			if (result == 1)
			{
				HttpContext.Session.SetString("GameName", newGame.GameName);
				return RedirectToAction("OthelloBoard");
			}
			else
			{
				return View();
			}
		}

		public IActionResult OthelloBoard()
		{
			string gameName = HttpContext.Session.GetString("GameName") ?? "";
			Console.WriteLine("Help: " + gameName);
			GameDetails initiatedGame = new GameMethods().GetGameByName(gameName, out string message);
			var userMethods = new UserMethods();
			Console.WriteLine(gameName);

			var user1Name = userMethods.GetUserInfoByID(initiatedGame.User1ID, out string msg1);
			ViewBag.User1Name = user1Name.Username;

			string initialState = "EEEEEEEEEEEEEEEEEEEEEEEEEEEBWEEEEEEWBEEEEEEEEEEEEEEEEEEEEEEEEEEE";
			return View(model: initialState);
		}
	}

}
