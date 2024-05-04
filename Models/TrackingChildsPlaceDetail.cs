using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;


namespace FalaKAPP.Models
{
    public class TrackingChildsPlaceDetail
    {
        [Required]
        public int TrackingChildMasterID { get; set; }
        [Required]
        public DateTime? DateTime { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
        
    }
}
