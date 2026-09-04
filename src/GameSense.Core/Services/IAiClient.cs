using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameSense.Core.Services
{
    public interface IAiClient
    {
        /// <summary>
        /// Sends a prompt to the configured AI provider and returns the raw response text (expected to contain structured JSON).
        /// </summary>
        Task<string> GenerateReviewJsonAsync(IEnumerable<string> franchiseNames, int questionsPerFranchise, string? promptStyle = null, CancellationToken cancellationToken = default);
    }
}
