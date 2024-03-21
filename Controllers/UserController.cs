using FalaKAPP.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;
using QRCodes.Controllers;
using static QRCoder.PayloadGenerator;
using System.Data;


namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("login/{Username},{Password}")]
        public ActionResult<PersonUsers> login(string Username, string Password)
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

                return NotFound("User not exsist");
            }
        }

        [HttpPost("signup")]
        public ActionResult<PersonUsers> signup([FromBody] PersonUsers user)
        {

            var conn1 = DatabaseSettings.dbConn;
            bool isExist = DatabaseSettings.isExists(user.Username);
            if (!isExist)
            {
                using (conn1)
                {
                    conn1.Open();
                    string sqlAdd = "INSERT INTO PersonUsers (Username, UserType, FullName, Password, PhoneNumber, Gender, Email, UsernameType, Latitude, Longitude) VALUES (@Username, @UserType, @FullName, @Password, @PhoneNumber, @Gender, @Email, @UsernameType, @Latitude, @Longitude)";
                    SqlCommand comm = new SqlCommand(sqlAdd, conn1);

                    comm.Parameters.AddWithValue("@Username", user.Username);
                    comm.Parameters.AddWithValue("@UserType", user.UserType.ToLower());
                    comm.Parameters.AddWithValue("@FullName", user.FullName);
                    comm.Parameters.AddWithValue("@Password", user.Password);
                    comm.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    comm.Parameters.AddWithValue("@Gender", user.Gender);
                    comm.Parameters.AddWithValue("@Email", user.Email);
                    comm.Parameters.AddWithValue("@UsernameType", user.UsernameType.ToLower());
                    comm.Parameters.AddWithValue("@Latitude", user.Latitude);
                    comm.Parameters.AddWithValue("@Longitude", user.Longitude);

                    int affectedRows = comm.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        int userId = getID(user.Username);
                        if (userId > 0)
                        {
                            user.UserID = userId;
                            return CreatedAtAction("GetByID", new { UserID = user.UserID }, user);
                        }
                        else
                        {
                            return NotFound("User not created");
                        }
                    }
                }
            }

            return BadRequest("This user already exists. Please try another username.");
        }
        [HttpPut("reset_password/{UserID}")]
        public IActionResult reset_password(int UserID, string newPassword)
        {
            string conn33 = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30";

            using (SqlConnection conn3 = new SqlConnection(conn33))
            {
                conn3.Open();
                Boolean isexist = DatabaseSettings.isIdExists(UserID);
                if (isexist)
                {
                    string sql = "UPDATE PersonUsers SET Password = '" + newPassword + "' WHERE UserID = '" + UserID + "'";
                    using (SqlCommand Command = new SqlCommand(sql, conn3))
                    {
                        Command.ExecuteNonQuery();
                    }
                    return Ok("Successfully updated");
                }
            }

            return NotFound("Error555");
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

        internal static int getID(string Username)
        {
            var conn2 = DatabaseSettings.dbConn;
            int userid;
            string sql = "SELECT UserID FROM PersonUsers WHERE username = @username";
            if (conn2.State != ConnectionState.Open)
            {
                conn2.Open();
            }
            SqlCommand cmd = new SqlCommand(sql, conn2);
            cmd.Parameters.AddWithValue("@username", Username);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                userid = Convert.ToInt32(reader["UserID"]);
            }
            else
            {
                userid = -1;
            }
            reader.Close();
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

            ActionResult<PersonUsers> user = signup(userTemp);
            var conn = DatabaseSettings.dbConn;
            int useridforchild = getID(username );
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
                    string linkqrcode = QrCodeController.GenerateAndStoreQRCode(useridforchild);
                    int verficationCode = GetRandomNumber();
                    // Create the child object
                    PersonChilds childTemp = new PersonChilds()
                    {
                        ChildID = useridforchild,
                        YearOfBirth = YearOfBirth,
                        MainImagePath = DatabaseSettings.ImageDirectory_AddPath + "/" + imageFileName,
                        KinshipT = kinshipT,
                        MainPersonInChargeID = MainPersonInChargeID,
                        QRCodeLink = linkqrcode,
                        VerificationCode = verficationCode,
                    };

                    // Add the child to personchild
                    ActionResult<PersonChilds> child = AddChild(childTemp);

                    if (child.Value != null)
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
        public ActionResult<PersonChilds> AddChild(PersonChilds child)
        {

            var conn = DatabaseSettings.dbConn;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            string sql = "insert into PersonChilds(ChildID , YearOfbirth, mainImagePath, kinshipT, MainPersonInChargeID)values ('" + child.ChildID + "', '" + child.YearOfBirth + "', '" + child.MainImagePath + "', '" + child.KinshipT + "', '" + child.MainPersonInChargeID + "')";
            SqlCommand cmd = new SqlCommand(sql, conn);
            int affectedrow = cmd.ExecuteNonQuery();
            if (affectedrow > 0)
            {
                conn.Close();
                return child;
            }
            else
            {
                conn.Close();
                return null;

            }

        }


        [HttpPost("createChildAccount")]  //this web API WILL BE called when we create childe with card by his phone
        public ActionResult CreateChildAccount(IFormFile MainImagePath, [FromForm] string username, string UserType, string FullName, string Password, int PhoneNumber, string Gender, string Email, string usernameType, int YearOfbirth)
        {
            var conn = DatabaseSettings.dbConn;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
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

            ActionResult<PersonUsers> user = signup(userTemp);
            
            int useridforchild = getID(username);
            
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
                    string linkqrcode = QrCodeController.GenerateAndStoreQRCode(useridforchild);
                    int verficationCode = GetRandomNumber();
                    // Create the child object
                    PersonChilds childTemp = new PersonChilds()
                    {
                        ChildID = useridforchild,
                        YearOfBirth = YearOfbirth,
                        MainImagePath = DatabaseSettings.ImageDirectory_AddPath + "/" + imageFileName,
                        QRCodeLink = linkqrcode,
                        VerificationCode = verficationCode,


                    };
                    // Add the child to personchild
                    ActionResult<PersonChilds> child = AddChild(childTemp);

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

        //generate qr code which represent url to object https://localhost:7111/api/child/22
        
        public static int GetRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000); // Generate a random number between 1000 and 9999
            return randomNumber;
        }
    } 
    }







            
       
    
