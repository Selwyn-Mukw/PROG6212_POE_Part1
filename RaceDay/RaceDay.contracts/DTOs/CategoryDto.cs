namespace RaceDay.contracts.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public int MinimumAge { get; set; }

        public int MaximumAge { get; set; }

        public decimal CategoryDistance { get; set; }
    }
}