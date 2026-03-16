using System.Collections.Generic;
using System.Threading.Tasks;
using FSTORE.Models;

namespace FSTORE.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetAllPostsAsync();
        Task<Post?> GetPostByIdAsync(string id);
        Task AddPostAsync(Post post);
        Task DeletePostAsync(string postId);

    }
}
