using System.ComponentModel.DataAnnotations;

namespace FalaKAPP.Models
{
    public class TrackingChildMaster
    {
        [Required] public int LinkChildsID { get; set; }
        [Required] public DateOnly StartTrackingDate { get; set; }
        public string StartLocation { get; set; }
        public DateOnly EndTrackingTim { get; set; }
        public string ChildTrackingStatues { get; set; }
        public string ParentReaction { get; set; }
        public string LastLocation { get; set; }
    }
}
