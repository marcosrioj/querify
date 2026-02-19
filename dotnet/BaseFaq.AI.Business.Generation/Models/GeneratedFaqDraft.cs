namespace BaseFaq.AI.Business.Generation.Models;

public sealed record GeneratedFaqDraft(
    string Question,
    string Summary,
    string Answer,
    int Confidence);