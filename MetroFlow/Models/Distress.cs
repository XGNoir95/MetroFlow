namespace MetroFlow.Models
{
    public class Distress
    {
        public int DistressId { get; set; }
        public int UserId { get; set; }
        public string StationName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = "Pending"; // e.g., Pending, In Progress, Resolved
    }
}
