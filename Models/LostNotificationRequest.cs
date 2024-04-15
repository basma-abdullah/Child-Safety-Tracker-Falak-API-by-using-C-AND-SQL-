using Humanizer;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace FalaKAPP.Models
{
    public class LostNotificationRequest
    {

            [JsonIgnore]
            public int LostNotificationRequestID { get; set; }
            [Required]public string requestTitle {  get; set; }
            [Required]public int mainPersonInChargeID { get; set; }
            [Required] public int TrackingChildMasterID { get; set; }
            [Required] public DateTime RequestLostNotificationDate { get; set; }
            public int LastLocationId { get; set; }
            [Required] public string NotificationStatus { get; set; }
            public string Comments { get; set; }
            public int LastResponseBy { get; set; }
            public int FoundBy { get; set; }

    }
}

