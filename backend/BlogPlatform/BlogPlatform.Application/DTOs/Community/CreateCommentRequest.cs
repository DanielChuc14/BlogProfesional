namespace BlogPlatform.Application.DTOs.Community;

public class CreateCommentRequest
{
    public Guid? ParentId { get; set; }
    public string Body { get; set; } = string.Empty;
}
