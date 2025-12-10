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

			string sqlQuery = "INSERT INTO [Game] (User1ID, GameStatus, Board, GameName) VALUES (@User1ID, @GameStatus, @Board, @GameName)";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@User1ID", gd.User1ID);
			cmd.Parameters.AddWithValue("@GameStatus", gd.GameStatus.ToString());
			cmd.Parameters.AddWithValue("@Board", gd.Board.ToString());
			cmd.Parameters.AddWithValue("@GameName", gd.GameName.ToString());

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

		public GameDetails GetGameInfoByID(int selectedGameID, out string message)
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
					gd.User1ID = (int)reader["User1ID"];
					gd.User2ID = (int)reader["User2ID"];
					gd.GameStatus = reader["GameStatus"].ToString();
					gd.Board = reader["Board"].ToString();
					gd.WinnerID = (int)reader["WinnderID"];
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

			Console.WriteLine("WE ARE HERE");

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
						User1ID = (int)reader["User1ID"],
						GameStatus = reader["GameStatus"].ToString(),
						GameName = reader["GameName"].ToString()
					});
				}

				if(allGames.Count == 0)
				{
					message = "No games were found";
					return null;
				}

				return allGames;
			}
			catch(Exception ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}

		public int UpdateUser2ID(GameDetails selectedGame, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [User2ID] = @User2ID WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@User2ID", selectedGame.User2ID);
			cmd.Parameters.AddWithValue("@GameID", selectedGame.GameID);

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

		public int UpdateGameStatus(GameDetails selectedGame, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [Game] SET [GameStatus] = @GameStatus WHERE [GameID] = @GameID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@GameStatus", selectedGame.GameStatus);
			cmd.Parameters.AddWithValue("@GameID", selectedGame.GameID);

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
	}
}