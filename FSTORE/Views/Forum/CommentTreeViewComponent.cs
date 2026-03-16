using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using FSTORE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Forum
{
    public class CommentTreeViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        public CommentTreeViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<Comment> comments, string postId)
        {
            var model = new CommentTreeModel
            {
                Comments = comments,
                PostId = postId,
                UserService = _userService
            };

            return View(model);
        }
    }
}
