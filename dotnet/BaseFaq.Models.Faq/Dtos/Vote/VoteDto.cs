namespace BaseFaq.Models.Faq.Dtos.Vote;

public class VoteDto
{
    public required Guid Id { get; set; }
    public required string UserPrint { get; set; }
    public required string Ip { get; set; }
    public required string UserAgent { get; set; }
    public required Guid FaqItemAnswerId { get; set; }
}
