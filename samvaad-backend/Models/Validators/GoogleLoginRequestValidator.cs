using FluentValidation;
using samvaad_backend.Models.DTOs.Auth;

namespace samvaad_backend.Models.Validators;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.Credential).NotEmpty().WithMessage("Google credential is required.");
    }
}
