namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface IClaimService
{
    string? GetName();

    string? GetEmail();

    string? GetExternalUserId();
}