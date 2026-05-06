namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface IUserIdProvider
{
    Guid GetUserId();
}