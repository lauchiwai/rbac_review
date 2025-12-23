namespace Common.DTOs.Template.Requests;

public class CreateLevel2ReviewRequest
{
    public int UserId { get; set; }

    public string? TemplateName { get; set; }

    public string? Description { get; set; }

    public int? Level1ReviewerId { get; set; }
}
