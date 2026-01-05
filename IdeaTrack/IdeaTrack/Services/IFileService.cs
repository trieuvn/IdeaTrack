using IdeaTrack.Models;

namespace IdeaTrack.Services
{
    public interface IFileService
    {
        Task<List<InitiativeFile>> UploadFilesAsync(List<IFormFile> files, int initiativeId);
        
        // Add delete capability if needed
        Task<bool> DeleteFileAsync(int fileId);
    }
}
