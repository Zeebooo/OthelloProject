using OthelloProject.Models.Methods;

namespace OthelloProject.Models
{
	public class OthelloLogic
	{
		public OthelloLogic() { }

		public bool isInsideBoard(int row, int col)
		{
			return row <= 7 && col <= 7 && row >= 0 && col >= 0;
		}

		public int flipIfValid(int[,] board, int row, int col, int dirRow, int dirCol, int player, GameDetails gd)
		{
			int nextRowInDir = row + dirRow;
			int nextColInDir = col + dirCol;

			if (!isInsideBoard(nextRowInDir, nextColInDir) || board[nextRowInDir, nextColInDir] == player)
			{
				return 0;
			}

			while (isInsideBoard(nextRowInDir, nextColInDir))
			{
				if (board[nextRowInDir, nextColInDir] == 0)
				{
					return 0;
				}

				if (board[nextRowInDir, nextColInDir] == player)
				{
					int flipRow = row + dirRow;
					int flipCol = col + dirCol;

					while (board[flipRow, flipCol] != player)
					{
						board[flipRow, flipCol] = player;
						flipRow += dirRow;
						flipCol += dirCol;
					}

					string newBoard = new ConverterMethods().ConvertBoardArrayToString(board);
					gd.Board = newBoard;
					int success = new GameMethods().UpdateBoard(gd, out string message);

					if (success != 1)
					{
						return -1;

					}
					return 1;
				}

				nextRowInDir += dirRow;
				nextColInDir += dirCol;
			}

			return 0;
		}

		public bool BoardState(int row, int col, int player, int[,] board, GameDetails gd)
		{
			if (!isInsideBoard(row, col) || board[row, col] != 0)
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

					flipped += flipIfValid(board, row, col, dirRow, dirCol, player, gd);

					if (flipped == -1)
					{
						return false;
					}

					if (dirRow == 1 && dirCol == 1 && flipped != 0) // Här måste vi lägga ut nya pjäsen
					{
						string currentBoard = new GameMethods().GetBoard(gd, out string message); // Hämta flippade brädet (minus pjäsen vi lägger ut) från databasen
						int[,] newBoard = new ConverterMethods().ConvertBoardStringToArray(currentBoard); // Gör om den till en array
						newBoard[row, col] = player; // Lägg in pjäsen i nya arrayen
						string updatedBoard = new ConverterMethods().ConvertBoardArrayToString(newBoard); // Gör om den till en string
						gd.Board = updatedBoard; // Uppdatera GameDetails med nya brädet
						int success = new GameMethods().UpdateBoard(gd, out string message2); // Uppdatera brädet i databasen
						
						return true;
					}

				}
			}

			return false;

		}
	}
}