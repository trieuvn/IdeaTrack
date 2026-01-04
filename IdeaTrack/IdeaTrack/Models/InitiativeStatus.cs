namespace IdeaTrack.Models
{
    /// <summary>
    /// Status workflow cua sang kien
    /// </summary>
    public enum InitiativeStatus
    {
        /// <summary>Ban nhap - Dang soan thao</summary>
        Draft = 0,
        
        /// <summary>Da nop, cho Lanh dao Khoa duyet</summary>
        Pending = 1,
        
        /// <summary>Khoa da duyet, cho Phong KHCN so duyet</summary>
        Faculty_Approved = 2,
        
        /// <summary>Dang duoc Hoi dong cham diem</summary>
        Evaluating = 3,
        
        /// <summary>Dang cham lai (vong moi)</summary>
        Re_Evaluating = 4,
        
        /// <summary>Request Revision - Tra ve cho tac gia</summary>
        Revision_Required = 5,
        
        /// <summary>Hoi dong da cham xong, cho quyet dinh cuoi cung</summary>
        Pending_Final = 6,
        
        /// <summary>Da duoc phe duyet</summary>
        Approved = 7,
        
        /// <summary>Bi tu choi</summary>
        Rejected = 8
    }
}
