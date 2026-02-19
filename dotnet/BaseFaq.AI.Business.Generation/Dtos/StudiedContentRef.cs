using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.AI.Business.Generation.Dtos;

public sealed record StudiedContentRef(ContentRefKind Kind, string Locator, string MainSubject);