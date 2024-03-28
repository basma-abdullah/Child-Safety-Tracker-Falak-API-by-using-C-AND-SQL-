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
    public class permission : ControllerBase
    {

        //To retrieve a list of MY children who have given permission and accepted 
        [HttpGet("givenacceptedpermission/personInChargeID")]
        public ActionResult<object> givenacceptedpermission(int personInChargeID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> Acceptedpermission = new List<object>();
                string sql = "select * from permissionToFollow where PersonInChargeID = @PersonInChargeID AND PermissionActivationStatus = 'enable'";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@personInChargeID", personInChargeID);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int permissionPersonID = reader.GetInt32(reader.GetOrdinal("permissionPersonID"));

                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));
                            PersonUsers permissionPersonData = DatabaseSettings.GetByID(permissionPersonID).Value;
                            string permmisionpersonfullname = permissionPersonData.FullName;
                            int permmisionpersonnumber = permissionPersonData.PhoneNumber;
                            PersonUsers childData = DatabaseSettings.GetByID(childID).Value;
                            string ChildFullName = permissionPersonData.FullName;
                            //string permmisionpersonfullname = GetFullName(permissionPersonID);
                            //string ChildFullName = GetFullName(childID);
                            var Acceptedpermissiondetail = new { ChildFullName, permmisionpersonfullname, kinshipT, permmisionpersonnumber };
                            Acceptedpermission.Add(Acceptedpermissiondetail);
                        }
                        conn.Close();
                    }
                }

                return Ok(Acceptedpermission);
            }

        }

        //To retrieve a list of OTHER children whe i have permission to follow
        [HttpGet("recivedPermission/PermissionPersonID")]
        public ActionResult<object> recivedacceptedPermission(int PermissionPersonID)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                List<object> Acceptedpermission = new List<object>();
                string sql = "select * from permissionToFollow where PermissionPersonID = @PermissionPersonID AND PermissionActivationStatus = 'enable'";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@PermissionPersonID", PermissionPersonID);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        do
                        {
                            int personInChargeID = reader.GetInt32(reader.GetOrdinal("personInChargeID"));
                            string kinshipT = reader.GetString(reader.GetOrdinal("kinshipT"));
                            int childID = reader.GetInt32(reader.GetOrdinal("ChildID"));
                            PersonUsers permissionPersonData = DatabaseSettings.GetByID(personInChargeID).Value;
                            string permmisionpersonfullname = permissionPersonData.FullName;
                            int permmisionpersonnumber = permissionPersonData.PhoneNumber;
                            PersonUsers childData = DatabaseSettings.GetByID(childID).Value;
                            string ChildFullName = permissionPersonData.FullName;
                            var Acceptedpermissiondetail = new { ChildFullName, permmisionpersonfullname, kinshipT, permmisionpersonnumber };
                            Acceptedpermission.Add(Acceptedpermissiondetail);
                        }
                        while (reader.Read());
                    }
                    conn.Close();
                }
                

                return Ok(Acceptedpermission);
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





        //To review the new permissions sent to the user
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

                                    string permissionPersonFullName = GetFullName(PermissionPersonID);
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


        //To retrieve a list of THE permission statues THAT i have SEND 


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



        [HttpPut]
        public IActionResult acceptedorRejectPermission( int permmitionpersonID, int mainpersoninchargeid, int ChildID, string PermissionActivationStatus)
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
                        if(PermissionActivationStatus == "enable")
                        {
                            bool addtofollowchild = insertToFollowChild(permmitionpersonID , mainpersoninchargeid , ChildID);
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



        public static bool insertToFollowChild(int permmitionperson , int mainpersoninchargeid, int ChildID)
        {
            FollowChilds followChilds = ToGetFollowChildInfo(mainpersoninchargeid, ChildID);
            using (SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                conn.Open();
                string sql = "INSERT INTO FollowChilds (PersonInChargeID, ChildID, TrackByApp, TrackByDevice, HasCard, TrackingActiveType, AllowToTrack) " +
                             "VALUES (@PersonInChargeID, @ChildID, @app, @device, 1, @TrackingActiveType, 1)";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@ChildID", ChildID);
                    command.Parameters.AddWithValue("@PersonInChargeID", permmitionperson);
                    command.Parameters.AddWithValue("@app", followChilds.TrackByApp);
                    command.Parameters.AddWithValue("@device", followChilds.TrackByDevice);
                    command.Parameters.AddWithValue("@TrackingActiveType", followChilds.TrackingActiveType);
                    int affectedrow = command.ExecuteNonQuery();
                    if (affectedrow>0)
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





    }


}














