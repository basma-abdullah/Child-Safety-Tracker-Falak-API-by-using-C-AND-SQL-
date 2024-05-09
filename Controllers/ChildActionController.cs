using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;
using static QRCoder.PayloadGenerator.SwissQrCode;


//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static QRCodes.Controllers.QrCodeController;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildActionController : ControllerBase
    {


        //to get children information and display result in home list 
        [HttpGet("ListChildHome/{UserID}")]
        public ActionResult<IEnumerable<object>> ListChildHome(int UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            string sql = "SELECT ch.ChildId, ch.mainImagePath, kinshipT, pu.FullName AS childName, pu.Gender, ch.YearOfBirth, pr.PhoneNumber AS parentnumber, ch.isConnect, ch.Boundry, ch.Longitude, ch.Latitude, fc.TrackingActiveType, fc.AllowTorack " +
                         "FROM PersonChilds ch " +
                         "JOIN PersonUsers pr ON ch.MainPersonInChargeID = pr.UserID " +
                         "JOIN PersonUsers pu ON ch.ChildID = pu.UserID " +
                         "JOIN FollowChilds fc ON ch.ChildID = fc.ChildID " +
                         "WHERE ch.MainPersonInChargeID = @UserID and fc.PersonInChargeID = @UserID ";

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
                    isConnect = reader.GetString(reader.GetOrdinal("isConnect")),
                    Latitude = Latitude,
                    Longitude = longitude,
                    TrackingActiveType = reader.GetString(reader.GetOrdinal("TrackingActiveType")),
                    AllowTorack = reader.GetString(reader.GetOrdinal("AllowTorack")),
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

        
        [HttpGet("GetChildLinkQRcode")]
        public ActionResult<applinktype> GetChildLinkQRcode(int ChildID)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    string sql = "SELECT VerificationCode, QRCodeLink FROM PersonChilds WHERE ChildID = @ChildID";
                    SqlCommand command2 = new SqlCommand(sql, conn, transaction);
                    command2.Parameters.AddWithValue("@ChildID", ChildID);
                    SqlDataReader reader = command2.ExecuteReader();

                    if (reader.Read())
                    {
                        applinktype applinktype = new applinktype
                        {
                            QRCodeLink = reader.GetString(reader.GetOrdinal("QRCodeLink")),
                            VerificationCode = reader.GetInt32(reader.GetOrdinal("VerificationCode"))
                        };

                        reader.Close();
                        return applinktype;
                    }
                    else
                    {
                        // Handle the case when no data is found for the given ChildID
                        // Return an appropriate response, such as NotFound or BadRequest
                        return NotFound();
                    }
                }
            }
        }
        // update child info 
        [HttpPut("updatechildinfo")]
        public async Task<ActionResult<object>> updatechildinfo (IFormFile CurrentImagePath, [FromForm] int ChildID, [FromForm] string FullName, [FromForm] string Gender, [FromForm] string KinshipT, [FromForm] int YearOfBirth, [FromForm] int Boundry, [FromForm] string AdditionalInformation)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {

                    string updateQuery1 = "UPDATE PersonUsers SET FullName = @FullName, Gender = @Gender where UserID =@ChildID";
                    SqlCommand updateQuery = new SqlCommand(updateQuery1, conn, transaction);
                    updateQuery.Parameters.AddWithValue("@ChildID", ChildID);
                    updateQuery.Parameters.AddWithValue("@FullName", FullName);
                    updateQuery.Parameters.AddWithValue("@Gender", Gender);
                    updateQuery.ExecuteNonQuery();



                    string updateQuery2 = "UPDATE PersonChilds SET KinshipT = @KinshipT, YearOfBirth = @YearOfBirth , Boundry = @Boundry , todayimagePath = @CurrentImagePath, AdditionalInformation = @AdditionalInformation where ChildID =@ChildID ";
                    SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);
                    // Check if the responseImagePath and model state are valid
                    if (CurrentImagePath != null && ModelState.IsValid)
                    {
                        // Generate the image file name
                        string imageFileName = $"{DateTime.Now:yyyyMMddHH}_{new Random().Next(1000, 9999)}";

                        // Add extension to image
                        imageFileName += Path.GetExtension(CurrentImagePath.FileName).ToLower();

                        // Get the image folder path
                        string imageFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), DatabaseSettings.ImageDirectory_AddPath));

                        // Save the uploaded image to the specified file path
                        using (var fileStream = new FileStream(Path.Combine(imageFolderPath, imageFileName), FileMode.Create))
                        {
                            await CurrentImagePath.CopyToAsync(fileStream);
                        }
                        command2.Parameters.AddWithValue("@ChildID", ChildID);
                        command2.Parameters.AddWithValue("@KinshipT", KinshipT);
                        command2.Parameters.AddWithValue("@YearOfBirth", YearOfBirth);
                        command2.Parameters.AddWithValue("@Boundry", Boundry);
                        command2.Parameters.AddWithValue("@AdditionalInformation", AdditionalInformation);
                        command2.Parameters.AddWithValue("@CurrentImagePath", DatabaseSettings.ImageDirectory_ReadPath + "/" + imageFileName);
                        command2.ExecuteNonQuery();
                        transaction.Commit();

                        int affectedRow = command2.ExecuteNonQuery();
                        if (affectedRow > 0)
                        {
                            return Ok("successfully update");
                        }
                        else
                        {
                            return BadRequest("not update");
                        }
                    }

                    return BadRequest("not update");
                }
            }
        }
        





        public class applinktype
        {
            public int VerificationCode { get; set; }
            public string QRCodeLink { get; set; }
        }





    }



}

    



    

