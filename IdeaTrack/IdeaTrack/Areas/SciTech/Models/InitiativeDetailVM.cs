using IdeaTrack.Models;
using System;
using System.Collections.Generic;

namespace IdeaTrack.Areas.SciTech.Models
{
    public class InitiativeDetailVM
    {
        public int Id { get; set; }
        public string InitiativeCode { get; set; }
        public string Title { get; set; }
        public string ProposerName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public InitiativeStatus Status { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Note { get; set; }
        public decimal Budget { get; set; }
        public string Code { get; set; }

        public List<InitiativeFileVM> Files { get; set; } = new List<InitiativeFileVM>();
    }

    
}
