namespace BaseFaq.AI.Business.Generation.Service;

public sealed record GeneratedFaqDraft(
    string Question,
    string Summary,
    string Answer,
    int Confidence,
    GenerationPromptData PromptData);

public sealed record GenerationPromptData(
    string Domain,
    string Version,
    string Provider,
    string Template,
    string Input,
    string OutputSchema);
