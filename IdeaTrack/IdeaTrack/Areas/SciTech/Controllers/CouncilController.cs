using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class CouncilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouncilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SciTech/Council
        public async Task<IActionResult> Index(int? id)
        {
            var vm = new BoardManagementVM();

            // Get list of boards
            vm.Boards = await _context.Boards
                .Include(b => b.Members)
                .OrderBy(b => b.FiscalYear)
                .ToListAsync();

            // Default ID: If id is null, use Id of first element
            vm.SelectedBoardId = id ?? vm.Boards.FirstOrDefault()?.Id;

            if (vm.SelectedBoardId.HasValue)
            {
                vm.SelectedBoard = await _context.Boards
                    .Include(b => b.Members)
                        .ThenInclude(m => m.User)
                            .ThenInclude(u => u.Department)
                    .FirstOrDefaultAsync(b => b.Id == vm.SelectedBoardId);
            }

            return View("~/Areas/SciTech/Views/Port/Councils.cshtml", vm);
        }

        // GET: /SciTech/Council/SearchUsers
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var query = _context.Users.Include(u => u.Department).AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(u => u.FullName.Contains(term) || u.Email.Contains(term));
            }

            var users = await query
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    departmentName = u.Department.Name
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }

        // POST: /SciTech/Council/SaveBoard
        [HttpPost]
        public async Task<IActionResult> SaveBoard(Board model)
        {
            try
            {
                if (model.Id == 0)
                {
                    // Create new
                    model.IsActive = true;
                    _context.Boards.Add(model);
                }
                else
                {
                    // Update
                    var boardInDb = await _context.Boards.FindAsync(model.Id);
                    if (boardInDb == null) return Json(new { success = false, message = "Council not found" });

                    boardInDb.BoardName = model.BoardName;
                    boardInDb.FiscalYear = model.FiscalYear;
                    boardInDb.Description = model.Description;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /SciTech/Council/DeleteBoard
        [HttpPost]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            var board = await _context.Boards
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null) return Json(new { success = false, message = "Council does not exist" });

            // Remove members first to avoid DB constraint errors
            if (board.Members != null && board.Members.Any())
            {
                _context.BoardMembers.RemoveRange(board.Members);
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /SciTech/Council/RemoveMember
        [HttpPost]
        public async Task<IActionResult> RemoveMember(int memberId)
        {
            var member = await _context.BoardMembers.FindAsync(memberId);
            if (member == null) return Json(new { success = false, message = "Member not found" });

            _context.BoardMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /SciTech/Council/AddMember
        [HttpPost]
        public async Task<IActionResult> AddMember(int boardId, int userId)
        {
            // Check for duplicate
            var exists = await _context.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);

            if (exists) return Json(new { success = false, message = "This personnel is already in the council." });

            var member = new BoardMember
            {
                BoardId = boardId,
                UserId = userId,
                Role = BoardRole.Member // Default to Member
            };

            _context.BoardMembers.Add(member);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /SciTech/Council/UpdateMemberRole
        [HttpPost]
        public IActionResult UpdateMemberRole(int memberId, int role)
        {
            try
            {
                var member = _context.BoardMembers.FirstOrDefault(m => m.Id == memberId);

                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found" });
                }

                member.Role = (BoardRole)role;
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
