using FalaKAPP.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class filterController : ControllerBase
    {
        [HttpGet ("filterByDate")]
        public ActionResult<object> filterByDate(int UserID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                string sql = "select RS.LostNotificationResponseID , RS.LostNotificationRequestID , RS.ResponseByPersonID , RS.ResponseStatus , RS.ResponseDate , RS.CurrentImagePath ,RS.accuracy , RS.Comments , ps.FullName , ps.PhoneNumber from LostNotificationResponse RS , LostNotificationRequest RQ , PersonUsers ps where RS.ResponseByPersonID = ps.UserID AND RS.LostNotificationRequestID = RQ.LostNotificationRequestID AND RQ.mainPersonInChargeID = @UserID ORDER BY RS.ResponseDate DESC  ";

                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    command.Parameters.AddWithValue("@UserID", UserID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<object> responseslist = new List<object>();

                        while (reader.Read())
                        {
                            var response = new
                            {
                             // Retrieve the response information from the reader
                             LostNotificationResponseID = reader.GetInt32(reader.GetOrdinal("LostNotificationResponseID")),
                             LostNotificationRequestID = reader.GetInt32(reader.GetOrdinal("LostNotificationRequestID")),
                             ResponseByPersonID = reader.GetInt32(reader.GetOrdinal("ResponseByPersonID")),
                             ResponseStatus = reader.GetString(reader.GetOrdinal("ResponseStatus")),
                             ResponseDate = reader.GetDateTime(reader.GetOrdinal("ResponseDate")),
                             CurrentImagePath = reader.GetString(reader.GetOrdinal("CurrentImagePath")),
                             accuracy = reader.GetInt32(reader.GetOrdinal("accuracy")),
                             Comments = reader.GetString(reader.GetOrdinal("Comments")),
                             FullName = reader.GetString(reader.GetOrdinal("FullName")),
                             PhoneNumber = reader.GetInt32(reader.GetOrdinal("PhoneNumber")),

                            };

                            responseslist.Add(response);
                        }

                        if (responseslist.Count > 0)
                        {
                            return Ok(responseslist);
                        }
                        else
                        {
                            return NotFound("no response found");
                        }
                    }
                }

            }

        }

        [HttpGet("filterByAccuracy")]
        public ActionResult<object> filterByAccuracy(int UserID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                string sql = "select RS.LostNotificationResponseID , RS.LostNotificationRequestID , RS.ResponseByPersonID , RS.ResponseStatus , RS.ResponseDate , RS.CurrentImagePath ,RS.accuracy , RS.Comments , ps.FullName , ps.PhoneNumber from LostNotificationResponse RS , LostNotificationRequest RQ , PersonUsers ps where RS.ResponseByPersonID = ps.UserID AND RS.LostNotificationRequestID = RQ.LostNotificationRequestID AND RQ.mainPersonInChargeID = @UserID ORDER BY RS.accuracy DESC  ";

                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    command.Parameters.AddWithValue("@UserID", UserID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<object> responseslist = new List<object>();

                        while (reader.Read())
                        {
                            var response = new
                            {
                                // Retrieve the response information from the reader
                                LostNotificationResponseID = reader.GetInt32(reader.GetOrdinal("LostNotificationResponseID")),
                                LostNotificationRequestID = reader.GetInt32(reader.GetOrdinal("LostNotificationRequestID")),
                                ResponseByPersonID = reader.GetInt32(reader.GetOrdinal("ResponseByPersonID")),
                                ResponseStatus = reader.GetString(reader.GetOrdinal("ResponseStatus")),
                                ResponseDate = reader.GetDateTime(reader.GetOrdinal("ResponseDate")),
                                CurrentImagePath = reader.GetString(reader.GetOrdinal("CurrentImagePath")),
                                accuracy = reader.GetInt32(reader.GetOrdinal("accuracy")),
                                Comments = reader.GetString(reader.GetOrdinal("Comments")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                PhoneNumber = reader.GetInt32(reader.GetOrdinal("PhoneNumber")),

                            };

                            responseslist.Add(response);
                        }

                        if (responseslist.Count > 0)
                        {
                            return Ok(responseslist);
                        }
                        else
                        {
                            return NotFound("no response found");
                        }
                    }
                }

            }

        }



        //create new response for specific request 
        [HttpPost("ResponseForSpecificRequest")]
        public async Task<ActionResult<object>> ResponseForSpecificRequest(IFormFile CurrentImagePath, [FromForm] int LostNotificationRequestID, [FromForm]  int UserID,[FromForm] string ResponseStatus, [FromForm] string Comments , [FromForm] int accuracy)
        {
            DateTime currentDateTime = DateTime.Now;
            using (SqlConnection connection = new SqlConnection(DatabaseSettings.dbConn))
            {
                connection.Open();
                string query = "INSERT INTO LostNotificationResponse(LostNotificationRequestID, ResponseByPersonID, ResponseStatus, ResponseDate, Longitude,Latitude,CurrentImagePath, accuracy,Comments) VALUES  (@LostNotificationRequestID, @UserID, @ResponseStatus, @ResponseDate,(SELECT Longitude FROM PersonUsers WHERE UserID = @UserID),(SELECT Latitude FROM PersonUsers WHERE UserID = @UserID),@CurrentImagePath, @accuracy,@Comments) ";
                SqlCommand comm = new SqlCommand(query, connection);

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

                    comm.Parameters.AddWithValue("@LostNotificationRequestID", LostNotificationRequestID);
                    comm.Parameters.AddWithValue("@UserID", UserID);
                    comm.Parameters.AddWithValue("@ResponseStatus", ResponseStatus);
                    comm.Parameters.AddWithValue("@ResponseDate", currentDateTime);
                    comm.Parameters.AddWithValue("@CurrentImagePath", DatabaseSettings.ImageDirectory_ReadPath + "/" + imageFileName);
                    comm.Parameters.AddWithValue("@accuracy", accuracy);
                    comm.Parameters.AddWithValue("@Comments", Comments);
                    



                    int affectedRow = comm.ExecuteNonQuery();
                    if (affectedRow > 0)
                    {
                        return Ok("successfully created");
                    }
                    else
                    {
                        return BadRequest("not created");
                    }
                }

                return BadRequest("not created");
            }
        }





       [HttpGet("usersNearChild/{childId}")]
        public ActionResult<IEnumerable<object>> GetRequestsNearUser(int childId)
        {
            // Retrieve the child's location from the 'personchild' table
            using (SqlConnection connection = new SqlConnection(DatabaseSettings.dbConn))
            {
                connection.Open();
                string getChildLocationQuery = "SELECT Longitude, Latitude FROM PersonUsers WHERE UserID = @ChildID";
                SqlCommand getChildLocationCommand = new SqlCommand(getChildLocationQuery, connection);
                getChildLocationCommand.Parameters.AddWithValue("@ChildID", childId);

                using (SqlDataReader reader = getChildLocationCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        double userLongitude = Convert.ToDouble(reader["Longitude"]);
                        double userLatitude = Convert.ToDouble(reader["Latitude"]);
                        reader.Close();

                        // Find requests near the child's location within 30 meters
                        string getRequestsNearChildQuery = "SELECT LRQ.LostNotificationRequestID, LRQ.requestTitle, LRQ.mainPersonInChargeID, " +
                            "LRQ.RequestLostNotificationDate, LRQ.NotificationStatus, LRQ.Comments " +
                            "FROM LostNotificationRequest LRQ " +
                            "JOIN volunteerHistoricalLocation v ON LRQ.LastLocationId = v.volunteerLocationId " +
                            "WHERE SQRT(POWER(v.Longitude - @UserLongitude, 2) + POWER(v.Latitude - @UserLatitude, 2)) <= 0.0063"; // 0.0003 degrees is approximately 30 meters

                        SqlCommand getRequestsNearChildCommand = new SqlCommand(getRequestsNearChildQuery, connection);
                        getRequestsNearChildCommand.Parameters.AddWithValue("@UserLongitude", userLongitude);
                        getRequestsNearChildCommand.Parameters.AddWithValue("@UserLatitude", userLatitude);

                        List<object> requestList = new List<object>();

                        using (SqlDataReader requestReader = getRequestsNearChildCommand.ExecuteReader())
                        {
                            while (requestReader.Read())
                            {
                                var request = new
                                {
                                    LostNotificationRequestID = Convert.ToInt32(requestReader["LostNotificationRequestID"]),
                                    requestTitle = Convert.ToString(requestReader["requestTitle"]),
                                    mainPersonInChargeID = Convert.ToInt32(requestReader["mainPersonInChargeID"]),
                                    RequestLostNotificationDate = Convert.ToDateTime(requestReader["RequestLostNotificationDate"]),
                                    NotificationStatus = Convert.ToString(requestReader["NotificationStatus"]),
                                    Comments = Convert.ToString(requestReader["Comments"])
                                };

                                requestList.Add(request);
                            }
                        }

                        if (requestList.Count > 0)
                        {
                            return Ok(requestList);
                        }
                        else
                        {
                            return NotFound("No requests found near the specified child's location.");
                        }
                    }
                    else
                    {
                        return NotFound("Child not found");
                    }
                }
            }
        }

    }
}

