namespace IdeaTrack.Services
{
    /// <summary>
    /// Generic service interface for file storage operations
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Upload a file to storage
        /// </summary>
        /// <param name="fileStream">File stream to upload</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folderPath">Folder path/Bucket (e.g., "reference-forms", "initiatives")</param>
        /// <returns>Public URL of the uploaded file</returns>
        Task<string?> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
        
        /// <summary>
        /// Delete a file from storage
        /// </summary>
        /// <param name="fileUrl">Full URL of the file to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string fileUrl);
        
        /// <summary>
        /// Get the connection string or provider type for display/debug
        /// </summary>
        string ProviderName { get; }
    }
}
