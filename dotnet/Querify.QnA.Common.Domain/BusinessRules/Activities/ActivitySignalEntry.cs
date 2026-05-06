using Querify.Models.QnA.Enums;

namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public readonly record struct ActivitySignalEntry(
    ActivityKind Kind,
    Guid? AnswerId,
    DateTime OccurredAtUtc,
    string? UserPrint,
    string? MetadataJson);