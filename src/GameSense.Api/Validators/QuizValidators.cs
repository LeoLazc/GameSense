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

public sealed class SubmitQuizAnswersCommandValidator : AbstractValidator<SubmitQuizAnswersCommand>
{
    public SubmitQuizAnswersCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.Answers)
            .NotEmpty()
            .Must(answers => answers.Select(answer => answer.QuestionId).Distinct().Count() == answers.Count)
            .WithMessage("Each question may only be submitted once.");
        RuleForEach(x => x.Answers).SetValidator(new SubmitQuizAnswerDtoValidator());
    }
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
