using System;
using System.IO;
using System.Xml;

namespace XMLSystem
{
    public static class XMLReader
    {
        public enum ErrorFlag
        {
            OK,
            INVALID_FILE_PATH,
            INVALID_STRUCTURE,
        }

        public readonly struct ParseResults(in XMLNode root, ErrorFlag error)
        {
            public readonly XMLNode root = root;
            public readonly ErrorFlag errorFlag = error;
        }

        public static ParseResults ReadFile(in string filePath)
        {
            if (!File.Exists(filePath)) return new(null, ErrorFlag.INVALID_FILE_PATH);

            XMLNode rootNode;
            try
            { rootNode = ParseXMLFile(in filePath); }
            catch
            { return new(null, ErrorFlag.INVALID_STRUCTURE); }

            if (rootNode == null) return new(null, ErrorFlag.INVALID_STRUCTURE);
            return new(rootNode, ErrorFlag.OK);
        }

        private static XMLNode ParseXMLFile(in string filePath)
        {
            StreamReader sr = new(filePath);
            XmlReaderSettings settings = new();

            using XmlReader reader = XmlReader.Create(sr, settings);
            XMLNode rootXMLNode = null;
            XMLNode currentNode = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        XMLElement currentElement = new(reader.Name);

                        //* Assign the new current node
                        if (currentNode == null)
                            currentNode = new(currentElement);
                        else
                        {
                            XMLNode newNode = new(currentElement);
                            currentNode.AddNode(in newNode);
                            newNode.Parent = currentNode;
                            currentNode = newNode;
                        }

                        //* Handle the root node if haven't yet
                        rootXMLNode ??= currentNode;

                        //* Fetch Attributes
                        while (reader.MoveToNextAttribute())
                            currentElement.AddAttribute(reader.Name, reader.Value);
                        reader.MoveToElement();

                        //* Check if was emtpy element, treat as EndElement
                        if (reader.IsEmptyElement)
                            if (currentNode != null)
                                currentNode = currentNode.Parent;
                        break;

                    case XmlNodeType.EndElement:
                        if (currentNode != null)
                            currentNode = currentNode.Parent;
                        break;

                    default:
                        break;
                }
            }
            return rootXMLNode;
        }
    }
}