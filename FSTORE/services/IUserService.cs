namespace FSTORE.Services
{
    public interface IUserService
    {
        Task<string> GetUserRole(string email);
        string GetRoleLabel(string role);
        string GetRoleColor(string role);
    }
}
