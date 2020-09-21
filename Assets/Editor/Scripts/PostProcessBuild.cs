using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace StarSalvager.Editor
{
    public static class PostProcessBuild
    {
        private const string REMOTE_DATA_PATH = "RemoteData";
        private const string ADD_TO_BUILD_PATH = "AddToBuild";
        private const string BUILD_DATA_PATH = "BuildData";


        //private static string DATA_FOLDER = $"{}_Data";
        private static readonly string EDITOR_PATH = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, REMOTE_DATA_PATH, ADD_TO_BUILD_PATH);

#if UNITY_STANDALONE_WIN
        private static string BUILD_PATH = Path.Combine(Application.productName + "_Data", BUILD_DATA_PATH);
#elif UNITY_STANDALONE_OSX
        private static readonly string BUILD_PATH = BUILD_DATA_PATH;
#endif
        
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    WindowsPost(pathToBuiltProject);
                    break;
                case BuildTarget.StandaloneOSX:
                    OsxPost(pathToBuiltProject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        private static void WindowsPost(string pathToBuiltProject)
        {
            var buildDirectory = new DirectoryInfo(pathToBuiltProject).Parent;
            Debug.Log($"{nameof(buildDirectory)}: {buildDirectory.FullName}");
            
            var dataDirectory = Path.Combine(buildDirectory.FullName, BUILD_PATH);
            Debug.Log($"Moving files from {EDITOR_PATH} to {buildDirectory}");
            
            var editorDirectory = new DirectoryInfo(EDITOR_PATH);
            var files = editorDirectory.GetFiles("*.txt");

            
            if (!Directory.Exists(dataDirectory))
                buildDirectory = Directory.CreateDirectory(dataDirectory);

            foreach (var file in files)
            {
                var path = Path.Combine(buildDirectory.FullName, file.Name);
                file.CopyTo(path, true);
                
                Debug.Log($"Copied {file.Name} to {path}");
            }
        }

        private static void OsxPost(string pathToBuiltProject)
        {
            var buildDirectory = new DirectoryInfo(pathToBuiltProject);
            
            var dataDirectory = Path.Combine(buildDirectory.FullName, "Contents", BUILD_PATH);
            Debug.Log($"Moving files from {EDITOR_PATH} to {buildDirectory}");
            
            var editorDirectory = new DirectoryInfo(EDITOR_PATH);
            var files = editorDirectory.GetFiles("*.txt");


            if (!Directory.Exists(dataDirectory))
            {
                var remoteDataPath = Path.Combine(pathToBuiltProject, "Contents", "RemoteData");
                Directory.CreateDirectory(remoteDataPath);

                buildDirectory = Directory.CreateDirectory(dataDirectory);

            }

            foreach (var file in files)
            {
                var path = Path.Combine(buildDirectory.FullName, file.Name);
                file.CopyTo(path, true);
                
                Debug.Log($"Copied {file.Name} to {path}");
            }
        }
    }
}

