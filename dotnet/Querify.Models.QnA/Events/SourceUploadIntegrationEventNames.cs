namespace Querify.Models.QnA.Events;

public static class SourceUploadIntegrationEventNames
{
    public const string CompletedExchangeName = "qna.source-upload.completed.v1";
    public const string CompletedQueueName = "qna.source-upload.completed.worker.v1";
    public const string StatusChangedExchangeName = "qna.source-upload.status-changed.v1";
    public const string StatusChangedQueueName = "qna.source-upload.status-changed.portal.v1";
}
