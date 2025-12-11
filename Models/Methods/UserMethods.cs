using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using OthelloProject.Models.Methods;

namespace OthelloProject.Models
{
	public class UserMethods
	{

		public UserMethods() { }

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
					if (reader["UserID"] != DBNull.Value){
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

		public int UpdateUserName(UserDetails selectedUser, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Username] = @Username";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Username", selectedUser.Username);

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

		public int UpdatePassword(UserDetails selectedUser, out string message)
		{
			message = "";

			var PasswordHasher = new PasswordHasher<UserDetails>();
			string hashedPassword = PasswordHasher.HashPassword(selectedUser, selectedUser.Password);
			selectedUser.Password = hashedPassword;

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Password] = @Password";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Password", selectedUser.Password);

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
		public int UpdateEmail(UserDetails selectedUser, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "UPDATE [User] SET [Email] = @Email";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@Email", selectedUser.Email);

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
	}
}