#pragma warning disable SKEXP0070 // Semantic Kernel Google Connector is experimental
#pragma warning disable SKEXP0001 // Semantic Kernel OpenAI/Core Connectors is experimental
#pragma warning disable SKEXP0010 // Semantic Kernel Custom Endpoint is experimental

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ecommerce.BLL
{
    public class LLMService : ILLMService
    {
        private readonly IConfiguration _configuration;
        private readonly ChatbotPlugins _chatbotPlugins;
        private readonly IChatRepository _chatRepository;

        public LLMService(IConfiguration configuration, ChatbotPlugins chatbotPlugins, IChatRepository chatRepository)
        {
            _configuration = configuration;
            _chatbotPlugins = chatbotPlugins;
            _chatRepository = chatRepository;
        }

        public async Task<ChatMessageResponse> ProcessMessageAsync(int userId, string role, string message)
        {
            // 1. Retrieve or Create a Chat Session for the user and role (if authenticated)
            ChatSession session;
            if (userId > 0)
            {
                session = await _chatRepository.GetSessionWithMessages(userId, role);

                if (session == null)
                {
                    session = await _chatRepository.CreateSession(userId, role);
                }
            }
            else
            {
                session = new ChatSession
                {
                    Id = 0,
                    UserId = 0,
                    Role = "Guest",
                    CreatedAt = DateTime.UtcNow,
                    Messages = new List<ChatMessage>()
                };
            }

            // 2. Validate API Key configuration (prefer Groq, fallback to Gemini)
            var groqApiKey = _configuration["GroqSettings:ApiKey"];
            var geminiApiKey = _configuration["GeminiSettings:ApiKey"];

            // Diagnostic log to check secret loading (masked for security)
            if (!string.IsNullOrEmpty(groqApiKey))
            {
                var masked = groqApiKey.Length > 10 
                    ? $"{groqApiKey.Substring(0, 6)}...{groqApiKey.Substring(groqApiKey.Length - 4)}" 
                    : groqApiKey;
                Console.WriteLine($"[DIAGNOSTIC] Loaded Groq ApiKey: {masked}");
            }
            else
            {
                Console.WriteLine("[DIAGNOSTIC] Groq ApiKey is NULL or EMPTY");
            }

            var isGroqConfigured = !string.IsNullOrWhiteSpace(groqApiKey) && groqApiKey != "YOUR_GROQ_API_KEY_HERE";
            var isGeminiConfigured = !string.IsNullOrWhiteSpace(geminiApiKey) && geminiApiKey != "YOUR_GEMINI_API_KEY_HERE";

            if (!isGroqConfigured && !isGeminiConfigured)
            {
                return new ChatMessageResponse
                {
                    Reply = "Welcome! The AI Chatbot is successfully integrated, but the API Key is not configured yet. Please update the `GroqSettings:ApiKey` or `GeminiSettings:ApiKey` value in the backend's `appsettings.json` file to activate AI responses.",
                    SessionId = session.Id
                };
            }

            // 3. Initialize Semantic Kernel with selected LLM Provider
            //HttpClientHandler is the component that manages how HttpClient connects to and communicates with web servers over HTTP/HTTPS.
            // Backend uses Httpclient to communicate with the LLM
            using var httpClientHandler = new HttpClientHandler
            {
                CheckCertificateRevocationList = false
            };
            using var httpClient = new HttpClient(httpClientHandler);

            //Kernel is a run-time object that coordinates AI models, plugins and services to execute AI tasks.
            //Kernel is created using Builder Pattern
            var kernelBuilder = Kernel.CreateBuilder();

            if (isGroqConfigured)
            {
                var groqModelId = _configuration["GroqSettings:ModelId"];
                if (string.IsNullOrWhiteSpace(groqModelId))
                {
                    groqModelId = "llama-3.3-70b-versatile";
                }
                //Configure AI Service
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: groqModelId,
                    apiKey: groqApiKey,
                    endpoint: new Uri("https://api.groq.com/openai/v1"),
                    httpClient: httpClient
                );
            }
            else
            {
                //Configure AI Service
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                    modelId: "gemini-2.5-flash",
                    apiKey: geminiApiKey!,
                    httpClient: httpClient
                );
            }

            var kernel = kernelBuilder.Build();

            // 4. Add the e-commerce tools plugin
            kernel.Plugins.AddFromObject(_chatbotPlugins, "ECommercePlugins");

            // 5. Select System Prompt based on user role (centralized in LLMPrompts)
            string resolvedId = userId > 0 ? userId.ToString() : "";
            string systemPrompt = LLMPrompts.GetSystemPrompt(role, resolvedId);

            // 6. Build the Chat History context
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);

            var previousMessages = session.Messages.OrderBy(m => m.CreatedAt).ToList();
            foreach (var msg in previousMessages)
            {
                if (msg.Sender.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    chatHistory.AddUserMessage(msg.Content);
                }
                else
                {
                    chatHistory.AddAssistantMessage(msg.Content);
                }
            }

            // 7. Save user message to database (if authenticated)
            if (userId > 0)
            {
                await _chatRepository.AddMessage(session.Id, "User", message);
            }

            // Add new user message to kernel history
            chatHistory.AddUserMessage(message);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // 7.5 Guest User Restriction Check
            if (userId == 0 || role == "Guest")
            {
                var isRestricted = await IsGuestQueryRestricted(message, chatCompletionService);
                if (isRestricted)
                {
                    return new ChatMessageResponse
                    {
                        Reply = LLMPrompts.GuestRestrictedResponse,
                        SessionId = 0
                    };
                }
            }

            // 8. Pre-filter Gate: Intent Classifier
            var isQueryValid = await IsQueryValid(message, role, chatCompletionService);
            if (!isQueryValid)
            {
                var deflectingResponse = LLMPrompts.GetDeflectingResponse(role);
                if (userId > 0)
                {
                    await _chatRepository.AddMessage(session.Id, "AI", deflectingResponse);
                }

                return new ChatMessageResponse
                {
                    Reply = deflectingResponse,
                    SessionId = session.Id
                };
            }

            // 9. Execute chat completion with auto function invocation enabled
            try
            {
                var executionSettings = new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
                var replyText = reply.Content ?? "I'm sorry, I could not process that request.";

                // 9. Save AI response to database (if authenticated)
                if (userId > 0)
                {
                    await _chatRepository.AddMessage(session.Id, "AI", replyText);
                }

                return new ChatMessageResponse
                {
                    Reply = replyText,
                    SessionId = session.Id
                };
            }
            catch (Exception ex) when (isGroqConfigured && isGeminiConfigured)
            {
                // Fallback to Gemini if Groq throws an error (e.g., HTTP 429 Rate Limit)
                try
                {
                    var fallbackKernelBuilder = Kernel.CreateBuilder();
                    fallbackKernelBuilder.AddGoogleAIGeminiChatCompletion(
                        modelId: "gemini-2.5-flash",
                        apiKey: geminiApiKey!,
                        httpClient: httpClient
                    );
                    var fallbackKernel = fallbackKernelBuilder.Build();
                    fallbackKernel.Plugins.AddFromObject(_chatbotPlugins, "ECommercePlugins");
                    
                    var fallbackChatCompletionService = fallbackKernel.GetRequiredService<IChatCompletionService>();
                    var executionSettings = new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    };

                    var reply = await fallbackChatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, fallbackKernel);
                    var replyText = reply.Content ?? "I'm sorry, I could not process that request.";

                    if (userId > 0)
                    {
                        await _chatRepository.AddMessage(session.Id, "AI", replyText);
                    }

                    return new ChatMessageResponse
                    {
                        Reply = replyText,
                        SessionId = session.Id
                    };
                }
                catch (Exception fallbackEx)
                {
                    var errorMessage = $"An error occurred while communicating with Groq: {ex.Message}. Fallback to Gemini also failed: {fallbackEx.Message}";

                    if (userId > 0)
                    {
                        await _chatRepository.AddMessage(session.Id, "AI", errorMessage);
                    }

                    return new ChatMessageResponse
                    {
                        Reply = errorMessage,
                        SessionId = session.Id
                    };
                }
            }
            catch (Exception ex)
            {
                var providerName = isGroqConfigured ? "Groq" : "Gemini";
                var errorMessage = $"An error occurred while communicating with {providerName}: {ex.Message}";

                if (userId > 0)
                {
                    await _chatRepository.AddMessage(session.Id, "AI", errorMessage);
                }

                return new ChatMessageResponse
                {
                    Reply = errorMessage,
                    SessionId = session.Id
                };
            }
        }

        public async Task<ChatSessionDTO?> GetActiveSessionHistory(int userId, string role)
        {
            var session = await _chatRepository.GetSessionWithMessages(userId, role);

            if (session == null) return null;

            return new ChatSessionDTO
            {
                Id = session.Id,
                UserId = session.UserId,
                Role = session.Role,
                CreatedAt = session.CreatedAt,
                Messages = session.Messages.OrderBy(m => m.CreatedAt).Select(m => new ChatMessageDTO
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
        }

        public async Task ClearActiveSession(int userId, string role)
        {
            await _chatRepository.ClearSessions(userId, role);
        }

        private async Task<bool> IsQueryValid(string message, string role, IChatCompletionService chatService)
        {
            var classificationPrompt = LLMPrompts.BuildIntentClassifierPrompt(role);

            var classificationHistory = new ChatHistory();
            classificationHistory.AddSystemMessage(classificationPrompt);
            classificationHistory.AddUserMessage(message);

            try
            {
                var response = await chatService.GetChatMessageContentAsync(classificationHistory);
                var verdict = response.Content?.Trim().ToUpperInvariant() ?? LLMPrompts.IntentVerdict.Invalid;
                return verdict == LLMPrompts.IntentVerdict.Valid;
            }
            catch
            {
                return true;
            }
        }

        private async Task<bool> IsGuestQueryRestricted(string message, IChatCompletionService chatService)
        {
            var prompt = LLMPrompts.BuildGuestGatekeeperPrompt();

            var history = new ChatHistory();
            history.AddSystemMessage(prompt);
            history.AddUserMessage(message);

            try
            {
                var response = await chatService.GetChatMessageContentAsync(history);
                var verdict = response.Content?.Trim().ToUpperInvariant() ?? LLMPrompts.GuestVerdict.Restricted;

                return verdict != LLMPrompts.GuestVerdict.Allowed;
            }
            catch
            {
                return true;
            }
        }

        public async Task<Dictionary<string, string>> GenerateSpecsAsync(string productName, string productDescription, string specDescription)
        {
            var groqApiKey = _configuration["GroqSettings:ApiKey"];
            var geminiApiKey = _configuration["GeminiSettings:ApiKey"];

            var isGroqConfigured = !string.IsNullOrWhiteSpace(groqApiKey) && groqApiKey != "YOUR_GROQ_API_KEY_HERE";
            var isGeminiConfigured = !string.IsNullOrWhiteSpace(geminiApiKey) && geminiApiKey != "YOUR_GEMINI_API_KEY_HERE";

            if (!isGroqConfigured && !isGeminiConfigured)
            {
                throw new InvalidOperationException("AI service is not configured. Please check your API settings.");
            }

            using var httpClientHandler = new HttpClientHandler
            {
                CheckCertificateRevocationList = false
            };
            using var httpClient = new HttpClient(httpClientHandler);

            var kernelBuilder = Kernel.CreateBuilder();

            if (isGroqConfigured)
            {
                var groqModelId = _configuration["GroqSettings:ModelId"];
                if (string.IsNullOrWhiteSpace(groqModelId) || groqModelId == "llama3-8b-8192" || groqModelId == "llama-3.1-8b-instant")
                {
                    groqModelId = "llama-3.3-70b-versatile";
                }
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: groqModelId,
                    apiKey: groqApiKey!,
                    endpoint: new Uri("https://api.groq.com/openai/v1"),
                    httpClient: httpClient
                );
            }
            else
            {
                // Use a lightweight model for spec generation
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                    modelId: "gemini-2.5-flash-lite",
                    apiKey: geminiApiKey!,
                    httpClient: httpClient
                );
            }

            var kernel = kernelBuilder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            var prompt = LLMPrompts.GetGenerateSpecsPrompt(productName, productDescription, specDescription);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a system that returns raw JSON containing product technical specifications. Do not include markdown codeblocks or wrap the JSON. Return only the JSON object.");
            chatHistory.AddUserMessage(prompt);

            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
            var content = response.Content?.Trim() ?? "{}";

            // If the LLM returned markdown code blocks, strip them
            if (content.StartsWith("```"))
            {
                var lines = content.Split('\n');
                var list = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("```")) continue;
                    list.Add(lines[i]);
                }
                content = string.Join("\n", list).Trim();
            }

            var firstBrace = content.IndexOf('{');
            var lastBrace = content.LastIndexOf('}');
            if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
            {
                content = content.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            try
            {
                var specs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var cleanSpecs = new Dictionary<string, string>();
                if (specs != null)
                {
                    foreach (var kvp in specs)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Key))
                        {
                            cleanSpecs[kvp.Key.Trim()] = kvp.Value?.Trim() ?? "";
                        }
                    }
                }
                return cleanSpecs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse JSON response: {content}. Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to parse generated specifications from AI response: {ex.Message}. LLM Response: {content}");
            }
        }
    }
}