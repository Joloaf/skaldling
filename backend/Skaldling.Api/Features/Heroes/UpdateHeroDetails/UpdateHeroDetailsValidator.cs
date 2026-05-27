using FluentValidation;

namespace Skaldling.Api.Features.Heroes.UpdateHeroDetails;

public class UpdateHeroDetailsValidator : AbstractValidator<UpdateHeroDetailsCommand>
{
    public UpdateHeroDetailsValidator()
    {
        RuleFor(c => c.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Hero name is required.")
            .MaximumLength(100).WithMessage("Hero name cannot exceed 100 characters.")
            .Matches(@"^[\p{L}\p{N}\s\-']+$")
            .WithMessage("Hero name can only contain letters, numbers, spaces, hyphens, and apostrophes.");

        RuleFor(c => c.ReadingAge)
            .InclusiveBetween(4, 12)
            .WithMessage("Reading age must be between 4 and 12.");
    }
}