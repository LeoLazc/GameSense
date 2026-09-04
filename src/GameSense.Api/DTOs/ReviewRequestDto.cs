using System.Collections.Generic;
using MediatR;

namespace GameSense.Api.DTOs
{
    // Make this DTO the MediatR request to keep controller thin.
    public class ReviewRequestDto : IRequest<System.Collections.Generic.List<ReviewResponseDto>>
    {
        // Names of the 3 franchises to evaluate. Expect exactly 3 entries.
        public List<string> FranchiseNames { get; set; } = new List<string>();

        // Optional: number of questions to generate per franchise (default 5)
        public int QuestionsPerFranchise { get; set; } = 5;

        // Optional: model settings or prompt style
        public string? PromptStyle { get; set; }
    }
}
