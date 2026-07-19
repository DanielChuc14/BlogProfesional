using BlogPlatform.Application.DTOs.Profile;
using FluentValidation;

namespace BlogPlatform.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name must not be empty.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters.")
            .When(x => x.Bio is not null);

        RuleFor(x => x.About)
            .MaximumLength(2000).WithMessage("About must not exceed 2000 characters.")
            .When(x => x.About is not null);

        RuleForEach(x => x.SocialLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.Platform)
                .NotEmpty().WithMessage("Platform is required.")
                .MaximumLength(50);

            link.RuleFor(l => l.Url)
                .NotEmpty().WithMessage("URL is required.")
                .MaximumLength(512)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("URL must be a valid absolute URL.");
        });
    }
}
