using System.Text;
using System.Text.Json;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Answers;
using Querify.QnA.Common.Domain.BusinessRules.Questions;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Common.Domain.Entities;
using Querify.Tools.Seed.Abstractions;
using Querify.Tools.Seed.Configuration;

namespace Querify.Tools.Seed.Application;

public sealed class QnASeedService : IQnASeedService
{
    private const string SeedLanguage = "en-US";
    private static readonly DateTime SeedBaseTimeUtc = new(2026, 4, 6, 12, 0, 0, DateTimeKind.Utc);

    private static readonly string[] FeedbackReasons =
    [
        "Missing account-specific edge case",
        "Needs a clearer troubleshooting path",
        "This answer is outdated for the current product flow",
        "The cited source does not match the final recommendation"
    ];

    public bool HasData(QnADbContext dbContext)
    {
        return dbContext.Spaces.Any() ||
               dbContext.Questions.Any() ||
               dbContext.Answers.Any() ||
               dbContext.Tags.Any() ||
               dbContext.Sources.Any() ||
               dbContext.Activities.Any() ||
               dbContext.SpaceTags.Any() ||
               dbContext.SpaceSources.Any() ||
               dbContext.QuestionTags.Any() ||
               dbContext.QuestionSourceLinks.Any() ||
               dbContext.AnswerSourceLinks.Any();
    }

    public void Seed(QnADbContext dbContext, Guid tenantId, SeedCounts counts)
    {
        var selectedSpaces = SelectCatalog(counts);
        if (selectedSpaces.Count == 0)
        {
            return;
        }

        var tags = BuildTags(selectedSpaces, tenantId, counts.TagCount);
        var tagsByName = tags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);

        var sources = BuildSources(selectedSpaces, tenantId, counts.SourceCount);
        var sourcesByUrl = sources.ToDictionary(source => source.Locator, StringComparer.OrdinalIgnoreCase);

        var spaces = selectedSpaces
            .Select((definition, index) => CreateSpace(definition, tenantId, index))
            .ToList();
        var acceptedAnswerAssignments = new List<AcceptedAnswerAssignment>();

        dbContext.Tags.AddRange(tags);
        dbContext.Sources.AddRange(sources);
        dbContext.Spaces.AddRange(spaces);
        dbContext.SaveChanges();

        for (var spaceIndex = 0; spaceIndex < selectedSpaces.Count; spaceIndex++)
        {
            var definition = selectedSpaces[spaceIndex];
            var space = spaces[spaceIndex];

            SeedSpaceRelationships(space, definition, tagsByName, sourcesByUrl, counts, tenantId);
            SeedQuestions(
                dbContext,
                space,
                definition,
                tagsByName,
                sourcesByUrl,
                counts,
                tenantId,
                spaceIndex,
                acceptedAnswerAssignments);
        }

        dbContext.SaveChanges();

        foreach (var assignment in acceptedAnswerAssignments)
        {
            QuestionRules.ApplyAcceptedAnswer(assignment.Question, assignment.Answer);
        }

