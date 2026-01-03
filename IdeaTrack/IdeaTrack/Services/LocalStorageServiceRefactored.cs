using IdeaTrack.Services;

namespace IdeaTrack.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalStorageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string ProviderName => "Local";

        public LocalStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalStorageService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            try
            {
                // Ensure unique filename
                var uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
                var relativePath = Path.Combine("uploads", folderPath);
                var folderFullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (!Directory.Exists(folderFullPath))
                {
                    Directory.CreateDirectory(folderFullPath);
                }

                var fullPath = Path.Combine(folderFullPath, uniqueFileName);

                using (var destStream = new FileStream(fullPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(destStream);
                }

                // Construct URL
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request != null 
                    ? $"{request.Scheme}://{request.Host}" 
                    : ""; 
                
                // Return relative path for portability, or absolute if needed. 
                // Using root-relative URL is best for web apps.
                var url = $"/uploads/{folderPath}/{uniqueFileName}";

                _logger.LogInformation("File uploaded locally: {FullPath}", fullPath);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file locally: {FileName}", fileName);
                return null;
            }
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Assuming URL starts with /uploads/...
                if (string.IsNullOrEmpty(fileUrl) || !fileUrl.StartsWith("/uploads/"))
                {
                    _logger.LogWarning("Invalid local file URL for deletion: {Url}", fileUrl);
                    return Task.FromResult(false);
                }

                var relativePath = fileUrl.TrimStart('/');
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted local file: {FullPath}", fullPath);
                    return Task.FromResult(true);
                }
                
                _logger.LogWarning("File not found for deletion: {FullPath}", fullPath);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting local file: {Url}", fileUrl);
                return Task.FromResult(false);
            }
        }
    }
}
