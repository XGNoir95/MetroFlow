namespace MetroFlow.Models
{
    public class LostItem
    {
        public int LostItemId { get; set; }
        public string? Description { get; set; }
        public DateTime DateLost { get; set; }
        public string? LocationLost { get; set; }
        public string? ContactInfo { get; set; }
        public bool IsClaimed { get; set; }
    }
}
