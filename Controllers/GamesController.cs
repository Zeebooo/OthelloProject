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
		public IActionResult Games(bool sorted, string search)
		{
			string message;
			List<GameDetails> availableGames = new GameMethods().GetAllGames(out message);

			if (availableGames == null || !availableGames.Any())
			{
				ViewBag.NoGames = "There are no games available";
				return View();
			}

			if (sorted)
			{
				availableGames = availableGames.OrderByDescending(ag => ag.GameStatus).ToList();
				ViewBag.Sorted = sorted;
			}

			if(!string.IsNullOrEmpty(search) && availableGames.Any(ag => ag.GameName.Contains(search, StringComparison.OrdinalIgnoreCase)))
			{
				availableGames = availableGames.Where(ag => ag.GameName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
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
			GameDetails initiatedGame = new GameMethods().GetGameByName(gameName, out string message);
			var userMethods = new UserMethods();

			var user1Name = userMethods.GetUserInfoByID(initiatedGame.User1ID, out string msg1);
			var user2Name = userMethods.GetUserInfoByID(initiatedGame.User2ID, out string msg2);
			ViewBag.User1Name = user1Name.Username;
			if (user2Name != null){
				ViewBag.User2Name = user2Name.Username;
			}

			string initialState = "EEEEEEEEEEEEEEEEEEEEEEEEEEEBWEEEEEEWBEEEEEEEEEEEEEEEEEEEEEEEEEEE";
			return View(model: initialState);
		}

		public IActionResult JoinGame(GameDetails gd)
		{
			gd.GameName = new GameMethods().GetGameInfoByID(gd.GameID, out string message).GameName;
			gd.User2ID = (int)(HttpContext.Session.GetInt32("UserID"));
			HttpContext.Session.SetString("GameName", gd.GameName);

			GameMethods gm = new GameMethods();
			int currentStatus = gm.UpdateGameStatus(gd.GameID, out string message2);
			int updateUser2 = gm.UpdateUser2ID(gd, out string msg);

			if(currentStatus == 1 && updateUser2 == 1)
			{
				return RedirectToAction("OthelloBoard");
			}
			else{
				return View();
			}

		}
	}

}
