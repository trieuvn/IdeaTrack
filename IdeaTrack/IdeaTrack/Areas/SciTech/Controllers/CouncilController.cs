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

        // ==========================================
        // 1. DASHBOARD (INITIATIVE LIST)
        // ==========================================
        // GET: /SciTech/Council
        public async Task<IActionResult> Index(string searchString)
        {
            // Allowed statuses for Council Dashboard
            var allowedStatuses = new[] 
            { 
                 InitiativeStatus.Evaluating,
                 InitiativeStatus.Re_Evaluating,
                 InitiativeStatus.Pending_Final,
                 InitiativeStatus.Approved,
                 InitiativeStatus.Rejected
            };

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Category)
                .Include(i => i.Department)
                .Include(i => i.Files)
                .Where(i => allowedStatuses.Contains(i.Status))
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.InitiativeCode.Contains(searchString));
            }
            
            // Stats
            var pendingData = _context.Initiatives.Where(i => allowedStatuses.Contains(i.Status));
            ViewBag.CountAwaiting = await pendingData.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved);
            ViewBag.CountApproved = await pendingData.CountAsync(i => i.Status == InitiativeStatus.Pending_Final || i.Status == InitiativeStatus.Approved);
            ViewBag.CountRejected = await pendingData.CountAsync(i => i.Status == InitiativeStatus.Rejected || i.Status == InitiativeStatus.Revision_Required);
            
            // List sorted by latest
            var items = await query.OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt).ToListAsync();

            // Reuse InitiativeReportVM or similar logic (using flat list for simpler dashboard)
            // But since we need to match the View expectations, let's pass the List or VM. 
            // The previous Dashboard used a VM. Let's send list to view via ViewBag or Model.
            // Based on user request "Index... ensure View Details link is correct", we will create a custom View.
            // Let's pass the List<Initiative> directly or a VM. 
            // Better to use a VM to hold search params + list.
            
            ViewBag.SearchString = searchString;
            return View("~/Areas/SciTech/Views/Council/Index.cshtml", items);
        }

        // ==========================================
        // 2. INITIATIVE DETAIILS
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var i = await _context.Initiatives
                .Include(x => x.Creator)
                .Include(x => x.Department)
                .Include(x => x.Category)
                .Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (i == null) return NotFound();

            var vm = new InitiativeDetailVM
            {
                Id = i.Id,
                Title = i.Title,
                InitiativeCode = i.InitiativeCode,
                Description = i.Description,
                ProposerName = i.Creator?.FullName ?? "Unknown",
                DepartmentName = i.Department?.Name ?? "N/A",
                Category = i.Category?.Name ?? "N/A",
                Status = i.Status,
                SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                Budget = i.Budget,
                Files = i.Files.Select(f => new InitiativeFileVM 
                { 
                    FileName = f.FileName 
                }).ToList()
            };
            
            // Pass Code for Icon logic in View
            // Map Dept Code if possible, or use ID. For now let's pass Dept Name or Code to View.
            // The VM has 'Code' property? Let's check VM definition.
            // Looking at Approve.cshtml, it uses Model.Code for switch case. 
            // InitiativeDetailVM likely has 'Code'. Let's verify VM later. 
            // For now assuming we can populate Code from Department Code if available.
            
            // Helper to get Dept Code
            var dept = await _context.Departments.FindAsync(i.DepartmentId);
            vm.Code = dept?.Code ?? "GEN"; // GEN for General

             return View("~/Areas/SciTech/Views/Council/Details.cshtml", vm);
        }

        // ==========================================
        // 3. APPROVE (Council Approves)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            // Council Approved -> Pending Final Decision
            initiative.Status = InitiativeStatus.Pending_Final; 

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. REJECT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Rejected; 

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. REQUEST REVISION
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRevision(int id, string comments)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Revision_Required;

            if (!string.IsNullOrEmpty(comments))
            {
                _context.RevisionRequests.Add(new RevisionRequest
                {
                    InitiativeId = id,
                    RequestContent = comments,
                    RequestedDate = DateTime.Now,
                    IsResolved = false,
                    Status = "Open",
                    RequesterId = 1 // TODO: User ID
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // 6. BOARD MANAGEMENT (Legacy/Admin)
        // ==========================================
        // GET: /SciTech/Council/ManageBoards
        public async Task<IActionResult> ManageBoards(int? id)
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
