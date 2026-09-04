using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameSense.Core.Models;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Services
{
    internal class AiFranchise
    {
        public string? name { get; set; }
        public List<AiQuestion>? questions { get; set; }
        public AiReview? review { get; set; }
    }

    internal class AiQuestion
    {
        public string? text { get; set; }
        public string? answer { get; set; }
    }

    internal class AiReview
    {
        public string? text { get; set; }
        public int? score { get; set; }
    }

    internal class AiResponse
    {
        public List<AiFranchise>? franchises { get; set; }
    }

    public class ReviewService : IReviewService
    {
        private readonly GameSenseDbContext _db;
        private readonly IAiClient _ai;

        public ReviewService(GameSenseDbContext db, IAiClient ai)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _ai = ai ?? throw new ArgumentNullException(nameof(ai));
        }

        public async Task<List<Review>> GenerateAndSaveReviewsAsync(IEnumerable<string> franchiseNames, int questionsPerFranchise, string? promptStyle = null, CancellationToken cancellationToken = default)
        {
            var raw = await _ai.GenerateReviewJsonAsync(franchiseNames, questionsPerFranchise, promptStyle, cancellationToken);

            var json = ExtractJson(raw);
            AiResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<AiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                // Throw a domain-specific exception so middleware can translate it to a proper HTTP response
                throw new GameSense.Core.Exceptions.AiResponseParseException("AI response could not be parsed as JSON.", ex);
            }

            if (parsed?.franchises == null || parsed.franchises.Count == 0)
            {
                throw new InvalidOperationException("AI response did not contain any franchises.");
            }

            var createdReviews = new List<Review>();

            foreach (var af in parsed.franchises)
            {
                if (string.IsNullOrWhiteSpace(af.name)) continue;

                var name = af.name.Trim();
                var franchise = await _db.Franchises.FirstOrDefaultAsync(f => f.Name.ToLower() == name.ToLower(), cancellationToken);
                if (franchise == null)
                {
                    franchise = new Franchise { Name = name };
                    _db.Franchises.Add(franchise);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                // Questions
                if (af.questions != null)
                {
                    foreach (var q in af.questions.Take(50))
                    {
                        if (string.IsNullOrWhiteSpace(q?.text)) continue;
                        var question = new Question
                        {
                            QuestionText = q.text.Trim(),
                            ExpectedAnswer = q.answer?.Trim() ?? string.Empty,
                            EvaluationCriteria = string.Empty,
                            QuestionWeight = 1
                        };
                        _db.Questions.Add(question);
                    }
                }

                // Review
                var review = new Review
                {
                    FranchiseId = franchise.Id,
                    ContentJson = JsonSerializer.Serialize(af, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    Score = af.review?.score
                };
                _db.Reviews.Add(review);

                await _db.SaveChangesAsync(cancellationToken);

                // Load navigation
                await _db.Entry(review).Reference(r => r.Franchise).LoadAsync(cancellationToken);
                createdReviews.Add(review);
            }

            return createdReviews;
        }

        private static string ExtractJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "{}";

            var first = raw.IndexOf('{');
            var last = raw.LastIndexOf('}');
            if (first >= 0 && last > first)
            {
                return raw.Substring(first, last - first + 1);
            }

            return raw;
        }
    }
}
