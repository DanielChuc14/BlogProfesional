using BlogPlatform.Application.DTOs.Posts;
using FluentValidation;

namespace BlogPlatform.Application.Validators.Posts;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title must not be empty.")
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Excerpt)
            .MaximumLength(500).WithMessage("Excerpt must not exceed 500 characters.")
            .When(x => x.Excerpt is not null);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(512).WithMessage("Cover image URL must not exceed 512 characters.")
            .When(x => x.CoverImageUrl is not null);

        RuleFor(x => x.TagIds)
            .Must(t => t!.Count <= 10).WithMessage("A post can have at most 10 tags.")
            .When(x => x.TagIds is not null);
    }
}
