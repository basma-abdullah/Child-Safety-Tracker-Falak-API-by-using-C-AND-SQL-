using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using FalaKAPP.Models;
using Humanizer;
using System;


namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FindLostChildController : ControllerBase
    {
        [HttpPost("addNewFindLostChild")]
        public async Task<ActionResult<object>> addNewFindLostChild( IFormFile CurrentImagePath , [FromForm] FindLostChildInput respones)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseSettings.dbConn))
            {
                DateTime currentDateTime = DateTime.Now;
                connection.Open();
                string query = "INSERT INTO FindLostChild(responesTitle, HelperID, FindLostChildDate, ApproximateAge, responseImagePath, Comments ,LocationID) VALUES (@responesTitle, @HelperID, @FindLostChildDate, @ApproximateAge, @responseImagePath, @Comments , (select VoulnteerChildLocationID from PersonUsers where UserID = @UserID ))";
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

                    comm.Parameters.AddWithValue("@responesTitle", respones.responesTitle);
                    comm.Parameters.AddWithValue("@HelperID", respones.HelperID);
                    comm.Parameters.AddWithValue("@FindLostChildDate", currentDateTime);
                    comm.Parameters.AddWithValue("@ApproximateAge", respones.ApproximateAge);
                    comm.Parameters.AddWithValue("@responseImagePath", DatabaseSettings.ImageDirectory_ReadPath + "/" + imageFileName);
                    comm.Parameters.AddWithValue("@Comments", respones.Comments);
                    comm.Parameters.AddWithValue("@UserID", respones.UserID);

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


        [HttpGet("getcurrentrespones/{UserID}")]
        public ActionResult<Object> getcurrentrespones(int UserID)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseSettings.dbConn))
            {
                connection.Open();
                string query = $"SELECT DISTINCT F.FindLostChildID, F.responesTitle, F.FindLostChildDate, F.ApproximateAge, F.responseImagePath, F.NotificationStatus, F.Comments " +
                               $"FROM FindLostChild F " +
                               $"JOIN PersonUsers P ON F.HelperID = P.UserID " +
                               $"WHERE F.HelperID = @UserID AND NotificationStatus != 'received'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserID);

                    SqlDataReader reader = command.ExecuteReader();

                    List<object> responeslist = new List<object>();
                    while (reader.Read())
                    {
                        var respones = new
                        {
                            responesTitle = reader.GetString(reader.GetOrdinal("responesTitle")),
                            FindLostChildDate = reader.GetDateTime(reader.GetOrdinal("FindLostChildDate")),
                            ApproximateAge = reader.GetInt32(reader.GetOrdinal("ApproximateAge")),
                            responseImagePath = reader.GetString(reader.GetOrdinal("responseImagePath")),
                            NotificationStatus = reader.GetString(reader.GetOrdinal("NotificationStatus")),
                            Comments = reader.GetString(reader.GetOrdinal("Comments")),
                        };

                        responeslist.Add(respones);
                    }

                    connection.Close();

                    if (responeslist.Count > 0)
                    {
                        return Ok(responeslist);
                    }

                    return NotFound("No child found");
                }
            }
        }


        [HttpPut("updateresponesstate")]
        public IActionResult updateresponesstate(int FindLostChildID, string NotificationStatus)
        {
            SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn);
            {
                conn.Open();

                string sql = "UPDATE FindLostChild SET NotificationStatus = @NotificationStatus WHERE FindLostChildID = @FindLostChildID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Add parameters and their values
                    cmd.Parameters.AddWithValue("@NotificationStatus", NotificationStatus);
                    cmd.Parameters.AddWithValue("@FindLostChildID", FindLostChildID);
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


        }
    }
}















                
            
