namespace RaceDay.Mvc.ViewModels
{
    public class ProfileVm
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? ProfilePictureURL { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}