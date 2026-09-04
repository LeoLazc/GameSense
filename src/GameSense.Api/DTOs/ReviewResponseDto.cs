using System;
using System.Collections.Generic;

namespace GameSense.Api.DTOs
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public string FranchiseName { get; set; } = null!;
        public DateTime GeneratedAt { get; set; }
        public List<ReviewQuestionDto> Questions { get; set; } = new List<ReviewQuestionDto>();
        public string ReviewText { get; set; } = null!;
        public int? Score { get; set; }
    }
}
