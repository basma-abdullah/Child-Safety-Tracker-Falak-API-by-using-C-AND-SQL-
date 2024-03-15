using System.Drawing;

namespace FalaKAPP.Models
{
    public class PersonChilds
    {
        public int ChildID { get; set; }
        public int YearOfBirth { get; set; }
        public string MainImagePath { get; set; }
        public string VerificationCode { get; set; }
        public string QRCode { get; set; }
        public string KinshipT { get; set; }
        public int MainPersonInChargeID { get; set; }
        public string ChildStatus { get; set; }
        public int Boundry {  get; set; }
        public string LastLocation { get; set; }
        public string todayimagePath { get; set; }
        public string AdditionalInformation { get; set; }
    }
}
