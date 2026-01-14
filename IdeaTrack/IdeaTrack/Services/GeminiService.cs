using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IdeaTrack.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http = new();

        // ⚠️ Demo only – đưa sang appsettings.json khi production
        private readonly string _apiKey;
        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"]
                      ?? throw new Exception("Gemini API key is missing");
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

NỘI DUNG CẦN LÀM RÕ (trình bày thành đoạn văn):
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

            // 1️⃣ Sinh bản tóm tắt ban đầu
            string summary = await CallGeminiAsync(SUMMARY_PROMPT + "\n" + content);

            // 2️⃣ Nếu chưa đủ 380 từ → mở rộng có ngữ cảnh
            if (CountWords(summary) < MIN_WORDS)
            {
                string expandPrompt = $@"
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

            // 3️⃣ Giới hạn tối đa 420 từ – KHÔNG cắt dở câu
            return EnforceWordLimitBySentence(summary, MAX_WORDS);
        }

        private async Task<string> CallGeminiAsync(string prompt)
        {
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
                    temperature = 0.6,
                    maxOutputTokens = 1600
                }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"[Gemini API ERROR] {response.StatusCode}: {raw}";

            using var doc = JsonDocument.Parse(raw);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                return "[Gemini API ERROR] No candidates returned.";

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return string.IsNullOrWhiteSpace(text)
                ? "[Gemini API ERROR] Empty response."
                : text;
        }

        // ✅ Đếm từ ổn định cho tiếng Việt
        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return Regex.Matches(text, @"\b\p{L}+\b").Count;
        }

        // ✅ Giới hạn từ nhưng chỉ cắt theo CÂU
        private string EnforceWordLimitBySentence(string text, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            var result = new StringBuilder();
            int wordCount = 0;

            foreach (var sentence in sentences)
            {
                int sentenceWords = CountWords(sentence);
                if (wordCount + sentenceWords > maxWords)
                    break;

                result.Append(sentence).Append(" ");
                wordCount += sentenceWords;
            }

            var finalText = result.ToString().Trim();

            if (!Regex.IsMatch(finalText, @"[\.!\?]$"))
                finalText += ".";

            return finalText;
        }
    }
}
