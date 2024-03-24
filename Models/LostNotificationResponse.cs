using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace FalaKAPP.Models
{
    public class LostNotificationResponse
    {
        [JsonIgnore]
        public int LostNotificationResponseID { get; set; }
        [Required]
        public int LostNotificationRequestID { get; set; }
        [Required]
        public int ResponseByPersonID { get; set; }
        [Required]
        public string ResponseStatus { get; set; }
        [Required]
        public DateTime ResponseDate { get; set; }
        [Required]
        public string MyLocation { get; set; }

        public string CurrentImagePath { get; set; }
        public int accuracy { get; set; }
        public string Comments { get; set; }

    }
}
