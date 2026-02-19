using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Common.Providers.Abstractions;

public interface IAiProviderRuntimeContextResolver
{
    AiProviderRuntimeContext Resolve(AiProviderContext providerContext, AiCommandType commandType);
}