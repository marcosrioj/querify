using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.Tools.Seed.Abstractions;
using Querify.Tools.Seed.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tools.Seed.Application;

public sealed class BigDataSeedService : IBigDataSeedService
{
    private const string SeedLanguage = "en-US";
    private static readonly DateTime SeedNow = new(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);

    public bool HasData(QnADbContext dbContext)
    {
        return dbContext.Spaces.IgnoreQueryFilters().Any(space => space.CreatedBy == SeedMarkers.BigData) ||
               dbContext.Questions.IgnoreQueryFilters().Any(question => question.CreatedBy == SeedMarkers.BigData) ||
               dbContext.Activities.IgnoreQueryFilters().Any(activity => activity.CreatedBy == SeedMarkers.BigData);
    }

    public void Seed(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        SeedSpaces(dbContext, tenantId, settings);
        SeedTags(dbContext, tenantId, settings);
        SeedSources(dbContext, tenantId, settings);
        SeedSpaceTags(dbContext, tenantId, settings);
        SeedSpaceSources(dbContext, tenantId, settings);
        SeedQuestions(dbContext, tenantId, settings);
        SeedAnswers(dbContext, tenantId, settings);
        AssignAcceptedAnswers(dbContext, settings);
        SeedQuestionTags(dbContext, tenantId, settings);
        SeedQuestionSourceLinks(dbContext, tenantId, settings);
        SeedAnswerSourceLinks(dbContext, tenantId, settings);
        SeedActivities(dbContext, tenantId, settings);
    }

