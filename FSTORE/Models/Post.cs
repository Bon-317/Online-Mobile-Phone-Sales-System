using Google.Cloud.Firestore;

namespace FSTORE.Models
{
    [FirestoreData]
    public class Post
    {
        public string Id { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreProperty]
        public string AuthorEmail { get; set; }

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        public List<Comment> Comments { get; set; } = new();
    }
}