using System;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using XMLSystem;

namespace DataContainer.DatabaseSys.Databases.TemplateDatabase
{
    #region Legend
    /// <Template version="[REQUIRED]" name="[OPTIONAL]">
    ///     <Tags>  //  Must have <Tag/> only as children 
    ///         <Tag name="[REQUIRED]" type="[Project | Software]" />
    ///     </Tags>
    /// 
    ///     <Structure>
    ///         .// When <GitFiles/> are included,
    ///         .// auto create .tmp files in empty folders
    ///         <Folder name="project">
    ///             .// Marks project as GDExt compat
    ///             .// Any attribute names listed will be created OS folders
    ///             .// in a bin folder.
    ///             <GDExtension ...=""/>
    /// 
    ///             .// ref needs to point to a relative path in `template_assets`
    ///             <File name="[REQUIRED]" ref="[REQUIRED]">
    /// 
    ///             .// Marks the godot project directory
    ///             .// Can be anywhere under <Structure/> if <GDHubMeta> used
    ///             .// otherwise must be child of <Structure/>
    ///             <GodotFiles/>   
    ///         </Folder>
    ///         
    ///         .// When included, must be child of <Structure/>
    ///         <GitFiles/> 
    /// 
    ///         .// Allows non-root placement of <GodotFiles/>
    ///         .// If included, <GodotFiles> must not be on the same level
    ///         .// Will auto set path to <GodotFiles/>
    ///         <GDHubMeta/>  
    ///     </Structure>
    /// </Template>
    /// 
    #endregion

    public partial class TemplateData
    {
        private const string VALID_VERSION = "1";

        private static bool HandleTemplateElement(in TemplateData template,
            in XMLNode templateNode, in string fileName)
        {
            if (templateNode.ElementName != "Template")
            {
                LogError($"Node is not <Template/>. Found {templateNode}");
                return false;
            }

            string versionID = templateNode.GetAttributeValue("version");
            if (versionID != VALID_VERSION)
            {
                LogError($"Missing or Invalid version ID: {templateNode}");
                return false;
            }

            string name = templateNode.GetAttributeValue("name");
            name ??= fileName;
            template.Name = name;
            return true;
        }

        private static bool HandleTagsElement(in TemplateData template,
            in XMLNode tagsNode)
        {
            if (tagsNode == null)
            {
                LogError($"Node is not <Tags/>. Found {tagsNode}");
                return false;
            }

            foreach (XMLNode node in tagsNode.GetChildNodes())
            {
                if (node.ElementName != "Tag")
                {
                    LogError($"Invalid element under <Tags/>. Found {node}");
                    return false;
                }

                string tagName = node.GetAttributeValue("name");
                if (tagName == null)
                {
                    LogError(GenerateTagError("NULL", "...", "name is missing."));
                    return false;
                }

                string type = node.GetAttributeValue("type");
                if (type != "Project" && type != "Software")
                {
                    LogError(GenerateTagError(
                        "...", "???", "type must be `Project` or `Software`."
                    ));
                    return false;
                }

                bool isSoftware = false;
                if (type == "Software") isSoftware = true;

                TagKey tagKey = new(tagName, isSoftware);
                if (isSoftware)
                    template._softwareTags.Add(tagKey);
                else
                    template._projectTags.Add(tagKey);
            }
            return true;
        }

        private static bool HandleStructureElement(in TemplateData template,
            in XMLNode structureNode)
        {
            if (structureNode == null)
            {
                LogError($"Node is not <Structure/>. Found {structureNode}");
                return false;
            }

            if (structureNode.ChildCount == 0)
            {
                LogError($"{structureNode} has no children elements.");
                return false;
            }

            //* Create Git files if needed
            bool isGitBased = false;
            XMLNode gitNode = structureNode.GetNode("GitFiles");
            if (gitNode != null)
            {
                isGitBased = true;
                HandleGitElement(in template);
            }

            //* Check if this is a GDExt project or a standard godot project
            XMLNode godotNode = structureNode.GetNode("GodotFiles");
            XMLNode gdhubNode = structureNode.GetNode("GDHubMeta");

            if ((godotNode == null && gdhubNode == null)
                || (godotNode != null && gdhubNode != null))
            {
                LogError("<Structure/> must have either a <GodotFiles/> or <GDHubMeta> child element.");
                return false;
            }

            return HandleStructureSubElements(
                in template,
                in structureNode,
                in isGitBased,
                gdhubNode != null
            );
        }

        private static void HandleGitElement(in TemplateData template)
        {
            template._buildInstructions.Add(new([".gitignore"], ""));
            template._buildInstructions.Add(new([".gitattributes"], ""));
        }

        private static void HandleGodotFilesElement(in TemplateData template,
            in string[] currentPath)
        {
            template._buildInstructions.Add(new([.. currentPath, "project.godot"], ""));
            template._buildInstructions.Add(new([.. currentPath, "icon.svg"], ""));
        }

        private static bool HandleFolderElement(in TemplateData template,
            in XMLNode folderNode, ref string[] currentPath, in bool createGDIgnore,
            out string[] godotFilesPath)
        {
            string folderName = folderNode.GetAttributeValue("name");
            if (folderName == null)
            {
                LogError("Error: <Folder name='NULL'/> | missing name attribute.");
                godotFilesPath = [];
                return false;
            }

            //* Check if folder is empty
            if (folderNode.ChildCount > 0)
            {
                string[] newPath = [.. currentPath, folderName];
                bool state = HandleSubFolderElements(
                    in template,
                    in folderNode,
                    ref newPath,
                    in createGDIgnore,
                    out string[] godotFilesLocation
                );
                godotFilesPath = godotFilesLocation;
                return state;
            }

            string[] folderPath = [.. currentPath, folderName];
            CreateEmptyFolder(in template, in folderPath, in createGDIgnore);

            godotFilesPath = [];
            return true;
        }

