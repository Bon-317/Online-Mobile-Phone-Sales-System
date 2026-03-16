using System;
using System.ComponentModel.DataAnnotations;

namespace FSTORE.Validation
{
    // Lớp này kế thừa từ ValidationAttribute để tạo ra một luật mới
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            // Gán thông báo lỗi mặc định
            ErrorMessage = "Ngày hết hạn phải muộn hơn thời gian hiện tại.";
        }

        // Đây là hàm kiểm tra logic
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // 'value' chính là giá trị từ trường ExpiryDateTime
            if (value is DateTime dateTimeValue)
            {
                // So sánh với thời gian (local) hiện tại
                if (dateTimeValue > DateTime.Now)
                {
                    // Hợp lệ (ngày ở tương lai)
                    return ValidationResult.Success;
                }
            }

            // Không hợp lệ (ngày ở quá khứ hoặc hiện tại)
            return new ValidationResult(ErrorMessage);
        }
    }
}