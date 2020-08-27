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
        private static string EDITOR_PATH = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, REMOTE_DATA_PATH, ADD_TO_BUILD_PATH);
        private static string BUILD_PATH = Path.Combine(Application.productName + "_Data", BUILD_DATA_PATH);
        
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var buildDirectory = new DirectoryInfo(pathToBuiltProject).Parent;
            
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
    }
}

