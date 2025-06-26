using System.Collections.Generic;

namespace XMLSystem
{
    public class XMLNode
    {
        private readonly XMLElement _baseElement;
        private HashSet<XMLNode> _subNodes = [];

        public string ElementName => _baseElement.ElementName;
        public XMLNode Parent { get; set; } = null;
        public long ChildCount => _subNodes.Count;

        private XMLNode() { }

        public XMLNode(in XMLElement baseElement) => _baseElement = baseElement;

        public bool HasSubNode(in string elementName)
        {
            foreach (XMLNode node in _subNodes)
                if (node.ElementName == elementName) return true;
            return false;
        }

        public void AddNode(in XMLNode node)
            => _subNodes.Add(node);

        public void AddAttribute(in string name, in string value)
        {
            if (_baseElement.HasAttribute(name)) return;
            _baseElement.AddAttribute(in name, in value);
        }

        public string GetAttributeValue(in string attributeName)
            => _baseElement.GetAttributeValue(in attributeName);

        public XMLNode GetNode(in string subNode)
        {
            foreach (XMLNode node in _subNodes)
                if (node.ElementName == subNode) return node;
            return null;
        }

        public XMLNode GetNode(in string[] path)
        {
            if (path.Length == 0) return this;
            return GetNode(in path, 0);
        }

        public XMLNode[] GetChildNodes() => [.. _subNodes];

        public override string ToString() => _baseElement.ToString();

        private XMLNode GetNode(in string[] path, int depth)
        {
            //* Went to the bottom and found nothing
            if (depth >= path.Length) return null;

            foreach (XMLNode node in _subNodes)
                if (node.ElementName == path[depth])
                    return GetNode(in path, depth + 1);

            //* Doesn't exists
            return null;
        }

    }
}