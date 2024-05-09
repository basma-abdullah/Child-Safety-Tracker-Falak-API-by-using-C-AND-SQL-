using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class trackingController : ControllerBase
    {
        //child device location and tracking :

        //when check child device have more than 3% charge the state will be update yes for more than 3% and no for less
        [HttpPut("updateChildButtery")]
        public IActionResult UpdateChildButtery(int childID, string enabletrackingstate)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                int rowsAffected;
                conn.Open();
                if (enabletrackingstate == "yes") { 
                
                string sql = "UPDATE PersonChilds SET isConnect = @enabletrackingstate WHERE ChildID = @ChildID";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@ChildID", childID);
                command.Parameters.AddWithValue("@enabletrackingstate", enabletrackingstate);

                rowsAffected = command.ExecuteNonQuery();
                }
                else
                {
                    string sql = "UPDATE PersonChilds SET isConnect = @enabletrackingstate , Longitude = null , Latitude = null  WHERE ChildID = @ChildID";
                    SqlCommand command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@enabletrackingstate", enabletrackingstate);

                    rowsAffected = command.ExecuteNonQuery();
                }
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


        //update is connect then add location to volunteerchild table
        //update child location each 1 minute will call this API 
        //if invoke from child mobile the modeltype will be a app
        //if invoke from bracelet or OQ device the modeltype will be tracker
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
                    SqlCommand command = new SqlCommand(sql, conn, transaction);
                    command.Parameters.AddWithValue("@ChildID", childID);
                    command.Parameters.AddWithValue("@Longitude", Longitude);
                    command.Parameters.AddWithValue("@Latitude", Latitude);
                    command.ExecuteNonQuery();
                    transaction.Commit();

                    return Ok();

                }
            }
        }



        //parent tracking :

        //update parent location each 1 minute will call this API 
        [HttpPost("Updateandinsertlastlocationforparent")]
        public IActionResult Updateandinsertlastlocationforparent(int UserID, decimal Latitude, decimal Longitude, string DevicesuppliedType)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {

                    string insertQuery = "INSERT INTO volunteerHistoricalLocation (PersonID, dateTime, Longitude,Latitude , DevicesuppliedType) VALUES (@PersonID, @dateTime, @Longitude, @Latitude , @DevicesuppliedType)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);
                    insertCommand.Parameters.AddWithValue("@PersonID", UserID);
                    insertCommand.Parameters.AddWithValue("@dateTime", currentDateTime);
                    insertCommand.Parameters.AddWithValue("@Longitude", Longitude);
                    insertCommand.Parameters.AddWithValue("@Latitude", Latitude);
                    insertCommand.Parameters.AddWithValue("@DevicesuppliedType", DevicesuppliedType);
                    insertCommand.ExecuteNonQuery();


                    string updateQuery2 = "UPDATE PersonUsers SET Latitude = @Latitude, Longitude = @Longitude , VoulnteerChildLocationID = (select TOP 1 volunteerLocationId from volunteerHistoricalLocation where PersonID = @UserID ORDER BY dateTime DESC) WHERE UserID = @UserID";
                    SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);
                    command2.Parameters.AddWithValue("@Latitude", Latitude);
                    command2.Parameters.AddWithValue("@Longitude", Longitude);
                    command2.Parameters.AddWithValue("@UserID", UserID);
                    command2.ExecuteNonQuery();
                    transaction.Commit();

                    return Ok();





                }
            }
        }

        // this the enable or disabke button in home for start track child or end tracking  :
        [HttpPut("updateparentreaction")]
        public IActionResult updateparentreaction(int userID,int childID , string AllowTorack)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "UPDATE FollowChilds SET AllowTorack =@AllowTorack WHERE LinkChildsID = (select LinkChildsID from FollowChilds where PersonInChargeID = @userID AND ChildId = @childID)";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@AllowTorack", AllowTorack);
                command.Parameters.AddWithValue("@childID", childID);
                command.Parameters.AddWithValue("@userID", userID);

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


        //to get child history in history page 
        [HttpGet("trackinghistory/{userID}/{childID}")]
        public ActionResult<IEnumerable<object>> GetTrackingHistory(int userID, int childID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            string sql = @"
                            SELECT TOP 30 trackDetail.Longitude, trackDetail.Latitude
                            FROM volunteerHistoricalLocation AS trackDetail , FollowChilds as fc
                            where fc.PersonInChargeID =@userID  and fc.ChildId =@childID and ChildId = PersonID and  fc.TrackingActiveType = trackDetail.DevicesuppliedType
                            ORDER BY trackDetail.dateTime DESC";
            
            using (var command = new SqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ChildID", childID);

                var resultList = new List<object>();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            double longitude = reader.GetDouble(reader.GetOrdinal("Longitude"));
                            double latitude = reader.GetDouble(reader.GetOrdinal("Latitude"));

                            var result = new { Longitude = longitude, Latitude = latitude };
                            resultList.Add(result);
                        }
                    }
                }

                if (resultList.Count > 0)
                {
                    conn.Close();
                    return Ok(resultList);
                }
                else
                {
                    conn.Close();
                    return NotFound("Tracking details not found");
                }
            }


        }

    }
}
