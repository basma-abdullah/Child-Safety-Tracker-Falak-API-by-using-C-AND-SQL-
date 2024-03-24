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
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            {
                conn.Open();
                Boolean isexist = DatabaseSettings.isIdExists(UserID);
                if (isexist)
                {
                    string sql = "UPDATE PersonUsers SET FullName = @FullName, Email = @Email WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // Add parameters and their values
                        cmd.Parameters.AddWithValue("@FullName", FullName);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@UserID", UserID);

                        conn.Open();
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

        //change phonenumber 
        [HttpPut("changePhoneNumber/{UserID}")]
        public IActionResult changePhoneNumber(int UserID, string PhoneNumber)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
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

                        conn.Open();
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



        // to delete child from parent
        [HttpPut("unlinkchild/{ChildID}")]
        public IActionResult unlinkchild(int ChildID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                Boolean isexist = DatabaseSettings.isIdExists(ChildID);
                if (isexist== true)
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



        // child tracking management 




        //get all child name with active tracking type that link by particular parent
        [HttpGet("ChildMangment/{UserID}")]
        public IActionResult GetchildsAndActiveTrackingType(string UserID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            string sql = "SELECT PC.ChildID, CN.FullName, FC.TrackingActiveType " +
                         "FROM PersonChilds PC  " +
                         "JOIN PersonUsers PU ON PC.MainPersonInChargeID = PU.UserID " +
                         "JOIN PersonUsers CN ON PC.ChildID = CN.UserID " +
                         "JOIN FollowChilds FC ON PC.ChildID = FC.ChildID " +
                         "WHERE PC.MainPersonInChargeID = @UserID";

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


    }
}
