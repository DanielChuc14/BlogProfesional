using BlogPlatform.Application.DTOs.Community;
using FluentValidation;

namespace BlogPlatform.Application.Validators.Community;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required.")
            .MaximumLength(2000).WithMessage("Comment body must not exceed 2000 characters.");
    }
}
