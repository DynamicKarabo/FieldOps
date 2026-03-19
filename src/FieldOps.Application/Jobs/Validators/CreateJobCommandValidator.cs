using FieldOps.Application.Jobs.Commands;
using FluentValidation;

namespace FieldOps.Application.Jobs.Validators;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.JobType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.SiteAddress).NotNull();
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x)
            .Must(x => x.SiteLatitude.HasValue == x.SiteLongitude.HasValue)
            .WithMessage("SiteLatitude and SiteLongitude must be supplied together.");
    }
}
