using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace SunbirdAIBlazorSandbox.Services
{
    public class SpeechToTextService
    {
        private readonly HttpClient _httpClient;
        private const string URL = "https://api.sunbird.ai/tasks/stt";
        
        public SpeechToTextService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            string token = configuration["SunbirdAI:AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<SpeechToTextResponse> ConvertToTextAsync(string audioPath, string language = "lug", string adapter = "lug")
        {
            try 
            {
                using var multipartContent = new MultipartFormDataContent();
                
                var fileInfo = new FileInfo(audioPath);
                var fileContent = new StreamContent(File.OpenRead(audioPath));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                multipartContent.Add(fileContent, "audio", fileInfo.Name);
                
                multipartContent.Add(new StringContent(language), "language");
                multipartContent.Add(new StringContent(adapter), "adapter");
                
                var response = await _httpClient.PostAsync(URL, multipartContent);
                
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {jsonString}");
                
                response.EnsureSuccessStatusCode();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<SpeechToTextResponse>(jsonString, options);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ConvertToTextAsync: {ex}");
                throw;
            }
        }
    }

    public class SpeechToTextResponse
    {
        public string Audio_Transcription { get; set; }
        public object Diarization_Output { get; set; }
        public string Formatted_Diarization_Output { get; set; }
        public int Audio_Transcription_Id { get; set; }
        public string Audio_Url { get; set; }
        public string Language { get; set; }
        public bool Was_Audio_Trimmed { get; set; }
        public object Original_Duration_Minutes { get; set; }
    }
}