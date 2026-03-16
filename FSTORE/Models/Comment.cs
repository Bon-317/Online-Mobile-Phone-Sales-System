using Google.Cloud.Firestore;

namespace FSTORE.Models
{
    [FirestoreData]
    public class Comment
    {
        public string Id { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreProperty]
        public string AuthorEmail { get; set; }

        [FirestoreProperty]
        public string ParentId { get; set; }

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        public List<Comment> Children { get; set; } = new();
        [FirestoreProperty]
        public string PostId { get; set; }

    }
}
