using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Abstractions;

public interface IAiProviderProfileRegistry
{
    AiProviderProfile Resolve(string provider);
}