using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data.SqlClient;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("login/{Username},{Password}")]
        public IActionResult login(string username, string Password)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            Boolean isexist = DatabaseSettings.isExists(username);
            if(isexist)
            {
                string sql = "SELECT * FROM PersonUsers WHERE Username = '" + username + "' AND Password = '" + Password + "'";
                SqlCommand Comm = new SqlCommand(sql, conn);
                //query string
                SqlDataReader reader = Comm.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    PersonUsers user = new PersonUsers
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
                    conn.Close();
                    return Ok(user);
                }

                reader.Close();
            }
            conn.Close();
            return NotFound("user not found");
        }

        [HttpPost("signup")]
        public IActionResult signup([FromBody] PersonUsers user)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            Boolean isexist = DatabaseSettings.isExists(user.Username);
            if (!isexist)
            {
               
                string sqladd = "INSERT INTO PersonUsers (Username, UserType, FullName, Password, PhoneNumber, Gender, Email, UsernameType) VALUES ('" + user.PhoneNumber + "','" + user.UserType + "','" + user.FullName + "','" + user.Password + "','" + user.PhoneNumber + "','" + user.Gender + "','" + user.Email + "','Phone')";
                SqlCommand comm = new SqlCommand(sqladd, conn);
                comm.ExecuteNonQuery();
                conn.Close();
                return Ok("Sucessfully added");
            }
            conn.Close();

            return NotFound("error");

        }

        [HttpPut("reset_password/{Username}")]
        public IActionResult reset_password(string Username, string newPassword)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            Boolean isexist = DatabaseSettings.isExists(Username);
            if (isexist)
            {
                string sql = "UPDATE PersonUsers SET Password = '" + newPassword + "' WHERE Username = '" + Username + "'";
                SqlCommand Command = new SqlCommand(sql, conn);
                Command.ExecuteNonQuery();
                conn.Close(); 
                return Ok("Sucessfully updated");
            }
            conn.Close();

            return NotFound("error");
        }


        [HttpDelete("delete_account/{Username}")]
        public IActionResult delete_account(string Username)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            Boolean isexist = DatabaseSettings.isExists(Username);
            if (isexist)
            {
                string sql = "DELETE FROM PersonUsers WHERE Username = '" + Username + "'";
                SqlCommand Command = new SqlCommand(sql, conn);
                Command.ExecuteNonQuery();
                conn.Close();
                return Ok("Sucessfully deleted");
            }
            
            conn.Close();
            return NotFound("error");
            
        }
    }
}
