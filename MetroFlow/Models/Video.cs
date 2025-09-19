using System;

namespace MetroFlow.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }

        // Add these properties
        public int Duration { get; set; } // Duration in seconds
        public DateTime UploadTime { get; set; }

        // Helper property to get formatted duration
        public string FormattedDuration
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(Duration);
                return $"{(int)time.TotalMinutes}:{time.Seconds:00}";
            }
        }

        // Helper property to get relative time string
        public string TimeAgo
        {
            get
            {
                TimeSpan timeSince = DateTime.Now - UploadTime;

                if (timeSince.TotalMinutes < 1)
                    return "just now";
                if (timeSince.TotalMinutes < 60)
                    return $"{(int)timeSince.TotalMinutes} minutes ago";
                if (timeSince.TotalHours < 24)
                    return $"{(int)timeSince.TotalHours} hours ago";
                if (timeSince.TotalDays < 30)
                    return $"{(int)timeSince.TotalDays} days ago";

                return $"{(int)(timeSince.TotalDays / 30)} months ago";
            }
        }
    }
}