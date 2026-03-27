namespace samvaad_backend.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file to the local wwwroot/uploads directory.
    /// Returns the relative URL path that can be served statically (e.g. /uploads/abc.png).
    /// </summary>
    Task<string> SaveAsync(IFormFile file, string subfolder = "uploads");

    /// <summary>Deletes a file given its relative URL path.</summary>
    void Delete(string relativeUrl);
}
