using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.AI.Business.Generation.Models;

public sealed record StudiedContentRef(ContentRefKind Kind, string Locator, string MainSubject);