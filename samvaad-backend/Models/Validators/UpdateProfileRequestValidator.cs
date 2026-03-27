using FluentValidation;
using samvaad_backend.Models.DTOs.Users;

namespace samvaad_backend.Models.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_.]{3,50}$").WithMessage("Username may only contain letters, digits, underscores and dots (3–50 chars).")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Bio)
            .MaximumLength(300).WithMessage("Bio cannot exceed 300 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500).WithMessage("Cover image URL cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl));
    }
}
