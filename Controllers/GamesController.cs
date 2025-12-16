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

			if (!string.IsNullOrEmpty(search) && availableGames.Any(ag => ag.GameName.Contains(search, StringComparison.OrdinalIgnoreCase)))
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
			if (new GameMethods().GetGameByName(newGame.GameName, out string message1).GameName != null)
			{
				return View();
			}
			else
			{
				string initialState = "EEEEEEEEEEEEEEEEEEEEEEEEEEEBWEEEEEEWBEEEEEEEEEEEEEEEEEEEEEEEEEEE";
				newGame.Board = initialState;
				int result = new GameMethods().InsertGame(newGame, out string message2);

				if (result == 1)
				{
					HttpContext.Session.SetString("GameName", newGame.GameName);
					HttpContext.Session.SetInt32("CurrentPlayer", 1);
					return RedirectToAction("OthelloBoard");
				}
				else
				{
					return View();
				}
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
			if (user2Name != null)
			{
				ViewBag.User2Name = user2Name.Username;
			}

			int currentPlayer = new GameMethods().GetCurrentPlayer(initiatedGame, out string msg3);
			ViewBag.CurrentPlayer = currentPlayer;
			List<(int row, int col)> validMoves = new OthelloLogic().GetValidMoves(initiatedGame, currentPlayer);

			var cm = new ConverterMethods();
			string boardString = initiatedGame.Board;
			int[,] boardArray = cm.ConvertBoardStringToArray(boardString);

			int player1Points = 0;
			int player2Points = 0;

			for (int row = 0; row <= 7; row++)
			{
				for (int col = 0; col <= 7; col++)
				{
					if (boardArray[row, col] == 1)
					{
						player2Points++;
					}
					else if (boardArray[row, col] == 2)
					{
						player1Points++;
					}
				}
			}
			
			ViewBag.Player2Points = player2Points;
			ViewBag.Player1Points = player1Points;

			if (validMoves.IsNullOrEmpty())
			{
				if (player1Points > player2Points)
				{
					initiatedGame.WinnerID = initiatedGame.User1ID;
					initiatedGame.GameStatus = "Finished";
					int successWinnerID = new GameMethods().UpdateGameWinnerID(initiatedGame, out string message2);
					int successStatus = new GameMethods().UpdateGameStatus(initiatedGame.GameID, out string message3);
					Console.WriteLine("Winner is: " + user2Name.Username);
				}
				else if (player2Points > player1Points)
				{
					initiatedGame.WinnerID = initiatedGame.User2ID;
					initiatedGame.GameStatus = "Finished";
					int successWinnerID = new GameMethods().UpdateGameWinnerID(initiatedGame, out string messge4);
					int successStatus = new GameMethods().UpdateGameStatus(initiatedGame.GameID, out string message5);
					Console.WriteLine("Winner is: " + user1Name.Username);
				}

				return View(model: boardArray);
			}



			return View(model: boardArray);
		}

		public IActionResult JoinGame(GameDetails gd)
		{
			gd.GameName = new GameMethods().GetGameInfoByID(gd.GameID, out string message).GameName;
			gd.User2ID = (int)HttpContext.Session.GetInt32("UserID");
			HttpContext.Session.SetString("GameName", gd.GameName);

			GameMethods gm = new GameMethods();
			int currentStatus = gm.UpdateGameStatus(gd.GameID, out string message2);
			int updateUser2 = gm.UpdateUser2ID(gd, out string msg);

			if (currentStatus == 1 && updateUser2 == 1)
			{
				HttpContext.Session.SetInt32("CurrentPlayer", 2);
				return RedirectToAction("OthelloBoard");
			}
			else
			{
				return View();
			}

		}

		public IActionResult GameList(bool sorted)
		{
			List<GameDetails> availableGames = new GameMethods().GetAllGames(out string message);

			if (sorted)
			{
				availableGames = availableGames.OrderByDescending(ag => ag.GameStatus).ToList();
				ViewBag.Sorted = sorted;
			}

			return PartialView("GameList", availableGames);

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult LeaveGame()
		{
			var gameName = HttpContext.Session.GetString("GameName");
			if (!string.IsNullOrEmpty(gameName))
			{
				new GameMethods().DeleteGameByName(gameName, out string _);
			}

			HttpContext.Session.Remove("GameName");
			return RedirectToAction("Games");
		}

		[HttpPost]
		public IActionResult makeMove(int row, int col, int currentplayer)
		{
			GameMethods gm = new GameMethods();
			GameDetails gd = new GameDetails();
			OthelloLogic gameLogic = new OthelloLogic();

			string currentGame = HttpContext.Session.GetString("GameName") ?? "";
			gd = gm.GetGameByName(currentGame, out string message1);

			string currentBoard = gm.GetBoard(gd, out string message2);
			int[,] newBoard = new ConverterMethods().ConvertBoardStringToArray(currentBoard);

			bool success = gameLogic.BoardState(row, col, currentplayer, newBoard, gd);

			if (success == false)
			{
				return RedirectToAction("OthelloBoard");
			}
			else if (success == true)
			{
				if (currentplayer == 1)
				{
					gd.CurrentPlayer = 2;
					gm.UpdateCurrentPlayer(gd, out string message3);
				}
				else if (currentplayer == 2)
				{
					gd.CurrentPlayer = 1;
					gm.UpdateCurrentPlayer(gd, out string message4);
				}

			}

			return RedirectToAction("OthelloBoard");
		}
	}
}
