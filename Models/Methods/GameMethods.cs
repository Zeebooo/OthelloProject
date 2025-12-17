using Microsoft.Data.SqlClient;
using OthelloProject.Models.Methods;

namespace OthelloProject.Models
{
	public class GameMethods
	{
		public GameMethods() { }

		public IConfigurationRoot GetConnection() // Metod för att hämta koppling till databasen
		{
			var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
			return builder;
		}

		public SqlConnection Connect() // Hjälpfuntion
		{
			SqlConnection dbConnection = new SqlConnection(GetConnection().GetSection("ConnectionStrings").GetSection("DefaultConnection").Value);
			return dbConnection;
		}

		public int InsertGame(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "INSERT INTO [Game] (User1ID, GameStatus, Board, GameName, CurrentPlayer) VALUES (@User1ID, @GameStatus, @Board, @GameName, @CurrentPlayer)";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@User1ID", gd.User1ID);
			cmd.Parameters.AddWithValue("@GameStatus", gd.GameStatus.ToString());
			cmd.Parameters.AddWithValue("@Board", gd.Board.ToString());
			cmd.Parameters.AddWithValue("@GameName", gd.GameName.ToString());
			cmd.Parameters.AddWithValue("@CurrentPlayer", 1);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				message = "Successfully created a new game";
				if (rowsAffected != 1)
				{
					message = "An error occurred when creating a game";
				}
				return rowsAffected;
			}
			catch (SqlException ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public GameDetails GetGameInfoByID(int? selectedGameID, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [Game] WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameID", selectedGameID);
			GameDetails gd = new GameDetails();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					gd.GameID = (int)reader["GameID"];
					gd.GameName = reader["GameName"].ToString();
					gd.User1ID = (int)reader["User1ID"];
					gd.GameStatus = reader["GameStatus"].ToString();
					gd.Board = reader["Board"].ToString();
					if (reader["User2ID"] != DBNull.Value)
					{
						gd.User2ID = (int)reader["User2ID"];
					}
				}
				return gd;
			}
			catch (SqlException ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}

		public GameDetails GetGameByName(string gameName, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [Game] WHERE [GameName] = @GameName";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameName", gameName);
			GameDetails gd = new GameDetails();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					gd.GameID = (int)reader["GameID"];
					gd.User1ID = (int)reader["User1ID"];
					gd.GameStatus = reader["GameStatus"].ToString();
					gd.Board = reader["Board"].ToString();
					if (reader["User2ID"] != DBNull.Value)
					{
						gd.User2ID = (int)reader["User2ID"];
					}
					if(reader["WinnerID"] != DBNull.Value)
					{
						gd.WinnerID = (int)reader["WinnerID"];
					}
				}
				return gd;
			}
			catch (SqlException ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}

		public List<GameDetails> GetAllGames(out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [Game]";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			List<GameDetails> allGames = new List<GameDetails>();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					allGames.Add(new GameDetails
					{
						GameID = (int)reader["GameID"],
						User1ID = (int)reader["User1ID"],
						GameStatus = reader["GameStatus"].ToString(),
						GameName = reader["GameName"].ToString()
					});
				}

				if (allGames.Count == 0)
				{
					message = "No games were found";
					return null;
				}

				return allGames;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}

		public string GetBoard(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT [Board] FROM [Game] WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameID", gd.GameID);
			string newBoard = "";

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					newBoard = reader["Board"].ToString();
				}

				if (newBoard == null)
				{
					message = "An error occurred while retrieving board";
					return null;
				}

				return newBoard;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}

		public int GetCurrentPlayer(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT [CurrentPlayer] FROM [Game] WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameID", gd.GameID);
			int currentPlayer = 0;

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					currentPlayer = (int)reader["CurrentPlayer"];
				}

				if (currentPlayer == 0)
				{
					message = "An error occurred while retrieving current player";
					return 0;
				}

				return currentPlayer;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateCurrentPlayer(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [CurrentPlayer] = @CurrentPlayer WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@CurrentPlayer", gd.CurrentPlayer);
			cmd.Parameters.AddWithValue("@GameID", gd.GameID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected != 1)
				{
					message = "An error occurred while updating current player";
					return 0;
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateUser2ID(GameDetails game, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [User2ID] = @User2ID WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@User2ID", game.User2ID);
			cmd.Parameters.AddWithValue("@GameID", game.GameID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected != 1)
				{
					message = "An error occurred while updating User2ID";
					return 0;
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateGameStatus(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [GameStatus] = @GameStatus WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameStatus", gd.GameStatus);
			cmd.Parameters.AddWithValue("@GameID", gd.GameID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected != 1)
				{
					message = "An error occurred while updating GameStatus";
					return 0;
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateGameWinnerID(GameDetails selectedGame, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [WinnerID] = @WinnerID WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@WinnerID", selectedGame.WinnerID);
			cmd.Parameters.AddWithValue("@GameID", selectedGame.GameID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected != 1)
				{
					message = "An error occurred while updating WinnerID";
					return 0;
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateBoard(GameDetails gd, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [Board] = @Board WHERE [GameID] = @GameID";

			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Board", gd.Board);
			cmd.Parameters.AddWithValue("@GameID", gd.GameID);

			try
			{
				conn.Open();

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected != 1)
				{
					message = "An error occurred while updating board";
					return 0;
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}

		}

		public int DeleteGameByName(string gameName, out string message)
		{
			message = "";

			if (string.IsNullOrEmpty(gameName))
			{
				message = "Game name is required.";
				return 0;
			}

			SqlConnection conn = Connect();
			string sqlQuery = "DELETE FROM [Game] WHERE [GameName] = @GameName";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@GameName", gameName);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected != 1)
				{
					message = "No game deleted.";
				}
				return rowsAffected;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		public List<RecentGame> GetRecentGamesForUser(int userId, int take, out string message)
		{
			message = "";
			var recentGames = new List<RecentGame>();

			using SqlConnection conn = Connect();

			string sqlQuery = @"
				SELECT TOP (@Take)
					CASE 
						WHEN g.User1ID = @UserId THEN u2.Username
						WHEN g.User2ID = @UserId THEN u1.Username
						ELSE NULL
					END AS OpponentName,
					CASE 
						WHEN g.WinnerID IS NULL THEN 'In progress'
						WHEN g.WinnerID = @UserId THEN 'Win'
						ELSE 'Loss'
					END AS Result
				FROM [Game] g
				LEFT JOIN [User] u1 ON g.User1ID = u1.UserID
				LEFT JOIN [User] u2 ON g.User2ID = u2.UserID
				WHERE g.User1ID = @UserId OR g.User2ID = @UserId
				ORDER BY g.GameID DESC";

			using SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@Take", take);

			try
			{
				conn.Open();
				using SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					string opponent = reader["OpponentName"] != DBNull.Value ? reader["OpponentName"].ToString() : "Unknown";
					string result = reader["Result"] != DBNull.Value ? reader["Result"].ToString() : "In progress";
					recentGames.Add(new RecentGame
					{
						OpponentName = opponent,
						Result = result
					});
				}
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			return recentGames;
		}
	}
}
