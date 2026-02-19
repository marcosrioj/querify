namespace BaseFaq.AI.Business.Generation.Models;

public sealed record GenerationPromptData(
    string Domain,
    string Version,
    string Provider,
    string Template,
    string Input,
    string OutputSchema);