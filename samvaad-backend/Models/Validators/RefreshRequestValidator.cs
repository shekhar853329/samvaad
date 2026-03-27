using FluentValidation;
using samvaad_backend.Models.DTOs.Auth;

namespace samvaad_backend.Models.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
