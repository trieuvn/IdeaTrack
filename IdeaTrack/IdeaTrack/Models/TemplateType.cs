namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// TEMPLATE TYPE - Loại Mẫu Đánh giá
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Classifies evaluation templates by their function in the workflow.
    /// Different types are used at different stages of review.
    /// 
    /// USAGE:
    /// - Scoring: Used by final council for detailed point-based evaluation
    /// - Screening: Used by faculty leaders for initial pass/fail review
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// Scoring (Chấm điểm) - For detailed evaluation by council.
        /// Used at university level. Contains multiple criteria with points.
        /// Produces numerical score and percentage for ranking.
        /// </summary>
        Scoring = 1,
        
        /// <summary>
        /// Screening (Sàng lọc) - For faculty-level initial review.
        /// Used for quick Approve/Reject decisions.
        /// May have basic criteria checklist without detailed scoring.
        /// </summary>
        Screening = 2
    }
}
