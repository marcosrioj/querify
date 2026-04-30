using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Activities;

public readonly record struct ActivitySignalEntry(
    ActivityKind Kind,
    Guid? AnswerId,
    DateTime OccurredAtUtc,
    string? UserPrint,
    string? MetadataJson);