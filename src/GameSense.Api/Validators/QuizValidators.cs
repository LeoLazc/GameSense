using FluentValidation;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;

namespace GameSense.Api.Validators;

public sealed class SubmitQuizAnswerDtoValidator : AbstractValidator<SubmitQuizAnswerDto>
{
    public SubmitQuizAnswerDtoValidator()
    {
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.AnswerText).NotEmpty().MaximumLength(4000);
    }
}

public sealed class SubmitQuizAnswerCommandValidator : AbstractValidator<SubmitQuizAnswerCommand>
{
    public SubmitQuizAnswerCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.AnswerText).NotEmpty().MaximumLength(4000);
    }
}

public sealed class GetQuizResultQueryValidator : AbstractValidator<GetQuizResultQuery>
{
    public GetQuizResultQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}
