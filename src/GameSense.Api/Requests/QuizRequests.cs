using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests;

public sealed record StartQuizCommand(int UserId) : IRequest<QuizSessionDto>;
public sealed record SubmitQuizAnswerCommand(int UserId, int SessionId, int QuestionId, string AnswerText) : IRequest<QuizAnswerResponseDto?>;
public sealed record GetQuizResultQuery(int UserId, int SessionId) : IRequest<QuizResultDto?>;
public sealed record GenerateReviewsCommand(int UserId, ReviewRequestDto Request) : IRequest<List<ReviewResponseDto>>;
