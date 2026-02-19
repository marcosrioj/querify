namespace BaseFaq.AI.Business.Generation.Models;

public sealed record GenerationPromptData(
    string Template,
    string Input,
    string OutputSchema);