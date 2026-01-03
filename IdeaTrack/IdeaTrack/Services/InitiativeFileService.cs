using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Services
{
    public class InitiativeFileService : IFileService
    {
        private readonly ILogger<InitiativeFileService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public InitiativeFileService(
            ILogger<InitiativeFileService> logger, 
            ApplicationDbContext context,
            IStorageService storageService)
        {
            _logger = logger;
            _context = context;
            _storageService = storageService;
        }

        public async Task<List<InitiativeFile>> UploadFilesAsync(List<IFormFile> files, int initiativeId)
        {
            var uploadedFiles = new List<InitiativeFile>();

            if (files == null || files.Count == 0)
                return uploadedFiles;

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        using var stream = file.OpenReadStream();
                        var fileUrl = await _storageService.UploadFileAsync(stream, file.FileName, "initiatives");

                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            var initiativeFile = new InitiativeFile
                            {
                                FileName = file.FileName,
                                FilePath = fileUrl,
                                FileType = Path.GetExtension(file.FileName).ToLower(),
                                FileSize = file.Length,
                                UploadDate = DateTime.Now,
                                InitiativeId = initiativeId
                            };

                            _context.InitiativeFiles.Add(initiativeFile);
                            uploadedFiles.Add(initiativeFile);
                            
                            _logger.LogInformation("File {FileName} uploaded for initiative {InitiativeId} via {Provider}", 
                                file.FileName, initiativeId, _storageService.ProviderName);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "Error saving file {FileName} for initiative {InitiativeId}", file.FileName, initiativeId);
                        // Continue uploading other files
                    }
                }
            }

            if (uploadedFiles.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return uploadedFiles;
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _context.InitiativeFiles.FindAsync(fileId);
            if (file == null) return false;

            var success = await _storageService.DeleteFileAsync(file.FilePath);
            // Even if storage delete fails (maybe file missing), remove DB record
            _context.InitiativeFiles.Remove(file);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
