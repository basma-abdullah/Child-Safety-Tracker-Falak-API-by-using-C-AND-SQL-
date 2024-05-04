using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static QRCodes.Controllers.QrCodeController;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildActionController : ControllerBase
    {
        //link child to thier parent 
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



        //to get children information and display result in home list 
        [HttpGet("ChildHome/{UserID}")]
        public ActionResult<IEnumerable<object>> Getchild(int UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            //query 
            string sql = "select ch.ChildId ,ch.mainImagePath, kinshipT ,pu.FullName as childName,pu.Gender, ch.YearOfBirth ,pr.PhoneNumber as parentnumber , ch.Boundry , ch.Longitude, ch.Latitude from PersonChilds ch ,PersonUsers pu , PersonUsers pr where MainPersonInChargeID = @UserID AND (ch.ChildID = pu.UserID) AND MainPersonInChargeID = pr.UserID";
            

            SqlCommand Comm = new SqlCommand(sql, conn);
            Comm.Parameters.AddWithValue("@UserID", UserID);

            SqlDataReader reader = Comm.ExecuteReader();

            List<object> childsprofile = new List<object>();
            while (reader.Read())
            {
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
                var child = new
                {
                    childid = reader.GetInt32(reader.GetOrdinal("ChildID")),
                    MainImagePath = reader.GetString(reader.GetOrdinal("MainImagePath")),
                    kinshipT = reader.GetString(reader.GetOrdinal("kinshipT")),
                    childName = reader.GetString(reader.GetOrdinal("childName")),
                    Gender = reader.GetString(reader.GetOrdinal("Gender")),
                    YearOfBirth = reader.GetInt32(reader.GetOrdinal("YearOfBirth")),
                    Boundry = reader.GetInt32(reader.GetOrdinal("Boundry")),
                    parentnumber = reader.GetInt32(reader.GetOrdinal("parentnumber")),
                    Latitude = Latitude,
                    Longitude = longitude,
            };
                

                childsprofile.Add(child);
            }

            conn.Close();

            if (childsprofile.Count > 0)
            {
                return Ok(childsprofile);
            }

            return NotFound("Link your children");
        }


        [HttpPut("updateChildLocation")]
        public IActionResult updateChildLocation(int childID, float Longitude , float Latitude)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "UPDATE PersonChilds SET Longitude = @Longitude , Latitude = @Latitude WHERE ChildID = @ChildID";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@ChildID", childID);
                command.Parameters.AddWithValue("@Longitude", Longitude);
                command.Parameters.AddWithValue("@Latitude", Latitude);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
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

    }


