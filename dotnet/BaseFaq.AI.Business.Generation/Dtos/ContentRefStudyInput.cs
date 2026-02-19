using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.AI.Business.Generation.Dtos;

public sealed record ContentRefStudyInput(ContentRefKind Kind, string Locator);