using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
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
		
		public UserDetails GetUserInfoByID(int userid, out string message)
		{
			message = "";

			SqlConnection conn = Connect();

			string sqlQuery = "SELECT * FROM [User] WHERE [UserID] = @UserID";
			SqlCommand cmd = new SqlCommand(sqlQuery, conn);

			cmd.Parameters.AddWithValue("@UserID", System.Data.SqlDbType.Int).Value = userid;
			UserDetails ud = new UserDetails();

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					ud.Username = reader["Username"].ToString();
					ud.UserID = (int)reader["UserID"];
				}
				
				return ud;
			}
			catch(SqlException ex)
			{
				message = ex.Message;
				return null;
			}
			finally
			{
				conn.Close();
			}
		}
	}

}