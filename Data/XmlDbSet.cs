using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SalesOrderApp.Data
{
    public class XmlDbSet<T> where T : class, new()
    {
        private readonly string _filePath;

        public XmlDbSet(string filePath)
        {
            _filePath = filePath;
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (!File.Exists(_filePath))
            {
                var doc = new XDocument(new XElement(typeof(T).Name + "s"));
                doc.Save(_filePath);
            }
        }

        private XDocument LoadXml()
        {
            return XDocument.Load(_filePath);
        }

        private void SaveXml(XDocument doc)
        {
            doc.Save(_filePath);
        }

        public List<T> GetAll()
        {
            XDocument doc = LoadXml();
            return doc.Descendants(typeof(T).Name).Select(x => Deserialize(x)).ToList();
        }

        public T GetById(int id)
        {
            XDocument doc = LoadXml();
            var element = doc.Descendants(typeof(T).Name).FirstOrDefault(x => (int)x.Element("Id") == id);
            return element != null ? Deserialize(element) : null;
        }

        public void Add(T entity, bool autoIncrementId = true)
        {
            XDocument doc = LoadXml();
            XElement root = doc.Element(typeof(T).Name + "s");

            if (autoIncrementId)
            {
                // Auto-increment the Id
                int newId = (root.Elements(typeof(T).Name).Select(x => (int?)x.Element("Id")).Max() ?? 0) + 1;
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(entity, newId);
                }
            }

            root.Add(Serialize(entity));
            SaveXml(doc);
        }

        public void Update(T entity)
        {
            XDocument doc = LoadXml();
            var idProperty = typeof(T).GetProperty("Id");
            var id = (int)idProperty.GetValue(entity);
            var element = doc.Descendants(typeof(T).Name).FirstOrDefault(x => (int)x.Element("Id") == id);
            if (element != null)
            {
                element.ReplaceWith(Serialize(entity));
                SaveXml(doc);
            }
        }

        public void Delete(T entity)
        {
            XDocument doc = LoadXml();
            var idProperty = typeof(T).GetProperty("Id");
            var id = (int)idProperty.GetValue(entity);
            var element = doc.Descendants(typeof(T).Name).FirstOrDefault(x => (int)x.Element("Id") == id);
            if (element != null)
            {
                element.Remove();
                SaveXml(doc);
            }
        }

        private T Deserialize(XElement element)
        {
            var entity = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                var propValue = Convert.ChangeType(element.Element(prop.Name)?.Value, prop.PropertyType, CultureInfo.InvariantCulture);
                prop.SetValue(entity, propValue);
            }
            return entity;
        }

        private XElement Serialize(T entity)
        {
            var element = new XElement(typeof(T).Name);
            foreach (var prop in typeof(T).GetProperties())
            {
                // Skip properties that has the XmlIgnore attribute
                if (prop.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
                element.Add(new XElement(prop.Name, prop.GetValue(entity)));
            }
            return element;
        }
    }
}
