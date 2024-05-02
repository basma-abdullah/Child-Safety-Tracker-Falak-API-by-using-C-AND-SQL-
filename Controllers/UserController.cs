using FalaKAPP.Models;
//using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Data.SqlClient;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;
using QRCodes.Controllers;
using static QRCoder.PayloadGenerator;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;



namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        //login for parent and child
        [HttpGet("login/{Username},{Password}")]
        public ActionResult<PersonUsers> login(string Username, string Password)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
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
                        // Nullable float type
                        float? Latitude = null;

                        if (!reader.IsDBNull(reader.GetOrdinal("Latitude")))
                        {
                            Latitude = (float)reader.GetDouble(reader.GetOrdinal("Latitude"));
                        }
                        // Nullable float type
                        float? longitude = null;

                        if (!reader.IsDBNull(reader.GetOrdinal("Longitude")))
                        {
                            longitude = (float)reader.GetDouble(reader.GetOrdinal("Longitude"));
                        }

                        user.Latitude = Latitude;
                        user.Longitude = longitude;

                        reader.Close();
                        return user;
                    }
                }

                return NotFound("User does not exist");
            }
        }




        //sign UP for parent 
        [HttpPost("signup")]
        public ActionResult<PersonUsers> signup([FromBody] PersonUsers useruser)
        {
            int affectedRows;
            string sqlAdd;
            SqlCommand comm;
            bool isExist = DatabaseSettings.isExists(useruser.Username);
            if (isExist == false)
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    conn.Open();
                  
                    //if (useruser.UserType == "parent") {
                        sqlAdd = "INSERT INTO PersonUsers (Username, UserType, FullName, Password, PhoneNumber, Gender, Email, UsernameType) VALUES (@Username, @UserType, @FullName, @Password, @PhoneNumber, @Gender, @Email, @UsernameType)";
                        comm = new SqlCommand(sqlAdd, conn);
                        comm.Parameters.AddWithValue("@Username", useruser.Username);
                        comm.Parameters.AddWithValue("@UserType", useruser.UserType.ToLower());
                        comm.Parameters.AddWithValue("@FullName", useruser.FullName);
                        comm.Parameters.AddWithValue("@Password", useruser.Password);
                        if (useruser.PhoneNumber.HasValue)
                        {
                            comm.Parameters.AddWithValue("@PhoneNumber", useruser.PhoneNumber);
                        }
                        else
                        {
                            comm.Parameters.AddWithValue("@PhoneNumber", DBNull.Value);
                        }

                        comm.Parameters.AddWithValue("@Gender", useruser.Gender);
                        if (useruser.Email != null)
                        {
                            comm.Parameters.AddWithValue("@Email", useruser.Email);
                        }
                        else
                        {
                            comm.Parameters.AddWithValue("@Email", DBNull.Value);
                        }
                    
                        comm.Parameters.AddWithValue("@UsernameType", useruser.UsernameType.ToLower());
                        affectedRows = comm.ExecuteNonQuery();
            
                    
                    if (affectedRows > 0)
                    {
                        int userId = DatabaseSettings.getID(useruser.Username);
                        if (userId > 0)
                        {
                            useruser.UserID = userId;
                            return Ok(useruser);
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





        //add child APIs either by his phone or by his parent phone:

        // This web API will be called when we create a child with a card by his parent phone (generate QRCODE button)
        [HttpPost("addParentChild")]
        public ActionResult AddParentChild(IFormFile MainImagePath, [FromForm] string username, [FromForm] string UserType, [FromForm] string FullName, [FromForm] string Password, [FromForm] int? PhoneNumber, [FromForm] string Gender, [FromForm] string Email, [FromForm] string usernameType,
           [FromForm] int YearOfBirth, [FromForm] string kinshipT, [FromForm] int MainPersonInChargeID)
        {
       
            // Create the child user
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
            int useridforchild = DatabaseSettings.getID(username);
            if (user != null)
            {
                // Check if the MainImagePath and model state are valid
                if (MainImagePath != null && ModelState.IsValid)
                {


                    // Generate the image file name
                    string imageFileName = useridforchild.ToString();

                    //add extension to image
                    imageFileName += Path.GetExtension(MainImagePath.FileName).ToLower();

                    // Get the image folder path
                    string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),DatabaseSettings.ImageDirectory_AddPath));

                    // Save the uploaded image to the specified file path
                    using (var fileStream = new FileStream(Path.Combine(imageFolderPath,imageFileName), FileMode.Create))

                    MainImagePath.CopyToAsync(fileStream);

    


                    //generate link qrcode for child to be use later on link process
                    string linkqrcode = QrCodeController.GenerateAndStoreQRCode(useridforchild);

                    //generate link number for child to be use later on link process
                    int verficationCode = GetRandomNumber();




                    // Create the child object
                    PersonChilds childTemp = new PersonChilds()
                    {
                        ChildID = useridforchild,
                        YearOfBirth = YearOfBirth,
                        MainImagePath = DatabaseSettings.ImageDirectory_ReadPath + "/" + imageFileName,

                    KinshipT = kinshipT,
                        MainPersonInChargeID = MainPersonInChargeID,
                        QRCodeLink = linkqrcode,
                        VerificationCode = verficationCode,
                    };


                    // Add the child to personchild
                    ActionResult<PersonChilds> child = AddChild(childTemp);

                    if (child.Value != null)
                    {
                        //insert in follow child account
                        bool insertfollow = SettingController.insertHasCardMethod(useridforchild, MainPersonInChargeID , false ,false,"hascard");
                        if (insertfollow)
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



        // Add the child method to add child opject to personchild table
        public static ActionResult<PersonChilds> AddChild(PersonChilds child)
        {

            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            string sql = "insert into PersonChilds(ChildID , YearOfbirth, mainImagePath,VerificationCode , QRCodeLink, kinshipT, MainPersonInChargeID)values ('" + child.ChildID + "', '" + child.YearOfBirth + "', '" + child.MainImagePath + "', '" + child.VerificationCode + "', '" + child.QRCodeLink + "', '" + child.KinshipT + "', '" + child.MainPersonInChargeID + "')";
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





        //this web API WILL BE called when we create childe with card by his phone
        [HttpPost("createChildAccount")]  
        public ActionResult CreateChildAccount(IFormFile MainImagePath , [FromForm] string username, [FromForm] string UserType, [FromForm] string FullName, [FromForm] string Password, [FromForm] int? PhoneNumber, [FromForm] string Gender, [FromForm] string Email, [FromForm] string usernameType, [FromForm] int YearOfbirth)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
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

            int useridforchild = DatabaseSettings.getID(username);

            if (user != null)
                // Check if the MainImagePath and model state are valid
                if (MainImagePath != null && ModelState.IsValid)
                {

                    // Generate the image file name
                    string imageFileName = useridforchild.ToString();

                    //add extension to image
                    imageFileName += Path.GetExtension(MainImagePath.FileName).ToLower();

                    // Get the image folder path
                    string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DatabaseSettings.ImageDirectory_AddPath));

                    // Save the uploaded image to the specified file path
                    using (var fileStream = new FileStream(Path.Combine(imageFolderPath, imageFileName), FileMode.Create))

                        MainImagePath.CopyToAsync(fileStream);


                    //generate link qrcode 
                    string linkqrcode = QrCodeController.GenerateAndStoreQRCode(useridforchild);

                    //generate link verification code 
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
                    ActionResult<PersonChilds> child = AddChildacc(childTemp);

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

        //to add child in personchild table
        public static ActionResult<PersonChilds> AddChildacc(PersonChilds child)
        {

            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            string sql = "insert into PersonChilds(ChildID , YearOfbirth, mainImagePath,VerificationCode , QRCodeLink)values ('" + child.ChildID + "', '" + child.YearOfBirth + "', '" + child.MainImagePath + "', '" + child.VerificationCode + "', '" + child.QRCodeLink  + "')";
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


        //generate random verification code which represent url to object https://localhost:7111/api/child/22,22222
        public static int GetRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000); // Generate a random number between 1000 and 9999
            return randomNumber;
        }




    








    } 
    }







            
       
    
