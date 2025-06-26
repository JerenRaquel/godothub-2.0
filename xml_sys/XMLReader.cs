using System;
using System.Collections.Generic;
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

        public readonly struct ParseResults(in XMLNode root, ErrorFlag error,
            in Dictionary<string, ulong> parsedElements)
        {
            public readonly XMLNode root = root;
            public readonly ErrorFlag errorFlag = error;
            public readonly Dictionary<string, ulong> parsedElements = parsedElements;
        }

        public static ParseResults ReadFile(in string filePath)
        {
            if (!File.Exists(filePath))
                return new(null, ErrorFlag.INVALID_FILE_PATH, null);

            XMLNode rootNode;
            Dictionary<string, ulong> elementKey;
            try
            {
                rootNode = ParseXMLFile(
                    in filePath,
                    out Dictionary<string, ulong> parsedElements
                );
                elementKey = parsedElements;
            }
            catch
            { return new(null, ErrorFlag.INVALID_STRUCTURE, null); }

            if (rootNode == null)
                return new(null, ErrorFlag.INVALID_STRUCTURE, null);
            return new(in rootNode, ErrorFlag.OK, in elementKey);
        }

        private static XMLNode ParseXMLFile(in string filePath,
            out Dictionary<string, ulong> parsedElements)
        {
            StreamReader sr = new(filePath);
            XmlReaderSettings settings = new();

            using XmlReader reader = XmlReader.Create(sr, settings);
            XMLNode rootXMLNode = null;
            parsedElements = [];
            XMLNode currentNode = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        XMLElement currentElement = new(reader.Name);
                        if (!parsedElements.ContainsKey(reader.Name))
                            parsedElements.Add(reader.Name, 0);
                        parsedElements[reader.Name]++;

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