using System;
using System.Collections.Generic;
using IdeaTrack.Models;

namespace IdeaTrack.Areas.Faculty.Models
{
    public class FacultyDashboardVM
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalInitiatives { get; set; }
        public List<FacultyInitiativeItem> Initiatives { get; set; } = new List<FacultyInitiativeItem>();
    }

    public class FacultyInitiativeItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string InitiativeCode { get; set; }
        public string ProposerName { get; set; }
        public InitiativeCategory Category { get; set; }
        public InitiativeStatus Status { get; set; }
        public DateTime SubmittedDate { get; set; }
    }

    public class FacultyInitiativeDetailVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string InitiativeCode { get; set; }
        public string Description { get; set; }
        public string ProposerName { get; set; }
        public decimal Budget { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string Status { get; set; }
        public InitiativeCategory Category { get; set; }
        public List<InitiativeFileVM> Files { get; set; }
    }
    
    public class InitiativeFileVM
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
}
