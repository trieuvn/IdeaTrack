namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// ERROR VIEW MODEL - Model cho trang lỗi
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Standard ASP.NET Core model for displaying error pages.
    /// Contains request tracking information for debugging.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unique identifier for the failed request.
        /// Used for tracking/debugging in logs.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Whether to display the RequestId to users.
        /// True if RequestId has a value.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
