namespace IdeaTrack.Areas.SciTech.Models
{
    /// <summary>
    /// ViewModel for displaying co-author information
    /// </summary>
    public class CoAuthorVM
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsCreator { get; set; }
    }
}
