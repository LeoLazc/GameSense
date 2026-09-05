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
        CreateMap<Review, ReviewResponseDto>().ForCtorParam(nameof(ReviewResponseDto.Username), opt => opt.MapFrom(s => s.User.Username));
        CreateMap<Game, GameResponseDto>()
            .ForCtorParam(nameof(GameResponseDto.FranchiseName), opt => opt.MapFrom(s => s.Franchise == null ? s.FranchiseName : s.Franchise.Name))
            .ForCtorParam(nameof(GameResponseDto.Reviews), opt => opt.MapFrom(s => s.Reviews));
        CreateMap<GotyNominee, GotyNomineeResponseDto>().ForCtorParam(nameof(GotyNomineeResponseDto.Name), opt => opt.MapFrom(s => s.Game.Name));
        CreateMap<GotyPrediction, GotyPredictionResponseDto>().ForCtorParam(nameof(GotyPredictionResponseDto.Nominees), opt => opt.MapFrom(s => s.Nominees));
    }
}
