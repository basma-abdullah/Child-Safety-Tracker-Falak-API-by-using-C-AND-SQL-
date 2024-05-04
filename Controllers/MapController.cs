using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {










































































        /*
        [HttpGet("tracking_children")]
        public ActionResult<IEnumerable<object>> GetTrackinglocationForChildren(int userID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            List<object> results = new List<object>();


            string sql = @"
                            SELECT TOP 1 follow.ChildId, trackDetail.Longitude, trackDetail.Latitude
                            FROM FollowChilds AS follow
                            JOIN TrackingChildMaster AS trackMaster ON follow.LinkChildsID = trackMaster.LinkChildsID
                            JOIN TrackingChildPlaceDetail AS trackDetail ON trackMaster.TrackingChildMasterID = trackDetail.TrackingChildMasterID
                            WHERE follow.PersonInChargeID = @userID
                            ORDER BY trackDetail.DateTime DESC";

            using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@userID", userID);

                    using (var reader = command.ExecuteReader())
                    {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildId"));
                            double longitude = reader.GetDouble(reader.GetOrdinal("Longitude"));
                            double latitude = reader.GetDouble(reader.GetOrdinal("Latitude"));

                            var result = new { ChildID = childID, Longitude = longitude, Latitude = latitude };
                            results.Add(result);
                        }
                    }
                }
                }
            

            conn.Close();

            if (results.Count > 0)
            {
                return Ok(results);
            }
            else
            {
                return NotFound("Tracking details not found for any childID");
            }
        }
        */
        /*
        [HttpGet("tracking_child/{userID}/{childID}")]
        public IActionResult GetTrackingDetails(int userID, int childID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();
   
                string sql = @"
                    SELECT TOP 1 trackDetail.Longitude, trackDetail.Latitude
                    FROM PersonUsers AS users
                    JOIN PersonChilds AS child ON users.UserID = child.MainPersonInChargeID
                    JOIN FollowChilds AS follow ON child.ChildID = follow.ChildId
                    JOIN TrackingChildMaster AS trackMaster ON follow.LinkChildsID = trackMaster.LinkChildsID
                    JOIN TrackingChildPlaceDetail AS trackDetail ON trackMaster.TrackingChildMasterID = trackDetail.TrackingChildMasterID
                    WHERE users.UserID = @UserID AND child.ChildID = @ChildID
                    ORDER BY trackDetail.DateTime DESC";

                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.Parameters.AddWithValue("@ChildID", childID);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            double longitude = reader.GetInt32(reader.GetOrdinal("Longitude"));
                            double latitude = reader.GetInt32(reader.GetOrdinal("Latitude"));
                            
                            conn.Close();
                            var result = new { Longitude = longitude, Latitude = latitude };
                            return Ok(result);
                        }
                        else
                        {
                        conn.Close();
                        return NotFound("Tracking details not found");
                        }
                    }

                    
                }
            
            }
        */
        /*
        [HttpGet("trackinghistory/{userID}/{childID}")]
        public ActionResult<IEnumerable<object>> GetTrackingHistory(int userID, int childID)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            conn.Open();

            string sql = @"
                            SELECT TOP 10  trackDetail.Longitude, trackDetail.Latitude
                            FROM FollowChilds AS follow
                            JOIN TrackingChildMaster AS trackMaster ON follow.LinkChildsID = trackMaster.LinkChildsID
                            JOIN TrackingChildPlaceDetail AS trackDetail ON trackMaster.TrackingChildMasterID = trackDetail.TrackingChildMasterID
                            WHERE follow.PersonInChargeID = @UserID AND follow.ChildID = @ChildID
                            ORDER BY trackDetail.DateTime DESC";

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
                    conn.Close() ;
                    return Ok(resultList);
                }
                else
                {
                    conn.Close();
                    return NotFound("Tracking details not found");
                }
            }


            }
        */
        }

    }



