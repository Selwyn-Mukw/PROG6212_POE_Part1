using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("USER", Schema = "dbo")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Participant";

        [StringLength(500)]
        public string? ProfilePictureURL { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Event> OrganisedEvents { get; set; }
            = new List<Event>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}