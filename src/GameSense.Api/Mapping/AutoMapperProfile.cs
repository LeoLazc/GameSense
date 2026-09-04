using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Models;

namespace GameSense.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Request DTO is handled directly by MediatR handler; no command mapping needed.

            CreateMap<User, UserResponseDto>();
            CreateMap<Question, QuizQuestionDto>();
            CreateMap<QuizAnswer, QuizAnswerDto>()
                .ForCtorParam(nameof(QuizAnswerDto.Score), opt => opt.MapFrom(s => s.AiScore))
                .ForCtorParam(nameof(QuizAnswerDto.Confidence), opt => opt.MapFrom(s => s.AiConfidence))
                .ForCtorParam(nameof(QuizAnswerDto.Evaluation), opt => opt.MapFrom(s => s.AiEvaluation));
            CreateMap<QuizSession, QuizResultDto>()
                .ForCtorParam(nameof(QuizResultDto.ExpertiseScore), opt => opt.MapFrom(s => s.User == null ? null : s.User.ExpertiseScore))
                .ForCtorParam(nameof(QuizResultDto.Answers), opt => opt.MapFrom(s => s.QuizAnswers));

            CreateMap<Review, ReviewResponseDto>()
                .ForMember(d => d.FranchiseName, opt => opt.MapFrom(s => s.Franchise != null ? s.Franchise.Name : string.Empty))
                .ForMember(d => d.Questions, opt => opt.MapFrom<ContentQuestionsResolver>())
                .ForMember(d => d.ReviewText, opt => opt.MapFrom<ContentTextResolver>());
        }
    }

    internal class ContentQuestionsResolver : IValueResolver<Review, ReviewResponseDto, List<ReviewQuestionDto>>
    {
        private readonly ILogger<ContentQuestionsResolver> _logger;

        public ContentQuestionsResolver(ILogger<ContentQuestionsResolver> logger)
        {
            _logger = logger;
        }

        public List<ReviewQuestionDto> Resolve(Review source, ReviewResponseDto destination, List<ReviewQuestionDto> destMember, ResolutionContext context)
        {
            var list = new List<ReviewQuestionDto>();
            if (string.IsNullOrWhiteSpace(source.ContentJson)) return list;

            try
            {
                using var doc = JsonDocument.Parse(source.ContentJson);
                JsonElement root = doc.RootElement;
                JsonElement target;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("franchises", out var f) && f.ValueKind == JsonValueKind.Array)
                {
                    target = f.EnumerateArray().FirstOrDefault();
                }
                else
                {
                    target = root;
                }

                if (target.ValueKind == JsonValueKind.Object && target.TryGetProperty("questions", out var qProp) && qProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var q in qProp.EnumerateArray())
                    {
                        var text = q.GetProperty("text").GetString() ?? string.Empty;
                        var answer = q.TryGetProperty("answer", out var a) ? a.GetString() : null;
                        list.Add(new ReviewQuestionDto { Text = text, Answer = answer });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log parsing issues for observability and return empty fallback
                _logger.LogWarning(ex, "Failed to parse ContentJson for Review.Id={ReviewId}", source.Id);
            }

            return list;
        }
    }

    internal class ContentTextResolver : IValueResolver<Review, ReviewResponseDto, string>
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ContentTextResolver> _logger;

        public ContentTextResolver(Microsoft.Extensions.Logging.ILogger<ContentTextResolver> logger)
        {
            _logger = logger;
        }

        public string Resolve(Review source, ReviewResponseDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.ContentJson)) return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(source.ContentJson);
                JsonElement root = doc.RootElement;
                JsonElement target;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("franchises", out var f) && f.ValueKind == JsonValueKind.Array)
                {
                    target = f.EnumerateArray().FirstOrDefault();
                }
                else
                {
                    target = root;
                }

                if (target.ValueKind == JsonValueKind.Object && target.TryGetProperty("review", out var r) && r.ValueKind == JsonValueKind.Object)
                {
                    return r.GetProperty("text").GetString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract review text from ContentJson for Review.Id={ReviewId}", source.Id);
            }

            return source.ContentJson;
        }
    }
}
