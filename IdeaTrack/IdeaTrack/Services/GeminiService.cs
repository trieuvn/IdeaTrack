using System.Text;
using System.Text.Json;

namespace IdeaTrack.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http = new();
        private readonly string _apiKey = "AIzaSyDu3fmo3r1Awk2ZAbbnL7HIVHVfZIy0J_0";

        // Prompt tiếng Việt – rõ nhiệm vụ + giới hạn
        private const string SUMMARY_PROMPT = @"
Bạn là trợ lý AI tóm tắt tài liệu cho hội đồng đánh giá.  
Tóm tắt tối đa 200 từ, chỉ tập trung mục tiêu, ý tưởng chính và giải pháp, bỏ chi tiết phụ.  
NỘI DUNG:";

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
    },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 500
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
