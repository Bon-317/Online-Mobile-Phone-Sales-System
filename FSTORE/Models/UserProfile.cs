using Google.Cloud.Firestore;

namespace FSTORE.Models
{
    [FirestoreData]
    public class UserProfile
    {
        [FirestoreProperty("uid")] public string Uid { get; set; }
        [FirestoreProperty("name")] public string Name { get; set; }
        [FirestoreProperty("email")] public string Email { get; set; }
        [FirestoreProperty("address")] public string Address { get; set; }
        [FirestoreProperty("phone")] public string Phone { get; set; }
        [FirestoreProperty("imageUrl")] public string ImageUrl { get; set; }
        [FirestoreProperty("role")] public string Role { get; set; }
    }
}