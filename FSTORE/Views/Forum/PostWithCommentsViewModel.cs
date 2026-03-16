using System.Collections.Generic;
using FSTORE.Models;

namespace FSTORE.Forum
{
    public class PostWithCommentsViewModel
    {
        public Post Post { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public string RoleLabel { get; set; } = string.Empty;
        public string RoleColor { get; set; } = "#000000";
        public string RoleName { get; set; } = "user";
    }

    public class CommentFormModel
    {
        public string PostId { get; set; } = string.Empty;
        public string? ParentId { get; set; }
    }
}
