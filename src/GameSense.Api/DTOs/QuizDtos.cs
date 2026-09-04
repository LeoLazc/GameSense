namespace GameSense.Api.DTOs;

public sealed record QuizQuestionDto(int Id, int CategoryId, int Difficulty, string QuestionText, decimal QuestionWeight);
public sealed record QuizProgressDto(int Answered, int Total);
public sealed record QuizSessionDto(int Id, DateTime StartedAt, DateTime? CompletedAt, string Status, decimal? FinalScore, IReadOnlyList<QuizQuestionDto> Questions, QuizProgressDto Progress);
public sealed record SubmitQuizAnswerDto(int QuestionId, string AnswerText);
public sealed record QuizAnswerDto(int Id, int QuestionId, string AnswerText, decimal Score, decimal Confidence, string Evaluation, DateTime AnsweredAt);
public sealed record QuizResultDto(int Id, DateTime StartedAt, DateTime? CompletedAt, string Status, decimal? FinalScore, decimal? ExpertiseScore, IReadOnlyList<QuizAnswerDto> Answers);
public sealed record QuizAnswerResponseDto(bool Completed, QuizProgressDto Progress, QuizQuestionDto? NextQuestion, QuizResultDto? Result);
