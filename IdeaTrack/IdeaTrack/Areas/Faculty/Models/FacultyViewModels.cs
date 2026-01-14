using System;
using System.Collections.Generic;
using IdeaTrack.Models;

namespace IdeaTrack.Areas.Faculty.Models
{
    public class FacultyDashboardVM
    {
        // Status Counts
        public int PendingCount { get; set; }           // Status = Pending (1)
        public int FacultyApprovedCount { get; set; }   // Status = Faculty_Approved (2)
        public int EvaluationCount { get; set; }        // Status = Evaluating (3) OR Re_Evaluating (4)
        public int PendingFinalCount { get; set; }      // Status = Pending_Final (6)
        public int ApprovedCount { get; set; }          // Status = Approved (7)
        public int RejectedCount { get; set; }          // Status = Rejected (8)
        public int RevisionRequiredCount { get; set; }  // Status = Revision_Required (5)
        public int TotalInitiatives { get; set; }
        
        public List<FacultyInitiativeItem> Initiatives { get; set; } = new List<FacultyInitiativeItem>();
    }

    public class FacultyInitiativeItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string InitiativeCode { get; set; }
        public string ProposerName { get; set; }  // Primary author name
        public int MemberCount { get; set; }       // Total number of authors
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
        public string ProposerName { get; set; }  // Primary author name
        public List<string> Authors { get; set; } = new List<string>(); // All authors names
        public decimal Budget { get; set; }
        public DateTime SubmittedDate { get; set; }
        public InitiativeStatus Status { get; set; }
        public InitiativeCategory Category { get; set; }
        public List<InitiativeFileVM> Files { get; set; }
    }
    
    public class InitiativeFileVM
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }

    public class FacultyProgressVM
    {
        public List<Initiative> Initiatives { get; set; } = new List<Initiative>();
        
        // Filter values
        public int? SelectedYearId { get; set; }
        public int? SelectedPeriodId { get; set; }
        public int? SelectedCategoryId { get; set; }
        
        // Filter options
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Years { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Periods { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Categories { get; set; }
    }
}
