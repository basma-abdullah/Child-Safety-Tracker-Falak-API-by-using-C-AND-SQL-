using FalaKAPP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Permission : ControllerBase
    {

        //To retrieve a list of MY children who have given permission and accepted البرمشن الى اشوف فيه اطفالي الى معطية مراقبتهم لشخص
        [HttpGet("givenacceptedpermission/{personInChargeID}")]
        public ActionResult<IEnumerable<object>> GivenAcceptedPermission(int personInChargeID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> acceptedPermissions = new List<object>();
                string sql = "SELECT * FROM permissionToFollow WHERE PersonInChargeID = @PersonInChargeID AND PermissionActivationStatus = 'enable'";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@PersonInChargeID", personInChargeID);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int permissionPersonID = reader.GetInt32(reader.GetOrdinal("permissionPersonID"));
                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));

                            ActionResult<PersonUsers?> result = DatabaseSettings.GetByID(permissionPersonID);
                            if (result.Result is OkObjectResult okResult && okResult.Value is PersonUsers user)
                            {
                                string fullName = user.FullName;
                                int phoneNumber = user.PhoneNumber;

                                string permissionPersonFullName = GetFullName(permissionPersonID);
                                string childFullName = GetFullName(childID);

                                var acceptedPermissionDetail = new
                                {
                                    ChildFullName = childFullName,
                                    PermissionPersonFullName = permissionPersonFullName,
                                    KinshipT = kinshipT,
                                    PhoneNumber = phoneNumber
                                };

                                acceptedPermissions.Add(acceptedPermissionDetail);
                            }
                        }

                        conn.Close();
                        return Ok(acceptedPermissions);
                    }

                    return NotFound("No permissions given");
                }
            }
        }

        //To retrieve a list of OTHER children whe i have permission to follow اشوف أطفال غيري الى اراقبهم
        [HttpGet("recivedPermission/{PermissionPersonID}")]
        public ActionResult<IEnumerable<object>> RecivedAcceptedPermission(int PermissionPersonID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> acceptedPermissions = new List<object>();
                string sql = "SELECT * FROM permissionToFollow WHERE PermissionPersonID = @PermissionPersonID AND PermissionActivationStatus = 'enable'";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@PermissionPersonID", PermissionPersonID);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int personInChargeID = reader.GetInt32(reader.GetOrdinal("personInChargeID"));
                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));

                            ActionResult<PersonUsers?> result = DatabaseSettings.GetByID(personInChargeID);
                            if (result.Result is OkObjectResult okResult && okResult.Value is PersonUsers user)
                            {
                                string fullName = user.FullName;
                                int phoneNumber = user.PhoneNumber;

                                string permissionPersonFullName = GetFullName(personInChargeID);
                                string childFullName = GetFullName(childID);

                                var acceptedPermissionDetail = new
                                {
                                    ChildFullName = childFullName,
                                    PermissionPersonFullName = permissionPersonFullName,
                                    KinshipT = kinshipT,
                                    PhoneNumber = phoneNumber
                                };

                                acceptedPermissions.Add(acceptedPermissionDetail);
                            }
                        }
                        conn.Close();
                        return Ok(acceptedPermissions);
                    }

                    return NotFound("No permissions provided");
                }

            }
        }


        //To give a permition to person based on thier phone number 
        [HttpPost("Givepermission/PermissionPersonID")]
        public ActionResult<object> GivePermission(int PersonInChargeID, string PermissionPersonphonenumber, int ChildID, string kinshipT)
        {
            int permissionpersonid = DatabaseSettings.getID(PermissionPersonphonenumber);
            if (permissionpersonid != 0)
            {
                using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
                {
                    conn.Open();
                    string sql = "INSERT into permissionToFollow( PersonInChargeID, PermissionPersonID,ChildID , KinshipT )values ( @PersonInChargeID , @permissionpersonid , @ChildID ,@KinshipT)";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {

                        command.Parameters.AddWithValue("@PersonInChargeID", PersonInChargeID);
                        command.Parameters.AddWithValue("@permissionpersonid", permissionpersonid);
                        command.Parameters.AddWithValue("@ChildID", ChildID);
                        command.Parameters.AddWithValue("@KinshipT", kinshipT);
                        int affectedrow = command.ExecuteNonQuery();
                        if (affectedrow > 0)
                        {
                            return Ok("succeffully send");
                        }
                        else
                        {
                            return BadRequest("not send");
                        }
                    }


                }
            }
            else
            {
                return NotFound("user not found");

            }

        }





        //To review the new permissions sent to the user رؤية الاذونات الواصلة
        [HttpGet("Getmynewpermission/{PermissionPersonID}")]
        public IActionResult Getmynewpermission(int PermissionPersonID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> Acceptedpermission = new List<object>();
                string sql = "SELECT * FROM permissionToFollow WHERE PermissionPersonID = @PermissionPersonID AND PermissionActivationStatus IS NULL";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@PermissionPersonID", PermissionPersonID);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int personInChargeID = reader.GetInt32(reader.GetOrdinal("personInChargeID"));
                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));

                            ActionResult<PersonUsers> result = DatabaseSettings.GetByID(personInChargeID);
                            if (result.Result is OkObjectResult okResult)
                            {
                                PersonUsers user = (PersonUsers)okResult.Value; // Explicit cast to PersonUsers
                                if (user != null)
                                {
                                    string fullname = user.FullName;
                                    int phoneNumber = user.PhoneNumber;

                                    string permissionPersonFullName = GetFullName(personInChargeID);
                                    string childFullName = GetFullName(childID);
                                    var acceptedPermissionDetail = new { ChildFullName = childFullName, PermissionPersonFullName = permissionPersonFullName, kinshipT, PhoneNumber = phoneNumber };
                                    Acceptedpermission.Add(acceptedPermissionDetail);
                                }
                            }
                        }
                    }
                }

                return Ok(Acceptedpermission);
            }
        }


        //To retrieve a list of THE permission statues THAT i have SEND اشوف حالات الاذونات الى ارسلتها 


        [HttpGet("Getpermissionstatues/{personInChargeID}")]
        public IActionResult Getpermissionstatues(int personInChargeID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> Acceptedpermission = new List<object>();
                string sql = "SELECT * FROM permissionToFollow WHERE personInChargeID = @personInChargeID AND (PermissionActivationStatus IS NULL OR PermissionActivationStatus = 'disable')";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@personInChargeID", personInChargeID);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));
                            int PermittionPersonID = reader.GetInt32(reader.GetOrdinal("PermissionPersonID"));
                            ActionResult<PersonUsers> result = DatabaseSettings.GetByID(PermittionPersonID);
                            if (result.Result is OkObjectResult okResult)
                            {
                                PersonUsers user = (PersonUsers)okResult.Value; // Explicit cast to PersonUsers
                                if (user != null)
                                {
                                    string fullname = user.FullName;
                                    int phoneNumber = user.PhoneNumber;

                                    string permissionPersonFullName = GetFullName(PermittionPersonID);
                                    string childFullName = GetFullName(childID);
                                    var acceptedPermissionDetail = new { ChildFullName = childFullName, PermissionPersonFullName = permissionPersonFullName, kinshipT, PhoneNumber = phoneNumber };
                                    Acceptedpermission.Add(acceptedPermissionDetail);
                                }
                            }
                        }
                    }
                }

                return Ok(Acceptedpermission);
            }
        }






        private string GetFullName(int userID)
        {
            ActionResult<PersonUsers> result = DatabaseSettings.GetByID(userID);
            if (result.Result is OkObjectResult okResult)
            {
                PersonUsers user = (PersonUsers)okResult.Value; // Explicit cast to PersonUsers
                if (user != null)
                {
                    return user.FullName;
                }
            }

            return string.Empty;
        }

        // الرد على اذن

        [HttpPut("ReplyToPermission")]
        public IActionResult acceptedorRejectPermission(int permmitionpersonID, int mainpersoninchargeid, int ChildID, string PermissionActivationStatus)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "UPDATE permissionToFollow SET PermissionActivationStatus = @PermissionActivationStatus WHERE PermissionPersonID = @permmitionpersonID AND PersonInChargeID = @mainpersoninchargeid AND ChildID = @ChildID";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@permmitionpersonID", permmitionpersonID);
                    command.Parameters.AddWithValue("@mainpersoninchargeid", mainpersoninchargeid);
                    command.Parameters.AddWithValue("@ChildID", ChildID);
                    command.Parameters.AddWithValue("@PermissionActivationStatus", PermissionActivationStatus);
                    int affectrow = command.ExecuteNonQuery();
                    if (affectrow > 0)
                    {
                        if (PermissionActivationStatus == "enable")
                        {
                            bool addtofollowchild = InsertToFollowChild(permmitionpersonID, mainpersoninchargeid, ChildID);
                            if (addtofollowchild)
                            {
                                return Ok("succefully updated");
                            }
                            else
                            {
                                BadRequest("not linked");
                            }
                        }
                        return Ok("updated");
                    }
                    else { return BadRequest("not"); }
                }


            }
        }


        public static bool InsertToFollowChild(int permissionPerson, int mainPersonInChargeID, int childID)
        {
            FollowChilds followChilds = ToGetFollowChildInfo(mainPersonInChargeID, childID);
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();

                // التحقق مما إذا كانت الصف متواجدة بالفعل في الجدول
                string checkExistingQuery = "SELECT COUNT(*) FROM FollowChilds WHERE PersonInChargeID = @PersonInChargeID AND ChildID = @ChildID";
                using (SqlCommand checkExistingCommand = new SqlCommand(checkExistingQuery, conn))
                {
                    checkExistingCommand.Parameters.AddWithValue("@PersonInChargeID", permissionPerson);
                    checkExistingCommand.Parameters.AddWithValue("@ChildID", childID);
                    int existingRowsCount = (int)checkExistingCommand.ExecuteScalar();
                    if (existingRowsCount > 0)
                    {

                        return false;
                    }
                }

                // إذا وصلنا إلى هذه النقطة، يعني أن الصف غير موجود، ويمكننا إضافته
                string insertQuery = "INSERT INTO FollowChilds (PersonInChargeID, ChildID, TrackByApp, TrackByDevice, HasCard, TrackingActiveType, AllowTorack) " +
                                     "VALUES (@PersonInChargeID, @ChildID, @app, @device, 1, @TrackingActiveType, 1)";
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, conn))
                {
                    insertCommand.Parameters.AddWithValue("@ChildID", childID);
                    insertCommand.Parameters.AddWithValue("@PersonInChargeID", permissionPerson);
                    insertCommand.Parameters.AddWithValue("@app", followChilds.TrackByApp);
                    insertCommand.Parameters.AddWithValue("@device", followChilds.TrackByDevice);
                    insertCommand.Parameters.AddWithValue("@TrackingActiveType", followChilds.TrackingActiveType);
                    int affectedRows = insertCommand.ExecuteNonQuery();
                    if (affectedRows > 0)
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


        public static FollowChilds ToGetFollowChildInfo(int personinchargeid, int ChildID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "SELECT * FROM FollowChilds WHERE ChildID = @ChildID AND PersonInChargeID = @PersonInChargeID"; using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", ChildID);
                    command.Parameters.AddWithValue("@PersonInChargeID", personinchargeid);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read() && reader.HasRows)
                    {
                        FollowChilds followChilds = new FollowChilds()
                        {
                            TrackByApp = reader.GetString(reader.GetOrdinal("TrackByApp")),
                            TrackByDevice = reader.GetString(reader.GetOrdinal("TrackByDevice")),
                            HasCard = reader.GetString(reader.GetOrdinal("HasCard")),
                            TrackingActiveType = reader.GetString(reader.GetOrdinal("TrackingActiveType")),
                            AllowTorack = reader.GetString(reader.GetOrdinal("AllowTorack"))
                        };

                        return followChilds;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }




        // حذف الـ permission المرسل
        [HttpDelete("DeleteReceivedPermission/{PermissionPersonID}/{PersonInChargeID}/{ChildID}")]
        public IActionResult DeleteReceivedPermission(int PermissionPersonID, int PersonInChargeID, int ChildID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteFollowChildsSql = "DELETE FROM FollowChilds WHERE  ChildID = @ChildID AND  PersonInChargeID = (SELECT permissionPersonID from permissionToFollow where permissionPersonID = @permissionPersonID and ChildID = @ChildID AND PersonInChargeID =@PersonInChargeID)";
                        using (SqlCommand deleteFollowChildsCommand = new SqlCommand(deleteFollowChildsSql, conn, transaction))
                        {
                            deleteFollowChildsCommand.Parameters.AddWithValue("@PermissionPersonID", PermissionPersonID);
                            deleteFollowChildsCommand.Parameters.AddWithValue("@PersonInChargeID", PersonInChargeID);
                            deleteFollowChildsCommand.Parameters.AddWithValue("@ChildID", ChildID);

                            deleteFollowChildsCommand.ExecuteNonQuery();
                        }

                        string deleteReceivedPermissionSql = "DELETE FROM permissionToFollow WHERE PermissionPersonID = @PermissionPersonID AND PersonInChargeID = @PersonInChargeID AND ChildID = @ChildID AND PermissionActivationStatus = 'enable'";
                        using (SqlCommand deleteReceivedPermissionCommand = new SqlCommand(deleteReceivedPermissionSql, conn, transaction))
                        {
                            deleteReceivedPermissionCommand.Parameters.AddWithValue("@PermissionPersonID", PermissionPersonID);
                            deleteReceivedPermissionCommand.Parameters.AddWithValue("@PersonInChargeID", PersonInChargeID);
                            deleteReceivedPermissionCommand.Parameters.AddWithValue("@ChildID", ChildID);

                            int affectedRows = deleteReceivedPermissionCommand.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {
                                transaction.Commit();
                                return Ok("The permission has been successfully deleted");
                            }
                            else
                            {
                                transaction.Rollback();
                                return NotFound("Permission not found");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest("An error occurred during the deletion operation: " + ex.Message);
                    }
                }
            }
        }



    }
}










//public static string GetFullName(int permmitionPersonID)
//{
//    using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
//    {
//        conn.Open();
//        string sql = "select * from PersonUsers where UserID = @UserID  '";
//        using (SqlCommand command = new SqlCommand(sql, conn))
//        {
//            command.Parameters.AddWithValue("@UserID", permmitionPersonID);
//            SqlDataReader reader = command.ExecuteReader();
//            reader.Read();
//            if (reader.HasRows)
//            {

//                string GetName = reader.GetString(reader.GetOrdinal("Fullname"));


//                reader.Close();
//                return GetName;
//            }

//        }

//    }
//    return "Not Found";
//}











