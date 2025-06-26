using System.Collections.Generic;

namespace XMLSystem
{
    public class XMLElement
    {
        private Dictionary<string, string> _attributes = [];

        public string ElementName { get; private set; }
        public string[] AttributeNames => [.. _attributes.Keys];

        private XMLElement() { }

        public XMLElement(in string elementName) => ElementName = elementName;

        public bool HasAttribute(in string attributeName)
            => _attributes.ContainsKey(attributeName);

        public void AddAttribute(in string attributeName, in string attributeValue)
        {
            if (_attributes.ContainsKey(attributeName)) return;
            _attributes.Add(attributeName, attributeValue);
        }

        public string GetAttributeValue(in string attributeName)
        {
            if (!_attributes.TryGetValue(attributeName, out string value))
                return null;
            return value;
        }

        public override string ToString()
        {
            string result = $"<{ElementName}";
            foreach (KeyValuePair<string, string> entry in _attributes)
                result += $" {entry.Key}=" + '"' + entry.Value + '"';

            return result + '>';
        }
    }
}