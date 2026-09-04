using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameSense.Core.Models;

namespace GameSense.Core.Services
{
    public interface IReviewService
    {
        /// <summary>
        /// Generate reviews for the provided franchise names by calling the IAiClient, parsing the structured JSON, and persisting results.
        /// Returns the created Review entities (with Franchise navigation loaded).
        /// </summary>
        Task<List<Review>> GenerateAndSaveReviewsAsync(IEnumerable<string> franchiseNames, int questionsPerFranchise, string? promptStyle = null, CancellationToken cancellationToken = default);
    }
}
