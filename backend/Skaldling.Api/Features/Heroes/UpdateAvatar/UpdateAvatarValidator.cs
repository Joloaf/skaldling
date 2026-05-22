using FluentValidation;

namespace Skaldling.Api.Features.Heroes.UpdateAvatar;

public class UpdateAvatarValidator : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarValidator()
    {
        RuleFor(c => c.SpriteIds).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Hero avatar selection is required.")
            .Must(ids => ids.Length >= 1).WithMessage("Hero avatar must include at least one sprite.")
            .Must(ids => ids.Length <= 10).WithMessage("Hero avatar cannot contain more than 10 sprites.")
            .Must(ids => ids.Distinct().Count() == ids.Length).WithMessage("Hero avatars must be unique.");
    }
}