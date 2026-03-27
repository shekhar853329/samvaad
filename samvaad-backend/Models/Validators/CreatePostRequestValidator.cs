using FluentValidation;
using samvaad_backend.Models.DTOs.Posts;

namespace samvaad_backend.Models.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .MaximumLength(500).WithMessage("Post content cannot exceed 500 characters.");

        // We want to ensure that either Content has text OR they attached MediaFiles
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || (x.MediaFiles != null && x.MediaFiles.Count > 0))
            .WithMessage("A post must contain either text content or media attachments.");

        // Enforce 100MB limit per file
        RuleForEach(x => x.MediaFiles)
            .Must(file => file.Length <= 100 * 1024 * 1024)
            .WithMessage((request, file) => $"File '{file.FileName}' exceeds the maximum allowed size of 100MB.");
    }
}
