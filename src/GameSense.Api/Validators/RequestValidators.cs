using FluentValidation;
using GameSense.Api.Requests;

namespace GameSense.Api.Validators;

public sealed class GetGameQueryValidator : AbstractValidator<GetGameQuery>
{
    public GetGameQueryValidator()
    {
        RuleFor(x => x.GameId).GreaterThan(0);
    }
}

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.Request)
            .NotNull()
            .SetValidator(new CreateReviewRequestValidator());
    }
}

public sealed class SaveGotyPredictionCommandValidator : AbstractValidator<SaveGotyPredictionCommand>
{
    public SaveGotyPredictionCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Request)
            .NotNull()
            .SetValidator(new SaveGotyPredictionRequestValidator());
    }
}

public sealed class GetGotyPredictionQueryValidator : AbstractValidator<GetGotyPredictionQuery>
{
    public GetGotyPredictionQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public sealed class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
{
    public StartQuizCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
