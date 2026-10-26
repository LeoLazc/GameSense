using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Core.Models;

namespace GameSense.Api.Mapping;

public sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
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
            .ForCtorParam(nameof(ReviewResponseDto.Username), opt => opt.MapFrom(s => s.User.Username))
            .ForCtorParam(nameof(ReviewResponseDto.GameTitle), opt => opt.MapFrom(s => s.GameTitle))
            .ForCtorParam(nameof(ReviewResponseDto.GameImageUrl), opt => opt.MapFrom(s => s.GameImageUrl));
    }
}
