using System.ComponentModel.DataAnnotations;

namespace MetroFlow.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        [Required]
        [Url]
        public string Url { get; set; } = "";

        public string Category { get; set; } = "General";
    }
}