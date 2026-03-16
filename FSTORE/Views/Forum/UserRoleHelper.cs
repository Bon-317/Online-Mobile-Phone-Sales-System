using System.Threading.Tasks;
using FSTORE.Services;

namespace FSTORE.Forum
{
    public static class UserRoleHelper
    {
        /// <summary>
        /// Trả về nhãn và màu sắc của vai trò người dùng dựa trên email.
        /// </summary>
        public static async Task<(string Label, string Color)> GetRoleInfoAsync(IUserService userService, string email)
        {
            var role = await userService.GetUserRole(email);
            var label = userService.GetRoleLabel(role);
            var color = userService.GetRoleColor(role);

            return (label, color);
        }
    }
}
