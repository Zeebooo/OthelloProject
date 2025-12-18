using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using OthelloProject.Models.Methods;
using System;
using Microsoft.Identity.Client;

namespace OthelloProject.Models
{
	public class UserMethods
	{

		public UserMethods() { } // Tom konstruktor

		public IConfigurationRoot GetConnection() // Metod för att hämta koppling till databasen
		{
			var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
			return builder;
		}

		public SqlConnection Connect() // Hjälpfunktion
		{
			SqlConnection dbConnection = new SqlConnection(GetConnection().GetSection("ConnectionStrings").GetSection("DefaultConnection").Value);
			return dbConnection;
		}

		/*	Namn: InsertUser
			Tar in en UserDetails som innehåller all nödvändig
			info om den nya användaren och sätter in det i 
			databasen.
		*/
		public int InsertUser(UserDetails ud, out string message)
		{
			message = "";
			var passwordHasher = new PasswordHasher<UserDetails>();
			ud.Password = passwordHasher.HashPassword(ud, ud.Password);

			SqlConnection conn = Connect();

			string sqlQuery = "INSERT INTO [User] (Username, Email, [Password]) VALUES (@Username, @Email, @Password)";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@Username", ud.Username);
			cmd.Parameters.AddWithValue("@Email", ud.Email);
			cmd.Parameters.AddWithValue("@Password", ud.Password);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();
				message = "User successfully registered.";

				if (rowsAffected != 1)
				{
					message = "User registration failed.";
				}

				return rowsAffected;
			}
			catch (SqlException ex)
			{
				if (ex.Number == 2627) // Unique constraint error number
				{
					if (ex.Message.Contains("Username"))
					{
						message = "Username already exists.";
						return -1;
					}
					else if (ex.Message.Contains("Email"))
					{
						message = "Email is already taken.";
						return -2;
					}
				}

				message = "An error occurred while registering the user: " + ex.Message;
				return 0;
			}
			finally
			{
				conn.Close();
			}
		}

		/*	Namn: GetUserInfoByID
			Tar in en int som är ett UserID och hämtar all information (förutom lösenord)
			relaterat till den användaren från databasen. 
		*/
		public UserDetails GetUserInfoByID(int? selectedUserID, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [User] WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@UserID", selectedUserID);
			UserDetails ud = new UserDetails();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					ud.Username = reader["Username"].ToString();
					ud.Email = reader["Email"].ToString();
					if (reader["UserID"] != DBNull.Value)
					{
						ud.UserID = (int)reader["UserID"];
					}
				}

				return ud;
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

		/*	Namn: VerifyLogin
			Tar in en sträng som är ett användarnamn och hämtar allt utom email
			relaterat till den användaren från databasen. Detta för att sedan
			jämföras i controllern med det som skrevs in i inloggningssidan.	
		*/
		public UserDetails? VerifyLogin(string username, out string message)
		{
			message = "";

			SqlConnection conn = Connect();
			string sqlQuery = "SELECT * FROM [User] WHERE [Username] = @Username";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@Username", username);

			UserDetails? ud = null;

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				if (reader.Read())
				{
					ud = new UserDetails
					{
						UserID = (int)reader["UserID"],
						Username = reader["Username"].ToString(),
						Password = reader["Password"].ToString()
					};
				}

				return ud;
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

		/*	Namn: GetAllUsers
			Tar inte in något, hämtar alla användare från databasen
			och returnerar dem i en lista. 
		*/
		public List<UserDetails> GetAllUsers(out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [User]";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			List<UserDetails> udList = new List<UserDetails>();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					udList.Add(new UserDetails
					{
						UserID = (int)reader["UserID"],
						Username = reader["Username"].ToString(),
						Email = reader["Email"].ToString()
					});

					if (udList.Count() == 0)
					{
						message = "No users were found";
						return null;
					}
				}
				return udList;
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

