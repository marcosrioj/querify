namespace BaseFaq.AI.Business.Generation.Dtos;

public sealed record GeneratedFaqDraft(
    string Question,
    string Summary,
    string Answer,
    int Confidence);