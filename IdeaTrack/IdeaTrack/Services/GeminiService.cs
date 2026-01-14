using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IdeaTrack.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly List<string> _apiKeys;

        public GeminiService(IConfiguration configuration, HttpClient httpClient)
        {
            _http = httpClient;

            _apiKeys = configuration
                .GetSection("Gemini:ApiKeys")
                .Get<List<string>>()
                ?? throw new Exception("Gemini API keys are missing");

            if (_apiKeys.Count < 2)
                throw new Exception("Cần ít nhất 2 Gemini API key để dự phòng");
        }

        private const int MIN_WORDS = 380;
        private const int MAX_WORDS = 420;

        private const string SUMMARY_PROMPT = @"
Bạn là trợ lý AI chuyên tóm tắt TÀI LIỆU HỌC THUẬT – ĐÀO TẠO phục vụ HỘI ĐỒNG ĐÁNH GIÁ.

YÊU CẦU ĐỘ DÀI:
- Viết từ 380 đến 420 từ.
- Văn phong học thuật, rõ ràng, mạch lạc.

NGUYÊN TẮC BẮT BUỘC:
- Không làm sai lệch nội dung.
- Không bịa đặt thông tin.
- Được phép diễn giải học thuật dựa trên nội dung gốc.
- PHẢI KẾT THÚC BẰNG CÂU HOÀN CHỈNH.

NỘI DUNG CẦN LÀM RÕ:
1. Mục tiêu tổng quát của chương trình/tài liệu.
2. Đối tượng đào tạo và nhu cầu thực tiễn.
3. Định hướng nội dung và cấu trúc.
4. Phương pháp hoặc cách tiếp cận đào tạo.
5. Giá trị và tác động đối với người học và xã hội.

HÌNH THỨC:
- 1–2 đoạn liên tục.
- Không gạch đầu dòng.
- Không tiêu đề.

NỘI DUNG TÀI LIỆU:
";

        public async Task<string> SummarizeAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Không có nội dung để tóm tắt.";

            if (CountWords(content) < 40)
                return "Nội dung tài liệu quá ngắn, không đủ dữ liệu để tạo bản tóm tắt 400 từ.";

            string summary = await CallGeminiAsync(SUMMARY_PROMPT + "\n" + content);

            if (CountWords(summary) < MIN_WORDS)
            {
                var expandPrompt = $@"
NỘI DUNG GỐC:
{content}

BẢN TÓM TẮT HIỆN TẠI:
{summary}

YÊU CẦU:
- Mở rộng bản tóm tắt để đạt 380–420 từ.
- Giữ nguyên bản chất nội dung.
- Không lặp câu.
- Không thêm thông tin ngoài tài liệu.
- PHẢI KẾT THÚC BẰNG CÂU HOÀN CHỈNH.
";
                summary = await CallGeminiAsync(expandPrompt);
            }

            return EnforceWordLimitBySentence(summary, MAX_WORDS);
        }

        // ================= Gemini call + fallback =================
        private async Task<string> CallGeminiAsync(string prompt)
        {
            foreach (var apiKey in _apiKeys)
            {
                var url =
                    $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

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
                        temperature = 0.6,
                        maxOutputTokens = 1600
                    }
                };

                var response = await _http.PostAsync(
                    url,
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                var raw = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(raw);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }

                if (response.StatusCode is
                    System.Net.HttpStatusCode.TooManyRequests or
                    System.Net.HttpStatusCode.Forbidden or
                    System.Net.HttpStatusCode.InternalServerError)
                {
                    continue;
                }

                return $"[Gemini API ERROR] {response.StatusCode}: {raw}";
            }

            return "[Gemini API ERROR] All API keys exhausted (quota or invalid).";
        }

        // ================= Utils =================

        private int CountWords(string text) =>
            string.IsNullOrWhiteSpace(text)
                ? 0
                : Regex.Matches(text, @"\b\p{L}+\b").Count;

        private string EnforceWordLimitBySentence(string text, int maxWords)
        {
            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            var sb = new StringBuilder();
            int count = 0;

            foreach (var s in sentences)
            {
                int w = CountWords(s);
                if (count + w > maxWords) break;
                sb.Append(s).Append(" ");
                count += w;
            }

            var result = sb.ToString().Trim();
            return Regex.IsMatch(result, @"[\.!\?]$") ? result : result + ".";
        }
    }
}
