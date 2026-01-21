using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Services
{
    /// <summary>
    /// Implementation of core business logic for Initiative operations.
    /// </summary>
    public class InitiativeService : IInitiativeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InitiativeService> _logger;

        public InitiativeService(ApplicationDbContext context, ILogger<InitiativeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> AutoAssignToBoardAsync(int initiativeId)
        {
            try
            {
                // 1. Load initiative with Category
                var initiative = await _context.Initiatives
                    .Include(i => i.Category)
                    .FirstOrDefaultAsync(i => i.Id == initiativeId);

                if (initiative == null)
                {
                    _logger.LogWarning("AutoAssign: Initiative {Id} not found", initiativeId);
                    return false;
                }

                var category = initiative.Category;
                if (category == null || category.BoardId == null || category.TemplateId == null)
                {
                    _logger.LogWarning("AutoAssign: Category {CategoryId} has no Board/Template configured", initiative.CategoryId);
                    return false;
                }

                // 2. Get all active BoardMembers for that Board
                var boardMembers = await _context.BoardMembers
                    .Where(bm => bm.BoardId == category.BoardId)
                    .ToListAsync();

                if (!boardMembers.Any())
                {
                    _logger.LogWarning("AutoAssign: Board {BoardId} has no members", category.BoardId);
                    return false;
                }

                // 3. Create InitiativeAssignment for each member
                foreach (var member in boardMembers)
                {
                    // Check if assignment already exists for this round
                    var existingAssignment = await _context.InitiativeAssignments
                        .AnyAsync(a => a.InitiativeId == initiativeId 
                                    && a.MemberId == member.UserId 
                                    && a.RoundNumber == initiative.CurrentRound);

                    if (existingAssignment) continue;

                    var assignment = new InitiativeAssignment
                    {
                        InitiativeId = initiativeId,
                        BoardId = category.BoardId,
                        MemberId = member.UserId,
                        TemplateId = category.TemplateId.Value,
                        RoundNumber = initiative.CurrentRound,
                        StageName = $"Round {initiative.CurrentRound}",
                        AssignedDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(14), // Default 2 weeks deadline
                        Status = AssignmentStatus.Assigned
                    };

                    _context.InitiativeAssignments.Add(assignment);
                }

                // 4. Set Initiative.Status = Evaluating
                initiative.Status = InitiativeStatus.Evaluating;

                await _context.SaveChangesAsync();

                _logger.LogInformation("AutoAssign: Created {Count} assignments for Initiative {Id}", 
                    boardMembers.Count, initiativeId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoAssign: Error assigning Initiative {Id}", initiativeId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateNewRoundAsync(int initiativeId)
        {
            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Assignments)
                    .FirstOrDefaultAsync(i => i.Id == initiativeId);

                if (initiative == null)
                {
                    _logger.LogWarning("CreateNewRound: Initiative {Id} not found", initiativeId);
                    return false;
                }

                // 1. Lock all current round assignments
                var currentRoundAssignments = initiative.Assignments
                    .Where(a => a.RoundNumber == initiative.CurrentRound)
                    .ToList();

                foreach (var assignment in currentRoundAssignments)
                {
                    if (assignment.Status != AssignmentStatus.Completed)
                    {
                        assignment.Status = AssignmentStatus.Completed;
                        assignment.DecisionDate = DateTime.Now;
                    }
                }

                // 2. Increment CurrentRound
                initiative.CurrentRound++;
                initiative.Status = InitiativeStatus.Re_Evaluating;

                // 3. Get board members and create new assignments
                var category = initiative.Category;
                if (category?.BoardId != null && category?.TemplateId != null)
                {
                    var boardMembers = await _context.BoardMembers
                        .Where(bm => bm.BoardId == category.BoardId)
                        .ToListAsync();

                    foreach (var member in boardMembers)
                    {
                        var newAssignment = new InitiativeAssignment
                        {
                            InitiativeId = initiativeId,
                            BoardId = category.BoardId,
                            MemberId = member.UserId,
                            TemplateId = category.TemplateId.Value,
                            RoundNumber = initiative.CurrentRound,
                            StageName = $"Round {initiative.CurrentRound}",
                            AssignedDate = DateTime.Now,
                            DueDate = DateTime.Now.AddDays(14),
                            Status = AssignmentStatus.Assigned
                        };

                        _context.InitiativeAssignments.Add(newAssignment);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("CreateNewRound: Initiative {Id} now on Round {Round}", 
                    initiativeId, initiative.CurrentRound);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateNewRound: Error for Initiative {Id}", initiativeId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Initiative?> CopyToDraftAsync(int initiativeId, int userId)
        {
            try
            {
                // 1. Load initiative with all related data
                var source = await _context.Initiatives
                    .Include(i => i.Files)
                    .Include(i => i.Category)
                    .FirstOrDefaultAsync(i => i.Id == initiativeId);

                if (source == null)
                {
                    _logger.LogWarning("CopyToDraft: Initiative {Id} not found", initiativeId);
                    return null;
                }

                // 2. Generate new initiative code
                var today = DateTime.Today;
                var codePrefix = $"SK-{today:yyyy}-";
                var lastCode = await _context.Initiatives
                    .Where(i => i.InitiativeCode.StartsWith(codePrefix))
                    .OrderByDescending(i => i.InitiativeCode)
                    .Select(i => i.InitiativeCode)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastCode != null)
                {
                    var lastNumberStr = lastCode.Replace(codePrefix, "");
                    if (int.TryParse(lastNumberStr, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                // 3. Create new Initiative as Draft
                var newInitiative = new Initiative
                {
                    InitiativeCode = $"{codePrefix}{nextNumber:D4}",
                    Title = $"[Copy] {source.Title}",
                    Description = source.Description,
                    Budget = source.Budget,
                    Status = InitiativeStatus.Draft,
                    CreatedAt = DateTime.Now,
                    SubmittedDate = null,
                    PeriodId = null, // Not submitted yet
                    CreatorId = userId,
                    DepartmentId = source.DepartmentId,
                    CategoryId = source.CategoryId,
                    CurrentRound = 1
                };

                _context.Initiatives.Add(newInitiative);
                await _context.SaveChangesAsync();

                // 4. Create authorship record
                var authorship = new InitiativeAuthorship
                {
                    InitiativeId = newInitiative.Id,
                    AuthorId = userId,
                    IsCreator = true,
                    JoinedAt = DateTime.Now
                };
                _context.InitiativeAuthorships.Add(authorship);

                // 5. Copy files (reference only - actual file copy would need Supabase integration)
                foreach (var file in source.Files)
                {
                    var newFile = new InitiativeFile
                    {
                        InitiativeId = newInitiative.Id,
                        FileName = file.FileName,
                        FilePath = file.FilePath, // Same path reference
                        FileType = file.FileType,
                        UploadDate = DateTime.Now
                    };
                    _context.InitiativeFiles.Add(newFile);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("CopyToDraft: Created new draft {NewId} from {SourceId}", 
                    newInitiative.Id, initiativeId);

                return newInitiative;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CopyToDraft: Error copying Initiative {Id}", initiativeId);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ClonePeriodDataAsync(int sourcePeriodId, int targetPeriodId)
        {
            try
            {
                // 1. Load all categories from source period
                var sourceCategories = await _context.InitiativeCategories
                    .Where(c => c.PeriodId == sourcePeriodId)
                    .ToListAsync();

                if (!sourceCategories.Any())
                {
                    _logger.LogWarning("ClonePeriod: No categories found in Period {Id}", sourcePeriodId);
                    return false;
                }

                // 2. Create copies in target period, preserving Board and Template links
                foreach (var category in sourceCategories)
                {
                    // Check if category with same name already exists in target
                    var exists = await _context.InitiativeCategories
                        .AnyAsync(c => c.PeriodId == targetPeriodId && c.Name == category.Name);

                    if (exists) continue;

                    var newCategory = new InitiativeCategory
                    {
                        Name = category.Name,
                        Description = category.Description,
                        PeriodId = targetPeriodId,
                        BoardId = category.BoardId, // Preserve link
                        TemplateId = category.TemplateId // Preserve link
                    };

                    _context.InitiativeCategories.Add(newCategory);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("ClonePeriod: Cloned {Count} categories from Period {Source} to {Target}",
                    sourceCategories.Count, sourcePeriodId, targetPeriodId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClonePeriod: Error cloning from {Source} to {Target}", 
                    sourcePeriodId, targetPeriodId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateFinalResultAsync(int initiativeId, InitiativeStatus decision, int decidedByUserId)
        {
            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Assignments)
                        .ThenInclude(a => a.EvaluationDetails)
                    .Include(i => i.FinalResult)
                    .FirstOrDefaultAsync(i => i.Id == initiativeId);

                if (initiative == null)
                {
                    _logger.LogWarning("CreateFinalResult: Initiative {Id} not found", initiativeId);
                    return false;
                }

                // Only allow Approved or Rejected as final decision
                if (decision != InitiativeStatus.Approved && decision != InitiativeStatus.Rejected)
                {
                    _logger.LogWarning("CreateFinalResult: Invalid decision {Status}", decision);
                    return false;
                }

                // Calculate average score from current round
                var currentRoundAssignments = initiative.Assignments
                    .Where(a => a.RoundNumber == initiative.CurrentRound && a.Status == AssignmentStatus.Completed)
                    .ToList();

                decimal averageScore = 0;
                if (currentRoundAssignments.Any())
                {
                    var totalScores = currentRoundAssignments
                        .SelectMany(a => a.EvaluationDetails)
                        .Sum(d => d.ScoreGiven);
                    var memberCount = currentRoundAssignments.Count;
                    averageScore = memberCount > 0 ? totalScores / memberCount : 0;
                }

                // Determine rank based on score (simplified logic)
                string rank = averageScore switch
                {
                    >= 90 => "Excellent",
                    >= 80 => "Good",
                    >= 70 => "Average",
                    >= 60 => "Pass",
                    _ => "Fail"
                };

                // Create or update FinalResult
                if (initiative.FinalResult == null)
                {
                    var finalResult = new FinalResult
                    {
                        InitiativeId = initiativeId,
                        AverageScore = averageScore,
                        Rank = rank,
                        ChairmanDecision = decision == InitiativeStatus.Approved ? "Approved" : "Rejected",
                        ChairmanId = decidedByUserId,
                        DecisionDate = DateTime.Now
                    };
                    _context.FinalResults.Add(finalResult);
                }
                else
                {
                    initiative.FinalResult.AverageScore = averageScore;
                    initiative.FinalResult.Rank = rank;
                    initiative.FinalResult.ChairmanDecision = decision == InitiativeStatus.Approved ? "Approved" : "Rejected";
                    initiative.FinalResult.ChairmanId = decidedByUserId;
                    initiative.FinalResult.DecisionDate = DateTime.Now;
                }

                initiative.Status = decision;

                await _context.SaveChangesAsync();

                _logger.LogInformation("CreateFinalResult: Initiative {Id} finalized as {Status} with score {Score}",
                    initiativeId, decision, averageScore);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateFinalResult: Error for Initiative {Id}", initiativeId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<int> SyncCategoryInitiativesAsync(int categoryId)
        {
            try
            {
                // 1. Load category with Board and Template info
                var category = await _context.InitiativeCategories
                    .Include(c => c.Board)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null)
                {
                    _logger.LogWarning("SyncCategory: Category {Id} not found", categoryId);
                    return -1;
                }

                if (category.BoardId == null || category.TemplateId == null)
                {
                    _logger.LogWarning("SyncCategory: Category {Id} has no Board/Template configured", categoryId);
                    return -1;
                }

                // 2. Find all initiatives in this category with Evaluating, Re_Evaluating, or Approved status
                var initiatives = await _context.Initiatives
                    .Include(i => i.Assignments)
                    .Where(i => i.CategoryId == categoryId && 
                               (i.Status == InitiativeStatus.Evaluating || 
                                i.Status == InitiativeStatus.Re_Evaluating ||
                                i.Status == InitiativeStatus.Approved))
                    .ToListAsync();

                if (!initiatives.Any())
                {
                    _logger.LogInformation("SyncCategory: No initiatives to sync in Category {Id}", categoryId);
                    return 0;
                }

                // 3. Get all board members of the NEW board
                var boardMembers = await _context.BoardMembers
                    .Where(bm => bm.BoardId == category.BoardId)
                    .ToListAsync();

                if (!boardMembers.Any())
                {
                    _logger.LogWarning("SyncCategory: Board {BoardId} has no members", category.BoardId);
                    return -1;
                }

                int syncedCount = 0;

                foreach (var initiative in initiatives)
                {
                    // 4. Lock all current round assignments as Completed
                    var currentRoundAssignments = initiative.Assignments
                        .Where(a => a.RoundNumber == initiative.CurrentRound)
                        .ToList();

                    foreach (var assignment in currentRoundAssignments)
                    {
                        if (assignment.Status != AssignmentStatus.Completed)
                        {
                            assignment.Status = AssignmentStatus.Completed;
                            assignment.DecisionDate = DateTime.Now;
                            assignment.ReviewComment = "Auto-closed due to category sync";
                        }
                    }

                    // 5. Increment round and set status to Re_Evaluating
                    initiative.CurrentRound++;
                    initiative.Status = InitiativeStatus.Re_Evaluating;

                    // 6. Create new assignments for NEW board members
                    foreach (var member in boardMembers)
                    {
                        var newAssignment = new InitiativeAssignment
                        {
                            InitiativeId = initiative.Id,
                            BoardId = category.BoardId,
                            MemberId = member.UserId,
                            TemplateId = category.TemplateId.Value,
                            RoundNumber = initiative.CurrentRound,
                            StageName = $"Round {initiative.CurrentRound}",
                            AssignedDate = DateTime.Now,
                            DueDate = DateTime.Now.AddDays(14),
                            Status = AssignmentStatus.Assigned,
                            ReviewComment = "Auto-assigned via category sync"
                        };

                        _context.InitiativeAssignments.Add(newAssignment);
                    }

                    syncedCount++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("SyncCategory: Synced {Count} initiatives in Category {Id} to Board {BoardId}",
                    syncedCount, categoryId, category.BoardId);

                return syncedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncCategory: Error syncing Category {Id}", categoryId);
                return -1;
            }
        }
    }
}
