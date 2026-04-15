namespace BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

public static class VisibilityScopeExtensions
{
    public static bool IsPubliclyVisible(this VisibilityScope visibility) =>
        visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed;
}
