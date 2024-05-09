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

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        //link by app 
        [HttpPut("LinkChildByApplication")]
        public IActionResult linkchild([FromForm] int parentuserid, [FromForm] int childid, [FromForm] string kinshipT, [FromForm] int Boundry, [FromForm] string AdditionalInformation)
        {
            int affectedRows = 0;
            bool insertfollowchild = false;
            bool isMainPersonInChargeIDExists = DatabaseSettings.isMainPersonInChargeIDExists(childid);

            if (!isMainPersonInChargeIDExists)
            {
                if (DatabaseSettings.isIdExists(parentuserid) && DatabaseSettings.isIdExists(childid))
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                    {

                        string sql = "UPDATE PersonChilds SET MainPersonInChargeID = @UserID, kinshipT = @KinshipT, Boundry = @Boundry, AdditionalInformation = @AdditionalInformation WHERE ChildID = @ChildID";
                        using (SqlCommand command = new SqlCommand(sql, conn))
                        {
                            command.Parameters.AddWithValue("@UserID", parentuserid);
                            command.Parameters.AddWithValue("@ChildID", childid);
                            command.Parameters.AddWithValue("@KinshipT", kinshipT);
                            command.Parameters.AddWithValue("@Boundry", Boundry);
                            command.Parameters.AddWithValue("@AdditionalInformation", AdditionalInformation);

                            conn.Open();
                            affectedRows = command.ExecuteNonQuery();
                        }

                        insertfollowchild = SettingController.insertorupdateAppMethod(childid, parentuserid);
                    }
                }
                if (affectedRows > 0 && insertfollowchild)
                {
                    return Ok("link success");
                }
                else
                {

                    return BadRequest("not linked");

                }

            }

            else if (isMainPersonInChargeIDExists)
            {
                return BadRequest("You cannot link the child. Try requesting tracking permission ");
            }
            else
            {
                return BadRequest("not linked");
            }

        }



        //before implement link by app APP this will verify from verification code if it is true then the link by app API will be invoke 
        //قبل ما ينفذ الربط بالجوال راح يتاكد من رقم الربط اذا كان صح يستدعي الربط بالجوال اذا كان خطأ ما يضيف ولا يربط 
        //link by verification code will be invoked when user enter 4  digit code for link by application
        [HttpGet("verify_verification_code")]
        public IActionResult verify_verification_code(int ChildID, int VerificationCode)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "SELECT * FROM PersonChilds WHERE ChildID = @ChildID ";
                SqlCommand Comm = new SqlCommand(sql, conn);
                Comm.Parameters.AddWithValue("@ChildID", ChildID);

                SqlDataReader reader = Comm.ExecuteReader();
                int verify = reader.GetInt32(reader.GetOrdinal("VerificationCode"));
                if (reader.Read() && verify == VerificationCode)
                {
                    reader.Close();
                    return Ok();
                }
                else
                {
                    reader.Close();
                    return BadRequest();
                }
            }
        }


        //link by oq device :
        [HttpPost("linkbydevice")]
        public IActionResult linkbydevice([FromForm] int UserID, [FromForm] int childID, [FromForm] int serialNumber, [FromForm] int Version, [FromForm] int DeviceBattery, [FromForm] string kinshipT, [FromForm] int Boundry, [FromForm] string AdditionalInformation)
        {
            bool isserialnumberexist = SerialNumberexist(serialNumber);
            if (isserialnumberexist == false)
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        string insertQuery = "INSERT INTO Devices (ModelTypeID, SerialNumber, Version ,DeviceBattery , ChildID) VALUES ((select DeviceTypeID from deviceModeltype where ModelType = 'OQ'  ), @SerialNumber, @Version ,@DeviceBattery , @ChildID)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);
                        insertCommand.Parameters.AddWithValue("@SerialNumber", serialNumber);
                        insertCommand.Parameters.AddWithValue("@Version", Version);
                        insertCommand.Parameters.AddWithValue("@DeviceBattery", DeviceBattery);
                        insertCommand.Parameters.AddWithValue("@ChildID", childID);
                        insertCommand.ExecuteNonQuery();

                        int affectedRows = 0;
                        string sql = "UPDATE PersonChilds SET MainPersonInChargeID = @UserID, kinshipT = @KinshipT, Boundry = @Boundry, AdditionalInformation = @AdditionalInformation WHERE ChildID = @ChildID";
                        SqlCommand command = new SqlCommand(sql, conn, transaction);

                        command.Parameters.AddWithValue("@UserID", UserID);
                        command.Parameters.AddWithValue("@ChildID", childID);
                        command.Parameters.AddWithValue("@KinshipT", kinshipT);
                        command.Parameters.AddWithValue("@Boundry", Boundry);
                        command.Parameters.AddWithValue("@AdditionalInformation", AdditionalInformation);
                        affectedRows = command.ExecuteNonQuery();

                        transaction.Commit();
                        bool insertfollowchild = SettingController.insertorupdateDeviceMethod(childID, UserID);

                        if (insertfollowchild && affectedRows > 0)
                        {
                            return Ok();
                        }
                        else
                        {
                            return BadRequest();
                        }


                    }
                }
            }
            else
            {
                return BadRequest("This device is already connected");
            }
        }



        //to check if serail number exist or not 
        public static bool SerialNumberexist(int SerialNumber)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                string sql = "SELECT * FROM Devices WHERE SerialNumber = @SerialNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", SerialNumber);
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




        //link by genrate qrcode

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


            UserController user1 = new UserController();

            ActionResult<PersonUsers> user =user1.signup(userTemp);
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
                    string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DatabaseSettings.ImageDirectory_AddPath));

                    // Save the uploaded image to the specified file path
                    using (var fileStream = new FileStream(Path.Combine(imageFolderPath, imageFileName), FileMode.Create))

                        MainImagePath.CopyToAsync(fileStream);




                    //generate link qrcode for child to be use later on link process
                    string linkqrcode = QrCodeController.GenerateAndStoreQRCode(useridforchild);

                    //generate link number for child to be use later on link process
                    int verficationCode = UserController.GetRandomNumber();




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
                        bool insertfollow = SettingController.insertHasCardMethod(useridforchild, MainPersonInChargeID, false, false, "hascard");
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


    }
}
