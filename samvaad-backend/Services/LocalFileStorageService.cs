namespace samvaad_backend.Services;

public class LocalFileStorageService(IWebHostEnvironment env) : Interfaces.IFileStorageService
{
    public async Task<string> SaveAsync(IFormFile file, string subfolder = "uploads")
    {
        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(root, subfolder);
        Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(dir, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/{subfolder}/{fileName}";
    }

    public void Delete(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return;
        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(root, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
