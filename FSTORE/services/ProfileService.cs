using Google.Cloud.Firestore;
using FSTORE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class ProfileService
    {
        private readonly FirestoreDb _firestore;

        public ProfileService(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        // ✅ Lấy thông tin người dùng
        public async Task<UserProfile> GetProfileAsync(string uid)
        {
            var docRef = _firestore.Collection("Users").Document(uid);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<UserProfile>() : null;
        }

        // ✅ Cập nhật thông tin người dùng (không ghi đè null)
        public async Task<bool> UpdateProfileAsync(string uid, UserProfile model)
        {
            try
            {
                var docRef = _firestore.Collection("Users").Document(uid);
                var updates = new Dictionary<string, object>
                {
                    { "name", model.Name ?? string.Empty },
                    { "email", model.Email ?? string.Empty },
                    { "phone", model.Phone ?? string.Empty },
                    { "address", model.Address ?? string.Empty },
                    { "imageUrl", model.ImageUrl ?? string.Empty },
                    { "role", model.Role ?? "user" }
                };

                await docRef.UpdateAsync(updates);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
