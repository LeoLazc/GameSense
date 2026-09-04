using FluentValidation;
using GameSense.Api.Requests;

namespace GameSense.Api.Validators
{
    public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(query => query.Id)
                .GreaterThan(0)
                .WithMessage("User id must be greater than zero.");
        }
    }
}
