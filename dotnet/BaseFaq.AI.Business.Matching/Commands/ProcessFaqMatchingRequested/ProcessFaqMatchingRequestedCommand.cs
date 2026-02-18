using BaseFaq.Models.Ai.Contracts.Matching;
using MediatR;

namespace BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;

public sealed record ProcessFaqMatchingRequestedCommand(
    FaqMatchingRequestedV1 Message,
    string HandlerName,
    string MessageId) : IRequest;