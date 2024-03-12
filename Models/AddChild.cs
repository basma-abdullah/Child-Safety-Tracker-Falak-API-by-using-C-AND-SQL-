using System.ComponentModel.DataAnnotations;
namespace FalaKAPP.Models
{
    public class AddChild
    {
        //Child Name -PU
        [Required] public string Username { get; set; }
        [Required] public string UserType { get; set; }
        [Required] public string FullName { get; set; }
        [Required] public string Password { get; set; }
        public int PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        [Required] public string UsernameType { get; set; }

        //KinshipT -PC
        public string KinshipT { get; set; }
        //Email -PU

        public int YearOfBirth { get; set; }

        // image -PCM
        public string ImagePath { get; set; }
    }
}
