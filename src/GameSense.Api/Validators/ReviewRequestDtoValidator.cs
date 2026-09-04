using FluentValidation;
using GameSense.Api.DTOs;

namespace GameSense.Api.Validators
{
    public class ReviewRequestDtoValidator : AbstractValidator<ReviewRequestDto>
    {
        public ReviewRequestDtoValidator()
        {
            RuleFor(x => x.FranchiseNames)
                .NotNull().WithMessage("FranchiseNames is required")
                .Must(list => list.Count == 3).WithMessage("Exactly 3 franchise names must be provided");

            RuleForEach(x => x.FranchiseNames)
                .NotEmpty().WithMessage("Franchise name cannot be empty")
                .MaximumLength(200).WithMessage("Franchise name is too long");

            RuleFor(x => x.QuestionsPerFranchise)
                .InclusiveBetween(1, 10).WithMessage("QuestionsPerFranchise must be between 1 and 10");
        }
    }
}
