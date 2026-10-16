using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("CATEGORY", Schema = "dbo")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CategoryType { get; set; } = string.Empty;

        public int MinimumAge { get; set; }

        public int MaximumAge { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal CategoryDistance { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event Event { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}