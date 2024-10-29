namespace Fall2024_Assignment3_jtadams5.Services
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using Azure;
    using Azure.AI.OpenAI;
    using Azure.Identity;
    using OpenAI.Chat;
    using System.ClientModel;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using System;

    public class OpenAIService
    {
        private readonly SecretClient _secretClient;
        private const string AiDeployment = "gpt-35-turbo";
        private readonly HttpClient _httpClient;

        public OpenAIService(IConfiguration configuration)
        {
            var keyVaultName = "Fall2024-jtadams5-Vault";
            var kUri = $"https://{keyVaultName}.vault.azure.net";

            _secretClient = new SecretClient(new Uri(kUri), new DefaultAzureCredential());


            
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetFakeMovieTweetsAsync(string movieTitle,int movieYear)
        {
            var _apiKey = await _secretClient.GetSecretAsync("ApiKey1");
            var _endpoint = await _secretClient.GetSecretAsync("Endpoint");
            var p = $"Create 20 short tweets of varying moods reviewing the movie '{movieTitle}'.";

            ApiKeyCredential credential = new(_apiKey.Value.Value);
            AzureOpenAIClient azureClient = new(new Uri(_endpoint.Value.Value), credential);
            ChatClient chatClient = azureClient.GetChatClient(AiDeployment);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You are an AI assistant that helps people find information. Your main task, when prompted for it, is to generate random tweets of random lengths reviewing a given movie name. Use the information you know about the movie to form these fake opinions. Be sure to include a variety of good and bad reviews, rating the movie from 1-10, from perspectives of those intersted in different genres, pointing out different praise and critcism of the movie. Utilize details like score, actors, director, etc."),
                new UserChatMessage($"Generate 10 fake tweets that each answer the following question with various moods: How would you rate the movie {movieTitle}, which came out in {movieYear} out of 10?")
            };

            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages);
            string review = result.Value.Content[0].Text;
            

            // Going to try and split this into a string array

            return review;

        }

        public async Task<string> GetFakeActorTweetsAsync(string actorTitle)
        {

            var _apiKey = await _secretClient.GetSecretAsync("ApiKey1");
            var _endpoint = await _secretClient.GetSecretAsync("Endpoint");
            var p = $"Create 20 short tweets of varying moods reviewing the movie '{actorTitle}'.";

            ApiKeyCredential credential = new(_apiKey.Value.Value);
            AzureOpenAIClient azureClient = new(new Uri(_endpoint.Value.Value), credential);
            ChatClient chatClient = azureClient.GetChatClient(AiDeployment);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You are an AI assistant that helps people find information. Your main task, when prompted for it, is to generate random tweets of random lengths about a famous actor and their work. Use the information you know about the actor to form these fake opinions. Be sure to include a variety of good and very negative tweets, from perspectives of those who love the actor to those that hate the actor, pointing out different praise and critcism of the actor."),
                new UserChatMessage($"Generate 10 fake tweets that comment things about the following actor in various tones: {actorTitle}. Be sure to sometimes include movies the actor is in and why they feel that way about the actor.")
            };

            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages);
            string review = result.Value.Content[0].Text;


            // Going to try and split this into a string array

            return review;

        }
    }

}
