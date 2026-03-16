using Google.Cloud.Firestore;
using FSTORE.Models;
using FSTORE.Services;

public class PostService : IPostService
{
    private readonly FirestoreDb _db;

    public PostService(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        var snapshot = await _db.Collection("Posts").OrderByDescending("CreatedAt").GetSnapshotAsync();
        var posts = new List<Post>();

        foreach (var doc in snapshot.Documents)
        {
            var post = doc.ConvertTo<Post>();
            post.Id = doc.Id;
            posts.Add(post);
        }

        return posts;
    }

    public async Task<Post?> GetPostByIdAsync(string id)
    {
        var doc = await _db.Collection("Posts").Document(id).GetSnapshotAsync();
        if (!doc.Exists) return null;

        var post = doc.ConvertTo<Post>();
        post.Id = doc.Id;
        return post;
    }

    public async Task AddPostAsync(Post post)
    {
        var docRef = _db.Collection("Posts").Document();
        post.Id = docRef.Id;
        await docRef.SetAsync(post);
    }

    public async Task DeletePostAsync(string postId)
    {
        await _db.Collection("Posts").Document(postId).DeleteAsync();
    }

}
