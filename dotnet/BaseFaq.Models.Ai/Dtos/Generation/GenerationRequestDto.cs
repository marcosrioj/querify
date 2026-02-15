namespace BaseFaq.Models.Ai.Dtos.Generation;

public sealed record GenerationRequestDto(
    Guid FaqId,
    Guid TenantId,
    string Language,
    string? IdempotencyKey,
    Guid? RequestedByUserId);