        //* These are not direct child folders of <Structure/>
        private static bool HandleSubFolderElements(in TemplateData template,
            in XMLNode folderNode, ref string[] currentPath,
            in bool createGDIgnore, out string[] godotFilesPath)
        {
            //* Handle sub elements
            string[] foundGodotFiles = [];
            foreach (XMLNode node in folderNode.GetChildNodes())
            {
                switch (node.ElementName)
                {
                    case "Folder":
                        if (!HandleFolderElement(in template, in node,
                            ref currentPath, in createGDIgnore,
                            out string[] foundGodotFilesPath))
                        {
                            godotFilesPath = [];
                            return false;
                        }
                        if (foundGodotFilesPath.Length > 0)
                            foundGodotFiles = foundGodotFilesPath;
                        break;

                    case "File":
                        if (!HandleFileElement(in template, in node, in currentPath))
                        {
                            godotFilesPath = [];
                            return false;
                        }
                        break;

                    case "GodotFiles":
                        foundGodotFiles = currentPath;
                        HandleGodotFilesElement(in template, in currentPath);
                        break;

                    case "GDExtension":
                        HandleGDExt(in template, in node, in currentPath, in createGDIgnore);
                        break;

                    default:
                        LogError($"Invalid Element Found: {node}");
                        godotFilesPath = [];
                        return false;
                }
            }
            godotFilesPath = foundGodotFiles;
            return true;
        }

        private static bool HandleStructureSubElements(in TemplateData template,
            in XMLNode structureNode, in bool createGDIgnore, in bool usingGDHubMetaFile)
        {
            string[] foundGodotFilesPath = [];
            foreach (XMLNode subNode in structureNode.GetChildNodes())
            {
                switch (subNode.ElementName)
                {
                    case "Folder":
                        string[] path = [];
                        bool state = HandleFolderElement(
                            in template,
                            in subNode,
                            ref path,
                            createGDIgnore,
                            out string[] godotFilesPath
                        );
                        if (!state) return false;

                        if (godotFilesPath.Length > 0)
                            foundGodotFilesPath = godotFilesPath;
                        break;

                    case "File":
                        if (!HandleFileElement(in template, in subNode, []))
                            return false;
                        break;

                    case "GodotFiles":
                        HandleGodotFilesElement(in template, []);
                        break;

                    //* Already handled/Handled later
                    case "GitFiles":
                    case "GDHubMeta":
                        break;

                    default:
                        LogError($"Invalid Element Found: {subNode}");
                        return false;
                }
            }

            if (usingGDHubMetaFile)
            {
                FileData fileData = new([".gdhub"], "");
                fileData.metadata.Add(
                    "godotProjectRelativePath",
                    string.Join('/', foundGodotFilesPath)
                );
                template._buildInstructions.Add(fileData);
            }

            return true;
        }

        private static bool HandleFileElement(in TemplateData template,
            in XMLNode fileNode, in string[] currentPath)
        {
            string fileName = fileNode.GetAttributeValue("name");
            if (fileName == null)
            {
                LogError("Error: <File/> missing name attribute.");
                return false;
            }

            string refPath = fileNode.GetAttributeValue("ref");
            if (refPath == null)
            {
                LogError("Error: <File/> missing ref attribute.");
                return false;
            }

            template._buildInstructions.Add(new(
                [.. currentPath, fileName],
                refPath
            ));
            return true;
        }

        private static bool HandleGDExt(in TemplateData template,
            in XMLNode gdextNode, in string[] currentPath, in bool createGDIgnore)
        {
            if (gdextNode == null)
            {
                LogError($"Node is not <GDExtension/>. Found {gdextNode}");
                return false;
            }

            if (!gdextNode.HasSiblingNode("GodotFiles"))
            {
                LogError($"{gdextNode} must be siblings with <GodotFiles/>");
                return false;
            }

            //* Create the bin folder
            string[] folderPath = [.. currentPath, "bin"];
            CreateEmptyFolder(
                in template,
                in folderPath,
                createGDIgnore && gdextNode.AttributeNames.Length == 0
            );

            //* Add .gdextension to the bin folder
            string[] gdextFile = [.. currentPath, "bin", ".gdextension"];
            template._buildInstructions.Add(new(in gdextFile, ""));

            //* Create OS based folders in bin based on attribute name
            foreach (string attributeName in gdextNode.AttributeNames)
            {
                string[] osBinFolder = [.. currentPath, "bin", attributeName];
                CreateEmptyFolder(
                    in template,
                    in osBinFolder,
                    createGDIgnore
                );
            }
            return true;
        }

        private static void CreateEmptyFolder(in TemplateData template,
            in string[] currentPath, in bool createGDIgnore)
        {
            if (createGDIgnore)
                template._buildInstructions.Add(new(
                    [.. currentPath, ".placeholder"], ""
                ));
            else
                template._buildInstructions.Add(new(currentPath, null));
        }

        private static string GenerateTagError(string tagName, string tagType,
            string error)
            => $"Error: <Tag name='{tagName}' type='{tagType}'> | {error}";

        // TODO: Replace console writes with Notification System
        private static void LogError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }
    }
}