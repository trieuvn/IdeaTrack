using System.Text;
using System.Text.Json;

namespace IdeaTrack.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http = new();
        private readonly string _apiKey = "AIzaSyCcC-GOYOwXsj3jyQezRdVYyTFZFqC2kNw";

        // Prompt tiếng Việt – rõ nhiệm vụ + giới hạn
        private const string SUMMARY_PROMPT = @"
Bạn là trợ lý AI chuyên tóm tắt tài liệu cho hội đồng đánh giá.

NHIỆM VỤ:
Tóm tắt nội dung tài liệu bên dưới với ĐỘ DÀI TỐI ĐA 200 TỪ.

YÊU CẦU BẮT BUỘC:
- Nếu bản tóm tắt vượt quá 200 từ, hãy tự rút gọn lại.
- Chỉ tập trung vào mục tiêu, ý tưởng chính, giải pháp đề xuất và nội dung cốt lõi.
- Loại bỏ ví dụ, dẫn chứng, tài liệu tham khảo và chi tiết phụ.
- Không thêm nhận xét cá nhân hoặc suy đoán.
- Viết bằng tiếng Việt rõ ràng, mạch lạc, trang trọng.
- Chỉ xuất ra MỘT đoạn văn duy nhất.

NỘI DUNG TÀI LIỆU:
";

        public async Task<string> SummarizeAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Không có nội dung để tóm tắt.";

            var prompt = SUMMARY_PROMPT + "\n" + content;

            var url =
                $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                return "AI không thể tạo bản tóm tắt.";

            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);

            var summary = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // 🛡️ CẮT CỨNG 200 TỪ – ĐẢM BẢO KHÔNG BAO GIỜ VƯỢT
            return EnforceWordLimit(summary, 200);
        }

        private string EnforceWordLimit(string text, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= maxWords)
                return text;

            return string.Join(' ', words.Take(maxWords)) + "...";
        }
    }
}
