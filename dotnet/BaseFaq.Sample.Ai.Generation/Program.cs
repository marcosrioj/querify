using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace BaseFaq.Sample.Ai.Generation;

public static class Program
{
    private const int DefaultSuppliedContextMaxCharacters = 10000;
    private const string REFERENCE_CONTEXT_KEY = "{{REFERENCE_CONTEXT}}";
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);

    private static readonly Regex ScriptRegex = new(
        @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StyleRegex = new(
        @"<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex WhiteSpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static async Task Main()
    {
        var settings = GetSettings();
        var userPrompt = await BuildUserPromptAsync(settings);

        var client = new ChatClient(settings.Model, settings.ApiKey);

        var completionResult = await client.CompleteChatAsync(
            [
                new SystemChatMessage(settings.SystemPrompt),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = DefaultSuppliedContextMaxCharacters
            });
        var completion = completionResult.Value;

        var generatedText = string.Concat(
            completion.Content
                .Where(part => !string.IsNullOrWhiteSpace(part.Text))
                .Select(part => part.Text));

        Console.WriteLine(string.IsNullOrWhiteSpace(generatedText) ? "<empty response>" : generatedText);
    }

    private static IConfiguration BuildConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
        }

        var baseDirectory = AppContext.BaseDirectory;
        if (!string.Equals(baseDirectory, Directory.GetCurrentDirectory(), StringComparison.OrdinalIgnoreCase))
        {
            builder.AddJsonFile(Path.Combine(baseDirectory, "appsettings.json"), optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile(Path.Combine(baseDirectory, $"appsettings.{environmentName}.json"),
                    optional: true);
            }
        }

        builder.AddEnvironmentVariables();
        return builder.Build();
    }

    private static Settings GetSettings()
    {
        var apiKey = BuildConfiguration().GetSection("OpenAiKey").Value
                     ?? throw new InvalidOperationException("OpenAiKey is not set");
        var model = "gpt-5.2";
        var systemPrompt = """
                           You are a multilingual FAQ generation engine.
                           Objective: transform supplied reference context into a high-quality FAQ draft.
                           Hard constraints:
                           - Use only supplied context. Do not invent facts, numbers, policies, or citations.
                           - If evidence is incomplete, keep claims conservative and add uncertainty notes.
                           - Keep language aligned with the requested language.
                           - Produce output that is valid against the required JSON schema.
                           - Return JSON only, with no markdown fences or extra prose.
                           Quality bar:
                           - Question should be explicit and user-facing.
                           - Summary must be concise and scannable.
                           - Answer should be structured, practical, and traceable to cited references.
                           - Confidence should reflect evidence quality and coverage.
                           Optimize for strict JSON reliability and concise factual language; avoid stylistic verbosity.
                           """;

        var userPrompt = """
                         TASK
                         Generate an FAQ draft strictly from the supplied REFERENCE_CONTEXT.

                         TARGET_LANGUAGE
                         pt-br

                         SCOPE
                         - Generate between 6 and 8 user-facing questions.
                         - Focus only on tags explicitly supported by the reference.
                         - Do not expand beyond the documented scope.

                         REFERENCE_CONTEXT
                         ---

                         ---

                         OUTPUT_SCHEMA
                         {
                           "faqs": [
                             {
                               "question": "string",
                               "summary": "string",
                               "answer": "string",
                               "sources": [
                                 {
                                   "excerpt": "string",
                                   "reference_id": "string"
                                 }
                               ],
                               "confidence": "low | medium | high"
                             }
                           ]
                         }

                         STRICT RULES
                         - Use ONLY the REFERENCE_CONTEXT above.
                         - If the context does not clearly support a claim, omit it.
                         - If uncertainty exists, explicitly state it in the answer.
                         - Do not fabricate citations.
                         - All sources.excerpt must be exact substrings from REFERENCE_CONTEXT.
                         - Return JSON only.
                         """;

        var suppliedContextUrl =
            "https://www.mercadolivre.com.br/super-nintendo-fat-original-completo--super-mario-world/up/MLBU3683017391";

        var suppliedContextMaxCharacters = DefaultSuppliedContextMaxCharacters;

        return new Settings(
            apiKey,
            model,
            systemPrompt,
            userPrompt,
            suppliedContextUrl,
            suppliedContextMaxCharacters);
    }

    private static async Task<string> BuildUserPromptAsync(Settings settings)
    {
        var suppliedContext = await LoadSuppliedContextAsync(
            settings.SuppliedContextUrl!,
            settings.SuppliedContextMaxCharacters);

        return settings.UserPrompt.Replace(REFERENCE_CONTEXT_KEY, suppliedContext);
    }

    private static async Task<string> LoadSuppliedContextAsync(string url, int maxCharacters)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                "OpenAi:SuppliedContextUrl must be a valid HTTP/HTTPS URL.");
        }

        using var httpClient = CreateHttpClient();
        var rawContent = await httpClient.GetStringAsync(uri);

        var sanitizedContent = SanitizeContext(rawContent);
        if (sanitizedContent.Length <= maxCharacters)
        {
            return sanitizedContent;
        }

        var truncated = sanitizedContent[..maxCharacters];
        var breakIndex = truncated.LastIndexOf(' ');
        if (breakIndex > 0)
        {
            truncated = truncated[..breakIndex];
        }

        return $"{truncated} ...";
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("BaseFaq.Sample.Ai.Generation", "1.0"));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("text/html"));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("text/plain"));

        return httpClient;
    }

    private static string SanitizeContext(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return string.Empty;
        }

        var withoutScripts = ScriptRegex.Replace(rawContent, " ");
        var withoutStyles = StyleRegex.Replace(withoutScripts, " ");
        var withoutTags = HtmlTagRegex.Replace(withoutStyles, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhiteSpaceRegex.Replace(decoded, " ").Trim();
    }

    private sealed record Settings(
        string ApiKey,
        string Model,
        string SystemPrompt,
        string UserPrompt,
        string SuppliedContextUrl,
        int SuppliedContextMaxCharacters);
}