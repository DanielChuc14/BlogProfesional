using BlogPlatform.Domain.Entities.Auth;

namespace BlogPlatform.Domain.Entities.Content;

public class Comment : Common.BaseEntity
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentId { get; set; }
    public string Body { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Post Post { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
    public ICollection<CommentLike> CommentLikes { get; set; } = [];
}
