using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Service.Interface;

namespace Service.Implement;

public class ChatGptService : IChatgptService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatGptService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = "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"; // Thay bằng API Key của bạn
    }

    public async Task<string> GetChatGptResponse(string content)
    {
        var requestData = new
        {
            model = "gpt-3.5-turbo", // Model của ChatGPT
            messages = new[]
            {
                new { role = "user", content = content }
            }
        };

        var requestBody =
            new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return $"Error: {response.StatusCode} - {errorContent}";
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
        return jsonResponse.choices[0].message.content;
    }
}