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
        public static string dbConn = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30";



        internal static bool isExists(string Username)
        {

            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                string sql = "SELECT * FROM PersonUsers WHERE Username = @Username";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool exists = true;
                            return exists;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }
            }
        }


        internal static bool isIdExists(int ID)
        {
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                string sql = "SELECT * FROM PersonUsers WHERE UserID = @ID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
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
        }

        internal static int getID(string Username)
        {
            int userid;
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                string sql = "SELECT UserID FROM PersonUsers WHERE username = @username";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", Username);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userid = Convert.ToInt32(reader["UserID"]);
                            conn.Close();
                            return userid;
                        }
                        else
                        {
                            userid = -1;
                            conn.Close();
                            return userid;

                        }
                    }
                }
            }
        }

        internal static ActionResult<PersonUsers?> GetByID(int UserID)
        {
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                conn.Open(); // Open the database connection

                string sql = "SELECT * FROM PersonUsers WHERE UserID = @UserID";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@UserID", UserID);

                SqlDataReader reader = command.ExecuteReader();
                PersonUsers? user = null;

                if (reader.Read())
                {
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
                }

                reader.Close();

                if (user != null)
                {
                    return new OkObjectResult(user);
                }
                else
                {
                    return new NotFoundResult(); // Return appropriate result when no user is found
                }
            }
        }


        public static bool isMainPersonInChargeIDExists(int childid)
        {
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                string sql = "SELECT COUNT(*) FROM PersonChilds WHERE ChildID = @ChildID AND MainPersonInChargeID IS NOT NULL";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", childid);
                    conn.Open();
                    int count = (int)command.ExecuteScalar();
                    if (count > 0)
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
    }
}