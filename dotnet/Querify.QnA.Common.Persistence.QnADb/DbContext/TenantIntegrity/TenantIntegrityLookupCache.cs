using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal sealed class TenantIntegrityLookupCache(QnADbContext dbContext)
{
    private readonly TenantIntegrityLookupCacheBase _tenantLookup = new(dbContext);
    private Dictionary<Guid, AnswerTenantIntegrityLookup>? _answers;
    private Dictionary<Guid, QuestionTenantIntegrityLookup>? _questions;
    private Dictionary<Guid, Guid>? _sourceTenants;
    private Dictionary<Guid, Guid>? _spaceTenants;
    private Dictionary<Guid, Guid>? _tagTenants;

    internal Guid GetSpaceTenant(Guid id)
    {
        return _tenantLookup.GetTenant<Space>(id, ref _spaceTenants);
    }

    internal Guid GetQuestionTenant(Guid id)
    {
        return GetQuestion(id).TenantId;
    }

    internal QuestionTenantIntegrityLookup GetQuestion(Guid id)
    {
        _questions ??= SeedQuestionCache();

        if (_questions.TryGetValue(id, out var cached)) return cached;

        var databaseLookup = dbContext.Questions
            .IgnoreQueryFilters()
            .Where(question => question.Id == id)
            .Select(question => new QuestionTenantIntegrityLookup
            {
                TenantId = question.TenantId,
                ParentAnswerId = question.ParentAnswerId
            })
            .SingleOrDefault();

        if (databaseLookup is null)
            throw new ApiErrorException(
                $"Referenced {nameof(Question)} '{id}' was not found.",
                (int)HttpStatusCode.NotFound);

        _questions[id] = databaseLookup;
        return databaseLookup;
    }

    internal AnswerTenantIntegrityLookup GetAnswer(Guid id)
    {
        _answers ??= SeedAnswerCache();

        if (_answers.TryGetValue(id, out var cached)) return cached;

        var databaseLookup = dbContext.Answers
            .IgnoreQueryFilters()
            .Where(answer => answer.Id == id)
            .Select(answer => new AnswerTenantIntegrityLookup
            {
                TenantId = answer.TenantId,
                QuestionId = answer.QuestionId,
                Status = answer.Status,
                Visibility = answer.Visibility
            })
            .SingleOrDefault();

        if (databaseLookup is null)
            throw new ApiErrorException(
                $"Referenced {nameof(Answer)} '{id}' was not found.",
                (int)HttpStatusCode.NotFound);

        _answers[id] = databaseLookup;
        return databaseLookup;
    }

    internal Guid GetSourceTenant(Guid id)
    {
        return _tenantLookup.GetTenant<Source>(id, ref _sourceTenants);
    }

    internal Guid GetTagTenant(Guid id)
    {
        return _tenantLookup.GetTenant<Tag>(id, ref _tagTenants);
    }

    private Dictionary<Guid, AnswerTenantIntegrityLookup> SeedAnswerCache()
    {
        var cache = new Dictionary<Guid, AnswerTenantIntegrityLookup>();

        foreach (var entry in dbContext.ChangeTracker.Entries<Answer>()
                     .Where(entry => entry.State != EntityState.Deleted))
            cache[entry.Entity.Id] = new AnswerTenantIntegrityLookup
            {
                TenantId = entry.Entity.TenantId,
                QuestionId = entry.Entity.QuestionId,
                Status = entry.Entity.Status,
                Visibility = entry.Entity.Visibility
            };

        return cache;
    }

    private Dictionary<Guid, QuestionTenantIntegrityLookup> SeedQuestionCache()
    {
        var cache = new Dictionary<Guid, QuestionTenantIntegrityLookup>();

        foreach (var entry in dbContext.ChangeTracker.Entries<Question>()
                     .Where(entry => entry.State != EntityState.Deleted))
            cache[entry.Entity.Id] = new QuestionTenantIntegrityLookup
            {
                TenantId = entry.Entity.TenantId,
                ParentAnswerId = entry.Entity.ParentAnswerId
            };

        return cache;
    }
}

internal sealed class AnswerTenantIntegrityLookup
{
    public required Guid TenantId { get; init; }
    public required Guid QuestionId { get; init; }
    public required AnswerStatus Status { get; init; }
    public required VisibilityScope Visibility { get; init; }
}

internal sealed class QuestionTenantIntegrityLookup
{
    public required Guid TenantId { get; init; }
    public Guid? ParentAnswerId { get; init; }
}