		/*	Namn: UpdateUserName
			Tar in en UserDetails som inparameter, denna innehåller ett nytt 
			username. Uppdaterar sedan användarnamnet till rätt UserID.
		*/
		public int UpdateUserName(UserDetails selectedUser, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Username] = @Username WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Username", selectedUser.Username);
			cmd.Parameters.AddWithValue("@UserID", selectedUser.User1ID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected != 1)
				{
					message = "Error occurred when updating username";
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

		/*	Namn: UpdatePassword
			Tar in en UserDetails som inparameter, denna innehåller ett nytt 
			lösenord. Uppdaterar sedan lösenordet till rätt UserID.
		*/
		public int UpdatePassword(UserDetails selectedUser, out string message)
		{
			message = "";

			var PasswordHasher = new PasswordHasher<UserDetails>();
			string hashedPassword = PasswordHasher.HashPassword(selectedUser, selectedUser.Password);
			selectedUser.Password = hashedPassword;

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Password] = @Password WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Password", selectedUser.Password);
			cmd.Parameters.AddWithValue("@UserID", selectedUser.UserID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected != 1)
				{
					message = "Error occurred when updating password";
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

		/*	Namn: UpdatEmail
			Tar in en UserDetails som inparameter, denna innehåller ett nytt 
			Email. Uppdaterar sedan emailen till rätt UserID.
		*/
		public int UpdateEmail(UserDetails selectedUser, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Email] = @Email WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Email", selectedUser.Email);
			cmd.Parameters.AddWithValue("@UserID", selectedUser.User1ID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected != 1)
				{
					message = "Error occurred when updating email";
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

		/*	Namn: DeleteUser
			Tar in en int i form av ett UserID och tar bort den raden från
			User tabellen i databasen. Kör sedan en "Cleanup" där den kollar
			igenom hela games tabellen och kollar efter games där varken
			User1ID eller User2ID finnsi User tabellen och tar bort
			dessa games.
		*/
		public int DeleteUser(int selectedUserID, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "DELETE FROM [User] WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@UserID", selectedUserID);

			try
			{
				conn.Open();
				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected != 1)
				{
					message = "An error occurred while removing user";
					return 0;
				}


				if (rowsAffected > 0)
				{
					string sqlCleanupQuery = @"DELETE FROM [Game] 
					WHERE [User1ID] NOT IN (SELECT [UserID] FROM [User])
					AND [User2ID] NOT IN (SELECT [UserID] FROM [User])";

					SqlCommand cleanupCmd = new SqlCommand(sqlCleanupQuery, conn);
					cleanupCmd.ExecuteNonQuery();
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

		/*	Namn: GetUserByEmail
			Tar in en sträng som är en email och hämtar sedan allt relaterat
			till den användaren från databasen. Returneras i en UserDetails.
		*/
		public UserDetails? GetUserByEmail(string email, out string message)
		{
			message = "";
			using SqlConnection conn = Connect();
			string sqlQuery = "SELECT TOP 1 * FROM [User] WHERE [Email] = @Email";
			using SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@Email", email);

			try
			{
				conn.Open();
				using SqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					return new UserDetails
					{
						UserID = (int)reader["UserID"],
						Username = reader["Username"].ToString(),
						Email = reader["Email"].ToString(),
					};
				}
				return null;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return null;
			}
		}

		/*	Namn: GetWinningStats
			Tar in ett UserID och kollar igenom databasen och summerar antalet matcher
			som en avnändare har vunnit och skickar tillbaka det till controllern.
		*/
		public int GetWinningStats(int userID, out int totalGames, out int gamesWon, out string message)
		{
			message = "";
			totalGames = 0;
			gamesWon = 0;
			SqlConnection conn = Connect();
			string sqlQuery = "SELECT COUNT(*) AS TotalGames, SUM(CASE WHEN (WinnerID = @UserID) THEN 1 ELSE 0 END) AS GamesWon FROM [Game] WHERE User1ID = @UserID OR User2ID = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);
			cmd.Parameters.AddWithValue("@UserID", userID);
			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					totalGames = reader["TotalGames"] != DBNull.Value ? Convert.ToInt32(reader["TotalGames"]) : 0;
					gamesWon = reader["GamesWon"] != DBNull.Value ? Convert.ToInt32(reader["GamesWon"]) : 0;
				}
				return 1; // Success
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0; // Failure
			}
			finally
			{
				conn.Close();
			}
		}

		/*	Namn: UpdateUserProfile
			Tar in användarinformation förutom lösenord och uppdaterar dessa i databasen
		*/
		public int UpdateUserProfile(int userId, string username, string email, out string message)
		{
			message = "";
			using SqlConnection conn = Connect();
			string sql = "UPDATE [User] SET Username = @Username, Email = @Email WHERE UserID = @UserID";
			using SqlCommand cmd = new SqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@Username", username);
			cmd.Parameters.AddWithValue("@Email", email);
			cmd.Parameters.AddWithValue("@UserID", userId);

			try
			{
				conn.Open();
				int rows = cmd.ExecuteNonQuery();
				if (rows != 1) message = "Ingen rad uppdaterades.";
				return rows;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
		}

		/*	Namn: UpdatePasswordById
			Tar in ett UserID och ett o-hashat lösenord. Den hashar sedan lösernordet och
			uppdaterar det gamla lösenordet i databasen med det nya.
		*/
		public int UpdatePasswordById(int userId, string rawPassword, out string message)
		{
			message = "";
			var hasher = new PasswordHasher<UserDetails>();
			var ud = new UserDetails { UserID = userId };
			string hashed = hasher.HashPassword(ud, rawPassword);

			using SqlConnection conn = Connect();
			string sql = "UPDATE [User] SET [Password] = @Password WHERE UserID = @UserID";
			using SqlCommand cmd = new SqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@Password", hashed);
			cmd.Parameters.AddWithValue("@UserID", userId);

			try
			{
				conn.Open();
				int rows = cmd.ExecuteNonQuery();
				if (rows != 1) message = "Ingen rad uppdaterades.";
				return rows;
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return 0;
			}
		}

	}
}
