using FileBotGPT.InterfacesServices;
using FileBotGPT.Model;


namespace FileBotGPT.Services
{
    public class GptRequestService : IGptRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? baseUrl;
        private string? apikey;
        public GptRequestService(HttpClient httpClient, IConfiguration configuration)
        {
            //Inject HttpClient, IConfiguration through constructor
            _httpClient = httpClient;
            _configuration = configuration;

            // Initialize base URL and API key from configuration
            this.baseUrl = _configuration.GetValue<string>("APISettings:BaseURL");
            this.apikey = _configuration.GetValue<string>("APISettings:apiKey");
        }

        // Method to construct the prompt for GPT request
        private string MakePrompt(PostBotRequest botData)
        {
            string addContent;  // Declare variable to hold additional content
            string definiteLanguage = "Write not in Russian"; // Define prompt for non-Russian language

            // Construct initial prompt
            string prompt =
                "Write a summary that is clear, easy to understand, detailed, and contains basic information. " +
                "Give examples where necessary.Write about" + botData.text + "." +
                "Use " + botData.language + " language for all text. Format in markdown.";
            if (botData.language != "Russian" || botData.language != "English") { prompt += definiteLanguage; } // Append language prompt if not Russian or English
            if (botData.contents == "yes")
            {
                addContent = "Add contents at the beginning.";  // Define additional content if requested
            }
            else { addContent = ""; }
            prompt += addContent;

            // Return the constructed prompt
            return prompt;
        }

        // Method to send a POST request with prompt content and retrieve response asynchronously
        private async Task<string> PostRequestContentAsync(string prompt)
        {
            var data = new PostGptRequest
            {
                apikey = this.apikey,
                prompt = prompt
            };

            // Construct the URL for the POST request
            var url = baseUrl + "chat/";

            // Send POST request with JSON data and await response
            var response = await _httpClient.PostAsJsonAsync(url, data);

            // Read and return response content as string asynchronously
            return await response.Content.ReadAsStringAsync();
        }

        // Method to send GPT request with bot data asynchronously
        public async Task<string> PostGptRequestAsync(PostBotRequest data)
        {
            // Generate prompt for GPT request
            string prompt = MakePrompt(data);

            // Send GPT request with generated prompt and await response
            string gptResponse = await PostRequestContentAsync(prompt);

            // Return GPT response
            return gptResponse;
        }
    }
}
