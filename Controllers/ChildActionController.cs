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
        //link child to thier parent by child mobile . 
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



        //to get children information and display result in home list 
        [HttpGet("ChildHome/{UserID}")]
        public ActionResult<string> Getchild(int UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            //query 
            string sql = "SELECT ch.ChildId, ch.mainImagePath, kinshipT, pu.FullName AS childName, pu.Gender, ch.YearOfBirth, pr.PhoneNumber AS parentnumber, ch.isConnect, ch.Boundry, ch.Longitude, ch.Latitude ,fc.TrackingActiveType , fc.AllowTorack  FROM PersonChilds ch, PersonUsers pu , FollowChilds fc WHERE MainPersonInChargeID = @UserID AND (ch.ChildID = pu.UserID) AND MainPersonInChargeID = pr.UserID  AND  fc.PersonInChargeID = @UserID AND fc.ChildID = ch.ChildID ";

            SqlCommand Comm = new SqlCommand(sql, conn);
            Comm.Parameters.AddWithValue("@UserID", UserID);

            SqlDataReader reader = Comm.ExecuteReader();

            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.AppendLine("{"); // Start of JSON object

            while (reader.Read())
            {
                float? latitude = null;
                if (!reader.IsDBNull(reader.GetOrdinal("Latitude")))
                {
                    latitude = (float)reader.GetDouble(reader.GetOrdinal("Latitude"));
                }

                float? longitude = null;
                if (!reader.IsDBNull(reader.GetOrdinal("Longitude")))
                {
                    longitude = (float)reader.GetDouble(reader.GetOrdinal("Longitude"));
                }

                resultBuilder.AppendLine($"\"child{reader.GetInt32(reader.GetOrdinal("ChildId"))}\": {{"); // Start of child object with unique key

                resultBuilder.AppendLine($"\"mainImagePath\": \"{reader.GetString(reader.GetOrdinal("mainImagePath"))}\",");
                resultBuilder.AppendLine($"\"kinshipT\": \"{reader.GetString(reader.GetOrdinal("kinshipT"))}\",");
                resultBuilder.AppendLine($"\"childName\": \"{reader.GetString(reader.GetOrdinal("childName"))}\",");
                resultBuilder.AppendLine($"\"gender\": \"{reader.GetString(reader.GetOrdinal("Gender"))}\",");
                resultBuilder.AppendLine($"\"yearOfBirth\": {reader.GetInt32(reader.GetOrdinal("YearOfBirth"))},");
                resultBuilder.AppendLine($"\"boundry\": {reader.GetInt32(reader.GetOrdinal("Boundry"))},");
                resultBuilder.AppendLine($"\"parentnumber\": \"{reader.GetInt32(reader.GetOrdinal("parentnumber"))}\",");
                resultBuilder.AppendLine($"\"isConnect\": \"{reader.GetString(reader.GetOrdinal("isConnect"))}\",");
                resultBuilder.AppendLine($"\"latitude\": {(latitude.HasValue ? latitude.ToString() : "null")},");
                resultBuilder.AppendLine($"\"longitude\": {(longitude.HasValue ? longitude.ToString() : "null")}");

                resultBuilder.AppendLine("}},"); // End of child object
            }

            conn.Close();

            if (resultBuilder[resultBuilder.Length - 3] == ',')
            {
                resultBuilder.Remove(resultBuilder.Length - 3, 1); // Remove the trailing comma
            }

            resultBuilder.AppendLine("}"); // End of JSON object

            if (resultBuilder.Length > 2)
            {
                return Ok(resultBuilder.ToString());
            }

            return NotFound("Link your children");
        }



        //to get children information and display result in home list 
        [HttpGet("testChildHome/{UserID}")]
        public ActionResult<IEnumerable<object>> testGetchild(int UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            //query 
            // string sql = "SELECT ch.ChildId, ch.mainImagePath, kinshipT, pu.FullName AS childName, pu.Gender, ch.YearOfBirth, pr.PhoneNumber AS parentnumber, ch.isConnect, ch.Boundry, ch.Longitude, ch.Latitude ,fc.TrackingActiveType , fc.AllowTorack  FROM PersonChilds ch, PersonUsers pu , FollowChilds fc WHERE MainPersonInChargeID = @UserID AND (ch.ChildID = pu.UserID) AND MainPersonInChargeID = pr.UserID  AND  fc.PersonInChargeID = @UserID AND fc.ChildID = ch.ChildID ";

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



        //update is connect then add location to volunteerchild table
        //update child location each 1 minute will call this API 
        [HttpPost("UpdateandinsertlastlocationforChild")]
        public IActionResult UpdateandinsertlastlocationforChild(int childID, decimal Latitude, decimal Longitude, string DevicesuppliedType)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {

                    string insertQuery = "INSERT INTO volunteerHistoricalLocation (PersonID, dateTime, Longitude,Latitude , DevicesuppliedType) VALUES (@childID, @dateTime, @Longitude, @Latitude , @DevicesuppliedType)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);
                    insertCommand.Parameters.AddWithValue("@childID", childID);
                    insertCommand.Parameters.AddWithValue("@dateTime", currentDateTime);
                    insertCommand.Parameters.AddWithValue("@Longitude", Longitude);
                    insertCommand.Parameters.AddWithValue("@Latitude", Latitude);
                    insertCommand.Parameters.AddWithValue("@DevicesuppliedType", DevicesuppliedType);
                    insertCommand.ExecuteNonQuery();


                    string sql = "UPDATE PersonChilds SET Longitude = @Longitude , Latitude = @Latitude , VoulnteerChildLocationID = (select TOP 1 volunteerLocationId from volunteerHistoricalLocation where PersonID = @ChildID ORDER BY dateTime DESC)  WHERE ChildID = @ChildID";
                    SqlCommand command = new SqlCommand(sql, conn , transaction);
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@Longitude", Longitude);
                    command.Parameters.AddWithValue("@Latitude", Latitude);
                    command.ExecuteNonQuery();
                    transaction.Commit();

                    return Ok();

                }
            }
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
        public class applinktype
        {
            public int VerificationCode { get; set; }
            public string QRCodeLink { get; set; }
        }

    }



}

    



    

