using System.Text;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;

namespace BaseFaq.Tools.Seed.Application;

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

            SeedSpaceRelationships(dbContext, space, definition, tagsByName, sourcesByUrl, counts, tenantId);
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
            assignment.Question.AcceptedAnswerId = assignment.Answer.Id;
            assignment.Question.AcceptedAnswer = assignment.Answer;
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
        return spaceDefinitions
            .SelectMany(definition => definition.Items)
            .DistinctBy(item => item.SourceUrl, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(0, maxSourceCount))
            .Select(item => new Source
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Kind = ResolveSourceKind(item.SourceName),
                Locator = item.SourceUrl,
                Label = item.SourceLabel,
                ContextNote = $"Curated evidence for {item.Question}",
                ExternalId = NormalizeKey($"{item.SourceName}-{item.Question}", "seed-source"),
                Language = SeedLanguage,
                MediaType = "text/html",
                Checksum = Convert.ToHexString(Guid.NewGuid().ToByteArray()),
                MetadataJson =
                    $$"""{"catalog":"seed-qna","sourceName":"{{EscapeJson(item.SourceName)}}","sourceLabel":"{{EscapeJson(item.SourceLabel)}}"}""",
                Visibility = VisibilityScope.PublicIndexed,
                AllowsPublicCitation = true,
                AllowsPublicExcerpt = true,
                IsAuthoritative = true,
                CapturedAtUtc = SeedBaseTimeUtc.AddDays(-14),
                LastVerifiedAtUtc = SeedBaseTimeUtc.AddDays(-1),
                CreatedBy = "seed",
                UpdatedBy = "seed"
            })
            .ToList();
    }

    private static Space CreateSpace(SeedSpaceDefinition definition, Guid tenantId, int index)
    {
        return new Space
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = definition.Name,
            Slug = NormalizeKey(definition.Name, $"space-{index + 1}"),
            Summary = BuildSpaceSummary(definition),
            Language = SeedLanguage,
            Kind = SpaceKind.ModeratedCollaboration,
            Visibility = VisibilityScope.PublicIndexed,
            AcceptsQuestions = true,
            AcceptsAnswers = true,
            PublishedAtUtc = SeedBaseTimeUtc.AddDays(-(index + 30)),
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
    }

    private static string BuildSpaceSummary(SeedSpaceDefinition definition)
    {
        var topics = string.Join(", ", definition.Tags.Take(3));
        return $"Operational QnA space for {definition.Name} covering {definition.Items.Count} canonical questions and curated sources on {topics}.";
    }

    private static void SeedSpaceRelationships(
        QnADbContext dbContext,
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
            dbContext.SpaceTags.Add(new SpaceTag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SpaceId = space.Id,
                Space = space,
                TagId = tag.Id,
                Tag = tag,
                CreatedBy = "seed",
                UpdatedBy = "seed"
            });
        }

        foreach (var sourceUrl in definition.Items
                     .Select(item => item.SourceUrl)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Where(sourcesByUrl.ContainsKey)
                     .Take(Math.Max(0, counts.SourcesPerSpace)))
        {
            var source = sourcesByUrl[sourceUrl];
            dbContext.SpaceSources.Add(new SpaceSource
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SpaceId = space.Id,
                Space = space,
                SourceId = source.Id,
                Source = source,
                CreatedBy = "seed",
                UpdatedBy = "seed"
            });
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
        for (var questionIndex = 0; questionIndex < definition.Items.Count; questionIndex++)
        {
            var item = definition.Items[questionIndex];
            var moderatorIdentity = BuildModeratorIdentity(spaceIndex, questionIndex);
            var createdAtUtc = SeedBaseTimeUtc.AddDays(-(spaceIndex * 3 + questionIndex + 1));
            var publishedAtUtc = createdAtUtc.AddHours(2);
            var resolvedAtUtc = publishedAtUtc.AddHours(2);
            var isValidated = item.AiConfidenceScore >= 90;
            var validatedAtUtc = isValidated ? resolvedAtUtc.AddHours(3) : (DateTime?)null;
            var questionId = Guid.NewGuid();

            var primaryAnswer = CreatePrimaryAnswer(
                tenantId,
                questionId,
                item,
                moderatedBy: moderatorIdentity.UserPrint,
                createdAtUtc,
                publishedAtUtc,
                resolvedAtUtc,
                validatedAtUtc);

            var question = new Question
            {
                Id = questionId,
                TenantId = tenantId,
                SpaceId = space.Id,
                Space = space,
                Title = item.Question,
                Summary = item.ShortAnswer,
                ContextNote = $"Imported from {item.SourceName}. Snapshot refreshed on {SeedBaseTimeUtc:yyyy-MM-dd}.",
                Status = isValidated ? QuestionStatus.Validated : QuestionStatus.Answered,
                Visibility = VisibilityScope.PublicIndexed,
                OriginChannel = ChannelKind.Import,
                AiConfidenceScore = Math.Clamp(item.AiConfidenceScore, 0, 100),
                FeedbackScore = 0,
                Sort = questionIndex + 1,
                AnsweredAtUtc = resolvedAtUtc,
                ValidatedAtUtc = validatedAtUtc,
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
                publishedAtUtc,
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

            var activities = BuildActivities(
                question,
                primaryAnswer,
                alternateAnswer,
                item,
                counts.SignalsPerQuestion,
                moderatorIdentity,
                createdAtUtc,
                publishedAtUtc,
                resolvedAtUtc,
                validatedAtUtc,
                spaceIndex,
                questionIndex);

            foreach (var activity in activities)
            {
                question.Activities.Add(activity);
            }

            question.LastActivityAtUtc = activities.Max(activity => activity.OccurredAtUtc);
            question.FeedbackScore = ActivitySignals.ComputeFeedbackScore(activities.Select(ToSignalEntry));
            dbContext.Questions.Add(question);
            acceptedAnswerAssignments.Add(new AcceptedAnswerAssignment(question, primaryAnswer));
        }
    }

    private static Answer CreatePrimaryAnswer(
        Guid tenantId,
        Guid questionId,
        SeedQuestionDefinition item,
        string moderatedBy,
        DateTime createdAtUtc,
        DateTime publishedAtUtc,
        DateTime resolvedAtUtc,
        DateTime? validatedAtUtc)
    {
        return new Answer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuestionId = questionId,
            Headline = item.ShortAnswer,
            Body = item.Answer,
            Kind = AnswerKind.Official,
            Status = validatedAtUtc is not null ? AnswerStatus.Validated : AnswerStatus.Published,
            Visibility = VisibilityScope.PublicIndexed,
            ContextNote = $"Curated and reviewed from {item.SourceName}. Primary guidance derived from {item.SourceLabel}.",
            AuthorLabel = moderatedBy,
            AiConfidenceScore = Math.Clamp(item.AiConfidenceScore, 0, 100),
            Score = Math.Max(1, item.HelpfulFeedbackPercent / 10),
            Sort = 1,
            PublishedAtUtc = publishedAtUtc,
            ValidatedAtUtc = validatedAtUtc,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        };
    }

    private static Answer? CreateAlternateAnswer(
        Guid tenantId,
        Question question,
        SeedQuestionDefinition item,
        int questionIndex,
        DateTime publishedAtUtc,
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
            Body = $"Legacy workflow retained for audit purposes before the validated answer from {item.SourceName} replaced it.",
            Kind = AnswerKind.Imported,
            Status = AnswerStatus.Archived,
            Visibility = VisibilityScope.Internal,
            ContextNote = $"Retained for audit only. Superseded by the current operational answer based on {item.SourceLabel}.",
            AuthorLabel = moderatedBy,
            AiConfidenceScore = Math.Max(35, item.AiConfidenceScore - 25),
            Score = 1,
            Sort = 2,
            PublishedAtUtc = publishedAtUtc.AddMinutes(-30),
            RetiredAtUtc = publishedAtUtc.AddHours(8),
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
            question.Tags.Add(new QuestionTag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                QuestionId = question.Id,
                Question = question,
                TagId = tag.Id,
                Tag = tag,
                CreatedBy = "seed",
                UpdatedBy = "seed"
            });
        }

        if (!sourcesByUrl.TryGetValue(item.SourceUrl, out var source))
        {
            return;
        }

        question.Sources.Add(new QuestionSourceLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            SourceId = source.Id,
            Source = source,
            Role = SourceRole.QuestionOrigin,
            Order = 1,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        });
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

        answer.Sources.Add(new AnswerSourceLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AnswerId = answer.Id,
            Answer = answer,
            SourceId = source.Id,
            Source = source,
            Role = SourceRole.CanonicalReference,
            Order = 1,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        });

        answer.Sources.Add(new AnswerSourceLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AnswerId = answer.Id,
            Answer = answer,
            SourceId = source.Id,
            Source = source,
            Role = SourceRole.Citation,
            Order = 2,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        });
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

        answer.Sources.Add(new AnswerSourceLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AnswerId = answer.Id,
            Answer = answer,
            SourceId = source.Id,
            Source = source,
            Role = SourceRole.SupportingContext,
            Order = 1,
            CreatedBy = "seed",
            UpdatedBy = "seed"
        });
    }

    private static List<Activity> BuildActivities(
        Question question,
        Answer primaryAnswer,
        Answer? alternateAnswer,
        SeedQuestionDefinition item,
        int signalCount,
        SeedActorIdentity moderatorIdentity,
        DateTime createdAtUtc,
        DateTime publishedAtUtc,
        DateTime resolvedAtUtc,
        DateTime? validatedAtUtc,
        int spaceIndex,
        int questionIndex)
    {
        var activities = new List<Activity>
        {
            CreateInternalActivity(
                question,
                null,
                ActivityKind.QuestionCreated,
                moderatorIdentity,
                createdAtUtc),
            CreateInternalActivity(
                question,
                null,
                ActivityKind.QuestionSubmitted,
                moderatorIdentity,
                createdAtUtc.AddMinutes(10)),
            CreateInternalActivity(
                question,
                null,
                ActivityKind.QuestionApproved,
                moderatorIdentity,
                createdAtUtc.AddMinutes(25)),
            CreateInternalActivity(
                question,
                primaryAnswer,
                ActivityKind.AnswerCreated,
                moderatorIdentity,
                createdAtUtc.AddMinutes(40)),
            CreateInternalActivity(
                question,
                primaryAnswer,
                ActivityKind.AnswerPublished,
                moderatorIdentity,
                publishedAtUtc),
            CreateInternalActivity(
                question,
                primaryAnswer,
                ActivityKind.AnswerAccepted,
                moderatorIdentity,
                resolvedAtUtc)
        };

        if (validatedAtUtc is not null)
        {
            activities.Add(
                CreateInternalActivity(
                    question,
                    primaryAnswer,
                    ActivityKind.AnswerValidated,
                    moderatorIdentity,
                    validatedAtUtc.Value));
        }

        if (alternateAnswer is not null)
        {
            activities.Add(
                CreateInternalActivity(
                    question,
                    alternateAnswer,
                    ActivityKind.AnswerCreated,
                    moderatorIdentity,
                    createdAtUtc.AddMinutes(45)));
            activities.Add(
                CreateInternalActivity(
                    question,
                    alternateAnswer,
                    ActivityKind.AnswerRetired,
                    moderatorIdentity,
                    alternateAnswer.RetiredAtUtc ?? resolvedAtUtc.AddHours(4),
                    notes: "Retired after the validated canonical answer was published."));
        }

        activities.AddRange(BuildFeedbackActivities(question, item, signalCount, resolvedAtUtc, spaceIndex, questionIndex));
        activities.AddRange(BuildVoteActivities(question, primaryAnswer, item, signalCount, resolvedAtUtc, spaceIndex, questionIndex));

        if (item.HelpfulFeedbackPercent < 75)
        {
            activities.Add(BuildReportActivity(question, primaryAnswer, spaceIndex, questionIndex, resolvedAtUtc.AddHours(18)));
        }

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
            yield return new Activity
            {
                Id = Guid.NewGuid(),
                TenantId = question.TenantId,
                QuestionId = question.Id,
                Question = question,
                Kind = ActivityKind.FeedbackReceived,
                ActorKind = ActorKind.Customer,
                ActorLabel = identity.UserPrint,
                UserPrint = identity.UserPrint,
                Ip = identity.Ip,
                UserAgent = identity.UserAgent,
                Notes = like ? "Helpful outcome confirmed." : "Customer still needs clarification.",
                MetadataJson = ActivitySignals.CreateFeedbackMetadata(
                    identity.UserPrint,
                    identity.Ip,
                    identity.UserAgent,
                    like,
                    like ? null : FeedbackReasons[(spaceIndex + questionIndex + signalIndex) % FeedbackReasons.Length]),
                OccurredAtUtc = startAtUtc.AddHours(12).AddMinutes(signalIndex),
                CreatedBy = identity.UserPrint,
                UpdatedBy = identity.UserPrint
            };
        }
    }

    private static IEnumerable<Activity> BuildVoteActivities(
        Question question,
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
            yield return new Activity
            {
                Id = Guid.NewGuid(),
                TenantId = question.TenantId,
                QuestionId = question.Id,
                Question = question,
                AnswerId = answer.Id,
                Answer = answer,
                Kind = ActivityKind.VoteReceived,
                ActorKind = ActorKind.Customer,
                ActorLabel = identity.UserPrint,
                UserPrint = identity.UserPrint,
                Ip = identity.Ip,
                UserAgent = identity.UserAgent,
                Notes = voteValue > 0 ? "Useful answer ranking signal." : "Ranking signal favored another answer.",
                MetadataJson = ActivitySignals.CreateVoteMetadata(
                    identity.UserPrint,
                    identity.Ip,
                    identity.UserAgent,
                    voteValue),
                OccurredAtUtc = startAtUtc.AddHours(24).AddMinutes(signalIndex),
                CreatedBy = identity.UserPrint,
                UpdatedBy = identity.UserPrint
            };
        }
    }

    private static Activity BuildReportActivity(
        Question question,
        Answer answer,
        int spaceIndex,
        int questionIndex,
        DateTime occurredAtUtc)
    {
        var identity = BuildCustomerIdentity("report", spaceIndex, questionIndex, 0);
        return new Activity
        {
            Id = Guid.NewGuid(),
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKind.ReportReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            Notes = "Seeded moderation signal for a public thread.",
            MetadataJson = ActivitySignals.CreateReportMetadata(
                identity.UserPrint,
                identity.Ip,
                identity.UserAgent,
                "Needs moderator review after a policy change."),
            OccurredAtUtc = occurredAtUtc,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };
    }

    private static Activity CreateInternalActivity(
        Question question,
        Answer? answer,
        ActivityKind kind,
        SeedActorIdentity identity,
        DateTime occurredAtUtc,
        string? notes = null)
    {
        return new Activity
        {
            Id = Guid.NewGuid(),
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer?.Id,
            Answer = answer,
            Kind = kind,
            ActorKind = ActorKind.Moderator,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            Notes = notes,
            OccurredAtUtc = occurredAtUtc,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };
    }

    private static SeedActorIdentity BuildModeratorIdentity(int spaceIndex, int questionIndex)
    {
        return new SeedActorIdentity(
            UserPrint: $"seed-moderator-{spaceIndex + 1:00}-{questionIndex + 1:00}",
            Ip: $"198.51.100.{((spaceIndex * 17) + questionIndex) % 200 + 1}",
            UserAgent: "BaseFaq.QnA.Seed/1.0");
    }

    private static SeedActorIdentity BuildCustomerIdentity(string prefix, int spaceIndex, int questionIndex, int signalIndex)
    {
        return new SeedActorIdentity(
            UserPrint: $"seed-{prefix}-{spaceIndex + 1:00}-{questionIndex + 1:00}-{signalIndex + 1:00}",
            Ip: $"203.0.113.{((spaceIndex * 31) + (questionIndex * 7) + signalIndex) % 200 + 1}",
            UserAgent: $"BaseFaq.QnA.Seed.{prefix}/1.0");
    }

    private static SourceKind ResolveSourceKind(string sourceName)
    {
        if (sourceName.Contains("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            return SourceKind.Repository;
        }

        if (sourceName.Contains("Slack", StringComparison.OrdinalIgnoreCase))
        {
            return SourceKind.Article;
        }

        if (sourceName.Contains("USPS", StringComparison.OrdinalIgnoreCase) ||
            sourceName.Contains("TSA", StringComparison.OrdinalIgnoreCase))
        {
            return SourceKind.ProductNote;
        }

        return SourceKind.Article;
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

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static ActivitySignalEntry ToSignalEntry(Activity activity)
    {
        return new ActivitySignalEntry(
            activity.Kind,
            activity.AnswerId,
            activity.OccurredAtUtc,
            activity.UserPrint,
            activity.MetadataJson);
    }

    private sealed record AcceptedAnswerAssignment(Question Question, Answer Answer);

    private readonly record struct SeedActorIdentity(string UserPrint, string Ip, string UserAgent);
}
