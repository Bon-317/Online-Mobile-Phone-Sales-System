using Google.Cloud.Firestore;
using FSTORE.Models;

namespace FSTORE.Services
{
    public class CommentService : ICommentService
    {
        private readonly FirestoreDb _db;

        public CommentService(FirestoreDb db)
        {
            _db = db;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            var docRef = _db.Collection("Comments").Document();
            comment.Id = docRef.Id;
            await docRef.SetAsync(comment);
        }

        public async Task DeleteCommentAsync(string commentId)
        {
            await _db.Collection("Comments").Document(commentId).DeleteAsync();
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
        {
            var snapshot = await _db.Collection("Comments")
                .WhereEqualTo("PostId", postId)
                .OrderBy("CreatedAt")
                .GetSnapshotAsync();

            var all = snapshot.Documents.Select(doc =>
            {
                var cmt = doc.ConvertTo<Comment>();
                cmt.Id = doc.Id;
                return cmt;
            }).ToList();

            var dict = all.ToDictionary(c => c.Id);
            foreach (var cmt in all)
            {
                if (!string.IsNullOrEmpty(cmt.ParentId) && dict.ContainsKey(cmt.ParentId))
                {
                    dict[cmt.ParentId].Children ??= new List<Comment>();
                    dict[cmt.ParentId].Children.Add(cmt);
                }
            }

            return all.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();
        }
    }
}
