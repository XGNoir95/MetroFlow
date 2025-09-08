namespace MetroFlow.Models
{
    public class TicketOption
    {
        public int Id { get; set; }
        public string Type { get; set; } // "SingleJourney", "MRTPass"
        public string Title { get; set; }
        public string Description { get; set; }
        public List<PurchaseMethod> PurchaseMethods { get; set; }
    }
}