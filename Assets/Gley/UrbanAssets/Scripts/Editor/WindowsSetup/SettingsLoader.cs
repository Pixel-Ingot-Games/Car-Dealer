using UnityEditor;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    internal class SettingsLoader
    {
        private readonly string _path;


        internal SettingsLoader(string path)
        {
            _path = path;
        }


        internal T LoadSettingsAsset<T>() where T : SettingsWindowData
        {
            T settingsWindowData = (T)AssetDatabase.LoadAssetAtPath(_path, typeof(T));

            if (settingsWindowData == null)
            {
                SettingsWindowData asset = ScriptableObject.CreateInstance<T>().Initialize();
                string[] pathFolders = _path.Split('/');
                string tempPath = pathFolders[0];
                for (int i = 1; i < pathFolders.Length - 1; i++)
                {
                    if (!AssetDatabase.IsValidFolder(tempPath + "/" + pathFolders[i]))
                    {
                        AssetDatabase.CreateFolder(tempPath, pathFolders[i]);
                        AssetDatabase.Refresh();
                    }

                    tempPath += "/" + pathFolders[i];
                }

                AssetDatabase.CreateAsset(asset, _path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                settingsWindowData = (T)AssetDatabase.LoadAssetAtPath(_path, typeof(T));
            }

            return settingsWindowData;
        }
    }
}