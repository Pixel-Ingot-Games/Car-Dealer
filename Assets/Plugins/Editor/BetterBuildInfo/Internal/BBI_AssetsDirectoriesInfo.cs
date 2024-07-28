using System.Collections.Generic;
using System.Linq;
using System.Text;
using Better.BuildInfo.Internal;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    [System.Serializable]
    public class AssetsDirectoriesInfo
    {
        public bool IsEmpty
        {
            get { return paths.Count == 0; }
        }

        public List<string> paths = new List<string>();
        public List<DirectoryEntry> entries = new List<DirectoryEntry>();

        [System.Serializable]
        public class DirectoryEntry
        {
            public int assetsCount;
            public int usedAssetsCount;
            public int editorAssetsCount;
            public bool hadSpecialEditorDirectory;
            public FileSize size;
        }

        

        public bool GetDirectoryInfo(string path, out int assetsCount, out int usedAssetsCount, out int editorAssetsCount, out FileSize size)
        {
            var idx = paths.IndexOf(path);
            if (idx < 0)
            {
                assetsCount = 0;
                usedAssetsCount = 0;
                editorAssetsCount = 0;
                size = FileSize.Zero;
                return false;
            }

            assetsCount = entries[idx].assetsCount;
            usedAssetsCount = entries[idx].usedAssetsCount;
            editorAssetsCount = entries[idx].editorAssetsCount;
            size = entries[idx].size;
            return true;
        }

        private static string[] SpecialEditorDirectories = new[]
        {
            "/Editor",
            "/Editor Default Resources",
            "/Gizmos",
            "/StreamingAssets",
            "/Plugins/x86",
            "/Plugins/x86_64",
            "/Plugins/x64",
            "/Plugins/Android",
            "/Plugins/iOS",
            "/Plugins/WSA",
            "/Plugins/Tizen",
            "/Plugins/PSVita",
            "/Plugins/PS4",
            "/Plugins/SamsungTV",
        };

        public static bool IsInSpecialEditorDirectory(string directoryPath)
        {
            foreach (var part in SpecialEditorDirectories)
            {
                var index = directoryPath.IndexOf(part, System.StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    continue;
                if (index == directoryPath.Length - part.Length)
                    return true;
                if (directoryPath[index + part.Length] == '/')
                    return true;
            }
            return false;
        }

        public void Refresh(IEnumerable<string> allAssets, IEnumerable<AssetInfo> usedAssets)
        {
            foreach (var assetPath in allAssets)
            {
                string directoryPath;
                bool isDirectory = true;

                if (Directory.Exists(assetPath))
                {
                    directoryPath = assetPath.TrimEnd('/', '\\');
                }
                else
                {
                    isDirectory = false;
                    directoryPath = ReliablePath.GetDirectoryName(assetPath);
                }
                
                bool hadSpecialEditorDirectory = false;

                while ( !string.IsNullOrEmpty(directoryPath) && IsInSpecialEditorDirectory(directoryPath) )
                {
                    hadSpecialEditorDirectory = true;
                    directoryPath = ReliablePath.GetDirectoryName(directoryPath);
                }

                // add directory entries
                while ( !string.IsNullOrEmpty(directoryPath) )
                {
                    var directoryIndex = paths.BinarySearch(directoryPath);
                    if (directoryIndex < 0)
                    {
                        directoryIndex = ~directoryIndex;
                        paths.Insert(directoryIndex, directoryPath);
                        entries.Insert(directoryIndex, new DirectoryEntry());
                    }

                    if (!isDirectory)
                    {
                        if (hadSpecialEditorDirectory)
                        {
                            ++entries[directoryIndex].editorAssetsCount;
                        }
                        else
                        {
                            ++entries[directoryIndex].assetsCount;
                        }
                    }

                    entries[directoryIndex].hadSpecialEditorDirectory |= hadSpecialEditorDirectory;
                    directoryPath = ReliablePath.GetDirectoryName(directoryPath);
                }
            }

            // now do the count
            foreach (var asset in usedAssets)
            {
                var guid = AssetDatabase.AssetPathToGUID(asset.path);

                if (IsInSpecialEditorDirectory(asset.path))
                    continue;

                if (string.IsNullOrEmpty(guid))
                    continue;

                var directoryPath = ReliablePath.GetDirectoryName(asset.path);

                while (!string.IsNullOrEmpty(directoryPath))
                {
                    var directoryIndex = paths.BinarySearch(directoryPath);
                    if (directoryIndex >= 0)
                    {
                        ++entries[directoryIndex].usedAssetsCount;
                        entries[directoryIndex].size += asset.size; 
                        directoryPath = ReliablePath.GetDirectoryName(directoryPath);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
