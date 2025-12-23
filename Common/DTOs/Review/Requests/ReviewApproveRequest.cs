namespace Common.DTOs.Review.Requests;

public class ReviewApproveRequest : ReviewActionRequest
{
    public int? NextReviewerId { get; set; }
}