        dbContext.SaveChanges();
    }

    private static List<SeedSpaceDefinition> SelectCatalog(SeedCounts counts)
    {
        return QnASeedCatalog.Build()
            .Take(Math.Max(0, counts.SpaceCount))
            .Select(definition => definition with
            {
                Items = definition.Items.Take(Math.Max(0, counts.QuestionsPerSpace)).ToArray()
            })
            .Where(definition => definition.Items.Count > 0)
            .ToList();
    }

    private static List<Tag> BuildTags(
        IReadOnlyCollection<SeedSpaceDefinition> spaceDefinitions,
        Guid tenantId,
        int maxTagCount)
    {
        return spaceDefinitions
            .SelectMany(definition => definition.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(0, maxTagCount))
            .Select(tagValue => new Tag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = tagValue,
                CreatedBy = "seed",
                UpdatedBy = "seed"
            })
            .ToList();
    }

    private static List<Source> BuildSources(
        IReadOnlyCollection<SeedSpaceDefinition> spaceDefinitions,
        Guid tenantId,
        int maxSourceCount)
    {
        var uploadedSources = BuildUploadedSourceSamples(tenantId)
            .Take(Math.Max(0, maxSourceCount))
            .ToList();
        var remainingSourceCount = Math.Max(0, maxSourceCount - uploadedSources.Count);

        uploadedSources.AddRange(spaceDefinitions
            .SelectMany(definition => definition.Items)
            .DistinctBy(item => item.SourceUrl, StringComparer.OrdinalIgnoreCase)
            .Take(remainingSourceCount)
            .Select(item => CreateSource(item, tenantId)));

        return uploadedSources;
    }

    private static IEnumerable<Source> BuildUploadedSourceSamples(Guid tenantId)
    {
        var productManualSourceId = Guid.Parse("1f6d6a9a-44df-4c8f-8d8d-0e6a4f012001");
        var privacyPolicySourceId = Guid.Parse("1f6d6a9a-44df-4c8f-8d8d-0e6a4f012002");

        yield return CreateUploadedSource(
            tenantId,
            productManualSourceId,
            "Manual de produto.pdf",
            "Product manual PDF",
            184_320,
            "sha256:5f1a3e9b17f7d913cb6c0a1f3e02a7b8f0d4b8bda19ad16f8a9ad7cb0c7f0001");

        yield return CreateUploadedSource(
            tenantId,
            privacyPolicySourceId,
            "Política de privacidade.pdf",
            "Privacy policy PDF",
            96_512,
            "sha256:5f1a3e9b17f7d913cb6c0a1f3e02a7b8f0d4b8bda19ad16f8a9ad7cb0c7f0002");
    }

    private static Source CreateUploadedSource(
        Guid tenantId,
        Guid sourceId,
        string fileName,
        string label,
        long sizeBytes,
        string checksum)
    {
        var storageKey = SourceStorageKey.BuildVerifiedKey(tenantId, sourceId, fileName);
        return new Source
        {
            Id = sourceId,
            TenantId = tenantId,
            Kind = SourceKind.Pdf,
            Locator = storageKey,
            StorageKey = storageKey,
            Label = label,
            ContextNote = "Seeded uploaded source sample",
            ExternalId = $"seed-upload-{sourceId:N}",
            Language = SeedLanguage,
            MediaType = "application/pdf",
            SizeBytes = sizeBytes,
            Checksum = checksum,
            MetadataJson = JsonSerializer.Serialize(new
            {
                source = "seed",
                storage = "synthetic"
            }),
            Visibility = VisibilityScope.Internal,
            LastVerifiedAtUtc = SeedBaseTimeUtc,
            UploadStatus = SourceUploadStatus.Verified,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
    }

    private static Source CreateSource(SeedQuestionDefinition item, Guid tenantId)
    {
        var source = new Source
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Kind = ResolveSourceKind(item),
            Locator = item.SourceUrl,
            Label = item.SourceLabel,
            ContextNote = BuildSourceContextNote(item),
            ExternalId = BuildSourceExternalId(item),
            Language = SeedLanguage,
            MediaType = ResolveMediaType(item),
            Checksum = BuildChecksum(item),
            MetadataJson = BuildSourceMetadataJson(item),
            Visibility = VisibilityScope.Public,
            LastVerifiedAtUtc = BuildVerifiedAtUtc(item),
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };

        SourceRules.EnsureVisibilityAllowed(source, source.Visibility);
        return source;
    }

    private static Space CreateSpace(SeedSpaceDefinition definition, Guid tenantId, int index)
    {
        var space = new Space
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = definition.Name,
            Slug = NormalizeKey(definition.Name, $"space-{index + 1}"),
            Summary = BuildSpaceSummary(definition),
            Language = SeedLanguage,
            Status = SpaceStatus.Active,
            Visibility = VisibilityScope.Public,
            AcceptsQuestions = true,
            AcceptsAnswers = true,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };

        SpaceRules.EnsureVisibilityAllowed(space, space.Visibility);
        return space;
    }

    private static string BuildSpaceSummary(SeedSpaceDefinition definition)
    {
        var topics = string.Join(", ", definition.Tags.Take(3));
        return $"Operational QnA space for {definition.Name} covering {definition.Items.Count} canonical questions and curated sources on {topics}.";
    }

    private static void SeedSpaceRelationships(
        Space space,
        SeedSpaceDefinition definition,
        IReadOnlyDictionary<string, Tag> tagsByName,
        IReadOnlyDictionary<string, Source> sourcesByUrl,
        SeedCounts counts,
        Guid tenantId)
    {
        foreach (var tagName in definition.Tags
                     .Where(tagsByName.ContainsKey)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Take(Math.Max(0, counts.TagsPerSpace)))
        {
            var tag = tagsByName[tagName];
            SpaceRules.EnsureTagLink(space, tag, tenantId, "seed");
        }

        foreach (var sourceUrl in definition.Items
                     .Select(item => item.SourceUrl)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Where(sourcesByUrl.ContainsKey)
                     .Take(Math.Max(0, counts.SourcesPerSpace)))
        {
            var source = sourcesByUrl[sourceUrl];
            SpaceRules.EnsureSourceLink(space, source, tenantId, "seed");
        }
    }

    private static void SeedQuestions(
        QnADbContext dbContext,
        Space space,
        SeedSpaceDefinition definition,
        IReadOnlyDictionary<string, Tag> tagsByName,
        IReadOnlyDictionary<string, Source> sourcesByUrl,
        SeedCounts counts,
        Guid tenantId,
        int spaceIndex,
        ICollection<AcceptedAnswerAssignment> acceptedAnswerAssignments)
    {
        SpaceRules.EnsureAcceptsQuestions(space);
        SpaceRules.EnsureAcceptsAnswers(space);

        for (var questionIndex = 0; questionIndex < definition.Items.Count; questionIndex++)
        {
            var item = definition.Items[questionIndex];
            var moderatorIdentity = BuildModeratorIdentity(spaceIndex, questionIndex);
            var createdAtUtc = SeedBaseTimeUtc.AddDays(-(spaceIndex * 3 + questionIndex + 1));
            var answerActiveEventAtUtc = createdAtUtc.AddHours(2);
            var resolvedAtUtc = answerActiveEventAtUtc.AddHours(2);
            var questionId = Guid.NewGuid();

            var primaryAnswer = CreatePrimaryAnswer(
                tenantId,
                questionId,
                item,
                moderatedBy: moderatorIdentity.UserPrint,
                createdAtUtc);

            var question = new Question
            {
                Id = questionId,
                TenantId = tenantId,
                SpaceId = space.Id,
                Space = space,
                Title = item.Question,
                Summary = item.ShortAnswer,
                ContextNote = BuildQuestionContextNote(item),
                Status = QuestionStatus.Active,
                Visibility = VisibilityScope.Public,
                OriginChannel = ResolveOriginChannel(item, questionIndex),
                AiConfidenceScore = Math.Clamp(item.AiConfidenceScore, 0, 100),
                FeedbackScore = 0,
                Sort = questionIndex + 1,
                CreatedBy = "seed",
                UpdatedBy = "seed"
            };

            primaryAnswer.QuestionId = question.Id;
            primaryAnswer.Question = question;
            question.Answers.Add(primaryAnswer);

            var alternateAnswer = CreateAlternateAnswer(
                tenantId,
                question,
                item,
                questionIndex,
                moderatorIdentity.UserPrint);
            if (alternateAnswer is not null)
            {
                question.Answers.Add(alternateAnswer);
            }

            ApplyQuestionRelationships(question, definition, item, tagsByName, sourcesByUrl, tenantId);
            ApplyAnswerRelationships(primaryAnswer, item, sourcesByUrl, tenantId);
            if (alternateAnswer is not null)
            {
                ApplyAlternateAnswerRelationships(alternateAnswer, item, sourcesByUrl, tenantId);
            }

            QuestionRules.EnsureSupportedStatus(question.Status);
            AnswerRules.EnsureSupportedStatus(primaryAnswer.Status);
            if (alternateAnswer is not null)
            {
                AnswerRules.EnsureSupportedStatus(alternateAnswer.Status);
            }

            QuestionRules.EnsureVisibilityAllowed(question, question.Visibility);
            AnswerRules.EnsureVisibilityAllowed(primaryAnswer, primaryAnswer.Visibility);
            if (alternateAnswer is not null)
            {
                AnswerRules.EnsureVisibilityAllowed(alternateAnswer, alternateAnswer.Visibility);
            }

            var activities = BuildActivities(
                question,
                primaryAnswer,
                alternateAnswer,
                item,
                counts.SignalsPerQuestion,
                moderatorIdentity,
                createdAtUtc,
                answerActiveEventAtUtc,
                resolvedAtUtc,
                spaceIndex,
                questionIndex);

            question.LastActivityAtUtc = activities.Max(activity => activity.OccurredAtUtc);
            question.FeedbackScore = ActivitySignals.ComputeFeedbackScore(
                activities.Select(ActivityEntityMetadata.ToSignalEntry));
            dbContext.Questions.Add(question);
            acceptedAnswerAssignments.Add(new AcceptedAnswerAssignment(question, primaryAnswer));
        }
    }

    private static Answer CreatePrimaryAnswer(
        Guid tenantId,
        Guid questionId,
        SeedQuestionDefinition item,
        string moderatedBy,
        DateTime createdAtUtc)
    {
        return new Answer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuestionId = questionId,
            Headline = item.ShortAnswer,
            Body = BuildPrimaryAnswerBody(item),
            Kind = AnswerKind.Official,
            Status = AnswerStatus.Active,
            Visibility = VisibilityScope.Public,
            ContextNote = $"Curated and reviewed from {item.SourceName}. Primary guidance derived from {item.SourceLabel}.",
            AuthorLabel = moderatedBy,
            AiConfidenceScore = Math.Clamp(item.AiConfidenceScore, 0, 100),
            Score = Math.Max(1, item.HelpfulFeedbackPercent / 10),
            Sort = 1,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
    }

    private static Answer? CreateAlternateAnswer(
        Guid tenantId,
        Question question,
        SeedQuestionDefinition item,
        int questionIndex,
        string moderatedBy)
    {
        if (questionIndex % 4 != 0)
        {
            return null;
        }

        return new Answer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            Headline = $"Earlier escalation path for {item.SourceName}",
            Body = $"Legacy workflow retained for audit purposes before the active answer from {item.SourceName} replaced it.",
            Kind = AnswerKind.Imported,
            Status = AnswerStatus.Archived,
            Visibility = VisibilityScope.Internal,
            ContextNote = $"Retained for audit only. Superseded by the current operational answer based on {item.SourceLabel}.",
            AuthorLabel = moderatedBy,
            AiConfidenceScore = Math.Max(35, item.AiConfidenceScore - 25),
            Score = 1,
            Sort = 2,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
    }

    private static void ApplyQuestionRelationships(
        Question question,
        SeedSpaceDefinition definition,
        SeedQuestionDefinition item,
        IReadOnlyDictionary<string, Tag> tagsByName,
        IReadOnlyDictionary<string, Source> sourcesByUrl,
        Guid tenantId)
    {
        foreach (var tagName in definition.Tags
                     .Where(tagsByName.ContainsKey)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Take(2))
        {
            var tag = tagsByName[tagName];
            QuestionRules.EnsureTagLink(question, tag, tenantId, "seed");
        }

        if (!sourcesByUrl.TryGetValue(item.SourceUrl, out var source))
        {
            return;
        }

        var link = QuestionRules.CreateSourceLink(
            question,
            source,
            SourceRole.Origin,
            1,
            tenantId,
            "seed");

        question.Sources.Add(link);
        source.Questions.Add(link);
    }

    private static void ApplyAnswerRelationships(
        Answer answer,
        SeedQuestionDefinition item,
        IReadOnlyDictionary<string, Source> sourcesByUrl,
        Guid tenantId)
    {
        if (!sourcesByUrl.TryGetValue(item.SourceUrl, out var source))
        {
            return;
        }

        var link = AnswerRules.CreateSourceLink(
            answer,
            source,
            SourceRole.Evidence,
            1,
            tenantId,
            "seed");

        answer.Sources.Add(link);
        source.Answers.Add(link);
    }

    private static void ApplyAlternateAnswerRelationships(
        Answer answer,
        SeedQuestionDefinition item,
        IReadOnlyDictionary<string, Source> sourcesByUrl,
        Guid tenantId)
    {
        if (!sourcesByUrl.TryGetValue(item.SourceUrl, out var source))
        {
            return;
        }

        var link = AnswerRules.CreateSourceLink(
            answer,
            source,
            SourceRole.Context,
            1,
            tenantId,
            "seed");

        answer.Sources.Add(link);
        source.Answers.Add(link);
    }

    private static List<Activity> BuildActivities(
        Question question,
        Answer primaryAnswer,
        Answer? alternateAnswer,
        SeedQuestionDefinition item,
        int signalCount,
        SeedActorIdentity moderatorIdentity,
        DateTime createdAtUtc,
        DateTime answerActiveEventAtUtc,
        DateTime resolvedAtUtc,
        int spaceIndex,
        int questionIndex)
    {
        var activities = new List<Activity>();
        var moderator = CreateModeratorActor(moderatorIdentity);
        var emptySnapshot = new Dictionary<string, object?>(StringComparer.Ordinal);

        var questionActivity = ActivityAppender.AddQuestionActivity(
            question,
            ActivityKind.QuestionCreated,
            moderator,
            "Created",
            emptySnapshot,
            ActivityEntityMetadata.SnapshotQuestion(question),
            ActivityEntityMetadata.QuestionContext(question),
            createdAtUtc);
        if (questionActivity is not null)
        {
            activities.Add(questionActivity);
        }

        var answerActivity = ActivityAppender.AddAnswerActivity(
            primaryAnswer,
            ActivityKind.AnswerCreated,
            moderator,
            "Created",
            emptySnapshot,
            ActivityEntityMetadata.SnapshotAnswer(primaryAnswer),
            ActivityEntityMetadata.AnswerContext(primaryAnswer),
            answerActiveEventAtUtc);
        if (answerActivity is not null)
        {
            activities.Add(answerActivity);
        }

        if (alternateAnswer is not null)
        {
            var archiveActivity = ActivityAppender.AddAnswerActivity(
                alternateAnswer,
                ActivityKindStatusMap.ForAnswerStatus(alternateAnswer.Status),
                moderator,
                "StatusChanged",
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["Status"] = AnswerStatus.Active.ToString(),
                    ["Visibility"] = VisibilityScope.Public.ToString()
                },
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["Status"] = alternateAnswer.Status.ToString(),
                    ["Visibility"] = alternateAnswer.Visibility.ToString()
                },
                ActivityEntityMetadata.AnswerContext(alternateAnswer),
                resolvedAtUtc.AddHours(4));

            if (archiveActivity is not null)
            {
                activities.Add(archiveActivity);
            }
        }

        activities.AddRange(BuildFeedbackActivities(question, item, signalCount, resolvedAtUtc, spaceIndex, questionIndex));
        activities.AddRange(BuildVoteActivities(primaryAnswer, item, signalCount, resolvedAtUtc, spaceIndex, questionIndex));

        return activities;
    }

    private static IEnumerable<Activity> BuildFeedbackActivities(
        Question question,
        SeedQuestionDefinition item,
        int signalCount,
        DateTime startAtUtc,
        int spaceIndex,
        int questionIndex)
    {
        var safeSignalCount = Math.Max(0, signalCount);
        if (safeSignalCount == 0)
        {
            yield break;
        }

        var likeCount = (int)Math.Round(
            safeSignalCount * Math.Clamp(item.HelpfulFeedbackPercent, 0, 100) / 100d,
            MidpointRounding.AwayFromZero);
        likeCount = Math.Clamp(likeCount, 0, safeSignalCount);

        for (var signalIndex = 0; signalIndex < safeSignalCount; signalIndex++)
        {
            var like = signalIndex < likeCount;
            var identity = BuildCustomerIdentity("feedback", spaceIndex, questionIndex, signalIndex);
            yield return ActivityAppender.AddFeedbackActivity(
                question,
                CreateCustomerActor(identity),
                like,
                like ? null : FeedbackReasons[(spaceIndex + questionIndex + signalIndex) % FeedbackReasons.Length],
                like ? "Helpful outcome confirmed." : "Customer still needs clarification.",
                startAtUtc.AddHours(12).AddMinutes(signalIndex));
        }
    }

    private static IEnumerable<Activity> BuildVoteActivities(
        Answer answer,
        SeedQuestionDefinition item,
        int signalCount,
        DateTime startAtUtc,
        int spaceIndex,
        int questionIndex)
    {
        var safeSignalCount = Math.Max(0, signalCount);
        if (safeSignalCount == 0)
        {
            yield break;
        }

        var upvoteCount = (int)Math.Round(
            safeSignalCount * Math.Clamp(item.HelpfulFeedbackPercent, 0, 100) / 100d,
            MidpointRounding.AwayFromZero);
        upvoteCount = Math.Clamp(upvoteCount, 0, safeSignalCount);

        for (var signalIndex = 0; signalIndex < safeSignalCount; signalIndex++)
        {
            var voteValue = signalIndex < upvoteCount ? 1 : -1;
            var identity = BuildCustomerIdentity("vote", spaceIndex, questionIndex, signalIndex);
            yield return ActivityAppender.AddVoteActivity(
                answer,
                CreateCustomerActor(identity),
                voteValue,
                voteValue > 0 ? "Useful answer ranking signal." : "Ranking signal favored another answer.",
                startAtUtc.AddHours(24).AddMinutes(signalIndex));
        }
    }

    private static ActivityActor CreateModeratorActor(SeedActorIdentity identity)
    {
        return new ActivityActor(
            ActorKind.Moderator,
            identity.UserPrint,
            identity.Ip,
            identity.UserAgent,
            null,
            identity.UserPrint,
            false);
    }

    private static ActivityActor CreateCustomerActor(SeedActorIdentity identity)
    {
        return new ActivityActor(
            ActorKind.Customer,
            identity.UserPrint,
            identity.Ip,
            identity.UserAgent,
            null,
            null,
            true);
    }

    private static SeedActorIdentity BuildModeratorIdentity(int spaceIndex, int questionIndex)
    {
        return new SeedActorIdentity(
            UserPrint: $"seed-moderator-{spaceIndex + 1:00}-{questionIndex + 1:00}",
            Ip: $"198.51.100.{((spaceIndex * 17) + questionIndex) % 200 + 1}",
            UserAgent: "Querify.QnA.Seed/1.0");
    }

    private static SeedActorIdentity BuildCustomerIdentity(string prefix, int spaceIndex, int questionIndex, int signalIndex)
    {
        return new SeedActorIdentity(
            UserPrint: $"seed-{prefix}-{spaceIndex + 1:00}-{questionIndex + 1:00}-{signalIndex + 1:00}",
            Ip: $"203.0.113.{((spaceIndex * 31) + (questionIndex * 7) + signalIndex) % 200 + 1}",
            UserAgent: $"Querify.QnA.Seed.{prefix}/1.0");
    }

    private static string BuildSourceContextNote(SeedQuestionDefinition item)
    {
        var product = ResolveProductName(item);
        var businessArea = ResolveBusinessArea(item);
        var trustTier = ResolveTrustTier(item);

        return $"{trustTier} {product} reference for {businessArea}. Curated as reusable evidence for: {item.Question}";
    }

    private static string BuildSourceExternalId(SeedQuestionDefinition item)
    {
        var productKey = NormalizeKey(ResolveProductName(item), "product");
        var labelKey = NormalizeKey(item.SourceLabel, "source");
        return $"seed-qna:{productKey}:{labelKey}";
    }

    private static string BuildChecksum(SeedQuestionDefinition item)
    {
        var payload = string.Join('|', item.SourceName, item.SourceLabel, item.SourceUrl, item.Question, item.Answer);
        return Convert
            .ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(payload)))
            .ToLowerInvariant();
    }

    private static string BuildSourceMetadataJson(SeedQuestionDefinition item)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["catalog"] = "seed-qna",
            ["sourceName"] = item.SourceName,
            ["sourceLabel"] = item.SourceLabel,
            ["product"] = ResolveProductName(item),
            ["businessArea"] = ResolveBusinessArea(item),
            ["audience"] = ResolveAudience(item),
            ["trustTier"] = ResolveTrustTier(item),
            ["canonicalQuestion"] = item.Question,
            ["answerPreview"] = item.ShortAnswer,
            ["helpfulFeedbackPercent"] = Math.Clamp(item.HelpfulFeedbackPercent, 0, 100),
            ["aiConfidenceScore"] = Math.Clamp(item.AiConfidenceScore, 0, 100),
            ["sourceKind"] = ResolveSourceKind(item).ToString(),
            ["mediaType"] = ResolveMediaType(item),
            ["lastCatalogRefreshUtc"] = SeedBaseTimeUtc.ToString("O")
        };

        return JsonSerializer.Serialize(metadata);
    }

    private static DateTime BuildVerifiedAtUtc(SeedQuestionDefinition item)
    {
        return SeedBaseTimeUtc.AddDays(-(BuildStableOffset(item.SourceUrl, 21) + 1));
    }

    private static int BuildStableOffset(string value, int modulo)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return hash[0] % Math.Max(1, modulo);
    }

    private static string BuildQuestionContextNote(SeedQuestionDefinition item)
    {
        return $"Imported from {item.SourceName} for {ResolveBusinessArea(item)}. Snapshot refreshed on {SeedBaseTimeUtc:yyyy-MM-dd}; active answer confidence is {item.AiConfidenceScore}%.";
    }

    private static string BuildPrimaryAnswerBody(SeedQuestionDefinition item)
    {
        var product = ResolveProductName(item);
        var businessArea = ResolveBusinessArea(item).ToLowerInvariant();

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            item.Answer,
            $"Operational guidance: treat this as the canonical {product} answer for {businessArea} when the customer is asking the same policy or workflow question. Confirm account-specific state, eligibility, dates, regional availability, and any irreversible action before applying it.",
            $"Source handling: cite \"{item.SourceLabel}\" as the primary public reference. Re-check the source when feedback turns negative, confidence drops below the confidence threshold, or the upstream product flow changes.");
    }

    private static SourceKind ResolveSourceKind(SeedQuestionDefinition item)
    {
        var source = $"{item.SourceName} {item.SourceLabel} {item.SourceUrl}";

        if (ContainsAny(source, "terms", "policy", "policies", "real id", "identification", "signature services"))
        {
            return SourceKind.GovernanceRecord;
        }

        if (ContainsAny(source, "whatcanibring/items", "premium plans", "basic plans", "managed accounts", "package intercept"))
        {
            return SourceKind.ProductNote;
        }

        if (ContainsAny(source, "docs.github.com", "support.google.com", "support.apple.com", "support.spotify.com", "slack.com/help", "airbnb.com/help"))
        {
            return SourceKind.Article;
        }

        if (ContainsAny(source, "tsa.gov", "usps.com", "airbnb.com"))
        {
            return SourceKind.WebPage;
        }

        return SourceKind.Article;
    }

    private static string ResolveMediaType(SeedQuestionDefinition item)
    {
        return item.SourceUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "text/html";
    }

    private static string ResolveProductName(SeedQuestionDefinition item)
    {
        var sourceName = item.SourceName;

        if (sourceName.Contains("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            return "GitHub";
        }

        if (sourceName.Contains("Google", StringComparison.OrdinalIgnoreCase))
        {
            return "Google Account";
        }

        if (sourceName.Contains("Apple", StringComparison.OrdinalIgnoreCase))
        {
            return "Apple";
        }

        if (sourceName.Contains("Spotify", StringComparison.OrdinalIgnoreCase))
        {
            return "Spotify";
        }

        if (sourceName.Contains("Slack", StringComparison.OrdinalIgnoreCase))
        {
            return "Slack";
        }

        if (sourceName.Contains("TSA", StringComparison.OrdinalIgnoreCase))
        {
            return "TSA";
        }

        if (sourceName.Contains("USPS", StringComparison.OrdinalIgnoreCase))
        {
            return "USPS";
        }

        if (sourceName.Contains("Airbnb", StringComparison.OrdinalIgnoreCase))
        {
            return "Airbnb";
        }

        return sourceName;
    }

    private static string ResolveBusinessArea(SeedQuestionDefinition item)
    {
        var text = $"{item.Question} {item.SourceLabel}".ToLowerInvariant();

        if (ContainsAny(text, "2fa", "two-step", "password", "passkey", "recovery", "hacked", "suspicious", "secure"))
        {
            return "Account security";
        }

        if (ContainsAny(text, "subscription", "billing", "refund", "payment", "charge", "purchase", "premium", "plan", "cancel"))
        {
            return "Billing and subscriptions";
        }

        if (ContainsAny(text, "repository", "repo", "fork", "archive", "transfer", "visibility"))
        {
            return "Repository operations";
        }

        if (ContainsAny(text, "notification", "watch", "mention", "huddle", "canvas", "channel", "guest", "workspace", "playlist", "family", "duo"))
        {
            return "Collaboration and access";
        }

        if (ContainsAny(text, "icloud", "backup", "restore", "storage"))
        {
            return "Backup and storage";
        }

        if (ContainsAny(text, "tsa", "travel", "checkpoint", "real id", "laptop", "liquid", "medication", "battery", "precheck"))
        {
            return "Travel screening";
        }

        if (ContainsAny(text, "mail", "package", "forward", "redelivery", "po box", "signature", "hold mail"))
        {
            return "Mail and package operations";
        }

        if (ContainsAny(text, "airbnb", "reservation", "booking", "host", "monthly stay", "experience"))
        {
            return "Reservations and cancellations";
        }

        return "Customer support knowledge";
    }

    private static string ResolveAudience(SeedQuestionDefinition item)
    {
        var text = $"{item.Question} {item.Answer}".ToLowerInvariant();

        if (ContainsAny(text, "owner", "admin", "manager", "host", "plan manager"))
        {
            return "operators and account admins";
        }

        if (ContainsAny(text, "guest", "traveler", "passenger", "member", "customer", "users"))
        {
            return "customers and end users";
        }

        return "support agents";
    }

    private static string ResolveTrustTier(SeedQuestionDefinition item)
    {
        if (item.AiConfidenceScore >= 95 && item.HelpfulFeedbackPercent >= 92)
        {
            return "High-trust";
        }

        if (item.AiConfidenceScore >= 90)
        {
            return "Reviewed";
        }

        return "Monitor";
    }

    private static ChannelKind ResolveOriginChannel(SeedQuestionDefinition item, int questionIndex)
    {
        if (ContainsAny(item.SourceName, "Help", "Support", "Docs"))
        {
            return questionIndex % 5 == 0 ? ChannelKind.Import : ChannelKind.HelpCenter;
        }

        return questionIndex % 4 == 0 ? ChannelKind.Api : ChannelKind.Import;
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        return needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeKey(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasDash = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
                continue;
            }

            if (previousWasDash)
            {
                continue;
            }

            builder.Append('-');
            previousWasDash = true;
        }

        var normalized = builder
            .ToString()
            .Trim('-');

        return string.IsNullOrWhiteSpace(normalized)
            ? fallback
            : normalized;
    }

    private sealed record AcceptedAnswerAssignment(Question Question, Answer Answer);

    private readonly record struct SeedActorIdentity(string UserPrint, string Ip, string UserAgent);
}
