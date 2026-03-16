using Google.Cloud.Firestore;
using FSTORE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class VoucherService
    {
        private readonly FirestoreDb _db;
        private readonly CollectionReference _voucherCollection;

        public VoucherService(FirestoreDb db)
        {
            _db = db;
            _voucherCollection = _db.Collection("vouchers");
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            var snapshot = await _voucherCollection.GetSnapshotAsync();
            var list = new List<Voucher>();

            foreach (var doc in snapshot.Documents)
            {
                var v = doc.ConvertTo<Voucher>();
                v.DocumentId = doc.Id;

                // Chuyển Timestamp → DateTime Local để hiển thị
                if (v.expiryDate != null)
                {
                    v.ExpiryDateTime = v.expiryDate.ToDateTime().ToLocalTime();
                }

                list.Add(v);
            }
            return list;
        }

        public async Task CreateVoucherAsync(Voucher voucher)
        {
            voucher.code = voucher.code.Trim().ToUpper();

            // ÉP CHUẨN UTC CHO FIRESTORE CŨ
            var utcTime = DateTime.SpecifyKind(voucher.ExpiryDateTime, DateTimeKind.Local).ToUniversalTime();
            voucher.expiryDate = Timestamp.FromDateTime(utcTime);

            // Dùng code làm DocumentId → dễ tìm, dễ xóa
            await _voucherCollection.Document(voucher.code).SetAsync(voucher);
        }

        public async Task<Voucher?> GetVoucherByIdAsync(string documentId)
        {
            var snapshot = await _voucherCollection.Document(documentId).GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            var v = snapshot.ConvertTo<Voucher>();
            v.DocumentId = snapshot.Id;

            if (v.expiryDate != null)
            {
                v.ExpiryDateTime = v.expiryDate.ToDateTime().ToLocalTime();
            }
            return v;
        }

        public async Task UpdateVoucherAsync(Voucher voucher)
        {
            if (string.IsNullOrEmpty(voucher.DocumentId))
                throw new InvalidOperationException("DocumentId không được null");

            voucher.code = voucher.code.Trim().ToUpper();

            var utcTime = DateTime.SpecifyKind(voucher.ExpiryDateTime, DateTimeKind.Local).ToUniversalTime();
            voucher.expiryDate = Timestamp.FromDateTime(utcTime);

            await _voucherCollection.Document(voucher.DocumentId).SetAsync(voucher, SetOptions.MergeAll);
        }

        public async Task DeleteVoucherAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
                await _voucherCollection.Document(id).DeleteAsync();
        }

        public async Task ToggleVoucherStatusAsync(string id)
        {
            var snap = await _voucherCollection.Document(id).GetSnapshotAsync();
            if (snap.Exists)
            {
                bool current = snap.ContainsField("isActive") && snap.GetValue<bool>("isActive");
                await _voucherCollection.Document(id).UpdateAsync("isActive", !current);
            }
        }

        // DÙNG CHO GIỎ HÀNG – TÌM VOUCHER THEO CODE
        public async Task<Voucher?> GetValidVoucherAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            code = code.Trim().ToUpper();
            var snap = await _voucherCollection.Document(code).GetSnapshotAsync();

            if (!snap.Exists) return null;

            var v = snap.ConvertTo<Voucher>();
            v.DocumentId = snap.Id;

            if (!v.isActive) return null;
            if (v.expiryDate != null && v.expiryDate.ToDateTime() < DateTime.UtcNow) return null;

            return v;
        }
    }
}