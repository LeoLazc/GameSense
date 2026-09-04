using System;

namespace GameSense.Core.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int FranchiseId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Raw structured JSON content produced by the AI (questions + review body)
        public string ContentJson { get; set; } = null!;

        // Optional aggregated score (0-100) produced by the AI
        public int? Score { get; set; }

        // Navigation
        public Franchise? Franchise { get; set; }
    }
}
