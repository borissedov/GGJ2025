using System.Text.Json.Serialization;

namespace GlobalGameJam2025_Bubbles.Services.GptOutput.DeepSeek;

public class ChatRequestPayload
        {
            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("messages")]
            public List<MessagePayload> Messages { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        // Message details for the request payload
        public class MessagePayload
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        // Response received from DeepSeek
        public class DeepSeekResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            [JsonPropertyName("created")]
            public int Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("system_fingerprint")]
            public string SystemFingerprint { get; set; }

            // "object" is a reserved keyword in C# so we use "Object" instead.
            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("usage")]
            public Usage Usage { get; set; }
        }

        // Details for each choice returned in the response
        public class Choice
        {
            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; }

            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        // The assistant message
        public class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        // Token usage details (if needed)
        public class Usage
        {
            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }