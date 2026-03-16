using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using FSTORE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using FSTORE.Forum;

namespace FSTORE.Controllers
{
    public class ForumController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public ForumController(IPostService postService, ICommentService commentService, IUserService userService)
        {
            _postService = postService;
            _commentService = commentService;
            _userService = userService;
        }

        // Hiển thị danh sách bài viết
        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();
            var viewModels = new List<PostWithCommentsViewModel>();

            foreach (var post in posts)
            {
                var role = await _userService.GetUserRole(post.AuthorEmail);
                var label = _userService.GetRoleLabel(role);
                var color = _userService.GetRoleColor(role);

                var comments = await _commentService.GetCommentsByPostIdAsync(post.Id);
                var commentTree = BuildCommentTree(comments);

                viewModels.Add(new PostWithCommentsViewModel
                {
                    Post = post,
                    Comments = commentTree,
                    RoleLabel = label,
                    RoleColor = color,
                    RoleName = role
                });
            }

            ViewBag.UserService = _userService;
            return View(viewModels); // ✅ Truyền đúng kiểu
        }


        // Hiển thị form đăng bài
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý đăng bài
        [HttpPost]
        public async Task<IActionResult> Create(Post model)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Content))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return View(model);
            }

            model.AuthorEmail = User.Identity?.Name ?? "anonymous";
            model.CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow);

            await _postService.AddPostAsync(model);
            TempData["Success"] = "✅ Đã đăng bài thành công.";
            return RedirectToAction("Index");
        }

        // Hiển thị chi tiết bài viết + bình luận
        public async Task<IActionResult> Detail(string id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            post.Comments = await _commentService.GetCommentsByPostIdAsync(id);
            return View(post);
        }

        // Thêm bình luận
        [HttpPost]
        public async Task<IActionResult> AddComment(string postId, string content, string? parentId)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Detail", new { id = postId });

            var comment = new Comment
            {
                PostId = postId,
                ParentId = parentId,
                Content = content,
                AuthorEmail = User.Identity?.Name ?? "Ẩn danh",
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await _commentService.AddCommentAsync(comment);
            return RedirectToAction("Detail", new { id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(string id)
        {
            var role = await _userService.GetUserRole(User.Identity?.Name ?? "");
            if (role != "admin") return Unauthorized();

            await _postService.DeletePostAsync(id);
            TempData["Success"] = "🗑️ Bài viết đã được xóa.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(string id, string postId)
        {
            var role = await _userService.GetUserRole(User.Identity?.Name ?? "");
            if (role != "admin") return Unauthorized();

            await _commentService.DeleteCommentAsync(id);
            TempData["Success"] = "🗑️ Bình luận đã được xóa.";
            return RedirectToAction("Detail", new { id = postId });
        }


        // Xây dựng cây bình luận
        private List<Comment> BuildCommentTree(List<Comment> flatComments)
        {
            var lookup = flatComments.ToDictionary(c => c.Id);
            var roots = new List<Comment>();

            foreach (var comment in flatComments)
            {
                if (string.IsNullOrEmpty(comment.ParentId))
                {
                    roots.Add(comment);
                }
                else if (lookup.ContainsKey(comment.ParentId))
                {
                    var parent = lookup[comment.ParentId];
                    parent.Children ??= new List<Comment>();
                    parent.Children.Add(comment);
                }
            }

            return roots;
        }
    }
}
