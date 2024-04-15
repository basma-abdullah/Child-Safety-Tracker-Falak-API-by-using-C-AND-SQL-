using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FalaKAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class filterController : ControllerBase
    {
        [HttpGet]
        public ActionResult filterByDate(int UserID)
        {
            using(SqlConnection conn = new SqlConnection(DatabaseSettings.dbConn))
            {
                string sql = "";
                using(SqlCommand cmd = new SqlCommand(sql , conn) { 
                    
                    
                    
                }
            }

        }
    }
}
