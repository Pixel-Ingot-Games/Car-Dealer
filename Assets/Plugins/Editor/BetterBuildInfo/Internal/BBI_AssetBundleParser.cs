using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Better.BuildInfo.Internal
{
    internal sealed class AssetBundleParser
    {
        private const string multiLineStringEnd = @""" (string)";

        private readonly Dictionary<string, int> knownTypes = new Dictionary<string, int>()
        {
            {"int", 4},
            {"unsigned int", 4},
            {"SInt64", 8},
            {"UInt64", 8},
            {"SInt32", 4},
            {"UInt32", 4},
            {"SInt16", 2},
            {"UInt16", 2},
            {"SInt8", 1},
            {"UInt8", 1},
            {"char", 1},
            {"float", 4},
            {"double", 8},
            {"Vector4f", 16},
            {"Vector3f", 12},
            {"Vector2f", 8},
            {"bool", 1},
            {"GUID", 16},
            {"ColorRGBA", 16},
            {"RectOffset", 16},
        };

        private readonly Regex dataRegex = new Regex(@"\s*\(([\s\w]+)\) #\d+: ", RegexOptions.Compiled);

        private readonly Regex objectHeaderRegex = new Regex(@"^ID: (\-?[a-f0-9]+) \(ClassID: (\d+)\) (\w+)", RegexOptions.Compiled);

        private readonly Regex scalarFieldRegex = new Regex(@"\t*(\w+) (\S+) \((.*)\)", RegexOptions.Compiled);

        private readonly Regex stringFieldRegex = new Regex(@"\t*(\w+) ""(.*)"" \(string\)", RegexOptions.Compiled);


        public AssetBundle Parse(string name, Stream bundle, string unityToolsPath)
        {
            return Parse(name, path =>
            {
                using (var copy = File.Create(path))
                {
                    bundle.CopyTo(copy);
                    copy.Flush();
                }
            }, unityToolsPath);
        }

        public AssetBundle Parse(string name, Action<string> copyToCallback, string unityToolsPath)
        {
            var tempDirPath = ReliablePath.Combine(ReliablePath.GetTempPath(), ReliablePath.GetRandomFileName());
            var directory = Directory.CreateDirectory(tempDirPath);

            var result = new AssetBundle()
            {
                name = name
            };
            var exports = new List<Tuple<AssetId, string>>();

            try
            {
                var tempFilePath = ReliablePath.Combine(tempDirPath, "bundle");
                copyToCallback(tempFilePath);

                result.compressedSize = new FileInfo(tempFilePath).Length;

                RunProcessEnsureSuccess(ReliablePath.Combine(unityToolsPath, "WebExtract"), tempFilePath);

                var resultDirectory = tempFilePath + "_Data";
                foreach (var binPath in Directory.GetFiles(resultDirectory))
                {
                    result.uncompressedSize += new FileInfo(binPath).Length;

                    if (ReliablePath.GetExtension(binPath) == ".resS")
                        continue;

                    RunProcessEnsureSuccess(ReliablePath.Combine(unityToolsPath, "binary2text"), binPath);

                    var fileName = ReliablePath.GetFileName(binPath).ToLower();

                    using (var reader = File.OpenText(binPath + ".txt"))
                    {
                        result.files.Add(fileName);
                        result.isSceneBundle |= AppendBundleResults(fileName, reader, result.assets, exports);
                    }
                }

                // time to assign paths
                var idToExportPath = new Dictionary<AssetId, string>();
                var unprocessedExports = new HashSet<AssetId>();
                foreach (var pair in exports)
                {
                    if (!unprocessedExports.Add(pair.Item1))
                        continue;
                    idToExportPath.Add(pair.Item1, pair.Item2);
                }


                foreach (var asset in result.assets)
                {
                    asset.bundle = result;
                    if (idToExportPath.TryGetValue(asset.id, out asset.assetPath))
                    {
                        unprocessedExports.Remove(asset.id);
                    }
                }

                foreach (var id in unprocessedExports)
                {
                    var path = idToExportPath[id];
                    result.assets.Add(new Asset()
                    {
                        id = id,
                        assetPath = path,
                        classID = path.EndsWith(".unity") ? YamlClassId.SceneAsset : YamlClassId.Unknown,
                        bundle = result,
                    });
                }


                FlattenGameObjectsAndComponents(result);

                return result;
            }
            finally
            {
                directory.Delete(true);
            }
        }

        private static int CountChar(char c, string line, int start)
        {
            int count = 0;

            for (int i = start, max = line.Length; i < max; ++i)
            {
                if (line[i] == c)
                    ++count;
            }

            return count;
        }

        private static void FlattenGameObjectsAndComponents(AssetBundle result)
        {
            // flatten components
            var idToGameObject = result.assets.Where(x => x.classID == YamlClassId.GameObject).ToDictionary(x => x.id, x => new { go = x, children = new List<Asset>(), components = new List<Asset>() });
            var transformIdToGameObject = result.assets.Where(x => x.classID.IsTransform() && x.gameObject.IsValid).ToDictionary(x => x.id, x => idToGameObject[x.gameObject]);
            var roots = new List<Asset>();

            // find root game objects
            foreach (var transform in result.assets.Where(x => x.classID.IsTransform()))
            {
                if (!transform.gameObject.IsValid)
                    continue;

                var entry = idToGameObject[transform.gameObject];
                if (!transform.transformParent.IsValid)
                {
                    roots.Add(entry.go);
                }
                else
                {
                    var parentEntry = transformIdToGameObject[transform.transformParent];
                    parentEntry.children.Add(entry.go);
                }
            }

            // find components
            foreach (var component in result.assets.Where(x => x.classID.IsComponent()))
            {
                if (!component.gameObject.IsValid)
                    continue;

                var entry = idToGameObject[component.gameObject];
                entry.components.Add(component);
            }

            // remap game objects to roots
            Dictionary<AssetId, AssetId> remapToRootGameObject = new Dictionary<AssetId, AssetId>();
            foreach (var root in roots)
            {
                var gameObjects = new Queue<Asset>(new[] { root });
                while (gameObjects.Count > 0)
                {
                    var gameObject = gameObjects.Dequeue();
                    var gameObjectEntry = idToGameObject[gameObject.id];

                    if (gameObject != root)
                    {
                        root.estimatedSize += gameObject.estimatedSize;
                        remapToRootGameObject.Add(gameObject.id, root.id);
                        foreach (var id in gameObject.references)
                            root.references.Add(id);
                    }

                    foreach (var component in gameObjectEntry.components)
                    {
                        root.estimatedSize += component.estimatedSize;
                        remapToRootGameObject.Add(component.id, root.id);
                        foreach (var id in component.references)
                            root.references.Add(id);
                    }

                    foreach (var childGameObject in gameObjectEntry.children)
                    {
                        gameObjects.Enqueue(childGameObject);
                    }
                }
            }

            // remove all components, all non-root game objects & asset bundle
            result.assets.RemoveAll(x => x.gameObject.IsValid && x.classID.IsComponent());
            result.assets.RemoveAll(x => x.classID == YamlClassId.GameObject && !roots.Contains(x));
            result.assets.RemoveAll(x => x.classID == YamlClassId.AssetBundle);

            // remap references everywhere
            foreach (var asset in result.assets)
            {
                HashSet<AssetId> newReferences = new HashSet<AssetId>();
                foreach (var id in asset.references)
                {
                    AssetId remapped;
                    if (remapToRootGameObject.TryGetValue(id, out remapped))
                    {
                        newReferences.Add(remapped);
                    }
                    else
                    {
                        newReferences.Add(id);
                    }
                }
                asset.references = newReferences;
            }
        }

        private void ReadPPtrOrThrow(StreamReader reader, out int fileId, out long pathId)
        {
            string name, type;
            long value;

            fileId = default(int);
            pathId = default(long);

            var line = reader.ReadLine();
            if (line != null && TryGetLongFieldValue(line, out name, out value, out type) && name == "m_FileID")
            {
                line = reader.ReadLine();
                fileId = (int)value;
                if (line != null && TryGetLongFieldValue(line, out name, out pathId, out type) && name == "m_PathID")
                {
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Unable to read PathId: " + line);
                }
            }

            throw new InvalidOperationException("Unable to read FileId: " + line);
        }

        private static void RunProcessEnsureSuccess(string fileName, string arguments)
        {
            var outputStringBuilder = new StringBuilder();

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = $"\"{arguments}\"";
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.EnableRaisingEvents = false;
                process.OutputDataReceived += (sender, eventArgs) => outputStringBuilder.AppendLine(eventArgs.Data);
                process.ErrorDataReceived += (sender, eventArgs) => outputStringBuilder.AppendLine(eventArgs.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Invalid exit code: {process.ExitCode}\nOutput: {outputStringBuilder}");
                }
            }
        }

        private static string SkipUntilStartsWith(StreamReader reader, string prefix, int minIndent)
        {
            int skipped = 0;
            string line;
            for (line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                ++skipped;
                if (line.Length == 0)
                    continue;

                int indent;
                for (indent = 0; indent < line.Length && line[indent] == '\t'; ++indent)
                    ;

                if (indent < minIndent)
                    throw new InvalidOperationException("Indent");

                if (string.Compare(line, indent, prefix, 0, prefix.Length) == 0)
                    break;
            }
            return line;
        }

        private bool TryGetLongFieldValue(string line, out string name, out long value, out string type)
        {
            var m = scalarFieldRegex.Match(line);

            if (m.Success && long.TryParse(m.Groups[2].Value, out value))
            {
                name = m.Groups[1].Value;
                type = m.Groups[3].Value;
                return true;
            }

            value = default(long);
            name = default(string);
            type = default(string);
            return false;
        }

        private bool GetTypeSize(string type, out int size)
        {
            return knownTypes.TryGetValue(type, out size);
        }

        private bool AppendBundleResults(string archivePath, StreamReader reader, List<Asset> assets, List<Tuple<AssetId, string>> exports)
        {
            int lineIndex = 0;
            string line = reader.ReadLine();

            bool isSceneAssetBundle = false;
            Dictionary<int, string> fileIdToPath = new Dictionary<int, string>() { { 0, archivePath } };

            {
                Regex pathRegex = new Regex(@"^path\((\d+)\)\: ""(.*)"" GUID: ([0-9A-Za-z]+) Type: (\d+)", RegexOptions.Compiled);
                // get externals
                for (line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (line.StartsWith("External References"))
                        continue;

                    var match = pathRegex.Match(line.Trim());
                    if (!match.Success)
                        break;

                    var archiveName = match.Groups[2].Value.ToLower();
                    if (archiveName.StartsWith("archive:/"))
                    {
                        archiveName = archiveName.Substring("archive:/".Length);
                        var split = archiveName.Split('/');
                        if (split.Length == 2 && split[1].StartsWith(split[0]))
                        {
                            archiveName = split[1];
                        }
                    }

                    fileIdToPath.Add(int.Parse(match.Groups[1].Value), archiveName);
                }
            }

            Asset current = null;

            // now we're ready
            int emptyLineCounter = 0;
            int maxWaitForSize = 0;

            for (; line != null; line = reader.ReadLine(), ++lineIndex)
            {
                go_back:
                if (line.Length == 0)
                {
                    ++emptyLineCounter;
                    continue;
                }

                bool possiblyNewObject = emptyLineCounter > 1;
                emptyLineCounter = 0;

                if (possiblyNewObject)
                {
                    // entries are usually seprated with two empty lines, but you never know...
                    var m = objectHeaderRegex.Match(line);
                    if (m.Success)
                    {
                        current = new Asset();
                        current.id.archivePath = archivePath;
                        current.id.pathId = long.Parse(m.Groups[1].Value);
                        current.classID = (YamlClassId)int.Parse(m.Groups[2].Value);

                        assets.Add(current);

                        if (current.classID == YamlClassId.AssetBundle)
                        {
                            // AssetBundle, need to use it to get the paths
                            // skip until we get to the container
                            SkipUntilStartsWith(reader, "m_Container", 1);
                            line = reader.ReadLine();

                            if (line != null)
                            {
                                m = scalarFieldRegex.Match(line);

                                int containerSize;
                                if (m.Success && m.Groups[1].Value == "size" && int.TryParse(m.Groups[2].Value, out containerSize))
                                {
                                    for (int i = 0; i < containerSize; ++i)
                                    {
                                        int fileId;
                                        long pathId;

                                        SkipUntilStartsWith(reader, "data", 2);
                                        var pathLine = SkipUntilStartsWith(reader, "first", 3);
                                        SkipUntilStartsWith(reader, "asset", 3);
                                        ReadPPtrOrThrow(reader, out fileId, out pathId);

                                        m = stringFieldRegex.Match(pathLine);
                                        if (m.Success)
                                        {
                                            var id = new AssetId()
                                            {
                                                archivePath = fileIdToPath[fileId],
                                                pathId = pathId,
                                            };

                                            exports.Add(Tuple.Create(id, m.Groups[2].Value));
                                        }
                                        else
                                        {
                                            throw new InvalidOperationException("Failed to parse line: " + pathLine);
                                        }
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Failed to parse line: " + line);
                                }
                            }

                            line = SkipUntilStartsWith(reader, "m_IsStreamedSceneAssetBundle", 1);
                            {
                                m = scalarFieldRegex.Match(line);
                                int tmp;
                                if (m.Success && int.TryParse(m.Groups[2].Value, out tmp))
                                {
                                    isSceneAssetBundle = tmp != 0;
                                }
                            }
                        }
                        continue;
                    }
                }

                if (current == null)
                {
                    throw new InvalidOperationException("current == null");
                }

                // skip tabs
                int p = 0;
                for (; p < line.Length && line[p] == '\t'; ++p)
                    ;

                int nameStart = p;
                for (; p < line.Length && line[p] != ' '; ++p)
                    ;

                int nameEnd = p++;
                string name = line.Substring(nameStart, nameEnd - nameStart);

                if (line.EndsWith(")"))
                {
                    int typeStartIndex = line.LastIndexOf('(', line.Length - 1, line.Length - p);
                    if (typeStartIndex >= 0)
                    {
                        if (maxWaitForSize > 0)
                        {
                            --maxWaitForSize;
                            if (string.Compare(line, nameStart, "size", 0, 4) == 0)
                            {
                                var valueStr = line.Substring(p, typeStartIndex - p);
                                long value;
                                if (long.TryParse(valueStr, out value))
                                {
                                    current.estimatedSize += value;
                                }
                                maxWaitForSize = 0;
                            }
                        }

                        string typeName = line.Substring(typeStartIndex + 1, line.Length - typeStartIndex - 2);
                        int size;
                        if (GetTypeSize(typeName, out size))
                        {
                            current.estimatedSize += size;
                        }
                        else if (typeName == "StreamingInfo")
                        {
                            maxWaitForSize = 3;
                        }
                        else if (typeName == "string")
                        {
                            var strLength = line.Length - p
                                - 2 // for 2 quotes
                                - 1 // for space after 2nd quote
                                - 2 // for parenthesis around type
                                - 6; // for the name of the type

                            if (line.StartsWith("\tm_Name "))
                            {
                                current.name = line.Substring(p + 1, strLength);
                            }
                            else if (current.classID == YamlClassId.Shader && line.StartsWith("\t\tm_Name ") && string.IsNullOrEmpty(current.name))
                            {
                                current.name = line.Substring(p + 1, strLength);
                            }
                            current.estimatedSize += strLength;
                        }
                        else if (typeName.StartsWith("PPtr<"))
                        {
                            current.estimatedSize += 12;

                            int fileId;
                            long pathId;
                            ReadPPtrOrThrow(reader, out fileId, out pathId);

                            var id = new AssetId()
                            {
                                pathId = pathId,
                                archivePath = fileIdToPath[fileId]
                            };

                            if (id.IsValid)
                            {
                                if (typeName == "PPtr<GameObject>" && name == "m_GameObject" && nameStart == 1 && current.classID.IsComponent())
                                {
                                    if (current.gameObject.IsValid)
                                        throw new InvalidOperationException("Expected valid GameObject");
                                    current.gameObject = id;
                                }
                                else if (typeName == "PPtr<Transform>" && name == "m_Father" && nameStart == 1 && current.classID.IsTransform())
                                {
                                    if (current.transformParent.IsValid)
                                        throw new InvalidOperationException("Expected valid Transform parent");
                                    current.transformParent = id;
                                }
                                else if (!current.id.Equals(id))
                                {
                                    current.references.Add(id);
                                }
                            }
                        }

                        continue;
                    }
                }

                Debug.Assert(maxWaitForSize == 0);
                maxWaitForSize = 0;

                if (p < line.Length)
                {
                    if (string.Compare(line, nameStart, "data", 0, 4) == 0)
                    {
                        // maybe inline data then?

                        var m = dataRegex.Match(line, p);
                        if (m.Success)
                        {
                            var typeName = m.Groups[1].Value;
                            int size;
                            if (GetTypeSize(typeName, out size))
                            {
                                // count spaces...
                                int count = CountChar(' ', line, m.Index + m.Length);

                                if (count == 24)
                                {
                                    count = 25;
                                    // fast mode engage!
                                    string prevLine = line;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (string.Compare(prevLine, 0, line, 0, nameEnd) == 0)
                                        {
                                            count += 25;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    // oops, let's actually count
                                    m = dataRegex.Match(prevLine, p);
                                    if (m.Success)
                                    {
                                        count -= 24 - CountChar(' ', prevLine, m.Index + m.Length);
                                    }
                                    current.estimatedSize += count * size;
                                    if (line != null)
                                        goto go_back;
                                }
                                else
                                {
                                    current.estimatedSize += (count + 1) * size;
                                }
                            }
                            continue;
                        }
                    }

                    // maybe multiline string start then?
                    if (line[p] == '\"')
                    {
                        ++p;
                        int length = line.Length - p;
                        while ((line = reader.ReadLine()) != null)
                        {
                            ++lineIndex;
                            if (line.EndsWith(multiLineStringEnd))
                            {
                                length += line.Length - multiLineStringEnd.Length;
                                break;
                            }
                            else
                            {
                                length += line.Length;
                            }
                        }
                        current.estimatedSize += length;
                        continue;
                    }
                }

                Log.Debug("Failed to parse line: " + line);
            }

            return isSceneAssetBundle;
        }

        [DebuggerDisplay("{archivePath} : {pathId}")]
        public struct AssetId : IEquatable<AssetId>
        {
            [XmlAttribute]
            public string archivePath;

            [XmlAttribute]
            public long pathId;

            public bool IsValid => pathId != 0;

            public bool Equals(AssetId other)
            {
                return other.archivePath == archivePath && other.pathId == pathId;
            }

            public override bool Equals(object obj)
            {
                if (obj is AssetId)
                    return Equals((AssetId)obj);
                return false;
            }

            public override int GetHashCode()
            {
                int result = pathId.GetHashCode();
                if (archivePath != null)
                {
                    result = unchecked(result * 31 + archivePath.GetHashCode());
                }
                return result;
            }

            public override string ToString()
            {
                return string.Format("({0} : {1})", archivePath, pathId);
            }
        }

        [DebuggerDisplay("({classID}, {name}, {estimatedSize}, {id}, {assetPath})")]
        public class Asset
        {
            public string assetPath;
            public YamlClassId classID;
            public long estimatedSize;

            // only valid for components
            [XmlIgnore]
            public AssetId gameObject;

            public AssetId id;

            public string name;
            public HashSet<AssetId> references = new HashSet<AssetId>();

            // only valid for transforms
            [XmlIgnore]
            public AssetId transformParent;
            [XmlIgnore]
            public AssetBundle bundle;
        }

        public class AssetBundle
        {
            public List<Asset> assets = new List<Asset>();
            public long compressedSize;
            public List<string> files = new List<string>();
            public long uncompressedSize;
            public bool isSceneBundle;
            public string name;
        }
    }
}