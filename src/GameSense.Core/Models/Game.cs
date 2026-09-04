namespace GameSense.Core.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public int FranchiseId { get; set; }

        public Franchise? Franchise { get; set; }
    }
}
