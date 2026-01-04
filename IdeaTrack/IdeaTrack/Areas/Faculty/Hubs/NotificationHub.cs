using Microsoft.AspNetCore.SignalR;

namespace IdeaTrack.Areas.Faculty.Hubs
{
    public class NotificationHub : Hub
    {
        // Client hoac Controller se goi ham nay khi nop ho so thanh cong
        public async Task SendSubmissionNotification(string userName, string initiativeTitle)
        {
            // Gui thong bao cho TAT CA client dang mo trang Dashboard
            await Clients.All.SendAsync("ReceiveSubmission", userName, initiativeTitle);
        }
    }
}
