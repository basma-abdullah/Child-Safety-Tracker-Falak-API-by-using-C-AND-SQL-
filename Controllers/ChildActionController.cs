using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using static YourNamespace.Controllers.QrCodeController;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildActionController : ControllerBase
    {
        [HttpPut]
        public IActionResult linklbyapplication(int userID , string childUserName , string childPassword) 
        {
            var conn = DatabaseSettings.dbConn;
            
            UserController userController = new UserController();
            IActionResult loginResult = userController.login(childUserName, childPassword);
            int childID=-1;
            if (loginResult is OkObjectResult okResult && okResult.Value is List<PersonUsers> persons && persons.Count > 0)
            {
                conn.Open();
                childID = persons[0].UserID;
                string sql = "UPDATE PersonChilds SET MainPersonInChargeID = @userID WHERE ChildID = @childID";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@userID", userID);
                command.Parameters.AddWithValue("@childID", childID);
                command.ExecuteNonQuery();
                conn.Close();
            }
            if (childID != -1)
            {
                var childInfo = GetChildInformation(childID, userID);
                var parentInfo = GetParentInformation(userID);

                dynamic mergedInfo = new ExpandoObject();
                mergedInfo.ChildInfo = childInfo;
                mergedInfo.ParentInfo = parentInfo;

                return Ok(mergedInfo);
            }
            else
            {
                conn.Close();
                return BadRequest("User not created");
            }
            
        }

        private ChildInformation GetChildInformation(int childId, int userId)
        {
            using (var connection = DatabaseSettings.dbConn)
            {
                connection.Open();

                string query = $"SELECT p.FullName, c.YearOfBirth, p.Gender, c.AdditionalInformation, c.KinshipT, c.Boundry, c.MainImagePath " +
                               $"FROM PersonUsers P, PersonChilds c " +
                               $"WHERE c.childID = @childId AND c.MainPersonInChargeID = @userId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@childId", childId);
                    command.Parameters.AddWithValue("@userId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve the child information from the reader
                            var fullName = reader.GetString(reader.GetOrdinal("FullName"));
                            var yearOfBirth = reader.GetInt32(reader.GetOrdinal("YearOfBirth"));
                            var gender = reader.GetString(reader.GetOrdinal("Gender"));
                            var additionalInformation = reader.GetString(reader.GetOrdinal("AdditionalInformation"));
                            var KinshipT = reader.GetString(reader.GetOrdinal("KinshipT"));
                            var Boundry = reader.GetInt32(reader.GetOrdinal("Boundry"));
                            var MainImagePath = reader.GetString(reader.GetOrdinal("MainImagePath"));
                            return new ChildInformation
                            {
                                FullName = fullName,
                                YearOfBirth = yearOfBirth,
                                Gender = gender,
                                AdditionalInformation = additionalInformation,
                                KinshipT = KinshipT,
                                Boundry = Boundry,
                                MainImagePath = MainImagePath
                            };
                        }
                    }
                }
            }

            return null;
        }

        private ParentInformation GetParentInformation(int userId)
        {
            using (var connection = DatabaseSettings.dbConn )
            {
                connection.Open();

                string query = $"SELECT u.PhoneNumber " +
                               $"FROM PersonUsers u " +
                               $"WHERE u.UserID = @userId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve the parent information from the reader
                           
                            var phoneNumber = reader.GetInt32(reader.GetOrdinal("PhoneNumber"));

                            return new ParentInformation
                            {
                                
                                PhoneNumber = phoneNumber
                            };
                        }
                    }
                }
            }

            return null;
        }

        [HttpGet("ChildHome/{Username}")]
        public IActionResult Getchild(string Username)
        {
            var conn = DatabaseSettings.dbConn;
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




        [HttpGet("ChildProfile/{childID}")]
        public IActionResult ChildProfile(string childID)
        {
            var conn = DatabaseSettings.dbConn;

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

        [HttpGet("link/{Username},{Password}")]
        public IActionResult link(string Username, string Password)
        {
            var conn = DatabaseSettings.dbConn;
            conn.Open();
            string sql = "SELECT * FROM PersonUsers WHERE Username = '" + Username + "' AND Password = '" + Password + "'";
            SqlCommand Comm = new SqlCommand(sql, conn);
            //query 

            SqlDataReader reader = Comm.ExecuteReader();

            if (reader.Read())
            {

                reader.Close();
                conn.Close();

                return Ok(reader);
            }
            reader.Close();
            conn.Close();
            return NotFound("child not found");
        }

        /* [HttpPut("TrackingType/")]
      public IActionResult TrackingType()
      {
          var conn = PersonUserController.dbConn;
          conn.Open();
          string sql = "UPDATE PersonUsers SET Password = '" + Password + "' WHERE Username = '" + Username + "'";
          SqlCommand Command = new SqlCommand(sql, conn);
          int rowsAffected = Command.ExecuteNonQuery();
          conn.Close();
          if (rowsAffected > 0)
          {
              return Ok("updated");
          }

          return NotFound("user not found");
      }*/


        public class ChildInformation
        {
            public string FullName { get; set; }
            public int YearOfBirth { get; set; }
            public string Gender { get; set; }
            public string AdditionalInformation { get; set; }
            public string KinshipT {  get; set; }
            public int Boundry { get; set; }
            public string MainImagePath { get; set; }
        }

        public class ParentInformation
        {
            public int PhoneNumber { get; set; }
        }

    }

}
