using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("ENROLMENT", Schema = "dbo")]
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        [Required]
        [StringLength(30)]
        [Column(TypeName = "varchar(30)")]
        public string EnrolmentStatus { get; set; } = string.Empty;

        [ForeignKey(nameof(ParticipantID))]
        public User Participant { get; set; } = null!;

        [ForeignKey(nameof(EventID))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(CategoryID))]
        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}