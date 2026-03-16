using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class UserService : IUserService
    {
        private readonly FirestoreDb _db;

        public UserService(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<string> GetUserRole(string email)
        {
            var doc = await _db.Collection("Users").Document(email).GetSnapshotAsync();
            if (!doc.Exists || !doc.ContainsField("role"))
                return "user"; 

            return doc.GetValue<string>("role");
        }

        public string GetRoleLabel(string role) => role switch
        {
            "admin" => "Quản trị viên",
            "staff" => "Nhân viên",
            _ => "Thành viên"
        };

        public string GetRoleColor(string role) => role switch
        {
            "admin" => "#d9534f",
            "staff" => "#0275d8",
            _ => "#5cb85c"
        };
    }
}
