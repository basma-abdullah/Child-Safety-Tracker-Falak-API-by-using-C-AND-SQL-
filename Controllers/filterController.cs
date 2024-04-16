using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Data.SqlClient;

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

    }
}

