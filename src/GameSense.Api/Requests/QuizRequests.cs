using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests;

public sealed record StartQuizCommand(int UserId) : IRequest<QuizSessionDto>;
public sealed record SubmitQuizAnswersCommand(int UserId, int SessionId, IReadOnlyList<SubmitQuizAnswerDto> Answers) : IRequest<QuizAnswerResponseDto?>;
public sealed record GetQuizResultQuery(int UserId, int SessionId) : IRequest<QuizResultDto?>;
