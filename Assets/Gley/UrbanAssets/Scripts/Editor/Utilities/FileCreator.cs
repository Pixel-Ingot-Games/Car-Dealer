using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Gley.UrbanSystem.Editor
{
    internal class FileCreator
    {
        internal static void CreateAgentTypesFile<T>(List<string> agentCategories, string directive, string fileNamespace, string folderPath) where T : struct, IConvertible
        {
            if (agentCategories == null)
            {
                agentCategories = new List<string>();
                var allCarTypes = Enum.GetValues(typeof(T)).Cast<T>();
                foreach (T car in allCarTypes)
                {
                    agentCategories.Add(car.ToString());
                }
            }

            CreateFolder("Assets" + folderPath);

            string text =
            "// Automatically generated.\n" +
            "#if " + directive + "\n" +
            "namespace " + fileNamespace + "\n" +
            "{\n" +
            "\tpublic enum " + typeof(T).Name + "\n" +
            "\t{\n";
            for (int i = 0; i < agentCategories.Count; i++)
            {
                text += "\t\t" + agentCategories[i] + ",\n";
            }
            text += "\t}\n" +
                "}\n" +
                "#endif";

            File.WriteAllText(Application.dataPath + folderPath + "/" + typeof(T).Name + ".cs", text);
            AssetDatabase.Refresh();
        }

        internal static T LoadOrCreateLayers<T>(string path) where T : ScriptableObject
        {
            T layerSetup = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (layerSetup == null)
            {
                T asset = ScriptableObject.CreateInstance<T>();
                string folderPath = path.Remove(path.LastIndexOf('/'));
                CreateFolder(folderPath);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                layerSetup = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
            }

            return layerSetup;
        }

        static void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string tempPath = "";
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    tempPath += folders[i];
                    if (!AssetDatabase.IsValidFolder(tempPath + "/" + folders[i + 1]))
                    {
                        AssetDatabase.CreateFolder(tempPath, folders[i + 1]);
                        AssetDatabase.Refresh();
                    }
                    tempPath += "/";
                }
            }
        }
    }
}
