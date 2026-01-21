using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace IdeaTrack.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IConfiguration configuration, HttpClient httpClient, ILogger<GeminiService> logger)
        {
            _http = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        private List<string> GetApiKeys()
        {
            var keys = _configuration
                .GetSection("Gemini:ApiKeys")
                .Get<List<string>>() ?? new List<string>();
            
            if (keys.Count == 0)
            {
                _logger.LogWarning("No Gemini API keys configured in gemini.json");
            }
            
            return keys;
        }

        private string GetModel() => _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        private int GetMaxOutputTokens() => _configuration.GetValue<int>("Gemini:MaxOutputTokens", 1600);
        private double GetTemperature() => _configuration.GetValue<double>("Gemini:Temperature", 0.6);

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
            var apiKeys = GetApiKeys();
            
            if (apiKeys.Count == 0)
            {
                return "[Error] No Gemini API keys configured. Please add keys to gemini.json";
            }

            var model = GetModel();
            var maxTokens = GetMaxOutputTokens();
            var temperature = GetTemperature();

            for (int i = 0; i < apiKeys.Count; i++)
            {
                var apiKey = apiKeys[i];
                var keyMasked = $"{apiKey[..8]}...{apiKey[^4..]}";
                
                _logger.LogInformation("Trying Gemini API key {KeyIndex}/{TotalKeys}: {MaskedKey}", 
                    i + 1, apiKeys.Count, keyMasked);

                var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

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
                        temperature = temperature,
                        maxOutputTokens = maxTokens
                    }
                };

                try
                {
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
                        {
                            _logger.LogInformation("Gemini API call successful using key {KeyIndex}", i + 1);
                            return text;
                        }
                    }

                    // Check if we should try next key (quota limit, forbidden, server error)
                    if (response.StatusCode is
                        System.Net.HttpStatusCode.TooManyRequests or    // 429 - Quota exceeded
                        System.Net.HttpStatusCode.Forbidden or          // 403 - Key invalid/blocked
                        System.Net.HttpStatusCode.InternalServerError)  // 500 - Server error
                    {
                        _logger.LogWarning("Key {KeyIndex} failed with {StatusCode}, trying next key...", 
                            i + 1, response.StatusCode);
                        continue;
                    }

                    // For other errors, return immediately
                    _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, raw);
                    return $"[Gemini API ERROR] {response.StatusCode}: {raw}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception calling Gemini API with key {KeyIndex}", i + 1);
                    continue;
                }
            }

            _logger.LogError("All {TotalKeys} Gemini API keys exhausted", apiKeys.Count);
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
