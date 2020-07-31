using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace StarSalvager.Editor
{
    public class PostProcessBuild
    {
        private static string EDITOR_PATH = Application.dataPath + "/RemoteData/AddToBuild/";
        private static string BUILD_PATH = "/RemoteData/AddToBuild/";
        
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            Debug.Log($"Moving files from {EDITOR_PATH} to Build path");
            
            var editorDirectory = new DirectoryInfo(EDITOR_PATH);
            var files = editorDirectory.GetFiles("*.txt");
            

            var buildDirectory = new DirectoryInfo(pathToBuiltProject).Parent;
            
            if (!Directory.Exists(Path.Combine(buildDirectory.FullName, BUILD_PATH)))
                buildDirectory = Directory.CreateDirectory(buildDirectory.FullName + BUILD_PATH);

            foreach (var file in files)
            {
                var path = Path.Combine(buildDirectory.FullName, file.Name);
                file.CopyTo(path);
                
                Debug.Log($"Copied {file.Name} to {path}");
            }
        }
    }
}

