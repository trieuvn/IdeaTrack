namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE STATUS - Trạng thái Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Defines the workflow states an initiative passes through from
    /// draft to final approval/rejection.
    /// 
    /// WORKFLOW:
    /// Draft → Pending → Faculty_Approved → Evaluating → Pending_Final → Approved
    ///                                                                 → Rejected
    ///                 → Revision_Required (can return to any previous state)
    ///                 → Re_Evaluating (for additional evaluation rounds)
    /// </summary>
    public enum InitiativeStatus
    {
        /// <summary>
        /// Draft - Author is still composing the initiative.
        /// Not visible to reviewers. Can be edited freely.
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Pending - Submitted by author, waiting for Faculty Leader review.
        /// Faculty Leader can approve (→Faculty_Approved) or reject (→Rejected).
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// Faculty_Approved - Faculty Leader has approved.
        /// Waiting for OST (Office of Science & Technology) to assign to council.
        /// </summary>
        Faculty_Approved = 2,
        
        /// <summary>
        /// Evaluating - Currently being scored by council members.
        /// Board members are completing their evaluations.
        /// </summary>
        Evaluating = 3,
        
        /// <summary>
        /// Re_Evaluating - Undergoing additional evaluation round.
        /// Used when Chairman requests new round of scoring.
        /// </summary>
        Re_Evaluating = 4,
        
        /// <summary>
        /// Revision_Required - Returned to author for corrections.
        /// Author must address issues and resubmit.
        /// </summary>
        Revision_Required = 5,
        
        /// <summary>
        /// Pending_Final - All evaluations complete, waiting for Chairman decision.
        /// Chairman reviews scores and makes final Approved/Rejected decision.
        /// </summary>
        Pending_Final = 6,
        
        /// <summary>
        /// Approved - Initiative has been officially approved.
        /// Final state - eligible for rewards/recognition.
        /// </summary>
        Approved = 7,
        
        /// <summary>
        /// Rejected - Initiative was not approved.
        /// Final state - no further action possible.
        /// </summary>
        Rejected = 8
    }
}
