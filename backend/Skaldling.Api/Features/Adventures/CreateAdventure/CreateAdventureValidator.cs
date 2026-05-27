using FluentValidation;

namespace Skaldling.Api.Features.Adventures.CreateAdventure;

public class CreateAdventureValidator : AbstractValidator<CreateAdventureCommand>
{
    public CreateAdventureValidator()
    {
        RuleFor(c => c.HeroId).NotEmpty().WithMessage("Hero is required.");
        RuleFor(c => c.ThemeId).NotEmpty().WithMessage("Theme is required.");

        RuleFor(c => c.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Adventure title is required.")
            .MaximumLength(200).WithMessage("Adventure title cannot exceed 200 characters.");

        // Moral is optional (2026-05-26). MaximumLength accepts null.
        RuleFor(c => c.Moral)
            .MaximumLength(500).WithMessage("Moral cannot exceed 500 characters.");

        // FinaleReward is optional (2026-05-26). Same shape as Moral.
        RuleFor(c => c.FinaleReward)
            .MaximumLength(200).WithMessage("Finale reward cannot exceed 200 characters.");

        RuleFor(c => c.NarrativeStyle)
            .IsInEnum().WithMessage("Narrative style must be a valid value.");

        RuleFor(c => c.Tone)
            .IsInEnum().WithMessage("Tone must be a valid value.");

        RuleFor(c => c.Days).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Days are required.")
            .Must(days => days.Length == 5).WithMessage("An adventure must have exactly 5 days.")
            .Must(days => days.Select(d => d.DayNumber).Distinct().Count() == 5)
                .WithMessage("Day numbers must be 1 through 5, each appearing once.")
            .Must(days => days.All(d => d.DayNumber >= 1 && d.DayNumber <= 5))
                .WithMessage("Day numbers must be between 1 and 5.");

        RuleForEach(c => c.Days).ChildRules(day =>
        {
            day.RuleFor(d => d.Tasks).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Each day must have tasks.")
                .Must(tasks => tasks.Length >= 1 && tasks.Length <= 3)
                    .WithMessage("Each day must have between 1 and 3 tasks.");

            day.RuleForEach(d => d.Tasks).ChildRules(task =>
            {
                task.RuleFor(t => t.Description).Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("Task description is required.")
                    .MaximumLength(500).WithMessage("Task description cannot exceed 500 characters.");

                task.RuleFor(t => t.PointValue)
                    .GreaterThan(0).WithMessage("Task point value must be positive.")
                    .LessThanOrEqualTo(1000).WithMessage("Task point value cannot exceed 1000.");

                task.RuleFor(t => t.Difficulty)
                    .IsInEnum().WithMessage("Difficulty must be a valid value.");
            });
        });
    }
}