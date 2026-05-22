using FluentValidation;

namespace Skaldling.Api.Features.Heroes.CreateHero;

public class CreateHeroValidator : AbstractValidator<CreateHeroCommand>
{
    public CreateHeroValidator()
    {
        RuleFor(c => c.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Hero name is required.")
            .MaximumLength(100).WithMessage("Hero name cannot exceed 100 characters.")
            /* Allowing hero names to include letters (covering a wider range than just English), numbers,
            spaces, hyphens and apostrophes - No special characters beyond the bare minimum for names. */
            .Matches(@"^[\p{L}\p{N}\s\-']+$")
                .WithMessage("Hero name can only contain letters, numbers, spaces, hyphens, and apostrophes.");

        RuleFor(c => c.SpriteIds).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Hero avatar selection is required.")
            .Must(ids => ids.Length >= 1).WithMessage("Hero avatar must include at least one sprite.")
            .Must(ids => ids.Length <= 10).WithMessage("Hero avatar cannot contain more than 10 sprites.")
            .Must(ids => ids.Distinct().Count() == ids.Length).WithMessage("Hero avatars must be unique.");
    }
}