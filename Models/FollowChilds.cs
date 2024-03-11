using System.ComponentModel.DataAnnotations;

namespace FalaKAPP.Models
{
    public class FollowChilds
    {
        [Required] public int  PersonInChargeID {  get; set; }
        [Required] public int ChildId { get; set; }
        public string TrackByApp {  get; set; }
        public string TrackByDevice { get; set; }
        public string HasCard { get; set; }
        public string TrackingActiveType { get; set; }
        public string AllowTorack { get; set; }

    }
}
