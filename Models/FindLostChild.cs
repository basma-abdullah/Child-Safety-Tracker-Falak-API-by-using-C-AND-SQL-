using Humanizer;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FalaKAPP.Models
{
    public class FindLostChild
    {
        public int FindLostChildID { get; set; }
        [Required] public string responesTitle { get; set; }
        [Required] public int HelperID { get; set; }
        [Required] public DateTime FindLostChildDate { get; set; }
        public int ApproximateAge { get; set; }
        public string responseImagePath { get; set; }
        [Required] public string NotificationStatus { get; set; }
        public string Comments { get; set; }

    }

    public class FindLostChildInput
    {
        [Required]
        public string responesTitle { get; set; }

        [Required]
        public int HelperID { get; set; }

        public int ApproximateAge { get; set; }

        [Required]
        public string NotificationStatus { get; set; }

        public string Comments { get; set; }
    }
}

