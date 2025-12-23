namespace Common.DTOs.Template.Requests;

public class CreateLevel3ReviewRequest
{
    public int UserId { get; set; }

    public string? TemplateName { get; set; }

    public string? Description { get; set; }

    public int? Level1ReviewerId { get; set; }

    public int? Level2ReviewerId { get; set; }
}
