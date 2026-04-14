using System.Text;
using System.Text.Json;
namespace LibraryManagementSystem.Services;

public class SummaryService
{
    // 1. Put your verified API Key here
    private readonly string _apiKey = "YOUR_API_KEY";
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<string> GenerateSummaryAsync(string title, string genre)
    {
        try
        {
            // Using the precise V1 endpoint for Gemini 2.5 Flash
            string url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = $"Write a 2-sentence plot summary for the book '{title}' in the {genre} genre. Make it engaging." }
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                return $"AI Error ({response.StatusCode}): {errorDetails}";
            }

            string resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);

            // Standard path to extract text from Gemini's JSON response
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text?.Trim() ?? "Summary was empty.";
        }
        catch (Exception ex)
        {
            return $"Connection Error: {ex.Message}";
        }
    }
}
