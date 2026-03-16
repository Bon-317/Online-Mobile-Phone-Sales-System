using Google.Cloud.Firestore;
using FSTORE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class OrderHistoryService
    {
        private readonly FirestoreDb _firestore;

        public OrderHistoryService(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        public async Task<List<OrderModel>> GetPaidOrdersByUserAsync(string uid)
        {
            var query = _firestore.Collection("Orders")
                                  .WhereEqualTo("Uid", uid)
                                  .WhereEqualTo("PaymentStatus", "Success");

            var snapshot = await query.GetSnapshotAsync();
            var orders = new List<OrderModel>();

            foreach (var doc in snapshot.Documents)
            {
                if (doc.Exists)
                    orders.Add(doc.ConvertTo<OrderModel>());
            }

            return orders.OrderByDescending(o => o.CreatedAt).ToList();
        }
    }
}
