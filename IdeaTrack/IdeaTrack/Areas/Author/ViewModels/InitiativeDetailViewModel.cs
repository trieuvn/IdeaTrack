namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;

    public class InitiativeDetailViewModel
    {
        public Initiative Initiative { get; set; } = new();
        public List<InitiativeFile> Files { get; set; } = new();
    }
}
