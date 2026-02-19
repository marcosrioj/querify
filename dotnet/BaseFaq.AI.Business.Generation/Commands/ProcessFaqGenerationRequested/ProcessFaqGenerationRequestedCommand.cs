using BaseFaq.Models.Ai.Contracts.Generation;
using MediatR;

namespace BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;

public sealed record ProcessFaqGenerationRequestedCommand(
    FaqGenerationRequestedV1 Message) : IRequest;
