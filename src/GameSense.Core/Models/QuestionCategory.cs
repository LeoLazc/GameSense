namespace GameSense.Core.Models
{
    public class QuestionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
