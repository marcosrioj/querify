namespace Querify.Common.Infrastructure.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class SkipTenantAccessValidationAttribute : Attribute
{
}