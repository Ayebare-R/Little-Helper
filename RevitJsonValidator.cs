using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LittleHelper
{
    public static class JsonValidator
    {
        public static bool TryValidateJson(string json, out JObject parsedJson, out string errorMessage)
        {
            try
            {
                parsedJson = JObject.Parse(json);

                if (!parsedJson.ContainsKey("method") || !parsedJson.ContainsKey("parameters"))
                {
                    errorMessage = "The JSON is missing required fields: 'method' and/or 'parameters'.";
                    return false;
                }

                var parameters = parsedJson["parameters"];
                if (parameters == null || !parameters.HasValues)
                {
                    errorMessage = "The 'parameters' field is invalid or empty.";
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch (JsonReaderException ex)
            {
                parsedJson = null;
                errorMessage = $"Invalid JSON format: {ex.Message}";
                return false;
            }
        }
    }
}
