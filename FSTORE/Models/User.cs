using Google.Cloud.Firestore;

namespace FSTORE.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string uid { get; set; }

        [FirestoreProperty]
        public string email { get; set; }

        [FirestoreProperty]
        public string name { get; set; }

        [FirestoreProperty]
        public string role { get; set; }

        [FirestoreProperty]
        public string address { get; set; }

        [FirestoreProperty]
        public string phone { get; set; }

        [FirestoreProperty]
        public string imageUrl { get; set; }
    }
}
