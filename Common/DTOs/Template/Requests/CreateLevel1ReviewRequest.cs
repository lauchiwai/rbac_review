namespace Common.DTOs.Template.Requests;

public class CreateLevel1ReviewRequest
{
    public int UserId { get; set; }

    public string? TemplateName { get; set; }

    public string? Description { get; set; }
}
