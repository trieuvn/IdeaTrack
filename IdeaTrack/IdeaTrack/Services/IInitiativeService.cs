namespace IdeaTrack.Services
{
    using IdeaTrack.Models;

    /// <summary>
    /// Core business logic service for Initiative operations.
    /// Handles auto-assignment, multi-round evaluation, copy-to-draft, and period cloning.
    /// </summary>
    public interface IInitiativeService
    {
        /// <summary>
        /// Auto-assign initiative to all board members when OST approves.
        /// Called when status changes from Faculty_Approved to Evaluating.
        /// Creates InitiativeAssignment records for each board member.
        /// </summary>
        /// <param name="initiativeId">The initiative to assign</param>
        /// <returns>True if successful, false if no board/template configured</returns>
        Task<bool> AutoAssignToBoardAsync(int initiativeId);

        /// <summary>
        /// Create a new evaluation round for re-evaluation.
        /// Locks current round assignments and creates new ones for next round.
        /// </summary>
        /// <param name="initiativeId">The initiative to re-evaluate</param>
        /// <returns>True if successful</returns>
        Task<bool> CreateNewRoundAsync(int initiativeId);

        /// <summary>
        /// Deep copy a rejected initiative to a new draft.
        /// Copies all content including files, resets status and dates.
        /// </summary>
        /// <param name="initiativeId">The source initiative (usually Rejected)</param>
        /// <param name="userId">The user who will own the new draft</param>
        /// <returns>The newly created draft Initiative</returns>
        Task<Initiative?> CopyToDraftAsync(int initiativeId, int userId);

        /// <summary>
        /// Clone categories from source period to target period.
        /// Preserves Board and Template links for easy setup of new periods.
        /// </summary>
        /// <param name="sourcePeriodId">The period to copy from</param>
        /// <param name="targetPeriodId">The period to copy to</param>
        /// <returns>True if successful</returns>
        Task<bool> ClonePeriodDataAsync(int sourcePeriodId, int targetPeriodId);

        /// <summary>
        /// Create final result snapshot when PKHCN makes final decision.
        /// Locks the result permanently in FinalResults table.
        /// </summary>
        /// <param name="initiativeId">The initiative to finalize</param>
        /// <param name="decision">Approved or Rejected</param>
        /// <param name="decidedByUserId">The user making the decision</param>
        /// <returns>True if successful</returns>
        Task<bool> CreateFinalResultAsync(int initiativeId, InitiativeStatus decision, int decidedByUserId);
    }
}
