namespace Querify.Models.QnA.Dtos.Question;

public class QuestionGetRequestDto
{
    public bool IncludeAnswers { get; set; } = true;
    public bool IncludeTags { get; set; } = true;
    public bool IncludeSources { get; set; } = true;
    public bool IncludeActivity { get; set; }
}
