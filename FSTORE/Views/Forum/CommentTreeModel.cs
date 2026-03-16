using System.Collections.Generic;
using FSTORE.Models;
using FSTORE.Services;

namespace FSTORE.Forum
{
    public class CommentTreeModel
    {
        public List<Comment> Comments { get; set; } = new();
        public string PostId { get; set; } = string.Empty;
        public IUserService UserService { get; set; }
    }
}
