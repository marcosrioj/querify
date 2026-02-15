using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Persistence.AiDb;
using BaseFaq.AI.Common.Persistence.AiDb.Entities;
using BaseFaq.AI.Common.Providers.Abstractions;
using BaseFaq.AI.Common.Providers.Models;
using BaseFaq.AI.Generation.Business.Worker.Abstractions;
using BaseFaq.AI.Generation.Business.Worker.Observability;
using BaseFaq.AI.Generation.Business.Worker.Service;
using BaseFaq.Models.Ai.Enums;
using BaseFaq.Models.Faq.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace BaseFaq.AI.Generation.Business.Worker.Commands.ProcessFaqGenerationRequested;

public sealed class ProcessFaqGenerationRequestedCommandHandler(
    AiDbContext aiDbContext,
    IAiProviderCredentialAccessor aiProviderCredentialAccessor,
    IFaqIntegrationDbContextFactory faqIntegrationDbContextFactory,
    IGenerationFaqWriteService generationFaqWriteService,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqGenerationRequestedCommand>
{
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

        var providerCredential = aiProviderCredentialAccessor.GetCurrent();
        var job = CreateProcessingJob(message, providerCredential);
        aiDbContext.GenerationJobs.Add(job);

        try
        {
            await SaveJobWithTracingAsync("generation.ai_db.job_create", cancellationToken);
            return new GenerationProcessingContext(job, providerCredential);
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
                processingContext.ProviderCredential,
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

    private async Task<bool> IsJobAlreadyCreatedAsync(FaqGenerationRequestedV1 message,
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
        AiProviderCredential providerCredential)
    {
        return new GenerationJob
        {
            Id = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            RequestedByUserId = message.RequestedByUserId,
            FaqId = message.FaqId,
            Language = message.Language,
            PromptProfile = message.PromptProfile,
            IdempotencyKey = message.IdempotencyKey,
            RequestedUtc = message.RequestedUtc,
            StartedUtc = DateTime.UtcNow,
            Status = GenerationJobStatus.Processing,
            Provider = providerCredential.Provider,
            Model = providerCredential.Model
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
        AiProviderCredential providerCredential,
        GenerationJob job,
        CancellationToken cancellationToken)
    {
        var studiedRefs = await LoadStudiedRefsAsync(message, cancellationToken);
        using var providerActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.provider.generate", ActivityKind.Client);

        AddProviderActivityTags(providerActivity, providerCredential, message, studiedRefs);
        AddDraftArtifact(message, studiedRefs, job);
        await WriteDraftFaqItemAsync(message, studiedRefs, cancellationToken);

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
        await using var faqDbContext = faqIntegrationDbContextFactory.Create(message.TenantId);
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

    private static void AddProviderActivityTags(
        Activity? providerActivity,
        AiProviderCredential providerCredential,
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs)
    {
        providerActivity?.SetTag("gen_ai.system", providerCredential.Provider);
        providerActivity?.SetTag("gen_ai.request.model", providerCredential.Model);
        providerActivity?.SetTag("basefaq.ai_key_slot", providerCredential.SelectedSlot);
        providerActivity?.SetTag("basefaq.ai_key_configured",
            !string.IsNullOrWhiteSpace(providerCredential.ApiKey));
        providerActivity?.SetTag("basefaq.correlation_id", message.CorrelationId.ToString("D"));
        providerActivity?.SetTag("basefaq.content_ref.total_count", studiedRefs.TotalCount);
        providerActivity?.SetTag("basefaq.content_ref.processed_count", studiedRefs.ProcessedCount);
        providerActivity?.SetTag("basefaq.content_ref.skipped_count", studiedRefs.SkippedCount);
    }

    private static void AddDraftArtifact(
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        GenerationJob job)
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
                    processedKinds = studiedRefs.StudiedRefs.Select(x => x.Kind.ToString()).ToArray()
                }),
                GenerationArtifact.MaxMetadataJsonLength)
        });
    }

    private async Task WriteDraftFaqItemAsync(
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        CancellationToken cancellationToken)
    {
        await generationFaqWriteService.WriteAsync(
            new GenerationFaqWriteRequest(
                message.CorrelationId,
                message.FaqId,
                message.TenantId,
                Truncate(BuildDraftQuestion(studiedRefs), 1000),
                Truncate(BuildDraftSummary(message.FaqId, studiedRefs), 250),
                Truncate(BuildDraftContent(message.FaqId, studiedRefs), 5000),
                null,
                null,
                null,
                80),
            cancellationToken);
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

    private sealed record GenerationProcessingContext(GenerationJob Job, AiProviderCredential ProviderCredential);
}