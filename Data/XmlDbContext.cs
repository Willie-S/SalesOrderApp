using SalesOrderApp.Models;
using SalesOrderApp.Utilities;
using System.Xml.Linq;

namespace SalesOrderApp.Data
{
    public class XmlDbContext
    {
        private readonly string _usersFilePath;
        private readonly string _userRolesFilePath;

        public XmlDbContext(string usersFilePath, string userRolesFilePath)
        {
            _usersFilePath = usersFilePath;
            _userRolesFilePath = userRolesFilePath;

            SeedData();
        }

        private void SeedData()
        {
            XDocument doc = XDocument.Load(_userRolesFilePath);
            XElement userRoles = doc.Element("UserRoles");

            // Get existing UserRole IDs
            var existingIds = doc.Descendants("UserRole")
                                 .Select(x => (int)x.Element("Id"))
                                 .ToHashSet();

            foreach (UserRoleEnum role in Enum.GetValues(typeof(UserRoleEnum)))
            {
                // Skip existing roles
                if (existingIds.Contains((int)role))
                    continue;

                // Add new role
                userRoles.Add(new XElement("UserRole",
                    new XElement("Id", (int)role),
                    new XElement("Name", role.ToString())
                ));
            }

            // Save changes to XML file
            doc.Save(_userRolesFilePath);
        }

        public async Task AddUserAsync(User user)
        {
            XDocument doc = XDocument.Load(_usersFilePath);
            XElement usersElement = doc.Element("Users");

            if (usersElement == null)
            {
                usersElement = new XElement("Users");
                doc.Add(usersElement);
            }

            int maxId = doc.Descendants("User").Select(x => (int?)x.Element("Id")).Max() ?? 0;
            int newId = maxId++;

            usersElement.Add(new XElement("User",
                new XElement("Id", newId),
                new XElement("FirstName", GeneralHelper.NormaliseString(user.FirstName)),
                new XElement("LastName", GeneralHelper.NormaliseString(user.LastName)),
                new XElement("Email", GeneralHelper.NormaliseStringForEmail(user.Email)),
                new XElement("PasswordHash", user.PasswordHash),
                new XElement("UserRoleId", user.UserRoleId),
                new XElement("DateCreated", DateTime.Now),
                new XElement("DateUpdated", DateTime.Now)
            ));

            doc.Save(_usersFilePath);
            user.Id = newId;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            XDocument doc = XDocument.Load(_usersFilePath);
            XElement userElement = doc.Descendants("User").FirstOrDefault(x => (string)x.Element("Email") == email);
            if (userElement == null) return null;

            return new User
            {
                Id = (int)userElement.Element("Id"),
                FirstName = (string)userElement.Element("FirstName"),
                LastName = (string)userElement.Element("LastName"),
                Email = (string)userElement.Element("Email"),
                PasswordHash = (string)userElement.Element("PasswordHash"),
                UserRoleId = (int)userElement.Element("UserRoleId"),
                DateCreated = (DateTime)userElement.Element("DateCreated"),
                DateUpdated = (DateTime)userElement.Element("DateUpdated")
            };
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            XDocument doc = XDocument.Load(_usersFilePath);
            return doc.Descendants("User").Any(x => GeneralHelper.CompareStrings((string)x.Element("Email"), email));
        }
    }
}
