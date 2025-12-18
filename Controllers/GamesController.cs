using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OthelloProject.Models;
using OthelloProject.Models.Methods;

namespace OthelloProject
{

	public class GamesController : Controller
	{

		/*
			Första sidan som visar alla tillgängliga spel i en lista.
		*/
		[HttpGet]
		public IActionResult Games(bool sorted, string search)
		{
			string message;
			//Hämta listan av alla  tillgängliga spel 
			List<GameDetails> availableGames = new GameMethods().GetAllGames(out message);

			//Om det inte finns några tillgängliga spel
			if (availableGames == null || !availableGames.Any())
			{
				ViewBag.NoGames = "There are no games available";
				return View();
			}

			//Om användaren sorterar efter tillgänglighet
			if (sorted)
			{
				availableGames = availableGames.OrderByDescending(ag => ag.GameStatus).ToList();
				ViewBag.Sorted = sorted;
			}

			//Om användaren har skrivit något i sökfältet
			if (!string.IsNullOrEmpty(search) && availableGames.Any(ag => ag.GameName.Contains(search, StringComparison.OrdinalIgnoreCase)))
			{
				//Sortera spel efter vad användaren har skrivit
				availableGames = availableGames.Where(ag => ag.GameName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			return View(availableGames);
		}

		/*
			Hjälp funktion som tar bort ett spel som är klart och tar användaren tillbaka till första sidan.
		*/ 
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult FinishedGame()
		{
			HttpContext.Session.Remove("GameName");
			HttpContext.Session.Remove("Winner");
			HttpContext.Session.Remove("Player1Points");
			HttpContext.Session.Remove("Player2Points");
			HttpContext.Session.Remove("CurrentPlayer");
			HttpContext.Session.Remove("PlayerNumber");
			return RedirectToAction("Games", "Games");
		}

		/*
			Tar användaren till en vy där den kan lägga till ett spel
		*/
		[HttpGet]
		public IActionResult AddGame()
		{
			//Användaren som vill skapa ett spel 
			int? selectedUser = HttpContext.Session.GetInt32("UserID");
			ViewBag.User1ID = selectedUser;

			return View();
		}

		/*
			Skapar ett spel och skickar det till databasen
		*/
		[HttpPost]
		public IActionResult AddGame(GameDetails newGame)
		{
			// Återställer spelsession som ligger kvar
			HttpContext.Session.Remove("Winner");
			HttpContext.Session.Remove("Player1Points");
			HttpContext.Session.Remove("Player2Points");
			HttpContext.Session.Remove("CurrentPlayer");
			HttpContext.Session.Remove("PlayerNumber");

			//Hämtar det tillagda spelet från databasen genom GetGameByName
			GameDetails gd = new GameMethods().GetGameByName(newGame.GameName, out string message1);

			//Om spelet inte finns *gör inget*
			if (gd.GameName != null)
			{
				return View();
			}
			else //Annars tillsätt start-tillståndet till spelet och sätt in den i databasen 
			{
				string initialState = "EEEEEEEEEEEEEEEEEEEEEEEEEEEBWEEEEEEWBEEEEEEEEEEEEEEEEEEEEEEEEEEE";
				newGame.Board = initialState;
				int result = new GameMethods().InsertGame(newGame, out string message2);

				//Om spelet infogades korrekt sätts spelnamnet till namnet givet användaren. 
				if (result == 1)
				{
					HttpContext.Session.SetString("GameName", newGame.GameName);
					//PlayerNumber är vilken pjäs spelaren får. 1 är svart och 2 är vit

					HttpContext.Session.SetInt32("PlayerNumber", 1);
					return RedirectToAction("OthelloBoard");
				}
				else
				{
					return View();
				}
			}

		}

		/*
			Returnera vyn för spelbrädet 
		*/
		public IActionResult OthelloBoard()
		{
			//Hämta spelet via en sessionsvariabel
			string gameName = HttpContext.Session.GetString("GameName") ?? "";
			GameDetails initiatedGame = new GameMethods().GetGameByName(gameName, out string message);

			//Hämta speletbrärdet som är en sträng och gör den till en int array
			string boardString = initiatedGame.Board;
			int[,] boardArray = new ConverterMethods().ConvertBoardStringToArray(boardString);

			return View(model: boardArray);
		}

		/*
			Returnerar en partiellvy som visar spelbrädet 
		*/
		public IActionResult OthelloGameBoard()
		{
			//Hämta spelet via en sessionsvariabel
			string gameName = HttpContext.Session.GetString("GameName") ?? "";
			GameDetails initiatedGame = new GameMethods().GetGameByName(gameName, out string message);

			//Hämtar namnen för spelare 1 och spelare 2
			var user1Name = new UserMethods().GetUserInfoByID(initiatedGame.User1ID, out string msg1);
			var user2Name = new UserMethods().GetUserInfoByID(initiatedGame.User2ID, out string msg2);

			//Om användarnamnet inte är tomt sätt den i en sessionsvariabel
			if (user1Name != null)
			{
				HttpContext.Session.SetString("User1Name", user1Name.Username);
			}

			//Om användarnamnet inte är tomt sätt den i en sessionsvariabel
			if (user2Name != null)
			{
				HttpContext.Session.SetString("User2Name", user2Name.Username);
			}
			
			//Hämtar den "nuvarande" spelaren alltså vems tur det är och sätter det i en sessionsvariabel 
			int currentPlayer = new GameMethods().GetCurrentPlayer(initiatedGame, out string msg3);
			HttpContext.Session.SetInt32("CurrentPlayer", currentPlayer);
			//Om nuvarande spelare får 1 får andra spelare 2
			int otherPlayer = currentPlayer == 1 ? 2 : 1;

			//Hämta en spelbrädet och gör den till en int array
			string boardString = initiatedGame.Board;
			int[,] boardArray = new ConverterMethods().ConvertBoardStringToArray(boardString);

			int player1Points = 0;
			int player2Points = 0;

			//Går igenom hela spelbrädet och räknar poäng
			for (int row = 0; row <= 7; row++)
			{
				for (int col = 0; col <= 7; col++)
				{
					//Om spelbrädet innehåller 1(svart) ge spelare 1 en poäng
					if (boardArray[row, col] == 1)
					{
						player1Points++;
					}
					else if (boardArray[row, col] == 2) //Annars om den är 2(vit) ge spelare 2 en poäng
					{
						player2Points++;
					}
				}
			}

			//Sätt poängen sessionnsvariabler för att kunna uppdatera dessa genom ajax
			HttpContext.Session.SetInt32("Player2Points", player2Points);
			HttpContext.Session.SetInt32("Player1Points", player1Points);

			//Hämta antalet giltiga drag för båda spelare för att sedan räkna ut när spelet är klart. 
			List<(int row, int col)> currentPlayerMoves = new OthelloLogic().GetValidMoves(initiatedGame, currentPlayer);
			List<(int row, int col)> otherPlayerMoves = new OthelloLogic().GetValidMoves(initiatedGame, otherPlayer);

			//Om spelet har en vinnare sätt "Winner" till vinnarens namn 
			if (initiatedGame.WinnerID != null)
			{
				//Sätt namnet på vinnaren till det spelare ett om id matchar annars spelare 2
				var winnerName = initiatedGame.WinnerID == initiatedGame.User1ID ? user1Name?.Username : user2Name?.Username;
				
				//Om winnerName inte är tomt sätt namnet i en sessionsvariabel 
				if (!string.IsNullOrEmpty(winnerName))
				{
					HttpContext.Session.SetString("Winner", winnerName);
				}
				return PartialView("OthelloGameBoard", boardArray);
			}

			//Om nuvarande spelare inte har några spel kvar och andra spelaren har det sätt andra spelare till nuvarande spelare
			if (currentPlayerMoves.IsNullOrEmpty() && !otherPlayerMoves.IsNullOrEmpty())
			{
				initiatedGame.CurrentPlayer = otherPlayer;
				new GameMethods().UpdateCurrentPlayer(initiatedGame, out string _);
				HttpContext.Session.SetInt32("CurrentPlayer", otherPlayer);
			}
			//Om båda spelare inte har några giltiga drag kvar jämför poäng och anse vinnare.
			else if (currentPlayerMoves.IsNullOrEmpty() && otherPlayerMoves.IsNullOrEmpty()) 
			{

				//Om spelare två har fler poäng sätt spelet till "Finished", uppdatera WinnerID och status 
				if (player2Points > player1Points)
				{
					initiatedGame.WinnerID = initiatedGame.User2ID;
					initiatedGame.GameStatus = "Finished";

					//int success == 1 om den lyckas uppdatera
					int successWinnerID = new GameMethods().UpdateGameWinnerID(initiatedGame, out string _);
					int successStatus = new GameMethods().UpdateGameStatus(initiatedGame, out string _);

					//Om båda lyckas uppdatera och användanamn på spelare 2 inte är null anse spelare två till vinnare
					if (successWinnerID == 1 && successStatus == 1 && user2Name != null)
					{
						HttpContext.Session.SetString("Winner", user2Name.Username);
					}
				}

				//Annars om spelare ett har fler poäng än spelare två  sätt spelet till "Finished" och uppdatera WinnerID och status
				else if (player1Points > player2Points)
				{
					
					initiatedGame.WinnerID = initiatedGame.User1ID;
					initiatedGame.GameStatus = "Finished";

					//int success == 1 om den lyckas uppdatera 
					int successWinnerID = new GameMethods().UpdateGameWinnerID(initiatedGame, out string _);
					int successStatus = new GameMethods().UpdateGameStatus(initiatedGame, out string _);

					//Om båda lyckas uppdatera och användarnamn på spelare 1 inte är null anse spelare ett till vinnare
					if (successWinnerID == 1 && successStatus == 1 && user1Name != null)
					{
						HttpContext.Session.SetString("Winner", user1Name.Username);
					}
				}
				//Om båda har lika många poäng sätt "Winner" till "Draw"
				else
				{
					initiatedGame.GameStatus = "Finished";
					new GameMethods().UpdateGameStatus(initiatedGame, out string _);
					HttpContext.Session.SetString("Winner", "Draw");
				}
				return PartialView("OthelloGameBoard", boardArray);
			}

			return PartialView("OthelloGameBoard", boardArray);
		}

		/*
			Lägger till en spelare i ett spel och returnerar spelbrädet 
		*/
		public IActionResult JoinGame(GameDetails gd)
		{
			//Återställ spelsession som ligger kvar
			HttpContext.Session.Remove("Winner");
			HttpContext.Session.Remove("Player1Points");
			HttpContext.Session.Remove("Player2Points");
			HttpContext.Session.Remove("CurrentPlayer");
			HttpContext.Session.Remove("PlayerNumber");

			//Hämtar spelinfo
			gd.GameName = new GameMethods().GetGameInfoByID(gd.GameID, out string message).GameName;
			gd.User2ID = (int)HttpContext.Session.GetInt32("UserID");

			//Sätt status till "Playing" och sätt namnet i en sessionsvariabel
			gd.GameStatus = "Playing";
			HttpContext.Session.SetString("GameName", gd.GameName);

			//Uppdaterar status och spelare 2 i databasen
			int currentStatus = new GameMethods().UpdateGameStatus(gd, out string message2);
			int updateUser2 = new GameMethods().UpdateUser2ID(gd, out string msg);

			//Om båda uppdateras korrekt 
			if (currentStatus == 1 && updateUser2 == 1)
			{
				//Sätt spelare två till vit i en sessions variabel och returnera vyn
				HttpContext.Session.SetInt32("PlayerNumber", 2);
				return RedirectToAction("OthelloBoard");
			}
			else
			{
				return View();
			}
		}

		/*
			Hämta en partiellvy av spellistan
		*/
		public IActionResult GameList(bool sorted)
		{
			List<GameDetails> availableGames = new GameMethods().GetAllGames(out string message);

			//Om den får "sorted" visa den sorterade listan istället
			if (sorted)
			{
				availableGames = availableGames.OrderByDescending(ag => ag.GameStatus).ToList();
				ViewBag.Sorted = sorted;
			}

			return PartialView("GameList", availableGames);
		}

		/*
			Lämna spel vyn och returnera första vyn.
		*/
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult LeaveGame()
		{
			//Ta fram vilket spel som ska lämnas
			var gameName = HttpContext.Session.GetString("GameName");
			GameDetails gd = new GameMethods().GetGameByName(gameName, out string message);

			//Om spelnamnet inte är null och det inte finns en andra spelare ta bort spelet
			if (!string.IsNullOrEmpty(gameName) && gd.User2ID == null)
			{
				new GameMethods().DeleteGameByName(gameName, out string _);
			}
			//Annars om användare 1 ID i spelet stämmer överens med sessionsvariabeln sätt andra spelaren till vinnare
			else if (HttpContext.Session.GetInt32("UserID") == gd.User1ID)
			{
				gd.WinnerID = gd.User2ID;
				gd.GameStatus = "Finished";
				int updateWinner = new GameMethods().UpdateGameWinnerID(gd, out string message2);
				int updateGameStatus = new GameMethods().UpdateGameStatus(gd, out string message3);

			}
			//Annars om användare 2 ID stämmer överens med sessionsvariablen sätt första spelaren till vinnare
			else if (HttpContext.Session.GetInt32("UserID") == gd.User2ID)
			{
				gd.WinnerID = gd.User1ID;
				gd.GameStatus = "Finished";
				int updateWinner = new GameMethods().UpdateGameWinnerID(gd, out string message2);
				int updateGameStatus = new GameMethods().UpdateGameStatus(gd, out string message3);
			}



			
			HttpContext.Session.Remove("GameName");
			return RedirectToAction("Games");
		}

		[HttpPost]
		public IActionResult makeMove(int row, int col, int currentplayer)
		{
			GameMethods gm = new GameMethods();
			OthelloLogic gameLogic = new OthelloLogic();

			string currentGame = HttpContext.Session.GetString("GameName") ?? "";
			GameDetails gd = gm.GetGameByName(currentGame, out string message1);

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
