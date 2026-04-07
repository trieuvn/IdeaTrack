namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE STATUS - Trạng thái Sáng kiến
    /// ======================================================================
    /// 
    /// WORKFLOW:
    /// Draft → Pending → Faculty_Approved → Evaluating → Pending_Final → Processing → Approved
    ///                                                                              → Rejected (council)
    ///                                                                              → Rejected_SL (school-level, Approver)
    ///                 → Revision_Required (can return to any previous state)
    ///                 → Re_Evaluating (for additional evaluation rounds)
    /// </summary>
    public enum InitiativeStatus
    {
        /// <summary>Draft - Author is still composing the initiative.</summary>
        Draft = 0,

        /// <summary>Pending - Submitted by author, waiting for Faculty Leader review.</summary>
        Pending = 1,

        /// <summary>Faculty_Approved - Faculty Leader has approved. Waiting for OST.</summary>
        Faculty_Approved = 2,

        /// <summary>Evaluating - Currently being scored by council members.</summary>
        Evaluating = 3,

        /// <summary>Re_Evaluating - Undergoing additional evaluation round.</summary>
        Re_Evaluating = 4,

        /// <summary>Revision_Required - Returned to author for corrections.</summary>
        Revision_Required = 5,

        /// <summary>Pending_Final - All evaluations complete, waiting for Chairman decision.</summary>
        Pending_Final = 6,

        /// <summary>Processing - Council approved, pending final Approver sign-off.</summary>
        Processing = 7,

        /// <summary>Rejected - Initiative was not approved by council/faculty. Final state.</summary>
        Rejected = 8,

        /// <summary>Approved - Final approval by Approver. Eligible for recognition.</summary>
        Approved = 9,

        /// <summary>Rejected_SL - Rejected at school level by Approver. May allow resubmission with deadline.</summary>
        Rejected_SL = 10
    }
}
