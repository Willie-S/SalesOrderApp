namespace SalesOrderApp.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum UserRoleEnum
    {
        Admin = 1,
        Guest
    }
}
