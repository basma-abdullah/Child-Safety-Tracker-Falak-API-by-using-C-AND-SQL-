using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        //personal information API 
        //change phonenumber 
        [HttpPut("changeEmailAndName/{UserID}")]
        public IActionResult changeEmailAndName(int UserID, string Email, string FullName)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                Boolean isexist = DatabaseSettings.isIdExists(UserID);
                if (isexist == true)
                {
                    conn.Open();
                    string sql = "UPDATE PersonUsers SET FullName = @FullName, Email = @Email WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // Add parameters and their values
                        cmd.Parameters.AddWithValue("@FullName", FullName);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@UserID", UserID);

                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            return Ok("successfully updated");
                        }
                        else
                        {
                            return NotFound("Error not updated");
                        }
                    }
                }
                else
                {
                    return NotFound("user not found");
                }
            }

        }

        //change phonenumber 
        [HttpPut("changePhoneNumber/{UserID}")]
        public IActionResult changePhoneNumber(int UserID, string PhoneNumber)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();

                Boolean isexist = DatabaseSettings.isIdExists(UserID);
                if (isexist)
                {
                    string sql = "UPDATE PersonUsers SET PhoneNumber = @PhoneNumber, Username = @Username WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {

                        // Add parameters and their values
                        cmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        cmd.Parameters.AddWithValue("@Username", PhoneNumber);
                        cmd.Parameters.AddWithValue("@UserID", UserID);

                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            conn.Close();
                            return Ok("successfully updated");
                        }
                        else
                        {
                            return NotFound("Error not updated");
                        }
                    }
                }
                else
                {
                    return NotFound("user not found");
                }
            }
        } 
    



        //reset password API 
        [HttpPut("reset_password/{UserID}")]
        public IActionResult reset_password(int UserID, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                Boolean isexist = DatabaseSettings.isIdExists(UserID);
                if (isexist)
                {
                    string sql = "UPDATE PersonUsers SET Password = '" + newPassword + "' WHERE UserID = '" + UserID + "'";
                    using (SqlCommand Command = new SqlCommand(sql, conn))
                    {
                        Command.ExecuteNonQuery();
                    }
                    return Ok("Successfully updated");
                }
                else
                {
                    return NotFound("not updated user not found");
                }
            }


        }



        // to delete or unlink child from parent
        [HttpPut("unlinkchild/{ChildID}")]
        public IActionResult unlinkchild(int ChildID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                Boolean isexist = DatabaseSettings.isIdExists(ChildID);
                if (isexist == true)
                {
                    string sql = "UPDATE PersonChilds SET MainPersonInChargeID = NULL , KinshipT = NULL  WHERE ChildID = @ChildID";
                    SqlCommand command = new SqlCommand(sql, conn);

                    command.Parameters.AddWithValue("@ChildID", ChildID);

                    command.ExecuteNonQuery();
                    conn.Close();
                    return Ok("Sucessfully deleted");
                }

                conn.Close();
                return NotFound("error");


            }
        }




  





        //get all child name with active tracking type that link by particular parent
        [HttpGet("ChildMangment/{UserID}")]
        public IActionResult GetchildsAndActiveTrackingType(string UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            string sql = "SELECT PC.ChildID, CN.FullName, FC.TrackingActiveType "+
                         "FROM PersonChilds PC "+
                         "JOIN PersonUsers PU ON PC.MainPersonInChargeID = PU.UserID "+
                         "JOIN PersonUsers CN ON PC.ChildID = CN.UserID "+
                         "JOIN FollowChilds FC ON PC.ChildID = FC.ChildID "+
                         "WHERE PC.MainPersonInChargeID = @UserID ";

            SqlCommand command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@UserID", UserID);

            SqlDataReader reader = command.ExecuteReader();

            List<object> children = new List<object>();

            while (reader.Read())
            {
                var child = new
                {
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    ChildID = reader.GetInt32(reader.GetOrdinal("ChildID")),
                    TrackingActiveType = reader.GetString(reader.GetOrdinal("TrackingActiveType")),
                };

                children.Add(child);
            }

            conn.Close();

            if (children.Count > 0)
            {
                return Ok(children);
            }

            return NotFound("Link your children");
        }




        //to get a list of all available tracking method for one child 
        [HttpGet("TrackingOption")]
        public ActionResult<IEnumerable<string>> AvailableTrackingType(int userID, int childID)
        {
            List<string> trackingOption = new List<string>();
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "SELECT FC.TrackByApp, FC.TrackByDevice " +
                             "FROM FollowChilds FC , PersonChilds PC " +
                             "WHERE FC.ChildId = PC.ChildID  AND FC.PersonInChargeID = @UserID and FC.ChildId = @ChildID";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@UserID", userID);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        string app = reader.GetString(reader.GetOrdinal("TrackByApp"));
                        string device = reader.GetString(reader.GetOrdinal("TrackByDevice"));

                        if (app == "1" && device == "1")
                        {
                            trackingOption.Add("app");
                            trackingOption.Add("device");
                        }
                        else if (app == "1" && device == "0")
                        {
                            trackingOption.Add("app");
                        }
                        else if (app == "0" && device == "1")
                        {
                            trackingOption.Add("device");
                        }
                    }
                    trackingOption.Add("hascard");
                }
            }

            return Ok(trackingOption);
        }


        [HttpPut("updateDefualtTrackingMethod")]
        public IActionResult updateDefualtTrackingMethod(int UserID , int ChildID , string TrackingActiveType)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "UPDATE FollowChilds SET TrackingActiveType = @TrackingActiveType WHERE ChildID = @ChildID AND PersonInChargeID =@PersonInChargeID ";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@PersonInChargeID", UserID);
                    command.Parameters.AddWithValue("@ChildID", ChildID);
                    command.Parameters.AddWithValue("@TrackingActiveType", TrackingActiveType);
                    int affectrow = command.ExecuteNonQuery();
                    if (affectrow > 0)
                    {
                        return Ok("successfully updated");
                    }
                    else { return BadRequest("not updated");
                    }
                }
            }            

        }

        //manage tracking type help method
        // check if child has a tracking method
        public static bool isFollow(int childID, int userID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM FollowChilds FC " +
                             "WHERE FC.ChildID = @ChildID AND FC.PersonInChargeID = @UserID";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@UserID", userID);
                    int count = (int)command.ExecuteScalar();
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }



        //public static bool isFollow(int childID, int userID)
        //{

        //    using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
        //    {
        //        conn.Open();
        //        string sql = "SELECT FC.TrackByApp, FC.TrackByDevice FROM FollowChilds FC " +
        //                     "INNER JOIN PersonChilds PC ON FC.ChildId = PC.ChildID " +
        //                     "WHERE PC.ChildID = @ChildID AND PC.MainPersonInChargeID = @UserID";
        //        using (SqlCommand command = new SqlCommand(sql, conn))
        //        {
        //            command.Parameters.AddWithValue("@ChildID", childID);
        //            command.Parameters.AddWithValue("@UserID", userID);
        //            SqlDataReader reader = command.ExecuteReader();
        //            if (reader.Read() && reader.HasRows)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }


        //    }

        //}




        //insertHasCardMethod to insert 
        public static bool insertHasCardMethod(int childID, int userID, bool app, bool device, string TrackingActiveType)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "INSERT INTO FollowChilds (PersonInChargeID, ChildID, TrackByApp, TrackByDevice, HasCard, TrackingActiveType) " +
                             "VALUES (@PersonInChargeID, @ChildID, @app, @device, 1, @TrackingActiveType)";

                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@PersonInChargeID", userID);
                    command.Parameters.AddWithValue("@app", app);
                    command.Parameters.AddWithValue("@device", device);
                    command.Parameters.AddWithValue("@TrackingActiveType", TrackingActiveType);


                    int reader = command.ExecuteNonQuery();
                    if (reader > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        }

        //insert or update AppMethod in link child         
        public static bool insertorupdateAppMethod(int childID, int userID)
        {
            bool isfollow = isFollow(childID, userID);
            if (isfollow)
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    conn.Open();
                    string sql = "UPDATE FollowChilds SET TrackByApp = @app, TrackingActiveType = @TrackingActiveType WHERE ChildID = @ChildID AND PersonInChargeID =@MainPersonInChargeID ";
                    using(SqlCommand command = new SqlCommand(sql,conn))
                    {
                        command.Parameters.AddWithValue("@ChildID", childID);
                        command.Parameters.AddWithValue("@MainPersonInChargeID", userID);
                        command.Parameters.AddWithValue("@app", 1);
                        command.Parameters.AddWithValue("@TrackingActiveType", "app");
                        int affectrow = command.ExecuteNonQuery();
                        if (affectrow > 0)
                        {
                            return true;
                        }
                        else { return false; }
                    }


                }

            }
            else
            {
                bool isinsert = insertHasCardMethod(childID, userID , true, false, "app");
                if (isinsert)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }


        //insert or update AppMethod in link child         
        public static bool insertorupdateDeviceMethod(int childID, int userID)
        {
            bool isfollow = isFollow(childID, userID);
            if (isfollow)
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    conn.Open();
                    string sql = "UPDATE FollowChilds SET TrackByDevice = @Device, TrackingActiveType = @TrackingActiveType WHERE ChildID = @ChildID AND PersonInChargeID =@MainPersonInChargeID ";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@ChildID", childID);
                        command.Parameters.AddWithValue("@MainPersonInChargeID", userID);
                        command.Parameters.AddWithValue("@Device", 1);
                        command.Parameters.AddWithValue("@TrackingActiveType", "device");
                        int affectrow = command.ExecuteNonQuery();
                        if (affectrow > 0)
                        {
                            return true;
                        }
                        else { return false; }
                    }


                }

            }
            else
            {
                bool isinsert = insertHasCardMethod(childID, userID, false, true, "device");
                if (isinsert)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

    }
}

    
