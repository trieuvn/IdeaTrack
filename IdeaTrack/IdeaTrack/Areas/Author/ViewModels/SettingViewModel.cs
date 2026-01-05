namespace IdeaTrack.Areas.Author.ViewModels
{
    public class SettingViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? AcademicRank { get; set; }
        public string? Degree { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public bool EmailAlertsEnabled { get; set; } = false;
    }
}
