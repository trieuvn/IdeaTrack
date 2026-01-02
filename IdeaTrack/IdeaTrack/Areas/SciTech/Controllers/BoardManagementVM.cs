using IdeaTrack.Models;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    public class BoardManagementVM
    {
        public List<Board> Boards { get; set; } = new List<Board>();
        public Board SelectedBoard { get; set; }
        public int? SelectedBoardId { get; set; }
    }
}