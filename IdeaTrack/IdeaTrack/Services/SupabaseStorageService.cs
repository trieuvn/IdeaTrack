using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IdeaTrack.Services
{
    /// <summary>
    /// Implementation of Supabase Storage operations
    /// Uses Supabase REST API for file uploads
    /// </summary>
    public class SupabaseStorageService : ISupabaseStorageService
    {
        private readonly string _supabaseUrl;
        private readonly string _serviceKey;
        private readonly string _bucketName;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SupabaseStorageService> _logger;

        public SupabaseStorageService(
            IConfiguration configuration,
            ILogger<SupabaseStorageService> logger)
        {
            _supabaseUrl = configuration["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url not configured");
            _serviceKey = configuration["Supabase:ServiceKey"] ?? throw new ArgumentNullException("Supabase:ServiceKey not configured");
            _bucketName = configuration["Supabase:BucketName"]?.TrimEnd('.') ?? "pdf-bucket"; // Remove trailing dot
            _logger = logger;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
            _httpClient.DefaultRequestHeaders.Add("apikey", _serviceKey);
        }

        public async Task<string?> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            try
            {
                // Generate unique file name to avoid conflicts
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = $"{folderPath}/{uniqueFileName}";

                // Prepare the upload URL
                var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";

                // Get content type
                var contentType = GetContentType(extension);

                using var content = new StreamContent(fileStream);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                // Upload the file
                var response = await _httpClient.PostAsync(uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to upload file to Supabase: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                // Return the public URL
                var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{filePath}";
                
                _logger.LogInformation("File uploaded successfully: {OriginalName} -> {PublicUrl}", fileName, publicUrl);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Supabase: {FileName}", fileName);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var filePath = GetFilePathFromUrl(fileUrl);
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogWarning("Invalid file URL: {FileUrl}", fileUrl);
                    return false;
                }

                // Prepare the delete URL
                var deleteUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";
                var response = await _httpClient.DeleteAsync(deleteUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete file from Supabase: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Supabase: {FileUrl}", fileUrl);
                return false;
            }
        }

        public string GetFilePathFromUrl(string fileUrl)
        {
            // URL format: {supabaseUrl}/storage/v1/object/public/{bucketName}/{filePath}
            var publicPrefix = $"/storage/v1/object/public/{_bucketName}/";
            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath;
            
            if (path.Contains(publicPrefix))
            {
                return path.Substring(path.IndexOf(publicPrefix) + publicPrefix.Length);
            }

            return string.Empty;
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
