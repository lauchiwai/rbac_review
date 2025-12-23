namespace Common.DTOs.Review.Requests;

public class ReviewActionRequest
{
    public int UserId { get; set; }

    public int TodoId { get; set; }

    public string? Comment { get; set; }
}