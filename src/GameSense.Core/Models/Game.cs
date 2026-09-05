namespace GameSense.Core.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public int? FranchiseId { get; set; }
        public string? ExternalId { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? FranchiseExternalId { get; set; }
        public string? FranchiseName { get; set; }
        public string? CatalogProvider { get; set; }
        public DateTime? LastSyncedAt { get; set; }

        public Franchise? Franchise { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<GotyNominee> GotyNominations { get; set; } = new List<GotyNominee>();
        public ICollection<GotyPrediction> GotyPredictions { get; set; } = new List<GotyPrediction>();
    }
}
