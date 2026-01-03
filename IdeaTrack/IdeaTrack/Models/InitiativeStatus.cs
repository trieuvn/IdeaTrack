namespace IdeaTrack.Models
{
    /// <summary>
    /// Trạng thái workflow của sáng kiến
    /// </summary>
    public enum InitiativeStatus
    {
        /// <summary>Bản nháp - Đang soạn thảo</summary>
        Draft = 0,
        
        /// <summary>Đã nộp, chờ Lãnh đạo Khoa duyệt</summary>
        Pending = 1,
        
        /// <summary>Khoa đã duyệt, chờ Phòng KHCN sơ duyệt</summary>
        Faculty_Approved = 2,
        
        /// <summary>Đang được Hội đồng chấm điểm</summary>
        Evaluating = 3,
        
        /// <summary>Đang chấm lại (vòng mới)</summary>
        Re_Evaluating = 4,
        
        /// <summary>Yêu cầu chỉnh sửa - Trả về cho tác giả</summary>
        Revision_Required = 5,
        
        /// <summary>Hội đồng đã chấm xong, chờ quyết định cuối cùng</summary>
        Pending_Final = 6,
        
        /// <summary>Đã được phê duyệt</summary>
        Approved = 7,
        
        /// <summary>Bị từ chối</summary>
        Rejected = 8
    }
}
