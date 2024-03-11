using System.ComponentModel.DataAnnotations;

namespace FalaKAPP.Models
{
    public class PersonUsers
    {
        public int UserID { get; set; }
        [Required] public string Username { get; set; }
        [Required] public string UserType { get; set; }
        [Required] public string FullName { get; set; }
        [Required] public string Password { get; set; }
        public int PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        [Required] public string UsernameType { get; set; }
    }
}
