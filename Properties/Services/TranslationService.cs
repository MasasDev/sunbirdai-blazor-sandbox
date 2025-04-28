using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SunbirdAIBlazorSandbox.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private const string URL = "https://api.sunbird.ai/tasks/nllb_translate";

        public TranslationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            string token = configuration["SunbirdAI:AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<TranslationResponse> TranslateTextAsync(string text, string sourceLanguage = "lug", string targetLanguage = "eng")
        {
            try
            {
                var requestData = new TranslationRequest
                {
                    Source_Language = sourceLanguage,
                    Target_Language = targetLanguage,
                    Text = text
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(URL, content);
                
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Translation API Response: {jsonString}");
                
                response.EnsureSuccessStatusCode();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                };
                
                var result = JsonSerializer.Deserialize<TranslationResponse>(jsonString, options);

                return result!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in TranslateTextAsync: {ex}");
                throw;
            }
        }
    }

    public class TranslationRequest
    {
        [JsonPropertyName("source_language")]
        public string Source_Language { get; set; }

        [JsonPropertyName("target_language")]
        public string Target_Language { get; set; }

         [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class TranslationResponse
    {
        [JsonPropertyName("output")]
        public OutputData Output { get; set; }
    }

    public class OutputData
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("translated_text")]
        public string TranslatedText { get; set; }
    }
}