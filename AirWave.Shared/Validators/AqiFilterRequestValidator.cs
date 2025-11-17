using AirWave.Shared.Models;
using FluentValidation;

namespace AirWave.Shared.Validators;

public class AqiFilterRequestValidator : AbstractValidator<AqiFilterRequest>
{
    public AqiFilterRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after or equal to start date");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.StartDate.HasValue)
            .WithMessage("Start date cannot be in the future");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date cannot be in the future");
    }
}
