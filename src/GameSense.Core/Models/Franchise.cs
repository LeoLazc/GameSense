namespace GameSense.Core.Models
{
    public class Franchise
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Navigation
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
