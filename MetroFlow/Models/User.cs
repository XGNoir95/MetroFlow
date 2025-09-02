namespace MetroFlow.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ImageUrl { get; set; }= "";
        public DateTime CreatedAt { get; set; }
        public string? Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}
