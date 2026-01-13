namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;

    public class InitiativeDetailViewModel
    {
        public Initiative Initiative { get; set; } = new();
        public List<InitiativeFile> Files { get; set; } = new();
        
        /// <summary>
        /// Latest revision request for displaying rejection/revision reason
        /// </summary>
        public RevisionRequest? LatestRevisionRequest { get; set; }
        
        /// <summary>
        /// Final evaluation result for approved initiatives
        /// </summary>
        public FinalResult? FinalResult { get; set; }
    }
}
