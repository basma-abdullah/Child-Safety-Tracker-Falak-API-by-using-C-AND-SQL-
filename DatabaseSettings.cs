using FalaKAPP.Controllers;
using FalaKAPP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace FalaKAPP
{
    public class DatabaseSettings
    {
            public DatabaseSettings() { }
            public static string ImageDirectory_AddPath = "wwwroot\\FalakImage";
            public static string ImageDirectory_ReadPath = "FalakImage";
            public static string application_URLRequestPath = "http://localhost:5218";
            public static SqlConnection dbConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30");



        internal static bool isExists(string Username)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM PersonUsers WHERE Username = @Username";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool exists = reader.Read();
                        conn.Close();
                        return exists;
                    }
                }
            }
        }


        internal static bool isIdExists(int ID)
        {
            if (dbConn.State != ConnectionState.Open)
            {
                dbConn.Open();
            }

            string sql = "SELECT * FROM PersonUsers WHERE UserID = @ID";
            using (SqlCommand cmd = new SqlCommand(sql, dbConn))
            {
                cmd.Parameters.AddWithValue("@ID", ID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        internal static ActionResult<PersonUsers> GetByID(int UserID)
        {
            dbConn.Open();
            string sql = "SELECT * FROM PersonUsers WHERE UserID = @UserID ";
            SqlCommand command = new SqlCommand(sql, dbConn);
            command.Parameters.AddWithValue("@UserID", UserID);

            SqlDataReader reader = command.ExecuteReader();
            PersonUsers user = new PersonUsers();
            if (reader.HasRows)
            {
                reader.Read();
                user = new PersonUsers
                {
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    UserType = reader.GetString(reader.GetOrdinal("UserType")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Password = reader.GetString(reader.GetOrdinal("Password")),
                    PhoneNumber = reader.GetInt32(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.GetString(reader.GetOrdinal("Gender")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    UsernameType = reader.GetString(reader.GetOrdinal("UsernameType")),
                };

                reader.Close();

            }
            if (user != null)
            {
                return new OkObjectResult(user);
            }
            else
            {
                return new StatusCodeResult(404);
            }
        }
    }
}
