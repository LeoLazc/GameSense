using FluentValidation;
using GameSense.Api.DTOs;

namespace GameSense.Api.Validators;

public sealed class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 100);
    }
}

public sealed class SaveGotyPredictionRequestValidator : AbstractValidator<SaveGotyPredictionRequest>
{
    public SaveGotyPredictionRequestValidator()
    {
        RuleFor(x => x.GameIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(x => x.Count == 6)
            .WithMessage("Exactly six nominee games are required.")
            .Must(x => x.Distinct().Count() == 6)
            .WithMessage("Nominee game IDs must be distinct.")
            .Must(x => x.All(id => id > 0))
            .WithMessage("Nominee game IDs must be positive.");
        RuleFor(x => x.GotyGameId).GreaterThan(0);
    }
}
