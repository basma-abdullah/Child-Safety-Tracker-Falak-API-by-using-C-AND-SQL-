using Humanizer;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FalaKAPP.Models
{
    public class permissionToFollow
    {
        [JsonIgnore]
        public int PermissionID {  get; set; }
        [Required]
        public int PersonInChargeID {  get; set; }
        [Required]
        public int PermissionPersonID {  get; set; }
        [Required]
        public int ChildID { get; set; }
        [Required]
        public string KinshipT {  get; set; }
        [Required]
        public string PermissionActivationStatus { get; set; }
    }
}
