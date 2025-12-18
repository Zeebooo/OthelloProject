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

		public int flipIfValid(int[,] board, int row, int col, int dirRow, int dirCol, int player)
		{
			int nextRowInDir = row + dirRow;
			int nextColInDir = col + dirCol;

			if (!isInsideBoard(nextRowInDir, nextColInDir) || board[nextRowInDir, nextColInDir] == player)
			{
				return 0; // no opponent piece adjacent
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

					return 1; // flipped in this direction
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

					flipped += flipIfValid(board, row, col, dirRow, dirCol, player);

				}
			}

			if (flipped <= 0)
			{
				return false; // no flips = invalid move
			}

			// Place the new piece and persist the updated board once
			board[row, col] = player;
			string updatedBoard = new ConverterMethods().ConvertBoardArrayToString(board);
			gd.Board = updatedBoard;
			int success = new GameMethods().UpdateBoard(gd, out string message2); // Uppdatera brädet i databasen

			return success == 1;

		}

		public List<(int row, int col)> GetValidMoves(GameDetails gd, int player)
		{
			string board = gd.Board;
			int[,] boardArray = new ConverterMethods().ConvertBoardStringToArray(board);
			List<(int r, int c)> validMoves = new List<(int r, int c)>();

			for (int row = 0; row <= 7; row++)
			{
				for (int col = 0; col <= 7; col++)
				{
					if (boardArray[row, col] == 0)
					{
						for (int rowDir = -1; rowDir <= 1; rowDir++)
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
