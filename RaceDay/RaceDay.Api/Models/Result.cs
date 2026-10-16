using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("RESULT", Schema = "dbo")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan FinishTime { get; set; }

        public int FinishPosition { get; set; }

        [Required]
        [StringLength(30)]
        public string ResultStatus { get; set; } = string.Empty;

        [ForeignKey(nameof(EnrolmentID))]
        public Enrolment Enrolment { get; set; } = null!;
    }
}