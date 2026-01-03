namespace IdeaTrack.Services
{
    /// <summary>
    /// Service interface for Supabase Storage operations
    /// </summary>
    public interface ISupabaseStorageService
    {
        /// <summary>
        /// Upload a file to Supabase Storage
        /// </summary>
        /// <param name="fileStream">File stream to upload</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folderPath">Folder path in bucket (e.g., "reference-forms")</param>
        /// <returns>Public URL of the uploaded file</returns>
        Task<string?> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
        
        /// <summary>
        /// Delete a file from Supabase Storage
        /// </summary>
        /// <param name="fileUrl">Full URL of the file to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string fileUrl);
        
        /// <summary>
        /// Get the file path from a full URL
        /// </summary>
        string GetFilePathFromUrl(string fileUrl);
    }
}
