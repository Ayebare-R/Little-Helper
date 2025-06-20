using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;










namespace LittleHelper
{
    
    public static class EnvLoader
{
    public static void Load(string filePath = ".env")
    {
        if (!File.Exists(filePath)) return;
        
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', 2);
            if (parts.Length != 2) continue;
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Usage:
EnvLoader.Load();
    public static class ChatGPTService
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        public static async Task<(string userResponse, string jsonCommand)> GetResponse(List<ChatMessage> conversationHistory)
        {
            // Build the messages list for the API call
            var messages = conversationHistory.Select(msg => new ChatMessage
            {
                role = msg.role,
                content = msg.content
            }).ToList();


            string systemPrompt = @"
                You are an assistant integrated into a Revit add-in.
                Your purpose is to understand user requests and turn them into Revit API calls if possible.
                You ask the user follow up questions to make sure you have all the relevant information you need before making an API call.

                You communicate with the Revit API via JSON commands that you mark with <JSON></JSON> tags, so that the API can parse them.
                Your JSON commands need to follow the syntax of the Revit API. You must declare a method and parameters necessary to call the API.
                Your communication with the user is in natural language. Your tone is friendly and slightly quirky.

                Example:
                User: ""Create a wall that's 10 metres long and 3 metres high""
                Assistant: ""Great choice! I've sketched out a wall that's 10 metres long and 3 metres high. Shall I place it in your Revit window?""
                User: ""Yes, please place at coordinates (0, 0, 0)""
                Assistant: ""Fantastic! Your wall has been constructed starting at (0,0,0). Let me know if you'd like to add windows or doors to it!""
                ";

            // Add the system prompt if necessary
            if (!messages.Any(m => m.role == "system"))
            {
                messages.Insert(0, new ChatMessage
                {
                    role = "system",
                    content = systemPrompt
                });
            }


            // Handle token limits
            const int maxTokens = 3500;
            int currentTokenCount = EstimateTokenCount(messages);

            while (currentTokenCount > maxTokens)
            {
                // Remove the second message (after system prompt)
                messages.RemoveAt(1);
                currentTokenCount = EstimateTokenCount(messages);
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = messages
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


                try
                {
                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log the error response for debugging
                        Console.WriteLine($"API Error: {response.StatusCode}");
                        Console.WriteLine($"Error Details: {responseString}");
                        throw new Exception($"API call failed with status code {response.StatusCode}");
                    }

                    var responseObject = JsonConvert.DeserializeObject<ChatGPTResponse>(responseString);
                    string result = responseObject.choices[0].message.content.Trim();

                    return ValidateAndParseResponse(result);
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., network errors, deserialization errors)
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    throw;
                }
            }
        }

        private static (string userResponse, string jsonCommand) ValidateAndParseResponse(string response)
        {
            string jsonPattern = @"<JSON>(.*?)<\/JSON>";
            Match jsonMatch = Regex.Match(response, jsonPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            string jsonCommand = jsonMatch.Success ? jsonMatch.Groups[1].Value : null;

            if (!string.IsNullOrEmpty(jsonCommand))
            {
                if (!JsonValidator.TryValidateJson(jsonCommand, out JObject _, out string errorMessage))
                {
                    Console.WriteLine($"JSON Validation Failed: {errorMessage}");
                    jsonCommand = null;
                }
            }

            // string userMessage = Regex.Replace(response, jsonPattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase).Trim();

            string userMessage = Regex.Replace(response, jsonPattern, "").Trim();
            return (userMessage, jsonCommand);
        }

        private static int EstimateTokenCount(List<ChatMessage> messages)
        {
            int tokenCount = 0;
            foreach (var msg in messages)
            {
                // Simple estimation: 1 token per 4 characters
                tokenCount += msg.content.Length / 4;
            }
            return tokenCount;
        }

    }



    // Strongly-typed classes for deserialization
    public class ChatGPTResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public ChatMessage message { get; set; }
    }

    public class ChatMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }



}
