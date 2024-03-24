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
        [HttpPut ("LinkChildByApplication")]
        public IActionResult linkchild([FromForm] int parentuserid, [FromForm] int childid, [FromForm] string kinshipT, [FromForm]  int Boundry, [FromForm] string AdditionalInformation)
        {
            int affectedRows = 0;
            bool insertfollowchild = false;
            if (DatabaseSettings.isIdExists(parentuserid) && DatabaseSettings.isIdExists(childid))
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    string sql = "UPDATE PersonChilds SET MainPersonInChargeID = @UserID, kinshipT = @KinshipT, Boundry = @Boundry, AdditionalInformation = @AdditionalInformation WHERE ChildID = @ChildID";
                    ;
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
                // 
                return Ok("link success");
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
                string sql = "SELECT * FROM PersonChilds WHERE ChildID = @ChildID AND VerificationCode = @VerificationCode";
                SqlCommand Comm = new SqlCommand(sql, conn);
                Comm.Parameters.AddWithValue("@ChildID", ChildID);
                Comm.Parameters.AddWithValue("@VerificationCode", VerificationCode);
                SqlDataReader reader = Comm.ExecuteReader();

                if (reader.Read())
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
        [HttpGet("ChildHome/{Username}")]
        public IActionResult Getchild(string Username)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();


            string sql = "SELECT PC.ChildID, PC.MainImagePath, PU.FullName FROM PersonChilds PC JOIN PersonUsers PU ON PC.ChildID = PU.UserID WHERE PC.ChildID = ChildID";
            SqlCommand Comm = new SqlCommand(sql, conn);
            Comm.Parameters.AddWithValue("@Username", Username);

            SqlDataReader reader = Comm.ExecuteReader();

            if (reader.Read())
            {
                var child = new
                {
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    MainImagePath = reader.GetString(reader.GetOrdinal("MainImagePath")) 
                };
                conn.Close();
                return Ok(child);
            }
            conn.Close();
            return NotFound("Link your children");


        }



        //to get more deteail information when click on specific child 
        [HttpGet("ChildProfile/{childID}")]
        public IActionResult ChildProfile(string childID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);

            conn.Open();

            string sql = @"SELECT
                    PU.FullName AS FullName,
                    PC.YearOfBirth AS YearOfBirth,
                    PC.MainImagePath AS MainImagePath,
                    PU.PhoneNumber AS PhoneNumber,
                    PC.QRCode AS QRCode,
                    PC.Boundry AS Boundary,
                    D.DeviceBattery AS DeviceBattery,
                    FC.TrackingActiveType AS TrackingActiveType,
                    LNR.LostNotificationRequestId AS LostNotificationRequestId
                FROM
                    PersonChilds PC
                    INNER JOIN PersonUsers PU ON PC.ChildID = PU.UserID
                    LEFT JOIN Devices D ON D.ChildID = PC.ChildID
                    LEFT JOIN FollowChilds FC ON FC.ChildId = PC.ChildID
                    LEFT JOIN LostNotificationRequest LNR ON LNR.TrackingChildMasterID = PC.ChildID
                WHERE
                    PC.ChildID = @ChildID";

            using (SqlCommand comm = new SqlCommand(sql, conn))
            {
                comm.Parameters.AddWithValue("@ChildID", childID);

                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var childProfile = new
                        {
                            MainImagePath = reader.IsDBNull(reader.GetOrdinal("MainImagePath")) ? null : reader.GetString(reader.GetOrdinal("MainImagePath")),
                            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                            YearOfBirth = reader.IsDBNull(reader.GetOrdinal("YearOfBirth")) ? default(DateTime) : reader.GetDateTime(reader.GetOrdinal("YearOfBirth")),
                            TrackingActiveType = reader.IsDBNull(reader.GetOrdinal("TrackingActiveType")) ? null : reader.GetString(reader.GetOrdinal("TrackingActiveType")),
                            DeviceBattery = reader.IsDBNull(reader.GetOrdinal("DeviceBattery")) ? default(int) : reader.GetInt32(reader.GetOrdinal("DeviceBattery")),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? default(int) : reader.GetInt32(reader.GetOrdinal("PhoneNumber")),
                            QRCode = reader.IsDBNull(reader.GetOrdinal("QRCode")) ? default(string) : reader.GetString(reader.GetOrdinal("QRCode")),
                            Boundary = reader.IsDBNull(reader.GetOrdinal("Boundary")) ? default(int) : reader.GetInt32(reader.GetOrdinal("Boundary")),
                            LostNotificationRequestId = reader.IsDBNull(reader.GetOrdinal("LostNotificationRequestId")) ? default(int) : reader.GetInt32(reader.GetOrdinal("LostNotificationRequestId"))
                        };

                        reader.Close();
                        conn.Close();
                        return Ok(childProfile);
                    }
                    else
                    {
                        reader.Close();
                        conn.Close();
                        return NotFound("Empty");
                    }
                }
            }

   
        }


    
        }

    }


