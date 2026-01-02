using Microsoft.AspNetCore.SignalR;

namespace IdeaTrack.Areas.Faculty.Hubs
{
    public class NotificationHub : Hub
    {
        // Client hoặc Controller sẽ gọi hàm này khi nộp hồ sơ thành công
        public async Task SendSubmissionNotification(string userName, string initiativeTitle)
        {
            // Gửi thông báo cho TẤT CẢ client đang mở trang Dashboard
            await Clients.All.SendAsync("ReceiveSubmission", userName, initiativeTitle);
        }
    }
}
