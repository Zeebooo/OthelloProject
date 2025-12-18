using OthelloProject.Models.Methods;

namespace OthelloProject.Models
{
	public class OthelloLogic
	{
		public OthelloLogic() { } // Tom konstruktor

		/*	Namn: isInsideBoard
			Tar in en int som representerar nuvarande rad och en som
			representerar nuvarande kolumn. Kollar så att de är mellan
			0 och 7 (alltså innanför brädert) och returnerar true
			om det stämmer. 
		*/
		public bool isInsideBoard(int row, int col)
		{
			return row <= 7 && col <= 7 && row >= 0 && col >= 0;
		}

		/*	Namn: flipIfValid
			Tar in nuvarande brädet, raden, kolumnen, en direction för raden och kolumnen och en player.
			Provar först att hoppa ett steg åt hållet vi är påväg mot och kollar så man är innanför
			brädet och att man inte hamnar på en egen pjäs. I while loopen fortsätter vi att gå i
			riktningen tills vi hamnat på vår egna bricka igen, då börjar vi stegvis vända motståndarnas
			brickor som leder upp till den brickan vi stannade på. 
		*/
		public int flipIfValid(int[,] board, int row, int col, int dirRow, int dirCol, int player)
		{
			int nextRowInDir = row + dirRow;
			int nextColInDir = col + dirCol;

			if (!isInsideBoard(nextRowInDir, nextColInDir) || board[nextRowInDir, nextColInDir] == player)
			{
				return 0; // ingen motståndarpjäs bredvid
			}

			while (isInsideBoard(nextRowInDir, nextColInDir))
			{
				if (board[nextRowInDir, nextColInDir] == 0) // Kollar så vi inte hamnat på en tom ruta
				{
					return 0;
				}

				if (board[nextRowInDir, nextColInDir] == player)
				{
					int flipRow = row + dirRow;
					int flipCol = col + dirCol;

					while (board[flipRow, flipCol] != player)
					{
						board[flipRow, flipCol] = player; // Flippar alla pjäsen emellan den vi försöker lägga ut på och fram till nästa egna bricka. 
						flipRow += dirRow;
						flipCol += dirCol;
					}

					return 1; // flipped in this direction
				}

				nextRowInDir += dirRow;
				nextColInDir += dirCol;
			}

			return 0;
		}

		/*	Namn: BoardState
			Tar in nuvarande raden, kolumnen, spelare, brädet och GameDetails.
			Kollar directions från nuvarande position och skickar dem till 
			flipIfValid för vändning. Vänder sedan på brickan på nuvarande
			position innan den är klar. 
		*/
		public bool BoardState(int row, int col, int player, int[,] board, GameDetails gd)
		{
			if (!isInsideBoard(row, col) || board[row, col] != 0) // Kollar så vi är innanför brädet och att platsen inte är tom
			{
				return false;
			}

			int flipped = 0;
			for (int dirRow = -1; dirRow <= 1; dirRow++)
			{
				for (int dirCol = -1; dirCol <= 1; dirCol++)
				{
					if (dirRow == 0 && dirCol == 0) // detta är ingen riktning
					{
						continue;
					}

					flipped += flipIfValid(board, row, col, dirRow, dirCol, player);

				}
			}

			if (flipped <= 0)
			{
				return false; // inga vändningar, inte ett giltigt drag. 
			}

			// Placerar pjäsen på nuvarande position. 
			board[row, col] = player;
			string updatedBoard = new ConverterMethods().ConvertBoardArrayToString(board);
			gd.Board = updatedBoard;
			int success = new GameMethods().UpdateBoard(gd, out string message2); // Uppdatera brädet i databasen

			return success == 1;

		}

		/*	Namn: GetValidMoves
			Tar in en GameDetails och en player (1 eller 2). Går igenom alla 
			platser på hela brädet som finns i gd och lägger in alla möjliga
			drag i en lista.
		*/
		public List<(int row, int col)> GetValidMoves(GameDetails gd, int player)
		{
			string board = gd.Board;
			int[,] boardArray = new ConverterMethods().ConvertBoardStringToArray(board);
			List<(int r, int c)> validMoves = new List<(int r, int c)>();

			for (int row = 0; row <= 7; row++) // For-loopar för att gå igenom alla platser
			{
				for (int col = 0; col <= 7; col++)
				{
					if (boardArray[row, col] == 0)
					{
						for (int rowDir = -1; rowDir <= 1; rowDir++) // For-loopar för att kolla alla directions från varje plats
						{
							for (int colDir = -1; colDir <= 1; colDir++)
							{
								int nextRowInDir = row + rowDir;
								int nextColInDir = col + colDir;

								if (!isInsideBoard(nextRowInDir, nextColInDir) || boardArray[nextRowInDir, nextColInDir] == player)
								{
									continue;
								}

								while (isInsideBoard(nextRowInDir, nextColInDir))
								{
									if (boardArray[nextRowInDir, nextColInDir] == 0)
									{
										break;
									}

									if (boardArray[nextRowInDir, nextColInDir] == player)
									{
										validMoves.Add((row, col));
										break;
									}

									nextRowInDir += rowDir;
									nextColInDir += colDir;
								}
							}
						}
					}
				}
			}

			return validMoves;
		}
	}
}