    private static void SeedSpaces(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Spaces" (
                "Id",
                "Name",
                "Slug",
                "Summary",
                "Language",
                "Status",
                "Visibility",
                "AcceptsQuestions",
                "AcceptsAnswers",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('10000000-0000-0000-0000-' || lpad(to_hex(space_index::bigint), 12, '0'))::uuid,
                'Seed Big Data Space ' || space_index,
                'seed-big-data-space-' || space_index,
                'Synthetic performance space ' || space_index,
                {SeedLanguage},
                {(int)SpaceStatus.Active},
                {(int)VisibilityScope.Public},
                true,
                true,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedTags(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Tags" (
                "Id",
                "Name",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('11000000-0000-0000-0000-' || lpad(to_hex(tag_index::bigint), 12, '0'))::uuid,
                'seed-big-data-tag-' || tag_index,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.TagCount}) AS tags(tag_index)
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedSources(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Sources" (
                "Id",
                "Kind",
                "Locator",
                "Label",
                "ContextNote",
                "ExternalId",
                "Language",
                "MediaType",
                "Checksum",
                "MetadataJson",
                "Visibility",
                "LastVerifiedAtUtc",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('12000000-0000-0000-0000-' || lpad(to_hex(source_index::bigint), 12, '0'))::uuid,
                {(int)SourceKind.InternalNote},
                'seed-big-data://source/' || source_index,
                'Seed Big Data Source ' || source_index,
                'Synthetic source for performance data.',
                'seed-big-data-source-' || source_index,
                {SeedLanguage},
                'application/json',
                md5('seed-big-data-source-' || source_index),
                json_build_object('seed', 'big-data', 'kind', 'source')::text,
                {(int)VisibilityScope.Public},
                {SeedNow},
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SourceCount}) AS sources(source_index)
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedSpaceTags(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "SpaceTags" (
                "Id",
                "SpaceId",
                "TagId",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('13000000-0000-0000-0000-' || lpad(to_hex(tag_ordinal), 12, '0'))::uuid,
                ('10000000-0000-0000-0000-' || lpad(to_hex(space_index::bigint), 12, '0'))::uuid,
                ('11000000-0000-0000-0000-' || lpad(to_hex(tag_ordinal), 12, '0'))::uuid,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.TagsPerSpace}) AS tags(tag_slot)
            CROSS JOIN LATERAL (
                SELECT ((space_index - 1) * {settings.TagsPerSpace} + tag_slot)::bigint AS tag_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedSpaceSources(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "SpaceSources" (
                "Id",
                "SpaceId",
                "SourceId",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('14000000-0000-0000-0000-' || lpad(to_hex(source_ordinal), 12, '0'))::uuid,
                ('10000000-0000-0000-0000-' || lpad(to_hex(space_index::bigint), 12, '0'))::uuid,
                ('12000000-0000-0000-0000-' || lpad(to_hex(source_ordinal), 12, '0'))::uuid,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.SourcesPerSpace}) AS sources(source_slot)
            CROSS JOIN LATERAL (
                SELECT ((space_index - 1) * {settings.SourcesPerSpace} + source_slot)::bigint AS source_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedQuestions(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Questions" (
                "Id",
                "Title",
                "Summary",
                "ContextNote",
                "Status",
                "Visibility",
                "OriginChannel",
                "AiConfidenceScore",
                "FeedbackScore",
                "Sort",
                "SpaceId",
                "AcceptedAnswerId",
                "LastActivityAtUtc",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                'Seed Big Data Question ' || question_ordinal,
                'Synthetic question ' || question_ordinal,
                'Synthetic performance question for space ' || space_index,
                {(int)QuestionStatus.Active},
                {(int)VisibilityScope.Public},
                {(int)ChannelKind.Widget},
                (75 + (question_ordinal % 20))::integer,
                ((question_ordinal % 11) - 5)::integer,
                question_index::integer,
                ('10000000-0000-0000-0000-' || lpad(to_hex(space_index::bigint), 12, '0'))::uuid,
                NULL,
                {SeedNow} - ((question_ordinal % 365)::integer * interval '1 day'),
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN LATERAL (
                SELECT ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedAnswers(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Answers" (
                "Id",
                "Headline",
                "Body",
                "Kind",
                "Status",
                "Visibility",
                "ContextNote",
                "AuthorLabel",
                "AiConfidenceScore",
                "Score",
                "Sort",
                "QuestionId",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('16000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                'Seed Big Data Answer ' || question_ordinal,
                'Synthetic answer body for performance question ' || question_ordinal || '.',
                {(int)AnswerKind.Imported},
                {(int)AnswerStatus.Active},
                {(int)VisibilityScope.Public},
                'Synthetic performance answer.',
                'seed-big-data',
                (75 + (question_ordinal % 20))::integer,
                (1 + (question_ordinal % 50))::integer,
                1,
                ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN LATERAL (
                SELECT ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void AssignAcceptedAnswers(QnADbContext dbContext, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            UPDATE "Questions" AS question
            SET
                "AcceptedAnswerId" = question_seed.answer_id,
                "UpdatedDate" = {SeedNow},
                "UpdatedBy" = {SeedMarkers.BigData}
            FROM (
                SELECT
                    ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid AS question_id,
                    ('16000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid AS answer_id
                FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
                CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
                CROSS JOIN LATERAL (
                    SELECT ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal
                ) ordinals
            ) AS question_seed
            WHERE question."Id" = question_seed.question_id
              AND question."CreatedBy" = {SeedMarkers.BigData};
            """);
    }

    private static void SeedQuestionTags(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "QuestionTags" (
                "Id",
                "QuestionId",
                "TagId",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('17000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('11000000-0000-0000-0000-' || lpad(to_hex(tag_ordinal), 12, '0'))::uuid,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN LATERAL (
                SELECT
                    ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal,
                    ((space_index - 1) * {settings.TagsPerSpace} + ((question_index - 1) % {settings.TagsPerSpace}) + 1)::bigint AS tag_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedQuestionSourceLinks(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "QuestionSourceLinks" (
                "Id",
                "QuestionId",
                "SourceId",
                "Role",
                "Order",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('18000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('12000000-0000-0000-0000-' || lpad(to_hex(source_ordinal), 12, '0'))::uuid,
                {(int)SourceRole.Origin},
                1,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN LATERAL (
                SELECT
                    ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal,
                    ((space_index - 1) * {settings.SourcesPerSpace} + ((question_index - 1) % {settings.SourcesPerSpace}) + 1)::bigint AS source_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedAnswerSourceLinks(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "AnswerSourceLinks" (
                "Id",
                "AnswerId",
                "SourceId",
                "Role",
                "Order",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('19000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('16000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                ('12000000-0000-0000-0000-' || lpad(to_hex(source_ordinal), 12, '0'))::uuid,
                {(int)SourceRole.Evidence},
                1,
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN LATERAL (
                SELECT
                    ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal,
                    ((space_index - 1) * {settings.SourcesPerSpace} + ((question_index - 1) % {settings.SourcesPerSpace}) + 1)::bigint AS source_ordinal
            ) ordinals
            ON CONFLICT DO NOTHING;
            """);
    }

    private static void SeedActivities(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Activities" (
                "Id",
                "QuestionId",
                "AnswerId",
                "Kind",
                "ActorKind",
                "ActorLabel",
                "UserPrint",
                "Ip",
                "UserAgent",
                "Notes",
                "MetadataJson",
                "OccurredAtUtc",
                "TenantId",
                "CreatedDate",
                "CreatedBy",
                "UpdatedDate",
                "UpdatedBy",
                "IsDeleted")
            SELECT
                ('1a000000-0000-0000-0000-' || lpad(to_hex(activity_ordinal), 12, '0'))::uuid,
                ('15000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid,
                CASE
                    WHEN activity_index % 2 = 0
                        THEN ('16000000-0000-0000-0000-' || lpad(to_hex(question_ordinal), 12, '0'))::uuid
                    ELSE NULL
                END,
                CASE
                    WHEN activity_index % 2 = 0 THEN {(int)ActivityKind.VoteReceived}
                    ELSE {(int)ActivityKind.FeedbackReceived}
                END,
                {(int)ActorKind.Customer},
                'seed-big-data-user-' || ((question_ordinal + activity_index) % 1000000),
                'seed-big-data-user-' || ((question_ordinal + activity_index) % 1000000),
                concat(
                    '10.',
                    ((question_ordinal % 250) + 1),
                    '.',
                    ((activity_index % 250) + 1),
                    '.',
                    (((question_ordinal + activity_index) % 250) + 1)),
                'Querify.QnA.Seed.BigData/1.0',
                'Synthetic interaction.',
                json_build_object(
                    'seed',
                    'big-data',
                    'value',
                    CASE WHEN activity_index % 3 = 0 THEN -1 ELSE 1 END)::text,
                {SeedNow} - ((question_ordinal % 365)::integer * interval '1 day') + ((activity_index % 24)::integer * interval '1 hour'),
                {tenantId},
                {SeedNow},
                {SeedMarkers.BigData},
                {SeedNow},
                {SeedMarkers.BigData},
                false
            FROM generate_series(1, {settings.SpaceCount}) AS spaces(space_index)
            CROSS JOIN generate_series(1, {settings.QuestionsPerSpace}) AS questions(question_index)
            CROSS JOIN generate_series(1, {settings.ActivitiesPerQuestion}) AS activities(activity_index)
            CROSS JOIN LATERAL (
                SELECT
                    ((space_index - 1) * {settings.QuestionsPerSpace} + question_index)::bigint AS question_ordinal
            ) question_ordinals
            CROSS JOIN LATERAL (
                SELECT ((question_ordinal - 1) * {settings.ActivitiesPerQuestion} + activity_index)::bigint AS activity_ordinal
            ) activity_ordinals
            ON CONFLICT DO NOTHING;
            """);
    }
}
