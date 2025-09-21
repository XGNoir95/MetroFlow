namespace MetroFlow.Models
{
    public class Distress
    {
        public int DistressId { get; set; }

        // Foreign Key to User (from Identity)
        public int UserId { get; set; }

        // Type of Emergency (Medical, Security, Fire, Other)
        public string EmergencyType { get; set; } = "";

        // Station where the incident occurred
        public string StationName { get; set; } = "";

        // Additional details provided by the user
        public string Description { get; set; } = "";

        // Optional: severity level (Low, Medium, High, Critical)
        public string Severity { get; set; } = "Medium";

        // When the report was submitted
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        // Status workflow: Pending → In Progress → Resolved
        public string Status { get; set; } = "Pending";

        // Navigation property (if using ASP.NET Identity)
        public virtual User? User { get; set; }
    }
}
