using Gley.UrbanSystem.Internal;
using System.IO;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace Gley.UrbanSystem.Editor
{
    public class VersionUpdater
    {
        struct FileToDelete
        {
            public string File;
            public string Guid;

            public FileToDelete(string file, string guid)
            {
                File = file;
                Guid = guid;
            }
        }

        public void DeleteSceneComponents()
        {
            if (MonoBehaviourUtilities.TryGetObject("GleyTrafficSystem", out var result))
            {
                Debug.Log($"{result.Value.name} removed");
                MonoBehaviourUtilities.DestroyImmediate(result.Value);
            }
            Debug.Log("Scene objects removed.");
        }

        public void DeleteProjectScripts()
        {
            var files = GetFiles();
            foreach (var file in files)
            {
                DeleteScript(file);
            }
            Debug.Log("Scripts removed.");
            AssetDatabase.Refresh();
            DeleteEmptyFolders();
        }

        public void DeleteEmptyFolders()
        {
            var directories = GetAllDirectories("Gley");
            foreach (var directory in directories)
            {
                DeleteDirectoryIfEmpty(directory);
            }
        }

        private FileToDelete[] GetFiles()
        {
            FileToDelete[] files = new FileToDelete[]
            {
                new FileToDelete("AgentRoutesSetupWindowBase","e9c835f28dffb9e4b80d6217bcda391a"),
                new FileToDelete("ConnectionPoolBase","5b234b3baf09ce04aa00ad372f497021"),
                new FileToDelete("ConnectionWaypoints","01c566cc35aed5544a66ce925a8dfa1d"),
                new FileToDelete("ConnectRoadsWindowBase","4d544145cd698544bb5500aec63bb284"),
                new FileToDelete("Creator","741376913795acb47b57bbafdd438728"),
                new FileToDelete("CurrentSceneData","ec31ac80f9a24f649b7e678efcc7eb89"),
                new FileToDelete("CurrentSceneDataEditor","b6f8c80b4950b544da2cbed7600f7df6"),
                new FileToDelete("Data","aa7bfb06a5e715b49a2ffaa39c23efc1"),
                new FileToDelete("DrawGrid","e5fea1c239366094ca4a414d2a5c0415"),
                new FileToDelete("DrawGridCell","39619d97ed591ee4d90bfc33fb4e43a8"),
                new FileToDelete("DrawRoadConnectors","94b8bdacf9edf404f9e85bf4ac88a9fa"),
                new FileToDelete("DriveActions","ef3d8c4f048ad9646ae209b7ee0dc31c"),
                new FileToDelete("EditRoadWindowBase","3a9819f173c92eb469c1c4e2c25244d7"),
                new FileToDelete("EditWaypointWindowBase","d03d2d89672774e409337b896c5dafeb"),
                new FileToDelete("GridCell","b13b57c6b8806664a9a4b6ff220a98fe"),
                new FileToDelete("GridData","a958fb4a892c66e48976526113fa8166"),
                new FileToDelete("GridEditor","511be4a27a18c31499f80e8a6affd5de"),
                new FileToDelete("GridManager","2b6e3cb08b056f24aa02ca6e77ba3dcb"),
                new FileToDelete("GridRow","4efd1a25031d36044a7bade675cb04a2"),
                new FileToDelete("IntersectionData","5f3b4281a12b37249b27f6469bf0580f"),
                new FileToDelete("IntersectionStopWaypoints","e353083596db4fd488fc036c42e050b0"),
                new FileToDelete("IntersectionWindowBase","a590f61a06261f7438fc5a265e8c3142"),
                new FileToDelete("ISetupWindow","8a95d350a33693943af055db3ece480f"),
                new FileToDelete("LaneData","ab8e34c6d1e2ea242b13cf0553423852"),
                new FileToDelete("LaneDrawer","9ead197e31cc4d74290feff59612534c"),
                new FileToDelete("LayerOperations","a10c15fec7afd5a41b253815c3288603"),
                new FileToDelete("LinkOtherLanes","9868f73b27c5c42449e543396e914957"),
                new FileToDelete("NewRoadWindow","c5c98ad6e8fbae3469bb3aeff904b6bd"),
                new FileToDelete("NewRoadWindowBase","64fdec5590e96b645bfaa3408223e5ab"),
                new FileToDelete("PathFinding","6846495980c41654fbad936f7bc3745e"),
                new FileToDelete("RoadConnections","feffd2ab77b3d9c4b8c1e500d7b3ccfe"),
                new FileToDelete("RoadConnectionsBase","5dcea19014f5a3f439d68bee7ea7d309"),
                new FileToDelete("RoadCreator","4e18bcd4d6b139f4e932e079c2b65303"),
                new FileToDelete("RoadData","db7fdc58efb4aa64888536ff1da4d909"),
                new FileToDelete("RoadDrawer","c6da0a9bf8343214580c4d7b513781c2"),
                new FileToDelete("RoadSetupWindowBase","8ccb93efc7bf5b04caf6550249e3e254"),
                new FileToDelete("RoadsLoader","952f092aacddd9b44bdda285acdc1f28"),
                new FileToDelete("SceneDrawer","c4c0e53d51140a149862949f734c0d8d"),
                new FileToDelete("ShowStopWaypoints","59881f3c4c67f934aa5b49e41ed18752"),
                new FileToDelete("ShowWaypointsBase","dcbc26340e7de41448ade1cb52ec19ea"),
                new FileToDelete("TrafficConnectionData","2332ec62e029acc41ab6e9a54821f710"),
                new FileToDelete("TrafficConnectionWaypoints","ffda06a2ca21bdb48b98dae40013dbea"),
                new FileToDelete("TrafficSettingsLoader","7bd23851496d7994da924fa141f31b30"),
                new FileToDelete("TrafficVehicles","244e382b69e9af840a2295f0bb1e9a4e"),
                new FileToDelete("TrafficWaypointData","9f9b53e8cb30c1041aef053a7c5c899f"),
                new FileToDelete("TrafficWindownNavigationData","39dab57bb5987be4eb1509faffdac2b7"),
                new FileToDelete("UrbanManager","ea9d2f28e387ff540844fa928c060e4e"),
                new FileToDelete("ViewRoadsWindowBase","20b8bc67987584c42975eb967550f1f6"),
                new FileToDelete("WaypointBase","88cbcd0a784c00f40bafed9f0d2908b3"),
                new FileToDelete("WaypointDrawer","983d2a6ad577d704584378a9c41acb8b"),
                new FileToDelete("WaypointDrawerBase","cec8bc013f2ea2740a473c1fc21bca6b"),
                new FileToDelete("WaypointManagerBase","b9efd637a30135f49bc0c2103390c672"),
                new FileToDelete("WaypointsGenerator","ddaf2853dcbec374d8b3c3ba2a03fbbe"),
                new FileToDelete("WaypointsGeneratorTraffic","5afaff1845b454f438bfa1112756a075"),
                new FileToDelete("CityTests","e2fc7c4638857ec4c8883a958abae5dc"),

            };
            return files;
        }

        private string[] GetDirectories()
        {
            string[] directories = new string[]
            {
                "Gley/UrbanAssets/Scripts/Unused"
            };
            return directories;
        }

        private string[] GetAllDirectories(string path)
        {
            string fullPath = Path.Combine(Application.dataPath,path);
            return Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories);
        }

        private void DeleteDirectoryIfEmpty(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path cannot be empty.");
                return;
            }

            // Convert relative path to absolute path
            string fullPath = Path.Combine(Application.dataPath, path);
            if (Directory.Exists(fullPath))
            {
                // Check if the directory is empty
                if (IsDirectoryEmpty(fullPath))
                {
                    // Convert full path to relative path for AssetDatabase
                    string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);

                    bool success = AssetDatabase.DeleteAsset(relativePath);
                    if (success)
                    {
                        Debug.Log($"Successfully deleted empty directory: {relativePath}");
                    }
                }
            }
        }

        private bool IsDirectoryEmpty(string path)
        {
            return Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;
        }

        private void DeleteScript(FileToDelete file)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(file.Guid);

            if (!string.IsNullOrEmpty(assetPath))
            {
                if (!assetPath.Contains(file.File))
                {
                    Debug.LogWarning($"{file.File} has the same GUID like {assetPath}");
                }
                bool success = AssetDatabase.DeleteAsset(assetPath);
                if (success)
                {
                    Debug.Log($"Successfully deleted asset at path: {assetPath}");
                }
            }
        }
    }
}