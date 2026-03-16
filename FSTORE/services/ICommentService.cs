using System.Collections.Generic;
using System.Threading.Tasks;
using FSTORE.Models;

namespace FSTORE.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsByPostIdAsync(string postId);
        Task AddCommentAsync(Comment comment);
        Task DeleteCommentAsync(string commentId);

    }
}
