using SalesOrderApp.Models;
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
    }
}
