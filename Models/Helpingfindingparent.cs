using System.ComponentModel.DataAnnotations;

namespace FalaKAPP.Models
{
    public class Helpingfindingparent
    {
        [Required] public int HelpingFindingparentID { get; set; }
        [Required] public DateOnly HelpDate { get; set; }
        public string HelpingStatus { get; set; }
        public string currentImagePath { get; set; }
        public int accuracy { get; set; }
        public int HelperPersonID { get; set; }
        [Required] public int LinkChildsID { get; set; }
  

    }
}


