using System.Xml.Serialization;

namespace SalesOrderApp.Models
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int UserRoleId { get; set; }

        // Navigation properties
        [XmlIgnore]
        public UserRole UserRole { get; set; }
    }
}
