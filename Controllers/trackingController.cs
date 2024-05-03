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
        [HttpPut("updateChildButtery")]
        public IActionResult UpdateChildButtery(int childID, string enabletrackingstate)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "UPDATE PersonChilds SET isConnect = @enabletrackingstate WHERE ChildID = @ChildID";
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@ChildID", childID);
                command.Parameters.AddWithValue("@enabletrackingstate", enabletrackingstate);

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







        [HttpGet("checkForTracking/{childId}")]
        public ActionResult<IEnumerable<object>> checkForTracking(int childID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "select t.TrackingChildMasterID from FollowChilds f , TrackingChildMaster t where t.LinkChildsID = f.LinkChildsID AND f.Childid = @childID AND ParentReaction = 'open' ";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@childID", childID);
                    var resultList = new List<object>();

                    SqlDataReader rowsAffected = command.ExecuteReader();
                    if (rowsAffected.HasRows)
                    {
                        while (rowsAffected.Read())
                        {

                            int id = rowsAffected.GetInt32(rowsAffected.GetOrdinal("TrackingChildMaster"));
                            resultList.Add(id);
                        }
                        return Ok(resultList);
                    }
                    else
                    {
                        return BadRequest();
                    }


                }

            }

        }


        //ligic error 
        [HttpPost("Updatelastlocation")]
        public IActionResult Updateandinsertlastlocation(List<int> trackingID, DateTime DateTime12, float Latitude, float Longitude)
        {

        using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {

                    try
                    {

                        string insertQuery = "INSERT INTO TrackingChildPlaceDetail (TrackingChildMasterID, DateTimeLoc, Latitude, Longitude) VALUES (@TrackingChildMasterID, @DateTime12, @Latitude, @Longitude)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);

                        if (DateTime12 != null)
                        {
                            insertCommand.Parameters.AddWithValue("@DateTime12", DateTime12);
                        }
                        else
                        {
                            insertCommand.Parameters.AddWithValue("@DateTime12", DBNull.Value);
                        }
                        insertCommand.Parameters.AddWithValue("@Longitude", Longitude);
                        insertCommand.Parameters.AddWithValue("@Latitude", Latitude);

                        foreach (int trackingMasterID in trackingID)
                        {
                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@TrackingChildMasterID", trackingMasterID);
                            insertCommand.ExecuteNonQuery();
                        }


                        string updateQuery2 = "UPDATE TrackingChildMaster SET LastLocationLongitude = @LastLocationLongitude, LastLocationLatitude = @LastLocationLatitude WHERE TrackingChildMasterID = @TrackingChildMasterID";
                        SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);
                        command2.Parameters.AddWithValue("@LastLocationLatitude", Latitude);
                        command2.Parameters.AddWithValue("@LastLocationLongitude", Longitude);

                        //command2.Parameters.AddWithValue("@DateTime12", DateTime12);


                        foreach (int trackingMasterID in trackingID)
                        {
                            command2.Parameters.Clear();
                            command2.Parameters.AddWithValue("@TrackingChildMasterID", trackingMasterID);
                            command2.ExecuteNonQuery();
                        }



                        transaction.Commit();

                        return Ok();
                    }
                    catch (Exception ex)
                    {

                        var errorResponse = new { error = ex.Message };
                        return BadRequest(errorResponse);

                    }

                }
            }
        }





        //parent tracking :





    }
}
   



    

