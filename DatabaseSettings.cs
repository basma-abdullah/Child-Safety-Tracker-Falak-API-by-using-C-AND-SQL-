using System.Data.SqlClient;

namespace FalaKAPP
{
    public class DatabaseSettings
    {
        public DatabaseSettings() { }
        public static SqlConnection dbConn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\OneDrive\\المستندات\\FalakDB.mdf;Integrated Security=True;Connect Timeout=30");

        // validation functions
        internal static bool isExists(string username)
        {
            string sql = "SELECT * FROM PersonUsers WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(sql, dbConn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
             
                    return reader.Read();
                }
            }
        }
    }
}
