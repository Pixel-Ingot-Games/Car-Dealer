using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnitySprite = UnityEngine.Sprite;

namespace Better.BuildInfo.Internal
{
    public static class BuildLogParser
    {
        private const string SpriteAtlasPrefix = "Built-in Texture2D: ";

        public static Dictionary<string, long> GetLastBuildAssetsSizes(string buildLogPath, Dictionary<string, long> scenes)
        {
            Log.Debug("Using editor log at: {0}", buildLogPath);

            // copying log
            var tempFileName = ReliablePath.GetTempFileName();
            Dictionary<string, long> assets = new Dictionary<string, long>();

            try
            {
                File.Copy(buildLogPath, tempFileName, true);
                List<string> assetsLines = null;

#if UNITY_2017_1_OR_NEWER
                var parsingLevels = false;
                var levelSizeRegex = new Regex(@"Level \d+ '(.*)' uses .* ([\d.]+) ([MK]B) uncompressed", RegexOptions.IgnoreCase | UnityVersionAgnostic.CompiledRegexOptions);
#endif

                int lineCount = 0;

                using (ProfileSection.Create("Reading Log"))
                using (var reader = new StreamReader(File.Open(tempFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    for (string line = reader.ReadLine(); line != null; line = reader.ReadLine(), ++lineCount)
                    {
                        if (line.StartsWith("Used Assets") && line.EndsWith("sorted by uncompressed size:"))
                        {
                            Log.Debug("Found assets marker in the log at line {0}", lineCount);
                            if (assetsLines == null)
                                assetsLines = new List<string>();
                            else
                                assetsLines.Clear();
                        }
                        else if (assetsLines != null)
                        {
                            assetsLines.Add(line);
                        }

#if UNITY_2017_1_OR_NEWER
                        if (line.Equals("***Player size statistics***", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Debug("Found level statistics start at line {0}", lineCount);
                            scenes.Clear();
                            parsingLevels = true;
                        }
                        else if (parsingLevels)
                        {
                            var match = levelSizeRegex.Match(line);
                            if (match.Success)
                            {
                                Log.Debug("Found level size at line {0}: {1}", lineCount, match.Value);
                                var path = match.Groups[1].Value;

                                var size = SafeParseUnitySize(match.Groups[2].Value, match.Groups[3].Value, path);

                                long existingSize;
                                if (!scenes.TryGetValue(path, out existingSize))
                                {
                                    scenes.Add(path, size);
                                }
                                else
                                {
                                    scenes[path] += size;
                                }
                            }
                            else
                            {
                                Log.Debug("Level statistics ended at {0}", lineCount);
                                parsingLevels = false;
                            }
                        }
#endif
                    }

                    if (assetsLines == null)
                    {
                        throw new ArgumentException("No asset info found in the log");
                    }
                }

                // now go through all the lines until separator is found
                using (ProfileSection.Create("Parsing Assets"))
                {
                    // line example:  8.1 mb	 1.1% Assets/UI/Menu/HUB/SpriteSheets/HUBSpriteSheetMapRegions.png
                    // unity 5.6.1 has a bug:  2.7 mb	 1.$% Resources/unity_builtin_extra
                    //                   mac:  2.7 mb    inf% Resources/unity_builtin_extra
                    var assetRegex = new Regex(@"^\s*(\d+\.\d+)\s*([mk]b)\s*(?:\d+\.[\d\$]+|inf)%\s*(.*)$", UnityVersionAgnostic.CompiledRegexOptions);

                    // get assets and sizes
                    foreach (var line in assetsLines)
                    {
                        var match = assetRegex.Match(line);
                        if (!match.Success)
                        {
                            break;
                        }

                        var path = match.Groups[3].Value;

                        if (path.StartsWith(SpriteAtlasPrefix))
                        {
                            // ignore this, on Android size seems ok, but on PC & iOS it's complete bollocks
                            // 2017_3_1: for new sprites atlases, this entry "merges" by resolution, which is utterly useless
                            continue;
                        }

                        assets.Add(path, SafeParseUnitySize(match.Groups[1].Value, match.Groups[2].Value, path));
                    }
                }
            }
            finally
            {
                try
                {
                    File.Delete(tempFileName);
                }
                catch (System.Exception) { }
            }

            return assets;
        }

        private static long SafeParseUnitySize(string size, string unit, string assetPath)
        {
            double parsedSize;
            if (!Utils.TryParseInvariant(size, out parsedSize))
            {
                Log.Warning("Unable to parse size {0} (path: {1})", size, assetPath);
                return 0;
            }

            int sizeMult;
            if (unit.Equals("gb", StringComparison.OrdinalIgnoreCase))
            {
                sizeMult = 1024 * 1024 * 1024;
            }
            else if (unit.Equals("mb", StringComparison.OrdinalIgnoreCase))
            {
                sizeMult = 1024 * 1024;
            }
            else if (unit.Equals("kb", StringComparison.OrdinalIgnoreCase))
            {
                sizeMult = 1024;
            }
            else if (unit.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                sizeMult = 1;
            }
            else
            {
                Log.Warning("Unrecognized size unit: {0} (path: {1})", unit, assetPath);
                sizeMult = 1;
            }

            return (long)(parsedSize * sizeMult);
        }
    }
}
