using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Persistence.AiDb;
using BaseFaq.AI.Common.Persistence.AiDb.Entities;
using BaseFaq.AI.Generation.Business.Worker.Observability;
using BaseFaq.AI.Generation.Business.Worker.Service;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Ai.Enums;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Generation.Business.Worker.Commands.ProcessFaqGenerationRequested;

public sealed class ProcessFaqGenerationRequestedCommandHandler(
    AiDbContext aiDbContext,
    TenantDbContext tenantDbContext,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IConfiguration configuration,
    ILogger<ProcessFaqGenerationRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqGenerationRequestedCommand>
{
    private const string PromptProfileFallback = "default";
    private const string PromptDomain = "generation";
    private const string PromptVersion = "2026-02-15.generation.v1";

    public async Task Handle(ProcessFaqGenerationRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;
        var processingContext = await TryInitializeProcessingAsync(command, message, cancellationToken);
        if (processingContext is null)
        {
            return;
        }

        await ExecuteJobLifecycleAsync(message, processingContext, cancellationToken);
        await MarkProcessedAsync(command.HandlerName, command.MessageId, cancellationToken);
    }

    private async Task<GenerationProcessingContext?> TryInitializeProcessingAsync(
        ProcessFaqGenerationRequestedCommand command,
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        if (await IsMessageAlreadyProcessedAsync(command.HandlerName, command.MessageId, cancellationToken))
        {
            return null;
        }

        if (await IsJobAlreadyCreatedAsync(message, cancellationToken))
        {
            await MarkProcessedAsync(command.HandlerName, command.MessageId, cancellationToken);
            return null;
        }

        var providerContext = await ResolveTenantAiProviderAsync(
            message.TenantId,
            AiCommandType.Generation,
            cancellationToken);

        var job = CreateProcessingJob(message, providerContext);
        aiDbContext.GenerationJobs.Add(job);

        try
        {
            await SaveJobWithTracingAsync("generation.ai_db.job_create", cancellationToken);
            return new GenerationProcessingContext(job, providerContext);
        }
        catch (DbUpdateException ex) when (IsDuplicateJobException(ex))
        {
            await MarkProcessedAsync(command.HandlerName, command.MessageId, cancellationToken);
            return null;
        }
    }

    private async Task ExecuteJobLifecycleAsync(
        FaqGenerationRequestedV1 message,
        GenerationProcessingContext processingContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await ProcessGenerationAsync(
                message,
                processingContext.ProviderContext,
                processingContext.Job,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await FailGenerationAsync(message, processingContext.Job, ex, cancellationToken);
        }
    }

    private async Task<bool> IsMessageAlreadyProcessedAsync(
        string handlerName,
        string messageId,
        CancellationToken cancellationToken)
    {
        return await aiDbContext.ProcessedMessages
            .AnyAsync(
                x => x.HandlerName == handlerName && x.MessageId == messageId,
                cancellationToken);
    }

    private async Task<bool> IsJobAlreadyCreatedAsync(
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        return await aiDbContext.GenerationJobs
            .AnyAsync(
                x =>
                    x.CorrelationId == message.CorrelationId ||
                    (x.FaqId == message.FaqId && x.IdempotencyKey == message.IdempotencyKey),
                cancellationToken);
    }

    private static GenerationJob CreateProcessingJob(
        FaqGenerationRequestedV1 message,
        GenerationAiProviderContext providerContext)
    {
        return new GenerationJob
        {
            Id = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            RequestedByUserId = message.RequestedByUserId,
            FaqId = message.FaqId,
            Language = message.Language,
            PromptProfile = BuildPromptProfile(providerContext),
            IdempotencyKey = message.IdempotencyKey,
            RequestedUtc = message.RequestedUtc,
            StartedUtc = DateTime.UtcNow,
            Status = GenerationJobStatus.Processing,
            Provider = providerContext.Provider,
            Model = providerContext.Model
        };
    }

    private async Task SaveJobWithTracingAsync(string activityName, CancellationToken cancellationToken)
    {
        using var activity =
            GenerationWorkerTracing.ActivitySource.StartActivity(activityName, ActivityKind.Internal);

        activity?.SetTag("db.system", "postgresql");
        activity?.SetTag("db.name", "bf_ai_db");

        await aiDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessGenerationAsync(
        FaqGenerationRequestedV1 message,
        GenerationAiProviderContext providerContext,
        GenerationJob job,
        CancellationToken cancellationToken)
    {
        var studiedRefs = await LoadStudiedRefsAsync(message, cancellationToken);
        var promptData = BuildPromptData(message, studiedRefs, providerContext);

        using var providerActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.provider.generate", ActivityKind.Client);

        AddProviderActivityTags(providerActivity, providerContext, message, studiedRefs, promptData);
        AddDraftArtifact(message, studiedRefs, job, promptData);
        await WriteGeneratedFaqItemAsync(message, studiedRefs, cancellationToken);

        job.Status = GenerationJobStatus.Succeeded;
        job.CompletedUtc = DateTime.UtcNow;
        job.ErrorCode = null;
        job.ErrorMessage = null;

        await SaveJobWithTracingAsync("generation.ai_db.job_complete", cancellationToken);
        await PublishGenerationReadyAsync(message, job.Id, cancellationToken);
    }

    private async Task<ContentRefStudyResult> LoadStudiedRefsAsync(
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        await using var faqDbContext = CreateFaqDbContext(message.TenantId);

        var contentRefs = await faqDbContext.FaqContentRefs
            .AsNoTracking()
            .Where(x => x.FaqId == message.FaqId && x.TenantId == message.TenantId)
            .Select(x => new ContentRefStudyInput(x.ContentRef.Kind, x.ContentRef.Locator))
            .ToListAsync(cancellationToken);

        if (contentRefs.Count == 0)
        {
            throw new InvalidOperationException(
                $"FAQ '{message.FaqId}' must have at least one ContentRef to continue generation.");
        }

        return ContentRefStudyService.Study(contentRefs);
    }

    private async Task WriteGeneratedFaqItemAsync(
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        CancellationToken cancellationToken)
    {
        using var writeActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.faq_db.write", ActivityKind.Internal);

        writeActivity?.SetTag("db.system", "postgresql");
        writeActivity?.SetTag("db.name", "bf_faq_db");
        writeActivity?.SetTag("basefaq.correlation_id", message.CorrelationId.ToString("D"));
        writeActivity?.SetTag("basefaq.tenant_id", message.TenantId.ToString("D"));
        writeActivity?.SetTag("basefaq.faq_id", message.FaqId.ToString("D"));

        await using var faqDbContext = CreateFaqDbContext(message.TenantId);

        var faq = await faqDbContext.Faqs
            .FirstOrDefaultAsync(x => x.Id == message.FaqId, cancellationToken);

        if (faq is null)
        {
            throw new InvalidOperationException(
                $"FAQ '{message.FaqId}' was not found for tenant '{message.TenantId}'.");
        }

        var itemId = CreateDeterministicFaqItemId(message.CorrelationId, message.FaqId, message.TenantId);
        var question = Truncate(BuildDraftQuestion(studiedRefs), FaqItem.MaxQuestionLength);
        var shortAnswer = Truncate(BuildDraftSummary(message.FaqId, studiedRefs), FaqItem.MaxShortAnswerLength);
        var answer = Truncate(BuildDraftContent(message.FaqId, studiedRefs), FaqItem.MaxAnswerLength);

        var faqItem = await faqDbContext.FaqItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.Id == itemId && x.FaqId == message.FaqId && x.TenantId == message.TenantId,
                cancellationToken);

        if (faqItem is null)
        {
            faqItem = new FaqItem
            {
                Id = itemId,
                TenantId = message.TenantId,
                FaqId = message.FaqId,
                Question = question,
                ShortAnswer = shortAnswer,
                Answer = answer,
                AdditionalInfo = null,
                CtaTitle = null,
                CtaUrl = null,
                VoteScore = 0,
                AiConfidenceScore = 80,
                IsActive = true,
                Sort = await GetNextSortAsync(faqDbContext, message, cancellationToken)
            };

            faqDbContext.FaqItems.Add(faqItem);
        }
        else
        {
            faqItem.Question = question;
            faqItem.ShortAnswer = shortAnswer;
            faqItem.Answer = answer;
            faqItem.AdditionalInfo = null;
            faqItem.CtaTitle = null;
            faqItem.CtaUrl = null;
            faqItem.AiConfidenceScore = 80;
            faqItem.IsActive = true;
            faqItem.IsDeleted = false;
            faqItem.DeletedDate = null;
            faqItem.DeletedBy = null;
        }

        await faqDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "FAQ generation write completed for CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}.",
            message.CorrelationId,
            message.FaqId,
            message.TenantId);
    }

    private static async Task<int> GetNextSortAsync(
        FaqDbContext faqDbContext,
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var maxSort = await faqDbContext.FaqItems
            .Where(x => x.FaqId == message.FaqId && !x.IsDeleted)
            .Select(x => (int?)x.Sort)
            .MaxAsync(cancellationToken);

        return (maxSort ?? 0) + 1;
    }

    private static void AddProviderActivityTags(
        Activity? providerActivity,
        GenerationAiProviderContext providerContext,
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        GenerationPromptData promptData)
    {
        providerActivity?.SetTag("gen_ai.system", providerContext.Provider);
        providerActivity?.SetTag("gen_ai.request.model", providerContext.Model);
        providerActivity?.SetTag("basefaq.ai_key_configured", !string.IsNullOrWhiteSpace(providerContext.ApiKey));
        providerActivity?.SetTag("basefaq.correlation_id", message.CorrelationId.ToString("D"));
        providerActivity?.SetTag("basefaq.content_ref.total_count", studiedRefs.TotalCount);
        providerActivity?.SetTag("basefaq.content_ref.processed_count", studiedRefs.ProcessedCount);
        providerActivity?.SetTag("basefaq.content_ref.skipped_count", studiedRefs.SkippedCount);
        providerActivity?.SetTag("basefaq.prompt.domain", promptData.Domain);
        providerActivity?.SetTag("basefaq.prompt.version", promptData.Version);
        providerActivity?.SetTag("basefaq.prompt.provider", promptData.Provider);
    }

    private static void AddDraftArtifact(
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        GenerationJob job,
        GenerationPromptData promptData)
    {
        var draftContent = BuildDraftContent(message.FaqId, studiedRefs);

        job.Artifacts.Add(new GenerationArtifact
        {
            GenerationJobId = job.Id,
            ArtifactType = GenerationArtifactType.Draft,
            Sequence = 1,
            Content = Truncate(draftContent, GenerationArtifact.MaxContentLength),
            MetadataJson = Truncate(
                JsonSerializer.Serialize(new
                {
                    contentRefTotal = studiedRefs.TotalCount,
                    contentRefProcessed = studiedRefs.ProcessedCount,
                    contentRefSkipped = studiedRefs.SkippedCount,
                    processedKinds = studiedRefs.StudiedRefs.Select(x => x.Kind.ToString()).ToArray(),
                    promptDomain = promptData.Domain,
                    promptVersion = promptData.Version,
                    promptProvider = promptData.Provider,
                    promptTemplateHash = ComputeHash(promptData.Template),
                    promptInputHash = ComputeHash(promptData.Input),
                    outputSchemaHash = ComputeHash(promptData.OutputSchema)
                }),
                GenerationArtifact.MaxMetadataJsonLength)
        });
    }

    private async Task PublishGenerationReadyAsync(
        FaqGenerationRequestedV1 message,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqGenerationReadyV1
        {
            EventId = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            JobId = jobId,
            FaqId = message.FaqId,
            TenantId = message.TenantId,
            OccurredUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task FailGenerationAsync(
        FaqGenerationRequestedV1 message,
        GenerationJob job,
        Exception ex,
        CancellationToken cancellationToken)
    {
        job.Status = GenerationJobStatus.Failed;
        job.CompletedUtc = DateTime.UtcNow;

        const string errorCode = "GENERATION_FAILED";
        var errorMessage = ex.Message.Length <= GenerationJob.MaxErrorMessageLength
            ? ex.Message
            : ex.Message[..GenerationJob.MaxErrorMessageLength];

        job.ErrorCode = errorCode;
        job.ErrorMessage = errorMessage;

        using var failJobActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.ai_db.job_fail", ActivityKind.Internal);

        failJobActivity?.SetTag("db.system", "postgresql");
        failJobActivity?.SetTag("db.name", "bf_ai_db");
        failJobActivity?.SetTag("exception.type", ex.GetType().Name);

        await aiDbContext.SaveChangesAsync(cancellationToken);
        await PublishGenerationFailedAsync(message, job.Id, errorCode, errorMessage, cancellationToken);
    }

    private async Task PublishGenerationFailedAsync(
        FaqGenerationRequestedV1 message,
        Guid jobId,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqGenerationFailedV1
        {
            EventId = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            JobId = jobId,
            FaqId = message.FaqId,
            TenantId = message.TenantId,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            OccurredUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task MarkProcessedAsync(string handlerName, string messageId, CancellationToken cancellationToken)
    {
        aiDbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            HandlerName = handlerName,
            MessageId = messageId,
            ProcessedUtc = DateTime.UtcNow
        });

        try
        {
            await aiDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another consumer execution may persist the same dedupe key first.
        }
    }

    private static bool IsDuplicateJobException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        return message.Contains("IX_GenerationJob_CorrelationId", StringComparison.Ordinal) ||
               message.Contains("IX_GenerationJob_FaqId_IdempotencyKey", StringComparison.Ordinal);
    }

    private static string BuildDraftQuestion(ContentRefStudyResult studyResult)
    {
        if (studyResult.ProcessedCount == 0)
        {
            return "Generated draft question based on available content references";
        }

        var kinds = string.Join(", ", studyResult.StudiedRefs.Select(x => x.Kind.ToString()));
        return $"Generated draft question based on: {kinds}";
    }

    private static string BuildDraftSummary(Guid faqId, ContentRefStudyResult studyResult)
    {
        return
            $"Draft summary for FAQ {faqId}. ContentRefs total={studyResult.TotalCount}, processed={studyResult.ProcessedCount}, skipped={studyResult.SkippedCount}.";
    }

    private static string BuildDraftContent(Guid faqId, ContentRefStudyResult studyResult)
    {
        if (studyResult.ProcessedCount == 0)
        {
            return
                $"Generated draft placeholder for FAQ {faqId}. No processable ContentRef kind was found (all were skipped by business rules).";
        }

        var lines = studyResult.StudiedRefs
            .Select(x => $"{x.Kind} ({x.Locator}): {x.MainSubject}");

        return
            $"Generated draft placeholder for FAQ {faqId}. Source study:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static Guid CreateDeterministicFaqItemId(Guid correlationId, Guid faqId, Guid tenantId)
    {
        var input = $"{correlationId:N}:{faqId:N}:{tenantId:N}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));

        hash[6] = (byte)((hash[6] & 0x0F) | (3 << 4));
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }

    private static string BuildPromptProfile(GenerationAiProviderContext providerContext)
    {
        var provider = providerContext.Provider?.Trim();
        if (string.IsNullOrWhiteSpace(provider))
        {
            return PromptProfileFallback;
        }

        var profile = $"{provider}-{PromptProfileFallback}";
        return profile.Length <= GenerationJob.MaxPromptProfileLength
            ? profile
            : profile[..GenerationJob.MaxPromptProfileLength];
    }

    private static GenerationPromptData BuildPromptData(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        GenerationAiProviderContext providerContext)
    {
        var provider = NormalizeProvider(providerContext.Provider);

        return new GenerationPromptData(
            PromptDomain,
            PromptVersion,
            provider,
            ResolvePromptTemplate(providerContext.Prompt),
            BuildPromptInput(request, studiedRefs),
            BuildOutputSchema());
    }

    private static string ResolvePromptTemplate(string? prompt)
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        return
            "You are a multilingual FAQ generation engine. Use only supplied context and return schema-compliant JSON.";
    }

    private static string BuildPromptInput(FaqGenerationRequestedV1 request, ContentRefStudyResult studiedRefs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Task: generate a FAQ draft from studied references.");
        builder.AppendLine($"faqId: {request.FaqId:D}");
        builder.AppendLine($"tenantId: {request.TenantId:D}");
        builder.AppendLine($"language: {request.Language}");
        builder.AppendLine($"refs.total: {studiedRefs.TotalCount}");
        builder.AppendLine($"refs.processed: {studiedRefs.ProcessedCount}");
        builder.AppendLine($"refs.skipped: {studiedRefs.SkippedCount}");
        builder.AppendLine("references:");

        foreach (var studiedRef in studiedRefs.StudiedRefs)
        {
            builder.AppendLine(
                $"- kind={studiedRef.Kind}, locator={studiedRef.Locator}, inferredSubject={studiedRef.MainSubject}");
        }

        builder.AppendLine("Output must follow the provided JSON schema exactly.");
        return builder.ToString();
    }

    private static string BuildOutputSchema()
    {
        return """
               {
                 "type": "object",
                 "required": ["question", "summary", "answer", "confidence", "citations", "uncertaintyNotes"],
                 "properties": {
                   "question": { "type": "string", "maxLength": 1000 },
                   "summary": { "type": "string", "maxLength": 250 },
                   "answer": { "type": "string", "maxLength": 5000 },
                   "confidence": { "type": "integer", "minimum": 0, "maximum": 100 },
                   "citations": {
                     "type": "array",
                     "items": { "type": "string", "maxLength": 2000 }
                   },
                   "uncertaintyNotes": {
                     "type": "array",
                     "items": { "type": "string", "maxLength": 500 }
                   }
                 }
               }
               """;
    }

    private static string NormalizeProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "unknown"
            : provider.Trim().ToLowerInvariant();
    }

    private async Task<GenerationAiProviderContext> ResolveTenantAiProviderAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken)
    {
        var provider = await tenantDbContext.TenantAiProviders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AiProvider.Command == commandType)
            .OrderByDescending(x => x.AiProvider.Provider.ToLower() == "openai")
            .ThenBy(x => x.AiProvider.Provider)
            .ThenBy(x => x.AiProvider.Model)
            .Select(x => new GenerationAiProviderContext(
                x.AiProvider.Provider,
                x.AiProvider.Model,
                x.AiProvider.Prompt,
                x.AiProviderKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{tenantId}' has no AI provider configured for '{commandType}'.");
        }

        return provider;
    }

    private FaqDbContext CreateFaqDbContext(Guid tenantId)
    {
        var connectionString = tenantConnectionStringProvider.GetConnectionString(tenantId);

        var options = new DbContextOptionsBuilder<FaqDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FaqDbContext(
            options,
            new IntegrationSessionService(tenantId, ResolveAiUserId(configuration)),
            configuration,
            new StaticTenantConnectionStringProvider(connectionString),
            new HttpContextAccessor());
    }

    private static Guid ResolveAiUserId(IConfiguration configuration)
    {
        return Guid.TryParse(configuration["Ai:UserId"], out var configuredUserId)
            ? configuredUserId
            : Guid.Empty;
    }

    private sealed class IntegrationSessionService(Guid tenantId, Guid userId) : ISessionService
    {
        public Guid GetTenantId(AppEnum app) => tenantId;
        public Guid GetUserId() => userId;
    }

    private sealed class StaticTenantConnectionStringProvider(string connectionString)
        : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid tenantId) => connectionString;
    }

    private sealed record GenerationAiProviderContext(
        string Provider,
        string Model,
        string? Prompt,
        string? ApiKey);

    private sealed record GenerationPromptData(
        string Domain,
        string Version,
        string Provider,
        string Template,
        string Input,
        string OutputSchema);

    private sealed record GenerationProcessingContext(GenerationJob Job, GenerationAiProviderContext ProviderContext);
}