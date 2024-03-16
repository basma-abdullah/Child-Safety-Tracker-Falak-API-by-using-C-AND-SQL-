using FalaKAPP.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("login/{Username},{Password}")]
        public IActionResult login(string Username, string Password)
        {
            using (var conn = DatabaseSettings.dbConn)
            {
                conn.Open();
                bool isExist = DatabaseSettings.isExists(Username);
                if (isExist)
                {
                    string sql = "SELECT * FROM PersonUsers WHERE Username = @Username AND Password = @Password";
                    SqlCommand command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Password", Password);

                    SqlDataReader reader = command.ExecuteReader();

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
                        return Ok(user);
                    }

                    reader.Close();
                }

                return NotFound("User not found");
            }
        }

        [HttpPost("signup")]
        public IActionResult signup([FromBody] PersonUsers user)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            using (conn){ 
                Boolean isexist = DatabaseSettings.isExists(user.Username);
                if (!isexist)
                {
                    string sqladd = "INSERT INTO PersonUsers (Username, UserType, FullName, Password, PhoneNumber, Gender, Email, UsernameType, Latitude, Longitude) VALUES ('" + user.Username + "', '" + user.UserType.ToLower() + "', '" + user.FullName + "', '" + user.Password + "', '" + user.PhoneNumber + "', '" + user.Gender + "', '" + user.Email + "', '" + user.UsernameType.ToLower() + "', '" + user.Latitude + "', '" + user.Longitude + "')";
                    SqlCommand comm = new SqlCommand(sqladd, conn);
                    int affectedrow = comm.ExecuteNonQuery();
                    if (affectedrow > 0)
                    {
                        int userid = getID(user.Username);
                        if (userid > 0)
                        {
                            user.UserID = userid;
                            conn.Close();
                            return Ok(user);
                        }
                        else
                        {
                            conn.Close();
                            return BadRequest(" user not created");
                        }
                    }
                }
            }
            conn.Close();
            return BadRequest(" user not created");
        }


        [HttpPut("reset_password/{Username}")]
        public IActionResult reset_password(string Username, string newPassword)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            Boolean isexist = DatabaseSettings.isExists(Username);
            if (isexist)
            {
                string sql = "UPDATE PersonUsers SET Password = '" + newPassword + "' WHERE ChildID = '" + Username + "'";
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



        //Method section

        public static int getID(string Username)
        {
            int userid;
            var conn = DatabaseSettings.dbConn;
            string sql = $"SELECT UserID FROM PersonUsers WHERE username = @username";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", Username);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            { userid = Convert.ToInt32(reader["UserID"]); }
            else userid = -1;
            reader.Close();
            conn.Close();
            return userid;
        }


        [HttpPost("addParentChild")]
        // This web API will be called when we create a child with a card by his parent
        public ActionResult AddParentChild(IFormFile MainImagePath, [FromForm] string username, string UserType, string FullName, string Password, int PhoneNumber, string Gender, string Email, string usernameType,
            int YearOfBirth, string kinshipT, int MainPersonInChargeID)
        {
            // Create the user
            PersonUsers userTemp = new PersonUsers()
            {
                Username = username,
                UserType = UserType,
                FullName = FullName,
                Password = Password,
                PhoneNumber = PhoneNumber,
                Gender = Gender,
                Email = Email,
                UsernameType = usernameType
            };
            PersonUsers user = (PersonUsers)signup(userTemp);

            if (user != null)
            {
                // Check if the MainImagePath and model state are valid
                if (MainImagePath != null && ModelState.IsValid)
                {
                    // Extract the original file name and extension
                    string fileName = Path.GetFileName(MainImagePath.FileName);

                    // Generate the image file name
                    string imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName).ToLower();

                    // Get the image folder path
                    string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DatabaseSettings.ImageDirectory_AddPath));

                    // Combine the image folder path with the image file name
                    string imageFilePath = Path.Combine(imageFolderPath, imageFileName);

                    // Save the uploaded image to the specified file path
                    using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        MainImagePath.CopyTo(fileStream);
                    }

                    // Create the child object
                    PersonChilds childTemp = new PersonChilds()
                    {
                        ChildID = user.UserID,
                        YearOfBirth = YearOfBirth,
                        MainImagePath = DatabaseSettings.ImageDirectory_AddPath + "/" + imageFileName,
                        KinshipT = kinshipT,
                        MainPersonInChargeID = MainPersonInChargeID
                    };

                    // Add the child to personchild
                    PersonChilds child = (PersonChilds)AddChild(childTemp);

                    if (child != null)
                    {
                        return Ok("Child created successfully");
                    }
                    else
                    {
                        return BadRequest("Error: Child account not created");
                    }
                }
                else
                {
                    return BadRequest("Error: Invalid image or model state");
                }
            }

            return BadRequest("Error: User not created");
        }


        // Add the child 
        [HttpPost("addChild")]
        public IActionResult AddChild([FromForm] PersonChilds child)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            string sql = "insert into PersonChilds(ChildID , YearOfbirth, mainImagePath, kinshipT, MainPersonInChargeID)values ('" + child.ChildID + "', '" + child.YearOfBirth + "', '" + child.MainImagePath + "', '" + child.KinshipT + "', '" + child.MainPersonInChargeID + "')";
            SqlCommand cmd = new SqlCommand(sql, conn);
            int affectedrow = cmd.ExecuteNonQuery();
            if (affectedrow > 0)
            {
                conn.Close();
                return Ok(child);
            }
            else {
                conn.Close();
                return null;
                
            }
            conn.Close();
        }


        [HttpPost("createChildAccount")]  //this web API WILL BE called when we create childe with card by his phone
        public ActionResult CreateChildAccount(IFormFile MainImagePath, [FromForm] string username, string UserType, string FullName, string Password, int PhoneNumber, string Gender, string Email, string usernameType, int YearOfbirth )
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            // we have to create his user
            PersonUsers userTemp = new PersonUsers()
            {
                Username = username,
                UserType = UserType,
                FullName = FullName,
                Password = Password,
                PhoneNumber = PhoneNumber,
                Gender = Gender,
                Email = Email,
                UsernameType = usernameType
            };

            PersonUsers user = (PersonUsers)signup(userTemp);

            if (user != null)
                // Check if the MainImagePath and model state are valid
                if (MainImagePath != null && ModelState.IsValid)
                {
                    // Extract the original file name and extension
                    string fileName = Path.GetFileName(MainImagePath.FileName);

                    // Generate the image file name
                    string imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName).ToLower();

                    // Get the image folder path
                    string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DatabaseSettings.ImageDirectory_AddPath));

                    // Combine the image folder path with the image file name
                    string imageFilePath = Path.Combine(imageFolderPath, imageFileName);

                    // Save the uploaded image to the specified file path
                    using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        MainImagePath.CopyTo(fileStream);
                    }

                    // Create the child object
                    PersonChilds childTemp = new PersonChilds()
                    {
                        ChildID = user.UserID,
                        YearOfBirth = YearOfbirth,
                        MainImagePath = DatabaseSettings.ImageDirectory_AddPath + "/" + imageFileName,
                    };

                    // Add the child to personchild
                    PersonChilds child = (PersonChilds)AddChild(childTemp);

                    if (child != null)
                    {
                        return Ok("Child created successfully");
                    }
                    else
                    {
                        return BadRequest("Error: Child account not created");
                    }
                }
                else
                {
                    return BadRequest("Error: Invalid image or model state");
                }
            
            return BadRequest("Error: Invalid image or model state");
        }

        //generate qr code which represent url to object https://localhost:7111/api/Items/22



       







    }
}






            
       
    
