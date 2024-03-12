using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildActionController : ControllerBase
    {
        [HttpPost("AddChild")]
        public IActionResult AddChild([FromBody] AddChild child)
        {
            var conn = DatabaseSettings.dbConn;
            try
            {
                conn.Open();

                string sqlchildadd = "INSERT INTO PersonUsers (Username, UserType, FullName, Password, PhoneNumber, Gender, Email, UsernameType) VALUES (@Username, @UserType, @FullName, @Password, @PhoneNumber, @Gender, @Email, 'Email')";
                using (SqlCommand cmd = new SqlCommand(sqlchildadd, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", child.Username);
                    cmd.Parameters.AddWithValue("@UserType", child.UserType);
                    cmd.Parameters.AddWithValue("@FullName", child.FullName);
                    cmd.Parameters.AddWithValue("@Password", child.Password);
                    cmd.Parameters.AddWithValue("@PhoneNumber", child.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Gender", child.Gender);
                    cmd.Parameters.AddWithValue("@Email", child.Email);
                    // Execute the SQL query
                    cmd.ExecuteNonQuery();
                }

                string sql2 = "SELECT UserID FROM PersonUsers WHERE Username = @Username";
                int UserID;
                using (SqlCommand cmd2 = new SqlCommand(sql2, conn))
                {
                    cmd2.Parameters.AddWithValue("@Username", child.Username);
                    SqlDataReader reader = cmd2.ExecuteReader();
                    if (reader.Read())
                    {
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
                    }
                    else
                    {
                        reader.Close();
                        return NotFound("User not found");
                    }
                    reader.Close();
                }

                string sql3 = "INSERT INTO PersonChilds (ChildID, KinshipT, YearOfBirth) VALUES (@ChildID, @KinshipT, @YearOfBirth)";
                using (SqlCommand cmd3 = new SqlCommand(sql3, conn))
                {
                    cmd3.Parameters.AddWithValue("@ChildID", UserID);
                    cmd3.Parameters.AddWithValue("@KinshipT", child.KinshipT);
                    cmd3.Parameters.AddWithValue("@YearOfBirth", child.YearOfBirth);
                    // Execute the SQL query
                    cmd3.ExecuteNonQuery();
                }

                string sql4 = "INSERT INTO personChilds_Images (ChildID, ImagePath) VALUES (@ChildID, @ImagePath)";
                using (SqlCommand cmd4 = new SqlCommand(sql4, conn))
                {
                    cmd4.Parameters.AddWithValue("@ChildID", UserID);
                    cmd4.Parameters.AddWithValue("@ImagePath", child.ImagePath);
                    // Execute the SQL query
                    cmd4.ExecuteNonQuery();
                }

                return Ok("Successfully added");
            }
            catch (Exception ex)
            {
                // Handle the exception here, log it or perform any necessary actions
                return StatusCode(500, ex);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
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


    }
}
