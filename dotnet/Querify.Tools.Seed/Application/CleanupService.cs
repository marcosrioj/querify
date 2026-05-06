using Querify.Common.EntityFramework.Tenant;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.Tools.Seed.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tools.Seed.Application;

public sealed class CleanupService : ICleanupService
{
    public void CleanTenantDb(TenantDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"BillingWebhookInboxes\", \"EmailOutboxes\", \"TenantConnections\", \"TenantUsers\", \"Tenants\", \"Users\" RESTART IDENTITY CASCADE;");
    }

    public void CleanQnADb(QnADbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"Activities\", \"AnswerSourceLinks\", \"QuestionSourceLinks\", \"QuestionTags\", \"SpaceSources\", \"SpaceTags\", \"Answers\", \"Questions\", \"Sources\", \"Tags\", \"Spaces\" RESTART IDENTITY CASCADE;");
    }

    public void CleanBigDataQnADb(QnADbContext dbContext)
    {
        dbContext.Database.ExecuteSqlInterpolated($"""
            UPDATE "Questions"
            SET "AcceptedAnswerId" = NULL
            WHERE "CreatedBy" = {SeedMarkers.BigData};

            DELETE FROM "Activities"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "QuestionId" IN (
                    SELECT "Id"
                    FROM "Questions"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "AnswerSourceLinks"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "AnswerId" IN (
                    SELECT "Id"
                    FROM "Answers"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "QuestionSourceLinks"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "QuestionId" IN (
                    SELECT "Id"
                    FROM "Questions"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "QuestionTags"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "QuestionId" IN (
                    SELECT "Id"
                    FROM "Questions"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "Answers"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "QuestionId" IN (
                    SELECT "Id"
                    FROM "Questions"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "Questions"
            WHERE "CreatedBy" = {SeedMarkers.BigData};

            DELETE FROM "SpaceSources"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "SpaceId" IN (
                    SELECT "Id"
                    FROM "Spaces"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "SpaceTags"
            WHERE "CreatedBy" = {SeedMarkers.BigData}
               OR "SpaceId" IN (
                    SELECT "Id"
                    FROM "Spaces"
                    WHERE "CreatedBy" = {SeedMarkers.BigData}
               );

            DELETE FROM "Sources"
            WHERE "CreatedBy" = {SeedMarkers.BigData};

            DELETE FROM "Tags"
            WHERE "CreatedBy" = {SeedMarkers.BigData};

            DELETE FROM "Spaces"
            WHERE "CreatedBy" = {SeedMarkers.BigData};
            """);
    }
}
