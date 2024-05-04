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
        public static List<int> checkForTracking(int childID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "select t.TrackingChildMasterID from FollowChilds f , TrackingChildMaster t where t.LinkChildsID = f.LinkChildsID AND f.Childid = @childID AND ParentReaction = 'open' ";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@childID", childID);
                    var resultList = new List<int>();

                    SqlDataReader rowsAffected = command.ExecuteReader();
                    if (rowsAffected.HasRows)
                    {
                        while (rowsAffected.Read())
                        {

                            int id = rowsAffected.GetInt32(rowsAffected.GetOrdinal("TrackingChildMasterID"));
                            resultList.Add(id);
                        }

                        return resultList;
                    }
                    else
                    {
                        return null;
                    }
                }

            }

        }


        // it's wooorkkk ^
        [HttpPost("Updatelastlocation")]
        public IActionResult Updateandinsertlastlocation(int childID, decimal Latitude, decimal Longitude)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    List<int> trackingID = checkForTracking(childID);
                    if (trackingID != null)
                    {


                        string insertQuery = "INSERT INTO TrackingChildPlaceDetail (TrackingChildMasterID, DateTimeLoc, Latitude, Longitude) VALUES (@TrackingChildMasterID, @DateTime12, @Latitude, @Longitude)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);




                        foreach (int trackingMasterID in trackingID)
                        {
                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@DateTime12", currentDateTime);
                            insertCommand.Parameters.AddWithValue("@Longitude", Longitude);
                            insertCommand.Parameters.AddWithValue("@Latitude", Latitude);
                            insertCommand.Parameters.AddWithValue("@TrackingChildMasterID", trackingMasterID);
                            insertCommand.ExecuteNonQuery();
                        }
                        
                        string updateQuery2 = "UPDATE TrackingChildMaster SET LastChildLocationLongitude = @LastLocationLongitude, LastChildLocationLatitude = @LastLocationLatitude , EndTrackingTime = @EndTrackingTime WHERE TrackingChildMasterID = @TrackingChildMasterID";
                        SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);



                        foreach (int trackingMasterID in trackingID)
                        {
                            command2.Parameters.Clear();
                            command2.Parameters.AddWithValue("@LastLocationLatitude", Latitude);
                            command2.Parameters.AddWithValue("@LastLocationLongitude", Longitude);
                            command2.Parameters.AddWithValue("@EndTrackingTime", currentDateTime);
                            command2.Parameters.AddWithValue("@TrackingChildMasterID", trackingMasterID);
                            command2.ExecuteNonQuery();
                        }

                        string updateQuery3 = "UPDATE PersonChilds SET Longitude = @LastLocationLongitude, Latitude = @LastLocationLatitude  WHERE ChildID = @ChildID";
                        SqlCommand command3 = new SqlCommand(updateQuery3, conn, transaction);
                        command3.Parameters.AddWithValue("@LastLocationLatitude", Latitude);
                        command3.Parameters.AddWithValue("@LastLocationLongitude", Longitude);
                        command3.Parameters.AddWithValue("@ChildID", childID);
                        command3.ExecuteNonQuery();

                        transaction.Commit();

                        return Ok();


                    }
                    else
                    {
                        return BadRequest();
                    }

                }
            }
        }



        //parent tracking :
        //start track child
        [HttpPost("insertnewmastertrack")]
        public IActionResult insertnewmastertrack(int userID, int childID)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    string updateQuery2 = "insert into TrackingChildMaster(LinkChildsID , StartTrackingDate , StartChildLocationlong , StartChildLocationlat) values ((select LinkChildsID from FollowChilds where PersonInChargeID = @userID AND ChildId = @childID) ,@startTrackingTime , (select Longitude from PersonChilds where  ChildId = @childID ) , (select Latitude from PersonChilds where  ChildId = @childID ) )";
                    SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);
                    command2.Parameters.Clear();
                    command2.Parameters.AddWithValue("@userID", userID);
                    command2.Parameters.AddWithValue("@childID", childID);
                    command2.Parameters.AddWithValue("@startTrackingTime", currentDateTime);
                    command2.ExecuteNonQuery();
                    transaction.Commit();
                    return Ok();
                }
            }
        }


        //update parent location
        [HttpPost("Updateandinsertlastlocationforparent")]
        public IActionResult Updateandinsertlastlocationforparent(int UserID, decimal Latitude, decimal Longitude)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
            
                        string insertQuery = "INSERT INTO volunteerHistoricalLocation (PersonID, dateTime, Longitude,Latitude) VALUES (@PersonID, @dateTime, @Longitude, @Latitude)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, conn, transaction);

                            insertCommand.Parameters.Clear();
                            insertCommand.Parameters.AddWithValue("@PersonID", UserID);
                            insertCommand.Parameters.AddWithValue("@dateTime", currentDateTime);
                            insertCommand.Parameters.AddWithValue("@Longitude", Longitude);
                            insertCommand.Parameters.AddWithValue("@Latitude", Latitude);
                            insertCommand.ExecuteNonQuery();


                    string updateQuery2 = "UPDATE PersonUsers SET Latitude = @Latitude, Longitude = @Longitude , Volunteeractivationstatus = @Volunteeractivationstatus WHERE UserID = @UserID";
                        SqlCommand command2 = new SqlCommand(updateQuery2, conn, transaction);
                            command2.Parameters.Clear();
                            command2.Parameters.AddWithValue("@Latitude", Latitude);
                            command2.Parameters.AddWithValue("@Longitude", Longitude);
                            command2.Parameters.AddWithValue("@Volunteeractivationstatus", Volunteeractivationstatus);
                            command2.ExecuteNonQuery();
     

                       

                        transaction.Commit();

                        return Ok();


                     
        

                }
            }
        }

    }
}
